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
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.ConfigOM
{
	[DbSchema("avantik")]
	[Table("Projects")]
	[Serializable]
	public class AvantikProject : EntityBase
	{
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

		public string Description
		{
			get
			{
				return GetValue<string>("Description");
			}
			set
			{
				SetValue("Description", value);
			}
		}

		public DateTime BeginDate
		{
			get
			{
				return GetValue<DateTime>("BeginDate");
			}
			set
			{
				SetValue("BeginDate", value);
			}
		}

		public DateTime? EndDate
		{
			get
			{
				return GetValue<DateTime?>("EndDate");
			}
			set
			{
				SetValue("EndDate", value);
			}
		}

		public string TransportURL
		{
			get
			{
				return GetValue<string>("TransportURL");
			}
			set
			{
				SetValue("TransportURL", value);
			}
		}

		public string StartPageURL
		{
			get
			{
				return GetValue<string>("StartPageURL");
			}
			set
			{
				SetValue("StartPageURL", value);
			}
		}

		public bool? IsTransport
		{
			get
			{
				return GetValue<bool?>("IsTransport");
			}
			set
			{
				SetValue("IsTransport", value);
			}
		}

		public bool? CheckSpam
		{
			get
			{
				return GetValue<bool?>("CheckSpam");
			}
			set
			{
				SetValue("CheckSpam", value);
			}
		}

		public bool? TransportHideMSISDN
		{
			get
			{
				return GetValue<bool?>("TransportHideMSISDN");
			}
			set
			{
				SetValue("TransportHideMSISDN", value);
			}
		}
	}
}
