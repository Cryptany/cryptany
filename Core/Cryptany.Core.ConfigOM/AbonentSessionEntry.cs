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
using Cryptany.Core.ConfigOM.Interfaces;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptany.Core.ConfigOM
{
	[Table("SessionEntry")]
	[DbSchema("kernel")]
	[DenyLoadAllOnGetById]
	public class AbonentSessionEntry : EntityBase
	{
		[Relation( RelationType.OneToOne, typeof(AbonentSession), "ID")]
		[FieldName("SessionId")]
		public AbonentSession Session
		{
			get
			{
				return GetValue<AbonentSession>("Session");
			}
			set
			{
				SetValue("Session", value);
			}
		}

		public string EntryKey
		{
			get
			{
				return GetValue<string>("EntryKey");
			}
			set
			{
				SetValue("EntryKey", value);
			}
		}

		public string EntryValue
		{
			get
			{
				return GetValue<string>("EntryValue");
			}
			set
			{
				SetValue("EntryValue", value);
			}
		}
	}
}
