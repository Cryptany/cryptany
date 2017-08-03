using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XMLBlockSettings.Validators;
using System.Xml.Serialization;
using System.IO;

namespace XMLBlockSettings
{
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public class BlockSettings
    {
        [System.Xml.Serialization.XmlElementAttribute("Condition", typeof(Condition), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public List<Condition> Conditions;

        private BaseValidator validator;

        public BlockSettings(string BlockType)
        {
            switch (BlockType)
            {
                case "1":
                    this.validator = new SendSMSValidator();
                    break;
                case "2":
                    this.validator = new SubscriptionValidator();
                    break;
            }
        }


        public void LoadSettings(string XMLSettings) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(BlockSettings));
            string buffer = XMLSettings.Substring(XMLSettings.IndexOf("<BlockSettings"));
            buffer = buffer.Replace(Environment.NewLine, "");

            Stream XMLStream = new MemoryStream();

            var SetingsInBytes = Encoding.Default.GetBytes(buffer);
            XMLStream.Write(SetingsInBytes, 0, SetingsInBytes.Length);

            XMLStream.Seek(0, SeekOrigin.Begin);
            List < Condition > BuffConditions = ((BlockSettings)serializer.Deserialize(XMLStream)).Conditions;
            validator.Validate(BuffConditions);
            this.Conditions = BuffConditions;
        }

        public string SaveSettings(BlockSettings Settings)
        {
            validator.Validate(Settings.Conditions);
            this.Conditions = Settings.Conditions;

            XmlSerializer serializer = new XmlSerializer(typeof(BlockSettings));
            Stream str = new MemoryStream();

            serializer.Serialize(str, Settings);

            str.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(str);
            string result = reader.ReadToEnd();
            result = result.Substring(result.IndexOf("<BlockSettings"));
            result = result.Replace(Environment.NewLine, "");

            return result;
        }


    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
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

}
