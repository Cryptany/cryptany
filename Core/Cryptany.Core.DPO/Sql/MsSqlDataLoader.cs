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
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.Predicates;

namespace Cryptany.Core.DPO.Sql
{
	public class MsSqlDataLoader : ILoader
	{
		//private readonly SqlConnection _connection;
		readonly string _connectionString = string.Empty;
		public List<Type> _loaded = new List<Type>();
		private readonly PersistentStorage _ps;
		private readonly Type _type;
		private readonly Mapper _mapper;
		public DateTime _dt;
		private string _selectAll;
		private string _selectById;
		private string _idParamaterName = "@@id";

		public MsSqlDataLoader(PersistentStorage ps, string connectionString, Type type)
		{
			_type = type;
			_mapper = new Mapper(type, ps);
			_connectionString = connectionString;
			_ps = ps;
			//_connection = new SqlConnection(connectionString);
		}

		//public SqlConnection Connection
		//{
		//    get
		//    {
		//        //return new SqlConnection(_connectionString);
		//        return _connection;
		//    }
		//}

		public string ConnectionString
		{
			get
			{
				return _connectionString;
			}
		}

		public PersistentStorage Ps
		{
			get
			{
				return _ps;
			}
		}
		
		private string BuildSqlSelect(Type type)
		{
			return BuildSqlSelect(type, "");
		}

		private string BuildSqlSelect(Type type, string whereString)
		{
			ObjectDescription od = new ObjectDescription(type, _ps);
			string select = "SELECT ";
			foreach (PropertyDescription property in od.MappedProperties)
			{
				//if ( property.IsRelation || property.IsNonPersistent || property.IsInternal )
				//    continue;
				if (select != "SELECT ")
					select += ", ";
				select += Mapper[property.Name, true];
			}
			select += " FROM ";
			select += Mapper.FullTableName;

			string where = " WHERE ";
			TableAttribute attr = od.DbTableAttribute;
			if (attr != null && attr.ToString() != "")
				where += "( " + attr.ToString() + " )";
			if ( whereString != "" )
			{
				if(attr != null && attr.ToString() != "")
					where += " AND ( " + whereString + " )";
				else
					where += whereString;

			}
			if (where.Trim() != "WHERE")
				select += where;

			return select;
		}

		private string BuildSqlSelect(Type type, object id)
		{
			if ( _selectById == null )
			{
				foreach ( GetByIdCommandAttribute attr in type.GetCustomAttributes(typeof(GetByIdCommandAttribute), true) )
				{
					_selectById = attr.GetCommand();
					_idParamaterName = attr.GetIdParameterName();
				}
				if ( _selectById == null )
				{
					string where = "";
					ObjectDescription od = new ObjectDescription(type,_ps);
                    where += Mapper[od.IdField.Name, true] + " = ";
					where += _idParamaterName;
					_selectById = BuildSqlSelect(type, where);
				}
			}
			return _selectById.Replace(_idParamaterName, IdToString(id));
		}

		private string BuildGetMTMQuery(PropertyDescription property)
		{
			return BuildGetMTMQuery(property, null);
		}

		private string BuildGetMTMQuery(PropertyDescription property, object entityId)
		{
			Cryptany.Core.DPO.Mapper m = new Mapper(property.ReflectedObject.ObjectType, _ps);
			string mtmTableName = "";
			if ( !string.IsNullOrEmpty(property.RelationAttribute.SchemaName) )
				mtmTableName = property.RelationAttribute.SchemaName + "."
					+ property.RelationAttribute.MamyToManyRelationTable;
			else if ( m.SchemaName != "" )
				mtmTableName = m.SchemaName + "." + property.RelationAttribute.MamyToManyRelationTable;
			else
				mtmTableName = property.RelationAttribute.MamyToManyRelationTable;
			string select = "SELECT t1.";
			select += property.RelationAttribute.MtmRelationTableParentColumn + ", ";
			select += property.RelationAttribute.MtmRelationTableChildColumn + " ";
			select += "FROM " + mtmTableName + " AS t1 ";

			ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
			TableAttribute attr = od.DbTableAttribute;
			if ( attr != null && attr.ToString() != "" )
			{
				string select2 = "INNER JOIN " + Mapper.FullTableName + " AS t2 ON ";
				select2 += "t1." + property.RelationAttribute.MtmRelationTableParentColumn + " = ";
                select2 += "t2." + Mapper[od.IdField.Name, true];
				select2 += " AND t2." + attr.ToString();
				select += select2;
			}
			if ( entityId != null )
			{
				string where = " WHERE t1." + m[od.IdField.Name, true] + " = " + IdToString(entityId);
				select += where;
			}
			return select;
		}

