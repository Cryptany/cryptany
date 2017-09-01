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
using System.Reflection;
using System.Data;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects;
using System.Threading;

namespace Cryptany.Core.DPO
{
	public static class ClassFactory
	{
		private static Dictionary<object, PersistentStorage> _ps = new Dictionary<object, PersistentStorage>();
		private static Dictionary<string, ObjectDescription> _metaObjects = new Dictionary<string, ObjectDescription>();
		private static Dictionary<string, ObjectFactoryDescription> _objectFactories = new Dictionary<string, ObjectFactoryDescription>();

        public static FacadeBuilder GetFacadeBuilder(Type entityType, PersistentStorage ps)
        {
            return ps.GetFacadeBuilder(entityType);
        }

		public static ObjectDescription GetObjectDescription(Type type, PersistentStorage ps)
		{
            lock (_metaObjects)
            {
                if (!_metaObjects.ContainsKey(type.ToString()))
                    _metaObjects.Add(type.ToString(), new ObjectDescription(type, ps));
                return _metaObjects[type.ToString()];
            }
		}

		public static InstanceDescription GetInstanceDescription(IEntity entity)
		{
			return new InstanceDescription(entity);
		}

		public static void SetObjectFactory(Type type, IObjectFactory factory)
		{
            lock (_objectFactories)
            {
                if (_objectFactories.ContainsKey(type.ToString()))
                    _objectFactories[type.ToString()] = new ObjectFactoryDescription(factory);
                else
                    _objectFactories.Add(type.ToString(), new ObjectFactoryDescription(factory));
            }
		}

		public static void SetObjectFactory(Type type, DefaultConstruction defaultConstruction,
			ConstructionWithArgs constructionWithArgs)
		{
            lock (_objectFactories)
            {
                ObjectFactoryDescription fd = new ObjectFactoryDescription(defaultConstruction, constructionWithArgs);
                if (_objectFactories.ContainsKey(type.ToString()))
                    _objectFactories[type.ToString()] = fd;
                else
                    _objectFactories.Add(type.ToString(), fd);
            }
		}

		public static ObjectFactoryDescription GetObjectFactory(Type type)
		{
            lock (_objectFactories)
            {
                if (!_objectFactories.ContainsKey(type.ToString()))
                    _objectFactories.Add(type.ToString(), new ObjectFactoryDescription(new DefaultObjectFactory()));
                return _objectFactories[type.ToString()];
            }
		}

        //public static void SetThreadDefaultPs(Thread thread, PersistentStorage ps)
        //{
        //    _currentThreadPs[thread] = ps;
        //}

		public static bool PsExists(string key)
		{
            lock (_ps)
            {
                return _ps.ContainsKey(key);
            }
		}

        //public static PersistentStorage GetThreadDefaultPs(Thread thread)
        //{
        //    return _currentThreadPs[thread];
        //}

		public static PersistentStorage CreatePersistentStorage(DataSet dataSet)
		{
			lock ( _ps )
			{
				if ( !_ps.ContainsKey(dataSet) )
					_ps.Add(dataSet, new PersistentStorage(dataSet, null));
                //if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
                //    _currentThreadPs.Add(System.Threading.Thread.CurrentThread, _ps[dataSet]);
                //else
                //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[dataSet];
				return _ps[dataSet];
			}
		}

		public static PersistentStorage CreatePersistentStorage(DataSet dataSet, string configuration)
		{
			lock ( _ps )
			{
				if ( !_ps.ContainsKey(dataSet) )
					_ps.Add(dataSet, new PersistentStorage(dataSet, configuration));
                //if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
                //    _currentThreadPs.Add(System.Threading.Thread.CurrentThread, _ps[dataSet]);
                //else
                //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[dataSet];
				return _ps[dataSet];
			}
		}

		public static PersistentStorage CreatePersistentStorage(string key, DataSet dataSet, string configuration)
		{
			lock ( _ps )
			{
				if ( !_ps.ContainsKey(key) )
					_ps.Add(key, new PersistentStorage(dataSet, configuration));
                //if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
                //    _currentThreadPs.Add(System.Threading.Thread.CurrentThread, _ps[key]);
                //else
                //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
				return _ps[key];
			}
		}

