using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptany.Common.Settings
{
	/// <summary>
	/// Interface for settings provider.
	/// </summary>
	public interface ISettingsProvider : IDisposable
	{
		/// <summary>
		/// Indexed access method. Get/set
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		object this[string key]
		{
			get;
			set;
		}
       
		/// <summary>
		/// Reloads data from data source. All unsaved data will be lost!
		/// </summary>
		void Reload();

		/// <summary>
		/// Saves modified data
		/// </summary>
		void Save();

	}
	
}
