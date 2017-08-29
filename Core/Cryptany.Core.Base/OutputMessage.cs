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
using System.Runtime.Serialization;

namespace Cryptany.Core.Base
{
	[Serializable]
	public class OutputMessage : ISerializable
	{
		private Guid _ID;
		private Guid _projectID;
		private string _Source;
		private string _Destination;
		private Guid _InboxMsgID;
		private string _TransactionId;


        /// <summary>
        /// для Ussd-связь с входящим запросом
        /// </summary>
		private Nullable<uint> _USSD_UserMessageReference = 0;
        /// <summary>
        ///  Признак платности ussd-сообщения
        /// </summary>
		private Nullable<byte> _USSD_Charging;
        /// <summary>
        /// тарифная категория
        /// </summary>
		private string _USSD_MessageContentType;
        /// <summary>
        /// директива на выход из портала
        /// </summary>
		private Nullable<byte> _USSD_DialogDirective;
        //не используется 
		private string _HTTP_UID;
		private string _HTTP_Protocol;
		private string _HTTP_Operator;
        private string _HTTP_Category;
		private string _HTTP_DeliveryStatus;
		//private bool _isBalanced;

        [NonSerialized]
		public Guid TariffId = Guid.Empty;

		private ushort _partsCount;
		private Content _Content;
		private MessagePriority _priority;
	    private Guid _smscId = Guid.Empty;
	    private DateTime _ttl;
	    private DateTime _timeReceived;
        private bool _isFlash = false;
        private bool notifyMessageState;

        private MessageType _type=MessageType.Standard;

		public OutputMessage()
		{
			_priority = MessagePriority.Normal;
		    TTL = DateTime.MaxValue;
            _type = MessageType.Standard;
		}

		/// <summary>
		/// ISerializable interface requirement
		/// </summary>
		public OutputMessage(SerializationInfo info, StreamingContext context)
		{
			_ID = (Guid)info.GetValue("_ID", typeof(Guid));
			//_channelID = (Guid)info.GetValue("_channelID", typeof(Guid));
			_projectID = (Guid)info.GetValue("_projectID", typeof(Guid));
			//_tariffId = (Guid)info.GetValue("_tariffId", typeof(Guid));
			_Source = info.GetString("_Source");
			_Destination = info.GetString("_Destination");
			_Content = info.GetValue("_Content", typeof(Content)) as Content;
			_priority = (MessagePriority)info.GetValue("_priority", typeof(MessagePriority));
			_TransactionId = info.GetString("_TransactionId");
			_InboxMsgID = (Guid)info.GetValue("_InboxMsgID", typeof(Guid));
			//_isBalanced = info.GetBoolean("_isBalanced");
            
			_partsCount = info.GetUInt16("_partsCount");
			_HTTP_DeliveryStatus = info.GetString("_HTTP_DeliveryStatus");
			_HTTP_Category = info.GetString("_HTTP_Category");
			_HTTP_Operator = info.GetString("_HTTP_Operator");
			_HTTP_Protocol = info.GetString("_HTTP_Protocol");
			_HTTP_UID = info.GetString("_HTTP_UID");
			_USSD_UserMessageReference = (uint?)info.GetValue("_USSD_UserMessageReference", typeof(uint?));
			_USSD_Charging = (byte?)info.GetValue("_USSD_Charging", typeof(byte?));
			//_USSD_MessageContentType = info.GetString("_USSD_MessageContentType");
			_USSD_DialogDirective = (byte?)info.GetValue("_USSD_DialogDirective", typeof(byte?));

            
       
			//_deliveryStatus = (MessageDeliveryStatus)info.GetValue("_deliveryStatus", typeof(MessageDeliveryStatus));
            try
            {
                _smscId = (Guid) info.GetValue("_smscId", typeof (Guid));
                _IsPayed = info.GetBoolean("_IsPayed");
            }
            catch
            {}

		    // TODO: доработать алгоритм времени жизни сообщения
            //TTL = DateTime.MaxValue;
            try
            {
                TTL = info.GetDateTime("TTL");
            }
            catch
            { }

            try
            {
                IsFlash = info.GetBoolean("IsFlash");
            }
            catch
            { }

            try
            {
                NotifyMessageState = info.GetBoolean("notifyMessageState");
            }
            catch
            { }

      
            try
            {
                _type = (MessageType)info.GetValue("_type", typeof(MessageType));
               


            }
            catch {  }
           
            
          
            
		}

