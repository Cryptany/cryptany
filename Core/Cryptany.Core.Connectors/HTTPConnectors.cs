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
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using Cryptany.Core.Connectors;
using Cryptany.Core.Management;
using Cryptany.Common.Logging;
using System.Xml;

namespace Cryptany.Core
{
    public class RequestState
    {
        public HttpWebRequest request;
        public object[] parameters;
    }

    /// <summary>
    /// HTTPConnector class: interacts with SMSC by HTTP protocol
    /// </summary>
    public class HTTPConnector : AbstractConnector
    {
        
        public enum RequestType { GET, POST } ;

        /// <summary>
        /// Делегат доступа к асинхронному методу получения данных
        /// </summary>
        protected AsyncCallback m_asyncCallBack_Rcv;

        /// <summary>
        /// Делегат доступа к асинхронному методу отсылки данных
        /// </summary>
        protected AsyncCallback m_asyncCallBack_Snd;

        /// <summary>
        /// Объект, содержащий результат выполнения асинхронного метода получения данных
        /// </summary>
        protected IAsyncResult m_asyncResult_Rcv;

        /// <summary>
        /// Объект, содержащий результат выполнения асинхронного метода отсылки данных
        /// </summary>
        protected IAsyncResult m_asyncResult_Snd;

        protected HTTPMessageManager m_HTTPmm;  //ссылка на MessageManager
        protected HttpListener m_listener;
        protected OutputMessage m_outputMsg;
        
        protected string m_prefix;
        protected string m_requestURL;
        protected string m_serverPass;
        protected string m_systemID;
        
        public AutoResetEvent OutgoingMessageReady = new AutoResetEvent(false);
        
        /// <summary>
        /// Прокси для веб запросов
        /// </summary>
        protected IWebProxy Proxy;

        public HTTPConnector(string scheme, string host, string port, string path, string requestURL, string systemID,
                             string password, HTTPMessageManager httpmm)
            : base(httpmm)
        {
            // Инициализация логгера (XMLLogger)
			// Инициализация параметров соединения
            try
            {
                RequestURL = requestURL;
                
                // инициализация HttpListener
                Listener = new HttpListener();
                Prefix = GetPrefixString(scheme, host, port, path);
                if (!string.IsNullOrEmpty(Prefix))
                {
                    if (!Listener.Prefixes.Contains(Prefix))
                        Listener.Prefixes.Add(Prefix);
                }
                else
                {
                    throw new Exception("The prefix for http listener was not generated!");
                }
                // Инициализация веб прокси
                Proxy = WebRequest.GetSystemWebProxy();
                Proxy.Credentials = CredentialCache.DefaultCredentials;
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector constructor: " + e, LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.Critical);
            }
            SystemID = systemID;
            ServerPass = password;
            HTTPmm = httpmm;
            OutputMsg = null;
        }

        public HTTPMessageManager HTTPmm
        {
            get { return m_HTTPmm; }
            set { m_HTTPmm = value; }
        }

        public string RequestURL
        {
            get { return m_requestURL; }
            set { m_requestURL = value; }
        }

        public HttpListener Listener
        {
            get { return m_listener; }
            set { m_listener = value; }
        }

        public string Prefix
        {
            get { return m_prefix; }
            set { m_prefix = value; }
        }

        public string SystemID
        {
            get { return m_systemID; }
            set { m_systemID = value; }
        }

        public string ServerPass
        {
            get { return m_serverPass; }
            set { m_serverPass = value; }
        }

        public OutputMessage OutputMsg
        {
            get { return m_outputMsg; }
            set { m_outputMsg = value; }
        }

        // Сформировать префикс для HttpListener
        private string GetPrefixString(string scheme, string host, string port, string path)
        {
            string prefix = "";
            try
            {
                if (!string.IsNullOrEmpty(scheme) && (scheme.Equals("HTTP", StringComparison.InvariantCultureIgnoreCase) || scheme.Equals("HTTPS",StringComparison.InvariantCultureIgnoreCase)))
                {
                    prefix += scheme;
                }
                else
                {
                    throw new Exception("The scheme (http or https) should be specified!");
                }
                if (!string.IsNullOrEmpty(host))
                    // host name ("localhost"), "*" (request don't matched any other prefix for port), "+" (all requests to port)
                {
                    prefix += "://" + host;
                }
                else
                {
                    throw new Exception("The host name should be specified!");
                }
                if (!string.IsNullOrEmpty(port))
                {
                    prefix += ":" + port;
                }
                else
                {
                    if (host == "*" || host == "+")
                    {
                        throw new Exception("The port should be specified!");
                    }
                }
                if (!string.IsNullOrEmpty(path))
                {
                    prefix += "/" + path;
                }
                prefix += "/"; // prefix must end with "/"
            }
            catch (Exception e)
            {
                prefix = string.Empty;
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector GetPrefixString method: " + e,
                                                LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.Critical);
            }
            return prefix;
        }

        private HttpWebRequest CreateRequest(RequestType reqType, byte[] bytes, string contentType)
        {
            // инициализация HttpWebRequest
            HttpWebRequest webRequest = null;
            switch (reqType)
            {
                case RequestType.GET:
                    webRequest = (HttpWebRequest) WebRequest.Create(RequestURL + Encoding.UTF8.GetString(bytes));
                    webRequest.Proxy = Proxy;
                    
                    webRequest.Method = "GET";
                    webRequest.ContentType = contentType; // всегда null
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    break;
                case RequestType.POST:
                    webRequest = (HttpWebRequest) WebRequest.Create(RequestURL);
                    webRequest.Proxy = Proxy;
                    webRequest.Method = "POST";
                    webRequest.ContentType = contentType;
                    webRequest.Credentials = new NetworkCredential(SystemID, ServerPass);
                    webRequest.PreAuthenticate = true;
                    Stream reqStream = webRequest.GetRequestStream();
                    reqStream.Write(bytes, 0, bytes.Length);
                    reqStream.Close();
                    break;
            }
            if (webRequest != null) webRequest.KeepAlive = false;
            return webRequest;
        }

