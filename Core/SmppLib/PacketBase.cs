using System;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Collections.Generic;

namespace avantMobile.SmppLib
{
    //классы для работы с данными smpp пакета
    public class SmppDate
    {
        private DateTime m_Date;

        public DateTime Date
        {
            get { return m_Date; }
            set { m_Date = value; }
        }

        public uint Length
        {
            get
            {
                if (Date == DateTime.MinValue)
                    return 1;
                return 17;
            }
        }

        public SmppDate()
        {
            Date = DateTime.MinValue;
        }

        public SmppDate(DateTime dt)
        {
            Date = dt;
        }

        public byte[] GetEncoded() //побайтовый разбор даты
        {
            if (Date == DateTime.MinValue)
            {
                return new byte[] {0x00}; //SMPP NULL
            }
            string dt = Date.ToString("yyMMddHHmmssfzz");
            char cc = dt[13];
            dt = dt.Remove(13, 1);
            dt += cc;
            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] dt_bytes = new byte[17]; // dt.Length + 1 (C-OctetString terminating byte)
            ascii.GetBytes(dt).CopyTo(dt_bytes, 0);
            return dt_bytes;
        }

        public uint Parse(byte[] msg)
        {
            int newpos;
            string td = SupportOperations.getStringValue(msg, 0, out newpos);
            if (string.IsNullOrEmpty(td))
            {
                Date = DateTime.MinValue;
                return (uint) newpos;
            }
            // replace trailing zone sign (+ or -) to zone leading place
            string td2 = td.Insert(13, td.Substring(td.Length - 1, 1));
            td = td2.Remove(td2.Length - 1);
            Date = DateTime.ParseExact(td, "yyMMddHHmmssfzz", CultureInfo.CurrentCulture);
            return (uint) newpos;
        }
    }
    public class SmppAddress //NPI + TON + Addr
        {
            private byte m_NPI;
            public byte NPI
            {
                get { return m_NPI; }
                set { m_NPI = value; }
            }
            
            private byte m_TON;
            public byte TON
            {
                get { return m_TON; }
                set { m_TON = value; }
            }

            private string m_Address;
            public string Address
            {
                get { return m_Address; }
                set {
                    if (value.Length > 41)
                        throw new OverflowException("Addres range must be 41 or less octets in length");
                    m_Address = value; 
                }
            }

            public SmppAddress() : this(1, 1, ""){}
            public SmppAddress(byte ton, byte npi, string addr_range)
            {
                if (addr_range.Length > 41)
                    throw new OverflowException("Address range is too long");
                Address = addr_range;
                NPI = npi;
                TON = ton;
            }

            /// <summary>
            /// Get encoded SMPP address
            /// </summary>
            /// <returns>encoded byte array</returns>
            public byte [] GetEncoded()
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                byte[] localArray = new byte[2 + ascii.GetByteCount(Address) + 1]; // TON + NPI + address length + finising zero
                localArray[0] = TON;
                localArray[1] = NPI;
                ascii.GetBytes(Address).CopyTo(localArray, 2);
                localArray[ascii.GetByteCount(Address) + 2] = 0;
                return localArray;
            }

            /// <summary>
            /// Parses incoming byte array. May throw exceptions
            /// </summary>
            /// <param name="msg">incoming byte array</param>
            /// <returns>the number of bytes consumed</returns>
            public uint Parse(byte[] msg)
            {
                TON = msg[0];
                NPI = msg[1];
                int idx = Array.IndexOf(msg, (byte)0, 2 );
                ASCIIEncoding ascii = new ASCIIEncoding();
                Address = ascii.GetString(msg, 2, idx - 2);
                return (uint)idx+1;
            }

            public uint Length
            {
                get
                {
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    return (uint)ascii.GetByteCount(Address) + 2 + 1;
                }
            }
        }
    public class SupportOperations //доп. операции для работы с данными из пакета
		{
            public static ulong GuidToULong(Guid i)
            {
                var arr = i.ToByteArray();
                ulong h = FromBigEndianULong(arr);
                Array.Copy(arr, 8, arr, 0, 8);
                ulong l = FromBigEndianULong(arr);
                return h ^ l;
            }

			public static byte[] ToBigEndian(ushort from)
			{
				byte[] localArray = new byte [2];
				localArray[0] = (byte)(from>>8);
				localArray[1] = (byte)(from&255);
				return localArray;
			}

            public static byte[] ToBigEndian(int from)
            {
                byte[] localArray = new byte[4];
                localArray[0] = (byte)(from >> 24);
                localArray[1] = (byte)((from >> 16) & 255);
                localArray[2] = (byte)((from >> 8) & 255);
                localArray[3] = (byte)(from & 255);
                return localArray;
            }

            public static byte[] ToBigEndian(uint from)
            {
                return ToBigEndian((int)from);
            }

            public static byte[] ToBigEndian(ulong from)
            {
                byte[] localArray = new byte[8];
                for (int i = 0; i < 8; i++)
                    localArray[7 - i] = (byte)((from >> (8 * i)) & 255);
                return localArray;
            }

            public static ushort FromBigEndianUShort(byte[] msg)
            {
                if (msg.Length < 2)
                    throw new IndexOutOfRangeException("Incoming byte array is too small");
                return (ushort)((msg[0]<<8) + msg[1]);
            }

			public static int FromBigEndianInt(byte[] msg)
            {
                if (msg.Length < 4)
                    throw new IndexOutOfRangeException("Incoming byte array is too small");
                int ttt = (msg[0] << 24) + (msg[1] << 16) + (msg[2] << 8) + msg[3];
                return ttt;
            }

            public static ulong FromBigEndianULong(byte[] msg)
            {
                if (msg.Length < 8)
                    throw new IndexOutOfRangeException("Incoming byte array is too small");
                ulong ttt = 0;
                for (int i = 0; i < 8; i++)
                    ttt = (ttt << 8) + msg[i];
                return ttt;
            }

			public static uint FromBigEndianUInt(byte[] msg)
            {
                return (uint)FromBigEndianInt(msg);
            }

            /// <summary>
            /// Gets a null-terminated ASCII string value
            /// </summary>
            /// <param name="msg">incoming byte array</param>
            /// <param name="startIndex">index of a byte to search from</param>
            /// <param name="newIdx">position of the character next to the end of string</param>
            /// <returns>string</returns>
            public static string getStringValue(byte[] msg, int startIndex, out int newIdx)
            {
                newIdx = startIndex + 1;
                int idx = Array.IndexOf(msg, (byte) 0, startIndex);
                if (idx == -1)
                    return "";
                newIdx = idx + 1;
                ASCIIEncoding ascii = new ASCIIEncoding();
                return ascii.GetString(msg, startIndex, idx - startIndex);
            }

            /// <summary>
            /// Gets a null-terminated Windows-1251 string value
            /// </summary>
            /// <param name="msg">incoming byte array</param>
            /// <param name="startIndex">index of a byte to search from</param>
            /// <param name="newIdx">position of the character next to the end of string</param>
            /// <returns>string</returns>
            public static string get1251StringValue(byte[] msg, int startIndex, out int newIdx)
            {
                newIdx = startIndex + 1;
                int idx = Array.IndexOf(msg, (byte)0, startIndex);
                if (idx == -1)
                    return "";
                newIdx = idx + 1;
                Encoding enc = Encoding.GetEncoding(1251);
                return enc.GetString(msg, startIndex, idx - startIndex);
            }
		}
    public class OptionalParameter
    {
        public enum tagEnum : ushort
        {
            unknown = 0x0,
            dest_addr_submit = 0x0005,
            dest_network_type = 0x0006,
            dest_bearer_type = 0x0007,
            dest_telematics_id = 0x0008,

            source_addr_submit = 0x000D,
            source_network_type = 0x000E,
            source_bearer_type = 0x000F,
            source_telematics_id = 0x0010,

            qos_time_to_live = 0x0017,
            payload_type = 0x0019,
            additional_status_info_text = 0x001D,
            receipted_message_id = 0x001E,
            ms_msg_wait_facilities = 0x0030,
            privacy_indicator = 0x0201,

            source_subaddress = 0x0202,
            dest_subaddress = 0x0203,

            user_message_reference = 0x0204,
            user_response_code = 0x0205,

            source_port = 0x020A, //важно. Для взаимодействия с Вымпелкомом
            destination_port = 0x020B,

            sar_msg_ref_num = 0x020C,
            language_indicator = 0x020D,
            sar_total_segments = 0x020E,
            sar_segment_seqnum = 0x020F,

            SC_interface_version = 0x0210,

            callback_num_pres_ind = 0x0302,
            callback_num_atag = 0x0303,
            number_of_messages = 0x0304,
            callback_num = 0x0381,

            dpf_result = 0x0420,
            set_dpf = 0x0421,
            ms_availability_status = 0x0422,
            network_error_code = 0x0423,
            message_payload = 0x0424,
            delivery_failure_reason = 0x0425,
            more_messages_to_send = 0x0426,
            message_state = 0x0427,

            ussd_service_op = 0x0501,
            billing_identification = 0x060B,
            display_time = 0x1201,
            sms_signal = 0x1203,
            ms_validity = 0x1204,

            alert_on_message_delivery = 0x130C,
            its_reply_type = 0x1380,
            its_session_info = 0x1383,

            service_id = 0x1400, // Доп. параметр для РТК

            charging_id = 0x2010,
            // Life: флаг тарификации входящего сообщения (режим MO), исходящие сообщения не тарифицируются 

            charging = 0x4901, // USSD Portal: индикатор о необходимости произвести тарификацию 
            message_content_type = 0x4903,
            // USSD Portal: тип передаваемой в сообщении информации (для тарифицируемых сообщений) 
            dialog_directive = 0x4910,
            // USSD Portal: директива на обработку диалога от контент-провайдера в платформу агрегатора 
            phone_vendor_and_model = 0x4920, // USSD Portal: производитель телефона и модель телефона 
            subscription_command = 0x4951, // USSD Portal: команда подтверждения подписки или подписки на клуб

            whoisd_expected_message_transport_type = 0x4904 //Тип транспорта ожидаемого пакета. 0x02 – SMS


        };

        private tagEnum m_param;
        private ushort m_length;
        private byte[] m_value;

        /// <summary>
        /// The parameter tag. SMPP 3.4 parameter definition supported at the time of writing
        /// </summary>
        public tagEnum Param
        {
            get { return m_param; }
            set
            {
                if (!Enum.IsDefined(typeof (tagEnum), value))
                    m_param = tagEnum.unknown;
                else
                    m_param = value;
            }
        }

        /// <summary>
        /// The length of the optional parameter. 
        /// SMPP v.3.4 restriction: Must be less than 65535.
        /// </summary>
        public ushort Length
        {
            get { return m_length; }
            set
            {
                if (value > 65535)
                    throw new ArgumentOutOfRangeException("The optional param length value is too large: " +
                                                          value.ToString());
                m_length = value;
            }
        }

        /// <summary>
        /// The value property. Contains the value of the optional parameter
        /// </summary>
        public byte[] Value
        {
            get
            {
                if (m_value.Length == m_length)
                    return m_value;
                byte[] localValue = new byte[m_length];
                Array.Clear(localValue, 0, localValue.Length);
                m_value.CopyTo(localValue, 0);
                return localValue;
            }
            set
            {
                if (value.Length > ushort.MaxValue)
                    throw new OverflowException("Too long optional param value");
                m_length = (ushort) value.Length;
                m_value = new byte[m_length];
                value.CopyTo(m_value, 0);
            }
        }

        /// <summary>
        /// Parses incoming byte array, filling the member fields
        /// </summary>
        /// <param name="source">source byte array -- the array to read from. Reading starts from index 0.</param>
        /// <returns>number of bytes read</returns>
        public uint Parse(byte[] source)
        {
            ushort paramTag, paramLength;
            paramTag = (ushort) ((source[0] << 8) + source[1]);
            if (!Enum.IsDefined(typeof (tagEnum), paramTag))
                throw new ArgumentOutOfRangeException("Wrong OP tag value: " + paramTag.ToString());

            paramLength = (ushort) ((source[2] << 8) + source[3]);
            if (paramLength > 240)
                throw new OverflowException("Wrong OP length value: " + paramLength.ToString());
            m_value = new byte[paramLength];
            m_length = paramLength;
            m_param = (tagEnum) paramTag;
            Array.Copy(source, 4, m_value, 0, paramLength);
            return (uint) paramLength + 4;
        }

        public byte[] GetEncoded()
        {
            byte[] localEncoded = new byte[m_length + 4];
            SupportOperations.ToBigEndian(Convert.ToUInt16(m_param)).CopyTo(localEncoded, 0);
            SupportOperations.ToBigEndian(m_length).CopyTo(localEncoded, 2);
            m_value.CopyTo(localEncoded, 4);
            return localEncoded;
        }
    }
    
    /// <summary>
    /// Реализует структуру SMPP пакета
    /// Header: Length, CommandID, CommandStatus, SequenceNumber
    /// Message: messageID
    /// </summary>
    public partial class PacketBase
    {
        #region Fields
        //Header SMPP
        protected commandIdEnum m_commandId;
        protected commandStatusEnum m_statusCode = commandStatusEnum.ESME_ROK;
        protected static uint sequenceNumber = 0;
        protected uint m_sequenceNumber = 0;
        //message
        private Guid _messageID;
        private uint _realStatusCode;
        public delegate void ParseFallbackHandler();
        #endregion

        #region Properties
        /// <summary>
        /// The SMPP command ID property (see SMPP 3.4 specification)
        /// </summary>
        public commandIdEnum CommandId
        {
            get { return m_commandId; }
            set
            {
                if (!Enum.IsDefined(typeof(commandIdEnum), value))
                    throw new ArgumentOutOfRangeException("Wrong command Id: " + value.ToString());
                m_commandId = value;
            }
        }

        /// <summary>
        /// The command result code (see SMPP 3.4 specification)
        /// </summary>
        public commandStatusEnum StatusCode
        {
            get { return m_statusCode; }
            set
            {
                if (!Enum.IsDefined(typeof(commandStatusEnum), value))
                    throw new ArgumentOutOfRangeException("Wrong command status code: " + value.ToString());
                m_statusCode = value;
            }
        }

        public uint SequenceNumber
        {
            get { return m_sequenceNumber; }
            set { m_sequenceNumber = value; }
        }

        public Guid MessageID
        {
            get { return _messageID; }
            set { _messageID = value; }
        }

        public uint RealStatusCode
        {
            get { return _realStatusCode; }
            set { _realStatusCode = value; }
        }

        /// <summary>
        /// Command length property: returns the size of the command fields. The size of
        /// the 'command length' field is not counted! (see SMPP 3.4 specification)
        /// Overridable. Each type of packets will usually have it's own implementation of this method
        /// </summary>
        public virtual uint CommandLength
        {
            get { return 12; } // 3 four-byte words
        }

        /// <summary>
        /// Packet length property: the size of the overall packet (including 'command length' field)
        /// Not overridable as it uses the CommandLength method
        /// </summary>
        public uint PacketLength
        {
            get { return CommandLength + 4; } // plus 4-byte command length field
        }
        
        #endregion

        public PacketBase()
        {
            SequenceNumber = setNewSequenceNumber();
            CommandId = commandIdEnum.generic_nack;
            StatusCode = commandStatusEnum.ESME_ROK;
        }

        public PacketBase(uint seqNum)
        {
            SequenceNumber = seqNum;
            CommandId = commandIdEnum.generic_nack;
            StatusCode = commandStatusEnum.ESME_ROK;
        }

        public uint setNewSequenceNumber()
        {
            Mutex m = new Mutex(false, "smppNewSeqMutex" + Process.GetCurrentProcess().Id);
            m.WaitOne();
            sequenceNumber++;
            SequenceNumber = sequenceNumber;
            m.ReleaseMutex();
            return SequenceNumber;
        }

        /// <summary>
        /// Returns the whole encoded packet that is ready for transmitting
        /// </summary>
        /// <returns>the byte array ready for transmission over TCP/IP</returns>
        public virtual byte[] GetEncoded()
        {
            byte[] retArray = new byte[16];
            SupportOperations.ToBigEndian(PacketLength).CopyTo(retArray, 0);
            SupportOperations.ToBigEndian((uint)m_commandId).CopyTo(retArray, 4);
            SupportOperations.ToBigEndian((uint)m_statusCode).CopyTo(retArray, 8);
            SupportOperations.ToBigEndian(m_sequenceNumber).CopyTo(retArray, 12);
            return retArray;
        }

        public virtual uint Parse(byte[] msg, ParseFallbackHandler handler)
        {
            try
            {
                return Parse(msg);
            }
            catch (Exception)
            {
                if (handler != null)
                    handler();
            }
            return 0;
        }

        /// <summary>
        /// Parses incoming packet (SMPP part only)
        /// </summary>
        /// <param name="msg">byte array containing the data to parse</param>
        /// <returns>number of bytes read</returns>
        public virtual uint Parse(byte[] msg)
        {
            Trace.WriteLine("SMPP: получаем длину команды");
            uint cmdLen = SupportOperations.FromBigEndianUInt(msg);
            Trace.WriteLine("SMPP: длина команды " + cmdLen);
            byte[] localArray = new byte[4];
            Trace.WriteLine("SMPP: получаем id команды");
            Array.Copy(msg, 4, localArray, 0, 4);
            uint _commandId = SupportOperations.FromBigEndianUInt(localArray);
            if (Enum.IsDefined(typeof(commandIdEnum), _commandId))
                CommandId = (commandIdEnum)_commandId;
            else
                throw new UnknownCommandException(_commandId);
            Trace.WriteLine("SMPP: id команды " + CommandId);
            Trace.WriteLine("SMPP: получаем статус команды");
            Array.Copy(msg, 8, localArray, 0, 4);
            uint _status = SupportOperations.FromBigEndianUInt(localArray);
            Trace.WriteLine("SMPP: статус команды " + _status);
            _realStatusCode = _status;
            if (Enum.IsDefined(typeof(commandStatusEnum), _status))
                StatusCode = (commandStatusEnum)_status;
            else
                StatusCode = commandStatusEnum.ESME_RUNKNOWNERR;
            Trace.WriteLine("SMPP: получаем номер последовательности");
            Array.Copy(msg, 12, localArray, 0, 4);
            SequenceNumber = SupportOperations.FromBigEndianUInt(localArray);
            Trace.WriteLine("SMPP: номер последовательности " + SequenceNumber);
            return 16;
        }
    }
    //внутренние типы, перечисления
    public partial class PacketBase
    {
        public enum commandIdEnum : uint
        {
            generic_nack = 0x80000000,
            bind_receiver = 0x00000001,
            bind_receiver_resp = 0x80000001,
            bind_transmitter = 0x00000002,
            bind_transmitter_resp = 0x80000002,
            query_sm = 0x00000003,
            query_sm_resp = 0x80000003,
            submit_sm = 0x00000004,
            submit_sm_resp = 0x80000004,
            deliver_sm = 0x00000005,
            deliver_sm_resp = 0x80000005,
            unbind = 0x00000006,
            unbind_resp = 0x80000006,
            replace_sm = 0x00000007,
            replace_sm_resp = 0x80000007,
            cancel_sm = 0x00000008,
            cancel_sm_resp = 0x80000008,

            bind_transceiver = 0x00000009,
            bind_transceiver_resp = 0x80000009,
            outbind = 0x0000000B,
            enquire_link = 0x00000015,
            enquire_link_resp = 0x80000015,
            submit_multi = 0x00000021,
            submit_multi_resp = 0x80000021,
            alert_notification = 0x00000102,
            data_sm = 0x00000103,
            data_sm_resp = 0x80000103,
            register_service = 0x00000201,
            register_service_resp = 0x80000201,
        };

        public enum dataCodingEnum : byte
        {
            defaultAlphabet = 0,
            dcASCII,
            dc8bit1,
            dcLatin1,
            dc8bit2,
            dcJIS,
            dcCyrillic,
            dcHebrew,
            dcUCS2,
            dcPictogram,
            dcISO2022JP,
            dcKanjiJIS = 13,
            dcKSC,
            dcFlashUnicode = 24
        }

        public enum MessageState : byte
        {
            enroute = 1,
            delivered,
            expired,
            deleted,
            undeliverable,
            accepted,
            unknown,
            rejected
        }

        public enum commandStatusEnum : uint
        {
            ESME_ROK = 0x00000000,              // Everything is Ok
            ESME_RINVMSGLEN = 0x00000001,       // Message Length is invalid
            ESME_RINVCMDLEN = 0x00000002,       // Command Length is invalid
            ESME_RINVCMDID = 0x00000003,        // Invalid Command ID
            ESME_RINVBNDSTS = 0x00000004,       // Incorrect BIND Status for given command
            ESME_RALYBND = 0x00000005,          // ESME Already in Bound State
            ESME_RINVPRTFLG = 0x00000006,       // Invalid Priority Flag
            ESME_RINVREGDLVFLG = 0x00000007,    // Invalid Registered Delivery Flag
            ESME_RSYSERR = 0x00000008,          // System Error
            ESME_RINVSRCADR = 0x0000000A,       // Invalid Source Address
            ESME_RINVDSTADR = 0x0000000B,       // Invalid Dest Addr
            ESME_RINVMSGID = 0x0000000C,        // Message ID is invalid
            ESME_RBINDFAIL = 0x0000000D,        // Bind Failed
            ESME_RINVPASWD = 0x0000000E,        // Invalid Password
            ESME_RINVSYSID = 0x0000000F,        // Invalid System ID
            ESME_RCANCELFAIL = 0x00000011,      // Cancel SM Failed
            ESME_RREPLACEFAIL = 0x00000013,     // Replace SM Failed
            ESME_RMSGQFUL = 0x00000014,         // Message Queue Full
            ESME_RINVSERTYP = 0x00000015,       // Invalid Service Type
            ESME_RINVNUMDESTS = 0x00000033,     // Invalid number of destinations
            ESME_RINVDLNAME = 0x00000034,       // Invalid Distribution List name
            ESME_RINVDESTFLAG = 0x00000040,     // Destination flag is invalid (submit_multi)
            ESME_RINVSUBREP = 0x00000042,
            
            // Invalid ‘submit with replace’ request (i.e. submit_sm with replace_if_present_flag set)
            ESME_RINVESMCLASS = 0x00000043,         // Invalid esm_class field data
            ESME_RCNTSUBDL = 0x00000044,            // Cannot Submit to Distribution List
            ESME_RSUBMITFAIL = 0x00000045,          // submit_sm or submit_multi failed
            ESME_RINVSRCTON = 0x00000048,           // Invalid Source address TON
            ESME_RINVSRCNPI = 0x00000049,           // Invalid Source address NPI
            ESME_RINVDSTTON = 0x00000050,           // Invalid Destination address TON
            ESME_RINVDSTNPI = 0x00000051,           // Invalid Destination address NPI
            ESME_RINVSYSTYP = 0x00000053,           // Invalid system_type field
            ESME_RINVREPFLAG = 0x00000054,          // Invalid replace_if_present flag
            ESME_RINVNUMMSGS = 0x00000055,          // Invalid number of messages
            ESME_RSMSPUSHNOTSUPPORTED = 0x00000057, // SMSPUSHNOTSUPPORTED
            ESME_RTHROTTLED = 0x00000058,           // Throttling error (ESME has exceeded allowed message limits)
            ESME_RINVSCHED = 0x00000061,            // Invalid Scheduled Delivery Time
            ESME_RINVEXPIRY = 0x00000062,           // Invalid message validity period (Expiry time)
            ESME_RINVDFTMSGID = 0x00000063,         // Predefined Message Invalid or Not Found
            ESME_RX_T_APPN = 0x00000064,            // ESME Receiver Temporary App Error Code
            ESME_RX_P_APPN = 0x00000065,            // ESME Receiver Permanent App Error Code
            ESME_RX_R_APPN = 0x00000066,            // ESME Receiver Reject Message Error Code
            ESME_RQUERYFAIL = 0x00000067,           // Query_sm request failed
            ESME_RINVOPTPARSTREAM = 0x000000C0,     // Error in the optional part of the PDU Body
            ESME_ROPTPARNOTALLWD = 0x000000C1,      // Optional Parameter not allowed
            ESME_RINVPARLEN = 0x000000C2,           // Invalid Parameter Length.
            ESME_RMISSINGOPTPARAM = 0x000000C3,     // Expected Optional Parameter missing
            ESME_RINVOPTPARAMVAL = 0x000000C4,      // Invalid Optional Parameter Value
            ESME_RDELIVERYFAILURE = 0x000000FE,     // Delivery Failure (used for data_sm_resp)
            ESME_RUNKNOWNERR = 0x000000FF,          // Unknown Error
            ESME_RTRANSPORTERR = 0x00000400,        // Transport Error
            ESME_RGATEWAYFAILURE = 0x00000401,      // Gateway Failure (Unable to reach internal gateway)
            ESME_RPERMANENTFAILURE = 0x00000402,    // Permanent Network Failure (To external gateway)
            ESME_RTEMPORARYFAILURE = 0x00000403,    // Temporary Network Failure (To external gateway)
            ESME_RINVSUBSCRIBER = 0x00000404,       // Invalid Subscriber
            ESME_RINVMSG = 0x00000405,              // Invalid Message
            ESME_RPROTOCOLERR = 0x00000406,         // Gateway Protocol Error
            ESME_RDUPLICATEDMSG = 0x00000407,       // Duplicated Messages
            ESME_RBARREDBYUSER = 0x00000408,        // Barred by User
            ESME_RCANCELLEDBYSYS = 0x00000409,      // Cancelled by System
            ESME_REXPIRED = 0x0000040A,             // Message Expired
            ESME_RTIMEOUT = 0x00000421,             // Timeout Error
            ESME_R_LIFE_LICENSE_ERR = 0x00000422,   // License Error
            ESME_R_LIFE_NOT_ENOUGH_MONEY = 0x00000423, // Life system: not enough money on subscriber account
            
            ESME_R_MTS_AccountNotActive = 0x410,        //MTS account is not active
            ESME_R_MTS_UNKNOWN_SUBSCRIBER = 0x413,
            ESME_R_MTS_UNKNOWN_ACCOUNT = 0x414,
            ESME_R_MTS_Tarification_Error = 0x416,
            ESME_R_MTS_UNKNOWN_ERR = 0x440,             // MTS: too many sendings in the time period
            ESME_R_MTS_IN_System_NOTAVAILABLE = 0x441,  // MTS: too many sendings in the time period
            ESME_R_MTS_SLA_ERR = 0x442,                 // MTS: too many sendings in the time period
            ESME_R_MTS_NoPullForPush = 0x443,           // MTS: no pull for push
            ESME_R_MTS_CPNotFound = 0x444,              // MTS: content provider not found
            ESME_R_MTS_ServiceNotActive = 0x445,        // MTS: service not active
            ESME_R_MTS_IncorrectConcatFragment = 0x446, // MTS: incorrect concatenated fragment
            ESME_R_MTS_ChargingSettings_ERR = 0x447,    // MTS: charging settings error
            ESME_R_MTS_LicenseViolated = 0x448,         // MTS: incorrect concatenated fragment
            ESME_R_MTS_NoRulesFound = 0x449,            // MTS: incorrect concatenated fragment
            ESME_R_MTS_SubscriberIsBlocked = 0x44A,     // MTS: incorrect concatenated fragment
            ESME_R_MTS_AbonentInBlackList = 0x44B,
            ESME_R_MTS_AbonentBlocked = 0x44C,          //MTS: financially blocked
            ESME_R_MTS_InitialBlock = 0x44D,            //MTS : sim has no abonent yet
            ESME_R_MTS_LowBalance = 0x077,
            ESME_R_MTS_FirstBlocked = 0x0546,           // 1350
            ESME_R_MTS_AnotherBlocked = 0x0547,         // 1351
            ESME_R_MTS_PartialBlocked = 0x0548,         // 1352
            ESME_R_MTS_VoiceCallBlocked = 0x0549,       // 1353
            ESME_R_MTS_AccountIsBlocked = 0x85,         // MTS account is blocked
            ESME_R_USSD_SMDELIVERYFAILURE = 0x4A0,      //delivery failure

            ESME_R_BEELINE_ServiceNotActive = 0x501,
            ESME_R_BEELINE_IncorrectService = 0x502,
            ESME_R_BEELINE_RequestDistributingViolation = 0x503,
            ESME_R_BEELINE_DistributingServiceBlocked = 0x504,
            ESME_R_BEELINE_PartnerBlocked = 0x505,
            ESME_R_BEELINE_RegionBlockedForService = 0x506,
            ESME_R_BEELINE_RegionBlockedForPartner = 0x507,
            ESME_R_BEELINE_RegionNotActive = 0x508,
            ESME_R_BEELINE_AbonentNotSubscribed = 0x509,
            ESME_R_BEELINE_AbonentBlocked = 0x510,
            ESME_R_BEELINE_ServiceBlockedForRegion = 0x511,
            ESME_R_BEELINE_PartnerBlockedForRegion = 0x512,
            ESME_R_BEELINE_MaxPushTimeout = 0x513,
            ESME_R_BEELINE_MaxReply = 0x514,
            ESME_R_BEELINE_MaxTransTimeout = 0x515,
            ESME_R_BEELINE_AccountNotRegistered = 0x516,
            ESME_R_BEELINE_NotRegisteredAck = 0x517,
            ESME_R_BEELINE_MaxReplyPushMode = 0x518,
            ESME_R_BEELINE_NotConnected = 0x519,
            ESME_R_BEELINE_DBError = 0x53C,
            ESME_R_BEELINE_Provider_AbonentAlreadySubscribed = 0x551,
            ESME_R_BEELINE_Provider_IncorrectServiceNumber = 0x552,
            ESME_R_BEELINE_Provider_IncorrectOptionalParams = 0x553,
            ESME_R_BEELINE_Provider_TemporaryUnavailableService = 0x554,
            ESME_R_BEELINE_Provider_ServiceDenied = 0x22B,
            ESME_R_BEELINE_Provider_IncorrectRequest = 0x556,
            ESME_R_BEELINE_Provider_SystemError = 0x600,
            ESME_R_BEELINE_Provider_QuotaError = 0x610,
            ESME_R_BEELINE_Provider_IncorrectChargeLevel = 0x601,

            ESME_R_Bercut_ServiceNotActive = 0x521,
            ESME_R_Bercut_IncorrectService = 0x522,
            ESME_R_Bercut_RequestDistributingViolation = 0x523,
            ESME_R_Bercut_DistributingServiceBlocked = 0x524,
            ESME_R_Bercut_PartnerBlocked = 0x525,
            ESME_R_Bercut_RegionBlockedForService = 0x526,
            ESME_R_Bercut_RegionNotActive = 0x529,
            ESME_R_Bercut_IncorrectSubscribeLocation = 0x52D,
            ESME_R_Bercut_IncorrectTrafficType = 0x52E,
            ESME_R_Bercut_RegionNotFound = 0x52F,
            ESME_R_Bercut_AbonentNotSubscribed = 0x534,
            ESME_R_Bercut_AbonentBlocked = 0x535,
            ESME_R_Bercut_ServiceBlockedForRegion = 0x537,
            ESME_R_Bercut_PartnerBlockedForRegion = 0x538,
            ESME_R_Bercut_IncorrectRequest = 0x539,
            ESME_R_Bercut_MaxPushTimeout = 0x560,
            ESME_R_Bercut_MaxReply = 0x558,
            ESME_R_Bercut_NoChargeLevel = 0x642,
            ESME_R_Bercut_NoTariffication = 0x643,
            ESME_R_Bercut_NotRegisteredAck = 0x542,
            ESME_R_Bercut_AccountNotRegistered = 0x601,
            ESME_R_Bercut_ServiceNumberBlockedForAbonent = 0x62C
        };
    }

    public class BindPacketBase : PacketBase
    {
        private string m_systemId;
        private string m_password;
        private string m_systemType;
        protected byte m_interface_version;
        private SmppAddress m_address_range;

        public string SystemId
        {
            get { return m_systemId; }
            set
            {
                if (value.Length > 16)
                    throw new OverflowException("SystemId must be 16 or less octets in length");
                m_systemId = value;
            }
        }

        public string Password
        {
            get { return m_password; }
            set
            {
                if (value.Length > 9)
                    throw new OverflowException("Password must be 9 or less octets in length");
                m_password = value;
            }
        }

        public string SystemType
        {
            get { return m_systemType; }
            set
            {
                if (value.Length > 16)
                    throw new OverflowException("SystemType must be 13 or less octets in length");
                m_systemType = value;
            }
        }

        public SmppAddress AddressRange
        {
            get { return m_address_range; }
            set { m_address_range = value; }
        }

        public override uint CommandLength
        {
            get
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                return base.CommandLength + m_address_range.Length +
                       (uint)ascii.GetByteCount(m_systemType) + (uint)ascii.GetByteCount(m_systemId) +
                       (uint)ascii.GetByteCount(m_password) + 4;
            }
        }

        public BindPacketBase()
        {
            SystemId = "";
            SystemType = "";
            Password = "";
            m_interface_version = 0x34;
            AddressRange = new SmppAddress();
        }

        public override uint Parse(byte[] msg)
        {
            int pos = (int) base.Parse(msg);
            int newpos;
            if (pos == 0)
                return 0;
            SystemId = SupportOperations.getStringValue(msg, pos, out newpos);
            pos = newpos;
            Password = SupportOperations.getStringValue(msg, pos, out newpos);
            pos = newpos;
            SystemType = SupportOperations.getStringValue(msg, pos, out newpos);
            return (uint) newpos;
        }

        public override byte[] GetEncoded()
        {
            byte[] localArray = new byte[PacketLength];
            int idx = base.GetEncoded().Length;
            base.GetEncoded().CopyTo(localArray, 0);
            // add bind fields
            ASCIIEncoding ascii = new ASCIIEncoding();
            ascii.GetBytes(SystemId).CopyTo(localArray, idx);
            idx += ascii.GetByteCount(SystemId);
            localArray[idx] = 0;
            idx++;
            ascii.GetBytes(Password).CopyTo(localArray, idx);
            idx += ascii.GetByteCount(Password);
            localArray[idx] = 0;
            idx++;
            ascii.GetBytes(SystemType).CopyTo(localArray, idx);
            idx += ascii.GetByteCount(SystemType);
            localArray[idx] = 0;
            idx++;
            localArray[idx] = m_interface_version;
            idx++;
            m_address_range.GetEncoded().CopyTo(localArray, idx);
            return localArray;
        }
    }

    public class BindResponseBase : PacketBase
    {
        protected string m_systemId;

        public string SystemId
        {
            get { return m_systemId; }
            set { m_systemId = value; }
        }

        public override uint CommandLength
        {
            get
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                return base.CommandLength + (uint)ascii.GetByteCount(m_systemId) + 1;
            }
        }

        public override byte[] GetEncoded()
        {
            byte[] localArray = new byte[this.PacketLength];
            int idx = base.GetEncoded().Length;
            base.GetEncoded().CopyTo(localArray, 0);
            // add bind fields
            ASCIIEncoding ascii = new ASCIIEncoding();
            ascii.GetBytes(SystemId).CopyTo(localArray, idx);
            idx += ascii.GetByteCount(SystemId);
            localArray[idx] = 0;
            return localArray;
        }

        public override uint Parse(byte[] msg)
        {
            int pos = (int) base.Parse(msg);
            int newpos;
            if (pos == 0)
                return 0;
            SystemId = SupportOperations.getStringValue(msg, pos, out newpos);
            return (uint) newpos;
        }
    }

    /// <summary>
    /// Представляет данные SMPP пакета:
    /// SrcAddress (TON+NPI+IP), DstAddress (TON+NPI+IP)
    /// ESMClass, RegisteredDelivery, OptionalParams list
    /// </summary>
    public class DataPacketBase : PacketBase
    {
        private string m_serviceType;
        private SmppAddress m_Source;
        private SmppAddress m_Destination;
        private byte m_esmClass;
        private byte m_registeredDelivery;
        private dataCodingEnum m_dataCoding;
        private ushort _partNumber;
        private List<OptionalParameter> optionalParamList;

        public string ServiceType
        {
            get { return m_serviceType; }
            set { m_serviceType = value; }
        }

        public SmppAddress Source
        {
            get { return m_Source; }
            set { m_Source = value; }
        }

        public SmppAddress Destination
        {
            get { return m_Destination; }
            set { m_Destination = value; }
        }

        public byte EsmClass
        {
            get { return m_esmClass; }
            set { m_esmClass = value; }
        }

        public byte RegisteredDelivery
        {
            get { return m_registeredDelivery; }
            set { m_registeredDelivery = value; }
        }

        public dataCodingEnum DataCoding
        {
            get { return m_dataCoding; }
            set { m_dataCoding = value; }
        }

        public ushort PartNumber
        {
            get { return _partNumber; }
            set { _partNumber = value; }
        }

        public List<OptionalParameter> OptionalParamList
        {
            get { return optionalParamList; }
            set { optionalParamList = value; }
        }

        public DataPacketBase()
        {
            m_serviceType = "";
            optionalParamList = new List<OptionalParameter>();
            m_Source = new SmppAddress();
            m_Destination = new SmppAddress();
        }

        public override uint CommandLength
        {
            get
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                uint opListLength = 0;
                foreach (OptionalParameter op in optionalParamList)
                {
                    opListLength += 4 + (uint) op.Length; // Parameter Tag + Length + Value
                }
                return base.CommandLength + (uint) ascii.GetByteCount(m_serviceType) +
                       (uint) m_Source.Address.Length + (uint) m_Destination.Address.Length +
                       10 + opListLength;
            }
        }
    }

    public class MessageRespBase : PacketBase
    {
        protected string m_messageId;

        public string MessageId
        {
            get { return m_messageId; }
            set { m_messageId = value; }
        }

        public MessageRespBase()
        {
            m_messageId = "";
        }

        public override uint CommandLength
        {
            get
            {
                ASCIIEncoding ascii = new ASCIIEncoding();
                return base.CommandLength + (uint) ascii.GetByteCount(m_messageId) + 1;
            }
        }

        public override byte[] GetEncoded()
        {
            byte[] localArray = new byte[this.PacketLength];
            int idx = base.GetEncoded().Length;
            base.GetEncoded().CopyTo(localArray, 0);
            ASCIIEncoding ascii = new ASCIIEncoding();
            ascii.GetBytes(m_messageId).CopyTo(localArray, idx);
            idx += ascii.GetByteCount(m_messageId);
            localArray[idx] = 0;
            return localArray;
        }

        public override uint Parse(byte[] msg)
        {
            uint bytesParsed = base.Parse(msg);
            byte[] localArray = new byte[msg.Length - bytesParsed];
            if (localArray.Length > 1)
            {
                Array.Copy(msg, bytesParsed, localArray, 0, msg.Length - bytesParsed);
                int newpos = 0;
                m_messageId = SupportOperations.getStringValue(localArray, 0, out newpos);
            }
            ASCIIEncoding ascii = new ASCIIEncoding();
            return bytesParsed + (uint) ascii.GetByteCount(m_messageId) + 1;
        }
    }

    /// <summary>
    /// Base class for message-containing packets
    /// </summary>
    public class MessagePacketBase : DataPacketBase
    {
        private byte m_protocolId;
        private byte m_priorityFlag;
        private byte m_replaceIfPresent;
        private byte m_defaultSMMessageId;
        private int m_SMLength;
        private byte[] m_messageText;
        private SmppDate m_scheduledDeliveryDate;
        private SmppDate m_validityPeriod;

        public byte ProtocolId
        {
            get { return m_protocolId; }
            set { m_protocolId = value; }
        }

        public byte PriorityFlag
        {
            get { return m_priorityFlag; }
            set { m_priorityFlag = value; }
        }

        public bool ReplaceIfPresent
        {
            get { return m_replaceIfPresent == 1; }
            set { m_replaceIfPresent = value ? (byte)1 : (byte)0; }
        }

        public byte DefaultSMMessageId
        {
            get { return m_defaultSMMessageId; }
            set { m_defaultSMMessageId = value; }
        }

        public int ShortMessageLength
        {
            get { return m_SMLength; }
            set { m_SMLength = value; }
        }

        public byte[] MessageText
        {
            get { return m_messageText; }
            set { m_messageText = value; }
        }

        public SmppDate ScheduledDeliveryDate
        {
            get { return m_scheduledDeliveryDate; }
            set { m_scheduledDeliveryDate = value; }
        }

        public SmppDate ValidityPeriod
        {
            get { return m_validityPeriod; }
            set { m_validityPeriod = value; }
        }

        public override uint CommandLength
        {
            get
            {
                return base.CommandLength +
                       m_scheduledDeliveryDate.Length + m_validityPeriod.Length +
                       ((m_messageText != null) ? (uint)m_messageText.Length : 0) + 5;
            }
        }

        public MessagePacketBase()
        {
            m_scheduledDeliveryDate = new SmppDate();
            m_validityPeriod = new SmppDate();
        }

        public override byte[] GetEncoded()
        {
            byte[] localArray = new byte[PacketLength];

            int idx = base.GetEncoded().Length;

            base.GetEncoded().CopyTo(localArray, 0);
            ASCIIEncoding ascii = new ASCIIEncoding();
            ascii.GetBytes(ServiceType).CopyTo(localArray, idx);
            idx += ascii.GetByteCount(ServiceType);
            localArray[idx] = 0;
            idx++;
            Source.GetEncoded().CopyTo(localArray, idx);
            idx += Source.GetEncoded().Length;

            Destination.GetEncoded().CopyTo(localArray, idx);
            idx += Destination.GetEncoded().Length;

            localArray[idx] = EsmClass;
            idx++;
            localArray[idx] = ProtocolId;
            idx++;
            localArray[idx] = PriorityFlag;
            idx++;
            Trace.WriteLine("Encoding Message. CommandLength " + CommandLength + " base commandLength " +
                            base.CommandLength);

            m_scheduledDeliveryDate.GetEncoded().CopyTo(localArray, idx);
            idx += m_scheduledDeliveryDate.GetEncoded().Length;
            m_validityPeriod.GetEncoded().CopyTo(localArray, idx);
            idx += m_validityPeriod.GetEncoded().Length;
            localArray[idx++] = RegisteredDelivery;
            localArray[idx++] = m_replaceIfPresent;
            localArray[idx++] = (byte)DataCoding;
            localArray[idx++] = DefaultSMMessageId;

            if (ShortMessageLength > 255) throw new ApplicationException("Длина входящего сообщения больше 255 символов!");
            localArray[idx++] = (byte)ShortMessageLength;

            if (MessageText.Length > 0) // just copy the text
            {
                MessageText.CopyTo(localArray, idx);
            }
            Trace.WriteLine("Encoding Message idx=" + idx);
            idx += MessageText.Length;
            foreach (OptionalParameter op in OptionalParamList)
            {
                op.GetEncoded().CopyTo(localArray, idx);
                idx += op.GetEncoded().Length;
            }
            Trace.WriteLine("Encoding Message idx=" + idx);
            return localArray;
        }

        public override uint Parse(byte[] msg)
        {
            Trace.WriteLine("SMPP: Parsing MessagePacket Base");

            Trace.WriteLine("SMPP: получаем длину команды");
            int pos = (int)base.Parse(msg);
            uint cmdLength = SupportOperations.FromBigEndianUInt(msg);

            Trace.WriteLine("SMPP: получаем тип сервиса");
            int newpos = 0;
            ServiceType = SupportOperations.getStringValue(msg, pos, out newpos);

            byte[] localArray = new byte[msg.Length - newpos];
            if (localArray.Length < msg.Length - newpos)
                return 0;

            Trace.WriteLine("SMPP: Copy1 " + newpos + " " + msg.Length);
            Array.Copy(msg, newpos, localArray, 0, msg.Length - newpos);
            
            Trace.WriteLine("SMPP: array before source = " + pos);
            pos = newpos + (int)Source.Parse(localArray);
            localArray = new byte[msg.Length - pos];

            if (localArray.Length < msg.Length - pos)
                return 0;

            Array.Copy(msg, pos, localArray, 0, msg.Length - pos);
            Trace.WriteLine("SMPP: array before destination = " + pos);
            pos += (int)Destination.Parse(localArray);
            EsmClass = msg[pos++];
            m_protocolId = msg[pos++];
            m_priorityFlag = msg[pos++];
            localArray = new byte[msg.Length - pos];
            if (localArray.Length < msg.Length - pos)
                return 0;
            Array.Copy(msg, pos, localArray, 0, msg.Length - pos);
            Trace.WriteLine("SMPP: array before deliverydate= " + pos);
            pos += (int)m_scheduledDeliveryDate.Parse(localArray);
            localArray = new byte[msg.Length - pos];
            Array.Copy(msg, pos, localArray, 0, msg.Length - pos);
            Trace.WriteLine("SMPP: array before validity = " + pos);
            pos += (int)m_validityPeriod.Parse(localArray);
            RegisteredDelivery = msg[pos++];
            m_replaceIfPresent = msg[pos++];
            byte datacoding = msg[pos++];
            System.Collections.BitArray dcbits = new System.Collections.BitArray(new byte[] { datacoding });
            if (dcbits[7] == dcbits[6] == dcbits[2] == false && dcbits[3] == true)
                DataCoding = dataCodingEnum.dcUCS2;
            else DataCoding = dataCodingEnum.defaultAlphabet;
            Trace.WriteLine("SMPP: position after datacoding = " + pos);
            m_defaultSMMessageId = msg[pos++];
            m_SMLength = msg[pos++];
            if (m_SMLength > 0)
            {
                m_messageText = new byte[m_SMLength];
                if (m_messageText == null)
                    return 0;
                Array.Copy(msg, pos, m_messageText, 0, m_SMLength);
                pos += m_SMLength;
            }
            Trace.WriteLine("SMPP: parsing optional params");
            localArray = new byte[2];
            while (pos < cmdLength)
            {
                OptionalParameter op = new OptionalParameter();
                if (msg.Length - pos < 2)
                {
                    Trace.WriteLine(String.Format(
                        "SMPP: Invalid opparam length..newpos={0} locarraylen={1} pktlen={2}", pos, localArray.Length,
                        msg.Length));
                    return 0;
                }
                Array.Copy(msg, pos, localArray, 0, 2);
                op.Param =
                    (OptionalParameter.tagEnum)
                    Enum.Parse(typeof(OptionalParameter.tagEnum),
                               SupportOperations.FromBigEndianUShort(localArray).ToString());
                pos += 2;
                if (msg.Length - pos < 2)
                {
                    Trace.WriteLine(
                        String.Format(
                            "SMPP: Invalid oplength length..newpos={0} locarraylen={1} pktlen={2} smsglen={3}", pos,
                            localArray.Length, msg.Length, m_SMLength));
                    return 0;
                }
                Array.Copy(msg, pos, localArray, 0, 2);
                op.Length = SupportOperations.FromBigEndianUShort(localArray);
                pos += 2;
                op.Value = new byte[op.Length];
                if (msg.Length - pos < op.Length)
                {
                    Trace.WriteLine(
                        String.Format(
                            "SMPP: Invalid opvalue length..newpos={0} locarraylen={1} pktlen={2} oplength={3}", pos,
                            localArray.Length, msg.Length, op.Length));
                    return 0;
                }
                Array.Copy(msg, pos, op.Value, 0, op.Length);
                pos += op.Length;
                OptionalParamList.Add(op);
            }
            return (uint)pos;
        }
    }
}