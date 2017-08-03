
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;

//using NS = UniplatServiceWCFClient.UniplatProxy;
using NS = UniplatServiceWCF;

namespace UniplatServiceWCFClient
{
    class Program
    {
        static void Main(string[] args)
        {

            const string password = "123";

            //var p = new UniplatServiceWCFMobicom.UniplatServiceWCFMobicom();

            var client = new NS.UniplatService();//UniplatServiceClient();
            {
                Console.WriteLine("Sending DebitMoney");
                var paymentNumber = DateTime.Now.Ticks.ToString().Substring(1, 10);
                var reqBeeline = new NS.ReqDebit()
                    {
                        Amount = 1000,
                        Comment = "Test",
                        PartnerCode = "tst",
                        ServiceId = "13632",//"6889",
                        MSISDN = "9099952793", //"9035618208", //"9168036435",//"9099952793",//"9055812453",
                        PaymentNumber = paymentNumber,
                        PaymentId = Guid.NewGuid().ToString()
                    };

                var reqMts = new NS.ReqDebit()
                {
                    Amount = 1500,
                    Comment = "Test",
                    PartnerCode = "tst",
                    ServiceId = "6889", //"6907",
                    MSISDN = "9104501874", //"9521873493", //"9035618208", //"9168036435",//"9099952793",//"9055812453",
                    PaymentNumber = paymentNumber,
                    PaymentId = Guid.NewGuid().ToString()
                };

                var req = reqMts;

                req.Hash = GetHash(req.PaymentId + req.PartnerCode + req.ServiceId + req.MSISDN + req.PaymentNumber +
                           req.Amount + password);
                var resp = client.DebitMoney(req);
                if (resp.ResultCode == 0)
                {
                    Console.WriteLine("Succsessfull response received");
                    Console.WriteLine("Send state request?");
                    int x = 0;
                    while (Console.ReadLine() == "y") //(x ++ < 10)
                    {
                        var reqState = new NS.ReqState() { PaymentId = req.PaymentId, PartnerCode = "tst" };
                        reqState.Hash = GetHash(reqState.PaymentId + reqState.PartnerCode + password);
                        resp = client.GetDebitState(reqState);
                        Console.WriteLine(resp.ResultCode + " " + resp.ResultComment);
                    }
                }
                else
                {
                    Console.WriteLine("Bad request. Error code: " + resp.ResultCode);
                }
                Console.ReadKey();
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
    }
}
