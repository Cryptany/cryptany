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
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Messaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Cryptany.Core.ConfigOM;
using Cryptany.Core.Services.Management;
using Cryptany.Common.Utils;
using Cryptany.Common.Logging;
using Cryptany.Core.Management;
using Cryptany.Core.Management.WMI;
using System.Diagnostics;
using Cryptany.Core.MsmqLog;
using Cryptany.Core.Services;
using Cryptany.Core.Caching;

namespace Cryptany.Core
{
    public class Router : IRouter
    {
        /// <summary>
        /// ExceptionOccured agruments
        /// </summary>
        public class ExceptionOccuredEventArgs
        {
            private readonly Exception _ex;

            public ExceptionOccuredEventArgs(Exception ex)
            {
                _ex = ex;
            }

            public Exception Exception
            {
                get
                { return _ex; }
            }
        }
        /// <summary>
        /// Delegate for exception event processing
        /// </summary>
        public delegate void ExceptionOccuredDelegate(object sender, ExceptionOccuredEventArgs e);
        /// <summary>
        /// Exception event
        /// </summary>
        public event ExceptionOccuredDelegate OnExceptionOcurred;

        private readonly int ServiceCode;
        private readonly GenericRouter _routerInfo;
        protected GlobalErrorService _globalErrorService;
        protected Dictionary<Guid, IService> _Services;             // loaded services collection
        private readonly Thread _thWork;                            // message processing thread
        private bool _disposing;                                    // if we are under disposal or not
        private readonly AutoResetEvent stopPeekCompleteEvent;      // message processing finished event
        private readonly ILogger _monitorLogger;
        private readonly List<RegExp> _activeRegexes = null;
        private readonly Dictionary<string, List<RegExp>> _regexsBySn = new Dictionary<string, List<RegExp>>();
        private Cache abonentsSessions;

        /// <summary>
        /// Possible message processing results
        /// </summary>
        protected enum ProcessResult
        {
            UnknownResult = -1,
            Successfull = 0,
            ExceptionOccured = 1,
            RegexNotFound = 2,
            ServiceNotFound = 3,
            ProcessedByGlobalError,
            ProcessedUnsuccessfully
        }

        private void InitLogger()
        {
            _logger.DefaultSource = "Cryptany.RouterService"; // индекс службы роутера не указывается
            _logger.DefaultServiceSource = "Router" + ServiceCode;
        }
        
        #region ILoggable Members

        private readonly ILogger _logger = LoggerFactory.Logger;
        ILogger ILoggable.Logger
        {
            get { return _logger; }
        }

        #endregion
        
        /// <summary>
        /// Router statistics
        /// </summary>
        protected class RouterStat
        {
            private double _smsPerSecAvg = 0.0;
            private double _smsPerSecPotential = 0.0;
            private double _totalProcTimeInMillisecs = 0.0;
            //private double _smsProcTimeAvg = 0.0;
            private double _smsProcTimeMax = 0.0;
            private long _totalSmsProcessed = 0L;

            private DateTime _startTime;
            private bool _isStarted = false;

            private Guid _maxProcTimeMsgId;
            private string _maxProcTimeMsgText;

            private readonly Queue<double> _last10Sms = new Queue<double>();

            private const string pccRouterPerformanceName = "RouterPerformance";
            private static PerformanceCounterCategory _routerPerformance;
            private PerformanceCounter pc = new PerformanceCounter();


            static RouterStat()
            {

            }


            public double SmsPerSecAvg
            {
                get
                {

                    DateTime now = DateTime.Now;
                    if (_totalProcTimeInMillisecs == 0.0)// the router is mega-fast
                        _smsPerSecAvg = double.PositiveInfinity;
                    else
                    {
                        TimeSpan s = now - _startTime;
                        _smsPerSecAvg = _totalSmsProcessed / s.TotalSeconds;
                    }
                    return _smsPerSecAvg;
                }
            }

