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
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Cryptany.Foundation.MailSender;

namespace Cryptany.Core.Monitoring
{
    public partial class MainService : ServiceBase
    {

        private static readonly Hashtable _data = new Hashtable();
        private static readonly ManualResetEvent canModifyData = new ManualResetEvent(true);
        private static ManagementScope ConnectorsMs;
        private static ManagementScope RouterMs;

        private string ConnectorServer;
        private string RouterServer;
        private string MailServer;
        private string MailFrom;
        private string MailTo;

        private int MonitoringInterval;

        private int RouterQueueTimeout;
        private int RouterProcessingTimeout;
        private int RouterSendToConnectorTimeout;
        private int ConnectorQueueTimeout;
           
        private Thread thMonitoringWork;
        public MainService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ConnectorServer = ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.ConnectorServer"];
            ConnectorsMs = new ManagementScope(@"\\"+ConnectorServer+@"\root\cimv2\Applications\Cryptany");
            RouterServer = ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.RouterServer"];
            RouterMs = new ManagementScope(@"\\" + RouterServer + @"\root\cimv2\Applications\Cryptany");

            MailServer = ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.MailServer"];
            MailFrom = ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.MailFrom"];
            MailTo = ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.MailTo"];

            RouterQueueTimeout = int.Parse(ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.RouterQueueTimeout"]);
            RouterProcessingTimeout = int.Parse(ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.RouterProcessingTimeout"]);
            RouterSendToConnectorTimeout = int.Parse(ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.RouterSendToConnectorTimeout"]);
            ConnectorQueueTimeout = int.Parse(ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.ConnectorQueueTimeout"]);
            
            ConnectionOptions oConn = new ConnectionOptions();
            oConn.Username = "monitoring";
            oConn.Password = "Fvcnthlfv185";
            RouterMs.Options = oConn;
            RouterMs.Connect();

            
            ConnectorsMs.Connect();
            
            
            thMonitoringWork = new Thread(Thread_Monitoring);
            thMonitoringWork.Priority = ThreadPriority.AboveNormal;
            thMonitoringWork.IsBackground = false;
            thMonitoringWork.Name = "MonitoringThread";
            thMonitoringWork.Start();
        }

        private void Thread_Monitoring()
        {
            Thread thReceived = new Thread(Thread_CatchReceived);
            thReceived.IsBackground = true;
            Thread thProcessing = new Thread(Thread_CatchProcessing);
            thProcessing.IsBackground = true;
            Thread thProcessed = new Thread(Thread_CatchProcessed);
            thProcessed.IsBackground = true;
            Thread thSubmitted = new Thread(Thread_CatchSubmitted);
            thSubmitted.IsBackground = true;
            Thread thSent = new Thread(Thread_CatchSent);
            thSent.IsBackground = true;
            try
            {
                MonitoringInterval = int.Parse(ConfigurationManager.AppSettings["Cryptany.Core.Monitoring.MonitoringInterval"]);
                
                thReceived.Start();
                
                thProcessing.Start();
                
                thProcessed.Start();
                
                thSubmitted.Start();
                
                thSent.Start();

                while (true)
                {
                    ObserveData();
                    Thread.Sleep(TimeSpan.FromMinutes(MonitoringInterval));
                }
            }
            catch (ThreadAbortException)
            {
                //thReceived.Abort();
                //thReceived.Join();
                //thProcessing.Abort();
                //thProcessing.Join();
                //thProcessed.Abort();
                //thProcessed.Join();
                //thSubmitted.Abort();
                //thSubmitted.Join();
                //thSent.Abort();
                //thSent.Join();
            }
        }


        private void Thread_CatchReceived()
        {
            try
            {
                using (ManagementEventWatcher _watcher = new ManagementEventWatcher(ConnectorsMs, new EventQuery("SELECT * FROM SMSReceivedEvent")))
                {
                    while (true)
                    {
                        ManagementBaseObject mbo = _watcher.WaitForNextEvent();
                        if (mbo != null)
                        {
                            Trace.WriteLine("Monitoring: SMSReceived");
                            MonitoringData md = new MonitoringData();
                            md.id = new Guid(mbo.Properties["ID"].Value.ToString());
                            md.smscid = int.Parse(mbo.Properties["SMSCId"].Value.ToString());
                            md.dtReceived = DateTime.Parse(mbo.Properties["Time"].Value.ToString());
                            canModifyData.WaitOne();
                            _data.Add(md.id, md);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                base.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }

        }

        private void Thread_CatchProcessing()
        {
            try
            {
                using (ManagementEventWatcher _watcher = new ManagementEventWatcher(RouterMs, new EventQuery("SELECT * FROM SMSProcessingEvent")))
                {
                    while (true)
                    {
                        ManagementBaseObject mbo = _watcher.WaitForNextEvent();
                        if (mbo != null)
                        {
                            Trace.WriteLine("Monitoring: SMSProcessing");
                            Guid id = new Guid(mbo.Properties["ID"].Value.ToString());
                            if (_data.ContainsKey(id))
                            {
                                MonitoringData md = (MonitoringData)_data[id];
                                md.dtProcessing = DateTime.Parse(mbo.Properties["Time"].Value.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                base.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private void Thread_CatchProcessed()
        {
            try
            {
                using (ManagementEventWatcher _watcher = new ManagementEventWatcher(RouterMs, new EventQuery("SELECT * FROM SMSProcessedEvent")))
                {
                    while (true)
                    {
                        ManagementBaseObject mbo = _watcher.WaitForNextEvent();

                        if (mbo != null)
                        {
                            Trace.WriteLine("Monitoring: SMSProcessed");
                            Guid id = new Guid(mbo.Properties["InboxID"].Value.ToString());
                            if (_data.ContainsKey(id))
                            {
                                MonitoringData md = (MonitoringData)_data[id];
                                md.dtProcessed = DateTime.Parse(mbo.Properties["Time"].Value.ToString());
                                md.Count = ((ushort)mbo.Properties["SMSCount"].Value);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Monitoring: " + ex);
                base.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private void Thread_CatchSubmitted()
        {
            try
            {
                using (
                    ManagementEventWatcher _watcher = new ManagementEventWatcher(RouterMs,
                                                                                 new EventQuery(
                                                                                     "SELECT * FROM SMSSubmittedEvent"))
                    )
                {
                    while (true)
                    {
                        ManagementBaseObject mbo = _watcher.WaitForNextEvent();

                        if (mbo != null)
                        {
                            Trace.WriteLine("Monitoring: SMSSubmitted");

                            Guid id = new Guid(mbo.Properties["InboxID"].Value.ToString());
                            if (_data.ContainsKey(id))
                            {
                                MonitoringData md = (MonitoringData) _data[id];
                                SMSData s = new SMSData();
                                s.dtSubmitted = DateTime.Parse(mbo.Properties["Time"].Value.ToString());
                                s.id = new Guid(mbo.Properties["ID"].Value.ToString());
                                s.smscid = int.Parse(mbo.Properties["SMSCId"].Value.ToString());
                                canModifyData.WaitOne();
                                md.sms.Add(s.id, s);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                base.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private void Thread_CatchSent()
        {
            try
            {
                using (ManagementEventWatcher _watcher = new ManagementEventWatcher(ConnectorsMs, new EventQuery("SELECT * FROM SMSSentEvent")))
                {
                    while (true)
                    {
                        ManagementBaseObject mbo = _watcher.WaitForNextEvent();

                        if (mbo != null)
                        {
                            Trace.WriteLine("Monitoring: SMSSent");
                            Guid id = new Guid(mbo.Properties["InboxID"].Value.ToString());
                            if (_data.ContainsKey(id))
                            {
                                MonitoringData md = (MonitoringData)_data[id];
                                Guid sid = new Guid(mbo.Properties["ID"].Value.ToString());
                                if (md.sms.ContainsKey(sid))
                                {
                                    SMSData s = (SMSData)md.sms[id];
                                    s.dtSent = DateTime.Parse(mbo.Properties["Time"].Value.ToString());

                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                base.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
            }
        }

        private void ObserveData()
        {
            try
            {
                canModifyData.Reset();
                List<Guid> _itemstoRemove = new List<Guid>();

                StringBuilder sb = new StringBuilder();

                
                foreach (MonitoringData md in _data.Values)
                {
                    if (md.dtProcessing != DateTime.MinValue)
                    {
                        if (md.dtProcessing - md.dtReceived > TimeSpan.FromMinutes(RouterQueueTimeout))
                        {
                            Trace.WriteLine("Monitoring: too large router queue size (performance degraded) ");
                            sb.AppendLine(" too large router queue size (performance degraded)");
                        }
                        if (md.dtProcessed != DateTime.MinValue)
                        {
                            if (md.sms.Count != md.Count)
                            {
                                if (DateTime.Now - md.dtProcessed > TimeSpan.FromMinutes(RouterSendToConnectorTimeout))
                                {
                                    Trace.WriteLine("Monitoring: error sending SMS from router to connector");
                                    sb.AppendLine("error sending SMS from router to connector " + md.smscid);
                                }
                            }
                            else
                            {
                                bool canremove = true;
                                foreach (SMSData s in md.sms.Values)
                                {
                                    if (s.dtSent == DateTime.MinValue &&
                                        DateTime.Now - s.dtSubmitted > TimeSpan.FromMinutes(ConnectorQueueTimeout))
                                    {
                                        Trace.WriteLine("Monitoring: connector message queue is too large, it possibly not working");
                                        sb.AppendLine("connector message queue is too large, it possibly not working: " + md.smscid );
                                        //очередь в коннекторе или он не работает
                                        ServiceController sc = new ServiceController("Cryptany.ConnectorService" + s.smscid, ConnectorServer);
                                        if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.StartPending)
                                        {
                                            Trace.WriteLine("Monitoring: connector is stopped. Starting service");
                                            sb.AppendLine("connector is stopped. Starting service");
                                            sc.Start();
                                            Thread.Sleep(1000);
                                        }
                                        else
                                        {
                                            if (PerformanceCounterCategory.InstanceExists(s.smscid.ToString(), "Connector Service"))
                                            {
                                                PerformanceCounter pc = new PerformanceCounter("Connector Service",
                                                                                               "Connection State",
                                                                                               s.smscid.ToString(),
                                                                                               ConnectorServer);
                                                if (pc.NextSample().RawValue == 0) //нет коннекта с SMSC
                                                {
                                                    Trace.WriteLine("Monitoring: нет подключения к SMSC " + s.smscid);
                                                    sb.AppendLine("нет подключения к SMSC " + s.smscid);
                                                }
                                                else
                                                {

                                                    ObjectQuery oq =
                                                        new ObjectQuery(
                                                            @"select * from SMPPConnectorState WHERE Code = " +
                                                            s.smscid);
                                                    ManagementObjectSearcher searcher =
                                                        new ManagementObjectSearcher(ConnectorsMs, oq);
                                                    ManagementObjectCollection moc = searcher.Get();
                                                    if (moc.Count > 0)
                                                    {
                                                        foreach (ManagementObject mo in moc)
                                                        {
                                                            DateTime dtLastSentToSmsc =
                                                                DateTime.Parse(
                                                                    (string) mo.Properties["LastPDUOutTime"].Value);
                                                            if (DateTime.Now - dtLastSentToSmsc >
                                                                TimeSpan.FromMinutes(5))
                                                            {
                                                                Trace.WriteLine(
                                                                    "Monitoring: Connection is established but SMS not sending: " +
                                                                    s.smscid + " Restarting service.");
                                                                sb.AppendLine(
                                                                    "Connection is established but SMS not sending: " +
                                                                    s.smscid + " Restarting service.");
                                                                sc.Stop();
                                                                while (sc.Status != ServiceControllerStatus.Stopped)
                                                                {
                                                                    Thread.Sleep(1000);
                                                                }
                                                                sc.Start();
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        canremove = false;
                                        break;
                                    }

                                }

                                if (canremove)
                                    _itemstoRemove.Add(md.id);
                            }

                        }
                        else
                        {
                            if (DateTime.Now - md.dtProcessing > TimeSpan.FromMinutes(RouterProcessingTimeout))
                            {
                                Trace.WriteLine("Monitoring: error processing SMS in router");
                                sb.AppendLine("Error processing SMS in router");
                                // ошибка обработки смс в роутере
                            }
                        }

                    }
                    else if (DateTime.Now - md.dtReceived > TimeSpan.FromMinutes(RouterQueueTimeout))
                    {
                        Trace.WriteLine("Monitoring: Message didn't pass to router");
                        sb.AppendLine("Message didn't pass to router");
                        ServiceController sc = new ServiceController("Cryptany.RouterService1", RouterServer);
                        if (sc.Status != ServiceControllerStatus.Running && sc.Status != ServiceControllerStatus.StartPending)
                        {
                            Trace.WriteLine("Monitoring: Router is stopped. Starting service");
                            sb.AppendLine("Router is stopped. Starting service");
                            sc.Start();
                            Thread.Sleep(1000);
                        }
                        // ошибка в коннекторе md.smscid (сообщение не дошло до роутера)
                        _itemstoRemove.Add(md.id);

                    }

                    
                }
                if (_data.Count == 0)
                {
                    Trace.WriteLine("Monitoring: There's no single WMI-event during last " + MonitoringInterval + " minutes");
                    sb.AppendLine("There's no single WMI-event during last " + MonitoringInterval + " minutes");
                }
                else
                {
                    foreach (Guid id in _itemstoRemove)
                    {
                        _data.Remove(id);
                    }
                }


                if (sb.Length > 0)
                {
                    Trace.WriteLine("Monitoring: sending email");
                    MailSender ms = new MailSender();
                    ms.SendMail( "Transport system monitoring", sb.ToString());
                }

            }
            catch(Exception ex)
            {
                Trace.WriteLine("Monitoring:" + ex);
                base.EventLog.WriteEntry(ex.ToString(), EventLogEntryType.Error);
                MailSender ms = new MailSender();
                ms.SendMail( "Transport system monitoring", "Error in monitoring service: " + ex);
                Stop();
            }
            finally
            {
                canModifyData.Set();
            }

        }

        protected override void OnStop()
        {
            thMonitoringWork.Abort();
            thMonitoringWork.Join();

        }
    }
}
