using System;
using System.Collections.Generic;
using System.Text;

namespace avantMobile.Settings
{
	/// <summary>
	/// Интерфейс провайдера настроек.
	/// </summary>
	public interface ISettingsProvider : IDisposable
	{
		/// <summary>
		/// Обеспечивает доступ к значению по указанному ключу. Get/set
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[string key]
		{
			get;
			set;
		}
       
		/// <summary>
		/// Заново загружает значения из источника данных. Вся несохранённая информация будет потеряна
		/// </summary>
		void Reload();
		/// <summary>
		/// Сохраняет сделанные изменения.
		/// </summary>
		void Save();

	}
	
}
