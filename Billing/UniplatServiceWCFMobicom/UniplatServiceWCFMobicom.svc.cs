using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using MobicomLibrary;
using UniplatServiceData;

namespace UniplatServiceWCFMobicom
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "UniplatServiceWCFMobicom" in code, svc and config file together.
    public class UniplatServiceWCFMobicom : IUniplatServiceWCFMobicom
    {

        UPSEntities _storage = new UPSEntities();

        static UniplatServiceWCFMobicom()
        {
            IWebProxy proxy = WebRequest.GetSystemWebProxy();
            proxy.Credentials = CredentialCache.DefaultCredentials;
            WebRequest.DefaultWebProxy = proxy;
            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

            //var entities = new UPSEntities();
            //foreach (var bank in entities.Banks.ToArray())
            //{
            //    var updater = new MerchantDataUpdater(bank, entities);
            //    updater.Update();
            //}


        }
        
        public MobicomRegisterRequestOperationResponse MobicomRegisterRequestOperation(MobicomRegisterRequestOperationRequest request)
        {
            var res = new MobicomRegisterRequestOperationResponse();
            res.MCRegistRes = MobicomRegisterRequestOperation(request.MCRegistReq);
            return res;
        }

        public MobicomReserveRequestOperationResponse MobicomReserveRequestOperation(MobicomReserveRequestOperationRequest request)
        {
            throw new NotImplementedException();
        }

        public MobicomReserveExpressRequestOperationResponse MobicomReserveExpressRequestOperation(MobicomReserveExpressRequestOperationRequest request)
        {
            throw new NotImplementedException();
        }

        private MobicomRegisterResponse MobicomRegisterRequestOperation(MobicomRegisterRequest request)
        {

            Trace.WriteLine("Phone:" + request.Client.Phone + " Number:" + request.Payment.result);

            var response = new MobicomRegisterResponse();
            response.Result.code = 1;
            response.Result.comment = "";

            try
            {
                var initialRequest = LogRegisterRequest(request);

                if (request.Owner.id == null)
                    return response;

                var payment = _storage.Payments.SingleOrDefault(p => p.OwnerId == request.Owner.id);

                if (payment == null)
                    return response;

                if (!request.CheckHashCode(payment.Partner.Password))
                    return response;

                payment.Status = GetOrCreateStatus(request.Payment.result, payment.ProviderCode);

                response.Agregator.id = payment.Merchant.Bank.AgregarorId;
                response.Merchant.id = payment.Merchant.Code;
                response.Owner.id = payment.OwnerId;
                response.Client.Phone.number = payment.PaymentNumber;
                response.Payment.amount = payment.Amount;
                response.Payment.currency = payment.Currency;
                response.Transaction.id = request.Transaction.id;
                response.Message.comment = payment.SMSComment;

                NotifyPartner(payment);
                LogRegisterResponse(response);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Ошибка при получении статуса платежа: " + e);
                response.Result.code = 1;
                response.Result.comment = "";
                throw;
            }
            finally
            {
                TrySaveChanges();
            }
            return response;
        }

        private void NotifyPartner(UniplatServiceData.Payment payment)
        {
        }

        private Status GetOrCreateStatus(int? resultCode, int? providerCode)
        {
            Status status;
            status =
                _storage.Status.SingleOrDefault(s => s.ProviderId == (providerCode ?? 0) && s.Code == (resultCode ?? -1));

            if (status == null)
            {
                status = _storage.Status.CreateObject();
                status.Id = Guid.NewGuid();
                status.Name = null;
                status.Code = providerCode ?? 0;
                status.ProviderId = providerCode;
                status.PartnerDescription = "Ошибка связи с провайдером";
                status.Description = "Доселе неизвестный код ошибки";
                status.PartnerCode = 1;

                _storage.Status.AddObject(status);
            }

            return status;
        }

        private MobicomMessage LogRegisterRequest(MobicomRegisterRequest request)
        {
            var logEntry = _storage.MobicomMessages.CreateObject();

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "MobicomRegisterRequestOperation";
            logEntry.Action = "Request";
            logEntry.AgreagatorId = Truncate(request.Agregator.id.ToString(), 10);
            logEntry.MerchantId = Truncate(request.Merchant.id.ToString(), 10);
            logEntry.OwnerId = Truncate(request.Owner.id, 50);
            logEntry.ClientPhoneNumber = Truncate(request.Client.Phone.number, 10);
            logEntry.PaymentAmount = Truncate(request.Payment.amount.ToString(), 10);
            logEntry.PaymentCurrency = Truncate(request.Payment.currency.ToString(), 3);
            logEntry.PaymentResult = Truncate(request.Payment.result.ToString(), 10);
            logEntry.TransactionId = Truncate(request.Transaction.id, 50);
            logEntry.Version = Truncate(request.version, 10);
            logEntry.Hash = Truncate(request.hash, 50);

            _storage.MobicomMessages.AddObject(logEntry);

            return logEntry;
        }

        private MobicomMessage LogRegisterResponse(MobicomRegisterResponse response)
        {
            var logEntry = _storage.MobicomMessages.CreateObject();

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "MobicomRegisterRequestOperation";
            logEntry.Action = "Request";
            logEntry.AgreagatorId = Truncate(response.Agregator.id.ToString(), 10);
            logEntry.MerchantId = Truncate(response.Merchant.id.ToString(), 10);
            logEntry.OwnerId = Truncate(response.Owner.id, 50);
            logEntry.ClientPhoneNumber = Truncate(response.Client.Phone.number, 10);
            logEntry.PaymentAmount = Truncate(response.Payment.amount.ToString(), 10);
            logEntry.PaymentCurrency = Truncate(response.Payment.currency.ToString(), 3);
            logEntry.TransactionId = Truncate(response.Transaction.id, 50);
            logEntry.MessageComment = Truncate(response.Message.comment, 50);
            logEntry.ResultCode = Truncate(response.Result.code.ToString(), 10);
            logEntry.ResultComment = Truncate(response.Result.comment, 255);
            logEntry.Version = Truncate(response.version, 10);
            //logEntry.Hash = Truncate(response.hash, 50);

            _storage.MobicomMessages.AddObject(logEntry);

            return logEntry;
        }

        private string Truncate(string s, int length)
        {
            return s == null ? null : s.Substring(0, Math.Min(s.Length, length));
        }

        private void TrySaveChanges()
        {
            try
            {
                _storage.SaveChanges();
            }
            catch (Exception e)
            {
                EventLog.WriteEntry("UniplatServiceWCF", "Ошибка сохранения данных в БД : " + e, EventLogEntryType.Error);
                _storage = new UPSEntities();
            }
        }

    }

}
