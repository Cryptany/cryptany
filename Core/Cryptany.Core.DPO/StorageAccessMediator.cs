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
using System.Data.SqlClient;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.Sql;

namespace Cryptany.Core.DPO
{
    public enum StorageAccessMediatorType
    {
        DataSet,
        MsSql
    }

    public abstract class StorageAccessMediator
    {
        protected StorageAccessMediatorType _mediatorType;
        protected bool _locked = false;

        public bool Locked
        {
            get
            {
                return _locked;
            }
        }

        public abstract void Reset();
        public abstract void BeginCollectData();
        public abstract void AddCommand(StorageCommand command);
        public abstract void EndCollectData();
        public abstract StorageCommandResult EndCollectAndFlushData();
        public abstract void CancelCollectData();
        public abstract StorageCommandResult FlushData();
    }

    public class DataSetStorageAccessMediator : StorageAccessMediator
    {
        private DataSet _dataset = null;
        protected StorageCommandResult _commandResult;

        public DataSetStorageAccessMediator(DataSet dataSet)
        {
            _dataset = dataSet;
            _mediatorType = StorageAccessMediatorType.DataSet;
        }

        public StorageCommandResult LastCommandResult
        {
            get
            {
                return _commandResult;
            }
        }

        public override void BeginCollectData()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void AddCommand(StorageCommand command)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void EndCollectData()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override StorageCommandResult EndCollectAndFlushData()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override void CancelCollectData()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override StorageCommandResult FlushData()
        {
            throw new Exception("The method or operation is not implemented.");
        }


        public override void Reset()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }

    public class MsSqlStorageAccessMediator : StorageAccessMediator
    {
        private SqlConnection _connection = null;
        private List<StorageCommand> _list = new List<StorageCommand>();
        private Dictionary<PropertyDescription, List<EntityBase>> _mtmRelationships = new Dictionary<PropertyDescription, List<EntityBase>>();
        private string _script;
        private PersistentStorage _ps;
        private Dictionary<Type, SqlScriptBuilder> _builders = new Dictionary<Type, SqlScriptBuilder>();

        public MsSqlStorageAccessMediator(PersistentStorage ps, SqlConnection connection)
        {
            _connection = connection;
            _mediatorType = StorageAccessMediatorType.MsSql;
            _ps = ps;
        }

        public PersistentStorage Ps
        {
            get
            {
                return _ps;
            }
        }

        public override void BeginCollectData()
        {
            if (_locked)
                throw new Exception("Cannot begin collect data while in collecting state");
            _list.Clear();
            _mtmRelationships.Clear();
            _script = "";
            _locked = false;
        }

        public override void AddCommand(StorageCommand command)
        {
            if (_locked)
                throw new Exception("The mediator is locked");
            if (command.Mode == StorageCommandMode.Entity)
            {
                if (!_builders.ContainsKey(command.EntityType))
                    _builders.Add(command.EntityType, new SqlScriptBuilder(new Mapper(command.EntityType, _ps), _ps));
                _list.Add(command);
            }
            else
            {
                if (!_mtmRelationships.ContainsKey(command.ManyToManyProperty))
                {
                    List<EntityBase> l = new List<EntityBase>();
                    l.AddRange(command.Entities);
                    _mtmRelationships.Add(command.ManyToManyProperty, l);
                }
                else
                    foreach (EntityBase e in command.Entities)
                        if (!_mtmRelationships[command.ManyToManyProperty].Contains(e))
                            _mtmRelationships[command.ManyToManyProperty].Add(e);
            }
        }

        public override void EndCollectData()
        {
            _locked = true;
            foreach (StorageCommand command in _list)
            {
                string script;
                switch (command.CommandType)
                {
                    case StorageCommandType.Insert:
                        script = _builders[command.EntityType].CreateInsertStatement(command.Entities);
                        break;
                    case StorageCommandType.Update:
                        script = _builders[command.EntityType].CreateUpdateStatement(command.Entities);
                        break;
                    default:
                        script = _builders[command.EntityType].CreateDeleteStatement(command.Entities);
                        break;
                }
                _script += script + "\r\n\r\n";
            }
            string clearMtm = "";
            string addMtm = "";
            foreach (PropertyDescription pd in _mtmRelationships.Keys)
            {
                clearMtm += SqlScriptBuilder.CreateMtmDelete(pd, _ps, _mtmRelationships[pd]) + "\r\n";
                addMtm += SqlScriptBuilder.CreateMtmInsert(pd, _ps, _mtmRelationships[pd]) + "\r\n";
            }
            _script = clearMtm + "\r\n\r\n" + _script + "\r\n\r\n" + addMtm;
            string guid = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            _script = "DECLARE @status int\r\nDECLARE @msg VARCHAR(255)\r\n" +
                "BEGIN TRAN savetran" + guid + "\r\n\r\n" +
                "BEGIN TRY\r\n\r\n" +
                _script +
                "SELECT @status = 1\r\nCOMMIT TRAN savetran" + guid + "\r\n" +
                "END TRY\r\n" +
                "BEGIN CATCH\r\nROLLBACK TRAN savetran" + guid + "\r\n" +
                "SELECT @status = 0\r\n" +
                "SET @Msg =ERROR_MESSAGE()\r\nRAISERROR(@Msg,16,1)\r\nEND CATCH\r\nSELECT 'SUCCESS'";
        }

