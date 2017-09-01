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
using System.Management.Instrumentation;

namespace Cryptany.Core.Management.WMI
{
    
   // [InstrumentationClass(InstrumentationType.Event)]
    public class SMSReceivedEvent : BaseEvent
    {
        /// <summary>
        /// ID коннектора, в котором произошла ошибка
        /// </summary>
        public string ID;

        public string SMSCId;
        /// <summary>
        /// Тип ошибки (Error, Critical, Info, Debug)
        /// </summary>
        public string Time;
       

    }

    //[InstrumentationClass(InstrumentationType.Event)]
    public class SMSProcessingEvent : BaseEvent
    {
        /// <summary>
        /// ID коннектора, в котором произошла ошибка
        /// </summary>
        public string ID;
        /// <summary>
        /// Тип ошибки (Error, Critical, Info, Debug)
        /// </summary>
        public string Time;

       

    }
   // [InstrumentationClass(InstrumentationType.Event)]
    public class SMSProcessedEvent : BaseEvent
    {
        /// <summary>
        /// ID коннектора, в котором произошла ошибка
        /// </summary>
        public string InboxID;
        /// <summary>
        /// Тип ошибки (Error, Critical, Info, Debug)
        /// </summary>
        public string Time;

        public ushort SMSCount;

    }

   // [InstrumentationClass(InstrumentationType.Event)]
    public class SMSSubmittedEvent : BaseEvent
    {
        /// <summary>
        /// ID коннектора, в котором произошла ошибка
        /// </summary>
        public string InboxID;
        /// <summary>
        /// Тип ошибки (Error, Critical, Info, Debug)
        /// </summary>
        public string Time;

        public string ID;

        public string SMSCId;

    }

    //[InstrumentationClass(InstrumentationType.Event)]
    public class SMSSentEvent : BaseEvent
    {
        /// <summary>
        /// ID коннектора, в котором произошла ошибка
        /// </summary>
        public string InboxID;
        /// <summary>
        /// Тип ошибки (Error, Critical, Info, Debug)
        /// </summary>
        public string Time;

        public string ID;

        public string SMSCId;
    }
    
}
