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
using System.Data.SqlClient;
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.MetaObjects.Attributes;

namespace Cryptany.Core.DPO.Sql
{
	/// <summary>
	/// Description of SqlScriptBuilder.
	/// </summary>
	public class SqlScriptBuilder
	{
		private Mapper _mapper;
		private PersistentStorage _ps;
		private string _insertTemplate = "INSERT INTO @@TableName ( @@FieldList ) @@Values";
		private string _updateTemplate = "UPDATE @@TableName SET @@NameValuePairs WHERE ID = @@ID";
		private string _updateInBatchTemplate = "UPDATE @@TableName SET @@NameValuePairs FROM (@@ValuesTable) AS temp WHERE @@TableName.ID = temp.ID";
		private string _deleteTemplate = "DELETE FROM @@TableName WHERE ID IN ( @@ID )";

		public SqlScriptBuilder(Mapper mapper, PersistentStorage ps)
		{
			_ps = ps;
			_mapper = mapper;
			_insertTemplate = _insertTemplate.Replace("@@TableName", mapper.FullTableName);
			_updateTemplate = _updateTemplate.Replace("@@TableName", mapper.FullTableName);
			_updateInBatchTemplate = _updateInBatchTemplate.Replace("@@TableName", mapper.FullTableName);
			_deleteTemplate = _deleteTemplate.Replace("@@TableName", mapper.FullTableName);
		}

		private static string ValueToString(object value)
		{
			if ( value == null )
				return "NULL";
			if (  value is string )
				return "'" + (value as string).Replace("'", "''") + "'";
			if ( value is bool )
				return ((bool)value) ? "1" : "0";
			if ( value is Guid )
				return "'" + value.ToString() + "'";
			if ( value is DateTime )
				return "'" + ((DateTime)value).ToString("u").Replace("Z", "") + "'";
			if ( value is char )
				return ((char)value).ToString();
			if ( value as EntityBase != null )
				return ValueToString((value as EntityBase).ID);
			if ( value.GetType().IsEnum )
				return Convert.ToInt32(value).ToString();
			return value.ToString();
		}
		
		public PersistentStorage Ps
		{
			get
			{
				return _ps;
			}
		}

		public Mapper SqlMapper
		{
			get
			{
				return _mapper;
			}
		}
		
		public string CreateUpdateStatement(EntityBase e)
		{
			ObjectDescription od = ClassFactory.GetObjectDescription(_mapper.EntityType, _ps);
			string nameValuePairs = "";
			foreach (PropertyDescription pd in od.Properties)
			{
				if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( nameValuePairs != "" )
					nameValuePairs += ", ";
				if ( pd.IsOneToOneRelation )
					nameValuePairs += SqlMapper[pd.Name] + " = " + (e[pd.Name] as EntityBase)[pd.RelationAttribute.RelatedColumn];
				else
					nameValuePairs += SqlMapper[pd.Name] + " = " + ValueToString(e[pd.Name]);
			}
            nameValuePairs += ", " + ValueToString(e.ID) + " AS " + _mapper[od.IdField.Name];
			return _updateTemplate.Replace("@@NameValuePairs", nameValuePairs).Replace("@@ID", ValueToString(e.ID));
		}
		
		private string GetFieldList()
		{
			string list = "";
			ObjectDescription od = ClassFactory.GetObjectDescription(_mapper.EntityType, _ps);
			foreach (PropertyDescription pd in od.Properties)
			{
				if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( list != "" )
					list += ", ";
                list += SqlMapper[pd.Name, true];
			}
			return list;
		}
		
		private string GetValueList(EntityBase e)
		{
			string list = "";
			ObjectDescription od = ClassFactory.GetObjectDescription(_mapper.EntityType, _ps);
			foreach (PropertyDescription pd in od.Properties)
			{
				if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( list != "" )
					list += ", ";
				if ( pd.IsOneToOneRelation )
					list += (e[pd.Name] as EntityBase)[pd.RelationAttribute.RelatedColumn];
				else
					list += ValueToString(e[pd.Name]);
			}
			return list;
		}

