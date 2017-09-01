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
using System.Runtime.Serialization;
using Cryptany.Core.Interaction;

namespace Cryptany.Core
{
    [Serializable]
    public class MessageStatus : ISerializable
    {/// <summary>
        /// OutboxID
        /// </summary>
        public Guid ID;
        /// <summary>
        /// Статус доставки
        /// </summary>
        public MessageDeliveryStatus Status;
        /// <summary>
        /// Описание статуса
        /// </summary>
        public string StatusDescription;
        /// <summary>
        /// Время изменения статуса
        /// </summary>
        public DateTime StatusTime;

        /// <summary>
        /// Тип сообщения - обычная, флеш, тихая и т.д. 
        /// </summary>
        public MessageType MessageType;
        /// <summary>
        /// Id, выданный на смс-центре
        /// </summary>
        public string SMSCMsgId;


        public MessageStatus()
        {


        }



        public MessageStatus(SerializationInfo info, StreamingContext context)
        {
            ID = (Guid)info.GetValue("_ID", typeof(Guid));
            Status =(MessageDeliveryStatus) info.GetValue("_Status",typeof (MessageDeliveryStatus));
            StatusDescription = info.GetString("_StatusDescription");
            StatusTime = info.GetDateTime("_StatusTime");
            SMSCMsgId = info.GetString("_SMSCMsgId");

            MessageType = (MessageType)info.GetValue("_type",typeof (MessageType));
        }



        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_ID", ID);

            info.AddValue("_Status", Status);

            info.AddValue("_StatusDescription", StatusDescription);
            info.AddValue("_StatusTime", StatusTime);
            info.AddValue("_SMSCMsgId", SMSCMsgId);
            info.AddValue("_type", MessageType);

        }


    }
}
