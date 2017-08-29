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
using System.Text;

namespace Cryptany.Core
{
    namespace SmppLib
    {
        public class BindTransmitter : BindPacketBase
        {
            public BindTransmitter()
            {
                m_commandId = commandIdEnum.bind_transmitter;
            }
        }

        public class BindReceiver : BindPacketBase
        {
            public BindReceiver()
            {
                m_commandId = commandIdEnum.bind_receiver;
            }
        }

        public class BindTransceiver : BindPacketBase
        {
            public BindTransceiver()
            {
                m_commandId = commandIdEnum.bind_transceiver;
            }
        }

        public class BindResponseReceiver : BindResponseBase
        {
            public BindResponseReceiver()
            {
                m_commandId = commandIdEnum.bind_receiver_resp;
            }
            public BindResponseReceiver(ref BindReceiver pak)
            {
                m_commandId = commandIdEnum.bind_receiver_resp;
                SequenceNumber = pak.SequenceNumber;
            }
        }

        public class BindResponseTransmitter : BindResponseBase
        {
            public BindResponseTransmitter()
            {
                m_commandId = commandIdEnum.bind_transmitter_resp;
            }

            public BindResponseTransmitter(ref BindTransmitter pak)
            {
                m_commandId = commandIdEnum.bind_transmitter_resp;
                SequenceNumber = pak.SequenceNumber;
            }
        }

        public class BindResponseTransceiver : BindResponseBase
        {
            public BindResponseTransceiver()
            {
                m_commandId = commandIdEnum.bind_transceiver_resp;
            }

            public BindResponseTransceiver(ref BindTransceiver pak)
            {
                m_commandId = commandIdEnum.bind_transceiver_resp;
                SequenceNumber = pak.SequenceNumber;
            }
        }

        public class LinkClose : PacketBase
        {
        }

        public class QueryShortMessage : PacketBase
        {

            public QueryShortMessage()
            {

                m_commandId =commandIdEnum.query_sm;
            }



            private SmppAddress m_Source=new SmppAddress();

            public SmppAddress Source
            {
                get { return m_Source; }
                set { m_Source = value; }
            }


            protected string m_messageId;

            public string SMSCMessageId
            {
                get { return m_messageId; }
                set { m_messageId = value; }
            }
            public override uint CommandLength
            {
                get
                {
                    ASCIIEncoding ascii = new ASCIIEncoding();

                    return base.CommandLength +
                        (uint)m_Source.Address.Length + (uint)4 + (uint)ascii.GetByteCount(m_messageId);
                       
                        
                }
            }

            public override byte[] GetEncoded()
            {

                byte[] localArray = new byte[PacketLength];
                int idx = base.GetEncoded().Length;
                base.GetEncoded().CopyTo(localArray, 0);

                ASCIIEncoding ascii = new ASCIIEncoding();

                ascii.GetBytes(m_messageId).CopyTo(localArray, idx);
                 idx += ascii.GetByteCount(m_messageId);
                localArray[idx] = 0;//нуль - терминатор


                m_Source.GetEncoded().CopyTo(localArray, idx+1);

                return localArray;

            }


        }

        public class QueryShortMessageResponse : MessageRespBase
        {

             public  QueryShortMessageResponse()
            {
                m_commandId = commandIdEnum.unbind_resp;
            }

            public  QueryShortMessageResponse(ref QueryShortMessage pak)
            {
                m_commandId = commandIdEnum.query_sm_resp;
                m_sequenceNumber = pak.SequenceNumber;
               
            }

            private SmppDate m_Final_Date=new SmppDate();

            public SmppDate Final_Date
            {
                get { return m_Final_Date; }
                set { m_Final_Date = value; }
            }

            private byte m_message_state;

            public byte Message_State
            {
                get { return m_message_state; }
                set { m_message_state = value; }
            }


            private byte m_error_code;

            public byte Error_Code
            {
                get { return m_error_code; }
                set { m_error_code = value; }
            }


