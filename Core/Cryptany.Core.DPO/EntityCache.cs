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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO
{
	[Serializable]
    public class EntityCache : Cache<object, EntityBase>, IEntityCache
	{
		private Type _type;
		//private Dictionary<object, IEntity> _cache = new Dictionary<object, IEntity>();
		private Indexes _rawIndexes = new Indexes();


		public EntityCache(Type type)
		{
			_type = type;
		}

		public Type Type
		{
			get
			{
				return _type;
			}
		}

		public Indexes RawIndexes
		{
			get
			{
				return _rawIndexes;
			}
		}

		//public int Count
		//{
		//    get
		//    {
		//        return _cache.Count;
		//    }
		//}

		//public bool Contains(object id)
		//{
		//    return _cache.ContainsKey(id);
		//}

		//public void Add(object id, Entity item)
		//{
		//    _cache.Add(id, item);
		//}

        public void Add(EntityBase item)
		{
			_cache.Add(item.ID, item);
			foreach (string prop in item.SourceRawValues.Keys)
			{
				if ( !_rawIndexes[prop].ContainsKey(item.SourceRawValues[prop]) )
					_rawIndexes[prop][item.SourceRawValues[prop]] = new List<EntityBase>();
				_rawIndexes[prop][item.SourceRawValues[prop]].Add(item);
			}
		}

		public void Delete(EntityBase item)
		{
			DeleteById(item.ID);
		}

		public void DeleteById(object id)
		{
			_cache.Remove(id);
		}
		
		public List<EntityBase> GetAll()
		{
            List<EntityBase> list =  new List<EntityBase>();
            foreach (EntityBase e in _cache.Values)
                list.Add(e);
            return list;
		}

		public List<T> GetAll<T>()
		{
            
            List<T> list =  new List<T>();
            foreach (T e in _cache.Values)
                list.Add(e);
            return list;
		}

        public EntityBase GetById(object id)
		{
            //if ( !_cache.Contains(id) )
            //    return null;
            //else
				return (EntityBase)_cache[id];
		}

		public T GetById<T>(object id)
		{
            //if ( !_cache.Contains(id) )
            //    return default(T);
            //else
				return (T)_cache[id];
		}

		public void Refresh(ILoader loader)
		{
			this._cache.Clear();
			//loader.LoadAll();
            foreach ( EntityBase entity in loader.LoadAll() )
			{
				//if (!_
				_cache.Add(entity.ID, entity);
			}
		}

        public void Reload(List<EntityBase> list)
		{
			_cache.Clear();
            foreach ( EntityBase e in list )
				_cache.Add(e.ID, e);
		}
	}
}
