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
    [Table("Services")]
    public class Service : EntityBase
	{
		//private Guid _ID;
		private string _name;
		private string _className;
		private bool _enabled;
		private List<ServiceSetting> _settings = new List<ServiceSetting>();

		public Service()
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

		[ReadOnlyField]
		public string ClassName
		{
			get
			{
				return GetValue<string>("ClassName");
			}
			set
			{
				SetValue("ClassName", value);
			}
		}

		public bool Enabled
		{
			get
			{
				return GetValue<bool>("Enabled");
			}
			set
			{
				SetValue("Enabled", value);

			}
		}

		[Relation(RelationType.OneToMany, typeof(ServiceSetting), "Service")]
		public List<ServiceSetting> ServiceSettings
		{
			get
			{
				return _settings;
			}
		}

		public override string ToString()
		{
			return string.Format("{0}", Name);
		}
	}
}
