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


namespace Cryptany.Core.Interaction
{
    [Serializable]
    public enum MessageDeliveryStatus
    {
        Unknown = 0,
        Undelivered = 1,
        Delivered = 2,
        LowBalance = 3,
        AbonentNotExists = 4,
        ServiceNotAvailable = 5,
        UnknownAccount = 6
    }

}