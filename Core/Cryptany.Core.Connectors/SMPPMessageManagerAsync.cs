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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Timers;
using System.Threading;
using System.Messaging;
using Cryptany.Core.Connectors;
using Cryptany.Core.Management.WMI;
using Cryptany.Core.SmppLib;
using Cryptany.Common.Logging;
using Cryptany.Core.Interaction;
using Cryptany.Common.Utils;
using Cryptany.Core.Management;
using System.Configuration;
using System.Linq;


namespace Cryptany.Core
{


    public class SMPPMessageManagerAsync : SMPPMessageManager
    {

        public InternalConnectorQueue PDUList_OUT;     // внутренняя очередь исходящих PDU
        /// <summary>
        /// Constructor of the SMPPMM. Reads up the database and prepares caches for work.
        /// </summary>
        public SMPPMessageManagerAsync(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {}

        protected void SetPDUOutResponseCounter(DateTime timeSent)
        {
            pcPDUOutResponseTime.RawValue = (long)(DateTime.Now - timeSent).TotalMilliseconds;
            Trace.WriteLine("Updated PDUOutResponseCounter");
        }

        /// <summary>
        /// Проверка - возможна ли отправка следующего пользовательского сообщения
        /// </summary>
        /// <returns></returns>
        public override bool CanSendNextMessage()
        {

            if (ReadyToSendMessages.WaitOne())
            {
                if (SMPPSettings.RepeatSendTimeout > 0)
                    Thread.Sleep(SMPPSettings.RepeatSendTimeout);
                return true;
            }
            return false;

        }

        protected override void Init(AbstractConnectorSettings settings)
        {
            PDUList_OUT = new InternalConnectorQueue((settings as SMPPSettings).Max_internal_queue_size);
            base.Init(settings);
        }

        public override bool SendUserData(OutputMessage outputMessage)
        {
            lock (this)
            {
                Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name + " start sending smpp message " + outputMessage);
                //в асинхронном режиме не ждем ответа
                SendMessage = outputMessage;
                _connectorState._lastReceivedFromRouterTime = DateTime.Now;
                pcSMSInQueueTime.RawValue = (long)(_connectorState._lastReceivedFromRouterTime - SendMessage.TimeReceived).TotalMilliseconds;
                return SendOutMessage();
            }
        }


        protected override bool SendOutMessage()
        {
            if (SendMessage == null)
            {
                ServiceManager.LogEvent("Sending Out Message....SendMessage is null!", EventType.Error, EventSeverity.High);
                Logger.Write(new LogMessage(" Sending Out Message....SendMessage is null! ", LogSeverity.Error));
                return true;
            }

            if (SendMessage.Content == null)
            {
                ServiceManager.LogEvent("Sending Out Message....Content is null!", EventType.Error, EventSeverity.High);
                Logger.Write(new LogMessage(" Sending Out Message....Content is null! ", LogSeverity.Error));
                return true;
            }
            PDUTypesForUserData PDUType = SMPPSettings.Send_by_DATA_SM ? PDUTypesForUserData.DATA_SM : PDUTypesForUserData.SUBMIT_SM;

            bool sendOk = false;
            ushort sarCount = 1;
            int shortMessageLength;
            int sar_segment_length;
            int max_message_length;

            PacketBase.dataCodingEnum dataCoding;

            try
            {
                byte[] userData = SendMessage.Content.Body;
                if (userData == null) return true;
                if (SendMessage.IsFlash)
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_Flash);

                    shortMessageLength = SMPPSettings.Short_message_length_ru;
                    sar_segment_length = SMPPSettings.Sar_segment_length_ru;
                    max_message_length = SMPPSettings.Max_message_length_ru;
                }
                else if (((TextContent)SendMessage.Content).isUnicode)
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_unicode);

