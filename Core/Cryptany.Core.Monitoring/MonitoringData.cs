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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Cryptany.Core.Monitoring
{
    internal class MonitoringData
    {
        public Guid id = Guid.Empty;
        public int smscid = 0;
        public DateTime dtReceived = DateTime.MinValue;
        public DateTime dtProcessing = DateTime.MinValue;
        public DateTime dtProcessed = DateTime.MinValue;
        public ushort Count = 0;
        public Hashtable sms = new Hashtable();

    }

    internal class SMSData
    {
        public Guid id = Guid.Empty;
        public int smscid = 0;
        public DateTime dtSubmitted = DateTime.MinValue;
        public DateTime dtSent = DateTime.MinValue;
    }
}
