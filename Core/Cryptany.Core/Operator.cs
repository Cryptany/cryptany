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
using System.Data;

namespace Cryptany
{
	namespace Core
	{
		/// <summary>
		/// Summary description for Operator.
		/// </summary>
		[Serializable]
		public class Operator
		{
            private Guid _dbId;

			public Guid DatabaseId 
			{
				get 
				{
					return _dbId;
				}
			}

            [NonSerialized]
            private string _name;

			public string Name 
			{
				get 
				{
					return _name;
				}
			}

            [NonSerialized]
            private string _displayName;

            public string DisplayName
            {
                get
                {
                    return _displayName;
                }
            }

            private int _zoneIntId;

            public int ZoneIntId
            {
                get
                {
                    return _zoneIntId;
                }
            }

		    private Guid _brandId;

		    public Guid BrandId
		    {
                get { return _brandId; }
		    }
			public Operator()
			{
			}

            public Operator(Guid id)
            {
                CacheDataSet.OperatorsRow row = CoreClassFactory.CreateConfigProvider().CacheDS.Operators.FindById(id);
                if (row != null)
                {
                    _dbId = row.Id;
                    _name = row.Name;
                    if (row["DisplayName"] == DBNull.Value || row["DisplayName"] == null)
                    {
                        _displayName = "Not defined";
                    }
                    else
                    {
                        _displayName = row.DisplayName;
                    }
                    // ID зоны оператора
                    _brandId = row.BrandId;
                    _zoneIntId = GetOperatorZoneId(id);
                }
            }

            /// <summary>
			/// Создаёт объект по известному ID в базе
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
            public static Operator GetByID(Guid id)
            {
                Operator result = null;
                if (id != Guid.Empty)
                {
                    CacheDataSet.OperatorsRow row = CoreClassFactory.CreateConfigProvider().CacheDS.Operators.FindById(id);
                    if (row != null)
                    {
                        result = new Operator();
                        result._dbId = row.Id;
                        result._name = row.Name;
                        if (row["DisplayName"] == DBNull.Value || row["DisplayName"] == null)
                        {
                            result._displayName = "Not defined";
                        }
                        else
                        {
                            result._displayName = row.DisplayName;
                        }
                        // ID зоны оператора
                        result._zoneIntId = GetOperatorZoneId(id);
                    }
                }
                return result;
            }

            
            public List<SMSC> GetSMSCs()
            {
                List<SMSC> res = new List<SMSC>();
                CacheDataSet.OperatorsRow row = CoreClassFactory.CreateConfigProvider().CacheDS.Operators.FindById(DatabaseId);
                CacheDataSet.SMSC2OpRow[] smsc2ops = row.GetSMSC2OpRows();
                foreach(CacheDataSet.SMSC2OpRow smsc2op in smsc2ops)
                {
                    res.Add(new SMSC(smsc2op.SMSCId));
                }
                return res;
            }
            private static int GetOperatorZoneId(Guid id)
            {
                // Определить целочисленный идентификатор зоны оператора для B2B 
                int zoneIntId = 0;
                DataRow[] drOpZone = CoreClassFactory.CreateConfigProvider().CacheDS.OperatorsToZones.Select("OperatorId = '" + id.ToString() + "'");
                if (drOpZone.Length > 0)
                {
                    // есть зона, связанная с оператором
                    DataRow[] drOpZoneId = CoreClassFactory.CreateConfigProvider().CacheDS.OperatorZones.Select("Id = '" + drOpZone[0]["ZoneId"].ToString() + "'");
                    if (drOpZoneId.Length > 0)
                    {
                        zoneIntId = Convert.ToInt32(drOpZoneId[0]["IntId"]);
                    }
                }
                return zoneIntId;
            }

            public struct OperatorSerializationInfo
            {
                public Guid   _ID;
                public string _Name;
                public string _DisplayName;
                public int    _ZoneIntId;
            }

            public OperatorSerializationInfo SerializationInfo
            {
                get
                {
                    OperatorSerializationInfo result;
                    result._ID          = _dbId;
                    result._Name        = _name;
                    result._DisplayName = _displayName;
                    result._ZoneIntId   = _zoneIntId;
                    return result;
                }
                set
                {
                    _dbId        = value._ID;
                    _name        = value._Name;
                    _displayName = value._DisplayName;
                    _zoneIntId   = value._ZoneIntId;
                }
            }
        }
	}
}