            public double SmsPerSecEffectiveAvg
            {
                get
                {
                    _smsPerSecPotential = (_totalSmsProcessed / _totalProcTimeInMillisecs) * 1000;
                    return _smsPerSecPotential;
                }
            }

            public double SmsPerSecEffectiveCurrent
            {
                get
                {
                    double sum = 0.0;
                    foreach (double d in _last10Sms)
                        sum += d;
                    if (_last10Sms.Count > 0)
                    {
                        sum /= _last10Sms.Count;// this gives avg proc time
                        sum = 1000.0 / sum;
                    }
                    else
                        sum = double.PositiveInfinity;
                    return sum;
                }
            }

            public double TotalProcTime
            {
                get
                {
                    return _totalProcTimeInMillisecs;
                }
            }

            public double SmsProcTimeAvg
            {
                get
                {
                    return _totalProcTimeInMillisecs / _totalSmsProcessed;
                }
            }

            public double SmsProcTimeMax
            {
                get
                {
                    return _smsProcTimeMax;
                }
            }

            public long TotalSmsProcessed
            {
                get
                {
                    return _totalSmsProcessed;
                }
            }

            public Guid MaxProcTimeMsgId
            {
                get
                {
                    return _maxProcTimeMsgId;
                }
            }

            public string MaxProcTimeMsgText
            {
                get
                {
                    return _maxProcTimeMsgText;
                }
            }

            public double[] Last10Sms
            {
                get
                {
                    return _last10Sms.ToArray();
                }
            }

            /// <summary>
            /// Implement Aitken interpolation scheme to estimate possible Router performance figures.
            /// </summary>
            /// <param name="procTimeInMillisecs"></param>
            /// <param name="msg"></param>
            public void AddNewSmsInfo(double procTimeInMillisecs, Message msg)
            {
                DateTime now = DateTime.Now;
                if (!_isStarted)
                {
                    _startTime = DateTime.Now;
                    _isStarted = true;
                }

                if (procTimeInMillisecs > _smsProcTimeMax)
                {
                    _smsProcTimeMax = procTimeInMillisecs;
                    _maxProcTimeMsgId = msg.InboxId;
                    _maxProcTimeMsgText = msg.Text;
                }
                _last10Sms.Enqueue(procTimeInMillisecs);
                if (_last10Sms.Count > 10)
                    _last10Sms.Dequeue();
                _totalProcTimeInMillisecs += procTimeInMillisecs;
                _totalSmsProcessed++;
            }
        }
        private readonly RouterStat _routerStats = new RouterStat();

        public Router(int ServiceCode)
        {
            this.ServiceCode = ServiceCode;
            InitLogger();
            _logger.Write(new LogMessage("Initializing router", LogSeverity.Debug));

            _thWork = new Thread(Thread_Work);
            _thWork.Name = "RouterWorkThread";

            _activeRegexes = ChannelConfiguration.DefaultPs.GetEntitiesByPredicate<RegExp>(
                delegate(RegExp r)
                {
                    return r.Token.Enabled;
                }
            );
            _logger.Write(new LogMessage("Loaded active regexs.", LogSeverity.Debug));
            if (_activeRegexes == null || _activeRegexes.Count == 0)
                _logger.Write(new LogMessage("Regex collection count = 0", LogSeverity.Debug));
            foreach (ServiceNumber sn in ChannelConfiguration.DefaultPs.GetEntities<ServiceNumber>())
                _regexsBySn.Add(sn.Number, new List<RegExp>());
            if (_regexsBySn.Keys.Count == 0)
            {
                throw new ApplicationException("Error loading settings from database: service numbers collection is empty.");

            }

            foreach (RegExp r in _activeRegexes)
            {

                Regex regex = r.RegexObject;// this forces the compilation of the regex; it is cacher within r for further use
                foreach (ServiceNumber sn in r.Token.ServiceNumbers)
                    _regexsBySn[sn.Number].Add(r);
            }

            LogRegexBySnDictionary();

            // Load services
            _Services = new Dictionary<Guid, IService>();

            stopPeekCompleteEvent = new AutoResetEvent(true);
            LoadServices();

            abonentsSessions = new Cache();
            abonentsSessions.ItemAddedOrRemoved += new ItemAddedOrRemovedEventHandler(UpdateActiveSesCounter);
            Trace.WriteLine("Router: starting read from queue");

            // Now launch the MONITORING
            _monitorLogger = LoggerFactory.Logger;
            _monitorLogger.DefaultSource = "Router" + ServiceCode;
            _monitorLogger.DefaultServiceSource = "MONITORING";

            _thWork.Start();

            Trace.WriteLine("Router: init done");
            _logger.Write(new LogMessage("Router init done", LogSeverity.Debug));


        }

