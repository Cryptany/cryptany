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

namespace Cryptany.Core.DPO.MetaObjects.Attributes
{
	public enum RelationType
	{
		OneToOne,
		OneToMany,
		ManyToMany
	}

	public enum DeleteMode
	{
		Cascade,
		Single
	}

	public delegate void GetMtmValuesType(IEntity entity, RelationAttribute attr);

    [Serializable]
    public class RelationAttribute : Attribute
	{
		private RelationType _relationType;
		private Type _relatedType;
		private string _relatedColumn;
		private string _schemaName;
		private string _mamyToManyRelationTable;
		private string _MtmRelationTableParentColumn;
		private string _MtmRelationTableChildColumn;
		private GetMtmValuesType _getMtmValuesHnd;
		private DeleteMode _deleteMode = DeleteMode.Single;
		private string _cascadingDeleteCondition = "";

		public RelationAttribute(RelationType relationType, Type relatedType, string relatedColumn)
		{
			if ( relationType == RelationType.ManyToMany )
				throw new Exception("Many-to-many relation type requires for explicit intermediate table name specification. Use another constructor overload.");
			_relationType = relationType;
			_relatedType = relatedType;
			_relatedColumn = relatedColumn;
		}

		public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable)
		{
			_relationType = RelationType.ManyToMany;
			_relatedType = relatedType;
			_relatedColumn = relatedColumn;
			_mamyToManyRelationTable = mamyToManyRelationTable;
			_MtmRelationTableChildColumn = _relatedType.Name + "ID";
		}

		public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable, string schemaName)
		{
			_relationType = RelationType.ManyToMany;
			_relatedType = relatedType;
			_relatedColumn = relatedColumn;
			_mamyToManyRelationTable = mamyToManyRelationTable;
			_MtmRelationTableChildColumn = _relatedType.Name + "ID";
			_schemaName = schemaName;
		}

		public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable,
			string MtmRelationTableParentColumn, string MtmRelationTableChildColumn)
		{
			_relationType = RelationType.ManyToMany;
			_relatedType = relatedType;
			_relatedColumn = relatedColumn;
			_mamyToManyRelationTable = mamyToManyRelationTable;
			_MtmRelationTableParentColumn = MtmRelationTableParentColumn;
			_MtmRelationTableChildColumn = MtmRelationTableChildColumn;
		}

		public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable,
			string MtmRelationTableParentColumn, string MtmRelationTableChildColumn, string schemaName)
		{
			_relationType = RelationType.ManyToMany;
			_relatedType = relatedType;
			_relatedColumn = relatedColumn;
			_mamyToManyRelationTable = mamyToManyRelationTable;
			_MtmRelationTableParentColumn = MtmRelationTableParentColumn;
			_MtmRelationTableChildColumn = MtmRelationTableChildColumn;
			_schemaName = schemaName;
		}

		public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable,
			string MtmRelationTableParentColumn, string MtmRelationTableChildColumn, DeleteMode deleteMode)
		{
			_relationType = RelationType.ManyToMany;
			_relatedType = relatedType;
			_relatedColumn = relatedColumn;
			_mamyToManyRelationTable = mamyToManyRelationTable;
			_MtmRelationTableParentColumn = MtmRelationTableParentColumn;
			_MtmRelationTableChildColumn = MtmRelationTableChildColumn;
			_deleteMode = deleteMode;
		}

		//public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable,
		//    string MtmRelationTableParentColumn, string MtmRelationTableChildColumn, 
		//    string cascadingDeleteCondition)
		//{
		//    _relationType = RelationType.ManyToMany;
		//    _relatedType = relatedType;
		//    _relatedColumn = relatedColumn;
		//    _mamyToManyRelationTable = mamyToManyRelationTable;
		//    _MtmRelationTableParentColumn = MtmRelationTableParentColumn;
		//    _MtmRelationTableChildColumn = MtmRelationTableChildColumn;
		//    _deleteMode = DeleteMode.Cascade;
		//    _cascadingDeleteCondition = cascadingDeleteCondition;
		//}

		//public RelationAttribute(Type relatedType, string relatedColumn, string mamyToManyRelationTable,
		//    string _MtmRelationTableParentColumn, string _MtmRelationTableChildColumn,
		//    GetMtmValuesType GetMtmValuesHnd)
		//{
		//    _relationType = RelationType.ManyToMany;
		//    _relatedType = relatedType;
		//    _relatedColumn = relatedColumn;
		//    _mamyToManyRelationTable = mamyToManyRelationTable;
		//    _MtmRelationTableParentColumn = MtmRelationTableParentColumn;
		//    _MtmRelationTableChildColumn = MtmRelationTableChildColumn;
		//    _getMtmValuesHnd += GetMtmValuesHnd;
		//}

		public RelationType RelationType
		{
			get
			{
				return _relationType;
			}
		}

		public Type RelatedType
		{
			get
			{
				return _relatedType;
			}
		}

		public string SchemaName
		{
			get
			{
				return _schemaName;
			}
		}

		public string RelatedColumn
		{
			get
			{
				return _relatedColumn;
			}
		}

		public string MamyToManyRelationTable
		{
			get
			{
				return _mamyToManyRelationTable;
			}
		}

		public string MtmRelationTableParentColumn
		{
			get
			{
				return _MtmRelationTableParentColumn;
			}
		}

		public string MtmRelationTableChildColumn
		{
			get
			{
				return _MtmRelationTableChildColumn;
			}
		}

		public DeleteMode DeleteMode
		{
			get
			{
				return _deleteMode;
			}
		}

		public string CascadingDeleteCondition
		{
			get
			{
				return _cascadingDeleteCondition;
			}
		}

		//public GetMtmValuesType GetMtmValuesHnd
		//{
		//    get
		//    {
		//        return _getMtmValuesHnd;
		//    }
		//}
	}
}
