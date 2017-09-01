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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Timers;
using System.Threading;
using System.Messaging;
using Cryptany.Core.Connectors;
using Cryptany.Core.Management.WMI;
using Cryptany.Core.SmppLib;
using Cryptany.Common.Logging;
using Cryptany.Core.Interaction;
using Cryptany.Common.Utils;
using Cryptany.Core.Management;
using System.Configuration;
using Cryptany.Core.Caching;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using MessageType = Cryptany.Core.MessageType;
using System.Linq;

namespace Cryptany.Core
{
    public enum AbonentState
    {
        NotBlocked,
        Blocked,
        Unknown
    }

    /// <summary>
    /// Manages all aspects of SMPP message creation -- multi-SMS messages etc. Translates string representation of the 
    /// message attributes into adjacent classes.
    /// </summary>
    public class SMPPMessageManager : AbstractMessageManager
    {
        /// <summary>
        /// последний пакет на отправку
        /// </summary>
        protected  PacketBase _lastOutPDU; 
        public PacketBase LastOutPdu
        {
            get { return _lastOutPDU; }
            set { _lastOutPDU = value; }
        }
        
        //состояние коннектора (времена отслыки/получения PDU и Response)
        protected readonly SMPPConnectorState _connectorState = new SMPPConnectorState(); 
        
        /// <summary>
        /// текущий номер последовательности для отправки пакета (номер последнего PDU)
        /// </summary>
        protected uint _currentSequenceNumber
        {
            get { return _lastOutPDU==null?  0: _lastOutPDU.SequenceNumber; }
            set
            {
                if (_lastOutPDU != null)
                    _lastOutPDU.SequenceNumber = value;
            }
        }

        /// <summary>
        /// буфер для частей SAR-сообщения
        /// </summary>
        public SMPPMessageParts SMPPMessageParts;

        /// <summary>
        /// Двусторонний коннектор
        /// </summary>
        public SMPPConnector SMPPConn_TRx;
        
        /// <summary>
        /// Отсылающий коннектор
        /// </summary>
        public SMPPConnector SMPPConn_Tx;
        
        /// <summary>
        /// принимающий коннектор
        /// </summary>
        public SMPPConnector SMPPConn_Rx;              

        public PDUTimeouts PDUTimeouts_TRx;
        public PDUTimeouts PDUTimeouts_Tx;
        public PDUTimeouts PDUTimeouts_Rx;

        public System.Timers.Timer timer;
        
        /// <summary>
        /// настройки SMPP из БД
        /// </summary>
        public SMPPSettings SMPPSettings;
        
        public enum PDUTypesForUserData { SUBMIT_SM, DATA_SM };    // типы PDU для отправки пользовательских данных
        public enum TransferModes { TRx, Tx, Rx };                 // режимы передачи данных
        protected readonly Mutex mut_Send = new Mutex(false);
        protected readonly ManualResetEvent waitingResp = new ManualResetEvent(true);
        protected Cache msgsWaitingReceits;
      
        #region PerformanceCounters

        protected PerformanceCounter pcInCounter;
        /// <summary>
        /// Счетчик времени ответа на входящие пакеты
        /// </summary>
        protected PerformanceCounter pcPDUInResponseTime;

        /// <summary>
        /// Счетчик времени ответа на исходящие пакеты
        /// </summary>
        protected PerformanceCounter pcPDUOutResponseTime;

        protected PerformanceCounter pcWaitingDeliveryMsgsCache;

        #endregion
        
        /// <summary>
        /// Constructor of the SMPPMM. Reads up the database and prepares caches for work.
        /// </summary>
        public SMPPMessageManager(ConnectorSettings cs, ILogger logger)
            : base(cs, logger)
        {
            try
            {
                SMPPMessageParts = new SMPPMessageParts(SMPPSettings);
                ReadyToSendMessages = new ManualResetEvent(false);
            }
            catch (Exception e)
            {
                try
                {
                    StateChanged(this, new StateChangedEventArgs(ConnectorState.Error, e.ToString()));
                }
                catch
                {

                }
                Logger.Write(new LogMessage("Exception in SMPPMessageManager constructor: " + e, LogSeverity.Error));
            }
        }

        protected override void InitPerformanceCountersExt()
        {
            pcInCounter = new PerformanceCounter();
            pcInCounter.CategoryName = "Connector Service";
            pcInCounter.CounterName = "received sms";
            pcInCounter.MachineName = ".";
            pcInCounter.InstanceName = AbstractSettings.SMSCCode.ToString();
            pcInCounter.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcInCounter.ReadOnly = false;
            pcInCounter.RawValue = 0;

            pcPDUInResponseTime = new PerformanceCounter();
            pcPDUInResponseTime.CategoryName = "Connector Service:SMPP";
            pcPDUInResponseTime.CounterName = "Incoming PDU response time, ms";
            pcPDUInResponseTime.MachineName = ".";
            pcPDUInResponseTime.InstanceName = AbstractSettings.SMSCCode.ToString();
            pcPDUInResponseTime.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcPDUInResponseTime.ReadOnly = false;

            pcPDUOutResponseTime = new PerformanceCounter();
            pcPDUOutResponseTime.CategoryName = "Connector Service:SMPP";
            pcPDUOutResponseTime.CounterName = "Outgoing PDU response time, ms";
            pcPDUOutResponseTime.MachineName = ".";
            pcPDUOutResponseTime.InstanceName = AbstractSettings.SMSCCode.ToString();
            pcPDUOutResponseTime.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcPDUOutResponseTime.ReadOnly = false;


            pcWaitingDeliveryMsgsCache = new PerformanceCounter();
            pcWaitingDeliveryMsgsCache.CategoryName = "Connector Service";
            pcWaitingDeliveryMsgsCache.CounterName = "waiting receits messages cache";
            pcWaitingDeliveryMsgsCache.MachineName = ".";
            pcWaitingDeliveryMsgsCache.InstanceName = AbstractSettings.SMSCCode.ToString();
            pcWaitingDeliveryMsgsCache.InstanceLifetime = PerformanceCounterInstanceLifetime.Process;
            pcWaitingDeliveryMsgsCache.ReadOnly = false;
         
        }

        protected override void ClosePerformanceCountersExt()
        {
            pcInCounter.RemoveInstance();
            pcInCounter.Close();
            pcInCounter.Dispose();

            pcPDUInResponseTime.RemoveInstance();
            pcPDUInResponseTime.Close();
            pcPDUInResponseTime.Dispose();

            pcPDUOutResponseTime.RemoveInstance();
            pcPDUOutResponseTime.Close();
            pcPDUOutResponseTime.Dispose();
        }
        
        //время между посылкой PDU и получением ответа
        protected void SetPDUOutResponseCounter()
        {
            pcPDUOutResponseTime.RawValue = (long)(_connectorState._lastPDUOutRespTime - _connectorState._lastPDUOutTime).TotalMilliseconds;
        }
        
        //время между получением PDU и ответом на него
        protected void SetPDUInResponseCounter()
        { pcPDUInResponseTime.RawValue = (long)(_connectorState._lastPDUInRespTime - _connectorState._lastPDUInTime).TotalMilliseconds; }

        protected override void SetPerformanceCounters()
        {
            pcPDUSMSProcessTime.RawValue = (long)(State._lastSentToRouterTime - State._lastSMSInTime).TotalMilliseconds;
        }

        protected override GenericConnector State
        {
            get { return _connectorState; }
        }

        //проверка наличия соединения сокета с SMSC
        public override bool ConnectedToSmsc
        {
            get
            {
                if (SMPPSettings.IsTransceiver)
                    return SMPPConn_Rx.SocketConnected;
                return SMPPConn_TRx.SocketConnected && SMPPConn_Tx.SocketConnected;
            }
        }

        private void UpdateCacheCounter()
        {
         pcWaitingDeliveryMsgsCache.RawValue = msgsWaitingReceits.Count;
        }

        //изменение настроек коннектора
        public override void UpdateSettings(AbstractConnectorSettings settings)
        {
            SMPPSettings = settings as SMPPSettings;
        }

        /// <summary>
        /// Should be called for initialization of SMPPMM
        /// </summary>
        protected override void Init(AbstractConnectorSettings settings)
        {
            UpdateSettings(settings);
            if (SMPPSettings.RegisteredDelivery == 1)
                InitReceitsCache();
            
            // Create connector(s) for SMPP
            _connectorState.Code = SMPPSettings.SMSCCode;
            _connectorState.ID = ConnectorId.ToString();
            _connectorState.Address = SMPPSettings.IPAddress;
            _connectorState.Port = int.Parse(SMPPSettings.Port);
            
            if (SMPPSettings.IsTransceiver)         // Transceiver mode
            {
                SMPPConn_TRx = new SMPPConnector(SMPPSettings.IPAddress, SMPPSettings.Port, SMPPSettings.SystemId, SMPPSettings.Password, this);
                SMPPConn_Tx = null;
                SMPPConn_Rx = null;

                PDUTimeouts_TRx = new PDUTimeouts(this);
                PDUTimeouts_Tx = null;
                PDUTimeouts_Rx = null;
                try
                {
                    if (SMPPConn_TRx.Start() == false)
                    {
                        throw new ApplicationException("TRx: connection initialization failed!");
                    }
                    if (Open(new BindTransceiver()) == false)
                    {
                        throw new ApplicationException("TRx: open failed!");
                    }
                }
                catch (ApplicationException e)
                {
                    _connectorState.State = ConnectorState.Error.ToString();
                    _connectorState.StateDescription = e.ToString();
                    ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
                    Logger.Write(new LogMessage("Exception in SMPPMessageManager Init method (TRx mode): " + e, LogSeverity.Error));
                }
            }
            else        // Transmitter - Receiver mode
            {
                SMPPConn_TRx = null;
                SMPPConn_Tx = new SMPPConnector(SMPPSettings.IPAddress, SMPPSettings.Port, SMPPSettings.SystemId, SMPPSettings.Password, this);
                SMPPConn_Rx = new SMPPConnector(SMPPSettings.IPAddress, SMPPSettings.Port, SMPPSettings.SystemId, SMPPSettings.Password, this);
                // Initialize PDU timeouts
                PDUTimeouts_TRx = null;
                PDUTimeouts_Tx = new PDUTimeouts(this);
                PDUTimeouts_Rx = new PDUTimeouts(this);
                try
                {
                    // Initialize socket connections and SMPP sequences
                    if (SMPPConn_Tx.Start() == false)
                        throw new ApplicationException("Tx: connection initialization failed!");
                    if (SMPPConn_Rx.Start() == false)
                        throw new ApplicationException("Rx: connection initialization failed!");
                    if (Open(new BindTransmitter()) == false)
                        throw new ApplicationException("TxOpen failed!");
                    if (Open(new BindReceiver()) == false)
                        throw new ApplicationException("RxOpen failed!");
                }
                catch (ApplicationException e)
                {
                    Logger.Write(new LogMessage("Exception in SMPPMessageManager Init method (Tx-Rx mode): " + e, LogSeverity.Error));
                }
            }
            // Initialize timer
            timer = new System.Timers.Timer(SMPPSettings.Check_timeout_interval);    // достаточно от 500 до 1000 ms.
            timer.Elapsed += CheckTimeouts;
            timer.Start();
        }

