using System;
using System.Collections.Generic;
using System.Text;

namespace avantMobile.Settings
{


	/// <summary>
	/// ��������� �������� ������������ ��� ���������/���������� ������ ���-������.
	/// <remarks>�� ����������.</remarks>
	/// </summary>
	public class WebServiceSettingsProvider : AbstractSettingsProvider
	{
		/// <summary>
		/// ����������� � �������� ��������� ��������� URL web �������
		/// </summary>
		/// <param name="source"></param>
		public WebServiceSettingsProvider(string source)
			: base(source) 
		{ }

		/// <summary>
		/// ��������� ���������
		/// </summary>
		protected override void LoadSettings()
		{

			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// ��������� ���������
		/// </summary>
		protected override void SaveSettings()
		{
			throw new Exception("The method or operation is not implemented.");
		}

	}
	
}
