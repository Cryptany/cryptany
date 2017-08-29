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
using System.Reflection;
using System.Data;
using System.Collections;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.DPO.DS
{
	public class DataSetLoader: ILoader
	{
		private Type _entityType;
		private Mapper _mapper;
		private bool _compiled = false;
		private DataSet _underlyngDataSet;
		private PersistentStorage _ps;
        private Dictionary<PropertyDescription,DataRelation> _foreignKeys = new Dictionary<PropertyDescription, DataRelation>();

		public DataSetLoader(PersistentStorage ps, Type entityType, DataSet dataSet)
		{
			_entityType = entityType;
			_underlyngDataSet = dataSet;
			_ps = ps;
		}

		public Type ReflectedType
		{
			get
			{
				return _entityType;
			}
		}

		public DataSet UnderlyngDataSet
		{
			get
			{
				return _underlyngDataSet;
			}
		}

		public Mapper Mapper
		{
			get
			{
				return _mapper;
			}
		}

        protected Dictionary<PropertyDescription, DataRelation> ForeignKeys
        {
            get
            {
                return _foreignKeys;
            }
        }

        public void Compile()
        {
			_mapper = new Mapper(ReflectedType, _ps);
            ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
            foreach (PropertyDescription prop in od.Relations)
            {
                if ( prop.IsOneToOneRelation )
                    continue;
				Mapper m = new Mapper(prop.RelatedType, _ps);
                RelationAttribute attr = prop.GetAttribute<RelationAttribute>();
                string parentCol = null;
                string childCol = null;
                DataTable table = UnderlyngDataSet.Tables[Mapper.TableName];
                string tableName = null;
                if ( prop.IsOneToManyRelation )
                {
                    parentCol = _mapper[od.IdField.Name];
                    childCol = m[attr.RelatedColumn];
                    tableName = m.TableName;
                }
                else // (prop.IsManyToManyRelation)
                {
                    parentCol = _mapper[od.IdField.Name];
                    childCol = attr.MtmRelationTableParentColumn;
                    tableName = attr.MamyToManyRelationTable;
                }
                bool ok = false;
                foreach ( DataRelation relation in table.ChildRelations )
                {
                    if ( relation.ChildTable.TableName.ToLower() == tableName.ToLower() &&
                        relation.ParentColumns[0].ColumnName.ToLower() == parentCol.ToLower() &&
                        relation.ChildColumns[0].ColumnName.ToLower() == childCol.ToLower() )
                    {
                        ForeignKeys.Add(prop, relation);
                        ok = true;
                        break;
                    }
                }
                if ( !ok )
                    throw new Exception("FUCK");
            }
            _compiled = true;
        }

		public List<EntityBase> LoadAll()
		{
			return LoadAll<EntityBase>();
		}

        public List<T> LoadAll<T>() where T : EntityBase
		{
			if ( !_compiled )
				Compile();

            ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
                

			if ( od.IsAggregated )
				throw new Exception("Cannot load aggregated types");
				//return _ps.GetEntities(ReflectedType);

			List<T> list = new List<T>(UnderlyngDataSet.Tables[Mapper.TableName].Rows.Count);
			foreach ( DataRow row in UnderlyngDataSet.Tables[Mapper.TableName].Rows )
			{
                if ( row.RowState == DataRowState.Deleted )
                    continue;
				TableAttribute tableAttr = od.DbTableAttribute;
				if ( tableAttr != null && tableAttr.Conditional )
					if ( !tableAttr.CheckConditions(row) )
						continue;

				T entity = od.CreateObject() as T;
				entity.BeginLoad();
                entity.CreatorPs = this._ps;
                entity[od.IdField.Name] = row[Mapper[od.IdField.Name]];
                entity.SourceRow = row;

				EntityCache cache = _ps.Caches[entity.GetType()];
				if ( !cache.Contains(entity.ID) )
					cache.Add(entity);
				else
				{
					//throw new Exception();
					entity = (T)cache[entity.ID];
				}

				list.Add(entity);
			}
			foreach ( T entity in list )
			{
				ObjectDescription ods = ClassFactory.GetObjectDescription(entity.GetType(), _ps);
				if ( !ods.IsWrapped )
					ReloadEntity(entity);// this is a reference type.... (I hate them!!!!)
				else
				{
					EntityBase e = null;//(EntityBase)ClassFactory.CreateObject(od.WrappedClass, _ps);//( entity as IWrapObject ).WrappedObject;
					//e.BeginLoad();
					//e.Ps = _ps;
					//e.SourceRow = entity.SourceRow;
                    e = _ps.GetEntityById(od.WrappedClass, entity.ID);
                    ( entity as IWrapObject ).WrappedObject = e;
					e.EndLoad();
					//ReloadEntity(e);
					//( entity as IWrapObject ).WrappedObject = e;
				}
				entity.EndLoad();

			}
			return list;
		}

		private DataRow GetUnderlyingRow(Type type, object id)
		{
            //ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType);
            //string sid = id is int ? id.ToString() : "'" + id.ToString() + "'";
            //DataView dv = UnderlyngDataSet.Tables[Mapper.TableName].DefaultView;
            //CreateFilterString("ID", dv, id);
            //DataRow row;
            //row = dv[0].Row;
            //return row;
		    return UnderlyngDataSet.Tables[Mapper.TableName].Rows.Find(id);
		}

        public EntityBase LoadEntityById(object id)
		{
			if ( !_compiled )
				Compile();

			ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);

			if ( od.IsAggregated )
				return _ps.GetEntityById(ReflectedType, id);

			DataRow row = GetUnderlyingRow(ReflectedType, id);
			if ( row == null )
				return null;
			
			TableAttribute tableAttr = od.DbTableAttribute;
			if ( tableAttr.Conditional )
				if ( !tableAttr.CheckConditions(row) )
					return null;

            EntityBase entity = od.CreateObject() as EntityBase;
			od.IdField.SetValue(entity, row[Mapper[od.IdField.Name]]);

			EntityCache cache = _ps.Caches[entity.GetType()];
			if ( !cache.Contains(entity.ID) )
				cache.Add(entity);
			else
				cache[entity.ID] = entity;

			ReloadEntity(entity);// this is a reference type.... (I hate them!!!!)
			return entity;
		}

        public void ReloadEntity(EntityBase entity)
		{
			if ( !_compiled )
				Compile();

			ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
			DataView dv = UnderlyngDataSet.Tables[Mapper.TableName].DefaultView;

			entity.BeginLoad();
            DataRow row = entity.SourceRow; //dv[0].Row;

			foreach ( PropertyDescription prop in od.Properties )
			{
				if ( prop.GetAttribute<NonPersistentAttribute>() != null )
					continue;
				if ( prop.IsId )
					continue;

				RelationAttribute relAttr = prop.GetAttribute<RelationAttribute>();
				if ( relAttr != null )
				{
                    if ( relAttr.RelationType == RelationType.ManyToMany )
					{
						GetMtmRelationValues(ref entity, row, relAttr, prop);
						continue;
					}
					else if ( relAttr.RelationType == RelationType.OneToOne )
					{
						GetOtoRelationValue(ref entity, row, relAttr, prop);
						continue;
					}
					else if ( relAttr.RelationType == RelationType.OneToMany )
					{
						GetOtmRealationValues(ref entity, row, relAttr, prop);
						continue;
					}
				}
				//DateTime dt1 = DateTime.Now;
                //prop.SetValue(entity, row[Mapper[prop.Name]]);
                (entity as EntityBase)[prop.Name] = row[Mapper[prop.Name]];
				//DateTime dt2 = DateTime.Now;
				//TimeSpan ts = dt2 - dt1;
			}
			entity.EndLoad();
		}

        private void GetOtmRealationValues(ref EntityBase entity, DataRow row,
			RelationAttribute relAttr, PropertyDescription property)
		{
			ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
			Cryptany.Core.DPO.Mapper m = new Mapper(relAttr.RelatedType, _ps);
		    DataRelation rel = ForeignKeys[property];
            //foreach ( DataRelation r in UnderlyngDataSet.Tables[Mapper.TableName].ChildRelations )
            //{
            //    if ( r.ChildTable == UnderlyngDataSet.Tables[m.TableName] )
            //        rel = r;
            //}
            //if ( rel == null )
            //    throw new Exception("The child relation was not found");

			( property.GetValue(entity) as IList ).Clear();

            string relatedTypeIdField = ClassFactory.GetObjectDescription(relAttr.RelatedType, _ps).IdField.Name;
            foreach ( DataRow childrow in entity.SourceRow.GetChildRows(rel) )
		    {
		        object childid = childrow[m[relatedTypeIdField]];
                if ( childid == null || childid == DBNull.Value )
                    continue;
                EntityBase e = _ps.GetEntityById(relAttr.RelatedType, childid);
                (property.GetValue(entity) as IList).Add(e);
            }
		}

        private void GetOtoRelationValue(ref EntityBase entity, DataRow row,
			RelationAttribute relAttr, PropertyDescription property)
		{
			Cryptany.Core.DPO.Mapper m = new Mapper(relAttr.RelatedType, _ps);
			//DataView dvChild = new DataView(UnderlyngDataSet.Tables[m.TableName]);
			object id = row[Mapper[property.Name]];
			//CreateFilterString(m[relAttr.RelatedColumn], dvChild, id);
			//object oId = GetIdAsObject(id);
            if ( id == null || id == DBNull.Value )
                return;
			IEntity e = _ps.GetEntityById(relAttr.RelatedType, id);
			property.SetValue(entity, e);
		}

        private void GetMtmRelationValues(ref EntityBase entity, DataRow row,
			RelationAttribute relAttr, PropertyDescription property)
		{
            DataRelation rel = ForeignKeys[property];
            //foreach ( DataRelation r in UnderlyngDataSet.Tables[Mapper.TableName].ChildRelations )
            //{
            //    if ( r.ChildTable == UnderlyngDataSet.Tables[relAttr.MamyToManyRelationTable] )
            //        rel = r;
            //}
            //if ( rel == null )
            //    throw new Exception("The child relation was not found");

            (property.GetValue(entity) as IList).Clear();
            foreach ( DataRow childrow in entity.SourceRow.GetChildRows(rel) )
            {
                object childid = childrow[relAttr.MtmRelationTableChildColumn];
                if ( childid == null || childid == DBNull.Value )
                    continue;
                EntityBase e = _ps.GetEntityById(relAttr.RelatedType, childid);
                (property.GetValue(entity) as IList).Add(e);
            }
		}

        //private void CreateFilterString(string AttrName, DataView dv, object id)
        //{
        //    //int iid;
        //    //if ( id is int )
        //    //    dv.RowFilter = AttrName + "=" + id.ToString();
        //    //    else if (id is string && int.TryParse(id as string, out iid))
        //    //    dv.RowFilter = AttrName + "=" + iid.ToString();
        //    //else
        //        dv.RowFilter = AttrName + "='" + id.ToString() + "'";
        //}

		private object GetRowId(DataRow row)
		{
			object id = row[Mapper[ClassFactory.GetObjectDescription(ReflectedType, _ps).IdField.Name]];
			return id;
		}



		List<T> ILoader.LoadAll<T>()// where T : EntityBase
		{
			List<EntityBase> list = LoadAll();
			return list.ConvertAll<T>(delegate(EntityBase e)
			{
				return e as T;
			});
		}

		public List<EntityBase> LoadByFieldValue(string field, object value)
		{
			throw new Exception("The method or operation is not implemented.");
		}

	}
}
