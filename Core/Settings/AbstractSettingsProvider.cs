using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Diagnostics;

namespace avantMobile.Settings
{
	/// <summary>
	/// ����������� ����� �������������� ������ ��� ���������������� ���������� ��������.
	/// </summary>
	public abstract class AbstractSettingsProvider : ISettingsProvider
	{

		//protected static Dictionary<string, ISettingsProvider> _Instances;


		/// <summary>
		/// ����������� �� ���������. �������� LoadSettings()
		/// </summary>
		public AbstractSettingsProvider()
		{
			_InternalCollection = new Dictionary<string, object>();
			LoadSettings();
		}
		
		/// <summary>
		/// ����������� ��������������� �������� Source. �������� LoadSettings()
		/// </summary>
		/// <param name="source"></param>
		public AbstractSettingsProvider(string source)
		{
			
			_InternalCollection = new Dictionary<string, object>();
			Source = source;
			LoadSettings();
		}

		/// <summary>
		/// ������� ���������� ���� ����/��������.
		/// </summary>
		protected Dictionary<string, object> _InternalCollection;
		
		/// <summary>
		/// ������������ ������ � �������� �� ���������� �����. Get/set
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
		/// �������� ��������. ���������� ����������� � ����������.
		/// </summary>
		protected abstract void LoadSettings();
		
		/// <summary>
		/// ���������� ��������. ���������� ����������� � ����������.
		/// </summary>
		protected abstract void SaveSettings();

		private string _Source;

		/// <summary>
		/// ������ "�������� ������"
		/// </summary>
		public string Source
		{
			get { return _Source; }
			set { _Source = value; }
		}

        private string _instance;

        /// <summary>
        /// ������ "�������� ������"
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
		/// ��. IDisposable
		/// </summary>
		public void Dispose()
		{
			SaveSettings();
			//throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region ISettingsProvider Members

		/// <summary>
		/// ������� �������� ���������� ��������� ������ � ������ ��������� ����������.
		/// </summary>
		public void Reload()
		{
			_InternalCollection.Clear();
			LoadSettings();
		}

		/// <summary>
		/// ��������� �������� ���������.
		/// </summary>
		public void Save()
		{
			SaveSettings();
		}

		#endregion

		/// <summary>
		/// ��������� �������� �� ������ � ��������������� ���.
		/// <remarks>����� �������� ��������� avantMobile.Settings.ConvertFromStringException � ������ �������.</remarks>
		/// </summary>
		/// <param name="value">��������� ������������� ������</param>
		/// <param name="typeName">���, ������� ����� ������� �� ������</param>
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
