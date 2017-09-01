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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Cryptany.Core.Connectors;
using Cryptany.Core;
using Cryptany.Common.Logging;
using Cryptany.Core.Management;

namespace Cryptany.Core.Connectors
{
    /// <summary>
    /// SMPPConnector class: Async socket connection to SMSC
    /// </summary>
    public class SMPPConnector : AbstractConnector
    {
        public SMPPMessageManager SMPPmm { get; set; }
        
        /// <summary>
        /// ip сервера, к которому подключается коннектор
        /// </summary>
        public IPEndPoint ServerAddress { get; set; }
        
        /// <summary>
        /// ID коннектора в системе
        /// </summary>
        public string SystemID { get; set; }
        
        /// <summary>
        /// Пароль коннектора
        /// </summary>
        public string ServerPass { get; set; }
        
        /// <summary>
        /// сокет соединения с сервером
        /// </summary>
        protected Socket m_socket;

        /// <summary>
        /// проверка сокета m_socket
        /// </summary>
        public bool SocketConnected
        {
            get
            {
                bool blockingState = m_socket.Blocking;
                Trace.WriteLine("ConnectorId = " + SMPPmm.ConnectorId + ": checking socket connection. blockingstate= " + blockingState);
                try
                {
                    byte[] tmp = new byte[1];
                    m_socket.Blocking = false;
                    m_socket.Send(tmp, 0, 0);
                    return true;
                }
                catch (SocketException e)
                {
                    Trace.WriteLine("ConnectorId = " + SMPPmm.ConnectorId + ": SocketException: " + e);
                    if (e.NativeErrorCode.Equals(10035))
                        return true;
                    else
                    {
                        return false;
                    }
                }
                finally
                {
                    m_socket.Blocking = blockingState;
                }
            }
        }
       
        /// <summary>
        /// Делегат доступа к асинхронному методу получения данных
        /// </summary>
        protected AsyncCallback m_asyncCallBack_Rcv;

        /// <summary>
        /// результат выполнения асинхронного метода получения данных
        /// </summary>
        protected IAsyncResult  m_asyncResult_Rcv;

        /// <summary>
        /// делегат доступа к асинхронному методу отсылки данных
        /// </summary>
        protected AsyncCallback m_asyncCallBack_Snd;

        /// <summary>
        /// результат выполнения асинхронного метода отсылки данных
        /// </summary>
        protected IAsyncResult m_asyncResult_Snd;

        public class SocketPacket
        {
            public readonly byte[] dataBuffer;
            public readonly Socket socket;

            public SocketPacket(Socket sock, int size)
            {
                socket = sock;
                dataBuffer = new byte[size];
            }

            /// <summary>
            /// ожидаем длину smpp-пакета или его содержимое
            /// </summary>
            public bool WaitingPacketLength { get; set; }

            /// <summary>
            /// длина ожидаемого пакета
            /// </summary>
            public uint PacketLength { get; set; }

            /// <summary>
            /// Текущий индекс в буффере
            /// </summary>
            public int BufferIdx { get; set; }
        }

        public SMPPConnector(string ipAddress, string port, string systemID, string password, SMPPMessageManager smppmm) : base(smppmm)
        {
            // Инициализация логгера (XMLLogger)
			// Инициализация параметров соединения
            try
            {
                string strIP = ipAddress;                            // м.б. в виде "127.  0.  0.  1"
                string[] strIpParts = strIP.Split('.');
                for (int i = 0; i < strIpParts.Length; i++)
                    strIpParts[i] = strIpParts[i].Trim();
                strIP = String.Join(".", strIpParts);
                IPAddress ip = IPAddress.Parse(strIP);
                
                int portNo = Convert.ToInt32(port);
                
                ServerAddress = new IPEndPoint(ip, portNo);
            }
            catch (Exception Exp)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector constructor: " + Exp, LogSeverity.Error));
                throw;
            }
            SystemID = systemID;
            ServerPass = password;
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SMPPmm = smppmm;
            m_socket.ReceiveBufferSize = SMPPmm.SMPPSettings.SocketDataBufferSize;
            m_socket.SendBufferSize = SMPPmm.SMPPSettings.SocketDataBufferSize;
            