		/// <summary>
		/// ID в базе (до выполнения AddToOutbox в базе отсутствует)
		/// </summary>
        public Guid ID
        {
            get { return _ID; }
            set { _ID = value; }
        }


		/// <summary>
		/// Контент для отправки
		/// </summary>
        public Content Content
        {
            get { return _Content; }
            set { _Content = value; }
        }

		/// <summary>
        /// ID канала, 
        /// </summary>
        //public Guid ChannelID
        //{
        //    get
        //    {
        //        return _channelID;
        //    }
        //    set
        //    {
        //        _channelID = value;
        //    }
        //}

		/// <summary>
        /// ID контрагентского ресурса
        /// </summary>
        public Guid ProjectID
        {
            get
            {
                return _projectID;
            }
            set
            {
                _projectID = value;
            }
        }

		public MessagePriority Priority
		{
			get
			{
				return _priority;
			}
			set
			{
				_priority = value;
			}
		}

		/// <summary>
		/// Откуда шлём
		/// </summary>
        public string Source
        {
            get { return _Source; }
            set { _Source = value; }
        }

		/// <summary>
		/// Куда шлём
		/// </summary>
        public string Destination
        {
            get { return _Destination; }
            set { _Destination = value; }
        }

		/// <summary>
		/// ID соответствующего входящего сообщения
		/// </summary>
        public Guid InboxMsgID
        {
            get { return _InboxMsgID; }
            set { _InboxMsgID = value; }
        }

		/// <summary>
		/// Идентификатор транзакции в пределах которой передаётся сообщение 
		/// (та фигня которая стоит в Destination после #)
		/// </summary>
		public string TransactionId
		{
			get { return _TransactionId; }
			set { _TransactionId = value; }
		}

        private bool _IsPayed;

        /// <summary>
        /// Признак платности сообщения (имеет смысл только для USSD и Kievstar сообщений)
        /// </summary>
        public bool IsPayed
        {
            get { return _IsPayed; }
            set { _IsPayed = value; }
        }

        /// <summary>
        /// SMPP user_message_reference (имеет смысл только для USSD сообщений)
        /// </summary>
        public Nullable<uint> USSD_UserMessageReference
        {
            get { return _USSD_UserMessageReference; }
            set { _USSD_UserMessageReference = value; }
        }

        /// <summary>
        /// SMPP USSD charging (имеет смысл только для USSD сообщений)
        /// </summary>
        public Nullable<byte> USSD_Charging
        {
            get { return _USSD_Charging; }
            set { _USSD_Charging = value; }
        }

        /// <summary>
        /// SMPP USSD message content type (имеет смысл только для USSD сообщений)
        /// </summary>
        public string USSD_MessageContentType
        {
            get { return _USSD_MessageContentType; }
            set { _USSD_MessageContentType = value; }
        }
 
        /// <summary>
        /// SMPP USSD dialog directive (имеет смысл только для USSD сообщений)
        /// </summary>
        public Nullable<byte> USSD_DialogDirective
        {
            get { return _USSD_DialogDirective; }
            set { _USSD_DialogDirective = value; }
        }

        /// <summary>
        /// содержит Operator Parameters !!! 
        /// </summary>
        public string HTTP_Category
        {
            get { return _HTTP_Category; }
            set { _HTTP_Category = value; }
        }

        /// <summary>
        /// HTTP UID (имеет смысл только для HTTP сообщений)
        /// </summary>
        public string HTTP_UID
        {
            get { return _HTTP_UID; }
            set { _HTTP_UID = value; }
        }


        /// <summary>
        /// HTTP Protocol (имеет смысл только для HTTP сообщений)
        /// </summary>
        public string HTTP_Protocol
        {
            get { return _HTTP_Protocol; }
            set { _HTTP_Protocol = value; }
        }

