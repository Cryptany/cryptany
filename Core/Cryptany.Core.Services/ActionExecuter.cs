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
using System.Data.SqlClient;
using System.Diagnostics;
using Cryptany.Core.ConfigOM;
using System.Messaging;
using Cryptany.Core.Interaction;
using Cryptany.Core.Management;
using Cryptany.Core.MsmqLog;
using Cryptany.Core.Services.Management;
using Cryptany.Common.Utils;
using Cryptany.Common.Logging;
using Cryptany.Core.DPO;
using Cryptany.Core.MacrosProcessors;
using System.ServiceModel;

namespace Cryptany.Core
{
	public static class ActionExecuter
	{
		public static bool ExecuteActions(Message msg, object objectId, List<OutputMessage> sendList)
		{
			bool res = true;
			Cryptany.Core.DPO.PersistentStorage ps = ChannelConfiguration.DefaultPs;
            GeneralObject generalObject = ps.GetEntityById<GeneralObject>(objectId);
            if (generalObject == null)
                return true;
			bool ok = RuleChecker.CheckObjectRules(generalObject, msg);
			if (!ok)
				return true;
			foreach ( Rule action in generalObject.Actions )
                res &= ExecuteAction(action, generalObject, msg, sendList);
			return res;
		}

		private static bool ExecuteAction(Rule action, GeneralObject generalObj, Message msg, List<OutputMessage> sendList)
		{
			bool res = true;
            if (action.Mode == RuleType.Expression)
            {
                if (RuleChecker.CheckObjectRules(generalObj, msg))
                {
                    if (RuleChecker.CheckRuleStatement(action.Statement1, msg))
                    {
                        if (action.Rule1 != null)
                            return ExecuteAction(action.Rule1, generalObj, msg, sendList);
                    }
                    else
                    {
                        if (action.Rule2 != null)
                            return ExecuteAction(action.Rule2, generalObj, msg, sendList);
                    }
                    return true;
                    // If none of the nested actions has been executed, then no error occured, so return a positive result
                }
            }
            else
            {
                if (res &= PerformAction(action, generalObj, msg, sendList))
                {
                    if (action.Rule1 != null)
                        res &= ExecuteAction(action.Rule1, generalObj, msg, sendList);
                }
                else
                {
                    if (action.Rule2 != null)
                        res &= ExecuteAction(action.Rule2, generalObj, msg, sendList);
                }
                if (res)
                    foreach (Rule a in action.Actions)
                        res &= ExecuteAction(a, action.Object, msg, sendList);
            }
		    return res;
		}

        private static bool PerformAction(Rule rule, GeneralObject generalObj, Message msg, List<OutputMessage> sendList)
		{
			bool res = true;
			if ( RuleChecker.CheckObjectRules(rule.Object, msg) )
			{
				foreach ( Statement statement in rule.Statements )
				{
				    switch (statement.Parameter.Name.ToUpper())
					{
					    case "MTCLUB":
					        res &= SetSubscription(statement, generalObj.ConcreteObject, msg);
					        break;
					
					    case "SENDMESSAGE":
					        //res &= MessageSender.SendMessages(statement, generalObj, msg, sendList);
					        break;
                        case "SENDCHAIN":
					        {
					            ParameterValue val = statement.ParameterValues[0];
					            res &= SendMessageChain(msg, new Guid(val.Value));
					            break;
					        }
                        case "SENDCHAIN2":
                            {
                                ParameterValue val = statement.ParameterValues[0];
                                res &= SendMessageChain_2(msg, new Guid(val.Value));
                               // res &= SendMessageChain_3(msg, new Guid(val.Value),generalObj.ConcreteObject, sendList);
                                break;


                            }
					    case "SENDANSWERMAP":
					        foreach ( ParameterValue val in statement.ParameterValues )
					        {
					            try
					            {
					                Guid id = new Guid(val.Value);
					                PersistentStorage ps = ChannelConfiguration.DefaultPs;//ClassFactory.CreatePersistentStorage(ChannelConfiguration.Ds);
					                AnswerMap am = (AnswerMap)ps.GetEntityById(typeof(AnswerMap), id);
					                res &= MessageSender.SendAnswerMap(am, msg,sendList);
					            }
					            catch ( Exception ex )
					            {
					                res = false;
					                throw new ApplicationException(string.Format("ActionExecutor: Unable to perform 'SEND-ANSWERMAP' (ID = '{0}') ", val.Value), ex);
					            }
					        }
					        break;
					
					    case "SENDANSWERBLOCK":
					        foreach ( ParameterValue val in statement.ParameterValues )
					        {
					            try
					            {
					                Guid id = new Guid(val.Value);
					                PersistentStorage ps = ChannelConfiguration.DefaultPs;//ClassFactory.CreatePersistentStorage(ChannelConfiguration.Ds);
					                AnswerBlock block = (AnswerBlock)ps.GetEntityById(typeof(AnswerBlock), id);
					                res &= MessageSender.SendAnswerBlock(block, msg, sendList);
					            }
					            catch ( Exception ex )
					            {
					                res = false;
					                throw new ApplicationException(
					                    string.Format("ActionExecutor: Unable to perform 'SEND-ANSWERBLOCK' (ID = '{0}'): {1} ", val.Value, ex.ToString()));
					            }
					        }
					        break;
					
				        case "SENDANSWER":
				            foreach ( ParameterValue val in statement.ParameterValues )
				            {
				                try
				                {
				                    Guid id = new Guid(val.Value);
				                    PersistentStorage ps = ChannelConfiguration.DefaultPs;//ClassFactory.CreatePersistentStorage(ChannelConfiguration.Ds);
				                    Answer ans = (Answer)ps.GetEntityById(typeof(Answer), id);
				                    res &= MessageSender.SendAnswer(ans, msg, sendList);
				                }
				                catch ( Exception ex )
				                {
				                    res = false;
				                    throw new ApplicationException(string.Format("ActionExecutor: Unable to perform 'SEND-ANSWER' (ID = '{0}') ", val.Value), ex);
				                }
				            }
				            break;
				    }
				}
			}
			return res;
		}

