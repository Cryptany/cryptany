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
using System.Diagnostics;
using System.ServiceProcess;
using Cryptany.Core.Management.WMI;
using System.Messaging;
using Cryptany.Common.Logging;

using Cryptany.Core;

namespace Cryptany.Core

{
   

    public class BalancingMessageManager : AbstractMessageManager
    {
        private struct MessageQueueSize
        {
            public readonly MessageQueue queue;
            public readonly uint size;

            public MessageQueueSize(MessageQueue mq, uint size)
            {
                queue = mq;
                this.size = size;
            }
        }

        private struct BalancingInfo
        {
            public readonly MessageQueue Queue;
            public readonly Cryptany.Core.ConfigOM.SMSC Smsc;
            public BalancingInfo(MessageQueue queue, Cryptany.Core.ConfigOM.SMSC smsc)
            {
                Queue = queue;
                Smsc = smsc;
            }
        }

        private static readonly List<BalancingInfo> _infos = new List<BalancingInfo>();
        private BalancingSettings _settings;

        private int _currentQueueIndex;

        protected static readonly Cryptany.Foundation.Diagnostics.ILogger _evLogger =
            Cryptany.Foundation.Diagnostics.LoggerFactory.CreateEventLogger(typeof(BalancingMessageManager));

        public override event EventHandler MessageReceived;

        public override event EventHandler MessageSent;

        public override event MessageStateChangedEventHandler MessageStateChanged;

        public override event StateChangedEventHandler StateChanged;


        public BalancingMessageManager(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
        }

        protected override GenericConnector State
        {
            get { throw new NotImplementedException(); }
        }

        protected override void Init(AbstractConnectorSettings settings)
        {

            _settings = settings as BalancingSettings;
            if (_settings == null) throw new ArgumentNullException("settings");

				Cryptany.Core.ConfigOM.ServiceInstance balancing = 
					Cryptany.Core.ConfigOM.ChannelConfiguration.DefaultPs.GetEntityById<Cryptany.Core.ConfigOM.ServiceInstance>(_settings.ServiceId);
                if (balancing == null)
                    throw new ApplicationException("Не найден сервис с идентификатором " + _settings.ServiceId + " для текущего коннектора");
				Cryptany.Core.ConfigOM.ServiceInstanceGroup group =
					Cryptany.Core.ConfigOM.ChannelConfiguration.DefaultPs.GetOneEntityByFieldValue<Cryptany.Core.ConfigOM.ServiceInstanceGroup>("BalancingServiceInstance", balancing);

				if ( group != null )
				{
                    
					_settings.balancingType = 
						(BalancingSettings.BalancingType)Enum.Parse(typeof(BalancingSettings.BalancingType), group.BalancingType.Name);
                    
					foreach ( Cryptany.Core.ConfigOM.ServiceInstance instance in group.Instances )
					{
                        Cryptany.Core.ConfigOM.SMSC smsc = Cryptany.Core.ConfigOM.ChannelConfiguration.DefaultPs.GetOneEntityByFieldValue<Crpytany.Core.ConfigOM.SMSC>("Instance", instance);
                        if (smsc == null) throw new ApplicationException("Не найден SMSC с id сервиса " + instance.ID);
						MessageQueue mq = ServicesConfigurationManager.GetOutgoingMessageQueue(smsc);
                        if (mq == null) throw new ApplicationException("Не найдена MSMQ-очередь с id сервиса " + instance.ID);
					    BalancingInfo bi = new BalancingInfo(mq, smsc);
						_infos.Add(bi);
					}
				}

                
                    if (StateChanged!= null)
                        StateChanged(this, new StateChangedEventArgs(ConnectorState.Connected, "OK"));
            
        }

        public override bool SendUserData(OutputMessage outputMessage, byte[] userData)
        {
            try
            {
                MessageQueue mq;
                switch(_settings.balancingType)
                {
                    case BalancingSettings.BalancingType.Balancing:
                        mq = GetSmallestQueue();
                        break;
                    case BalancingSettings.BalancingType.RoundRobin:
                        mq = GetNextQueue();
                        break;
                    default:
                        mq = GetNextQueue();
                        break;
                }
                if (mq != null)
                {
                    
                    mq.Send(outputMessage);
                }
                else
                {
                    return false;
                }
            }
            catch (ApplicationException e)
            {
                _evLogger.Log(e.ToString(), EventLogEntryType.Error);
                Logger.Write(new LogMessage(e.ToString(), LogSeverity.Error));
                if (StateChanged!= null) 
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, e.ToString())); 
                
            }
            
            if (MessageSent!= null)
                
                
                MessageSent(this, new EventArgs());
           
            return true;
        }

       

        protected override void InitPerformanceCountersExt()
        {
            //throw new System.NotImplementedException();
        }

        protected override void ClosePerformanceCountersExt()
        {
            //throw new System.NotImplementedException();
        }

        public override void Dispose()
        {
            
        }

        
        private static uint GetQueueSize(MessageQueue mq)
        {
            return MessageQueueExtensions.GetCount(mq);
        }

        private MessageQueue GetNextQueue()
        {
            if (_infos.Count == 0) return null;
            _currentQueueIndex++;
            if (_currentQueueIndex == _infos.Count)
            {
                _currentQueueIndex = 0;
            }
            return IsStarted(_infos[_currentQueueIndex].Smsc) ? _infos[_currentQueueIndex].Queue : null;
        }

        
        private static bool IsStarted(Crpytany.Core.ConfigOM.SMSC smsc)
        {
                using (ServiceController sc = new ServiceController("Cryptany.ConnectorService" + smsc.Code))
                    return (sc.Status == ServiceControllerStatus.Running) ||
                           (sc.Status == ServiceControllerStatus.StartPending);
        }

        private static MessageQueue GetSmallestQueue()
        {
            List<MessageQueueSize> counters = new List<MessageQueueSize>();
            foreach (BalancingInfo bi in _infos)
            {
                if (IsStarted(bi.Smsc))
                {
                    MessageQueueSize mqs = new MessageQueueSize(bi.Queue, GetQueueSize(bi.Queue));
                    counters.Add(mqs);
                }
            }

            if (counters.Count > 0)
            {
                counters.Sort(new Comparison<MessageQueueSize>(MessageQueueCompare));
                return counters[0].queue;
            }
            return null;
        }

        private static int MessageQueueCompare(MessageQueueSize x, MessageQueueSize y)
        {
            return x.size.CompareTo(y.size);
        }

        
        
    }


    public class BalancingSettings : AbstractConnectorSettings
    {


        public enum BalancingType
        {
            RoundRobin,
            Balancing
        }


        public BalancingType balancingType;

        public BalancingSettings(int SMSCCode)
            : base(SMSCCode)
        {
        }

        protected override void InitSettings(System.Xml.XmlDocument settings)
        {
            
            
        }
    }
   
}
