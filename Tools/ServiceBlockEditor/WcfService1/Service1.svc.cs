//#define OldScheme

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;
using System.ServiceModel.Activation;
using WcfService1.XMLSettings.SettingParams;
using XMLBlockSettings;
using System.IO;

namespace WcfService1
{
   

    public class RouteDBService : IRouteDBService, IPolicyRetriever
    {
        static string ConnectionString = ConfigurationManager.ConnectionStrings["RouterConnection"].ConnectionString;

        #region GetData
        public ListOfService GetServices()
        {
            List<Service> Services = new List<Service>();

            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                using (SqlCommand sqlCom = new SqlCommand(
                    @"  select 
                          serv.Id, 
                          serv.Name, 
                          serv.ServiceNumberId, 
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
                                serviceNumber = dr.IsDBNull(dr.GetOrdinal("serviceNumberId")) ? String.Empty : (string)dr["serviceNumberId"].ToString(),
                                typeid = (Guid)dr["ServiceTypeId"],
                                typename = (string)dr["ServiceTypeName"]
                            };
                            buffer.serviceBlocksId = GetServiceBlocks(buffer.id);
                            //buffer.expressions = GetServiceExpressions(buffer.id);
                            Services.Add(buffer);
                        }
                    }
                }
            }


            return new ListOfService { LOS = Services };
        }

        Dictionary<Guid, string> GetServiceBlocks(Guid id)
        {
            Dictionary<Guid, string> result = new Dictionary<Guid, string>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @" select id, name
                        from router.ServiceBlock
                        where ServiceId =  @ServId", con))
                {

                    cmd.Parameters.AddWithValue("@ServId", id);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add((Guid)dr["id"], (string)dr["name"]);
                        }
                    }
                }
            }
            return result;
        }

//        List<string> GetServiceExpressions(Guid id)
//        {
//            List<string> result = new List<string>();
//            using (SqlConnection con = new SqlConnection(ConnectionString))
//            {
//                con.Open();
//                using (SqlCommand cmd = new SqlCommand(
//                    @"select expression
//                      from router.RegularExpression
//                      where ServiceId = @ServId", con))
//                {