            public override uint Parse(byte[] msg)
            {
                int bytesparsed = (int)base.Parse(msg);

                byte[] localArray = new byte[msg.Length - bytesparsed];
                Array.Copy(msg, bytesparsed, localArray, 0, msg.Length - bytesparsed);
               int newpos = bytesparsed + (int)m_Final_Date.Parse(localArray);


                m_message_state = msg[newpos];
                newpos++;

                m_error_code = msg[newpos];

                newpos++;

                return (uint)newpos;



            }

            public override uint CommandLength
            {
                get
                {
                   

                    return base.CommandLength +
                         (uint)m_Final_Date.Length + (uint)2;


                }
            }



        }

        public class Unbind : PacketBase
        {
            public Unbind()
            {
                m_commandId = commandIdEnum.unbind;
            }
        }

        public class UnbindResponse : PacketBase
        {
            public UnbindResponse()
            {
                m_commandId = commandIdEnum.unbind_resp;
            }

            public UnbindResponse(ref Unbind pak)
            {
                m_commandId = commandIdEnum.unbind_resp;
                m_sequenceNumber = pak.SequenceNumber;
            }
        }

        public class EnquireLink : PacketBase
        {
            public EnquireLink()
            {
                m_commandId = commandIdEnum.enquire_link;
            }
        }

        public class EnquireLinkResponse : PacketBase
        {
            public EnquireLinkResponse()
            {
                m_commandId = commandIdEnum.enquire_link_resp;          
            }

            public EnquireLinkResponse(ref EnquireLink pak)
            {
                m_commandId = commandIdEnum.enquire_link_resp;
                m_sequenceNumber = pak.SequenceNumber;
            }
        }

        public class GenericNak : PacketBase
        {
            public GenericNak()
            {
                m_commandId = commandIdEnum.generic_nack;
            }
        }
        
        public class DeliverSM : MessagePacketBase
        {
            public DeliverSM()
            {
                m_commandId = commandIdEnum.deliver_sm;
            }
        }

        public class DataSM : DataPacketBase
        {
            public DataSM()
            {
                m_commandId = commandIdEnum.data_sm;
            }

            public override uint Parse(byte[] msg)
            {
                int pos = (int)base.Parse(msg);
                uint cmdLength = SupportOperations.FromBigEndianUInt(msg);
                int newpos = 0;
                byte[] localArray;
                ASCIIEncoding ascii = new ASCIIEncoding();
                 ServiceType = SupportOperations.getStringValue(msg, pos, out newpos);
                localArray = new byte[msg.Length - newpos];
                Array.Copy(msg, newpos, localArray, 0, msg.Length - newpos);
                pos = newpos + (int)Source.Parse(localArray);
                localArray = new byte[msg.Length - pos];
                Array.Copy(msg, pos, localArray, 0, msg.Length - pos);
                pos += (int)Destination.Parse(localArray);
                 EsmClass = msg[pos++];
                 RegisteredDelivery = msg[pos++];
                DataCoding = (dataCodingEnum)Enum.Parse(typeof(dataCodingEnum), msg[pos++].ToString());
                // parse optional params
                localArray = new byte[2];
                while (pos < cmdLength)
                {
                    OptionalParameter op = new OptionalParameter();
                    Array.Copy(msg, pos, localArray, 0, 2);
                    op.Param = (OptionalParameter.tagEnum)Enum.Parse(typeof(OptionalParameter.tagEnum), SupportOperations.FromBigEndianUShort(localArray).ToString());
                    pos += 2;
                    Array.Copy(msg, pos, localArray, 0, 2);
                    op.Length = SupportOperations.FromBigEndianUShort(localArray);
                    pos += 2;
                    op.Value = new byte[op.Length];
                    Array.Copy(msg, pos, op.Value, 0, op.Length);
                    pos += op.Length;
                    OptionalParamList.Add(op);
                }
                return (uint)pos;

            }
            public override byte[] GetEncoded()
            {
                byte[] localArray = new byte[this.PacketLength];
                byte[] arr = base.GetEncoded();
                int idx = arr.Length;
                arr.CopyTo(localArray, 0);
                ASCIIEncoding ascii = new ASCIIEncoding();
                ascii.GetBytes( ServiceType).CopyTo(localArray, idx);
                idx += ascii.GetByteCount(ServiceType);
                localArray[idx] = 0;
                idx++;
                Source.GetEncoded().CopyTo(localArray, idx);
                idx += Source.GetEncoded().Length;
                Destination.GetEncoded().CopyTo(localArray, idx);
                idx += Destination.GetEncoded().Length;
                localArray[idx++] = EsmClass;
                localArray[idx++] = RegisteredDelivery;
                localArray[idx++] = (byte)DataCoding;
                foreach (OptionalParameter op in OptionalParamList)
                {
                    op.GetEncoded().CopyTo(localArray, idx);
                    idx += op.GetEncoded().Length;
                }
                return localArray;

            }
       
          
        }

