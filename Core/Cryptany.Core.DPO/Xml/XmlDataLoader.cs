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
using System.Collections;

namespace Cryptany.Core.DPO.Xml
{
	public class XmlDataLoader : ILoader
	{
		private PersistentStorage _ps = null;
		private Type _type;
		private Mapper _mapper;
		private XmlReader _sourceReader;

		public XmlDataLoader(XmlReader reader, PersistentStorage ps, Type type)
		{
			_type = type;
			_ps = ps;
			_sourceReader = reader;
			_mapper = new Mapper(type, ps);
		}

		public XmlDataLoader(string fileName, string entitiesTagPath, PersistentStorage ps, Type type)
		{
			_type = type;
			_ps = ps;
			//_fileName = fileName;
			StreamReader streamReader = new StreamReader(fileName);
			XmlReaderSettings sett = new XmlReaderSettings();
			sett.CloseInput = true;
			sett.IgnoreComments = true;
			sett.IgnoreProcessingInstructions = true;
			sett.IgnoreWhitespace = true;
			XmlReader r = XmlReader.Create(streamReader, sett);
			_sourceReader = NavigateToNode(entitiesTagPath, r);
			_mapper = new Mapper(type, ps);
		}

		public Type ReflectedType
		{
			get
			{
				return _type;
			}
		}

		public Mapper Mapper
		{
			get
			{
				return _mapper;
			}
		}

		private XmlReader NavigateToNode(string nodePath, XmlReader reader)
		{
			string[] path = nodePath.Split('\\');
			if (reader.EOF)
				return null;
			foreach (string s in path)
				reader.ReadToDescendant(s);
			return reader.ReadSubtree();
		}

		public List<T> LoadAll<T>() where T : EntityBase
		{
			return new List<T>();
		}

		public List<EntityBase> LoadAll()
		{
			List<EntityBase> list = new List<EntityBase>();
			ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
			XmlReader reader = NavigateToNode(Mapper.TableName, _sourceReader);
			string entNodeName = "entity";

			reader.ReadToDescendant(od.ObjectType.Name);
			// Now we are at the first record of the needed table

			while (!reader.EOF)
			{
				XmlReader rdr = reader.ReadSubtree();// get the subtree of the entity instance
				rdr.Read();// Get the root node of the entity instance
				EntityBase entity = ReadObject(rdr, od);
				//SkipWhiteSpace(reader);
				_ps.Caches[ReflectedType].Add(entity);
				reader.ReadToFollowing(od.ObjectType.Name);
				list.Add(entity);
			}
			SetOtoRelations(list);
			SetOtmRelations(list);
			SetMtmRelations(list);
			return list;
		}

		protected EntityBase ReadObject(XmlReader reader, ObjectDescription od)
		{
			EntityBase e = (EntityBase)od.CreateObject();
			e.BeginLoad();

			if (reader.Name == od.ObjectType.Name)
			{
				e.ID = ReadId(reader.GetAttribute("id"));
				reader.Read();
				do
				{
					SkipWhiteSpace(reader);
					if (reader.Name == Mapper.TableName &&
						reader.NodeType == XmlNodeType.EndElement)
						break;// The entity have been read
					string propName = reader.Name;
					if (od.Properties.ContainsName(Mapper.GetByDbName(propName))) //   != null)
					{
						PropertyDescription property = od.Properties[Mapper.GetByDbName(propName)];
						bool doRead = true;
						if (property.IsNonPersistent)
							doRead = false;
						if (!doRead)
						{
							reader.Read();
							continue;
						}
						object value = ReadValue(property, reader);

						if (property.IsMapped)
							e[property.Name] = value;
						e.SourceRawValues.Add(propName, value);
						//od.SetReadPropertyValue(propName, e, value);
					}
				} while (reader.Name != Mapper.TableName && reader.NodeType != XmlNodeType.EndElement);
			}
			//}
			e.EndLoad();
			return e;
		}

