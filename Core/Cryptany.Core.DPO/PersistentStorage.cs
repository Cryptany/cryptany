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
using System.Data;
using System.Reflection;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects.DynamicEntityBuilding;
using Cryptany.Core.DPO.Predicates;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.DS;

namespace Cryptany.Core.DPO
{
	[Serializable]
	public class PersistentStorage : MarshalByRefObject
	{
        private enum PsMode
        {
            DataSet,
            MsSql,
			Xml
        }

		public enum PsImportMode
		{
			PreserveEntitiesStates,
			SetEntitiesStatesToNew
		}

		private DataSet _dataSet;
		private StorageAccessMediator _storageAccessMediator;
		private CachesContainer _caches = null;
		private Dictionary<Type, ILoader> _loaders = new Dictionary<Type, ILoader>();
		private Dictionary<Type, ISaver> _savers = new Dictionary<Type, ISaver>();
		private Dictionary<Type, Cache<object, EntityBase>> _underLoading = new Dictionary<Type, Cache<object, EntityBase>>();
		private Dictionary<string, Type> _customEntities = new Dictionary<string, Type>();
	    private static int i = 0;
	    private List<Type> _loadedTypes = new List<Type>();
		private Dictionary<Type, FacadeBuilder> _entityFacades = new Dictionary<Type, FacadeBuilder>();
        private PsMode _mode;
        private string _connectionString;
		private string _fileName;
		private string _entitiesTagPath;
		private Configuration.Configuration _configuration;
		private bool _loadAllOnGetById = true;

		public FacadeBuilder GetFacadeBuilder(Type entityType)
        {
            if ( !_entityFacades.ContainsKey(entityType) )
            {
                FacadeBuilder builder = new FacadeBuilder(entityType, this);
                _entityFacades.Add(entityType, builder);
            }
            return _entityFacades[entityType];
       }

		public PersistentStorage(DataSet dataSet, string configuration)
		{
			_dataSet = dataSet;
            _mode = PsMode.DataSet;
			LoadConfiguration(configuration);
			Init();
		}

		public PersistentStorage(string fileName, string entitiesTagPath, string configuration)
		{
			_fileName = fileName;
			_entitiesTagPath = entitiesTagPath;
			_mode = PsMode.Xml;
			LoadConfiguration(configuration);
			Init();
		}

		public PersistentStorage(Adapter adapter)
		{
			Init();
		}

		public PersistentStorage(string connectionString, string configuration)
        {
            _mode = PsMode.MsSql;
            _connectionString = connectionString;
			LoadConfiguration(configuration);
			Init();
			_storageAccessMediator = new MsSqlStorageAccessMediator(this, new System.Data.SqlClient.SqlConnection(connectionString));
		}

		private void LoadConfiguration(string configuration)
		{
			if (string.IsNullOrEmpty(configuration))
				return;
			System.IO.StringReader r = new System.IO.StringReader(configuration);
			System.Xml.XmlReader reader = System.Xml.XmlReader.Create(r);
			reader.ReadToFollowing("Entities");
			_configuration = Configuration.Configuration.Create(reader);
		}

		private void Init()
		{
			_caches = new CachesContainer(this);
		}

		public Configuration.Configuration PsComfiguration
		{
			get
			{
				return _configuration;
			}
		}

		public DataSet UnderlyingDataSet
		{
			get
			{
				return _dataSet;
			}
		}

		internal CachesContainer Caches
		{
			get
			{
				return _caches;
			}
		}

		public bool LoadAllOnGetById
		{
			get
			{
				return _loadAllOnGetById;
			}
			set
			{
				_loadAllOnGetById = value;
			}
		}

		public Type GetCustomEntityType(string name)
		{
			if ( !_customEntities.ContainsKey(name) )
			{
                UnaryOperation<EntityBase> u = delegate(EntityBase e)
				{
					return ( e as DynamicEntityBuilder ).Name == name;
				};
                List<EntityBase> l = Functions.Select<EntityBase>(GetEntities(typeof(DynamicEntityBuilder)), u);
				if ( l.Count == 0 )
					return null;
				
                _customEntities.Add(name, (l[0] as DynamicEntityBuilder).Type);
			}
			return _customEntities[name];
		}

