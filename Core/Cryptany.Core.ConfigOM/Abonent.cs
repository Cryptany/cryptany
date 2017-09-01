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
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects.Attributes;
using System.Data.SqlClient;
using System.Data;
using System.Messaging;

using System.Linq;

namespace Cryptany.Core.ConfigOM
{
    public enum AbonentState
    {
        NotBlocked=0,
        Blocked=1,
        Unknown=2
    }


	[DbSchema("kernel")]
	[DenyLoadAllOnGetById]
	[IdFieldName("MSISDN")]
	[GetByIdCommand("EXEC [kernel].[GetAbonentByMSISDN] @MSISDN = @@id", "@@id")]
	[Serializable]
	public class Abonent : EntityBase
	{
       [FieldName("IsBlocked")]
       	public AbonentState IsBlocked
		{
			get
			{
                return Enum.IsDefined(typeof(AbonentState), GetValue<int>("IsBlocked")) ?
                    (AbonentState)GetValue<int>("IsBlocked") : AbonentState.Unknown;
			}
            set
            {
                SetValue("IsBlocked", value);
            }
		}

		[NonPersistent]
		public Guid DatabaseId
		{
            get
            {
                return GetValue<Guid>("ID");
            }
            set
            {
                SetValue("ID", value);
            }
		}

        [IdField]
		[ObligatoryField]
		public string MSISDN
		{
			get
			{
				return GetValue<string>("MSISDN");
			}
			set
			{
				SetValue("MSISDN", value);
			}
		}

		[FieldName("RegionId")]
		[Relation( RelationType.OneToOne, typeof(Region), "ID")]
		public Region AbonentRegion 
		{
			get
			{
				return GetValue<Region>("AbonentRegion");
			}
			set
			{
				SetValue("AbonentRegion", value);
			}
		}


		[FieldName("OperatorId")]
		[Relation( RelationType.OneToOne, typeof(Operator), "ID")]
		public Operator AbonentOperator
		{
			get
			{
				return GetValue<Operator>("AbonentOperator");
			}
			set
			{
				SetValue("AbonentOperator", value);
			}
		}


		public bool IsFake
		{
			get
			{
				return GetValue<bool>("IsFake");
			}
			set
			{
				SetValue("IsFake", value);
			}
		}
	
		[Relation( RelationType.OneToOne, typeof(SMSC),"ID")]
		[FieldName("SendSMSCId")]
		public SMSC SendThroughSMSC
		{
			get
			{
				return GetValue<SMSC>("SendThroughSMSC");
			}
			set
			{
				SetValue("SendThroughSMSC", value);
			}
		}


	    private AbonentSession _session;
		[NonPersistent]
		public AbonentSession Session
		{
			get
			{
			    Debug.WriteLine("USSD: Getting abonent session");
                if (_session ==null)
                {
                     _session = ClassFactory.CreateObject<AbonentSession>(CreatorPs);
                }
                return _session;
			}
           
		}

        private Channel _lockedChannel;

        public  Channel LockedChannel
        {
            get
            {
                return _lockedChannel;
            }
            set
            {

                _lockedChannel = value;
            }

        }
        

	    public static Abonent GetByMSISDN(string Msisdn)
		{
			return ChannelConfiguration.DefaultPs.GetEntityById<Abonent>(Msisdn);
		}

      public static Abonent LoadAbonent (string msisdn)
      {
        return (Abonent) ChannelConfiguration.DefaultPs.GetLoader(typeof(Abonent)).LoadEntityById(msisdn);


      }

	    private List<string> _clubs = null;
        public List<string> Clubs
        {
            get
            {
                if (_clubs==null)
                {
                    _clubs = new List<string>();
                  ClubsManagerWCF.ClubsManager2Client  clubsClient =null;
                  try
                  {
                      clubsClient = new ClubsManagerWCF.ClubsManager2Client();
                      Dictionary<Guid, ClubsManagerWCF.Subscription> subs = clubsClient.GetSubscriptions(MSISDN, false);
                      _clubs = subs.Values.Select(s => s.Club.Id.ToString()).ToList();
                  }
                  catch (Exception ex)
                  {
                      Trace.Write("Router: " + ex);
                      throw new ApplicationException("Cannot get clubs list.");
                  }
                    finally
                    {
                        if (clubsClient != null)
                            clubsClient.Close();
                    }
                }
                return _clubs;
            }
            set
            {
                _clubs = value;
            }
        }
	}
}