            LingerOption lo = new LingerOption(true, 10);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, lo);
            m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, false);
        }

        /// <summary>
        /// Подключается к серверу, если ещё не подключен, и слушает
        /// </summary>
        public override bool Start()
        {
            bool isOk = false;
            try
            {
                if (!m_socket.Connected)
                    m_socket.Connect(ServerAddress);
                if (m_socket.Connected)
                {
                    // Wait for data asynchronously 
                    WaitForData_Rcv();
                    isOk = true;
                }
            }
            catch (Exception e)
            {
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.Critical);
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector Start method: " + e, LogSeverity.Error));
                isOk = false;
            }
            return isOk;
        }

        protected void WaitForData_Rcv()
        {
            //Wait for data asynchronously
            try
            {
                if  (m_asyncCallBack_Rcv == null) 
                    m_asyncCallBack_Rcv = OnDataReceived;
                //Start listening to the data asynchronously
                SocketPacket theSocPkt = new SocketPacket(m_socket, SMPPmm.SMPPSettings.SocketDataBufferSize);
                //ожидаем длину smpp-пакета
                theSocPkt.WaitingPacketLength = true;
                try
                {
                    SocketError errcode = SocketError.Success;
                    if (theSocPkt.socket.Connected)
                        theSocPkt.socket.BeginReceive(theSocPkt.dataBuffer, 0, 4,
                                                      SocketFlags.None, out errcode, m_asyncCallBack_Rcv, theSocPkt);
                    if (errcode != SocketError.Success)
                    {
                        if (Logger != null)
                            Logger.Write(new LogMessage("Exception in SMPPConnector OnDataReceived method: sockerror:" + errcode, LogSeverity.Error));
                        ServiceManager.LogEvent("Exception in SMPPConnector OnDataReceived method: sockerror:" + errcode, EventType.Error, EventSeverity.High);
                    }

                }
                catch (SocketException sex)
                {
                    if (Logger != null)
                        Logger.Write(new LogMessage("Exception in SMPPConnector OnDataReceived method: " + sex.Message + " sockerror:" + sex.ErrorCode, LogSeverity.Error));
                    ServiceManager.LogEvent(sex.ToString(), EventType.Error, EventSeverity.High);
                }
            }
            catch (SocketException sex)
            {
                Logger.Write(new LogMessage("Receive operation failed (code=" + sex.ErrorCode +"): " + sex.Message, LogSeverity.Error));
                ServiceManager.LogEvent("Receive operation failed (code=" + sex.ErrorCode +"): " + sex.Message, EventType.Error, EventSeverity.High);
            }
            catch(Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector WaitForData_Rcv method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
        }

        protected void OnDataReceived(IAsyncResult asyn)
        {
            SocketPacket stateObject = (SocketPacket)asyn.AsyncState;
            try
            {
                SocketError errcode;
                int iRx = stateObject.socket.EndReceive(asyn, out errcode);

                if (errcode != SocketError.Success)
                {
                    if (stateObject.socket.Connected)
                        stateObject.socket.Disconnect(true);
                        SMPPmm.pcConnectionState.RawValue = 0;
                }

                else if (iRx > 0)
                {
                    if (!stateObject.WaitingPacketLength)
                    {
                        if (iRx + stateObject.BufferIdx == stateObject.PacketLength) //получили пакет полностью
                        {
                            ByteArrayToString((int)stateObject.PacketLength, stateObject.dataBuffer, "Rcv",
                                              SMPPmm.SMPPSettings.LoggingEnabled);
                            //обработка полученного пакета
                            SMPPmm.Receive(this, (int) stateObject.PacketLength, stateObject.dataBuffer);
                            // декодирование полученного SMPP PDU
                            stateObject.WaitingPacketLength = true;
                            stateObject.PacketLength = 0;
                            stateObject.BufferIdx = 0;
                            Array.Clear(stateObject.dataBuffer, 0, stateObject.dataBuffer.Length);
                        }
                        else if (iRx + stateObject.BufferIdx < stateObject.PacketLength) //продолжаем получать часть пакета
                            stateObject.BufferIdx += iRx; 
                        else
                            Logger.Write(new LogMessage("Получены непонятные данные по сети..", LogSeverity.Error));
                    }
                    else
                    {
                        stateObject.PacketLength =  SMPPMessageManager.GetPacketLength(stateObject.dataBuffer);
                        if (stateObject.PacketLength > 0)
                        {
                            Trace.WriteLine("получили размер пакета " + stateObject.PacketLength);
                            stateObject.WaitingPacketLength = false;
                            stateObject.BufferIdx = 4;
                        }
                    }
                }
                else
                {
                    //Потеряли коннект
                    if (stateObject.socket.Connected)
                        stateObject.socket.Disconnect(true);
                    SMPPmm.pcConnectionState.RawValue = 0;
                }
            }
            catch(SocketException se)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector OnDataReceived method: socketerror " + se.SocketErrorCode, LogSeverity.Error));
                SMPPmm.pcConnectionState.RawValue = 0;
                ServiceManager.LogEvent(se.ToString(), EventType.Error, EventSeverity.High);
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector OnDataReceived method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
            finally
            {
                // Initiate new recieving cycle
                try
                {
                    if (stateObject.socket.Connected)
                    {
                        SocketError errcode;
                        int reqlen = 0;
                        if (stateObject.WaitingPacketLength)
                        {
                            if ((stateObject.BufferIdx + 4) <= stateObject.dataBuffer.Length)
                                reqlen = 4;
                        }
                        else
                        {
                            if (((int) stateObject.PacketLength - stateObject.BufferIdx) <=
                                (stateObject.dataBuffer.Length - stateObject.BufferIdx))
                                reqlen = (int) stateObject.PacketLength - stateObject.BufferIdx;
                        }
                        if (reqlen <= 0) //сбился tcp-траффик
                        {
                            stateObject.socket.Disconnect(true);
                        }
                        else
                        {
                            stateObject.socket.BeginReceive(stateObject.dataBuffer, stateObject.BufferIdx, reqlen,
                                SocketFlags.None, out errcode, m_asyncCallBack_Rcv, stateObject);
                                if (errcode != SocketError.Success && errcode != SocketError.Disconnecting && errcode != SocketError.NotConnected)
                                {
                                    if (Logger != null)
                                        Logger.Write(new LogMessage("Exception in SMPPConnector OnDataReceived method: sockerror:" + errcode,
                                                LogSeverity.Error));
                                    ServiceManager.LogEvent("Exception in OnDataReceived method: sockerror:" + errcode, EventType.Error, EventSeverity.High);
                                    SMPPmm.pcConnectionState.RawValue = 0;
                                }
                        }
                    }
                }
                catch(SocketException sex)
                {
                    if (Logger != null)
                    {
                        Logger.Write(new LogMessage("Exception in SMPPConnector OnDataReceived method: " + sex.Message + " sockerror:" + sex.SocketErrorCode, LogSeverity.Error));
                    }
                    ServiceManager.LogEvent(sex.ToString(), EventType.Error, EventSeverity.High);
                    SMPPmm.pcConnectionState.RawValue = 0;
                }
            }
        }					

        /// <summary>Stops the connector, purging all cached messages first and then unbinding connections</summary>
        /// <returns>The result of the operation</returns>
        public override bool Stop()
        {
            bool isOk;
            try
            {
                if (m_socket.Connected)
                    m_socket.Disconnect(true);
                SMPPmm.pcConnectionState.RawValue = 0;
                isOk = true;
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector Stop method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
                isOk = false;
            }
            return isOk;
        }

        public bool AsyncSend(byte[] bytes)
        {
            bool isOk;
            try
            {
                // Wait for data to send asynchronously 
                WaitForData_Snd(bytes);
                isOk = true;
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector AsyncSend method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
                isOk = false;
            }
            return isOk;
        }

        protected void WaitForData_Snd(byte[] bytes)
        {
            // Wait for data to send asynchronously
            try
            {
                if (m_asyncCallBack_Snd == null)
                    m_asyncCallBack_Snd = OnDataSent;
                // Start sending the data asynchronously
                SocketPacket theSocPkt = new SocketPacket(m_socket, SMPPmm.SMPPSettings.SocketDataBufferSize);
                m_asyncResult_Snd = m_socket.BeginSend(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, m_asyncCallBack_Snd, theSocPkt);
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector WaitForData_Snd method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
        }

        protected void OnDataSent(IAsyncResult asyn)
        {
            try
            {
                SocketPacket theSockId = (SocketPacket) asyn.AsyncState;
                int iRx = theSockId.socket.EndSend(asyn);
                ByteArrayToString(iRx, theSockId.dataBuffer, "Snd", SMPPmm.SMPPSettings.LoggingEnabled);
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector OnDataSent method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
        }

        public bool Send(byte[] bytes)
        {
            bool isOk = false;
            try
            {
                // Send data synchronously 
                if (m_socket.Connected)
                {
                    SocketError errCode;
                    //Debug.WriteLine("USSD: длина буффера " + bytes.Length);
                    int iRx = m_socket.Send(bytes,0 ,bytes.Length, SocketFlags.None, out errCode );
                    if (iRx==0)
                    {
                        if (m_socket.Connected)
                            m_socket.Disconnect(true);
                        SMPPmm.pcConnectionState.RawValue = 0;
                    }
                    else if (errCode == SocketError.Success)
                    {
                        ByteArrayToString(iRx, bytes, "Snd", SMPPmm.SMPPSettings.LoggingEnabled);
                        isOk = true;
                    }
                    else
                    {
                        if (m_socket.Connected)
                            m_socket.Disconnect(true);
                        SMPPmm.pcConnectionState.RawValue = 0;
                    }
                }
                else
                {
                    Logger.Write(new LogMessage("Send operation failed..not connected to SMSC ", LogSeverity.Error));
                    SMPPmm.pcConnectionState.RawValue = 0;
                }

           
            }
            catch (SocketException sex)
            {
                Logger.Write(new LogMessage("Send operation failed (code=" + sex.ErrorCode + "): " + sex.Message, LogSeverity.Error));
                ServiceManager.LogEvent(sex.ToString(), EventType.Error, EventSeverity.High);
                if (m_socket.Connected)
                    m_socket.Disconnect(true);
                SMPPmm.pcConnectionState.RawValue = 0;
                
            }
            catch (Exception e)
            {
                if (Logger != null)
                    Logger.Write(new LogMessage("Exception in SMPPConnector Send method: " + e, LogSeverity.Error));
                ServiceManager.LogEvent(e.ToString(), EventType.Error, EventSeverity.High);
            }
            if (!isOk) SMPPmm.ReadyToSendMessages.Reset(); 
            return isOk;
        }

        // Представление массива байт в виде символьной строки      
        public string ByteArrayToString(int bytesCount, byte[] bytes, string direction, bool saveToLog)
        {
            String dataStr = string.Empty;
            try
            {
                dataStr = Encoding.ASCII.GetString(bytes, 0, bytesCount);
                if (Logger != null && saveToLog)
                {
                    string InfStr = (direction == "Rcv") ? "Received data from SMPP server." : "Sent data to SMPP server.";
                    Trace.WriteLine(InfStr + " " + dataStr);
                }
            }
            catch (Exception)
            {}
            
            return dataStr;
        }
    }
}