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
using System.Text;
using System.Runtime.Serialization;
using Cryptany.Core.Base;

namespace Cryptany.Core
{
	/// <summary>
	/// входящее сообщение
	/// </summary>
	[Serializable]
	public class Message : ISerializable
	{
		private string _MSISDN;
		private Guid _SMSCId;
		private string _serviceNumber;


		private string _HTTP_Category;
		private string _HTTP_Operator;
		private string _HTTP_Protocol;
		private string _HTTP_UID;
		private Guid _InboxId;
		private string _messageText;
		private DateTime _MsgTime;

		private string _TransactionID;
		private string _USSD_PhoneModel;
		private uint? _USSD_UserMessageReference;
	    
		//private IService _recipientService;
		private MessageType _type = MessageType.Unknown;

		/// <summary>
		/// Конструктор по-умолчанию. Нужен для корректной десериализации.
		/// </summary>
		public Message()
		{
		}

		public Message(SerializationInfo info, StreamingContext context)
		{
			_MSISDN = info.GetString("_MSISDN");

			_serviceNumber = info.GetString("_serviceNumber");

			_SMSCId = (Guid)info.GetValue("_SMSCId", typeof(Guid));

			_HTTP_Category = info.GetString("_HTTP_Category");
			_HTTP_Operator = info.GetString("_HTTP_Operator");
			_HTTP_Protocol = info.GetString("_HTTP_Protocol");
			_HTTP_UID = info.GetString("_HTTP_UID");
			_InboxId = (Guid)info.GetValue("_InboxId", typeof(Guid));
			
			_messageText = info.GetString("_messageText");
			_MsgTime = info.GetDateTime("_MsgTime");
			_TransactionID = info.GetString("_TransactionID");
			_USSD_PhoneModel = info.GetString("_USSD_PhoneModel");
			_USSD_UserMessageReference = (uint?)info.GetValue("_USSD_UserMessageReference", typeof(uint?));
			_type = (MessageType)info.GetValue("_type", typeof(MessageType));
		}

        /// <summary>
        /// Основной конструктор
        /// </summary>
        /// <param name="msisdn">MSISDN</param>
        /// <param name="smscId">id коннектора, от которого получено сообщение</param>
        /// <param name="serviceNumber">полный текст сервисного номера (as is)</param>
        /// <param name="text">полный текст сообщения</param>
        /// <param name="isFake">получено ли физически?</param>
        public Message(Guid id, string msisdn, Guid smscId, string serviceNumber, string transactionID, string text)
        {
            _InboxId = id;
            _MsgTime = DateTime.Now;
            _MSISDN = msisdn;
            _SMSCId = smscId;
            _serviceNumber = serviceNumber;
            _TransactionID = transactionID;
            _messageText = (text ?? "");
            

        }


        
        /// <summary>
        /// Конструктор для HTTP-коннекторов
        /// </summary>
        /// <param name="msisdn">MSISDN</param>
        /// <param name="smscId">id коннектора, от которого получено сообщение</param>
        /// <param name="serviceNumber">полный текст сервисного номера (as is)</param>
        /// <param name="text">полный текст сообщения</param>
        /// <param name="isFake">получено ли физически?</param>
        /// <param name="category">идентификатор сотовой сети</param>
        /// <param name="uid">идентификатор запроса, назначенный оператором</param>
        /// <param name="protocol">идентификатор протокола</param>
        /// <param name="Operator">идентификатор оператора</param>
        public Message(Guid id, string msisdn, Guid smscId, string serviceNumber, string text, string category,
                       string uid, string protocol, string Operator)
        {
            _InboxId = id;
            _MsgTime = DateTime.Now;
			_MSISDN = msisdn;
			_SMSCId = smscId;
			_serviceNumber = serviceNumber;
            
            _messageText = (text ?? "");
            
            _HTTP_Category = category;
            _HTTP_UID = (uid ?? "");
            _HTTP_Protocol = protocol;
            _HTTP_Operator = Operator;

		}


        public Message(Guid id, string MSISDN, string sn, Guid smsc, string text, uint userMessageReference,
                       string phoneModel)
        {
            _InboxId = id;
            _MsgTime = DateTime.Now;
            _MSISDN = MSISDN;
            _SMSCId = smsc;
            _serviceNumber = sn;
            _messageText = (text ?? "");
            
            _MsgTime = DateTime.Now;
            
            _TransactionID = "";
            _USSD_UserMessageReference = userMessageReference;
            _USSD_PhoneModel = phoneModel;
        }


		/// <summary>
		/// ID транзакции в рамках которой получено сообщение
		/// </summary>
		public string TransactionID
		{
			get
			{
				return _TransactionID;
			}
		}

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
			get
			{
				return _InboxId;
			}
		}

	

		/// <summary>
		/// Get original message text
		/// </summary>
		public string Text
		{
			get
			{
				return _messageText;
			}
		}

		


		public uint? USSD_UserMessageReference
		{
			get
			{
				return _USSD_UserMessageReference;
			}
			set
			{
				_USSD_UserMessageReference = value;
			}
		}

		public string USSD_PhoneModel
		{
			get
			{
				return _USSD_PhoneModel;
			}
			set
			{
				_USSD_PhoneModel = value;
			}
		}

		public string HTTP_Category
		{
			get
			{
				return _HTTP_Category;
			}
			set
			{
				_HTTP_Category = value;
			}
		}

