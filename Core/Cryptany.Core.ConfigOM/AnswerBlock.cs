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
using System.Data;
using Cryptany.Core.ConfigOM.Interfaces;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.ConfigOM
{

    [Serializable]
    [DbSchema("services")]
    [Table("AnswerBlocks")]
	public class AnswerBlock : ConcreteObjectBase
    {
        private AnswerBlockType _BlockType;
        private AnswerMap _Map;
        private string _Name;

        private List<Answer> _Answers = new List<Answer>();
        private bool _NewAnswerBlock;

        [Relation(RelationType.OneToOne, typeof(AnswerMap), "ID")]
		[FieldName("AnswerMapID")]
		[ObligatoryField]
        public AnswerMap Map
        {
			get
			{
				return GetValue<AnswerMap>("Map");
			}
			set
			{
				SetValue("Map", value);
			}
        }

		[FieldName("AnswerBlockTypeID")]
		[Relation(RelationType.OneToOne, typeof(AnswerBlockType), "ID")]
		[ObligatoryField]
		public AnswerBlockType BlockType
        {
			get
			{
				return GetValue<AnswerBlockType>("BlockType");
			}
			set
			{
				SetValue("BlockType", value);
			}
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
        
		[Relation(RelationType.OneToMany, typeof(Answer), "Block")]
        public EntityList<Answer> Answers
        {
            get
            {
                return GetValue<EntityList<Answer>>("Answers");
                
            }
            set
            {
                SetValue("Answers", value);
            }
        }

		public int OrderIndex
		{
			get
			{
				return GetValue<int>("OrderIndex");
			}
			set
			{
				SetValue("OrderIndex", value);
			}
		}

		public AnswerBlock()
        {
            
        }

        public override string ToString()
        {
            return string.Format("AnswerBlock, Name: '{0}', Type: {1}", Name ?? "", BlockType != null ? BlockType.ToString() : "");
        }

		protected override string GetTableName()
		{
			return "AnswerBlocks";
		}
	}
}
