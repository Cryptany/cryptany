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
using System.Configuration;
using System.Messaging;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Management;
using Cryptany.Core.MsmqLog;
using Cryptany.Core.Services.Management;
using Cryptany.Core.DPO;
using Cryptany.Core;
using Cryptany.Common.Utils;
using Cryptany.Core.Interaction;
using Cryptany.Common.Logging;
using System.Linq;

namespace Cryptany.Core.Connectors.Management
{
    /// <summary>
    /// Distribute messages among connectors in a group
    /// </summary>
    public class ConnectorManager
    {
        private static readonly ILogger _logger = LoggerFactory.Logger;
        private static readonly Dictionary<Guid, List<ServiceInstanceGroup>> _serviceGroups = new Dictionary<Guid, List<ServiceInstanceGroup>>();
        private static List<BalancingManager> _managers=new List<BalancingManager>();

        static ConnectorManager()
        {
            _logger.DefaultSource = "Cryptany.Core.Connectors.Management.ConnectorManager";
            _logger.DefaultServiceSource = "ConnectorManager";
            InitServiceGroups();
        }

        /// <summary>
        /// Заполняет коллекцию сервисных групп из kernel.ServiceGroups
        /// </summary>
        private static void InitServiceGroups()
        {
            List<ServiceInstanceGroup> sigs = ChannelConfiguration.DefaultPs.GetEntities<ServiceInstanceGroup>();
            foreach(ServiceInstanceGroup sig in sigs)
            {
                foreach(ContragentResource cr in sig.ContragentResources)
                {
                    if (!_serviceGroups.ContainsKey((Guid)cr.ID))
                    {
                        _serviceGroups.Add((Guid) cr.ID, new List<ServiceInstanceGroup>());
                    }
                    _serviceGroups[(Guid) cr.ID].Add(sig);
                }
                _managers.Add(new BalancingManager(sig));
            }
            _logger.Write(new LogMessage("Загружено сервисных групп " + sigs.Count, LogSeverity.Debug));
        }

        /// <summary>
        /// Отсылает сообщение
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <returns>успех/провал</returns>
        public bool Send(OutputMessage msg)
        {
            return Send(msg, msg.Priority);
        }

