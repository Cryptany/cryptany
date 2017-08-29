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
using System.Text.RegularExpressions;

namespace Cryptany.Core.ConfigOM
{

    [Serializable]
    [DbSchema("services")]
    [Table("RegularExpressions")]
    public class RegExp : EntityBase
    {
		private string _Regex;
		private Token _token;

		public RegExp()
        {

			//_ID = Guid.NewGuid();
			//_Regex = null;
        }

        public string Regex
        {
            get
            {
                return (GetValue<string>("Regex") ?? "");
            }
            set
            {
                SetValue("Regex", value);
            }
        }

		[NonPersistent]
		public Regex RegexObject
		{
			get
			{
				Regex r = GetValue<Regex>("RegexObject");
				if ( r == null )
				{
					r = new Regex(Regex, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
					SetValue("RegexObject", r);
				}
				return r;
			}
		}

		public string Text
		{
			get
			{
				return (GetValue<string>("Text") ?? "");
			}
			set
			{
				SetValue("Text", value);
			}
		}

		[FieldName("TokenID")]
		[Relation(RelationType.OneToOne, typeof(Token), "ID")]
		[ObligatoryField]
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
		
        public override string ToString()
        {
            return Regex;
        }
    }

}
