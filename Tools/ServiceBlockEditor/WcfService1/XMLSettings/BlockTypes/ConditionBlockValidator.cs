using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WcfService1;
using WcfService1.XMLSettings.SettingParams;

namespace XMLBlockSettings
{

    public class ConditionBlockValidator : BaseBlock
    {
        public override List<string> blockParams
        {
            get
            {
                List<string> ConditionBlockParams = new List<string>();
                ConditionBlockParams.Add("Brand");
                ConditionBlockParams.Add("SMSC");
                ConditionBlockParams.Add("Region");
                ConditionBlockParams.Add("Subscriptions");
                return ConditionBlockParams;
            }
        }
        public override void Validate(List<Condition> BuffConditions)
        {
            if (BuffConditions.Count != BuffConditions.Select((a) => { return a.Property; }).Distinct().Count())
                throw new Exception("Обнаружены повторяющиеся параметры");

            if (BuffConditions.FirstOrDefault((a) =>
            {
                return a.Value == ""
                    && !(a.Operation == OperationType.Empty)
                    && !(a.Operation == OperationType.NotEmpty);
            }) != null)
                throw new Exception("Обнаружены параметры c пустым значением");

            if (BuffConditions.FirstOrDefault((a) =>
            {
                return a.Value.Contains(";")
                    && (a.Operation == OperationType.Equals)
                    && (a.Operation == OperationType.NotEquals);
            }) != null)
                throw new Exception("Обнаружены параметры c недопустимым значением для заданной операции значением");


            if (BuffConditions.Select(a => a.Property).Except(blockParams).Count() > 0)
                throw new Exception("В блоке присутствуют неверные параметры");
        }
        public static List<BaseParam> GetBlockSettingsInfo(string BlockType)
        {
            List<BaseParam> result = new List<BaseParam>();

            BaseParam Brand = new BaseParam()
                {
                BlockType = BlockType,
                listParam = true,
                name = "Brand",
                paramSource = "kernel.OperatorBrands",
                required = true
                };

            Brand.conditionOperators.Add("Один из", "In");
            Brand.conditionOperators.Add("Ни один из", "NotIn");
            Brand.conditionOperators.Add("Содержит", "Contains");
            Brand.conditionOperators.Add("Не содержит", "NotContains");
            Brand.GetParamTemplates();

            BaseParam SMSC = new BaseParam()
                {
                BlockType = BlockType,
                listParam = true,
                name = "SMSC",
                paramSource = "kernel.SMSC",
                required = true
                };

            SMSC.conditionOperators.Add("Один из", "In");
            SMSC.conditionOperators.Add("Ни один из", "NotIn");
            SMSC.conditionOperators.Add("Содержит", "Contains");
            SMSC.conditionOperators.Add("Не содержит", "NotContains");
            SMSC.GetParamTemplates();

            BaseParam Region = new BaseParam()
                {
                BlockType = BlockType,
                listParam = true,
                name = "Region",
                paramSource = "kernel.Regions",
                required = true
                };

            Region.conditionOperators.Add("Один из", "In");
            Region.conditionOperators.Add("Ни один из", "NotIn");
            Region.conditionOperators.Add("Содержит", "Contains");
            Region.conditionOperators.Add("Не содержит", "NotContains");
            Region.GetParamTemplates("kernel.Regions","NODE_NAME");

             BaseParam Subscriptions = new BaseParam()
                {
                BlockType = BlockType,
                listParam = true,
                name = "Subscriptions",
                paramSource = "clubs2.clubs.Clubs",
                required = true
                };
            Subscriptions.conditionOperators.Add("Один из", "In");
            Subscriptions.conditionOperators.Add("Ни один из", "NotIn");
            Subscriptions.conditionOperators.Add("Содержит", "Contains");
            Subscriptions.conditionOperators.Add("Не содержит", "NotContains");
            Subscriptions.GetParamTemplates();

            result.Add(Brand);
            result.Add(SMSC);
            result.Add(Region);
            result.Add(Subscriptions);

            return result;
        }
    }
}