        /// <summary>
        /// Отсылает сообщение, исходя из свойств, указанных в сообщении
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="messagePriority">приоритет сообщения внутри проекта</param>
        /// <returns>успех/провал</returns>
        public bool Send(OutputMessage msg, Interaction.MessagePriority messagePriority)
        {
            if (msg.ProjectID != Guid.Empty && _serviceGroups.ContainsKey(msg.ProjectID)) //отсылаем только, если указан проект-отправитель
            {
                List<ServiceInstanceGroup> sgs = _serviceGroups[msg.ProjectID];
                foreach (ServiceInstanceGroup sg in sgs)
                {
                    if (CheckRules(sg, msg))
                    {
                        //отсылаем
                        AddToOutbox(msg);
                        if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["TimeToLive"]))
                        {
                            int minutes;
                            if (int.TryParse(ConfigurationManager.AppSettings["TimeToLive"], out minutes))
                            {
                                msg.TTL = DateTime.Now.AddMinutes(minutes);
                            }
                        }
                        return _managers.Single(man => man.ServiceGroup == sg).SendBalancedMessage(msg, messagePriority);
                    }
                }
                throw new ApplicationException("Для проекта с id " + msg.ProjectID + " запрещена отсылка смс " + msg.ToString());
            }
            throw new ApplicationException("Не указан ресурс контрагента.");
        }

        /// <summary>
        /// проверить, удовлетворяет ли сообщение правилам сервисной группы
        /// </summary>
        /// <param name="sg"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        private static bool CheckRules(ServiceInstanceGroup sg, OutputMessage msg)
        {

            foreach(ServiceGroupRule sgr in sg.Rules)
            {
                
                if (!CheckRule(sgr, msg))
                {
                    return false;
                }
            }
               
            return true;   
        }

        private static bool CheckRule(ServiceGroupRule sgr, OutputMessage msg)
        {
            bool res = false;
            foreach (ServiceGroupRuleParameter sgrp in sgr.Parameters)
            {
                switch (sgrp.CheckType.CheckType)
                {
                    case RuleParameterCheckType.Equals:
                        switch (sgrp.Type.Type)
                        {
                            case RuleParameterType.ServiceNumber:
                                res = msg.Source == sgrp.Value;
                                break;
                            case RuleParameterType.Tariff:
                                res = msg.TariffId == new Guid(sgrp.Value);
                                break;
                        }
                        break;
                    case RuleParameterCheckType.NotEquals:
                        switch (sgrp.Type.Type)
                        {
                            case RuleParameterType.ServiceNumber:
                                res = msg.Source != sgrp.Value;
                                break;
                        }
                        break;
                    case RuleParameterCheckType.In:
                        switch (sgrp.Type.Type)
                        {
                            case RuleParameterType.ServiceNumber:
                                string[] sns = sgrp.Value.Split(';');
                                foreach (string sn in sns)
                                {
                                    if (msg.Source == sn)
                                    {
                                        res = true;
                                        break;
                                    }
                                }
                                break;
                            case RuleParameterType.Tariff:
                                string[] tariffs = sgrp.Value.Split(';');
                                foreach (string t in tariffs)
                                {
                                    if (msg.TariffId == new Guid(t))
                                    {
                                        res = true;
                                        break;
                                    }
                                }
                                break;
                        }
                        break;
                    case RuleParameterCheckType.NotIn:
                        switch (sgrp.Type.Type)
                        {
                            case RuleParameterType.ServiceNumber:
                                string[] sns = sgrp.Value.Split(';');
                                foreach (string sn in sns)
                                {   
                                    if (msg.Source == sn)
                                    {
                                        res = false;
                                        break;
                                    }
                                }
                                break;
                        }
                        break;
                }
                if (!res) return false;
            }
            return res;
         
        }

        /// <summary>
        /// Отсылает сообщение через явно указанный канал
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="smscId">SMSC ID</param>
        /// <returns>успех/провал</returns>
        public bool Send(OutputMessage msg, Guid smscId)
        {
            return Send(msg, smscId, Interaction.MessagePriority.Normal);
        }

        /// <summary>
        /// Отсылает сообщение по коду коннектора с приоритетом Normal
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="connectorCode">код коннектора (SMSC)</param>
        /// <returns>успех/провал</returns>
        public bool Send(OutputMessage msg, int connectorCode)
        {
            return Send(msg, connectorCode, Interaction.MessagePriority.Normal);
        }

        /// <summary>
        /// Отсылает сообщение по коду коннектора с указанным приоритетом
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="connectorCode">код коннектора (SMSC)</param>
        /// <param name="priority">приоритет сообщения</param>
        /// <returns>успех/провал</returns>
        public bool Send(OutputMessage msg, int connectorCode, Interaction.MessagePriority priority)
        {
            try
            {
                SMSC s = SMSC.GetSMSCByCode(connectorCode);
                if (s == null)
                {
                    _logger.Write(new LogMessage("Не найден коннектор с кодом " + connectorCode, LogSeverity.Error));
                    return false;
                }
                return Send(msg, s.DatabaseId,priority);
            }
            catch (Exception ex)
            {
                _logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                return false;
            }
        }
        
        /// <summary>
        /// Отсылает сообщение, исходя из свойств, указанных в сообщении
        /// </summary>
        /// <param name="msg">сообщение</param>
        /// <param name="smscId">SMSC ID</param>
        /// <param name="messagePriority">приоритет сообщения</param>
        /// <returns>успех/провал</returns>
        public bool Send(OutputMessage msg, Guid smscId, Interaction.MessagePriority messagePriority)
        {
            
			SMSC smsc = SMSC.GetSMSCById(smscId);
            if (smsc==null)
            {
                throw new ArgumentException(string.Format("Неверный id sms-центра: {0}", smscId), "smscId");
            }
            MessageQueue queue = ServicesConfigurationManager.GetOutgoingMessageQueue(smsc);
            if (queue != null)
            {
                
                System.Messaging.Message message = new System.Messaging.Message(msg);
                message.Formatter = new BinaryMessageFormatter();
                message.Priority = (System.Messaging.MessagePriority.Normal + (int)messagePriority);

                AddToOutbox(msg);
                
                queue.Send(message);
                return true;
            }
            return false;
        }

		/// <summary>
		/// Создает объект логгирования OutboxEntry и помещает его в очередь MSMQ логгера
		/// </summary>
        public static void AddToOutbox(OutputMessage msg)
        {
            try
            {
                string serviceNumber = Cryptany.Core.Message.GetServiceNumber(msg.Source);
                ServiceNumber sn = ServiceNumber.GetServiceNumberBySN(serviceNumber);//при необходимости убрать транзакцию

                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "Kernel.AddToOutbox";
                me.Parameters.Add("@ID", msg.ID);
                me.Parameters.Add("@Body", msg.TypedBody);
                me.Parameters.Add("@MsgTypeID", (int)msg.Type);

               
                me.Parameters.Add("@InboxID", msg.InboxMsgID != Guid.Empty ? msg.InboxMsgID : (object)DBNull.Value);
                me.Parameters.Add("@MessageTime", DateTime.Now);
                me.Parameters.Add("@SNId", sn.ID);

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
                _logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
            }
            catch (MessageQueueException e)
            {
                _logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
            }
        }

        /// <summary>
        /// Создает объект логгирования OutboxEntry и помещает его в очередь MSMQ логгера
        /// </summary>
        public static void AddToOutboxExternalAbonent(OutputMessage msg, Guid abonentId)
        {
            try
            {

                string serviceNumber = Cryptany.Core.Message.GetServiceNumber(msg.Source);
                ServiceNumber sn = ServiceNumber.GetServiceNumberBySN(serviceNumber);
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "Kernel.AddToOutbox";
                me.Parameters.Add("@ID", msg.ID);
                me.Parameters.Add("@Body", msg.TypedBody);
                me.Parameters.Add("@MsgTypeID", 0);
                me.Parameters.Add("@InboxID", msg.InboxMsgID != Guid.Empty ? msg.InboxMsgID : (object)DBNull.Value);
                me.Parameters.Add("@MessageTime", DateTime.Now);
                me.Parameters.Add("@SNId", sn.ID);
                me.Parameters.Add("@AbonentId", abonentId);
                if (msg.InboxMsgID == Guid.Empty && !string.IsNullOrEmpty(msg.HTTP_Category)) //рассылка и указана тарификация
                {
                    me.Parameters.Add("@OperatorParameter", msg.HTTP_Category);
                }
                using (MessageQueue _MSMQLoggerInputQueue = ServiceManager.MSMQLoggerInputQueue)
                {
                    _MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (ApplicationException ex)
            {
                _logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
            }
            catch (MessageQueueException e)
            {
                _logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
            }
        }

    }
}
