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
using System.Diagnostics;
using System.Messaging;
using System.Threading;
using Cryptany.Core.Management.WMI;
using Cryptany.Core.MsmqLog;
using Cryptany.Common.Utils;
using Cryptany.Common.Logging;
using Cryptany.Core;
using System.Collections.Generic;
using Cryptany.Core.Interaction;
using MSMQLogEntry = Cryptany.Core.MsmqLog.MSMQLogEntry;

namespace Cryptany.Core
{
    //представляет сущность-менеджер, содержащий в себе ссылку на коннектор, настройки отсылки, сообщение для пересылки
    public abstract class AbstractMessageManager : IMessageManager, IDisposable
    {
        private readonly Guid _connectorId; //Guid коннектора, с которым работает MessageManager
        private readonly AbstractConnectorSettings _abstractSettings; //Настройки коннектора (из бд)
        private readonly ILogger _logger;
        private readonly Thread thMonitor; //поток для мониторинга производительности

        #region Performance Counters

        protected PerformanceCounter pcIncomingMessagesPerSecond;
        protected PerformanceCounter pcOutgoingMessagesPerSecond;
        
        public PerformanceCounter pcConnectionState;

        /// <summary>
        /// Счетчик времени обработки входящего смс перед отсылкой в роутер
        /// </summary>
        protected PerformanceCounter pcPDUSMSProcessTime;

        /// <summary>
        /// Счетчик времени нахождения в очереди/ сек
        /// </summary>
        protected PerformanceCounter pcSMSInQueueTime;

        /// <summary>
        /// Счетчик попыток перепосылки/ сек
        /// </summary>
        protected PerformanceCounter pcResendCount;

        private void InitPerformanceCounters()
        {
            pcIncomingMessagesPerSecond = new PerformanceCounter();
            pcIncomingMessagesPerSecond.CategoryName = "Connector Service";
            pcIncomingMessagesPerSecond.CounterName = "# incoming messages / sec";
            pcIncomingMessagesPerSecond.MachineName = ".";
            pcIncomingMessagesPerSecond.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcIncomingMessagesPerSecond.InstanceName = AbstractSettings.SMSCCode.ToString();

            pcIncomingMessagesPerSecond.ReadOnly = false;

            pcOutgoingMessagesPerSecond = new PerformanceCounter();
            pcOutgoingMessagesPerSecond.CategoryName = "Connector Service";
            pcOutgoingMessagesPerSecond.CounterName = "# outgoing messages / sec";
            pcOutgoingMessagesPerSecond.MachineName = ".";
            pcOutgoingMessagesPerSecond.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcOutgoingMessagesPerSecond.InstanceName = AbstractSettings.SMSCCode.ToString();

            pcOutgoingMessagesPerSecond.ReadOnly = false;

            pcConnectionState = new PerformanceCounter();
            pcConnectionState.CategoryName = "Connector Service";
            pcConnectionState.CounterName = "Connection State";
            pcConnectionState.MachineName = ".";
            pcConnectionState.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcConnectionState.InstanceName = AbstractSettings.SMSCCode.ToString();

            pcConnectionState.ReadOnly = false;

            pcPDUSMSProcessTime = new PerformanceCounter();
            pcPDUSMSProcessTime.CategoryName = "Connector Service";
            pcPDUSMSProcessTime.CounterName = "SMS processing time, ms";
            pcPDUSMSProcessTime.MachineName = ".";
            pcPDUSMSProcessTime.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcPDUSMSProcessTime.InstanceName = AbstractSettings.SMSCCode.ToString();

            pcPDUSMSProcessTime.ReadOnly = false;

            pcSMSInQueueTime = new PerformanceCounter();
            pcSMSInQueueTime.CategoryName = "Connector Service";
            pcSMSInQueueTime.CounterName = "Time in outgoing queue, ms";
            pcSMSInQueueTime.MachineName = ".";
            pcSMSInQueueTime.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcSMSInQueueTime.InstanceName = AbstractSettings.SMSCCode.ToString();

            pcSMSInQueueTime.ReadOnly = false;

            pcResendCount = new PerformanceCounter();
            pcResendCount.CategoryName = "Connector Service";
            pcResendCount.CounterName = "SMS resend to SMSC count/sec";
            pcResendCount.MachineName = ".";
            pcResendCount.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcResendCount.InstanceName = AbstractSettings.SMSCCode.ToString();

            pcResendCount.ReadOnly = false;

            InitPerformanceCountersExt();
        }