		private string IdToString(object id)
		{
			if ( id is Guid || id is string )
				return "'" + id.ToString() + "'";
			return id.ToString();
		}

		private void GetReader(string query, List<object[]> values)
		{
			
			using (SqlConnection _connection = new SqlConnection(_connectionString))
			{

				using (SqlCommand command = new SqlCommand(query, _connection))
				{
					
					_connection.Open();
					using (SqlDataReader reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							object[] vals = new object[reader.FieldCount];
							reader.GetValues(vals);
							values.Add(vals);
						}
					}
				}
			}
			
		}

		public List<T> LoadAll<T>() where T : EntityBase
		{
			List<EntityBase> list = LoadAll();
			List<T> result = list.ConvertAll<T>(
				delegate(EntityBase e)
				{
					return e as T;
				});
			return result;
		}

		public List<EntityBase> LoadAll()
		{
			ObjectDescription od = new ObjectDescription(ReflectedType, _ps);
			string select = null;
			if ( _selectAll == null )
				_selectAll = select = BuildSqlSelect(ReflectedType);
			else
				select = _selectAll;
			//DateTime t1 = _dt = DateTime.Now;
			List<EntityBase> list = FillEntityList(select);
			if (list.Count > 0)
			{
				SetOtoRelations(list);
				SetOtmRelations(list);
				SetMtmRelations(list);
			}
			//}
			foreach ( EntityBase entity in list )
				entity.EndLoad();
			return list;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader">Will be closed</param>
		/// <returns></returns>
		private List<EntityBase> FillEntityList(string select)
		{
			ObjectDescription od = new ObjectDescription(ReflectedType, _ps);
			List<EntityBase> list = new List<EntityBase>();
			//int i = 0;
			//DateTime t21 = _dt = DateTime.Now;
           
                using (SqlConnection _connection = new SqlConnection(_connectionString))
                {

                    using (SqlCommand command = new SqlCommand(select, _connection))
                    {
                         try
                         {
                             _connection.Open();
                             using (SqlDataReader reader = command.ExecuteReader())
                             {
                                 if (reader != null && reader.HasRows)
                                 {
                                     while (reader.Read())
                                     {
                                         //i++;
                                         EntityBase entity = (EntityBase) od.CreateObject();
                                         entity.BeginLoad();
                                         for (int fieldIndex = 0; fieldIndex < reader.FieldCount; fieldIndex++)
                                         {
                                             string dbName = reader.GetName(fieldIndex);
                                             string name = Mapper.GetByDbName(dbName);
                                             if (string.IsNullOrEmpty(name))
                                                 continue;
                                             object o = reader.GetValue(fieldIndex);
                                             if (od.Properties.ContainsName(name) && od.Properties[name].IsMapped)
                                                 entity[name] = o;
                                             entity.SourceRawValues.Add(dbName, o);
                                         }
                                         list.Add(entity);

                                         if (!Ps.Caches[ReflectedType].Contains(entity.ID))
                                             Ps.Caches[ReflectedType].Add(entity);
                                         else
                                             Ps.Caches[ReflectedType][entity.ID] = entity;

                                     }
                                 }
                             }
                         }
                         catch (SqlException sex)
                        {
                            Trace.WriteLine("DPO: " + sex);
                            if (sex.Number == -2 || _connection.State == ConnectionState.Closed) //timeout или не открылось соединение
                            {
                                
                            }
                            else
                                throw;
                        }
                        _loaded.Add(ReflectedType);

                    }
                }
           
		    //DateTime t22 = DateTime.Now;
			//TimeSpan ts2 = t22 - t21;
			return list;
		}

		public EntityBase LoadEntityById(object id)
		{
			string select = BuildSqlSelect(_type, id);
			List<EntityBase> list  = FillEntityList(select);
			if (list.Count > 1)
				throw new Exception("LoadEntityById: query returned too much entities.");
			if (list.Count < 1)
				return null;
			//    throw new Exception("LoadEntityById: query return am empty result.");
			SetOtoRelations(list[0]);
			SetOtmRelations(list[0]);
			SetMtmRelations(list[0]);
			foreach ( EntityBase entity in list )
				entity.EndLoad();
			return list[0];
		}

		public List<EntityBase> LoadByFieldValue(string field, object value)
		{
			string select = BuildSqlSelect(_type, _mapper[field, true] + " = " + IdToString(value));
			SqlDataReader reader = null;
			List<EntityBase> list = FillEntityList(select);
			
			if (list.Count > 0)
			{
				SetOtoRelations(list);
				SetOtmRelations(list);
				SetMtmRelations(list);
			}
			
			return list;
		}

		public List<List<EntityBase>> LoadAllRelatedTypes()
		{
			ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
			List<List<EntityBase>> list = new List<List<EntityBase>>();
			foreach ( PropertyDescription p in od.Relations )
			{
				List<EntityBase> l =Ps.GetEntities(p.RelatedType);////
				list.Add(l);
			}
			return list;
		}

		//public List<List<EntityBase>> LoadAllRelatedTypesRec(Type type)
		//{
		//    List<List<EntityBase>> list = new List<List<EntityBase>>();
		//    ObjectDescription od = new ObjectDescription(type);
		//    foreach ( PropertyDescription prop in od.Relations )
		//    {
		//        bool load = true;
		//        foreach ( Type t in _loaded )
		//            if ( t == prop.RelatedType )
		//            {
		//                load = false;
		//                break;
		//            }
		//        if ( load )
		//        {
		//            LoadAll(prop.RelatedType);
		//            LoadAllRelatedTypesRec(prop.RelatedType);
		//        }
		//    }
		//    return list;
		//}

		public void SetOtoRelations(List<EntityBase> entities)
		{
			List<PropertyDescription> props = new List<PropertyDescription>();
			foreach ( PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Relations )
			{
				if (p.IsOneToOneRelation && !p.IsNonPersistent && !p.IsInternal)
				{
					if (!Ps.IsLoaded(p.RelatedType))
						Ps.GetEntities(p.RelatedType);
					props.Add(p);
				}
			}
			foreach ( EntityBase e in entities )
			{
				foreach ( PropertyDescription p in props )
				{
					if ( e.SourceRawValues[Mapper[p.Name]] != null )
					{
						if ( e.SourceRawValues[Mapper[p.Name]] is Guid && (Guid)e.SourceRawValues[Mapper[p.Name]] != Guid.Empty )
							e[p.Name] = Ps.GetEntityById(p.RelatedType, e.SourceRawValues[Mapper[p.Name]]);
						else
							e[p.Name] = null;
					}
				}
			}
		}

		public void SetOtmRelations(List<EntityBase> entities)
		{
			List<PropertyDescription> props = new List<PropertyDescription>();
			foreach ( PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Relations )
			{
				if (p.IsOneToManyRelation && !p.IsNonPersistent && !p.IsInternal)
				{
					if (!Ps.IsLoaded(p.RelatedType))
						Ps.GetEntities(p.RelatedType);
					props.Add(p);
				}
			}
			
			//SqlDataReader reader;
			foreach ( EntityBase entity in entities )
			{
				foreach ( PropertyDescription p in props )
				{
					ObjectDescription od = ClassFactory.GetObjectDescription(p.RelatedType,_ps);
					PropertyDescription pd = od.Properties[p.RelationAttribute.RelatedColumn];
					Indexes indexes = Ps.Caches[p.RelatedType].RawIndexes;
					Cryptany.Core.DPO.Mapper m = new Mapper(p.RelatedType, _ps);
					List<EntityBase> list = null;
					if ( indexes[m[pd.Name]].ContainsKey(entity.ID) )
						list = indexes[m[pd.Name]][entity.ID];
					if (list != null)
						foreach (EntityBase e in list)
						(p.GetValue(entity) as IList).Add(e);
				}
			}
		}

		public void SetMtmRelations(List<EntityBase> entities)
		{
			foreach (PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Relations)
			{
				if (p.IsManyToManyRelation && !p.IsNonPersistent && !p.IsInternal)
				{
					//
					if (!Ps.IsLoaded(p.RelatedType))
						Ps.GetEntities(p.RelatedType);

					//DateTime dt1 = DateTime.Now;
					List<object[]> list = new List<object[]>();
					GetReader(BuildGetMTMQuery(p), list);




					foreach (object[] val in list)
					{
						//DateTime dt3 = DateTime.Now;
						//int i = 0;

						//DateTime dt7 = DateTime.Now;
						EntityBase parentEntity = Ps.GetEntityById(p.ReflectedObject.ObjectType, val[0]);
						EntityBase childEntity = Ps.GetEntityById(p.RelatedType, val[1]);
						//DateTime dt8 = DateTime.Now;
						//TimeSpan ts3 = dt8 - dt7;
						//(parentEntity[p.Name] as IList).Add(childEntity);
                        IList parent = p.GetValue(parentEntity) as IList;
                        if (parent!=null)
                            parent.Add(childEntity);
						//i++;
					}
					//DateTime dt2 = DateTime.Now;
					//string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
					//path = System.IO.Path.GetDirectoryName(path);
					//path = System.IO.Path.Combine(path, "log.txt");
					//System.IO.StreamWriter w = new System.IO.StreamWriter(path, true);
					//w.WriteLine();
					//w.Write("Type name: ");
					//w.Write(p.ReflectedObject.ObjectType.Name);
					//w.Write(", property name: ");
					//w.Write(p.Name);
					//w.Write(", count: ");
					//w.Write(i);
					//w.Write(", estimated time: ");
					//w.WriteLine(dt2 - dt3);
					//w.Flush();
					//w.Close();
					//TimeSpan ts1 = dt2 - dt1;
					//TimeSpan ts2 = dt2 - dt3;
				}
			}
		}

		public void SetAllRelations(List<EntityBase> entities)
		{
			throw new NotImplementedException();
		}

		public void SetOtoRelations(EntityBase entity)
		{
			foreach ( PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType,_ps).Relations )
			{
				if ( p.IsOneToOneRelation && !p.IsNonPersistent && !p.IsInternal )
					if ( entity.SourceRawValues[Mapper[p.Name]] != null )
					if ( entity.SourceRawValues[Mapper[p.Name]] is Guid && (Guid)entity.SourceRawValues[Mapper[p.Name]] != Guid.Empty )
					entity[p.Name] = Ps.GetEntityById(p.RelatedType, entity.SourceRawValues[Mapper[p.Name]]);
			}
		}

