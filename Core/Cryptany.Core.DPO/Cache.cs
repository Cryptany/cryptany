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
using System.Text;
using System.Collections.Specialized;

namespace Cryptany.Core.DPO
{
	[Serializable]
	public class Cache<IdType, ValueType> : ICache<IdType, ValueType> where ValueType : EntityBase 
	{
        protected HybridDictionary _cache = new HybridDictionary();
		

		public Cache()
		{
		}

		public int Count
		{
			get
			{
				return _cache.Count;
			}
		}

		public IdType[] Keys
		{
			get
			{
				IdType[] arr = new IdType[_cache.Keys.Count];
				_cache.Keys.CopyTo(arr, 0);
				return arr;
			}
		}

		public ValueType[] Values
		{
			get
			{
				ValueType[] arr = new ValueType[_cache.Values.Count];
				_cache.Values.CopyTo(arr, 0);
				return arr;
			}
		}

		public ValueType this[IdType key]
		{
			get
			{
				//if ( _cache.ContainsKey(key) )
					return (ValueType)_cache[key];
				//else
				//    return default(ValueType);
			}
			set
			{
				_cache[key] = value;
			}
		}

		public bool Contains(IdType id)
		{
			return _cache.Contains(id);
		}

		public void Add(IdType id, ValueType item)
		{
            if (!_cache.Contains(id))
			_cache.Add(id, item);
		}

		public void Remove(IdType id)
		{
			_cache.Remove(id);
		}

        public EntityCollection<ValueType> GetAll() 
		{
            return new EntityCollection<ValueType>(_cache.Values);
            //foreach ( ValueType value in _cache.Values )
            //    list.Add(value);
            //return list;
		}

		public void Clear()
		{
			_cache.Clear();
		}
	}
}
