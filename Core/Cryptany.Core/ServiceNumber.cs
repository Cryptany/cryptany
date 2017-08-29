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
using System.Data;
using DataSetLib;
using DataSetLib.CacheDataSetTableAdapters;

namespace Cryptany
{
	namespace Core
	{
		/// <summary>
		/// ServiceNumber -- class representing service number (ISNN).
		/// </summary>
		[Serializable]
		public class ServiceNumber
		{
            private Guid _dbId;       // database Id
			
			public Guid DatabaseId 
			{
				get 
				{
					return _dbId;
				}
			}

            private string _value;    // actual value

			public string Value
			{
				get
				{
					return _value;
				}
			}

            public ServiceNumber()
            { 
            }

            public ServiceNumber(Guid id)
            {
                _dbId = id;
				CacheDataSet.ServiceNumbersRow row = CoreClassFactory.CreateConfigProvider().CacheDS.ServiceNumbers.FindById(id);
                _value = row.SN;
            }

            public ServiceNumber(string value)
            {
                DataRow[] dr = CoreClassFactory.CreateConfigProvider().CacheDS.ServiceNumbers.Select("SN = '" + value + "'");
                if (dr.Length > 0)
                {
                    _value = dr[0]["SN"].ToString();
                    _dbId  = (Guid) dr[0]["ID"];
                }
                else
                {
                    throw (new ApplicationException("Unknown service number " + value));
                }
            }

            public struct SNSerializationInfo
            {
                public Guid   _ID;
                public string _Value;
            }

            public SNSerializationInfo SerializationInfo
            {
                get
                {
                    SNSerializationInfo result;
                    result._ID    = _dbId;
                    result._Value = _value;
                    return result;
                }
                set
                {
                    _dbId  = value._ID;
                    _value = value._Value;
                }
            }  
        }
	}
}
