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
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text;
using System.Xml;
using System.Resources;
using System.Timers;
using System.Threading;
using System.Messaging;
using Cryptany.Core.SmppLib;
using Cryptany.Common.Logging;

using Cryptany.Core;

namespace Cryptany
{
    namespace Core
    {
        /// <summary>
        /// Manages all aspects of SMPP message creation for Life system. Translates string representation of the 
        /// message attributes into adjacent classes.
        /// </summary>
        public class SMPPMessageManagerLife : SMPPMessageManager
        {
            public SMPPMessageManagerLife(ConnectorSettings cs, ILogger logger)
                : base(cs, logger)
            { }

            /// <summary>
            /// PDU sending with user data (SUBMIT_SM, DATA_SM)
            /// </summary>
            /// <returns>
            /// isOk flag
            /// </returns>
            public override bool SendUserData(OutputMessage outputMessage, byte[] userData)
            {
                //if (Logger != null)
                //{
                //    Logger.Write(new LogMessage("SMPP Life connector SMSCCode = " + SMPPSettings.SMSCCode + " is sending message. Message properties: Destination = " + outputMessage.Destination + " Source = " + outputMessage.Source + " Text = " + ((TextContent)outputMessage.Content).MsgText + " Transaction = " + outputMessage.TransactionId, LogSeverity.Info));
                //}
                PDUTypesForUserData PDUType = SMPPSettings.Send_by_DATA_SM ? PDUTypesForUserData.DATA_SM : PDUTypesForUserData.SUBMIT_SM;
                bool sendOk = false;
                int sarCount = 0;
                // Максимальный размер короткого сообщения
                int shortMessageLength = 0;
                // SAR параметры
                int sar_segment_length = 0;
                int max_message_length = 0;
                // Кодировка сообщения
                PacketBase.dataCodingEnum dataCoding;
                if (outputMessage.Content is TextContent && ((TextContent)outputMessage.Content).isUnicode == true)
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), this.SMPPSettings.DataCoding_unicode);
                    // Преобразование входящего массива пользовательских данных в соотв. с заданной кодировкой                     
                    userData = UnicodeEncoding.Convert(Encoding.BigEndianUnicode, new UnicodeEncoding(true, true), userData);    // Add the Byte order mark preamble
                    // Инициализация максимального размера короткого сообщения
                    shortMessageLength = this.SMPPSettings.Short_message_length_ru;
                    // Инициализация SAR параметров
                    sar_segment_length = this.SMPPSettings.Sar_segment_length_ru;
                    max_message_length = this.SMPPSettings.Max_message_length_ru;
                }
                else
                {
                    dataCoding = (PacketBase.dataCodingEnum)Enum.Parse(typeof(PacketBase.dataCodingEnum), this.SMPPSettings.DataCoding_default);
                    // Инициализация максимального размера короткого сообщения
                    shortMessageLength = this.SMPPSettings.Short_message_length_eng;
                    // Инициализация SAR параметров
                    sar_segment_length = this.SMPPSettings.Sar_segment_length_eng;
                    max_message_length = this.SMPPSettings.Max_message_length_eng;
                }
                if (userData.Length > shortMessageLength && userData.Length <= max_message_length)           // длинное сообщение (может содержать несколько сегментов)
                {
                    // Разбить его на части по sar_segment_length байт 
                    sarCount = this.SMPPMessageParts.SplitToSAR(this.ConnectorId, userData, sar_segment_length);
                }
                switch (PDUType)
                {
                    case PDUTypesForUserData.SUBMIT_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength)                    // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {
                            SubmitSM sm = this.Create_SUBMIT_SM(outputMessage, dataCoding);
                            // Life system optional parameters
                            // 1. Charging_id 
                            OptionalParameter op = new OptionalParameter();
                            op.Param = OptionalParameter.tagEnum.charging_id;
                            op.Value = new byte[2] { 0x30, 0x00 };     // ASCII представление символьного значения "0" 
                            sm.OptionalParamList.Add(op);
                            // User message data
                            sm.ShortMessageLength = (byte)userData.Length;
                            sm.MessageText = userData;
                            // 2. Send SUBMIT_SM PDU to SMSC
                            outputMessage.TariffId = OutputMessage.GetTariffid(ConnectorId, outputMessage.Source);
                            UpdateOutboxAdditional(outputMessage.ID, outputMessage.TariffId, 1);
                            sendOk = this.EncodeAndSend((this.SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm, outputMessage.ID, true);
                        }
                        else if (userData.Length > max_message_length)
                        {
                            // Log the System error 
                            if (this.Logger != null)
                            {
                                this.Logger.Write(new LogMessage("Error Message ID: " + outputMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                            }
                        }
                        break;
                    case PDUTypesForUserData.DATA_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength)         // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {
                            DataSM dm = this.Create_DATA_SM(outputMessage, dataCoding);
                            // Life system optional parameters
                            // 1. Charging_id 
                            OptionalParameter op_ch = new OptionalParameter();
                            op_ch.Param = OptionalParameter.tagEnum.charging_id;
                            op_ch.Value = new byte[2] { 0x30, 0x00 };  // ASCII представление символьного значения "0" 
                            dm.OptionalParamList.Add(op_ch);
                            // User message data
                            OptionalParameter op_data = new OptionalParameter();
                            op_data.Param = OptionalParameter.tagEnum.message_payload;
                            op_data.Value = userData;
                            dm.OptionalParamList.Add(op_data);
                            // 2. Send DATA_SM PDU to SMSC
                            outputMessage.TariffId = OutputMessage.GetTariffid(ConnectorId, outputMessage.Source);
                            UpdateOutboxAdditional(outputMessage.ID, outputMessage.TariffId, 1);
                            sendOk = this.EncodeAndSend((this.SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm, outputMessage.ID, true);
                        }
                        else if (userData.Length > shortMessageLength && userData.Length <= max_message_length)          // длинное сообщение (несколько SAR сегментов)
                        {
                            for (int i = 1; i <= sarCount; i++)
                            {
                                DataSM dm = this.Create_DATA_SM(outputMessage, dataCoding);
                                dm.OptionalParamList.Clear();
                                // получить коллекцию всех SAR параметров с пользовательскими данными
                                dm.OptionalParamList = this.SMPPMessageParts.GetOptionalParameterList(this.ConnectorId, i);
                                // 2. Send DATA_SM PDU to SMSC
                                sendOk = this.EncodeAndSend((this.SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm, outputMessage.ID, true);
                            }
                        }
                        else if (userData.Length > max_message_length)
                        {
                            // Log the System error 
                            if (this.Logger != null)
                            {
                                this.Logger.Write(new LogMessage("Error Message ID: " + outputMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                            }
                        }
                        break;
                }
                return sendOk;
            }

            public override void RegPDUTime(PDUTimeouts.PDUDirection pduDirect, ref PDUTimeouts tmo, PacketBase pb, DateTime dt)
            {
                if (pduDirect == PDUTimeouts.PDUDirection.In)
                {
                    if (Enum.IsDefined(typeof(PacketBase.commandIdEnum), pb.CommandId))
                    {
                        // зарегистрировать получение входящего PDU
                        tmo.AnyInPDU_Time = dt;
                    }
                    else
                    {
                        // игнорировать получение входящего PDU
                        return;
                    }
                    // Обработка ответов SMSC (*_RESP PDU или GENERIC_NACK PDU)
                    if (pb.CommandId == PacketBase.commandIdEnum.bind_transceiver_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.bind_transmitter_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.deliver_sm_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.data_sm_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.enquire_link_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.unbind_resp ||
                        pb.CommandId == PacketBase.commandIdEnum.generic_nack)
                    {
                        tmo.NeedResp = false;
                        if (pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp ||
                            pb.CommandId == PacketBase.commandIdEnum.data_sm_resp)
                        {
                            tmo.PDURetryCount = 0;
                        }
                        if (pb.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)      // нет ошибки SMSC
                        {
                            // BIND_TRANSCEIVER_RESP, BIND_TRANSMITTER_RESP, BIND_RECEIVER_RESP 
                            if (pb.CommandId == PacketBase.commandIdEnum.bind_transceiver_resp ||
                                pb.CommandId == PacketBase.commandIdEnum.bind_transmitter_resp ||
                                pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp)
                            {
                                tmo.IsBound = true;
                            }
                            // UNBIND_RESP
                            if (pb.CommandId == PacketBase.commandIdEnum.unbind_resp)
                            {
                                tmo.IsBound = false;
                            }
                            // ENQUIRE_LINK_RESP
                            if (pb.CommandId == PacketBase.commandIdEnum.enquire_link_resp)
                            {
                                // игнорировать влияние на флаги ошибки
                                return;
                            }
                            // Сбросить флаги ошибки
                            tmo.ErrorPDUSeqNum = 0;
                            tmo.ErrorResp_Time = DateTime.MinValue;
                            tmo.ErrorRetryCount = 0;
                            tmo.ErrorPDU_TO = new TimeSpan(0, 0, this.SMPPSettings.ErrorPDU_TO_1);
                        }
                        else                                                             // есть ошибка SMSC
                        {
                            // BIND_TRANSCEIVER_RESP, BIND_TRANSMITTER_RESP, BIND_RECEIVER_RESP
                            if (pb.CommandId == PacketBase.commandIdEnum.bind_transceiver_resp ||
                                pb.CommandId == PacketBase.commandIdEnum.bind_transmitter_resp ||
                                pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp)
                            {
                                tmo.IsBound = false;
                            }
                            // Исключение: не регистрировать, как ошибку
                            if ((pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp || pb.CommandId == PacketBase.commandIdEnum.data_sm_resp) &&
                                (pb.StatusCode == PacketBase.commandStatusEnum.ESME_RINVDSTADR || pb.StatusCode == PacketBase.commandStatusEnum.ESME_RINVSRCADR ||
                                 pb.StatusCode == PacketBase.commandStatusEnum.ESME_RSYSERR || pb.StatusCode == PacketBase.commandStatusEnum.ESME_R_LIFE_NOT_ENOUGH_MONEY))
                            {
                                return;
                            }
                            // Регистрация ошибки SMSC: инициирует повторную отправку ошибочного пакета
                            if (tmo.ErrorPDUSeqNum == 0 || tmo.ErrorPDUSeqNum != pb.SequenceNumber)   // новая ошибка
                            {
                                // Throttling error, SMSC message queue full error, Unknown error, Timeout error, License error
                                if (pb.StatusCode == PacketBase.commandStatusEnum.ESME_RTHROTTLED ||
                                    pb.StatusCode == PacketBase.commandStatusEnum.ESME_RMSGQFUL ||
                                    pb.StatusCode == PacketBase.commandStatusEnum.ESME_RUNKNOWNERR ||
                                    pb.StatusCode == PacketBase.commandStatusEnum.ESME_RTIMEOUT ||
                                    pb.StatusCode == PacketBase.commandStatusEnum.ESME_R_LIFE_LICENSE_ERR)
                                {
                                    tmo.ErrorPDU_TO = new TimeSpan(0, 0, this.SMPPSettings.ErrorPDU_TO_2);
                                }
                                // Установить флаги ошибки
                                tmo.ErrorPDUSeqNum = pb.SequenceNumber;
                                tmo.ErrorResp_Time = dt;
                                tmo.ErrorRetryCount = 0;
                            }
                            else                                                                        // повторение ошибки
                            {
                                // Обновить флаги ошибки
                                tmo.ErrorResp_Time = dt;
                            }
                        }
                    }
                }
                if (pduDirect == PDUTimeouts.PDUDirection.Out)
                {
                    // зарегистрировать отправку исходящего PDU
                    if (pb.CommandId == PacketBase.commandIdEnum.bind_receiver ||
                        pb.CommandId == PacketBase.commandIdEnum.bind_transceiver ||
                        pb.CommandId == PacketBase.commandIdEnum.bind_transmitter ||
                        pb.CommandId == PacketBase.commandIdEnum.data_sm ||
                        pb.CommandId == PacketBase.commandIdEnum.enquire_link ||
                        pb.CommandId == PacketBase.commandIdEnum.submit_sm ||
                        pb.CommandId == PacketBase.commandIdEnum.unbind)
                    {
                        tmo.NeedResp = true;
                        if (pb.CommandId == PacketBase.commandIdEnum.enquire_link ||
                            pb.CommandId == PacketBase.commandIdEnum.submit_sm ||
                            pb.CommandId == PacketBase.commandIdEnum.data_sm)
                        {
                            tmo.DataOutPDU_Time = dt;
                        }
                    }
                }
            }
        }
    }
}