//                    cmd.Parameters.AddWithValue("@ServId", id);
//                    using (SqlDataReader dr = cmd.ExecuteReader())
//                    {
//                        while (dr.Read())
//                        {
//                            result.Add((string)dr["expression"]);
//                        }
//                    }
//                }
//            }
//            return result;
//        }

        //______________________________________________________________________________________________//

        public ServiceBlock GetServiceBlock(Guid ID, string Name)
        {
            ServiceBlock sBlock = new ServiceBlock();
            sBlock.name = Name; 
            List<Block> blocks = new List<Block>();


            //1. Качаем из БД блоки, входящие в данный сервисный блок (с типом блока)
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(

                                        #if OldScheme
                                        @"select b.*, bt.Name as blockTypeName from router.Block b
                                            inner join router.BlockType bt on bt.Id = b.BlockTypeId
                                            where ServiceBlockID  = @ServBlockId" 

                                        #else
                                        @"select 
	                                        be.IsVerification, 
                                            be.Id as BlockEntryId,
	                                        bt.Name as BlockTypeName, 
	                                        b.Id,
	                                        b.BlockTypeId,
	                                        b.Name,
	                                        b.Settings
                                        from 
	                                        router.BlockEntry be 
	                                        inner join router.Block b on b.Id = be.BlockId
	                                        inner join router.BlockType bt on bt.Id = b.BlockTypeId
                                        where 
	                                        be.ServiceBlockId = @ServBlockId"
                                            #endif
                                            , con))
                {

                    cmd.Parameters.AddWithValue("@ServBlockId", ID);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            blocks.Add(
                                new Block
                                {
                                    id = (Guid)dr["id"],
                                    isVerification = (bool)dr["isVerification"],
                                    name = (string)dr["name"],
                                    typeid = (Guid)dr["blockTypeid"],
                                    typename = (string)dr["blockTypeName"],
                                    settingsString = (string)dr["settings"],
                                    blockEntryId = (Guid)dr["BlockEntryId"]
                                });
                        }
                    }
                }
            }

            foreach (Block item in blocks)
            {
                //3. Разбираем XML, запихиваем в класс, проверяем валидность
                item.settings = BaseBlock.GetBlockSettings(item.typename, item.settingsString);
                //4. Качаем из БД ссылки для блоков
                item.links = GetBlockLinks(item.blockEntryId);
            }
            sBlock.blocks = blocks;
            sBlock.id = ID;
            return sBlock;

        }

        List<BlockLink> GetBlockLinks(Guid blockEntryId)
        {
            List<BlockLink> result = new List<BlockLink>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
#if OldScheme
                        @"select * from router.BlockLink
                            where ParentId = @BlockId 
                            OR ChildId = @BlockId" 
#else
@"	select
		                        childrenBe.BlockId as childId,
		                        parentBe.BlockId as parentId,
		                        childrenBe.Id as EchildId,
		                        parentBe.Id as EparentId,                                
		                        bl.Id,
		                        bl.Kind
	                        from 
		                        router.BlockLink bl
		                        inner join router.BlockEntry childrenBe on childrenBe.id = bl.ChildId 
		                        inner join router.BlockEntry parentBe on parentBe.id = bl.ParentId
                            where
		                        childrenBe.Id = @blockEntryId 
		                        OR parentBe.Id = @blockEntryId"
#endif


, con))
                {

                    cmd.Parameters.AddWithValue("@blockEntryId", blockEntryId);
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(
                                new BlockLink
                                {
                                    id = (Guid)dr["id"],
                                    linkedEntryBlockId = ((Guid)dr["EparentId"] == blockEntryId) ? (Guid)dr["EchildId"] : (Guid)dr["EparentId"],
                                    linkedBlockId = ((Guid)dr["EparentId"] == blockEntryId) ? (Guid)dr["childId"] : (Guid)dr["parentId"],
                                    output = ((Guid)dr["EparentId"] == blockEntryId),
                                    yes = (bool)dr["kind"]
                                });
                        }
                    }
                }
            }
            return result;
        }

        //______________________________________________________________________________________________//

        public List<Block> GetAllBlocks()
        {
            List<Block> result = new List<Block>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(
                    @"select 
			            b.* ,
			            bt.Name as BlockTypeName
		            from 
			            router.Block b
			            left join router.BlockType bt on bt.Id = b.BlockTypeId"
                                                , con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            try
                            {

                                result.Add(new Block()
                                {
                                    blockEntryId = new Guid(),
                                    id = (Guid)dr["Id"],
                                    isVerification = true,
                                    links = new List<BlockLink>(),
                                    name = (string)dr["Name"],
                                    settings = BaseBlock.GetBlockSettings((string)dr["BlockTypeName"], (string)dr["Settings"]),
                                    settingsString = (string)dr["Settings"],
                                    typeid = (Guid)dr["BlockTypeId"],
                                    typename = (string)dr["BlockTypeName"]
                                });

                            }
                            catch (Exception ex)
                            {
                                throw new Exception(((Guid)dr["Id"]).ToString() + "  " + (string)dr["Name"] + " " + ex.Message);
                            }
                        }
                    }
                }
            }
            return result;
        }

        public Dictionary<Guid, string> GetAllBlockTypes()
        {
            Dictionary<Guid, string> result = new Dictionary<Guid, string>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"select * from router.BlockType", con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add((Guid)dr["Id"], (string)dr["Name"]);
                        }
                    }
                }
            }
            return result;
        }

        public List<BaseParam> GetBlockInfo(string BlockType)
        {
            return BaseBlock.GetBlockSettingsInfo(BlockType);
        }

        public List<BlockSettingsParam> GetBlockSettingsParams()
        {
            List<BlockSettingsParam> result = new List<BlockSettingsParam>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(@"select * From
                (  
                SELECT  t.Id, CAST(t.Amount AS VARCHAR(15)) + ' ' + c.Code + ' ' + op.DisplayName +' '+SN.sn  AS Name, 'tariff' as type
                FROM         kernel.Tariff AS t INNER JOIN
                                      kernel.Operators AS op ON t.OperatorId = op.Id INNER JOIN
                                      kernel.Currency AS c ON c.Id = t.CurrencyId inner join 
						                kernel.servicenumbers sn on t.servicenumberid=sn.id
                where t.IsActive = 1 and t.TarifficationType = 1
                ) as a
                union
                (
                SELECT [Id]
                      ,[Name]
                      ,'club' as type
                  FROM [clubs2].[clubs].[Clubs]
                  )
                  order by type, name", con))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            result.Add(new BlockSettingsParam() {  id = (Guid)dr["id"], value = (string)dr["name"], type = (string)dr["type"] });
                        }
                    }
                }
            }

            result.Add(new BlockSettingsParam() { id = Guid.Empty, value = "Subscribe", type = "action" });
            result.Add(new BlockSettingsParam() { id = Guid.Empty, value = "Unsubscribe", type = "action" });
            result.Add(new BlockSettingsParam() { id = Guid.Empty, value = "UnsubscribeAll", type = "action" });
            return result;

           
        }

        #endregion

        #region DeleteData
        public void RemoveBlockFromServiceBlock(Guid ServiceBlockId)
        {

                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
#if OldScheme
                using (SqlCommand sqlCom = new SqlCommand(
                            "Delete router.Block where id = @BlockId"
                            , sqlCon))
                {
                    sqlCom.Parameters.AddWithValue("@BlockId", BlockId);
                    sqlCom.ExecuteNonQuery();
                } 
#else
                    /*
                 там есть такой триггер
                 
                   create trigger 
			            router.BlockEntry_delete
		            on 
			            router.BlockEntry
		            INSTEAD OF delete
		            as
		            set nocount on
		            delete
		            from 
			            router.BlockLink
		            where 
			            ChildId in ( select id from deleted )
			            OR ParentId in ( select id from deleted )
			
		            delete
		            from 
			            router.BlockEntry
		            WHERE 
			            id IN ( SELECT id FROM deleted )
			
		            return

                 */
//                    using (SqlCommand sqlCom = new SqlCommand(
//                                @"Delete router.BlockEntry 
//                            where Id = @BlockEntryId"
//                                , sqlCon))
//                    {
//                        sqlCon.Open();
//                        sqlCom.Parameters.AddWithValue("@BlockEntryId", BlockEntryId);
//                        sqlCom.ExecuteNonQuery();
//                    }
                    using (SqlCommand sqlCom = new SqlCommand(
                               @"Delete router.BlockEntry 
                            where ServiceBlockId = @ServiceBlockId"
                               , sqlCon))
                    {
                        sqlCon.Open();
                        sqlCom.Parameters.AddWithValue("@ServiceBlockId", ServiceBlockId);
                        sqlCom.ExecuteNonQuery();
                    }
                }
