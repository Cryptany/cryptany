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
using System.Data.SqlClient;
using System.IO;
using System.Messaging;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Interaction;
using Cryptany.Core.Management;
using Cryptany.Core.MacrosProcessors;
using Cryptany.Common.Logging;
using Cryptany.Core.ConfigOM.USSD;
using Cryptany.Foundation.Statistics;
using System.Diagnostics;
using Cryptany.Core.Services;
using Cryptany.Core.Services.Management;
using Cryptany.Common.Utils;

namespace Cryptany.Core
{
    /// <summary>
    /// USSD message processing service 
    /// </summary>
    public class USSDService : AbstractService
    {
        private static WAPLogger wl;// = new WAPLogger("redut", @"formatname:direct=os:redut\private$\wapstatistics");
        private static ISMSFeed feed;

        /// <summary>
        /// Конструктор по умолчанию. 
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Возникает вслучае ошибки при создании экземпляра сервиса</exception>
        public USSDService(IRouter router)  : base(router)
        {
           
        }

        public USSDService(IRouter router, string serviceName):base(router,serviceName)
        {
            wl = new WAPLogger("", ServicesConfigurationManager.StatisticsQueue.Path);
            feed = (ISMSFeed)Activator.GetObject(typeof(ISMSFeed), ServicesConfigurationManager.TransportManagerURL);
        }

