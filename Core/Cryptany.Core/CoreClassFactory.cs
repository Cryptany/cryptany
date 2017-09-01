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
using System.Reflection;
using System.Diagnostics;
using System.Data.SqlClient;
using Cryptany.Common.Utils;

namespace Cryptany.Core
{
	/// <summary>
	/// Абстрактный класс предоставляющий статические проперти и методы для создания объектов.
	/// </summary>
	/// <remarks>Конкретные классы внутрь  не зашиты, а получаются из настроек. Проверяйте что в настройках указаны все необходимые типы.</remarks>
	public class CoreClassFactory
	{
        private static IConfigProvider _ConfigProviderInstance;

		/// <summary>
		/// Очищает все разделяемые объекты. Следует вызвать перед выходом из сервиса.
		/// </summary>
		public static void DisposeSharedObjects()
        {
			if (_ConfigProviderInstance != null)
				_ConfigProviderInstance.Dispose();
			_ConfigProviderInstance = null;
		}
    }
} 
