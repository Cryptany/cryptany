using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace avantMobile.Settings
{
	/// <summary>
	/// Абстрактный класс обеспечивающий каркас для функционирования провайдера настроек.
	/// </summary>
	public abstract class AbstractSettingsProvider : ISettingsProvider
	{

		//protected static Dictionary<string, ISettingsProvider> _Instances;


		/// <summary>
		/// Конструктор по умолчанию. Вызывает LoadSettings()
		/// </summary>
		public AbstractSettingsProvider()
		{
			_InternalCollection = new Dictionary<string, object>();
			LoadSettings();
		}
		
		/// <summary>
		/// Конструктор устанавливающий значение Source. Вызывает LoadSettings()
		/// </summary>
		/// <param name="source"></param>
		public AbstractSettingsProvider(string source)
		{
			
			_InternalCollection = new Dictionary<string, object>();
			Source = source;
			LoadSettings();
		}

		/// <summary>
		/// Словарь содержащий пары ключ/значение.
		/// </summary>
		protected Dictionary<string, object> _InternalCollection;
		
		/// <summary>
		/// Обеспечивает доступ к значению по указанному ключу. Get/set
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
		/// Загрузка настроек. Необходимо реализовать в подклассах.
		/// </summary>
		protected abstract void LoadSettings();
		
		/// <summary>
		/// Сохранение настроек. Необходимо реализовать в подклассах.
		/// </summary>
		protected abstract void SaveSettings();

		private string _Source;

		/// <summary>
		/// Строка "Источник данных"
		/// </summary>
		public string Source
		{
			get { return _Source; }
			set { _Source = value; }
		}

        private string _instance;

        /// <summary>
        /// Строка "Источник данных"
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
		/// см. IDisposable
		/// </summary>
		public void Dispose()
		{
			SaveSettings();
			//throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region ISettingsProvider Members

		/// <summary>
		/// Очищает значения внутренней коллекции данных и заново загружает информацию.
		/// </summary>
		public void Reload()
		{
			_InternalCollection.Clear();
			LoadSettings();
		}

		/// <summary>
		/// Сохраняет внесённые изменения.
		/// </summary>
		public void Save()
		{
			SaveSettings();
		}

		#endregion

		/// <summary>
		/// Конвертит значение из строки в соответствующий тип.
		/// <remarks>Может генерить исключние avantMobile.Settings.ConvertFromStringException в влучае неудачи.</remarks>
		/// </summary>
		/// <param name="value">строковое представление данных</param>
		/// <param name="typeName">тип, который будет получен на выходе</param>
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
