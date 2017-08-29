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
using System.Data;

namespace Cryptany.Core.DPO.MetaObjects
{
	[Serializable]
	public class ObjectDescription
	{
		private Type _type;
		private PropertyDescriptionCollection _properties;
		private PropertyDescriptionCollection _allProperties;
		private PropertyDescriptionCollection _relations;
        private PropertyDescriptionCollection _mappedProperties;
		private PropertyDescriptionCollection _oneToOneProperties;
		private PropertyDescriptionCollection _oneToManyProperties;
		private PropertyDescriptionCollection _manyToManyProperties;
        private Attribute[] _attributes = null;
		private bool _isEntity = false;
		//private ObjectFactoryDescription _factoryDescription;
		private PropertyDescription _idField;
		private Type _idFiledType;

		private bool _isAggregated;
		private string _caption;
		private bool _isWrapped;
		private Type _wrappedClass;

		private bool _isVirtual;
		private bool _isDenyGetAllOnGetById = false;
		private string _getAllMethodName;
		private PersistentStorage _ps;
		private TableAttribute _tableAttribute = null;

		public ObjectDescription(Type type, PersistentStorage ps)
		{
            if (ps == null)
                throw new ApplicationException("PS is null");
            _ps = ps;
			_type = type;
			_properties = new PropertyDescriptionCollection(this);
			_allProperties = new PropertyDescriptionCollection(this);
			_relations = new PropertyDescriptionCollection(this);
			_mappedProperties = new PropertyDescriptionCollection(this);
			_oneToOneProperties = new PropertyDescriptionCollection(this);
			_oneToManyProperties = new PropertyDescriptionCollection(this);
			_manyToManyProperties = new PropertyDescriptionCollection(this);

			Configuration.EntityConfiguration ec = null;
			if (ps.PsComfiguration != null && ps.PsComfiguration.Entities.ContainsKey(type.Name))
			{
				ec = ps.PsComfiguration.Entities[type.Name];
			}

			foreach (PropertyInfo prop in _type.GetProperties())
			{
				Configuration.EntityPropertyConfiguration epc = null;
				if (ec != null&&ec.Properties.ContainsKey(prop.Name))
					epc = ec.Properties[prop.Name];
				PropertyDescription pd = new PropertyDescription(this, prop, epc);

				//// This is incorrect
				if ( pd.Name == "Item" )
					continue;
				////

				_allProperties.Add(pd);
				if (!pd.IsInternal)
					_properties.Add(pd);
				if (pd.IsRelation)
					_relations.Add(pd);
				if (pd.IsMapped)
					_mappedProperties.Add(pd);
				if (pd.IsId)
					_idField = pd;
				if ( pd.IsManyToManyRelation )
					_manyToManyProperties.Add(pd);
				if ( pd.IsOneToManyRelation )
					_oneToManyProperties.Add(pd);
				if ( pd.IsOneToOneRelation )
					_oneToOneProperties.Add(pd);
			}

			_attributes = new Attribute[_type.GetCustomAttributes(false).Length];
			for (int i = 0; i < _type.GetCustomAttributes(false).Length; i++)
				_attributes.SetValue(_type.GetCustomAttributes(false)[i], i);
			_isEntity = IsDescendentFrom(typeof(IEntity));
			//_factoryDescription = new ObjectFactoryDescription(new DefaultObjectFactory());

			if (ec != null && ec.Attributes.ContainsKey("AgregatedClass"))
				_isAggregated = Convert.ToBoolean(ec.Attributes["AgregatedClass"]);
			else
				_isAggregated = GetAttribute<AgregatedClassAttribute>() != null;

			if ( ec != null && ec.Attributes.ContainsKey("DenyLoadAllOnGetById") )
				_isDenyGetAllOnGetById = true;
			else _isDenyGetAllOnGetById = GetAttribute<DenyLoadAllOnGetByIdAttribute>() != null;

			if (ec != null && ec.Attributes.ContainsKey("Table"))
			{
				if (string.IsNullOrEmpty(ec.Attributes["Table"].Paramaters))
					throw new Exception("Invalid TableAttribute format");
				string[] param = ec.Attributes["Table"].Paramaters.Split(',');
				if (param.Length == 1)
					_tableAttribute = new TableAttribute(param[0]);
				else if (param.Length == 4)
				{
					ConditionOperation op;
					if (param[2] == "Equals")
						op = ConditionOperation.Equals;
					else
						op = ConditionOperation.NotEquals;
					_tableAttribute = new TableAttribute(param[0], param[1], op, param[3]);
				}
				else
					throw new Exception("Invalid TableAttribute format");
			}
			else
				_tableAttribute = GetAttribute<TableAttribute>();

			IdFieldTypeAttribute idTypeAttr = GetAttribute<IdFieldTypeAttribute>();
			if (idTypeAttr == null)
				_idFiledType = typeof(Guid);
			else
				_idFiledType = idTypeAttr.IdType;

			ClassCaptionAttribute attr = GetAttribute<ClassCaptionAttribute>();
			if (ec != null && ec.Attributes.ContainsKey("ClassCaption"))
				_caption = ec.Attributes["ClassCaption"].Paramaters;
			else
			{
				if (attr != null)
					_caption = attr.Caption;
				else
					_caption = type.Name;
			}

			WrappedClassAttribute wattr = GetAttribute<WrappedClassAttribute>();
			if (wattr != null)
			{
				if (!IsDescendentFrom(typeof(IWrapObject)))
					throw new Exception("A class marked with WrappedClass attribute must implement the IWrapObject interface");
				_isWrapped = true;
				_wrappedClass = wattr.WrappedType;
			}
			else
				_isWrapped = false;

			VirtualObjectAttribute vattr = GetAttribute<VirtualObjectAttribute>();
			if (vattr != null)
			{
				_isVirtual = true;
				_getAllMethodName = vattr.GetAllMethodName;
			}


			//foreach ( PropertyDescription pd in Properties )
			//{
			//    DataColumn col = new DataColumn(pd.Name, pd.PropertyType);
			//    //if ( col.DataType == typeof(int) )
			//    //    col.DefaultValue = default(int);
			//    //else if ( col.DataType == typeof(double) )
			//    //    col.DefaultValue = default(double);
			//    //else if ( col.DataType == typeof(byte) )
			//    //    col.DefaultValue = default(byte);
			//    //else if ( col.DataType == typeof(char) )
			//    //    col.DefaultValue = default(char);
			//    //else if ( col.DataType == typeof(DateTime) )
			//    //    col.DefaultValue = default(DateTime);
			//    //else if ( col.DataType == typeof(Guid) )
			//    //    col.DefaultValue = default(Guid);
			//    //else if ( col.DataType == typeof(float) )
			//    //    col.DefaultValue = default(float);
			//    //else if ( col.DataType == typeof(uint) )
			//    //    col.DefaultValue = default(uint);
			//    //else if ( col.DataType == typeof(bool) )
			//    //    col.DefaultValue = default(bool);
			//    //else if ( col.DataType == typeof(string) )
			//    //    col.DefaultValue = default(string);
			//    //else if ( col.DataType.IsEnum )
			//    //    col.DefaultValue = col.DataType.
			//    //else if ( col.DataType.IsEnum )
			//    //    col.DefaultValue = default(int);
			//    col.DefaultValue = pd.PropertyDefaultValue;
			//    Table.Columns.Add(col);
			//}
		}

