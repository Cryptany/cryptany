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

namespace Cryptany.Core.DB
{
    /// <summary>
    ///  Хранит список Российских DEF кодов. Позволяет быстро находить DEF код для номера
    /// </summary>
    internal class DefCodeContainer
    {
        Dictionary<long, DEFCode> codes;
        long[] numbers;
        public DefCodeContainer(DEFCode[] defs)
        {
            try
            {
                codes = defs.ToDictionary(c =>
                {
                    return ((long)c.DEF.Value) * 10000000 + c.RangeStart.Value;
                });
            }
            catch(Exception e)
            { 
				throw new ApplicationException("Cannot init DefCodeContainer with defs:", e);
			}
            var keys = codes.Keys.ToList();
            keys.Sort();
            numbers = keys.ToArray();
        }


        /// <summary>
        /// Определяем DEF код по номеру телефона
        /// </summary>
        /// <param name="msisdn"></param>
        /// <returns></returns>
        public DEFCode GetDefCodeByNumber(string msisdn)
        {
            long number = long.Parse(msisdn.Trim().Substring(1));
            int a = 0;
            int b = numbers.Length - 1;
            int iterations = 0;
            while (a < b)
            {
                int m = (a + b + 1) / 2;
                if (number < numbers[m])
                    b = m - 1;
                else
                    a = m;
                if (iterations++ > 20)
                    throw new ApplicationException("Binary search GetDefCodeByNumber cycled");
            }
            return codes[numbers[a]];
        }
    }
}
