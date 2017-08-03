using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace DBService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "DBService" in code, svc and config file together.
    public class DBService : IDBService
    {
        static string ConnectionString = ConfigurationManager.ConnectionStrings["RouterConnection"].ConnectionString;

        //Block IDBService.GetBlock(Guid id)
        //{
        //    return new Block();
        //}


        public ListOfService GetServices()
        {
            List<Service> Services = new List<Service>();
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                using (SqlCommand sqlCom = new SqlCommand(
                    @"  select 
                          serv.Id, 
                          serv.Name, 
                          serv.ServiceNumber, 
                          serv.ServiceTypeId, 
                          servt.Name as ServiceTypeName 
                          from 
                          router.Service serv
                          inner join router.ServiceType servt on servt.id = serv.ServiceTypeId"
                    , sqlCon))
                {
                    sqlCon.Open();
                    using (SqlDataReader dr = sqlCom.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Service buffer = new Service
                            {
                                id = (Guid)dr["id"],
                                name = (string)dr["name"],
                                serviceNumber = dr.IsDBNull(dr.GetOrdinal("serviceNumber")) ? String.Empty : (string)dr["serviceNumber"],
                                typeid = (Guid)dr["ServiceTypeId"],
                                typename = (string)dr["ServiceTypeName"]
                            };
                            buffer.blocksId = GetServiceBlocks(buffer.id);
                            buffer.expressions = GetServiceExpressions(buffer.id);
                            Services.Add(buffer);
                        }
                    }
                }
            }

            return new ListOfService { LOS = Services };
        }

        List<Guid> GetServiceBlocks(Guid id)
        {
            List<Guid> result = new List<Guid>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @" select id
                        from router.ServiceBlock
                        where ServiceId =  @ServId", con))
                {

                    cmd.Parameters.AddWithValue("@ServId", id);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add((Guid)dr["id"]);
                        }
                    }
                }
            }
            return result;
        }

        List<string> GetServiceExpressions(Guid id)
        {
            List<string> result = new List<string>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"select expression
                      from router.RegularExpression
                      where ServiceId = @ServId", con))
                {

                    cmd.Parameters.AddWithValue("@ServId", id);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add((string)dr["expression"]);
                        }
                    }
                }
            }
            return result;
        }
    }
}
