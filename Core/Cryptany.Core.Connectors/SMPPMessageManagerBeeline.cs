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
using System.Data.SqlClient;
using System.Text;
using System.Linq;
using Cryptany.Common.Utils;
using Cryptany.Common.Logging;
using Cryptany.Core.SmppLib;
using System.Configuration;
using System;
using System.Collections.Generic;
using Cryptany.Core.Interaction;
using Cryptany.Core.Caching;
using Cryptany.Core.MsmqLog;
using System.Messaging;
using Cryptany.Core.Connectors;
using Cryptany.Core.Management;
using System.Diagnostics;

namespace Cryptany.Core
{
    public class SMPPMessageManagerBeeline: SMPPMessageManagerAsync
    {
        [Serializable]
        public class ExpiredSubscriptionsRemover : Microsoft.Practices.EnterpriseLibrary.Caching.ICacheItemRefreshAction
        {
            SMPPMessageManagerAsync _smppmm;

            public ExpiredSubscriptionsRemover(SMPPMessageManagerAsync SMPPmm)
            {
                _smppmm = SMPPmm;
            }

            public void Refresh(string removedKey,Object expiredValue,
                Microsoft.Practices.EnterpriseLibrary.Caching.CacheItemRemovedReason removalReason)
            {
                if (removalReason ==  Microsoft.Practices.EnterpriseLibrary.Caching.CacheItemRemovedReason.Expired)
                {
                    var pi = _smppmm.PDUList_OUT.Infos.FirstOrDefault(info => info.Packet.SequenceNumber.ToString() == removedKey);
                    if (pi != null)
                        _smppmm.PDUList_OUT.Remove(pi);
                }
            }
        }

        public SMPPMessageManagerBeeline(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
            using (SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString))
            {
                con.Open();
                using (SqlCommand comm = new SqlCommand("[kernel].[GetServiceNumbersForClubs]", con))
                {
                    comm.CommandType = System.Data.CommandType.StoredProcedure;

                    using(SqlDataReader reader = comm.ExecuteReader())
                        while (reader.Read())
                        {
                            Guid clubId = reader.GetGuid(reader.GetOrdinal("Id"));
                            string sn   = reader.GetString(reader.GetOrdinal("SN"));
                            clubsToSN.Add(clubId, sn);
                        }
                }
            }
            Subscriptions.refreshener = new ExpiredSubscriptionsRemover(this);

            if (ConfigurationManager.AppSettings["BeelineSubscriptionMessageTimeout"] != null)
                Subscriptions._expirationTimeSpan =
                    TimeSpan.FromMinutes(int.Parse(ConfigurationManager.AppSettings["BeelineSubscriptionMessageTimeout"]));
            else
                Subscriptions._expirationTimeSpan = TimeSpan.FromMinutes(5);
        }

        /// <summary>
        /// слазить в  черный список и для заблокированных отдать оператору ошибку 
        /// </summary>
        /// <param name="dlsm"></param>
        /// <param name="dlsmr"></param>
        /// <returns></returns>
        protected override bool CheckDeliverSM(DeliverSM dlsm, DeliverSMResponse dlsmr)
        {
            string sn = Message.GetServiceNumber(dlsm.Destination.Address);
            string msisdn = dlsm.Source.Address;
            AbonentState abstate = CheckAbonentInBlackList(msisdn, sn);
            
            switch (abstate)
            {
                case AbonentState.Blocked:
                    dlsmr.StatusCode = PacketBase.commandStatusEnum.ESME_R_BEELINE_Provider_ServiceDenied;

                    break;
                case AbonentState.NotBlocked:
                    dlsmr.StatusCode = PacketBase.commandStatusEnum.ESME_ROK;
                    break;
                case AbonentState.Unknown:
                    dlsmr.StatusCode = PacketBase.commandStatusEnum.ESME_R_BEELINE_Provider_SystemError;
                    return false;
            }
            return true;
        }

