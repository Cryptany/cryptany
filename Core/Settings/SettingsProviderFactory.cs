using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Configuration;
using System.Diagnostics;
namespace avantMobile.Settings
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
				//try
				//{
					if (_SettingsProvider == null)
					{
						object[] args = new object[1];
						args[0] = ConfigurationManager.AppSettings["SettingsProviderSource"];

						string ClassName = ConfigurationManager.AppSettings["DefaultSettingsProviderClassName"];
                        
                        try
                        {
                            object tmpResult = Assembly.GetAssembly(Type.GetType(ClassName)).CreateInstance(ClassName,
                                                                                                            false,
                                                                                                            BindingFlags
                                                                                                                .
                                                                                                                CreateInstance,
                                                                                                            null, args,
                                                                                                            System.
                                                                                                                Globalization
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
					            EventLog.WriteEntry("Application", "Невозможно создать экземпляр класса " + ClassName + ": " + ex , EventLogEntryType.Error);
					    }
					    
					}
                    IsLoaded = true;
					return _SettingsProvider;
                //}
                //catch (Exception ex)
                //{
                //    Trace.WriteLine(ex, DateTime.Now.ToLongTimeString());
                //}

				//return null;
			}
		}

	}
}
