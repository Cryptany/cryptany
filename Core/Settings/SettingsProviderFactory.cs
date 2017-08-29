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
using System.Reflection;
using System.Configuration;
using System.Diagnostics;

namespace Cryptany.Common.Settings
{
	/// <summary>
	/// Класс предоставляет доступ к единственной копии провейдера настроек по-умолчанию.
	/// </summary>
	/// <remarks></remarks>
	public abstract class SettingsProviderFactory
	{
		private static ISettingsProvider _SettingsProvider;

        private static bool IsLoaded = false;
		/// <summary>
		/// Создаёт единственную копию поставщика настроек. Имя класса поставщика указывается в app.config configuration/appSettings/add[@key='DefaultSettingsProviderClassName']/@value
		/// В качестве аргумента передаётся значение указанное в app.config configuration/appSettings/add[@key='SettingsProviderSource']/@value
		/// </summary>
		public static ISettingsProvider DefaultSettingsProvider
		{
			get
			{
					if (_SettingsProvider == null)
					{
						object[] args = new object[1];
						args[0] = ConfigurationManager.AppSettings["SettingsProviderSource"];

						string ClassName = ConfigurationManager.AppSettings["DefaultSettingsProviderClassName"];
                        
                        try
                        {
                            object tmpResult = Assembly.GetAssembly(Type.GetType(ClassName)).CreateInstance(ClassName,
                                                                                                            false,
                                                                                                            BindingFlags.
                                                                                                                CreateInstance,
                                                                                                            null, args,
                                                                                                            System.Globalization
                                                                                                                .
                                                                                                                CultureInfo
                                                                                                                .
                                                                                                                CurrentCulture,
                                                                                                            null);
                            _SettingsProvider = tmpResult as ISettingsProvider;
                        }
                        catch (Exception ex)
					    {
                            if (!IsLoaded)
					            EventLog.WriteEntry("Application", "Unable to create class instance " + ClassName + ": " + ex , EventLogEntryType.Error);
					    }
					    
					}
                    IsLoaded = true;
					return _SettingsProvider;
                }
			}
		}

}
