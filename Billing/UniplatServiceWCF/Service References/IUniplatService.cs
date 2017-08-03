using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace UniplatServiceWCF
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    /// <summary>
    /// Контракт для работы партнеров с системой UniplatService
    /// </summary>
    [ServiceContract]
    public interface IUniplatService
    {
        [OperationContract]
        //[WebGet(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml,
        //    ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/GetDebitState")]
        Resp DebitMoney(ReqDebit request);

        [OperationContract]
        //[WebGet(BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Xml,
        //    ResponseFormat = WebMessageFormat.Xml, UriTemplate = "/GetDebitState")]
        Resp GetDebitState(ReqState request);
    }

    [DataContract]
    public class ReqDebit
    {
        private string _MSISDN;
        private string _paymentId;
        private string _operatorCode;
        private string _comment;
        private string _serviceId;
        private string _partnerCode;
        private int _amount;
        private string _hash;
        private string _paymentNumber;

        /// <summary>
        /// Номер телефона абонента для списания
        /// </summary>
        [DataMember]
        public string MSISDN 
        {
            get { return _MSISDN; }
            set { _MSISDN = value; }
        }

        /// <summary>
        /// Уникальный идентификатор платежа
        /// </summary>
        [DataMember]
        public string PaymentId
        {
            get { return _paymentId; }
            set { _paymentId = value; }
        }

        /// <summary>
        /// Код оператора (см справочник Операторы)
        /// </summary>
        [DataMember]
        public string OperatorCode
        {
            get { return _operatorCode; }
            set { _operatorCode = value; }
        }

        /// <summary>
        /// Oписание услуги (заказа), который оплачивает клиент (будет передано в SMS-сообщении) максимальная длина 15 символов.
        /// </summary>
        [DataMember]
        public string Comment
        {
            get { return _comment; }
            set { _comment = value; }
        }

        /// <summary>
        /// Код сервиса по которому происходит списание (см справочник Сервисы)
        /// </summary>
        [DataMember]
        public string ServiceId
        {
            get { return _serviceId; }
            set { _serviceId = value; }
        }

        /// <summary>
        /// Код партнера, переданный при регистрации
        /// </summary>
        [DataMember]
        public string PartnerCode
        {
            get { return _partnerCode; }
            set { _partnerCode = value; }
        }

        /// <summary>
        /// Сумма списания в копейках
        /// </summary>
        [DataMember]
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        /// <summary>
        /// md5 хеш конкатенации следующих параметров:
        /// PaymentId + PartnerCode + ServiceId + MSISDN + PaymentNumber + Amount + Password
        /// Где Password – секретный пароль Партнера, полученный при регистрации партнера в Системе.
        /// </summary>
        [DataMember]
        public string Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        /// <summary>
        /// уникальный номер списания (будет передан абоненту в SMS )
        /// Первые три символа – код партнера, остальные 7 символов – номер платежа (уникальный за последние 30 дней), например «0031234567»
        /// </summary>
        [DataMember]
        public string PaymentNumber
        {
            get { return _paymentNumber; }
            set { _paymentNumber = value; }
        }

        private static Regex _paymentIdRegex = new Regex(@"^.*$", RegexOptions.Compiled),
                             _partnerCodeRegex = new Regex(@"^.{1,3}$", RegexOptions.Compiled),
                             _serviceIdRegex = new Regex(@"^\d+$", RegexOptions.Compiled),
                             _msisdnRegex = new Regex(@"^\d{10}$", RegexOptions.Compiled),
                             _paymentNumberRegex = new Regex(@"^.{10}$", RegexOptions.Compiled),
                             _amountRegex = new Regex(@"^\d+$", RegexOptions.Compiled);

        internal bool CheckParametersRegex(out string wrongParameters)
        {
            var list = new List<string>();
            if (!_paymentIdRegex.IsMatch(PaymentId ?? ""))
                list.Add("PaymentId");
            if (!_partnerCodeRegex.IsMatch(PartnerCode ?? ""))
                list.Add("PartnerCode");
            if (!_serviceIdRegex.IsMatch(ServiceId ?? ""))
               list.Add("ServiceId");
            if (!_msisdnRegex.IsMatch(MSISDN ?? ""))
                list.Add("MSISDN");
            if (!_paymentNumberRegex.IsMatch(PaymentNumber ?? ""))
                list.Add("PaymentNumber");
            if (!_amountRegex.IsMatch(Amount.ToString()))
                list.Add("Amount");

            if (list.Count == 0)
            {
                wrongParameters = "";
                return true;
            }
            
            wrongParameters = list.Aggregate((cur, i) => cur + ", " + i);
            return false;
        }

        internal bool CheckHashCode(string password)
        {
            var checkingValue = PaymentId + PartnerCode + ServiceId + MSISDN + PaymentNumber + Amount + password;
            var data = Encoding.GetEncoding(1251).GetBytes(checkingValue);
            var md5 = MD5.Create();

            var hash = Convert.ToBase64String(md5.ComputeHash(data));

            return Hash == hash;
        }
    }

    [DataContract]
    public class ReqState
    {
        private string _paymentId;
        private string _partnerCode;
        private string _hash;

        /// <summary>
        /// Идентификатор транзакции, заданный Партнером.
        /// </summary>
        [DataMember]
        public string PaymentId
        {
            get { return _paymentId; }
            set { _paymentId = value; }
        }

        /// <summary>
        /// Код партнера, переданный при регистрации
        /// </summary>
        [DataMember]
        public string PartnerCode
        {
            get { return _partnerCode; }
            set { _partnerCode = value; }
        }

        /// <summary>
        /// md5 хеш конкатенации следующих параметров:
        /// PaymentId + PartnerCode + Password
        /// Где Password – секретный пароль Партнера, полученный при регистрации партнера в Системе.
        /// </summary>
        [DataMember]
        public string Hash
        {
            get { return _hash; }
            set { _hash = value; }
        }

        private static Regex _paymentIdRegex = new Regex(@"^.*$", RegexOptions.Compiled),
                             _partnerCodeRegex = new Regex(@"^.{1,3}$", RegexOptions.Compiled);

        internal bool CheckParametersRegex(out string wrongParameters)
        {
            var list = new List<string>();
            if (!_paymentIdRegex.IsMatch(PaymentId ?? ""))
                list.Add("PaymentId");
            if (!_partnerCodeRegex.IsMatch(PartnerCode ?? ""))
                list.Add("PartnerCode");
            if (list.Count == 0)
            {
                wrongParameters = "";
                return true;
            }
            wrongParameters = list.Aggregate((cur, i) => cur + ", " + i);
            return false;
        }

        internal bool CheckHashCode(string password)
        {
            var checkingValue = PaymentId + PartnerCode + password;
            var data = Encoding.GetEncoding(1251).GetBytes(checkingValue);
            var md5 = MD5.Create();

            var hash = Convert.ToBase64String(md5.ComputeHash(data));

            return Hash == hash;
        }
    }

    [DataContract]
    public class Resp
    {
        private string _paymentId;
        private int _resultCode;
        private string _resultcoment;

        /// <summary>
        /// Идентификатор транзакции, заданный Партнером.
        /// </summary>
        [DataMember]
        public string PaymentId
        {
            get { return _paymentId; }
            set { _paymentId = value; }
        }

        /// <summary>
        /// Код результата обработки запроса. Это основное значение, на которое необходимо ориентироваться, выстраивая бизнес-логику приложения. Возможные коды ошибок обработки на этом этапе:
        /// •	0 — запрос принят в обработку,
        /// •	2 — нарушен формат параметра,
        /// •	3 — указаны ошибочные идентификаторы поставщика услуги,
        /// •	4 — ошибка в hash-значении,
        /// •	5 — сервер временно не принимает запросы в связи с высокой нагрузкой. Повторите попытку позже,
        /// •	6 — номер мобильного телефона не опознан, неизвестный провайдер,
        /// •	9 — категория не найдена,
        /// •	11 — ошибка связи с Провайдером.
        /// </summary>
        [DataMember]
        public int ResultCode
        {
            get { return _resultCode; }
            set { _resultCode = value; }
        }

        /// <summary>
        /// Комментарий к результату обработки в произвольном текстовом формате.
        /// </summary>
        [DataMember]
        public string Resultcoment
        {
            get { return _resultcoment; }
            set { _resultcoment = value; }
        }
    }

}
