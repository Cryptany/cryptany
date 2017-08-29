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
using System.Data;
using System.Data.SqlClient;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.DPO
{
    public class SqlDbLoader : ILoader
    {
        private Type _entityType;
        private Mapper _mapper;
        private bool _compiled = false;
        private SqlConnection _connection;
        private PersistentStorage _ps;

        public SqlDbLoader(PersistentStorage ps, Type entityType, SqlConnection connection)
		{
			_entityType = entityType;
            _connection = connection;
            _mapper = new Mapper(_entityType);
			_ps = ps;
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

        public List<IEntity> LoadAll()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEntity LoadEntityById(object Id)
        {
            ObjectDescription od = ClassFactory.GetObjectDescription(ReflectedType);
            string sqlQuery = SqlQueryBuilder.BuildSelectSingleRow(od, Id);
            SqlCommand command = new SqlCommand(sqlQuery, _connection);
            SqlDataReader reader = command.ExecuteReader();
            if ( !reader.HasRows )
                return null;
            IEntity e = (IEntity)od.CreateObject();
            foreach(PropertyDescription prop in od.Properties)
            {
                if ( prop.IsRelation )
                    continue;
                prop.SetValue(e, reader[Mapper[prop.Name]]);
            }
            foreach(PropertyDescription prop in od.Relations)
            {
                if ( prop.IsOneToOneRelation )
                {
                    object oid = reader[Mapper[prop.Name]];
                    Type t = prop.RelatedType;
                    IEntity ent = _ps.GetEntityById(t, oid);
                    prop.SetValue(e, ent);
                }
            }
            return e;
        }

        public void ReloadEntity(IEntity entity)
        {
            throw new Exception("The method or operation is not implemented.");
        }

    }

    public static class SqlQueryBuilder
    {
        public static string  BuildSelectSingleRow(ObjectDescription od, object id)
        {
            Mapper m = new Mapper(od.ObjectType);
            string sqlQuery = "SELECT ";
            foreach (PropertyDescription prop in od.Properties)
            {
                sqlQuery += m[prop.Name] + ",";
            }
            if ( sqlQuery[sqlQuery.Length - 1] == ',' )
                sqlQuery = sqlQuery.Remove(sqlQuery.Length - 1);
            else
                throw new Exception("There is no data to load from Database");
            sqlQuery += " FROM ";
            sqlQuery += m.TableName + " ";
            sqlQuery += "WHERE ";
            sqlQuery += m[od.IdField.Name] + " = ";
            if ( id is int )
                sqlQuery += id.ToString();
            else
                sqlQuery += "'" + id.ToString() + "'";
            return sqlQuery;
        }
    }
}
