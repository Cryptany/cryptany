using System;
using System.ComponentModel;
using System.Configuration;
using System.ServiceProcess;
using Cryptany.Core.Management;
using Cryptany.Core;


namespace Cryptany.Core.Router.RouterServices
{
    public class RouterService : ServiceBase
    {
        private readonly int ServiceCode;
        private Router _Router;

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private Container components;

        

        /// <summary>
        /// Default constructor
        /// </summary>
        public RouterService(int ServiceCode)
        {
            // This call is required by the Windows.Forms Component Designer.
            InitializeComponent();
            this.ServiceCode = ServiceCode;
            ServiceName = "Cryptany.Core.Router.RouterService" + this.ServiceCode;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["RouterServiceID"]))
                ServiceManager.ServiceId = new Guid(ConfigurationManager.AppSettings["RouterServiceID"]);
            // For local debug use this:
            // this.OnStart(new string[0]);
        }

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // RouterService
            // 
            ServiceName = "Cryptany.Core.Router.RouterService";
        }

        

        /// <summary>
        /// Set things in motion so your service can do its work.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            try
            {
                
                if (ServiceCode != 0)
                {
                    EventLog.WriteEntry("Starting router service. ServiceCode = " + ServiceCode, System.Diagnostics.EventLogEntryType.Information);
                    
                    // Router initialization
                    _Router = new Router(ServiceCode);
                    _Router.OnExceptionOcurred += _Router_OnExceptionOcurred;
					//_Router.RouterCriticalError += _Router_RouterCriticalError;
                    EventLog.WriteEntry("Router service has started. ServiceCode = " + ServiceCode,
                                        System.Diagnostics.EventLogEntryType.Information);
                    
                }
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(
                    "Failed to start router service! Exception: " + ex + ". ServiceCode = " + ServiceCode,
                    System.Diagnostics.EventLogEntryType.Error);
                ServiceManager.LogEvent("Failed to start router service! Exception: " + ex, EventType.Error, EventSeverity.Critical);

                throw new ApplicationException(ex.Message);
            }
        }

        void _Router_OnExceptionOcurred(object sender, Router.ExceptionOccuredEventArgs e)
        {
            EventLog.WriteEntry(e.Exception.ToString(), System.Diagnostics.EventLogEntryType.Error);
        }

        private void _Router_RouterCriticalError(object sender, Router.RouterCriticalErrorEventArgs e)
        {
            EventLog.WriteEntry("Router service is being stopped due to exception: " + e.FiredException.ToString(), System.Diagnostics.EventLogEntryType.Error);
            Stop();
        }

        /// <summary>
        /// Stop this service.
        /// </summary>
        protected override void OnStop()
        {
            try
            {

                EventLog.WriteEntry("Stopping router service. ServiceCode = " + ServiceCode,
                                    System.Diagnostics.EventLogEntryType.Information);
                
                _Router.Dispose();
                EventLog.WriteEntry("Router service has stopped. ServiceCode = " + ServiceCode,
                                    System.Diagnostics.EventLogEntryType.Information);
                
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry("Failed to stop router service. " + ex + ". ServiceCode = " + ServiceCode,
                                    System.Diagnostics.EventLogEntryType.Error);
                
            }
            
        }
    }
}