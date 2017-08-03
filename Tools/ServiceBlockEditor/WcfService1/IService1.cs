using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Serialization;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace WcfService1
{
    [ServiceContract]
    public interface IRouteDBService
    {
        #region GetData
        [OperationContract]
        ListOfService GetServices();

        [OperationContract]
        ServiceBlock GetServiceBlock(Guid ID, string Name);

        [OperationContract]
        List<Block> GetAllBlocks();

        [OperationContract]
        Dictionary<Guid,string> GetAllBlockTypes();

        [OperationContract]
        List<BlockSettingsParam> GetBlockSettingsParams();

        [OperationContract]
        List<BaseParam> GetBlockInfo(string BlockType);

        #endregion

        #region DeleteData
        [OperationContract]
        void RemoveBlockFromServiceBlock(Guid BlockEntryId);

        //[OperationContract]
        //void RemoveServiceBlockFromService(Guid ServiceBlockId); 
        #endregion

        #region InsertData
        [OperationContract]
        void SaveServiceBlock(ServiceBlock serviceBlock);

        [OperationContract]
        string CreateNewBlock(Block Block);
        #endregion

      
    }
    [ServiceContract(SessionMode = SessionMode.NotAllowed)] 
    public interface IPolicyRetriever
    {
        #region accessPolicy
        [OperationContract, WebGet(UriTemplate = "clientaccesspolicy.xml")]
        Stream GetSilverlightPolicy();
        [OperationContract, WebGet(UriTemplate = "crossdomain.xml")]
        Stream GetFlashPolicy();
        #endregion
    }

    #region for getservice()
    [DataContract]
    public class Service
    {
        [DataMember]
        public Guid id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public Guid typeid { get; set; }
        [DataMember]
        public string typename { get; set; }

        [DataMember]
        public string serviceNumber { get; set; }

        //[DataMember]
        //public List<string> expressions { get; set; }

        [DataMember]
        public Dictionary<Guid, string> serviceBlocksId { get; set; }
    }

    [DataContract]
    public class ListOfService
    {
        [DataMember]
        public List<Service> LOS { get; set; }
    } 
    #endregion
    [DataContract]
    public class ServiceBlock
    {
        [DataMember]
        public Guid id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public List<Block> blocks { get; set; } 
    }
    [DataContract]
    public class Block 
    {

        [DataMember]
        public Guid id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public Guid typeid { get; set; }
        [DataMember]
        public string typename { get; set; }

        [DataMember]
        public List<BlockLink> links { get; set; } 

        [DataMember]
        public bool isVerification { get; set; }

        [DataMember]
        public _BlockSettings settings { get; set; }

        [DataMember]
        public string settingsString { get; set; }

        [DataMember]
        public Guid blockEntryId { get; set; }
    }
    [DataContract]
    public class BlockLink
    {
        [DataMember]
        public Guid id { get; set; }
        [DataMember]
        public bool yes { get; set; }
        /// <summary>
        /// true, если исходит из блока, владеющего данной связью
        /// </summary>
        [DataMember]
        public bool output { get; set; }
        [DataMember]
        public Guid linkedBlockId { get; set; }
        [DataMember]
        public Guid linkedEntryBlockId { get; set; }
    }  
    #region XMLSettings
    [DataContract]
    public class _BlockSettings
    {
        [DataMember]
        public List<_Condition> Conditions { get; set; }
    }

    [DataContract]
    public class _Condition
    {
        [DataMember]
        public string Property { get; set; }
        [DataMember]
        public string Operation { get; set; }
        [DataMember]
        public string Value { get; set; }
    }
    #endregion
    [DataContract]
    public class BlockSettingsParam
    {
        [DataMember]
        public Guid id { get; set; }

        [DataMember]
        public string value { get; set; }

        [DataMember]
        public string type { get; set; }
    }
    #region BlockInfo
    [DataContract]
    public class BaseParam
    {
        static string ConnectionString = ConfigurationManager.ConnectionStrings["RouterConnection"].ConnectionString;
        
        public BaseParam()
        {
            paramTemplates = new Dictionary<Guid, string>();
            conditionOperators = new Dictionary<string, string>();
        }
        /// <summary>
        /// Имя типа блока
        /// </summary>
        [DataMember]
        public string BlockType { get; set; }
        
        /// <summary>
        /// обязательный параметр
        /// </summary>
        [DataMember]
        public bool required { get; set; }

        /// <summary>
        /// таблица из которой брать шаблоны вариантов
        /// </summary>
        [DataMember]
        public string paramSource { get; set; }

        /// <summary>
        /// варианты значений
        /// </summary>
        [DataMember]
        public Dictionary<Guid, string> paramTemplates { get; set; }

        /// <summary>
        /// имя параметра
        /// </summary>
        [DataMember]
        public string name { get; set; }

        /// <summary>
        /// списковый ли параметр 
        /// </summary>
        [DataMember]
        public bool listParam { get; set; }

        /// <summary>
        /// Типы сравнения (Equals/In/NotIn etc)
        /// </summary>
        [DataMember]
        public Dictionary<string,string> conditionOperators { get; set; }

        public void GetParamTemplates(string table, string field)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"select * From " + table, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            paramTemplates.Add((Guid)dr["id"], (string)dr[field]);
                        }
                    }
                }
            }
        }

        public void GetParamTemplates()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"select * From " + paramSource, con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            paramTemplates.Add((Guid)dr["id"], (string)dr["Name"]);
                        }
                    }
                }
            }
        }

        public void GetParamTemplatesForTariff()
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"
                                    SELECT  
                                        t.Id, 
                                        CAST(t.Amount AS VARCHAR(15)) + ' ' + c.Code + ' ' + op.DisplayName +' '+SN.sn  AS Name
                                    FROM         
                                        kernel.Tariff AS t 
                                        INNER JOIN kernel.Operators AS op ON t.OperatorId = op.Id 
                                        INNER JOIN kernel.Currency AS c ON c.Id = t.CurrencyId 
						                INNER JOIN kernel.servicenumbers sn on t.servicenumberid=sn.id
                                    WHERE 
                                        t.IsActive = 1 
                                                        ", con)) // AND t.TarifficationType = 1
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            paramTemplates.Add((Guid)dr["id"], (string)dr["Name"]);
                        }
                    }
                }
            }
        }
    } 
    #endregion
}
