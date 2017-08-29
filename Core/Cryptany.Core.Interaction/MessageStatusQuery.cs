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
using System.Text;

namespace Cryptany.Core.Interaction
{
    [Serializable]
   public class MessageStatusQuery
    {

        public string SMSCMsgId;

        public string source_addr;

        //public Guid outboxId;

        //public MessageStatusQuery( string smscMsgId, string source_addr)
        //{

  
        //    this.SMSCMsgId = smscMsgId;
        //    this.source_addr = source_addr;

        //}
        public override string ToString()
        {
            return string.Format("Message State Query: source_addr {0} smscMsgId  {1}", source_addr, SMSCMsgId);
        }


    }
}
