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
using Cryptany.Core.SmppLib;
using Cryptany.Core.Caching;
using Cryptany.Common.Utils;
using System.Messaging;
using System.Diagnostics;
using Cryptany.Common.Logging;
using Cryptany.Core.Management;
using Cryptany.Core.Interaction;
using System.Data.SqlClient;
using System.Configuration;
using Cryptany.Core.MsmqLog;

namespace Cryptany.Core
{
    class SMPPMessageManagerRTC : SMPPMessageManager
    {
        public SMPPMessageManagerRTC(ConnectorSettings cs, Logging.ILogger logger) 
            : base(cs, logger)
        { }

        SubscriptionMessage sendSubMsg;

        protected override bool SendOutMessage()
        {
            //SendMessage.Source = "1020"; // чит

            Logger.Write(new LogMessage("SendOutMessage Начинаем отправку сообщения " + SendMessage, LogSeverity.Info));
            if (string.IsNullOrEmpty(SendMessage.OperatorSubscriptionId))
            {
                Logger.Write(new LogMessage("SendOutMessage надо послать RegisterService " + SendMessage, LogSeverity.Info));
                if (!CreateAndSendRegisterServiceMessage())
                {
                    Logger.Write(new LogMessage("SendOutMessage не смогли получить serviceId" + SendMessage, LogSeverity.Info));
                    ReadyToSendMessages.Set();
                    return false;
                }
                Logger.Write(new LogMessage("SendOutMessage получили serviceId" + SendMessage, LogSeverity.Info));
            }
            
            return base.SendOutMessage();
        }

        public override bool SendSubscription(SubscriptionMessage subgmsg)
        {
            RegisterService rs = new RegisterService();

            sendSubMsg = subgmsg;

            rs.DestinationAddr = subgmsg.MSISDN;

            switch (subgmsg.actionType)
            {
                case SubscriptionActionType.Subscribe:
                    rs.ServiceState = RegisterService.serviceStateEnum.ActivateSubscription;
                    break;
                case SubscriptionActionType.Unsubscribe:
                    rs.ServiceState = RegisterService.serviceStateEnum.DeactivateService;
                    break;
                default:
                    Logger.Write(new LogMessage("Не предусмотрена команда UnsubscribeAll в SendSubscription", LogSeverity.Error));
                    return false;
            }

            Logger.Write(new LogMessage("SendSubscription Запрашиваем параметры RegisterService", LogSeverity.Info));
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString))
            {
                con.Open();
                using (SqlCommand comm = new SqlCommand("[kernel].[GetSubscriptionInfoForRegisterService]", con))
                {
                    comm.CommandType = System.Data.CommandType.StoredProcedure;

                    comm.Parameters.AddWithValue("@SubscriptionId", subgmsg.clubs.First()); // !!!!!!!!!! Это идентификатор активированной подписки, а не клуба !!

                    SqlDataReader reader = comm.ExecuteReader();
                    if (!reader.Read())
                    {
                        Logger.Write(new LogMessage("[kernel].[GetSubscriptionInfoForRegisterService] не вернула записей для @subscriptionId=" + subgmsg.clubs.First(), LogSeverity.Error));
                        return false;
                    }

                    UpdateFromReader(rs, reader);
                }
            }

            Logger.Write(new LogMessage("SendSubscription Получили параметры RegisterService", LogSeverity.Info));
            if (!CanSendNextMessage())
                return false;
            ReadyToSendMessages.Reset();

