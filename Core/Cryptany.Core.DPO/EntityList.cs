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
using System.Collections;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.Predicates;

namespace Cryptany.Core.DPO
{
	[Serializable]
	public class EntityList<T> : CollectionBase, IList<T> where T : EntityBase
	{
		private readonly EntityBase _owner = null;
		private readonly PropertyDescription _property = null;

		public EntityList()
		{
		}
		
		public EntityList(EntityBase owner, PropertyDescription property) : base()
		{
			_owner = owner;
			_property = property;
		}

		internal EntityList(EntityBase owner, PropertyDescription property, int capacity)
			: base(capacity)
		{
			_owner = owner;
			_property = property;
		}

		public void Add(T e)
		{
			List.Add(e);
			SetOwnerState(EntityState.Changed);
			if ( _property != null && _property.IsOneToManyRelation )
			{
				ObjectDescription newObjectDescription = ClassFactory.GetObjectDescription(e.GetType(), _owner.CreatorPs);
				foreach ( PropertyDescription pd in newObjectDescription.Relations )
				{
					if ( !pd.IsOneToOneRelation || !pd.IsMapped)
						continue;
					if ( pd.RelationAttribute.RelatedColumn == _property.ReflectedObject.IdField.Name &&
					    pd.RelatedType.IsAssignableFrom(_owner.GetType()) )
					{
						pd.SetValue(e, _owner);
						break;
					}
				}
			}
		}

		public bool Remove(T e)
		{
			List.Remove(e);
		    
			SetOwnerState(EntityState.Changed);
			if ( _property != null &&  _property.IsOneToManyRelation )
			{
				ObjectDescription newObjectDescription = ClassFactory.GetObjectDescription(e.GetType(), _owner.CreatorPs);
				foreach ( PropertyDescription pd in newObjectDescription.Relations )
				{
					if ( !pd.IsOneToOneRelation || !pd.IsMapped )
						continue;
					if ( pd.RelationAttribute.RelatedColumn == _property.ReflectedObject.IdField.Name &&
					    pd.RelatedType.IsAssignableFrom(_owner.GetType()) )
					{
						pd.SetValue(e, null );
						break;
					}
				}
			}
			return true;
		}

		private void SetOwnerState(EntityState state)
		{
			if (_owner==null)
				return;
            //if (state == EntityState.Changed)
            //{
            //    if (_owner.State == EntityState.Unchanged)
					_owner.SetState(EntityState.Changed);
			//}
		}

		protected override void OnInsert(int index, object value)
		{
			base.OnInsert(index, value);
			SetOwnerState(EntityState.Changed);
		}

		protected override void OnRemove(int index, object value)
		{
			base.OnRemove(index, value);
			SetOwnerState(EntityState.Changed);
		}

		protected override void OnClear()
		{
			base.OnClear();
			SetOwnerState(EntityState.Changed);
		}

		public T this[int index]
		{
			get
			{
				return (T)InnerList[index];
			}
			set
			{
				InnerList[index] = value;
			}
		}

		#region IList<T> Members

		public int IndexOf(T item)
		{
			return base.List.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			base.InnerList.Insert(index, item);
		}

		#endregion

		#region ICollection<T> Members


		public bool Contains(T item)
		{
			return base.List.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			base.List.CopyTo(array, arrayIndex);
		}

		public bool IsReadOnly
		{
			get
			{
				return base.List.IsFixedSize;
			}
		}

		#endregion

		#region IEnumerable<T> Members

		public class EntityListEnumerator<T> : IEnumerator<T> where T : EntityBase
		{
			private EntityList<T> _owner;
			private int currentIndex = -1;

			internal EntityListEnumerator(EntityList<T> owner)
			{
				_owner = owner;
			}

			#region IEnumerator<T> Members

			public T Current
			{
				get
				{
					if ( currentIndex >= 0 )
						return _owner[currentIndex];
					else
						throw new Exception("Enumerator not advanced.");
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}

			public bool MoveNext()
			{
				return _owner.Count > ++currentIndex;
				
			}

			public void Reset()
			{
				currentIndex = -1;
			}

			#endregion
		}

		public new IEnumerator<T> GetEnumerator()
		{
			return new EntityListEnumerator<T>(this);
		}

		#endregion

		/// <summary>
		/// Generates a detached EntityList with only those elements satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="fieldName">The name of the attribute to match</param>
		/// <param name="value">The value the specified attribute should be matched with</param>
		/// <returns>Detached EntityList with only those elements satisfying the the specified condition</returns>
		public EntityList<T> Select(string fieldName, object value)
		{
			EntityList<T> list = new EntityList<T>();
			foreach(T entity in List)
				if ( entity[fieldName] == value )
				{
					list.Add(entity);
				}
			return list;
		}
		
		/// <summary>
		/// Generates a detached EntityList with only those elements satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="condition">A predicate that the element of the current list should match in order to be selected</param>
		/// <returns>Detached EntityList with only those elements satisfying the the specified condition</returns>
		public EntityList<T> Select(UnaryOperation<T> condition)
		{
			EntityList<T> list = new EntityList<T>();
			foreach(T entity in List)
				if ( condition(entity) )
				{
					list.Add(entity);
				}
			return list;
		}


        public void Sort(Comparison<T> comparison)
	    {
            List<T> l = new List<T>(this);
            l.Sort(comparison);
            Clear();
            foreach (T entity in l)
                Add(entity);
	    }

	    /// <summary>
		/// Searches for the first element satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="fieldName">The name of the attribute to match</param>
		/// <param name="value">The value the specified attribute should be matched with</param>
		/// <returns>The first element to satisfy the specified condition</returns>
		public T SelectOne(string fieldName, object value)
		{
			foreach(T entity in List)
				if ( entity[fieldName] == value )
				{
					return entity;
				}
			return default(T);
		}
		
		/// <summary>
		/// Searches for the first element satisfying the the specified condition. The algorithm performs
		/// linear walkthrought on the elements of the list
		/// </summary>
		/// <param name="condition">A predicate that the element of the current list should match in order to be selected</param>
		/// <returns>The first element to satisfy the specified condition</returns>
		public T SelectOne(Cryptany.Core.DPO.Predicates.UnaryOperation<T> condition)
		{
			foreach(T entity in List)
				if ( condition(entity) )
				{
					return entity;
				}
			return default(T);
		}
}
}
