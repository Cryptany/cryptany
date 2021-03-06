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
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Resources;
using Cryptany.Core.Interaction;
using Cryptany.Core.Management.WMI;
using System.IO;
using System.Xml;
using Cryptany.Common.Logging;
using Cryptany.Core;

namespace Cryptany.Core.Connectors
{
    public class HttpMessageManagerInfonXML : HTTPMessageManager
    {
        public override event EventHandler MessageReceived;
        public override event EventHandler MessageSent;
        public override event MessageStateChangedEventHandler MessageStateChanged;
        public override event StateChangedEventHandler StateChanged;
        public override event EventHandler RequireReinit;

        public HttpMessageManagerInfonXML(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {

        }

        public override bool SendUserData(OutputMessage outputMessage, byte[] userData)
        {
            bool sendOk = false;
            if (userData.Length > 0) // короткое сообщение 
            {
                if (outputMessage.ID != Guid.Empty)
                {
                    SendMessageID = outputMessage.ID;
                }
            
                //byte[] bytes = Create_POST_request(outputMessage);
                //sendOk = HTTPConn.AsyncSend(bytes, HTTPConnector.RequestType.POST, "text/xml");
                HTTPConn.OutputMsg = outputMessage;
                outputMessage.TariffId = OutputMessage.GetTariffid(ConnectorId, outputMessage.Source);
                UpdateOutboxAdditional(outputMessage.ID, outputMessage.TariffId, 1);
                AddToOutboxMessageParts(outputMessage.Content.Body, outputMessage.ID, outputMessage.HTTP_UID.ToString(), 1);
                HTTPConn.OutgoingMessageReady.Set();
                // реальная отправка выполняется в HTTPConnector
                sendOk = true;
                if (sendOk)
                {
                    
                    MessageSent(this, new EventArgs());
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Connected, ""));
                }
            }
            
            return sendOk;
        }
        protected override byte[] Create_POST_request(OutputMessage outputMessage)
        {
            string postString = EncodeResponse(outputMessage);
            byte[] bytes = _HTTPSettings.DataCoding.GetBytes(postString);
            return bytes;
        }
        
