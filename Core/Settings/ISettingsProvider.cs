using System;
using System.Collections.Generic;
using System.Text;

namespace avantMobile.Settings
{
	/// <summary>
	/// ��������� ���������� ��������.
	/// </summary>
	public interface ISettingsProvider : IDisposable
	{
		/// <summary>
		/// ������������ ������ � �������� �� ���������� �����. Get/set
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[string key]
		{
			get;
			set;
		}
       
		/// <summary>
		/// ������ ��������� �������� �� ��������� ������. ��� ������������ ���������� ����� ��������
		/// </summary>
		void Reload();
		/// <summary>
		/// ��������� ��������� ���������.
		/// </summary>
		void Save();

	}
	
}