        public override void Receive(SMPPConnector conn, int bytesCount, byte[] bytes)
        {
            int idx = 0;
            byte[] packlen = new byte[4];

            while (idx < bytesCount)
            {
                if (idx + 4 > bytesCount) break; //крайнемаловероятно

                Array.Clear(packlen, 0, 4);

                packlen[0] = bytes[idx];
                packlen[1] = bytes[idx + 1];
                packlen[2] = bytes[idx + 2];
                packlen[3] = bytes[idx + 3];

                if (packlen[3] == 255) // отвалился коннект
                    break;

                uint len = SupportOperations.FromBigEndianUInt(packlen);

                if (len <= 3) break;
                if (idx + len > bytesCount) break; //крайнемаловероятно

                byte[] packet = new byte[len];
                if (packet == null || packet.Length < 4)
                    Logger.Write(new LogMessage("out of memory or end of the array", LogSeverity.Error));
                
                Array.Copy(bytes, idx, packet, 0, len);

                PacketBase pb = new PacketBase();
                try
                {
                    pb.Parse(packet);
                }
                catch (Exception ex)
                {
                    ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                    Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                    return;
                }
                idx += (int)len;
                Logger.Write(new LogMessage("Incoming! CmndID: " + pb.CommandId + ", Seq " + pb.SequenceNumber + ", Status" + pb.StatusCode, LogSeverity.Info));

                //отличие от SMPPMM.Receive:
                // 1.нет проверки на номер последовательности

                if (SMPPSettings.IsTransceiver)
                    RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_TRx, pb, DateTime.Now);//очень важная функция, выставляются таймауты
                else
                {
                    if (ReferenceEquals(conn, SMPPConn_Tx)) //режим трансмиттер
                        RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_Tx, pb, DateTime.Now);
                    else //режим ресивер
                    {
                        if (pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp ||
                            pb.CommandId == PacketBase.commandIdEnum.enquire_link ||
                            pb.CommandId == PacketBase.commandIdEnum.unbind_resp ||
                            pb.CommandId == PacketBase.commandIdEnum.unbind ||
                            pb.CommandId == PacketBase.commandIdEnum.generic_nack)
                        
                            RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_Rx, pb, DateTime.Now);
                    }
                }

                switch (pb.CommandId)
                {
                    case PacketBase.commandIdEnum.bind_transceiver_resp:
                        
                        BindResponseTransceiver brt = new BindResponseTransceiver();
                        brt.Parse(packet);
                        if (brt.StatusCode == PacketBase.commandStatusEnum.ESME_ROK || brt.StatusCode == PacketBase.commandStatusEnum.ESME_RALYBND)
                        {
                            Logger.Write( new LogMessage( "Connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, LogSeverity.Info));
                            ServiceManager.LogEvent("Connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, EventType.Info, EventSeverity.Normal);
                            pcConnectionState.RawValue = 1;
                            _connectorState.State = "Connected";
                            // 2.нет проверки SendMessage == null перед установкой события
                            ReadyToSendMessages.Set();
                        }
                        else
                        {
                            pcConnectionState.RawValue = 0;
                            ReadyToSendMessages.Reset();
                            Logger.Write( new LogMessage( "Not connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, LogSeverity.Info));
                            _connectorState.State = "Not connected";
                            ServiceManager.LogEvent("Not connected (transceiver) " + brt.SystemId + "...Status = " +brt.StatusCode, EventType.Error, EventSeverity.High);
                        }
                        break;

                    case PacketBase.commandIdEnum.bind_transmitter_resp:

                        BindResponseTransmitter brt_t = new BindResponseTransmitter();
                        brt_t.Parse(packet);
                        if (brt_t.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        {
                            pcConnectionState.RawValue = 1;
                            ServiceManager.LogEvent("Connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, EventType.Info, EventSeverity.Normal);
                            Logger.Write(new LogMessage( "Connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, LogSeverity.Info));
                            _connectorState.State = "Connected";
                            // 3. нет проверки SendMessage == null
                            ReadyToSendMessages.Set();
                        }
                        else
                        {
                            pcConnectionState.RawValue = 0;
                            ServiceManager.LogEvent("Not connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, EventType.Error, EventSeverity.High);
                            Logger.Write(new LogMessage("Not connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, LogSeverity.Error));
                            _connectorState.State = "Not connected";
                            ReadyToSendMessages.Reset();
                        }
                        break;

                    case PacketBase.commandIdEnum.bind_receiver_resp:
                       
                        BindResponseReceiver brt_r = new BindResponseReceiver();
                        brt_r.Parse(packet);
                        if (brt_r.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        {
                            pcConnectionState.RawValue = 1;
                            ServiceManager.LogEvent("Connected (receiver) to " + brt_r.SystemId + "...Status = " + brt_r.StatusCode, EventType.Info, EventSeverity.Normal);
                            Logger.Write(  new LogMessage( "Connected (receiver) to " + brt_r.SystemId + "...Status = " + brt_r.StatusCode, LogSeverity.Info));
                            _connectorState.State = "Connected";
                        }
                        else
                        {
                            pcConnectionState.RawValue = 0;
                            ServiceManager.LogEvent("Not connected (receiver) to " + brt_r.SystemId + "...Status = " +brt_r.StatusCode, EventType.Error, EventSeverity.High);
                            Logger.Write( new LogMessage( "Not connected (receiver) to " + brt_r.SystemId + "...Status = " + brt_r.StatusCode, LogSeverity.Error));
                            _connectorState.State = "Not connected";
                        }

                        break;
                    case PacketBase.commandIdEnum.data_sm:
                        {
                            DataSM dtsm = new DataSM();
                            dtsm.Parse(packet);
                            DataSMResponse dtsm_r = new DataSMResponse(ref dtsm);
                            EncodeAndSendResp( (SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, dtsm_r);
                            // SAR message processing
                            bool isSarMsg_DT = false;
                            ushort refNum_DT = 0x00;
                            byte[] mergedMessage_DT = null;
                            bool[] flags = Cryptany.Common.Utils.Math.GetBitsArray(dtsm.EsmClass);
                            bool _isreceipt = flags[2];
                            if (_isreceipt)       //логика для отчетов о доставке
                            {
                                ProcessDeliveryReceit(dtsm);
                                // 4. наличие вот этого break
                                break;
                            }
                            // 5. foreach и дальше не заключены в else предыдущего if
                            foreach (OptionalParameter op in dtsm.OptionalParamList)
                                if (op.Param == OptionalParameter.tagEnum.sar_msg_ref_num)
                                {
                                    isSarMsg_DT = true;
                                    refNum_DT = SMPPMessageParts.PutMessagePart(dtsm);
                                    if (refNum_DT > 0)// пришли все SAR блоки сообщения с данным sar_msg_ref_num 
                                        mergedMessage_DT = SMPPMessageParts.MergeFromSAR(refNum_DT);
                                    break;
                                }
                            
                            if (dtsm.Source.Address != "" && (isSarMsg_DT == false || refNum_DT > 0))
                            {
                                int SARCount = 0;
                                byte[] userData = null;
                                if (isSarMsg_DT == false) // не SAR сообщение
                                {
                                    foreach (OptionalParameter op in dtsm.OptionalParamList)
                                    {
                                        if (op.Param == OptionalParameter.tagEnum.message_payload)
                                        {
                                            userData = op.Value;
                                            break;
                                        }
                                    }
                                }
                                else // SAR сообщение
                                {
                                    foreach (OptionalParameter op in dtsm.OptionalParamList)
                                        if (op.Param == OptionalParameter.tagEnum.sar_total_segments)
                                        {
                                            SARCount = op.Value[0];
                                            break;
                                        }
                                    userData = mergedMessage_DT;
                                }
                                // Create MSMQ message and send it to the Router main input MSMQ queue
                                if (userData == null) userData = Encoding.Default.GetBytes("");

                                string MSISDN = "";
                                string msgText = "";
                                string serviceNumber = "";
                                string transactionID = "";

                                if (dtsm.Destination.Address.Contains("#"))
                                    transactionID = Message.GetTransactionId(dtsm.Destination.Address);
                                else
                                    transactionID = Message.GetTransactionId(dtsm.Source.Address);
                                MSISDN = Message.GetMSISDN(dtsm.Source.Address);
                                serviceNumber = Message.GetServiceNumber(dtsm.Destination.Address);
                                msgText = Enum.GetName(typeof(PacketBase.dataCodingEnum), dtsm.DataCoding) == SMPPSettings.DataCoding_unicode ? Encoding.BigEndianUnicode.GetString(userData) : Encoding.Default.GetString(userData);
                                if (string.IsNullOrEmpty(serviceNumber) || string.IsNullOrEmpty(MSISDN))
                                {
                                    Logger.Write(new LogMessage("В сообщении не указан сервисный номер или msisdn", LogSeverity.Error));
                                    ServiceManager.LogEvent("В сообщении не указан сервисный номер или msisdn",EventType.Error,EventSeverity.High);
                                    break;
                                }
                                Message newMessage = new Message(IdGenerator.NewId, MSISDN, ConnectorId, serviceNumber, transactionID, msgText);
                                Send_MSMQ_MessageToRouterInputQueue(newMessage);
                            }
                        }
                        break;

                    // 6. отличие обработки data_sm_resp
                    case PacketBase.commandIdEnum.data_sm_resp:
            
                        DataSMResponse dtsmr = new DataSMResponse();
                        dtsmr.Parse(packet);
                        PacketInfo info_found = GetUserData(dtsmr.SequenceNumber);
                        if (info_found != null)
                        {
                            SetPDUOutResponseCounter(info_found.SentTime);
                            string message_id = dtsmr.MessageId;
                            UpdateOutboxState(info_found.Message, (int)GetMessageStateString(dtsmr.StatusCode),
                                                  dtsmr.RealStatusCode.ToString(), message_id);
                            if ((info_found.Packet as DataSM).RegisteredDelivery == 1 && dtsmr.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                                msgsWaitingReceits.Add<OutputMessage>(message_id, info_found.Message);
                        }
                        _connectorState.State = "Connected";
                        break;

                    // предположительно единственный нужный case для beeline
                    case PacketBase.commandIdEnum.deliver_sm:
                        {
                            DeliverSM dlsm = new DeliverSM();
                            dlsm.Parse(packet);
                            DeliverSMResponse dlsm_r = new DeliverSMResponse(ref dlsm);
                            bool isSarMsg_DL = false;
                            ushort refNum_DL = 0;
                            byte[] mergedMessage_DL = null;
                            
                            //логика для отчетов о доставке
                            bool[] esmFlags = Cryptany.Common.Utils.Math.GetBitsArray(dlsm.EsmClass);
                            bool _isreceipt = esmFlags[2];
                            bool _isUDH = esmFlags[6];

                            if (_isreceipt) // отчет о доставке на терминал абонента
                            {
                                EncodeAndSendResp( (SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, dlsm_r);
                                ProcessDeliveryReceit(dlsm);
                            }
                            else // входящее сообщение
                            {
                                pcIncomingMessagesPerSecond.Increment();
                                _connectorState._lastSMSInTime = DateTime.Now;
                                //Для входящих сообщений делаем проверку
                                bool continueProcessing = true;
                                if (ConfigurationManager.AppSettings["CheckInMessage"] != null && bool.Parse(ConfigurationManager.AppSettings["CheckInMessage"]))
                                    continueProcessing = CheckDeliverSM(dlsm, dlsm_r); //установка статуса команды OK у dlsm_r
                                
                                //сразу отсылаем ответ с кодом команды deliver_sm_resp
                                EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, dlsm_r);

                                if (!continueProcessing) break;
                                byte[] userData = null;

                                if (_isUDH)
                                {
                                    if (!UDHParser.Parse(dlsm))
                                        break;
                                }

                                //проверка optional_parameter'ов (мб вот тут стоит обрабывать сообщение)
                                foreach (OptionalParameter op in dlsm.OptionalParamList)
                                {
                                    if (op.Param == OptionalParameter.tagEnum.sar_msg_ref_num)
                                    {
                                        isSarMsg_DL = true;
                                        refNum_DL = SMPPMessageParts.PutMessagePart(dlsm);
                                        if (refNum_DL > 0) // пришли все SAR блоки сообщения с данным sar_msg_ref_num 
                                            mergedMessage_DL = SMPPMessageParts.MergeFromSAR(refNum_DL);
                                        break;
                                    }
                                    
                                }
                               
                                if (dlsm.Source.Address != "" && (isSarMsg_DL == false || refNum_DL > 0))
                                {
                                    if (isSarMsg_DL == false) // не SAR сообщение
                                    {
                                        if (dlsm.ShortMessageLength > 0) // короткое сообщение
                                            userData = dlsm.MessageText;
                                        else  // длинное сообщение или кривое
                                        {
                                            foreach (OptionalParameter op in dlsm.OptionalParamList)
                                            {
                                                if (op.Param == OptionalParameter.tagEnum.message_payload)
                                                {
                                                    userData = op.Value;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else // SAR сообщение
                                        userData = mergedMessage_DL;
                                    
                                    pcInCounter.Increment();
                                    if (userData == null) userData = Encoding.Default.GetBytes("");

                                    string MSISDN = "";
                                    string msgText = "";
                                    string serviceNumber = "";
                                    string transactionID = "";

                                    if (dlsm.Destination.Address.Contains("#"))
                                        transactionID = Message.GetTransactionId(dlsm.Destination.Address);
                                    else
                                        transactionID = Message.GetTransactionId(dlsm.Source.Address);
                                    MSISDN = Message.GetMSISDN(dlsm.Source.Address);
                                    serviceNumber = Message.GetServiceNumber(dlsm.Destination.Address);

                                    msgText = Enum.GetName(typeof(PacketBase.dataCodingEnum), dlsm.DataCoding) == SMPPSettings.DataCoding_unicode ?
                                        Encoding.BigEndianUnicode.GetString(userData) : Encoding.Default.GetString(userData);

                                    if (string.IsNullOrEmpty(serviceNumber) || string.IsNullOrEmpty(MSISDN))
                                    {
                                        Logger.Write(new LogMessage("В сообщении не указан сервисный номер или msisdn", LogSeverity.Error));
                                        ServiceManager.LogEvent("В сообщении не указан сервисный номер или msisdn",EventType.Error,EventSeverity.High);
                                        break;
                                    }

                                    msgText = InjectMessageBody(msgText, dlsm_r);
                                    Message newMessage = new Message(IdGenerator.NewId, MSISDN, ConnectorId, serviceNumber, transactionID, msgText);
                                    
                                    // кидаем сообщение роутеру на обработку
                                    Send_MSMQ_MessageToRouterInputQueue(newMessage);
                                    string log = string.Empty;
                                    log = "To Router! MSISDN: " + newMessage.MSISDN + ", SMSC: " +
                                          newMessage.SMSCId + ", Msg: " + newMessage.Text;

                                    foreach (OptionalParameter param in dlsm.OptionalParamList)
                                    {
                                        if (param.Param == OptionalParameter.tagEnum.source_port)
                                        {
                                            string paramValue = string.Empty;
                                            log += ", source_port: ";
                                            for (int i = 0; i < param.Value.Length; i++)
                                            {
                                                log += param.Value[i].ToString();
                                                paramValue += param.Value[i].ToString();
                                            }
                                            if (paramValue == "03")
                                            {
                                                //передать подписочному сервису
                                            }
                                            if (paramValue == "04")
                                            {
                                                //отписка
                                            }
                                        }
                                    }
                                    
                                    Logger.Write((new LogMessage(log, LogSeverity.Error)));
                                    
                                }
                            }
                            break;
                        }
                    case PacketBase.commandIdEnum.enquire_link:

                        EnquireLink enqlnk = new EnquireLink();
                        enqlnk.Parse(packet);
                        EnquireLinkResponse enqlnk_r = new EnquireLinkResponse(ref enqlnk);
                        EncodeAndSendResp( (SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, enqlnk_r);

                        break;
                    case PacketBase.commandIdEnum.enquire_link_resp:
            
                        EnquireLinkResponse enqlnkr = new EnquireLinkResponse();
                        enqlnkr.Parse(packet);
                        _connectorState.State = "Connected";

                        // 7. нет установки события на готовность отсылки сообщения
                        break;
                    case PacketBase.commandIdEnum.submit_sm_resp:

                        SubmitSMResponse ssmr = new SubmitSMResponse();
                        ssmr.Parse(packet);
                        _connectorState.State = "Connected";
                        PacketInfo pb_ssm_fnd_info = GetUserData(ssmr.SequenceNumber);

                        if (pb_ssm_fnd_info != null)
                        {
                            PacketBase pb_ssm_fnd = pb_ssm_fnd_info.Packet;
                            SetPDUOutResponseCounter(pb_ssm_fnd_info.SentTime);
                            SubmitSM ssm_fnd = (SubmitSM) pb_ssm_fnd_info.Packet;

                            MessageDeliveryStatus mds = GetMessageStateString(ssmr.StatusCode);

                            if (ProcessAsSpecialResp(pb_ssm_fnd_info, ssmr, mds))
                                // Если это ответ на подписку IVR Beeline, то прекращаем обработку
                                break;

                            string message_id = ssmr.MessageId;
                            /// FIX: MessageId может быть пустым даже при коде ответа ESME_ROK, необходимо сгенерировать суррогатный messageid
                            //if (string.IsNullOrEmpty(message_id))
                            //    message_id = ssm_fnd.PartNumber + ssm_fnd.MessageID.ToString();


                            if (mds != MessageDeliveryStatus.Unknown)
                            {
                                if (ssm_fnd.PartNumber == pb_ssm_fnd_info.Message.PartsCount ||
                                    mds != MessageDeliveryStatus.Delivered)
                                    //если пришел ответ на последнюю или на непоследнюю часть пришел финальный ответ
                                {
                                    if (SMPPSettings.UseMessageState && pb_ssm_fnd_info.Message.InboxMsgID == Guid.Empty)
                                    {
                                        Cryptany.Core.Management.WMI.MessageState evt =
                                            new Cryptany.Core.Management.WMI.MessageState();

                                        evt.ID = pb_ssm_fnd_info.Message.ID.ToString();
                                        evt.Status = mds.ToString();
                                        evt.StatusDescription = ((int) ssmr.RealStatusCode).ToString();
                                        evt.StatusTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

                                        System.Messaging.Message message = new System.Messaging.Message(evt, new BinaryMessageFormatter());
                                        message.AttachSenderId = false;
                                        using (MessageQueue msgQueue = ServiceManager.MessageStateQueue)
                                        {
                                            msgQueue.Send(message);
                                        }
                                    }
                                    UpdateOutboxState(pb_ssm_fnd_info.Message, (int) mds, ssmr.RealStatusCode.ToString(),
                                                      message_id);

                                    if (mds == MessageDeliveryStatus.Delivered && ssm_fnd.RegisteredDelivery == 1)
                                    {
                                        msgsWaitingReceits.Add<OutputMessage>(message_id, pb_ssm_fnd_info.Message);
                                        Trace.WriteLine("Добавили сообщение в кэш");
                                    }
                                    //удаляем все исходящие PDU, относящиеся к этому сообщению
                                    PDUList_OUT.RemoveOutPackets(pb_ssm_fnd_info.Message.ID);
                                }
                            }
                            else
                                AddOutboxSendHistory(pb_ssm_fnd_info.Message.ID, (int) mds, ssmr.RealStatusCode.ToString());
                        }

                        break;
                    case PacketBase.commandIdEnum.query_sm_resp:

                        QueryShortMessageResponse qsmr = new QueryShortMessageResponse();
                        if (qsmr.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        {
                            int i = (int)qsmr.Parse(packet);

                            Logger.Write(new LogMessage("Parsed query_sm_resp mes_id=" + qsmr.MessageId + " final_date=" + qsmr.Final_Date.Date.ToString() + " message_state=" + qsmr.Message_State + " errorcode=" + qsmr.Error_Code, LogSeverity.Info));
                        }

                        break;
                    case PacketBase.commandIdEnum.unbind_resp:

                        _connectorState.State = "Not connected";
                        UnbindResponse ubr = new UnbindResponse();
                        ubr.Parse(packet);
                   
                        break;
                    case PacketBase.commandIdEnum.unbind:

                        ReadyToSendMessages.Reset();
                        ServiceManager.LogEvent("Получили команду unbind", EventType.Warning, EventSeverity.Normal);

                        _connectorState.State = "Not connected";
                        Unbind ub = new Unbind();
                        ub.Parse(packet);
 
                        UnbindResponse ub_r = new UnbindResponse(ref ub);

                        if (SMPPSettings.IsTransceiver)
                        {
                            EncodeAndSendResp(TransferModes.TRx, ub_r);
                            PDUTimeouts_TRx.IsBound = false;
                        }
                        else if (ReferenceEquals(conn, SMPPConn_Tx))
                        {
                            EncodeAndSendResp(TransferModes.Tx, ub_r);
                            PDUTimeouts_Tx.IsBound = false;
                        }
                        else
                        {
                            EncodeAndSendResp(TransferModes.Rx, ub_r);
                            PDUTimeouts_Rx.IsBound = false;
                        }


                        break;
                    case PacketBase.commandIdEnum.outbind:
                      
                        ReadyToSendMessages.Reset();
                      
                        ServiceManager.LogEvent("Получили команду outbind", EventType.Warning, EventSeverity.Normal);
                        Logger.Write(  new LogMessage("Получили команду outbind", LogSeverity.Debug));
                        break;
                    case PacketBase.commandIdEnum.generic_nack:
                        ReadyToSendMessages.Reset();
                        GenericNak gn = new GenericNak();
                        gn.Parse(packet);
                        ServiceManager.LogEvent("Получили команду generic_nack " + gn.StatusCode + " (" + gn.RealStatusCode + ")", EventType.Warning, EventSeverity.High);
              
                        break;
                }

            }
        }
        
        /// <summary>
        /// вставить префикс, чтобы сработал блокировочный сервис
        /// </summary>
        /// <param name="msgText"></param>
        /// <param name="dlsm_r"></param>
        /// <returns></returns>
        protected override string InjectMessageBody(string msgText, DeliverSMResponse dlsm_r)
        {
            if (dlsm_r.StatusCode == PacketBase.commandStatusEnum.ESME_R_BEELINE_Provider_ServiceDenied)
            {
                return "(blocked) " + msgText;
            }
            return base.InjectMessageBody(msgText, dlsm_r);
        }

        protected string mask4041 = ConfigurationManager.AppSettings["mask4041"];

        /// <summary>
        /// прописать тарифную категорию и скрыть номер в случае рассылки с 3044 и 4041
        /// </summary>
        /// <param name="pb"></param>
        protected override void SetOperatorParameters(PacketBase pb)
        {
            SubmitSM sm = pb as SubmitSM;
            if (sm != null)
            {
                
                if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                {
                    OptionalParameter op = new OptionalParameter();
                    op.Param = OptionalParameter.tagEnum.source_subaddress; // признак тарифа для ответной смс
                    op.Value = Encoding.ASCII.GetBytes(SendMessage.HTTP_Category);
                    sm.OptionalParamList.Add(op);
                }

                if (string.IsNullOrEmpty(SendMessage.TransactionId) && (SendMessage.Source.StartsWith("3044") || SendMessage.Source.StartsWith("1020_")))//рассылка Билайн
                {
                    sm.Source.Address = "usluga1020";
                }
                if (!string.IsNullOrEmpty(mask4041) && SendMessage.Source.StartsWith("4041")) // Номер 4041 всегда заменяется на 30440
                    sm.Source.Address = mask4041;
                //sm.Source.Address = "1020";
            }
        }

        /// <summary>
        /// Спец обработка Submit_SM_Resp
        /// </summary>
        /// <param name="pb_ssm_fnd_info">инфо об отправленном пакете, на который пришел ответ</param>
        /// <param name="ssmr">пришедший пакет</param>
        /// <param name="mds">статус доставки для базы</param>
        /// <returns>true если сообщение является ответом на подписку </returns>
        protected override bool ProcessAsSpecialResp(PacketInfo pb_ssm_fnd_info, SubmitSMResponse ssmr, MessageDeliveryStatus mds)
        {
            if (pb_ssm_fnd_info.Message.Content != null) // Для PDU не должно быть соответствующего отправленного SMS
                return false;

            SubmitSM ssm = (SubmitSM)pb_ssm_fnd_info.Packet;

            if (ssm.OptionalParamList.Count(op => op.Param == OptionalParameter.tagEnum.source_port) == 0) // Вдруг все таки не подписка
                return false;


            SubscriptionMessage sub;
            if (!Subscriptions.GetItem(ssmr.SequenceNumber.ToString(), out sub))
            {// Пытамся достать соответствующее сообщение о подписке из кэша
                Logger.Write(new LogMessage("ProcessAsSpecialResp: Для SubmitSMResponse не найдено соответствующее сообщение о подписке", LogSeverity.Error));
                return true;
            }

            UpdateSubscriptionInfoForOperators(sub, mds, ssmr.StatusCode); // Сливаем в базу инфу по пришедшему пакету

            PDUList_OUT.Infos.Remove(pb_ssm_fnd_info); // Удаляем пакет на который пришел ответ 
            Subscriptions.Remove(ssmr.SequenceNumber.ToString()); // И соответствующее сообщение о подписке
            return true;
        }

        Cache Subscriptions = new Cache();
        Dictionary<Guid, string> clubsToSN = new Dictionary<Guid,string>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subgmsg">Сообщение о подписке. В поле SmsId</param>
        /// <returns>Отправили ли сообщение</returns>
        public override bool SendSubscription(SubscriptionMessage subgmsg)
        {
            bool sendOk;

            lock (this)
            {
                SendMessage = new OutputMessage();
                SendMessage.Content = null; // Обнуляем контент, это будет признаком того, что ответное сообщение является ответом на подписочное
                SendMessage.ID = Guid.NewGuid();
                SendMessage.PartsCount = 1;

                // Проставляем сервисный номер по идентификатору клуба 
                if (!clubsToSN.ContainsKey(subgmsg.clubs.First())) 
                {
                    Logger.Write(new LogMessage("SendSubscription: Для клуба " + subgmsg.clubs.First() + " не найден соответствующий сервисный номер", LogSeverity.Error));
                    return false;
                }

                string sn = clubsToSN[subgmsg.clubs.First()];
                //------------------------------CreateSubmitSM-------------------------------------------
                SubmitSM sm = new SubmitSM();
                sm.Source.TON = SMPPSettings.Source_TON;
                sm.Source.NPI = SMPPSettings.Source_NPI;
                sm.Source.Address = sn ?? "";
                sm.Destination.TON = SMPPSettings.Destination_TON;
                sm.Destination.NPI = SMPPSettings.Destination_NPI;
                sm.Destination.Address = subgmsg.MSISDN ?? "";
                sm.DataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_default);
                sm.ServiceType = SMPPSettings.ServiceType;
                sm.DefaultSMMessageId = SMPPSettings.DefaultSMMessageId;
                sm.MessageID = System.Guid.NewGuid();
                sm.RegisteredDelivery = 0;
                sm.ProtocolId = 0;
                sm.MessageText = System.Text.Encoding.ASCII.GetBytes("Manual subscription message N2 2");
                sm.ShortMessageLength = (byte)System.Text.Encoding.ASCII.GetByteCount("Manual subscription message N2 2");

                //------------------------------Optional Parameter-------------------------------------------
                var op = CreateSubscriptionOptionalParameter(subgmsg);
                if (op == null)
                    return false;
                sm.OptionalParamList.Add(op);

                sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);

                if (sendOk) //Если отправилось, добавляем в кэш подписку
                    Subscriptions.Add<SubscriptionMessage>(sm.SequenceNumber.ToString(), subgmsg);
            }

            return sendOk;
        }

        private OptionalParameter CreateSubscriptionOptionalParameter(SubscriptionMessage subgmsg)
        {
            OptionalParameter op = new OptionalParameter();
            op.Param = OptionalParameter.tagEnum.source_port; // Тот самый параметр

            // Если МегаБокс или ФанБокс, т.е. IVR, то действуем по старой схеме
            if (subgmsg.clubs.First() == new Guid("440BAED5-75B6-439D-8FCD-822FA24B60F1") ||
                subgmsg.clubs.First() == new Guid("6CDE45D4-E5A0-4ACB-93B1-8177F8ED4B45"))
            {
                switch (subgmsg.actionType)
                {
                    case SubscriptionActionType.Subscribe:
                        op.Value = SupportOperations.ToBigEndian((ushort) 1);
                        break;
                    case SubscriptionActionType.Unsubscribe:
                        op.Value = SupportOperations.ToBigEndian((ushort) 2);
                        break;
                    default:
                        Logger.Write(new LogMessage("Не предусмотрена команда UnsubscribeAll в SendSubscription",
                                                    LogSeverity.Error));
                        return null;
                }
            }
            else // В противном случае это ТВ клуб и разрешена только отписка
            {
                switch (subgmsg.actionType)
                {
                    case SubscriptionActionType.Subscribe:
                        Logger.Write(new LogMessage(
                                         "Не предусмотрена команда Subscribe для ТВ клуба в SendSubscription",
                                         LogSeverity.Error));
                        return null;
                    case SubscriptionActionType.Unsubscribe:
                        op.Value = SupportOperations.ToBigEndian((ushort) 4);
                        break;
                    default:
                        Logger.Write(new LogMessage("Не предусмотрена команда UnsubscribeAll в SendSubscription",
                                                    LogSeverity.Error));
                        return null;
                }
            }
            return op;
        }

        public void UpdateSubscriptionInfoForOperators(SubscriptionMessage sub, MessageDeliveryStatus mds, PacketBase.commandStatusEnum status)
        {

            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "[kernel].[UpdateSubscriptionInfoForOperators]";
                me.Parameters.Add("@SubscriptionId ", sub.smsId);
                me.Parameters.Add("@IsSubscribe", sub.actionType == SubscriptionActionType.Subscribe);
                me.Parameters.Add("@Delivered", (byte)mds);
                me.Parameters.Add("@StateDescription", status.ToString());
                me.Parameters.Add("@StateTime", DateTime.Now);
                me.Parameters.Add("@MSISDN", sub.MSISDN);
                me.Parameters.Add("@ClubId", sub.clubs.First());
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
            //  catch (InvalidCastException ice)
        }
    }
}