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
using System.ComponentModel;
using System.Diagnostics;
using System.Messaging;
using System.Net;
using System.Security.Principal;
using System.ServiceProcess;
using Cryptany.Core;

namespace Cryptany.Core.ConnectorServices
{
    class ConnectorLoader
    {
        private static int _code;

        // The main entry point for the process
        [LoaderOptimization( LoaderOptimization.SingleDomain)]
        public static void Main(string[] args)
        {
            Console.WriteLine(Process.GetCurrentProcess().MainModule.FileName);
            Console.ReadLine();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.DomainUnload += CurrentDomain_DomainUnload;
            // Load all services into memory
            if (args.Length > 0)
            {
                if (args[0] == "-i")
                {
                    ConnectorSettings cs;
                    if (args.Length > 1)
                    {
                        try
                        {
                            cs = SMSProxy.LoadSettingsFromDB(int.Parse(args[1]));
                          
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.Read();
                            return;
                        }
                        if (cs == null)
                        {
                            Console.WriteLine("SMSC not found!");
                            Console.Read();
                            return;
                        }
                        try
                        {
                            Installer inst = new Installer();
                            inst.InstallService(Process.GetCurrentProcess().MainModule.FileName + " /svc " + args[1],
                                                "Cryptany.ConnectorService" + args[1], "Cryptany.ConnectorService" + args[1],
                                                cs.Name);

                            CreateOutputQueue(cs.ID);
                            InstallPerformanceCounters();
                            if (!EventLog.SourceExists("Cryptany.ConnectorService" + args[1]))
                            {
                                EventLog.CreateEventSource("Cryptany.ConnectorService" + args[1], "Application");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Exception while trying to install service: {0}", e);
                        }
                    }
                }
                else if (args[0] == "-u")
                {
                    if (args.Length > 1)
                    {
                        ConnectorSettings cs;
                        try
                        {

                            cs = SMSProxy.LoadSettingsFromDB(int.Parse(args[1]));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            Console.ReadKey();
                            return;
                        }
                        if (cs == null)
                        {

                            Console.WriteLine("SMSC not found!");
                            Console.ReadKey();
                            return;
                        }


                        Installer inst = new Installer();
                        inst.UnInstallService("Cryptany.ConnectorService" + args[1]);

                        DeleteOutputQueue(cs.ID);
                    }
                    else
                    {
                        ServiceController[] scs= ServiceController.GetServices();
                        Installer inst = new Installer();
                        foreach(ServiceController sc in scs)
                        {
                            if (sc.ServiceName.StartsWith("Cryptany.ConnectorService"))
                            {
                                inst.UnInstallService(sc.ServiceName);
                            }
                        }
                        DeleteOutputQueues();
                        UninstallPerformanceCounters();
                    }
                }
                else if (args[0].Equals("/svc"))
                {
                    _code = int.Parse(args[1]);
                    ServiceBase sb = new ConnectorService(_code);
                    ServiceBase.Run(sb);
                }
            }
            
               
            
        }

        static void CurrentDomain_DomainUnload(object sender, EventArgs e)
        {
            if (_code>0)
                EventLog.WriteEntry("Cryptany.ConnectorService" + _code, "AppDomain is about to unload.", EventLogEntryType.Information);
        }
        private static void InstallPerformanceCounters()
        {
            if (PerformanceCounterCategory.Exists("Connector Service"))
            {
                PerformanceCounterCategory.Delete("Connector Service");
            }
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();

                CounterCreationData opsPerSecond1 = new CounterCreationData();
                opsPerSecond1.CounterName = "# incoming messages / sec";
                opsPerSecond1.CounterHelp = "Number of incoming messages from SMSC per second";
                opsPerSecond1.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(opsPerSecond1);

                CounterCreationData opsPerSecond2 = new CounterCreationData();
                opsPerSecond2.CounterName = "# outgoing messages / sec";
                opsPerSecond2.CounterHelp = "Number of outgoing messages to SMSC per second";
                opsPerSecond2.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(opsPerSecond2);

                CounterCreationData ConnectionState = new CounterCreationData();
                ConnectionState.CounterName = "Connection State";
                ConnectionState.CounterHelp = "State of the SMSC connection (1-connected, 0-disconnected)";
                ConnectionState.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(ConnectionState);

                CounterCreationData SmsProcessing = new CounterCreationData();
                SmsProcessing.CounterName = "SMS processing time, ms";
                SmsProcessing.CounterHelp = "SMS processing time";
                SmsProcessing.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(SmsProcessing);

                CounterCreationData SmsLoading = new CounterCreationData();
                SmsLoading.CounterName = "Time in outgoing queue (average)";
                SmsLoading.CounterHelp = "Time in outgoing queue";
                SmsLoading.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(SmsLoading);

                CounterCreationData SmsResendCount = new CounterCreationData();
                SmsResendCount.CounterName = "SMS resend to SMSC count/sec";
                SmsResendCount.CounterHelp = "SMS resend to SMSC count";
                SmsResendCount.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(SmsResendCount);

                CounterCreationData OutSmsAverage = new CounterCreationData();
                OutSmsAverage.CounterName = "sended sms";
                OutSmsAverage.CounterHelp = "average sended sms/hour";
                OutSmsAverage.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(OutSmsAverage);

                CounterCreationData InSmsAverage = new CounterCreationData();
                InSmsAverage.CounterName = "received sms";
                InSmsAverage.CounterHelp = "average sended sms/hour";
                InSmsAverage.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(InSmsAverage);

                CounterCreationData MsgswaitingReceits = new CounterCreationData();
                MsgswaitingReceits.CounterName = "waiting receits messages cache";
                MsgswaitingReceits.CounterHelp = "number of sent and waiting receits messages ";
                MsgswaitingReceits.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(MsgswaitingReceits);
                // create new category with the counters above

                PerformanceCounterCategory.Create("Connector Service",  "Connector Service category ", PerformanceCounterCategoryType. MultiInstance, counters);

            }



            if (PerformanceCounterCategory.Exists("Connector Service:SMPP"))
            {
                PerformanceCounterCategory.Delete("Connector Service:SMPP");
            }
            {
                CounterCreationDataCollection counters = new CounterCreationDataCollection();


                CounterCreationData opsPerSecond1 = new CounterCreationData();
                opsPerSecond1.CounterName = "Incoming PDU response time, ms";
                opsPerSecond1.CounterHelp = "Incoming PDU response time";
                opsPerSecond1.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(opsPerSecond1);

                CounterCreationData opsPerSecond2 = new CounterCreationData();
                opsPerSecond2.CounterName = "Outgoing PDU response time, ms";
                opsPerSecond2.CounterHelp = "Outgoing PDU response time";
                opsPerSecond2.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(opsPerSecond2);

            // create new category with the counters above

                PerformanceCounterCategory.Create("Connector Service:SMPP",
                        "Connector Service:SMPP category ", PerformanceCounterCategoryType.MultiInstance, counters);
            }
        }

