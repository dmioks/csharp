using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Dmioks.Common.Binary;
using Dmioks.Common.Collections;
using Dmioks.Common.Utils;
using Dmioks.Common.Binary;
using Dmioks.Common.Entity;
using Dmioks.Common.Json;
using Dmioks.Common.Logs;

namespace Dmioks.Common.Server
{
    public delegate void DelegateOnClose<T>(SocketClient<T> sc, string sReason) where T : IMessage;
    public delegate void DelegateOnBeginFileReceive(BinSocketClient bsc, BinFileStream bfs);

    public abstract class SocketClient<T> : ISocketClient<T>, IDisposable where T : IMessage
    {
        protected const int DEFAULT_TRY_SEND_PERIOD_MILLES = 10;
        protected const int DEFAULT_TRY_READ_PERIOD_MILLES = 100;

        private static readonly ILog m_logger = LogFactory.GetLogger(typeof(SocketClient<T>));

        protected readonly TcpClient       m_tc;
        protected readonly BinRead         m_br;
        protected readonly BinWrite        m_bw;

        protected readonly Stream m_stream;

        public readonly int Id;
        public readonly string UniqueName;
        protected readonly IConnectionCallback<T> m_cc;
        public readonly int AlivePeriod;

        protected long m_lLastSendId      = 0;
        protected long m_lLastReceivedId  = 0;
        protected long m_lBytesSend       = 0;
        protected long m_lBytesReceieve   = 0;
        protected object m_objSendLock    = new Object();
        protected object m_objReceiveLock = new Object();

        protected int m_iNextFileId = 0;
        protected object m_objLocker = new object();

        protected readonly SyncBlockingQueue<T> m_queueToSend;
        protected readonly SyncBlockingQueue<T> m_queueReceivedFiles;
        protected readonly SyncDictionary<long, AsyncResponse> m_diIdToResponse = new SyncDictionary<long, AsyncResponse>();
        protected readonly SyncDictionary<int, BinFileStream> m_diFileIdToBinFileStream = new SyncDictionary<int, BinFileStream>();

        protected volatile bool m_bStop = false;
        protected string m_sCloseReason = null;

        public event DelegateOnClose<T> SocketClientClosed;
        public event DelegateOnBeginFileReceive OnBeginFileReceive;

        public readonly int QueueCapacity;

        protected SocketClient(TcpClient tc, Stream stream, string sBaseName, int iUniqueId, IConnectionCallback<T> cc, int iAlivePeriodMilles, int iQueueCapacity)
        {
            Debug.Assert(tc != null);
            Debug.Assert(stream != null);
            Debug.Assert(!string.IsNullOrEmpty(sBaseName));
            Debug.Assert(0 < iUniqueId);
            Debug.Assert(cc != null);

            m_tc = tc;
            m_stream = stream;
            m_cc = cc;
            stream.ReadTimeout = 11000;
            m_br = new BinRead(m_stream);
            m_bw = new BinWrite(m_stream);

            this.Id = iUniqueId;
            this.UniqueName = string.Concat(sBaseName, iUniqueId);
            this.AlivePeriod = iAlivePeriodMilles;
            this.QueueCapacity = iQueueCapacity;

            m_queueToSend = new SyncBlockingQueue<T>(iQueueCapacity);
            m_queueReceivedFiles = new SyncBlockingQueue<T>(iQueueCapacity);
        }

        public bool IsAlive
        {
            get { return !m_bStop && m_tc.Connected; }
        }

        protected int GetNextFileId()
        {
            lock (m_objLocker)
            {
                return ++ m_iNextFileId;
            }
        }

        public void TryEnqueue(T objMessage)
        {
            if (m_tc.Connected && !m_bStop)
            {
                try
                {
                    while (!m_queueToSend.TryEnqueue(DEFAULT_TRY_SEND_PERIOD_MILLES, objMessage))
                    {
                        if (m_bStop)
                        {
                            break;
                        }
                    }
                }
                catch (Exception excp)
                {
                    throw new Exception($"TryEnqueue() ERROR for {objMessage}", excp);
                }
            }
        }

