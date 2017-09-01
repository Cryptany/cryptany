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
using Cryptany.Core.ConfigOM;

namespace Cryptany.Core.ConfigOM
{
	[DbSchema("kernel")]
	[Table("SMSCSettings")]
	[Serializable]
	public class SmscSettings : EntityBase
	{
		[Relation( RelationType.OneToOne, typeof(SMSC), "ID")]
		[FieldName("SMSCId")]
		public SMSC Smsc
		{
			get
			{
				return GetValue<SMSC>("Smsc");
			}
			set
			{
				SetValue("Smsc", value);
			}
		}

		public string ProtocolType
		{
			get
			{
				return GetValue<string>("ProtocolType");
			}
			set
			{
				SetValue("ProtocolType", value);
			}
		}

		
		public string Settings
		{
			get
			{
				return GetValue<string>("Settings");
			}
			set
			{
				SetValue("Settings", value);
			}
		}

		public string ManagerClass
		{
			get
			{
				return GetValue<string>("ManagerClass");
			}
			set
			{
				SetValue("ManagerClass", value);
			}
		}
	}
}
