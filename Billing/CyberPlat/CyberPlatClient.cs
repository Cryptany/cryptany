using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using org.CyberPlat;
using System.Web;
using Avant.Monitoring;

namespace CyberPlat
{
    public class CyberPlatParams
    {
        private readonly Dictionary<string, string> _params = new Dictionary<string, string>();

        public void SetValue(string key, string value)
        {
            if (!_params.ContainsKey(key))
            {
                _params.Add(key, value);
            }
            else
            {
                _params[key] = value;
            }
        }

        public override string ToString()
        {
            return _params.Aggregate<KeyValuePair<string, string>, string>(null, (current, param) => current + param.Key + "=" + param.Value + "\r\n");
        }
    }

    public class CyberPlatClient
    {
        private static string SecretKey
        {
            get { return Properties.Settings.Default.SecretKey; }
        }

        private static string PublicKey
        {
            get { return Properties.Settings.Default.PublicKey; }
        }

        private static string Password
        {
            get { return Properties.Settings.Default.Password; }
        }

        private static string SerialNumber
        {
            get { return Properties.Settings.Default.SerialNumber; }
        }

        private static string PayService
        {
            get { return Properties.Settings.Default.PayService; }
        }

        private static string StatusService
        {
            get { return Properties.Settings.Default.StatusService; }
        }

        public CyberPlatClient()
        {
            try
            {
                IPriv.Initialize();
            }
            catch (Exception ex)
            {
                Functions.AddEvent("Ошибка при инициализации CyberPlatClient!", string.Format("CyberPlatClient!: [{0}]", ex.Message), EventType.Critical, null, ex);
                throw;
            }
        }

        /// <summary>
        /// Выполнение запроса на оплату
        /// </summary>
        /// <param name="msisdn">номер телефона</param>
        /// <param name="emitent">код оператора: 3 - МегаФон; 4 - МТС; 5 - Билайн;</param>
        /// <param name="amount">сумма к списанию</param>
        /// <param name="serviceid">код услуги</param>
        /// <param name="session">уникальный внешний идентификатор платежа - длина не более 20 символов</param>
        public string PayRequest(string msisdn, string emitent, int amount, string serviceid, string session)
        {
            var param = new CyberPlatParams();
            param.SetValue("NUMBER", msisdn);
            //переводим копейки в рубли и заменяем разделитель
            param.SetValue("AMOUNT", string.Format("{0:N2}", (decimal)amount / 100).Replace(",", "."));
            param.SetValue("SERVICEID", serviceid);
            param.SetValue("EMITENT", emitent);
            param.SetValue("SESSION", session);
            param.SetValue("SD", "17031");
            param.SetValue("AP", "17032");
            param.SetValue("OP", "17034");
            var body = Sign(param.ToString());
            Functions.AddEvent("PayRequest", string.Format("host: [{0}]; body: [{1}]", PayService, body), EventType.Info);
            var res = Post(PayService, body);
            Functions.AddEvent("PayRequest", string.Format("res: [{0}]", res), EventType.Info);
            var v = VerifySign(res);
            Done();
            return res;
        }

        public string GetStatus()
        {
            var param = new CyberPlatParams();
            param.SetValue("NUMBER", "9096021999");  //"9098888558"
            param.SetValue("AMOUNT", "101");
            param.SetValue("EMITENT", "13");
            param.SetValue("SESSION", "558");
            param.SetValue("SERVICEID", "5");
            param.SetValue("SD", "17031");
            param.SetValue("AP", "17032");
            param.SetValue("OP", "17034");
            var body = Sign(param.ToString());
            Functions.AddEvent("GetStatus", string.Format("host: [{0}]; body: [{1}]", PayService, body), EventType.Info);
            var res = Post(StatusService, body);
            Functions.AddEvent("GetStatus", string.Format("res: [{0}]", res), EventType.Info);
            var v = VerifySign(res);
            return res;
        }

        private string Sign(string message)
        {
            string res = null;
            IPrivKey sec = null;
            try
            {
                sec = IPriv.openSecretKey(SecretKey, Password);
                res = sec.signText(message);
            }
            catch (IPrivException ex)
            {
                Functions.AddEvent("Ошибка при подписывании сообщения!", string.Format("Sign!: [{0}]", ex.Message), EventType.Critical, null, ex);
            }
            if (sec != null)
                sec.closeKey();
            return res;
        }

        private bool VerifySign(string message)
        {
            var res = false;
            IPrivKey pub = null;
            try
            {
                pub = IPriv.openPublicKey(PublicKey, Convert.ToUInt32(SerialNumber, 10));
                pub.verifyText(message);
                Functions.AddEvent("Подпись верна!",
                                string.Format("VerifySign: [{0}]", message), EventType.Info);
                res = true;
            }
            catch (IPrivException ex)
            {
                Functions.AddEvent("Ошибка при проверке подписи!", string.Format("VerifySign!: [{0}]", ex.Message), EventType.Critical, null, ex);
            }
            if (pub != null)
                pub.closeKey();
            return res;
        }

        private static string PrepareData(string data)
        {
            string res = null;
            try
            {
                res = "inputmessage=" + HttpUtility.UrlEncode(data);
            }
            catch (Exception ex)
            {
                Functions.AddEvent("Ошибка при преобразовании данных!", string.Format("PrepareData!: [{0}]; data=[{1}]", ex.Message, data), EventType.Critical, null, ex);
            }
            return res;
        }

        private string Post(string url, string data)
        {
            string strReturn = null;
            try
            {
                //Encoding the post vars
                var buffer = Encoding.ASCII.GetBytes(PrepareData(data));
                //Initialisation with provided url

                //var nc = new NetworkCredential("User", "Password");
                //var cc = new CredentialCache {{"https://some.server.com", 443, "Basic", nc}};

                var webReq = (HttpWebRequest)WebRequest.Create(url);

                //Set method to post, otherwise postvars will not be used
                webReq.Method = "POST";
                //webReq.AllowAutoRedirect = false;
                //webReq.Credentials = cc;
                //webReq.PreAuthenticate = true;
                webReq.ContentType = "application/x-www-form-urlencoded";
                webReq.ContentLength = buffer.Length;
                var postData = webReq.GetRequestStream();
                postData.Write(buffer, 0, buffer.Length);
                //Closing is always important
                postData.Close();

                //Get the response handle, we have no true response yet
                var webResp = (HttpWebResponse)webReq.GetResponse();

                //information about the response
                var status = webResp.StatusCode;
                var server = webResp.Server;

                //read the response
                var webResponse = webResp.GetResponseStream();
                if (webResponse != null)
                {
                    var response = new StreamReader(webResponse);
                    strReturn = response.ReadToEnd().Trim();
                }
                else
                {
                    Debug.Print("webResponse == null");
                }
            }
            catch (Exception ex)
            {
                Functions.AddEvent("Ошибка в методе Post!", string.Format("Post!: [{0}]; url=[{1}]; data=[{2}]", ex.Message, url, data), EventType.Critical, null, ex);
            }
            return strReturn;
        }

        public void Done()
        {
            IPriv.Done();
        }
    }
}
