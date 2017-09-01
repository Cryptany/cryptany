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
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Threading;

namespace Cryptany.Common.Logging
{
	
	/// <summary>
	/// Сообщение для помещения в лог.
	/// <see cref="Cryptany.Common.Logging.ILogger"/>
	/// </summary>
	public class LogMessage
	{
		#region Constructors
		/// <summary>
		/// Конструктор по умолчанию. Нужен для коректной сериализации
		/// </summary>
		public LogMessage()
		{
			_Source = null;
			_MessageTime = DateTime.Now;
			_Severity = LogSeverity.Info;
			_MessageText = null;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="msgText">Текст сообщения</param>
		/// <param name="severity">Значимость события</param>
		public LogMessage(string msgText, LogSeverity severity)
		{
			_Source = null;
			_MessageTime = DateTime.Now;
			_Severity = severity;
			_MessageText = msgText;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="msgText">Текст сообщения</param>
		/// <param name="severity">Значимость события</param>
		/// <param name="source">Источник сообщения</param>
		public LogMessage(string msgText, LogSeverity severity, string source)
		{
			_Source = source;
			_MessageTime = DateTime.Now;
			_Severity = severity;
			_MessageText = msgText;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="msgText">Текст сообщения</param>
		/// <param name="severity">Значимость события</param>
		/// <param name="source">Источник сообщения</param>
		/// <param name="msgTime">Время события</param>
		public LogMessage(string msgText, LogSeverity severity, string source, DateTime msgTime)
		{
			_Source = source;
			_MessageTime = msgTime;
			_Severity = severity;
			_MessageText = msgText;
		}

		#endregion


		#region ILogMessage Members

		private string _Source;

		/// <summary>
		/// Message source
		/// </summary>
		public string Source
		{
			get { return _Source; }
			set { _Source = value; }
		}


		private DateTime _MessageTime;

		/// <summary>
		/// Message time
		/// </summary>
		public DateTime MessageTime
		{
			get { return _MessageTime; }
			set { _MessageTime = value; }
		}


		private LogSeverity _Severity;

		/// <summary>
		/// Message severity
		/// </summary>
		public LogSeverity Severity
		{
			get { return _Severity; }
			set { _Severity = value; }
		}

		private string _MessageText;

		/// <summary>
		/// Message text
		/// </summary>
		public string MessageText
		{
			get { return _MessageText; }
			set { _MessageText = value; }
		}

		#endregion
	}
}
