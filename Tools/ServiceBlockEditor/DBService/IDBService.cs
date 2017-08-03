using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using XMLBlockSettings;

namespace DBService
{
    [ServiceContract]
    public interface IDBService
    {
        //[OperationContract]
        //Block GetBlock(Guid id);

        [OperationContract]
        ListOfService GetServices();
    }

    //[DataContract]
    //public class Block
    //{
    //    [DataMember]
    //    Guid id { get; set; }

    //    [DataMember]
    //    string name { get; set; }

    //    [DataMember]
    //    Guid blockTypeID { get; set; }

    //    [DataMember]
    //    string blockType { get; set; }

    //    [DataMember]
    //    BlockSettings settings { get; set; }

    //    [DataMember]
    //    bool isVerification { get; set; }

    //}

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

        [DataMember]
        public List<string> expressions { get; set; }

        [DataMember]
        public List<Guid> blocksId { get; set; }
    }

    [DataContract]
    public class ListOfService
    {
        [DataMember]
        public List<Service> LOS { get; set; }
    }
}
