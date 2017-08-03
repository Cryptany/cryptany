// -----------------------------------------------------------------------
// <copyright file="BaseValidator.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XMLBlockSettings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using WcfService1;
    using WcfService1.XMLSettings.SettingParams;
    using System.Xml.Serialization;
    using System.IO;
    using XMLBlockSettings.Validators;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class BaseBlock
    {
        public virtual List<string> blockParams { get; set; }
        static BaseBlock block;

        public virtual void Validate(List<Condition> BuffConditions)
        {
            if (BuffConditions.Count != BuffConditions.Select((a) => { return a.Property; }).Distinct().Count())
                throw new Exception("Обнаружены повторяющиеся параметры");

            if (BuffConditions.FirstOrDefault((a) => { return a.Value == ""; }) != null)
                throw new Exception("Обнаружены параметры c пустым значением");


            if (BuffConditions.Select(a => a.Property).Except(blockParams).Count() > 0)
                throw new Exception("В блоке присутствуют неверные параметры");
        }

        public static List<BaseParam> GetBlockSettingsInfo(string BlockType)
        {
            switch (BlockType)
            {
                case "ConditionBlock":
                    return ConditionBlockValidator.GetBlockSettingsInfo(BlockType);
                    
                case "SendSMSBlock":
                    return SendSMSValidator.GetBlockSettingsInfo(BlockType);
                    
                case "SubscribtionConfirmBlock":
                    return SubscribtionConfirmValidator.GetBlockSettingsInfo(BlockType);
                    
                case "SubscribtionBlock":
                    return SubscriptionValidator.GetBlockSettingsInfo(BlockType);

                default:
                    return new List<BaseParam>();
                    
            }
        }

        public static _BlockSettings GetBlockSettings(string BlockType, string XMLString)
        {
            _BlockSettings bSettings = new _BlockSettings();
            bSettings.Conditions = new List<_Condition>();

            #region десереализация
            XmlSerializer serializer = new XmlSerializer(typeof(BlockSettings));
            string buffer = XMLString.Substring(XMLString.IndexOf("<BlockSettings"));
            buffer = buffer.Replace(Environment.NewLine, "");

            Stream XMLStream = new MemoryStream();

            var SetingsInBytes = Encoding.UTF8.GetBytes(buffer);
            XMLStream.Write(SetingsInBytes, 0, SetingsInBytes.Length);

            XMLStream.Seek(0, SeekOrigin.Begin);
            List<Condition> BuffConditions = ((BlockSettings)serializer.Deserialize(XMLStream)).Conditions;
            #endregion

            switch (BlockType)
            {
                case "ConditionBlock":
                    block = new ConditionBlockValidator();
                    break;
                case "SendSMSBlock":
                    block = new SendSMSValidator();
                    break;
                case "SubscribtionConfirmBlock":
                    block = new SubscribtionConfirmValidator();
                    break;
                case "SubscribtionBlock":
                    block = new SubscriptionValidator();
                    break;
            }

            block.Validate(BuffConditions);

            foreach (var item in BuffConditions)
                bSettings.Conditions.Add(new _Condition() {  Property = item.Property, Operation = item.Operation.ToString(), Value = item.Value});

            return bSettings;
        }

        public static string GetXMLString(Block Block)
        {
            List<Condition>  BuffConditions = new List<Condition>();
            foreach (var item in Block.settings.Conditions)
                BuffConditions.Add(new Condition() { Property = item.Property, Operation = (OperationType)Enum.Parse(typeof(OperationType), item.Operation), Value = item.Value });
           
            #region проверка
            switch (Block.typename)
            {
                case "ConditionBlock":
                    block = new ConditionBlockValidator();
                    break;
                case "SendSMSBlock":
                    block = new SendSMSValidator();
                    break;
                case "SubscribtionConfirmBlock":
                    block = new SubscribtionConfirmValidator();
                    break;
                case "SubscribtionBlock":
                    block = new SubscriptionValidator();
                    break;
            }

            block.Validate(BuffConditions);
            #endregion

            #region сериализация
            XmlSerializer serializer = new XmlSerializer(typeof(BlockSettings));
            Stream str = new MemoryStream();

            serializer.Serialize(str, new BlockSettings() { Conditions = BuffConditions }); 
            #endregion

            str.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(str);
            string result = reader.ReadToEnd();
            result = result.Substring(result.IndexOf("<BlockSettings"));
            result = result.Replace(Environment.NewLine, "");

            return result;
        }

        #region реализация
        //public void LoadSettings(string XMLSettings)
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(BlockSettings));
        //    string buffer = XMLSettings.Substring(XMLSettings.IndexOf("<BlockSettings"));
        //    buffer = buffer.Replace(Environment.NewLine, "");

        //    Stream XMLStream = new MemoryStream();

        //    var SetingsInBytes = Encoding.Default.GetBytes(buffer);
        //    XMLStream.Write(SetingsInBytes, 0, SetingsInBytes.Length);

        //    XMLStream.Seek(0, SeekOrigin.Begin);
        //    List<Condition> BuffConditions = ((BlockSettings)serializer.Deserialize(XMLStream)).Conditions;
        //    validator.Validate(BuffConditions);
        //    this.Conditions = BuffConditions;
        //}

        //public string SaveSettings(SettingsParsing Settings)
        //{
        //    validator.Validate(Settings.Conditions);
        //    this.Conditions = Settings.Conditions;

        //    XmlSerializer serializer = new XmlSerializer(typeof(SettingsParsing));
        //    Stream str = new MemoryStream();

        //    serializer.Serialize(str, Settings);

        //    str.Seek(0, SeekOrigin.Begin);
        //    StreamReader reader = new StreamReader(str);
        //    string result = reader.ReadToEnd();
        //    result = result.Substring(result.IndexOf("<BlockSettings"));
        //    result = result.Replace(Environment.NewLine, "");

        //    return result;
        //} 
        #endregion
    }


    #region XMLSettings

    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class BlockSettings
    {
        [System.Xml.Serialization.XmlElementAttribute("Condition", typeof(Condition), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Condition> Conditions;
    }


    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    public enum OperationType
    {
        Default,
        /// <remarks/>
        In,

        /// <remarks/>
        NotIn,

        /// <remarks/>
        Equals,

        /// <remarks/>
        NotEquals,

        /// <remarks/>
        Contains,

        /// <remarks/>
        NotContains,

        /// <remarks/>
        Empty,

        /// <remarks/>
        NotEmpty,
    }


    public class Condition
    {
        [XmlAttribute]
        public string Property;
        [XmlAttribute()]
        public OperationType Operation;
        [XmlAttribute]
        public string Value;
    }
    #endregion



}