		private object ReadValue(PropertyDescription property, XmlReader reader)
		{
			{
				object o = null;
				try
				{
					if (property.PropertyType.Name == "Byte[]")
					{
						object b = reader.ReadElementContentAs(typeof(byte[]), null);
						MemoryStream mem = new MemoryStream(b as byte[]);
						o = mem.ToArray();
					}
					else if (property.PropertyType.IsEnum)
					{
						int i = reader.ReadElementContentAsInt();
						o = i;
					}
					else if (property.IsManyToManyRelation)
					{
						o = ReadMtmValues(property, reader);
					}
					else
					{
						string s = reader.ReadElementContentAsString();
						o = Convert.ChangeType(s, property.PropertyType);
					}
				}
				catch (Exception e)
				{
					throw new Exception("Unable to read value", e);
					o = null;
				}
				return o;
			}
		}

		private object ReadMtmValues(PropertyDescription property, XmlReader reader)
		{
			ArrayList a = new ArrayList();
			for (reader.ReadToDescendant("Item"); reader.NodeType != XmlNodeType.EndElement; reader.Read())
			{
				string s = reader.ReadString();
				a.Add(ReadId(s));
			}
			return a;
		}
		
		private void SkipWhiteSpace(XmlReader reader)
		{
			while(reader.NodeType != XmlNodeType.Element)
			{
				reader.Read();
			} 
		}

		private object ReadId(string id)
		{
			int i = 0;
			if (int.TryParse(id, out i))
				return i;
			Guid guid = new Guid(id);
			return guid;
		}

		public void SetOtoRelations(List<EntityBase> entities)
		{
			List<PropertyDescription> props = new List<PropertyDescription>();
			foreach (PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Relations)
			{
				if (p.IsOneToOneRelation && !p.IsNonPersistent && !p.IsInternal)
					props.Add(p);
			}
			foreach (EntityBase e in entities)
			{
				e.BeginLoad();
				foreach (PropertyDescription p in props)
				{
					e[p.Name] = _ps.GetEntityById(p.RelatedType, e.SourceRawValues[Mapper[p.Name]]);
				}
				e.EndLoad();
			}
		}

		public void SetOtmRelations(List<EntityBase> entities)
		{
			List<PropertyDescription> props = new List<PropertyDescription>();
			foreach (PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Relations)
			{
				if (p.IsOneToManyRelation && !p.IsNonPersistent && !p.IsInternal)
				{
					if (!_ps.IsLoaded(p.RelatedType))
						_ps.GetEntities(p.RelatedType);
					props.Add(p);
				}
			}

			//SqlDataReader reader;
			foreach (EntityBase entity in entities)
			{
				entity.BeginLoad();
				foreach (PropertyDescription p in props)
				{
					ObjectDescription od = ClassFactory.GetObjectDescription(p.RelatedType, _ps);
					PropertyDescription pd = od.Properties[p.RelationAttribute.RelatedColumn];
					Indexes indexes = _ps.Caches[p.RelatedType].RawIndexes;
					Cryptany.Core.DPO.Mapper m = new Mapper(p.RelatedType, _ps);
					List<EntityBase> list = indexes[m[pd.Name]][entity.ID];
					if (list != null)
						foreach (EntityBase e in list)
							(p.GetValue(entity) as IList).Add(e);
				}
				entity.EndLoad();
			}
		}

		public void SetMtmRelations(List<EntityBase> entities)
		{
			foreach (EntityBase e in entities)
			{
				List<PropertyDescription> l = new List<PropertyDescription>();
				foreach (PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Properties)
				{
					if (!p.IsManyToManyRelation)
						continue;
					l.Add(p);
				}
				e.BeginLoad();
				foreach (PropertyDescription p in l)
				{
					foreach (object id in e.SourceRawValues[p.Name] as IList)
					{
						(e[p.Name] as IList).Add(_ps.GetEntityById(p.RelatedType, id));
					}
				}				
				e.EndLoad();
			}
		}
		
		public EntityBase LoadEntityById(object Id)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void ReloadEntity(EntityBase entity)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#region ILoader Members


		List<T> ILoader.LoadAll<T>()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public List<EntityBase> LoadByFieldValue(string field, object value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
