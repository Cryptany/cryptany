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
using System.Text;
using System.Diagnostics;
using Cryptany.Common.Logging;
using Cryptany.Core.SmppLib;
using Cryptany.Core;
using Cryptany.Core.Management;
using System.Configuration;
using System.Xml;
using Cryptany.Core.Connectors;
using System.Messaging;
using System.Threading;
using Cryptany.Core.Interaction;


namespace Cryptany.Core
{
    /// <summary>
    /// универсальный класс для работы c SAR-сообщениями
    /// SAR - сообщения из нескольких частей
    /// </summary>
    public class SMPPMessageParts
    {
        private ushort m_referenceId;

        public ushort ReferenceId //счетчик SAR reference id
        {
            get { return m_referenceId; }
            set { m_referenceId = value; }
        }

        private ushort _min_referenceId;
        private ushort _max_referenceId=255;

        private bool _process_first_in_part;//Отправлять первую часть в роутер, не собирать части
        
        public struct messagePartEntry
        {
            private ushort referenceId;
            public ushort ReferenceId
            {
                get { return referenceId; }
                set { referenceId = value; }
            }

            private byte sectionId;
            public byte SectionId
            {
                get { return sectionId; }
                set { sectionId = value; }
            }

            private byte totalSection;
            public byte TotalSections
            {
                get { return totalSection; }
                set { totalSection = value; }
            }
            private byte[] partBody;

            public byte[] PartBody
            {
                get { return partBody; }
                set { partBody = value; }
            }
        }

        public Hashtable msgPartsCache;

        public SMPPMessageParts(SMPPSettings set)
        {
            msgPartsCache = new System.Collections.Hashtable();
            _process_first_in_part = set.SendFirstSARToRouter;
            ReferenceId = set.MinSarReferenceId;
            _min_referenceId = set.MinSarReferenceId;
            _max_referenceId = set.MaxSarReferenceId;
        }

		/// <summary>
		/// сохранить в кэше часть входящего сообщения
		/// </summary>
		/// <param name="imsg">message instance to put in</param>
		/// <param name="imsg"></param>
        /// <returns>refNum of the full message, otherwise 0x00</returns>
        public ushort PutMessagePart(MessagePacketBase imsg)
        {
            messagePartEntry pe = new messagePartEntry();
            int i;
            if ((i = imsg.OptionalParamList.FindIndex(predicate_sar_total)) != -1)
            {
                pe.TotalSections = imsg.OptionalParamList[i].Value[0];
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP: Cannot find SAR_total");
                throw new ArgumentOutOfRangeException("Wrong message part: no sar_total OP");
            }
            if ((i = imsg.OptionalParamList.FindIndex(predicate_sar_seqnum)) != -1)
            {
                pe.SectionId = imsg.OptionalParamList[i].Value[0];
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP1: Cannot find SAR_seqnum");
                throw new ArgumentOutOfRangeException("Wrong message part: no sar_seqnum OP");
            }
            if ((i = imsg.OptionalParamList.FindIndex(predicate_sar_refnum)) != -1)
            {
                pe.ReferenceId =SupportOperations.FromBigEndianUShort( imsg.OptionalParamList[i].Value);      // byte[2] {0x00, 0x01}  второй байт значащий
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP1: Cannot find SAR_refnum");
                throw new ArgumentOutOfRangeException("Wrong message part: no sar_refnum OP");
            }
            if ((i = imsg.OptionalParamList.FindIndex(predicate_message_payload)) != -1)
            {
                pe.PartBody = new byte[imsg.OptionalParamList[i].Length];
                imsg.OptionalParamList[i].Value.CopyTo(pe.PartBody, 0);
            }
            else
            {
                if  (  imsg.ShortMessageLength<255)
                pe.PartBody=imsg.MessageText;

                else throw new ArgumentOutOfRangeException("No message payload and wrong Short Message Length! Sm_length="+imsg.ShortMessageLength);
                   
            }
            List<messagePartEntry> mpeList;
            if (msgPartsCache.ContainsKey(pe.ReferenceId))
            {
                mpeList = (List<messagePartEntry>)msgPartsCache[pe.ReferenceId];
                mpeList.Add(pe);
                
            }
            else
            {
                mpeList = new List<messagePartEntry>();
                mpeList.Add(pe);
                msgPartsCache[pe.ReferenceId] = mpeList;
            }


            
            if (_process_first_in_part)
                return (pe.SectionId == 1) ? pe.ReferenceId : (ushort)0;

            return (mpeList.Count == pe.TotalSections) ? pe.ReferenceId : (ushort) 0;
        }

