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

using Cryptany.Common.Utils;

namespace Cryptany.Common.Logging
{
	/// <summary>
	/// Thread-safe non-caching logger. Dumps logs to database.
	/// </summary>
	public class DBLogger : ILogger
	{
		#region properties

		private SqlConnection _Connection;

        /// <summary>
        /// Database connection
        /// </summary>
		protected SqlConnection Connection
		{
			get
			{
				if (_Connection == null)
				{
					_Connection = Database.Connection;
				}

				if (_Connection.State != ConnectionState.Open)
					_Connection.Open();

				return _Connection;
			}
		}





		#endregion


		private Mutex localMutex;
		/// <summary>
		/// Конструктор
		/// </summary>
		public DBLogger()
		{
			localMutex = new Mutex();
			_DefaultSource = "";
		}

		#region ILogger Members

		/// <summary>
		/// Записывает данные непосредственно в базу.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public bool Write(LogMessage msg)
		{
			try
			{
				// lock mutex
				if (!localMutex.WaitOne(new TimeSpan(0, 0, 5), true))
					return false;

				string query = "insert into common.DBLog ( msgTime, msgBody, source, severity) values (@msgTime, @msgBody, @source, @severity)";



				using (SqlCommand cmd = new SqlCommand(query, Connection))
				{
					cmd.Parameters.AddWithValue("@msgTime", msg.MessageTime);
					cmd.Parameters.AddWithValue("@msgBody", msg.MessageText);
					cmd.Parameters.AddWithValue("@source", (msg.Source==null?_DefaultSource:msg.Source));
					cmd.Parameters.AddWithValue("@severity", msg.Severity);

					cmd.ExecuteNonQuery();
				}

				localMutex.ReleaseMutex();
			}
			catch (Exception ex)
			{
				System.Diagnostics.Trace.Write(ex.ToString()); 
			}

			return true;
		}

		private string _DefaultSource;

		/// <summary>
		/// Источник сообщений "по-умолчанию"
		/// </summary>
		public string DefaultSource
		{
			get { return _DefaultSource; }
			set { _DefaultSource = value; }
		}

		private int _MaxCacheSize;

		/// <summary>
		/// Унаследовано от ILogger. Не используется.
		/// </summary>
		public int MaxCacheSize
		{
			get { return 0; }
			set
			{
			}
		}




		/// <summary>
		/// Унаследовано от ILogger. Не используется.
		/// </summary>
		public bool AutoFlush
		{
			get { return true; }
			set {  }
		}

		/// <summary>
		/// Унаследовано от ILogger. Не используется.
		/// </summary>
		public bool Flush()
		{
			return true;
		}






		/// <summary>
		/// Унаследовано от ILogger. Не используется.
		/// </summary>
		public TimeSpan MaxWriteDelay
		{
			get { return new TimeSpan(1); }
		}

		#endregion

		#region IDisposable Members
		/// <summary>
		/// Унаследовано от IDisposable.
		/// </summary>
		public void Dispose()
		{
			localMutex.Close();

			Connection.Dispose();
		}

		#endregion

		#region ILogger Members

		/// <summary>
		/// Унаследовано от ILogger. Не используется.
		/// </summary>
		TimeSpan ILogger.MaxWriteDelay
		{
			get
			{
				return new TimeSpan(10);
			}
			set
			{
			}
		}

		#endregion

        #region ILogger Members

        public string DefaultServiceSource
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }

}
