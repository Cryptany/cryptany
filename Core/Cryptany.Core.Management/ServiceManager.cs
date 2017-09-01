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

using System.Configuration;
using System.Messaging;
using Cryptany.Core.MsmqLog;
using System.Diagnostics;
using System;
using Cryptany.Common.Utils;

namespace Cryptany.Core.Management
{
    public static class ServiceManager
    {
        private static Guid _serviceId;
        public static Guid ServiceId
        {
            get { return _serviceId; }
            set
            {
                if (value == Guid.Empty)
                {
                    throw new ApplicationException("Cannot assign Guid.Empty to service Id!");
                }

                _serviceId = value;
            }
        }

        public static void LogEvent(string description, EventType type, EventSeverity severity)
        {
            try
            {
                if (ServiceId == Guid.Empty) return;
                StackTrace st = new StackTrace();
                string source = st.GetFrame(1).GetMethod().DeclaringType.FullName;
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "Kernel.AddServiceEvent";
                me.Parameters.Add("@ServiceId", ServiceId);
                me.Parameters.Add("@eventtype", (int)type);
                me.Parameters.Add("@source", source);
                me.Parameters.Add("@description", description);
                me.Parameters.Add("@severity", (int)severity);
                me.Parameters.Add("@timestamp", DateTime.Now);
                using (MessageQueue mq = MSMQLoggerInputQueue)
                {
                    mq.Send(me);
                }
            }
            catch(InvalidCastException ice)
            {
                
            }
            catch(ApplicationException aex)
            {
                
            }
            catch(MessageQueueException mqex)
            {
                
            }

        }

        public static MessageQueue MSMQLoggerInputQueue
        {
            get
            {
         
                string MSMQLoggerInputQueuePath = string.Empty;
                
                if (ConfigurationManager.AppSettings["MSMQLoggerQueuePath"] != null)
                {
                    MSMQLoggerInputQueuePath = ConfigurationManager.AppSettings["MSMQLoggerQueuePath"];
                }
                if (string.IsNullOrEmpty(MSMQLoggerInputQueuePath))
                    throw new ApplicationException("MSMQLoggerInputQueuePath definition is missing");
         
                MessageQueue _MSMQLoggerInputQueue = new MessageQueue(MSMQLoggerInputQueuePath, false, true, QueueAccessMode.SendAndReceive);
                _MSMQLoggerInputQueue.Formatter = new BinaryMessageFormatter();
                _MSMQLoggerInputQueue.DefaultPropertiesToSend.Recoverable = true;

                return _MSMQLoggerInputQueue;
            }
        }

        public static MessageQueue MessageStateQueue
        {
            get
            {
                string MessageStateQueuePath = string.Empty;
               
                if (ConfigurationManager.AppSettings["MessageStateQueuePath"] != null)
                {
                    MessageStateQueuePath = ConfigurationManager.AppSettings["MessageStateQueuePath"];
                }
                if (string.IsNullOrEmpty(MessageStateQueuePath)) throw new ApplicationException("MessageStateQueuePath definition is missing");
                MessageQueue _messageStateQueue = new MessageQueue(MessageStateQueuePath, false, true, QueueAccessMode.SendAndReceive);
                _messageStateQueue.Formatter = new BinaryMessageFormatter();
                _messageStateQueue.DefaultPropertiesToSend.Recoverable = true;
                return _messageStateQueue;
            }
        }

        public static MessageQueue GetServiceMessageQueue(Guid ServiceId)
        {
            
            string path = string.Format(@"formatname:direct=os:{0}\private$\cryptany_outputqueue{1}", Process.GetCurrentProcess().MachineName, ServiceId);
            MessageQueue result = new MessageQueue(path, false);
            result.Formatter = new BinaryMessageFormatter();
            DefaultPropertiesToSend dps = new DefaultPropertiesToSend();
            result.MessageReadPropertyFilter.ArrivedTime = true;
            dps.Recoverable = true;
            result.DefaultPropertiesToSend = dps;
            return result;
        }

        public static MessageQueue MainInputSMSQueue
        {

            get
            {
                string MainInputQueuePath = string.Empty;
                
                if (ConfigurationManager.AppSettings["InputSMSQueuePath"] != null)
                {
                    MainInputQueuePath = ConfigurationManager.AppSettings["InputSMSQueuePath"];
                }
                if (string.IsNullOrEmpty(MainInputQueuePath)) throw new ApplicationException("MainInputQueuePath is missing");
   
                MessageQueue _MainInputSMSQueue = new MessageQueue(MainInputQueuePath, false);
                _MainInputSMSQueue.DefaultPropertiesToSend.Recoverable = true;
                _MainInputSMSQueue.Formatter = new BinaryMessageFormatter();

                return _MainInputSMSQueue;
            }
        }
    }
}
