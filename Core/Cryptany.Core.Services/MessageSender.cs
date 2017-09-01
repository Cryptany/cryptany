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
using System.Configuration;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.DPO.Predicates;
using Cryptany.Core;
using Cryptany.Common.Utils;

namespace Cryptany.Core
{
	public class MessageSender
	{

		public static bool SendAnswerMap(AnswerMap map, Message msg, List<OutputMessage> sendList)
		{
			bool res = true;
            Trace.WriteLine("Router: checking blocks - " + map.AnswerBlocks.Count);
            foreach (AnswerBlock block in map.AnswerBlocks)
            {
                Trace.WriteLine("Router: checking block rules" + block.ID + " " + block.Name);
                if (RuleChecker.CheckObjectRules(block.Object, msg))
                {
                    Trace.WriteLine("Router: found block by rule...");
                    res &= SendAnswerBlock(block, msg, sendList);
                    Trace.WriteLine("Router: Sent message from block - " + res);

                    if (!res) 
                        break;

                 res &= ActionExecuter.ExecuteActions(msg, block.Object.ID, sendList);
                 Trace.WriteLine("Router: performed action for block - " + res);
                    
                }
                if (!res) break;

            }
			return res;
		}

        public static bool SendAnswerBlock(AnswerBlock block, Message msg, List<OutputMessage> sendList)
		{
		
            
            bool res = false;
			string abTypeName = block.BlockType.Name.Trim().ToUpper();
			switch ( abTypeName )
			{
				case "SINGLEMESSAGE":
                    res |= ProcessSingleMessageBlock(msg, block, sendList);
					break;
				case "CYCLE":
                    res |= ProcessCycleBlock(msg, block, sendList);
					break;
				case "VARIANTBLOCK":
                    res |= ProcessVariantBlock(msg, block, sendList);
					break;
				case "SEQUENTBLOCK":
                    res |= ProcessSequentMessageBlock(msg, block, sendList);
					break;
				case "RANDOMBLOCK":
                    res |= ProcessRandomMessageBlock(msg, block, sendList);
					break;
                case "CHAIN":
                    res |= true;
                    break;
			}
			return res;
		}

        private static bool ProcessSequentMessageBlock(Message msg, AnswerBlock block, List<OutputMessage> sendList)
		{
			bool ok = true;
			Comparison<Answer> comp =
				delegate(Answer answer1, Answer answer2)
				{
					return answer1.PosInCycle.CompareTo(answer2.PosInCycle);
				};
			block.Answers.Sort(comp);
            Trace.WriteLine("Router: looking for answers");
			foreach ( Answer answer in block.Answers )
			{
                Trace.WriteLine("Router: checking answer rules");
                if (RuleChecker.CheckObjectRules(answer.Object, msg))
                {
                    Trace.WriteLine("Router: found answer by rule");
                    ok &= SendAnswer(answer, msg, sendList);
                    Trace.WriteLine("Router: tried to send answer found by rules - " + ok);
                }
			}
			return ok;
		}

        private static bool ProcessSingleMessageBlock(Message msg, AnswerBlock block, List<OutputMessage> sendList)
		{
			bool ok = false;
            if (block.Answers.Count == 0)
            {
                throw new ArgumentOutOfRangeException("block", string.Format("There are no answers in SINGLE answer block id={0}, msg = {1}", block.ID, msg));
            }

			
			Answer answer = block.Answers[0];
			if ( RuleChecker.CheckObjectRules(answer.Object, msg) )
				ok |= SendAnswer(answer, msg, sendList);
			
			return ok;
		}

        private static bool ProcessRandomMessageBlock(Message msg, AnswerBlock block, List<OutputMessage> sendList)
		{
			bool ok = false;
			if ( block.Answers.Count == 0 )
			{
                throw new ArgumentOutOfRangeException("block",string.Format("There are no answers in RANDOM answer block id={0}, msg = {1}", block.ID, msg));
			}
			Comparison<Answer> comp =
				delegate(Answer answer1, Answer answer2)
				{
					return answer1.PosInCycle - answer2.PosInCycle;
				};
			block.Answers.Sort(comp);
			DateTime now = DateTime.Now;
			Random rnd = new Random(now.Millisecond + 1000 * now.Second + 10000 * now.Minute + 100000 * now.Hour);
			int indexToSend = rnd.Next(0, block.Answers.Count);
			ok |= SendAnswer(block.Answers[indexToSend], msg, sendList);
			return ok;
		}

