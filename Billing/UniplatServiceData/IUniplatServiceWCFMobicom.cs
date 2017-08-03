

using System;
using System.Security.Cryptography;
using System.Text;

namespace MobicomLibrary
{
    public partial class MobicomRegisterRequest
    {
        public bool CheckHashCode(string password)
        {
            var checkingValue = Owner.id + Client.Phone.number + Payment.amount + Payment.currency + Payment.result +
                                Transaction.id + password;
            var data = Encoding.GetEncoding(1251).GetBytes(checkingValue);
            var md5 = MD5.Create();

            var Hash = Convert.ToBase64String(md5.ComputeHash(data));

            return Hash == hash;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", ConfigurationName = "IUniplatServiceWCFMobicom")]
    public interface IUniplatServiceWCFMobicom
    {

        // CODEGEN: Generating message contract since the operation MobicomReserveRequestOperation is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action = "urn:mobicomReserveRequest", ReplyAction = "urn:mobicomReserveRequest")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        MobicomReserveRequestOperationResponse MobicomReserveRequestOperation(MobicomReserveRequestOperationRequest request);

        // CODEGEN: Generating message contract since the operation MobicomReserveExpressRequestOperation is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action = "urn:mobicomReserveExpressRequest", ReplyAction = "urn:mobicomReserveExpressRequest")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        MobicomReserveExpressRequestOperationResponse MobicomReserveExpressRequestOperation(MobicomReserveExpressRequestOperationRequest request);

        // CODEGEN: Generating message contract since the operation MobicomRegisterRequestOperation is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action = "urn:mobicomRegisterRequest", ReplyAction = "urn:mobicomRegisterRequest")]
        [System.ServiceModel.XmlSerializerFormatAttribute()]
        MobicomRegisterRequestOperationResponse MobicomRegisterRequestOperation(MobicomRegisterRequestOperationRequest request);
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MobicomReserveRequest
    {

        private Agregator agregatorField;

        private Merchant merchantField;

        private Client clientField;

        private Payment paymentField;

        private Transaction transactionField;

        private string versionField;

        private string hashField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Agregator Agregator
        {
            get
            {
                return this.agregatorField;
            }
            set
            {
                this.agregatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public Merchant Merchant
        {
            get
            {
                return this.merchantField;
            }
            set
            {
                this.merchantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public Client Client
        {
            get
            {
                return this.clientField;
            }
            set
            {
                this.clientField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public Payment Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public Transaction Transaction
        {
            get
            {
                return this.transactionField;
            }
            set
            {
                this.transactionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string hash
        {
            get
            {
                return this.hashField;
            }
            set
            {
                this.hashField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MobicomRegisterResponse
    {

        private Agregator agregatorField;

        private Merchant merchantField;

        private Owner ownerField;

        private Client clientField;

        private Payment paymentField;

        private Transaction transactionField;

        private Message messageField;

        private Result resultField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Agregator Agregator
        {
            get
            {
                return this.agregatorField;
            }
            set
            {
                this.agregatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public Merchant Merchant
        {
            get
            {
                return this.merchantField;
            }
            set
            {
                this.merchantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public Owner Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public Client Client
        {
            get
            {
                return this.clientField;
            }
            set
            {
                this.clientField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public Payment Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public Transaction Transaction
        {
            get
            {
                return this.transactionField;
            }
            set
            {
                this.transactionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public Message Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
        public Result Result
        {
            get
            {
                return this.resultField;
            }
            set
            {
                this.resultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MobicomRegisterRequest
    {

        private Agregator agregatorField;

        private Merchant merchantField;

        private Owner ownerField;

        private Client clientField;

        private Regular regularField;

        private Payment paymentField;

        private Transaction transactionField;

        private string versionField;

        private string hashField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Agregator Agregator
        {
            get
            {
                return this.agregatorField;
            }
            set
            {
                this.agregatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public Merchant Merchant
        {
            get
            {
                return this.merchantField;
            }
            set
            {
                this.merchantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public Owner Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public Client Client
        {
            get
            {
                return this.clientField;
            }
            set
            {
                this.clientField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 4)]
        public Regular Regular
        {
            get
            {
                return this.regularField;
            }
            set
            {
                this.regularField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public Payment Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public Transaction Transaction
        {
            get
            {
                return this.transactionField;
            }
            set
            {
                this.transactionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string hash
        {
            get
            {
                return this.hashField;
            }
            set
            {
                this.hashField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MobicomReserveExpressResponse
    {

        private Owner ownerField;

        private Payment paymentField;

        private Message messageField;

        private Result resultField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Owner Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public Payment Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public Message Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public Result Result
        {
            get
            {
                return this.resultField;
            }
            set
            {
                this.resultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MobicomReserveExpressRequest
    {

        private Agregator agregatorField;

        private Merchant merchantField;

        private Owner ownerField;

        private string versionField;

        private string hashField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Agregator Agregator
        {
            get
            {
                return this.agregatorField;
            }
            set
            {
                this.agregatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public Merchant Merchant
        {
            get
            {
                return this.merchantField;
            }
            set
            {
                this.merchantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public Owner Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string hash
        {
            get
            {
                return this.hashField;
            }
            set
            {
                this.hashField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MobicomReserveResponse
    {

        private Agregator agregatorField;

        private Merchant merchantField;

        private Client clientField;

        private Payment paymentField;

        private Transaction transactionField;

        private Message messageField;

        private Result resultField;

        private Owner ownerField;

        private string versionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Agregator Agregator
        {
            get
            {
                return this.agregatorField;
            }
            set
            {
                this.agregatorField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public Merchant Merchant
        {
            get
            {
                return this.merchantField;
            }
            set
            {
                this.merchantField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 2)]
        public Client Client
        {
            get
            {
                return this.clientField;
            }
            set
            {
                this.clientField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 3)]
        public Payment Payment
        {
            get
            {
                return this.paymentField;
            }
            set
            {
                this.paymentField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 4)]
        public Transaction Transaction
        {
            get
            {
                return this.transactionField;
            }
            set
            {
                this.transactionField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 5)]
        public Message Message
        {
            get
            {
                return this.messageField;
            }
            set
            {
                this.messageField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 6)]
        public Result Result
        {
            get
            {
                return this.resultField;
            }
            set
            {
                this.resultField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 7)]
        public Owner Owner
        {
            get
            {
                return this.ownerField;
            }
            set
            {
                this.ownerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class MobicomReserveRequestOperationRequest
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", Order = 0)]
        public MobicomReserveRequest MCResrvReq;

        public MobicomReserveRequestOperationRequest()
        {
        }

        public MobicomReserveRequestOperationRequest(MobicomReserveRequest MCResrvReq)
        {
            this.MCResrvReq = MCResrvReq;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class MobicomReserveRequestOperationResponse
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", Order = 0)]
        public MobicomReserveResponse MCResrvRes;

        public MobicomReserveRequestOperationResponse()
        {
        }

        public MobicomReserveRequestOperationResponse(MobicomReserveResponse MCResrvRes)
        {
            this.MCResrvRes = MCResrvRes;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class MobicomReserveExpressRequestOperationRequest
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", Order = 0)]
        public MobicomReserveExpressRequest MCResrvExReq;

        public MobicomReserveExpressRequestOperationRequest()
        {
        }

        public MobicomReserveExpressRequestOperationRequest(MobicomReserveExpressRequest MCResrvExReq)
        {
            this.MCResrvExReq = MCResrvExReq;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class MobicomReserveExpressRequestOperationResponse
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", Order = 0)]
        public MobicomReserveExpressResponse MCResrvExRes;

        public MobicomReserveExpressRequestOperationResponse()
        {
        }

        public MobicomReserveExpressRequestOperationResponse(MobicomReserveExpressResponse MCResrvExRes)
        {
            this.MCResrvExRes = MCResrvExRes;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class MobicomRegisterRequestOperationRequest
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", Order = 0)]
        public MobicomRegisterRequest MCRegistReq;

        public MobicomRegisterRequestOperationRequest()
        {
        }

        public MobicomRegisterRequestOperationRequest(MobicomRegisterRequest MCRegistReq)
        {
            this.MCRegistReq = MCRegistReq;
        }
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped = false)]
    public partial class MobicomRegisterRequestOperationResponse
    {

        [System.ServiceModel.MessageBodyMemberAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", Order = 0)]
        public MobicomRegisterResponse MCRegistRes;

        public MobicomRegisterRequestOperationResponse()
        {
        }

        public MobicomRegisterRequestOperationResponse(MobicomRegisterResponse MCRegistRes)
        {
            this.MCRegistRes = MCRegistRes;
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IUniplatServiceWcfMobicomChannel : IUniplatServiceWCFMobicom, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class UniplatServiceWcfMobicomClient : System.ServiceModel.ClientBase<IUniplatServiceWCFMobicom>, IUniplatServiceWCFMobicom
    {

        public UniplatServiceWcfMobicomClient()
        {
        }

        public UniplatServiceWcfMobicomClient(string endpointConfigurationName) :
            base(endpointConfigurationName)
        {
        }

        public UniplatServiceWcfMobicomClient(string endpointConfigurationName, string remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public UniplatServiceWcfMobicomClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
            base(endpointConfigurationName, remoteAddress)
        {
        }

        public UniplatServiceWcfMobicomClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
            base(binding, remoteAddress)
        {
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobicomReserveRequestOperationResponse IUniplatServiceWCFMobicom.MobicomReserveRequestOperation(MobicomReserveRequestOperationRequest request)
        {
            return base.Channel.MobicomReserveRequestOperation(request);
        }

        public MobicomReserveResponse MobicomReserveRequestOperation(MobicomReserveRequest MCResrvReq)
        {
            MobicomReserveRequestOperationRequest inValue = new MobicomReserveRequestOperationRequest();
            inValue.MCResrvReq = MCResrvReq;
            MobicomReserveRequestOperationResponse retVal = ((IUniplatServiceWCFMobicom)(this)).MobicomReserveRequestOperation(inValue);
            return retVal.MCResrvRes;
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobicomReserveExpressRequestOperationResponse IUniplatServiceWCFMobicom.MobicomReserveExpressRequestOperation(MobicomReserveExpressRequestOperationRequest request)
        {
            return base.Channel.MobicomReserveExpressRequestOperation(request);
        }

        public MobicomReserveExpressResponse MobicomReserveExpressRequestOperation(MobicomReserveExpressRequest MCResrvExReq)
        {
            MobicomReserveExpressRequestOperationRequest inValue = new MobicomReserveExpressRequestOperationRequest();
            inValue.MCResrvExReq = MCResrvExReq;
            MobicomReserveExpressRequestOperationResponse retVal = ((IUniplatServiceWCFMobicom)(this)).MobicomReserveExpressRequestOperation(inValue);
            return retVal.MCResrvExRes;
        }

        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        MobicomRegisterRequestOperationResponse IUniplatServiceWCFMobicom.MobicomRegisterRequestOperation(MobicomRegisterRequestOperationRequest request)
        {
            return base.Channel.MobicomRegisterRequestOperation(request);
        }

        public MobicomRegisterResponse MobicomRegisterRequestOperation(MobicomRegisterRequest MCRegistReq)
        {
            MobicomRegisterRequestOperationRequest inValue = new MobicomRegisterRequestOperationRequest();
            inValue.MCRegistReq = MCRegistReq;
            MobicomRegisterRequestOperationResponse retVal = ((IUniplatServiceWCFMobicom)(this)).MobicomRegisterRequestOperation(inValue);
            return retVal.MCRegistRes;
        }
    }

}
