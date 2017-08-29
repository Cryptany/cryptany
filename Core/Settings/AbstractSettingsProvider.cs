using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace Cryptany.Common.Settings
{
	/// <summary>
	/// Abstract class with basic functionality available for all derivatives.
	/// </summary>
	public abstract class AbstractSettingsProvider : ISettingsProvider
	{

		//protected static Dictionary<string, ISettingsProvider> _Instances;


		/// <summary>
		/// Default constructor. Just calls LoadSettings()
		/// </summary>
		public AbstractSettingsProvider()
		{
			_InternalCollection = new Dictionary<string, object>();
			LoadSettings();
		}
		
		/// <summary>
		/// Constructor that sets Source. Calls LoadSettings()
		/// </summary>
		/// <param name="source"></param>
		public AbstractSettingsProvider(string source)
		{
			
			_InternalCollection = new Dictionary<string, object>();
			Source = source;
			LoadSettings();
		}

		/// <summary>
		/// Internal storage for settings.
		/// </summary>
		protected Dictionary<string, object> _InternalCollection;
		
		/// <summary>
		/// Indexer for convenience. Get/set
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public object this[string key]
		{
			get
			{
                if ( !ContainsKey(key) )
                    return null;
				return _InternalCollection[key];
			}
			set
			{

				if (_InternalCollection.ContainsKey(key))
				{
					_InternalCollection[key] = value;
				}
				else
				{
					_InternalCollection.Add(key, value);
				}
			}
		}

        public bool ContainsKey(string key)
        {
            return _InternalCollection.ContainsKey(key);
        }

	    /// <summary>
		/// Contract for Load settings.
		/// </summary>
		protected abstract void LoadSettings();
		
		/// <summary>
		/// Contract for Save settings
		/// </summary>
		protected abstract void SaveSettings();

		private string _Source;

		/// <summary>
		/// Name of settings Data source
		/// </summary>
		public string Source
		{
			get { return _Source; }
			set { _Source = value; }
		}

        private string _instance;

        /// <summary>
        /// Name of instrance of Data Source
        /// </summary>
        public string Instance
        {
            get { return _instance; }
            set { _instance = value;
            LoadSettings();
            }
        }

		#region IDisposable Members

		/// <summary>
		/// ref. IDisposable
		/// </summary>
		public void Dispose()
		{
			SaveSettings();
			//throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region ISettingsProvider Members

		/// <summary>
		/// Clears out internal collection and reload it from source
		/// </summary>
		public void Reload()
		{
			_InternalCollection.Clear();
			LoadSettings();
		}

		/// <summary>
		/// Saves all changes settings.
		/// </summary>
		public void Save()
		{
			SaveSettings();
		}

		#endregion

		/// <summary>
		/// Converts value from string to required type.
		/// <remarks>Throws Cryptany.Core.Settings.ConvertFromStringException</remarks>
		/// </summary>
		/// <param name="value">String data representation</param>
		/// <param name="typeName">Needed typename</param>
		/// <returns></returns>
		protected object ConvertFromString(string value, string typeName)
		{
			object tmpResult = null;
			//Type destType = Type.GetType(typeName);
			try
			{
				
				//tmpResult = Assembly.GetAssembly(destType).CreateInstance(typeName);

				if (typeName == typeof(string).ToString())
				{
					tmpResult = value;
				}
				else if (typeName == typeof(System.Guid).ToString())
				{
					tmpResult = new Guid(value);
				}
				else if (typeName == typeof(System.Int32).ToString())
				{
					tmpResult = Int32.Parse(value);
				}
				else if (typeName == typeof(System.Boolean).ToString())
				{
					tmpResult = Boolean.Parse(value);
				}
				else if  (typeName == typeof(System.Double).ToString())
				{
					tmpResult = Double.Parse(value);
				}
				else if (typeName == typeof(System.Decimal).ToString())
				{
					tmpResult = Decimal.Parse(value);
				}
				else if (typeName == typeof(System.DateTime).ToString())
				{
					tmpResult = DateTime.Parse(value);
				}
				else if (typeName == typeof(System.Char).ToString())
				{
					tmpResult = Char.Parse(value);
				}
				else
				{
					throw (new ArgumentException("Unsupported data type", typeName));
				}
			}
			catch (Exception ex)
			{
				string errorDesc = "Error converting string value '" + value + "' to " + typeName.ToString();

				Trace.WriteLine(errorDesc);
				throw (new ConvertFromStringException(errorDesc, ex));

			}

			return tmpResult;

		}

	}

	
}
