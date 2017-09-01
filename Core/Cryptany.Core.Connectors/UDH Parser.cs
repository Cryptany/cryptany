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
using System.Text;
using Cryptany.Core.SmppLib;
using Cryptany.Core.Management;
using Cryptany.Core;


namespace Cryptany.Core.Connectors
{
   public static class UDHParser
    {


       public static bool Parse(MessagePacketBase msg)
       {

           int udhLen;

           byte[] MessageText;

           if (msg.ShortMessageLength > 0)//текст сообщения и udh заголовок в short_message
           {
               MessageText = msg.MessageText;
               udhLen = MessageText[0];
               //отбросим лишнее
               byte[] user_Data = new byte[MessageText.Length - udhLen - 1];
               Array.Copy(MessageText, udhLen + 1, user_Data, 0,
                       MessageText.Length - udhLen - 1);
               msg.MessageText = user_Data;
               msg.ShortMessageLength = user_Data.Length;
           }

           else
           {

               int idx_payload = msg.OptionalParamList.FindIndex(SMPPMessageParts.predicate_message_payload);

               if (idx_payload > -1)//текст сообщения и udh заголовок в message_payload
               {
                   MessageText = msg.OptionalParamList[idx_payload].Value;
                   udhLen = MessageText[0];

                   byte[] user_Data = new byte[MessageText.Length - udhLen - 1];
                   Array.Copy(MessageText, udhLen + 1, user_Data, 0,
                           MessageText.Length - udhLen - 1);
                   msg.OptionalParamList[idx_payload].Value = user_Data;

  
               }
               else
               {
                   ServiceManager.LogEvent("В udh сообщении на найден текст сообщения", EventType.Error, EventSeverity.High);

                   return false;
               }
            }
          

           int idx= 1;
           

           while (idx <= udhLen)
           {
               byte IEid = MessageText[idx];
               int IELen = MessageText[idx +1];

               if (IEid == 0x00 || IEid==0x08)  //concatenated message - создадим SAR параметры
                   {


                       //referenceId
                       if (msg.OptionalParamList.FindIndex(SMPPMessageParts.predicate_sar_refnum) ==
                                              -1)
                       {
                           OptionalParameter op_sar_msg_ref_num = new OptionalParameter();
                       
                           op_sar_msg_ref_num.Param = OptionalParameter.tagEnum.sar_msg_ref_num;
                           op_sar_msg_ref_num.Value = IEid == 0x00 ? new byte[] { 0x00, MessageText[idx + 2] } : new byte[] {MessageText[idx + 2],MessageText[idx+3]};
                           
                           msg.OptionalParamList.Add(op_sar_msg_ref_num);
                       }
                  
                       //total segments
                       if (msg.OptionalParamList.FindIndex(SMPPMessageParts.predicate_sar_total) == -1)
                       {
                           OptionalParameter op_sar_total_segments = new OptionalParameter();
                       
                           op_sar_total_segments.Param = OptionalParameter.tagEnum.sar_total_segments;
                           op_sar_total_segments.Value = IEid == 0x00 ? new byte[] { MessageText[idx + 3] } : new byte[] { MessageText[idx + 4] };
                           msg.OptionalParamList.Add(op_sar_total_segments);
                       }
                 

                       //part number
                       if (msg.OptionalParamList.FindIndex(SMPPMessageParts.predicate_sar_seqnum) ==
                           -1)
                       {
                           OptionalParameter op_sar_segment_seqnum = new OptionalParameter();
                    
                           op_sar_segment_seqnum.Param = OptionalParameter.tagEnum.sar_segment_seqnum;
                           op_sar_segment_seqnum.Value = IEid == 0x00 ? new byte[] { MessageText[idx + 4] } : new byte[] { MessageText[idx + 5] };
                           msg.OptionalParamList.Add(op_sar_segment_seqnum);
                       }
                    


                   }

                   //else   if (IEid == 0x0a)  //Text formatting
                   //{

                   //    Array.Copy(MessageText, udhLen + 1, msg.MessageText, 0, MessageText.Length - udhLen - 1);
                   //    msg.ShortMessageLength = msg.MessageText.Length;

                   //}

               idx = idx + IELen + 2;  //перейти к следующему IE


           }

    

           return true;

       }




    }
}
