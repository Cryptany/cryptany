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
using Cryptany.Core.Interaction;
using Cryptany.Core;
using Cryptany.Common.Logging;
using Cryptany.Core.SmppLib;

namespace Cryptany.Core.Connectors 
{
    public class SMPPMessageManagerVASMedia : SMPPMessageManager
    {
        public SMPPMessageManagerVASMedia(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
           
        }
        protected override MessageDeliveryStatus GetMessageStateString(PacketBase.commandStatusEnum code)
        {
            switch(code)
            {
                case PacketBase.commandStatusEnum.ESME_RINVSUBSCRIBER:
                case PacketBase.commandStatusEnum.ESME_RINVDSTADR:
                    return MessageDeliveryStatus.Undelivered;
                default:
                    return base.GetMessageStateString(code);
            }
            
        }
    }
}