        #region IService Members

    
        

	
		protected override bool ProcessMessageInner(Message msg, AnswerMap map)
        {
            
			//return false;
            LogContainer lc = new LogContainer();
            lc.RequestTime = DateTime.Now;
            
            lc.ClientAddress = msg.MSISDN;
            lc.AdditionalParams = new List<LogParametrs>();
            Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
            if (ab==null)// не смогли получить абонента по MSISDN
            {
                AnswerMessage("Извините, технические неполадки. Попробуйте зайти позже", msg, false, 0x00, null, null,
                              Guid.Empty);
                return true;
            }
		    AbonentSession session = ab.Session;
            string sessionData = string.Empty;
            string backCard = string.Empty;
            try
            {
                //  Сохранить сообщение, помещенное во входящую очередь Роутера, в базе данных
               Router.AddToInbox(msg);
                Logger.Write(new LogMessage("Getting session state for abonent " + msg.MSISDN + " on service number " + msg.ServiceNumberString, LogSeverity.Notice));

                
                //Debug.WriteLine("USSD: Получаем текущую карточку абонента");
                Debug.WriteLine("USSD: Получаем предыдущую карточку абонента");
                if (!string.IsNullOrEmpty(session["ussdCard" + msg.ServiceNumberString]))
                    sessionData = session["ussdCard" + msg.ServiceNumberString];
             
                
                if (!string.IsNullOrEmpty(sessionData))
                {
                    backCard = sessionData;
                    Card prevCard = Card.GetByName(sessionData);
                    if (prevCard != null)
                    {
                        if (prevCard.Items.ContainsKey(msg.Text))
                        {
                            string text = prevCard.Items[msg.Text];
                            if (!string.IsNullOrEmpty(text))
                            {
                                LogParametrs lp = new LogParametrs();
                                lp.Name = "Text";
                                Debug.WriteLine("USSD: Текст пункта меню: " + text);
                                lp.Value = text.Trim();
                                lc.AdditionalParams.Add(lp);
                            }
                        }
                    }
                }
                //Logger.Write(new LogMessage("Выставляем предыдущую карточку" + (string.IsNullOrEmpty(sessionData)?"":sessionData), LogSeverity.Notice));
                //if (!string.IsNullOrEmpty(sessionData)) session["ussdBackCard" + msg.SN.Number] = sessionData;
                Logger.Write(new LogMessage("Проверяем нужна ли новая сессия", LogSeverity.Notice));
                Card c;
                
                if (msg.Text == "RENEWSESSION" || string.IsNullOrEmpty(sessionData))
                {

                    Logger.Write(new LogMessage("New session required for abonent " + msg.MSISDN + " on service number " + msg.ServiceNumberString, LogSeverity.Notice));
                    session["sessionId" + msg.ServiceNumberString] = Guid.NewGuid().ToString();
                    
                    // no data, get default card
                    backCard = "";
                    session["ussdCard" + msg.ServiceNumberString] = "";
                    //object m = snToCardId[msg.SN.Number];
                    c = ServiceCard.GetCardByShortNumber(msg.ServiceNumberString);
                    
                    sessionData = c == null ? "default" : c.Name;
                    ab.Session["ussdCard" + msg.ServiceNumberString] = sessionData;
                    Logger.Write(new LogMessage("New session, getting default card: " + sessionData, LogSeverity.Notice));
                    //currentCardList = (UssdDS.CardsRow[])cardsData.Cards.Select("name = '" + sessionData + "'");
                    c = Card.GetByName(sessionData);
                    if (c==null)
                    {
                        send_next_card(ab,msg, "Извините, технические неполадки. Попробуйте зайти позже", sessionData, null);
                        return true;
                    }
                    session["ContragentResource" + msg.ServiceNumberString] = ServiceCard.GetSaleChannelByShortNumber(msg.ServiceNumberString).ContragentResource.Name;
                    session["ContragentResourceId" + msg.ServiceNumberString] = ServiceCard.GetSaleChannelByShortNumber(msg.ServiceNumberString).ContragentResource.ID.ToString();

                    send_next_card(ab,msg, c.Text, sessionData, c);
                    return true;
                }

               

                
                
                // check for actions
                //currentCardList = (UssdDS.CardsRow[])cardsData.Cards.Select("name = '" + sessionData + "'");
                Logger.Write(new LogMessage("Getting the card: " + sessionData, LogSeverity.Notice));
                c = Card.GetByName(sessionData);
                if (c==null)
                {
                    // card not found, send default card
                    //sessionData = (string)snToCardId[msg.SN.Number];
                    Logger.Write(new LogMessage("Current card not found, getting default card: " + c.Name,
                                                    LogSeverity.Notice));
                    c = ServiceCard.GetCardByShortNumber(msg.ServiceNumberString);
                    if (c != null)
                    {

                        
                        
                        send_next_card(ab,msg, c.Text, c.Name, c);
                        return true;
                    }
                }
                else
                {
                    // card found
                    //UssdDS.CardsRow currentRow = currentCardList[0];
                    //UssdDS.ActionsRow[] actionList =
                    //    (UssdDS.ActionsRow[])
                    //    cardsData.Actions.Select("cardid = '" + currentRow.id + "' and key = '" + msg.Text + "'");
                    List<CardOperation> actions = CardOperation.GetByCardAndKey(c, msg.Text);
                    
                    if (actions.Count == 0) // send the same card
                    {
                        Logger.Write(new LogMessage("No next action found, returning the same card: " + sessionData,
                                                    LogSeverity.Notice));
                        send_next_card(ab,msg, c.Text, sessionData, c);
                        return true;
                    }
                    else
                    {
                        foreach (CardOperation action in actions)
                        {
                            // parse action content
                            Logger.Write(new LogMessage("Action found: " + action.Operation.Type.ID + " " + action.Operation.Type.Name, LogSeverity.Notice));
                            //UssdDS.ActionsRow aRow = actionList[0];
                            switch (action.Operation.Type.ID.ToString().ToLower())
                            {
                                case "5beb9efc-e412-4c34-a5ae-c8e5748a5372": // send_next_card
                                    {
                                        
                                        string cardid = action.Operation.GetParameterValueByCode("cardid");
                                        if (cardid != string.Empty)
                                        {
                                            Card linkCard = Card.GetByID(new Guid(cardid));
                                            if (linkCard != null)
                                            {
                                                send_next_card(ab, msg, linkCard.Text, linkCard.Name,
                                                               linkCard);
                                            }
                                            else
                                            {
                                                Logger.Write(new LogMessage("Не найдена карточка с id: " + cardid, LogSeverity.Error));
                                            }
                                        }
                                        else
                                        {
                                            Logger.Write(new LogMessage("Не задан параметр cardid", LogSeverity.Error));
                                        }
                                        

                                    }
                                    break;
                                case "cc008451-cf23-dc11-83ef-0030488b09dd": // pay content
                                    {
                                        string content = action.Operation.GetParameterValueByCode("content");
                                        //string cardid = action.Operation.GetParameterValueByCode("cardid");
                                        //Card linkCard = null;
                                        //if (cardid!=string.Empty)
                                        //{
                                        //    linkCard = Card.GetByID(new Guid(cardid));
                                        //}
                                        string tariff = action.Operation.GetParameterValueByCode("tariff");
                                        if (content != string.Empty && tariff != string.Empty)// && linkCard!=null)
                                        {
                                            //send_next_card(msg, linkCard.Text, linkCard.Name, linkCard);
                                            Guid dldid = send_payment(msg, content, tariff);
                                            LogParametrs lp = new LogParametrs();
                                            lp.Name = "DownloadId";
                                            lp.Value = dldid.ToString();
                                            lc.AdditionalParams.Add(lp);
                                            //send_wap_link(msg, url);
                                        }
                                    }
                                    break;
                                case "cec1f344-cf23-dc11-83ef-0030488b09dd": // wap link
                                    string url = action.Operation.GetParameterValueByCode("url");
                                    if (url != string.Empty)
                                    {
                                        send_wap_link(ab,msg, url);
                                    }
                                    
                                    break;
                                case "a31fe907-379d-de11-8024-0030488b09dd": // подписка на клуб
                                    {
                                        string sn = action.Operation.GetParameterValueByCode("sn");
                                        string text = action.Operation.GetParameterValueByCode("text");
                                        string clubid = action.Operation.GetParameterValueByCode("clubid");
                                        LogParametrs lp = new LogParametrs();
                                        lp.Name = "ClubId";
                                        lp.Value = clubid;
                                        lc.AdditionalParams.Add(lp);
                                        if (sn != string.Empty && text != string.Empty && clubid != string.Empty)
                                        {

                                            subscribe_club(ab,msg, sn, text,
                                                           new Guid(session[
                                                                   "ContragentResourceId" + msg.ServiceNumberString]),
                                                           new Guid(clubid));
                                        }
                                    }
                                    break;
                                //case "ce97a032-71f7-dd11-a400-0030488b09dd": // оплата контента и подписка на ЭроGold
                                //    {
                                //        //string content = action.Operation.GetParameterValueByCode("content");
                                //        //string tariff = action.Operation.GetParameterValueByCode("tariff");
                                //        //if (content != string.Empty && tariff != string.Empty)
                                //        //{
                                //        //    send_payment(msg, content, tariff);
                                //        //    //send_wap_link(msg, url);
                                //        //}
                                //        string sn = action.Operation.GetParameterValueByCode("sn");
                                //        string text = action.Operation.GetParameterValueByCode("text");
                                //        string clubid = action.Operation.GetParameterValueByCode("clubid");
                                //        if (sn!=string.Empty && text!=string.Empty && clubid!=string.Empty)
                                //        {
                                //            subscribe_club(msg, sn, text,
                                //                           new Guid(
                                //                               msg.Abonent.Session[
                                //                                   "ContragentResourceId" + msg.SN.Number]),
                                //                           new Guid(clubid));
                                //        }
                                //        //subscribe_club(msg,
                                //        //               "Dlya tebya! EroGold! Paket novih igr, melodiy kajduyu nedelyu! Tel. podderjki  (495) 5457225. Otkaz - SMS s tekstom EROGOLD na 770643. Cena za paket vsego 161 rub.(s NDS)",
                                //        //               new Guid(
                                //        //                   msg.Abonent.Session["ContragentResourceId" + msg.SN.Number]),
                                //        //               new Guid("b513d4e3-b658-4268-9a72-8dea3e230d87"));
                                //    }
                                //    break;

                                //case "cd97a032-71f7-dd11-a400-0030488b09dd": //оплата контента и подписка на Эроклуб
                                //    send_payment(msg, action.Content.ToString(), action.Tariff.ToString());
                                //    subscribe_club(msg, "", new Guid("768cf94c-a687-4adb-a51c-06a493f42e39"));
                                //    break;
                                case "cf0152df-9c25-dc11-83ef-0030488b09dd": // link back
                                    //ccL = (UssdDS.CardsRow[])cardsData.Cards.Select("name = '" + backCard + "'");
                                    c = Card.GetByName(backCard);
                                    send_next_card(ab,msg, c.Text, c.Name, c);
                                    break;
                                case "cb008451-cf23-dc11-83ef-0030488b09dd": // link up
                                    //ccL = (UssdDS.CardsRow[])currentRow.GetChildRows("Cards_Cards");
                                    //ccL = (UssdDS.CardsRow[])cardsData.Cards.Select("id = '" + ccL[0].parentid + "'");
                                    send_next_card(ab,msg, c.Parent.Text, c.Parent.Name, c.Parent);
                                    break;
                                //case "99b70341-9a25-dc11-83ef-0030488b09dd": // exit card
                                //    {
                                //        string text = action.Operation.GetParameterValueByCode("cardid");
                                //        send_exit_card(msg, text);
                                //    }
                                //    break;

                                //case "7a536c4c-9a25-dc11-83ef-0030488b09dd": // main card
                                //    {
                                //        string text = action.Operation.GetParameterValueByCode("cardid");
                                //        send_mainsystem_card(msg, text);
                                //    }
                                //    break;

                                default:
                                    send_mainsystem_card(ab,msg, "Спасибо за использование ресурса!");
                                    break;
                            }
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage("Exception in USSDService ProcessMessage method: " + ex, LogSeverity.Error));

            }
            finally
            {
                //запишем статистику в очередь
                
                try
                {

                    lc.ApplicationName = session["ContragentResource" + msg.ServiceNumberString];
                    string sessionid = session["sessionId" + msg.ServiceNumberString];

                    lc.Session = string.IsNullOrEmpty(sessionid) ? "" : sessionid;
                    LogParametrs lp = new LogParametrs();
                    lp.Name = "PrevCard";
                    //string prevcard = session["ussdBackCard" + msg.SN.Number];
                    lp.Value = backCard;
                    
                    lc.AdditionalParams.Add(lp);
                    lp = new LogParametrs();
                    lp.Name = "Card";
                    string card = session["ussdCard" + msg.ServiceNumberString];
                    lp.Value = string.IsNullOrEmpty(card) ? "" : card;
                    lc.AdditionalParams.Add(lp);
                    lp = new LogParametrs();
                    lp.Name = "Key";
                    lp.Value = msg.Text;
                    lc.AdditionalParams.Add(lp);
                    WAPLogger.SendLog(wl, lc);
                }
                catch(Exception ex)
                {
                    Logger.Write(new LogMessage("Exception in USSDService ProcessMessage method: " + ex, LogSeverity.Error));
                }

            }
            return true;
        }

        private void subscribe_club(Abonent ab, Message msg,string serviceNumber,  string Text,  Guid resourceId, Guid clubId)
        {
            //throw new Exception("The method or operation is not implemented.");
            string errText = string.Empty;
            try
            {
                Guid smsid = feed.FeedSMSText(serviceNumber, msg.MSISDN, Text, resourceId, out errText);
                if (smsid!=Guid.Empty) // можем подписывать
                {
                    SubscriptionMessage sm = new SubscriptionMessage();
                    sm.abonentId = ab.DatabaseId;
                    sm.MSISDN = msg.MSISDN;
                    sm.actionType = SubscriptionActionType.Subscribe;
                    sm.resourceId = resourceId;
                    sm.smsId = msg.InboxId;
                    sm.actionTime = DateTime.Now;
                    sm.clubs.Add(clubId);
                    using (MessageQueue mq = ServicesConfigurationManager.SubscriptionQueue)
                    {

                        mq.Send(sm);
                    }
                }
                else
                {
                    Logger.Write(new LogMessage("Exception in USSDService subscribe_club method - send sms: " + errText, LogSeverity.Error));
                }
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage("Exception in USSDService subscribe_club method: " + ex, LogSeverity.Error));
            }

        }

       
        #endregion
      

        private void send_next_card(Abonent ab, Message msg, string cardText, string cardName, Card card)
        {
            ab.Session["ussdCard" + msg.ServiceNumberString] = cardName;
            AnswerMessage(cardText, msg, false, 0x00, null, null, Guid.Empty);
        }

        private void send_mainsystem_card(Abonent ab,Message msg, string text)
        {
            ab.Session["ussdCard" + msg.ServiceNumberString] = "";
            AnswerMessage(text, msg, false, 0x00, null, null, Guid.Empty);
        }

        private Guid send_payment(Message msg, string contentCode, string tariff)
        {
            Guid tariffId = Guid.Empty;
            Tariff t = TariffBinding.GetTariffByCategory(int.Parse(tariff));
            if (t != null)
                tariffId = (Guid)t.ID;
            Guid scId=Guid.Empty;// = new Guid(snToSC[msg.SN.Number].ToString());
            SaleChannel sc = ServiceCard.GetSaleChannelByShortNumber(msg.ServiceNumberString);
            if (sc != null)
                scId = (Guid)sc.ID;
            
            Guid downloadId;
            string linkStr = ContentProcessor.GetContentDownloadLink(int.Parse(contentCode), scId, tariffId, msg.MSISDN, out downloadId);
            AnswerMessage(linkStr, msg, true, 0x01, "CAT" + tariff.Trim() + "_", 0x00, downloadId);
            return downloadId;
        }

        private void send_wap_link(Abonent ab,Message msg, string linkUrl)
        {
            ab.Session["ussdCard" + msg.ServiceNumberString] = "";
            AnswerMessage(linkUrl, msg, false, 0x00, null, null, Guid.Empty);
        }

       

        private Guid AnswerMessage(string answer, Message msg, bool IsPayed, byte USSD_Charging,
                                   string USSD_MessageContentType, byte? USSD_DialogDirective, Guid downloadId)
        {
            try
            {
                TextContent tc = new TextContent(answer);
                //throw new NotImplementedException();
                return SendContent(msg, tc, Logger, IsPayed, (uint)msg.USSD_UserMessageReference, USSD_Charging,
                                USSD_MessageContentType, USSD_DialogDirective, downloadId);
                
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage("Exception in USSDService AnswerMessage method: " + ex, LogSeverity.Error));
                return Guid.Empty;
            }
        }