        protected void WriteMessage(T objMessage)
        {
            if (!m_bStop && m_tc.Connected)
            {
                long lId = this.GetNextSendId();
                objMessage.Id = lId;

                this.WriteMessageImp(objMessage);

                if (objMessage.AsyncResponse != null)
                {
                    switch (objMessage.MessageType)
                    {
                        case eMessageType.File:

                            objMessage.AsyncResponse.Set();
                            break;

                        case eMessageType.Request:

                            m_diIdToResponse.Add(objMessage.Id, objMessage.AsyncResponse);
                            break;

                        default:

                            throw new InvalidDataException($"WriteMessage() ERROR. AsyncResponse CANNOT be set for type {objMessage.MessageType} in {objMessage}");
                    }
                }
            }
        }

        public T SendRequest(T objRequestMessage, int iMillesecondsTimeout)
        {
            AsyncResponse ar = new AsyncResponse();

            return SendRequest(objRequestMessage, ar, iMillesecondsTimeout);
        }

        public T SendRequest(T objRequestMessage, AsyncResponse ar, int iMillesecondsTimeout)
        {
            Debug.Assert(objRequestMessage != null && objRequestMessage.MessageType == eMessageType.Request);

            ar.Reset();
            objRequestMessage.AsyncResponse = ar;

            this.TryEnqueue(objRequestMessage);
            ar.WaitResponse(iMillesecondsTimeout);

            return (T) ar.ResponseMessage;
        }

        public void Run()
        {
            ThreadHelper.Execute(this, ReadFileThread);
            ThreadHelper.Execute(this, ReadThread);
            ThreadHelper.Execute(this, WriteThread);
        }

        // Count of sent messages
        public long SentMessagesCount
        {
            get
            {
                lock (m_objSendLock)
                {
                    return m_lLastSendId;
                }
            }
            protected set
            {
                lock (m_objSendLock)
                {
                    m_lLastSendId = value;
                }
            }
        }

        protected long GetNextSendId()
        {
            lock (m_objSendLock)
            {
                m_lLastSendId ++;
                return m_lLastSendId;
            }
        }

        // Count of received messages
        public long ReceivedMessagesCount
        {
            get
            {
                lock (m_objReceiveLock)
                {
                    return m_lLastReceivedId;
                }
            }
            protected set
            {
                lock (m_objReceiveLock)
                {
                    m_lLastReceivedId = value;
                }
            }
        }

        // Count of sent bytes
        public long SentBytesCount
        {
            get
            {
                lock (m_objSendLock)
                {
                    return m_lBytesSend;
                }
            }
            protected set
            {
                lock (m_objSendLock)
                {
                    m_lBytesSend = value;
                }
            }
        }

        // Count of received bytes
        public long ReceivedBytesCount
        {
            get
            {
                lock (m_objReceiveLock)
                {
                    return m_lBytesReceieve;
                }
            }
            protected set
            {
                lock (m_objReceiveLock)
                {
                    m_lBytesReceieve = value;
                }
            }
        }

        protected abstract T ReadMessage();
        protected abstract void WriteMessageImp(T objMessage);

        protected abstract T Create(eMessageType emt);

        protected abstract void ProcessEvent(T objEvent);
        protected abstract void ProcessPong(T objPong);

