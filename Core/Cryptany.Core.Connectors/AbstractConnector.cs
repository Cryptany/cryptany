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
using Cryptany.Common.Logging;

namespace Cryptany.Core
{
    /// <summary>
	/// Абстрактный класс предоставляющий каркас для приёма сообщений.
	/// Стоит переписать с учётом того, что многое уже реализовано, 
	/// например создание очередей сообщений и т.п.
    /// </summary>
    public abstract class AbstractConnector: IDisposable
    {
        protected Guid _connectorId; // ID коннектора в базе данных
        protected Guid _serviceId;
        protected AbstractMessageManager _manager;
        
        public Guid ConnectorId 
        {
            get { return _connectorId; }
            set { _connectorId = value; }
        }

        protected ILogger Logger
        {
            get { return _manager.Logger; }
        }
		
		protected AbstractConnector(AbstractMessageManager manager)
        {
            _manager = manager;
            _connectorId = Guid.Empty;
        }

		protected AbstractConnector(Guid cId)
        {
            _connectorId = cId;
        }

        public void Dispose()
        {}

        public virtual bool Initialize()
        {
            return true;
        }
        public virtual bool Start()
        {
            return true;
        }
        public virtual bool Stop()
        {
            return true;
        }
        public virtual bool Pause()
        {
            return true;
        }
    }
}
