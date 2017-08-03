using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using UniplatServiceData;

namespace UniplatServiceWCF
{
    public class MerchantDataUpdater
    {
        private UPSEntities _entities;
        private Bank _bank;
        public MerchantDataUpdater(Bank bank, UPSEntities entities)
        {
            _bank = bank;
            _entities = entities;
        }

        public void Update()
        {

            try
            {

                IWebProxy Proxy = WebRequest.GetSystemWebProxy();
                Proxy.Credentials = CredentialCache.DefaultCredentials;
                HttpWebRequest.DefaultWebProxy = Proxy;

                // Отключаем проверку сертификатов путем замены класса проверки сертификатов на пустой
                //System.Net.ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();

                var requestPattern = ConfigurationManager.AppSettings["merchantReportRequest"];
                var df = DateTime.Now.AddYears(-3).ToString("yyyy-MM-dd");
                var dt = DateTime.Now.ToString("yyyy-MM-dd");
                var a = _bank.AgregarorId;
                var s = GetHash(df + dt + a + _bank.Password);

                var reqestString = string.Format(requestPattern, df, dt, a, s);
                var request = (HttpWebRequest)WebRequest.Create(reqestString);

                var response = request.GetResponse();
                //var xmlDoc = new XmlDocument();
                using (var stream = response.GetResponseStream()) // new FileStream(@"..\..\MerchantReport.xml", FileMode.Open, FileAccess.Read))
                {
                    var report =
                        (MobicomMerchantReport.MerchantReport)
                        new XmlSerializer(typeof (MobicomMerchantReport.MerchantReport)).Deserialize(stream);
                    SaveToDB(report);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при получении информации по мерчантам: " + e);
            }
        }

        private void SaveToDB(MobicomMerchantReport.MerchantReport report)
        {
            foreach (var reportMerchant in report.Merchant)
            {
                Merchant merchant = _entities.Merchants.SingleOrDefault(m => m.Code == reportMerchant.id && m.BankId == _bank.Id);
                if (merchant == null)
                {
                    merchant = _entities.Merchants.CreateObject();
                    merchant.Id = Guid.NewGuid();
                    merchant.Bank = _bank;
                    _entities.AddToMerchants(merchant);
                }

                merchant.CreateTime = reportMerchant.CreateTime;
                merchant.EditTime = reportMerchant.EditTime;
                merchant.CloseTime = reportMerchant.CloseTime;
                merchant.OwnerId = reportMerchant.Owner.id;
                merchant.Name = reportMerchant.name;
                merchant.Code = reportMerchant.id;


                foreach (var reportProvider in reportMerchant.ProviderLinks)
                {
                    var operatorId = GetOperatorIdByCode(reportProvider.id);
                    Merchants2Operators m2o = _entities.Merchants2Operators.SingleOrDefault(i => i.Merchant.Id == merchant.Id && i.OperatorParameter.Code == reportProvider.id);
                    OperatorParameter operatorParameter;
                    if (m2o == null)
                    {
                        operatorParameter = _entities.OperatorParameters.CreateObject();
                        operatorParameter.Id = Guid.NewGuid();

                        _entities.AddToOperatorParameters(operatorParameter);

                        m2o = _entities.Merchants2Operators.CreateObject();
                        m2o.Id = Guid.NewGuid();
                        m2o.Merchant = merchant;
                        m2o.OperatorParameter = operatorParameter;
                        m2o.BrandId = operatorId;

                        _entities.AddToMerchants2Operators(m2o);
                    }
                    else
                        operatorParameter = m2o.OperatorParameter;

                    operatorParameter.Code = reportProvider.id;
                    operatorParameter.ProviderCode = reportProvider.Category.providerCode;
                    operatorParameter.MinAmount = reportProvider.Category.MinAmount;
                    operatorParameter.MaxAmount = reportProvider.Category.MaxAmount;
                    operatorParameter.AbonentInterest = reportProvider.Category.AbonentInterest;
                    operatorParameter.Active = reportProvider.Category.active;
                }
            }
            _entities.SaveChanges();
        }

        private Guid GetOperatorIdByCode(int id)
        {
            switch (id)
            {
                case 1: // Вымпелком
                    return new Guid("1F2D6739-B5C1-4096-B7A9-CAF4AC454058");
                case 2: // МТС
                    return new Guid("46C0E592-917B-4663-83DB-8A9F13C19BCE");
                case 3: // Мегафон
                    return new Guid("96C4C632-1815-4CE9-BCD2-B2C05BFF539E");
                case 4: // Теле2
                    return new Guid("EB5926B9-9690-4C4D-AE68-AC0137D0DEC5");

                default:
                    throw new InvalidDataException("Unknown provider");
                    return new Guid("00000000-0000-0000-0000-000000000000");
            }
        }

        private string GetHash(string sStr)
        {
            //byte[] data = Encoding.ASCII.GetBytes(sStr);
            byte[] data = Encoding.GetEncoding(1251).GetBytes(sStr);

            // экземпляр MD5
            MD5 md5 = MD5.Create();

            // получаю хеш
            byte[] result = md5.ComputeHash(data);

            return HttpUtility.UrlEncode(Convert.ToBase64String(result));
        }
    }
}