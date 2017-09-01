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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Cryptany.Core.Router.Data;
using Cryptany.Common.Utils;
using Cryptany.Core.Monitoring;

namespace Cryptany.Core.Router.Data
{
    partial class TVService
    {
        Regex[] _Tokens;

        /// <summary>
        /// Набор регулярных вражений для определения принадлежнасти сообщения ТВ сервису
        /// </summary>
        public Regex[] Tokens
        {
            get
            {
                if (_Tokens == null)
                {
                    _Tokens = (from r in DBCache.RegularExpressions.Values
                                   where r.TokenId == this.TokenId
                                   select new Regex(r.Regex)).ToArray();
                }
                return _Tokens;
            }
        }

        /// <summary>
        /// Список сервисных блоков для данного сообщения
        /// </summary>
        List<ServiceBlock> ServiceBlocks{
            get
            {
                return (from sb in DBCache.ServiceBlocks.Values
                       where sb.ServiceId == this.Id
                       select sb).ToList();
            }
        }

        /// <summary>
        ///  Сервисный номер
        /// </summary>
        public string ServiceNumber
        {
            get
            {
                if (ServiceNumberId.HasValue)
                    return DBCache.ServiceNumbers[ServiceNumberId.Value].SN;
                return null;
            }
        }

        /// <summary>
        ///  Токен
        /// </summary>
        public string TokenName
        {
            get
            {
                return DBCache.Tokens[TokenId.Value].Name;
            }
        }

        /// <summary>
        ///  Тип сервисного блока
        /// </summary>
        public ServiceTypeValue Type
        {
            get
            {
                return DBCache.ServiceTypes.Values.Where(st => st.Id == this.ServiceTypeId).First().Value;
            }
        }

        /// <summary>
        /// Определяет ошибочный для данного сервисного номера
        /// </summary>
        /// <param name="sn">Сервисный номер</param>
        /// <returns>Требуемый ошибочный сервис</returns>
        public static TVService ErrorService(string sn)
        {
            // Ищем все ошибочные сервисы для данного номера
            var es = DBCache.Services.Values.Where(s => s.Type == ServiceTypeValue.ErrorService && s.ServiceNumber == sn);
            if (es.Count() > 0)
                return es.First();
            //  Если их нет, то возвращаем глобальный ошибочный
            return GlobalErrorService;
        }

        //public TVService ErrorService
        //{
        //    get
        //    {
        //        return ErrorService(ServiceNumber);
        //    }
        //}

        static public TVService GlobalErrorService
        {
            get
            {
                return DBCache.Services.Values.First(s => s.Type == ServiceTypeValue.GlobalErrorService);
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        public TVService() { }

        /// <summary>
        /// Подбирает сервисный блок для обработки данного сообщения
        /// </summary>
        /// <param name="msg">Обрабатываемое сообщение</param>
        /// <returns></returns>
        public ServiceBlock ChooseServiceBlock(Message msg)
        {
            // Выбираем все подходящие сервисные блоки
            var res = (from b in ServiceBlocks
                       where b.Check(msg)
                       select b).ToList();

            int cnt = res.Count();
            // Ла-ла-ла
            if (cnt > 1)
            {
                string sblocks = res.ConcatStrings(s => s.Name, ';');
                string errorMsg = "Для сообщения " + msg + " подходит не единственный сервисный блок. Невозможно выбрать между (" + sblocks + ")";
                Functions.AddEvent("Неоднозначный сервис", errorMsg, EventType.Debug);
            }

            // Ла-ла-ла
            if (cnt == 0)
            {
                Functions.AddEvent("Нет подходящих сервисных блоков",
                                   "Для сообщения " + msg + " в " + Name +
                                   " нет подходящих сервисных блоков. Обработка передана ошибочному сервису.",
                                   EventType.Info);
                return ErrorService(msg.ServiceNumberString).ServiceBlocks.First();
            }

            return res.First();
        }

        /// <summary>
        /// Для ошибочного ТВ сервиса возвращает его единственный сервисный блок.
        /// </summary>
        public ServiceBlock DefaultServiceBlock
        {
            get
            {
                if (Type == ServiceTypeValue.TVService)
                    throw new InvalidOperationException("Свойство предназначено только для ошибочных ТВ сервисов");
                return ServiceBlocks.First();
            }
        }

        /// <summary>
        /// Проверяет сообщение на принадлежность к данному ТВ сервису
        /// </summary>
        /// <param name="msg">Проверяемое сообщение</param>
        /// <returns>true если сообщение подходит</returns>
        public bool Check(Message msg)
        {
            // Проверяем сервисный номер
            if (ServiceNumber != msg.ServiceNumberString)
                return false;
            // 
            foreach (var r in Tokens)
                if (r.IsMatch(msg.Text))
                    // Если подошло хотя бы одно выражение
                    return true;
            // Нет, так нет
            return false;
        }
    }
}