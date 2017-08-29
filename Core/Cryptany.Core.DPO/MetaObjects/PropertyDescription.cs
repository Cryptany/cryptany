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

namespace Cryptany.Core.DPO.MetaObjects
{
	[Serializable]
	public class PropertyDescription
	{
		private PropertyInfo _property = null;
		private Attribute[] _attrs = null;
		private bool _isInternal = false;
		private bool _isRelation = false;
		private bool _isOneToOneRelation = false;
		private bool _isOneToManyRelation = false;
		private bool _isManyToManyRelation = false;
		private bool _isReadOnly = false;
		private bool _isNonPersistent = false;
		private bool _isId = false;
		private RelationAttribute _relationAttribute;
		private ObjectDescription _reflectedObject;
		private Type _relatedType;
		private bool _isObligatory = false;
		private string _caption;
	    private object _propertyDefaultValue = null;

        private class DefaultValue<T>
        {
            public T Value = default(T);
        }

		internal PropertyDescription(ObjectDescription objectDescription, PropertyInfo property, Configuration.EntityPropertyConfiguration epc)
		{
			_property = property;
			CreateAttributeArray();
			if ( epc != null && epc.Attributes.Count > 0 )
			{
			}
			else
				foreach ( Attribute attr in _attrs )
				{
					if ( attr is InternalAttribute )
						_isInternal = true;
					if ( attr is RelationAttribute )
					{
						_isRelation = true;
						_relationAttribute = (RelationAttribute)attr;
						if ( (attr as RelationAttribute).RelationType == RelationType.OneToOne )
							_isOneToOneRelation = true;
						if ( (attr as RelationAttribute).RelationType == RelationType.OneToMany )
							_isOneToManyRelation = true;
						if ( (attr as RelationAttribute).RelationType == RelationType.ManyToMany )
							_isManyToManyRelation = true;
						_relatedType = _relationAttribute.RelatedType;
					}
					if ( attr is ReadOnlyFieldAttribute )
						_isReadOnly = true;
					if ( attr is NonPersistentAttribute )
						_isNonPersistent = true;
					if ( attr is ObligatoryFieldAttribute )
						_isObligatory = true;
					if ( attr is CaptionAttribute )
						_caption = (attr as CaptionAttribute).Caption;
				}

			if ( _property.GetCustomAttributes(typeof(IdFieldAttribute), true) != null && _property.GetCustomAttributes(typeof(IdFieldAttribute), true).Length > 0 )
				_isId = true;
			else
				_isId = false;
			foreach ( Attribute attr in objectDescription.ObjectType.GetCustomAttributes(typeof(IdFieldNameAttribute), true) )
				_isId = (attr as IdFieldNameAttribute).Name == Name;

			_reflectedObject = objectDescription;
			if ( IsReadOnly )
				_isObligatory = true;
			if ( string.IsNullOrEmpty(_caption) )
				_caption = Name;

			_propertyDefaultValue = GetDefaultValue(PropertyType);
		}

		public PropertyInfo Property
		{
			get
			{
				return _property;
			}
		}

		public string Name
		{
			get
			{
				return _property.Name;
			}
		}

		public Type PropertyType
		{
			get
			{
				return _property.PropertyType;
			}
		}

		public Attribute[] Attributes
		{
			get
			{
				return _attrs;
			}
		}

		public bool IsInternal
		{
			get
			{
				return _isInternal;
			}
		}

		public bool IsRelation
		{
			get
			{
				return _isRelation;
			}
		}

		public bool IsOneToOneRelation
		{
			get
			{
				return _isOneToOneRelation;
			}
		}

		public bool IsOneToManyRelation
		{
			get
			{
				return _isOneToManyRelation;
			}
		}

		public bool IsManyToManyRelation
		{
			get
			{
				return _isManyToManyRelation;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return _isReadOnly;
			}
		}

		public bool IsNonPersistent
		{
			get
			{
				return _isNonPersistent;
			}
		}

		public bool IsId
		{
			get
			{
				return _isId;
			}
		}

        public bool IsMapped
        {
            get
            {
                return !IsInternal && !IsNonPersistent && !IsManyToManyRelation && !IsOneToManyRelation;
            }
        }

		public Type RelatedType
		{
			get
			{
				return _relatedType;
			}
		}

		public RelationAttribute RelationAttribute
		{
			get
			{
				return _relationAttribute;
			}
		}

		public ObjectDescription ReflectedObject
		{
			get
			{
				return _reflectedObject;
			}
		}

		public bool IsObligatory
		{
			get
			{
				return _isObligatory;
			}
		}

		public string Caption
		{
			get
			{
				return _caption;
			}
		}

        public object PropertyDefaultValue
        {
            get
            {
                return _propertyDefaultValue;
            }
        }

	    private void CreateAttributeArray()
		{
			_attrs = new Attribute[_property.GetCustomAttributes(true).Length];
			for ( int i = 0; i < _attrs.Length; i++ )
				_attrs.SetValue(_property.GetCustomAttributes(true)[i], i);
		}

		public Attribute GetAttribute(Type t)
		{
			foreach ( object attr in _attrs )
				if ( attr.GetType() == t )
					return attr as Attribute;
			return null;
		}

		public T GetAttribute<T>()
		{
			foreach ( object attr in _attrs )
				if ( attr.GetType() == typeof(T) )
					return (T)attr;
			return default(T);
		}

		public object GetValue(object o)
		{
		    return o == null ? null : _property.GetValue(o, null);
		}

	    public T GetValue<T>(object o)
		{
			return (T)_property.GetValue(o, null);
		}

		public object GetValue(object o, params object[] index)
		{
			return _property.GetValue(o, index);
		}

		public T GetValue<T>(object o, params object[] index)
		{
			return (T)_property.GetValue(o, index);
		}

		public void SetValue(object o, object value)
		{
			if ( value == DBNull.Value )
			{
                if ( !_property.PropertyType.IsValueType )
                    value = null;
                else
                    value = GetDefaultValue(_property.PropertyType);
			}
            //try
            //{
				_property.SetValue(o, value, null);
            //}
            //catch
            //{
            //}
		}

		public void SetValue(object o, object value, params object[] index)
		{
			_property.SetValue(o, value, index);
		}

        public List<EntityBase> GetAllRelatedValues(PersistentStorage ps)
		{
			return ps.GetEntities(this.RelatedType);
		}

        private object GetDefaultValue(Type propertyType)
        {
            Type t = typeof (DefaultValue<>);
            t = t.MakeGenericType(propertyType);
            object defaultValue = t.GetConstructor(new Type[0]).Invoke(null);
            return defaultValue.GetType().GetField("Value").GetValue(defaultValue);
        }
	}
}
