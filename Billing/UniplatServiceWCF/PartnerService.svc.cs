using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using CyberPlat;
using MobicomLibrary;
using System.Linq;
using System.Diagnostics;
using UniplatServiceData;
using System.Configuration;


namespace UniplatServiceWCF
{
    public class UniplatService : IUniplatService
    {

        UPSEntities _storage = new UPSEntities();

        static UniplatService()
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

        private const int CurrencyId = 643;

        private static readonly Guid MEGAPHONE_OPERATOR_BREND = new Guid("96C4C632-1815-4CE9-BCD2-B2C05BFF539E");

        #region IUniplatService

        public Resp DebitMoney(ReqDebit request)
        {
            Trace.WriteLine("ReqDebit for " + request.MSISDN + " amount=" + request.Amount);
            MobicomMessage initialResponse = null;
            Func<int, string, Resp> createAndSaveDebitResp =
                (code, comment) => CreateAndSaveResp(request.PaymentId, code, comment, "DebitMoney", initialResponse);

            try
            {
                // Если это тестовый режим, то ...
                if (ConfigurationManager.AppSettings["IsTesting"].ToLower() == "true")      // Если это тестовый режим, то ...
                {
                    #region Log
                    var initialRequest = LogReqDebit(request);
                    var payment = FillPaymentWithRequest(request);

                    string wrongParameter;
                    Guid paymentId;
                    if (!Guid.TryParse(request.PaymentId, out paymentId))
                        return createAndSaveDebitResp(2, "Incorrect parameter: PaymentId");
                    payment.Id = paymentId;

                    if (!request.CheckParametersRegex(out wrongParameter))
                        return createAndSaveDebitResp(2, "Incorrect parameter: " + wrongParameter);

                    var partner = _storage.Partners.SingleOrDefault(p => p.Code == request.PartnerCode);
                    if (partner == null)
                        return createAndSaveDebitResp(3, "Incorrect parameter: PartnerCode");
                    payment.Partner = partner;

                    if (!request.CheckHashCode(partner.Password))
                        return createAndSaveDebitResp(4, "Incorrect hash-value");

                    var brand = _storage.GetBrandByMSISDN("7" + request.MSISDN).Single();
                    var brandNotFound = brand.Id == Guid.Empty;

                    //Определяем агрегатора которому предназначен запрос
                    int merchOwnerId = int.Parse(request.ServiceId);

                    var merch2Op =
                        partner.Partner2Merchants.Select(p2m => p2m.Merchant)
                               .Where(m => m.Code == merchOwnerId)
                               .SelectMany(m => m.Merchants2Operators)
                               .SingleOrDefault(
                                   m2o => m2o.BrandId == brand.Id && m2o.OperatorParameter.Active);
                    // Условие переделать

                    if (merch2Op == null)
                        return createAndSaveDebitResp(3, "ServiceId");

                    var merchant = merch2Op.Merchant;
                    payment.Merchant = merchant;

                    var bank = merchant.Bank;
                    var agregatorId = bank.AgregarorId;

                //-------------------------------------
                //развилка по банкам/платежным системам
                //-------------------------------------

                if (brand.Id == MEGAPHONE_OPERATOR_BREND)
                {
                    #region КиберПлат

                    var clnt = new CyberPlatClient();
                    var res=clnt.PayRequest(request.MSISDN, merch2Op.OperatorParameter.Code.ToString(), request.Amount, request.ServiceId,request.PaymentNumber);
                    Debug.Print(res);
                    #endregion
                }
                else
                {
                    #region Мобиком
                    // Основные параметры подключения к МобиКом
                    // ****************************************************************
                    var mobiAgregator = new Agregator { id = agregatorId };
                    var mobiMerchant = new MobicomLibrary.Merchant { id = merchant.Code };
                    // Переменные параметры подключения
                    // ****************************************************************
                    var mobiPhone = new Phone { number = request.MSISDN, provider = merch2Op.OperatorParameter.Code };
                    var mobyClient = new Client { Phone = mobiPhone };
                    var mobiPayment = new MobicomLibrary.Payment { currency = CurrencyId, amount = request.Amount };
                    _storage.Payments.AddObject(payment);
                    var mobiMessage = new Message
                        {
                            bill = request.PaymentNumber,
                            comment = request.Comment
                        };

                    Owner mobiOwner = new Owner { id = Guid.NewGuid().ToString().Replace("-", "") };
                    // Идентификатор транзакции, заданный Агрегатором. (Регулярное выражение для проверки: ^[A-Za-z0-9]+$ ). Этот идентификатор должен быть уникальным в течении всего периода взаимодействия с системой Mobicom, причём в пределах обоих типов сообщений: MCStartReq и MCStartExReq.
                    payment.OwnerId = mobiOwner.id;
                    _storage.Payments.AddObject(payment);
                    //// Регистрируем запрос в Мобиком и получаем статус выполнения запроса.
                    //var mobiClient = new mobicomTypeClient();
                    //MobicomStartResponse resp = mobiClient.MobicomStartRequestOperation(mobiRequest);
                    //initialResponse = LogMobicomStartResponse(resp);
                    ////+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    //if (resp.Result.code.HasValue && resp.Result.code.Value != 0)
                    //    return createAndSaveDebitResp(11, resp.Result.comment);
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Создаем и инициализируем объект MobicomStartRequest для быстрой регистрации запроса на стороне Мобиком
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    var mobiRequest = new MobicomStartRequest
                        {
                            Agregator = mobiAgregator,
                            Merchant = mobiMerchant,
                            Client = mobyClient,
                            version = bank.ProtocolVersion,
                            Owner = mobiOwner,
                            Payment = mobiPayment,
                            Message = mobiMessage
                        };

                    string ss = mobiRequest.Owner.id + mobiRequest.Client.Phone.number +
                                mobiRequest.Client.Phone.provider.ToString() +
                                mobiRequest.Payment.amount.ToString() + mobiRequest.Payment.currency.ToString() +
                                mobiRequest.Message.bill +
                                mobiRequest.Message.comment + bank.Password;
                    mobiRequest.hash = GetHash(ss);
                    LogMobicomStartRequest(mobiRequest, initialRequest);

                    // Регистрируем запрос в Мобиком и получаем статус выполнения запроса.
                    var mobiClient = new mobicomTypeClient();
                    MobicomStartResponse resp = mobiClient.MobicomStartRequestOperation(mobiRequest);
                    initialResponse = LogMobicomStartResponse(resp);
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    if (resp.Result.code.HasValue && resp.Result.code.Value != 0)
                        return createAndSaveDebitResp(11, resp.Result.comment);

                    #endregion
                }
                return createAndSaveDebitResp(0, "");
                    //createAndSaveDebitResp(0, "");

                    #endregion

                    #region Emulation

                    //var eagregatorId = 33; // bank.AgregarorId;


                    // //Основные параметры подключения
                    // //****************************************************************
                    //var emobiAgregator = new ServiceEmulation.Agregator { id = eagregatorId };

                    //var emobiMerchant = new ServiceEmulation.Merchant { id = 6889 };

                    // //Переменные параметры подключения
                    // //****************************************************************
                    //var emobiPhone = new ServiceEmulation.Phone { number = request.MSISDN, provider = 2 };

                    //emobiPhone.provider = 3; // 1 — «Вымпелком», 2 — «Мобильные Телесистемы», 3 — «Мегафон»
                    //var emobClient = new ServiceEmulation.Client { Phone = emobiPhone }; //var mobyClient = new Client { Phone = mobiPhone };

                    //var emobiPayment = new ServiceEmulation.Payment { currency = CurrencyId, amount = request.Amount };

                    //var emobiMessage = new ServiceEmulation.Message
                    //{
                    //    bill = request.PaymentNumber,
                    //    comment = request.Comment
                    //};

                    //var emobiOwner = new ServiceEmulation.Owner { id = Guid.NewGuid().ToString().Replace("-", "") };
                    // //Идентификатор транзакции, заданный Агрегатором. (Регулярное выражение для проверки: ^[A-Za-z0-9]+$ ). Этот идентификатор должен быть уникальным в течении всего периода взаимодействия с системой Mobicom, причём в пределах обоих типов сообщений: MCStartReq и MCStartExReq.


                    ////+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    //// Создаем и инициализируем объект MobicomStartRequest для быстрой регистрации запроса на стороне Мобиком
                    ////+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    //var emobiRequest = new ServiceEmulation.MobicomStartRequest
                    //{
                    //    Agregator = emobiAgregator,
                    //    Merchant = emobiMerchant,
                    //    Client = emobClient,
                    //    version = "1.0", //bank.ProtocolVersion,
                    //    Owner = emobiOwner,
                    //    Payment = emobiPayment,
                    //    Message = emobiMessage
                    //};


                    //string ess = emobiRequest.Owner.id + emobiRequest.Client.Phone.number +
                    //            emobiRequest.Client.Phone.provider.ToString() +
                    //            emobiRequest.Payment.amount.ToString() + emobiRequest.Payment.currency.ToString() +
                    //            emobiRequest.Message.bill +
                    //            emobiRequest.Message.comment + "Qq12345678"; //+ bank.Password;
                    //emobiRequest.hash = GetHash(ess);
                    //LogMobicomStartRequest(mobiRequest, initialRequest);


                    //// Создаем слиентское подключение к сервису Мобиком
                    //var emobiClient = new ServiceEmulation.MobicomEmulationClient("BasicHttpBinding_IMobicomEmulation");

                    //// Регистрируем запрос в Мобиком и получаем статус выполнения запроса.
                    //ServiceEmulation.MobicomStartResponse eresp = emobiClient.MobicomStartRequestOperation(emobiRequest);

                    //return createAndSaveDebitResp(eresp.Result.code.GetValueOrDefault(0), eresp.Result.comment);

                    #endregion
                }
                else    // Иначе это боевой режим
                {
                    #region Pay
                    var initialRequest = LogReqDebit(request);
                    var payment = FillPaymentWithRequest(request);

                    string wrongParameter;
                    Guid paymentId;
                    if (!Guid.TryParse(request.PaymentId, out paymentId))
                        return createAndSaveDebitResp(2, "Incorrect parameter: PaymentId");
                    payment.Id = paymentId;

                    if (!request.CheckParametersRegex(out wrongParameter))
                        return createAndSaveDebitResp(2, "Incorrect parameter: " + wrongParameter);

                    var partner = _storage.Partners.SingleOrDefault(p => p.Code == request.PartnerCode);
                    if (partner == null)
                        return createAndSaveDebitResp(3, "Incorrect parameter: PartnerCode");
                    payment.Partner = partner;

                    if (!request.CheckHashCode(partner.Password))
                        return createAndSaveDebitResp(4, "Incorrect hash-value");

                    var brand = _storage.GetBrandByMSISDN("7" + request.MSISDN).Single();
                    var brandNotFound = brand.Id == Guid.Empty;

                    //Определяем агрегатора которому предназначен запрос
                    int merchOwnerId = int.Parse(request.ServiceId);

                    /*
                    var merch2Op1 =
                        partner.Partner2Merchants.Select(p2m => p2m.Merchant).ToArray();

                    var merch2Op2 =
                        partner.Partner2Merchants.Select(p2m => p2m.Merchant)
                               .Where(m => m.Code == merchOwnerId).ToArray();

                    var merch2Op3 =
                        partner.Partner2Merchants.Select(p2m => p2m.Merchant)
                               .Where(m => m.Code == merchOwnerId)
                               .SelectMany(m => m.Merchants2Operators).ToArray();
                    */

                    var merch2Op =
                        partner.Partner2Merchants.Select(p2m => p2m.Merchant)
                               .Where(m => m.Code == merchOwnerId)
                               .SelectMany(m => m.Merchants2Operators)
                               .SingleOrDefault(
                                   m2o => m2o.BrandId == brand.Id && m2o.OperatorParameter.Active);
                    // Условие переделать

                    if (merch2Op == null)
                        return createAndSaveDebitResp(3, "ServiceId");

                    var merchant = merch2Op.Merchant;
                    payment.Merchant = merchant;

                    var bank = merchant.Bank;
                    var agregatorId = bank.AgregarorId;
                    // Основные параметры подключения
                    // ****************************************************************
                    var mobiAgregator = new Agregator { id = agregatorId };

                    var mobiMerchant = new MobicomLibrary.Merchant { id = merchant.Code };
                    // Переменные параметры подключения
                    // ****************************************************************
                    var mobiPhone = new Phone { number = request.MSISDN, provider = merch2Op.OperatorParameter.Code };

                    var mobyClient = new Client { Phone = mobiPhone };
                    var mobiPayment = new MobicomLibrary.Payment { currency = CurrencyId, amount = request.Amount };

                    var mobiMessage = new Message
                    {
                        bill = request.PaymentNumber,
                        comment = request.Comment
                    };

                    Owner mobiOwner = new Owner { id = Guid.NewGuid().ToString().Replace("-", "") };
                    // Идентификатор транзакции, заданный Агрегатором. (Регулярное выражение для проверки: ^[A-Za-z0-9]+$ ). Этот идентификатор должен быть уникальным в течении всего периода взаимодействия с системой Mobicom, причём в пределах обоих типов сообщений: MCStartReq и MCStartExReq.
                    payment.OwnerId = mobiOwner.id;

                    _storage.Payments.AddObject(payment);


                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    // Создаем и инициализируем объект MobicomStartRequest для быстрой регистрации запроса на стороне Мобиком
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    var mobiRequest = new MobicomStartRequest
                        {
                            Agregator = mobiAgregator,
                            Merchant = mobiMerchant,
                            Client = mobyClient,
                            version = bank.ProtocolVersion,
                            Owner = mobiOwner,
                            Payment = mobiPayment,
                            Message = mobiMessage
                        };

                    string ss = mobiRequest.Owner.id + mobiRequest.Client.Phone.number +
                                mobiRequest.Client.Phone.provider.ToString() +
                                mobiRequest.Payment.amount.ToString() + mobiRequest.Payment.currency.ToString() +
                                mobiRequest.Message.bill +
                                mobiRequest.Message.comment + bank.Password;
                    mobiRequest.hash = GetHash(ss);
                    LogMobicomStartRequest(mobiRequest, initialRequest);

                    // Регистрируем запрос в Мобиком и получаем статус выполнения запроса.
                    var mobiClient = new mobicomTypeClient();
                    MobicomStartResponse resp = mobiClient.MobicomStartRequestOperation(mobiRequest);
                    initialResponse = LogMobicomStartResponse(resp);
                    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                    if (resp.Result.code.HasValue && resp.Result.code.Value != 0)
                        return createAndSaveDebitResp(11, resp.Result.comment);

                    return createAndSaveDebitResp(0, "");

                    #endregion
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine("Ошибка в DebitMoney: " + e);
                return createAndSaveDebitResp(13, "UniplatService internal error");
            }
            finally
            {
                TrySaveChanges();
            }
        }

        public Resp GetDebitState(ReqState request)
        {
            //MobicomMessage initialResponse = null;
            Func<int, string, Resp> createAndSaveDebitStateResp =
                (i, com) => CreateAndSaveResp(request.PaymentId, i, com, "GetDebitState", null);

            try
            {
                var initialRequest = LogReqState(request);

                Guid paymentId;
                if (!Guid.TryParse(request.PaymentId, out paymentId))
                    return createAndSaveDebitStateResp(2, "Incorrect parameter: PaymentId");

                string wrongParameter;
                if (!request.CheckParametersRegex(out wrongParameter))
                    return createAndSaveDebitStateResp(2, "Incorrect parameter: " + wrongParameter);

                var payment =
                    _storage.Payments.SingleOrDefault(p => p.Id == paymentId && p.Partner.Code == request.PartnerCode);
                if (payment == null)
                    return createAndSaveDebitStateResp(1, "Unknown pair (PaymentId, PartnerCode)");

                if (!request.CheckHashCode(payment.Partner.Password))
                    return createAndSaveDebitStateResp(4, "Incorrect hash-value");

                if (payment.Status != null && payment.Status.IsTerminal)
                    return CreateAndSaveResp(request.PaymentId, payment.Status.PartnerCode, payment.Status.PartnerDescription,
                                             "GetDebitState", null);

                var mobiRequest = new MobicomStatusRequest();
                mobiRequest.version = payment.Merchant.Bank.ProtocolVersion;
                mobiRequest.Owner = new Owner { id = payment.OwnerId };
                mobiRequest.Agregator = new Agregator { id = payment.Merchant.Bank.AgregarorId };
                mobiRequest.hash = GetHash(mobiRequest.Owner.id + payment.Merchant.Bank.Password);
                LogMobicomStatusRequest(mobiRequest, initialRequest);

                var mobiClient = new mobicomTypeClient();
                var response = mobiClient.MobicomStatusRequestOperation(mobiRequest);

                return CreateNLogStateResponse(payment, response);
            }
            catch (Exception e)
            {
                Trace.WriteLine("Ошибка в GetDebitState: " + e);
                return createAndSaveDebitStateResp(13, "UniplatService internal error");
            }
            finally
            {
                TrySaveChanges();
            }
        }

        /// <summary>
        /// Формирует ответ клиенту на основании ответа полученного от Мобикома
        /// </summary>
        /// <param name="response">Полученный от Мобикома ответ</param>
        /// <param name="payment">Платеж, состояние которого запрашивает клиент</param>
        /// <returns></returns>
        private Resp CreateNLogStateResponse(UniplatServiceData.Payment payment, MobicomStatusResponse response)
        {
            var paymentId = payment.Id.ToString();
            MobicomMessage initialResponse = LogMobicomStatusResponse(response);
            Func<int, string, Resp> createAndSaveDebitStateResp =
                (i, com) => CreateAndSaveResp(paymentId, i, com, "GetDebitState", initialResponse);

            string name = "InProcess";
            int code = 0;

            // Если ошибка в запросе
            if (response.Result.code.HasValue && response.Result.code.Value != 0)
            {
                name = "ProtocolError";
                code = response.Result.code.Value;
                payment.StatusDescription = response.Result.comment;
            }
            // Если ошибка при инициации платежа
            else if (response.Start.Result.code.HasValue && response.Start.Result.code.Value != 0)
            {
                name = null;
                code = response.Start.Result.code.Value;
                payment.StatusDescription = response.Start.Result.comment;
            }
            // Если установлен статус платежа
            else if (response.Payment.result.HasValue)
            {
                name = null;
                code = response.Payment.result.Value;
            }

            var status = GetOrCreateStatus(name, code, response.Client.Phone.provider);

            payment.Status = status;
            payment.StatusTime = DateTime.Now;

            return createAndSaveDebitStateResp(status.PartnerCode, status.PartnerDescription);
        }

        private Status GetOrCreateStatus(string name, int code, int? provider)
        {
            Status status;
            if (!string.IsNullOrEmpty(name))
                status = _storage.Status.SingleOrDefault(s => s.Name == name && s.Code == code);
            else
            {
                var pId = provider ?? 0;
                status = _storage.Status.SingleOrDefault(s => s.Code == code && s.ProviderId == pId);
            }

            if (status == null)
            {
                status = _storage.Status.CreateObject();
                status.Id = Guid.NewGuid();
                status.Name = name;
                status.Code = code;

                // Статус с ошибкой оператора
                if (name == null)
                {
                    status.ProviderId = provider;
                    status.PartnerDescription = "Ошибка на стороне провайдера";
                }
                // Статус об ошибке в сообщении
                else
                {
                    status.ProviderId = null;
                    status.PartnerDescription = "Ошибка связи с провайдером";
                }
                status.Description = "Доселе неизвестный код ошибки";
                status.PartnerCode = -1;

                _storage.Status.AddObject(status);
            }

            return status;
        }

        private void TrySaveChanges()
        {
            try
            {
                _storage.SaveChanges();
            }
            catch (Exception e)
            {
                Trace.WriteLine("UniplatServiceWCF", "Ошибка сохранения данных в БД : " + e);
                EventLog.WriteEntry("UniplatServiceWCF", "Ошибка сохранения данных в БД : " + e, EventLogEntryType.Error);
                _storage = new UPSEntities();
            }
        }

        #endregion

        #region Log Client Packets

        /// <summary>
        /// Создет ответ для клиента и сохраняет его в БД
        /// </summary>
        /// <param name="paymentId">Идентификатор платежа обработанного в запросе</param>
        /// <param name="resultCode">Целочисленный результат обработки платежа</param>
        /// <param name="resultComment">Комментарий к результату обработки</param>
        /// <param name="method">Метод сервиса(DebitMoney или GetDebitState), в ответ на который формируется Resp</param>
        /// <param name="initialResponse">Запсись в логе сообщений протокола Mobicom, на основании которой сформирован данный ответ</param>
        /// <returns></returns>
        private Resp CreateAndSaveResp(string paymentId, int resultCode, string resultComment, string method, MobicomMessage initialResponse)
        {
            var response = new Resp() { PaymentId = paymentId, ResultCode = resultCode, ResultComment = resultComment };

            LogResp(response, method, initialResponse);

            return response;
        }

        /// <summary>
        /// Запиывает в БД запрос ReqDebit клиента
        /// </summary>
        /// <param name="request">Переданный клиентом запрос</param>
        /// <returns>Добавленная в БД запись</returns>
        private ClientMessage LogReqDebit(ReqDebit request)
        {
            var logEntry = _storage.ClientMessages.CreateObject();

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "DebitMoney";
            logEntry.Action = "Request";
            logEntry.PaymentId = Truncate(request.PaymentId, 50);
            logEntry.PaymentNumber = Truncate(request.PaymentNumber, 10);
            logEntry.PartnerCode = Truncate(request.PartnerCode, 3);
            logEntry.MSISDN = Truncate(request.MSISDN, 10);
            logEntry.Amount = Truncate(request.Amount.ToString(), 10);
            logEntry.Comment = Truncate(request.Comment, 50);
            logEntry.OperatorCode = Truncate(request.OperatorCode, 3);
            logEntry.ServiceId = Truncate(request.ServiceId, 50);
            logEntry.Hash = Truncate(request.Hash, 50);

            _storage.ClientMessages.AddObject(logEntry);

            return logEntry;
        }

        /// <summary>
        /// Запиывает в БД запрос ReqState клиента
        /// </summary>
        /// <param name="request">Переданный клиентом запрос</param>
        /// <returns>Добавленная в БД запись</returns>
        private ClientMessage LogReqState(ReqState request)
        {
            var logEntry = _storage.ClientMessages.CreateObject();

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "GetDebitState";
            logEntry.Action = "Request";
            logEntry.PaymentId = Truncate(request.PaymentId, 50);
            logEntry.PartnerCode = Truncate(request.PartnerCode, 3);
            logEntry.Hash = Truncate(request.Hash, 50);

            _storage.ClientMessages.AddObject(logEntry);

            return logEntry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resp">Ответ отправляемый пользователю</param>
        /// <param name="method">Метод сервиса(DebitMoney или GetDebitState), в ответ на который формируется Resp</param>
        /// <param name="initialMessage">Запсись в таблице MobicomMessages, на основании которой сформирован данный ответ</param>
        /// <returns></returns>
        private ClientMessage LogResp(Resp resp, string method, MobicomMessage initialMessage)
        {
            var logEntry = _storage.ClientMessages.CreateObject();
            logEntry.MobicomMessage = initialMessage;

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = method;
            logEntry.Action = "Response";
            logEntry.PaymentId = Truncate(resp.PaymentId, 50);
            logEntry.ResultCode = Truncate(resp.ResultCode.ToString(), 10);
            logEntry.ResultComment = Truncate(resp.ResultComment, 50);

            _storage.ClientMessages.AddObject(logEntry);

            return logEntry;
        }

        /// <summary>
        /// Создает и инициализирует объект Payment по поступившему от клиента запросу
        /// </summary>
        /// <param name="request">Переданный клиентом запрос</param>
        /// <returns>Объект частично описывающий совершаемый платеж</returns>
        private UniplatServiceData.Payment FillPaymentWithRequest(ReqDebit request)
        {
            var payment = _storage.Payments.CreateObject();
            payment.PaymentNumber = request.PaymentNumber;
            payment.MSISDN = request.MSISDN ?? "NULL";
            payment.Amount = request.Amount;
            payment.Currency = CurrencyId;
            payment.SMSComment = request.Comment;
            payment.MessageTime = DateTime.Now;

            return payment;
        }

        #endregion

        #region Log Mobicom Packets

        /// <summary>
        /// Используется для обрезания строк до нужной длины. Entity кидает исключение если этого не делать
        /// </summary>
        static private string Truncate(string s, int length)
        {
            return s == null ? null : s.Substring(0, Math.Min(s.Length, length));
        }

        /// <summary>
        /// Логирует отправляемый Мобикому запрос на инициализацию платежа
        /// </summary>
        /// <param name="request">Отправлемый запрос</param>
        /// <param name="initialMessage">Запись в таблице ClientMessages, на оснолвании которой сформирован отправляемый запрос</param>
        /// <returns>Заведенная в БД запись</returns>
        private MobicomMessage LogMobicomStartRequest(MobicomStartRequest request, ClientMessage initialMessage)
        {
            var logEntry = _storage.MobicomMessages.CreateObject();
            logEntry.ClientMessage = initialMessage;

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "MobicomStartRequestOperation";
            logEntry.Action = "Request";
            logEntry.AgreagatorId = Truncate(request.Agregator.id.ToString(), 10);
            logEntry.MerchantId = Truncate(request.Merchant.id.ToString(), 10);
            logEntry.OwnerId = Truncate(request.Owner.id, 50);
            logEntry.ClientPhoneNumber = Truncate(request.Client.Phone.number, 10);
            logEntry.ClientPhoneProvider = Truncate(request.Client.Phone.provider.ToString(), 3);
            logEntry.PaymentAmount = Truncate(request.Payment.amount.ToString(), 10);
            logEntry.PaymentCurrency = Truncate(request.Payment.currency.ToString(), 3);
            logEntry.MessageBill = Truncate(request.Message.bill, 10);
            logEntry.MessageComment = Truncate(request.Message.comment, 50);
            logEntry.Hash = Truncate(request.hash, 50);
            logEntry.Version = Truncate(request.version, 10);

            _storage.MobicomMessages.AddObject(logEntry);

            return logEntry;
        }

        /// <summary>
        /// Логирует полученный от Мобикома ответ на запрос инициализации платежа
        /// </summary>
        /// <param name="response">Полученный ответ</param>
        /// <returns>Заведенная в БД запись</returns>
        private MobicomMessage LogMobicomStartResponse(MobicomStartResponse response)
        {
            var logEntry = _storage.MobicomMessages.CreateObject();

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "MobicomStartResponseOperation";
            logEntry.Action = "Response";
            logEntry.OwnerId = Truncate(response.Owner.id, 50);
            logEntry.Version = Truncate(response.version, 10);
            logEntry.ResultCode = Truncate(response.Result.code.ToString(), 10);
            logEntry.ResultComment = Truncate(response.Result.comment, 255);

            _storage.MobicomMessages.AddObject(logEntry);

            return logEntry;
        }

        /// <summary>
        /// Логирует отправляемый Мобикому запрос состояния платежа
        /// </summary>
        /// <param name="request">Отправлемый запрос</param>
        /// <param name="initialMessage">Запись в таблице ClientMessages, на оснолвании которой сформирован отправляемый запрос</param>
        /// <returns>Заведенная в БД запись</returns>
        private MobicomMessage LogMobicomStatusRequest(MobicomStatusRequest request, ClientMessage initialMessage)
        {
            var logEntry = _storage.MobicomMessages.CreateObject();
            logEntry.ClientMessage = initialMessage;

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "MobicomStatusRequestOperation";
            logEntry.Action = "Request";
            logEntry.AgreagatorId = Truncate(request.Agregator.id.ToString(), 10);
            logEntry.OwnerId = Truncate(request.Owner.id, 50);
            logEntry.Hash = Truncate(request.hash, 50);
            logEntry.Version = Truncate(request.version, 10);

            _storage.MobicomMessages.AddObject(logEntry);

            return logEntry;
        }

        /// <summary>
        ///  Логирует полученный от Мобикома ответ на запрос состояния платежа
        /// </summary>
        /// <param name="response">Полученный ответ</param>
        /// <returns>Заведенная в БД запись</returns>
        private MobicomMessage LogMobicomStatusResponse(MobicomStatusResponse response)
        {
            var logEntry = _storage.MobicomMessages.CreateObject();

            logEntry.Id = Guid.NewGuid();
            logEntry.CreateTime = DateTime.Now;
            logEntry.Method = "MobicomStatusResponseOperation";
            logEntry.Action = "Response";
            logEntry.AgreagatorId = Truncate(response.Agregator.id.ToString(), 10);
            logEntry.MerchantId = Truncate(response.Merchant.id.ToString(), 10);
            logEntry.OwnerId = Truncate(response.Owner.id, 50);
            logEntry.ClientPhoneNumber = Truncate(response.Client.Phone.number, 10);
            logEntry.ClientPhoneProvider = Truncate(response.Client.Phone.provider.ToString(), 3);
            logEntry.PaymentAmount = Truncate(response.Payment.amount.ToString(), 10);
            logEntry.PaymentCurrency = Truncate(response.Payment.currency.ToString(), 3);
            logEntry.PaymentResult = Truncate(response.Payment.result.ToString(), 10);
            logEntry.ResultCode = Truncate(response.Result.code.ToString(), 10);
            logEntry.ResultComment = Truncate(response.Result.comment, 255);
            logEntry.StartResultCode = Truncate(response.Start.Result.code.ToString(), 10);
            logEntry.StartResultComment = Truncate(response.Start.Result.comment, 255);
            logEntry.ReserveResultCode = Truncate(response.Reserve.Result.code.ToString(), 10);
            logEntry.ReserveResultComment = Truncate(response.Reserve.Result.comment, 255);
            logEntry.RegisterResultCode = Truncate(response.Register.Result.code.ToString(), 10);
            logEntry.RegisterResultComment = Truncate(response.Register.Result.comment, 255);
            logEntry.RefundResultCode = Truncate(response.Refund.Result.code.ToString(), 10);
            logEntry.RefundResultComment = Truncate(response.Refund.Result.comment, 255);
            logEntry.Version = Truncate(response.version, 10);

            _storage.MobicomMessages.AddObject(logEntry);

            return logEntry;
        }

        #endregion

        private string GetHash(string sStr)
        {
            byte[] data = Encoding.GetEncoding(1251).GetBytes(sStr);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(data);
            string sResult = Convert.ToBase64String(result);
            return sResult;
        }
    }
}
