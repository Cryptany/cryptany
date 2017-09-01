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
using System.Data;
using System.Data.SqlClient;


namespace Cryptany
{
    namespace Core
    {
        /// <summary>
        /// Abonent -- a class repersenting abonent
        /// </summary>
        [System.Xml.Serialization.XmlInclude(typeof(Abonent))]
		[Serializable]
        public class Abonent
        {
            private Guid _dbId;
            /// <summary>
            /// ID в базе. get
            /// </summary>
            public Guid DatabaseId
            {
                get
                {
                    return _dbId;
                }
            }

            private Region _region;
            /// <summary>
            /// Регион к которому приписан абонент. get
            /// </summary>
            public Region Region
            {
                get
                {
                    return _region;
                }
            }

            private Operator _operator;
            /// <summary>
            /// Оператор к которому приписан абонент. get
            /// </summary>
            public Operator Operator
            {
                get
                {
                    return _operator;
                }
            }

            private string _MSISDN;
            /// <summary>
            /// № телефона абонента. get
            /// </summary>
            public string MSISDN
            {
                get
                {
                    return _MSISDN;
                }
            }

            /// <summary>
            /// Конструктор по умолчанию. Необходим для корректной десериализации.
            /// </summary>
            public Abonent()
            {
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="msisdn">№ телефона</param>
            /// <param name="dbId">id в базе данных</param>
            /// <param name="region">регион  к которому приписан абонент</param>
            /// <param name="op">оператор к которому приписан абонент</param>
            public Abonent(string msisdn, Guid dbId, ref Region region, ref Operator op)
            {
                _MSISDN   = msisdn;
                _dbId     = dbId;
                _region   = region;
                _operator = op;
            }

            /// <summary>
            /// Инициализировать абонента по его номеру
            /// Если абонент для данного номера не существует (новый), то создать нового абонента в БД
            /// </summary>
            /// <param name="MSISDN">Номер абонента</param>
            /// <returns></returns>
            public static Abonent GetAbonentByMSISDN(string MSISDN)
            {
                Abonent abonent = null;
                try
                {
                    //DataTable dt = new DataTable("AbonentInfo");
                    using(SqlConnection conn = CoreClassFactory.Connection)
                    {
                        using (SqlCommand cmd = new SqlCommand("Kernel.GetAbonentByMSISDN", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@MSISDN", MSISDN);
                            //SqlDataAdapter sda = new SqlDataAdapter();
                            cmd.CommandTimeout = 0;
                            //sda.SelectCommand = cmd;
                            //sda.Fill(dt);
                            SqlDataReader sdr = cmd.ExecuteReader();
                            if (sdr!=null && sdr.Read())
                            {
                                Guid abonentID = Convert.IsDBNull(sdr["ID"])
                                                     ? Guid.Empty
                                                     : new Guid(sdr["ID"].ToString());
                                string msisdn = Convert.ToString(sdr["MSISDN"]);
                                Guid operatorID = Convert.IsDBNull(sdr["OperatorID"])
                                                      ? Guid.Empty
                                                      : new Guid(sdr["OperatorID"].ToString());
                                Operator op = Operator.GetByID(operatorID);
                                Guid regionId = Convert.IsDBNull(sdr["AreaID"])
                                                    ? Guid.Empty
                                                    : new Guid(sdr["AreaID"].ToString());
                                Region region = Region.GetByID(regionId);
                                abonent = new Abonent(msisdn, abonentID, ref region, ref op);
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {

                    ///TODO: что-нить записать
                    
                    //ILogger logger = Logging.LoggerFactory.Logger;
                    //logger.DefaultSource = "Abonent";
                    //if (logger != null)
                    //{
                    //    logger.Write(new LogMessage(ex +
                    //    ". MSISDN = " + MSISDN, LogSeverity.Error));
                    //}
                }
                catch (ApplicationException e)
                {
                    ///TODO: что-нить записать
                    /// 
                    //ILogger logger = Logging.LoggerFactory.Logger;
                    //logger.DefaultSource = "Abonent";
                    //if (logger != null)
                    //{
                    //    logger.Write(new LogMessage(e + 
                    //    ". MSISDN = " + MSISDN, LogSeverity.Error));
                    //}
                }
                return abonent;
            }

            /// <summary>
            /// Возвращает MSISDN в формате 79031236547
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return _MSISDN;
            }

            /// <summary>
            /// Возвращает MSISDN в формате +79031236547
            /// </summary>
            /// <returns></returns>
            public string ToStringPlus()
            {
                return "+" + _MSISDN;
            }

            /// <summary>
            /// Строка из датасета, описывающая канал, на котором залочен абонент. Если абонент нигде не залочен, вернёт null. (get/set)
            /// </summary>
			[System.Xml.Serialization.XmlIgnore]
            public Guid LockedChannel
			{
				get
				{
					ISessionManager sesMngr = CoreClassFactory.CreateSessionManager();
					string idStr = sesMngr[MSISDN]["CurrentChannel"];
					if ( string.IsNullOrEmpty(idStr) )
					{
						// ИМХО это как=то неправильно. Если не залочен никуда -- значит не залочен и нефиг всяких 
						// дефолтов плодить !!!!
						idStr = "0249672F-B111-4653-917A-A2EF791B3194"; //дефолтный ченэл..надо вынести в настройки
						sesMngr[MSISDN]["CurrentChannel"] = idStr;
					}
					try
					{
						Guid chId = new Guid(idStr);
						return chId;
					}
					catch (Exception ex)
					{
						return Guid.Empty; // And that's not certain. Returning an empty guid permits no obvious way 
						//to indicate that an error has occured.
						
					}
				}
				set
				{
					ISessionManager sesMngr = CoreClassFactory.CreateSessionManager();
					sesMngr[MSISDN]["CurrentChannel"] = value.ToString();

				}
			}

			//[System.Xml.Serialization.XmlIgnore]
			public ISession GetAbonentSession()
			{
				
					ISessionManager sesMngr = CoreClassFactory.CreateSessionManager();
					return sesMngr[MSISDN];
				//}
			}
            /// <summary>
            /// Вспомогательная структура для сериализации объекта
            /// </summary>
            public struct AbonentSerializationInfo
            {
                public Guid     _ID;
                public Region   _Region;
                public Operator _Operator;
                public string   _MSISDN;
            }

            /// <summary>
            /// Вспомогательное поле, предназначенное для сериализации/десериализации объекта (get/set)
            /// </summary>
            public AbonentSerializationInfo SerializationInfo
            {
                get
                {
                    AbonentSerializationInfo result;
                    result._ID       = _dbId;
                    result._Region   = _region;
                    result._Operator = _operator;
                    result._MSISDN   = _MSISDN;
                    return result;
                }
                set
                {
                    _dbId     = (Guid) value._ID;
                    _region   = value._Region;
                    _operator = value._Operator;
                    _MSISDN   = value._MSISDN;
                }
            }
        }
    }
}
