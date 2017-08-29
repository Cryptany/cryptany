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
using System.Configuration;
using System.Data;
using System.Collections.Generic;
using Cryptany.Core.DPO;


namespace Cryptany.Core.ConfigOM
{
    /// <summary>
    /// Работа с хранилищем данных PersistentStorage
    /// </summary>
    public class ChannelConfiguration 
    {
        /// <summary>
        /// хранилище данных по-умолчанию - xml, dataset, MSsql
        /// </summary>
        public static PersistentStorage DefaultPs
		{
            get
            {
                if (ClassFactory.PsExists("Default"))
                    return ClassFactory.CreatePersistentStorage("Default");

                Cryptany.Common.Settings.ISettingsProvider settings = Cryptany.Common.Settings.SettingsProviderFactory.DefaultSettingsProvider;
                string config = settings["CryptanyConfigOMSettings"] as string;

                string connectionString = ConfigurationManager.ConnectionStrings["defaultConnectionString"].ConnectionString;
                return ClassFactory.CreatePersistentStorage("Default", connectionString, config);
            }
		}

        public ChannelConfiguration()
        {
			PersistentStorage ps = Ps;
            //Answer - таблица текстов ответных смс
			ps.GetEntities(typeof(Answer));
        }

        /// <summary>
        /// возвращает ссылку на default-хранилище
        /// </summary>
		public PersistentStorage Ps
		{
			get
			{
				return DefaultPs; 
            }
		}

        /// <summary>
        /// удаляет default-хранилище
        /// </summary>
        public static void SetDSToNull()
        {
			ClassFactory.DeletePersistentStorage("Default");
        }

        /// <summary>
        /// пересоздаёт default-хранилище
        /// </summary>
        public static PersistentStorage ReloadDefaultPs()
		{
			SetDSToNull();
			return DefaultPs;
		}

        /// <summary>
        /// useless shit
        /// </summary>
        /// <param name="dontLoad"></param>
        public ChannelConfiguration(bool dontLoad)
        { }

        /// <summary>
        /// useless shit
        /// </summary>
        public static void ReloadDataSet()
        { }
    }
}
