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
using System.Xml;

namespace Cryptany.Core
{
    [Serializable]
    public class ConnectorSettings: IComparable<ConnectorSettings>
    {
        
        public Guid ID { get; set; }
        public int Code { get; set; } 
        public string Name { get; set; }
        public string ManagerClass { get; set; }
        public string ProtocolType { get; set; }
        public Guid ServiceId { get; set; }
        public string Settings { get; set; } //путь к xml-файлу настроек коннектора
        
        public int CompareTo(ConnectorSettings other)
        {
            if (Code!=other.Code)
                return -1;
            if (ID != other.ID)
                return -1;
            if (ServiceId != other.ServiceId)
                return -1;
            if (!ProtocolType.Equals(other.ProtocolType, StringComparison.InvariantCultureIgnoreCase))
                return -1;
            if (!ManagerClass.Equals(other.ManagerClass, StringComparison.InvariantCultureIgnoreCase))
                return -1;
            if (!Settings.Equals(other.Settings, StringComparison.InvariantCultureIgnoreCase))
                return -1;
            return 0;
        }
    }

    public abstract class AbstractConnectorSettings
    {
        //public-поля класса?!
        public Guid SMSCId;     //kernel.SMSC.Id
        public int SMSCCode;    //kernel.SMSC.Code
        public Guid ServiceId;  //kernel.SMSC.ServiceId или service из services.Services? 
        
        public string IPAddress;  //хранит то же самое, что и Host. зачем?!
        public string Host;
        public string Port;
        
        public string SystemId;
        public string Password;
        
        public bool UseMessageState;
        public bool UseMonitoring;

        public int MonitoringSleepTime=15000;    //время простоя между циклами
        public int RepeatSendTimeout=300;        //Время между попытками отсылки
        public int RepeatSendCount = 5;          //Количество попыток отсылки
        public bool IsAsyncMode;                 //Асинхронный/нет
        public int MaxTarifficationThreads = 3;  //макс. количество потоков тарификации
        
        public string CBGLogin = "";      //CBG = Content Billing Gateway
        public string CBGPassword = "";

        //useless shit
        protected AbstractConnectorSettings(int SMSCCode)
        {
            
            
        }

        public static AbstractConnectorSettings GetSettings(ConnectorSettings cs)
        {
            AbstractConnectorSettings result = null;

            // TODO: переписать инициализацию настроек коннектора, используя reflection
            switch (cs.ProtocolType)
            {
                case "HTTP":
                    result = new HTTPSettings(cs.Code);
                    break;
                case "SMPP":
                    result = new SMPPSettings(cs.Code);
                    break;
               

            }

            result.SMSCCode = cs.Code;
            result.SMSCId = cs.ID;
            result.ServiceId = cs.ServiceId;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(Convert.ToString(cs.Settings));
            result.InitSettings(xd);
            return result;
        }
        protected abstract void  InitSettings(XmlDocument settings);
    }
}
