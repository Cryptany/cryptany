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
using Cryptany.Core.ConfigOM.Interfaces;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.ConfigOM
{
    [Serializable]
    [DbSchema("services")]
    [Table("Rules")]
	public class Rule : ConcreteObjectBase
    {
        //private Guid _ID;
		private string _name;
		private Condition _condition;
		private RuleStatement _statement1;
		private RuleStatement _statement2;
		private RuleCategory _category;

		public Rule()
		{
			//_ID = Guid.NewGuid();
		}

		[ObligatoryField]
		public string Name
		{
			get
			{
				return GetValue<string>("Name");
			}
			set
			{
				SetValue("Name", value);
			}
		}


		[Relation(RelationType.OneToOne, typeof(Condition), "ID")]
		[FieldName("ConditionID")]
		public Condition Condition
		{
			get
			{
				return GetValue<Condition>("Condition");
			}
			set
			{
				SetValue("Condition", value);
			}
		}

		[Relation(RelationType.OneToOne, typeof(RuleStatement), "ID")]
		[FieldName("Statement1ID")]
		public RuleStatement Statement1
		{
			get
			{
				return GetValue<RuleStatement>("Statement1");
			}
			set
			{
				SetValue("Statement1", value);
			}
		}

		[Relation(RelationType.OneToOne, typeof(RuleStatement), "ID")]
		[FieldName("Statement2ID")]
		public RuleStatement Statement2
		{
			get
			{
				return GetValue<RuleStatement>("Statement2");
			}
			set
			{
				SetValue("Statement2", value);
			}
		}

		[Relation(RelationType.OneToOne, typeof(Rule), "ID")]
		[FieldName("Rule1ID")]
		public Rule Rule1
		{
			get
			{
				return GetValue<Rule>("Rule1");
			}
			set
			{
				SetValue("Rule1", value);
			}
		}

		[Relation(RelationType.OneToOne, typeof(Rule), "ID")]
		[FieldName("Rule2ID")]
		public Rule Rule2
		{
			get
			{
				return GetValue<Rule>("Rule2");
			}
			set
			{
				SetValue("Rule2", value);
			}
		}

		[Relation(RelationType.OneToOne, typeof(RuleCategory), "ID")]
		[FieldName("CategoryID")]
		public RuleCategory Category
		{
			get
			{
				return GetValue<RuleCategory>("Category");
			}
			set
			{
				SetValue("Category", value);
			}
		}

		[NonPersistent]
		public EntityCollection<Statement> Statements
		{
			get
			{
				EntityCollection<Statement> coll = GetValue<EntityCollection<Statement>>("Statements");
				if (coll == null)
				{
					coll = new EntityCollection<Statement>();
					if (Statement1 != null)
					{
						foreach (Statement statement in Statement1.Statements)
							coll.Add(statement);
					}
					SetValue("Statements", coll);
					coll.ItemAdded += coll_ItemAdded;
					coll.ItemRemoved += coll_ItemRemoved;
				}
				return coll;

			}
		}

		private void coll_ItemRemoved(object sender, EntityCollectionItemEventArgs e)
		{
			if ( Statement1 == null )
			{
				Statement1 = ClassFactory.CreateObject<RuleStatement>(this.CreatorPs);
				Statement1.ID = Guid.NewGuid();
			}
			if ( Statement1.Statements.Contains(e.Item as Statement) )
				Statement1.Statements.Remove(e.Item as Statement);
			Statement1.Statements = Statement1.Statements;//To force the entity's state to change
		}

		[FieldName("RuleType")]
		public RuleType Mode
		{
			get
			{
				return GetValue<RuleType>("Mode");
			}
			set
			{
				SetValue("Mode", value);
			}
		}

		private void coll_ItemAdded(object sender, EntityCollectionItemEventArgs e)
		{
			if (Statement1 == null)
			{
				Statement1 = ClassFactory.CreateObject<RuleStatement>(this.CreatorPs);
				Statement1.ID = Guid.NewGuid();
			}
			if ( !Statement1.Statements.Contains(e.Item as Statement) )
				Statement1.Statements.Add(e.Item as Statement);
			if ((e.Item as Statement).RuleStatement == null)
				(e.Item as Statement).RuleStatement = Statement1;
			else if ((e.Item as Statement).RuleStatement != Statement1)
				throw new Exception("The statement has a different parent rule statement, I don't know what to do");
			Statement1.Statements = Statement1.Statements;//To force the entity's state to change
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder(string.Format("Rule '{0}'", Name));
			return sb.ToString();
		}

		protected override string GetTableName()
		{
			return "Rules";
		}
	}

	public enum RuleType
	{
		Basic = 1,
		Expression = 2
	}
}