        public class SubmitSM : MessagePacketBase
        {
        
            public SubmitSM()
            {
                m_commandId = commandIdEnum.submit_sm;
            }
        }

        public class DeliverSMResponse : MessageRespBase
        {
            public DeliverSMResponse()
            {
                m_commandId = commandIdEnum.deliver_sm_resp;
            }

            public DeliverSMResponse(ref DeliverSM pak)
            {
                m_commandId = commandIdEnum.deliver_sm_resp;
                m_sequenceNumber = pak.SequenceNumber;
            }
        }

        public class DataSMResponse : MessageRespBase
        {
            public DataSMResponse()
            {
                m_commandId = commandIdEnum.data_sm_resp;
            }

            public DataSMResponse(ref DataSM pak)
            {
                m_commandId = commandIdEnum.data_sm_resp;
                m_sequenceNumber = pak.SequenceNumber;
            }
        }

        public class SubmitSMResponse : MessageRespBase
        {
            public SubmitSMResponse()
            {
                m_commandId = commandIdEnum.submit_sm_resp;
            }

            public SubmitSMResponse(ref SubmitSM pak)
            {
                m_commandId = commandIdEnum.submit_sm_resp;
                m_sequenceNumber = pak.SequenceNumber;
            }
        }

        public class RegisterService : PacketBase
        {

            public override uint CommandLength
            {
                get
                {
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    Encoding w1251 = Encoding.GetEncoding(1251);
                    return base.CommandLength +
                           8 + // RequestId
                           (uint)ascii.GetByteCount(DestinationAddr) + 1 +
                           1 + // ServiceState
                           8 + // ServiceTypeId
                           (uint)w1251.GetByteCount(ServiceClass) + 1 +
                           (uint)w1251.GetByteCount(ServiceDescr) + 1 +
                           4 + // ServiceCost
                           1 + // ServicePeriod
                           1 + // ActivationType
                           (uint)ascii.GetByteCount(ActivationAddr) + 1 +
                           (uint)w1251.GetByteCount(ActivationMessage) + 1 +
                           8; // ServiceId
                }
            }

            ulong m_request_id;
            public ulong RequestId
            {
                get { return m_request_id; }
                set { m_request_id = value; }
            }

            string m_destination_addr;
            public string DestinationAddr
            {
                get { return m_destination_addr; }
                set
                {
                    if (value.Length > 21)
                        throw new OverflowException("SystemId must be 21 or less octets in length");
                    m_destination_addr = value;
                }
            }

            public enum serviceStateEnum : byte
            {
                DeactivateService           = 0x00,
                ActivateSingleService       = 0x01,
                ActivateSubscription        = 0x02,
                SendAutorizationCode        = 0x03
            }

            serviceStateEnum m_service_state;
            public serviceStateEnum ServiceState
            {
                get
                {
                    return m_service_state;
                }
                set
                {
                    if (!Enum.IsDefined(typeof(serviceStateEnum), value))
                        throw new ArgumentOutOfRangeException("Wrong serviceState Id: " + value.ToString());
                    else
                        m_service_state = value;
                }
            }

