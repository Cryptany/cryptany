/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Messaging;
using System.Threading;
using System.Reflection;
using System.Xml.Serialization;
using Cryptany.Core.Interaction;
using Cryptany.Core.Management;
using Cryptany.Core.Management.WMI;
using Cryptany.Core;
using Cryptany.Common.Utils;
using Cryptany.Common.Logging;
using Message=System.Messaging.Message;


namespace Cryptany.Core.ConnectorServices
{
    public class SMSProxy: IDisposable
    {
        private readonly ILogger logger = LoggerFactory.Logger;

        public event EventHandler OnStop;

        private AutoResetEvent smsProcessingCompleted;

        private readonly int SMSCCode;                              // Connector code

        private AbstractMessageManager _absMessageManager;

        private MessageQueue connectorQueue;

        private ConnectorSettings _connectorSettings;

        private System.Timers.Timer _settingsTimer;

        #region PerformanceCounters

        /// <summary>
        /// Счетчик времени ответа на входящие пакеты
        /// </summary>
        protected PerformanceCounter pcOutCounter;

        /// <summary>
        /// Счетчик времени ответа на исходящие пакеты
        /// </summary>

        #endregion
        
        public SMSProxy(int SMSCCode)
        {
            this.SMSCCode = SMSCCode;
            InitLogger();
            InitPerformanceCounters(SMSCCode);
        }

        protected void InitPerformanceCounters(int code)
        {
            pcOutCounter = new PerformanceCounter();
            pcOutCounter.CategoryName = "Connector Service";
            pcOutCounter.CounterName = "sended sms";
            pcOutCounter.MachineName = ".";
            pcOutCounter.InstanceName = code.ToString();
            pcOutCounter.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcOutCounter.ReadOnly = false;
            pcOutCounter.RawValue = 0;
        }
            
        protected void ClosePerformanceCounters()
        {
            pcOutCounter.RemoveInstance();
            pcOutCounter.Close();
            pcOutCounter.Dispose();
        }

        public void Init(bool newProxy)
        {
            logger.Write(new LogMessage("Start init process...", LogSeverity.Info));
            smsProcessingCompleted = new AutoResetEvent(true);
           
            if (newProxy)
            {
                try
                {
                    _connectorSettings = LoadSettingsFromDB(SMSCCode);
                    SaveSettings(_connectorSettings);
                }
                catch (Exception ex)
                {
                    ServiceManager.LogEvent("Cannot load connector settings from database: " + ex, EventType.Warning, EventSeverity.High);
                    logger.Write(  new LogMessage("Cannot load connector settings from database: " + SMSCCode + ": " + ex, LogSeverity.Alert));
                    
                    _settingsTimer = new System.Timers.Timer(3*60*1000);
                    _settingsTimer.Elapsed += new System.Timers.ElapsedEventHandler(_settingsTimer_Elapsed);
                    _settingsTimer.Start();
                    _connectorSettings = LoadSettings();
                }
            }
            if (_connectorSettings != null)
            {
                ServiceManager.ServiceId = _connectorSettings.ServiceId;
                _absMessageManager = AbstractMessageManager.Create(_connectorSettings, logger);
                if (_absMessageManager == null) throw new ApplicationException("Cannot create _absMessageManager");
                _absMessageManager.StateChanged += StateChanged;
                _absMessageManager.RequireReinit += RequireReinit;
               
                QueueReInit();

                logger.Write(new LogMessage("Successfully inited: Connector code = " + _connectorSettings.Code, LogSeverity.Info));
                ServiceManager.LogEvent("Successfully inited", EventType.Info, EventSeverity.Normal);
            }
        }

      

        void _settingsTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                _settingsTimer.Stop();
                ConfigurationManager.RefreshSection("appSettings");
                ConfigurationManager.RefreshSection("connectionStrings");
                ConnectorSettings cs = LoadSettingsFromDB(SMSCCode);
                