        private static void UninstallPerformanceCounters()
        {
            if (PerformanceCounterCategory.Exists("Connector Service"))
            {
                PerformanceCounterCategory.Delete("Connector Service");
            }

            if (PerformanceCounterCategory.Exists("Connector Service:SMPP"))
            {
                PerformanceCounterCategory.Delete("Connector Service:SMPP");
            }
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
                Exception ex = e.ExceptionObject as Exception;
                EventLog.WriteEntry("Cryptany.ConnectorService" + _code, "Unhandled exception in connector " + _code + ": " + ex ?? "", EventLogEntryType.Error);
        }

        private static void CreateOutputQueue(Guid id)
        {
            if (!MessageQueue.Exists(@".\private$\cryptany_outputqueue" + id))
            {
                MessageQueue mq = MessageQueue.Create(@".\private$\cryptany_outputqueue" + id);
                mq.SetPermissions("NT AUTHORITY\\NETWORK SERVICE", MessageQueueAccessRights.FullControl);
            }          
        }

        private static void DeleteOutputQueue(Guid id)
        {
            if (MessageQueue.Exists(@".\private$\cryptany_outputqueue" + id))
            {
                MessageQueue q = new MessageQueue(@".\private$\cryptany_outputqueue" + id);
                
                q.SetPermissions(WindowsIdentity.GetCurrent().Name, MessageQueueAccessRights.FullControl);
                MessageQueue.Delete(@".\private$\cryptany_outputqueue" + id);
            }
            
        }

        private static void DeleteOutputQueues()
        {
            MessageQueue[] mqs =  MessageQueue.GetPrivateQueuesByMachine(".");
            foreach(MessageQueue mq in mqs)
            {
                if (mq.Path.Contains("cryptany_outputqueue"))
                {
                    MessageQueue.Delete(mq.Path);
                }
            }
        }


      
        
    }
    [RunInstaller(false)]
    public class ConnectorInstaller : System.Configuration.Install.Installer
    {
        private ServiceProcessInstaller serviceProcessInstaller;
        

        private Container components = null;

        public ConnectorInstaller()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            serviceProcessInstaller = new ServiceProcessInstaller();
            serviceProcessInstaller.Account = ServiceAccount.User;
        }

    }
}