        /// <summary>
        /// сохранить в кэше часть входящего сообщения, переданного с помощью data_sm (не используется)
        /// </summary>
        /// <param name="imsg">message instance to put in</param>
        /// <returns>refNum of the full message, otherwise 0x00</returns>
        public ushort PutMessagePart(DataPacketBase imsg)
        {
            messagePartEntry pe = new messagePartEntry();
            int i;

            if ((i = imsg.OptionalParamList.FindIndex(predicate_sar_total)) != -1)
            {
                pe.TotalSections = imsg.OptionalParamList[i].Value[0];
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP2: Cannot find SAR_total");
                throw new ArgumentOutOfRangeException("Wrong message part: no sar_total OP");
            }

            if ((i = imsg.OptionalParamList.FindIndex(predicate_sar_seqnum)) != -1)
            {
                pe.SectionId = imsg.OptionalParamList[i].Value[0];
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP2: Cannot find SAR_seqnum");
                throw new ArgumentOutOfRangeException("Wrong message part: no sar_seqnum OP");
            }

            if ((i = imsg.OptionalParamList.FindIndex(predicate_sar_refnum)) != -1)
            {
                pe.ReferenceId =  SupportOperations.FromBigEndianUShort(  imsg.OptionalParamList[i].Value);      // byte[2] {0x00, 0x01}  второй байт значащий
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP2: Cannot find SAR_refnum");
                throw new ArgumentOutOfRangeException("Wrong message part: no sar_refnum OP");
            }

            if ((i = imsg.OptionalParamList.FindIndex(predicate_message_payload)) != -1)
            {
                pe.PartBody = new byte[imsg.OptionalParamList[i].Length];
                imsg.OptionalParamList[i].Value.CopyTo(pe.PartBody, 0);
            }
            else
            {
                Trace.WriteLine("SMPPMessageParts: PMP2: Cannot find message_payload");
                throw new ArgumentOutOfRangeException("Wrong message part: no message_payload OP");
            }
            List<messagePartEntry> mpeList;
            if (msgPartsCache.ContainsKey(pe.ReferenceId))
            {
                mpeList = (List<messagePartEntry>)msgPartsCache[pe.ReferenceId];
                ((List<messagePartEntry>)msgPartsCache[pe.ReferenceId]).Add(pe);
                
            }
            else
            {
                mpeList = new List<messagePartEntry>();
                mpeList.Add(pe);
                msgPartsCache[pe.ReferenceId] = mpeList;
            }
            return (mpeList.Count == pe.TotalSections) ? pe.ReferenceId : (ushort)0;
        }

        /// <summary>
        /// достать собранное сообщение по  его id 
        /// </summary>
        /// <param name="connectorId">Connector Id</param>
        /// <param name="refNum">sar_msg_ref_num of the merging message</param>
        /// <returns>merged user data</returns>
        public byte[] MergeFromSAR(ushort refNum)
        {
            int arraySize = 0;
            SortedList<int, byte[]> sl = new SortedList<int, byte[]>();
            List<messagePartEntry> li = (List<messagePartEntry>)msgPartsCache[refNum];
            foreach (messagePartEntry mpe in li)
            {
                if (sl.ContainsKey( mpe.SectionId) == false)
                {
                    arraySize += mpe.PartBody.Length;
                    sl.Add(mpe.SectionId, mpe.PartBody);
                }
                
            }
            if (arraySize > 0)
            {
                byte[] mergedData = new byte[arraySize];
                int pos = 0;
                foreach(KeyValuePair<int, byte[]> kvp in sl)
                {
                    kvp.Value.CopyTo(mergedData, pos);
                    pos += kvp.Value.Length;
                }
                msgPartsCache.Remove(refNum);
                return mergedData;
            }
            return null;
           
        }

        public static bool predicate_receipt_id(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.receipted_message_id)
                return true;
            return false;
            
        }

