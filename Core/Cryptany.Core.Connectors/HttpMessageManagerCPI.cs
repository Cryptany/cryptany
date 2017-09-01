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
    public class HttpMessageManagerCPI : HTTPMessageManager
    {
        public override event EventHandler MessageReceived;
        public override event EventHandler MessageSent;
        public override event MessageStateChangedEventHandler MessageStateChanged;
        public override event StateChangedEventHandler StateChanged;

        public HttpMessageManagerCPI(ConnectorSettings cs, ILogger logger)
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
            
                byte[] bytes = Create_POST_request(outputMessage);
                //outputMessage.TariffId = OutputMessage.GetTariffid(ConnectorId, outputMessage.Source);
                UpdateOutboxAdditional(outputMessage.ID, 1);
                AddToOutboxMessageParts(outputMessage.Content.Body, outputMessage.ID, outputMessage.HTTP_UID.ToString(), 1);
                sendOk = HTTPConn.AsyncSend(bytes, HTTPConnector.RequestType.POST, "text/xml");
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
            //try
            //{
            //    XmlDocument doc = new XmlDocument();
            //    doc.LoadXml(dataStr);
            //    if (doc["message"].Attributes["mid"] == null) // content-request
            //    {
            //        XmlAttribute xa = doc.CreateAttribute("mid");
            //        xa.Value = inboxMsgID.ToString();
            //        doc["message"].Attributes.Append(xa);
            //    }
            //    else // delivery response
            //    {
            //        XmlAttribute xa = doc.CreateAttribute("dmid");
            //        xa.Value = doc["message"].Attributes["mid"].Value;
            //        doc["message"].Attributes.Append(xa);
            //    }
            //    using (StringWriter sw = new StringWriter())
            //    {
            //        doc.Save(sw);
            //        postData = sw.ToString();
            //    }
            //}
            //catch (Exception e)
            //{
            //    _evLogger.Log(dataStr + " " + e, System.Diagnostics.EventLogEntryType.Warning);
            //}

            SaveToLog(postData, "Rcv", true);
            string responseString = string.Empty;
            statusCode = 200;
            if (inboxMsgID != Guid.Empty)
            {
                // Create HTTP response with status OK                    
                 // OK
                // Fill response with data

                //if (_HTTPSettings.IsAsyncMode)
                //{

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(dataStr);
                    string rid = doc["message"].Attributes["rid"].Value;
                    string mid = "mid=\"" + inboxMsgID + "\"";
                    if (doc["message"].Attributes["mid"] != null)
                        mid = "dmid=\"" + doc["message"].Attributes["mid"].Value + "\"";

                    responseString =
                        @"<?xml version=""1.0"" encoding=""UTF-8""?><report><status>Accepted</status></report>";
                    string responseLogString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><report rid=\"" + rid + "\" " +
                                               mid + "><status>Accepted</status></report>";
                    SaveToLog(responseLogString, "Snd", true);

                //}
                //else
                //{


                //    if (HTTPConn.OutgoingMessageReady.WaitOne(_HTTPSettings.SyncMode_RespWaitTime, false))
                //    {
                       
                //        if (HTTPConn.OutputMsg != null)
                //        {
                //            // Have got the answer after waiting
                //            if (((OutputMessage)HTTPConn.OutputMsg).InboxMsgID == inboxMsgID)
                //            {
                //                if (responseString.Length > 0)
                //                {
                //                    responseString += "&";
                //                }
                //                responseString +=
                //                    EncodeResponse((OutputMessage)HTTPConn.OutputMsg);
                //            }
                //            else
                //            {
                //                _evLogger.Log(
                //                    "HTTPConnector " + _HTTPSettings.SMSCCode +
                //                    " OnDataReceived method: Request - response sequence is broken!",
                //                    System.Diagnostics.EventLogEntryType.Error);
                //                if (Logger != null)
                //                {
                //                    Logger.Write(
                //                        new LogMessage(
                //                            "HTTPConnector " + _HTTPSettings.SMSCCode +
                //                    " OnDataReceived method: Request - response sequence is broken!",
                //                            LogSeverity.Error));
                //                }
                //            }
                //        }
                //        else
                //        {
                //            statusCode = 503;
                //            // Haven't got the answer after waiting
                            
                //        }
                //        HTTPConn.OutputMsg = null;
                //    }
                //}


                
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
            object[] res = new string[2];
            //string dataStr = _HTTPSettings.DataCoding.GetString(body);
            //try
            //{

            //    XmlDocument doc = new XmlDocument();
            //    doc.LoadXml(dataStr);
            //    res[0] = doc["message"].Attributes["rid"].Value;
            //    res[1] = doc["message"].Attributes["mid"].Value;


            //}
            //catch (Exception e)
            //{

            //    _evLogger.Log(dataStr + " " + e, System.Diagnostics.EventLogEntryType.Error);
            //}
            return res;
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
            
                //try
                //{
                //    XmlDocument doc = new XmlDocument();
                //    doc.LoadXml(dataStr);
                //    XmlAttribute ridA = doc.CreateAttribute("rid");
                //    ridA.Value = (string)rs.parameters[0];
                //    doc["report"].Attributes.Append(ridA);
                //    XmlAttribute midA = doc.CreateAttribute("mid");
                //    midA.Value = (string)rs.parameters[1];
                //    doc["report"].Attributes.Append(midA);
                //    using (StringWriter sw = new StringWriter())
                //    {
                //        doc.Save(sw);
                //        SaveToLog(sw.ToString(), "Rcv", true);
                //    }
                //}
                //catch (Exception e)
                //{

                //    _evLogger.Log(e.ToString(), System.Diagnostics.EventLogEntryType.Error);
                //}
            
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
                    
                    xmltw.WriteStartDocument();
                    xmltw.WriteStartElement("message");
                    xmltw.WriteAttributeString("rid", message.HTTP_UID.Trim());
                    xmltw.WriteAttributeString("mid", message.ID.ToString());
                    xmltw.WriteAttributeString("paid", "true");
                    xmltw.WriteAttributeString("bearer", "SMS");
                    xmltw.WriteStartElement("sin");
                    xmltw.WriteString(message.Destination);
                    xmltw.WriteEndElement();
                    xmltw.WriteStartElement("body");
                    xmltw.WriteAttributeString("content-type", @"text/plain");
                    xmltw.WriteString(msgText);
                    xmltw.WriteEndElement(); // body
                    xmltw.WriteEndElement(); // message
                    xmltw.WriteEndDocument();
                    xmltw.Flush();

                }
                byte[] bytes = ms.ToArray();
                result = _HTTPSettings.DataCoding.GetString(bytes);
            }
            int idx = result.IndexOf("<?");
            if (idx > 0)
                result = result.Substring(idx, result.Length - idx);
            return result;
        }

        public override HTTPMessageParameters DecodeRequest(string requestStr)
        {
            
            HTTPMessageParameters mp = new HTTPMessageParameters();
            XmlDocument xd = new XmlDocument();
          
            xd.LoadXml(requestStr);
            if (xd["message"]["service"].InnerText == "content-request") // user request message
            {
                mp.UID = xd["message"].Attributes["rid"].Value;
                mp.Protocol = "";
                mp.SN = xd["message"]["sn"].InnerText;
                mp.MSISDN = xd["message"]["sin"].InnerText;
                mp.Operator = "";
                mp.Text = xd["message"]["body"].InnerText;
                mp.Category = "";
                mp.DeliveryStatus = "";
            }
            else // delivery status message
            {
                mp.UID = xd["message"].Attributes["rid"].Value;
                mp.MID = new Guid(xd["message"].Attributes["mid"].Value);

                MessageDeliveryStatus mds = MessageDeliveryStatus.Unknown;
                string state = xd["message"]["status"].InnerText;
                try
                {
                    mds = (MessageDeliveryStatus)Enum.Parse(typeof(MessageDeliveryStatus), state,
                                       true);

                    
                }
                catch
                {
                    if (state.Equals("Rejected", StringComparison.InvariantCultureIgnoreCase))
                        mds = MessageDeliveryStatus.LowBalance;
                    if (state.Equals("Undeliverable", StringComparison.InvariantCultureIgnoreCase))
                        mds = MessageDeliveryStatus.Undelivered;
                }
                mp.DeliveryStatus = mds.ToString();// Convert.ToString(xd["delivery-state"].Attributes["status"].Value);
                
                if (xd["message"]["status"].Attributes.GetNamedItem("error") != null)
                    mp.DeliveryStatusDescription = xd["message"]["status"].Attributes["error"].Value;
            }
          
            return mp;
        }
        
    }
}