            ulong m_service_type_id;
            public ulong ServiceTypeId
            {
                get { return m_service_type_id; }
                set { m_service_type_id = value; }
            }

            string m_service_class;
            public string ServiceClass
            {
                get { return m_service_class; }
                set
                {
                    if (value.Length > 255)
                        throw new OverflowException("SystemId must be 255 or less octets in length");
                    m_service_class = value;
                }
            }

            string m_service_descr;
            public string ServiceDescr
            {
                get { return m_service_descr; }
                set
                {
                    if (value.Length > 255)
                        throw new OverflowException("SystemId must be 255 or less octets in length");
                    m_service_descr = value;
                }
            }

            uint m_service_cost;
            public uint ServiceCost
            {
                get { return m_service_cost; }
                set { m_service_cost = value; }
            }

            public enum servicePeriodEnum : byte
            {
                Once = 0x00,
                OncePerDay = 0x01,
                OncePerWeek = 0x02,
                OncePerMonth = 0x03,
                OncePerYear = 0x04,
            }

            servicePeriodEnum m_service_period;
            public servicePeriodEnum ServicePeriod
            {
                get
                {
                    return m_service_period;
                }
                set
                {
                    if (!Enum.IsDefined(typeof(servicePeriodEnum), value))
                        throw new ArgumentOutOfRangeException("Wrong servicePeriod Id: " + value.ToString());
                    else
                        m_service_period = value;
                }
            }

            public enum activationTypeEnum : byte
            {
                SMS = 0x01,
                USSD = 0x02,
                WAP = 0x03,
                AuthorizationCode = 0x04,
                Other = 0x00
            }

            activationTypeEnum m_activation_type;
            public activationTypeEnum ActivationType
            {
                get
                {
                    return m_activation_type;
                }
                set
                {
                    if (!Enum.IsDefined(typeof(activationTypeEnum), value))
                        throw new ArgumentOutOfRangeException("Wrong servicePeriod Id: " + value.ToString());
                    else
                        m_activation_type = value;
                }
            }

            string m_activation_addr;
            public string ActivationAddr
            {
                get { return m_activation_addr; }
                set
                {
                    if (value.Length > 21)
                        throw new OverflowException("SystemId must be 21 or less octets in length");
                    m_activation_addr = value;
                }
            }

            string m_activation_message;
            public string ActivationMessage
            {
                get { return m_activation_message; }
                set
                {
                    if (value.Length > 255)
                        throw new OverflowException("SystemId must be 255 or less octets in length");
                    m_activation_message = value;
                }
            }

            ulong m_service_id;
            public ulong ServiceId
            {
                get { return m_service_id; }
                set { m_service_id = value; }
            }

            public RegisterService()
            {
                m_commandId = commandIdEnum.register_service;
                RequestId = SupportOperations.GuidToULong(Guid.NewGuid());
                ActivationType = RegisterService.activationTypeEnum.SMS;
            }

            public override uint Parse(byte[] msg)
            {
                int pos = (int)base.Parse(msg);
                int newpos;
                if (pos == 0)
                    return 0;
                
                byte[] localArray = new byte[8];

                Array.Copy(msg, pos, localArray, 0, 8);
                RequestId = SupportOperations.FromBigEndianULong(localArray);
                pos += 8;


                DestinationAddr = SupportOperations.getStringValue(msg, pos, out newpos);
                pos = newpos;


                ServiceState = (serviceStateEnum)msg[pos];
                pos++;


                Array.Copy(msg, pos, localArray, 0, 8);
                ServiceTypeId = SupportOperations.FromBigEndianULong(localArray);
                pos += 8;


                ServiceClass = SupportOperations.get1251StringValue(msg, pos, out newpos);
                pos = newpos;


                ServiceDescr = SupportOperations.get1251StringValue(msg, pos, out newpos);
                pos = newpos;


                Array.Copy(msg, pos, localArray, 0, 4);
                ServiceCost = SupportOperations.FromBigEndianUInt(localArray);
                pos += 4;


                ServicePeriod = (servicePeriodEnum)msg[pos];
                pos++;


                ActivationType = (activationTypeEnum)msg[pos];
                pos++;


                ActivationAddr = SupportOperations.getStringValue(msg, pos, out newpos);
                pos = newpos;


                ActivationMessage = SupportOperations.get1251StringValue(msg, pos, out newpos);
                pos = newpos;


                Array.Copy(msg, pos, localArray, 0, 8);
                ServiceId = SupportOperations.FromBigEndianULong(localArray);
                pos += 8;

                return (uint)pos;
            }

