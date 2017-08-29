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