		public string CreateUpdateStatement(IList<EntityBase> list)
		{
			string fieldList = "";
			string nameValuePairs = "";
			ObjectDescription od = ClassFactory.GetObjectDescription(_mapper.EntityType, _ps);
			foreach (PropertyDescription pd in od.Properties)
			{
				if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				if ( fieldList != "" )
					fieldList += ", ";
				fieldList += SqlMapper[pd.Name];
				
				if ( nameValuePairs != "" )
					nameValuePairs += ", ";
                nameValuePairs += _mapper[pd.Name, true] + " = temp." + _mapper[pd.Name, true];
			}
			string valuesTable = "";
			foreach ( EntityBase e in list)
			{
				string valuesRow = "";
				foreach (PropertyDescription pd in od.Properties)
				{
					if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
						continue;
					if ( valuesRow != "" )
						valuesRow += ", ";
                    valuesRow += ValueToString(e[pd.Name]) + " AS " + _mapper[pd.Name];
				}
				valuesRow += ", " + ValueToString(e.ID) + " AS " + _mapper[od.IdField.Name];
				valuesRow = "SELECT " + valuesRow;
				if ( valuesTable != "" )
					valuesTable += "\r\nUNION ALL\r\n";
				valuesTable += valuesRow;
			}
			return _updateInBatchTemplate.Replace("@@FieldList", fieldList).Replace("@@NameValuePairs", nameValuePairs).Replace("@@ValuesTable", valuesTable);
		}

		public string CreateDeleteStatement(EntityBase e)
		{
			return CreateDeleteStatement(new List<EntityBase>( new EntityBase[] { e } ));
		}

		public string CreateDeleteStatement(IList<EntityBase> list)
		{
			string ids = "";
			foreach ( EntityBase e in list )
			{
				if ( ids != "" )
					ids += ", ";
				ids += ValueToString(e.ID);
			}
			return _deleteTemplate.Replace("@@ID", ids);
		}

		public string CreateInsertStatement(EntityBase e)
		{
			return CreateInsertStatement(new List<EntityBase>( new EntityBase[] { e } ));
		}

		public string CreateInsertStatement(IList<EntityBase> list)
		{
			string fieldList = "";
			ObjectDescription od = ClassFactory.GetObjectDescription(_mapper.EntityType, _ps);
			fieldList += _mapper[od.IdField.Name, true];
			foreach (PropertyDescription pd in od.Properties)
			{
				if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation )
					continue;
				//if ( fieldList != "" )
				fieldList += ", ";
				fieldList += SqlMapper[pd.Name];
			}
			string valuesTable = "";
			foreach ( EntityBase e in list)
			{
                
				string valuesRow = "";
				valuesRow += ValueToString(e.ID);
				foreach (PropertyDescription pd in od.Properties)
				{
					if ( pd == od.IdField || pd.IsNonPersistent || pd.IsOneToManyRelation || pd.IsManyToManyRelation  )
						continue;
                    
					//if ( valuesRow != "" )
					valuesRow += ", ";
					valuesRow += ValueToString(e[pd.Name]);
				}
				valuesRow = "SELECT " + valuesRow;
				if ( valuesTable != "" )
					valuesTable += "\r\nUNION ALL\r\n";
				valuesTable += valuesRow;
			}
			return _insertTemplate.Replace("@@FieldList", fieldList).Replace("@@Values", valuesTable);
		}

		public static string CreateMtmInsert(PropertyDescription pd, PersistentStorage ps, IList<EntityBase> entities)
		{
			RelationAttribute attr = pd.GetAttribute<RelationAttribute>();
			if ( attr == null || attr.RelationType != RelationType.ManyToMany )
				throw new Exception("A many-to-many relationship expected");
			string script = "INSERT INTO " + (string.IsNullOrEmpty(attr.SchemaName) ? "" : attr.SchemaName + ".") +
				attr.MamyToManyRelationTable + "(" + attr.MtmRelationTableChildColumn + "," + attr.MtmRelationTableParentColumn + ")\r\n";
			string data = "";
			foreach ( EntityBase entity in entities )
				foreach ( EntityBase e in pd.GetValue<System.Collections.IList>(entity) )
					if ( string.IsNullOrEmpty(data) )
						data = "SELECT " + ValueToString(e.ID) + ", " + ValueToString(entity.ID) + "\r\n";
					else
						data += "UNION ALL\r\nSELECT " + ValueToString(e.ID) + ", " + ValueToString(entity.ID) + "\r\n";
			if ( string.IsNullOrEmpty(data) )
				return "";
			else
				script += data;
			return script;
		}

		public static string CreateMtmDelete(PropertyDescription pd,PersistentStorage ps, IList<EntityBase> entities)
		{
			RelationAttribute attr = pd.GetAttribute<RelationAttribute>();
			if ( attr == null || attr.RelationType != RelationType.ManyToMany )
				throw new Exception("A many-to-many relationship expected");
			string script = "DELETE FROM " + (string.IsNullOrEmpty(attr.SchemaName) ? "" : attr.SchemaName + ".") + 
				attr.MamyToManyRelationTable + "\r\nWHERE ";
			script += attr.MtmRelationTableParentColumn+" IN (";
			string data = "";
			foreach ( EntityBase entity in entities )
				if ( string.IsNullOrEmpty(data) )
					data = ValueToString(entity.ID);
				else
					data += ", " + ValueToString(entity.ID);
			script += data + ")\r\n";
			return script;
		}
	}
}
