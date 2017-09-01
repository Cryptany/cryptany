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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Cryptany.Core.Management.WMI;
using Cryptany.Common.Logging;
using System.Xml;

namespace Cryptany.Core
{
    public class HttpMessageManagerSms2Web : HTTPMessageManager
    {
        public override event EventHandler MessageReceived;
        public override event EventHandler MessageSent;
        public override event MessageStateChangedEventHandler MessageStateChanged;
        public override event StateChangedEventHandler StateChanged;
        public override event EventHandler RequireReinit;

        public HttpMessageManagerSms2Web(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {

        }

        public override bool SendUserData(OutputMessage outputMessage)
        {

            if (outputMessage.Content.Body.Length > 0) // короткое сообщение 
            {
                if (outputMessage.ID != Guid.Empty)
                {
                    SendMessageID = outputMessage.ID;
                }

                HTTPConn.OutputMsg = outputMessage;
                UpdateOutboxAdditional(outputMessage.ID, 1);
                HTTPConn.OutgoingMessageReady.Set();

                    MessageSent(this, new EventArgs());
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Connected, ""));
                    return true;
            }

            return false;
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
            Guid inboxMsgID = Receive(HTTPConn, postData);

            SaveToLog(postData, "Rcv", true);
            string responseString = "";
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
                            responseString +=
                                EncodeResponse((OutputMessage)HTTPConn.OutputMsg);
                    }
                }
                else
                {
                    statusCode = 503;
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
                        outputMsg.Content = new TextContent("Service unavailable");
                        outputMsg.IsPayed = false;
                        responseString = EncodeResponse(outputMsg);
                    }

                    //}
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

            _HTTPSettings.ContentType = "text/html; charset=windows-1251";
            string result;// = ((TextContent)message.Content).MsgText;
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlTextWriter xmltw = new XmlTextWriter(ms, _HTTPSettings.DataCoding))
                {
                        byte[] bdy = ((TextContent)message.Content).Body;
                            xmltw.WriteStartDocument();
                            xmltw.WriteStartElement("content");
                            xmltw.WriteStartElement("sms");
                            xmltw.WriteStartElement("abonent");
                            xmltw.WriteString(message.Destination);
                            xmltw.WriteEndElement(); // abonent
                            xmltw.WriteStartElement("message");
                            if ((((TextContent)message.Content).isUnicode && ((TextContent)message.Content).MsgText.Length > 70) || (((TextContent)message.Content).isUnicode==false && ((TextContent)message.Content).MsgText.Length > 160))
                            {
                                xmltw.WriteAttributeString("type", "concat");
                            }
                            xmltw.WriteString(((TextContent)message.Content).MsgText);
                            xmltw.WriteEndElement(); // content-text
                            if (((TextContent)message.Content).isUnicode)
                            {
                                xmltw.WriteStartElement("dcs");
                                xmltw.WriteString("8");
                                xmltw.WriteEndElement();
                            }
                            xmltw.WriteEndElement(); // sms
                            xmltw.WriteEndElement(); // content
                            xmltw.WriteEndDocument();
                            xmltw.Flush();
                }
                byte[] bytes = ms.ToArray();
                result = _HTTPSettings.DataCoding.GetString(bytes);
            }
            return result;
        }

        public override HTTPMessageParameters DecodeRequest(string requestStr)
        {
            HTTPMessageParameters mp = new HTTPMessageParameters();
            mp.DeliveryStatus = "";
            if (!string.IsNullOrEmpty(requestStr))
            {
                string[] hparams = requestStr.Split('&');
                for (int i = 0; i < hparams.Length; i++)
                {
                    string[] hparamsvalues = hparams[i].Split('=');
                    string key = hparamsvalues[0];
                    string value = hparamsvalues[1];
                    switch(key)
                    {
                        case "id":
                            mp.UID = value;
                            break;
                        case "sms":
                            mp.Text = value;
                            break;
                        case "abonent":
                            mp.MSISDN = value;
                            break;
                        case "dest":
                            mp.SN = value;
                            break;
                        //case "pass":
                        //    if (value!="")

                    }
                }
            }
            return mp;
        }
       
    }
}
