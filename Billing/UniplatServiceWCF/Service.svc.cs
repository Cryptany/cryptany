using System;
using System.Collections.Generic;

namespace UniplatServiceWCF
{
    public class Service : IService
    {
        private readonly List<string> _comments = new List<string>
            {
                "запрос принят в обработку",
                "нарушен формат параметра: «имя_параметра» («шаблон»)",
                "указаны ошибочные идентификаторы поставщика услуги",
                "ошибка в hash-значении",
                "сервер временно не принимает запросы в связи с высокой нагрузкой. Повторите попытку позже",
                "номер мобильного телефона не опознан, неизвестный провайдер",
                "категория не найдена",
                "ошибка связи с Провайдером"
            };

        private Resp CreateRandomResp()
        {
            var r = new Random();
            return new Resp
                {
                PaymentId = Guid.NewGuid().ToString(),
                ResultCode = r.Next(0, 11),
                Resultcoment = _comments[r.Next(0,7)]
            };
        }

        public Resp DebitMoney(ReqDebit request)
        {
            return CreateRandomResp();
        }

        public Resp GetDebitState(ReqState request)
        {
            return CreateRandomResp();
        }
    }
}