        /// <summary>
        /// Message processing thread
        /// </summary>
        private void Thread_Work()
        {

            using (MessageQueue _queue = ServicesConfigurationManager.GetInputSMSQueue(ServiceCode))
            {
                while (!_disposing)
                {
                    System.Messaging.Message msg = _queue.Receive();
                    stopPeekCompleteEvent.Reset();
                    //обработка
                    InputSMSQueue_ReceiveCompleted((Message)msg.Body);
                }
            }
        }

        private void LogRegexBySnDictionary()
        {
            string regexCounts = "";
            int iCount = 0;
            foreach (string sn in _regexsBySn.Keys)
            {
                if (regexCounts == "")
                    regexCounts = sn + ":" + _regexsBySn[sn].Count;
                else
                    regexCounts += ";\r\n " + sn + ":" + _regexsBySn[sn].Count;
                iCount += _regexsBySn[sn].Count;
            }
            if (iCount == 0)
            {
                throw new ApplicationException("Error loading settings from database: RegEx collection is empty.");
            }
            _logger.Write(new LogMessage("Tracing the RegexBySn collection content (only counts): " + regexCounts, LogSeverity.Debug));
        }

        public void Dispose()
        {
            _disposing = true;
            stopPeekCompleteEvent.WaitOne();

            _logger.Write(new LogMessage("Finished processing messages. Disposing router ", LogSeverity.Debug));

            ClearServices();
            _logger.Dispose();
        }

        public bool CallService(Guid id, Message msg, object[] args)
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        public IService[] GetLoadedServices()
        {
            int count = _Services.Values.Count;
            IService[] services = (IService[])Array.CreateInstance(typeof(IService), count);
            _Services.Values.CopyTo(services, 0);
            return services;
        }

        public IService GetServiceByType(Type t)
        {
            foreach (IService s in _Services.Values)
            {
                if (s.GetType() == t)
                    return s;
            }
            return null;
        }

        public T GetService<T>()
        {
            foreach (IService s in _Services.Values)
            {
                if (s.GetType() == typeof(T))
                    return (T)s;
            }
            return default(T);
        }

