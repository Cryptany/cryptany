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
    [Serializable]
    [DbSchema("services")]
    [Table("Channels", "ServiceID", ConditionOperation.Equals, "ACD73D22-DAC5-4DD9-945E-D5E4CC82ADC2")]
    [WrappedClass(typeof(TvadChannel))]
	public class AggregationChannel : Channel, IWrapObject
	{
		private Token _ChannelToken;
		private TvadChannel _channel;

		public AggregationChannel()
		{
			_channel = (TvadChannel)ClassFactory.CreateObject(typeof(TvadChannel), ClassFactory.GetThreadDefaultPs(System.Threading.Thread.CurrentThread));
			_channel.AnswerMaps.Add((AnswerMap)ClassFactory.CreateObject(typeof(AnswerMap), ClassFactory.GetThreadDefaultPs(System.Threading.Thread.CurrentThread)));

		    IEntity e = new TvadChannel();
		}

		public AggregationChannel(TvadChannel channel)
		{
			//_channel = channel;
		}

        [NonPersistent]
        [Relation(RelationType.OneToOne, typeof(Token), "ID")]
        public Token ChannelToken
        {
            get
            {
                return GetValue<Token>("ChannelToken");
            }
            set
            {
                SetValue("ChannelToken", value);
                if (_channel.AnswerMaps.Count > 0)
                    _channel.AnswerMaps[0].Token = _ChannelToken;
            }
        }

        EntityBase IWrapObject.WrappedObject
		{
			get
			{
                if ( _channel.AnswerMaps.Count > 0 )
                    _channel.AnswerMaps[0].Token = this.ChannelToken;
				_channel.Enabled = this.Enabled;
				_channel.ID = this.ID;
				_channel.Name = this.Name;
				_channel.ContragentResource = this.ContragentResource;
				_channel.Service = this.Service;
				_channel.State = this.State;
				_channel.Rules.Clear();
				foreach ( Rule r in this.Rules )
					_channel.Rules.Add(r);
				return _channel;
			}
			set
			{
				_channel = (TvadChannel)value;
                if ( _channel.AnswerMaps.Count < 1 )
                    this.ChannelToken = null;
                else
                    this.ChannelToken = _channel.AnswerMaps[0].Token;
				this.Enabled = _channel.Enabled;
				this.ID = _channel.ID;
				this.Name = _channel.Name;
				this.ContragentResource = _channel.ContragentResource;
				this.Service = _channel.Service;
				this.State = _channel.State;
				this.Rules.Clear();
				foreach ( Rule r in _channel.Rules )
					this.Rules.Add(r);
			}
		}
	}

}