        protected void OnMessageReceivedImp(T objMessage)
        {
            try
            {
                switch (objMessage.MessageType)
                {
                    case eMessageType.Ping:

                        T objPong = this.Create(eMessageType.Pong);

                        // Help Props
                        objPong.RequestId = objMessage.Id;
                        objPong.RequestTime = objMessage.Time;

                        this.TryEnqueue(objPong);

                        break;

                    case eMessageType.Request:

                        m_cc.ProcessRequest(this, objMessage);
                        break;

                    case eMessageType.Response:

                        if (0 < objMessage.RequestId)
                        {
                            if (m_diIdToResponse.TryGetValue(objMessage.RequestId, out AsyncResponse asyncResponse))
                            {
                                asyncResponse.ResponseMessage = objMessage;
                                m_diIdToResponse.Remove(objMessage.RequestId);
                            }
                        }
                        else
                        {
                            Debug.Assert(false, $"Message does not have RequestId {objMessage}");
                        }

                        break;

                    case eMessageType.Event:

                        this.ProcessEvent(objMessage);
                        break;

                    case eMessageType.Close:

                        this.Close("Closed by request from Other End");
                        break;

                    case eMessageType.Pong:

                        this.ProcessPong(objMessage);
                        break;

                    case eMessageType.EmptyAlive:

                        // DK - At he moment does nothing here

                        break;

                    default:

                        Debug.Assert(false, $"Received message with unsupported type {objMessage.MessageType}");
                        break;
                }
            }
            catch (Exception excp)
            {
                m_logger.Error(excp, "OnMessageReceivedImp() ERROR for {0}", objMessage);
                this.Close($"OnMessageReceivedImp() ERROR for {objMessage}");
            }
        }

        protected void OnMessageReceived(T objMessage)
        {
            Debug.Assert(objMessage != null);

            if (objMessage.MessageType == eMessageType.File)
            {
                m_queueReceivedFiles.TryEnqueue(DEFAULT_TRY_READ_PERIOD_MILLES, objMessage);
            }
            else
            {
                ThreadHelper.Execute(objMessage, delegate (object objParam)
                {
                    this.OnMessageReceivedImp(objMessage);

                    return null;
                });
            }
        }

        public void Close(string sReason)
        {
            if (!m_bStop)
            {
                m_tc.Close();
                m_sCloseReason = $"{this} Closed due to reason '{sReason}'";
                m_logger.Warn(m_sCloseReason);
                m_bStop = true;

                if (this.SocketClientClosed != null)
                {
                    this.SocketClientClosed(this, sReason);
                }
            }
        }

        private static object ReadFileThread(Object objParam)
        {
            SocketClient<T> thisClient = objParam as SocketClient<T>;
            Debug.Assert(thisClient != null);

            T objMessage = default(T);

            try
            {
                while (!thisClient.m_bStop && thisClient.m_tc.Connected)
                {
                    thisClient.m_queueReceivedFiles.TryDequeue(DEFAULT_TRY_READ_PERIOD_MILLES, out objMessage);

                    if (objMessage != null)
                    {
                        Debug.Assert(objMessage.MessageType == eMessageType.File);

                        FileMessageBody fmb = objMessage.FileBody;

                        BinFileStream bfs = null;

                        if (fmb != null)
                        {
                            int iFileId = fmb.FileId;

                            if (fmb.ChunkId == FileMessageBody.VERY_FIRST_CHUNK_ID)
                            {
                                string sContext = fmb.Context;
                                string sFileName = fmb.FileName;

                                bfs = new BinFileStream(thisClient as BinSocketClient, sContext, sFileName, iFileId, eBinFileStreamType.ReadOnly);
                                thisClient.m_diFileIdToBinFileStream.Add(iFileId, bfs);

                                if (thisClient.OnBeginFileReceive != null)
                                {
                                    thisClient.OnBeginFileReceive(thisClient as BinSocketClient, bfs);
                                }

                                bfs.ProcessReadFileChunk(fmb);
                            }
                            else
                            {
                                if (thisClient.m_diFileIdToBinFileStream.TryGetValue(iFileId, out bfs))
                                {
                                    bfs.ProcessReadFileChunk(fmb);
                                }
                                else
                                {
                                    thisClient.Close($"Received File Message but cannot find appropriate stream. {fmb}");
                                }
                            }

                            if (fmb.IsFinalChunk)
                            {
                                thisClient.m_diFileIdToBinFileStream.Remove(iFileId);
                            }
                        }
                        else
                        {
                            thisClient.Close($"Received File Message without FileBody {fmb}");
                        }

                        objMessage = default(T);
                    }
                }
            }
            catch (Exception excp)
            {
                m_logger.Error(excp, "{0} ReadFileThread ERROR for {1}:\r\n", thisClient, objMessage);
                thisClient.Close(excp.Message);
            }

            return null;
        }