        /// <summary>
        /// Processing incoming message taken out from queue
        /// </summary>
        private void InputSMSQueue_ReceiveCompleted(Message msg)
        {
            Trace.WriteLine("Router: Got message");
            _logger.Write(new LogMessage("Got message.", LogSeverity.Debug));
            ProcessResult res = ProcessResult.UnknownResult;

            try
            {
                DateTime dtStartProcess = DateTime.Now;
                
                // call processing handler
                res = ProcessMessage(msg);

                DateTime endTime = DateTime.Now;
                lock (_routerStats)
                {
                    _routerStats.AddNewSmsInfo((endTime - dtStartProcess).TotalMilliseconds, msg);
                }

                DateTime dtEndProcess = DateTime.Now;
                _logger.Write(new LogMessage(string.Format("Processing message completed. (msg={0}); Total message process time, ms: {1}", msg, (dtEndProcess - dtStartProcess).TotalMilliseconds), LogSeverity.Info));
            }
            catch (MessageQueueException mex)
            {

                if (OnExceptionOcurred != null)
                {
                    ExceptionOccuredEventArgs args = new ExceptionOccuredEventArgs(mex);
                    OnExceptionOcurred(this, args);
                }
                _logger.Write(new LogMessage(mex.ToString(), LogSeverity.Error));
                ServiceManager.LogEvent(mex.ToString(), EventType.Error, EventSeverity.Critical);
                res = ProcessResult.ExceptionOccured;

            }
            catch (ApplicationException ex)
            {
                if (OnExceptionOcurred != null)
                {
                    ExceptionOccuredEventArgs args = new ExceptionOccuredEventArgs(ex);
                    OnExceptionOcurred(this, args);
                }
                _logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                res = ProcessResult.ProcessedUnsuccessfully;

            }
            catch (Exception ex)
            {
                if (OnExceptionOcurred != null)
                {
                    ExceptionOccuredEventArgs args = new ExceptionOccuredEventArgs(ex);
                    OnExceptionOcurred(this, args);
                }
                _logger.Write(new LogMessage(ex.ToString(), LogSeverity.CriticalError));
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.Critical);
                res = ProcessResult.ExceptionOccured;


            }
            finally
            {
                stopPeekCompleteEvent.Set();

            }
        }

        /// <summary>
        /// Send an e-mail with error report. The address is taken from the config file.
        /// </summary>
        /// <param name="errorText">Error message text to send</param>
        protected void ReportError(string errorText)
        {
            string sendTo;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ErrorReportMail"]))
                sendTo = ConfigurationManager.AppSettings["ErrorReportMail"];
            else
                sendTo = "error-report@cryptany.com";
            TrySendMail("error-report@cryptany.com", sendTo, "Router critical error", errorText);
        }
        private void UpdateActiveSesCounter()
        {


        }
        protected void TrySendMail(string from, string to, string subject, string body)
        {
            try
            {
                string smtpServerIP = ConfigurationManager.AppSettings["SmtpServerIP"];
                Mail sender = new Mail(smtpServerIP);//("192.168.10.4");
                sender.SendMail(from, to, subject, body, System.Net.Mail.MailPriority.High);
                _logger.Write(new LogMessage("Notification mail has been sent successfully", LogSeverity.Debug));
            }
            catch (Exception ex)
            {
                _logger.Write(new LogMessage("Unable to send a mail. Exception details follow: " + ex.ToString(), LogSeverity.CriticalError));
            }
        }

        /// <summary>
        /// Logs the router's loading stats, such as maximum and average SMS' flow rate and processing time
        /// </summary>
        private void DoLogRouterLoadingStats()
        {
            _monitorLogger.Write(new LogMessage("MONITORING: Starting the loading monitoring thread", LogSeverity.Debug));
            while (true)
            {
                Thread.Sleep(new TimeSpan(0, 0, 30));// 30 secs

                try
                {

                    string info = string.Format("MONITORING: TotalSmsProcessed = {0}," +
                                                " Sms per second, avg = {1}," +
                                                " Sms processing time, avg = {2} ms," +
                                                " Sms processing time, max = {3} ms (text='{6}',id='{5}')," +
                                                " avg effective productivity, sms/sec = {4}," +
                                                " current effective productivity, sms/sec = {7}",
                                                _routerStats.TotalSmsProcessed.ToString(),
                                                double.IsInfinity(_routerStats.SmsPerSecAvg) ? "INFINITY" : _routerStats.SmsPerSecAvg.ToString(),
                                                _routerStats.SmsProcTimeAvg.ToString(),
                                                _routerStats.SmsProcTimeMax.ToString(),
                                                double.IsInfinity(_routerStats.SmsPerSecEffectiveAvg) ? "INFINITY" : _routerStats.SmsPerSecEffectiveAvg.ToString(),
                                                _routerStats.MaxProcTimeMsgId.ToString(),
                                                _routerStats.MaxProcTimeMsgText,
                                                double.IsInfinity(_routerStats.SmsPerSecEffectiveCurrent) ? "INFINITY" : _routerStats.SmsPerSecEffectiveCurrent.ToString());

                    _monitorLogger.Write(new LogMessage(info, LogSeverity.Debug));
                }
                catch (Exception ex)
                {
                    _logger.Write(new LogMessage("Error while writing statistic " + ex, LogSeverity.Error));
                }
            }
        }

