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
using System.Diagnostics;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.DPO.Predicates;
namespace Cryptany.Core.MacrosProcessors
{

     public  class HelpDeskProcessor : MacrosProcessor
    {



        public override string Execute(OutputMessage msg, Dictionary<string, string> parameters)
        {
            Trace.WriteLine("Router: обрабатываем макрос с контактами техподдержки");
            return GetContacts(msg,parameters);
        }



        private string GetContacts(OutputMessage msg, Dictionary<string, string> parameters)
     {
     

         ServiceNumber sn = ServiceNumber.GetServiceNumberBySN(Message.GetServiceNumber(msg.Source));//отбросить транзакцию
         UnaryOperation<Tariff> u = delegate(Tariff tt)
         {
             return tt.SN == sn && tt.IsActive;
         };
         List<Tariff> t = ChannelConfiguration.DefaultPs.GetEntitiesByPredicate(u);
         if (t.Count == 0) return "";//нет активного тарифа - не даем номер СТП


         string contacts="";

         if (parameters.ContainsKey("smstext") && (!String.IsNullOrEmpty(parameters["smstext"])))
         {
             contacts = parameters["smstext"];
         }
         else if (parameters.ContainsKey("#default#") && (!String.IsNullOrEmpty(parameters["#default#"])))
         {
             contacts = parameters["#default#"];
         }

         return contacts;

     }


        public override string MacrosName
        {
            get { return "HELPDESK"; }
        }



    }
}
