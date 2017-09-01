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
using DataSetLib;
using DataSetLib.CacheDataSetTableAdapters;

namespace Cryptany
{
	namespace Core
	{
		/// <summary>
		/// ����� ������� (���������������)
		/// </summary>
		[Serializable]
		public class Region
		{
            private Guid _dbId;
			/// <summary>
			/// ID ������� � ����
			/// </summary>
			public Guid DatabaseId 
			{
				get 
				{
					return _dbId;
				}
			}

            [NonSerialized]
            private string _name;

			/// <summary>
			/// �������� �������
			/// </summary>
			public string Name 
			{
				get 
				{
					return _name;
				}
			}

			/// <summary>
			/// ����������� ��-���������
			/// </summary>
			public Region()
			{
			}

			/// <summary>
			/// ��������� �����, ��������� ����� �� ���������� �������������� ������� � ����.
			/// </summary>
			/// <param name="id"></param>
			/// <returns></returns>
            public static Region GetByID(Guid id)
            {
                Region result = null;
                if (id != Guid.Empty)
                {
                    CacheDataSet.RegionsRow row = CoreClassFactory.CreateConfigProvider().CacheDS.Regions.FindByID(id);
                    if (row != null)
                    {
                        result = new Region();
                        result._dbId = row.ID;
                        result._name = row.NODE_NAME;
                    }
                }
                return result;
            }

			/// <summary>
			/// ��������������� ��������� ��� ������������ �������
			/// </summary>
            public struct RegionSerializationInfo
            {
                public Guid _ID;
                public string _Name;
            }

			/// <summary>
			/// 
			/// </summary>
            public RegionSerializationInfo SerializationInfo
            {
                get
                {
                    RegionSerializationInfo result;
                    result._ID = _dbId;
                    result._Name = _name;
                    return result;
                }
                set
                {
                    _dbId = value._ID;
                    _name = value._Name;
                }
            }
        }
	}
}