        public static bool predicate_message_state(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.message_state)
                return true;
            return false;
        }

        public static bool predicate_network_error_code(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.network_error_code)
                return true;
            return false;
            
        }
        public static bool predicate_sar_refnum(OptionalParameter p)
        {
            if (p.Param==OptionalParameter.tagEnum.sar_msg_ref_num)
                return true;
            return false;
            
        }

        public static bool predicate_sar_total(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.sar_total_segments)
                return true;
            return false;
            
        }

        public static bool predicate_sar_seqnum(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.sar_segment_seqnum)
                return true;
            return false;
            
        }

        public static bool predicate_message_payload(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.message_payload)
                return true;
            return false;
            
        }

        /// <summary>
        /// разбить на части исходящее сообщение и сохранить в кэше полученные опциональные параметры
        /// </summary>
        /// <param name="connectorId">connector id</param>
        /// <param name="bytes">user data byte array</param>
        /// <param name="Sar_segment_length">the size of SAR segment</param>
        /// <returns>SAR blocks quantity</returns>
        public ushort SplitToSAR(Guid connectorId, byte[] bytes, int Sar_segment_length)
        {
            int maxSARLen = Sar_segment_length;
            int lastSARLen = bytes.Length % maxSARLen;     // размер последнего доп. блока (если он нужен)
            //СМС с sar параметрами на СМСЦ переводятся в формат с UDH, то длина текстовой части СМС не должна превышать 134 октетов (140-6).
            //в GSM-default длина текста каждого СМС в несжатом виде должна быть не более 153 символов (134*8/7=153), а в юникоде 67 символов (134 /2=67). 
            ushort SARCount = (lastSARLen == 0) ? (ushort)(bytes.Length / maxSARLen) : (ushort)(bytes.Length / maxSARLen + 1);
            List<messagePartEntry> mpeList = new List<messagePartEntry>();
           
            ReferenceId++;

            if (ReferenceId > _max_referenceId)//smsc не поддерживают 2-х байтный номер
                ReferenceId = _min_referenceId;

             Trace.WriteLine(" ConnectorId "+connectorId+" Дробим на части sms " + bytes.Length + " кол-во сегментов: " + SARCount+" ref_num "+ReferenceId );

            for (int i = 0; i < SARCount; i++)
            {
                messagePartEntry pe = new messagePartEntry();

                pe.ReferenceId   =  ReferenceId;
                pe.TotalSections = (byte) SARCount;
                pe.SectionId     = (byte) (i + 1);
                int srcBeg = maxSARLen * i;
              
                if (i == SARCount - 1 && lastSARLen != 0)
                {
                    pe.PartBody = new byte[lastSARLen];
                    Array.Copy(bytes, srcBeg, pe.PartBody, 0, lastSARLen);
                }
                else
                {
                    pe.PartBody = new byte[maxSARLen];
                    Array.Copy(bytes, srcBeg, pe.PartBody, 0, maxSARLen);
                }
                mpeList.Add(pe);
            }
            msgPartsCache[connectorId] = mpeList;
            return SARCount;
        }

        /// <summary>
        ///Получить список опциональных параметров по номеру сегмента
        /// </summary>
        /// <param name="connectorId">connector id</param>
        /// <param name="seqnum">sar sequence number</param>
        /// <returns>list of smpp optional parameters, corresponding to sar blocks</returns>
        public List<OptionalParameter> GetOptionalParameterList(Guid connectorId, int seqnum)
        {
            List<messagePartEntry> li = (List<messagePartEntry>) msgPartsCache[connectorId];
            List<OptionalParameter> lp = new List<OptionalParameter>();
            OptionalParameter op_data = new OptionalParameter();                // пользовательские данные
            op_data.Param = OptionalParameter.tagEnum.message_payload;
            OptionalParameter op_sar_msg_ref_num = new OptionalParameter();     // идентификатор разделяемого сообщения
            op_sar_msg_ref_num.Param = OptionalParameter.tagEnum.sar_msg_ref_num;
            OptionalParameter op_sar_total_segments = new OptionalParameter();  // общее количество сегментов разделяемого сообщения
            op_sar_total_segments.Param = OptionalParameter.tagEnum.sar_total_segments;
            OptionalParameter op_sar_segment_seqnum = new OptionalParameter();  // номер сегмента разделяемого сообщения
            op_sar_segment_seqnum.Param = OptionalParameter.tagEnum.sar_segment_seqnum;
            messagePartEntry mpe = li[seqnum - 1];
            // 1. SAR optional parameters
            // 1.1 SAR reference id
            op_sar_msg_ref_num.Value = SupportOperations.ToBigEndian(mpe.ReferenceId);
            //new byte[2] {0x00, mpe.ReferenceId}; 
            lp.Add(op_sar_msg_ref_num);
            // 1.2 SAR segments count
            op_sar_total_segments.Value = new byte[1] {mpe.TotalSections};
            lp.Add(op_sar_total_segments);
            // 1.3 SAR segment number
            op_sar_segment_seqnum.Value = new byte[1] {mpe.SectionId};
            lp.Add(op_sar_segment_seqnum);
            // 1.4 SAR segment user data
            op_data.Value = mpe.PartBody;
            lp.Add(op_data);
            return lp;
        }
    }
    
    //best description ever!
    /// <summary>
    /// All SMPP settings 
    /// </summary>
    public class SMPPSettings : AbstractConnectorSettings
    {
        //снова useless shit
        public SMPPSettings(int SMSCCode)
            : base(SMSCCode)
        {
        }

        //чтение настроек из xml-файла
        protected override void InitSettings(XmlDocument settings)
        {
            //а в классе настроек http коннектора, этой проверки нет
            if (settings == null) throw new ArgumentNullException("settings");

            if (settings["common"] == null) throw new ApplicationException("Не задан элемент common в настройках");
            IEnumerator ienum = settings["common"].Attributes.GetEnumerator();
            while (ienum.MoveNext())
            {
                XmlAttribute attr = ((XmlAttribute)ienum.Current);
                try
                {
                    //задрот
                    Type fieldType = GetType().GetField(attr.Name).FieldType;
                    GetType().GetField(attr.Name).SetValue(this, System.ComponentModel.TypeDescriptor.GetConverter(fieldType).ConvertFromString(attr.Value));
                }
                catch { }

            }
        }

        //опять public-поля =(
        public bool IsTransceiver = true;
        public string SystemType = "";
        public int SocketDataBufferSize = 1024;
        public bool LoggingEnabled = true;

        public int Short_message_length_eng = 161;
        public int Sar_segment_length_eng = 161;
        public int Max_message_length_eng = 256;
        public int Short_message_length_ru = 140;
        public int Sar_segment_length_ru = 153;
        public int Max_message_length_ru = 1400;
        public int Check_timeout_interval = 15000;

        public byte BIND_TON;
        public byte BIND_NPI;
        public byte Source_TON = 1;
        public byte Source_NPI;
        public byte Destination_TON = 1;
        public byte Destination_NPI = 1;

        public string DataCoding_default = PacketBase.dataCodingEnum.defaultAlphabet.ToString();
        public string DataCoding_unicode = PacketBase.dataCodingEnum.dcUCS2.ToString();
        public string DataCoding_Flash = PacketBase.dataCodingEnum.dcFlashUnicode.ToString();
        public string ServiceType = "";
        public byte DefaultSMMessageId;
        public byte RegisteredDelivery;
        public int ConnBroken_TO_1 = 60;
        public int ConnBroken_TO_2 = 90;
        public int EnquireLink_TO = 40;
        public int RepeatPDU_TO = 30;
        public int ErrorPDU_TO_1 = 10;
        public int ErrorPDU_TO_2 = 10;
        public int ErrorPDU_TO_3 = 10;
        public int Max_NeedResp_PDURetryCount = 4;
        public int Max_Error_PDURetryCount = 2;
        public int Max_Connect_RetryCount = 600;// получается около 24 часов при попытках 10 раз в час
        public int MaxNonRespPDU = 25;
        public bool Send_by_DATA_SM;
        public bool TransactionIdInMSISDN;
        public int Max_internal_queue_size = 500;
        public bool IsLimitedSpeed;
        public int TimePerSms;
        public bool SendFirstSARToRouter = false;
        public ushort MinSarReferenceId=0;
        public ushort MaxSarReferenceId=255;
        public int ReceitsExpirationTimeout;
        public bool SendSarPartInPayload = true;
    }

    /// <summary>
    /// хранит информацию об отправленном PDU и связанное с ним исходящее сообщение 
    /// </summary>
    public class PacketInfo
    {
        public PacketBase Packet { get; set; }
        public DateTime SentTime { get; set; }
        public DateTime ReceivedTime { get; set; }
        
        public PDUTimeouts.PDUDirection direction { get; set; }
        
        OutputMessage msg;
        public OutputMessage Message
        {
            get { return msg; }
        }

        public PacketInfo(PacketBase pb, DateTime time, PDUTimeouts.PDUDirection dir)
        {
            Packet = pb;
            direction = dir;

            if (dir == PDUTimeouts.PDUDirection.In)
                ReceivedTime = time;

            if (dir == PDUTimeouts.PDUDirection.Out)
                SentTime = time;
        }

        public PacketInfo(PacketBase pb, OutputMessage msg, DateTime time, PDUTimeouts.PDUDirection dir)
        {
            Packet = pb;
            this.msg = msg;
            direction = dir;
            if (dir == PDUTimeouts.PDUDirection.In)
                ReceivedTime = time;

            if (dir == PDUTimeouts.PDUDirection.Out)
                SentTime = time;
        }
    }

    /// <summary>
    /// очередь для хранения в памяти отправленных PDU
    /// </summary>
    public class InternalConnectorQueue
    {
        List<PacketInfo> _infos = new List<PacketInfo>();

        public readonly int MaxQueueSize;

        public InternalConnectorQueue(int maxQueueSize)
        {
            MaxQueueSize = maxQueueSize;
        }

        public void AddPacket(PacketInfo value)
        {

            lock (_infos)
            {
                _infos.Add(value);
            }
        }

        public PacketInfo this[int i]
        {
            get { return _infos[i]; }
            set { _infos[i] = value; }
        }

        public List<PacketInfo> Infos
        {
            get { return _infos; }

        }
        public int Count
        {
            get { return _infos.Count; }
        }

        public int RemoveOutPackets(Guid MsgId)
        {
            lock (_infos)
            {
                return _infos.RemoveAll(delegate(PacketInfo info)
                    //странности...
                { return ((info as PacketInfo).Message != null && (info as PacketInfo).Message.ID == MsgId); });
            }
        }

        public bool Remove(PacketInfo pi)
        {
            lock (_infos)
            {
                return _infos.Remove(pi);
            }
        }

        /// <summary>
        ///найти отправленный PDU по id сообщения
        /// </summary>
        /// <param name="MsgId"></param>
        /// <returns></returns>
        public PacketInfo FindOutPacket(Guid MsgId)
        {

            foreach (PacketInfo info in _infos)
            {
                if (info.Message != null && info.Message.ID == MsgId)
                    return info;
            }
            return null;

        }

    }

    /// <summary>
    ///  класс для проверки таймаутов
    /// </summary>
    public class PDUTimeouts
    {
        //опять public-поля >_<
        public bool IsBound;                      //флаг установленного SMPP подключения
        public enum PDUDirection { In, Out };

        protected SMPPMessageManager m_SMPPmm;

        public SMPPMessageManager SMPPmm
        {
            get { return m_SMPPmm; }
            set { m_SMPPmm = value; }
        }

        protected bool m_needResp;

        public bool NeedResp
        {
            get { return m_needResp; }
            set { m_needResp = value; }
        }

        public Guid ErrorMessageId;
        public int ConnRetryCount;          // счетчик попыток повторного соединения после обрыва связи
        public int PDURetryCount;           // счетчик попыток повторных отправок SUBMIT_SM PDU или DATA_SM PDU в случае отсутствия SUBMIT_SM_RESP PDU или DATA_SM_RESP PDU
        public int ErrorRetryCount;         // счетчик попыток повторных отправок SUBMIT_SM PDU или DATA_SM PDU в случае получения *_RESP PDU со статусом ошибки
        public DateTime ConnBroken_Time;    // время обрыва соединения по TCP и (или) SMPP
        public DateTime AnyInPDU_Time;      // время получения последнего любого PDU: вызывает ENQUIRE_LINK 
        public DateTime DataOutPDU_Time;    // время отправки последнего не *_RESP PDU: вызывает повтор отправки PDU
        public DateTime ErrorResp_Time;     // время получения *_RESP PDU с сообщением о переполнении очереди
        public TimeSpan ConnBroken_TO;      // после обрыва соединения: таймаут повторной попытки подключения BIND_TRANSCEIVER
        public TimeSpan EnquireLink_TO;     // при отсутствии приема любых PDU: таймаут, инициирующий посылку ENQUIRE_LINK
        public TimeSpan RepeatPDU_TO;       // при отсутствии приема *_RESP PDU: таймаут, инициирующий повторную посылку любого PDU
        public TimeSpan ErrorPDU_TO;        // интервал повторных отправок SUBMIT_SM PDU или DATA_SM PDU в случае получения *_RESP PDU со статусом ошибки

        /// <summary>
        /// Timeouts initialization
        /// </summary>
        public PDUTimeouts(SMPPMessageManager smppmm)
        {
            SMPPmm = smppmm;
            ConnBroken_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.ConnBroken_TO_1);
            EnquireLink_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.EnquireLink_TO);
            RepeatPDU_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.RepeatPDU_TO);
            ErrorPDU_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.ErrorPDU_TO_1);
            ConnBroken_Time = AnyInPDU_Time = DataOutPDU_Time = ErrorResp_Time = DateTime.MinValue;
            ConnRetryCount = PDURetryCount = ErrorRetryCount = 0;
        }

        public bool CheckTimeout(SMPPMessageManager.TransferModes trMode, ref SMPPConnector conn)
        {
            BindPacketBase bpb = null;
            try
            {
                bool checkSocketConnection = false;
                switch (trMode)
                {
                    case SMPPMessageManager.TransferModes.TRx:
                        bpb = new BindTransceiver();
                        checkSocketConnection = true;
                        break;
                    case SMPPMessageManager.TransferModes.Tx:
                        bpb = new BindTransmitter();
                        break;
                    case SMPPMessageManager.TransferModes.Rx:
                        bpb = new BindReceiver();
                        checkSocketConnection = true;
                        break;
                }

                // Проверить состояние TCP и SMPP соединения
                if ((checkSocketConnection && !conn.SocketConnected) || !IsBound)    // TCP and (or) SMPP connection is broken
                {
                    IsBound = false;
                    SMPPmm.ReadyToSendMessages.Reset();

                    if (SMPPmm.LastOutPdu is EnquireLink)//чтобы 2 раза не реконнектиться
                    {
                        SMPPmm.LastOutPdu = null;
                        NeedResp = false;
                        DataOutPDU_Time = DateTime.MinValue;
                    }

                    if (ConnBroken_Time == DateTime.MinValue)// остановить коннектор
                    {
                        SMPPmm.Logger.Write(new LogMessage("Connection broken!", LogSeverity.Alert));
                        ServiceManager.LogEvent(" Connection broken!", EventType.Warning, EventSeverity.High);
                        ConnBroken_Time = DateTime.Now;
                        conn.Stop();
                        conn = null;
                        conn = new SMPPConnector(SMPPmm.SMPPSettings.IPAddress, SMPPmm.SMPPSettings.Port, SMPPmm.SMPPSettings.SystemId, SMPPmm.SMPPSettings.Password, SMPPmm);
                    }
                    else
                    {
                        if ((DateTime.Now - ConnBroken_Time) > ConnBroken_TO)
                        {
                            ConnRetryCount++;
                            ConnBroken_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.ConnBroken_TO_2);

                            if (ConnRetryCount <= SMPPmm.SMPPSettings.Max_Connect_RetryCount) // Пропубуем законнектиться
                            {
                                SMPPmm.Logger.Write(new LogMessage("Reconnecting...", LogSeverity.Alert));
                                bool connOk = conn.Start();
                                bool openOk = SMPPmm.Open(bpb);

                                Thread.Sleep(new TimeSpan(0, 0, 5));
                                if (connOk == false || openOk == false || IsBound == false)
                                {
                                    ServiceManager.LogEvent("Failed to reconnect!", EventType.Warning, EventSeverity.High);
                                    ConnBroken_Time = DateTime.MinValue;
                                }
                            }
                            else
                            {

                                SMPPmm.Logger.Write(new LogMessage("ConnectorId = " + SMPPmm.ConnectorId + ". Transfer mode: " + Enum.GetName(typeof(SMPPMessageManager.TransferModes), trMode) + ". Can not initiate TCP or SMPP connection!", LogSeverity.CriticalError));
                                ServiceManager.LogEvent("ConnectorI = " + SMPPmm.ConnectorId + ". Transfer mode: " + Enum.GetName(typeof(SMPPMessageManager.TransferModes), trMode) + ". Can not initiate TCP or SMPP connection!", EventType.Warning, EventSeverity.Critical);
                                return false;
                            }
                        }
                    }
                }
                else                                                       // SMPP and TCP connection is Ok
                {
                    ConnBroken_Time = DateTime.MinValue;
                    ConnRetryCount = 0;
                    ConnBroken_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.ConnBroken_TO_1);

                    bool IsAsyncMode = (SMPPmm is SMPPMessageManagerAsync);

                    if (trMode == SMPPMessageManager.TransferModes.Rx)
                    {
                        // достаточно
                        return true;
                    }

                    if (!IsAsyncMode)// синхронный режим
                    {
                        if (NeedResp && DataOutPDU_Time != DateTime.MinValue && (DateTime.Now - DataOutPDU_Time) > RepeatPDU_TO)
                        {
                            PacketBase pb = SMPPmm.LastOutPdu;

                            if (pb != null && pb.CommandId == PacketBase.commandIdEnum.enquire_link && SMPPmm.SendMessage == null)   // нет ответа на ENQUIRE_LINK PDU  в синхр. режиме 
                            {
                                if (SMPPmm.PDUTimeouts_TRx != null)
                                {
                                    SMPPmm.PDUTimeouts_TRx.IsBound = false;
                                }
                                else
                                {
                                    SMPPmm.PDUTimeouts_Tx.IsBound = SMPPmm.PDUTimeouts_Rx.IsBound = false;
                                }
                                SMPPmm.Logger.Write(new LogMessage("Превышен таймаут ожидания ответа на enquire_link", LogSeverity.Debug));
                                ServiceManager.LogEvent("ConnectorId = " + SMPPmm.ConnectorId + ". Transfer mode: " + Enum.GetName(typeof(SMPPMessageManager.TransferModes), trMode) + ". Превышен таймаут ожидания ответа на enq_link", EventType.Error, EventSeverity.High);
                                SMPPmm.ReadyToSendMessages.Reset();
                                //  DataOutPDU_Time = DateTime.MinValue;// чтобы этот блок не срабатывал все время
                                return true;

                            }

                            if (pb != null && (pb.CommandId == PacketBase.commandIdEnum.submit_sm || pb.CommandId == PacketBase.commandIdEnum.data_sm))   // нет ответа на SUBMIT_SM PDU или DATA_SM PDU
                            {
                                PDURetryCount++;

                                if (PDURetryCount > SMPPmm.SMPPSettings.Max_NeedResp_PDURetryCount)       // не разрешенная повторная попытка отправки PDU
                                {
                                    if (SMPPmm.PDUTimeouts_TRx != null)
                                    {
                                        SMPPmm.PDUTimeouts_TRx.IsBound = false;
                                    }
                                    else
                                    {
                                        SMPPmm.PDUTimeouts_Tx.IsBound = SMPPmm.PDUTimeouts_Rx.IsBound = false;
                                    }

                                    SMPPmm.UpdateOutboxState(SMPPmm.SendMessage, (int)MessageDeliveryStatus.Undelivered, "max_no_resp", "");
                                    SMPPmm.SendMessage = null;
                                    SMPPmm.LastOutPdu = null;
                                    SMPPmm.ReadyToSendMessages.Set();
                                    ServiceManager.LogEvent("Закончились попытки ожидания ответа на пакет PDURetryCount=" + PDURetryCount, EventType.Error, EventSeverity.High);
                                    SMPPmm.Logger.Write(new LogMessage("Закончились попытки ожидания ответа на пакет PDURetryCount=" + PDURetryCount, LogSeverity.Debug));
                                    PDURetryCount = 0;
                                }
                                else     // разрешенная повторная попытка отправки PDU
                                {
                                    // Перепослать сообщение целиком
                                    SMPPmm.ResendOutMessage();

                                }
                                return true;
                            }
                            if (pb != null && (pb.CommandId == PacketBase.commandIdEnum.register_service))
                            {
                                //if (SMPPmm.PDUTimeouts_TRx != null)
                                //{
                                //    SMPPmm.PDUTimeouts_TRx.IsBound = false;
                                //}
                                //else
                                //{
                                //    SMPPmm.PDUTimeouts_Tx.IsBound = SMPPmm.PDUTimeouts_Rx.IsBound = false;
                                //}


                                SMPPmm.Logger.Write(new LogMessage("Error while checking timeouts. По таймауту отвалился RegisterService ", LogSeverity.Error));
                                
                                SMPPmm.LastOutPdu = null;
                                SMPPmm.ReadyToSendMessages.Set();
                                PDURetryCount = 0;
                            }
                            else
                            {
                                SMPPmm.Logger.Write(new LogMessage("Error while checking timeouts. Ожидаем ответ, не найден пакет для перепосылки. LastOutPDU command id  " + pb.CommandId, LogSeverity.Error));
                                ServiceManager.LogEvent("Error while checking timeouts. Ожидаем ответ, не найден пакет для перепосылки ", EventType.Error, EventSeverity.High);

                            }

                        }
                        // Не послать ли enquire_link
                        if (!NeedResp && (SMPPmm.SendMessage == null) && AnyInPDU_Time != DateTime.MinValue && (DateTime.Now - AnyInPDU_Time) > EnquireLink_TO)
                        {
                            SMPPmm.Logger.Write(new LogMessage("Отправляем enquire_link по таймауту", LogSeverity.Debug));
                            SMPPmm.SendEnquireLink();
                            return true;
                        }

                        // было получено сообщение о переполнении очереди
                        if (!NeedResp && (SMPPmm.SendMessage != null) && ErrorMessageId != Guid.Empty &&
                            ErrorResp_Time != DateTime.MinValue && (DateTime.Now - ErrorResp_Time) > ErrorPDU_TO)
                        {

                            if (ErrorMessageId != SMPPmm.SendMessage.ID)
                            {
                                ServiceManager.LogEvent("не найдено исходящее смс со статусом ошибки ErrorMessageId=" + ErrorMessageId, EventType.Error, EventSeverity.High);
                                SMPPmm.Logger.Write(new LogMessage("не найдено исходящее смс со статусом ошибки ErrorMessageId=" + ErrorMessageId, LogSeverity.Error));
                                return true;
                            }

                            ErrorRetryCount++;
                            if (ErrorRetryCount > SMPPmm.SMPPSettings.Max_Error_PDURetryCount)//больше не посылаем
                            {

                                SMPPmm.UpdateOutboxState(SMPPmm.SendMessage, (int)MessageDeliveryStatus.Undelivered, "sla", "");

                                if (SMPPmm.SMPPSettings.UseMessageState && SMPPmm.SendMessage.InboxMsgID == Guid.Empty)
                                {

                                    Cryptany.Core.Management.WMI.MessageState evt = new Cryptany.Core.Management.WMI.MessageState();

                                    evt.ID = ErrorMessageId.ToString();
                                    evt.Status = MessageDeliveryStatus.Undelivered.ToString();
                                    evt.StatusDescription = "SLA_ERR";
                                    evt.StatusTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    System.Messaging.Message message = new System.Messaging.Message(evt, new BinaryMessageFormatter());
                                    message.AttachSenderId = false;
                                    using (MessageQueue msgQueue = ServiceManager.MessageStateQueue)
                                    {
                                        msgQueue.Send(message);
                                    }
                                }

                                ErrorMessageId = Guid.Empty;
                                ErrorRetryCount = 0;
                                ErrorResp_Time = DateTime.MinValue;
                                ErrorPDU_TO = new TimeSpan(0, 0, SMPPmm.SMPPSettings.ErrorPDU_TO_1);
                                SMPPmm.SendMessage = null;
                                SMPPmm.LastOutPdu = null;
                                SMPPmm.ReadyToSendMessages.Set();
                                SMPPmm.Logger.Write(new LogMessage("Закончились попытки перепосылки сообщения со статусом ошибки", LogSeverity.Info));

                            }
                            else
                            {

                                SMPPmm.Logger.Write(new LogMessage("Пересылаем сообщение со статусом ошибки ", LogSeverity.Info));
                                SMPPmm.ResendOutMessage();
                                return true;
                            }
                        }
                    }


                    if (IsAsyncMode)//асинхронный режим
                    {

                        if (NeedResp && DataOutPDU_Time != DateTime.MinValue && (DateTime.Now - DataOutPDU_Time) > RepeatPDU_TO)// нет ответа на enquire_link
                        {
                            ServiceManager.LogEvent("Не получили ответа на  enquire_link", EventType.Warning, EventSeverity.High);
                            SMPPmm.Logger.Write(new LogMessage("Не получили ответа на  enquire_link", LogSeverity.Debug));
                            IsBound = false;
                            SMPPmm.ReadyToSendMessages.Reset();
                           // NeedResp = false;??
                            //DataOutPDU_Time = DateTime.MinValue;
                            return true;
                        }

                        // Не послать ли enquire_link
                        if (!NeedResp && AnyInPDU_Time != DateTime.MinValue && (DateTime.Now - AnyInPDU_Time) > EnquireLink_TO)
                        {
                            SMPPmm.Logger.Write(new LogMessage("Отправляем enquire_link по таймауту", LogSeverity.Debug));
                            SMPPmm.SendEnquireLink();
                            return true;
                        }

                        if (ErrorResp_Time != DateTime.MinValue && DateTime.Now - ErrorResp_Time < ErrorPDU_TO)
                        {
                            return true;// очередь на смсц переполнена, не надо ничего перепосылать 

                        }
                        else
                        {// проверяем список отправленных сообщений без статуса, при необходимости перепосылаем

                            List<PacketInfo> nonRespPDUs = new List<PacketInfo>((SMPPmm as SMPPMessageManagerAsync).PDUList_OUT.Infos);
                            Dictionary<Guid, List<PacketInfo>> nonRespMsgs = new Dictionary<Guid, List<PacketInfo>>();

                            foreach (PacketInfo info in nonRespPDUs)
                            {
                                if (nonRespMsgs.ContainsKey(info.Message.ID))
                                    nonRespMsgs[info.Message.ID].Add(info);
                                else
                                {
                                    nonRespMsgs.Add(info.Message.ID, new List<PacketInfo>());
                                    nonRespMsgs[info.Message.ID].Add(info);
                                }
                            }

                            SMPPmm.Logger.Write(new LogMessage("checking timeouts..сообщений без статуса: " + nonRespMsgs.Count, LogSeverity.Info));

                            if (nonRespMsgs.Count > 0)
                            {
                                Thread.CurrentThread.Name = "PDUTimeouts";
                                foreach (KeyValuePair<Guid, List<PacketInfo>> msgInfo in nonRespMsgs)
                                {
                                    foreach (PacketInfo info in msgInfo.Value)
                                    {
                                        if (msgInfo.Value.TrueForAll(pi => DateTime.Now - pi.SentTime > RepeatPDU_TO))// &&
                                            //(info.Packet as SubmitSM).PartNumber == info.Message.PartsCount)не нужно условие
                                            //  (info.Packet as SubmitSM).PartNumber == 1)//может быть отсылали только одну часть
                                        {
                                            SMPPmm.ReadyToSendMessages.Reset();
                                            SMPPmm.Logger.Write(new LogMessage("Не получили статуса от SMSC в течение " + RepeatPDU_TO.TotalSeconds + " секунд! Перепосылаем", LogSeverity.Error));
                                            ServiceManager.LogEvent("Не получили статуса от SMSC в течение " + RepeatPDU_TO.TotalSeconds + " секунд! Перепосылаем. MessageId=" + info.Message.ID, EventType.Warning, EventSeverity.High);
                                            //перепосылаем
                                            if (SMPPmm is Tele2MessageManager)
                                                (SMPPmm as Tele2MessageManager).SendToSMSC(info.Message);//чтобы не было повторной тарификации
                                            
                                            else SMPPmm.SendUserData(info.Message);
                                            break;//одно сообщение перепосылаем только 1 раз
                                        }
                                    }

                                }
                            }

                            SMPPmm.ReadyToSendMessages.Set();
                            return true;
                        }
                    }
                }
            }

            catch (Exception Exp)
            {
                Trace.WriteLine("Exception in SMPPMessageManager CheckTimeout method: " + Exp);
                ServiceManager.LogEvent("Exception in SMPPMessageManager CheckTimeout method: " + Exp+" Stack Trace "+Exp.StackTrace, EventType.Error, EventSeverity.High);
                SMPPmm.Logger.Write(new LogMessage("Exception in SMPPMessageManager CheckTimeout method: " + Exp, LogSeverity.Error));

            }
            return true; 
        }
    }
}