        private static bool ProcessVariantBlock(Message msg, AnswerBlock block, List<OutputMessage> sendList)
		{
			return ProcessCycleBlock(msg, block, sendList);
		}

        private static bool ProcessCycleBlock(Message msg, AnswerBlock block, List<OutputMessage> sendList)
		{
			bool ok = false;
			if ( block.Answers.Count == 0 )
			{
                throw new ArgumentOutOfRangeException("block", string.Format("There are no answers in CYCLE answer block id={0}, msg = {1}", block.ID, msg));
			}
			int curCyclePos;
			string keyName = "cbp" + block.ID;
		    Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
			string tmp = ab.Session[keyName];

			if ( !string.IsNullOrEmpty(tmp) && int.TryParse(tmp, out curCyclePos) )
				curCyclePos = (curCyclePos + 1) % block.Answers.Count;
			else
				curCyclePos = 0;

			ab.Session[keyName] = curCyclePos.ToString();

			Answer ans = block.Answers[curCyclePos];
			ok |= SendAnswer(ans, msg, sendList);
			return ok;
		}

		public static bool SendAnswer(Answer ans, Message msg, List<OutputMessage> sendList)
		{
            TextContent content = new TextContent(ans.Body);
		    ActionExecuter.ExecuteActions(msg, ans.Object.ID, sendList);
            OutputMessage om = CreateOutMessage(content, msg, ans.Block.Map.Channel,Guid.Empty, ans.Object.ObjectProcessOptions);
            if (om!=null)
            {
                sendList.Add(om);
                return true;
            }
		    Trace.WriteLine("Router: Couldn't generate answer.");
            return false;
			
		}


		public static OutputMessage CreateOutMessage(Content contentToSend, Message msg,Channel ch,Guid tariffId,IList<Rule> options)
		{
		    if ( options == null )
				options = new List<Rule>();
	
            OutputMessage outMsg = GetOutMessage(contentToSend, msg);
			try
			{
                string errorMes = "";
                Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
                ActionExecuter.ProcessMacros(outMsg, ch.ContragentResource, ref errorMes);
				SetOutMessageProperties(outMsg, msg, options);
                outMsg.ProjectID = (Guid)ch.ContragentResource.ID;
				
                if (outMsg.SmscId == Guid.Empty)
                {
                    throw new ApplicationException("Cannot send message. No output connector defined");
                    
                }
                if (tariffId != Guid.Empty)
                {

                    Tariff t = ChannelConfiguration.DefaultPs.GetEntityById<Tariff>(tariffId);
                    try
                    {
                        outMsg.TariffId = tariffId;
                        outMsg.HTTP_Category = t.OperatorParameters;
                        string sn = t.SN.Number;
                        outMsg.Source = sn;
                    }
                    catch (Exception ex)
                    {
                        Trace.Write("Router: " + ex);
                        throw new ApplicationException("Couldn't find tariff for output message. TariffId= " + tariffId.ToString(), ex);
                    }
                }

                else  if (string.IsNullOrEmpty(outMsg.HTTP_Category))
                {
                    ServiceNumber sn = ServiceNumber.GetServiceNumberBySN(OutputMessage.GetServiceNumber(outMsg.Source));
                    UnaryOperation<Tariff> u = delegate(Tariff tt)
                                                 {
                                                     return tt.SN == sn &&
                                                            tt.Operator == ab.AbonentOperator &&
                                                            tt.TarifficationType == TarifficationType.InboundOutbound &&
                                                            tt.IsActive;
                                                 };
                    List<Tariff> t = ChannelConfiguration.DefaultPs.GetEntitiesByPredicate(u);

                    if (t.Count > 0)
                    {
                        outMsg.TariffId = (Guid)t[0].ID;
                        outMsg.HTTP_Category = t[0].OperatorParameters;

                    }
                }
              

			}
			catch ( Exception ex )
			{
			    Trace.Write("Router: " + ex);
			    throw new ApplicationException("Unable to send message " + outMsg, ex);
			}
            return outMsg;
		}


