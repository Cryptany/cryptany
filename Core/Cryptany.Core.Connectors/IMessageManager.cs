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
using Cryptany.Core.Management.WMI;

namespace Cryptany.Core
{

    public class StateChangedEventArgs : EventArgs
    {
        private readonly ConnectorState _state = ConnectorState.Idle;
        private readonly string _stateDescription = "";

        public ConnectorState State
        {
            get { return _state; }
        }

        public string StateDescription
        {
            get { return _stateDescription; }
        }


        public StateChangedEventArgs(ConnectorState state, string stateDescription)
        {
            _state = state;
            _stateDescription = stateDescription;
        }
    }

    public class MessageStateChangedEventArgs : EventArgs
    {
        private readonly string _id;
        private readonly string _state = "";
        private readonly string _stateDescription = "";

        public string ID
        {
            get { return _id; }
        }
        public string State
        {
            get { return _state; }
        }

        public string StateDescription
        {
            get { return _stateDescription; }
        }


        public MessageStateChangedEventArgs(string state, string stateDescription, string id)
        {
            _state = state;
            _id = id;
            _stateDescription = stateDescription;
        }
    }
    public delegate void StateChangedEventHandler(object obj, StateChangedEventArgs e);
    public delegate void MessageStateChangedEventHandler(object obj, MessageStateChangedEventArgs e);

    /// <summary>
    /// Интерфейс, определяющий события для менеджеров сообщений любого типа
    /// </summary>
    public interface IMessageManager
    {
        event EventHandler MessageReceived;
        event EventHandler MessageSent;
        event MessageStateChangedEventHandler MessageStateChanged;
        event StateChangedEventHandler StateChanged;
        event EventHandler RequireReinit;
        
    }
}
