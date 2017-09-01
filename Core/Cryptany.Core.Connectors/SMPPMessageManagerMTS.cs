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
using Cryptany.Core.SmppLib;
using System.Text;
using Cryptany.Common.Logging;

namespace Cryptany.Core
{
      public  class SMPPMessageManagerMTS: SMPPMessageManager
    {

          
        public SMPPMessageManagerMTS(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
           
        }


        protected override bool CheckDeliverSM(DeliverSM dlsm, DeliverSMResponse dlsmr)
        {
            string sn = Message.GetServiceNumber(dlsm.Destination.Address);
            string msisdn = dlsm.Source.Address;
            AbonentState abstate = CheckAbonentInBlackList(msisdn, sn);

            switch (abstate)
            {
                case AbonentState.Blocked:
                    dlsmr.StatusCode = PacketBase.commandStatusEnum.ESME_R_MTS_AbonentInBlackList;

                    break;
                case AbonentState.NotBlocked:
                case AbonentState.Unknown:
                    dlsmr.StatusCode = PacketBase.commandStatusEnum.ESME_ROK;
                    break;
          

            }
            return true;
        }
        protected override string InjectMessageBody(string msgText, DeliverSMResponse dlsm_r)
        {
            if (dlsm_r.StatusCode == PacketBase.commandStatusEnum.ESME_R_MTS_AbonentInBlackList)
            {
                return "(blocked) " + msgText;
            }
            return base.InjectMessageBody(msgText, dlsm_r);
        }

    }
}