        /// <summary>
        /// Starts connector to go
        /// </summary>
        /// <returns>the result of the operation, typically true</returns>
        public override bool Start()
        {
            bool isOk = false;
            try
            {
                if (!Listener.IsListening)
                {
                    Listener.Start();
                }
                if (Listener.IsListening)
                {
                    // Wait for data asynchronously 
                    WaitForData_Rcv();
                    isOk = true;
                }
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector Start method: " + e, LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.Critical);
                isOk = false;
            }
            return isOk;
        }

        protected void WaitForData_Rcv()
        {
            // Wait for data asynchronously
            try
            {
                if (m_asyncCallBack_Rcv == null)
                {
                    m_asyncCallBack_Rcv = OnDataReceived;
                }
                // Start listening to the data asynchronously
                if (Listener.IsListening)
                {
                    m_asyncResult_Rcv = Listener.BeginGetContext(m_asyncCallBack_Rcv, Listener);
                }
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector WaitForData_Rcv method: " + e,
                                                LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
        }

        protected void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                HttpListener listener = (HttpListener) asyn.AsyncState;
                HttpListenerContext context = null;
                try
                {
                    context = listener.EndGetContext(asyn);
                }
                catch
                {
                    return;
                }
                
                HttpListenerResponse response = context.Response;
                //Guid inboxMsgID = Guid.Empty;
                //string dataStr = string.Empty;
                int statusCode = 200;
                byte[] resp = HTTPmm.ProcessHttpRequest(context.Request.QueryString, context.Request.InputStream, out statusCode);
               
                // Send HTTP response to client
                if (resp.Length > 0)
                {
                    
                    response.ContentLength64 = resp.Length;
                    response.AppendHeader("Content-Type",((HTTPSettings)HTTPmm.AbstractSettings).ContentType );
                    Stream resStream = response.OutputStream;
                    resStream.Write(resp, 0, resp.Length);
                }
                response.StatusCode = statusCode;
                
                response.Close();
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector OnDataReceived method: " + e,
                                                LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
            finally
            {
                // Initiate new recieving cycle
                WaitForData_Rcv();
            }
        }

        /// <summary>Stops the connector, purging all cached messages first and then unbinding connections</summary>
        /// <returns>The result of the operation</returns>
        public bool Stop(bool discardQueuedRequests)
        {
            bool isOk;
            try
            {
                if (Listener.IsListening)
                {

                    if (discardQueuedRequests)
                    {
                        Listener.GetType().InvokeMember("RemoveAll", BindingFlags.NonPublic | BindingFlags.InvokeMethod | BindingFlags.Instance, null, Listener, new object[] { false });
                        Listener.Abort();
                    }
                    else
                        Listener.Close();
                }
                
                //Listener = null;
                Thread.Sleep(90000);
                isOk = true;
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector Stop method: " + e, LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
                isOk = false;
            }
            
            return isOk;
        }

        public bool AsyncSend(byte[] bytes, RequestType reqType, string contentType)
        {
            bool isOk;
            try
            {
                // Wait for data to send asynchronously 
                WaitForData_Snd(bytes, reqType, contentType);
                isOk = true;
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector AsyncSend method: " + e, LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
                isOk = false;
            }
            return isOk;
        }

        protected void WaitForData_Snd(byte[] bytes, RequestType reqType, string contentType)
        {
            // Wait for data to send asynchronously
            try
            {
                if (m_asyncCallBack_Snd == null)
                {
                    m_asyncCallBack_Snd = OnDataSent;
                }
                // Prepare request
                HttpWebRequest webRequest = CreateRequest(reqType, bytes, contentType);
                // Save sent data 
                HTTPmm.SaveToLog(bytes, "Snd", true);
                // Start sending the data asynchronously
                RequestState rs = new RequestState();
                //if (HTTPmm.HTTPSettings.FormattingType == FormattingType.Kievstar_XML)
                //{
                    
                //}
                
                rs.request = webRequest;
                
                m_asyncResult_Snd = webRequest.BeginGetResponse(m_asyncCallBack_Snd, rs);
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector WaitForData_Snd method: " + e,
                                                LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
        }

        protected void OnDataSent(IAsyncResult asyn)
        {
            try
            {
                HttpWebRequest webRequest = ((RequestState)asyn.AsyncState).request;
                
                //string rid = ((RequestState)asyn.AsyncState).id;
                //string mid = ((RequestState)asyn.AsyncState).mid;
                
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.EndGetResponse(asyn))
                {

                    HTTPmm.ProcessWebResponse(webResponse, (RequestState) asyn.AsyncState);
                }
                
                // Close response
                
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector OnDataSent method: " + e, LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
        }

        public bool Send(byte[] bytes, RequestType reqType, string contentType)
        {
            bool isOk = false;
            try
            {
                // Prepare request
                HttpWebRequest webRequest = CreateRequest(reqType, bytes, contentType);
                // Save sent data 
                HTTPmm.SaveToLog(bytes, "Snd", true);
                // Send data synchronously
                HttpWebResponse webResponse = (HttpWebResponse) webRequest.GetResponse();
                // Save response data 
                HTTPmm.SaveToLog(webResponse, "Rcv", true);
                // Close response
                webResponse.Close();
                isOk = true;
            }
            catch (Exception e)
            {
                if (Logger != null)
                {
                    Logger.Write(new LogMessage("Exception in HTTPConnector Send method: " + e, LogSeverity.Error));
                }
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
            return isOk;
        }

        
    }
}
