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
using System.Web;
using System.Configuration;
using System.Reflection;

namespace Cryptany.Common.Logging
{
    /// <summary>
    /// Logger factory.
    /// </summary>
	public abstract class LoggerFactory
	{
		/// <summary>
		/// Предоставляет логер для общественного пользования в web приложения. 
		/// Не использовать в windows приложениях!!!
		/// </summary>
		public static ILogger WebSharedLogger
		{
			get
			{
				ILogger result = null;
				const string webSharedLoggerKey = "webSharedLogger";
				object tmp = HttpContext.Current.Cache[webSharedLoggerKey];
				if( tmp!=null)
					result = tmp as ILogger;

				if (result == null)
				{
                    string ClassName = "Cryptany.Common.Logging.XMLLogger";//SettingsProviderFactory.DefaultSettingsProvider["DefaultLoggerClassName"] as string;

					object tmpResult = Assembly.GetAssembly(Type.GetType(ClassName)).CreateInstance(ClassName);

					result = tmpResult as ILogger;
                    result.MaxCacheSize = 500000;//int)SettingsProviderFactory.DefaultSettingsProvider["DefaultLoggerCacheSize"];
					result.MaxWriteDelay =  new TimeSpan(0,0,30);
					result.AutoFlush = false;

					HttpContext.Current.Cache.Add(webSharedLoggerKey, result, null,
						System.Web.Caching.Cache.NoAbsoluteExpiration,
						new TimeSpan(0, 1, 0), System.Web.Caching.CacheItemPriority.Normal, new System.Web.Caching.CacheItemRemovedCallback( OnRemoverLoggerFromCache ) );

				}

				return result;
			}
		}

		public static ILogger CreateXmlLogger(string filePath)
		{
			return null;
		}

		/// <summary>
		/// Cleanup resources
		/// </summary>
		/// <param name="key"></param>
		/// <param name="obj"></param>
		/// <param name="reason"></param>
		private static void OnRemoverLoggerFromCache(string key, object obj, System.Web.Caching.CacheItemRemovedReason reason )
		{
			ILogger logger = obj as ILogger;
			logger.Dispose();
		}

		/// <summary>
		/// Предоставляет логер для общественного пользования в windows приложения. 
		/// Не использовать в web приложениях!!!
		/// </summary>
		public static ILogger Logger
		{
			get
			{
				return new SharedLogger();
			}
		}


		private class SharedLogger : ILogger
		{
			private ILogger logger;
			private int refCount;

			public SharedLogger()
			{
				refCount++;

				if (logger == null)
				{
                    string ClassName = "Cryptany.Common.Logging.XMLLogger";
				    object tmpResult = Assembly.GetAssembly(Type.GetType(ClassName)).CreateInstance(ClassName);

					logger = tmpResult as ILogger;
				}
			}


			#region ILogger Members

			private string _DefaultSource;
			public string DefaultSource
			{
				get
				{ return _DefaultSource; }
				set
				{
				    _DefaultSource = value;
                    logger.DefaultSource = value;
                    
				}
			}

			public bool AutoFlush
			{
				get
				{ return false; }
				set
				{ }
			}


			public int MaxCacheSize
			{
				get
				{
				    return logger.MaxCacheSize;// (int)SettingsProviderFactory.DefaultSettingsProvider["DefaultLoggerCacheSize"];
				}
				set
				{ }
			}

			public TimeSpan MaxWriteDelay
			{
				get
				{
				    return logger.MaxWriteDelay;
					//return new TimeSpan(0, 0, (int)SettingsProviderFactory.DefaultSettingsProvider["DefaultLoggerWriteDelay"]);
				}
				set
				{ }
			}

			public bool Write(LogMessage msg)
			{
				if (DefaultSource != null && msg.Source == null)
				{
					msg.Source = DefaultSource;
				}

				return logger.Write(msg);
			}

			public bool Flush()
			{
				return logger.Flush();
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				refCount--;
				if (refCount <= 0)
				{
					DisposeInternalObject();
				}
			}

			#endregion

			public void DisposeInternalObject()
			{
				if (logger != null)
					logger.Dispose();
				logger = null;
			}

            #region ILogger Members

            public string DefaultServiceSource
            {
                get
                {
                    return logger.DefaultServiceSource;
                }
                set
                {
                    logger.DefaultServiceSource = value;
                }
            }

            #endregion
        }

	}
}
