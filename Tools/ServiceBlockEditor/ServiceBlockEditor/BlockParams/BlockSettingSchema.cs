
//using System.Xml.Serialization;

///// <remarks/>
//[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
//[System.SerializableAttribute()]
//[System.Diagnostics.DebuggerStepThroughAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
//[System.Xml.Serialization.XmlRootAttribute(Namespace="", IsNullable=false)]
//public partial class BlockSettings {
    
//    private object[] itemsField;
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute("Condition", typeof(SingleConditionType), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
//    [System.Xml.Serialization.XmlElementAttribute("Setting", typeof(SingleSettingType), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
//    public object[] Items {
//        get {
//            return this.itemsField;
//        }
//        set {
//            this.itemsField = value;
//        }
//    }
//}

///// <remarks/>
//[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
//[System.SerializableAttribute()]
//[System.Diagnostics.DebuggerStepThroughAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//public partial class SingleConditionType {
    
//    private string propertyField;
    
//    private OperationType operationField;
    
//    private bool operationFieldSpecified;
    
//    private string valueField;
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string Property {
//        get {
//            return this.propertyField;
//        }
//        set {
//            this.propertyField = value;
//        }
//    }
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public OperationType Operation {
//        get {
//            return this.operationField;
//        }
//        set {
//            this.operationField = value;
//        }
//    }
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlIgnoreAttribute()]
//    public bool OperationSpecified {
//        get {
//            return this.operationFieldSpecified;
//        }
//        set {
//            this.operationFieldSpecified = value;
//        }
//    }
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string Value {
//        get {
//            return this.valueField;
//        }
//        set {
//            this.valueField = value;
//        }
//    }
//}

///// <remarks/>
//[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
//[System.SerializableAttribute()]
//public enum OperationType {
    
//    /// <remarks/>
//    In,
    
//    /// <remarks/>
//    NotIn,
    
//    /// <remarks/>
//    Equals,
    
//    /// <remarks/>
//    NotEquals,
//}

///// <remarks/>
//[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
//[System.SerializableAttribute()]
//[System.Diagnostics.DebuggerStepThroughAttribute()]
//[System.ComponentModel.DesignerCategoryAttribute("code")]
//public partial class SingleSettingType {
    
//    private string propertyField;
    
//    private string valueField;
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string Property {
//        get {
//            return this.propertyField;
//        }
//        set {
//            this.propertyField = value;
//        }
//    }
    
//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string Value {
//        get {
//            return this.valueField;
//        }
//        set {
//            this.valueField = value;
//        }
//    }
//}
