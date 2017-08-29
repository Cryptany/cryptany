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
	[Serializable]
	[DbSchema("services")]
	[Table("Objects")]
	public class GeneralObject : EntityBase
	{
		private bool _concreteObjectSearched = false;

		[Relation(RelationType.OneToOne, typeof(ObjectTable), "ID")]
		[FieldName("ObjectTablesID")]
		[ObligatoryField]
		public ObjectTable ObjectTable
		{
			get
			{
				return GetValue<ObjectTable>("ObjectTable");
			}
			set
			{
				SetValue("ObjectTable", value);
			}
		}

		[FieldName("NamespaceID")]
		[Relation(RelationType.OneToOne, typeof(Channel), "ID")]
		public ObjectNamespace Namespace
		{
			get
			{
				return GetValue<ObjectNamespace>("Namespace");
			}
			set
			{
				SetValue("Namespace", value);
			}
		}

		[NonPersistent]
		[ReadOnlyField]
		public EntityBase ConcreteObject
		{
			get
			{
				if (!_concreteObjectSearched)
				{
					Type[] types = new Type[] { typeof(AnswerMap), typeof(Answer), typeof(Channel), 
						typeof(Rule), typeof(Token), typeof(AnswerBlock) };
					foreach (Type t in types)
					{
						List<EntityBase> l = CreatorPs.GetEntitiesByFieldValue(t, "Object", this);
						if (l.Count > 0)
						{
							SetValue("ConcreteObject", l[0]);
							break;
						}
					}
					_concreteObjectSearched = true;
				}
				return GetValue<EntityBase>("ConcreteObject");
			}
		}

		[Relation(typeof(Rule), "ID", "ObjectRules", "ObjectID", "RuleID", "services")]
		public EntityList<Rule> Rules
		{
            get
            {
                EntityList<Rule> l = GetValue<EntityList<Rule>>("Rules");
                //if (l == null)
                  //  l = new EntityList<Rule>(this, );
                SetValue("Rules", l);
                return l;
            }
			set
			{
				SetValue("Rules", value);
			}
		}

		[Relation(typeof(Rule), "ID", "ObjectActionRules", "ObjectID", "RuleID", "services")]
		public EntityList<Rule> Actions
		{
			get
			{
				return GetValue<EntityList<Rule>>("Actions");
			}
			set
			{
				SetValue("Actions", value);
			}
		}

		[Relation(typeof(Rule), "ID", "ObjectsProcOptionRules", "ObjectID", "RuleID", "services")]
		public EntityList<Rule> ObjectProcessOptions
		{
			get
			{
				return GetValue<EntityList<Rule>>("ObjectProcessOptions");
			}
			set
			{
				SetValue("ObjectProcessOptions", value);
			}
		}

		public override string ToString()
		{
			string s = "GeneralObject (ConcreteObject = '{0}')";
			string ss = ConcreteObject != null ? ConcreteObject.ToString() : "<NULL>";
			return string.Format(s, ss);
		}
	}
}
