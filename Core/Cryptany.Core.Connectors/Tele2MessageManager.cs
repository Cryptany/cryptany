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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cryptany.Common.Logging;
using System.Threading;
using Cryptany.Core.CBGTarifficator;
using Cryptany.Core.Interaction;
using System.Diagnostics;
using Cryptany.Core.Management;

namespace Cryptany.Core
{
    public class Tele2MessageManager : SMPPMessageManagerAsync
    {
        public Tele2MessageManager(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
          
        }


        protected override void Init(AbstractConnectorSettings settings)
        {
            base.Init(settings);
            InitTarifficationThreads(settings.MaxTarifficationThreads);
            sem = new Semaphore(settings.MaxTarifficationThreads, settings.MaxTarifficationThreads);
            t = new Tarifficator(settings.CBGLogin, settings.CBGPassword, Logger);
        }

        public static Semaphore sem;
       
        List<Thread> threadsList;
        Tarifficator t;

        public bool SendToSMSC(OutputMessage outputMessage)
        {
            return base.SendUserData(outputMessage)
                ;

        }

        public override bool SendUserData(OutputMessage outputMessage)
        {
            //lock (this)
            //{
                if (string.IsNullOrEmpty(outputMessage.HTTP_Category))//бесплатное
                    return SendToSMSC(outputMessage);

                 
                else
                {
                    Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name + " Processing Tariffication.Sending To CBG "+outputMessage);

                    sem.WaitOne();//дождаться окончания хотя бы 1 потока
                    Thread completedThread = null;
                    bool canstart = false;

                    while (!canstart)
                    {

                        if (threadsList.Where(th => !th.IsAlive).Count() > 0)
                        {
                            completedThread = threadsList.First(th => (!th.IsAlive));
                            canstart = true;

                        }
                        else Thread.Sleep(10);//еще не завершился

                    }

                    if (completedThread.ThreadState == System.Threading.ThreadState.Unstarted)
                    {
                        completedThread.Start(outputMessage);

                    }

                    else
                    {
                        threadsList.Remove(completedThread);
                        Thread newThread = new Thread(ProcessTariffication);
                        newThread.Name = completedThread.Name;
                        threadsList.Add(newThread);
                        newThread.Start(outputMessage);
                    }
                    return true;
                }
            //}

        }

        private void InitTarifficationThreads(int count)
        {

            threadsList = new List<Thread>(count); 

            for (int i = 0; i < count; i++)
            {
                Thread _th = new Thread(ProcessTariffication);
                threadsList.Add(_th);
                _th.Name = i.ToString();

            }
        }

        private void ProcessTariffication(object om)
        {

                long returnCode=0;
                int status=-1;
                string transactionId="";
                int ProviderTransactionId;
                try
                {
                    OutputMessage msg = (OutputMessage)om;
                    Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name + " Processing Tariffication. OutputMessage " + msg);

                    int amount = int.Parse(msg.HTTP_Category);

                    MessageDeliveryStatus mds = t.TariffAbonent(msg.ID, msg.Destination, amount, out returnCode, out ProviderTransactionId, out status, out transactionId);

                    Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name +
                        " TarifficationResults " + mds + " returnCode " + returnCode + " status " + status);

                    if (mds == MessageDeliveryStatus.Delivered && msg.Type == MessageType.Standard)
                    {
                        Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name + " sending tarifficated message");
                        SendToSMSC(msg);
                    }
                    else
                    {
                        Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name + " updating message state");
                        //Dictionary<string, object> dict = new Dictionary<string, object>();
                        //dict.Add("@providerTransactionId", ProviderTransactionId);
                        UpdateOutboxState(msg, (int)mds, status > -1 ? status.ToString() : returnCode.ToString(), transactionId);

                    }
                }

                catch (Exception ex)
                {
                    Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                    ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                    throw new ApplicationException("Ошибка обработки платного сообщения ", ex);
                   // throw new CBGTarifficationException(returnCode > 0);

                }
                finally
                {
                    Trace.WriteLine("Connector " + SMPPSettings.SMSCCode  +" Thread " + Thread.CurrentThread.Name + " semaphore Released");
                    sem.Release();
                }
               }

    }

    //public class CBGTarifficationException : Exception
    //{

    //    public bool _processed;

    //    public CBGTarifficationException(bool processed)
        
    //    {
    //        this._processed = processed;
           
    //    }

    //    public override string ToString()
    //    {
    //        return  base.ToString()+" processed: "+_processed;
    //    }

    //}

}
