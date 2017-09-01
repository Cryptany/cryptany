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
using System.Management.Instrumentation;

namespace Cryptany.Core.Management.WMI
{
    [InstrumentationClass(InstrumentationType.Instance)]
    public class SMPPConnectorState : GenericConnector
    {
        [IgnoreMember]
        public DateTime _lastPDUInTime; //время получения последнего PDU
        [IgnoreMember]
        public DateTime _lastPDUInRespTime; //время последнего ответа на входящий PDU
        [IgnoreMember]
        public DateTime _lastPDUOutTime; //время последней отсылки PDU
        [IgnoreMember]
        public DateTime _lastPDUOutRespTime; //время последнего ответа на исходящий PDU
        
        
        public string LastPDUInTime
        {
            get
            {
                return _lastPDUInTime.ToString();
            }
        }

        public string LastPDUInRespTime
        {
            get
            {
                return _lastPDUInRespTime.ToString();
            }
        }

        public string LastPDUOutTime
        {
            get
            {
                return _lastPDUOutTime.ToString();
            }
        }

        public string LastPDUOutRespTime
        {
            get
            {
                return _lastPDUOutRespTime.ToString();
            }
        }
        
        public SMPPConnectorState()
        {
            _lastPDUInTime = DateTime.MinValue;
            _lastPDUInRespTime = DateTime.MinValue;
            _lastPDUOutTime = DateTime.MinValue;
            _lastPDUOutRespTime = DateTime.MinValue;
            Instrumentation.Publish(this);
                 
        }

        [IgnoreMember]
        public void Close()
        {
            Instrumentation.Revoke(this);
        }
        
    }
}
