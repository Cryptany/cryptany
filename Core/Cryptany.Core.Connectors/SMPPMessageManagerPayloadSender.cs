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
using Cryptany.Core.SmppLib;
using Cryptany.Core.Management;


namespace Cryptany.Core
{
    /// <summary>
    /// не дробит на части длинные смс, весь текст помещает в message_payload
    /// </summary>
   public class SMPPMessageManagerPayloadSender:SMPPMessageManager
    {



       public SMPPMessageManagerPayloadSender(ConnectorSettings cs, ILogger logger)
                : base(cs, logger)
            { }


       protected override bool SendOutMessage()
       {


           if (SendMessage == null) return true;
           if (SendMessage.Content == null) return true;

           PDUTypesForUserData PDUType = SMPPSettings.Send_by_DATA_SM ? PDUTypesForUserData.DATA_SM : PDUTypesForUserData.SUBMIT_SM;
           bool sendOk = false;

           int max_message_length;
           PacketBase.dataCodingEnum dataCoding;

           try
           {
               byte[] userData = SendMessage.Content.Body;
               if (userData == null) return true;
               if (SendMessage.IsFlash)//флеш-смс
               {
                   dataCoding = (PacketBase.dataCodingEnum) Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_Flash);
   
                   max_message_length = SMPPSettings.Max_message_length_ru;
               }
               else if (((TextContent)SendMessage.Content).isUnicode)//юникод
               {
                   dataCoding = (PacketBase.dataCodingEnum) Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_unicode);
  
                  max_message_length = SMPPSettings.Max_message_length_ru;
               }
               else//все остальное
               {
                   dataCoding =
                       (PacketBase.dataCodingEnum)
                       Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_default);
                   // Инициализация максимального размера короткого сообщения
                   //shortMessageLength = SMPPSettings.Short_message_length_eng;
                   //// Инициализация SAR параметров
                   //sar_segment_length = SMPPSettings.Sar_segment_length_eng;
                   max_message_length = SMPPSettings.Max_message_length_eng;
               }

               switch (PDUType)
               {
                   case PDUTypesForUserData.SUBMIT_SM:
                       if (userData.Length > 0 && userData.Length <= max_message_length)
                       {
                           SendMessage.PartsCount = 1;
                           SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
                           sm.ShortMessageLength = 0;
                          

                           OptionalParameter op = new OptionalParameter();
                           op.Param = OptionalParameter.tagEnum.message_payload; // все сообщение помещаем в Payload, не дробим на части
                           op.Value = userData;
                           sm.OptionalParamList.Add(op);
                           sm.MessageText =new byte[0];
                           sm.PartNumber = 1;

                           if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                           {
                               SetOperatorParameters(sm);
                           }

                           sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);

                       }
        
                       else if (userData.Length > max_message_length)
                       {

                           Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                           ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);

                           SendMessage = null;
                           ReadyToSendMessages.Set();
                       }
                       else
                       {
                           Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!",
                                              LogSeverity.Debug));
                           ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ".  Message length = 0!", EventType.Error, EventSeverity.High);
                           SendMessage = null;
                           ReadyToSendMessages.Set();
                       }
                       break;
                   case PDUTypesForUserData.DATA_SM:
                       if (userData.Length > 0 && userData.Length <= max_message_length)
                       {
                           DataSM dm = Create_DATA_SM(SendMessage, dataCoding);
                           OptionalParameter op_data = new OptionalParameter();
                           op_data.Param = OptionalParameter.tagEnum.message_payload;
                           op_data.Value = userData;
                           dm.OptionalParamList.Add(op_data);
                           dm.PartNumber = 1;
                           sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm);

                       }
       
                       else if (userData.Length > max_message_length)
                       {
                           // Log the System error 
                           if (Logger != null)
                           {
                               Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));

                           }
                           ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);

                           SendMessage = null;
                           ReadyToSendMessages.Set();
                       }
                       else
                       {
                           Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!", LogSeverity.Debug));
                           ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ".  Message length = 0!!", EventType.Error, EventSeverity.High);
                           SendMessage = null;
                           ReadyToSendMessages.Set();
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
           return sendOk;
       }

    }
}
