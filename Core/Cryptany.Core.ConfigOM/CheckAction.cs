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
using Cryptany.Core.DPO.MetaObjects;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO;

namespace Cryptany.Core.ConfigOM
{
    [Serializable]
    [DbSchema("services")]
    [Table("CheckActions")]
    public class CheckAction : EntityBase
	{
		private string _name = "";

		public CheckAction()
		{
		}

		public string Name
		{
			get
			{
				return GetValue<string>("Name");
			}
			set
			{
				SetValue("Name", value);
			}
		}

		[NonPersistent]
		public CheckActionPredicate Predicate
		{
			get
			{
				if (Name == "Between")
					return CheckActionPredicate.Between;
				if (Name == "Contains")
					return CheckActionPredicate.Contains;
				if ( Name.ToUpper().Trim() == "EQUALS" )
					return CheckActionPredicate.IsEqual;
				if ( Name.ToUpper().Trim() == "IN" )
					return CheckActionPredicate.In;
				if (Name == "Less")
					return CheckActionPredicate.Less;
				if (Name == "More")
					return CheckActionPredicate.More;
				if (Name == "NotContains")
					return CheckActionPredicate.NotContains;
				if ( Name.ToUpper().Trim() == "NOT EQUALS" )
					return CheckActionPredicate.NotIsEqual;
				if ( Name.ToUpper().Trim() == "NOT IN" )
					return CheckActionPredicate.NotIn;
				if (Name == "NotLess")
					return CheckActionPredicate.NotLess;
				if (Name == "NotMore")
					return CheckActionPredicate.NotMore;
				if ( Name.ToUpper().Trim() == "INTERSECTS WITH" )
					return CheckActionPredicate.IntersectsWith;
				if ( Name.ToUpper().Trim() == "NOT INTERSECTS WITH" )
					return CheckActionPredicate.NotIntersectsWith;
                if (Name.ToUpper().Trim() == "IS EMPTY")
                    return CheckActionPredicate.IsEmpty;
                if (Name.ToUpper().Trim() == "IS NOT EMPTY")
                    return CheckActionPredicate.IsNotEmpty;
				return CheckActionPredicate.Unknown;
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public enum CheckActionPredicate
	{
		/// <summary>
		/// Проверяемое значение равно эталонному
		/// </summary>
		IsEqual,
		/// <summary>
		/// Проверяемое значение не равно эталонному
		/// </summary>
		NotIsEqual,
		/// <summary>
		/// Проверяемое значение больше эталонного
		/// </summary>
		More,
		/// <summary>
		/// Проверяемое значение не больше эталонного
		/// </summary>
		NotMore,
		/// <summary>
		/// Проверяемое значение меньше эталонного
		/// </summary>
		Less,
		/// <summary>
		/// Проверяемое значение не меньше эталонного
		/// </summary>
		NotLess,
		/// <summary>
		/// Проверяемое значение содержится в списке эталонных
		/// </summary>
		In,
		/// <summary>
		/// Проверяемое значение не содержится в списке эталонных
		/// </summary>
		NotIn,
		/// <summary>
		/// Проверяемое значение содержит одно из эталонных
		/// </summary>
		Contains,
		/// <summary>
		/// Проверяемое значение не содержит одно из эталонных
		/// </summary>
		NotContains,
		/// <summary>
		/// Проверяемое значение находится в диапазоне от одного эталонного до другого
		/// </summary>
		Between,
		/// <summary>
		/// Проверяемое и эталонное значения - суть множества, которые требуется проверить на пересекаемость
		/// </summary>
		IntersectsWith,
		/// <summary>
		/// Проверяемое и эталонное значения - суть множества, которые требуется проверить на непересекаемость
		/// </summary>
		NotIntersectsWith,
		/// <summary>
		/// Неизвестный предикат
		/// </summary>
		Unknown,
        /// <summary>
        /// Проверка, реализованная во внешнем методе
        /// </summary>
        ExternalCheck,
        /// <summary>
        /// Проверяемая коллекция пуста
        /// </summary>
        IsEmpty,
        /// <summary>
        /// Проверяемая коллекция не пуста
        /// </summary>
        IsNotEmpty
	}

}
