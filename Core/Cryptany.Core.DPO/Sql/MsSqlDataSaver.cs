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
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO.Sql
{
	public class MsSqlDataSaver : ISaver
	{
		private readonly Type _entityType;
	    private readonly PersistentStorage _ps;

	    public MsSqlDataSaver( PersistentStorage ps, Type entityType )
		{
			_entityType = entityType;
			_ps = ps;
		}

		public PersistentStorage Ps
		{
			get
			{
				return _ps;
			}
		}

		public Type ReflectedType
		{
			get
			{
				return _entityType;
			}
		}

		public System.Data.DataSet UnderlyngDataSet
		{
			get
			{
				throw new Exception("This is inapplicable to SQL Server-oriented saver");
			}
		}

		public void Save(List<EntityBase> entities)
		{
			foreach ( EntityBase entity in entities )
				Save(entity);
		}

		public void Save(EntityBase entity)
		{
			if ( entity.State == EntityState.Unchanged || entity.State == EntityState.NewDeleted )
				return;

			ObjectDescription od = ClassFactory.GetObjectDescription(entity.GetType(), _ps);
			EntityBase ent;
			if ( od.IsWrapped )
			{
				ent = ((IWrapObject) entity).WrappedObject;
				_ps.SaveInner(ent);
				return;
			}
		    ent = entity;


		    bool isNew = ent.State == EntityState.New;
			bool isDeleted = ent.State == EntityState.Deleted;
			//ent.State = EntityState.Unchanged;

			if ( isNew && ent.ID == null )
				throw new Exception("Id is null: entity type " + ent);

			if ( isNew && !_ps.Caches[ent.GetType()].Contains(ent.ID) )
				_ps.Caches[ent.GetType()].Add(ent);


			foreach ( PropertyDescription pd in od.ManyToManyProperties )
				_ps.SourceStorageAccessMediator.AddCommand(new StorageCommand(ent, pd));

			StorageCommand command = new StorageCommand(ent);
			ent.AcceptChanges();

            switch (command.CommandType)
            {
                case StorageCommandType.Insert:
                case StorageCommandType.Update:
                    foreach (PropertyDescription pd in od.OneToOneProperties)
                    {
                        EntityBase propvalue = pd.GetValue(ent) as EntityBase;
                        if (propvalue != null)
                            _ps.SaveInner(propvalue);
                    }
                    _ps.SourceStorageAccessMediator.AddCommand(command);
                    foreach (PropertyDescription pd in od.OneToManyProperties)
                    {
                        IList propvalue = pd.GetValue(ent) as IList;
                        if (propvalue != null && propvalue.Count>0)
                            foreach (EntityBase e in propvalue) 
                                _ps.SaveInner(e);
                    }
                    break;
                case StorageCommandType.Delete:
                    foreach (PropertyDescription pd in od.OneToManyProperties)
                    {
                        IList propvalue = pd.GetValue(ent) as IList;
                        if (propvalue != null && propvalue.Count > 0)
                            foreach (EntityBase e in propvalue)
                                _ps.SaveInner(e);
                    }
                    _ps.SourceStorageAccessMediator.AddCommand(command);
                    foreach (PropertyDescription pd in od.OneToOneProperties)
                    {
                        EntityBase propvalue = pd.GetValue(ent) as EntityBase;
                        if (propvalue != null)
                            _ps.SaveInner(propvalue);
                    }
                    break;
                
                   
            }

			if ( isDeleted )
			{
				if ( _ps.Caches[ent.GetType()].Contains(ent.ID) )
					_ps.Caches[ent.GetType()].Delete(ent);
			}
			ent.AcceptChanges();
		}

	}
}
