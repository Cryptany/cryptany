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
using System.Data.SqlClient;
using Cryptany.Common.Utils;

namespace Cryptany.Core
{
	/// <summary>
	/// –еализаци€ провайдера конфигурации подт€гивающа€ всЄ из базы данных
	/// <see cref="Cryptany.Core.IConfigProvider"/>
	/// </summary>
    public class DBConfigProvider: IConfigProvider
    {
        //protected ILogger _Logger;

        public DBConfigProvider()
        {
			//_Logger = Logging.LoggerFactory.Logger;
            //_Logger.DefaultSource = "DBConfigProvider";
        }
		
		public void Dispose()
		{
		}

	}
}