        public override byte[] ProcessHttpRequest(NameValueCollection queryString, Stream inputStream, out int statusCode)
        {
            string dataStr = string.Empty;
            
            StreamReader reqSr = new StreamReader(inputStream, _HTTPSettings.DataCoding);
            dataStr = reqSr.ReadToEnd();
                            // Save received data 
            string postData = dataStr;
            Guid inboxMsgID = Receive(HTTPConn, dataStr);
            
            SaveToLog(postData, "Rcv", true);
            string responseString = string.Empty;
            statusCode = 200;
            if (inboxMsgID != Guid.Empty)
            {
                // Create HTTP response with status OK                    
                 // OK
                // Fill response with data

               


                    if (HTTPConn.OutgoingMessageReady.WaitOne(_HTTPSettings.SyncMode_RespWaitTime, false))
                    {
                       
                        if (HTTPConn.OutputMsg != null)
                        {
                            // Have got the answer after waiting
                            if (((OutputMessage)HTTPConn.OutputMsg).InboxMsgID == inboxMsgID)
                            {
                                responseString = EncodeResponse((OutputMessage)HTTPConn.OutputMsg);
                                //Logger.Write(new LogMessage("Получили верный ответ от роутера",LogSeverity.Error));
                            }
                            else
                            {
                                
                                _evLogger.Log(
                                    "HTTPConnector " + _HTTPSettings.SMSCCode +
                                    " OnDataReceived method: Request - response sequence is broken!",
                                    System.Diagnostics.EventLogEntryType.Error);
                                if (Logger != null)
                                {
                                    Logger.Write(
                                        new LogMessage(
                                            "HTTPConnector " + _HTTPSettings.SMSCCode +
                                    " OnDataReceived method: Request - response sequence is broken!",
                                            LogSeverity.Error));
                                }
                                try
                                {

                                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error,"request - response sequence is broken"));
                                }
                                catch
                                { }
                                try
                                {
                                    
                                    RequireReinit(this, new EventArgs());
                                }
                                catch
                                { }
                            }
                        }
                        else
                        {
                            statusCode = 503;
                            Logger.Write(new LogMessage("Не получили ответ от роутера вовремя", LogSeverity.Error));
                            // Haven't got the answer after waiting
                            if (HTTPMsgParams != null)
                            {
                                OutputMessage outputMsg = new OutputMessage();
                                outputMsg.ID = Guid.NewGuid();
                                outputMsg.Source = HTTPMsgParams.SN;
                                outputMsg.Destination = HTTPMsgParams.MSISDN;
                                outputMsg.HTTP_Category = "";
                                outputMsg.HTTP_UID = HTTPMsgParams.UID;
                                outputMsg.HTTP_Protocol = HTTPMsgParams.Protocol;
                                outputMsg.HTTP_Operator = HTTPMsgParams.Operator;
                                outputMsg.HTTP_DeliveryStatus = "";
                                outputMsg.Content = new TextContent("");
                                outputMsg.IsPayed = false;
                                responseString = EncodeResponse(outputMsg);
                            }

                        }
                        HTTPConn.OutputMsg = null;
                    }
                

                
            }
            else
            {
                // Create HTTP response with status ERROR
                statusCode = 400; // Bad request
            }
            return _HTTPSettings.DataCoding.GetBytes(responseString);
            
        }

        public override object[] PrepareParams(byte[] body)
        {
            return null;
        }
        public override void ProcessWebResponse(HttpWebResponse response, RequestState rs)
        {
            HttpWebRequest req = rs.request;
            // Save response data 
            string dataStr;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                SaveToLog("Получили ответ HTTP 200 OK.", "Rcv", true);
            }
            else
            {
                SaveToLog("Получили ответ HTTP " + response.StatusCode + " " + response.StatusDescription, "Rcv", true);
            }
            Stream resStream = response.GetResponseStream();

            using (StreamReader resSr = new StreamReader(resStream, _HTTPSettings.DataCoding))
            {
                dataStr = resSr.ReadToEnd();
            }
            
            SaveToLog(dataStr, "Rcv", true);
        }
        public override string EncodeResponse(OutputMessage message)
        {
            string result;
            string msgText = ((TextContent)message.Content).MsgText;
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlTextWriter xmltw = new XmlTextWriter(ms, _HTTPSettings.DataCoding))
                {

                    if (message.HTTP_DeliveryStatus == "") // user request message
                    {
                        if (msgText.Length == 0) // текст ответного сообщения пустой
                        {
                            //Logger.Write(new LogMessage("Отвечаем пустым сообщением", LogSeverity.Error));
                            xmltw.WriteStartDocument();
                            xmltw.WriteStartElement("response");
                            xmltw.WriteAttributeString("uid", message.HTTP_UID.Trim());
                            xmltw.WriteAttributeString("protocol", message.HTTP_Protocol.Trim());
                            xmltw.WriteString(@"<noreply/>");
                            xmltw.WriteEndElement();
                            xmltw.WriteEndDocument();
                            xmltw.Flush();
                        }
                        else // текст ответного сообщения не пустой
                        {
                            //Logger.Write(new LogMessage("Отвечаем сообщением с контентом", LogSeverity.Error));
                            xmltw.WriteStartDocument();
                            xmltw.WriteStartElement("response");
                            xmltw.WriteAttributeString("uid", message.HTTP_UID.Trim());
                            xmltw.WriteAttributeString("protocol", message.HTTP_Protocol.Trim());
                            xmltw.WriteStartElement("message");
                            xmltw.WriteAttributeString("number", "1");
                            xmltw.WriteStartElement("abonent");
                            xmltw.WriteAttributeString("isnn", message.Source.Trim());
                            xmltw.WriteAttributeString("phone", message.Destination.Trim());
                            xmltw.WriteAttributeString("operator", message.HTTP_Operator.Trim());
                            xmltw.WriteEndElement(); // abonent
                            xmltw.WriteStartElement("content-text");
                            xmltw.WriteAttributeString("content-type", @"text/plain");
                            xmltw.WriteString(msgText);
                            xmltw.WriteEndElement(); // content-text
                            xmltw.WriteEndElement(); // message
                            xmltw.WriteEndElement(); // response
                            xmltw.WriteEndDocument();
                            xmltw.Flush();
                        }
                    }
                    else // delivery status message
                    {
                        //Logger.Write(new LogMessage("Отвечаем на доставку сообщения", LogSeverity.Error));
                        xmltw.WriteStartDocument();
                        xmltw.WriteStartElement("ok");
                        xmltw.WriteAttributeString("uid", message.HTTP_UID.Trim());
                        xmltw.WriteEndElement();
                        xmltw.WriteEndDocument();
                        xmltw.Flush();
                    }

                }
                byte[] bytes = ms.ToArray();
                result = _HTTPSettings.DataCoding.GetString(bytes);
            }
            return result;
        }

        public override HTTPMessageParameters DecodeRequest(string requestStr)
        {
           
            HTTPMessageParameters mp = new HTTPMessageParameters();
            XmlDocument xd = new XmlDocument();

            xd.LoadXml(requestStr);
            if (xd["delivery-state"] == null) // user request message
            {
                mp.UID = Convert.ToString(xd["request"].Attributes["uid"].Value);
                mp.Protocol = Convert.ToString(xd["request"].Attributes["protocol"].Value);
                mp.SN = Convert.ToString(xd["request"]["abonent"].Attributes["isnn"].Value);
                mp.MSISDN = Convert.ToString(xd["request"]["abonent"].Attributes["phone"].Value);
                if (mp.MSISDN == "1008" || mp.MSISDN == "1010" || mp.MSISDN == "1012")
                    mp.MSISDN = "1834";
                mp.Operator = Convert.ToString(xd["request"]["abonent"].Attributes["operator"].Value);
                mp.Text = Convert.ToString(xd["request"]["content-text"].InnerText);
                mp.Category = "";
                mp.DeliveryStatus = "";
            }
            else // delivery status message
            {
                mp.UID = xd["delivery-state"].Attributes["uid"].Value;
                MessageDeliveryStatus mds = MessageDeliveryStatus.Unknown;
                string state = xd["delivery-state"].Attributes["status"].Value;
                try
                {
                    mds = (MessageDeliveryStatus)Enum.Parse(typeof(MessageDeliveryStatus), state,
                                       true);
                    
                    
                }
                catch
                {
                    if (state.Equals("sms-delivered", StringComparison.InvariantCultureIgnoreCase))
                        mds = MessageDeliveryStatus.Delivered;
                    if (state.Equals("Undeliverable", StringComparison.InvariantCultureIgnoreCase))
                        mds = MessageDeliveryStatus.Undelivered;
                }
                mp.DeliveryStatus = mds.ToString();// Convert.ToString(xd["delivery-state"].Attributes["status"].Value);
                mp.DeliveryStatusDescription = state;
            }
            
            return mp;
        }
        
    }
}
