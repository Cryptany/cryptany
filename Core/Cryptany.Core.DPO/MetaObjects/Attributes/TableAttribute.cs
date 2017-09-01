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

namespace Cryptany.Core.DPO.MetaObjects.Attributes
{
    [Serializable]
    public class TableAttribute : Attribute
	{
		string _tableName;
		//private List<Condition> _conditions = new List<Condition>();
		private string _fieldName = "";
		private ConditionOperation _operation;
		private string _value = "";


		public TableAttribute(string tableName)
		{
			_tableName = tableName;
		}

		public TableAttribute(string tableName, string fieldName, ConditionOperation operation, string value)
		{
			_tableName = tableName;
			_fieldName = fieldName;
			_operation = operation;
			_value = value;

			//foreach ( Condition arg in args )
			//    _conditions.Add(arg);
		}

		public string TableName
		{
			get
			{
				return _tableName;
			}
		}

		//public List<Condition> Conditions
		//{
		//    get
		//    {
		//        return _conditions;
		//    }
		//}

		public bool Conditional
		{
			get
			{
				return _fieldName != "";
			}
		}

		public bool CheckConditions(DataRow row)
		{
			//bool res = false;
			//if (!Conditional)
			//    return true;
			//for ( int i = 0; res = _conditions[i].Check(row) && i < _conditions.Count; i++ )
			//    if ( !res )
			//        return false;
			//return true;
			string rowvalue = row[_fieldName].ToString();
			switch ( _operation )
			{
				case ConditionOperation.Equals:
                    if ( row.Table.Columns[_fieldName].DataType == typeof(Guid) )
                        return rowvalue.ToUpper() == _value.ToUpper();
                    else
                        return rowvalue == _value;
				case ConditionOperation.NotEquals:
                    if ( row.Table.Columns[_fieldName].DataType == typeof(Guid) )
                        return rowvalue.ToUpper() != _value.ToUpper();
                    else
                        return rowvalue != _value;
				//case ">=":
				//    return rowvalue >= _value;
				//case "<=":
				//    return rowvalue <= _value;
				//case ">":
				//    return rowvalue > _value;
				//case "<":
				//    return rowvalue < _value;
				default:
					break;
			}
			return false;
		}

		public override string ToString()
		{
			string s = "";
			if (_fieldName != null && _fieldName != "")
			{
				s = _fieldName;
				if (_operation == ConditionOperation.Equals)
					s += " = ";
				else if (_operation == ConditionOperation.NotEquals)
					s += " <> ";
				double d;
				if (!double.TryParse(_value, out d))
					s += "'" + _value + "'";
				else
					s += _value;
			}

			return s;
		}
	}

	public enum ConditionOperation
	{
		Equals,
		NotEquals
	}

	//public struct Condition
	//{
	//    private string _fieldName;
	//    private ConditionOperation _operation;
	//    private string _value;

	//    public Condition(string fieldName, ConditionOperation operation, string value)
	//    {
	//        _fieldName = fieldName;
	//        _operation = operation;
	//        _value = value;
	//    }

	//    public string FieldName
	//    {
	//        get
	//        {
	//            return _fieldName;
	//        }
	//    }

	//    public ConditionOperation Operation
	//    {
	//        get
	//        {
	//            return _operation;
	//        }
	//    }

	//    public string Value
	//    {
	//        get
	//        {
	//            return _value;
	//        }
	//    }

	//    public bool Check(DataRow row)
	//    {
	//        string rowvalue = row[_fieldName].ToString();
	//        switch ( _operation )
	//        {
	//            case ConditionOperation.Equals:
	//                return rowvalue == _value;
	//            case ConditionOperation.NotEquals:
	//                return rowvalue != _value;
	//            //case ">=":
	//            //    return rowvalue >= _value;
	//            //case "<=":
	//            //    return rowvalue <= _value;
	//            //case ">":
	//            //    return rowvalue > _value;
	//            //case "<":
	//            //    return rowvalue < _value;
	//            default:
	//                break;
	//        }
	//        return false;
	//    }
	//}
}