        /// <summary>
        /// HTTP Protocol (имеет смысл только для HTTP сообщений)
        /// </summary>
        public string HTTP_Operator
        {
            get { return _HTTP_Operator; }
            set { _HTTP_Operator = value; }
        }


        /// <summary>
        /// HTTP Delivery status (имеет смысл только для HTTP сообщений)
        /// </summary>
        public string HTTP_DeliveryStatus
        {
            get { return _HTTP_DeliveryStatus; }
            set { _HTTP_DeliveryStatus = value; }
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

        ///// <summary>
        ///// Необходимо ли маршрутизировать сообщение через балансирующий коннектор
        ///// </summary>
        //public bool IsBalanced
        //{
        //    get { return _isBalanced; }
        //    set { _isBalanced = value; }
        //}


		/// <summary>
		/// Статус доставки (по умолчанию - Unknown)
		/// </summary>
        //public MessageDeliveryStatus DeliveryStatus
        //{
        //    get
        //    {
        //        return _deliveryStatus;
        //    }
        //    set
        //    {
        //        _deliveryStatus = value;
        //    }
        //}

        /// <summary>
        /// Возвращает тип сообщения.
        /// </summary>
        //public AnswerMessageType MsgType
        //{
        //    get 
        //    {
        //        if (_Content is TextContent)
        //        {
        //            return AnswerMessageType.Text;
        //        }
        //        //else if (_Content is WapPushContent)
        //        //{
        //        //    return AnswerMessageType.WapPush;
        //        //}
        //        return AnswerMessageType.Content;
                
        //    }
        //}

        /// <summary>
        /// Возвращает в зависимости от отсылаемого контента бинарные данные, 
        /// текстовую строку или URL отсылаемый через WAP push.
        /// </summary>
        public object TypedBody
        {
            get
            {
                if (_Content is TextContent)
                {
                    TextContent tmp = _Content as TextContent;
                    return tmp.MsgText;
                }
                //else if (_Content is WapPushContent)
                //{
                //    WapPushContent tmp = _Content as WapPushContent;
                //    return tmp.URL;
                //}
                //else
                //{
                    return _Content.Body;
                //}
            }
        }
        
        //public Guid TariffId
        //{
        //    get { return _tariffId; }
        //    set { _tariffId = value; }
        //}

        public ushort PartsCount
        {
            get { return _partsCount; }
            set { _partsCount = value; }
        }


		///// <summary>
		///// Создает объект логгирования OutboxEntry и помещает его в очередь MSMQ логгера
		///// </summary>
		//public void AddToOutbox()
		//{
		//    try
		//    {
		//        OutboxEntry outboxEntry = new OutboxEntry(this.ID, TypedBody, MsgType, InboxMsgID);
		//        MessageQueue MSMQLoggerInputQueue = ServicesConfigurationManager.GetMSMQLoggerInputQueue();
		//        MSMQLoggerInputQueue.Send(outboxEntry);
		//    }
		//    catch (Exception e)
		//    {
		//        ILogger logger = Logging.LoggerFactory.Logger;
		//        logger.DefaultSource = "OutputMessage";
		//        if (logger != null)
		//        {
		//            logger.Write(new LogMessage("Exception in OutputMessage AddToOutbox method: " + e.ToString(), LogSeverity.Error));
		//        }
		//    }
		//}

        //public static Guid GetTariffid(Guid smscID, string serviceNumber)
        //{
        //    try
        //    {
        //        CacheDataSet.ServiceNumbersRow[] servnums =
        //            (CacheDataSet.ServiceNumbersRow[])
        //            CoreClassFactory.CreateConfigProvider().CacheDS.ServiceNumbers.Select("SN='" + serviceNumber + "'");
        //        if (servnums.Length > 0)
        //        {
        //            CacheDataSet.TariffSNOpRow[] tariffs =
        //                (CacheDataSet.TariffSNOpRow[])
        //                CoreClassFactory.CreateConfigProvider().CacheDS.TariffSNOp.Select("SMSCId = '" +
        //                                                                                  smscID.ToString() +
        //                                                                                  "' AND ServiceNumberId = '" +
        //                                                                                  servnums[0].Id.ToString() +
        //                                                                                  "' AND ISNULL(IsInbound,0)=0");
        //            if (tariffs.Length > 0)
        //            {
        //                return tariffs[0].TariffId;
        //            }
        //        }
        //    }
        //    catch
        //    {
                
        //    }
        //    return Guid.Empty;
        //}
        public static string GetServiceNumber(string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                throw new ArgumentNullException("src");
            }
            string serviceNumber = "";
            int snIdPos = src.IndexOf('#');
            if (snIdPos > 0)
                serviceNumber = src.Substring(0, snIdPos);
            else
                serviceNumber = src;
            return serviceNumber;
        }

