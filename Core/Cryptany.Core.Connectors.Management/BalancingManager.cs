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
using Cryptany.Core.ConfigOM;
using Cryptany.Core;
using System.Diagnostics;
using System.Messaging;
using Cryptany.Common.Utils;
using Cryptany.Core.Services.Management;

namespace Cryptany.Core.Connectors.Management
{
     internal class BalancingManager
     {
       private ServiceInstanceGroup _sg;

        private int CurrentQueueIndex = 0;

        public BalancingManager(ServiceInstanceGroup sg)
        {

            _sg = sg;
            
        }

        public ServiceInstanceGroup ServiceGroup
        {

            get
            { return _sg; }

        }

        private MessageQueue GetNextQueue()
        {
            //if (Instances.Count == 0)
            //{
            //    return BalancingServiceInstance.Info.Queue;
            //}
            CurrentQueueIndex++;
            if (CurrentQueueIndex == _sg.SMSCs.Count)
            {
                CurrentQueueIndex = 0;
            }
           // return sg.SMSCs[CurrentQueueIndex].Info.Queue;
            return ServicesConfigurationManager.GetOutgoingMessageQueue(_sg.SMSCs[CurrentQueueIndex]);
        }


        private MessageQueue GetSmallestQueue()
        {

            List<MessageQueueSize> counters = new List<MessageQueueSize>();


            foreach (ServiceInstance si in _sg.SMSCs)
            {
                MessageQueue mq = ServicesConfigurationManager.GetOutgoingMessageQueue(si);
                MessageQueueSize mqs = new MessageQueueSize(mq, MSMQ.GetCount(mq));
                counters.Add(mqs);
                Trace.WriteLine("Balancing manager: "+mqs);
            }

            if (counters.Count > 0)
            {
                counters.Sort(new Comparison<MessageQueueSize>(MessageQueueCompare));
                return counters[0].queue;
            }

            Trace.WriteLine("Balancing: no counters");
            return null;
        }

        private struct MessageQueueSize
        {
            public readonly MessageQueue queue;
            public readonly uint size;

            public MessageQueueSize(MessageQueue mq, uint size)
            {
                queue = mq;
                this.size = size;
            }
            public override string  ToString()
            {
                return string.Format(" MessageQueue {0} Size {1}", queue.Path, size);
            }
        }


        private static int MessageQueueCompare(MessageQueueSize x, MessageQueueSize y)
        {
            return x.size.CompareTo(y.size);
        }


         internal bool SendBalancedMessage(OutputMessage om)
        {
            
            return SendBalancedMessage(om, Cryptany.Core.Interaction.MessagePriority.Normal);
        }

         internal bool SendBalancedMessage(OutputMessage om, Cryptany.Core.Interaction.MessagePriority priority)
         {

             MessageQueue mq;
             switch (_sg.Type.Type)
             {
                 case BalancingType.Balancing:
                     mq = GetSmallestQueue();
                     break;
                 case BalancingType.RoundRobin:
                     mq = GetNextQueue();
                     break;
                 default:
                     mq = GetNextQueue();
                     break;
             }
             if (mq != null)
             {

                 System.Messaging.Message m = new System.Messaging.Message(om);
                 m.Priority = System.Messaging.MessagePriority.Normal + (int)priority;
                 m.AttachSenderId = false;
                 m.Formatter = new BinaryMessageFormatter();
                 mq.Send(m);
                 return true;
             }
             Trace.WriteLine("Balancing: Suitable queue not found:" + _sg.Type.Type);
             return false;

         }


    }


}