        /// <summary>
        /// Message processing handler
        /// </summary>
        private ProcessResult ProcessMessage(Message msg)
        {
            _logger.Write(new LogMessage("Begin processing message. Message text: " + msg, LogSeverity.Info));

            Abonent ab = Abonent.LoadAbonent(msg.MSISDN); // reload from database even if it is in cache
            if (ab == null)
            {
                _logger.Write(new LogMessage("Can't get abonent for this message: processing by global error service..." + msg, LogSeverity.Alert));
                ServiceManager.LogEvent("Can't get abonent for this message: processing by global error service..." + msg, EventType.Error, EventSeverity.Critical);
                return ProcessUnprocessedMessage(msg);

            }
            AddToInbox(msg);
            AnswerMap answerMap = null;
            ProcessResult res;
            if (ab.IsBlocked == AbonentState.Blocked)
            {
                
                _logger.Write(new LogMessage("Abonent is blocked. Redirecting to blocked abonent channel... msg: " + msg, LogSeverity.Alert));
                BlockedAbonentChannel blockedAbsChannel = ChannelConfiguration.DefaultPs.GetOneEntityByFieldValue<BlockedAbonentChannel>("Enabled", true);
                answerMap = blockedAbsChannel.GetDefaultMainAnswerMap();
            }
            else
            {
                DateTime dtBeginMatchRegex = DateTime.Now;
                answerMap = MatchRegex(ab, msg);
                DateTime dtEndMatchRegex = DateTime.Now;
                _logger.Write(new LogMessage(string.Format("Router.ProcessMessage(msg= {0}), match regex time, ms: {1}", msg.ToString(), (dtEndMatchRegex - dtBeginMatchRegex).TotalMilliseconds), LogSeverity.Info));

                if (answerMap == null)
                {
                    ServiceNumber sn = ServiceNumber.GetServiceNumberBySN(msg.ServiceNumberString);
                    Channel errorChannel = sn.GetEnabledErrorChannel();

                    if (errorChannel != null)
                    {
                        answerMap = errorChannel.GetDefaultMainAnswerMap();
                    }
                }
            }
            //нашли карту
            if (answerMap != null)
            {
                _logger.Write(new LogMessage("Redirecting message to channel: " + (Guid)answerMap.Channel.ID + ", msg: " + msg, LogSeverity.Info));
                AddToChannelInbox(msg, (Guid)answerMap.Channel.ID, (Guid)answerMap.ID);
                ab.LockedChannel = FindLockedChannel(ab, answerMap.Channel.Service);
                res = FindAndProcess(msg, answerMap);
            }
            else
            {
                _logger.Write(new LogMessage("Unable to bind the input message to  any channel: answer map not found", LogSeverity.Info));
                res = ProcessResult.ServiceNotFound;
            }
            if (res != ProcessResult.Successfull)
                res = ProcessUnprocessedMessage(msg);

            else WriteCurrentAbonentLock(ab, answerMap.Channel);

            ab.Clubs = null;
            return res;
        }
        private void WriteCurrentAbonentLock(Abonent ab, Channel ch)
        {
            Guid ServiceId = (Guid)ch.Service.ID;

            Dictionary<Guid, Channel> sessions;

            if (!abonentsSessions.GetItem<Dictionary<Guid, Channel>>(ab.MSISDN, out sessions)) // no session found
            {
                sessions = new Dictionary<Guid, Channel>();
                sessions.Add(ServiceId, ch);
                // abonentsSessions.Add<Dictionary<Guid, Channel>>(ab.MSISDN, sessions);
            }
            else
            {

                if (sessions == null) //timed out
                {
                    sessions = new Dictionary<Guid, Channel>();
                    sessions.Add(ServiceId, ch);
                }

                else
                {
                    if (sessions.ContainsKey(ServiceId)) // Found session for service
                    {
                        sessions[ServiceId] = ch;

                    }
                    else // No session for the service yet
                    {

                        sessions.Add(ServiceId, ch);
                    }
                }

            }
            abonentsSessions.Add<Dictionary<Guid, Channel>>(ab.MSISDN, sessions); // Add or update

        }


