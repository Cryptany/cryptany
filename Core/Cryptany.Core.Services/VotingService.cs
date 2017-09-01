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
using System.Data;
using System.Data.SqlClient;
using System.Messaging;
using System.Text.RegularExpressions;
using DataSetLib;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Services;

namespace Cryptany.Core
{
    public class VotingService : AbstractService
    {
        public VotingService(IRouter router) : base(router)
        {
        }

        public VotingService(IRouter router, string serviceName)
            : base(router, serviceName)
        {
        }

//		protected override void ProcessMessageInner(Message msg, ChannelConfigDS.RegularExpressionsRow regexRow)
//        {
//            DateTime voteTime = DateTime.Now;
//            string[] msgParts = msg.Text.Trim().Split(' ');
//            string voteName = msgParts[0];
//            string voteValue = "";
//            if (msgParts.Length > 1)
//                voteValue = msgParts[1];
//			ChannelConfigDS.ChannelsRow votingChannel =
//				(ChannelConfigDS.ChannelsRow)
//				_ConfigDataSet.Channels.Select("ID='" + msg.Abonent.LockedChannel.ID.ToString() + "'")[0];
//            //ChannelConfigDS.ChannelsRow votingChannel = (ChannelConfigDS.ChannelsRow)_ConfigDataSet.Channels.Select("ID='CBF25CF0-DFEA-4198-A4D3-34E827C395C9'")[0];
//            ChannelConfigDS.VotingsRow[] votings = votingChannel.GetVotingsRows();
//            {
//                //ILogger logger = LoggerFactory.Logger;
//                //logger.DefaultSource = "VotingService";
//                //logger.Write(new LogMessage(string.Format("There are {0} votings associated with channel ID = {1}", votings.Length.ToString(), votingChannel.id.ToString()), LogSeverity.Debug));
//                //logger.Write(new LogMessage("Abonent locked channel ID = " + ((msg.Abonent.LockedChannel != null) ? msg.Abonent.LockedChannel.id.ToString() : Guid.Empty.ToString()), LogSeverity.Debug));
//            }
//
//            if (msgParts.Length < 2)
//            {
//                string abName = "Subscribe";
//                string answerText = "";
//                ChannelConfigDS.AnswerBlocksRow[] abrows =
//                    votings[0].ChannelsRow.GetAnswerMapsRows()[0].GetAnswerBlocksRows();
//                foreach (ChannelConfigDS.AnswerBlocksRow abrow in abrows)
//                {
//                    if (abrow.Name == abName)
//                    {
//                        answerText = abrow.GetAnswersRows()[0].Body;
//                        break;
//                    }
//                }
//                SendContent(new TextContent(answerText), msg, true);
//                //ERROR
//                return;
//            }
//            bool processed = false;
//            string topTemplateMes = "T[O0][PR]";
//            Regex regex = new Regex(topTemplateMes);
//            if (regex.IsMatch(voteValue.ToUpper()))
//            {
//                SendContent(new TextContent(GetTop10Message(votings[0].Name)), msg, true);
//                processed = true;
//            }
//            else
//                foreach (
//                    ChannelConfigDS.VotingCatalogueItemsRow position in
//                        votings[0].VotingCataloguesRow.GetVotingCatalogueItemsRows())
//                {
//                    Regex regex1 = new Regex("^" + position.VoteCode + "$");
//                    if (regex1.IsMatch(voteValue.ToUpper()))
//                    {
//                        SaveVote(position.ID, msg, votings[0].ID);
//                        string answerText = GetMessageOfAnswerBlockNamed("VoteAnswer",
//                                                                         votingChannel.GetAnswerMapsRows()[0]);
//                        if (answerText == null)
//                            answerText = "Vash golos uchten {#CONTENTCODE#}";
//                        string errorText = "";
//                        string oldans = answerText;
//                        answerText = ProcessContentCodeMacros(answerText, position);
//                        try
//                        {
//							ActionExecuter.ProcessMacros(answerText, msg, ref answerText, ref errorText);
//                        }
//                        catch
//                        {
//                            answerText = oldans.Replace("{#CONTENTCODE#}", "");
//                        }
//						MessageSender.SendContent(new TextContent(answerText), msg);
//                        processed = true;
//                        CheckForPrise(msg, votingChannel, votings[0]);
//                        break;
//                    }
//                }
//            if (!processed)
//            {
//                string answerText = GetMessageOfAnswerBlockNamed("Error", votingChannel.GetAnswerMapsRows()[0]);
//                if (answerText == null)
//                    answerText = string.Format("Neverniy kod. Otprav'te {0} i poluchite instrukciyu.", voteName);
//                string errorText = "";
//				ActionExecuter.ProcessMacros(answerText, msg, ref answerText, ref errorText);
//				MessageSender.SendContent(new TextContent(answerText), msg);
//                return;
//            }
//        }

        //private string GetMessageOfAnswerBlockNamed(string abName, ChannelConfigDS.AnswerMapsRow amRow)
        //{
        //    foreach (ChannelConfigDS.AnswerBlocksRow abRow in amRow.GetAnswerBlocksRows())
        //    {
        //        if (abRow.Name == abName)
        //        {
        //            try
        //            {
        //                return abRow.GetAnswersRows()[0].Body;
        //            }
        //            catch
        //            {
        //                return null;
        //            }
        //        }
        //    }
        //    return null;
        //}

