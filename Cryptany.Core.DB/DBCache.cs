/*
   Copyright 2006-2017 Cryptany, Inc.

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;
using System.Data;
using System.Data.Objects;
using System.Diagnostics;

namespace Cryptany.Core.DB
{
    public static class DBCache
    {
        static TransportEntities data;
        static DefCodeContainer defs = null;
        static public void Refresh()
        {
            defs = new DefCodeContainer(data.GetDefCodes().ToArray());
            ServiceNumbers = data.ServiceNumber.ToArray();
            Tariffs = data.Tariff.ToArray();
            ContragentResources = data.ContragentResource.ToArray();
            SMSCSettings = data.SMSCSettings.ToArray();
            ParameterTypes = data.ParameterType.ToArray();
            CheckTypes = data.CheckType.ToArray();
            SMSCs = data.SMSC.ToArray();
            ServiceGroups = data.ServiceGroup.ToArray();
            ServiceGroupRules = data.ServiceGroupRule.ToArray();
            SmscToServiceGroups = data.SmscToServiceGroup.ToArray();
            RegionsToRegionGroup = data.RegionsToRegionGroup.ToArray();
        }
        static DBCache()
        {
            try
            {
                data = TransportEntities.Entities;
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
            }
            DBCache.Refresh();
        }
        #region Properties
        public static ServiceNumber[] ServiceNumbers
        {
            get;
            private set;
        }
        public static Tariff[] Tariffs
        {
            get;
            private set;
        }
        public static ContragentResource[] ContragentResources
        {
            get;
            private set;
        }
        public static ServiceGroup[] ServiceGroups
        {
            get;
            private set;
        }
        public static SMSCSettings[] SMSCSettings
        {
            get;
            private set;
        }
        public static ParameterType[] ParameterTypes
        {
            get;
            private set;
        }
        public static CheckType[] CheckTypes
        {
            get;
            private set;
        }
        public static SMSC[] SMSCs
        {
            get;
            private set;
        }
        public static SmscToServiceGroup[] SmscToServiceGroups
        {
            get;
            private set;
        }
        public static ServiceGroupRule[] ServiceGroupRules
        {
            get;
            private set;
        }
        public static RegionsToMacroRegions[] RegionsToMacroRegion
        {
            get;
            private set;
        }
        public static RegionsToRegionGroup[] RegionsToRegionGroup
        {
            get;
            private set;
        }
        #endregion

        static public T GetEntityById<T>(Guid id) where T : EntityObject
        {
            List<int> v = new List<int>();
            if (id == null)
                return null;
            object[] attributes = typeof(T).GetCustomAttributes(typeof(EdmEntityTypeAttribute), false);
            EdmEntityTypeAttribute attr = attributes[0] as EdmEntityTypeAttribute;
            string setName = data.DefaultContainerName + "." + attr.Name;
            EntityKey key = new EntityKey(setName, "Id", id);
            object result;
            if (data.TryGetObjectByKey(key, out result))
                return result as T;
            return null;
        }

        static public ServiceNumber GetServiceNumberBySN(string sn)
        {
            var set = ServiceNumbers.Where(n => { return n.SN == sn; });
            //ServiceNumber.Where("it.sn = @number", new ObjectParameter("number", sn));
            return set.FirstOrDefault();
        }

        static public Tariff[] GetActiveTariffsBySN(string sn)
        {
            var serviceNumber = GetServiceNumberBySN(sn);
            if (serviceNumber == null)
                return null;
            var set = Tariffs.Where(t =>
            { return t.ServiceNumberId == serviceNumber.Id && (t.IsActive.HasValue ? t.IsActive.Value : false); });
            //Tariff.Where("it.ServiceNumberId = @snid && it.isActive", new ObjectParameter("snid", serviceNumber.Id));
            return set.ToArray();
        }

        static public SMSC GetSMSCByCode(int code)
        {
            var set = SMSCs.Where(s => { return s.Code == code; });
            //SMSC.Where("it.Code = @c", new ObjectParameter("c", code));
            return set.First();
        }

        static public Guid GetBrandGuidBySN(string number)
        {
            var def = defs.GetDefCodeByNumber(number);
            return (def == null) ? Guid.Empty : def.brandId.Value;
        }

        static public Guid GetRegionGroupGuidBySN(string number)
        {
            var def = defs.GetDefCodeByNumber(number);
            if (def == null)
                return Guid.Empty;
            var link = RegionsToRegionGroup.SingleOrDefault(l => { return l.RegionId == def.RegionId; });
            return (link == null) ? Guid.Empty : link.RegionGroupId.Value;
        }

        static public Guid GetMacroRegionGuidBySN(string number)
        {
            var def = defs.GetDefCodeByNumber(number);
            if (def == null)
                return Guid.Empty;
            var link = RegionsToMacroRegion.SingleOrDefault(l => { return l.RegionId == def.RegionId; });
            return (link == null) ? Guid.Empty : link.MacroRegionId;
        }
    }
}
