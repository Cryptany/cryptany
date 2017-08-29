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
using System.Data.Objects.DataClasses;
using System.Data;
using System.Data.Objects;

namespace Cryptany.Core.DB
{
    internal partial class TransportEntities
    {
        static TransportEntities _entities = null;
        
        /// <summary>
        /// Common class instance. Use instead of constructor
        /// </summary>
        internal static TransportEntities Entities
        {
            get
            {
                if (_entities == null)
                {
                    _entities = new TransportEntities();
                }
                return _entities;
            }
        }

       
    }
}
