

namespace _UniplatServiceWCFMobicom
{
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
    public partial class Agregator
    {

        private long idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public long id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
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
    public partial class Merchant
    {

        private long idField;

        private string nameField;

        private Owner ownerField;

        private string brandField;

        private string wwwField;

        private System.Nullable<System.DateTime> createTimeField;

        private System.Nullable<System.DateTime> editTimeField;

        private System.Nullable<System.DateTime> closeTimeField;

        private Account[] accountLinksField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public long id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 3)]
        public string brand
        {
            get
            {
                return this.brandField;
            }
            set
            {
                this.brandField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 4)]
        public string www
        {
            get
            {
                return this.wwwField;
            }
            set
            {
                this.wwwField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 5)]
        public System.Nullable<System.DateTime> createTime
        {
            get
            {
                return this.createTimeField;
            }
            set
            {
                this.createTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 6)]
        public System.Nullable<System.DateTime> editTime
        {
            get
            {
                return this.editTimeField;
            }
            set
            {
                this.editTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 7)]
        public System.Nullable<System.DateTime> closeTime
        {
            get
            {
                return this.closeTimeField;
            }
            set
            {
                this.closeTimeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true, Order = 8)]
        public Account[] AccountLinks
        {
            get
            {
                return this.accountLinksField;
            }
            set
            {
                this.accountLinksField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Owner
    {

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Account
    {

        private string accountField;

        private string nameField;

        private Bank bankField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public string account
        {
            get
            {
                return this.accountField;
            }
            set
            {
                this.accountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public Bank Bank
        {
            get
            {
                return this.bankField;
            }
            set
            {
                this.bankField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Bank
    {

        private string bikField;

        private string nameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public string bik
        {
            get
            {
                return this.bikField;
            }
            set
            {
                this.bikField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Client
    {

        private Phone phoneField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public Phone Phone
        {
            get
            {
                return this.phoneField;
            }
            set
            {
                this.phoneField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Phone
    {

        private string numberField;

        private System.Nullable<int> providerField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string number
        {
            get
            {
                return this.numberField;
            }
            set
            {
                this.numberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public System.Nullable<int> provider
        {
            get
            {
                return this.providerField;
            }
            set
            {
                this.providerField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Payment
    {

        private Payer payerField;

        private string narrativeField;

        private System.Nullable<long> amountField;

        private System.Nullable<int> currencyField;

        private SupplierOrgInfo supplierOrgInfoField;

        private string kbkField;

        private string okatoField;

        private BudgetIndex budgetIndexField;

        private Param[] paramsField;

        private System.Nullable<int> resultField;

        private int kindField;

        private bool kindFieldSpecified;

        private string supplierBillIDField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public Payer Payer
        {
            get
            {
                return this.payerField;
            }
            set
            {
                this.payerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string narrative
        {
            get
            {
                return this.narrativeField;
            }
            set
            {
                this.narrativeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public System.Nullable<long> amount
        {
            get
            {
                return this.amountField;
            }
            set
            {
                this.amountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 3)]
        public System.Nullable<int> currency
        {
            get
            {
                return this.currencyField;
            }
            set
            {
                this.currencyField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 4)]
        public SupplierOrgInfo SupplierOrgInfo
        {
            get
            {
                return this.supplierOrgInfoField;
            }
            set
            {
                this.supplierOrgInfoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 5)]
        public string kbk
        {
            get
            {
                return this.kbkField;
            }
            set
            {
                this.kbkField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 6)]
        public string okato
        {
            get
            {
                return this.okatoField;
            }
            set
            {
                this.okatoField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 7)]
        public BudgetIndex BudgetIndex
        {
            get
            {
                return this.budgetIndexField;
            }
            set
            {
                this.budgetIndexField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true, Order = 8)]
        public Param[] Params
        {
            get
            {
                return this.paramsField;
            }
            set
            {
                this.paramsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 9)]
        public System.Nullable<int> result
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
        public int kind
        {
            get
            {
                return this.kindField;
            }
            set
            {
                this.kindField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool kindSpecified
        {
            get
            {
                return this.kindFieldSpecified;
            }
            set
            {
                this.kindFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string supplierBillID
        {
            get
            {
                return this.supplierBillIDField;
            }
            set
            {
                this.supplierBillIDField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Payer
    {

        private string nameField;

        private string accountField;

        private Address addressField;

        private string innField;

        private string identifierField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string account
        {
            get
            {
                return this.accountField;
            }
            set
            {
                this.accountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public Address Address
        {
            get
            {
                return this.addressField;
            }
            set
            {
                this.addressField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 3)]
        public string inn
        {
            get
            {
                return this.innField;
            }
            set
            {
                this.innField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 4)]
        public string identifier
        {
            get
            {
                return this.identifierField;
            }
            set
            {
                this.identifierField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Address
    {

        private string viewField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string view
        {
            get
            {
                return this.viewField;
            }
            set
            {
                this.viewField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class SupplierOrgInfo
    {

        private string nameField;

        private string innField;

        private string kppField;

        private Account accountField;

        private Address[] addressesField;

        private Contact[] contactsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string inn
        {
            get
            {
                return this.innField;
            }
            set
            {
                this.innField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public string kpp
        {
            get
            {
                return this.kppField;
            }
            set
            {
                this.kppField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 3)]
        public Account Account
        {
            get
            {
                return this.accountField;
            }
            set
            {
                this.accountField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true, Order = 4)]
        public Address[] Addresses
        {
            get
            {
                return this.addressesField;
            }
            set
            {
                this.addressesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(IsNullable = true, Order = 5)]
        public Contact[] Contacts
        {
            get
            {
                return this.contactsField;
            }
            set
            {
                this.contactsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Contact
    {

        private string kindField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string kind
        {
            get
            {
                return this.kindField;
            }
            set
            {
                this.kindField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 1)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class BudgetIndex
    {

        private string statusField;

        private string paymentTypeField;

        private string purposeField;

        private string taxPeriodField;

        private string taxDocNumberField;

        private string taxDocDateField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public string status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string paymentType
        {
            get
            {
                return this.paymentTypeField;
            }
            set
            {
                this.paymentTypeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public string purpose
        {
            get
            {
                return this.purposeField;
            }
            set
            {
                this.purposeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 3)]
        public string taxPeriod
        {
            get
            {
                return this.taxPeriodField;
            }
            set
            {
                this.taxPeriodField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 4)]
        public string taxDocNumber
        {
            get
            {
                return this.taxDocNumberField;
            }
            set
            {
                this.taxDocNumberField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 5)]
        public string taxDocDate
        {
            get
            {
                return this.taxDocDateField;
            }
            set
            {
                this.taxDocDateField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Param
    {

        private string nameField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Transaction
    {

        private string idField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Message
    {

        private string billField;

        private string commentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public string bill
        {
            get
            {
                return this.billField;
            }
            set
            {
                this.billField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Result
    {

        private System.Nullable<int> codeField;

        private string extraField;

        private string commentField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
        public System.Nullable<int> code
        {
            get
            {
                return this.codeField;
            }
            set
            {
                this.codeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public string extra
        {
            get
            {
                return this.extraField;
            }
            set
            {
                this.extraField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public string comment
        {
            get
            {
                return this.commentField;
            }
            set
            {
                this.commentField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Refund
    {

        private Result resultField;

        private System.Nullable<System.DateTime> timestampField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public System.Nullable<System.DateTime> timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Register
    {

        private Result resultField;

        private System.Nullable<System.DateTime> timestampField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public System.Nullable<System.DateTime> timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Reserve
    {

        private Result resultField;

        private System.Nullable<System.DateTime> timestampField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public System.Nullable<System.DateTime> timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Start
    {

        private Result resultField;

        private System.Nullable<System.DateTime> timestampField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 0)]
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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
        public System.Nullable<System.DateTime> timestamp
        {
            get
            {
                return this.timestampField;
            }
            set
            {
                this.timestampField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("svcutil", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class Regular
    {

        private Owner ownerField;

        private Payment paymentField;

        private Start startField;

        private Reserve reserveField;

        private Register registerField;

        private Refund refundField;

        private Transaction transactionField;

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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 1)]
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
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 2)]
        public Start Start
        {
            get
            {
                return this.startField;
            }
            set
            {
                this.startField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 3)]
        public Reserve Reserve
        {
            get
            {
                return this.reserveField;
            }
            set
            {
                this.reserveField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 4)]
        public Register Register
        {
            get
            {
                return this.registerField;
            }
            set
            {
                this.registerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 5)]
        public Refund Refund
        {
            get
            {
                return this.refundField;
            }
            set
            {
                this.refundField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable = true, Order = 6)]
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
