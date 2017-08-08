using System;
using System.Collections.Generic;
using System.Text;

namespace avantMobile.Settings
{


	/// <summary>
	/// Провайдер настроек использующий для получения/сохранения данных веб-сервис.
	/// <remarks>Не реализован.</remarks>
	/// </summary>
	public class WebServiceSettingsProvider : AbstractSettingsProvider
	{
		/// <summary>
		/// Конструктор в качестве параметра принимает URL web сервиса
		/// </summary>
		/// <param name="source"></param>
		public WebServiceSettingsProvider(string source)
			: base(source) 
		{ }

		/// <summary>
		/// Загружает настройки
		/// </summary>
		protected override void LoadSettings()
		{

			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Сохраняет настройки
		/// </summary>
		protected override void SaveSettings()
		{
			throw new Exception("The method or operation is not implemented.");
		}

	}
	
}
