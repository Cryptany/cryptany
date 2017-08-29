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
using Cryptany.Core.DPO.MetaObjects.Attributes;
using Cryptany.Core.DPO;
using Cryptany.Core.DPO.MetaObjects;

namespace Cryptany.Core.ConfigOM.Quiz
{
	[Serializable]
	[DbSchema("services")]
	[Table("Channels", "ServiceID", ConditionOperation.Equals, "253B0FD6-B864-4CCC-BC96-29F129393971")]
	public class QuizChannel2 : TvadChannel
	{
	}

    [Serializable]
	[DbSchema("services")]
	[Table("Channels", "ServiceID", ConditionOperation.Equals, "253B0FD6-B864-4CCC-BC96-29F129393971")]
	[WrappedClass(typeof(QuizChannel2))]
    public class QuizChannel : Channel, IWrapObject
    {
        private List<Answer> _questions = new List<Answer>();
        private List<AnswerMap> _answers = new List<AnswerMap>();
        private TvadChannel ch;

        public QuizChannel()
        {
        }

        [NonPersistent]
        public List<Answer> Questions
        {
            get
            {
                return _questions;
            }
        }

        [NonPersistent]
        public List<AnswerMap> Answers
        {
            get
            {
                return _answers;
            }
        }

        [NonPersistent]
        public Token MainRequest
        {
            get
            {
                return GetValue<Token>("MainRequest");
            }
            set
            {
                SetValue("MainRequest", value);
            }
        }

        #region IWrapObject Members
        EntityBase IWrapObject.WrappedObject
        {
            get
            {
				if (ch == null)
					ch = ClassFactory.CreateObject<TvadChannel>(CreatorPs);
				else if (ch.CreatorPs == null)
					ch.CreatorPs = CreatorPs;
                ch.ID = ID;
                ch.Name = Name;
                AnswerMap amQuestions = null;
                AnswerMap amSubscription = null;
                foreach ( AnswerMap amm in ch.AnswerMaps )
                {
                    if ( amm.Name == "Questions" )
                        amQuestions = amm;
                    else if ( amm.Name == "Subscribe" )
                        amSubscription = amm;
                }
                ch.AnswerMaps.Clear();
                if ( amSubscription == null )
                {
					amSubscription = ClassFactory.CreateObject<AnswerMap>(CreatorPs);
                    amSubscription.Token = MainRequest;
                    amSubscription.Name = "Subscribe";
                    amSubscription.State = EntityState.New;
                    amSubscription.Token = MainRequest;
                    if ( MainRequest != null )
                    {
                        MainRequest.AnswerMaps.Add(amSubscription);
                        MainRequest.Name = MainRequest.Name;
                    }
                    //ch.AnswerMaps.Add(amSubscription);
                }
                ch.AnswerMaps.Add(amSubscription);


				if (amQuestions == null)
				{
					amQuestions = ClassFactory.CreateObject<AnswerMap>(CreatorPs);
					amQuestions.Name = "Questions";
					amQuestions.State = EntityState.New;
				}
				if (amQuestions.AnswerBlocks.Count == 0)
				{
					amQuestions.AnswerBlocks.Add(new AnswerBlock());
					amQuestions.AnswerBlocks[0].State = EntityState.New;
					amQuestions.AnswerBlocks[0].Map = amQuestions;
					List<EntityBase> l = null;
					if (CreatorPs != null)
						l = CreatorPs.GetEntitiesByFieldValue(typeof(AnswerBlockType), "Name", "QuizBlock");
					else
						l = ClassFactory.GetThreadDefaultPs(System.Threading.Thread.CurrentThread).GetEntitiesByFieldValue(typeof(AnswerBlockType), "Name", "QuizBlock");
					AnswerBlockType btype = null;
					if (l == null || l.Count == 0)
					{
						btype = ClassFactory.CreateObject<AnswerBlockType>(CreatorPs);
						btype.Name = "QuizBlock";
						btype.ID = Guid.NewGuid();
						btype.State = EntityState.New;
					}
					else
						btype = (AnswerBlockType)l[0];
					amQuestions.AnswerBlocks[0].BlockType = btype;
				}
                ch.AnswerMaps.Add(amQuestions);
                foreach ( AnswerMap am in Answers )
                {
                    ch.AnswerMaps.Add(am);
                }
                amQuestions.AnswerBlocks[0].Answers.Clear();
                foreach ( Answer ans in Questions )
                {
                    //ans.State = EntityState.New;
                    amQuestions.AnswerBlocks[0].Answers.Add(ans);
                }
				ch.ContragentResource = ContragentResource;
                ch.Service = Service;
                ch.State = State;
                foreach ( Rule rule in Rules )
                {
                    ch.Rules.Add(rule);
                }
                return ch;
                //throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                ch = value as TvadChannel;
                this.ID = ch.ID;
                Name = ch.Name;
				ContragentResource = ch.ContragentResource;
                Service = ch.Service;
                State = ch.State;
                foreach ( Rule rule in ch.Rules )
                {
                    Rules.Add(rule);
                }
                foreach ( AnswerMap am in ch.AnswerMaps )
                {
                    if ( am.Name == "Subscribe" )
                    {
                        MainRequest = am.Token;
                    }
                    else if ( am.Name == "Questions" )
                    {
                        foreach ( Answer ans in am.AnswerBlocks[0].Answers )
                        {
                            _questions.Add(ans);
                        }
                    }
                    else
                    {
                        Answers.Add(am);
                    }
                }
                //throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion
    }
}
