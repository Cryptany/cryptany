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
using System.Configuration;
using System.Diagnostics;
using System.Messaging;
using Cryptany.Core.MsmqLog;
using Cryptany.Common.Settings;
using System.Data.SqlClient;
using Cryptany.Common.Utils;

namespace Cryptany.Core.Management
{
	public class ServicesConfigurationManager
	{

		private static Dictionary<Guid, MessageQueue> _OutQueues;
		public static MessageQueue GetOutgoingMessageQueue(Cryptany.Core.ConfigOM.SMSC smsc)
		{
            MessageQueue result = null;
            if (_OutQueues == null)
            {
                _OutQueues = new Dictionary<Guid, MessageQueue>();
            }
		    else
			{
			    if (_OutQueues.ContainsKey(smsc.DatabaseId))
                    result = _OutQueues[smsc.DatabaseId];
            }
			if ( result == null )
			{
				string path;

				if ( smsc.Settings!=null && smsc.Settings.ProtocolType.Equals("HTTP", StringComparison.InvariantCultureIgnoreCase) )
				{
					path = (string)SettingsProviderFactory.DefaultSettingsProvider["OutputSMSQueuePrefix_Ext"] + smsc.DatabaseId;
				}
				else
				{

					path = (string)SettingsProviderFactory.DefaultSettingsProvider["OutputSMSQueuePrefix"] + smsc.DatabaseId;
				}
                if (ConfigurationManager.AppSettings["OutputSMSQueuePath"] != null)
                {
                    path = ConfigurationManager.AppSettings["OutputSMSQueuePath"];
                }
                result = new MessageQueue(path, false);
				if ( !_OutQueues.ContainsKey(smsc.DatabaseId) )
				{
					_OutQueues.Add(smsc.DatabaseId, result);
				}
				result.Formatter = new BinaryMessageFormatter();
				DefaultPropertiesToSend dps = new DefaultPropertiesToSend();
                result.MessageReadPropertyFilter.ArrivedTime = true;
                dps.Recoverable = true;
				result.DefaultPropertiesToSend = dps;
			}
			return result;
		}


        public static MessageQueue SubscriptionQueue
        {
            get
            {
                string _subscriptionQueuePath = string.Empty;
                if (SettingsProviderFactory.DefaultSettingsProvider!=null)
                        _subscriptionQueuePath =SettingsProviderFactory.DefaultSettingsProvider["SubscriptionQueuePath"] as string;
                    if (ConfigurationManager.AppSettings["SubscriptionQueuePath"] != null)
                    {
                        _subscriptionQueuePath = ConfigurationManager.AppSettings["SubscriptionQueuePath"];
                    }

                MessageQueue _subscriptionQueue = new MessageQueue(_subscriptionQueuePath, false);
                _subscriptionQueue.DefaultPropertiesToSend.Recoverable = true;
                _subscriptionQueue.Formatter = new BinaryMessageFormatter();
                
                return _subscriptionQueue;
            }
        }

        public static MessageQueue StatisticsQueue
        {
            get
            {
                string _statisticsQueuePath = string.Empty;
                if (SettingsProviderFactory.DefaultSettingsProvider!=null)
                       _statisticsQueuePath = SettingsProviderFactory.DefaultSettingsProvider["StatisticsQueuePath"] as string;
                if (ConfigurationManager.AppSettings["StatisticsQueuePath"] != null)
                {
                    _statisticsQueuePath = ConfigurationManager.AppSettings["StatisticsQueuePath"];
                }

                MessageQueue _statisticsQueue = new MessageQueue(_statisticsQueuePath, false);
                _statisticsQueue.DefaultPropertiesToSend.Recoverable = true;
                _statisticsQueue.Formatter = new BinaryMessageFormatter();

                return _statisticsQueue;
            }
        }


        public static string TransportManagerURL
        {
            get
            {
                string _statisticsQueuePath = string.Empty;
                if (SettingsProviderFactory.DefaultSettingsProvider!=null)
                    _statisticsQueuePath = SettingsProviderFactory.DefaultSettingsProvider["TransportManagerURL"] as string;
                if (ConfigurationManager.AppSettings["TransportManagerURL"] != null)
                {
                    _statisticsQueuePath = ConfigurationManager.AppSettings["TransportManagerURL"];
                }



                return _statisticsQueuePath;
            }
        }

	    

