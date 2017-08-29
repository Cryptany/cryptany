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

namespace Cryptany.Core.ConfigOM.Audit
{
	//[Table("")]
	[DbSchema("Audit")]
	[Serializable]
	public class ChannelsUpdateRequestElement : EntityBase
	{
		public DateTime? BeginProcessTime
		{
			get
			{
				return GetValue<DateTime?>("BeginProcessTime");
			}
			set
			{
				SetValue("BeginProcessTime", value);
			}
		}

		public DateTime? EndProcessTime
		{
			get
			{
				return GetValue<DateTime?>("EndProcessTime");
			}
			set
			{
				SetValue("EndProcessTime", value);
			}
		}

		[FieldName("StateId")]
		[Relation(RelationType.OneToOne, typeof(ChannelsUpdateRequestState), "ID")]
		public ChannelsUpdateRequestState RequestElementState
		{
			get
			{
				return GetValue<ChannelsUpdateRequestState>("RequestElementState");
			}
			set
			{
				SetValue("RequestElementState", value);
			}
		}

		[FieldName("RequestId")]
		[Relation(RelationType.OneToOne, typeof(ChannelsUpdateRequest), "ID")]
		public ChannelsUpdateRequest Request
		{
			get
			{
				return GetValue<ChannelsUpdateRequest>("Request");
			}
			set
			{
				SetValue("Request", value);
			}
		}

		[FieldName("ChannelId")]
		[Relation(RelationType.OneToOne, typeof(Channel), "ID")]
		public Channel ChannelToPublish
		{
			get
			{
				return GetValue<Channel>("ChannelToPublish");
			}
			set
			{
				SetValue("ChannelToPublish", value);
			}
		}
	}
}
