using System;
using System.Globalization;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using MobicomLibrary;

namespace Uniplat.Classes
{
    public class TrustAllCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint sp, X509Certificate cert, WebRequest req, int problem)
        {
            return true;
        }
    }

    public class ClPayment
    {

        private Guid _mts
        {
            get
            {
                return new Guid("46C0E592-917B-4663-83DB-8A9F13C19BCE");
            }
        }
        private Guid _beeline
        {
            get
            {
                return new Guid("1F2D6739-B5C1-4096-B7A9-CAF4AC454058");
            }
        }

       

        public int SendPaymentToMobicom(string phoneNumber, long moneyCount, Guid operatorBrand)
        {
            try
            {
                // Отключаем проверку сертификатов путем замены класса проверки сертификатов на пустой
#pragma warning disable 612,618
                ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
#pragma warning restore 612,618

                // Создаем клиентское подключение к сервису Мобиком
                var cc = new mobicomTypeClient("BasicHttpBinding_MobicomService");

                // Основные параметры подключения
                // ****************************************************************
                var a = new Agregator {id = 29};
                var m = new Merchant {id = 6889};
                const string sVersion = "1.0";
                const string sPassword = "Fdk5637Gr";


                // Переменные параметры подключения
                // ****************************************************************
                var p = new Phone {number = phoneNumber};
                //p.provider = 3; // 1 — «Вымпелком», 2 — «Мобильные Телесистемы», 3 — «Мегафон»

                var c = new Client {Phone = p};

                var pt = new Payment {currency = 643, amount = moneyCount};

                var ms = new Message
                    {
                        bill = DateTime.Now.Ticks.ToString(CultureInfo.InvariantCulture).Substring(1, 10),
                        comment = "Test"
                    };

                var o = new Owner { id = Guid.NewGuid().ToString().Replace("-", "") }; // Идентификатор транзакции, заданный Агрегатором. (Регулярное выражение для проверки: ^[A-Za-z0-9]+$ ). Этот идентификатор должен быть уникальным в течении всего периода взаимодействия с системой Mobicom, причём в пределах обоих типов сообщений: MCStartReq и MCStartExReq.

                // ****************************************************************

                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                // Создаем и инициализируем объект MobicomStartRequest для быстрой регистрации запроса на стороне Мобиком
                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                var r = new MobicomStartRequest
                    {
                        Agregator = a,
                        Merchant = m,
                        Client = c,
                        version = sVersion,
                        Owner = o,
                        Payment = pt,
                        Message = ms
                    };

                // Генерируем Хэш-Код для  MobicomStartExpressRequest (1)Owner.id, 2)Client.Phone.number, 3)Client.Phone.provider, 4)Payment.amount, 5)Payment.currency, 6)Message.bill, 7)Message.comment, 8)секретный пароль Агрегатора.)
                string ss = r.Owner.id + r.Client.Phone.number + r.Client.Phone.provider.ToString() + r.Payment.amount.ToString() + r.Payment.currency.ToString() + r.Message.bill + r.Message.comment + sPassword;
                r.hash = GetHash(ss);

                // Регистрируем запрос в Мобиком и получаем статус выполнения запроса.
                MobicomStartResponse resp = cc.MobicomStartRequestOperation(r);
                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                if (resp.Result.code != null) return resp.Result.code.Value;
            }


            catch
            {
                return -1;
            }

            return -1;
        }



        private string GetHash(string sStr)
        {
            //byte[] data = Encoding.ASCII.GetBytes(sStr);
            byte[] data = Encoding.GetEncoding(1251).GetBytes(sStr);

            // экземпляр MD5
            MD5 md5 = MD5.Create();

            // получаю хеш
            byte[] result = md5.ComputeHash(data);

            return Convert.ToBase64String(result);
        }
    }
}