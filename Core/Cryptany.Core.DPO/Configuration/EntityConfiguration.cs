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
using System.Collections.Generic;
using System.Xml;

namespace Cryptany.Core.DPO.Configuration
{
    public class EntityConfiguration
    {
        private string _name;
        private List<string> _usings = new List<string>();
        private string _namespace;
        private Dictionary<string, EntityPropertyConfiguration> _properties = new Dictionary<string, EntityPropertyConfiguration>();
        private EntityAttributeConfigurationCollection _attributes = new EntityAttributeConfigurationCollection();

        public static EntityConfiguration Create(XmlNode node)
        {
            EntityConfiguration entity = new EntityConfiguration
            {
                _name = node.Attributes["name"].Value
            };

            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.Name == "Usings")
                {
                    foreach (XmlNode nn in n.ChildNodes)
                    {
                        if (nn.Name == "Using")
                            entity._usings.Add(nn.InnerXml);
                        else
                        {
                            EntityConfigurationError err = new EntityConfigurationError(ErrorType.UnexpectedNode, string.Format("Node 'Using' expected but '{0}' found", node.FirstChild.Name));
                            return null;
                        }
                    }
                }
                else if (n.Name == "Namespace")
                {
                    entity._namespace = n.InnerXml;
                }
                else if (n.Name == "Attributes")
                {
                    foreach (XmlNode nn in n.ChildNodes)
                    {
                        EntityAttributeConfiguration attribute = EntityAttributeConfiguration.Create(nn);
                        if (nn != null)
                            entity._attributes.Add(attribute.Name, attribute);
                        else
                        {
                            EntityConfigurationError err = new EntityConfigurationError(ErrorType.InvalidStructural, "The 'Attribute' object was not build correctly");
                            return null;
                        }
                    }
                }
                else if (n.Name == "Properties")
                {
                    foreach (XmlNode nn in n.ChildNodes)
                    {
                        EntityPropertyConfiguration property = EntityPropertyConfiguration.Create(nn);
                        if (property != null)
                            entity._properties.Add(property.Name, property);
                        else
                        {
                            EntityConfigurationError err = new EntityConfigurationError(ErrorType.InvalidStructural, "The 'Property' object was not build correctly");
                            return null;
                        }
                    }
                }
                else
                {
                    EntityConfigurationError err = new EntityConfigurationError(ErrorType.UnexpectedNode, string.Format("Node 'Usings', 'Namespace', 'Attributes' or 'Properties' expected but '{0}' found", node.FirstChild.Name));
                    return null;
                }
            }

            return entity;
        }

        private EntityConfiguration()
        {
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public Dictionary<string, EntityPropertyConfiguration> Properties
        {
            get
            {
                return _properties;
            }
            set
            {
                _properties = value;
            }
        }

        public EntityAttributeConfigurationCollection Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                _attributes = value;
            }
        }
    }
}