        public List<EntityBase> GetEntities(Type type)
        {
			if ( !ClassFactory.GetObjectDescription(type, this).IsAggregated )
			{
				ILoader loader = GetLoader(type);
				List<EntityBase> list = Caches.GetEntities(loader.ReflectedType);
				if ( (list == null || list.Count == 0) && !_loadedTypes.Contains(type) )
				{
					{
						if ( ClassFactory.GetObjectDescription(type, this).IsVirtual )
							list =
								(List<EntityBase>)
								type.GetMethod(ClassFactory.GetObjectDescription(type, this).GetAllMethodName,
											   BindingFlags.Static).Invoke(null, new object[] { this });
						else
							list = loader.LoadAll();
						_loadedTypes.Add(type);
					}
				}
				if ( list == null || list.Count == 0 )
					list = new List<EntityBase>();
				return list;
			}
			else
			{
				AgregatedClassAttribute aggr =
					ClassFactory.GetObjectDescription(type, this).GetAttribute<AgregatedClassAttribute>();
				List<EntityBase> l = new List<EntityBase>();
				foreach ( Type t in aggr.ChildTypes )
				{
					List<EntityBase> templist = GetEntities(t);
					if ( templist != null && templist.Count > 0 )
						l.AddRange(templist);
				}
				return l;
			}
        }

		public List<T> GetEntities<T>() where T : EntityBase
		{
			Type type = typeof(T);
			ObjectDescription objDescr = ClassFactory.GetObjectDescription(type, this);
			if ( !objDescr.IsAggregated )
			{
				ILoader loader = GetLoader(type);
				List<T> list = Caches.GetEntities<T>();
				if ( (list == null || list.Count == 0) && !_loadedTypes.Contains(type) )
				{
					{
						if ( objDescr.IsVirtual )
						{
							List<EntityBase> l =
								(List<EntityBase>)
								type.GetMethod(objDescr.GetAllMethodName,
											   BindingFlags.Static).Invoke(null, new object[] { this });
							list = l.ConvertAll<T>(
									delegate(EntityBase e)
									{
										return e as T;
									}
								);
						}
						else
							list = loader.LoadAll<T>();
						_loadedTypes.Add(type);
					}
				}
				if ( list == null || list.Count == 0 )
					list = new List<T>();
				return list;
			}
			else
			{
				AgregatedClassAttribute aggr =
					objDescr.GetAttribute<AgregatedClassAttribute>();
				List<T> l = new List<T>();
				foreach ( Type t in aggr.ChildTypes )
				{
					List<EntityBase> templist = GetEntities(t);
					if ( templist != null && templist.Count > 0 )
						foreach ( T ee in templist )
							l.Add(ee);
				}
				return l;
			}
		}

        public List<EntityBase> GetEntitiesByPredicate(Type type, UnaryOperation<EntityBase> op)
        {
            return Functions.Select<EntityBase>(GetEntities(type), op);
        }

		public List<T> GetEntitiesByPredicate<T>(UnaryOperation<T> op) where T : EntityBase
		{
			return Functions.Select<T>(GetEntities<T>(), op);
		}

        public List<EntityBase> GetEntitiesByFieldValue(Type type, string fieldName, object value)
        {
            UnaryOperation<EntityBase> op = delegate(EntityBase e)
            {
				if (e[fieldName] != null)
				{
				    return e[fieldName].Equals(value);
						
				}
				else
					return value == null;
            };
            return Functions.Select<EntityBase>(GetEntities(type), op);
        }

		public List<T> GetEntitiesByFieldValue<T>(string fieldName, object value) where T : EntityBase
		{
			UnaryOperation<T> op = delegate(T e)
			                           {
			                               return e!=null && e[fieldName] != null ? e[fieldName].Equals(value) : value == null;
			                           };
			return Functions.Select<T>(GetEntities<T>(), op);
		}

		public T GetOneEntityByFieldValue<T>(string filedName, object value) where T : EntityBase
		{
			T el =	Functions.SelectFirst(GetEntities<T>(),
					delegate(T e)
					    {
					        return e!=null && e[filedName]!=null && e[filedName].Equals(value);
					    }
			    );
			return el;
		}

		public T GetEntityById<T>(object id) where T : EntityBase
		{
			EntityBase en = GetEntityById(typeof(T), id);
			return en as T;
		}