        private static object ReadThread(Object objParam)
        {
            SocketClient<T> thisClient = objParam as SocketClient<T>;
            Debug.Assert(thisClient != null);

            long lLastReceivedId = 0;
            T objMessage = default(T);

            try
            {
                while (!thisClient.m_bStop && thisClient.m_tc.Connected)
                {
                    objMessage = thisClient.ReadMessage();
                    Debug.Assert(objMessage != null);

                    if (lLastReceivedId + 1 != objMessage.Id)
                    {
                        throw new Exception($"Incorrect read id sequence (expected={lLastReceivedId + 1}, received={objMessage.Id})");
                    }

                    lLastReceivedId ++;
                    thisClient.ReceivedMessagesCount = lLastReceivedId;

                    thisClient.OnMessageReceived(objMessage);

                    objMessage = default(T);
                }
            }
            catch (BinHelperException bhexcp)
            {
                thisClient.Close(bhexcp.Message);
            }
            catch (EndOfStreamException esexcp)
            {
                thisClient.Close(esexcp.Message);
            }
            catch (ObjectDisposedException odexcp)
            {
                thisClient.Close(odexcp.Message);
            }
            catch (IOException ioexcp)
            {
                thisClient.Close(ioexcp.Message);
            }
            catch (Exception excp)
            {
                m_logger.Error(excp, "{0} ReadThread ERROR for {1}:\r\n", thisClient, objMessage);
                thisClient.Close(excp.Message);
            }

            return null;
        }

        private static object WriteThread(Object objParam)
        {
            SocketClient<T> thisClient = objParam as SocketClient<T>;
            Debug.Assert(thisClient != null);

            T objMessage = default(T);

            try
            {
                if (0 < thisClient.AlivePeriod)
                {
                    objMessage = thisClient.Create(eMessageType.EmptyAlive);

                    thisClient.WriteMessage(objMessage);
                }

                int iTryTakeMilles = 0 < thisClient.AlivePeriod ? thisClient.AlivePeriod : DEFAULT_TRY_READ_PERIOD_MILLES;

                while (!thisClient.m_bStop && thisClient.m_tc.Connected)
                {
                    objMessage = default(T);

                    if (thisClient.m_queueToSend.TryDequeue(iTryTakeMilles, out T jsonObjectFromQueue))
                    {
                        thisClient.WriteMessage(jsonObjectFromQueue);
                    }
                    else if (0 < thisClient.AlivePeriod)
                    {
                        objMessage = thisClient.Create(eMessageType.EmptyAlive);

                        thisClient.WriteMessage(objMessage);
                    }
                }
            }
            catch (IOException ioexcp)
            {
                thisClient.Close(ioexcp.Message);
            }
            catch (ObjectDisposedException odexcp)
            {
                thisClient.Close(odexcp.Message);
            }
            catch (Exception excp)
            {
                m_logger.Error(excp, "{0} WriteThread ERROR for {1}:\r\n", thisClient, objMessage);
                thisClient.Close(excp.Message);
            }

            return null;
        }

        public override int GetHashCode()
        {
            return this.Id;
        }

        public override bool Equals(object obj)
        {
            SocketClient<T> sc = obj as SocketClient<T>;

            return obj != null && this.Id == sc.Id;
        }

        public virtual void Dispose()
        {
            if (SocketClientClosed != null)
            {
                Delegate[] clientList = SocketClientClosed.GetInvocationList();
                foreach (var d in clientList)
                    SocketClientClosed -= (d as DelegateOnClose<T>);
            }
 
            if (OnBeginFileReceive != null)
            {
                Delegate[] clientList = OnBeginFileReceive.GetInvocationList();
                foreach (var d in clientList)
                    OnBeginFileReceive -= (d as DelegateOnBeginFileReceive);
            }

            m_br.Dispose();
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}{{UniqueName={this.UniqueName}; Msg Sent/Received={this.SentMessagesCount}/{this.ReceivedMessagesCount}; Bytes Sent/Received={this.SentBytesCount}/{this.ReceivedBytesCount};}}";
        }
    }

    public interface ISocketClient<T>
    {
        void TryEnqueue(T jmResponse);
    }
}
