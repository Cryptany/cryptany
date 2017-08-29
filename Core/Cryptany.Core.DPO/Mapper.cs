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
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO
{
	public class Mapper
	{
		private string _tableName;
		private EntityFieldsMapper _entityFieldsMapper;
		private Type _entityType;
		private string _schemaName = "";
		private PersistentStorage _ps;

		public Mapper(Type entityType, PersistentStorage ps)
		{
			_ps = ps;
			_entityType = entityType;
			_entityFieldsMapper = new EntityFieldsMapper(EntityType, _ps);
			TableAttribute attr = ClassFactory.GetObjectDescription(_entityType, ps).DbTableAttribute;
			if ( attr == null )
				_tableName = entityType.Name;
			else
				_tableName = attr.TableName;
			DbSchemaAttribute attr2 = ClassFactory.GetObjectDescription(_entityType, ps).GetAttribute<DbSchemaAttribute>();
			if ( attr2 != null )
				_schemaName = attr2.SchemaName;
		}

		public Type EntityType
		{
			get
			{
				return _entityType;
			}
		}

		public string SchemaName
		{
			get
			{
				return _schemaName;
			}
			set
			{
				_schemaName = value;
			}
		}

		public string TableName
		{
			get
			{
				return _tableName;
			}
		}
		
		public string FullTableName
		{
			get
			{
				if ( _schemaName == "" || _schemaName == null )
					return _tableName;
				else
					return SchemaName + "." + _tableName;
			}
		}

		public string this[string fieldName]
		{
			get
			{
				return _entityFieldsMapper[fieldName];
			}
		}

	    public string this[string fieldName,  bool bracketed]
	    {
            get
            {
                if (bracketed) 
                    return "[" + this[fieldName] + "]";
                
                return this[fieldName];
            }
	    }

		public string GetByDbName(string dbName)
		{
			return _entityFieldsMapper.GetByDbName(dbName);
		}

		private class EntityFieldsMapper
		{
			PersistentStorage _ps;
			private readonly Dictionary<string, string> _entityFieldsMapping =
				new Dictionary<string, string>( new StringCaseInsensitiveComparer());
			private readonly Dictionary<string, string> _entityFieldsMappingInverse =
				new Dictionary<string, string>(new StringCaseInsensitiveComparer());

			public EntityFieldsMapper(Type entityType, PersistentStorage ps)
			{
				_ps = ps;
				ObjectDescription od = ClassFactory.GetObjectDescription(entityType, ps);
				foreach ( PropertyDescription prop in od.Properties )
				{
					string dbPropName = "";
					FieldNameAttribute attr = prop.GetAttribute<FieldNameAttribute>();
					if ( attr == null )
						dbPropName = prop.Name;
					else if ( prop.ReflectedObject.GetAttribute<IdFieldNameAttribute>() != null &&
					         prop.ReflectedObject.GetAttribute<IdFieldNameAttribute>() .Name == prop.Name)
						dbPropName = prop.ReflectedObject.GetAttribute<IdFieldNameAttribute>().Name;
					else
						dbPropName = attr.FieldName;
					_entityFieldsMapping.Add(prop.Name, dbPropName);
					_entityFieldsMappingInverse.Add(dbPropName, prop.Name);
				}
			}

			public string this[string fieldName]
			{
				get
				{
					return _entityFieldsMapping[fieldName];
				}
			}

			public string GetByDbName(string dbName)
			{
			    return _entityFieldsMappingInverse.ContainsKey(dbName) ? _entityFieldsMappingInverse[dbName] : null;
			}
		}
	}

}