        private Channel FindLockedChannel(Abonent ab, Service srv)
        {
            Guid ServiceId = (Guid)srv.ID;
            Dictionary<Guid, Channel> sessions = new Dictionary<Guid, Channel>();

            if (abonentsSessions.GetItem<Dictionary<Guid, Channel>>(ab.MSISDN, out sessions) && sessions != null && sessions.ContainsKey(ServiceId))
                return sessions[ServiceId];
            
            return null;
        }

        /// <summary>
        /// Request from another service to process in global service
        /// </summary>
        /// <param name="msg">Message to process</param>
        private void ErrorServiceRequest(Message msg)
        {
            ProcessUnprocessedMessage(msg);
        }

        private ProcessResult ProcessUnprocessedMessage(Message msg)
        {
            try
            {
                DateTime dtStartProcess = DateTime.Now;

                //Fix! globalErrCh is not needed anymore
                GlobalErrorChannel globalErrCh = ChannelConfiguration.DefaultPs.GetOneEntityByFieldValue<GlobalErrorChannel>("Name", "GLOBAL ERROR CHANNEL");
                Abonent ab = Abonent.GetByMSISDN(msg.MSISDN);
                ab.LockedChannel = FindLockedChannel(ab, globalErrCh.Service);

                AddToChannelInbox(msg, (Guid)globalErrCh.ID, globalErrCh.AnswerMaps.Count > 0 ? (Guid)globalErrCh.AnswerMaps[0].ID : Guid.Empty);
                _globalErrorService.ProcessMessage(msg, globalErrCh.AnswerMaps[0]);
                WriteCurrentAbonentLock(ab, globalErrCh);
                DateTime dtEndProcess = DateTime.Now;
                _logger.Write(new LogMessage(string.Format("Router.ProcessUnprocessedMessage(msg={0}), process time, ms: {1}", msg.ToString(), (dtEndProcess - dtStartProcess).TotalMilliseconds), LogSeverity.Info));
                return ProcessResult.ProcessedByGlobalError;
            }
            catch (Exception ex)
            {
                _logger.Write(
                    new LogMessage(
                        "Exception while trying to process message with Global error service. Message text: '" +
                        msg.Text + "'. Exception text: " + ex, LogSeverity.Error));
                return ProcessResult.ExceptionOccured;
            }
        }

        /// <summary>
        /// Call to service processing handler
        /// </summary>
        protected ProcessResult FindAndProcess(Message msg, AnswerMap am)
        {
            DateTime dtStartProcess = DateTime.Now;
            ProcessResult result;
            Guid serviceId = (Guid)am.Channel.Service.ID; // request service - TVAD, SubscriptionService, GlobalErrorService, ContragentSmsService 
            _logger.Write(new LogMessage(string.Format("Received Msg: {0} Relates to Service: {1}", msg.ToString(), am.Channel.Service.Name), LogSeverity.Info));
            
            if (_Services.ContainsKey(serviceId))
            {
                IService service = _Services[serviceId];
                if (service.ProcessMessage(msg, am))
                {
                    result = ProcessResult.Successfull;
                }
                else
                    result = ProcessResult.ProcessedUnsuccessfully;
            }
            else
                result = ProcessResult.ServiceNotFound;
            DateTime dtEndProcess = DateTime.Now;
            _logger.Write(new LogMessage(string.Format("FindAndProcess(msg={0}) Processing duration: {1} ms", msg.ToString(), (dtEndProcess - dtStartProcess).TotalMilliseconds), LogSeverity.Info));
            return result;
        }

