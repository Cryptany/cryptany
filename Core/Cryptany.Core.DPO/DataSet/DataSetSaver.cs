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
using System.Data;
using System.Collections;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO.DS
{
    public class DataSetSaver : ISaver
    {
        private Type _entityType;
        private Mapper _mapper;
        private bool _compiled = false;
        private DataSet _underlyngDataSet;
        private PersistentStorage _ps;

        public DataSetSaver(PersistentStorage ps, Type entityType, DataSet dataSet)
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

        public PersistentStorage Ps
        {
            get
            {
                return _ps;
            }
        }

        public void Compile()
        {
            _mapper = new Mapper(ReflectedType, _ps);
            _compiled = true;
        }

        public void Save(List<EntityBase> entities)
        {
            foreach (EntityBase entity in entities)
                Save(entity);
        }

        public void Save(EntityBase entity)
        {
            if (entity.State == EntityState.Unchanged || entity.State == EntityState.NewDeleted)
                return;
            if (!_compiled)
                Compile();

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

            DataRow row = GetUnderlyingRow(ent);

            bool isNew = ent.State == EntityState.New;

            if (isNew && !_ps.Caches[ent.GetType()].Contains(ent.ID))
                _ps.Caches[ent.GetType()].Add(ent);


            if (ent.State == EntityState.Deleted || ent.State == EntityState.NewDeleted)
            {
                if (_ps.Caches[ent.GetType()].Contains(ent.ID))
                    _ps.Caches[ent.GetType()].Delete(ent);
            }
            //else
            {
                ProcessProperties(ref ent, row);
                if (isNew)
                {
                    UnderlyngDataSet.Tables[Mapper.TableName].Rows.Add(row);
                    //UpdateParentEntities(ent);
                }
            }
            if (ent.State == EntityState.Deleted)
                row.Delete();
            ent.AcceptChanges();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns>If there is now row corresponding to the entity (because it has been recently created)
        /// then a new row is created</returns>
        private DataRow GetUnderlyingRow(IEntity entity)
        {
            ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
            DataView dv = new DataView(UnderlyngDataSet.Tables[Mapper.TableName]);
            DataRow row;
            if (entity.State == EntityState.New)
            {
                row = dv.Table.NewRow();
                if (row[Mapper[od.IdField.Name]] == null || row[Mapper[od.IdField.Name]] == DBNull.Value)
                    entity.ID = row[Mapper[od.IdField.Name]] = Guid.NewGuid();
                else
                    entity.ID = row[Mapper[od.IdField.Name]];
            }
            else
            {
                object id = od.IdField.GetValue(entity);

                row = dv.Table.Rows.Find(id);
            }
            return row;
        }

        private IEntity ProcessProperties(ref EntityBase entity, DataRow row)
        {
            ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType, _ps);
            List<PropertyDescription> l = new List<PropertyDescription>();
            foreach (PropertyDescription prop in od.Properties)
            {
                if (prop.GetAttribute<NonPersistentAttribute>() != null)
                    continue;
                if (prop.IsId)
                    continue;

                RelationAttribute relAttr = prop.GetAttribute<RelationAttribute>();
                if (relAttr != null)
                {
                    if (relAttr.RelationType == RelationType.ManyToMany)
                    {
                        l.Add(prop);
                        continue;
                    }
                    else if (relAttr.RelationType == RelationType.OneToOne)
                    {
                        SetOtoRelationValue(ref entity, row, relAttr, prop);
                        continue;
                    }
                    else if (relAttr.RelationType == RelationType.OneToMany)
                    {
                        l.Add(prop);
                        continue;
                    }
                }
                object val = prop.GetValue(entity);
                if (val != null)
                    row[Mapper[prop.Name]] = val;
                else
                    row[Mapper[prop.Name]] = DBNull.Value;
            }
            entity.AcceptChanges();
            foreach (PropertyDescription prop in l)
            {
                RelationAttribute relAttr = prop.GetAttribute<RelationAttribute>();
                if (relAttr.RelationType == RelationType.ManyToMany)
                {
                    SetMtmRelationValues(ref entity, row, relAttr, prop);
                }
                else if (relAttr.RelationType == RelationType.OneToMany)
                {
                    SetOtmRelationValues(ref entity, row, relAttr, prop);
                }
            }
            return entity;
        }

        private void SetOtmRelationValues(ref EntityBase entity, DataRow row,
            RelationAttribute relAttr, PropertyDescription property)
        {
            if (entity.State == EntityState.Deleted)
            {
                if (relAttr.DeleteMode == DeleteMode.Single)
                {
                }
                else if (relAttr.DeleteMode == DeleteMode.Cascade)
                {
                    foreach (EntityBase e in (IList)property.GetValue(entity))
                    {
                        e.Delete();
                        _ps.SaveInner(e);
                    }
                }
            }
            //else
            {
                IList l = ((IList)property.GetValue(entity));
                object[] objs = new object[l.Count];
                l.CopyTo(objs, 0);
                foreach (EntityBase e in objs)
                {
                    ObjectDescription od = ClassFactory.GetObjectDescription(relAttr.RelatedType, _ps);
                    od.Properties[relAttr.RelatedColumn].SetValue(e, entity);
                    _ps.SaveInner(e);
                    if (e.State == EntityState.NewDeleted || e.State == EntityState.Deleted)
                        (property.GetValue(entity) as IList).Remove(e);
                }
            }
        }

        private void SetOtoRelationValue(ref EntityBase entity, DataRow row,
            RelationAttribute relAttr, PropertyDescription property)
        {
            if (entity.State == EntityState.Deleted)
                return;
            Mapper m = new Mapper(relAttr.RelatedType, _ps);
            EntityBase e = (EntityBase)property.GetValue(entity);
            if (e != null)
            {
                //if (e.State == EntityState.New)
                _ps.SaveInner(e);
                object id = ClassFactory.GetObjectDescription(m.EntityType, _ps).IdField.GetValue(e);
                row[Mapper[property.Name]] = id;
            }
            else
                row[Mapper[property.Name]] = DBNull.Value;

        }

        private void SetMtmRelationValues(ref EntityBase entity, DataRow row,
            RelationAttribute relAttr, PropertyDescription property)
        {
            DataView dv = new DataView(UnderlyngDataSet.Tables[relAttr.MamyToManyRelationTable]);
            string rowfilter = CreateFilterString(relAttr.MtmRelationTableParentColumn, entity.ID.ToString());
            string rowfilter2 = string.Format(" and {0} = ", relAttr.MtmRelationTableChildColumn);

            List<object> ids = new List<object>();

            foreach (EntityBase e in (IList)property.GetValue(entity))
            {
                if (e.State == EntityState.New)
                {
                    _ps.SaveInner(e);
                }
                if (e.State == EntityState.Deleted)
                {
                    //the appropriate actions are performed later
                }
                if (e.State == EntityState.Changed)
                {
                    _ps.SaveInner(e);
                }

                rowfilter2 = string.Format(" and {0}", CreateFilterString(relAttr.MtmRelationTableChildColumn, e.ID));
                if (dv.Table.Select(rowfilter + rowfilter2).Length == 0)
                {
                    DataRow r = dv.Table.NewRow();
                    // TEMP !!!!
                    if (r.Table.PrimaryKey[0].DataType == typeof(Guid))
                        r[r.Table.PrimaryKey[0]] = Guid.NewGuid();
                    //
                    r[relAttr.MtmRelationTableParentColumn] = entity.ID;
                    r[relAttr.MtmRelationTableChildColumn] = e.ID;
                    dv.Table.Rows.Add(r);
                }
                ids.Add(e.ID);
            }

            DataView dMtm = _underlyngDataSet.Tables[relAttr.MamyToManyRelationTable].DefaultView;
            dMtm.RowFilter = "";
            string filter = CreateFilterString(relAttr.MtmRelationTableParentColumn, entity.ID);
            foreach (DataRow dr in dMtm.Table.Select(filter))
            {
                if (!ids.Contains(dr[relAttr.MtmRelationTableChildColumn]))
                    dr.Delete();
            }
        }

        private void UpdateParentEntities(EntityBase entity)
        {
            foreach (PropertyDescription property in ClassFactory.GetObjectDescription(entity.GetType(), _ps).Properties)
            {
                if (property.IsOneToOneRelation)
                {
                    EntityBase e = property.GetValue<EntityBase>(entity);
                    if (e != null && e.State == EntityState.Unchanged)
                        _ps.ReloadEntity(e);
                }
            }
        }

        private string CreateFilterString(string AttrName, object id)
        {
            if (id is int)
                return AttrName + "=" + id.ToString();
            else
                return AttrName + "='" + id.ToString() + "'";
        }
    }
}