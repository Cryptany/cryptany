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
using System.Text.RegularExpressions;
using DataSetLib;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Services;
namespace Cryptany.Core
{
    public class QuizService : TVADService
    {
        public QuizService(IRouter router, string name) : base(router, name)
        {
        }

		protected override bool ProcessMessageInner(Message msg, AnswerMap map)
        {
            //ISessionManager manager = CoreClassFactory.CreateSessionManager();
			Guid channelId = msg.Abonent.LockedChannel != null ? (Guid)msg.Abonent.LockedChannel.ID : Guid.Empty;

            bool newlyCreated;
            Answer answer = GetCurrentAnswer(channelId, msg.Abonent.DatabaseId, out newlyCreated);

            if (newlyCreated)
            {
            	return MessageSender.SendAnswer(answer, msg);
            }

            string correctAns = GetCorrectAns(answer);

            string abonentAns = msg.Text.ToUpper();
            Regex subscrWordRegex = GetSubscriptionRegex(channelId);
            //Regex subscrWordRegex = new 
            string firstWord = abonentAns.Split(' ')[0];
            if (subscrWordRegex.IsMatch(firstWord))
                abonentAns = abonentAns.Remove(0, firstWord.Length).Trim();

            DateTime ansTime = DateTime.Now;

            Guid nextQuestionId = Guid.NewGuid();
            AddQuizAnswer(channelId, msg.Abonent.DatabaseId, (Guid)answer.ID, abonentAns, correctAns, ansTime,
                          out nextQuestionId);

            answer = ChannelConfiguration.DefaultPs.GetEntityById<Answer>(nextQuestionId);
            return MessageSender.SendAnswer(answer, msg);
//            Content c1 = GetContentFromAnswer(msg, answer);
//            if (c1 != null)
//                SendContent(c1, msg, true);
        }

        private string GetCorrectAns(Answer ans)
        {
            //foreach ( DataSetLib.ChannelConfigDS.AnswerRulesRow arr in ans.GetAnswerRulesRows() )
            //{
            //    if ( arr.RulesRow.Name == "QuizCorrectAnswerA" )
            //        return "A";
            //    else if ( arr.RulesRow.Name == "QuizCorrectAnswerB" )
            //        return "B";
            //    else if ( arr.RulesRow.Name == "QuizCorrectAnswerC" )
            //        return "C";
            //}
            return null;
        }

        private void AddQuizAnswer(Guid channelId, Guid abonentId, Guid answerId,
                                   string abonentAnswer, string correctAnswer, DateTime ansTime, out Guid nextQuestionId)
        {
            SqlCommand command = new SqlCommand("services.AddQuizAnswer", CoreClassFactory.Connection);
            command.Parameters.AddWithValue("@ChannelId", channelId);
            command.Parameters.AddWithValue("@AbonentId", abonentId);
            command.Parameters.AddWithValue("@AnswerId", answerId);
            command.Parameters.AddWithValue("@AbonentAnswer", abonentAnswer);
            command.Parameters.AddWithValue("@CorrectAnswer", correctAnswer);
            command.Parameters.AddWithValue("@AnswerTime", ansTime);
            //command.Parameters.Add("@AnswerTime", ansTime);
            command.Parameters.Add("@NextQuestionId", SqlDbType.UniqueIdentifier);
            command.Parameters["@NextQuestionId"].Direction = ParameterDirection.Output;
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();
            if (command.Parameters["@NextQuestionId"].Value == null ||
                command.Parameters["@NextQuestionId"].Value == DBNull.Value)
                nextQuestionId = Guid.Empty;
            else
                nextQuestionId = (Guid) command.Parameters["@NextQuestionId"].Value;
        }

        private Regex GetSubscriptionRegex(Guid channelId)
        {
        	return ChannelConfiguration.DefaultPs.GetEntityById<Channel>(channelId).AnswerMaps.SelectOne("Name","Subscribe").Token.RegularExpressions[0].RegexObject;
        }

        private Cryptany.Core.ConfigOM.Answer GetCurrentAnswer(Guid channelId, Guid abonentId, out bool newlyCreated)
        {
            //SqlCommand command = new SqlCommand("exec services.GetCurrentQuizQuestionId @ChannelId ,@AbonentId , @QuestionId", CoreClassFactory.Connection);
            SqlCommand command = new SqlCommand("services.GetCurrentQuizQuestionId", 
                                                Cryptany.Core.Management.ServicesConfigurationManager.Connection);
            command.Parameters.AddWithValue("@ChannelId", channelId);
            command.Parameters.AddWithValue("@AbonentId", abonentId);
            command.Parameters.Add("@QuestionId", SqlDbType.UniqueIdentifier);
            command.Parameters["@QuestionId"].Direction = ParameterDirection.Output;
            command.Parameters.Add("@NewlyCreated", SqlDbType.Bit);
            command.Parameters["@NewlyCreated"].Direction = ParameterDirection.Output;
            command.CommandType = CommandType.StoredProcedure;
            command.ExecuteNonQuery();

            if (command.Parameters["@NewlyCreated"].Value == null ||
                command.Parameters["@NewlyCreated"].Value == DBNull.Value)
                newlyCreated = false;
            else
                newlyCreated = (bool) command.Parameters["@NewlyCreated"].Value;
            if (command.Parameters["@QuestionId"].Value == null ||
                command.Parameters["@QuestionId"].Value == DBNull.Value)
                return null;
            else
                return ChannelConfiguration.DefaultPs.GetEntityById<Answer>((Guid) command.Parameters["@QuestionId"].Value);
        }

//        private ChannelConfigDS.AnswerMapsRow GetAnswerMap(ChannelConfigDS.RegularExpressionsRow regexRow,
//                                                           Guid channelId)
//        {
//            foreach (ChannelConfigDS.AnswerMapsRow amRow in regexRow.TokensRow.GetAnswerMapsRows())
//            {
//                if (amRow.ChanelID == channelId)
//                    return amRow;
//            }
//            return null;
//        }

//        protected override void ProcessVariantBlock(Message msg, ChannelConfigDS.RegularExpressionsRow regexRow,
//                                                    ChannelConfigDS.AnswerBlocksRow answerBlock)
//        {
//            ChannelConfigDS.AnswersRow[] rows = answerBlock.GetAnswersRows();
//            if (rows.Length == 0)
//                return;
//            Random rnd = new Random();
//            int index = rnd.Next(0, rows.Length);
//            ChannelConfigDS.AnswersRow answer = rows[index];
//            if (!CheckAnswerRules(msg, answer))
//                return;
//            Content c = GetContentFromAnswer(msg, answer);
//            if (c != null)
//                SendContent(c, msg, true);
//        }
    }
}