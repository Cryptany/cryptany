using System;
using System.Collections.Generic;
using System.Text;

namespace Cryptany.Common.Settings
{
	/// <summary>
	/// Settings provider that uses Settings Web Service for storing/retrieving settings data
	/// <remarks>Not implemented yet.</remarks>
	/// </summary>
	public class WebServiceSettingsProvider : AbstractSettingsProvider
	{
		/// <summary>
		/// Constructor takes Settings web service URL as a parameter
		/// </summary>
		/// <param name="source">URL of Settings web service</param>
		public WebServiceSettingsProvider(string source)
			: base(source) 
		{ }

		/// <summary>
		/// Load settings
		/// </summary>
		protected override void LoadSettings()
		{

			throw new Exception("The method or operation is not implemented.");
		}

		/// <summary>
		/// Saves settings
		/// </summary>
		protected override void SaveSettings()
		{
			throw new Exception("The method or operation is not implemented.");
		}

	}
	
}