#endif
            
        }

        //public void RemoveServiceBlockFromService(Guid ServiceBlockId)
        //{
        //    using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
        //    {
        //        using (SqlCommand sqlCom = new SqlCommand(
        //                    "Delete router.ServiceBlock where id = @ServiceBlockId"
        //                    , sqlCon))
        //        {
        //            sqlCom.Parameters.AddWithValue("@ServiceBlockId", ServiceBlockId);
        //            sqlCom.ExecuteNonQuery();
        //        }
        //    }
        //} 
        #endregion

        #region AddData
        public void SaveServiceBlock(ServiceBlock serviceBlock)
        {
            //1. очищаем БД от старых блоков, вход в сервисный блок  
            //foreach (var block in serviceBlock.blocks)
            RemoveBlockFromServiceBlock(serviceBlock.id);
            
            //2. добавляем 
            foreach (var block in serviceBlock.blocks)
                SaveBlock(block, serviceBlock.id);

            foreach (var block in serviceBlock.blocks)
            // добавляем связи между блоками
            foreach (var link in block.links)
                SaveLink(link, block.blockEntryId);
    
        }

        void SaveBlock(Block block, Guid serviceBlockId)
        { 
             using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                using (SqlCommand sqlCom = new SqlCommand(
                    @"  insert into router.BlockEntry(id, serviceBlockId, BlockId, IsVerification)
		                values (@id, @serviceBlockId, @BlockId, @IsVerification)"
                    , sqlCon))
                {
                    sqlCom.Parameters.AddWithValue("@id", block.blockEntryId);
                    sqlCom.Parameters.AddWithValue("@BlockId", block.id);
                    sqlCom.Parameters.AddWithValue("@IsVerification", block.isVerification);
                    sqlCom.Parameters.AddWithValue("@serviceBlockId", serviceBlockId);
                    sqlCon.Open();
                    sqlCom.ExecuteNonQuery();
                }
            }

           
        }

        void SaveLink(BlockLink link, Guid BlockId)
        {
            if (!link.output) return;
            
            using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
            {
                using (SqlCommand sqlCom = new SqlCommand(
                    @"  insert into router.BlockLink(id, ParentId, ChildId, Kind)
		                values (@id, @ParentId, @ChildId, @Kind)"
                    , sqlCon))
                {
                    sqlCom.Parameters.AddWithValue("@id", link.id);
                    sqlCom.Parameters.AddWithValue("@ParentId", BlockId);
                    sqlCom.Parameters.AddWithValue("@ChildId", link.linkedBlockId);
                    sqlCom.Parameters.AddWithValue("@Kind", link.yes);
                    sqlCon.Open();
                    sqlCom.ExecuteNonQuery();
                }
            }
        }

        public string CreateNewBlock(Block Block)
        {
            try
            {
                Block.settingsString = BaseBlock.GetXMLString(Block);
                using (SqlConnection sqlCon = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand sqlCom = new SqlCommand(
                        @"    INSERT INTO 
		                            [avant2].[router].[Block](Id, Name, BlockTypeId,Settings)
                              VALUES
		                            (@Id, @Name, @TypeId, @Settings)"
                        , sqlCon))
                    {
                        sqlCom.Parameters.AddWithValue("@Id", Block.id);
                        sqlCom.Parameters.AddWithValue("@Name", Block.name);
                        sqlCom.Parameters.AddWithValue("@TypeId", Block.typeid);
                        sqlCom.Parameters.AddWithValue("@Settings", Block.settingsString);
                        sqlCon.Open();
                        sqlCom.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "Блок добавлен";
        }

        #endregion

//        #region accessPolicy
//        Stream StringToStream(string result)
//        {
//            WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
//            return new MemoryStream(Encoding.UTF8.GetBytes(result));
//        }
//        public Stream GetSilverlightPolicy()
//        {
////            string result = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
////<access-policy>
////  <cross-domain-access>
////    <policy>
////      <allow-from http-request-headers=""SOAPAction"">
////        <domain uri=""*""/>
////      </allow-from>
////      <grant-to>
////        <resource path=""/"" include-subpaths=""true""/>
////      </grant-to>
////    </policy>
////  </cross-domain-access>
////</access-policy>";
////            return StringToStream(result);
//            const string result = @"<?xml version=""1.0"" encoding=""utf-8""?>
//              <access-policy>    
//              <cross-domain-access>        
//              <policy>            
//              <allow-from http-request-headers=""*"">                
//              <domain uri=""*""/>            
//              </allow-from>            
//              <grant-to>                
//              <resource path=""/"" include-subpaths=""true""/>            
//              </grant-to>        
//              </policy>    
//              </cross-domain-access>
//              </access-policy>"; 

//              if (WebOperationContext.Current != null)                
//                WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml"; return   new MemoryStream(Encoding.UTF8.GetBytes(result)); 

//        }
//        public Stream GetFlashPolicy()
//        {
//            string result = @"<?xml version=""1.0"" ?>
//<!DOCTYPE cross-domain-policy SYSTEM 
//	""http://www.macromedia.com/xml/dtds/cross-domain-policy.dtd"">
//<cross-domain-policy>
//  <allow-http-request-headers-from domain=""*"" headers=""SOAPAction,Content-Type""/>
//</cross-domain-policy>";
//            return StringToStream(result);
//        }

//        public string Links()
//        {

//            return "path = " + ";";//Links(path, resource, extension);
//        }

//        #endregion


        Stream StringToStream(string result)
        {
            WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }
        public Stream GetSilverlightPolicy()
        {
            byte[] buffer = null;
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Position = 0;
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    sw.WriteLine(@"<?xml version=""1.0"" encoding=""utf-8""?>
                                    <access-policy>
                                        <cross-domain-access>
                                            <policy>
                                                <allow-from http-request-headers=""*"">
                                                    <domain uri=""*""/>
                                                </allow-from>
                                                <grant-to>
                                                    <resource path=""/"" include-subpaths=""true""/>
                                                </grant-to>
                                            </policy>
                                        </cross-domain-access>
                                    </access-policy>");
                }
                buffer = ms.GetBuffer();
            }
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
            return new MemoryStream(buffer);
//           string result = @"<?xml version=""1.0"" encoding=""utf-8""?>
//            <access-policy>
//                <cross-domain-access>
//                    <policy>
//                        <allow-from http-request-headers=""*"">
//                            <domain uri=""*""/>
//                        </allow-from>
//                        <grant-to>
//                            <resource path=""/"" include-subpaths=""true""/>
//                        </grant-to>
//                    </policy>
//                </cross-domain-access>
//            </access-policy>";

//            return StringToStream(result);
        }
        public Stream GetFlashPolicy()
        {
            string result = @"<?xml version=""1.0""?>
<!DOCTYPE cross-domain-policy SYSTEM ""http://www.macromedia.com/xml/dtds/cross-domain-policy.dtd"">
<cross-domain-policy>
    <allow-access-from domain=""*"" />
</cross-domain-policy>";
            return StringToStream(result);
        }
    }
}
