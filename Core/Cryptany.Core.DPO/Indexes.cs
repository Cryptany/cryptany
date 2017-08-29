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
using System.Diagnostics;
using System.Text;
using System.Collections.Specialized;

namespace Cryptany.Core.DPO
{
	public class Indexes
	{
		private HybridDictionary _indexes = new HybridDictionary();
		private List<object> _properties = new List<object>();

		public Indexes()
		{
		}

		public object[] Keys
		{
			get
			{
				return _properties.ToArray();
			}
		}

		public Dictionary<object, List<EntityBase>> this[string propertyName]
		{
			get
			{
				if ( !_indexes.Contains(propertyName) )
				{
					Dictionary<object, List<EntityBase>> index = new Dictionary<object, List<EntityBase>>();
					_indexes.Add(propertyName, index);
					_properties.Add(propertyName);
				}
				return (Dictionary<object, List<EntityBase>>)_indexes[propertyName];
			}
		}

		public List<EntityBase> GetEntities(string propertyName, object key)
		{
			return this[propertyName][key];
		}
	}

	public class IndexSet<T> where T : EntityBase
	{
		private Dictionary<string, Dictionary<object, List<T>>> _indexes = new Dictionary<string, Dictionary<object, List<T>>>();
		private string[] _indexedFiledsNames;

		public IndexSet(string[] fieldsNames)
		{
			_indexedFiledsNames = fieldsNames;
			if ( _indexedFiledsNames == null || _indexedFiledsNames.Length == 0 )
				return;
			foreach ( string s in _indexedFiledsNames )
				_indexes.Add(s, new Dictionary<object, List<T>>());
		}

		public void Clear()
		{
			foreach ( string s in _indexedFiledsNames )
				_indexes[s].Clear();
		}

		public void Add(T e)
		{
            foreach (string s in _indexedFiledsNames)
            {
                
                //Debug.Assert(e[s] == null, "USSD: значение свойства с именем '" + s + "' пусто");
                if (e[s] != null)
                {
                    if (_indexes[s].ContainsKey(e[s]))
                        _indexes[s][e[s]].Add(e);
                    else
                    {
                        _indexes[s][e[s]] = new List<T>();
                        _indexes[s][e[s]].Add(e);
                    }
                }
            }
		}

		public void Delete(T e)
		{
			foreach ( string s in _indexedFiledsNames )
				if ( _indexes[s].ContainsKey(e[s]) )
					if ( _indexes[s][e[s]].Contains(e) )
						_indexes[s][e[s]].Remove(e);
		}

		public Dictionary<object, List<T>> this[string fieldName]
		{
			get
			{
				return _indexes[fieldName];
			}
		}
	}
}