		public string HTTP_UID
		{
			get
			{
				return _HTTP_UID;
			}
			set
			{
				_HTTP_UID = value;
			}
		}

		public string HTTP_Protocol
		{
			get
			{
				return _HTTP_Protocol;
			}
			set
			{
				_HTTP_Protocol = value;
			}
		}

		public string HTTP_Operator
		{
			get
			{
				return _HTTP_Operator;
			}
			set
			{
				_HTTP_Operator = value;
			}
		}

		/// <summary>
		/// Свойство, предназначенное для сериализации/десериализации сообщения
		/// </summary>
		public MsgSerializationInfo SerializationInfo
		{
			get
			{
				MsgSerializationInfo result = new MsgSerializationInfo();
				result.MSISDN = _MSISDN;
				result.ServiceNumber = _serviceNumber;
				result.SMSCId = _SMSCId;
				result._Text = _messageText;
				result._ID = _InboxId;
				result._MsgTime = _MsgTime;
				
				result._TransactionID = _TransactionID;
				result._USSD_UserMessageReference = _USSD_UserMessageReference;
				result._USSD_PhoneModel = _USSD_PhoneModel;
				result._HTTP_Category = _HTTP_Category;
				result._HTTP_UID = _HTTP_UID;
				result._HTTP_Protocol = _HTTP_Protocol;
				result._HTTP_Operator = _HTTP_Operator;
				return result;
			}

			set
			{
				_MSISDN = value.MSISDN;
				_serviceNumber = value.ServiceNumber;
				_SMSCId = value.SMSCId;
				_messageText = value._Text;
				_InboxId = value._ID;
				_MsgTime = value._MsgTime;
				
				_TransactionID = value._TransactionID;
				_USSD_UserMessageReference = value._USSD_UserMessageReference;
				_USSD_PhoneModel = value._USSD_PhoneModel;
				_HTTP_Category = value._HTTP_Category;
				_HTTP_UID = value._HTTP_UID;
				_HTTP_Protocol = value._HTTP_Protocol;
				_HTTP_Operator = value._HTTP_Operator;
			}
		}

		public MessageType Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		public string MSISDN
		{
			get
			{
				return _MSISDN;
			}
		}

		public Guid SMSCId
		{
			get
			{
				return _SMSCId;
			}
		}

		public string ServiceNumberString
		{
			get
			{
				return _serviceNumber;
			}
		}

        public static string GetMSISDN(string adress)
        {
            string serviceNumber = "";
            int snIdPos = adress.IndexOf('#');
            serviceNumber = snIdPos > 0 ? adress.Substring(0, snIdPos) : adress;
            return serviceNumber;
        }
		public static string GetServiceNumber(string dest)
		{
			string serviceNumber = "";
			int snIdPos = dest.IndexOf('#');
			serviceNumber = snIdPos > 0 ? dest.Substring(0, snIdPos) : dest;
			return serviceNumber;
		}

		public static string GetTransactionId(string dest)
		{
			string transactionId = "";
			int transactionIdPos = dest.IndexOf('#') + 1;
			transactionId = transactionIdPos > 0 ? dest.Substring(transactionIdPos) : "";
			return transactionId;
		}

		/// <summary>
		/// Возвращает описание сообщения
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			string strText = Text ?? "";
			string str_InboxId = (_InboxId != Guid.Empty) ? _InboxId.ToString() : "";
			string strHttpUid = HTTP_UID ?? "";
			sb.AppendFormat("Message properties:\r\nText: {0}\r\nID: {1}\r\nSource: {2}\r\nDest: {3}\r\nHTTP_UID: {4}",
                            new object[] { strText, str_InboxId, MSISDN, ServiceNumberString, strHttpUid });
			return sb.ToString();
		}

		#region Nested type: MsgSerializationInfo

		/// <summary>
		/// Структура предназначенная для сериализации/десериализации сообщения
		/// </summary>
		public struct MsgSerializationInfo
		{
			public string MSISDN;
			public string ServiceNumber;
			public Guid SMSCId;
			public string _HTTP_Category;
			public string _HTTP_Operator;
			public string _HTTP_Protocol;
			public string _HTTP_UID;
			public Guid _ID;
			public DateTime _MsgTime;
			public int _SMSCount;
			public string _Text;
			public string _TransactionID;
			public string _USSD_PhoneModel;
			public uint? _USSD_UserMessageReference;
		}

		#endregion

		#region ISerializable Members

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_MSISDN", _MSISDN);
			info.AddValue("_serviceNumber", _serviceNumber);
			info.AddValue("_SMSCId", _SMSCId);

			info.AddValue("_HTTP_Category", _HTTP_Category);
			info.AddValue("_HTTP_Operator", _HTTP_Operator);
			info.AddValue("_HTTP_Protocol", _HTTP_Protocol);
			info.AddValue("_HTTP_UID", _HTTP_UID);
			info.AddValue("_InboxId", _InboxId);
			
			info.AddValue("_messageText", _messageText);
			info.AddValue("_MsgTime", _MsgTime);
			info.AddValue("_TransactionID", _TransactionID);
			info.AddValue("_USSD_PhoneModel", _USSD_PhoneModel);
			info.AddValue("_USSD_UserMessageReference", _USSD_UserMessageReference);
			info.AddValue("_type", _type);
		}

		#endregion
	}
}