        public override StorageCommandResult EndCollectAndFlushData()
        {
            EndCollectData();
            return FlushData();
        }

        public override void CancelCollectData()
        {
            _locked = false;
            _script = "";
            _mtmRelationships.Clear();
            _list.Clear();
        }

        public override StorageCommandResult FlushData()
        {
            using (SqlConnection c = new SqlConnection(_connection.ConnectionString))
            {
                c.Open();
                using (SqlCommand command = new SqlCommand(_script, c))
                {
                    command.CommandTimeout = 300;
                    object result = command.ExecuteScalar();
                    return new StorageCommandResult(StorageCommandResultState.Success, "Command succeded.", result);
                }
            }
            return new StorageCommandResult(StorageCommandResultState.Failed, "Command failed.", 0);
        }

        public override void Reset()
        {

            _locked = false;
            BeginCollectData();
        }
    }

    public enum StorageCommandType
    {
        Insert,
        Update,
        Delete,
        Select
    }

    public enum StorageCommandResultState
    {
        Success,
        Failed
    }

    public class StorageCommandResult
    {
        private StorageCommandResultState _state;
        private string _message;
        private object _result;

        internal StorageCommandResult(StorageCommandResultState state, string message, object result)
        {
            _state = state;
            _message = message;
            _result = result;
        }

        public object Result
        {
            get
            {
                return _result;
            }
        }

        public string Message
        {
            get
            {
                return _message;
            }
        }

        public StorageCommandResultState State
        {
            get
            {
                return _state;
            }
        }
    }

    public enum StorageCommandMode
    {
        Entity,
        MtmRelation
    }

    public class StorageCommand
    {
        //public static StorageCommand Create(object argument)

        protected StorageCommandType _commandType;
        protected List<EntityBase> _entities = new List<EntityBase>();
        protected PropertyDescription _propertyDescription;
        protected StorageCommandMode _mode;
        private readonly Type _entityType;
        protected SqlScriptBuilder _builder;

        internal StorageCommand(EntityBase argument)
        {
            if (argument == null)
                throw new Exception("The entity should be null");
            _entities.Add(argument);
            _entityType = argument.GetType();
            _mode = StorageCommandMode.Entity;
            SetCommandType();
        }

        internal StorageCommand(EntityBase entity, PropertyDescription pd)
        {
            if (entity == null)
                throw new Exception("The entity should be null");
            _propertyDescription = pd;
            if (!_propertyDescription.IsManyToManyRelation)
                throw new Exception("A many-to-many relation expected");
            _entities.Add(entity);
            _entityType = entity.GetType();
            _mode = StorageCommandMode.MtmRelation;
            SetCommandType();
        }

        internal StorageCommand(List<EntityBase> argument)
        {
            if (argument == null || argument.Count == 0)
                throw new Exception("The entity list should not be empty");
            _entityType = argument[0].GetType();
            foreach (EntityBase e in argument)
                if (e.GetType() != _entityType)
                    throw new Exception("All element of the entity list must of the same type");
            _entities = argument;
            _mode = StorageCommandMode.Entity;
            SetCommandType();
        }

        internal StorageCommand(List<EntityBase> entities, PropertyDescription pd)
        {
            if (entities == null || entities.Count == 0)
                throw new Exception("The entity list should not be empty");
            if (!_propertyDescription.IsManyToManyRelation)
                throw new Exception("A many-to-many relation expected");
            _entityType = entities[0].GetType();
            foreach (EntityBase e in entities)
                if (e.GetType() != _entityType)
                    throw new Exception("All element of the entity list must of the same type");
            _propertyDescription = pd;
            _entities = entities;
            SetCommandType();
            _mode = StorageCommandMode.MtmRelation;
        }

        public Type EntityType
        {
            get
            {
                return _entityType;
            }
        }

        private void SetCommandType()
        {
            switch (_entities[0].State)
            {
                case EntityState.New:
                    _commandType = StorageCommandType.Insert;
                    break;
                case EntityState.Deleted:
                    _commandType = StorageCommandType.Delete;
                    break;
                case EntityState.Changed:
                    _commandType = StorageCommandType.Update;
                    break;
                default:
                    throw new Exception("Invalid entity state");
            }
        }

        public List<EntityBase> Entities
        {
            get
            {
                return _entities;
            }
        }

        public StorageCommandType CommandType
        {
            get
            {
                return _commandType;
            }
        }

        public PropertyDescription ManyToManyProperty
        {
            get
            {
                return _propertyDescription;
            }
        }

        public StorageCommandMode Mode
        {
            get
            {
                return _mode;
            }
        }

        public string GenerateScript()
        {
            if (Mode == StorageCommandMode.Entity)
                return null;
            return null;
        }
    }

}