        private static bool SendMessageChain(Message msg, Guid chainId)
        {
            try
            {
                Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
                using (SqlConnection conn = Database.Connection)
                {
                    using (SqlCommand comm = new SqlCommand("kernel.DistributeChain", conn))
                    {
                        comm.CommandType = System.Data.CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("@inboxId", msg.InboxId);
                        comm.Parameters.AddWithValue("@chainId", chainId);
                        comm.Parameters.AddWithValue("@abonentId", ab.DatabaseId);
                        comm.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch(SqlException ex)
            {
                throw new ApplicationException("Error sending message chain.", ex);
            }
        }

        private static bool SendMessageChain_2(Message msg, Guid chainId)
        {

      
            Chains chain = (Chains)ChannelConfiguration.DefaultPs.GetEntityById(typeof(Chains), chainId);

            if (string.IsNullOrEmpty(chain.Text1)||chain.TariffId1==Guid.Empty)
                throw new ApplicationException("Error sending message chain. Wrong data. ChainId= "+chainId.ToString() );

            string errText;


            var factory = new ChannelFactory<ISMSSender>("TM_Endpoint");
            ISMSSender tm = factory.CreateChannel();
            Dictionary<string, object> addParams = new Dictionary<string, object>();

            addParams.Add("inboxId", msg.InboxId);
            addParams.Add("messagePriority", (int)Cryptany.Core.Interaction.MessagePriority.High);


            Guid outBoxId = tm.SendSMS(chain.TariffId1, msg.MSISDN, chain.Text1, new Guid("C935B842-1230-DF11-9845-0030488B09DC"), addParams, out errText);

            factory.Close();
            if (!(outBoxId == Guid.Empty))
            {
                try
                {
                    Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);

                    MSMQLogEntry mle = new MSMQLogEntry();
                    mle.CommandText = "services.DistributeChain";
                    mle.Parameters.Add("@inboxId", msg.InboxId);
                    mle.Parameters.Add("@chainId", chainId);
                    mle.Parameters.Add("@abonentId", ab.DatabaseId);
                    mle.Parameters.Add("@outboxId", outBoxId);

                    using (MessageQueue _MSMQLoggerInputQueue = ServiceManager.MSMQLoggerInputQueue)
                    {
                        _MSMQLoggerInputQueue.Send(mle);
                    }

                    return true;
                }
                catch (SqlException ex)
                {
                    throw new ApplicationException("Error sending message chain.", ex);
                }


            }

            throw new ApplicationException("Error sending message chain. ChainId=  "+chainId.ToString() + " " + errText);
        }

        private static bool SetSubscription(Statement statement, EntityBase obj, Message msg)
		{
			DateTime dt1 = DateTime.Now;
            Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
            SubscriptionMessage sm = new SubscriptionMessage();
            sm.abonentId = ab.DatabaseId;
		    sm.MSISDN = msg.MSISDN;
		    Trace.WriteLine("Router: getting resource by channel id");
		    AnswerMap map = obj as AnswerMap;
            if (map == null)
		    {
                AnswerBlock anb = obj as AnswerBlock;
                if (anb != null)
                    map = anb.Map;
            }
            if (map==null) return false;
            ContragentResource resource = GetChannelContragentResource(map.Channel);

			if ( resource == null )
			{
                throw new ApplicationException(string.Format("ActionExecuter: Unable to find ContragentResource for the channel (id='{0},Name='{1}').", map.Channel.ID.ToString(), map.Channel.Name));
			}
            sm.resourceId = (Guid)resource.ID;
            Trace.WriteLine("Router: got resource by channel id: " + sm.resourceId);
		    
            sm.smsId = msg.InboxId;
            sm.actionTime = DateTime.Now;
			
			
                switch (statement.CheckAction.Predicate)
                {
                    case CheckActionPredicate.In:
                        
                        sm.actionType = SubscriptionActionType.Subscribe;
                        break;
                    case CheckActionPredicate.NotIn:
                        
                        sm.actionType = SubscriptionActionType.Unsubscribe;
                        break;
                    case CheckActionPredicate.IsEqual:
                        sm.actionType = SubscriptionActionType.UnsubscribeAll;
                        break;
                }
			
			

			foreach ( ParameterValue val in statement.ParameterValues )
			{
				Guid clubId = new Guid(val.Value);
			    sm.clubs.Add(clubId);

			}

            using (MessageQueue mq = ServicesConfigurationManager.SubscriptionQueue)
            {
               
                mq.Send(sm);
            }
		    DateTime dt2 = DateTime.Now;
			TimeSpan ts = dt2-dt1;
			Trace.WriteLine(string.Format("SETSUBSCRIPTION: Time: {0} ms", ts.TotalMilliseconds));
			return true;
            
		}

        private static bool ProcessMacros(IDictionary<string,MacrosProcessor> processors, OutputMessage omsg, ref string errorText)
		{
            List<Macros> macroses = Macros.GetMacroses(((TextContent)omsg.Content).MsgText);
		    for(int i=macroses.Count-1;i>=0;i--)
            {
                Macros m = macroses[i]; 
                MacrosProcessor mp = processors[m.Name];
                if (mp!=null)
                {
                    string replaceString = mp.Execute(omsg,m.Parameters);
                    ((TextContent)omsg.Content).MsgText = ((TextContent)omsg.Content).MsgText.Replace(m.Text, replaceString);
                }
            }
            
            return true;

		}

        public static bool ProcessMacros(OutputMessage outMsg, ContragentResource cr, ref string errorText)
		{
            Dictionary<string, MacrosProcessor> processors = new Dictionary<string, MacrosProcessor>();
           
            ContentProcessor cpn = new ContentProcessor(cr);
             processors.Add(cpn.MacrosName, cpn);
           
		    ServiceNumberProcessor snp = new ServiceNumberProcessor();
            processors.Add(snp.MacrosName, snp);

            HelpDeskProcessor hdp = new HelpDeskProcessor();
            processors.Add(hdp.MacrosName,hdp);
		    
		    return ProcessMacros(processors, outMsg, ref errorText);
		}

		internal static ContragentResource GetChannelContragentResource(Channel channel)
		{
			ContragentResource res = null;
			PersistentStorage ps = ChannelConfiguration.DefaultPs;
            if (channel!=null && channel.ContragentResource != null)
                res = channel.ContragentResource;
            if (res == null && channel != null)
			{
				foreach (ServiceSetting ss in channel.Service.ServiceSettings)
				{
					if (ss.Name == "SubscriptionContragentResourceName")
					{
						//Guid id = new Guid(ss.Value);
						try
						{
							res = ps.GetEntitiesByFieldValue(typeof(ContragentResource), "Name", ss.Value)[0] as ContragentResource;
						}
						catch (Exception ex)
						{
							throw new Exception(string.Format("Unable to find the contragent resource by it's name, Name = {0}", ss.Value), ex);
						}
						//if (id != null && id != Guid.Empty)
						//{
						//    res = ps.GetEntityById(typeof(ContragentResource), id) as ContragentResource;
						//}
						break;
					}
				}
			}
			if (res == null)
			{
				List<EntityBase> l = ps.GetEntitiesByFieldValue(typeof(ContragentResource), "Name", "TV services");
				if (l.Count > 0)
					res = l[0] as ContragentResource;
			}
			return res;
		}
	}
}
