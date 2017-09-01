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
using Cryptany.Core;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using DataSetLib;

namespace Cryptany.Core.Services
{
	public class SessionManagerService : MarshalByRefObject, ISessionManager
	{
		private DataSetLib.SessionsDS _sessionDs = new DataSetLib.SessionsDS();
		private PersistentStorage _ps;

		public SessionManagerService()
		{
			if ( !InitDataSet() )
				return;
			_ps = ClassFactory.CreatePersistentStorage(_sessionDs);
			List<Session> list = _ps.GetEntities<Session>();
		}

		private bool InitDataSet()
		{
			try
			{
				DataSetLib.SessionsDSTableAdapters.SessionsTableAdapter sta = new DataSetLib.SessionsDSTableAdapters.SessionsTableAdapter();
				sta.Fill(_sessionDs.Sessions);
				DataSetLib.SessionsDSTableAdapters.SessionEntryTableAdapter seta = new DataSetLib.SessionsDSTableAdapters.SessionEntryTableAdapter();
				seta.Fill(_sessionDs.SessionEntry);
				return true;
			}
			catch ( Exception e )
			{
				Cryptany.Common.Logging.LoggerFactory.Logger.Write(new Cryptany.Common.Logging.LogMessage("Exception in Session manager InitDataSet method: " + e.ToString(), Cryptany.Common.Logging.LogSeverity.Error));
				return false;
			}
		}

		#region ISessionManager Members

		public ISession this[string idx]
		{
			get
			{
				Session s = _ps.GetEntityById<Session>(idx);
				if ( s == null )
				{
					s = ClassFactory.CreateObject<Session>(_ps);
					s.SetManager(this);
				}
				return s;
			}
		}

		public void Flush()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion
	}

	[Table("Sessions")]
	public class Session : EntityBase, ISession
	{
		private SessionManagerService _manager;

		#region ISession Members

		[NonPersistent]
		public string this[string idx]
		{
			get
			{
				if ( Entries.Indexes["EntryKey"].ContainsKey(idx) )
					return Entries.Indexes["EntryKey"][idx][0].EntryValue;
				else
				{
					SessionEntry se = ClassFactory.CreateObject<SessionEntry>(CreatorPs);
					se.AbonentSession = this;
					Entries.Add(se);
					return se.EntryValue;
				}
			}
			set
			{
				if ( Entries.Indexes["EntryKey"].ContainsKey(idx) )
					Entries.Indexes["EntryKey"][idx][0].EntryValue = value;
				else
				{
					SessionEntry se = ClassFactory.CreateObject<SessionEntry>(CreatorPs);
					se.AbonentSession = this;
					Entries.Add(se);
				}
			}
		}

		[NonPersistent]
		public ISessionManager Manager
		{
			get
			{
				return _manager;
			}
		}

		internal void SetManager(SessionManagerService manager)
		{
			_manager = manager;
		}

		#endregion

		[Relation(RelationType.OneToMany, typeof(SessionEntry), "AbonentSession")]
		public EntityCollection<SessionEntry> Entries
		{
			get
			{
				EntityCollection<SessionEntry> c = GetValue<EntityCollection<SessionEntry>>("Entries");
				if ( c.IndexableFieldsNames == null || c.IndexableFieldsNames.Length == 0 )
				{
					c.IndexableFieldsNames = new string[] { "EntryKey" };
				}
				return c;
			}
			set
			{
				SetValue("Entries", value);
			}
		}
	}

	[Table("SessionEntry")]
	public class SessionEntry : EntityBase
	{
		[FieldName("SessionID")]
		[Relation(RelationType.OneToOne, typeof(Session), "ID")]
		[ObligatoryField]
		public Session AbonentSession
		{
			get
			{
				return GetValue<Session>("Token");
			}
			set
			{
				SetValue("Token", value);
			}
		}

		public string EntryKey
		{
			get
			{
				return GetValue<string>("EntryKey");
			}
			set
			{
				SetValue("EntryKey", value);
			}
		}

		public string EntryValue
		{
			get
			{
				return GetValue<string>("EntryValue");
			}
			set
			{
				SetValue("EntryValue", value);
			}
		}
	}
}