            SaveRegisterServiceToDB(rs);
            Logger.Write(new LogMessage("SendSubscription Отправляем сообщение RegisterService", LogSeverity.Info));
            if (!EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, rs))
            {
                Logger.Write(new LogMessage("SendSubscription Не смогли отправить RegisterService для абонента " + subgmsg.MSISDN, LogSeverity.Error));
                return false;
            }

            return true;
        }

        protected bool CreateAndSendRegisterServiceMessage()
        {
            RegisterService rs = new RegisterService();
            rs.DestinationAddr = SendMessage.Destination;
            rs.ServiceState = RegisterService.serviceStateEnum.ActivateSingleService;

            Logger.Write(new LogMessage("CreateAndSendRegisterServiceMessage Запрашиваем из базы параметры отправляемого PDU " + SendMessage, LogSeverity.Info));
            
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString))
            {
                con.Open();
                using (SqlCommand comm = new SqlCommand("[kernel].[GetSmsInfoForRegisterService]", con))
                {
                    comm.CommandType = System.Data.CommandType.StoredProcedure;

                    comm.Parameters.AddWithValue("@inboxId", SendMessage.InboxMsgID);
                    comm.Parameters.AddWithValue("@ServiceNumber", SendMessage.Source);
                    comm.Parameters.AddWithValue("@ResourceId", SendMessage.ProjectID);

                    SqlDataReader reader = comm.ExecuteReader();
                    if (!reader.Read())
                    {
                        Logger.Write(new LogMessage("[kernel].[GetSmsInfoForRegisterService] не вернула записей для сообщения @outboxId=" + SendMessage.ID, LogSeverity.Error));
                        return false;
                    }

                    UpdateFromReader(rs, reader);
                }
            }

            Logger.Write(new LogMessage("CreateAndSendRegisterServiceMessage параметры получили, отпраквляем PDU " + SendMessage, LogSeverity.Info));

            SaveRegisterServiceToDB(rs);

            if (!EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, rs)) 
            {
                Logger.Write(new LogMessage("Не получили RegisterServiceResp втечение требуемого времини", LogSeverity.Error));
                ServiceManager.LogEvent("Не получили RegisterServiceResp втечение требуемого времини", EventType.Warning, EventSeverity.Normal);
                return false;
            }
            // Теперь в отправляемом сообщении должен проставиться ServiceId
            Logger.Write(new LogMessage("CreateAndSendRegisterServiceMessage Получили " + SendMessage, LogSeverity.Info));
            
            return true;
        }

        private void UpdateFromReader(RegisterService rs, SqlDataReader reader)
        {
            rs.ServiceState = (RegisterService.serviceStateEnum)reader.GetByte(reader.GetOrdinal("Service_State"));
            rs.ServiceTypeId = SupportOperations.GuidToULong(reader.GetGuid(reader.GetOrdinal("clubId")));
            rs.ServiceClass = reader.GetString(reader.GetOrdinal("service_class"));
            rs.ServiceDescr = reader.GetString(reader.GetOrdinal("service_descr"));
            rs.ServiceCost = (uint)reader.GetInt32(reader.GetOrdinal("Cost"));
            rs.ServicePeriod = (RegisterService.servicePeriodEnum)reader.GetByte(reader.GetOrdinal("service_period"));
            rs.ActivationAddr = reader.GetString(reader.GetOrdinal("activation_address"));
            rs.ActivationMessage = reader.GetString(reader.GetOrdinal("activation_message"));
            rs.ServiceId = 0;
            long tmp;
            if (!reader.IsDBNull(reader.GetOrdinal("service_id")))
                if (long.TryParse(reader.GetString(reader.GetOrdinal("service_id")), out tmp))
                    rs.ServiceId = (ulong)tmp;
        }

        public override void ProcessRegisterService(RegisterService rs)
        {
            
        }

        public override void ProcessRegisterServiceResp(RegisterServiceResp rsr)
        {
            Logger.Write(new LogMessage("Прилетел RegisterServiceResp. Добавляем в базу", LogSeverity.Info));
            UpdateRegisterService(rsr);

            if (!(LastOutPdu is RegisterService && _currentSequenceNumber == rsr.SequenceNumber))
            {
                Logger.Write(new LogMessage("У полученного RegisterServiceResp SequenceNumber не совпадает с отправленным или последний отправленный пакет не RegisterService", LogSeverity.Error));
                return;
            }

            RegisterService lastRS = LastOutPdu as RegisterService;

            if (rsr.StatusCode != PacketBase.commandStatusEnum.ESME_ROK)
            {
                Logger.Write(new LogMessage("Оператор вернул ошибку " + rsr.StatusCode + " в ответ на RegisterService. Код ошибки " + rsr.RealStatusCode, LogSeverity.Error));
                if (lastRS.ServiceState == RegisterService.serviceStateEnum.ActivateSingleService)
                    SendMessage.OperatorSubscriptionId = "";
                return;
            }

            switch (lastRS.ServiceState)
            {
                case RegisterService.serviceStateEnum.ActivateSubscription:
                    UpdateSubscriptionInfo(rsr.ServiceId);
                    ReadyToSendMessages.Set();
                    break;
                case RegisterService.serviceStateEnum.ActivateSingleService:
                    SendMessage.OperatorSubscriptionId = rsr.ServiceId.ToString();
                    UpdateSmsInfo(rsr.ServiceId);
                    break;
            }

            Logger.Write(new LogMessage("RegisterServiceResp успешно обработан", LogSeverity.Info));
                
        }

        protected override SubmitSM Create_SUBMIT_SM(OutputMessage outputMessage, SmppLib.PacketBase.dataCodingEnum dataCoding)
        {
            SubmitSM res = base.Create_SUBMIT_SM(outputMessage, dataCoding);

            Logger.Write(new LogMessage("Create_SUBMIT_SM всталяем serviceId в отправляемое сообщение", LogSeverity.Info));

            SetServiceIdParameter(outputMessage, res);

            return res;
        }

        void SetServiceIdParameter(OutputMessage outputMessage, SubmitSM ssm)
        {
            ulong serviceId;

            if (ulong.TryParse(outputMessage.OperatorSubscriptionId, out serviceId))
            {
                OptionalParameter serviceIdParam = new OptionalParameter();
                serviceIdParam.Param = OptionalParameter.tagEnum.service_id;
                serviceIdParam.Value = SupportOperations.ToBigEndian(serviceId);

                ssm.OptionalParamList.Add(serviceIdParam);

                Logger.Write(new LogMessage("SetServiceIdParameter ServiceId добавлен в PDU Submit_SM", LogSeverity.Info));
            }
            else
                Logger.Write(new LogMessage("SetServiceIdParameter вызван для ответа на сообщение с некорректным ServiceId=\"" + (outputMessage.OperatorSubscriptionId ?? "NULL") + "\". Попробуем отправить с пустым", LogSeverity.Error));
        }

        protected override bool SendSARMessage(PacketBase.dataCodingEnum dataCoding, ushort sarCount, bool SendSarPartInPayload)
        {
            bool sendOk = false;
            SendMessage.PartsCount = sarCount;
            Logger.Write(new LogMessage("SendSARMessage отправляем длинное сообщение", LogSeverity.Info));

            for (ushort i = 1; i <= sarCount; i++)
            {
                SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);

                sm.OptionalParamList.Clear();
                sm.PartNumber = i;
                sm.OptionalParamList = SMPPMessageParts.GetOptionalParameterList(ConnectorId, i);

                //if (i == 1)
                {
                    //Logger.Write(new LogMessage("SendSARMessage вставляем ServiceId для первой части", LogSeverity.Info));
                    SetServiceIdParameter(SendMessage, sm);
                }

                if (SendSarPartInPayload)
                {
                    sm.ShortMessageLength = 0;
                    sm.MessageText = new byte[0];

                }
                else
                {

                    OptionalParameter payload = sm.OptionalParamList.Single(item => item.Param == OptionalParameter.tagEnum.message_payload);
                    sm.OptionalParamList.Remove(payload);
                    sm.ShortMessageLength = (byte)payload.Value.Length;
                    sm.MessageText = payload.Value;
                }


                if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                {
                    SetOperatorParameters(sm);
                }

                Logger.Write(new LogMessage("SendSARMessage вызов EncodeAndSend", LogSeverity.Info));
                sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);

                if (!sendOk) break;
                if (!MultipartSendNextPart)
                {

                    break;
                }

                SleepBeforeNextMessage();
            }
            Logger.Write(new LogMessage("SendSARMessage отправить длинное сообщение", LogSeverity.Info));

            return sendOk;
        }

        public void UpdateSubscriptionInfo(ulong serviceId)
        {
            try
            {

                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "[kernel].[UpdateRegisterServiceForSubscription]";
                me.Parameters.Add("@SubscriptionId", sendSubMsg.clubs.First());
                me.Parameters.Add("@service_id", serviceId.ToString());
                using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (MessageQueueException e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }

        public void UpdateSmsInfo(ulong serviceId)
        {
            try
            {

                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "[kernel].[UpdateRegisterServiceForSMS]";
                me.Parameters.Add("@SMSId", SendMessage.ID);
                me.Parameters.Add("@service_id", serviceId.ToString());
                using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (MessageQueueException e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }

        private void SaveRegisterServiceToDB(RegisterService rs)
        {
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "[kernel].[AddRegisterServicePDU]";
                me.Parameters.Add("@SequenceNumber", (int)rs.SequenceNumber);
                me.Parameters.Add("@RequestId", (long)rs.RequestId);
                me.Parameters.Add("@DestinationAddr", rs.DestinationAddr);
                me.Parameters.Add("@ServiceState", (byte)rs.ServiceState);
                me.Parameters.Add("@ServiceTypeId", (long)rs.ServiceTypeId);
                me.Parameters.Add("@ServiceClass", rs.ServiceClass);
                me.Parameters.Add("@ServiceDescr", rs.ServiceDescr);
                me.Parameters.Add("@ServiceCost", (int)rs.ServiceCost);
                me.Parameters.Add("@ServicePeriod", (byte)rs.ServicePeriod);
                me.Parameters.Add("@ActivationType", (byte)rs.ActivationType);
                me.Parameters.Add("@ActivationAddress", rs.ActivationAddr);
                me.Parameters.Add("@ActivationMessage", rs.ActivationMessage);
                me.Parameters.Add("@ServiceId", (long)rs.ServiceId);
                using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (MessageQueueException e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }

        private void UpdateRegisterService(RegisterServiceResp rsr)
        {
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "[kernel].[UpdateRegisterServicePDU]";
                me.Parameters.Add("@SequenceNumber", (int)rsr.SequenceNumber);
                me.Parameters.Add("@RespCommandStatus", (int)((uint)rsr.StatusCode));
                me.Parameters.Add("@RespServiceId", (long)rsr.ServiceId);
                using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (MessageQueueException e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                }
            }
        }
    }
}