        /// <summary>
        ///  Инициализация кэша, в котором будут храниться сообщения, на которые ожидается отчет о доставке
        /// </summary>
        protected virtual void InitReceitsCache()
        {
            msgsWaitingReceits = new Cache();
            if (SMPPSettings.ReceitsExpirationTimeout > 0)
                msgsWaitingReceits._expirationTimeSpan = TimeSpan.FromMinutes(SMPPSettings.ReceitsExpirationTimeout);
            try
            {
                msgsWaitingReceits.ItemAddedOrRemoved += UpdateCacheCounter;
                LoadWaitingReceitsCache();
            }
            catch (Exception ex)
            {
                ServiceManager.LogEvent("Error while loading receits cache " + ex.Message, EventType.Error, EventSeverity.High);
            }
        }
        
        /// <summary>
        /// Из .dat файла загружает кэш с сообщениями,ожидающими отчета о доставке, сохраненный при остановке коннектора
        /// </summary>
        protected void LoadWaitingReceitsCache()
        {
            FileInfo execPath = new FileInfo(Assembly.GetExecutingAssembly().Location);
            string path = Path.Combine(execPath.Directory.FullName, "cache" + SMPPSettings.SMSCCode + ".dat");
            Dictionary<string, OutputMessage> dict = new Dictionary<string, OutputMessage>();
            Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Loading Cache ");
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    dict = (Dictionary<string, OutputMessage>)bf.Deserialize(fs);
                }

            }

