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
using System.IO;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using System.Reflection;
using System.Xml.XPath;
using System.Drawing;
using System.Collections;

namespace Cryptany.Core.DPO.Xml
{
	public class XmlDataSaver : ISaver
	{
		private Type _entityType;
		private Mapper _mapper;
		private PersistentStorage _ps;
		private XmlDocument _document;
		private XPathNavigator _navigator;
		private XmlWriter _sourceWriter;

		//public XmlDataSaver(XmlWriter writer, PersistentStorage ps, Type type)
		//{
		//    _entityType = type;
		//    _ps = ps;
		//    _sourceWriter = writer;
		//    _mapper = new Mapper(type);
		//}

		public XmlDataSaver(string fileName, string entitiesTagPath, PersistentStorage ps, Type type)
		{
			_entityType = type;
			_ps = ps;
			//_fileName = fileName;

			_document = new XmlDocument();
			_document.Load(fileName);
			_navigator = _document.CreateNavigator();


			XmlWriterSettings sett = new XmlWriterSettings();
			sett.CloseOutput = true;
			sett.Indent = true;
			sett.NewLineHandling = NewLineHandling.Entitize;
			_navigator = NavigateToNode(entitiesTagPath);
			_mapper = new Mapper(type, _ps);
		}

		private XPathNavigator NavigateToNode(string nodePath)
		{
			XPathNavigator navigator = _document.CreateNavigator();
			string[] path = nodePath.Split('\\');
			foreach (string s in path)
				navigator.MoveToChild(s, "");
			return navigator;
		}

		public Type ReflectedType
		{
			get
			{
				return _entityType;
			}
		}

		public Mapper Mapper
		{
			get
			{
				return _mapper;
			}
		}
		
		public System.Data.DataSet UnderlyngDataSet
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public void Save(EntityBase entity)
		{
			if (entity.State == EntityState.Unchanged)
				return;
			ObjectDescription od = ClassFactory.GetObjectDescription(entity.GetType(), _ps);
			EntityBase ent;
			if (od.IsWrapped)
			{
				ent = (entity as IWrapObject).WrappedObject;
				_ps.SaveInner(ent);
				return;
			}
			else
				ent = entity;
			if (!_navigator.MoveToFollowing(GetParentNodeName(), ""))
				CreateNodeContainer(_navigator);
			_navigator.MoveToFollowing(GetParentNodeName(), "");
			//foreach (EntityBase entity in entities)
			//{
				if (ent.State == EntityState.New)
					Insert(entity, _navigator);
				else if (ent.State == EntityState.Deleted)
					DeleteEntity(ent, _navigator);
				else
					Update(entity, _navigator);
			//}
		}

		public void Save(List<EntityBase> entities)
		{
			foreach (EntityBase e in entities)
				Save(e);
			//XmlDocument doc = (_command.Connection as XmlConnection).Document;
			//XPathNavigator nav = doc.CreateNavigator();
			//if (!nav.MoveToFollowing(GetParentNodeName(), ""))
			//    CreateNodeContainer(nav);
			//nav.MoveToFollowing(GetParentNodeName(), "");
			//foreach (EntityBase entity in entities)
			//{
			//    if (entity.Id != 0)
			//        Update(entity, nav);
			//    else
			//        Insert(entity, nav);
			//}
		}

		private void DeleteEntity(EntityBase entity, XPathNavigator nav)
		{
			nav.DeleteSelf();
		}

		private void CreateNodeContainer(XPathNavigator nav)
		{
			XmlWriter writer = nav.AppendChild();
			writer.WriteStartElement(GetParentNodeName());
			writer.WriteEndElement();
			writer.Close();
		}

		protected string GetParentNodeName()
		{
			return Mapper.TableName;
		}

		private void Insert(EntityBase entity, XPathNavigator nav)
		{
			nav.MoveToFirstChild();
			if (entity.ID == null)
			{
				throw new Exception("Ohh, what I've done?!");
			}
			nav.MoveToParent();
			XmlWriter writer = nav.AppendChild();
			CreateXmlNodeEl(entity, writer);
			writer.Close();
			nav.MoveToNext();
		}
		
