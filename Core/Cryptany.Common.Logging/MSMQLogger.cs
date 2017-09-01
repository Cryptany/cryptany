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
using System.Messaging;
using System.Diagnostics;
using System.Configuration;

namespace Cryptany.Common.Logging
{
	/// <summary>
	/// Non-caching logger to dump log messages to Message Queue
	/// </summary>
	public class MSMQLogger : ILogger
	{
		#region ILogger Members

		private string _DefaultSource;

		/// <summary>
		/// Default log message source
		/// </summary>
		public string DefaultSource
		{
			get { return _DefaultSource; }
			set { _DefaultSource = value; }
		}

		/// <summary>
		/// Inherited, always true
		/// </summary>
		public bool AutoFlush
		{
			get
			{
				return true;
			}
			set
			{
			}
		}

		/// <summary>
		/// Inherited from ILogger. Not used.
		/// </summary>
		public int MaxCacheSize
		{
			get
			{
				return 1;
			}
			set
			{ }
		}

		/// <summary>
		/// Inherited from ILogger. Not used.
		/// </summary>
		public TimeSpan MaxWriteDelay
		{
			get
			{
				return new TimeSpan(1);
			}
			set
			{ }
		}

		/// <summary>
		/// Writes message to Message Queue
		/// </summary>
		/// <param name="msg">Message to store</param>
		/// <returns>true if written successfully</returns>
		public bool Write(LogMessage msg)
		{
			try
			{
				//OutputQueue.Send(msg as LogMessage);
				return true;
			}
			catch (Exception ex)
			{
				Trace.WriteLine("Error in MSMQLogger: {0}", ex.ToString());
				return false;
			}
		}

		/// <summary>
		/// Inherited from ILogger. Not used.
		/// </summary>
		/// <returns></returns>
		public bool Flush()
		{
			return true;
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Диспозит объект, закрывает очередь сообщений.
		/// </summary>
		public void Dispose()
		{
//            if (_OutputQueue != null)
//                _OutputQueue.Dispose();
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