        protected AnswerMap MatchRegex(Abonent ab, Message msg)
        {
            //string text = msg.Translit.ToUpper();
            string text = Text.Transliterate(msg.Text);

            foreach (RegExp r in _regexsBySn[msg.ServiceNumberString])
            {
                if (r.RegexObject.IsMatch(text))
                {
                    if (r.Token == null)
                        _logger.Write(new LogMessage(string.Format("There is no token for this regex {0}", r.Regex), LogSeverity.Alert));
                    else if (!r.Token.Universal)
                    {
                        if (r.Token.AnswerMaps.Count == 0)
                        {
                            _logger.Write(new LogMessage(string.Format("No answer map assigned to the non-universal token name='{0}, id='{1}'", r.Token.Name, r.Token.ID.ToString()), LogSeverity.Alert));
                            return null;
                        }
                        if (r.Token.AnswerMaps.Count > 1)
                        {
                            _logger.Write(new LogMessage("Ambuigous match: several answer maps assossiated with the given nonuniversal token. Only one map will be processed", LogSeverity.Alert));
                            _logger.Write(new LogMessage("Searching for a map belonging to an enabled channel", LogSeverity.Info));
                        }

                        foreach (AnswerMap am in r.Token.AnswerMaps)
                        {
                            if (am.Channel == null)
                                _logger.Write(new LogMessage(string.Format("There is no channel for this answermap {0}", am.Name), LogSeverity.Alert));
                            if (am.Channel != null && am.Channel.Enabled)
                                return am;
                        }
                        _logger.Write(new LogMessage(string.Format("There is no enabled channel for the non-universal token name='{0}, id='{1}'", r.Token.Name, r.Token.ID.ToString()), LogSeverity.Alert));
                        return null;
                    }
                    else
                    {
                        Service srv = r.Token.AnswerMaps[0].Channel.Service;

                        Channel ch = FindLockedChannel(ab, srv);
                        if (ch != null)
                        {
                            _logger.Write(new LogMessage("Found service " + srv.ClassName + " and channel " + ch.Name + " for secondary token " + r.Token.Name, LogSeverity.Info));
                            foreach (AnswerMap am in ch.AnswerMaps)
                            {
                                if (am.Token == r.Token)
                                    return am;

                            }
                        }

                        _logger.Write(new LogMessage("Abonent MSISDN " + ab.MSISDN + ", is not locked to any channel. Service: " + srv.ClassName, LogSeverity.Info));
                    }
                }
            }
            _logger.Write(new LogMessage(string.Format("No token has matched the messge '{0}'", msg.ToString()), LogSeverity.Alert));
            return null;
        }