        private void ClosePerformanceCounters()
        {
            try
            {
                pcIncomingMessagesPerSecond.RemoveInstance();
                pcIncomingMessagesPerSecond.Close();
                pcIncomingMessagesPerSecond.Dispose();
                pcOutgoingMessagesPerSecond.RemoveInstance();
                pcOutgoingMessagesPerSecond.Close();
                pcOutgoingMessagesPerSecond.Dispose();
                pcConnectionState.RemoveInstance();
                pcConnectionState.Close();
                pcConnectionState.Dispose();
                pcPDUSMSProcessTime.RemoveInstance();
                pcPDUSMSProcessTime.Close();
                pcPDUSMSProcessTime.Dispose();
                pcSMSInQueueTime.RemoveInstance();
                pcSMSInQueueTime.Close();
                pcSMSInQueueTime.Dispose();
                pcResendCount.RemoveInstance();
                pcResendCount.Close();
                pcResendCount.Dispose();
                ClosePerformanceCountersExt();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                PerformanceCounter.CloseSharedResources();
            }
        }

        #endregion

        public ManualResetEvent ReadyToSendMessages;
        public OutputMessage SendMessage; //текущее сообщение для отсылки
        protected bool MultipartSendNextPart = false; //флаг посылки следующего сообщения несколькими пакетами

        public static AbstractMessageManager Create(ConnectorSettings cs, ILogger logger)
        {
            Type t = Type.GetType(cs.ManagerClass.Trim(), true, true);
            AbstractMessageManager res = Activator.CreateInstance(t, new object[] { cs, logger }) as AbstractMessageManager;
            return res;
        }
        public Guid ConnectorId
        { get { return _connectorId; } }

        public ILogger Logger
        { get { return _logger; } }

        public virtual bool ConnectedToSmsc
        { get { return false; } }

        public AbstractConnectorSettings AbstractSettings
        { get { return _abstractSettings; } }

        //потрясающе
        public virtual bool CanSendNextMessage()
        { return true; }

        public abstract void QueryMessageState(string message_id, string source_addr);

        protected abstract GenericConnector State { get; }
    
        protected abstract void Init(AbstractConnectorSettings settings);
        public abstract bool SendUserData(OutputMessage outputMessage);
        public abstract bool SendSMSCRequest(MessageStatusQuery query);
        public abstract bool SendSubscription(SubscriptionMessage subgmsg);
        protected abstract void InitPerformanceCountersExt();
        protected abstract void ClosePerformanceCountersExt();
        public abstract void UpdateSettings(AbstractConnectorSettings settings);
        
        protected virtual void SetPerformanceCounters()
        {
            pcPDUSMSProcessTime.RawValue = (long)(State._lastSentToRouterTime - State._lastSMSInTime).TotalMilliseconds;
        }
        //включает счётчики производительности
        protected void MonitoringThread()
        {
            if (_abstractSettings.MonitoringSleepTime == 0) return; // иначе этот поток возьмет 100% проца
            while (true)
            {
                SetPerformanceCounters();
                Trace.Write("Мониторинг коннектора: ждем следующей итерации");
                Thread.Sleep(TimeSpan.FromSeconds(_abstractSettings.MonitoringSleepTime));
            }
        }
        //запускает в отдельном потоке MonitoringThread
        protected AbstractMessageManager(ConnectorSettings cs, ILogger logger)
        {
            _logger = logger;
            _abstractSettings = AbstractConnectorSettings.GetSettings(cs);
            
            InitPerformanceCounters();
            _connectorId = AbstractSettings.SMSCId;
            Init(AbstractSettings);

            if (_abstractSettings.UseMonitoring)
            {
                thMonitor = new Thread(MonitoringThread);
                thMonitor.IsBackground = true;
                thMonitor.Name = "Connector Monitoring Thread";
                thMonitor.Start();
            }
        }

        public void AddOutboxSendHistory(Guid OutputMessageId, int state, string description)
        {
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "kernel.AddOutboxSendHistory";
                me.Parameters.Add("@OutboxId", OutputMessageId);
                me.Parameters.Add("@State", state);
                me.Parameters.Add("@EntryTime", DateTime.Now);
                me.Parameters.Add("@StateDescription", description);
                using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (MessageQueueException e)
            {
                try
                {
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, e.ToString()));
                }
                catch
                {
                }
                if (_logger != null)
                {
                    _logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }
        
