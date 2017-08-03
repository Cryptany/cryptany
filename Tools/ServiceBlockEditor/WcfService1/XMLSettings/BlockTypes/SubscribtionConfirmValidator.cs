// -----------------------------------------------------------------------
// <copyright file="SubscriptionValidator.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace XMLBlockSettings.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using WcfService1;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SubscribtionConfirmValidator : BaseBlock
    {
        public override List<string> blockParams
        {
            get
            {
                List<string> ConditionBlockParams = new List<string>();
                ConditionBlockParams.Add("ClubId");
                return ConditionBlockParams;
            }
        }

        public override void Validate(List<Condition> BuffConditions)
        {
            base.Validate(BuffConditions);
            List<string> properties = BuffConditions.Select(a => a.Property).ToList();
            if (!properties.Contains("ClubId"))
                throw new Exception("Не все обязательные параметры использованы");

        }

        public static List<BaseParam> GetBlockSettingsInfo(string BlockType)
        {
            List<BaseParam> result = new List<BaseParam>();

            BaseParam ClubId = new BaseParam()
            {
                BlockType = BlockType,
                listParam = false,
                name = "ClubId",
                paramSource = "clubs2.clubs.clubs",
                required = false
            };
            ClubId.conditionOperators.Add("", "Default");
            ClubId.GetParamTemplates();

            result.Add(ClubId);

            return result;
        }

    }
}
