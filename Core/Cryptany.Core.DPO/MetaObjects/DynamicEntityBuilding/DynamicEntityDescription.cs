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

namespace Cryptany.Core.DPO.MetaObjects.DynamicEntityBuilding
{
	public class DynamicEntityDescription
	{
		private string _name;
		private List<string> _usings = new List<string>();
		private string _namespace;
		private Dictionary<string, DynamicEntityPropertyDescription> _properties = new Dictionary<string, DynamicEntityPropertyDescription>();
		private DynamicEntityAttributeDescriptionCollection _attributes = new DynamicEntityAttributeDescriptionCollection();

		public static DynamicEntityDescription Create(XmlReader reader)
		{
			XmlReaderSettings sett = new XmlReaderSettings();
			sett.IgnoreComments = true;
			sett.IgnoreProcessingInstructions = true;
			sett.IgnoreWhitespace = true;

			XmlReader rdr = XmlReader.Create(reader, sett);

			XmlDocument doc = new XmlDocument();
			doc.Load(rdr);

			if ( doc.ChildNodes.Count != 1 )
			{
				DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidXmlDocument, "The xml document must only one node describing an entity");
				return null;
			}
			XmlNode node = doc.ChildNodes[0];
       
			DynamicEntityDescription entity = new DynamicEntityDescription();

			entity._name = node.Name;

			foreach ( XmlNode n in node.ChildNodes )
			{
				if ( n.Name == "Usings" )
				{
					foreach ( XmlNode nn in n.ChildNodes )
					{
						if ( nn.Name == "Using" )
							entity._usings.Add(nn.InnerXml);
						else
						{
							DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.UnexpectedNode, string.Format("Node 'Using' expected but '{0}' found", node.FirstChild.Name));
							return null;
						}
					}
				}
				else if ( n.Name == "Namespace" )
				{
					entity._namespace = n.InnerXml;
				}
				else if ( n.Name == "Attributes" )
				{
					foreach ( XmlNode nn in n.ChildNodes )
					{
						DynamicEntityAttributeDescription attribute = DynamicEntityAttributeDescription.Create(nn);
						if ( nn != null )
							entity._attributes.Add(attribute.Name, attribute);
						else
						{
							DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidStructural, "The 'Attribute' object was not build correctly");
							return null;
						}
					}
				}
				else if ( n.Name == "Properties" )
				{
					foreach ( XmlNode nn in n.ChildNodes )
					{
						DynamicEntityPropertyDescription property = DynamicEntityPropertyDescription.Create(nn);
						if ( property != null )
							entity._properties.Add(property.Name, property);
						else
						{
							DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidStructural, "The 'Property' object was not build correctly");
							return null;
						}
					}
				}
				else
				{
					DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.UnexpectedNode, string.Format("Node 'Usings', 'Namespace', 'Attributes' or 'Properties' expected but '{0}' found", node.FirstChild.Name));
					return null;
				}
			}

			return entity;
		}

		private DynamicEntityDescription()
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

		public List<string> Usings
		{
			get
			{
				return _usings;
			}
			set
			{
				_usings = value;
			}
		}

		public string Namespace
		{
			get
			{
				if ( _namespace == null )
					_namespace = "";
				return _namespace;
			}
			set
			{
				_namespace = value;
			}
		}

		public Dictionary<string, DynamicEntityPropertyDescription> Properties
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

		public DynamicEntityAttributeDescriptionCollection Attributes
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

		public string FullName
		{
			get
			{
				if ( Namespace == "" )
					return Name;
				else
					return Namespace + "." + Name;
			}
		}

		public string Compile()
		{
			string code = "";

			foreach ( string s in Usings )
				code += "using " + s + ";\r\n";

			if ( Namespace != null && Namespace != "" )
			{
				code += "namespace " + Namespace + "\r\n";
				code += "{\r\n";
			}

			foreach ( string s in Attributes.Keys )
				code += Attributes[s].Compile();

			code += "public class " + Name + " : Cryptany.Core.DPO.EntityBase\r\n";
			code += "{\r\n";

			code += "public " + Name + "()\r\n";
			code += "{\r\n}\r\n";

			foreach ( string s in Properties.Keys )
				code += Properties[s].Compile() + "\r\n";

			code += "}\r\n";

			if ( Namespace != null && Namespace != "" )
				code += "}\r\n";

			return code;
		}
	}
}
