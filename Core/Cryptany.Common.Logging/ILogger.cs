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

namespace Cryptany.Common.Logging
{
	/// <summary>
	/// Этот интерфейс должен имплементить любой используемый в системе логер
	/// </summary>
	public interface ILogger : IDisposable
	{
        /// <summary>
        /// Процесс в котором юзается логгер
        /// </summary>
        string DefaultServiceSource
        {
            get;
            set;
        }
		/// <summary>
		/// Испочник сообщения по умолчанию
		/// </summary>
		string DefaultSource
		{
			get;
			set;
		}

		/// <summary>
		/// Get/set автоматичеспое сохраниние записи сразу после Write
		/// </summary>
		bool AutoFlush
		{
			get;
			set;
		}

		
		/// <summary>
		/// Максимальное кол-во сообщений, которое может хранится в кеше ожидая сброса в конечное хранилище данных
		/// </summary>
		int MaxCacheSize
		{
			get;
			set;
		}

		/// <summary>
		/// Максимальное время которое сообщения могут находится в кеше ожидая сброса в конечное хранилище данных
		/// </summary>
		TimeSpan MaxWriteDelay
		{
			get;
			set;
		}

		/// <summary>
		/// Передаёт сообщение логеру. Сообщение помещается в кеш, если он есть, 
		/// или сохраняется в конечном хранилище данных. 
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		bool Write(LogMessage msg);

		/// <summary>
		/// Сбрасывает сообщения находящиеся в кеше в конечное хранилище данных.
		/// </summary>
		/// <returns></returns>
		bool Flush();
	}
}