        /// <summary>
        /// Fills out Services collection with active services and error services
        /// </summary>
        protected void LoadServices()
        {
            // Clean up services
            ClearServices();
            // Load services
            _logger.Write(new LogMessage("Start loading services", LogSeverity.Debug));
            foreach (Cryptany.Core.ConfigOM.Service service in ChannelConfiguration.DefaultPs.GetEntities<Service>())
            {
                try
                {
                    if (service.Name == "ERROR SERVICE")
                    {
                        _globalErrorService = (GlobalErrorService)InstantiateService(service);
                        _Services.Add(_globalErrorService.ID, _globalErrorService);
                        continue;
                    }

                    if (service.Name == "BLOCKED ABONENTS SERVICE")
                    { }

                    if (service.Enabled)
                    {
                        // Create service by its name
                        IService iservice = InstantiateService(service);
                        if (iservice != null)
                        {
                            _Services.Add(iservice.ID, iservice);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Write(new LogMessage("Failed to load service " + service.Name + ": " + e,
                                                 LogSeverity.Debug));
                }
            }
            _logger.Write(new LogMessage("Loading services finished", LogSeverity.Debug));
        }

        /// <summary>
        /// create instance of ClassName class from Services.Services database table and cast it to IService
        /// </summary>
        /// <param name="service">database record from Services.Services</param>
        /// <returns>IService instance</returns>
        private IService InstantiateService(Service service)
        {
            string className = "";
            IService tmpServ = null;

            className = service["ClassName"].ToString();
            try
            {
                tmpServ =
                    (IService)
                    Assembly.GetAssembly(Type.GetType(className)).CreateInstance(className,
                                                                                 false, 
                                                                                 BindingFlags.CreateInstance, 
                                                                                 null,
                                                                                 new object[] { this, service.Name }, 
                                                                                 null,
                                                                                 null);
                _logger.Write(new LogMessage("Service " + service.Name + " successfully loaded", LogSeverity.Debug));
            }
            catch
            {
                _logger.Write(new LogMessage("Failed to load service '" + service.Name + "' (it seems the instance has not been created properly)", LogSeverity.Debug));
            }
            
            return tmpServ;
        }

        /// <summary>
        /// Saves to database incoming message taken from queue
        /// </summary>
        /// <param name="msg"></param>
        public static void AddToInbox(Message msg)
        {
            ServiceNumber sn = ServiceNumber.GetServiceNumberBySN(msg.ServiceNumberString);

            MSMQLogEntry me = new MSMQLogEntry();
            me.DatabaseName = Database.DatabaseName;
            me.CommandText = "kernel.AddIncomingMessageToDB";
            me.Parameters.Add("@inboxId", msg.InboxId);
            me.Parameters.Add("@msgTime", msg.MessageTime);
            me.Parameters.Add("@MSISDN", msg.MSISDN);
            me.Parameters.Add("@msgText", msg.Text);
            me.Parameters.Add("@smscId", msg.SMSCId);
            me.Parameters.Add("@serviceNumberId", sn.DatabaseId);
            me.Parameters.Add("@transactionId", msg.TransactionID);
            me.Parameters.Add("@messageType", (int)msg.Type);
            using (MessageQueue _MSMQLoggerInputQueue = ServiceManager.MSMQLoggerInputQueue)
            {
                _MSMQLoggerInputQueue.Send(me);
            }

        }

        private void AddToChannelInbox(Message msg, Guid channelID, Guid answerMapId)
        {
            try
            {
                MSMQLogEntry me = new MSMQLogEntry();
                me.DatabaseName = Database.DatabaseName;
                me.CommandText = "Services.AddToChannelInbox";
                me.Parameters.Add("@ChannelID", channelID);
                me.Parameters.Add("@answerMapId", answerMapId);
                me.Parameters.Add("@InboxMsgId", msg.InboxId);

                using (MessageQueue MSMQLoggerInputQueue = ServiceManager.MSMQLoggerInputQueue)
                {
                    MSMQLoggerInputQueue.Send(me);
                }
            }
            catch (Exception e)
            {
                if (_logger != null)
                {
                    _logger.Write(new LogMessage("Exception in Router AddToChannelInbox method: " + e, LogSeverity.Error));
                }
            }
        }

        /// <summary>
        /// Clears out Services collection
        /// </summary>
        private void ClearServices()
        {
            // Clear services
            if (_Services != null && _Services.Count > 0)
            {
                foreach (IService srv in _Services.Values)
                {
                    srv.Dispose();
                }
                _Services.Clear();
            }
        }

        private ProcessResult ProcessSystemMessage(Message msg)
        {
            if (msg.Text == "Refresh_CCDS")
            {
                ChannelConfiguration.SetDSToNull();
                // Reload ChannelConfigDS
                ChannelConfiguration.ReloadDataSet();
                // Reload services
                LoadServices();
            }
            return ProcessResult.Successfull;
        }

        #region IRouter Members
        
        T IRouter.GetService<T>()
        {
            foreach (IService s in _Services.Values)
            {
                if (s.GetType() == typeof(T))
                    return (T)s;
            }
            return default(T);
        }

        public int RouterIndex
        {
            get
            {
                return ServiceCode;
            }

        }

        #endregion
    }
}
