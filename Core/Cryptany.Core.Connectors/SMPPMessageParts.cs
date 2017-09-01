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
using Cryptany.Core.SmppLib;
using System.Configuration;

namespace Cryptany.Core
{
    /// <summary>
    /// Collects SMPP sar message parts
    /// </summary>
    public class SMPPMessageParts
    {
        private SMPPMessageManager m_SMPPmm;

        public SMPPMessageManager SMPPmm
        {
            get { return m_SMPPmm; }
            set { m_SMPPmm = value; }
        }

        private ushort m_referenceId;

        public ushort ReferenceId                   // счетчик SAR reference id
        {
            get { return m_referenceId; }
            set { m_referenceId = value; }
        }
	
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

        public SMPPMessageParts(SMPPMessageManager smppmm)
        {
            msgPartsCache = new System.Collections.Hashtable();
            this.ReferenceId = 0;
            this.SMPPmm = smppmm;
        }

		/// <summary>
		/// Puts another message part into collection
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


            
            if (SMPPmm.SMPPSettings.SendFirstSARToRouter)

                return (pe.SectionId == 1) ? pe.ReferenceId : (ushort)0;

            else   return (mpeList.Count == pe.TotalSections) ? pe.ReferenceId : (ushort) 0;
        }

        /// <summary>
        /// Puts another message part into collection
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

        ///// <summary>
        ///// Check for full message existance 
        ///// </summary>
        ///// <returns>refNum of the full message, otherwise 0x00</returns>
        //public byte CheckForFullMessage(byte refNum, byte totalSections)
        //{
        //    List<messagePartEntry> mpeList;
        //    if (msgPartsCache.ContainsKey(refNum))
        //        mpeList = (List<messagePartEntry>)msgPartsCache[refNum];
        //    else
        //        return 0x00;
        //    // Check out Refnums
            
        //    if (mpeList.Count == totalSections)
        //    {
        //        return refNum;
        //    }
        //    return 0x00;
            
        //}

        /// <summary>
        /// Merge SAR message data 
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
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool predicate_message_state(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.message_state)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool predicate_network_error_code(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.network_error_code)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static bool predicate_sar_refnum(OptionalParameter p)
        {
            if (p.Param==OptionalParameter.tagEnum.sar_msg_ref_num)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool predicate_sar_total(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.sar_total_segments)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool predicate_sar_seqnum(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.sar_segment_seqnum)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool predicate_message_payload(OptionalParameter p)
        {
            if (p.Param == OptionalParameter.tagEnum.message_payload)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Split user data byte array to SAR blocks 
        /// </summary>
        /// <param name="connectorId">connector id</param>
        /// <param name="bytes">user data byte array</param>
        /// <param name="Sar_segment_length">the size of SAR segment</param>
        /// <returns>SAR blocks quantity</returns>
        public ushort 
            SplitToSAR(Guid connectorId, byte[] bytes, int Sar_segment_length)
        {
            int maxSARLen = Sar_segment_length;
            int lastSARLen = bytes.Length % maxSARLen;     // размер последнего доп. блока (если он нужен)
            //СМС с sar параметрами на СМСЦ переводятся в формат с UDH, то длина текстовой части СМС не должна превышать 134 октетов (140-6).
            //в GSM-default длина текста каждого СМС в несжатом виде должна быть не более 153 символов (134*8/7=153), а в юникоде 67 символов (134 /2=67). 
            ushort SARCount = (lastSARLen == 0) ? (ushort)(bytes.Length / maxSARLen) : (ushort)(bytes.Length / maxSARLen + 1);
            List<messagePartEntry> mpeList = new List<messagePartEntry>();
           
            ReferenceId++;

            if (ReferenceId > 255)//smsc не поддерживают 2-х байтный номер
                ReferenceId = 0;

            m_SMPPmm.Logger.Write(new LogMessage("Дробим на части sms " + bytes.Length + " кол-во сегментов: " + SARCount+" ref_num "+ReferenceId , LogSeverity.Debug));
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
        /// Get optional parameter list for cached SAR blocks
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
}