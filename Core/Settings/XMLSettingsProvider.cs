using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Cryptany.Common.Settings
{
	/// <summary>
	/// Setting provider that uses XML file for stroing/retrieving settings data
	/// <see cref="Cryptany.Common.Settings.AbstractSettingsProvider"/>
	/// </summary>
	public class XMLSettingsProvider : AbstractSettingsProvider
	{
		/// <summary>
		/// Construstor takes XML filename as param
		/// </summary>
		/// <param name="source">Full filename of XML file with settings</param>
		public XMLSettingsProvider(string source)
			: base(source) 
		{ }


		/// <summary>
		/// Load settings from XML file
		/// </summary>
		protected override void LoadSettings()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(Source);
			XmlNodeList nodes = doc.SelectNodes("settings/setting");
			foreach (XmlElement node in nodes)
			{
				string className = node.Attributes["type"].Value;

				string strVal = node.Attributes["value"].Value;
				object tmpValue = ConvertFromString(strVal, className);
				_InternalCollection.Add(node.Attributes["key"].Value, tmpValue);
			}
		}

		/// <summary>
		/// Saves settings to XML file
		/// </summary>
		protected override void SaveSettings()
		{
			XmlDocument doc = new XmlDocument();
			XmlElement root = doc.CreateElement("settings");


			foreach (string key in _InternalCollection.Keys)
			{
				XmlElement settingNode = doc.CreateElement("setting");

				XmlAttribute keyAttr = doc.CreateAttribute("key");
				keyAttr.Value = key;

				XmlAttribute valueAttr = doc.CreateAttribute("value");
				valueAttr.Value = _InternalCollection[key].ToString();

				XmlAttribute typeAttr = doc.CreateAttribute("type");
				typeAttr.Value = _InternalCollection[key].GetType().ToString();

				settingNode.Attributes.Append(keyAttr);
				settingNode.Attributes.Append(valueAttr);
				settingNode.Attributes.Append(typeAttr);

				root.AppendChild(settingNode);
			}

			doc.AppendChild(root);

			doc.Save(Source);
		}

	}
	
}
