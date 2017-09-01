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
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.ConfigOM
{
	[Table("sessions")]
	[DbSchema("kernel")]
	[DenyLoadAllOnGetById]
	[Serializable]
	public class AbonentSession : EntityBase
	{
		[Relation(RelationType.OneToOne, typeof(Abonent), "MSISDN")]
		[FieldName("Name")]
		public Abonent OwnerAbonent
		{
			get
			{
				return GetValue<Abonent>("OwnerAbonent");
			}
			set
			{
				SetValue("OwnerAbonent", value);
			}
		}

		[Relation(RelationType.OneToMany, typeof(AbonentSessionEntry), "Session")]
		public EntityCollection<AbonentSessionEntry> SessionEntries
		{
			get
			{
				EntityCollection<AbonentSessionEntry> val = GetValue<EntityCollection<AbonentSessionEntry>>("SessionEntries");
				if ( val.IndexableFieldsNames == null || val.IndexableFieldsNames.Length == 0 )
					val.IndexableFieldsNames = new string[] { "EntryKey" };
				return val;
			}
			set
			{
				SetValue("SessionEntries", value);
			}
		}

		[NonPersistent]
		public new string this[string idx]
		{
			get
			{
                
				if ( SessionEntries.Indexes["EntryKey"].ContainsKey(idx) )
					return SessionEntries.Indexes["EntryKey"][idx][0].EntryValue;
				else
				{
					AbonentSessionEntry se = ClassFactory.CreateObject<AbonentSessionEntry>(CreatorPs);
					se.Session = this;
                    se.EntryKey = idx;
                    se.EntryValue = string.Empty;
					SessionEntries.Add(se);
					return se.EntryValue;
				}
			}
			set
			{
				if ( SessionEntries.Indexes["EntryKey"].ContainsKey(idx) )
					SessionEntries.Indexes["EntryKey"][idx][0].EntryValue = value;
				else
				{
					AbonentSessionEntry se = ClassFactory.CreateObject<AbonentSessionEntry>(CreatorPs);
					se.Session = this;
					se.EntryKey = idx;
					se.EntryValue = value;
					SessionEntries.Add(se);
				}
			}
		}
	}
}
