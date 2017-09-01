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
using System.Runtime.Serialization;
using Cryptany.Core.Clubs2;

namespace Cryptany.Core.Router.Data
{
    [Serializable]
    public class EmptyMessage
    {
    }
    

    [Serializable]
    public class Message //: ISerializable
    {
        private string _MSISDN;
        private Guid _SMSCId;
        private string _serviceNumber;
        private Guid _InboxId;
        private string _messageText;
        private DateTime _MsgTime;
        private string _TransactionID;

        #region Constructors
        /// <summary>
        /// Конструктор по-умолчанию. Нужен для корректной десериализации.
        /// </summary>
        public Message()
        {
        }

        public Message(Cryptany.Core.Message msg)
            :this(msg.InboxId, msg.MSISDN, msg.SMSCId, msg.ServiceNumberString, msg.Text, msg.TransactionID)
        { }

        //public Message(SerializationInfo info, StreamingContext context)
        //{
        //    _MSISDN = info.GetString("_MSISDN");
        //    _serviceNumber = info.GetString("_serviceNumber");
        //    _SMSCId = (Guid)info.GetValue("_SMSCId", typeof(Guid));
        //    _InboxId = (Guid)info.GetValue("_InboxId", typeof(Guid));
        //    _messageText = info.GetString("_messageText");
        //    _MsgTime = info.GetDateTime("_MsgTime");
        //}

        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="msisdn">MSISDN</param>
        /// <param name="smscId">id коннектора, от которого получено сообщение</param>
        /// <param name="serviceNumber">полный текст сервисного номера (as is)</param>
        /// <param name="text">полный текст сообщения</param>
        /// <param name="isFake">получено ли физически?</param>
        public Message(Guid id, string msisdn, Guid smscId, string serviceNumber, string text, string transactionId)
        {
            _InboxId = id;
            _MsgTime = DateTime.Now;
            _MSISDN = msisdn;
            _SMSCId = smscId;
            _serviceNumber = serviceNumber;
            _messageText = (text ?? "");
            _TransactionID = transactionId;
        }

        #endregion
        #region Fields
        /// <summary>
        /// Время получения сообщения
        /// </summary>
        public DateTime MessageTime
        {
            get
            {
                return _MsgTime;
            }
            set
            {
                _MsgTime = value;
            }
        }

        /// <summary>
        /// Get ID of message in Inbox datatable
        /// </summary>
        public Guid InboxId
        {
            get { return _InboxId; }
        }

        /// <summary>
        /// Get original message text
        /// </summary>
        public string Text
        {
            get { return _messageText; }
            set { _messageText = value; }
        }

        public string MSISDN
        {
            get { return _MSISDN; }
        }

        public Guid SMSCId
        {
            get { return _SMSCId; }
        }

        public string ServiceNumberString
        {
            get { return _serviceNumber; }
        }

        public string TransactionID
        {
            get
            {
                return _TransactionID;
            }
        }

        public Guid OperatorId
        {
            get;
            set;
        }

        public Guid OperatorBrandId
        {
            get;
            set;
        }

        public Guid RegionId
        {
            get;
            set;
        }

        public Guid MacroRegionId
        {
            get;
            set;
        }

        public Guid AbonentId
        {
            get;
            set;
        }

        public Guid[] Subscriptions
        {
            get;
            set;
        }
        #endregion

        /// <summary>
        /// Возвращает описание сообщения
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
         /*   StringBuilder sb = new StringBuilder();
            string strText = Text ?? "";
            string str_InboxId = (_InboxId != Guid.Empty) ? _InboxId.ToString() : "";
            //string strAbonent_MSISDN = (Abonent != null && Abonent.MSISDN != null) ? Abonent.MSISDN : "";
            //string strSN_Value = (SN != null && SN.Number != null) ? SN.Number : "";
            //string strTariffID = (TariffID != Guid.Empty) ? TariffID.ToString() : "";
            string strHttpUid = HTTP_UID ?? "";
            sb.AppendFormat("Message properties:\r\nText: {0}\r\nID: {1}\r\nSource: {2}\r\nDest: {3}\r\nHTTP_UID: {4}",
                            new object[] { strText, str_InboxId, MSISDN, ServiceNumberString, strHttpUid });
            return sb.ToString();*/
            //return "[ from " + _MSISDN + "; to " + _serviceNumber + "text " + _messageText + "]";
            return "[ " + _MSISDN + " -> " + ServiceNumberString + "; \"" + _messageText + "\" ]";
        }

        //public void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    info.AddValue("_MSISDN", _MSISDN);
        //    info.AddValue("_serviceNumber", _serviceNumber);
        //    info.AddValue("_SMSCId", _SMSCId);
        //    info.AddValue("_InboxId", _InboxId);
        //    info.AddValue("_messageText", _messageText);
        //    info.AddValue("_MsgTime", _MsgTime);
        //}

    }
}
