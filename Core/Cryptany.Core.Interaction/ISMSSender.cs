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
using System.ServiceModel;
using System.Reflection;


namespace Cryptany.Core.Interaction
{
    /// <summary>
    /// Интерфейс для создания исходящего сообщения и отправки в коннектор
    /// </summary>
    [ServiceContract()]
    [ServiceKnownType(typeof(MessagePriority))]
    public interface ISMSSender
    {

        /// <summary>
        /// простой с тарифом
        /// </summary>
        /// <param name="tariffid"></param>
        /// <param name="msisdn"></param>
        /// <param name="body"></param>
        /// <param name="resourceid"></param>
        /// <param name="parameters"></param>
        /// <param name="errortext"></param>
        /// <returns>id исходящего сообщения</returns>
        [OperationContract (Name="SendByTariff")]
        Guid SendSMS(Guid tariffid, string msisdn, string body, Guid resourceid, Dictionary<string,object> parameters,  out string errortext);


        /// <summary>
        /// простой с использованием номера
        /// </summary>
        /// <param name="SN"></param>
        /// <param name="msisdn"></param>
        /// <param name="body"></param>
        /// <param name="resourceid"></param>
        /// <param name="parameters"></param>
        /// <param name="errortext"></param>
        /// <returns>id исходящего сообщения</returns>
        [OperationContract(Name = "SendByNumber")]
        [ServiceKnownType(typeof(MessagePriority))]
        Guid SendSMS(string SN, string msisdn, string body, Guid resourceid, Dictionary<string, object> parameters,  out string errortext);

    }
}