		private static bool SetOutMessageProperty(OutputMessage outMsg, Message msg, Statement option)
		{
			bool res = false;
			if ( option.Parameter.Name.ToUpper() == "SERVICENUMBER" )
			{
				string sn = option.ParameterValues[0].Value;
				int temp;
				if ( int.TryParse(sn, out temp) )
				{
					outMsg.Source = sn;
					res = true;
				}
				else
				{
					try
					{
						Guid id = new Guid(sn);
						ServiceNumber serviceNumber =
							ChannelConfiguration.DefaultPs.GetEntityById(typeof(ServiceNumber), id)
							as ServiceNumber;
					    string sernumber = serviceNumber.Number;
                        outMsg.Source =  sernumber;
						res = true;
					}
					catch
					{
                        throw new ApplicationException(string.Format("ActionExecuter.SetOutputMessageproperty: Unrecognized value is given as service number - '{0}'", sn));
						
					}
				}
			}
			else if ( option.Parameter.Name.ToUpper() == "CONNECTOR" )
			{
				string connector = option.ParameterValues[0].Value;// It is either the code, or the id of the connector
				long temp;
				if ( long.TryParse(connector, out temp) ) // Service number defined
				{
					SMSC smsc =
						ChannelConfiguration.DefaultPs.GetOneEntityByFieldValue<SMSC>("Code", temp);
					outMsg.SmscId = smsc.DatabaseId;
					res = true;
				}
				else
				{

					try
					{
						Guid id = new Guid(connector);
                        outMsg.SmscId = id;
						res = true;
					}
					catch(ApplicationException aex)
					{
                        throw new ApplicationException(string.Format("ActionExecuter.SetOutputMessageproperty: Unrecognized value is given as connector - '{0}'", connector));
					}
				}
				
			}

            else if (option.Parameter.Name.ToUpper() == "OPERATORPARAMETERS")
            {
                string parameters = option.ParameterValues[0].Value;// It is either the code, or the id of the connector
                outMsg.HTTP_Category = parameters;
                res = true;
               

            }
            else
            {
                throw new ApplicationException(string.Format("ActionExecuter.SetOutputMessageproperty: Unrecognized parameter name - '{0}'", option.Parameter.Name));
            }
			return res;
		}


		private static OutputMessage GetOutMessage(Content contentToSend, Message msg)
		{
			OutputMessage outMsg = new OutputMessage();
			outMsg.ID = IdGenerator.NewId;
			outMsg.TransactionId = msg.TransactionID;
			outMsg.InboxMsgID = msg.InboxId;
            if (!string.IsNullOrEmpty(msg.TransactionID))
                outMsg.Source = msg.ServiceNumberString + "#" + msg.TransactionID;
            else
			    outMsg.Source = msg.ServiceNumberString;
			outMsg.Destination = msg.MSISDN;
			outMsg.SmscId = msg.SMSCId;
			outMsg.HTTP_Category = msg.HTTP_Category;
			outMsg.HTTP_UID = msg.HTTP_UID;
			outMsg.HTTP_Protocol = msg.HTTP_Protocol;
			outMsg.HTTP_Operator = msg.HTTP_Operator;
			outMsg.HTTP_DeliveryStatus = "";
            outMsg.OperatorSubscriptionId = msg.OperatorSubscriptionId;

			if ( contentToSend is LinkToContent )
			{
				TextContent txtContent = new TextContent(ConfigurationManager.AppSettings["LinkToContentMessage"] +
				(contentToSend as LinkToContent).ContentURL);
				outMsg.Content = txtContent;
			}
			else
			{
				outMsg.Content = contentToSend;
			}
			outMsg.IsPayed = true;
			return outMsg;
		}

		public static Guid SendContent(Content contentToSend, Message msg,Channel ch,Guid tariffId, List<OutputMessage> sendList)
		{
            OutputMessage om = CreateOutMessage(contentToSend, msg, ch,tariffId,(IList<Rule>)null);
            if (om!=null)
            {
                sendList.Add(om);
                return om.ID;
            }
            return Guid.Empty;
		}

        public static Guid SendTextMessage(string message, Message msg,Channel ch,Guid tariffId, List<OutputMessage> sendList)
		{
			Content c = new TextContent(message);
            return SendContent(c, msg,ch, tariffId,sendList);
            
		}


		private static bool SetOutMessageProperties(OutputMessage outMsg, Message msg, IList<Rule> options)
		{
			bool res = true;
			foreach ( Rule option in options )
			{
				if ( RuleChecker.CheckObjectRules(option.Object, msg) )
					foreach ( Statement s in option.Statements )
						res &= SetOutMessageProperty(outMsg, msg, s);
			}
			return res;
		}
	
	}
}
