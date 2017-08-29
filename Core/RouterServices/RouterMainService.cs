using System;
using System.Messaging;
using System.Threading;
using System.ServiceProcess;
using Cryptany.Core.Management;
using Cryptany.Common.Logging;
using Cryptany.Common.Settings;

namespace Cryptany.Core.Router.RouterServices
{
    ///<summary>
    ///</summary>
    public partial class RouterMainService : ServiceBase
    {
        private AutoResetEvent stopPeekEvent;

        private AutoResetEvent stopPeekCompleteEvent;

        private MessageQueue MainInputSMSQueue;

        private int routerIndex;

        ///<summary>
        ///</summary>
        public int RouterIndex
        {
            get
            {
                return routerIndex;
            }

            set
            {
                int MaxRouterIndex = Convert.ToInt32(SettingsProviderFactory.DefaultSettingsProvider["MaxRouterIndex"]);
                if (value <= 0 || value > MaxRouterIndex)
                {
                    routerIndex = 1;
                }
                else
                {
                    routerIndex = value;
                }
            }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public RouterMainService()
        {
            InitializeComponent();
            RouterIndex = 1;
            
        }

        

		///<summary>
		///</summary>
		public void ManualStart()
		{
			OnStart(null);
		}

		///<summary>
		///</summary>
		public void ManualStop()
		{
			OnStop();
		}

        protected override void OnStart(string[] args)
        {
            try
            {
                
                EventLog.WriteEntry("Starting router main service.", System.Diagnostics.EventLogEntryType.Information);
               
                stopPeekEvent = new AutoResetEvent(false);
                stopPeekCompleteEvent = new AutoResetEvent(false);
                // Start main message input queue processing 
                MainInputSMSQueue = ServicesConfigurationManager.MainInputSMSQueue;
                MainInputSMSQueue.ReceiveCompleted += MainInputSMSQueue_ReceiveCompleted;
                MainInputSMSQueue.BeginReceive();
                EventLog.WriteEntry("Router main service has started.", System.Diagnostics.EventLogEntryType.Information);
                               
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Failed to start router main service! Exception: " + ex,
                                    System.Diagnostics.EventLogEntryType.Error);
               
            }
        }

        /// <summary>
        /// Receive process messages.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MainInputSMSQueue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
        {
            MessageQueue mq = sender as MessageQueue;
            try
            {
                if (mq != null)
                {
                    Message queueMsg = mq.EndReceive(e.AsyncResult);
                    if (queueMsg == null)
                    {
                        EventLog.WriteEntry("Router main service failed to recieve message.",
                                            System.Diagnostics.EventLogEntryType.Error);
                    }
                    else
                    {
                        Cryptany.Core.Message message = (Cryptany.Core.Message) queueMsg.Body;
                        // Distribute message from common queue to separate router queue
                        MessageQueue InputSMSQueue = ServicesConfigurationManager.GetInputSMSQueue(RouterIndex++);
                        InputSMSQueue.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Exception in Router main service while trying to process message!" + ex,
                                    System.Diagnostics.EventLogEntryType.Error);
            }
            finally
            {
                if (mq != null) mq.BeginReceive();
            }
        }

        protected override void OnStop()
        {
            try
            {
                // For remote debugging, use
                // Thread.Sleep(new TimeSpan(0, 0, 25));
                EventLog.WriteEntry("Stopping router main service.", System.Diagnostics.EventLogEntryType.Information);
                
                // Router main input queue shutdown
                if (stopPeekEvent.Set())
                    stopPeekCompleteEvent.WaitOne(3000, false);
                MainInputSMSQueue.Close();
                EventLog.WriteEntry("Router main service has stopped.", System.Diagnostics.EventLogEntryType.Information);
                
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Failed to stop router main service. " + ex,
                                    System.Diagnostics.EventLogEntryType.Error);
               
            }
            
        }
    }
}
