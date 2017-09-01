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
using System.Runtime.Serialization;


namespace Cryptany.Core.Management.WMI
{
    [Serializable]
    public struct MessageState
    {
        /// <summary>
        /// OutboxID
        /// </summary>
        public string ID;
        /// <summary>
        /// Статус доставки
        /// </summary>
        public string Status;
        /// <summary>
        /// Описание статуса
        /// </summary>
        public string StatusDescription;
        /// <summary>
        /// Время изменения статуса
        /// </summary>
        public string StatusTime;
    }

   

    [InstrumentationClass(InstrumentationType.Event)]
    public class MessageStatusChangedEvent : BaseEvent
    {
        /// <summary>
        /// OutboxID
        /// </summary>
        public string ID;
        /// <summary>
        /// Статус доставки
        /// </summary>
        public string Status;
        /// <summary>
        /// Описание статуса
        /// </summary>
        public string StatusDescription;
        /// <summary>
        /// Время изменения статуса
        /// </summary>
        public string StatusTime;

    }
}
