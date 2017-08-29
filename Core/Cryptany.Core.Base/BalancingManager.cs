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
using System.Messaging;
using Cryptany.Core.Base.Management;
using Cryptany.Core.DB;
using Cryptany.Common.Utils;
using System.Diagnostics;
using Message = Cryptany.Core.Message;
using MessagePriority = Cryptany.Core.Interaction.MessagePriority;
using Cryptany.Core;
using Cryptany.Core.Interaction;
using Cryptany.Core.MsmqLog;

namespace Cryptany.Core.Base
{
    static public class BalancingManager
    {
        //static readonly ILogger _logger = LoggerFactory.Logger;
        static Dictionary<ServiceGroup, int> counter = new Dictionary<ServiceGroup, int>();
        //static TransportEntities data = TransportEntities.Entities;
        #region Send'ы
        static public bool Send(OutputMessage message, MessagePriority priority)
        {
            ServiceGroup group = ResolveServiceGroup(message);
            SMSC connector = ChooseConnectorForGroup(group);
            return Send(message, connector.Id, priority);
        }

        static public bool Send(OutputMessage msg, Guid smscId, MessagePriority messagePriority)
        {

            //SMSC smsc = data.GetEntityById<SMSC>(smscId);
            //data.GetBrandGuidBySN("79035618208");
            if (smscId == Guid.Empty)
                throw new ArgumentException(string.Format("Неверный id sms-центра для сообщения: {0}", msg), "smscId");

            MessageQueue queue = ServiceManager.GetOutgoingMessageQueue(smscId);
            if (queue != null)
            {

                System.Messaging.Message message = new System.Messaging.Message(msg);
                message.Formatter = new BinaryMessageFormatter();
                message.Priority = (System.Messaging.MessagePriority.Normal + (int)messagePriority);

                AddToOutbox(msg);

                queue.Send(message);

                Trace.WriteLine("Message " + message + "added to outbox and to " + queue.Path);

                return true;
            }
            return false;
        }

        static public bool Send(OutputMessage message, int SMSCCode, MessagePriority priority)
        {
            SMSC smsc = DBCache.GetSMSCByCode(SMSCCode);
            return Send(message, smsc.Id, priority);
        }

        #endregion 
        #region Rules checking
        static private ServiceGroup ResolveServiceGroup(OutputMessage message)
        {
            ServiceGroup result = null;
            foreach (ServiceGroup group in DBCache.ServiceGroups)
                if (CheckGroupRules(group.Rules, message))
                {
                    if (result == null)
                        result = group;
                    else
                        throw new ApplicationException("Не однозначная сервисная группа для сообщения " + message);
                }

            if (result == null)
                throw new ApplicationException("Не удалось найти подхдящую сервисную группу для сообщения " + message);
            return result;
        }

        static private bool CheckGroupRules(IEnumerable<ServiceGroupRule> rules, OutputMessage message)
        {
            foreach (ServiceGroupRule rule in rules)
                if (!CheckGroupRule(rule, message))
                    return false;
            return true;
        }

        static private bool CheckGroupRule(ServiceGroupRule rule, OutputMessage message)
        {
            Guid checkingValue;
            Guid[] values = rule.Value.Split(';').Select(a => { return new Guid(a); }).ToArray();
            switch (rule.ParameterType)
            {
                case ParameterTypeValue.SN:
                    checkingValue = DBCache.GetServiceNumberBySN(message.Source).Id;
                    break;
                case ParameterTypeValue.Tariff:
                    checkingValue = message.TariffId;
                    break;
                case ParameterTypeValue.Resource:
                    checkingValue = message.ProjectID;
                    break;
                case ParameterTypeValue.RegionGroup:
                    checkingValue = DBCache.GetRegionGroupGuidBySN(message.Destination);
                    break;
                case ParameterTypeValue.Brand:
                    checkingValue = DBCache.GetBrandGuidBySN(message.Destination);
                    break;
                default:
                    throw new ApplicationException("Неизвестный ParameterType в функции CheckGroupRule");
            }


            switch (rule.CheckType)
            {
                case CheckTypeValue.Equals:
                    return checkingValue.Equals(values.FirstOrDefault());
                case CheckTypeValue.NotEquals:
                    return !checkingValue.Equals(values.FirstOrDefault());
                case CheckTypeValue.In:
                    return values.Contains(checkingValue);
                case CheckTypeValue.NotIn:
                    return !values.Contains(checkingValue);
            }
            throw new ApplicationException("Неизвестный CheckType в функции CheckGroupRule");
        }
        #endregion 
        //TODO: Количество сообщений во всех очередях обнуляется на каждом 32 запросе размера
        static private SMSC ChooseConnectorForGroup(ServiceGroup group)
        {
            SMSC result = null;
            uint minlen = uint.MaxValue;
            //Console.WriteLine("------------------");
            foreach (SMSC c in group.Connnectors)
            {
                //ServiceManager.GetOutgoingMessageQueue(c).GetCount();
                uint len = ServiceManager.GetOutgoingMessageQueue(c.Id).GetCount();
                //Console.WriteLine(c.Code.ToString() + " : " + len.ToString());
                if (len < minlen)
                {
                    minlen = len;
                    result = c;
                }
            }
            //Console.WriteLine("result : " + result.Code);
            return result;
        }


        /// <summary>
        /// Создает объект логгирования OutboxEntry и помещает его в очередь MSMQ логгера
        /// </summary>
        public static void AddToOutbox(OutputMessage msg)
        {
            try
            {
                string serviceNumber = Message.GetServiceNumber(msg.Source);
                ServiceNumber sn = DBCache.GetServiceNumberBySN(serviceNumber);

                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "Kernel.AddToOutbox";
                me.Parameters.Add("@ID", msg.ID);
                me.Parameters.Add("@Body", msg.TypedBody);
                me.Parameters.Add("@MsgTypeID", (int)msg.Type);


                me.Parameters.Add("@InboxID", msg.InboxMsgID != Guid.Empty ? msg.InboxMsgID : (object)DBNull.Value);
                me.Parameters.Add("@MessageTime", DateTime.Now);
                me.Parameters.Add("@SNId", sn.Id);

                me.Parameters.Add("@MSISDN", msg.Destination);
                if (!string.IsNullOrEmpty(msg.HTTP_Category)) //рассылка и указана тарификация
                {
                    me.Parameters.Add("@OperatorParameter", msg.HTTP_Category);
                }
                if (msg.TariffId != Guid.Empty)
                {
                    me.Parameters.Add("@TariffId", msg.TariffId);
                }
                if (msg.ProjectID != Guid.Empty)
                {
                    me.Parameters.Add("@ResourceId", msg.ProjectID);
                }

                using (MessageQueue _MSMQLoggerInputQueue = ServiceManager.MSMQLoggerInputQueue)//добавляем только в транспортную базу, для инсертов в остальные системы может не хватать данных
                {
                    _MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (ApplicationException ex)
            {
                //_logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                Trace.WriteLine(ex.ToString() + Environment.NewLine + ex.StackTrace + Environment.NewLine);
            }
            catch (MessageQueueException e)
            {
                //_logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                Trace.WriteLine(e.ToString() + Environment.NewLine + e.StackTrace + Environment.NewLine);
            }
        }


    }
}
