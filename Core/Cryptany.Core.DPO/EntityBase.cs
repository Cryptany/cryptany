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
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects;
using System.Data;
using System.Reflection;

namespace Cryptany.Core.DPO
{
    [Serializable]
	public abstract class EntityBase: MarshalByRefObject, IEntity
	{
		protected object _ID;
		private EntityState _state = EntityState.Unchanged;
	    private Dictionary<string, object> _sourceRawValues = new Dictionary<string,object>( new StringCaseInsensitiveComparer());
        private PersistentStorage _ps = null;
		[NonSerialized]		
		private DataRow _sourceRow = null;

		//private PersistentStorage _ps;
		//private InstanceDescription _description;
        //private Dictionary<string, object> _values = new Dictionary<string, object>();
        //private Dictionary<string, object> _oldValues = new Dictionary<string, object>();
        //private DataRow _row = null;
        private readonly ValueCollection _values = new ValueCollection();

        public delegate void PropertyValueSetDelegate(EntityBase entity, PropertyDescription property, object value);
        public delegate void PropertyValueGetDelegate(EntityBase entity, PropertyDescription property, object value);
        public delegate void PropertyValueChangedDelegate(EntityBase entity, PropertyDescription property, object oldValue, object newValue);

        public event PropertyValueSetDelegate PropertyValueSet;
        public event PropertyValueGetDelegate PropertyValueGet;
        public event PropertyValueChangedDelegate PropertyValueChanged;
        
        protected EntityBase()
        {
            //_row = ClassFactory.GetObjectDescription(GetType()).Table.NewRow();
            //foreach ( DataColumn c in _row.Table.Columns )
            //{
            //    if ( _row[c] == DBNull.Value )
            //        _row[c] = c.DefaultValue;
            //}
        }

        [Internal]
        public  PersistentStorage CreatorPs
        {
            get
            {
                return _ps;
            }
            set
            {
                _ps = value;
            }
        }

        [IdField]
		[ReadOnlyField]
		[FieldName("id")]
		public object ID
		{
			get
			{
                IdFieldNameAttribute idattr = ClassFactory.GetObjectDescription(GetType(), CreatorPs).GetAttribute<IdFieldNameAttribute>();
                if (idattr != null && idattr.Name != null)
                    return GetValue<object>(idattr.Name);

                return GetValue<object>("ID");
                
                
				//return _ID;
			}
			set
			{
                IdFieldNameAttribute idattr = ClassFactory.GetObjectDescription(GetType(), CreatorPs).GetAttribute<IdFieldNameAttribute>();
                if (idattr != null && idattr.Name != null)
                    SetValue(idattr.Name, value);
                else
                    SetValue("ID", value);
				//_ID = value;
			}
		}

		[Internal]
		public EntityState State
		{
			get
			{
                //return GetValue<EntityState>("State");
				return _state;
			}
			protected set
			{
                //SetValue("State", _state = value);
				_state = value;
			}
		}

        [Internal]
        internal object this [string key, bool oldValue]
        {
            get
            {
				return GetValueInternal(key, oldValue, false);
            }
            set
            {
                if ( oldValue )
                {
                    throw new Exception("You can not change the old value of the property");
                }
                else
                {
					ObjectDescription d = ClassFactory.GetObjectDescription(GetType(), CreatorPs);
					//ClassFactory.GetThreadDefaultPs(System.Threading.Thread.CurrentThread));
                    //if ( State != EntityState.Unchanged )
                    //{
                        //State = EntityState.Changed;
                        //SetValueToDic(_oldValues, key, value);
                        //if ( PropertyValueChanged != null )
                        //    PropertyValueChanged(this, d.Properties[key], _values[key,true], value);
                    //}
                    //SetValueToDic(_values, key, value);
                    _values[key] = value;
                    if ( PropertyValueSet != null )
                        PropertyValueSet(this, d.Properties[key], value);
                   if (State!= EntityState.New && State!=EntityState.NewDeleted  && State!=EntityState.Deleted)
                    State = EntityState.Changed;
                    
                }
            }
        }

		private object GetValueInternal(string key, bool oldValue, bool fromGetValue)
		{
			ObjectDescription d = ClassFactory.GetObjectDescription(GetType(), CreatorPs);
			//ClassFactory.GetThreadDefaultPs(System.Threading.Thread.CurrentThread));
			object val = null;
			if ( _values.IsUnassignedValue(key) && !fromGetValue )
			{
				val = d.Properties[key].GetValue(this);
				_values[key] = val;
				return val;
			}
			if ( oldValue )
			{
				//return GetValueFromDic(_oldValues, key);
				//return _row[key, DataRowVersion.Original];
				val = _values[key, true];
			}
			else
			{
				val = _values[key];//GetValueFromDic(_values, key);
				if ( PropertyValueGet != null )
					PropertyValueGet(this, d.Properties[key], val);
			}
			if ( (d.Properties[key].IsManyToManyRelation || d.Properties[key].IsOneToManyRelation) && val == null )
			{
				val = _values[key] = Activator.CreateInstance(d.Properties[key].PropertyType, this, d.Properties[key]);
			}
			return val;
		}

        [Internal]
        public object this[string key]
        {
            get
            {
                return this[key, false];
            }
            set
            {
                this[key, false] = value;
            }
        }

        protected T GetValue<T>(string name)
        {

			object val = GetValueInternal(name, false, true);
            if (val == null || val == DBNull.Value)
                    return default(T);
            try
            {
                return (T) val;
            }
            catch (InvalidCastException ice)
            {
                Debug.WriteLine(string.Format("DPO: Cannot change type of value {0} (from type {2}) to type {1}", val, typeof(T), val.GetType()));
            }
            return default(T);
        }

        protected void SetValue(string name, object value)
        {
            this[name] = value;
        }