        /// <summary>
        /// For USSD only: 
        /// Sends content (Binary/text/wap etc)
        /// Creates outgoing message, adds record 
        /// to outbox and puts outgoing message to outgoing queue.
        /// </summary>
        /// <param name="contentToSend"></param>
        /// <param name="logger"></param>
        public Guid SendContent(Message msg, Content contentToSend, ILogger logger, bool IsPayed, uint USSD_UserMessageReference,
                                byte USSD_Charging, string USSD_MessageContentType, byte? USSD_DialogDirective, Guid downloadId)
        {
            try
            {
                SMSC s = SMSC.GetSMSCById(msg.SMSCId);
                MessageQueue queue = ServicesConfigurationManager.GetOutgoingMessageQueue(s);
                if (queue != null)
                {
                    OutputMessage outMsg = new OutputMessage();
                    
                    outMsg.ID = IdGenerator.NewId;
                    outMsg.TransactionId = "";
                    outMsg.InboxMsgID = msg.InboxId;
                    outMsg.Source = msg.ServiceNumberString;
                    outMsg.Destination = msg.MSISDN;
                    logger.Write(new LogMessage("USSD: Message is sending content", LogSeverity.Debug));
                    if (contentToSend is LinkToContent)
                    {
                        TextContent txtContent =
                            new TextContent("Spasibo za pokupku! Skachaite vash zakaz zdes': " +
                                            (contentToSend as LinkToContent).ContentURL);
                        outMsg.Content = txtContent;
                        outMsg.IsPayed = true;
                    }
                    else
                    {
                        outMsg.Content = contentToSend;
                        outMsg.IsPayed = false;
                    }
                    // Additional USSD parameters
                    outMsg.IsPayed = IsPayed;
                    outMsg.USSD_UserMessageReference = USSD_UserMessageReference;
                    outMsg.USSD_Charging = USSD_Charging;
                    outMsg.USSD_MessageContentType = USSD_MessageContentType;
                    outMsg.USSD_DialogDirective = USSD_DialogDirective;
                    outMsg.TTL = DateTime.Now.AddMinutes(1);
                    //  Сохранить сообщение, помещенное в исходящую очередь Коннектора, в базе данных
                    Cryptany.Core.Connectors.Management.ConnectorManager.AddToOutbox(outMsg);
                    if (downloadId != Guid.Empty)
                    {
                        ActionExecuter.AddToDownloadOutbox(outMsg.ID, downloadId);
                    }
                    Trace.Write("USSD: Message sending:" + outMsg);
                    queue.Send(outMsg);

                    Trace.Write("USSD: Message sent to queue:" + queue.Path);
                    return outMsg.ID;

                }
                else
                {
                    logger.Write(
                        new LogMessage(
                            "Can't send message. Failed to connect to outgoing queue. SMSC id: " + msg.SMSCId,
                            LogSeverity.Error));
                    return Guid.Empty;
                }
            }
            catch (Exception ex)
            {
                logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                return Guid.Empty;
            }
        }
	}
}