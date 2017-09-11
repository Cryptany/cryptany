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
using System.Collections.Generic;

namespace Cryptany.Core.DPO.Predicates
{
    public delegate bool NaryOperation(params object[] oo);
    public delegate bool BinaryOperation(object o1, object o2);
    public delegate bool UnaryOperation(object o);
    public delegate bool NaryOperation<T>(params T[] oo);
    public delegate bool BinaryOperation<T>(T o1, T o2);
    public delegate bool UnaryOperation<T>(T o);

    public static class Functions
    {
        public static bool And(params bool[] pars)
        {
            bool res = true;
            foreach (bool b in pars)
                if (!(b && res))
                    return false;
            return true;
        }

        public static bool Or(params bool[] pars)
        {
            bool res = false;
            foreach (bool b in pars)
                if (b || res)
                    return true;
            return false;
        }

        public static bool Not(bool par)
        {
            return !par;
        }

        new public static bool Equals(object o1, object o2)
        {
            return o1 == o2;
        }

        public static bool NotEquals(object o1, object o2)
        {
            return o1 != o2;
        }

        public static List<T> Select<T>(List<T> list, UnaryOperation<T> where)
        {
            List<T> l = new List<T>();
            foreach (T e in list)
                if (where(e))
                    l.Add(e);
            return l;
        }

        public static T SelectFirst<T>(List<T> list, UnaryOperation<T> where)
        {
            T found = list.Find(
                        delegate (T el)
                        {
                            return where(el);
                        }
                    );

            return found;//[0];
                         //else
                         //	return default(T);
        }
    }

    public static class PartialApplication
    {
        public static UnaryOperation Apply(BinaryOperation binOp, object arg)
        {
            return delegate (object e)
            {
                return binOp(arg, e);
            };
        }
    }
}
