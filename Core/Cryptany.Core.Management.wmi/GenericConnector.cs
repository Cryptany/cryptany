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
    public enum ConnectorState
    {
        Idle,
        Connecting,
        Connected,
        Error
    }

    [InstrumentationClass(InstrumentationType.Abstract)]
    public class GenericConnector 
    {
        /// <summary>
        /// SMSC code  
        /// </summary>
        public int Code;
        /// <summary>
        /// SMSC id  
        /// </summary>
        public string ID;
        /// <summary>
        /// SMSC name  
        /// </summary>
        public string Name;
        /// <summary>
        /// Queue name  
        /// </summary>
        public string QueueName
        {
            get
            {
                return "cryptany_outputqueue" + ID;
            }
        }
        /// <summary>
        /// Кол-во принятых СМС 
        /// </summary>
        public int IncomingMessagesCount;
        /// <summary>
        /// Кол-во отосланных СМС 
        /// </summary>
        public int OutgoingMessagesCount;
        /// <summary>
        /// Кол-во доставленных СМС (если включена опция) 
        /// </summary>
        public int DeliveredMessagesCount;
        /// <summary>
        /// Статус коннектора (работает, перелинкуется, ошибка и т.п.)
        /// </summary>
        public string State;
        /// <summary>
        /// Описание статуса
        /// </summary>
        public string StateDescription;
        /// <summary>
        /// Тип коннектора 
        /// </summary>
        public string Type;
        /// <summary>
        /// Адрес SMSC 
        /// </summary>
        public string Address;
        /// <summary>
        /// Порт SMSC 
        /// </summary>
        public int Port;

        [IgnoreMember]
        public DateTime _lastSMSInTime;
        /// <summary>
        /// Дата/время последнего полученного СМС 
        /// </summary>
        public string LastSMSInTime
        {
            get
            {
                return _lastSMSInTime.ToString();
            }
        }
        
        /// <summary>
        /// Время последнего (пере)подсоединения к SMSC 
        /// </summary>
        public string LastConnectDate;
        /// <summary>
        /// Строка текущих настроек
        /// </summary>
        public string Settings;


        [IgnoreMember]
        public DateTime _lastSentToRouterTime;
        /// <summary>
        /// Время последней отсылки смски в роутер
        /// </summary>
        public string LastSentToRouterTime
        {
            get
            {
                return _lastSentToRouterTime.ToString();
            }
        }

        [IgnoreMember]
        public DateTime _lastReceivedFromRouterTime;
        /// <summary>
        /// Время последнего получения смски из роутера/transpormanager
        /// </summary>
        public string LastReceivedFromRouterTime
        {
            get
            {
                return _lastReceivedFromRouterTime.ToString();
            }
        }

        [IgnoreMember]
        public DateTime _lastResendTime;
        /// <summary>
        /// Время последнего перепосыла смски в SMSC
        /// </summary>
        public string LastResendTime
        {
            get
            {
                return _lastResendTime.ToString();
            }
        }
        
        public GenericConnector()
        {
            Code = 0;
            ID = Guid.Empty.ToString();
            IncomingMessagesCount = 0;
            OutgoingMessagesCount = 0;
            DeliveredMessagesCount = 0;
            State = ConnectorState.Idle.ToString();
            Type = "None";
            Address = "localhost";
            Port = 0;
            _lastSMSInTime = DateTime.MinValue;
            LastConnectDate = "";
            Settings = "";
            
            //Instrumentation.Publish(this);                        
        }

        
    }

    [InstrumentationClass(InstrumentationType.Event)]
    public class ConnectorErrorEvent : BaseEvent
    {
        /// <summary>
        /// ID коннектора, в котором произошла ошибка
        /// </summary>
        public int ID;
        /// <summary>
        /// Тип ошибки (Error, Critical, Info, Debug)
        /// </summary>
        public string ErrorSeverity;
        /// <summary>
        /// Описание ошибки
        /// </summary>
        public string ErrorDescription;
        /// <summary>
        /// Время возникновения ошибки
        /// </summary>
        public string ErrorTime;

    }

    


}
