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
using System.Xml;

namespace Cryptany.Core.DPO.Configuration
{
	public class Configuration
	{
		private Dictionary<string, EntityConfiguration> _conf = new Dictionary<string, EntityConfiguration>();
		
		public static Configuration Create(XmlReader reader)
		{
			XmlReaderSettings sett = new XmlReaderSettings();
			sett.IgnoreComments = true;
			sett.IgnoreProcessingInstructions = true;
			sett.IgnoreWhitespace = true;

			XmlReader rdr = XmlReader.Create(reader, sett);

			XmlDocument doc = new XmlDocument();
			doc.Load(rdr);

			if (doc.ChildNodes[0].Name != "Entities")
			{
				
				EntityConfigurationError err = new EntityConfigurationError(ErrorType.UnexpectedNode, "Expected node 'Entities', but '" + doc.Name + "' found");
				return null;
			}

			Configuration conf = new Configuration();

			foreach (XmlNode node in doc.ChildNodes[0].ChildNodes)
			{
				EntityConfiguration entity = EntityConfiguration.Create(node);
				if (entity != null)
					conf.Entities.Add(node.Attributes["name"].Value, entity);
			}
			return conf;
		}

		public Dictionary<string, EntityConfiguration> Entities
		{
			get
			{
				return _conf;
			}
		}
	}
}
