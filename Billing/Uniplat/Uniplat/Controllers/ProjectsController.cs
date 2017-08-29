using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;
using Uniplat.Classes;
using Recaptcha;
using System.Web.UI;
using System.Security.Cryptography;
using System.Text;

namespace Uniplat.Controllers
{
    public class ProjectsController : Controller
    {
        //
        // GET: /Projects/
        const string password = "123";

//        public static DataTable DefCodes = new DataTable();

        public ActionResult MoneyTransfer()
        {
            //LoadDefCodes();
            return View();
        }

        public ActionResult PaymentByMobile()
        {
            //LoadDefCodes();
            return View();
        }
        
        public ActionResult TaxiPayment()
        {
            //LoadDefCodes();
            return View();
        }

        public ActionResult PaymentFromMTM()
        {
            //LoadDefCodes();
            return View();
        }

        //[HttpPost]
        //public string FindOperator()
        //{
        //    string phone = Request.Params["param1"];
        //    phone = Regex.Replace(phone, "[^0-9]", String.Empty);
        //    if (phone.Length < 3) return "";
        //    for (int i = phone.Length; i < 10; i++) phone += '0';

        //    int def = int.Parse(phone.Substring(0, 3));
        //    long range = long.Parse(phone.Substring(3));
            
        //    var results = from myRow in DefCodes.AsEnumerable()
        //                  where
        //                  myRow.Field<int>("DEF") == def
        //                  && myRow.Field<long>("RangeStart") <= range
        //                  && myRow.Field<long>("RangeEnd") >= range
        //                  select myRow.Field<string>("Name");

        //    return (results.Any()) ? results.First() : "";
        //}

        [HttpPost]
        public string CountMoney()
        {
            int moneyWithProcent;
            if(int.TryParse(Request.Params["param1"], out moneyWithProcent))
                return "Сумма с учетом комиссии: " + CountMoney(moneyWithProcent).ToString(CultureInfo.InvariantCulture);
            return "Сумма с учетом комиссии: ";
        }

        public double CountMoney(int money)
        {
            return Math.Round(money * 107.45)/100;
        }

        [HttpGet]
        public string State()
        {
            if (Session["PaymentPartner"] == null || Session["PaymentId"] == null)
                return "Извините, сервис не доступен";

            string PaymentId = (string) Session["PaymentId"], PaymentPartner = (string) Session["PaymentPartner"];
            var client = new UniplatProxy.UniplatServiceClient();

            var reqState = new UniplatProxy.ReqState() { PaymentId = PaymentId, PartnerCode = PaymentPartner };
            reqState.Hash = GetHash(reqState.PaymentId + reqState.PartnerCode + password);
            var resp = client.GetDebitState(reqState);

            return resp.Resultcoment;
        }


