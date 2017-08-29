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

namespace Cryptany.Core.DPO.MetaObjects
{
	[Serializable]
	public class PropertyDescriptionCollection : IEnumerable<PropertyDescription>
	{
		private ObjectDescription _owner;
		private Dictionary<string, PropertyDescription> _byName = new Dictionary<string, PropertyDescription>();
		private Dictionary<PropertyInfo, PropertyDescription> _byPropInfo = new Dictionary<PropertyInfo, PropertyDescription>();
		private List<PropertyDescription> _items = new List<PropertyDescription>();

		internal PropertyDescriptionCollection(ObjectDescription owner)
		{
			_owner = owner;
		}

		internal void Add(PropertyDescription prop)
		{
			_byName.Add(prop.Name, prop);
			_byPropInfo.Add(prop.Property, prop);
			_items.Add(prop);
		}

		public PropertyDescription this[string name]
		{
			get
			{
				return _byName[name];
			}
		}

		public PropertyDescription this[PropertyInfo prop]
		{
			get
			{
				return _byPropInfo[prop];
			}
		}

		public PropertyDescription this[int index]
		{
			get
			{
				return _items[index];
			}
		}

		public bool ContainsName(string name)
		{
			return _byName.ContainsKey(name);
		}

		public int Count
		{
			get
			{
				return _items.Count;
			}
		}


		public IEnumerator<PropertyDescription> GetEnumerator()
		{
			return new PropertyDescriptionCollectionEnumerator(this);
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return new PropertyDescriptionCollectionEnumerator(this);
		}

		public class PropertyDescriptionCollectionEnumerator : IEnumerator<PropertyDescription>
		{
			private PropertyDescriptionCollection _owner;
			private int index = -1;

			internal PropertyDescriptionCollectionEnumerator(PropertyDescriptionCollection owner)
			{
				_owner = owner;
			}

			public PropertyDescription Current
			{
				get
				{
					if ( index < 0 )
						return _owner[0];
					return _owner[index];
				}
			}

			public void Dispose()
			{
			}

			object System.Collections.IEnumerator.Current
			{
				get
				{
					if ( index < 0 )
						return _owner[0];
					return _owner[index];
				}
			}

			public bool MoveNext()
			{
				if ( index < _owner.Count - 1 )
					index++;
				else
					return false;
				return true;
			}

			public void Reset()
			{
				index = -1;
			}
		}
	}
}
