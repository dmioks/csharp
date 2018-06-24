using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Dmioks.Common.Binary;
using Dmioks.Common.Utils;
using System.Net.Sockets;
using System.Diagnostics;
using System.IO;
using Dmioks.Common.Json;

namespace Dmioks.Common.Server
{
    public class WebSocketClient : JsonEntitySocketClient
    {
        protected bool m_bServerSide = true;

        public WebSocketClient(TcpClient tc, Stream stream, string sName, int iUniqueId, int iAlivePeriodMilles, int iWriteQueueCapacity) : base(tc, stream, sName, iUniqueId, null, iAlivePeriodMilles, iWriteQueueCapacity)
        {

        }

        /*
        protected override void OnMessageReceived(JsonMessage objMessage)
        {
            int iType = objMessage.Get<int>("type");

            if (iType == (int)eMessageType.Ping)
            {
                JsonMessage objMessagePong = new JsonMessage();

                while (!m_queueToSend.TryEnqueue(DEFAULT_TRY_SEND_PERIOD_MILLES, objMessagePong))
                {
                    if (m_bStop)
                    {
                        break;
                    }
                }
            }
        }

        protected override void ReadThreadImp(ref JsonMessage objRef)
        {
            if (m_bServerSide)
            {
                StringBuilder sb = StringBuilderPool.Instance.Get();
                SmallBuffer buf = new SmallBuffer();
                string sHeader = null;

                try
                {
                    while (!m_bStop && m_tc.Connected)
                    {
                        // Read until end of Client Handshake
                        char ch = m_br.ReadChar();
                        //Console.Write(ch + ' ');

                        sb.Append(ch);
                        if (buf.Append(ch))
                        {
                            sHeader = sb.ToString();
                            break;
                        }
                    }
                }
                finally
                {
                    StringBuilderPool.Instance.Put(sb);
                }

                Console.WriteLine(sHeader);
                HttpHeaderParser hhp = HttpHeaderParser.Parse(sHeader);

                if (!hhp.IsValidForRFC6455)
                {
                    this.Close("Client does not support RFC6455\r\n" + sHeader);
                }
                else
                {
                    string sHandshakeResponse = hhp.GenerateReply();

                    m_bw.WriteString(sHandshakeResponse);
                    /*
                    if (!m_queueToSend.TryEnqueue(DEFAULT_TRY_SEND_PERIOD_MILLES, sHandshakeResponse))
                    {
                        this.Close("Cannot send handshake to client");
                    }
                    * /
                }
            }

            while (!m_bStop && m_tc.Connected)
            {
                Object objMessage = ProtocolRfc6455.ReadMessage(m_br);
                Console.WriteLine(string.Concat(this.UniqueName, "  Msg B.E.<-F.E. ", objMessage));

                if (!m_queueToSend.TryEnqueue(100, objMessage as JsonMessage))
                {
                    this.Close("Cannot enqueue message");
                }

                /*
                byte bt = m_br.ReadByte();

                Console.Write(bt.ToString("X4") + ' ');

                //this.OnMessageReceivedImp(null);
                * /
            }
        }

        protected override void WriteThreadImp(ref JsonMessage objRef)
        {
            long lNextId = 0;
            string sMsg = null;

            if (0 < this.AlivePeriod)
            {
                lNextId++;
                objRef = new JsonMessage();
                //this.SendMessage(m_bw, bMsg, arrNumBuffer);
            }

            int iTryTakeMilles = 0 < this.AlivePeriod ? this.AlivePeriod : DEFAULT_TRY_SEND_PERIOD_MILLES;

            while (!m_bStop && m_tc.Connected)
            {
                if (m_queueToSend.TryDequeue(iTryTakeMilles, out JsonMessage jsonObject))
                {
                    lNextId++;
                    ProtocolRfc6455.WriteMessage(m_bw, sMsg);
                    Console.WriteLine(string.Concat(this.UniqueName, "  Msg B.E.->F.E. ", sMsg));
                    //bMsg.Id = lNextId;
                    //this.SendMessage(m_bw, bMsg, arrNumBuffer);
                }
                else if (0 < this.AlivePeriod)
                {
                    lNextId++;
                    //bMsg = BinMessage.CreateEmptyAlive(lNextId);
                    //this.SendMessage(m_bw, bMsg, arrNumBuffer);
                }
            }
        }
        */
    }

    sealed class SmallBuffer
    {
        const int BUF_SIZE = 4;
        private int m_i = 0;
        private int m_j = 0;
        private char[] m_arr = new char[BUF_SIZE];

        private static int Next (ref int i)
        {
            i ++;

            if (i == BUF_SIZE)
            {
                i = 0;
            }

            return i;
        }

        public bool Append (char ch)
        {
            m_arr[m_i] = ch;

            Next(ref m_i);

            if (ch == '\n')
            {
                m_j = m_i;
                return m_arr[m_j] == '\r' &&  m_arr[Next(ref m_j)] == '\n' &&  m_arr[Next(ref m_j)] == '\r';
            }

            return false;
        }
    }
}
