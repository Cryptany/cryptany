using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WcfService1;

namespace XMLBlockSettings
{

    public class SendSMSValidator : BaseBlock
    {
        public override List<string> blockParams
        {
            get
            {
                List<string> ConditionBlockParams = new List<string>();
                ConditionBlockParams.Add("ServiceNumber");
                ConditionBlockParams.Add("TariffId");
                ConditionBlockParams.Add("Text");
                ConditionBlockParams.Add("Offset");
                ConditionBlockParams.Add("AnswerToConnector");  
                return ConditionBlockParams;
            }
        }

        public override void Validate(List<Condition> BuffConditions)
        {
            base.Validate(BuffConditions);
            List<string> properties = BuffConditions.Select(a=>a.Property).ToList();
            if (!properties.Contains("Text"))
                throw new Exception("Не все обязательные параметры использованы");
            if(properties.Contains("ServiceNumber") && properties.Contains("TariffId"))
                throw new Exception("В блоке присутствует либо сервисный номер, либо тариф");
        }

        public static List<BaseParam> GetBlockSettingsInfo(string BlockType)
        {
            List<BaseParam> result = new List<BaseParam>();

            BaseParam ServiceNumber = new BaseParam()
            {
                BlockType = BlockType,
                listParam = true,
                name = "ServiceNumber",
                paramSource = "[kernel].[ServiceNumbers]",
                required = false
            };
            ServiceNumber.conditionOperators.Add("", "Default");
            ServiceNumber.GetParamTemplates("avant2.kernel.ServiceNumbers", "SN");

            BaseParam TariffId = new BaseParam()
            {
                BlockType = BlockType,
                listParam = false,
                name = "TariffId",
                paramSource = "avant2.kernel.Tariff",
                required = false
            };
            TariffId.conditionOperators.Add("", "Default");
            TariffId.GetParamTemplatesForTariff();

            BaseParam Text = new BaseParam()
            {
                BlockType = BlockType,
                listParam = false,
                name = "Text",
                paramSource = "",
                required = true
            };
            Text.conditionOperators.Add("", "Default");
            
            BaseParam Offset = new BaseParam()
            {
                BlockType = BlockType,
                listParam = false,
                name = "Offset",
                paramSource = "",
                required = false
            };
            Offset.conditionOperators.Add("", "Default");

            BaseParam AnswerToConnector = new BaseParam()
            {
                BlockType = BlockType,
                listParam = false,
                name = "AnswerToConnector",
                paramSource = "",
                required = false
            };
            AnswerToConnector.conditionOperators.Add("", "Default");
            AnswerToConnector.paramTemplates.Add(Guid.NewGuid(), Boolean.TrueString);
            AnswerToConnector.paramTemplates.Add(Guid.NewGuid(), Boolean.FalseString);

            result.Add(ServiceNumber);
            result.Add(TariffId);
            result.Add(Text);
            result.Add(Offset);
            result.Add(AnswerToConnector);

            return result;
        }
    }
}
