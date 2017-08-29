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
using Cryptany.Core.DPO.MetaObjects.Attributes;
using System.Collections.Specialized;

namespace Cryptany.Core.DPO
{
	[Serializable]
	public class CachesContainer
	{
        private readonly HybridDictionary _caches = new HybridDictionary();
		private readonly List<Type> _keys = new List<Type>();
		private readonly PersistentStorage _ps;

		public CachesContainer(PersistentStorage ps)
		{
			_ps = ps;
		}

		public Type[] Keys
		{
			get
			{
				return _keys.ToArray();
			}
		}

		public EntityCache this[Type type]
		{
			get
			{
				if ( !_caches.Contains(type) )
				{
					EntityCache cache = new EntityCache(type);
					_caches.Add(type, cache);
					_keys.Add(type);
				}
				return (EntityCache)_caches[type];
			}
		}

        public List<EntityBase> GetEntities(Type type)
		{
			if ( this[type] == null )
			{
				EntityCacheTypeAttribute attr = ClassFactory.GetObjectDescription(type, _ps).GetAttribute<EntityCacheTypeAttribute>();
				if ( attr == null )
					_caches.Add(type, new EntityCache(type));
				else
					_caches.Add(type, new EntityCache(attr.EntityCacheType));
			}
			//List<Entity> list = new List<Entity>();
			return this[type].GetAll();
			//return list;
		}

		public List<T> GetEntities<T>()
		{
			Type type = typeof(T);
			if ( this[type] == null )
			{
				EntityCacheTypeAttribute attr = ClassFactory.GetObjectDescription(type, _ps).GetAttribute<EntityCacheTypeAttribute>();
				if ( attr == null )
					_caches.Add(type, new EntityCache(type));
				else
					_caches.Add(type, new EntityCache(attr.EntityCacheType));
			}
			return this[type].GetAll<T>();
		}

        public EntityBase GetEntityById(Type type, object id)
		{
			return this[type].GetById(id);
		}

		public T GetEntityById<T>(object id)
		{
			Type type = typeof(T);
			return this[type].GetById<T>(id);
		}

		public void Refresh(ILoader loader)
		{
			if ( !_caches.Contains(loader.ReflectedType) )
				_caches.Add(loader.ReflectedType, new EntityCache(loader.ReflectedType));
			(_caches[loader.ReflectedType] as EntityCache).Refresh(loader);
		}

		public void Clear()
		{
			_caches.Clear();
		}
	}
}
