using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace MobicomMerchantReport
{
    public partial class MerchantReport
    {
        public DateTime DateFrom
        {
            get { return DateTime.ParseExact(dateFrom, "yyyy-MM-dd", CultureInfo.InvariantCulture); }
            set { dateFrom = value.ToString("yyyy-MM-dd"); }
        }

        public DateTime DateTo
        {
            get { return DateTime.ParseExact(dateTo, "yyyy-MM-dd", CultureInfo.InvariantCulture); }
            set { dateTo = value.ToString("yyyy-MM-dd"); }
        }
    }

    public partial class MerchantReportMerchant
    {
        private const string DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss";

        public DateTime CreateTime
        {
            get { return DateTime.ParseExact(createTime, DateFormat, CultureInfo.InvariantCulture); }
            set { createTime = value.ToString(DateFormat); }
        }

        /// <remarks/>
        public DateTime? EditTime
        {
            get { return string.IsNullOrEmpty(editTime) ? null : (DateTime?)DateTime.ParseExact(editTime, DateFormat, CultureInfo.InvariantCulture); }
            set { editTime = value.HasValue ? value.Value.ToString(DateFormat) : ""; }
        }

        /// <remarks/>
        public DateTime? CloseTime
        {
            get { return string.IsNullOrEmpty(closeTime) ? null : (DateTime?)DateTime.ParseExact(closeTime, DateFormat, CultureInfo.InvariantCulture); }
            set { closeTime = value.HasValue ? value.Value.ToString(DateFormat) : ""; }
        }
    }

    public partial class MerchantReportMerchantProviderLinksProviderCategory
    {
        /// <remarks/>
        public int? MinAmount
        {
            get { return string.IsNullOrEmpty(minAmount) ? null : (int?)int.Parse(minAmount); }
            set { minAmount = value == null ? "" : value.ToString(); }
        }

        /// <remarks/>
        public int? MaxAmount
        {
            get { return string.IsNullOrEmpty(maxAmount) ? null : (int?)int.Parse(maxAmount); }
            set { maxAmount = value == null ? "" : value.ToString(); }
        }

        /// <remarks/>
        public decimal? AbonentInterest
        {
            get { return string.IsNullOrEmpty(abonentInterest) ? null : (decimal?)decimal.Parse(abonentInterest, CultureInfo.InvariantCulture); }
            set { abonentInterest = value == null ? "" : value.ToString(); }
        }
    }
}