		public static MessageQueue GetInputSMSQueue(int ServiceCode)
		{
            string InputQueuePath = string.Empty;
            if (SettingsProviderFactory.DefaultSettingsProvider!=null)
                InputQueuePath = (string)SettingsProviderFactory.DefaultSettingsProvider["InputSMSQueuePath"] + ServiceCode;
                if (ConfigurationManager.AppSettings["InputSMSQueuePath"] != null)
                {
                    InputQueuePath = ConfigurationManager.AppSettings["InputSMSQueuePath"] + ServiceCode;
                }
				if ( string.IsNullOrEmpty(InputQueuePath))
                    throw new ApplicationException("Path to InputSMSQueuePath missing");

                MessageQueue _InputSMSQueue = new MessageQueue(InputQueuePath, false, true, QueueAccessMode.SendAndReceive);
                _InputSMSQueue.Formatter = new BinaryMessageFormatter();
				DefaultPropertiesToSend dps = new DefaultPropertiesToSend();
                dps.Recoverable = true;
                _InputSMSQueue.DefaultPropertiesToSend = dps;
                return _InputSMSQueue;
		} 


		public static MessageQueue MSMQLoggerInputQueue
		{
            get
            {
                string MSMQLoggerInputQueuePath = string.Empty;
                if (SettingsProviderFactory.DefaultSettingsProvider!=null)
                MSMQLoggerInputQueuePath =
                    SettingsProviderFactory.DefaultSettingsProvider["MSMQLoggerQueuePath"] as string;
                if (ConfigurationManager.AppSettings["MSMQLoggerQueuePath"] != null)
                {
                    MSMQLoggerInputQueuePath = ConfigurationManager.AppSettings["MSMQLoggerQueuePath"];
                }
                if (string.IsNullOrEmpty(MSMQLoggerInputQueuePath))
                    throw new ApplicationException("Path to MSMQLoggerInputQueuePath missing");
                MessageQueue _MSMQLoggerInputQueue = new MessageQueue(MSMQLoggerInputQueuePath, false, true,
                                                                      QueueAccessMode.SendAndReceive);
                _MSMQLoggerInputQueue.Formatter = new BinaryMessageFormatter();
                _MSMQLoggerInputQueue.DefaultPropertiesToSend.Recoverable = true;
                return _MSMQLoggerInputQueue;
            }
		}

		public static void AddToInbox(Cryptany.Core.Message msg)
		{

		    MSMQLogEntry me = new MSMQLogEntry();
            me.CommandText = "kernel.AddIncomingMessageToDB";
            me.Parameters.Add("@inboxId", msg.InboxId);
            me.Parameters.Add("@msgTime", msg.MessageTime);
		    me.Parameters.Add("@abonentId", (msg.Abonent != null &&
		                                     msg.Abonent.DatabaseId != Guid.Empty)
		                                        ? msg.Abonent.DatabaseId
		                                        : Guid.Empty);
            me.Parameters.Add("@msgText", msg.Text);
            me.Parameters.Add("@smscId", msg.SMSC.DatabaseId);
            me.Parameters.Add("@serviceNumberId", msg.SN.DatabaseId);
            me.Parameters.Add("@transactionId", msg.TransactionID);
            me.Parameters.Add("@messageType", (int)msg.Type);
                using (MessageQueue _MSMQLoggerInputQueue = MSMQLoggerInputQueue)
                {
                    _MSMQLoggerInputQueue.Send(me);
                }
			
		}

        public static void AddToDownloadOutbox(Guid outboxid, Guid downloadid)
        {
            MSMQLogEntry me = new MSMQLogEntry();
            me.CommandText = "kernel.AddToDownloadsOutbox";
            me.Parameters.Add("@outboxid", outboxid);
            me.Parameters.Add("@downloadid", downloadid);
            using (MessageQueue _MSMQLoggerInputQueue = MSMQLoggerInputQueue)
            {
                _MSMQLoggerInputQueue.Send(me);
            }

        }

		/// <summary>
		/// Return new opened database connection
		/// Do not forget to close it (better use using clause)
		/// </summary>
		/// <returns></returns>
		public static SqlConnection Connection
		{
			get
			{
				return Database.Connection;
			}
		}

	}
}