        //private void CheckForPrise(Message msg, Channel votingChannel,
        //                           ChannelConfigDS.VotingsRow votingRow)
        //{
        //    int smsCount = 0;
        //    if (msg.Abonent.Session["smsCount"] != null)
        //        smsCount = int.Parse(msg.Abonent.Session["smsCount"]);
        //    smsCount++;
        //    int smsLimit = 500;
        //    DataRow[] priseRows=null; //= _ConfigDataSet.VotingSpecialPrises.Select("Name = 'EVERY500'");
        //    if (priseRows[0]["Description"] != null && priseRows[0]["Description"] != DBNull.Value)
        //    {
        //        try
        //        {
        //            smsLimit = int.Parse(priseRows[0]["Description"].ToString());
        //        }
        //        catch
        //        {
        //        }
        //    }
        //    if (smsCount >= smsLimit)
        //    {
        //        smsCount = 0;
        //        SaveAbonentPrise(msg, votingRow, (Guid) priseRows[0]["ID"]);
        //        string messageText = "";
        //        foreach ( AnswerBlock ab in votingChannel.AnswerMaps[0].AnswerBlocks )
        //        {
        //            if (ab.Name == "Prise")
        //            {
        //                try
        //                {
        //                    messageText = ab.Answers[0].Body;
        //                }
        //                catch
        //                {
        //                    messageText = "Pozdravlyaem! Vash zapros okazalsya 500-m i vi poluchaete podarok";
        //                }
        //                MessageSender.SendTextMessage(messageText, msg);
        //                break;
        //            }
        //        }
        //    }
        //    msg.Abonent.Session["smsCount"] = smsCount.ToString();
        //}

        //private string ProcessContentCodeMacros(string text, ChannelConfigDS.VotingCatalogueItemsRow position)
        //{
        //    string macros = "{#CONTENTCODE#}";
        //    string substitution = "";
        //    if (position["ContentCatalogueID"] != null && position["ContentCatalogueID"] != DBNull.Value)
        //    {
        //        substitution = "{#CONTENT(" + position["ContentCatalogueID"] + ")#}";
        //    }
        //    text = text.Replace(macros, substitution);
        //    return text;
        //}

        //private void SaveAbonentPrise(Message msg, ChannelConfigDS.VotingsRow votingRow, Guid priseID)
        //{
        //    try
        //    {
        //        VotingAbonentSpecialPrisesEntry entry = new VotingAbonentSpecialPrisesEntry(votingRow.ID,
        //                                                                                    msg.Abonent.DatabaseId,
        //                                                                                    priseID, msg.InboxId);
        //        using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServicesConfigurationManager.MSMQLoggerInputQueue)
        //        {
        //            MSMQLoggerInputQueue.Send(entry);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Unable to log into VotingResults.", e.InnerException);
        //    }
        //}

        //private void SaveVote(Guid votePositionID, Message msg, Guid votingID)
        //{
        //    try
        //    {
        //        VotingResultsEntry votingResultsEntry = new VotingResultsEntry(votePositionID, msg.InboxId, votingID);
        //        using (MessageQueue MSMQLoggerInputQueue = Cryptany.Core.Management.ServicesConfigurationManager.MSMQLoggerInputQueue)
        //        {
        //            MSMQLoggerInputQueue.Send(votingResultsEntry);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Unable to log into VotingResults.", e.InnerException);
        //    }
        //}

        //private string GetTop10Message(string votingName)
        //{
        //    try
        //    {
        //        string ret = "Abonenty-lidery: ";
        //        using (SqlCommand cmd = new SqlCommand("votings.GetTop10Abonents", CoreClassFactory.Connection))
        //        {
        //            cmd.Parameters.AddWithValue("@votingName", votingName);
        //            cmd.Parameters.AddWithValue("@topN", 10);
        //            cmd.CommandType = CommandType.StoredProcedure;
        //            DataTable table = new DataTable();
        //            SqlDataAdapter adp = new SqlDataAdapter(cmd);
        //            adp.Fill(table);
        //            int i = 1;
        //            foreach (DataRow r in table.Rows)
        //            {
        //                string msisdn = r["MSISDN"].ToString();
        //                //Оставляем только первые 4 цифры номера (включая 7-ку) и последние 3
        //                int length = msisdn.Length;
        //                string first4 = msisdn.Substring(0, 4);
        //                string last3 = msisdn.Substring(length - 3, 3);
        //                string abonent = first4 + new string('x', length - 3 - 4) + last3;
        //                if (i > 1)
        //                    ret += ",";
        //                ret += " " + i + "." + abonent;
        //                i++;
        //            }
        //            return "TOP 10 uchastnikov s naibol'shim kolichestvom sms zaprosov: " + ret.Trim();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("Exception in VotingService (unable to get the top-10 abonents): ", e);
        //    }
        //}

		protected override bool ProcessMessageInner(Message msg, Cryptany.Core.ConfigOM.AnswerMap answerMap)
		{
			return false;
		}
	}
}