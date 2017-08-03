using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;

namespace XMLBlockSettings
{
//    public class BaseParam
//    {
//        static string ConnectionString = ConfigurationManager.ConnectionStrings["RouterConnection"].ConnectionString;

//        /// <summary>
//        /// обязательный параметр
//        /// </summary>
//        public bool required;

//        /// <summary>
//        /// таблица из которой брать шаблоны вариантов
//        /// </summary>
//        public string paramSource;

//        /// <summary>
//        /// варианты значений
//        /// </summary>
//        public Dictionary<Guid,string> paramTemplates;

//        /// <summary>
//        /// имя параметра
//        /// </summary>
//        public string name;

//        /// <summary>
//        /// списковый ли параметр 
//        /// </summary>
//        public bool listParam;

//        public void GetParamTemplates(string table, string field)
//        {
//            using (SqlConnection con = new SqlConnection(ConnectionString))
//            {
//                con.Open();
//                using (SqlCommand cmd = new SqlCommand(@"select * From " + table, con))
//                {
//                    using (SqlDataReader dr = cmd.ExecuteReader())
//                    {
//                        while (dr.Read())
//                        {
//                            paramTemplates.Add((Guid)dr["id"], (string)dr[field]);
//                        }
//                    }
//                }
//            }
//        }

//        public void GetParamTemplates()
//        {
//            using (SqlConnection con = new SqlConnection(ConnectionString))
//            {
//                con.Open();
//                using (SqlCommand cmd = new SqlCommand(@"select * From " + paramSource, con))
//                {
//                    using (SqlDataReader dr = cmd.ExecuteReader())
//                    {
//                        while (dr.Read())
//                        {
//                            paramTemplates.Add((Guid)dr["id"], (string)dr["Name"]);
//                        }
//                    }
//                }
//            }
//        }

//        public void GetParamTemplatesForTariff()
//        {
//            using (SqlConnection con = new SqlConnection(ConnectionString))
//            {
//                con.Open();
//                using (SqlCommand cmd = new SqlCommand(@"
//                                    SELECT  
//                                        t.Id, 
//                                        CAST(t.Amount AS VARCHAR(15)) + ' ' + c.Code + ' ' + op.DisplayName +' '+SN.sn  AS Name
//                                    FROM         
//                                        kernel.Tariff AS t 
//                                        INNER JOIN kernel.Operators AS op ON t.OperatorId = op.Id 
//                                        INNER JOIN kernel.Currency AS c ON c.Id = t.CurrencyId 
//						                INNER JOIN kernel.servicenumbers sn on t.servicenumberid=sn.id
//                                    WHERE 
//                                        t.IsActive = 1 
//                                        AND t.TarifficationType = 1
//                                                        ", con))
//                {
//                    using (SqlDataReader dr = cmd.ExecuteReader())
//                    {
//                        while (dr.Read())
//                        {
//                            paramTemplates.Add((Guid)dr["id"], (string)dr["Name"]);
//                        }
//                    }
//                }
//            }
//        }
//    }
}