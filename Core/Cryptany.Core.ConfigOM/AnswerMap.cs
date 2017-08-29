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
    [Table("AnswerMaps")]
	public class AnswerMap : ConcreteObjectBase
    {
		public AnswerMap()
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

		//[ObligatoryField]
		[Relation(RelationType.OneToOne, typeof(Token), "ID")]
		[FieldName("TokenID")]
		public Token Token
		{
			get
			{
				return GetValue<Token>("Token");
			}
			set
			{
				SetValue("Token", value);
			}
		}


		[Relation(RelationType.OneToMany, typeof(AnswerBlock), "Map")]
        public EntityList<AnswerBlock> AnswerBlocks
		{
			get
			{
                return GetValue<EntityList<AnswerBlock>>("AnswerBlocks");
                
                
			}
            set
            {
                SetValue("AnswerBlocks", value);
            }
		}

		[ObligatoryField]
		[FieldName("ChanelID")]
		[Relation(RelationType.OneToOne, typeof(Channel), "ID")]
		public Channel Channel
		{
			get
			{
				return GetValue<Channel>("Channel");
			}
			set
			{
				SetValue("Channel", value);
			}
		}

        [NonPersistent]
        [ReadOnlyField]
        public bool IsMain
        {
            get
            {
				return (Token == null) ? true : !Token.Universal;
            }
        }

        public override string ToString()
        {
            string token = "";
            token = Token != null ? Token.ToString() : "<NONE>";
            return string.Format("AnswerMap, Name: '{0}', Token: {1}", Name, token);
        }

		protected override string GetTableName()
		{
			return "AnswerMaps";
		}
	}
}