        public void UpdateOutboxState(OutputMessage OutputMessage, int state, string description, string SMSCMsgId)
        {
            UpdateOutboxState(OutputMessage, state, description, SMSCMsgId,null);
        }
        
        /// <summary>
        /// кидает статус сообщения в очереди логгеров
        /// </summary>
        public void UpdateOutboxState(OutputMessage OutputMessage, int state, string description, string SMSCMsgId, Dictionary<string, object> additionalParams)
        {
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "kernel.UpdateOutboxState";
                me.Parameters.Add("@ID", OutputMessage.ID);
                me.Parameters.Add("@State", state);
                me.Parameters.Add("@StateTime", DateTime.Now);
                me.Parameters.Add("@StateDescription", description);
                me.Parameters.Add("@SMSCMsgId", SMSCMsgId);
                me.Parameters.Add("@smscid", ConnectorId);
                me.Parameters.Add("@resourceId", OutputMessage.ProjectID);
                if (OutputMessage.PartsCount > 0)
                    me.Parameters.Add("@partscount", (int)OutputMessage.PartsCount);
                if (additionalParams != null && additionalParams.Count > 0)
                    foreach (KeyValuePair<string, object> pair in additionalParams)
                        me.Parameters.Add(pair.Key, pair.Value);
                  
                foreach (MessageQueue mq in Cryptany.Core.Services.Management.ServicesConfigurationManager.GetMSMQLoggerQueues(OutputMessage))
                {
                    using (mq)
                    {
                        lock (mq)
                        {
                            System.Messaging.Message msg = new System.Messaging.Message(me, new BinaryMessageFormatter());
                            mq.Send(msg);
                        }
                    }
                }
            }
            catch (MessageQueueException e)
            {
                try
                {
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, e.ToString()));
                }
                catch
                {
                }
                if (_logger != null)
                {
                    _logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }

            }
        }

        /// <summary>
        /// кидает в очереди логгеров статус, полученный в отчете о доставке
        /// </summary>
        public void UpdateTerminalDelivery(OutputMessage msg, string message_id, string state, DateTime time, Guid smscid)
        {
            if (msg == null)
            {
                UpdateTerminalDelivery(message_id, state, time, smscid);
                return;
            }
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "kernel.UpdateTerminalDelivery";
                me.Parameters.Add("@messageid", message_id);
                me.Parameters.Add("@smscid", smscid);
                me.Parameters.Add("@time", time);
                me.Parameters.Add("@value", state);
                me.Parameters.Add("@Id",msg.ID);
                me.Parameters.Add("@resourceId", msg.ProjectID);
                foreach (MessageQueue mq in Cryptany.Core.Services.Management.ServicesConfigurationManager.GetMSMQLoggerQueues(msg))
                {
                    using (mq)
                    { 
                        mq.Send(me);
                    }
                }
            }
            catch (MessageQueueException e)
            {
                try
                {
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, e.ToString()));
                }
                catch
                {

                }
                if (_logger != null)
                {
                    _logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }

        public void UpdateTerminalDelivery(string message_id, string state, DateTime time, Guid smscid)
        {
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "kernel.UpdateTerminalDelivery";
                me.Parameters.Add("@messageid", message_id);
                me.Parameters.Add("@smscid", smscid);
                me.Parameters.Add("@time", time);
                me.Parameters.Add("@value", state);
                using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (MessageQueueException e)
            {
                try
                {
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, e.ToString()));
                }
                catch
                {
                }
                if (_logger != null)
                {
                    _logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }
        
        #region IMessageManager Members

        public virtual event EventHandler MessageReceived;

        public virtual event EventHandler MessageSent;

        public virtual event MessageStateChangedEventHandler MessageStateChanged;

        public virtual event StateChangedEventHandler StateChanged;

        public virtual event EventHandler RequireReinit;

        public virtual event EventHandler SMSCActivity;

        #endregion

        #region IDisposable Members

        
        public virtual void Dispose()
        {
            ClosePerformanceCounters();
        }
        
        #endregion
    }
}