		public static PersistentStorage CreatePersistentStorage(string key, string connectionString)
        {
			lock ( _ps )
			{
				if ( !_ps.ContainsKey(key) )
					_ps.Add(key, new PersistentStorage(connectionString, null));
                //if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
                //    _currentThreadPs.Add(System.Threading.Thread.CurrentThread, _ps[key]);
                //else
                //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
				return _ps[key];
			}
        }

		public static PersistentStorage CreatePersistentStorage(string key, string connectionString, string configuration)
		{
			lock ( _ps )
			{
				if ( !_ps.ContainsKey(key) )
					_ps.Add(key, new PersistentStorage(connectionString, configuration));
                //if ( !_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread) )
                //    _currentThreadPs.Add(System.Threading.Thread.CurrentThread, _ps[key]);
                //else
                //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
				return _ps[key];
			}
		}

		public static PersistentStorage CreatePersistentStorage(string key)
		{
            //if (!_ps.ContainsKey(key))
            //    _ps.Add(key, new PersistentStorage(connectionString));
            //if (_currentThreadPs[System.Threading.Thread.CurrentThread] == null)
            //    _currentThreadPs[System.Threading.Thread.CurrentThread] = _ps[key];
			return _ps[key];
		}

		public static PersistentStorage CreatePersistentStorage(string key, string fileName, string entitiesTagPath, string configuration)
		{
            lock (_ps)
            {
                if (!_ps.ContainsKey(key))
                    _ps.Add(key, new PersistentStorage(fileName, entitiesTagPath, configuration));
                //if (_currentThreadPs.ContainsKey(System.Threading.Thread.CurrentThread))
                //    _currentThreadPs.Add(System.Threading.Thread.CurrentThread, _ps[key]);
                return _ps[key];
            }
		}

		public static PersistentStorage CreatePersistentStorage(IConnection connection)
		{
            lock (_ps)
            {
                if (!_ps.ContainsKey(connection))
                    _ps.Add(connection, new PersistentStorage(new Adapter(connection)));
                return _ps[connection];
            }
		}

		public static void DeletePersistentStorage(DataSet key)
		{
            lock (_ps)
            {
                PersistentStorage ps = _ps[key];
                ps.Dispose();
                _ps.Remove(key);
            }
		}

		public static void DeletePersistentStorage(string key)
		{
            lock (_ps)
            {
                PersistentStorage ps = _ps[key];
                ps.Dispose();
                _ps.Remove(key);
            }
		}

		public static object CreateObject(Type type, PersistentStorage ps)
		{
            lock (_metaObjects)
            {
                if (!_metaObjects.ContainsKey(type.ToString()))
                    _metaObjects.Add(type.ToString(), new ObjectDescription(type, ps));
                return _metaObjects[type.ToString()].CreateObject();
            }
		}

		public static object CreateObject(Type type, PersistentStorage ps, params object[] args )
		{
            lock (_metaObjects)
            {
                if (!_metaObjects.ContainsKey(type.ToString()))
                    _metaObjects.Add(type.ToString(), new ObjectDescription(type, ps));
                return _metaObjects[type.ToString()].CreateObject(args);
            }
		}

		public static T CreateObject<T>(PersistentStorage ps)
		{
            lock (_metaObjects)
            {
                if (!_metaObjects.ContainsKey(typeof (T).ToString()))
                    _metaObjects.Add(typeof (T).ToString(), new ObjectDescription(typeof (T), ps));
                T o = (T) _metaObjects[typeof (T).ToString()].CreateObject();
                (o as EntityBase).CreatorPs = ps;
                return o;
            }
		}

		public static T CreateObject<T>(PersistentStorage ps, params object[] args)
		{
            lock (_metaObjects)
            {
                if (!_metaObjects.ContainsKey(typeof (T).ToString()))
                    _metaObjects.Add(typeof (T).ToString(), new ObjectDescription(typeof (T), ps));
                T o = (T) _metaObjects[typeof (T).ToString()].CreateObject(args);
                (o as EntityBase).CreatorPs = ps;
                return o;
            }
		}
	}
}
