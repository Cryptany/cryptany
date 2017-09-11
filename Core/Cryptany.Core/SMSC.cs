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
using DataSetLib;
using System.Collections.Generic;

namespace Cryptany.Core
{

    /// <summary>
    /// Summary description for SMSC.
    /// </summary>
    [System.Xml.Serialization.XmlInclude(typeof(SMSC))]
    [Serializable]
    public class SMSC
    {
        private Operator[] _operators;

        public Operator[] Operators
        {
            get
            {
                if (_operators == null)
                {
                    CacheDataSet.SMSCRow row = CoreClassFactory.CreateConfigProvider().CacheDS.SMSC.FindById(_ID);
                    List<Operator> ops = new List<Operator>();
                    foreach (CacheDataSet.SMSC2OpRow smsc2op in row.GetSMSC2OpRows())
                    {
                        ops.Add(new Operator(smsc2op.OperatorId));
                    }
                    _operators = ops.ToArray();
                }
                return _operators;
            }
        }

        private bool _hasDelimiter;

        public bool HasDelimiter
        {
			get
			{
				return _hasDelimiter;
			}
			set
			{
				_hasDelimiter = value;
			}
        }

        private string _delimiterChar;

        public string DelimiterChar
        {
			get
			{
				return _delimiterChar;
			}
			set
			{
				_delimiterChar = value;
			}
        }

        private string _Name;

        public string Name
        {
			get
			{
				return _Name;
			}
			set
			{
				_Name = value;
			}      // Название центра обработки собщений
        }

        private Guid _ID;

        public Guid DatabaseId
        {
            get
            {
                return _ID;
            }
			set
			{
				_ID = value;
			}
        }

		private long _code;

		public long Code
		{
			get
			{
				return _code;       // Returns i.e. 'old' SMSC index (33 for MTS, 2 for Beeline etc.). Is needed for conversion purposes
			}
			set
			{
				_code = value;
			}
		}

		public SMSC()
		{
		}

        /// <summary>
        /// Конструктор по-умолчанию.Необходим для корректной сериализации/десериализации
        /// </summary>
        public SMSC(string _Name)
        {
            _hasDelimiter = false;
            _delimiterChar = "";
            this._Name = _Name;
        }

        /// <summary>
        /// Конструктор создающий объект по известному ID из базы.
        /// </summary>
        /// <param name="id"></param>
        public SMSC(Guid id)
        {
            _ID = id;

            CacheDataSet.SMSCRow row = CoreClassFactory.CreateConfigProvider().CacheDS.SMSC.FindById(id);
            if (row != null)
            {
                _Name = row.Name;
                _code     = row.Code;
                _hasDelimiter = row.IsDelimited;
                _delimiterChar = row.IsDelimited ? row.Delimiter : "";

            }
            else
                throw new ApplicationException("Не найден SMSC с id=" + id);
            //_oldIndex = 242;
        }

        /// <summary>
        /// Строка из базы описывающая данный объект.
        /// </summary>
        public CacheDataSet.SMSCRow Row
        {
            get
            {
                return CoreClassFactory.CreateConfigProvider().CacheDS.SMSC.FindById(_ID);
            }
        }

        /// <summary>
        /// Вспомогательная структура для сериализации/десериализации объекта
        /// </summary>
        public struct SMSCSerializationInfo
        {
            public Guid _ID;
            public Operator[] Operators;
        }

        /// <summary>
        /// Проперти для сериализации/десериализации объекта
        /// </summary>
        public SMSCSerializationInfo SerializationInfo
        {
            get
            {
                SMSCSerializationInfo result;
                result._ID = _ID;
                result.Operators = _operators;
                return result;
            }
            set
            {
                _ID = value._ID;
                _operators = value.Operators;
            }
        }
    }
}
