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
	public class DynamicEntityPropertyDescription
	{
		private string _name;
		private string _typeName;
		private DynamicEntityBuildingError _error;
		private DynamicEntityAttributeDescriptionCollection _attributes = new DynamicEntityAttributeDescriptionCollection();
		private string _getMethod;
		private string _setMethod;
		private bool _isList = false;

		public static DynamicEntityPropertyDescription Create(XmlNode node)
		{
			DynamicEntityPropertyDescription property = new DynamicEntityPropertyDescription();
			
			if ( node.Name != "Property" )
			{
				DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidXmlDocument, "A start element expected");
				return null;
			}

			property._name = node.Attributes["name"].InnerXml;

			if ( node.FirstChild.Name != "Type" )
			{
				DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidXmlDocument, string.Format("Node 'Type' expected but {0} found", node.FirstChild.Name));
				return null;
			}
			property._typeName = node.FirstChild.InnerXml;
			if ( property.TypeName.StartsWith("List") )
				property._isList = true;
			else
				property._isList = false;

			if ( node.ChildNodes.Count > 1 )
			{
				foreach ( XmlNode n in node.ChildNodes )
				{
					if ( n.Name == "Type" )
						continue;
					else if ( n.Name == "Attributes" )
						foreach ( XmlNode childNode in n.ChildNodes )
						{
							DynamicEntityAttributeDescription attr = DynamicEntityAttributeDescription.Create(childNode);
							if ( attr != null )
								property._attributes.Add(attr.Name, attr);
							else
							{
								DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.InvalidStructural, "The 'Attribute' object was not build correctly");
								return null;
							}
						}
					else if ( n.Name == "get" )
						property._getMethod = n.InnerXml;
					else if ( n.Name == "set" )
						property._setMethod = n.InnerXml;
					else
					{
						DynamicEntityBuildingError err = new DynamicEntityBuildingError(ErrorType.UnexpectedNode, string.Format("Node '{0}' is unexpected", n.Name));
						return null;
					}
				}
			}

			return property;
		}

		private DynamicEntityPropertyDescription()
		{
		}

		public string Name
		{
			get
			{
				return _name;
			}
		}

		public string TypeName
		{
			get
			{
				return _typeName;
			}
		}

		public DynamicEntityAttributeDescriptionCollection Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public string GetMethod
		{
			get
			{
				return _getMethod;
			}
			set
			{
				_getMethod = value;
			}
		}

		public string SetMethod
		{
			get
			{
				return _setMethod;
			}
			set
			{
				_setMethod = value;
			}
		}

		public bool IsList
		{
			get
			{
				return _isList;
			}
			set
			{
				_isList = value;
			}
		}

		public string Compile()
		{
			string code = "public ";
			code += TypeName + " " + Name + "\r\n";
			code += "{\r\n";

			code += "get\r\n";
			code += "{\r\n";
			if (GetMethod == null || GetMethod == "")
				code += "return GetValue<" + TypeName + ">(\"" + Name + "\");\r\n";
			else
				code += GetMethod;
			code += "}\r\n";

			//if ( !IsList || ( IsList && ( SetMethod != null && SetMethod != "" ) ) )
			{
				code += "set\r\n";
				code += "{\r\n";
				if ( SetMethod == null || SetMethod == "" )
					code += "SetValue(\"" + Name + "\", value);\r\n";
				else
					code += SetMethod;
				code += "}\r\n";
			}

			code += "}\r\n";

			//code += "private " + TypeName + " _" + Name;
			//if ( IsList )
			//    code += " = new " + TypeName + "()";
			//code += ";\r\n";

			string attrs = "";
			foreach ( string s in Attributes.Keys )
				attrs += Attributes[s].Compile() + "\r\n";

			code = attrs + code;

			return code;
		}
	}
}