                logger.Write(new LogMessage("Got new settings from database", LogSeverity.Alert));
                if (_connectorSettings != null && cs.CompareTo(_connectorSettings) != 0)
                {
                    logger.Write(
                        new LogMessage("Connector settings changed, reinitializing connector...",
                                       LogSeverity.Alert));
                }
                _connectorSettings = cs;
                ServiceManager.ServiceId = _connectorSettings.ServiceId;
                Dispose();
                Init(false);

            }
            catch (Exception ex)
            {
                logger.Write(
                        new LogMessage("Cannot load connector settings from database on timely fashion " + SMSCCode + ": " + ex,
                                       LogSeverity.Alert));
                _settingsTimer.Start();
            }
        }

      
        public static ConnectorSettings LoadSettingsFromDB(int Code)
        {
            ConnectorSettings cs = new ConnectorSettings();
            using (SqlConnection conn = Database.Connection)
            {
                using (SqlCommand comm = new SqlCommand("kernel.GetConnectorSettings", conn))
                //ID, имя, serviceId, protocolType (http, smpp), ManagerClass, Settings (xml)
                {
                    comm.CommandType = System.Data.CommandType.StoredProcedure;
                    comm.CommandTimeout = 120;
                    comm.Parameters.AddWithValue("@Code", Code);
                    using (SqlDataReader sdr = comm.ExecuteReader())
                    {
                        if (sdr != null && sdr.HasRows)
                        {
                            while (sdr.Read())
                            {
                                cs.Code = Code;
                                cs.ID = sdr.GetGuid(0);
                                cs.Name = sdr.GetString(1);
                                cs.ServiceId = sdr.GetGuid(2);
                                cs.ProtocolType = sdr.GetString(3);
                                cs.ManagerClass = sdr.GetString(4);
                                cs.Settings = sdr.GetString(5);
                            }
                            return cs;
                        }
                    }
                }
            }
            
            return null;
        }
        private ConnectorSettings LoadSettings()
        {
            try
            {
                FileInfo execPath = new FileInfo(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(execPath.Directory.FullName, SMSCCode + ".xml");
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    XmlSerializer ser = new XmlSerializer(typeof (ConnectorSettings), "http://cryptany.com");
                    return (ConnectorSettings) ser.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {
                logger.Write(new LogMessage("Cannot load connector settings from file:" + ex, LogSeverity.Alert));
            }
            return null;
        }

        private void SaveSettings(ConnectorSettings smscSettings)
        {
            try
            {
                FileInfo execPath = new FileInfo(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(execPath.Directory.FullName, SMSCCode + ".xml");
                using(FileStream fs = new FileStream(path, FileMode.Create))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(ConnectorSettings), "http://cryptany.com");
                    ser.Serialize(fs, smscSettings);
                    
                }
            }
            catch (Exception ex)
            {
                logger.Write(new LogMessage("Could not save connector settings to file:" + ex, LogSeverity.Alert));
            }
        }

        void _absMessageManager_SMSCActivity(object sender, EventArgs e)
        {
            
            
        }


        public void UpdateConnectorSettings()
        {
            _connectorSettings = LoadSettingsFromDB(SMSCCode);
            _absMessageManager.UpdateSettings(AbstractConnectorSettings.GetSettings(_connectorSettings));

        }

        private void QueueReInit()
        {
            MessageQueue.EnableConnectionCache = false;//чтобы по вызову Close перестал ее читать
            logger.Write(new LogMessage("Init connectorQueue. SMSCId = " + _connectorSettings.ID, LogSeverity.Info));
            connectorQueue = ServiceManager.GetServiceMessageQueue(_connectorSettings.ID);

            connectorQueue.PeekCompleted += connectorQueue_PeekCompleted;
            connectorQueue.BeginPeek(MessageQueue.InfiniteTimeout, connectorQueue);
        }

        static void _absMessageManager_MessageStateChanged(object obj, MessageStateChangedEventArgs e)
        {
           
        }

        private void FireError(string severity, string description)
        {
            ConnectorErrorEvent err = new ConnectorErrorEvent();
            err.ID = SMSCCode;
            err.ErrorSeverity = severity;
            err.ErrorDescription = description;
            err.ErrorTime = DateTime.Now.ToString();
            err.Fire();
        }

        void StateChanged(object sender, StateChangedEventArgs e)
        {
            try
            {
                switch (e.State)
                {

                    case ConnectorState.Error:
                        ServiceManager.LogEvent(e.StateDescription, EventType.Error, EventSeverity.High);
                        FireError(e.State.ToString(), e.StateDescription);
                        break;
                    case ConnectorState.Connected:
                        //genericConnector.LastConnectDate = DateTime.Now.ToString();
                        break;
                    default:
                        break;
                }
            }
            catch
            {
                
            }
        }

        
        
        void MessageReceived(object sender, EventArgs e)
        {
            //try
            //{
            //    genericConnector.IncomingMessagesCount++;

            //    genericConnector.LastMessageDate = DateTime.Now.ToString();
            //}
            //catch
            //{

            //}
        }

        void MessageDelivered(object sender, EventArgs e)
        {
            //try
            //{
            //    genericConnector.DeliveredMessagesCount++;
            //}
            //catch
            //{

            //}
            
        }

        void MessageSent(object sender, EventArgs e)
        {
            //try
            //{
            //    genericConnector.OutgoingMessagesCount++;
            //}
            
            //catch
            //{
                
            //}
        }

        void RequireReinit(object sender, EventArgs e)
        {

            Dispose();
            Init(false);
        }
		void connectorQueue_PeekCompleted(object sender, PeekCompletedEventArgs e)
		{
            bool sendOk = false;
            MessageQueue mq = (MessageQueue)e.AsyncResult.AsyncState;
		    Message queueMsg=null;
            try
            {
                queueMsg = mq.EndPeek(e.AsyncResult);

                if (!mq.CanRead)
                {

                    logger.Write(new LogMessage("Connector SMSCCode = " + SMSCCode + " can not read message queue!", LogSeverity.Error));

                    ServiceManager.LogEvent("Can not read message queue " + mq.Path, EventType.Error,
                                            EventSeverity.Critical);

                }
                else if (queueMsg.Body is OutputMessage)
                {
                    OutputMessage outputMessage = (OutputMessage)queueMsg.Body;
                    outputMessage.TimeReceived = DateTime.Now;
                    smsProcessingCompleted.Reset();


                    if (outputMessage.TTL < DateTime.Now) //message timeout, skip sending
                    {

                        logger.Write(new LogMessage("Message expired: Deleting message from the queue...", LogSeverity.Info));
                        _absMessageManager.UpdateOutboxState(outputMessage, (int)MessageDeliveryStatus.Undelivered, "rotten", "");

                        if (_absMessageManager.AbstractSettings.UseMessageState && outputMessage.InboxMsgID == Guid.Empty)
                        {
                            Cryptany.Core.Management.WMI.MessageState evt = new Cryptany.Core.Management.WMI.MessageState();
                            evt.ID = outputMessage.ID.ToString();
                            evt.Status = MessageDeliveryStatus.Undelivered.ToString();
                            evt.StatusDescription = "rotten";
                            evt.StatusTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                            Message message = new Message(evt, new BinaryMessageFormatter());
                            message.AttachSenderId = false;

                            using (MessageQueue msgQueue = ServiceManager.MessageStateQueue)
                            {
                                msgQueue.Send(message);
                            }
                        }
                        sendOk = true;
                        Thread.Sleep(50);
                        return;
                    }
                    int resendcount = 0;
                    Thread.CurrentThread.Name = "SMSProxy";
                    while (!sendOk)
                    {
                        if (outputMessage == null) throw new ArgumentNullException("outputMessage");
                        if (_absMessageManager.CanSendNextMessage())
                        {
                            logger.Write(new LogMessage("Sending outgoing message: " + outputMessage, LogSeverity.Info));
                            sendOk = _absMessageManager.SendUserData(outputMessage);
                        }

                        if (!sendOk)
                        {
                            resendcount++;
                            if (resendcount < _absMessageManager.AbstractSettings.RepeatSendCount)
                            {
                                Thread.Sleep(_absMessageManager.AbstractSettings.RepeatSendTimeout);

                            }
                            else
                            {
                                ServiceManager.LogEvent("Can't send message to SMSC. Marking as undelivered. MessageId=" + outputMessage.ID, EventType.Error, EventSeverity.High);
                                logger.Write(new LogMessage("Can't send message to SMSC. Marking as undelivered", LogSeverity.Info));
                                _absMessageManager.UpdateOutboxState(outputMessage, (int)MessageDeliveryStatus.Undelivered, "failed", "");


                                if (!(_absMessageManager is SMPPMessageManagerAsync))
                                    _absMessageManager.SendMessage = null;//для синхр...


                                if (_absMessageManager.AbstractSettings.UseMessageState && outputMessage.InboxMsgID == Guid.Empty)
                                {
                                    Cryptany.Core.Management.WMI.MessageState evt =
                                        new Cryptany.Core.Management.WMI.MessageState();

                                    evt.ID = outputMessage.ID.ToString();
                                    evt.Status = MessageDeliveryStatus.Undelivered.ToString();
                                    evt.StatusDescription = "send failed";
                                    evt.StatusTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    Message message = new Message(evt, new BinaryMessageFormatter());
                                    message.AttachSenderId = false;
                                    using (MessageQueue msgQueue = ServiceManager.MessageStateQueue)
                                    {
                                        msgQueue.Send(message);
                                    }
                                }


                                sendOk = true;
                            }
                            //_absMessageManager.ReadyToSendMessages.Set();
                        }
                        else
                        {
                            pcOutCounter.Increment();
                        }
                    }

                }
                else if (queueMsg.Body is WinServiceCommand)
                {
                    WinServiceCommand wsc = (WinServiceCommand)queueMsg.Body;
                    logger.Write(new LogMessage("Got ServiceManager command " + wsc.MethodName, LogSeverity.Info));


                    MethodInfo mi = this.GetType().GetMethod(wsc.MethodName);

                    if (mi != null)
                    {
                        mi.Invoke(this, wsc.parameters);
                    }
                    else
                    {
                        ServiceManager.LogEvent("Could not find method " + wsc.MethodName, EventType.Error, EventSeverity.High);
                        logger.Write(new LogMessage("Could not find method " + wsc.MethodName, LogSeverity.Info));
                    }
                    Trace.WriteLine("Connector " + _absMessageManager.AbstractSettings.SMSCCode + " Successfully invoked method " + wsc.MethodName);
                }

                else if (queueMsg.Body is MessageStatusQuery)
                {
                    smsProcessingCompleted.Reset();

                    MessageStatusQuery msq = (MessageStatusQuery)queueMsg.Body;

                    if (msq == null) throw new ArgumentNullException("outputMessage");
                    if (_absMessageManager.CanSendNextMessage())
                    {

                        logger.Write(new LogMessage("Sending  " + msq, LogSeverity.Info));
                        sendOk = _absMessageManager.SendSMSCRequest(msq);

                    }

                }

                else if (queueMsg.Body is SubscriptionMessage)
                {
                    smsProcessingCompleted.Reset();

                    SubscriptionMessage smsg = (SubscriptionMessage)queueMsg.Body;
                    if (smsg == null) throw new ArgumentNullException("subscriptionMessage");
                    if (_absMessageManager.CanSendNextMessage())
                    {

                        logger.Write(new LogMessage("Sending  " + smsg.actionType + " " + smsg.MSISDN, LogSeverity.Info));
                        sendOk = _absMessageManager.SendSubscription(smsg);

                    }
                }
            }
            catch (MessageQueueException mqe)
            {

                ServiceManager.LogEvent(mqe.ToString(), EventType.Error, EventSeverity.High);
                logger.Write(new LogMessage(mqe.MessageQueueErrorCode + " " + mqe, LogSeverity.Error));
                Thread.Sleep(new TimeSpan(0, 0, 5));
            }
            catch (ApplicationException ae)
            {
                ServiceManager.LogEvent(ae.ToString(), EventType.Error, EventSeverity.High);
                logger.Write(new LogMessage(ae.ToString(), LogSeverity.Error));
                if (OnStop != null)
                    OnStop(this, new EventArgs());
            }

			catch (Exception ex)
            {

                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                logger.Write(new LogMessage("Exception in SMSProxy reading a new message! " + ex, LogSeverity.Error));
                Thread.Sleep(new TimeSpan(0, 0, 5));
            }
            finally
            {
                if (queueMsg!=null) 
                    mq.ReceiveById(queueMsg.Id);
                    smsProcessingCompleted.Set();
                    mq.BeginPeek(MessageQueue.InfiniteTimeout, mq);
              
            }
		}


		public void Dispose()
		{
			try
			{
			    ClosePerformanceCounters();
			
                if (!smsProcessingCompleted.WaitOne(TimeSpan.FromSeconds(3),false))
                {
                    throw new Exception("SMS processing timeout elapsed!");
                }
                
                if (connectorQueue!=null)
			        connectorQueue.Close(); //stops peeking

                _absMessageManager.Dispose();
                _absMessageManager = null;

                logger.Write(new LogMessage("SMSCId = " + _connectorSettings.ID + " connector shutdown successfull!", LogSeverity.Info));
                ServiceManager.LogEvent("Shutdown successfull", EventType.Info, EventSeverity.Normal);
			}
			catch (Exception ex)
			{
				logger.Write(new LogMessage("Exception in SMSProxy Dispose method: " + ex, LogSeverity.Error));
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
			}
			
		}

        private void InitLogger()
        {
            logger.DefaultSource = "SMSProxy";
            logger.DefaultServiceSource = "Cryptany.ConnectorService" + SMSCCode;   
        }
	}	
}