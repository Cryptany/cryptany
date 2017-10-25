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
using System.Linq;
using System.Text;

namespace Cryptany.Core.DB
{
    public enum ParameterTypeValue
    {
        Resource,
        SN,
        Tariff,
        RegionGroup,
        MacroRegion,
        Brand,
        Unknown
    }

    public partial class ParameterType
    {
        public ParameterTypeValue Value
        {
            get
            {
                ParameterTypeValue res;
                if (Enum.TryParse<ParameterTypeValue>(Name, out res))
                    return res;
                return ParameterTypeValue.Unknown;
            }
        }
    }
}
