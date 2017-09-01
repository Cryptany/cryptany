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
using System.Text;

namespace Cryptany.Core.ConfigOM.DataAccessManager
{
    //[Serializable]
    //internal class PriceInfo :  IPriceInfo
    //{
    //    private Guid _operatotBrandId;
    //    private string _operatorBrandName;
    //    private decimal _amount;
    //    private string _currencyCode;
    //    private string _currencyName;

    //    public PriceInfo(OperatorBrand op, Tariff tariff)
    //    {
    //        OperatorBrandId = (Guid)op.ID;
    //        OperatorBrandName = op.Name;
    //        Amount = tariff.Value;
    //        CurrencyCode = tariff.Currency.Code;
    //        CurrencyName = tariff.Currency.Name;
    //    }

    //    public Guid OperatorBrandId
    //    {
    //        get
    //        {
    //            return _operatotBrandId;
    //        }
    //        set
    //        {
    //            _operatotBrandId = value;
    //        }
    //    }

    //    public string OperatorBrandName
    //    {
    //        get
    //        {
    //            return _operatorBrandName;
    //        }
    //        set
    //        {
    //            _operatorBrandName = value;
    //        }
    //    }

    //    public decimal Amount
    //    {
    //        get
    //        {
    //            return _amount;
    //        }
    //        set
    //        {
    //            _amount = value;
    //        }
    //    }

    //    public string CurrencyCode
    //    {
    //        get
    //        {
    //            return _currencyCode;
    //        }
    //        set
    //        {
    //            _currencyCode = value;
    //        }
    //    }

    //    public string CurrencyName
    //    {
    //        get
    //        {
    //            return _currencyName;
    //        }
    //        set
    //        {
    //            _currencyName = value;
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        string name = OperatorBrandName != null ? OperatorBrandName : "<?>";
    //        string amount = Amount.ToString();
    //        string curCode = CurrencyCode != null ? CurrencyCode : " ? ";
    //        return string.Format("{0} - {1}{2}", name, amount, curCode);
    //    }
    //}
}
