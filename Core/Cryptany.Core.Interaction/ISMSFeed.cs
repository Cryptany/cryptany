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
namespace Cryptany.Core.Interaction
{
    using System;
    /// <summary>
    /// Интерфейс отсылки смс через транспортную систему
    /// </summary>
	public interface ISMSFeed 
	{
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId, int connectorCode,  out string errText);

        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId, int connectorCode,
                         MessagePriority priority, out string errText);
		Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId,
			MessagePriority messagePriority, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid inboxId, Guid resourceId,
            MessagePriority messagePriority, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid inboxId, Guid resourceId,
                         MessagePriority messagePriority, string operatorParameters, out string errText);

        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId, bool asFlash, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId, bool asFlash, int connectorCode, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId,
            MessagePriority messagePriority, bool asFlash, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId, int connectorCode,
                         MessagePriority priority, bool asFlash, out string errText);
        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid inboxId, Guid resourceId,
            MessagePriority messagePriority, bool asFlash, out string errText);

        Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid inboxId, Guid resourceId,
                         MessagePriority messagePriority, string operatorParameters, bool asFlash, out string errText);
        //Guid FeedSMSText(string serviceNumber, string abonentMsisdn, string body, Guid resourceId,
        //                 MessagePriority messagePriority, Guid abonentId, out string errText);

        
	    MessageState GetOutboxMessageState(Guid id);
		string GetPreviewSms(string smsWithMacro, Guid contragentResourceId, string serviceNumber, string msisdn);


        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid tariffId, Guid resourceId,
    MessagePriority messagePriority, out string errText);
        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid tariffId, Guid resourceId, out string errText);

        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid inboxId, Guid tariffId, Guid resourceId,
                                 MessagePriority messagePriority, out string errText);

        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid inboxId, Guid tariffId, Guid resourceId,
                                 out string errText);

        Guid FeedSMSTextByTariff(Guid abonentId, string body, Guid inboxId, Guid tariffId, Guid resourceId,
                                 MessagePriority messagePriority, out string errText);

        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid inboxId, string TransactionId, Guid tariffId,
                                 Guid resourceId, MessagePriority messagePriority, out string errText);


        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid tariffId, Guid resourceId,
         MessagePriority messagePriority, bool asFlash, out string errText);
        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid tariffId, Guid resourceId, bool asFlash, out string errText);

        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid inboxId, Guid tariffId, Guid resourceId,
                                 MessagePriority messagePriority, bool asFlash, out string errText);

        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid inboxId, Guid tariffId, Guid resourceId, bool asFlash,
                                 out string errText);

        Guid FeedSMSTextByTariff(Guid abonentId, string body, Guid inboxId, Guid tariffId, Guid resourceId,
                                 MessagePriority messagePriority, bool asFlash, out string errText);
        Guid FeedSMSTextByTariff(Guid abonentId, string body, Guid inboxId, string TransactionId, Guid tariffId,
                                 Guid resourceId, MessagePriority messagePriority, out string errText);

        Guid FeedSMSTextByTariff(string abonentMsisdn, string body, Guid inboxId, string TransactionId, Guid tariffId,
                                 Guid resourceId, MessagePriority messagePriority, bool asFlash, out string errText);

        Guid FeedSMSTextByTariff(Guid abonentId, string body, Guid inboxId, string TransactionId, Guid tariffId,
                                 Guid resourceId, MessagePriority messagePriority, bool asFlash, out string errText);
  
	}

	
}