            foreach (KeyValuePair<string, OutputMessage> pair in dict)
            {
                if (pair.Value != null)
                {
                    Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Adding to cache. Key " + pair.Key + " Value " + pair.Value);

                    if (!msgsWaitingReceits.Contains(pair.Key))
                    {
                        msgsWaitingReceits.Add<OutputMessage>(pair.Key, pair.Value);
                        Logger.Write(new LogMessage("Добавили в кэш " + pair.Key + " " + pair.Value, LogSeverity.Info));
                    }
                    else
                    {
                        //Logger.Write(new LogMessage("Item with key " +pair.Key+" already exists", LogSeverity.Info));
                        ServiceManager.LogEvent("Item with key " + pair.Key + " already exists", EventType.Warning, EventSeverity.Low);
                    }

                }
                else
                {

                    ServiceManager.LogEvent("Loading cache key " + pair.Key + " value is null",EventType.Debug,EventSeverity.Low);
                }
            }
        }

        /// <summary>
        ///Ждет, когда станет возможно отправить следующее сообщение
        /// </summary>
        /// <returns></returns>
        public override bool CanSendNextMessage()
        {
            if (ReadyToSendMessages.WaitOne())
            {
                SleepBeforeNextMessage();
                return true;
            }
            return false;
        }

        /// <summary>
        ///  выжидает таймаут между сообщениями или их частями
        /// </summary>
        protected void SleepBeforeNextMessage()
        {
            if (SMPPSettings.IsLimitedSpeed)
            {
                int RespTime = (int)(_connectorState._lastPDUOutRespTime - _connectorState._lastPDUOutTime).TotalMilliseconds;
                if ((SMPPSettings.TimePerSms - RespTime) > 0)
                    Thread.Sleep(SMPPSettings.TimePerSms - RespTime);
            }
            else
            {
                if (SMPPSettings.RepeatSendTimeout > 0)
                    Thread.Sleep(SMPPSettings.RepeatSendTimeout);
            }
        }
       
        /// <summary>
        /// лезет в базу, чтобы проверить, не заблокирован ли абонент
        /// </summary>
        /// <param name="msisdn"></param>
        /// <param name="sn"></param>
        /// <returns></returns>
        protected AbonentState CheckAbonentInBlackList(string msisdn, string sn)
        {
            try
            {
                using (SqlConnection conn = Database.Connection)
                {
                    using (SqlCommand comm = new SqlCommand("kernel.CheckAbonentInBlackList", conn))
                    {
                        comm.CommandType = System.Data.CommandType.StoredProcedure;
                        comm.Parameters.AddWithValue("@MSISDN", msisdn);
                        comm.Parameters.AddWithValue("@SN", sn);
                        int res = (int)comm.ExecuteScalar();
                        return res == 1 ? AbonentState.Blocked : AbonentState.NotBlocked;
                    }
                }
            }
            catch (SqlException sex)
            {
                Logger.Write(new LogMessage(sex.ToString(), LogSeverity.Error));
                return AbonentState.Unknown;
            }
        }

        /// <summary>
        /// Calls corresponding timeout controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CheckTimeouts(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            bool res;
            if (SMPPSettings.IsTransceiver)
                res = PDUTimeouts_TRx.CheckTimeout(TransferModes.TRx, ref SMPPConn_TRx);
            else
            {
                res = PDUTimeouts_Tx.CheckTimeout(TransferModes.Tx, ref SMPPConn_Tx);
                res &= PDUTimeouts_Rx.CheckTimeout(TransferModes.Rx, ref SMPPConn_Rx);
            }
            if (res) timer.Start();
        }

        /// <summary>            
        /// Should be called for finalization of SMPPMM
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            // Stop timers
            if (timer != null)
            {
                timer.Enabled = false;
            }
            // Close SMPP sequences
            if (PDUTimeouts_TRx != null && PDUTimeouts_TRx.IsBound)
            {
                Close(TransferModes.TRx, true);
            }
            if (PDUTimeouts_Tx != null && PDUTimeouts_Tx.IsBound)
            {
                Close(TransferModes.Tx, true);
            }
            if (PDUTimeouts_Rx != null && PDUTimeouts_Rx.IsBound)
            {
                Close(TransferModes.Rx, true);
            }
            // Wait for SMSC response
            Thread.Sleep(new TimeSpan(0, 0, 3));
            // Close TCP connections
            if (SMPPConn_TRx != null)
            {
                SMPPConn_TRx.Stop();
            }
            if (SMPPConn_Tx != null)
            {
                SMPPConn_Tx.Stop();
            }
            if (SMPPConn_Rx != null)
            {
                SMPPConn_Rx.Stop();
            }
            _connectorState.Close();
            SaveMessageCaches();
        }
       
        /// <summary>
        /// сохраняет кэш сообщений в dat-файл
        /// </summary>
        protected void SaveMessageCaches()
        {
            try
            {
                FileInfo execPath = new FileInfo(Assembly.GetExecutingAssembly().Location);
                string path = Path.Combine(execPath.Directory.FullName, "cache" + SMPPSettings.SMSCCode + ".dat");
                if (SMPPSettings.RegisteredDelivery == 1 && msgsWaitingReceits != null)
                {
                    Dictionary<string, OutputMessage> msgsToSave = new Dictionary<string, OutputMessage>();
                    foreach (string key in msgsWaitingReceits.CacheItems)
                    {
                        OutputMessage om;
                        if(  msgsWaitingReceits.GetItem<OutputMessage>(key, out om) && om!=null)
                          msgsToSave.Add(key, om);
                    }
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        bf.Serialize(fs, msgsToSave);
                    }
                    Logger.Write(new LogMessage("Сохранили кэш  в файл:", LogSeverity.Alert));
                }
                else
                    File.Delete(path);
            }

            catch (Exception ex)
            {
                Logger.Write(new LogMessage("Не смогли сохранить кэш  в файл:" + ex, LogSeverity.Alert));
                ServiceManager.LogEvent("Не смогли сохранить кэш  в файл:" + ex, EventType.Error, EventSeverity.Normal);
            }
        }

        /// <summary>
        /// TRx, Tx, Rx transfer mode initialization 
        /// 1. Initialize corresponding BIND_* PDU
        /// 2. Send corresponding BIND_* PDU to SMSC
        /// </summary>
        /// <returns>
        /// isOk flag
        /// </returns>
        /// 
        public bool Open(BindPacketBase bpb)
        {
            // 1. Initialize corresponding BIND_* PDU
            bpb.SystemId = SMPPSettings.SystemId;
            bpb.Password = SMPPSettings.Password;
            bpb.SystemType = SMPPSettings.SystemType;
            bpb.AddressRange.TON = SMPPSettings.BIND_TON;
            bpb.AddressRange.NPI = SMPPSettings.BIND_NPI;
            // 2. Send corresponding BIND_* PDU to SMSC
            TransferModes trMode = TransferModes.TRx;       // по умолчанию режим Transceiver
            
            if (bpb is BindTransceiver)
                trMode = TransferModes.TRx;
            else if (bpb is BindTransmitter)
                trMode = TransferModes.Tx;
            else if (bpb is BindReceiver)
                trMode = TransferModes.Rx;
            
            _connectorState.State = "Connecting";
            bool sendOk = EncodeAndSend(trMode, bpb);
            return sendOk;
        }

        /// <summary>
        /// подготавливает сообщение к отсылке 
        /// (загоняет его в SendMessage AbstractMM'ра и обновляет state)
        /// </summary>
        /// <returns>
        /// isOk flag
        /// </returns>
        public override bool SendUserData(OutputMessage outputMessage)
        {
            if (outputMessage == null) throw new ArgumentNullException("outputMessage");
            if (!CanSendNextMessage()) return false;
            ReadyToSendMessages.Reset();
            SendMessage = outputMessage;

            MultipartSendNextPart = false;
            _connectorState._lastReceivedFromRouterTime = DateTime.Now;

            pcSMSInQueueTime.RawValue =
                (long)(_connectorState._lastReceivedFromRouterTime - SendMessage.TimeReceived).TotalMilliseconds; //поправить

            return SendOutMessage();
        }

        /// <summary>
        ///  Отсылает SendMessage
        /// </summary>
        /// <returns></returns>
        protected virtual bool SendOutMessage()
        {
            if (SendMessage == null) return true;
            if (SendMessage.Content == null) return true;
            PDUTypesForUserData PDUType = 
                SMPPSettings.Send_by_DATA_SM ? PDUTypesForUserData.DATA_SM : PDUTypesForUserData.SUBMIT_SM;
            bool sendOk = false;
            ushort sarCount = 1;
            int shortMessageLength;
            int sar_segment_length;
            int max_message_length;
            PacketBase.dataCodingEnum dataCoding;
            try
            {
                byte[] userData = SendMessage.Content.Body;
                if (userData == null) return true;
                if (SendMessage.IsFlash) //флеш-смс
                {
                    dataCoding =
                       (PacketBase.dataCodingEnum)
                       Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_Flash);
                    shortMessageLength = SMPPSettings.Short_message_length_ru;

                    sar_segment_length = SMPPSettings.Sar_segment_length_ru;
                    max_message_length = SMPPSettings.Max_message_length_ru;
                }
                else if (((TextContent)SendMessage.Content).isUnicode)//юникод
                {
                    dataCoding =
                        (PacketBase.dataCodingEnum)
                        Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_unicode);
                    // Инициализация максимального размера короткого сообщения
                    shortMessageLength = SMPPSettings.Short_message_length_ru;
                    // Инициализация SAR параметров
                    sar_segment_length = SMPPSettings.Sar_segment_length_ru;
                    max_message_length = SMPPSettings.Max_message_length_ru;
                }
                else//все остальное
                {
                    dataCoding =
                        (PacketBase.dataCodingEnum)
                        Enum.Parse(typeof(PacketBase.dataCodingEnum), SMPPSettings.DataCoding_default);
                    // Инициализация максимального размера короткого сообщения
                    shortMessageLength = SMPPSettings.Short_message_length_eng;
                    // Инициализация SAR параметров
                    sar_segment_length = SMPPSettings.Sar_segment_length_eng;
                    max_message_length = SMPPSettings.Max_message_length_eng;
                }
                if (userData.Length > shortMessageLength && userData.Length <= max_message_length)
                // длинное сообщение (больше 1 части)
                {
                    // Разбить его на части по sar_segment_length байт 
                    sarCount = SMPPMessageParts.SplitToSAR(ConnectorId, userData, sar_segment_length);
                }
                switch (PDUType)
                {
                    case PDUTypesForUserData.SUBMIT_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength)
                        // короткое сообщение (до shortMessageLength байт) ограничение SMSC
                        {
                            SendMessage.PartsCount = 1;
                            SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
                            sm.ShortMessageLength = (byte)userData.Length;
                            sm.MessageText = userData;
                            sm.PartNumber = 1;
                            if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                                SetOperatorParameters(sm);
                            
                            sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);

                        }
                        else if (userData.Length > shortMessageLength && userData.Length <= max_message_length)
                        {
                            sendOk = SendSARMessage(dataCoding, sarCount, SMPPSettings.SendSarPartInPayload);
                        }
                        else if (userData.Length > max_message_length)
                        {

                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);
                            SendMessage = null;
                            ReadyToSendMessages.Set();
                        }
                        else
                        {
                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!",  LogSeverity.Debug));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ".  Message length = 0!", EventType.Error, EventSeverity.High);
                            SendMessage = null;
                            ReadyToSendMessages.Set();
                        }
                        break;
                    case PDUTypesForUserData.DATA_SM:
                        if (userData.Length > 0 && userData.Length <= shortMessageLength)
                        {
                            DataSM dm = Create_DATA_SM(SendMessage, dataCoding);
                            OptionalParameter op_data = new OptionalParameter();
                            op_data.Param = OptionalParameter.tagEnum.message_payload;
                            op_data.Value = userData;
                            dm.OptionalParamList.Add(op_data);
                            dm.PartNumber = 1;
                            sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx,
                                                   dm);

                        }
                        else if (userData.Length > shortMessageLength && userData.Length <= max_message_length)
                        // длинное сообщение (несколько SAR сегментов)
                        {
                            for (ushort i = 1; i <= sarCount; i++)
                            {
                                DataSM dm = Create_DATA_SM(SendMessage, dataCoding);
                                dm.OptionalParamList.Clear();
                                dm.PartNumber = i;
                                dm.OptionalParamList = SMPPMessageParts.GetOptionalParameterList(ConnectorId, i);
                                sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, dm);

                            }
                        }
                        else if (userData.Length > max_message_length)
                        {
                            // Log the System error 
                            if (Logger != null)
                            {
                                Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length is too large!", LogSeverity.Debug));

                            }
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ". Message length is too large!", EventType.Error, EventSeverity.High);

                            SendMessage = null;
                            ReadyToSendMessages.Set();
                        }
                        else
                        {
                            Logger.Write(new LogMessage("Error Message ID: " + SendMessage.ID + ". Message length = 0!", LogSeverity.Debug));
                            ServiceManager.LogEvent("Error Message ID: " + SendMessage.ID + ".  Message length = 0!!", EventType.Error, EventSeverity.High);
                            SendMessage = null;
                            ReadyToSendMessages.Set();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                sendOk = false;
            }
            return sendOk;
        }

       /// <summary>
       /// отсылает SAR-сообщение
       /// </summary>
       /// <param name="dataCoding"></param>
       /// <param name="sarCount"></param>
       /// <param name="SendSarPartInPayload"></param>
       /// <returns></returns>
        protected virtual bool SendSARMessage(PacketBase.dataCodingEnum dataCoding,ushort sarCount,bool SendSarPartInPayload)
        {
            bool sendOk=false;
            SendMessage.PartsCount = sarCount;
            
            for (ushort i = 1; i <= sarCount; i++)
            {
                SubmitSM sm = Create_SUBMIT_SM(SendMessage, dataCoding);
            
                sm.OptionalParamList.Clear();
                sm.PartNumber = i;
                sm.OptionalParamList = SMPPMessageParts.GetOptionalParameterList(ConnectorId, i);

                if (SendSarPartInPayload)
                {
                    sm.ShortMessageLength = 0;
                    sm.MessageText = new byte[0];
                }
                else
                {
                    OptionalParameter payload = sm.OptionalParamList.Single(item => item.Param == OptionalParameter.tagEnum.message_payload);
                    sm.OptionalParamList.Remove(payload);
                    sm.ShortMessageLength = (byte)payload.Value.Length;
                    sm.MessageText = payload.Value;
                } 

                if (!string.IsNullOrEmpty(SendMessage.HTTP_Category))
                {
                    SetOperatorParameters(sm);
                }

                sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, sm);

                if (!sendOk) break;
                if (!MultipartSendNextPart) break;
                
                SleepBeforeNextMessage();
            }

            return sendOk;
        }

        /// <summary>
        ///Из исходящего сообщения создает PDU Submit_sm
        /// </summary>
        /// <returns>Initialized SUBMIT_SM PDU</returns>
        protected virtual SubmitSM Create_SUBMIT_SM(OutputMessage outputMessage, PacketBase.dataCodingEnum dataCoding)
        {
            // Create SUBMIT_SM PDU
            SubmitSM sm = new SubmitSM();
            sm.Source.TON = SMPPSettings.Source_TON;
            sm.Source.NPI = SMPPSettings.Source_NPI;
            sm.Source.Address = outputMessage.Source ?? ""; // при тестовом подключении – трёхзначный номер аккаунта, в коммерческой эксплуатации – сервисный номер
            sm.Destination.TON = SMPPSettings.Destination_TON;
            sm.Destination.NPI = SMPPSettings.Destination_NPI;
            sm.Destination.Address = outputMessage.Destination ?? ""; // номер телефона в междунар. формате, например "7916xxxxxxx"
            sm.DataCoding = dataCoding;
            sm.ServiceType = SMPPSettings.ServiceType;
            sm.DefaultSMMessageId = SMPPSettings.DefaultSMMessageId;
            sm.MessageID = outputMessage.ID;
            sm.RegisteredDelivery = SMPPSettings.RegisteredDelivery;

            if (outputMessage.Type == MessageType.Silent)// Тихие сообщения
                sm.ProtocolId = 64;
            return sm;
        }

        /// <summary>
        /// Из исходящего сообщения создает PDU Data_sm
        /// </smary>
        /// <returns>Initialized DATA_SM PDU</returns>
        protected virtual DataSM Create_DATA_SM(OutputMessage outputMessage, PacketBase.dataCodingEnum dataCoding)
        {
            // Create DATA_SM PDU
            DataSM dm = new DataSM();
            dm.Source.TON = SMPPSettings.Source_TON;
            dm.Source.NPI = SMPPSettings.Source_NPI;
            dm.Source.Address = outputMessage.Source ?? ""; // при тестовом подключении – трёхзначный номер аккаунта, в коммерческой эксплуатации – сервисный номер
            dm.Destination.TON = SMPPSettings.Destination_TON;
            dm.Destination.NPI = SMPPSettings.Destination_NPI;
            dm.Destination.Address = outputMessage.Destination ?? ""; // номер телефона в междунар. формате, например "7916xxxxxxx"
            dm.DataCoding = dataCoding;
            dm.ServiceType = SMPPSettings.ServiceType;
            dm.RegisteredDelivery = SMPPSettings.RegisteredDelivery;
            dm.MessageID = outputMessage.ID;
            return dm;
        }

        /// <summary>
        /// обрабатывает отчет о доставке
        /// </summary>
        /// <param name="rec"></param>
        protected virtual void ProcessDeliveryReceit(DataPacketBase rec)
        {
            int msidx = rec.OptionalParamList.FindIndex(SMPPMessageParts.predicate_receipt_id);
            OutputMessage receitMsg;
            try
            {
                string msgId="";
                string status="";

                if (msidx > -1 && rec.OptionalParamList[msidx].Length > 0) //есть id сообщения
                {
                    byte[] msgid = rec.OptionalParamList[msidx].Value;
                   
                    msidx = rec.OptionalParamList.FindIndex(SMPPMessageParts.predicate_message_state);
                    if (msidx > -1)
                    {
                        if (rec.OptionalParamList[msidx].Length > 0) //есть статус сообщения
                        {
                            byte[] state = rec.OptionalParamList[msidx].Value;
                            status  = ((PacketBase.MessageState)state[0]).ToString();
                            msgId = (new ASCIIEncoding()).GetString(msgid);
                        }
                    }
                }
                else
                {
                        byte[] descr = new byte[1];
                        DeliverSM dsm = (DeliverSM)rec;
                        if (dsm.MessageText != null && dsm.MessageText.Length > 0)
                        {
                            descr = dsm.MessageText;
                        }
                        ASCIIEncoding enc = new ASCIIEncoding();
                        string description = enc.GetString(descr);
                        int ididx = description.IndexOf("id:");
                        msgId = description.Substring(ididx + 3, description.IndexOf(' ', ididx) - ididx - 3);
                        int stidx = description.IndexOf("stat:");
                        status = description.Substring(stidx + 5, description.IndexOf(' ', stidx) - stidx - 5);
                }
                msgId = msgId.Trim('\0');
                if (!msgsWaitingReceits.GetItem<OutputMessage>(msgId, out receitMsg))//отчет на не последнюю часть или старый
                {
                    UpdateTerminalDelivery(msgId, status, DateTime.Now, ConnectorId);
                    //throw new Exception("В кэше не найдено сообщение для отчета о доставке. MsgId=%" + msgId+"%");
                }
                else
                {
                    UpdateTerminalDelivery(receitMsg, msgId, status, DateTime.Now, ConnectorId);
                    msgsWaitingReceits.Remove(msgId);
                }
            }
            catch (Exception ex)
            {
                ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
            }
        }

        /// <summary>
        ///запрос статуса ранее отправленного сообщения с помощью PDU Query_sm
        /// </summary>
        /// <param name="message_id"></param>
        /// <param name="source_addr"></param>
        public override void QueryMessageState(string message_id, string source_addr)
        {
            QueryShortMessage qsm = new QueryShortMessage();
            qsm.SMSCMessageId = message_id;
            qsm.Source.TON = SMPPSettings.Source_TON;
            qsm.Source.NPI = SMPPSettings.Source_NPI;
            qsm.Source.Address = source_addr;
            EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, qsm);
            Logger.Write(new LogMessage("Отправили query_sm. messageid=" + message_id + " source_addr=" + source_addr, LogSeverity.Info));
        }

        /// <summary>
        /// Вычисляет длину пакета
        /// </summary>
        public static uint GetPacketLength(byte[] msg)
        {
            byte[] packlen = new byte[4];
            Array.Clear(packlen, 0, 4);
            packlen[0] = msg[0];
            packlen[1] = msg[1];
            packlen[2] = msg[2];
            packlen[3] = msg[3];
            return packlen[3] == 255 ? 0 : SupportOperations.FromBigEndianUInt(packlen);
        }

        /// <summary>
        /// Encode and send any PDU non-resp PDU, then wait for _resp
        /// </summary>
        /// <param name="trMode">SMPP transfer mode</param>
        /// <param name="pb">PDU to send</param>
        /// <returns>send status</returns>
        public virtual bool EncodeAndSend(TransferModes trMode, PacketBase pb)
        {
            bool sendOk = false;
            SMPPConnector conn = null;
            PDUTimeouts tmo = null;
            try
            {
                mut_Send.WaitOne();
                byte[] bytes = pb.GetEncoded();

                if (pb is DataPacketBase || pb is EnquireLink || pb is RegisterService)
                {

                    if (pb is SubmitSM || pb is DataSM)
                    {
                        // _currentSequenceNumber = pb.SequenceNumber;
                        pcOutgoingMessagesPerSecond.Increment();
                    }
                    _lastOutPDU = pb;
                }
                switch (trMode)
                {
                    case TransferModes.TRx:
                        conn = SMPPConn_TRx;
                        tmo = PDUTimeouts_TRx;
                        break;
                    case TransferModes.Tx:
                        conn = SMPPConn_Tx;
                        tmo = PDUTimeouts_Tx;
                        break;
                    case TransferModes.Rx:
                        conn = SMPPConn_Rx;
                        tmo = PDUTimeouts_Rx;
                        break;
                }

                if (conn != null)
                {
                    sendOk = conn.Send(bytes);

                    if (sendOk)
                    {
                        RegPDUTime(PDUTimeouts.PDUDirection.Out, ref tmo, pb, DateTime.Now);
                        if (pb.CommandId == PacketBase.commandIdEnum.data_sm || pb.CommandId == PacketBase.commandIdEnum.submit_sm || pb.CommandId == PacketBase.commandIdEnum.register_service)
                        {
                            //_connectorState.State = "Waiting SMSC response";

                            if (!waitingResp.WaitOne(tmo.RepeatPDU_TO, true))
                            {
                                _currentSequenceNumber = 0;
                                MultipartSendNextPart = false;
                                Logger.Write(new LogMessage("Не получили ответа от SMSC в течение " + tmo.RepeatPDU_TO.TotalSeconds + " секунд!", LogSeverity.Error));
                                ServiceManager.LogEvent("Не получили ответа от SMSC в течение " + tmo.RepeatPDU_TO.TotalSeconds + " секунд!", EventType.Warning, EventSeverity.Normal);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
            }
            finally
            {
                mut_Send.ReleaseMutex();
            }
            return sendOk;
        }
        
        /// <summary>
        /// Encode and send _resp PDU
        /// </summary>
        /// <param name="trMode">SMPP transfer mode</param>
        /// <param name="pb">PDU to send</param>
        /// <returns>send status</returns>
        public bool EncodeAndSendResp(TransferModes trMode, PacketBase pb)
        {
            // 1. Encode PDU
            bool sendOk = false;
            SMPPConnector conn = null;
            PDUTimeouts tmo = null;
            try
            {
                byte[] bytes = pb.GetEncoded();

                switch (trMode)
                {
                    case TransferModes.TRx:
                        conn = SMPPConn_TRx;
                        tmo = PDUTimeouts_TRx;
                        break;
                    case TransferModes.Tx:
                        conn = SMPPConn_Tx;
                        tmo = PDUTimeouts_Tx;
                        break;
                    case TransferModes.Rx:
                        conn = SMPPConn_Rx;
                        tmo = PDUTimeouts_Rx;
                        break;
                }

                if (conn != null) sendOk = conn.Send(bytes);
                
                Logger.Write(new LogMessage("Resp sent! CmndID: " + pb.CommandId.ToString() + ", Seq: " + pb.SequenceNumber.ToString() + ", Status: " + pb.StatusCode.ToString(), LogSeverity.Info));
                _connectorState._lastPDUInRespTime = DateTime.Now;
                SetPDUInResponseCounter();

                if (sendOk)
                    RegPDUTime(PDUTimeouts.PDUDirection.Out, ref tmo, pb, DateTime.Now);
            }
            catch (Exception ex)
            {
                Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));
            }
            return sendOk;
        }

        /// <summary>
        /// EnquireLink PDU sending 
        /// 1. Create ENQUIRE_LINK PDU 
        /// 2. Send ENQUIRE_LINK PDU to SMSC
        /// </summary>
        /// <returns>
        /// isOk flag
        /// </returns>
        public bool SendEnquireLink()
        {
            ReadyToSendMessages.Reset();
            EnquireLink el = new EnquireLink();
            bool sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, el);
            return sendOk;
        }

        /// <summary>
        /// прописать необходимые operator_parameters
        /// </summary>
        /// <param name="pb"></param>
        protected virtual void SetOperatorParameters(PacketBase pb)
        {

        }
   
        protected static MessageDeliveryStatus GetMessageStateString(PacketBase.MessageState code)
        {
            switch (code)
            {
                case PacketBase.MessageState.expired:
                case PacketBase.MessageState.undeliverable:
                    return MessageDeliveryStatus.Undelivered;
                case PacketBase.MessageState.rejected:
                    return MessageDeliveryStatus.LowBalance;
                case PacketBase.MessageState.delivered:
                    return MessageDeliveryStatus.Delivered;
            }
            return MessageDeliveryStatus.Unknown;
        }
        
        /// <summary>
        /// из статуса, отданного оператором в _resp PDU, получить статус в нашей системе
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected virtual MessageDeliveryStatus GetMessageStateString(PacketBase.commandStatusEnum code)
        {
            switch (code)
            {
                case PacketBase.commandStatusEnum.ESME_RTHROTTLED: //0x58
                case PacketBase.commandStatusEnum.ESME_R_MTS_SLA_ERR://0x442
                //case PacketBase.commandStatusEnum.ESME_RMSGQFUL://20 убрать!!! тестовое
                    return MessageDeliveryStatus.Unknown;

                case PacketBase.commandStatusEnum.ESME_RSYSERR://0x8
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_AbonentNotSubscribed://0x509
                case PacketBase.commandStatusEnum.ESME_R_Bercut_AbonentNotSubscribed://0x534
                    return MessageDeliveryStatus.Undelivered;

                case PacketBase.commandStatusEnum.ESME_ROK:
                    return MessageDeliveryStatus.Delivered;

                case PacketBase.commandStatusEnum.ESME_R_MTS_LowBalance://0x077
                case PacketBase.commandStatusEnum.ESME_R_MTS_SubscriberIsBlocked://0x44A  ??
                case PacketBase.commandStatusEnum.ESME_R_MTS_Tarification_Error://0x416// И это не 3...
                case PacketBase.commandStatusEnum.ESME_R_MTS_AccountIsBlocked://0x85
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_AbonentBlocked://0x510
                case PacketBase.commandStatusEnum.ESME_R_Bercut_AbonentBlocked://0x535 ???
                case PacketBase.commandStatusEnum.ESME_R_Bercut_NoTariffication://0x643 ??
                case PacketBase.commandStatusEnum.ESME_R_Bercut_NoChargeLevel://0x642  ?? ну это явно не 3....
                case PacketBase.commandStatusEnum.ESME_R_MTS_AbonentBlocked://0x44C
                case PacketBase.commandStatusEnum.ESME_R_MTS_PartialBlocked:
                    return MessageDeliveryStatus.LowBalance;

                case PacketBase.commandStatusEnum.ESME_R_MTS_ServiceNotActive://0x445
                case PacketBase.commandStatusEnum.ESME_R_MTS_NoRulesFound:
                case PacketBase.commandStatusEnum.ESME_R_MTS_LicenseViolated:
                case PacketBase.commandStatusEnum.ESME_R_MTS_ChargingSettings_ERR:
                case PacketBase.commandStatusEnum.ESME_R_MTS_CPNotFound:
                case PacketBase.commandStatusEnum.ESME_R_MTS_NoPullForPush:
                case PacketBase.commandStatusEnum.ESME_RSMSPUSHNOTSUPPORTED:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_DistributingServiceBlocked:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_DistributingServiceBlocked:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_IncorrectService:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_IncorrectService:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_PartnerBlocked:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_PartnerBlocked:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_PartnerBlockedForRegion:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_PartnerBlockedForRegion:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_RegionBlockedForPartner:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_RegionBlockedForService:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_RegionBlockedForService:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_RegionNotActive:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_RegionNotActive:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_RegionNotFound:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_RequestDistributingViolation:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_RequestDistributingViolation:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_ServiceBlockedForRegion:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_ServiceBlockedForRegion:
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_ServiceNotActive:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_ServiceNotActive:

                    return MessageDeliveryStatus.ServiceNotAvailable;
                case PacketBase.commandStatusEnum.ESME_R_MTS_UNKNOWN_SUBSCRIBER:
                //case PacketBase.commandStatusEnum.ESME_RINVSUBSCRIBER://1028
                case PacketBase.commandStatusEnum.ESME_RINVDSTADR:
                case PacketBase.commandStatusEnum.ESME_R_MTS_AccountNotActive:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_ServiceNumberBlockedForAbonent:
                case PacketBase.commandStatusEnum.ESME_R_MTS_AbonentInBlackList:
                    return MessageDeliveryStatus.AbonentNotExists;

                case PacketBase.commandStatusEnum.ESME_R_MTS_UNKNOWN_ACCOUNT://0x414 - 1044
                case PacketBase.commandStatusEnum.ESME_R_BEELINE_AccountNotRegistered:
                case PacketBase.commandStatusEnum.ESME_R_Bercut_AccountNotRegistered:
                case PacketBase.commandStatusEnum.ESME_R_MTS_InitialBlock://0x44D -1101
                case PacketBase.commandStatusEnum.ESME_R_MTS_FirstBlocked:
                case PacketBase.commandStatusEnum.ESME_R_MTS_AnotherBlocked:
                case PacketBase.commandStatusEnum.ESME_R_MTS_VoiceCallBlocked:
                    return MessageDeliveryStatus.UnknownAccount;
            }
            return MessageDeliveryStatus.Undelivered;
        }

        /// <summary>
        /// полученный массив байт декодировать в SMPP PDU и нужным образом обработать
        /// </summary>
        public virtual void Receive(SMPPConnector conn, int bytesCount, byte[] bytes)
        {
            // TODO: рефакторинг...
            int idx = 0;
            byte[] packlen = new byte[4];
            
            while (idx < bytesCount)
            {
                if (idx + 4 > bytesCount) break; //крайнемаловероятно

                Array.Clear(packlen, 0, 4);
                packlen[0] = bytes[idx];
                packlen[1] = bytes[idx + 1];
                packlen[2] = bytes[idx + 2];
                packlen[3] = bytes[idx + 3];
                
                if (packlen[3] == 255) // отвалился коннект
                    break;

                uint len = SupportOperations.FromBigEndianUInt(packlen);

                if (len <= 3) break;
                if (idx + len > bytesCount) break; //крайнемаловероятно

                byte[] packet = new byte[len];
                if (packet == null || packet.Length < 4)
                    Logger.Write(new LogMessage("out of memory or end of the array", LogSeverity.Error));
                
                Array.Copy(bytes, idx, packet, 0, len);

                PacketBase pb = new PacketBase();
                try
                {
                    pb.Parse(packet);
                }
                catch (Exception ex)
                {
                    ServiceManager.LogEvent(ex.ToString(), EventType.Error, EventSeverity.High);
                    Logger.Write(new LogMessage(ex.ToString(), LogSeverity.Error));

                    return;
                }
                idx += (int)len;
                Logger.Write(new LogMessage("SMPP packet parsed succesfully " + pb.CommandId + " Command length " + len + " Command status:" + pb.StatusCode, LogSeverity.Info));
                Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + "SMPP packet parsed succesfully " + pb.CommandId);

                if (pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp || pb.CommandId == PacketBase.commandIdEnum.data_sm_resp)
                {
                    if (_currentSequenceNumber != pb.SequenceNumber)
                    {

                        ServiceManager.LogEvent("Неверный номер последовательности: " + pb.SequenceNumber + " Ожидался: " + _currentSequenceNumber, EventType.Error, EventSeverity.High);
                        Logger.Write(new LogMessage("Неверный номер последовательности: " + pb.SequenceNumber + " Ожидался: " + _currentSequenceNumber, LogSeverity.Error));
                        return;
                    }
                }

                if (SMPPSettings.IsTransceiver)
                    RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_TRx, pb, DateTime.Now);
                else
                {
                    if (ReferenceEquals(conn, SMPPConn_Tx)) //режим трансмиттер
                        RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_Tx, pb, DateTime.Now);
                    else //режим ресивер
                    {
                        if (pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp ||
                            pb.CommandId == PacketBase.commandIdEnum.enquire_link ||
                            pb.CommandId == PacketBase.commandIdEnum.unbind_resp ||
                            pb.CommandId == PacketBase.commandIdEnum.unbind ||
                            pb.CommandId == PacketBase.commandIdEnum.generic_nack)
                        
                            RegPDUTime(PDUTimeouts.PDUDirection.In, ref PDUTimeouts_Rx, pb, DateTime.Now);
                    }
                }

                switch (pb.CommandId)
                {
                    case PacketBase.commandIdEnum.bind_transceiver_resp:

                        BindResponseTransceiver brt = new BindResponseTransceiver();
                        brt.Parse(packet);
                        if (brt.StatusCode == PacketBase.commandStatusEnum.ESME_ROK || brt.StatusCode == PacketBase.commandStatusEnum.ESME_RALYBND)
                        {
                            Logger.Write(new LogMessage("Connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, LogSeverity.Info));
                            ServiceManager.LogEvent("Connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, EventType.Info, EventSeverity.Normal);
                            pcConnectionState.RawValue = 1;
                            _connectorState.State = "Connected";
                            if (SendMessage == null) ReadyToSendMessages.Set();

                        }
                        else
                        {
                            pcConnectionState.RawValue = 0;
                            ReadyToSendMessages.Reset();
                            Logger.Write(new LogMessage("Not connected (transceiver) to " + brt.SystemId + "...Status = " + brt.StatusCode, LogSeverity.Info));
                            _connectorState.State = "Not connected";
                            ServiceManager.LogEvent("Not connected (transceiver) " + brt.SystemId + "...Status = " + brt.StatusCode, EventType.Warning, EventSeverity.High);
                        }
                        break;

                    case PacketBase.commandIdEnum.bind_transmitter_resp:

                        BindResponseTransmitter brt_t = new BindResponseTransmitter();
                        brt_t.Parse(packet);
                        if (brt_t.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        {
                            pcConnectionState.RawValue = 1;
                            ServiceManager.LogEvent("Connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, EventType.Info, EventSeverity.Normal);
                            Logger.Write(new LogMessage("Connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, LogSeverity.Info));
                            _connectorState.State = "Connected";
                            if (SendMessage == null) ReadyToSendMessages.Set();
                        }
                        else
                        {
                            pcConnectionState.RawValue = 0;
                            ServiceManager.LogEvent("Not connected (transmitter) to " + brt_t.SystemId + "...Status = " +
                                brt_t.StatusCode, EventType.Error, EventSeverity.High);
                            Logger.Write(new LogMessage("Not connected (transmitter) to " + brt_t.SystemId + "...Status = " + brt_t.StatusCode, LogSeverity.Error));
                            _connectorState.State = "Not connected";
                            ReadyToSendMessages.Reset();
                        }

                        break;
                    case PacketBase.commandIdEnum.bind_receiver_resp:

                        BindResponseReceiver brt_r = new BindResponseReceiver();
                        brt_r.Parse(packet);
                        if (brt_r.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        {
                            pcConnectionState.RawValue = 1;
                            ServiceManager.LogEvent("Connected (receiver) to " + brt_r.SystemId + "...Status = " +
                                brt_r.StatusCode, EventType.Info, EventSeverity.Normal);
                            Logger.Write(new LogMessage("Connected (receiver) to " + brt_r.SystemId + "...Status = " + brt_r.StatusCode, LogSeverity.Info));
                            _connectorState.State = "Connected";
                        }
                        else
                        {
                            pcConnectionState.RawValue = 0;
                            ServiceManager.LogEvent("Not connected (receiver) to " + brt_r.SystemId + "...Status = " +
                                brt_r.StatusCode, EventType.Error, EventSeverity.High);
                            Logger.Write(new LogMessage("Not connected (receiver) to " + brt_r.SystemId + "...Status = " + brt_r.StatusCode, LogSeverity.Error));
                            _connectorState.State = "Not connected";
                        }

                        break;
                    case PacketBase.commandIdEnum.data_sm:
                        {
                            DataSM dtsm = new DataSM();
                            dtsm.Parse(packet);
                            DataSMResponse dtsm_r = new DataSMResponse(ref dtsm);
                            EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, dtsm_r);
                            // SAR message processing
                            bool isSarMsg_DT = false;  //SAR или не SAR
                            ushort refNum_DT = 0; //Пришли ли все сегменты 
                            byte[] mergedMessage_DT = null;
                            bool[] flags = Cryptany.Common.Utils.Math.GetBitsArray(dtsm.EsmClass);
                            bool _isreceipt = flags[2];

                            if (_isreceipt) //логика для отчетов о доставке
                                ProcessDeliveryReceit(dtsm);
                            
                            else//входящее
                            {
                                foreach (OptionalParameter op in dtsm.OptionalParamList)
                                {
                                    if (op.Param == OptionalParameter.tagEnum.sar_msg_ref_num) // This is SAR message 
                                    {
                                        isSarMsg_DT = true;
                                        refNum_DT = SMPPMessageParts.PutMessagePart(dtsm);
                                        if (refNum_DT > 0) // пришли все SAR блоки сообщения с данным sar_msg_ref_num 
                                            mergedMessage_DT = SMPPMessageParts.MergeFromSAR(refNum_DT);
                                        break;
                                    }
                                }

                                if (dtsm.Source.Address != "" && (isSarMsg_DT == false || refNum_DT > 0))
                                {
                                    int SARCount = 0;
                                    byte[] userData = null;
                                    if (isSarMsg_DT == false) // не SAR сообщение
                                    {
                                        foreach (OptionalParameter op in dtsm.OptionalParamList)
                                        {
                                            if (op.Param == OptionalParameter.tagEnum.message_payload)
                                            {
                                                userData = op.Value;
                                                break;
                                            }
                                        }
                                    }
                                    else // SAR сообщение
                                    {
                                        foreach (OptionalParameter op in dtsm.OptionalParamList)
                                        {
                                            if (op.Param == OptionalParameter.tagEnum.sar_total_segments)
                                            {
                                                SARCount = op.Value[0];
                                                break;
                                            }
                                        }
                                        userData = mergedMessage_DT;
                                    }
                                    // Create MSMQ message and send it to the Router main input MSMQ queue
                                    if (userData == null) userData = Encoding.Default.GetBytes("");

                                    string MSISDN = "";
                                    string msgText = "";
                                    string serviceNumber = "";
                                    string transactionID = "";

                                    if (dtsm.Destination.Address.Contains("#"))
                                        transactionID = Message.GetTransactionId(dtsm.Destination.Address);
                                    else
                                        transactionID = Message.GetTransactionId(dtsm.Source.Address);
                                    MSISDN = Message.GetMSISDN(dtsm.Source.Address);
                                    serviceNumber = Message.GetServiceNumber(dtsm.Destination.Address);
                                    msgText = Enum.GetName(typeof(PacketBase.dataCodingEnum), dtsm.DataCoding) == SMPPSettings.DataCoding_unicode ?
                                        Encoding.BigEndianUnicode.GetString(userData) : Encoding.Default.GetString(userData);
                                    if (string.IsNullOrEmpty(serviceNumber) || string.IsNullOrEmpty(MSISDN))
                                    {
                                        ServiceManager.LogEvent("В сообщении не указан сервисный номер или msisdn", EventType.Error, EventSeverity.High);
                                        Logger.Write(new LogMessage("В сообщении не указан сервисный номер или msisdn", LogSeverity.Error));
                                        break;
                                    }
                                    // Create MSMQ message and send it to the Router main input MSMQ queue
                                    Message newMessage = new Message(IdGenerator.NewId, MSISDN, ConnectorId, serviceNumber, transactionID, msgText);
                                    Send_MSMQ_MessageToRouterInputQueue(newMessage);
                                }
                            }
                        }
                        break;
                    case PacketBase.commandIdEnum.data_sm_resp:

                        DataSMResponse dtsmr = new DataSMResponse();
                        dtsmr.Parse(bytes);
                        
                        DataSM pb_dtsm_fnd = _lastOutPDU as DataSM;

                        if (pb_dtsm_fnd != null && pb_dtsm_fnd is DataSM)
                        {
                            DataSM dtsm_fnd = pb_dtsm_fnd;
                            MessageDeliveryStatus mds = GetMessageStateString(dtsmr.StatusCode);
                            if (mds != MessageDeliveryStatus.Unknown)
                            {
                                if (SendMessage != null && SendMessage.ID == dtsm_fnd.MessageID &&
                                    ((SendMessage.PartsCount == dtsm_fnd.PartNumber) ||
                                     (mds != MessageDeliveryStatus.Delivered)))
                                {
                                    string message_id = dtsmr.MessageId;
                                    UpdateOutboxState(SendMessage, (int) GetMessageStateString(dtsmr.StatusCode),
                                                      dtsmr.RealStatusCode.ToString(), message_id);

                                    if (mds == MessageDeliveryStatus.Delivered && dtsm_fnd.RegisteredDelivery == 1)
                                    {
                                        msgsWaitingReceits.Add<OutputMessage>(message_id, SendMessage);
                                    }

                                }
                                _connectorState.State = "Connected";

                                SendMessage = null;
                                ReadyToSendMessages.Set();
                            }
                            else
                            {
                                AddOutboxSendHistory(dtsm_fnd.MessageID, ((int) mds), dtsmr.RealStatusCode.ToString());
                            }
                            _connectorState.State = "Connected";
                            waitingResp.Set();
                        }
                        break;

                    case PacketBase.commandIdEnum.deliver_sm: // либо сообщение, либо отчет о доставке :(
                       {
                            DeliverSM dlsm = new DeliverSM(); //commandId = Deliver_SM
                            dlsm.Parse(packet);
                            DeliverSMResponse dlsm_r = new DeliverSMResponse(ref dlsm); //commandId = deliver_sm_resp, seq = dlsm.seq
                            bool isSarMsg_DL = false;  //SAR или не SAR
                            ushort refNum_DL = 0;      //пришли ли все части
                            byte[] mergedMessage_DL = null;
                            
                            bool[] esmFlags = Cryptany.Common.Utils.Math.GetBitsArray(dlsm.EsmClass);
                            bool _isreceipt = esmFlags[2];
                            bool _isUDH = esmFlags[6];

                            if (_isreceipt) // отчет о доставке на терминал абонента
                            {
                                EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, dlsm_r);
                                ProcessDeliveryReceit(dlsm);
                            }
                            else // входящее сообщение
                            {
                                pcIncomingMessagesPerSecond.Increment();
                                _connectorState._lastSMSInTime = DateTime.Now;

                                bool continueProcessing = true;
                                if (ConfigurationManager.AppSettings["CheckInMessage"] != null && bool.Parse(ConfigurationManager.AppSettings["CheckInMessage"]))
                                    continueProcessing = CheckDeliverSM(dlsm, dlsm_r); //dlsm_r.StatusCode = OK

                                EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, dlsm_r);

                                if (!continueProcessing) break;
                                byte[] userData = null;

                                if (_isUDH)
                                {
                                    if (!UDHParser.Parse(dlsm)) // распарсить UDH - заголовок
                                        break;
                                }
                                foreach (OptionalParameter op in dlsm.OptionalParamList)
                                {
                                    if (op.Param == OptionalParameter.tagEnum.sar_msg_ref_num) // This is SAR message 
                                    {
                                        isSarMsg_DL = true;
                                        refNum_DL = SMPPMessageParts.PutMessagePart(dlsm);
                                        if (refNum_DL > 0) // пришли все SAR блоки сообщения с данным sar_msg_ref_num 
                                            mergedMessage_DL = SMPPMessageParts.MergeFromSAR(refNum_DL);
                                        break;
                                    }
                                }
                                // Send user data from DELIVER_SM PDU to the Router input queue for processing
                                if (dlsm.Source.Address != "" && (isSarMsg_DL == false || refNum_DL > 0))
                                {
                                    if (isSarMsg_DL == false) // не SAR сообщение
                                    {
                                        if (dlsm.ShortMessageLength > 0) // короткое сообщение
                                            userData = dlsm.MessageText;
                                        else  // длинное сообщение или кривое
                                        {
                                            foreach (OptionalParameter op in dlsm.OptionalParamList)
                                            {
                                                if (op.Param == OptionalParameter.tagEnum.message_payload)
                                                {
                                                    userData = op.Value;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else // SAR сообщение
                                        userData = mergedMessage_DL;
                                    
                                    pcInCounter.Increment();
                                    if (userData == null) userData = Encoding.Default.GetBytes("");

                                    string MSISDN = "";
                                    string msgText = "";
                                    string serviceNumber = "";
                                    string transactionID = "";

                                    if (dlsm.Destination.Address.Contains("#"))
                                        transactionID = Message.GetTransactionId(dlsm.Destination.Address);
                                    else
                                        transactionID = Message.GetTransactionId(dlsm.Source.Address);
                                    MSISDN = Message.GetMSISDN(dlsm.Source.Address);
                                    serviceNumber = Message.GetServiceNumber(dlsm.Destination.Address);

                                    msgText = Enum.GetName(typeof(PacketBase.dataCodingEnum), dlsm.DataCoding) == SMPPSettings.DataCoding_unicode ? Encoding.BigEndianUnicode.GetString(userData) : Encoding.Default.GetString(userData);

                                    if (string.IsNullOrEmpty(serviceNumber) || string.IsNullOrEmpty(MSISDN))
                                    {
                                        ServiceManager.LogEvent("В сообщении не указан сервисный номер или msisdn", EventType.Error, EventSeverity.High);
                                        Logger.Write(new LogMessage("В сообщении не указан сервисный номер или msisdn", LogSeverity.Error));
                                        break;
                                    }
                                    msgText = InjectMessageBody(msgText, dlsm_r);
                                    Message newMessage = new Message(IdGenerator.NewId, MSISDN, ConnectorId, serviceNumber, transactionID, msgText);
                                    // Create MSMQ message and send it to Router main input MSMQ queue
                                    Send_MSMQ_MessageToRouterInputQueue(newMessage);
                                }
                            }
                            break;
                        }
                    case PacketBase.commandIdEnum.enquire_link:

                        EnquireLink enqlnk = new EnquireLink();
                        enqlnk.Parse(packet);
                        EnquireLinkResponse enqlnk_r = new EnquireLinkResponse(ref enqlnk);
                        EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, enqlnk_r);
                        
                        break;
                    case PacketBase.commandIdEnum.enquire_link_resp:

                        EnquireLinkResponse enqlnkr = new EnquireLinkResponse();
                        enqlnkr.Parse(packet);
                        _connectorState.State = "Connected";

                        if (SendMessage == null) ReadyToSendMessages.Set();
                        break;
                    case PacketBase.commandIdEnum.submit_sm_resp:

                        SubmitSMResponse ssmr = new SubmitSMResponse();
                        ssmr.Parse(packet);
                        _connectorState.State = "Connected";

                        if (SendMessage != null)
                        {
                            PacketBase pb_ssm_fnd = _lastOutPDU;
                            if (pb_ssm_fnd != null && pb_ssm_fnd.MessageID == SendMessage.ID)
                            {
                                SetPDUOutResponseCounter();
                                SubmitSM ssm_fnd = (SubmitSM)pb_ssm_fnd;
                                string message_id = ssmr.MessageId;
                                message_id= message_id.Trim();

                                MessageDeliveryStatus mds = GetMessageStateString(ssmr.StatusCode);

                                if (mds != MessageDeliveryStatus.Unknown)
                                {
                                    if (ssm_fnd.PartNumber == SendMessage.PartsCount || mds != MessageDeliveryStatus.Delivered) //если отослали все части или на непоследнюю часть пришел финальный ответ
                                    {
                                        if (SMPPSettings.UseMessageState && SendMessage.InboxMsgID == Guid.Empty)
                                        {

                                            Cryptany.Core.Management.WMI.MessageState evt = new Cryptany.Core.Management.WMI.MessageState();

                                            evt.ID = SendMessage.ID.ToString();
                                            evt.Status = mds.ToString();
                                            evt.StatusDescription = ((int)ssmr.RealStatusCode).ToString();
                                            evt.StatusTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                            System.Messaging.Message message = new System.Messaging.Message(evt, new BinaryMessageFormatter());
                                            message.AttachSenderId = false;
                                            using (MessageQueue msgQueue = ServiceManager.MessageStateQueue)
                                            {
                                                msgQueue.Send(message);
                                            }
                                        }
                                        UpdateOutboxState(SendMessage, (int)mds, ssmr.RealStatusCode.ToString(), message_id);

                                        if (mds == MessageDeliveryStatus.Delivered && ssm_fnd.RegisteredDelivery == 1)
                                        {
                                            msgsWaitingReceits.Add<OutputMessage>(message_id, SendMessage);
                                            Trace.WriteLine("Connector " + SMPPSettings.SMSCCode + " Item added to cache. Key=%" + message_id + "%");

                                        }
                                        SendMessage = null;
                                        ReadyToSendMessages.Set();
                                    }
                                }
                                else
                                {
                                    AddOutboxSendHistory(SendMessage.ID, (int)mds, ssmr.RealStatusCode.ToString());
                                }

                                MultipartSendNextPart = (pb.StatusCode == PacketBase.commandStatusEnum.ESME_ROK);
                                waitingResp.Set();
                            }
                            else
                            {
                                ServiceManager.LogEvent("Не найден исходящий пакет с Id " + ssmr.MessageID, EventType.Error, EventSeverity.Normal);
                                Logger.Write( new LogMessage("Не найден исходящий пакет с Id " + ssmr.MessageID, LogSeverity.Error));
                            }
                        }
                        else
                        {
                            Logger.Write( new LogMessage("Получен ответ от SMSC, но не найдено отсылаемое сообщение", LogSeverity.Error));
                            ServiceManager.LogEvent("Получен ответ от SMSC, но не найдено отсылаемое сообщение", EventType.Error, EventSeverity.Normal);
                        }
                        break;
                        
                    case PacketBase.commandIdEnum.query_sm_resp://Пока не используется
                        QueryShortMessageResponse qsmr = new QueryShortMessageResponse();
                        if (qsmr.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                        {
                            int i = (int)qsmr.Parse(packet);
                            Logger.Write(new LogMessage("Parsed query_sm_resp mes_id=" + qsmr.MessageId + " final_date=" + qsmr.Final_Date.Date.ToString() + " message_state=" + qsmr.Message_State + " errorcode=" + qsmr.Error_Code, LogSeverity.Info));
                        }
                        break;

                    case PacketBase.commandIdEnum.unbind_resp:
                        _connectorState.State = "Not connected";
                        UnbindResponse ubr = new UnbindResponse();
                        ubr.Parse(packet);
                        break;

                    case PacketBase.commandIdEnum.unbind:
                        ReadyToSendMessages.Reset();
                        ServiceManager.LogEvent("Получили команду unbind", EventType.Warning, EventSeverity.Normal);
                        _connectorState.State = "Not connected";
                        Unbind ub = new Unbind();
                        ub.Parse(packet);
                        // Send UNBIND_RESP to SMSC
                        UnbindResponse ub_r = new UnbindResponse(ref ub);

                        if (SMPPSettings.IsTransceiver)
                        {
                            EncodeAndSendResp(TransferModes.TRx, ub_r);
                            // Clear bound flag
                            PDUTimeouts_TRx.IsBound = false;
                        }
                        else if (ReferenceEquals(conn, SMPPConn_Tx))
                        {
                            EncodeAndSendResp(TransferModes.Tx, ub_r);
                            // Clear bound flag
                            PDUTimeouts_Tx.IsBound = false;
                        }
                        else
                        {
                            EncodeAndSendResp(TransferModes.Rx, ub_r);
                            // Clear bound flag
                            PDUTimeouts_Rx.IsBound = false;
                        }
                        break;

                    case PacketBase.commandIdEnum.outbind:

                        ServiceManager.LogEvent("Получили команду outbind", EventType.Debug, EventSeverity.Normal); Logger.Write( new LogMessage("Получили команду outbind",
                                           LogSeverity.Debug));
                        break;

                    case PacketBase.commandIdEnum.generic_nack:
                        ReadyToSendMessages.Reset();
                        GenericNak gn = new GenericNak();
                        gn.Parse(packet);
                        ServiceManager.LogEvent("Получили команду generic_nack " + gn.StatusCode + " (" + gn.RealStatusCode + ")", EventType.Warning, EventSeverity.High);
                        break;

                    case PacketBase.commandIdEnum.register_service:
                        Trace.WriteLine("Получена команда register_service");
                        RegisterService rs = new RegisterService();
                        rs.Parse(packet);
                        // Скорее всего запрос на отписку, но надо уточнять
                        ProcessRegisterService(rs);
                        RegisterServiceResp resp = new RegisterServiceResp();
                        resp.SequenceNumber = rs.SequenceNumber;
                        resp.ServiceId = rs.ServiceId;
                        EncodeAndSendResp((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Rx, resp);
                        break;

                    case PacketBase.commandIdEnum.register_service_resp:
                        Trace.WriteLine("Получена команда register_service_resp");
                        RegisterServiceResp rsr = new RegisterServiceResp();
                        rsr.Parse(packet);
                        ProcessRegisterServiceResp(rsr);
                        waitingResp.Set();
                        break;
                }
            }
        }

        public virtual void ProcessRegisterService(RegisterService rs)
        {
            throw new NotImplementedException();
        }

        protected virtual void CreateAndSendRegisterServiceMessage(Message incomingMessage){ }

        /// <summary>
        /// прописать префикс к тексту 
        /// </summary>
        /// <param name="msgText"></param>
        /// <param name="dlsm_r"></param>
        /// <returns></returns>
        protected virtual string InjectMessageBody(string msgText, DeliverSMResponse dlsm_r)
        {
            return msgText;
        }

        /// <summary>
        /// создать запрос query_sm и отправить в smsc
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public override bool SendSMSCRequest(MessageStatusQuery query)
        {  //надо ли?
            QueryShortMessage qsm = new QueryShortMessage();
            qsm.SMSCMessageId = query.SMSCMsgId;
            qsm.Source.TON = SMPPSettings.Source_TON;
            qsm.Source.NPI = SMPPSettings.Source_NPI;
            qsm.Source.Address = query.source_addr;

            bool sendOk = EncodeAndSend((SMPPSettings.IsTransceiver) ? TransferModes.TRx : TransferModes.Tx, qsm);

            Thread.Sleep(SMPPSettings.RepeatSendTimeout);
            return sendOk;
        }

        public override bool SendSubscription(SubscriptionMessage subgmsg)
        {
            Logger.Write(new LogMessage("Команда подписки не поддерживается данным типом коннектора", LogSeverity.Error));
            return false;
        }

        /// <summary>
        /// Проверяет валидность атрибутов входящего смс.
        /// </summary>
        /// <param name="dlsm">Входящее смс</param>
        /// <param name="dlsm_r">Ответ платформе оператора на входящее смс</param>
        /// <returns>Необходима ли дальнейшая обработка сообщения?</returns>
        protected virtual bool CheckDeliverSM(DeliverSM dlsm, DeliverSMResponse dlsm_r)
        {
            dlsm_r.StatusCode = PacketBase.commandStatusEnum.ESME_ROK;
            return true;
        }

        /// <summary>
        /// Перепослать смс-сообщение целиком
        /// </summary>
        /// <returns>Результат перепосылки</returns>
        public bool ResendOutMessage()
        {
            ReadyToSendMessages.Reset(); // запрещаем отсылать новые сообщения, пока идет перепосылка
            _connectorState._lastResendTime = DateTime.Now;
            return SendOutMessage();
        }

        /// <summary> 
        /// отправляет сообщение в очередь роутера
        /// </summary>
        /// <returns>
        /// isOk flag
        /// </returns>
        public virtual bool Send_MSMQ_MessageToRouterInputQueue(Message newMessage)
        {
            bool isOk = false;
            try
            {
                // Поместить сообщение в общую входящую очередь обработчика сообщений
                using (MessageQueue MainInputSMSQueue = ServiceManager.MainInputSMSQueue)
                {
                    MainInputSMSQueue.Send(newMessage);
                }
                _connectorState._lastSentToRouterTime = DateTime.Now;
                base.SetPerformanceCounters();
                isOk = true;

            }
            catch (Exception e)
            {
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.Critical);
                Logger.Write(new LogMessage("Exception in SMPPMessageManager Send_MSMQ_MessageToRouterInputQueue method: " + e, LogSeverity.Error));
            }
            return isOk;
        }

        /// <summary>
        /// TRx, Tx, Rx transfer mode finalization
        /// 1. Creates UNBIND PDU
        /// 2. Sends UNBIND PDU to SMSC
        /// </summary>
        /// <returns>
        /// isOk flag
        /// </returns>
        public bool Close(TransferModes trMode, bool waitForResp)
        {

            ReadyToSendMessages.Reset();
            MultipartSendNextPart = false;
            // 1. Create UNBIND PDU
            Unbind ub = new Unbind();
            // 2. Send UNBIND PDU to SMSC
            bool sendOk = EncodeAndSend(trMode, ub);
            return sendOk;
        }
        /// <summary>
        /// регистрирует прием или отправку PDU - выставляет поля PDU Timeouts и   _connectorState
        /// </summary>
        /// <param name="pduDirect"></param>
        /// <param name="tmo"></param>
        /// <param name="pb"></param>
        /// <param name="dt"></param>
        public virtual void RegPDUTime(PDUTimeouts.PDUDirection pduDirect, ref PDUTimeouts tmo, PacketBase pb, DateTime dt)
        {

            if (pb == null) throw new ArgumentNullException("pb");
            if (tmo == null) throw new ArgumentNullException("tmo");
            if (pduDirect == PDUTimeouts.PDUDirection.In)
            {
                tmo.AnyInPDU_Time = dt;

                if (pb.CommandId == PacketBase.commandIdEnum.data_sm_resp || pb.CommandId == PacketBase.commandIdEnum.submit_sm_resp)
                {//начало обработки submit_sm_resp и data_sm_resp
                    if (_currentSequenceNumber == pb.SequenceNumber)
                    {
                        _connectorState._lastPDUOutRespTime = DateTime.Now;
                        //пришел ответ от SMSC, сбрасываем флаг ожидания ответа
                        tmo.NeedResp = false;
                        tmo.PDURetryCount = 0;
                        MessageDeliveryStatus mds = GetMessageStateString(pb.StatusCode);

                        if (mds == MessageDeliveryStatus.Unknown)//переполнение очереди
                        {
                            ServiceManager.LogEvent("Переполнена очередь на SMSC " + pb.StatusCode, EventType.Warning, EventSeverity.Low);
                            ReadyToSendMessages.Reset();

                            if (tmo.ErrorMessageId == Guid.Empty) //новая ошибка отправки
                            {
                                tmo.ErrorMessageId = SendMessage.ID;
                                tmo.ErrorResp_Time = dt;
                                tmo.ErrorRetryCount = 0;
                                tmo.ErrorPDU_TO = new TimeSpan(0, 0, SMPPSettings.ErrorPDU_TO_2);

                            }
                            else // повторение ошибки
                            {
                                // ошибка возникла первый раз
                                if (TimeSpan.Compare(tmo.ErrorPDU_TO, new TimeSpan(0, 0, SMPPSettings.ErrorPDU_TO_1)) == 0)
                                {
                                    tmo.ErrorPDU_TO = new TimeSpan(0, 0, SMPPSettings.ErrorPDU_TO_2);
                                }
                                // ошибка возникла второй раз и далее
                                else if (TimeSpan.Compare(tmo.ErrorPDU_TO, new TimeSpan(0, 0, SMPPSettings.ErrorPDU_TO_2)) == 0)
                                {
                                    tmo.ErrorPDU_TO = new TimeSpan(0, 0, SMPPSettings.ErrorPDU_TO_3);
                                }

                                // Обновить время прихода ошибки
                                tmo.ErrorResp_Time = dt;
                            }

                        }

                        DataPacketBase pb_last = _lastOutPDU as DataPacketBase;

                        if (mds != MessageDeliveryStatus.Unknown &&
                          SendMessage != null && pb_last != null &&
                            (pb_last.PartNumber == SendMessage.PartsCount || mds != MessageDeliveryStatus.Delivered))// сбросить флаг переполнения очереди
                        {

                            tmo.ErrorMessageId = Guid.Empty;
                            tmo.ErrorResp_Time = DateTime.MinValue;
                            tmo.ErrorRetryCount = 0;
                            tmo.ErrorPDU_TO = new TimeSpan(0, 0, SMPPSettings.ErrorPDU_TO_1);
                        }
        
                      
                    }

                    else
                    {
                        ServiceManager.LogEvent(string.Format("Неверный номер последовательности : {1} (ожидался {0})",
                                                              _currentSequenceNumber, pb.SequenceNumber), EventType.Error, EventSeverity.High);
                        Logger.Write(new LogMessage(string.Format("Неверный номер последовательности : {1} (ожидался {0})",
                                                              _currentSequenceNumber, pb.SequenceNumber), LogSeverity.Error));
                        return;
                    }


                }//конец обработки submit и data и register_service

                // BIND_TRANSCEIVER_RESP, BIND_TRANSMITTER_RESP, BIND_RECEIVER_RESP 
                if (pb.CommandId == PacketBase.commandIdEnum.bind_transceiver_resp ||
                    pb.CommandId == PacketBase.commandIdEnum.bind_transmitter_resp ||
                    pb.CommandId == PacketBase.commandIdEnum.bind_receiver_resp)
                {
                    if (pb.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                    {

                        tmo.IsBound = true;
                        return;
                    }
                    else
                    {

                        tmo.IsBound = false;
                    }
                }

                // UNBIND_RESP
                if (pb.CommandId == PacketBase.commandIdEnum.unbind_resp)
                {
                    if (pb.StatusCode == PacketBase.commandStatusEnum.ESME_ROK)
                    {

                        tmo.IsBound = false;
                    }
                }

                if (pb.CommandId == PacketBase.commandIdEnum.enquire_link_resp)
                {
                    if (_currentSequenceNumber == pb.SequenceNumber)
                    {
                        tmo.NeedResp = false;

                    }
                }

                if (pb.CommandId == PacketBase.commandIdEnum.register_service_resp)
                {
                    if (_currentSequenceNumber == pb.SequenceNumber)
                    {
                        tmo.NeedResp = false;

                    }
                }
            }
            if (pduDirect == PDUTimeouts.PDUDirection.Out)
            {

                switch (pb.CommandId)
                {
                    case PacketBase.commandIdEnum.submit_sm:
                    case PacketBase.commandIdEnum.data_sm:
                    case PacketBase.commandIdEnum.register_service:

                        waitingResp.Reset();
                        tmo.NeedResp = true;
                        tmo.DataOutPDU_Time = dt;
                        _connectorState._lastPDUOutTime = DateTime.Now;
                        break;

                    case PacketBase.commandIdEnum.enquire_link:
                        tmo.NeedResp = true;
                        tmo.DataOutPDU_Time = dt;
                        break;


                }

            }
        }

        public virtual void ProcessRegisterServiceResp(RegisterServiceResp rsr)
        {
            throw new NotImplementedException();
        }

        #region IMessageManager Members

        public override event EventHandler MessageReceived;

        public override event EventHandler MessageSent;

        public override event MessageStateChangedEventHandler MessageStateChanged;

        public override event StateChangedEventHandler StateChanged;

        public override event EventHandler SMSCActivity;

        #endregion
    }
}

























