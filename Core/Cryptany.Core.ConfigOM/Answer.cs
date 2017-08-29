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
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.ConfigOM
{
	public enum ContentType
	{
		Text = 1,
		WapPush,
		BinaryContent,
		LinkToContent
	}


    [Serializable]
    [DbSchema("services")]
    [Table("Answers")]
	public class Answer : ConcreteObjectBase
	{
		public Answer()
		{
			_ID = Guid.NewGuid();
		}

		public string Body
		{
			get
			{
				return GetValue<string>("Body");
			}
			set
			{
               SetValue("Body", value);
            }
		}

		[FieldName("AnswerBlockID")]
		[Relation(RelationType.OneToOne, typeof(AnswerBlock), "ID")]
		public AnswerBlock Block
		{
			get
			{
               return GetValue<AnswerBlock>("Block");
			}
            set
            {
                SetValue("Block",  value);
            }
		}

		[ObligatoryField]
		[FieldName("ContentTypeID")]
		public ContentType MessageType
		{
            get
            {
                return GetValue<ContentType>("MessageType");
            }
            set
            {
               SetValue("MessageType", value);
            }
		}

		[Relation(RelationType.OneToOne, typeof(AnswerType), "ID")]
		[ObligatoryField]
		[FieldName("AnswerTypeID")]
		public AnswerType AnswerType
		{
            get
            {
                return GetValue<AnswerType>("AnswerType");
            }
            set
            {
                SetValue("AnswerType", value);
            }
		}

		public int PosInCycle
		{
            get
            {
                return GetValue<int>("PosInCycle");
            }
			set
			{
               SetValue("PosInCycle", value);
			}
		}

        public override string ToString()
        {
            string rules = "";
            foreach ( Rule r in Rules )
                if ( rules == "" )
                    rules = r.ToString();
                else
                    rules += "," + r.ToString();
            return string.Format("Answer, Text: '{0}', Rules: {1}", Body, rules); 
        }

		protected override string GetTableName()
		{
			return "Answers";
		}
	}
}
