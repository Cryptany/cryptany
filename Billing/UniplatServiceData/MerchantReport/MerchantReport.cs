﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 

namespace MobicomMerchantReport
{

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://mobicom.oceanbank.ru/xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", IsNullable = false)]
    public partial class MerchantReport
    {

        private MerchantReportMerchant[] merchantField;

        private string versionField;

        private string dateFromField;

        private string dateToField;

        private int agregatorField;

        private int totalCountField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Merchant")]
        public MerchantReportMerchant[] Merchant
        {
            get { return this.merchantField; }
            set { this.merchantField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string version
        {
            get { return this.versionField; }
            set { this.versionField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string dateFrom
        {
            get { return this.dateFromField; }
            set { this.dateFromField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string dateTo
        {
            get { return this.dateToField; }
            set { this.dateToField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int agregator
        {
            get { return this.agregatorField; }
            set { this.agregatorField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int totalCount
        {
            get { return this.totalCountField; }
            set { this.totalCountField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MerchantReportMerchant
    {

        private int idField;

        private string createTimeField;

        private string editTimeField;

        private string closeTimeField;

        private string brandField;

        private string nameField;

        private MerchantReportMerchantOwner ownerField;

        private MerchantReportMerchantProviderLinksProvider[] providerLinksField;

        /// <remarks/>
        public int id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }

        /// <remarks/>
        public string createTime
        {
            get { return this.createTimeField; }
            set { this.createTimeField = value; }
        }

        /// <remarks/>
        public string editTime
        {
            get { return this.editTimeField; }
            set { this.editTimeField = value; }
        }

        /// <remarks/>
        public string closeTime
        {
            get { return this.closeTimeField; }
            set { this.closeTimeField = value; }
        }

        /// <remarks/>
        public string brand
        {
            get { return this.brandField; }
            set { this.brandField = value; }
        }

        /// <remarks/>
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }

        /// <remarks/>
        public MerchantReportMerchantOwner Owner
        {
            get { return this.ownerField; }
            set { this.ownerField = value; }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Provider", typeof (MerchantReportMerchantProviderLinksProvider)
            , IsNullable = false)]
        public MerchantReportMerchantProviderLinksProvider[] ProviderLinks
        {
            get { return this.providerLinksField; }
            set { this.providerLinksField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MerchantReportMerchantOwner
    {

        private string idField;

        /// <remarks/>
        public string id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MerchantReportMerchantProviderLinksProvider
    {

        private int idField;

        private MerchantReportMerchantProviderLinksProviderCategory categoryField;

        /// <remarks/>
        public int id
        {
            get { return this.idField; }
            set { this.idField = value; }
        }

        /// <remarks/>
        public MerchantReportMerchantProviderLinksProviderCategory Category
        {
            get { return this.categoryField; }
            set { this.categoryField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://mobicom.oceanbank.ru/xsd")]
    public partial class MerchantReportMerchantProviderLinksProviderCategory
    {

        private string providerCodeField;

        private string minAmountField;

        private string maxAmountField;

        private string abonentInterestField;

        private bool activeField;

        /// <remarks/>
        public string providerCode
        {
            get { return this.providerCodeField; }
            set { this.providerCodeField = value; }
        }

        /// <remarks/>
        public string minAmount
        {
            get { return this.minAmountField; }
            set { this.minAmountField = value; }
        }

        /// <remarks/>
        public string maxAmount
        {
            get { return this.maxAmountField; }
            set { this.maxAmountField = value; }
        }

        /// <remarks/>
        public string abonentInterest
        {
            get { return this.abonentInterestField; }
            set { this.abonentInterestField = value; }
        }

        /// <remarks/>
        public bool active
        {
            get { return this.activeField; }
            set { this.activeField = value; }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://mobicom.oceanbank.ru/xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://mobicom.oceanbank.ru/xsd", IsNullable = false)]
    public partial class NewDataSet
    {

        private MerchantReport[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("MerchantReport")]
        public MerchantReport[] Items
        {
            get { return this.itemsField; }
            set { this.itemsField = value; }
        }
    }
}