		public Type ObjectType
		{
			get
			{
				return _type;
			}
		}

		public PropertyDescriptionCollection Properties
		{
			get
			{
				return _properties;
			}
		}

		public PropertyDescriptionCollection Relations
		{
			get
			{
				return _relations;
			}
		}

        /// <summary>
        /// Iterated through all the mapped properties, that is properties
        /// that are not relations, internal, non-persistent. Id-property is
        /// inculded.
        /// </summary>
        public PropertyDescriptionCollection MappedProperties
        {
            get
            {
                return _mappedProperties;
            }
        }

		public PropertyDescriptionCollection OneToOneProperties
		{
			get
			{
				return _oneToOneProperties;
			}
		}

		public PropertyDescriptionCollection OneToManyProperties
		{
			get
			{
				return _oneToManyProperties;
			}
		}

		public PropertyDescriptionCollection ManyToManyProperties
		{
			get
			{
				return _manyToManyProperties;
			}
		}
		
		public Attribute[] Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public Type IdFiledType
		{
			get
			{
				return _idFiledType;
			}
		}

		public bool IsEntity
		{
			get
			{
				return _isEntity;
			}
		}

		public PropertyDescription IdField
		{
			get
			{
				return _idField;
			}
		}

		public bool IsAggregated
		{
			get
			{
				return _isAggregated;
			}
		}

