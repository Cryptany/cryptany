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
using System.Data;

namespace Cryptany.Core.ConfigOM
{
    public class ConfigurationFacade : PersistentStorage
    {

        public ConfigurationFacade(string connstring, string configstr)
            : base(connstring, configstr)
        {
        }

		public EntityCollection<Channel> GetChannels()
		{
			List<EntityBase> ch = GetEntities(typeof(Channel));
			if (ch == null || ch.Count == 0)
				throw new Exception("Error");
			return new EntityCollection<Channel>(ch);
		}

        public EntityCollection<Channel> GetTVADChannels()
        {
            List<EntityBase> ch = GetEntities(typeof(TvadChannel));
            if ( ch == null || ch.Count == 0 )
                throw new Exception("Error");
            return new EntityCollection<Channel>(ch);
        }

        public void UpdateChannel(Channel ch)
        {
            Save(ch);
        }

        public void InsertChannel(Channel ch)
        {
            Save(ch);
        }

        public void DeleteChannel(Channel ch)
        {
            Save(ch);
        }

        public EntityCollection<Service> GetServices()
        {
            List<EntityBase> s = GetEntities(typeof(Service));
            if ( s == null || s.Count == 0 )
                throw new Exception("Error");
            return new EntityCollection<Service>(s);
        }

        public Service GetServiceById(object id)
        {
            return (Service)GetEntityById(typeof(Service), id);
        }
        
        public EntityCollection<Rule> GetRules()
        {
            return new EntityCollection<Rule>(GetEntities(typeof(Rule)));
        }

        public EntityCollection<Token> GetTokens()
        {
            return new EntityCollection<Token>(GetEntities(typeof(Token)));
        }

        public EntityCollection<ServiceNumber> GetServiceNumbers()
        {
            return new EntityCollection<ServiceNumber>(GetEntities(typeof(ServiceNumber)));
        }
    }
}