        public EntityBase GetEntityById(Type type, object id)
		{
			if ( id == null )
				return null;
			ILoader loader = GetLoader(type);
			if ( !ClassFactory.GetObjectDescription(type, this).IsAggregated )
			{
                EntityBase e = Caches.GetEntityById(loader.ReflectedType, id);
                if ( e == null )
					{
						if ( _loadAllOnGetById && !ClassFactory.GetObjectDescription(type, this).IsDenyGetAllOnGetById )
						{
						    List<EntityBase> temp = GetEntities(type);
						    e = Caches[loader.ReflectedType].Contains(id) ? Caches[loader.ReflectedType][id] : null;
						}
						else
						{
						    
						    e = loader.LoadEntityById(id);
						}
					}
				return e;
			}
			else
			{
				AgregatedClassAttribute aggr = ClassFactory.GetObjectDescription(type, this).GetAttribute<AgregatedClassAttribute>();
                List<EntityBase> l = new List<EntityBase>();
				foreach ( Type t in aggr.ChildTypes )
				{
					EntityBase e = GetEntityById(t, id);
					if ( e != null )
						return e;
				}
				return null;
			}
		}

		public bool IsLoaded(Type type)
		{
			return _loadedTypes.Contains(type);
		}

        public delegate void EntityProc(EntityBase e);

        public void ForAll(UnaryOperation<EntityBase> predicate, EntityProc proc)
		{
			foreach ( Type t in Caches.Keys )
			{
                foreach ( EntityBase e in Caches[t].Values )
					if ( predicate(e) )
						proc(e);
			}
		}

		public List<EntityBase> Select(UnaryOperation<EntityBase> predicate)
		{
			List<EntityBase> list = new List<EntityBase>();
			foreach (Type t in Caches.Keys)
			{
				foreach (EntityBase e in Caches[t].Values)
					if (predicate(e))
						list.Add(e);
			}
			return list;
		}

        public void ReloadEntity(EntityBase entity)
		{
			GetLoader(entity.GetType()).ReloadEntity(entity);
		}

        public void Save(EntityBase entity)
		{
			DateTime dt1 = DateTime.Now;
			
            SourceStorageAccessMediator.Reset();
			SaveInner(entity);
            SourceStorageAccessMediator.EndCollectAndFlushData();
			DateTime dt2 = DateTime.Now;
			TimeSpan ts = dt2 - dt1;
		}

		internal void SaveInner(EntityBase entity)
		{
			ISaver saver = GetSaver(entity.GetType());
			saver.Save(entity);
		}

		public void Save(List<EntityBase> entities)
		{
			SaveInner(entities);
		}

		internal void SaveInner(List<EntityBase> entities)
		{
			foreach ( EntityBase e in entities )
				SaveInner(e);
		}

		public ILoader GetLoader(Type type)
		{
            lock (_loaders)
            {
                if (_loaders.ContainsKey(type))
                    return _loaders[type];
                ILoader loader;
                switch (_mode)
                {
                    case PsMode.DataSet:
                        loader = new DataSetLoader(this, type, _dataSet);
                        break;
                    case PsMode.MsSql:
                        loader = new Sql.MsSqlDataLoader(this, _connectionString, type);
                        break;
                    case PsMode.Xml:
                        loader = new Xml.XmlDataLoader(_fileName, _entitiesTagPath, this, type);
                        break;
                    default:
                        throw new Exception("Undefined mode");
                }
                _loaders.Add(type, loader);
                return loader;
            }
		}

		public ISaver GetSaver(Type type)
		{
            lock (_savers)
            {
                if (_savers.ContainsKey(type))
                    return _savers[type];
                ISaver saver;
                if (_mode == PsMode.DataSet)
                    saver = new DataSetSaver(this, type, _dataSet);
                else if (_mode == PsMode.MsSql)
                    saver = new Sql.MsSqlDataSaver(this, type);
                else if (_mode == PsMode.Xml)
                    saver = new Xml.XmlDataSaver(_fileName, _entitiesTagPath, this, type);
                else
                    throw new Exception("Undefined mode");
                _savers.Add(type, saver);
                return saver;
            }
		}

		internal StorageAccessMediator SourceStorageAccessMediator
		{
            get { return _storageAccessMediator; }
		}

		public void Dispose()
		{
			Caches.Clear();
			_loadedTypes.Clear();
			_loaders.Clear();
			_savers.Clear();
			_underLoading.Clear();
			_entityFacades.Clear();
			_customEntities.Clear();
		}
	}
}