		public string Caption
		{
			get
			{
				return _caption;
			}
		}

		public ObjectFactoryDescription FactoryDescription
		{
			get
			{
				return ClassFactory.GetObjectFactory(this._type);
			}
		}
		
		//public void AssignFactory(IObjectFactory objectFactory)
		//{
		//    _factoryDescription = new ObjectFactoryDescription(objectFactory);
		//}

		//public void AssignFactory(DefaultConstruction defaultConstruction,
		//    ConstructionWithArgs constructionWithArgs)
		//{
		//    _factoryDescription = new ObjectFactoryDescription(defaultConstruction, constructionWithArgs);
		//}

		public bool IsDenyGetAllOnGetById
		{
			get
			{
				return _isDenyGetAllOnGetById;
			}
		}

		public object CreateObject()
		{
            object o = FactoryDescription.InvokeDefaultConstruction(_type);
            if (o is EntityBase)
            {
                (o as EntityBase).CreateNew();
                if (_ps == null) 
                    throw new ApplicationException("PS IS NULL");
                (o as EntityBase).CreatorPs = _ps;
            }
		    return o as EntityBase;
		}

		public object CreateObject(params object[] args)
		{
			object o = FactoryDescription.InvokeConstructionWithArgs(_type, args);
            if ( o is EntityBase )
            {
                (o as EntityBase).CreateNew();
                if (_ps == null)
                    throw new ApplicationException("PS IS NULL");
                (o as EntityBase).CreatorPs = _ps;
            }
			return o as EntityBase;
		}

		public Attribute GetAttribute(Type t)
		{
			foreach ( object attr in _attributes )
				if ( attr.GetType() == t )
					return attr as Attribute;
			return null;
		}

		public T GetAttribute<T>()
		{
			foreach ( object attr in _attributes )
				if ( attr.GetType() == typeof(T) )
					return (T)attr;
			return default(T);
		}

		public bool IsWrapped
		{
			get
			{
				return _isWrapped;
			}
		}

		public Type WrappedClass
		{
			get
			{
				return _wrappedClass;
			}
		}

		public bool IsVirtual
		{
			get
			{
				return _isVirtual;
			}
			set
			{
				_isVirtual = value;
			}
		}

		public string GetAllMethodName
		{
			get
			{
				return _getAllMethodName;
			}
			set
			{
				_getAllMethodName = value;
			}
		}

		public TableAttribute DbTableAttribute
		{
			get
			{
				return _tableAttribute;
			}
		}

	    public bool IsDescendentFrom(Type type)
		{
			//return _type.IsSubclassOf(type);
			return type.IsAssignableFrom(_type);
		}

		public override string ToString()
		{
			return string.Format("Type: {0}", this._type.Name);
		}

		public virtual string ToString(bool caption)
		{
			if ( !caption )
				return ToString();
			else
				return Caption;
		}
	}
}
