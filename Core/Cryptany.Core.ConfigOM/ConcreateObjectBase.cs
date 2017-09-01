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
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.ConfigOM
{
	public abstract class ConcreteObjectBase : EntityBase
	{
		[Relation(RelationType.OneToOne, typeof(GeneralObject), "ID")]
		[FieldName("ObjectID")]
		public GeneralObject Object
		{
			get
			{
				GeneralObject obj = GetValue<GeneralObject>("Object");
				if ( obj == null && State != EntityState.Loading )
				{
					obj = ClassFactory.CreateObject<GeneralObject>(CreatorPs);
					obj.CreatorPs = CreatorPs;
					obj.ID = Guid.NewGuid();
					List<EntityBase> l = CreatorPs.GetEntitiesByFieldValue(typeof(ObjectTable), "TableName", GetTableName());
					if ( l.Count > 0 )
						obj.ObjectTable = l[0] as ObjectTable;
					else
						throw new Exception(string.Format("{0} table not found", GetTableName()));
					SetValue("Object", obj);
				}
				return GetValue<GeneralObject>("Object");
			}
			set
			{
				SetValue("Object", value);
			}
		}

		protected abstract string GetTableName();

        [NonPersistent]
		public EntityCollection<Rule> Rules
		{
			get
			{
				EntityCollection<Rule> l = GetValue<EntityCollection<Rule>>("Rules");
                if (l == null)
                {
                    l = new EntityCollection<Rule>();
                    foreach (Rule r in Object.Rules)
                        l.Add(r);
                    l.ItemAdded += coll_ItemAdded;
                    l.ItemRemoved += coll_ItemRemoved;
                    l.CollectionCleared += coll_CollectionClearedRules;
                    SetValue("Rules", l);
                }
                return l;
			}
            set
            {
                SetValue("Rules", value);
            }
		}

		private void coll_ItemRemoved(object sender, EntityCollectionItemEventArgs e)
		{
			Object.Rules.Remove(e.Item as Rule);
		}

		private void coll_ItemAdded(object sender, EntityCollectionItemEventArgs e)
		{
			Object.Rules.Add(e.Item as Rule);
		}

		private void coll_CollectionClearedRules(object sender, EventArgs e)
		{
			Object.Rules.Clear();
		}

		private void coll_CollectionClearedActions(object sender, EventArgs e)
		{
			Object.Actions.Clear();
		}

		private void coll_CollectionClearedOptions(object sender, EventArgs e)
		{
			Object.ObjectProcessOptions.Clear();
		}

		[NonPersistent]
		public EntityCollection<Rule> Actions
		{
			get
			{
				EntityCollection<Rule> l = GetValue<EntityCollection<Rule>>("Actions");
				if ( l == null )
				{
					l = new EntityCollection<Rule>();
					foreach ( Rule r in Object.Actions )
						l.Add(r);
					l.ItemAdded += coll_ItemAdded2;
					l.ItemRemoved += coll_ItemRemoved2;
					l.CollectionCleared += coll_CollectionClearedActions;
					SetValue("Actions", l);
				}
				return l;
			}
		}

		private void coll_ItemRemoved2(object sender, EntityCollectionItemEventArgs e)
		{
			Object.Actions.Remove(e.Item as Rule);
		}

		private void coll_ItemAdded2(object sender, EntityCollectionItemEventArgs e)
		{
			Object.Actions.Add(e.Item as Rule);
		}

		[NonPersistent]
		public EntityCollection<Rule> ObjectProcessOptions
		{
			get
			{
				EntityCollection<Rule> l = GetValue<EntityCollection<Rule>>("ObjectProcessOptions");
				if ( l == null )
				{
					l = new EntityCollection<Rule>();
					foreach ( Rule r in Object.ObjectProcessOptions )
						l.Add(r);
					l.ItemAdded += coll_ItemAdded3;
					l.ItemRemoved += coll_ItemRemoved3;
					l.CollectionCleared += coll_CollectionClearedOptions;
					SetValue("ObjectProcessOptions", l);
				}
				return l;
			}
		}

		private void coll_ItemRemoved3(object sender, EntityCollectionItemEventArgs e)
		{
			Object.ObjectProcessOptions.Remove(e.Item as Rule);
		}

		private void coll_ItemAdded3(object sender, EntityCollectionItemEventArgs e)
		{
			Object.ObjectProcessOptions.Add(e.Item as Rule);
		}
	}
}
