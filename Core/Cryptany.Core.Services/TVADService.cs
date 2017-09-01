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
using System.Diagnostics;
using Cryptany.Core.ConfigOM;
using Cryptany.Common.Logging;
using Cryptany.Core.Services;

namespace Cryptany.Core
{
    public class TVADService : AbstractService
    {
        public TVADService(IRouter router) : base(router)
        { }

        public TVADService(IRouter router, string serviceName)
            : base(router, serviceName)
        { }



        private void ProcessMessageInner_ThreadWork(object state)
        { }

		protected override bool ProcessMessageInner(Message msg, AnswerMap map)
		{
			bool ok = true;
            if (map == null)
            {
                Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
                if (ab == null)
                {
                    return false;
                }
                Channel lockedChannel = ab.LockedChannel;

                ok = false;
                if (lockedChannel != null)
                {
                    foreach (AnswerMap am in lockedChannel.AnswerMaps)
                    {
                        if (am.IsMain)
                        {
                            
                            ok = TrySendAnswerMap(am, msg);
                            ok &= TryExecuteActions(am, msg);

                            break;
                        }
                    }
                    Logger.Write(new LogMessage(string.Format("TVAD.ProcessMessageInner: Abonent {0} is locked to the channel {1} and answerMap is null; unable to locate any main answermap; msg = {2}", msg.MSISDN, lockedChannel.Name, msg), LogSeverity.Alert));
                }
                else
                    Logger.Write(new LogMessage(string.Format("TVAD.ProcessMessageInner: Abonent {0} is not locked to any channel and answerMap is null; unable to locate any main answermap; msg = {1}", msg.MSISDN, msg), LogSeverity.Alert));
            }
            else
            {
                ok &= TrySendAnswerMap(map, msg);
                Trace.WriteLine("Router: tried to send answers - " + ok);

                ok &= TryExecuteActions(map, msg);
                Trace.WriteLine("Router: performed actions - " + ok);
            }
			return ok;
		}

		private bool TrySendAnswerMap(AnswerMap map, Message msg)
		{
			try
			{
				return MessageSender.SendAnswerMap(map, msg, _messages);
			}
			catch ( Exception ex )
			{
                Trace.WriteLine("Router:" + ex);
				Logger.Write(new LogMessage("Error occured while attempting to send answer map: " + ex.ToString(), LogSeverity.Error));
				return false;
			}
		}

		private bool TryExecuteActions(AnswerMap map, Message msg)
		{
			try
			{
				return ActionExecuter.ExecuteActions(msg, map.Object.ID, _messages);
			}
			catch ( Exception ex )
			{
				Logger.Write(new LogMessage("Error occured while attempting to execute actions: " + ex.ToString(), LogSeverity.Error));
				return false;
			}
		}

        protected AnswerMap GetAnswerMap(Message msg, Guid tokenId)
        {
			return null;
        }

	}
}