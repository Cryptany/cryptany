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
using Cryptany.Core.ConfigOM;
using Cryptany.Common.Logging;
using Cryptany.Core.Connectors.Management;
using Cryptany.Core.Services;

namespace Cryptany.Core
{
    /// <summary>
    /// Делегат для вызова сервисами глобального сервиса ошибок
    /// </summary>
    /// <param name="msg">сообщение</param>
    public delegate void ErrorServiceRequest(Message msg);
	
    /// <summary>
	/// Абстрактный сервис.  Реализует базовую функциональность по обработке входящего сообщения. Непосредственно обработка собщения реализуется в подклассах.
	/// </summary>
	public abstract class AbstractService:  IService
	{
        /// <summary>
        /// Indicates whether the instance have been initialized
        /// </summary>
        protected bool _initialized;

		protected Service _service;

        private static readonly ConnectorManager _connectorManager = new ConnectorManager();
	    protected readonly List<OutputMessage> _messages = new List<OutputMessage>();

		/// <summary>
		/// The instance of the Router that owns the current instance the service
		/// </summary>
		private readonly IRouter _router;

	    public ErrorServiceRequest errRequest;
        
		/// <summary>
		/// Конструктор по умолчанию. 
		/// </summary>
		/// <param name="router">Обязательный параметр. Ссылка на роутер, который создает данный экземпляр сервиса.
		/// Если параметр не указан, то возникнет NullParameterException</param>
		/// <remarks>Может выбросить ClassCreationException</remarks>
		protected AbstractService(IRouter router)
		{
			if ( router != null )
				_router = router;
			else
				throw new ArgumentNullException("router","AbstractService constructor: The Router parameter is not optional and must be given a non-null value.");
		}

	    protected AbstractService(IRouter router, string serviceName): this(router)
        {
            InitializeBase(serviceName);
        }

		/// <summary>
		/// Gets the instance of the Router that owns the current instance the service
		/// </summary>
		public IRouter OwningRouter
		{
			get
			{
				return _router;
			}
		}

        protected virtual void InitializeBase(string serviceName)
        {
            try
            {
                _service = ChannelConfiguration.DefaultPs.GetOneEntityByFieldValue<Service>("Name", serviceName);
                
                if ( _service == null )
                {
                    throw (new ApplicationException("Can't find config datarow for class " + this.GetType()));
                }
                if ( !_service.Enabled )
                {
                    throw (new ApplicationException("Service " + this.GetType() + " was successfully created, but it's temporary disabled. Services.Enabled==false"));
                }
                
                if ( Logger == null )
                {
                    throw (new ApplicationException("Can't create logger."));
                }
                
            }
            catch ( Exception ex )
            {
                if ( Logger != null )
                {
                    Logger.Write(new LogMessage(this.GetType().ToString() + " instance wasn.t created. Exception in abstract service constructor.\r\n" + ex.ToString(), LogSeverity.Error));
                }
                throw (new ApplicationException(this.GetType().ToString() + " instance wasn.t created. Exception in abstract service constructor.\r\n", ex));
            }
            _initialized = true;
        }

        public virtual void Initialize(string serviceName)
        {
            if (_initialized)
                throw new ApplicationException("This instance have been already initialized");
            InitializeBase(serviceName);
        }

		/// <summary>
		/// Создает объект логгирования InboxEntry и помещает его в очередь MSMQ логгера 
		/// </summary>
		#region IService Members

		/// <summary>
		/// ID сервиса в базе
		/// </summary>
		public virtual Guid ID
		{
			get { return (Guid)_service.ID; }
		}

		/// <summary>
		/// Имя сервиса
		/// </summary>
        public virtual string Name
        {
            get
            {
                return _service.Name;
            }
        }

		/// <summary>
		/// Обработка сообщения. Вызывает ProcessMessageInner
		/// </summary>
		/// <param name="msg"></param>
		/// <param name="answerMap"></param>
		public bool ProcessMessage(Message msg, AnswerMap answerMap)
		{
			_messages.Clear();
            //вызывает TVADService.ProcessMessageInner, ибо больше нечего
			if (ProcessMessageInner(msg, answerMap))
			{
			    bool res = true;
                foreach (OutputMessage omsg in _messages)
			    {
                    res &= _connectorManager.Send(omsg, omsg.SmscId,Cryptany.Core.Interaction.MessagePriority.High);
                }
			    return res;
			}
		    return false;
		}

		protected abstract bool ProcessMessageInner(Message msg, AnswerMap answerMap);

		/// <summary>
		/// Обработать сообщение, но не посылать никаких ответных СМС; метод может вызываться только другими сервисами по горизонтали
		/// </summary>
		/// <param name="msg">Сообщение абонента, которое необходимо обработать</param>
		/// <returns>True, если обработка прошла успешно, false в противном случае</returns>
		public virtual bool ProcessMessageSoundlessly(Message msg)
		{
			if ( Logger != null )
			{
				Logger.Write(new LogMessage("ProcessMessageSoundlessly is not implemented", LogSeverity.Error));
			}
			return false;
		}

		#endregion

        #region IDisposable Members
		
        /// <summary>
		/// от IDisposable
		/// </summary>
		public virtual void Dispose()
		{
            Logger.Dispose();
		}

		#endregion

		#region ILoggable Members

        public ILogger Logger
        {
            get { return _router.Logger; }
        }

        #endregion
    }
}
