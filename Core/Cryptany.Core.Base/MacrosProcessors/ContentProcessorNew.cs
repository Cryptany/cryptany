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
using System.Diagnostics;
using System.Messaging;
using Cryptany.Core.Base.Management;

using Cryptany.Foundation.ContentDelivery;
using Cryptany.Common.Utils;
using Cryptany.Core.DB;
using Cryptany.Core;
using Cryptany.Core.Interaction;
using Cryptany.Core.MsmqLog;

namespace Cryptany.Core.Base.MacrosProcessors
{
    public class ContentProcessor : MacrosProcessor
    {
        private readonly Guid ContragentResource;
        internal ContentProcessor(Guid contragentResource)
        {

            ContragentResource = contragentResource;
            
        }
        private static void AddToDownloadOutbox(Guid outboxid, Guid downloadid)
        {

            
            MSMQLogEntry me = new MSMQLogEntry();
            me.DatabaseName = Database.DatabaseName;
            me.CommandText = "kernel.AddToDownloadsOutbox";
            me.Parameters.Add("@outboxid", outboxid);
            me.Parameters.Add("@downloadid", downloadid);
            using (MessageQueue msmqLoggerInputQueue = ServiceManager.MSMQLoggerInputQueue)
            {
                msmqLoggerInputQueue.Send(me);
            }

        }

        private string GetDownload(OutputMessage msg, IDictionary<string, string> parameters)
        {

            int code;
            string result;
            Guid downloadId;
            if (parameters.ContainsKey("code"))
            {
                if (!int.TryParse(parameters["code"], out code))
                    throw new Exception("Неверный код контента " + code);
            }
            else if (parameters.ContainsKey("#default#"))
            {
                if (!int.TryParse(parameters["#default#"], out code))
                    throw new Exception("Неверный код контента " + code);
            }
            else
                throw new Exception("Невозможно сгенерировать ссылку. Не задан код контента.");

            
            try
            {
                
                result = GetContentDownloadLink(code, (Guid)ContragentResource, msg.Destination, out downloadId);
            }
            catch (Exception ex)
            {
                
                throw new Exception("Не удалось сгенерировать ссылку на скачку. Подробная информация записана в логе сервиса генерации ссылок.", ex);
            }
            if (string.IsNullOrEmpty(result)||downloadId==Guid.Empty)
            {
                throw new Exception("Не удалось сгенерировать ссылку на скачку. Подробная информация записана в логе сервиса генерации ссылок.");

            }
           
           AddToDownloadOutbox(msg.ID, downloadId);
           return result;
        }
        private static string GetContentDownloadLink(int contentCatalogId, Guid resourceId, string msisdn, out Guid downloadId)
        {
            string link;
            IContentDeliveryManager manager = Remoting.GetObject<IContentDeliveryManager>();
            downloadId = manager.GetContentDownloadLink(out link, msisdn, contentCatalogId, resourceId,
                                                new Guid("e7bbc079-6b59-4863-bb72-90fda2f93025"), Guid.Empty);
            return link;
        }

        public override string Execute(OutputMessage msg, Dictionary<string, string> parameters)
        {
            Trace.WriteLine("Router: генерируем ссылку новым сервисом");
            return GetDownload(msg, parameters);
        }


        public override string MacrosName
        {
            get { return "CONTENT"; }
        }
    }
}
