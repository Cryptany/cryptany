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
using Cryptany.Core.Management.WMI;
using System.IO;
using System.Xml;
using Cryptany.Common.Logging;
using System.Collections.Generic;
using System.Timers;
using System.Web;
using Cryptany.Core;

namespace Cryptany.Core.Connectors
{
    public class HttpMessageManagerDigital : HTTPMessageManager
    {
        public override event EventHandler MessageReceived;
        public override event EventHandler MessageSent;
        public override event MessageStateChangedEventHandler MessageStateChanged;
        public override event StateChangedEventHandler StateChanged;
        public override event EventHandler RequireReinit;

        private Dictionary<string, HTTPMessageParameters> _requests = new Dictionary<string, HTTPMessageParameters>();
        private Timer _timerCheckRequests;

        public HttpMessageManagerDigital(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
            _timerCheckRequests = new Timer();
            _timerCheckRequests.Interval = 600000; //10 минут
            _timerCheckRequests.Elapsed += _timerCheckRequests_Elapsed;
            _timerCheckRequests.Enabled = true;
        }

        void _timerCheckRequests_Elapsed(object sender, ElapsedEventArgs e)
        {
             try
            {
                _timerCheckRequests.Enabled = false;
                lock (_requests)
                {
                    foreach (HTTPMessageParameters request in _requests.Values)
                    {
                        if ((DateTime.Now - request.MessageTime) >= new TimeSpan(1, 0, 0))
                        {
                            _requests.Remove(request.UID);
                        }
                    }
                }
            }
            catch
            {
            }
            finally
            {
                _timerCheckRequests.Enabled = true;
            }
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
            
                byte[] bytes = Create_GET_request(outputMessage);
                sendOk = HTTPConn.AsyncSend(bytes, HTTPConnector.RequestType.GET, "text/plain");
                //HTTPConn.OutputMsg = outputMessage;
                //outputMessage.TariffId = OutputMessage.GetTariffid(ConnectorId, outputMessage.Source);
                UpdateOutboxAdditional(outputMessage.ID, 1);
                AddToOutboxMessageParts(outputMessage.Content.Body, outputMessage.ID, outputMessage.HTTP_UID.ToString(), 1);
                //HTTPConn.OutgoingMessageReady.Set();
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
        protected override byte[] Create_GET_request(OutputMessage outputMessage)
        {
            string queryString = "";
            queryString += "?phone=" + HttpUtility.UrlEncode(outputMessage.Destination, _HTTPSettings.DataCoding);
            queryString += "&text=" + HttpUtility.UrlEncode(((TextContent)outputMessage.Content).MsgText, _HTTPSettings.DataCoding);
            byte[] bytes = _HTTPSettings.DataCoding.GetBytes(queryString);
            return bytes;
        }
        
        public override byte[] ProcessHttpRequest(NameValueCollection queryString, Stream inputStream, out int statusCode)
        {
            string dataStr = string.Empty;
            
            //StreamReader reqSr = new StreamReader(inputStream, _HTTPSettings.DataCoding);
            //dataStr = reqSr.ReadToEnd();
                            // Save received data 
            //string postData = dataStr;
            Guid inboxMsgID = Receive(HTTPConn, queryString);
            
            //SaveToLog(postData, "Rcv", true);
            string responseString = string.Empty;
            statusCode = 200;
            if (inboxMsgID != Guid.Empty)
            {
                responseString = "OK"; //второй ответ
            }
            else
            {
                responseString = "Spasibo! Vash zakaz v obrabotke."; //первый ответ
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
            string result = "";
            
            
            return result;
        }

        public override HTTPMessageParameters DecodeRequest(NameValueCollection queryString)
        {
            HTTPMessageParameters mp = null;
            if (queryString["step"] == "1") //первый запрос, запоминаем его в очереди
            {
                mp = new HTTPMessageParameters();
                mp.UID = queryString["mssid"];
                mp.Text = queryString["text"];
                mp.Operator = queryString["net"];
                mp.MSISDN = queryString["phone"];
                mp.SN = queryString["sn"];
                lock (_requests)                                                                                                                                                                                                                                                                                                              
                {
                    _requests.Add(mp.UID, mp);
                }
                mp = null; // обратно не возвращаем..это еще не входящее сообщение
            }
            else
            {
                lock (_requests)
                {
                    if (_requests.ContainsKey(queryString["mssid"]))
                    {
                        HTTPMessageParameters param = _requests[queryString["mssid"]];
                        mp = param;
                        _requests.Remove(mp.UID);
                    }
                }

            }
            return mp;
        }
        
    }
}
