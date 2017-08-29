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
	public class ChannelsUpdateRequest : EntityBase
	{
		[ObligatoryField]
		[ReadOnlyField]
		public DateTime SubmissionTime
		{
			get
			{
				return GetValue<DateTime>("SubmissionTime");
			}
			set
			{
				SetValue("SubmissionTime", value);
			}
		}

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
		public ChannelsUpdateRequestState RequestState
		{
			get
			{
				return GetValue<ChannelsUpdateRequestState>("RequestState");
			}
			set
			{
				SetValue("RequestState", value);
			}
		}

		[Relation(RelationType.OneToMany, typeof(ChannelsUpdateRequestElement), "Request")]
		public EntityList<ChannelsUpdateRequestElement> RequestElements
		{
			get
			{
				return GetValue<EntityList<ChannelsUpdateRequestElement>>("RequestElements");
			}
			set
			{
				SetValue("RequestElements", value);
			}
		}

		public static ChannelsUpdateRequest CreateRequest(List<Channel> channels)
		{
			if ( channels == null || channels.Count == 0 )
				return null;
			DateTime creationTime = DateTime.Now;
			ChannelsUpdateRequest cur = ClassFactory.CreateObject<ChannelsUpdateRequest>(ChannelConfiguration.DefaultPs);
			cur.SubmissionTime = creationTime;
			cur.RequestState = ChannelsUpdateRequestState.GetDefaultState();
			foreach ( Channel ch in channels )
			{
				ChannelsUpdateRequestElement el = ClassFactory.CreateObject<ChannelsUpdateRequestElement>(ChannelConfiguration.DefaultPs);
				el.ChannelToPublish = ch;
				//el.SubmissionTime = creationTime;
				el.RequestElementState = ChannelsUpdateRequestState.GetDefaultState();
				cur.RequestElements.Add(el);
			}
			return cur;
		}
	}
}
