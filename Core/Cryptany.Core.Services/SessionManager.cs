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

namespace Cryptany.Core.Services
{
    public class SessionManager : ISessionManager
    {
        protected SessionsDS _SessionsDS;

        public SessionManager()
        {
            InitDataSet();
        }

        private void InitDataSet()
        {
            try
            {
            }
            catch (Exception e)
            {
                throw new ApplicationException("Can't init session's DataSet:" + e.Message);
            }
        }

        public ISession this[string idx]
        {
            get
            {
                Session result = null;
                if (idx == null)
                    return result;
                try
                {
                    DataRow[] rows = _SessionsDS.Sessions.Select("name='" + idx + "'");
                    if (rows.Length > 0)
                    {
                        SessionsDS.SessionsRow row = rows[0] as SessionsDS.SessionsRow;
                        result = new Session(this, row);
                    }
                    else
                    {
                        SessionsDS.SessionsRow newRow = _SessionsDS.Sessions.AddSessionsRow(Guid.NewGuid(), idx);
                        result = new Session(this, newRow);
                    }
                }
                catch (Exception e)
                {
                    throw new ApplicationException("Exception in Session manager indexer Get method: " + e.Message);
                }
                return result;
            }
        }

        public void Dispose()
        {
            Flush();
        }

        public void Flush()
        {
            try
            {
            }
            catch (Exception e)
            {
                throw new ApplicationException("Exception in Session manager Flush method: " + e.Message);
            }
        }


        public class Session : ISession
        {
            readonly SessionManager _Mngr;

            readonly SessionsDS.SessionsRow _Row;

            public Session(SessionManager mngr, SessionsDS.SessionsRow row)
            {
                _Mngr = mngr;
                _Row = row;
            }

            [NonPersistent]
            public string this[string idx]
            {
                get
                {
                    string result = null;
                    SessionsDS.SessionEntryRow[] rows = _Row.GetSessionEntryRows();
                    foreach (SessionsDS.SessionEntryRow row in rows)
                    {
                        if (row.EntryKey == idx)
                        {
                            result = row.EntryValue;
                        }
                    }
                    return result;
                }
                set
                {
                    SessionsDS.SessionEntryRow[] rows = _Row.GetSessionEntryRows();
                    foreach (SessionsDS.SessionEntryRow row in rows)
                    {
                        if (row.EntryKey == idx)
                        {
                            row.EntryValue = value;
                            return;
                        }
                    }
                    try
                    {
                        _Mngr._SessionsDS.SessionEntry.AddSessionEntryRow(Guid.NewGuid(), _Row, idx, value);
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException("Exception in Session indexer Set method: " + e.Message);
                    }
                }
            }

            public ISessionManager Manager
            {
                get { return _Mngr; }
            }
        }
    }
}
