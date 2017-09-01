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
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Messaging;
using Cryptany.Core.Connectors;
using Cryptany.Core.Interaction;
using Cryptany.Core.Management;
using Cryptany.Common.Utils;
using Cryptany.Core.SmppLib;
using Cryptany.Common.Logging;
using Cryptany.Core;

namespace Cryptany
{
    namespace Core
    {
        /// <summary>
        /// отправляет и принимает USSD-сообщения
        /// </summary>
        public class SMPPMessageManagerUSSD : SMPPMessageManager
        {
            public SMPPMessageManagerUSSD(ConnectorSettings cs, ILogger logger)
                : base(cs, logger)
            { }
            
         
            public override bool SendUserData(OutputMessage outputMessage)
            {
                if (!CanSendNextMessage()) return false;
                ReadyToSendMessages.Reset();
                SendMessage = outputMessage;

               return  SendOutMessage();
         
            }

            protected override bool SendOutMessage()
            {
                PDUTypesForUserData PDUType = SendMessage.IsPayed ? PDUTypesForUserData.DATA_SM : PDUTypesForUserData.SUBMIT_SM;
                bool sendOk = false;
                int shortMessageLength = 0;
                int max_message_length = 0;
                PacketBase.dataCodingEnum dataCoding;
                byte[] userData = SendMessage.Content.Body;

                //определение кодировки
                if (SendMessage.Content is TextContent && ((TextContent)SendMessage.Content).isUnicode)
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), this.SMPPSettings.DataCoding_unicode);
                    userData = Encoding.Convert(Encoding.BigEndianUnicode, new UnicodeEncoding(true, true), userData);    // Add the Byte order mark preamble
                    shortMessageLength = this.SMPPSettings.Short_message_length_ru;
                    max_message_length = this.SMPPSettings.Max_message_length_ru;

                }
                else
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), this.SMPPSettings.DataCoding_default);
                    shortMessageLength = this.SMPPSettings.Short_message_length_eng;
                    userData = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, userData);
                    max_message_length = this.SMPPSettings.Max_message_length_eng;

                }

                switch (PDUType)
                {   // ussd-ответ
                    case PDUTypesForUserData.SUBMIT_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength)                    // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {
                            SendMessage.PartsCount = 1;
                            SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
                            sm.ShortMessageLength = (byte)userData.Length;
                            sm.PartNumber = 1;
                            sm.MessageText = userData;
                            SetOperatorParameters(sm); ;
                            sendOk = this.EncodeAndSend((this.SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);
                        }
                        else if (userData.Length > max_message_length)
                        {

                            if (this.Logger != null)
                            {
                                this.Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                                ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);
                            }
                        }
                        else
                        {
                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!", LogSeverity.Debug));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length = 0!", EventType.Error, EventSeverity.High);
                            ReadyToSendMessages.Set();
                        }
                        break;
                        
                        //обычное сообщение (с контентом или уведомлением)
                    case PDUTypesForUserData.DATA_SM:
                        if (userData.Length > 0 && userData.Length <= max_message_length)         // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {

                            DataSM dm = Create_DATA_SM(SendMessage, dataCoding);
                            SendMessage.PartsCount = 1;
                            dm.PartNumber = 1;
                            OptionalParameter op_data = new OptionalParameter();
                            op_data.Param = OptionalParameter.tagEnum.message_payload;
                            op_data.Length = (ushort)SendMessage.Content.Body.Length;
                            op_data.Value = userData;
                            dm.OptionalParamList.Add(op_data);
                            SetOperatorParameters(dm);
                            sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm);

                        }
        
                        else if (userData.Length > max_message_length)
                        {
                            // Log the System error 
                            if (Logger != null)
                            {
                                Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                                ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);
                            }
                            ReadyToSendMessages.Set();
                        }
                        else
                        {
                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!", LogSeverity.Debug));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length = 0!", EventType.Error, EventSeverity.High);
                            ReadyToSendMessages.Set();
                        }
                        break;
                }
                return sendOk;
            }

            protected override MessageDeliveryStatus GetMessageStateString(PacketBase.commandStatusEnum code)
            {
                switch (code)
                {
                    case PacketBase.commandStatusEnum.ESME_R_USSD_SMDELIVERYFAILURE:
                        return MessageDeliveryStatus.Unknown;
                       
                    default:
                        return base.GetMessageStateString(code);
                }
            }


            
            /// <summary>
            ///создание PDU Submit_sm
            /// </summary>
            /// <returns>Initialized SUBMIT_SM PDU</returns>
            protected override SubmitSM Create_SUBMIT_SM(OutputMessage outputMessage, PacketBase.dataCodingEnum dataCoding)
            {
                // Create SUBMIT_SM PDU
                SubmitSM sm = new SubmitSM();
                sm.Source.TON = SMPPSettings.Source_TON;
                sm.Source.NPI = SMPPSettings.Source_NPI;
                sm.Source.Address = outputMessage.Source ?? "";                
                sm.Destination.TON = SMPPSettings.Destination_TON;
                sm.Destination.NPI = SMPPSettings.Destination_NPI;
                sm.Destination.Address = outputMessage.Destination ?? "";        
                sm.DataCoding = dataCoding;
                sm.ServiceType = SMPPSettings.ServiceType;
                sm.DefaultSMMessageId = SMPPSettings.DefaultSMMessageId;
                sm.MessageID = outputMessage.ID;
               // sm.RegisteredDelivery = SMPPSettings.RegisteredDelivery;
                sm.RegisteredDelivery = 0;//не нужен отчет о доставке
                if (outputMessage.USSD_UserMessageReference.HasValue) //был ussd-запрос, сейчас этот параметр необязателен
                {
                    OptionalParameter op = new OptionalParameter();
                    op.Param = OptionalParameter.tagEnum.user_message_reference;
                    op.Value = new byte[]
                                   {
                                       (byte) (outputMessage.USSD_UserMessageReference >> 8),
                                       (byte) (outputMessage.USSD_UserMessageReference)
                                   };
                    sm.OptionalParamList.Add(op);
                }

                if (!string.IsNullOrEmpty(SendMessage.USSD_SubscribeCommand)) // Данные для подписки на USSD клубы
                {

                    string pvalue = "CMD=" + SendMessage.USSD_SubscribeCommand;

                    if (SendMessage.USSD_ClubForSubscribeCommand.HasValue)
                        pvalue += ";CID=" + SendMessage.USSD_ClubForSubscribeCommand.Value;
                    if (!string.IsNullOrEmpty(SendMessage.USSD_SubscribeConfirmCommand))
                        pvalue += ";COMMAND=" + SendMessage.USSD_SubscribeConfirmCommand;

                    if (SendMessage.USSD_SubscriptionId.HasValue)
                        pvalue += ";SID=" + SendMessage.USSD_SubscriptionId.Value;

                    OptionalParameter op = new OptionalParameter();
                    op.Param = OptionalParameter.tagEnum.subscription_command;
                    op.Value = Encoding.Default.GetBytes(pvalue);
                    sm.OptionalParamList.Add(op);

                    Logger.Write(new LogMessage("Добавляем опциональный параметр: " + pvalue, LogSeverity.Info));
                }
         
                
                return sm;
            }
            //прописать тарифную категорию
            protected override void SetOperatorParameters(PacketBase pb)
            {
                if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                {
                    if (pb is DataSM)
                    {
                        DataSM dm = pb as DataSM;
                        OptionalParameter op_mct = new OptionalParameter();
                        op_mct.Param = OptionalParameter.tagEnum.message_content_type;
                        op_mct.Value = Encoding.Default.GetBytes(SendMessage.HTTP_Category);// Признак тарифа
                        dm.OptionalParamList.Add(op_mct);

                    }
                }
            }
            protected override DataSM Create_DATA_SM(OutputMessage outputMessage, PacketBase.dataCodingEnum dataCoding)
            {
                DataSM dm = new DataSM();
                dm.Source.TON = SMPPSettings.Source_TON;
                dm.Source.NPI = SMPPSettings.Source_NPI;
                dm.Source.Address = outputMessage.Source ?? "";                       
                dm.Destination.TON = SMPPSettings.Destination_TON;
                dm.Destination.NPI = SMPPSettings.Destination_NPI;
                dm.Destination.Address = outputMessage.Destination ?? "";         // номер телефона в междунар. формате, например "7916xxxxxxx"
                dm.DataCoding = dataCoding;
                dm.ServiceType = SMPPSettings.ServiceType;
                dm.MessageID = outputMessage.ID;

                if (outputMessage.USSD_UserMessageReference.HasValue) //был ussd-запрос, необязательный параметр
                {
                    Trace.WriteLine("outputMessage.USSD_UserMessageReference: " + outputMessage.USSD_UserMessageReference);
                    OptionalParameter op_umr = new OptionalParameter();
                    op_umr.Param = OptionalParameter.tagEnum.user_message_reference;
                    op_umr.Value = new byte[]
                                       {
                                           (byte) (outputMessage.USSD_UserMessageReference >> 8),
                                           (byte) (outputMessage.USSD_UserMessageReference)
                                       };
                    dm.OptionalParamList.Add(op_umr);
                }
                // 2. Charging
                OptionalParameter op_chr = new OptionalParameter();
                op_chr.Param = OptionalParameter.tagEnum.charging;
                op_chr.Value = new byte[] { 0x01 };
                dm.OptionalParamList.Add(op_chr);
                OptionalParameter op_dd = new OptionalParameter();
                op_dd.Param = OptionalParameter.tagEnum.dialog_directive;
                op_dd.Value = new byte[] { (outputMessage.USSD_DialogDirective.HasValue?outputMessage.USSD_DialogDirective.Value:(byte)0x00) };
                dm.OptionalParamList.Add(op_dd);
                dm.RegisteredDelivery = SMPPSettings.RegisteredDelivery;// нужен ли отчет о доставке
                return dm;
            }
            /// <summary>
            /// PDU receiving
            /// 1. Recognize incoming PDU type by command_id
            /// 2. Create corresponding PDU
            /// 3. Send response PDU if needed
            /// </summary>
            /// <returns>
            /// isOk flag
            /// </returns>
            public override void Receive(SMPPConnector conn, int bytesCount, byte[] bytes)
            {
                // 1. Recognize the type of incoming PDU and decode it
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
                    {

                        Logger.Write(new LogMessage("out of memory or end of the array", LogSeverity.Error));
                    }
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
                    idx += (int) len;
                    Logger.Write(new LogMessage("SMPP packet parsed succesfully " + pb.CommandId+" status: "+pb.StatusCode, LogSeverity.Info));

                    if (pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.data_sm_resp )
                    {
                        if (_currentSequenceNumber != pb.SequenceNumber) return;
                    }
                    
                    if (SMPPSettings.IsTransceiver)
                    {
                        RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_TRx, pb, DateTime.Now);
                    }
                    else
                    {
                        if (ReferenceEquals(conn, SMPPConn_Tx))
                        {
                            RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_Tx, pb, DateTime.Now);
                        }
                        else
                        {
                            if (pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp ||
                                pb.CommandId == PacketBase.commandIdEnum.enquire_link ||
                                pb.CommandId == PacketBase.commandIdEnum.unbind_resp ||
                                pb.CommandId == PacketBase.commandIdEnum.unbind ||
                                pb.CommandId == PacketBase.commandIdEnum.generic_nack)
                            {
                                RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_Rx, pb, DateTime.Now);
                            }
                        }
                    }
                
                    switch (pb.CommandId)
                    {
                        case PacketBase.commandIdEnum.bind_transceiver_resp:
                            BindResponseTransceiver brt = new BindResponseTransceiver();
                            brt.Parse(bytes);
                            if (brt.StatusCode == PacketBase.commandStatusEnum.ESME_ROK ||
                                brt.StatusCode == PacketBase.commandStatusEnum.ESME_RALYBND)
                            {
                                Logger.Write(  new LogMessage( "Connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, LogSeverity.Info));
                                ServiceManager.LogEvent("Connected (transceiver) to " + brt.SystemId + "...Status = " +  brt.StatusCode, EventType.Info, EventSeverity.Normal);
                                pcConnectionState.RawValue = 1;
                                _connectorState.State = "Connected";
                               
                              if (SendMessage==null)  ReadyToSendMessages.Set();
                               
                            }
                            else
                            {
                                pcConnectionState.RawValue = 0;
                                ReadyToSendMessages.Reset();
                                Logger.Write(  new LogMessage( "Not connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, LogSeverity.Info));
                                _connectorState.State = "Not connected";
                                ServiceManager.LogEvent("Not connected (transceiver) " + brt.SystemId + "...Status = " + brt.StatusCode, EventType.Error, EventSeverity.High);
                            }
                            break;
                        case PacketBase.commandIdEnum.bind_transmitter_resp:
                            BindResponseTransmitter brt_t = new BindResponseTransmitter();
                            brt_t.Parse(bytes);
                  
                             if( (brt_t.StatusCode == PacketBase.commandStatusEnum.ESME_ROK ||
                                brt_t.StatusCode == PacketBase.commandStatusEnum.ESME_RALYBND)&& SendMessage==null)
                            ReadyToSendMessages.Set();
                            break;
                        case PacketBase.commandIdEnum.bind_receiver_resp:
                            BindResponseReceiver brt_r = new BindResponseReceiver();
                            brt_r.Parse(bytes);
              
                            break;
                        case PacketBase.commandIdEnum.data_sm_resp:
                            DataSMResponse dtsmr = new DataSMResponse();
                            dtsmr.Parse(bytes);
                            DataSM pb_dtsm_fnd = _lastOutPDU as DataSM;

                            if (pb_dtsm_fnd != null && pb_dtsm_fnd is DataSM)
                            {
                                DataSM dtsm_fnd = (DataSM) pb_dtsm_fnd;
                                MessageDeliveryStatus mds = GetMessageStateString(dtsmr.StatusCode);
                                if (mds != MessageDeliveryStatus.Unknown)
                                {
                                    if (SendMessage != null && SendMessage.ID == dtsm_fnd.MessageID ) 
                                    {
                                        string message_id = dtsmr.MessageId;
                                        UpdateOutboxState(SendMessage, (int)GetMessageStateString(dtsmr.StatusCode),
                                            dtsmr.RealStatusCode.ToString(), message_id);

                                        if (mds == MessageDeliveryStatus.Delivered && dtsm_fnd.RegisteredDelivery == 1)
                                        {
                                            msgsWaitingReceits.Add<OutputMessage>(message_id, SendMessage);
                                            Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Item added to cache. Key=%" + message_id + "%");
                                        }

                                    }
                                    _connectorState.State = "Connected";
                                    SendMessage = null;
                                    ReadyToSendMessages.Set();
                                }
                                else
                                {
                                    AddOutboxSendHistory(dtsm_fnd.MessageID, ((int) mds), dtsmr.RealStatusCode.ToString());
                                }
                                _connectorState.State = "Connected";
                                waitingResp.Set();
                            }
                            break;
                        case PacketBase.commandIdEnum.deliver_sm:
                            DeliverSM dlsm = new DeliverSM();
                            dlsm.Parse(bytes);
                    
                            DeliverSMResponse dlsm_r = new DeliverSMResponse(ref dlsm);
                            EncodeAndSendResp((this.SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx,  dlsm_r);
                      

                            bool[] esmFlags = Cryptany.Common.Utils.Math.GetBitsArray(dlsm.EsmClass);
                            bool _isreceipt = esmFlags[2];
                            
                            if (!_isreceipt)
                            {
                                object userMessageReference = null;
                                string phoneModel = "";
                                foreach (OptionalParameter op in dlsm.OptionalParamList)
                                {
                                    if (op.Param == OptionalParameter.tagEnum.user_message_reference)
                                    {
                                        userMessageReference = (uint) (op.Value[1] | op.Value[0] << 8);
                                    }
                                    if (op.Param == OptionalParameter.tagEnum.phone_vendor_and_model)
                                    {
                                        if (Enum.GetName(typeof (PacketBase.dataCodingEnum), dlsm.DataCoding) ==
                                            this.SMPPSettings.DataCoding_default)
                                        {
                                            phoneModel = Encoding.Default.GetString(op.Value);
                                        }
                                        else if (Enum.GetName(typeof (PacketBase.dataCodingEnum), dlsm.DataCoding) ==
                                                 this.SMPPSettings.DataCoding_unicode)
                                        {
                                            phoneModel = Encoding.BigEndianUnicode.GetString(op.Value);
                                        }
                                        else
                                        {
                                            phoneModel = Encoding.Default.GetString(op.Value);
                                        }
                                    }
                                }
                          
                                if (dlsm.Source.Address != "" && userMessageReference != null)
                                {
                                    byte[] userData = null;
                                    if (dlsm.ShortMessageLength > 0) // короткое сообщение
                                    {
                                        userData = dlsm.MessageText;
                                    }
                                    // Create MSMQ message and send it to Router main input MSMQ queue
                                    Send_MSMQ_MessageToRouterInputQueue(dlsm, userData, Convert.ToUInt32(userMessageReference), phoneModel);
                                }
                            }
                            else
                            {
                                ProcessDeliveryReceit(dlsm);
                            }
                            break;
                        case PacketBase.commandIdEnum.enquire_link:
                            EnquireLink enqlnk = new EnquireLink();
                            enqlnk.Parse(bytes);
                            EnquireLinkResponse enqlnk_r = new EnquireLinkResponse(ref enqlnk);
                            EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, enqlnk_r);
                            _connectorState.State = "Connected";
                            break;
                        case PacketBase.commandIdEnum.enquire_link_resp:
                            EnquireLinkResponse enqlnkr = new EnquireLinkResponse();
                            enqlnkr.Parse(bytes);
                     
                            _connectorState.State = "Connected";
                            if (SendMessage == null) ReadyToSendMessages.Set();
                            break;
                        case PacketBase.commandIdEnum.submit_sm_resp:
                            SubmitSMResponse ssmr = new SubmitSMResponse();
                            ssmr.Parse(bytes);
                            //PacketBase pb_ssm_fnd = GetUserData(ssmr.SequenceNumber);
                            PacketBase pb_ssm_fnd =_lastOutPDU;;
                            if (SendMessage != null && pb_ssm_fnd != null && pb_ssm_fnd.MessageID == SendMessage.ID)
                            {
                                SubmitSM ssm_fnd = (SubmitSM) pb_ssm_fnd;
                                string message_id = ssmr.MessageId;
                     

                                MessageDeliveryStatus mds = GetMessageStateString(ssmr.StatusCode);
                                if (mds!=MessageDeliveryStatus.Unknown)
                                {
                                    if (SendMessage.PartsCount == ssm_fnd.PartNumber || mds != MessageDeliveryStatus.Delivered)
                                    {
                                        UpdateOutboxState(SendMessage, (int)GetMessageStateString(ssmr.StatusCode), GetMessageStateString(ssmr.StatusCode).ToString(), message_id);

                                        if (mds == MessageDeliveryStatus.Delivered && ssm_fnd.RegisteredDelivery == 1)
                                        {
                                            msgsWaitingReceits.Add<OutputMessage>(message_id, SendMessage);
                                        }
                                        
                                        SendMessage = null;
                                        ReadyToSendMessages.Set();
                                    }

                                }

                                else
                                {
                                    AddOutboxSendHistory(SendMessage.ID, (int) MessageDeliveryStatus.Unknown, ssmr.RealStatusCode.ToString());
                                }
                                _connectorState.State = "Connected";
                                waitingResp.Set();
                            }
                            break;
                    }
                }
            }

            /// <summary> 
            /// отправляет входящее сообщение в нужную ussd-очередь, под каждый номер своя очередь
            /// </summary>
            /// <returns>
            /// isOk flag
            /// </returns>
            public bool Send_MSMQ_MessageToRouterInputQueue(PacketBase pb, byte[] userData, uint userMessageReference, string phoneModel)
            {
                bool isOk = false;
                try
                {

                    if (userData != null && userData.Length > 0)
                    {

                        if (pb is DeliverSM)
                        {
                            DeliverSM dlsm = (DeliverSM) pb;
                       
                            string msgText = "";
                            if (Enum.GetName(typeof (PacketBase.dataCodingEnum), dlsm.DataCoding) ==
                                SMPPSettings.DataCoding_default)
                            {
                                msgText = Encoding.Default.GetString(userData);
                            }
                            else if (Enum.GetName(typeof (PacketBase.dataCodingEnum), dlsm.DataCoding) ==
                                     SMPPSettings.DataCoding_unicode)
                            {
                                msgText = Encoding.BigEndianUnicode.GetString(userData);
                            }
                            else
                            {
                                msgText = Encoding.Default.GetString(userData);
                            }

                            Message newMessage = new Message(IdGenerator.NewId, dlsm.Source.Address, dlsm.Destination.Address, ConnectorId, msgText,
                                                             userMessageReference, phoneModel);

                            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["redirectPortal"]) && bool.Parse(ConfigurationManager.AppSettings["redirectPortal"]))
                            {
                                using (
                                    MessageQueue mq =   new MessageQueue(ConfigurationManager.AppSettings["InputMQPrefix"] +dlsm.Destination.Address))
                                {
                                    System.Messaging.Message msg = new System.Messaging.Message(newMessage);
                                    msg.Formatter = new BinaryMessageFormatter();
                                    msg.AttachSenderId = false;
                                    mq.Send(msg);
                                    isOk = true;
                                }
                            }
                            else
                            {
                                using (
                                    MessageQueue MainInputSMSQueue =ServiceManager.MainInputSMSQueue)
                                {
                                    MainInputSMSQueue.Send(newMessage);
                                    isOk = true;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Logger != null)
                    {
                        this.Logger.Write(new LogMessage("Exception in SMPPMessageManagerUSSD Send_MSMQ_MessageToRouterInputQueue method: " + e.ToString(), LogSeverity.Error));
                    }
                }
                return isOk;
            }

       
        }
    }
}