		public void SetOtmRelations(EntityBase entity)
		{
			foreach ( PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType, _ps).Relations )
			{
				if ( p.IsOneToManyRelation && !p.IsNonPersistent && !p.IsInternal )
				{
					ObjectDescription od = ClassFactory.GetObjectDescription(p.RelatedType, _ps);
					PropertyDescription pd = od.Properties[p.RelationAttribute.RelatedColumn];
					Mapper m = new Mapper(pd.ReflectedObject.ObjectType, _ps);
					string select = "SELECT " + m[pd.ReflectedObject.IdField.Name, true] + " FROM " + m.FullTableName + " AS t" +
						" WHERE " + m[pd.Name, true] + " = " + IdToString(entity.ID);
					List<object[]> list = new List<object[]>();
					GetReader(select, list);

					foreach (object[] val in list)
					{
						object id = val[0];
						EntityBase e = _ps.GetEntityById(p.RelatedType, id);
						if (e != null)
							(p.GetValue(entity) as IList).Add(e);

					}


				}
			}
		}

		public void SetMtmRelations(EntityBase entity)
		{
			foreach ( PropertyDescription p in ClassFactory.GetObjectDescription(ReflectedType, _ps).Relations )
				if ( p.IsManyToManyRelation && !p.IsNonPersistent && !p.IsInternal )
			{
				
				if ( !Ps.IsLoaded(p.RelatedType) )
					Ps.GetEntities(p.RelatedType);

				List<object[]> list = new List<object[]>();
				GetReader(BuildGetMTMQuery(p, entity.ID), list);
				foreach (object[] val in list)
				{
					EntityBase parentEntity = Ps.GetEntityById(p.ReflectedObject.ObjectType, val[0]);
					EntityBase childEntity = Ps.GetEntityById(p.RelatedType, val[1]);
					(p.GetValue(parentEntity) as IList).Add(childEntity);
				}
			}
		}

		public void SetAllRelations(EntityBase entity)
		{
			throw new NotImplementedException();
		}


		#region ILoader Members

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

		public void ReloadEntity(EntityBase entity)
		{
          
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}
}
