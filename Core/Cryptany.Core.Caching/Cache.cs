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
using Microsoft.Practices.EnterpriseLibrary.Caching;
using Microsoft.Practices.EnterpriseLibrary.Caching.Expirations;
using System.Configuration;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using System.Collections.Generic;


namespace Cryptany.Core.Caching
{

    public delegate void ItemAddedOrRemovedEventHandler();

    public class Cache
    {
        private CacheManager CacheMan;
        public TimeSpan _expirationTimeSpan;
        //string LogName = "Application";
        CacheItemPriority _CacheItemPriority;
        public event ItemAddedOrRemovedEventHandler ItemAddedOrRemoved;

        public ICacheItemRefreshAction refreshener;

        public Cache()
        {

            try
            {
                CacheMan = CacheFactory.GetCacheManager();
                CacheMan.Add("Items", new List<string>());
            }

            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, "LoggingPolicy");
            }


            try
            {
                string CacheItemPriorityStr = ConfigurationManager.AppSettings["CacheItemPriority"];
                _CacheItemPriority = (CacheItemPriority)Enum.Parse(typeof(CacheItemPriority), CacheItemPriorityStr);
            }
            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, "LoggingPolicy");
                _CacheItemPriority = CacheItemPriority.Normal;
            }

            int Timeout;
            try
            {
                Timeout = Convert.ToInt32(ConfigurationManager.AppSettings["Timeout"]);
            }

            catch (Exception ex)
            {
                ExceptionPolicy.HandleException(ex, "LoggingPolicy");
                Timeout = 5;
            }
            _expirationTimeSpan = TimeSpan.FromMinutes(Timeout);


        }

        public void Add<T>(string key, T _value)
        {

            object obj = _value;
            try
            {

                Trace.WriteLine("Cache: Adding to cache key " + key + " Value " + _value);
                //ItemsRemover _itemsRemover = new ItemsRemover();
                //Trace.WriteLine("Cache: Создали _itemsRemover ");
                // _itemsRemover.ItemRemoved += new ItemAddedOrRemovedEventHandler(ItemAddedOrRemoved);
                Trace.WriteLine(" Cache: Adding key " + key + " obj " + obj + " ItemPriority " + _CacheItemPriority + " expiration " + _expirationTimeSpan);
                CacheMan.Add(key, obj, _CacheItemPriority, refreshener, new SlidingTime(_expirationTimeSpan));
                Trace.WriteLine("Cache: Successfully added to cache");
                (CacheMan["Items"] as List<string>).Add(key);
                Trace.WriteLine("Cache: Updated Items");
                
                if (ItemAddedOrRemoved != null) 
                    ItemAddedOrRemoved();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                ExceptionPolicy.HandleException(ex, "LoggingPolicy");
                throw new ArgumentException();
            }

        }
        public void Remove(string key)
        {
            if (CacheMan.Contains(key))
            {
                CacheMan.Remove(key);

                (CacheMan["Items"] as List<string>).Remove(key);

                if (ItemAddedOrRemoved != null) ItemAddedOrRemoved();
            }
        }

        public void Flush()
        {
            CacheMan.Flush();
            (CacheMan["Items"] as List<string>).Clear();
            if (ItemAddedOrRemoved != null) ItemAddedOrRemoved();
        }

        public bool GetItem<T>(string key, out T val)
        {


            if (CacheMan.Contains(key))
            {
                val = (T)CacheMan.GetData(key);
                return true;

            }
            val = default(T);
            return false;
            // throw new ElementNotInCacheException();

        }

        public bool Contains(string key)
        {

            return CacheMan.Contains(key);

        }

        public int Count
        {
            get { return CacheMan.Count - 1; }
        }

        //private void WriteToLog(Exception ex)
        //{
        //    EventInstance eventInst = new EventInstance(0, 0, EventLogEntryType.Error);
        //    EventLog.WriteEvent(LogName, eventInst, ex.ToString());

        //}

        public List<string> CacheItems
        {
            get { return CacheMan["Items"] as List<string>; }
        }


    }


    //[Serializable]
    //public class ItemsRemover : ICacheItemRefreshAction
    //{
    //    public event ItemAddedOrRemovedEventHandler ItemRemoved;

    //    public void Refresh(string removedKey,
    //     Object expiredValue,
    //     CacheItemRemovedReason removalReason)
    //    {

    //        ItemRemoved();

    //    }
    //}

    //public class ElementNotInCacheException : Exception
    //{
    //}

}