		private void Update(EntityBase entity, XPathNavigator nav)
		{
			bool hasChildren = true;
			if (nav.LocalName == GetParentNodeName())
				hasChildren = nav.MoveToFirstChild();
			if (MoveToId(ref nav, entity.ID.ToString()))
				WriteEntity(entity, nav);
			else
			{
				if (hasChildren)
					nav.MoveToParent();
				XmlWriter writer = nav.AppendChild();
				CreateXmlNodeEl(entity, writer);
				writer.Close();
				nav.MoveToNext();
			}
		}

		private void CreateXmlNodeEl(EntityBase entity, XmlWriter writer)
		{
			writer.WriteStartElement(Mapper.TableName);
			writer.WriteAttributeString("Id", entity.ID.ToString());
			foreach (PropertyDescription property in ClassFactory.GetObjectDescription(ReflectedType,_ps).Properties)
			{
				if (property.IsId || !(property.IsMapped || property.IsManyToManyRelation))
					continue;


				writer.WriteStartElement(property.Name);
				object value = null;
				try
				{
					if (property.IsManyToManyRelation)
					{
						WriteMtm(entity, property, writer);
					}
					else
					{
						if (property.IsOneToOneRelation)
							value = (entity[property.Name] as EntityBase).ID;
						else
							value = entity[property.Name];
						value = PrepareToSave(value);
						writer.WriteValue(value);
					}
				}
				catch
				{
					throw new ApplicationException(string.Format("Unable to write attribute: {0}", property.Name));
				}
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
		}

		private void WriteMtm(EntityBase e, PropertyDescription p, XmlWriter w)
		{
			foreach (EntityBase entity in e[p.Name] as IList)
			{
				w.WriteStartElement("Item");
				w.WriteValue(entity.ID.ToString());
				w.WriteEndElement();
			}
		}

		private void WriteEntity(EntityBase entity, XPathNavigator nav)
		{
			foreach (PropertyDescription prop in ClassFactory.GetObjectDescription(ReflectedType,_ps).Properties)
			{
				if (!(prop.IsManyToManyRelation || prop.IsMapped))
					continue;

				if (prop.IsId)
				{
					nav.MoveToAttribute("Id", "");
					nav.SetValue(Convert.ChangeType(entity[prop.Name], typeof(string)) as string);
					nav.MoveToParent();
					continue;
				}

				nav.MoveToChild(prop.Name, "");
				bool write = true;
				if (write)
				{
					try
					{
						object value = entity[prop.Name];
						SaveValue(value, prop, nav);
					}
					catch
					{
						throw new ApplicationException(string.Format("Unable to write attribute: {0}", prop.Name));
					}
				}
				nav.MoveToParent();
			}
		}

		private bool MoveToId(ref XPathNavigator nav, string id)
		{
			string attr = nav.GetAttribute("Id", "");
			bool succeed = true;
			while (attr != id)
			{
				succeed = nav.MoveToFollowing(nav.LocalName, nav.NamespaceURI);
				if (!succeed)
					return false;
				attr = nav.GetAttribute("Id", "");
			}
			return true;
		}

		private object PrepareToSave(object value)
		{
			if (value == null)
				value = "";
			else if (value.GetType().IsEnum)
				value = (int)value;
			else if (value is Image)
			{
				MemoryStream ms = new MemoryStream();
				((Image)value).Save(ms, ((Image)value).RawFormat);
				value = ms.ToArray();
			}
			else if (value is byte[])
			{
				MemoryStream ms = new MemoryStream(value as byte[]);
			}
			else
			{
			}
			return value;
		}

		private void SaveValue(object value, PropertyDescription property, XPathNavigator nav)
		{
			if (value == null)
				value = "";
			if (property.IsManyToManyRelation)
			{
				foreach (EntityBase e in value as IList)
				{
					nav.AppendChildElement("", "Item", "", e.ID.ToString());
				}
				return;
			}
			else if (property.IsOneToOneRelation)
			{
				value = (value as EntityBase).ID;
			}
			else
				value = PrepareToSave(value);
			nav.SetTypedValue(value);
		}

		#region ISaver Members


		public PersistentStorage Ps
		{
			get
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		#endregion
	}
}
