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
using Cryptany.Common.Logging;
using Cryptany.Core;

namespace Cryptany.Core.Services
{
    /// <summary>
    /// Обеспечивает вызов метода ProcessMessage одного из загруженных сервисов по его ID. В качестве параметров передаётся сообщение для обработки.
    /// </summary>
    public interface IRouter : IDisposable, ILoggable
    {
        /// <summary>
        /// Calls a specified service
        /// </summary>
        /// <param name="id">ServiceId</param>
        /// <param name="msg">Message for a service to process</param>
        /// <param name="args">parameters to service</param>
        /// <returns>true if processing was successful, false overwise</returns>
        bool CallService(Guid id, Message msg, object[] args);
        IService[] GetLoadedServices();
        IService GetServiceByType(Type t);
        T GetService<T>();
        int RouterIndex { get;}
    }

    /// <summary>
    /// Определяет интерфейс, который должны имплементить все сервисы обработки сообщений.
    /// </summary>
    public interface IService : IDisposable, ILoggable
    {
        /// <summary>
        /// ID сервиса в базе
        /// </summary>
        Guid ID
        {
            get;
        }

        /// <summary>
        /// Название сервиса
        /// </summary>
        String Name
        {
            get;
        }

        /// <summary>
        /// Роутер, которым он был создан
        /// </summary>
        IRouter OwningRouter
        {
            get;
        }

        /// <summary>
        /// Метод обрабатывающий входящее сообщение
        /// </summary>
        /// <param name="msg">сообщение для обработки</param>
        /// <param name="regexRow"></param>
        bool ProcessMessage(Message msg, Cryptany.Core.ConfigOM.AnswerMap answerMap);

        /// <summary>
        /// Обработать сообщение, но не посылать никаких ответных СМС; метод может вызываться только другими сервисами по горизонтали
        /// </summary>
        /// <param name="msg">Сообщение абонента, которое необходимо обработать</param>
        /// <returns>True, если обработка прошла успешно, false в противном случае</returns>
        bool ProcessMessageSoundlessly(Message msg);
    }

    /// <summary>
    /// Интерфейс менеджера сессий.
    /// </summary>
    public interface ISessionManager : IDisposable
    {
        /// <summary>
        /// обеспечивает доступ к сессии по ключу (как правило номеру абонента)
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        ISession this[string idx] { get; }

        /// <summary>
        /// Сохраняет сделанные изменения.
        /// </summary>
        void Flush();
    }

    /// <summary>
    /// Интерфейс сессии абонента. Обеспечивает хранение данных и доступ к ним 
    /// между обращениями абонента к системе.
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Обеспечивает доступ к произвольным данным хранящимся в сессии
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        string this[string idx] { get; set; }
        /// <summary>
        /// Менеджер сессии породивший текущий объект
        /// </summary>
        ISessionManager Manager { get; }
    }
}
