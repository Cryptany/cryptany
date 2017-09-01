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
using Cryptany.Common.Logging;
using Microsoft.Practices.EnterpriseLibrary.Caching;

namespace Cryptany.Core
{
       public   class SMPPMessageManagerBWC : SMPPMessageManagerPayloadSender
    {

           public SMPPMessageManagerBWC(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
           
        }

           protected override void InitReceitsCache()
           {
               base.InitReceitsCache();

               msgsWaitingReceits.refreshener = new NoReceitsRemover(this);
              
           }


          
    }
       [Serializable]
       public class NoReceitsRemover : ICacheItemRefreshAction
       {
           SMPPMessageManager _smppmm;

           public NoReceitsRemover(SMPPMessageManager SMPPmm)
           {
               _smppmm = SMPPmm;

           }



           public void Refresh(string removedKey,
            Object expiredValue,
            CacheItemRemovedReason removalReason)
           {

               if (removalReason == CacheItemRemovedReason.Expired)
               {
                   _smppmm.UpdateTerminalDelivery((OutputMessage)expiredValue, removedKey, "Timeout", DateTime.Now, _smppmm.ConnectorId);
               }
           }
       }

    

}
