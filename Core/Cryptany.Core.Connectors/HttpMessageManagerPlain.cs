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
using System.IO;
using Cryptany.Core.Management.WMI;
using Cryptany.Common.Logging;
using Cryptany.Core;

namespace Cryptany.Core.Connectors
{
    public class HttpMessageManagerPlain : HTTPMessageManager
    {
        public override event EventHandler MessageReceived;
        public override event EventHandler MessageSent;
        public override event MessageStateChangedEventHandler MessageStateChanged;
        public override event StateChangedEventHandler StateChanged;
        public override event EventHandler RequireReinit;
        public HttpMessageManagerPlain(ConnectorSettings cs, ILogger logger)
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
                HTTPConn.OutputMsg = outputMessage;
                //outputMessage.TariffId = OutputMessage.GetTariffid(ConnectorId, outputMessage.Source);
                UpdateOutboxAdditional(outputMessage.ID, 1);
                AddToOutboxMessageParts(outputMessage.Content.Body, outputMessage.ID, outputMessage.HTTP_UID.ToString(), 1);
                HTTPConn.OutgoingMessageReady.Set();
                //byte[] bytes = Create_POST_request(outputMessage);
                //sendOk = HTTPConn.AsyncSend(bytes, HTTPConnector.RequestType.POST, "application/octet-stream");
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

        public override Guid Receive(HTTPConnector conn, System.Collections.Specialized.NameValueCollection queryString)
        {
            Guid inboxMsgID = Guid.Empty;
            if (queryString != null && queryString.Count > 0)
            {
                // 1. Parse incoming GET parameters
                HTTPMsgParams = DecodeRequest(queryString);
                // 2. Create MSMQ message and send it to Router main input MSMQ queue 
                inboxMsgID = Send_MSMQ_MessageToRouterInputQueue(HTTPMsgParams);
                MessageReceived(this, new EventArgs());
            }
            return inboxMsgID;
        }
        public override HTTPMessageParameters DecodeRequest(System.Collections.Specialized.NameValueCollection queryString)
        {
            HTTPMessageParameters mp = new HTTPMessageParameters();
            foreach (string key in queryString.AllKeys)
            {
                switch (key)
                {
                    case "phone":
                        mp.MSISDN = queryString[key];
                        break;
                    case "shortcode":
                        mp.SN = queryString[key];
                        break;
                    case "text":
                        mp.Text = queryString[key];
                        break;
                    case "network":
                        mp.Category = queryString[key];
                        break;
                }
            }
            return mp;
        }
        public override string EncodeResponse(OutputMessage message)
        {

            return "text=" + ((TextContent)message.Content).MsgText;
           
        }

        public override byte[] ProcessHttpRequest(NameValueCollection queryString, Stream inputStream, out int statusCode)
        {
            string dataStr = string.Empty;

            StreamReader reqSr = new StreamReader(inputStream, _HTTPSettings.DataCoding);
           // dataStr = reqSr.ReadToEnd();
            // Save received data 
            string postData = dataStr;
            Guid inboxMsgID = Receive(HTTPConn, queryString);

            SaveToLog(postData, "Rcv", true);
            string responseString = string.Empty;
            statusCode = 200;
            if (inboxMsgID != Guid.Empty)
            {
                // Create HTTP response with status OK                    
                // OK
                // Fill response with data
                if (_HTTPSettings.IsAsyncMode)
                {

                    responseString = "status=1";
                }
                else
                {
                    responseString = "status=0";
                    if (HTTPConn.OutgoingMessageReady.WaitOne(_HTTPSettings.SyncMode_RespWaitTime, false))
                    {

                        if (HTTPConn.OutputMsg != null)
                        {
                            // Have got the answer after waiting
                            if (((OutputMessage) HTTPConn.OutputMsg).InboxMsgID == inboxMsgID)
                            {
                                if (responseString.Length > 0)
                                {
                                    responseString += "\r\n";
                                }
                                responseString +=
                                    EncodeResponse((OutputMessage) HTTPConn.OutputMsg);
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

                                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, "request - response sequence is broken"));
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
                            // Haven't got the answer after waiting

                        }
                        HTTPConn.OutputMsg = null;
                    }
                    else
                        statusCode = 503;
                }



            }
            else
            {
                // Create HTTP response with status ERROR
                statusCode = 400; // Bad request
            }
            return _HTTPSettings.DataCoding.GetBytes(responseString);

        }
    }
}