                    shortMessageLength = SMPPSettings.Short_message_length_ru;
                    sar_segment_length = SMPPSettings.Sar_segment_length_ru;
                    max_message_length = SMPPSettings.Max_message_length_ru;
                }
                else
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_default);

                    shortMessageLength = SMPPSettings.Short_message_length_eng;
                    sar_segment_length = SMPPSettings.Sar_segment_length_eng;
                    max_message_length = SMPPSettings.Max_message_length_eng;
                }
                if (userData.Length > shortMessageLength && userData.Length <= max_message_length)
                {

                    sarCount = SMPPMessageParts.SplitToSAR(ConnectorId, userData, sar_segment_length);
                }
                switch (PDUType)
                {
                    case PDUTypesForUserData.SUBMIT_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength) // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {
                            SendMessage.PartsCount = 1;
                            SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
                            sm.ShortMessageLength = (byte)userData.Length;
                            sm.MessageText = userData;
                            sm.PartNumber = 1;

                            if (!string.IsNullOrEmpty(SendMessage.HTTP_Category) || SendMessage.Source.StartsWith("4041"))
                            {
                                SetOperatorParameters(sm);
                            }

                            sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);

                        }

                        else if (userData.Length > shortMessageLength && userData.Length <= max_message_length)
                        {
                          sendOk= SendSARMessage(dataCoding, sarCount, SMPPSettings.SendSarPartInPayload);
                            //SendMessage.PartsCount = sarCount;
                            //for (ushort i = 1; i <= sarCount; i++)
                            //{
                            //    SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
                            //    sm.ShortMessageLength = 0;
                            //    sm.MessageText = new byte[0];
                            //    sm.OptionalParamList.Clear();
                            //    sm.PartNumber = i;
                            //    sm.OptionalParamList = SMPPMessageParts.GetOptionalParameterList(ConnectorId, i);

                            //    if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                            //    {
                            //        SetOperatorParameters(sm);
                            //    }


                            //    sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);
                            //    if (!sendOk) break;

                            //    if (SMPPSettings.RepeatSendTimeout > 0) //между частями тоже должен быть интервал
                            //        Thread.Sleep(SMPPSettings.RepeatSendTimeout);
                            //}
                        }
                        else if (userData.Length > max_message_length)
                        {

                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Error));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);
                      
                        }
                        else
                        {
                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!", LogSeverity.Error));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ".Message length = 0!", EventType.Error, EventSeverity.High);

                        }
                        break;
                    case PDUTypesForUserData.DATA_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength) // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {
                            DataSM dm = Create_DATA_SM(SendMessage, dataCoding);
                            OptionalParameter op_data = new OptionalParameter();
                            op_data.Param = OptionalParameter.tagEnum.message_payload;
                            op_data.Value = userData;
                            dm.OptionalParamList.Add(op_data);

                            sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm);

                        }
                        else if (userData.Length > shortMessageLength && userData.Length <= max_message_length) // длинное сообщение (несколько SAR сегментов)
                        {

                            for (int i = 1; i <= sarCount; i++)
                            {
                                DataSM dm = Create_DATA_SM(SendMessage, dataCoding);
                                dm.OptionalParamList.Clear();
                                dm.OptionalParamList = SMPPMessageParts.GetOptionalParameterList(ConnectorId, i);

                                sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm);

                            }
                        }
                        else if (userData.Length > max_message_length)
                        {
                            // Log the System error 
                            if (Logger != null)
                            {
                                Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                                ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);
                            }
                          
                        }
                        else
                        {
                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!", LogSeverity.Debug));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ".Message length = 0!", EventType.Error, EventSeverity.High);
                           
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                sendOk = false;
            }

            Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Thread " + Thread.CurrentThread.Name + " finished sending smpp message " + SendMessage);
            return sendOk;
        }

        protected override bool SendSARMessage(PacketBase.dataCodingEnum dataCoding, ushort sarCount,
                                               bool SendSarPartInPayload)
        {
            bool sendOk = false;
            SendMessage.PartsCount = sarCount;

            for (ushort i = 1; i <= sarCount; i++)
            {
                SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
                sm.OptionalParamList.Clear();
                sm.PartNumber = i;
                sm.OptionalParamList = SMPPMessageParts.GetOptionalParameterList(ConnectorId, i);

                if (SendSarPartInPayload)
                {
                    sm.ShortMessageLength = 0;
                    sm.MessageText = new byte[0];

                }
                else
                {

                    OptionalParameter payload =
                        sm.OptionalParamList.Single(item => item.Param == OptionalParameter.tagEnum.message_payload);
                    sm.OptionalParamList.Remove(payload);
                    sm.ShortMessageLength = (byte) payload.Value.Length;
                    sm.MessageText = payload.Value;
                }

                if (!string.IsNullOrEmpty(SendMessage.HTTP_Category) || SendMessage.Source.StartsWith("4041"))
                {
                    SetOperatorParameters(sm);
                }


                sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);
                if (!sendOk) break;

                if (SMPPSettings.RepeatSendTimeout > 0) //между частями тоже должен быть интервал
                    Thread.Sleep(SMPPSettings.RepeatSendTimeout);
            }

            return sendOk;

        }
        /// <summary>
        /// Закодировать и послать PDU, не ждем _resp, останавливаемся, только если список сообщений без ответа переполнен
        /// </summary>
        /// <param name="trMode">SMPP transfer mode</param>
        /// <param name="pb">PDU to send</param>
        /// <returns>send status</returns>
        public override bool EncodeAndSend(TransferModes trMode, PacketBase pb)
        {
            
            bool sendOk = false;
            SMPPConnector conn = null;
            PDUTimeouts tmo = null;

            try
            {
                mut_Send.WaitOne();
                byte[] bytes = pb.GetEncoded();

                switch (trMode)
                {
                    case TransferModes.TRx:
                        conn = SMPPConn_TRx;
                        tmo = PDUTimeouts_TRx;
                        break;
                    case TransferModes.Tx:
                        conn = SMPPConn_Tx;
                        tmo = PDUTimeouts_Tx;
                        break;
                    case TransferModes.Rx:
                        conn = SMPPConn_Rx;
                        tmo = PDUTimeouts_Rx;
                        break;
                }
     
                if ((pb is SubmitSM || pb is DataSM))
                {
                    pcOutgoingMessagesPerSecond.Increment();
                }


                if (pb.CommandId == PacketBase.commandIdEnum.submit_sm || pb.CommandId == PacketBase.commandIdEnum.data_sm)
                    PDUList_OUT.AddPacket(new PacketInfo(pb, SendMessage, DateTime.Now, PDUTimeouts.PDUDirection.Out));
                
                if (conn != null) sendOk = conn.Send(bytes);

                if (sendOk)
                {
                    RegPDUTime(PDUTimeouts.PDUDirection.Out, ref tmo, pb, DateTime.Now);

              
                    if (PDUList_OUT.Count > this.SMPPSettings.MaxNonRespPDU)
                    {
                        Logger.Write(new LogMessage("Переполнена очередь исходящих PDU без ответа", LogSeverity.Alert));
                       // ServiceManager.LogEvent("Переполнена очередь исходящих PDU без ответа", EventType.Warning, EventSeverity.Normal);
                        ReadyToSendMessages.Reset();
                        Thread.Sleep(SMPPSettings.ErrorPDU_TO_1);
                    }

                }
            }
            catch (Exception ex)
            {
                ReadyToSendMessages.Reset();
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));

            }
            finally
            {

                mut_Send.ReleaseMutex();

            }
            return sendOk;
        }

        /// <summary>
        //overriden for async mode
        /// </summary>
        /// <returns>
        /// isOk flag
        /// </returns>
        public override void Receive(SMPPConnector conn, int bytesCount, byte[] bytes)
        {
            // TODO: рефакторинг...
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

                    case PacketBase.commandIdEnum.deliver_sm: //нотификация от Оператора
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
                                    /*
                                    if (op.Param == OptionalParameter.tagEnum.source_port)
                                    {
                                        //op.value сделать int'ом
                                        switch (op.Value)
                                        {
                                            case 1:
                                                //разовый запрос (STOP/СТОП)
                                                //
                                                break;
                                            
                                            case 3:
                                                //подписка
                                                //
                                                break;
                                            case 4:
                                                //отписка
                                                //
                                                break;
                                            case 5:
                                                //регулярный платёж (charged N rub. Next pay date дата_и_время)
                                                //уже обрабатывается роутером при AddToInbox
                                                break;
                                            case 6:
                                     
                                                //блокировка услуги (block_subscription MSISDN)
                                                //
                                                break;

                                            case 7:
                                                //блокировка абонента
                                                break;
                                            case 8:
                                                //разблокировка абонента
                                                break;
                                            case 9:
                                                //смена MSISDN
                                                break;
                                        }
                                    }
                                   */ 
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

                                    //вывод значения source_port
                                    foreach (OptionalParameter param in dlsm.OptionalParamList)
                                    {
                                        if (param.Param == OptionalParameter.tagEnum.source_port)
                                        {
                                            log += ", source_port: ";
                                            for (int i = 0; i < param.Value.Length; i++ )
                                                log += param.Value[i].ToString();
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
        /// Спец обработка Submit_SM_Resp
        /// </summary>
        /// <param name="pb_ssm_fnd_info">инфо об отправленном пакете, на который пришел ответ</param>
        /// <param name="ssmr">пришедший пакет</param>
        /// <param name="mds">статус доставки для базы</param>
        /// <returns>true если сообщение является специальным</returns>
        protected virtual bool ProcessAsSpecialResp(PacketInfo pb_ssm_fnd_info, SubmitSMResponse ssmr, MessageDeliveryStatus mds)
        {
            return false;
        }

        /// <summary>
        /// Получает последний последний исходящий PDU по указанному номеру последовательности 
        /// </summary>
        /// <param name="seqNum">Номер последовательности</param>
        /// <returns>Исходящий PDU</returns>
        public PacketInfo GetUserData(uint seqNum)
        {
            PacketInfo info_found = null;


            lock (PDUList_OUT)
            {
                int lastPDUIndex = PDUList_OUT.Count - 1;
                //  lock ?
                if (seqNum != 0)
                {
                    // получить пользовательский PDU по номеру последовательности
                    for (int i = lastPDUIndex; i >= 0; i--)
                    {
                        if ((PDUList_OUT[i] as PacketInfo).Packet is SubmitSM || (PDUList_OUT[i] as PacketInfo).Packet is DataSM)
                        {
                            PacketBase pb = ((PacketInfo)PDUList_OUT[i]).Packet;

                            if ((PDUList_OUT[i] as PacketInfo).Packet.SequenceNumber == seqNum)
                            {
                                info_found = (PacketInfo)PDUList_OUT[i];
                                break;
                            }
                        }

                    }
                }
                else
                {
                    // получить последний исходящий PDU
                    for (int i = lastPDUIndex; i >= 0; i--)
                    {
                        PacketInfo info = (PacketInfo)PDUList_OUT[i];

                        if (info.Packet is SubmitSM || info.Packet is DataSM || info.Packet is EnquireLink)
                        {
                            //PacketBase pb = (PacketBase)PDUList_OUT[i];
                            info_found = info;
                            break;
                        }
                    }
                }
            }
            return info_found;
        }

        public override void RegPDUTime(PDUTimeouts.PDUDirection pduDirect, ref PDUTimeouts tmo, PacketBase pb, DateTime dt)
        {
            if (pb == null) throw new ArgumentNullException("pb");
            if (tmo == null) throw new ArgumentNullException("tmo");
            if (pduDirect == PDUTimeouts.PDUDirection.In)
            {
                tmo.AnyInPDU_Time = dt;

                if (pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp || pb.CommandId == PacketBase.commandIdEnum.data_sm)
                {
                    PacketInfo seq_info = GetUserData(pb.SequenceNumber);
                    PacketBase seq_pb_out = (seq_info == null) ? null : seq_info.Packet;
                    if (seq_pb_out == null)
                    {
                        Trace.WriteLine("Не найден пакет с номером " + pb.SequenceNumber);
                        return;
                    }

                    MessageDeliveryStatus mds = GetMessageStateString(pb.StatusCode);
                    if (mds == MessageDeliveryStatus.Unknown) //переполнена очередь на смсц
                    {
                        ServiceManager.LogEvent("Переполнена очередь на SMSC " + pb.StatusCode, EventType.Warning, EventSeverity.Low);
                        ReadyToSendMessages.Reset();
                        tmo.ErrorResp_Time = dt;
                    }

                    else if ((seq_pb_out as DataPacketBase).PartNumber == seq_info.Message.PartsCount || mds != MessageDeliveryStatus.Delivered)
                    {
                        tmo.ErrorResp_Time = DateTime.MinValue;
                        ReadyToSendMessages.Set();
                    }

                }

                if (pb.CommandId == PacketBase.commandIdEnum.enquire_link_resp)                   // && _currentSequenceNumber == pb.SequenceNumber)или добавить?
                    tmo.NeedResp = false;
                
                if (pb.CommandId == PacketBase.commandIdEnum.bind_transceiver_resp ||
                    pb.CommandId == PacketBase.commandIdEnum.bind_transmitter_resp ||
                    pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp)
                {
                    tmo.NeedResp = false;

                    if (pb.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)      // нет ошибки SMSC
                    {
                        tmo.IsBound = true;
                        return;
                    }
                    tmo.IsBound = false;
                }

                if (pb.CommandId == PacketBase.commandIdEnum.unbind)
                    tmo.NeedResp = false;
                
                if (pb.CommandId == PacketBase.commandIdEnum.unbind_resp)
                    if (pb.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        tmo.IsBound = false;
            }
            
            if (pduDirect == PDUTimeouts.PDUDirection.Out)
            {
                switch (pb.CommandId)
                {
                    case PacketBase.commandIdEnum.submit_sm:
                    case PacketBase.commandIdEnum.data_sm:
                        tmo.DataOutPDU_Time = dt;
                        break;

                    case PacketBase.commandIdEnum.enquire_link:
                        tmo.DataOutPDU_Time = dt;
                        tmo.NeedResp = true;
                        break;
                }
            }
        }
    }
}
        

            



   

  

    

  
        



    



















