        [Internal]
		internal Dictionary<string, object> SourceRawValues
        {
            get
            {
                return _sourceRawValues;
            }
            set
            {
                _sourceRawValues = value;
            }
        }

		[Internal]
		internal DataRow SourceRow
		{
			get
			{
				return _sourceRow;
			}
			set
			{
				_sourceRow = value;
			}
		}

		public void CreateNew()
		{
			ObjectDescription od = ClassFactory.GetObjectDescription(this.GetType(),this.CreatorPs);
			if (od.IdFiledType == typeof(Guid))
				this[od.IdField.Name] = Guid.NewGuid();
			State = EntityState.New;
		}

		public void Delete()
		{
			if ( State == EntityState.Changed || State == EntityState.Unchanged )
				State = EntityState.Deleted;
			else if ( State == EntityState.New )
				State = EntityState.NewDeleted;
		}

		public void AcceptChanges()
		{
			_values.AcceptChanges();
			//if ( State != EntityState.Deleted && State != EntityState.NewDeleted )
				State = EntityState.Unchanged;
		}

		public void RejectChanges()
		{
			_values.RejectChanges();
			if (State == EntityState.Changed || State == EntityState.Deleted)
				State = EntityState.Unchanged;
		}

		public void RejectChanges(string propertyName)
		{
			_values.RejectChanges(propertyName);
			if (_values.CheckUncommitedChanges())
				if (State == EntityState.Changed || State == EntityState.Deleted)
					State = EntityState.Unchanged;
		}

		internal void BeginLoad()
		{
			State = EntityState.Loading;
		}

		internal void EndLoad()
		{
			State = EntityState.Unchanged;
			AcceptChanges();
		}

		internal void SetState(EntityState state)
		{
			if ( state == EntityState.Deleted )
				Delete();
			else if ( State == EntityState.New )
			{// do nothing
			}
			else
				State = state;
		}

		[Serializable]
        private class ValueCollection
        {
            private readonly Dictionary<string, ValueContainer> _values = new Dictionary<string, ValueContainer>();

            public object this[string key, bool oldVersion]
            {
                get
                {
                    if (!_values.ContainsKey(key))
                    {
                        _values.Add(key, ValueContainer.Empty);
                    }
                    if ( oldVersion )
                    {
                        ValueContainer c = _values[key];
                        if ( c== ValueContainer.Empty || c.OldValue == ValueContainer.Empty )
                            return c.NewValue;
                    
                        return c.OldValue.NewValue;
                    }
                     
                    return _values[key].NewValue;
                    
                    
                }
                set
                {
                    if ( !_values.ContainsKey(key) )
                    {
                        _values.Add(key, new ValueContainer(value, ValueContainer.Empty));
                    }
                    else
                    {
                        // It should be as follows
                        //ValueContainer old = new ValueContainer(_values[key].NewValue, _values[key].OldValue);
                        //ValueContainer newv = new ValueContainer(value, old);
                        //_values[key] = newv;

                        // but temporarily it would be done in a palliative way
                        ValueContainer c = new ValueContainer(value, new ValueContainer(_values[key].OldValue, ValueContainer.Empty));
                        _values[key] = c;
                    }
                }
            }

			public bool IsUnassignedValue(string key)
			{
				return (!_values.ContainsKey(key)) || _values[key].IsEmpty;
			}

			public void AcceptChanges()
			{
				string[] s = new string[_values.Keys.Count];
				_values.Keys.CopyTo(s, 0);
				foreach (string key in s)
				{
					_values[key] = new ValueContainer(_values[key].NewValue, null);
					//_values[key].OldValue = ValueContainer.Empty;
				}
			}

			public void RejectChanges()
			{
				string[] s = new string[_values.Keys.Count];
				_values.Keys.CopyTo(s, 0);
				foreach (string key in s)
				{
					if (_values[key].OldValue != null)
						_values[key] = new ValueContainer(_values[key].OldValue.NewValue, _values[key].OldValue.OldValue);
					//_values[key].NewValue = _values[key].OldValue;
					//if (_values[key].OldValue != ValueContainer.Empty)
					//    _values[key].OldValue = _values[key].OldValue.NewValue;
				}
			}

			public void RejectChanges(string propertyName)
			{
				_values[propertyName] = new ValueContainer(_values[propertyName].OldValue.NewValue, _values[propertyName].OldValue.OldValue);
				//if (_values[propertyName].OldValue != ValueContainer.Empty)
				//    _values[propertyName].OldValue = _values[propertyName].OldValue.NewValue;
			}

			public bool CheckUncommitedChanges()
			{
				string[] keys = new string[_values.Keys.Count];
				_values.Keys.CopyTo(keys, 0);
				foreach (string key in keys)
				{
					if (_values[key].OldValue != ValueContainer.Empty)
						return false;
				}
				return true;
			}

            public object this[string key]
            {
                get
                {
                    return this[key, false];
                }
                set
                {
                    this[key, false] = value;
                }
            }

			[Serializable]
            private class ValueContainer
            {
                private bool _emptyValue = false;
                private object _newValue;
                private readonly ValueContainer _oldValue;

                public static readonly ValueContainer Empty;

                static ValueContainer()
                {
                    Empty = new ValueContainer(null, null);
                    Empty._emptyValue = true;
                }

                public ValueContainer(object newValue, ValueContainer oldValue)
                {
                    _newValue = newValue;
                    _oldValue = oldValue;
                }

                public object NewValue
                {
                    get
                    {
                        return _newValue;
                    }
                }

                public ValueContainer OldValue
                {
                    get
                    {
                        return _oldValue;
                    }
                }

				public bool IsEmpty
				{
					get
					{
						return _emptyValue;
					}
				}
            }
        }
    }
}