        public static string GetTransactionId(string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                throw new ArgumentNullException("src");
            }
            string transactionId = "";
            int transactionIdPos = src.IndexOf('#') + 1;
            if (transactionIdPos > 0)
                transactionId = src.Substring(transactionIdPos);
            else
                transactionId = "";
            return transactionId;
        }

        /// <summary>
        /// крайний срок, когда сообщение можно отправить 
        /// </summary>
	    public DateTime TTL
	    {
	        get { return _ttl; }
	        set { _ttl = value; }
	    }

        public DateTime TimeReceived
        {
            get { return _timeReceived; }
            set { _timeReceived = value; }
        }

	    public Guid SmscId
	    {
	        get { return _smscId; }
            set { _smscId = value; }
	    }

	    public bool IsFlash
	    {
	        get { return _isFlash; }
	        set { _isFlash = value; }
	    }


        /// <summary>
        /// отсылать ли уведомление о получении статуса
        /// </summary>
	    public bool NotifyMessageState
	    {
	        get { return notifyMessageState; }
	        set { notifyMessageState = value; }
	    }


	    /// <summary>
        /// Returns contents text string
        /// </summary>
        /// <returns>text string</returns>
        public override string ToString()
        {
            return string.Format("OutputMessage: text={0}, source={1}, destination={2},  http_category={3}, type={4} resourceid={5}", (_Content is TextContent) ? _Content.ToString() : "", _Source, _Destination, _HTTP_Category,Type.ToString(),ProjectID);
        }

		#region ISerializable Members

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("_ID", _ID);
			//info.AddValue("_channelID", _channelID);
			info.AddValue("_projectID", _projectID);
			//info.AddValue("_tariffId", _tariffId);
			info.AddValue("_Source", _Source);
			info.AddValue("_Destination", _Destination);
			info.AddValue("_Content", _Content);
			info.AddValue("_priority", _priority);
			info.AddValue("_TransactionId", _TransactionId);
			info.AddValue("_InboxMsgID", _InboxMsgID);
			//info.AddValue("_isBalanced", _isBalanced);
            info.AddValue("_IsPayed", _IsPayed);
			info.AddValue("_partsCount", _partsCount);
			info.AddValue("_HTTP_DeliveryStatus", _HTTP_DeliveryStatus);
			info.AddValue("_HTTP_Category", _HTTP_Category);
			info.AddValue("_HTTP_Operator", _HTTP_Operator);
			info.AddValue("_HTTP_Protocol", _HTTP_Protocol);
			info.AddValue("_HTTP_UID", _HTTP_UID);
			info.AddValue("_USSD_UserMessageReference", _USSD_UserMessageReference);
			info.AddValue("_USSD_Charging", _USSD_Charging);
			info.AddValue("_USSD_MessageContentType", _USSD_MessageContentType);
			info.AddValue("_USSD_DialogDirective", _USSD_DialogDirective);
			//info.AddValue("_deliveryStatus", _deliveryStatus);
            info.AddValue("_smscId", _smscId);
            info.AddValue("TTL", TTL);
		    info.AddValue("IsFlash", _isFlash);
            info.AddValue("notifyMessageState", NotifyMessageState);
            info.AddValue("_type",_type);
          
		}



		#endregion
	}
}
