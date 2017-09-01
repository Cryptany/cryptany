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
using Cryptany.Core.ConfigOM.Interfaces;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.ConfigOM
{
    public enum ChannelPublishState
    {
        Changed,
        Failed,
        Published
    }
    [Serializable]
    [DbSchema("services")]
    [AgregatedClass(typeof(TvadChannel), typeof(SubscriptionChannel), typeof(GlobalErrorChannel),
					 typeof(ContragentSmsChannel),
                   typeof(ContentCodesChannel))]
	public abstract class Channel : ConcreteObjectBase, IChannel
	{
		static Channel()
		{
		}

		[Relation(RelationType.OneToMany, typeof(AnswerMap), "Channel")]
		public EntityList<AnswerMap> AnswerMaps
		{
			get
			{
				EntityList<AnswerMap> l = GetValue<EntityList<AnswerMap>>("AnswerMaps");
				//SetValue("AnswerMaps", l);
				return l;
			}
			set
			{
				SetValue("AnswerMaps", value);
			}
		}

		/// <summary>
		/// Returns the answer map with the token specified. If no such, returns null
		/// </summary>
		/// <param name="token"></param>
		/// <returns></returns>
		public AnswerMap GetAnswerMapByToken(Token token)
		{
			foreach ( AnswerMap am in AnswerMaps )
				if ( am.Token == token )
					return am;
			return null;
		}

		public List<AnswerMap> GetMainAnswerMaps()
		{
			List<AnswerMap> list = new List<AnswerMap>();
			foreach ( AnswerMap am in AnswerMaps )
				if (am.IsMain)
					list.Add(am);
			return list;
		}

		public AnswerMap GetDefaultMainAnswerMap()
		{
            AnswerMap amWithNullToken = null;
		    List<AnswerMap> list = GetMainAnswerMaps();
            if(list.Count==0) return null;
            foreach (AnswerMap am in list)
            {
                if (am.Token == null)
                    amWithNullToken = am;
            }
		    return amWithNullToken ?? list[0];
		}

		public List<ServiceNumber> GetServiceNumbers()
		{
			List<ServiceNumber> list = new List<ServiceNumber>();
			foreach ( AnswerMap map in AnswerMaps )
				if ( map.Token != null && !map.Token.Universal )
					foreach ( ServiceNumber sn in map.Token.ServiceNumbers )
						if ( !list.Contains(sn) )
							list.Add(sn);
			return list;
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

		public bool Enabled
		{
			get
			{
				return GetValue<bool>("Enabled");
			}
			set
			{
				SetValue("Enabled",  value);
			}
		}


        [NonPersistent]
        public ChannelState PublishState
        {
            get
            {
                List<ChannelState> lst = CreatorPs.GetEntitiesByFieldValue<ChannelState>("Channel", this);
                if (lst.Count > 0)
                    return lst[0];
                else
                    return null;

            }
        }

		[FieldName("ContragentResourceID")]
		[Relation(RelationType.OneToOne, typeof(ContragentResource), "ID")]
		public ContragentResource ContragentResource
		{
			get
			{
				return GetValue<ContragentResource>("ContragentResource");
			}
			set
			{
				SetValue("ContragentResource", value);
			}
		}

		[ReadOnlyField]
		[Relation(RelationType.OneToOne, typeof(Service), "ID")]
		[FieldName("ServiceID")]
		public Service Service
		{
			get
			{
				return GetValue<Service>("Service");
			}
			set
			{
				SetValue("Service", value);
			}
		}

		public override string ToString()
		{
			if ( Service != null )
				return string.Format("{0} - [{1}]", Name, Service.ToString());
			else
				return string.Format("{0}", Name);
		}

		protected override string GetTableName()
		{
			return "Channels";
		}
	}
}