            public override byte[] GetEncoded()
            {
                byte[] localArray = new byte[this.PacketLength];
                byte[] arr = base.GetEncoded();
                int idx = arr.Length;
                arr.CopyTo(localArray, 0);
                ASCIIEncoding ascii = new ASCIIEncoding();
                Encoding w1251 = Encoding.GetEncoding(1251);

                SupportOperations.ToBigEndian(RequestId).CopyTo(localArray, idx);
                idx += 8;

                ascii.GetBytes(DestinationAddr).CopyTo(localArray, idx);
                idx += ascii.GetByteCount(DestinationAddr);
                localArray[idx++] = 0;

                localArray[idx++] = (byte)ServiceState;

                SupportOperations.ToBigEndian(ServiceTypeId).CopyTo(localArray, idx);
                idx += 8;

                w1251.GetBytes(ServiceClass).CopyTo(localArray, idx);
                idx += w1251.GetByteCount(ServiceClass);
                localArray[idx++] = 0;

                w1251.GetBytes(ServiceDescr).CopyTo(localArray, idx);
                idx += w1251.GetByteCount(ServiceDescr);
                localArray[idx++] = 0;

                SupportOperations.ToBigEndian(ServiceCost).CopyTo(localArray, idx);
                idx += 4;

                localArray[idx++] = (byte)ServicePeriod;

                localArray[idx++] = (byte)ActivationType;

                ascii.GetBytes(ActivationAddr).CopyTo(localArray, idx);
                idx += ascii.GetByteCount(ActivationAddr);
                localArray[idx++] = 0;

                w1251.GetBytes(ActivationMessage).CopyTo(localArray, idx);
                idx += w1251.GetByteCount(ActivationMessage);
                localArray[idx++] = 0;

                SupportOperations.ToBigEndian(ServiceId).CopyTo(localArray, idx);
                idx += 8;

                return localArray;
            }

        }

        public class RegisterServiceResp : PacketBase
        {

            public override uint CommandLength
            {
                get
                {
                    ASCIIEncoding ascii = new ASCIIEncoding();
                    Encoding w1251 = Encoding.GetEncoding(1251);
                    return base.CommandLength + 8;
                }
            }

            ulong m_service_id;
            public ulong ServiceId
            {
                get { return m_service_id; }
                set { m_service_id = value; }
            }

            public RegisterServiceResp()
            {
                m_commandId = commandIdEnum.register_service_resp;
                ServiceId = 0;
            }

            public override uint Parse(byte[] msg)
            {
                int pos = (int)base.Parse(msg);
                if (pos == 0)
                    return 0;

                byte[] localArray = new byte[8];

                try
                {
                    Array.Copy(msg, pos, localArray, 0, 8);
                    ServiceId = SupportOperations.FromBigEndianULong(localArray);
                }
                catch
                {
                    ServiceId = 0;
                }
                pos += msg.Length;

                return (uint)pos;
            }

            public override byte[] GetEncoded()
            {
                byte[] localArray = new byte[this.PacketLength];
                byte[] arr = base.GetEncoded();
                int idx = arr.Length;
                arr.CopyTo(localArray, 0);
                
                SupportOperations.ToBigEndian(ServiceId).CopyTo(localArray, idx);
                idx += 8;

                return localArray;
            }
        }
    }
}
