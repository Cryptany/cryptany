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
using System.Xml.XPath;
using System.Reflection;
using System.Collections;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO.DataSource
{
	public class XmlProvider : IProvider
	{
		//private IConnectionProvider _dbProvider = null;
		//private IObjectType _ObjectType = null;

		private string _source = "";
		XmlDocument _Doc = null;

		public XmlProvider(string FileName)
		{
			_source = FileName;
			_Doc = new XmlDocument();
			_Doc.Load(FileName);
		}

        public string Source
        {
            get
            {
                return _source;
            }
        }

        public List<IEntity> LoadEntities(ObjectDescription ObjectType)
		{
			List<IEntity> res = new List<IEntity>();
			XmlReader reader = ReadXmlDocument(ObjectType);

            reader.ReadToDescendant(ObjectType.ObjectType.Name);
			// Now we are at the first record of the needed table

			while ( !reader.EOF )
			{
				XmlReader rdr = reader.ReadSubtree();// get the subtree of the entity instance
				rdr.Read();// Get the root node of the entity instance
				IEntity entity = ReadObject(rdr, ObjectType);
				//SkipWhiteSpace(reader);
				reader.ReadToFollowing(ObjectType.ObjectType.Name);
				res.Add(entity);
			}
			return res;
		}

        protected IEntity ReadObject(XmlReader reader, ObjectDescription ObjectType)
		{
			IEntity ob = ObjectType.CreateInstance();

			//foreach (string propName in ObjectType.Properties.Keys)
			//{
            if (reader.Name == ObjectType.ObjectType.Name)
			{
				ob.Id = int.Parse(reader.GetAttribute("Id"));
                ob.Name = reader.GetAttribute("Name");
                do
				{
					SkipWhiteSpace(reader);
					if ( reader.Name == ObjectType.TableName &&
						reader.NodeType == XmlNodeType.EndElement )
						break;// The entuty have been read
					string propName = reader.Name;
					if ( ObjectType.PropertyExists(propName) ) //   != null)
					{
                        bool doRead = true;
                        object[] attrs = ObjectType.GetProperty(propName).GetCustomAttributes(true);
                        if (attrs != null && attrs.Length != 0)
                            foreach (object attr in attrs)
                            {
                                if (attr is NonPersistentAttribute)
                                    doRead = false;
                                if (attr is ReferenceAttribute)
                                    if (reader.MoveToFirstAttribute())
                                    {
                                        MemberAttributes attribs = new MemberAttributes();
                                        do
                                        {
                                            //reader.ReadAttributeValue();
                                            attribs.Add(reader.Name, reader.Value);
                                        } while (reader.MoveToNextAttribute());
                                        reader.MoveToElement();
                                        ob.Attributes.Add(propName, attribs);
                                    }
                            }
                        if (!doRead)
                        {
                            reader.Read();
                            continue;
                        }
                        if (ObjectType.GetProperty(propName).PropertyType.IsArray)
                        {
                            ReadArrayProp(ref ob, ObjectType.GetProperty(propName), reader);
                            reader.Read();
                            continue;
                        }
                        object value = reader.ReadElementContentAs(ObjectType.GetProperty(propName).PropertyType, null);
                        ObjectType.SetReadPropertyValue(propName, ob, value);
					}
				} while ( reader.Name != ObjectType.TableName && reader.NodeType != XmlNodeType.EndElement );
			}
			//}
			return ob;
		}

        private void ReadArrayProp(ref IEntity o, PropertyInfo prop, XmlReader reader)
        {
            Type type = prop.PropertyType.GetElementType();
            ArrayList arr = new ArrayList();
            for (reader.ReadToDescendant("Item"); reader.NodeType != XmlNodeType.EndElement; reader.Read())
                arr.Add(reader.ReadElementContentAs(type, null));
            Array a = Activator.CreateInstance(prop.GetValue(o, null).GetType(), arr.Count) as Array;
            for (int i = 0; i < arr.Count; i++)
                a.SetValue(arr[i], i);
            prop.SetValue(o, a, null);
            //prop.SetValue(o, Convert.ChangeType(arr, prop.PropertyType), null);
        }

        private void SkipWhiteSpace(XmlReader reader)
		{
			do
			{
				reader.Read();
			} while ( reader.NodeType == XmlNodeType.Whitespace ||
						reader.NodeType == XmlNodeType.ProcessingInstruction ||
						reader.NodeType == XmlNodeType.Notation );
		}

		protected object GetFieldValue(PropertyInfo prop, string colName, XmlReader reader)
		{
			if ( reader.ReadToFollowing(colName) )
			{
				object o = null;
				try
				{
					string s = reader.ReadElementContentAsString();
					o = Convert.ChangeType(s.Trim(), prop.PropertyType);
					//o = reader.ReadElementContentAs(prop.PropertyType, null);
				}
				catch
				{
					o = null;
					//?????
				}
				return o;
			}
			else
				throw new ApplicationException(string.Format("The property {0} have not been found in the XML file", prop.Name));
		}

		private void ReadXmlSchema(Type type)
		{
			// For validation?
		}

//        private XmlReader ReadXmlDocument(ObjectDescription type)
//        {
//            XmlReaderSettings sett = new XmlReaderSettings();
//            sett.IgnoreComments = true;
//            sett.IgnoreProcessingInstructions = true; //??
//            sett.IgnoreWhitespace = true;
//            //sett.Schemas.Add("","");
//            //sett.ValidationType = ValidationType.Schema;
//#if false
//            XmlReader xmlReader = XmlReader.Create(DbProvider.ConnectionString, sett);
//#else
//            XmlReader xmlReader = XmlReader.Create(type, sett);
//#endif
//            xmlReader.ReadToFollowing(type.TableName);
//            xmlReader = XmlReader.Create(xmlReader.ReadSubtree(), sett);
//            // The part of the document containing the selected Entity is loaded
//            // Now one may use it to get the list of instances...
//            return xmlReader;
//        }

		public int Save(IEntity entity)
		{
            ObjectDescription ObjectType = new ObjectDescription(entity.GetType());
			XmlDocument doc = new XmlDocument();
			doc.Load(Source);
			XPathNavigator nav = doc.CreateNavigator();
			nav.MoveToFollowing(ObjectType.TableName, "");
			int Id = 0;
			if ( entity.Id != 0 )
			{
				Update(entity, nav);
				Id = entity.Id;
			}
			else
				Id = Insert(entity, nav);
			doc.Save(Source);
			return Id;
		}

		public void Save(List<IEntity> entities)
		{
            ObjectDescription ObjectType = null;
            if (entities.Count > 0)
                ObjectType = new ObjectDescription(entities[0].GetType());
            else
                return;
			XmlDocument doc = new XmlDocument();
			doc.Load(Source);
			XPathNavigator nav = doc.CreateNavigator();
			nav.MoveToFollowing(ObjectType.TableName, "");
			foreach ( IEntity entity in entities )
			{
				if ( entity.Id != 0 )
					Update(entity, nav);
				else
					Insert(entity, nav);
			}
			doc.Save(Source);
		}

		private void Update(IEntity entity, XPathNavigator nav)
		{
			//XPathNavigator nav = GetDocNodeById(entity);
			//nav.MoveToFirstChild();

			//XPathNavigator nav = doc.CreateNavigator();
			//nav.MoveToFollowing(this.ObjectType.TableName + "s", "");
			nav.MoveToFirstChild();
			if ( nav.MoveToId(entity.Id.ToString()) )
				WriteEntity(entity, nav);
			else
			{
				nav.MoveToParent();
				XmlWriter writer = nav.AppendChild();
				CreateXmlNodeEl(entity, writer);
				writer.Close();
				nav.MoveToNext();
			}

		}

		private void WriteEntity(IEntity entity, XPathNavigator nav)
		{
            ObjectDescription ObjectType = new ObjectDescription(entity.GetType());
			foreach ( PropertyInfo prop in ObjectType.Properties )
			{
				nav.MoveToFollowing(prop.Name, "");
				bool write = true;
				foreach ( object o in prop.GetCustomAttributes(true) )
				{
					if ( o is NonPersistentAttribute )
						write = false;
				}
				if ( write )
				{
					try
					{
						object value = ObjectType.GetPropertyValueForSave(prop.Name, entity);
						nav.SetTypedValue(value);
					}
					catch
					{
						throw new ApplicationException(string.Format("Unable to write attribute: {0}", prop.Name));
					}
				}
				nav.MoveToParent();
			}
		}

        //private XPathNavigator GetDocNodeById(IEntity entity)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(FileName);
        //    XmlReader reader = ReadXmlDocument();
        //    XPathNavigator nav = doc.ReadNode(reader).CreateNavigator();
        //    //XPathNavigator nav = 
        //    //XmlNode node; 
        //    if ( !nav.HasChildren )
        //        return null;//!!!!
        //    nav.MoveToId(entity.Id.ToString());
        //    return nav;
        //}

		private XmlReader ReadXmlDocument(ObjectDescription ObjectType)
		{
			XmlReaderSettings sett = new XmlReaderSettings();
			sett.IgnoreComments = true;
			sett.IgnoreProcessingInstructions = true; //??
			//sett.Schemas.Add("","");
			//sett.ValidationType = ValidationType.Schema;
#if false
			XmlReader xmlReader = XmlReader.Create(DbProvider.ConnectionString, sett);
#else
			XmlReader xmlReader = XmlReader.Create(Source, sett);
#endif
			xmlReader.ReadToFollowing(ObjectType.TableName);
			xmlReader = XmlReader.Create(xmlReader.ReadSubtree(), sett);
			xmlReader.ReadToDescendant(ObjectType.TableName);
			// The part of the document containing the selected Entity is loaded
			// Now one may use it to get the list of instances...
			return xmlReader;
		}

		private int Insert(IEntity entity, XPathNavigator nav)
		{
			//XPathNavigator nav = GetDocNodeById(entity);
			//XmlDocument doc = new XmlDocument();
			//doc.Load(DbProvider.ConnectionString);
			nav.MoveToFirstChild();
			//nav.MoveToId(entity.Id.ToString());
			int maxId = 0;
			do
			{
				int id = int.Parse(nav.GetAttribute("Id", ""));
				if ( id > maxId )
					maxId = id;
			} while ( nav.MoveToNext() );
			entity.Id = ++maxId;
			nav.MoveToParent();
			XmlWriter writer = nav.AppendChild();
			CreateXmlNodeEl(entity, writer);
			writer.Close();
			nav.MoveToNext();
			//nav.WriteSubtree(writer);
			//WriteEntity(entity , nav);
			//doc.Save(DbProvider.ConnectionString);
			//nav.WriteSubtree(XmlWriter.Create("C:\\myxml1.xml"));
			//
			return 0;
		}

		private void CreateXmlNodeEl(IEntity entity, XmlWriter writer)
		{
            ObjectDescription ObjectType = new ObjectDescription(entity.GetType());
			writer.WriteStartElement(ObjectType.TableName);
			foreach ( PropertyInfo prop in entity.GetType().GetProperties() )
			{
				if ( prop.Name == "Id" )
				{
					writer.WriteAttributeString("Id", entity.Id.ToString());
					continue;
				}
				if ( prop.Name == "Name" )
				{
					writer.WriteAttributeString("Name", entity.Name);
					continue;
				}

				bool write = true;
				foreach ( object o in prop.GetCustomAttributes(true) )
				{
					if ( o is NonPersistentAttribute )
						write = false;
				}

				if ( write )
				{
					writer.WriteStartElement(prop.Name);
					object value = null;
					try
					{
                        value = ObjectType.GetPropertyValueForSave(prop.Name, entity);
						writer.WriteValue(value);
					}
					catch
					{
						throw new ApplicationException(string.Format("Unable to write attribute: {0}", prop.Name));
					}
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
			//writer.Flush();
		}

        //private object PrepareForSave(object value)
        //{
        //    if ( value == null )
        //        value = "";
        //    if ( value.GetType().IsEnum )
        //        value = (int)value;
        //    if ( value is Image )
        //    {
        //        MemoryStream ms = new MemoryStream();//((Image)value).Size.Width * ((Image)value).Size.Width);
        //        ((Image)value).Save(ms, ((Image)value).RawFormat);
        //        value = ms.ToArray();
        //        //value = "";
        //    }
        //    return value;
        //}

		public void Delete(IEntity entity)
		{
			throw new Exception("The method or operation is not implemented.");
		}

        public string GetText()
        {
            XmlReaderSettings sett = new XmlReaderSettings();
            sett.IgnoreComments = true;
            sett.IgnoreProcessingInstructions = true; //??
            //sett.Schemas.Add("","");
            //sett.ValidationType = ValidationType.Schema;
#if false
			XmlReader xmlReader = XmlReader.Create(DbProvider.ConnectionString, sett);
#else
            XmlReader xmlReader = XmlReader.Create(Source, sett);
#endif
            xmlReader.ReadToFollowing("Data");
            return xmlReader.ReadOuterXml();
        }
	}
}
