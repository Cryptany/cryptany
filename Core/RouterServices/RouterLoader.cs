using System.Diagnostics;
using System.ServiceProcess;
using System;

namespace RouterServices
{
    internal class RouterLoader
    {
        // The main entry point for the process
        
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            // Load all services into memory
            ServiceBase[] sb = new ServiceBase[]
                                   {
                                       //new RouterMainService(), 
                                       new RouterService(1)
                                       //, new RouterService(2),
                                       //new RouterService(3)
                                   };
            ServiceBase.Run(sb);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            EventLog.WriteEntry("Application", e.ExceptionObject.ToString());
            
        }
    }
}