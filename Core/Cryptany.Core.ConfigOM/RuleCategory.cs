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
    [Table("RulesCategories")]
	public class RuleCategory : EntityBase
	{
		private string _name;

		public RuleCategory()
		{
		}

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

		public List<Rule> GetRules()
		{
			List<EntityBase> rules = CreatorPs.GetEntitiesByFieldValue(typeof(Rule), "Category", this);
			return rules.ConvertAll<Rule>(
				new Converter<EntityBase, Rule>(
					delegate(EntityBase e)
					{
						return e as Rule;
					}
				)
			);
		}

		public List<EntityBase> GetRulesAsEntityBase()
		{
			return CreatorPs.GetEntitiesByFieldValue(typeof(Rule), "Category", this);
		}

		public override string ToString()
		{
			string s = "RuleCategory ('";
			s += Name + "')";
			return s;
		}

		public static List<Rule> GetRulesByCategoriesNames(params string[] names)
		{
			List<Rule> rules = new List<Rule>();
			foreach ( string n in names )
			{
				RuleCategory operatorCat = (RuleCategory)ClassFactory.CreatePersistentStorage("Default").GetEntitiesByFieldValue(typeof(RuleCategory), "Name", n)[0];
				if ( operatorCat == null )
					return null;
				rules.AddRange(operatorCat.GetRules());
			}
			return rules;
		}

		public static List<EntityBase> GetRulesByCategoriesNamesAsEntityBase(params string[] names)
		{
			List<EntityBase> rules = new List<EntityBase>();
			foreach ( string n in names )
			{
				RuleCategory operatorCat = (RuleCategory)ChannelConfiguration.DefaultPs.GetEntitiesByFieldValue(typeof(RuleCategory), "Name", n)[0];
				if ( operatorCat == null )
					return null;
				rules.AddRange(operatorCat.GetRulesAsEntityBase());
			}
			return rules;
		}
	}
}
