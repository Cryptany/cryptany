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
	/// Thread-safe caching logger. Dumps logs to database.
	/// </summary>
	public class DBCachingLogger : ILogger
	{
		private Mutex localMutex;

		DataTable tmpTable;

		System.Timers.Timer _WriteDelayTimer;

		/// <summary>
		/// Default constructor
		/// </summary>
		public DBCachingLogger()
		{
			localMutex = new Mutex();
			tmpTable = new DataTable();
			tmpTable.Columns.Add("msgTime", typeof(DateTime));
			tmpTable.Columns.Add("msgBody", typeof(string));
			tmpTable.Columns.Add("Source", typeof(string));
			tmpTable.Columns.Add("Severity", typeof(int));

			_MaxWriteDelay = new TimeSpan(0, 1, 0);
			_MaxCacheSize = 100;

			_WriteDelayTimer = new System.Timers.Timer(_MaxWriteDelay.TotalMilliseconds);
			_WriteDelayTimer.Enabled = false;
			_WriteDelayTimer.Elapsed += new System.Timers.ElapsedEventHandler(_WriteDelayTimer_Elapsed);
		}

		void _WriteDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			if (tmpTable.Rows.Count > 0)
				Flush();
		}


		#region ILogger Members

		/// <summary>
		/// Writes data to queue.
		/// </summary>
		/// <param name="msg">Message to save to log</param>
		/// <returns>true if data was written to queue sucessfully</returns>
		public bool Write(LogMessage msg)
		{
			// lock mutex
			if (!localMutex.WaitOne(new TimeSpan(0, 0, 5), true))
				return false;

			//start timer if need
			if (!_WriteDelayTimer.Enabled)
				_WriteDelayTimer.Start();


			object[] rowData = new object[4];
			rowData[0] = msg.MessageTime;
			rowData[1] = msg.MessageText;
			rowData[2] = msg.Source;
			rowData[3] = msg.Severity;
			tmpTable.Rows.Add(rowData);

			localMutex.ReleaseMutex();

			//flush if need
			if (AutoFlush || tmpTable.Rows.Count >= MaxCacheSize)
			{
				return Flush();
			}

			return true;
		}

		private string _DefaultSource;

		/// <summary>
		/// Default Datasource
		/// </summary>
		public string DefaultSource
		{
			get { return _DefaultSource; }
			set { _DefaultSource = value; }
		}


		private bool _AutoFlush;

		/// <summary>
		/// Set to true to flush cache automatically after each write
		/// </summary>
		public bool AutoFlush
		{
			get { return _AutoFlush; }
			set { _AutoFlush = value; }
		}

		/// <summary>
		/// Write data from queue to database
		/// </summary>
		/// <returns>true if operation succeeded</returns>
		public bool Flush()
		{
			if (!localMutex.WaitOne(new TimeSpan(0, 0, 5), true))
				return false;

			using (SqlConnection con = Database.Connection)
			{ 
				SqlBulkCopy bulkCopy = new SqlBulkCopy(con);
				bulkCopy.BulkCopyTimeout = 5;
				bulkCopy.ColumnMappings.Add(0, 1);
				bulkCopy.ColumnMappings.Add(1, 2);
				bulkCopy.ColumnMappings.Add(2, 3);
				bulkCopy.ColumnMappings.Add(3, 4);

				bulkCopy.DestinationTableName = "common.DBLog";

				bulkCopy.WriteToServer(tmpTable);			
			}


			tmpTable.Rows.Clear();

			localMutex.ReleaseMutex();

			return true;

		}


		private int _MaxCacheSize;

		/// <summary>
		/// Maximum log write queue size 
		/// When queue reaches this amount, Flush method is called automatically
		/// </summary>
		public int MaxCacheSize
		{
			get { return _MaxCacheSize; }
			set
			{
				_MaxCacheSize = value;

				if (_MaxCacheSize < tmpTable.Rows.Count)
				{
					Flush();
				}
			}
		}


		private TimeSpan _MaxWriteDelay;

		/// <summary>
		/// Maximum timeout to wait until flushing data to database
		/// Flush is called automatically after this timeout elapses
		/// </summary>
		public TimeSpan MaxWriteDelay
		{
			get { return _MaxWriteDelay; }
			set
			{
				_WriteDelayTimer.Stop();
				_MaxWriteDelay = value;
				_WriteDelayTimer.Interval = (double)_MaxWriteDelay.Milliseconds;
				_WriteDelayTimer.Start();
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Dispose resources. Derived from IDisposable
		/// </summary>
		public void Dispose()
		{
			localMutex.Close();


			_WriteDelayTimer.Stop();
			_WriteDelayTimer.Close();
			_WriteDelayTimer.Dispose();
			//throw new Exception("The method or operation is not implemented.");
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
