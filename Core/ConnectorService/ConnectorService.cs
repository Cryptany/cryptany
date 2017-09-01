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
using System.Threading;

namespace Cryptany.Core.ConnectorServices
{
    using System;
    using System.Diagnostics;
    using System.ServiceProcess;
    /// <summary>
    /// Main service for messages processing
    /// </summary>
	public class ConnectorService : ServiceBase
	{
        /// <summary>
        /// Экземпляр прокси-объекта
        /// </summary>
        private SMSProxy _Proxy;

        /// <summary>
        /// Код коннектора
        /// </summary>
        private readonly int      SMSCCode;

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ConnectorService(int SMSCCode)
        {
            // This call is required by the Windows.Forms Component Designer.
            InitializeComponent();
            this.SMSCCode = SMSCCode;
            ServiceName = "Cryptany.ConnectorService" + this.SMSCCode;
        }

        /// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            // 
            // ConnectorService
            // 
            this.AutoLog = false;
            this.ServiceName = "Cryptany.ConnectorService";
		}

        private void InitProxy()
        {
            _Proxy = new SMSProxy(SMSCCode);
            _Proxy.OnStop += _Proxy_OnStop;
            _Proxy.Init(true); 
        }
       
        /// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
            try
            {
                EventLog.WriteEntry("Starting connector service. SMSCCode = " + SMSCCode, EventLogEntryType.Information);
                InitProxy();
                EventLog.WriteEntry("Connector service has started. SMSCCode = " + SMSCCode, EventLogEntryType.Information);
            }
            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
 		}

        
        void _Proxy_OnStop(object sender, EventArgs e)
        {
            Stop();
        }

		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
            try
            {
                base.EventLog.WriteEntry("Stopping connector service. SMSCCode = " + SMSCCode, EventLogEntryType.Information);
                _Proxy.Dispose();
            }

            catch (Exception ex)
            {
                EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
            _Proxy = null;
		    Thread.Sleep(500);
            base.EventLog.WriteEntry("Connector service has stopped. SMSCCode = " + SMSCCode, EventLogEntryType.Information);
        }
	}
}