        [HttpPost]
        [RecaptchaControlMvc.CaptchaValidator]
        public string Pay(bool chbAccept, string phone2, string phone1, string money, bool captchaValid = true)
        {
            if (chbAccept)
                return "Для продолжения необходимо принять условия предоставления услуг";

            //if (!captchaValid)
            //    return "Введен неверный код с картинки";

            string phone = phone2;
            phone = Regex.Replace(phone, "[^0-9]", String.Empty);
            if (phone.Length < 10)
                return "Введен неверный номер телефона";

            int def = int.Parse(phone.Substring(0, 3));
            long range = long.Parse(phone.Substring(3));

            //var operatorBrands = from myRow in DefCodes.AsEnumerable()
            //                     where
            //                         myRow.Field<int>("DEF") == def
            //                         && myRow.Field<long>("RangeStart") <= range
            //                         && myRow.Field<long>("RangeEnd") >= range
            //                     select myRow.Field<Guid>("OperatorBrandId");

            //if (!operatorBrands.Any())
            //    return "Неизвестный телефонный оператор";

            long moneyCount;
            if (!long.TryParse(money, out moneyCount))
                return "Введена неверная сумма";

            //переводим рубли в копейки
            moneyCount = (long)CountMoney((int)moneyCount);
            moneyCount *= 100;

            //var payment = new ClPayment();
            //if (payment.SendPaymentToMobicom(phone, moneyCount, operatorBrands.First()) == 0)

            

            var client = new UniplatProxy.UniplatServiceClient(); // .UniplatService();
            {
                Console.WriteLine("Sending DebitMoney");
                var paymentNumber = DateTime.Now.Ticks.ToString().Substring(1, 10);
                var req = new UniplatProxy.ReqDebit()
                    {
                        Amount = (int) moneyCount, //1000,
                        Comment = "Ваше такси",
                        PartnerCode = "tst",
                        ServiceId = "6889",
                        MSISDN = phone, //"9099952793", //"9035618208", //"9168036435",//"9099952793",//"9055812453",
                        PaymentNumber = paymentNumber,
                        PaymentId = Guid.NewGuid().ToString()
                    };



                req.Hash = GetHash(req.PaymentId + req.PartnerCode + req.ServiceId + req.MSISDN + req.PaymentNumber +
                                   req.Amount + password);
                var resp = client.DebitMoney(req);


                if (resp.ResultCode == 0)
                {
                    Session["PaymentPartner"] = req.PartnerCode;
                    Session["PaymentId"] = req.PaymentId;
                    return "Ваша заявка принята";
                    #region comment
                    //Console.WriteLine("Succsessfull response received");
                    //Console.WriteLine("Send state request?");
                    //int x = 0;
                    //while (Console.ReadLine() == "y") //(x ++ < 10)
                    //{
                    //    var reqState = new UniplatServiceWCF.ReqState() {PaymentId = req.PaymentId, PartnerCode = "tst"};
                    //    reqState.Hash = GetHash(reqState.PaymentId + reqState.PartnerCode + password);
                    //    resp = client.GetDebitState(reqState);
                    //    Console.WriteLine(resp.ResultCode + " " + resp.Resultcoment);
                    //} 
                    #endregion
                }
                else
                {
                    return "Извините, сервис временно недоступен";
                }
            }
        }

        static private string GetHash(string sStr)
        {
            string sResult = "";

            // сначала конвертирую строку в байты
            //byte[] data = Encoding.Unicode.GetBytes(sStr);
            byte[] data = Encoding.ASCII.GetBytes(sStr);

            // экземпляр MD5
            MD5 md5 = MD5.Create();

            // получаю хеш
            byte[] result = md5.ComputeHash(data);

            // конвертирую хеш из byte[16] в строку шестнадцатиричного формата
            // (вида «3C842B246BC74D28E59CCD92AF46F5DA»)
            // это опциональный этап, если вам хеш нужен в строковом виде
            //sResult = BitConverter.ToString(result).Replace("-", string.Empty);

            sResult = Convert.ToBase64String(result);


            return sResult;
        }

//        private static DataTable _defCodes;

//        public static DataTable DefCodes
//        {
//            get
//            {
//                if (_defCodes == null || _defCodes.Rows.Count == 0)
//                {
//                    _defCodes=new DataTable();
//                    using (var con = new SqlConnection(
//                        "Data Source=;Initial Catalog=;Persist Security Info=True;User ID=uniplatservice;Password=Maslov3210"))
//                    {
//                        con.Open();
//                        using (var cmd = new SqlCommand(@"
//                    SELECT 
//                        d.Id,
//                        d.DEF,
//                        d.RangeStart,
//                        d.RangeEnd,
//                        ob.Id as OperatorBrandId,
//                        ob.Name + ' ' + r.NODE_NAME as Name
//                          FROM [kernel].[DEFcode] d
//                          join [kernel].Operators o on o.Id = d.OperatorId
//                          join [kernel].OperatorBrands ob on ob.Id = o.BrandId
//                          join [kernel].Regions r on r.ID = d.RegionId
//                    ", con))
//                        {
//                            var da = new SqlDataAdapter(cmd);
//                            da.Fill(_defCodes);
//                        }
//                    }
//                }
//                return _defCodes;
//            }
//        }
    }
}
