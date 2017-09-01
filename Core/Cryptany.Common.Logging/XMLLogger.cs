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
using System.Threading;
using System.IO;
using System.Xml;

namespace Cryptany.Common.Logging
{
	/// <summary>
	/// Кеширующий логер, записывающий данные в формате XML. 
	/// <remarks>При достижении определёного размера, файл в который производится запись переименовывается (к имени добавляется время).</remarks>
	/// </summary>
	public class XMLLogger : ILogger
	{
		/// <summary>
		/// Конструктор, он конструктор и есть.
		/// </summary>
		public XMLLogger()
		{
			_Messages = new List<LogMessage>();
            _DestFilePath = @"C:\Logs\Cryptany.log.xml";
            _MaxWriteDelay = new TimeSpan(0, 0, 10);
		    _MaxFileSize = 500000;
            
			_AutoFlush = false;

			_MaxCacheSize = 100;

			_FlushTimer = new System.Timers.Timer();
			_FlushTimer.Interval = _MaxWriteDelay.TotalMilliseconds;
			_FlushTimer.Elapsed += _FlushTimer_Elapsed;

			_FlushTimer.Start();
			
		}

		void _FlushTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
            _FlushTimer.Stop();
			if (_Messages.Count > 0)
				Flush();
            _FlushTimer.Start();
		}

		/// <summary>
		/// Конструктор, позволяющий указать путь к файлу, в который будет производиться запись данных.
		/// </summary>
		public XMLLogger(string filePath)
		{
			_Messages = new List<LogMessage>();
			_DestFilePath = filePath;
		    
			_AutoFlush = false;

			//_Mutex = new Mutex();

			_MaxCacheSize = 100;
            _DestFilePath = @"C:\Logs\Cryptany.log.xml";
            _MaxWriteDelay = new TimeSpan(0, 0, 10);
            _MaxFileSize = 500000;

			_FlushTimer = new System.Timers.Timer();
			_FlushTimer.Enabled = false;
			_FlushTimer.Interval = _MaxWriteDelay.TotalMilliseconds;
		    _FlushTimer.Elapsed += _FlushTimer_Elapsed;
		}

		private readonly List<LogMessage> _Messages;
		private string _DestFilePath;
		//private readonly Mutex _Mutex;
		//private DateTime _LastFlushTime;
		private readonly System.Timers.Timer _FlushTimer;
		//private readonly int _MutexWaitTime;
	    private DirectoryInfo _destPath;

		private long _MaxFileSize;

		/// <summary>
		/// Максимальный размер файла (в байтах), используемого для записи даных. Рекомендованеное значение 500000.
		/// </summary>
		public long MaxFileSize
		{
			get { return _MaxFileSize; }
			set { _MaxFileSize = value; }
		}

		#region ILogger Members
		private string _DefaultSource;

		/// <summary>
		/// Источник сообщений по-умолчанию
		/// </summary>
		public string DefaultSource
		{
			get { return _DefaultSource; }
			set
			{
			    _DefaultSource = value;
			     
                
			}
		}

		private bool _AutoFlush;

		/// <summary>
		/// Унаследовано от ILogger. Не рекомендуется выставлять в true.
		/// </summary>
		public bool AutoFlush
		{
			get { return _AutoFlush; }
			set { _AutoFlush = value; }
		}


		private int _MaxCacheSize;
		/// <summary>
		/// Максимальное количество сообщений в кеше. 
		/// При достижении порогового значения происходит автоматический вызов Flush
		/// </summary>
		public int MaxCacheSize
		{
			get { return _MaxCacheSize; }
			set { _MaxCacheSize = value; }
		}

		private TimeSpan _MaxWriteDelay;

		/// <summary>
		/// Максимальное время хранения сообщений в кеше. 
		/// При достижении порогового значения происходит автоматический вызов Flush
		/// </summary>
		public TimeSpan MaxWriteDelay
		{
			get { return _MaxWriteDelay; }
			set { _MaxWriteDelay = value; }
		}
		
		/// <summary>
		/// Помещает сообщение в очередь на запись в файл.
		/// </summary>
		/// <param name="msg"></param>
		/// <returns></returns>
		public bool Write(LogMessage msg)
		{
             try
             {
				lock ( _Messages )
					_Messages.Add(msg);
                    return true;
                }
                catch (Exception)
                {
                }

			    return false;
		}

		private void BackupBigFile()
		{
			if (File.Exists(_DestFilePath))
			{
			    FileInfo fi = new FileInfo(_DestFilePath);
			    long curFileSize = fi.Length;
				
				if (curFileSize > MaxFileSize)
				{ 
					string newFileName = Path.GetDirectoryName(_DestFilePath) + @"\" + DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss ") + Path.GetFileName(_DestFilePath);
					
					File.Move(_DestFilePath, newFileName);
				}
				
			}
		}

		/// <summary>
		/// Сохраняет данные в файл.
		/// </summary>
		/// <returns></returns>
		public bool Flush()
		{			
			try
			{
			    //if (_Mutex.WaitOne(_MutexWaitTime, false))
				{
                    try
                    {
                        BackupBigFile();
                        XmlDocument doc = new XmlDocument();
                        XmlElement root;
                        if (File.Exists(_DestFilePath))
                        {
                            doc.Load(_DestFilePath);
                            root = doc.DocumentElement;
                        }
                        else
                        {
                            root = doc.CreateElement("log");
                            doc.AppendChild(root);
                        }
                        lock (_Messages)
                        {
                            List<LogMessage> list = new List<LogMessage>(_Messages);
                            

                            foreach (LogMessage msg in list)
                            {
                                XmlElement entry = doc.CreateElement("entry");
                                XmlAttribute timeAttr = doc.CreateAttribute("MsgTime");
                                timeAttr.Value = msg.MessageTime.ToShortDateString() + " " +
                                                 msg.MessageTime.ToLongTimeString();
                                entry.Attributes.Append(timeAttr);
                                XmlAttribute severityAttr = doc.CreateAttribute("Severity");
                                severityAttr.Value = msg.Severity.ToString();
                                entry.Attributes.Append(severityAttr);
                                XmlAttribute textAttr = doc.CreateAttribute("Text");
                                textAttr.Value = msg.MessageText;
                                entry.Attributes.Append(textAttr);
                                root.AppendChild(entry);
                            }
                            doc.Save(_DestFilePath);
                            _Messages.Clear();
                        }
                        return true;
                    }
                    catch (Exception)
                    {
                    }
				}
			    return false;
			}
			catch (Exception)
			{
				return false;
			}			
		}
		#endregion

		#region IDisposable Members

		/// <summary>
		/// Унаследовано от IDisposable. Флашит данные, освобождает выделенные ресурсы. 
		/// </summary>
		public void Dispose()
		{
			if (_Messages.Count > 0)
				Flush();

			_FlushTimer.Close();
		}

		#endregion

        #region ILogger Members

	    private string _defaultServiceSource;
        public string DefaultServiceSource
        {
            get
            {
                return _defaultServiceSource;
            }
            set
            {
                // чтобы писалось в разные файлы
                _defaultServiceSource = value;
                _destPath = Directory.CreateDirectory(Path.Combine(Path.GetDirectoryName(_DestFilePath), _defaultServiceSource));
                FileInfo fi = new FileInfo(_DestFilePath);
                _DestFilePath = Path.Combine(_destPath.FullName, fi.Name);
            }
        }

        #endregion
    }
}
