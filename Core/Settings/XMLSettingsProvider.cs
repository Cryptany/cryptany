using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace avantMobile.Settings
{
	/// <summary>
	/// Провайдер настроек использующий для получения/сохранения данных XML файл.
	/// <see cref="avantMobile.Settings.AbstractSettingsProvider"/>
	/// </summary>
	public class XMLSettingsProvider : AbstractSettingsProvider
	{
		/// <summary>
		/// Конструктор в качестве параметра принимает путь к XML файлу.
		/// </summary>
		/// <param name="source"></param>
		public XMLSettingsProvider(string source)
			: base(source) 
		{ }


		/// <summary>
		/// Загружает настройки
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
		/// Сохраняет настройки
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
