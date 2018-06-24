using Dmioks.Common.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Collections;
using Dmioks.Common.Logs;

namespace Dmioks.Common.Server
{
    public delegate SocketClient<T> DelegateCreateClient<T> (TcpClient tc, Stream stream, int iUniqueId) where T : IMessage;

    public abstract class SocketServer<T> where T : IMessage
    {
        private int m_iConnectedCount = 0;

        protected readonly IPAddress m_ipAddress;
        protected readonly int m_iPort;

        protected readonly SyncDictionary<int, SocketClient<T>> m_diCurrentConnections = new SyncDictionary<int, SocketClient<T>>();

        private static ILog m_logger = LogFactory.GetLogger(typeof(SocketServer<T>));

        protected TcpListener m_listener = null;
        protected volatile bool m_bIsToStop = false;
        protected object m_objLocker = new object();
        protected readonly X509Certificate m_cert;

        public SocketServer(IPAddress ipAddr, int iPort, X509Certificate cert)
        {
            Debug.Assert(ipAddr != null);
            Debug.Assert(0 < iPort);

            m_cert = cert;

            m_ipAddress = ipAddr;
            m_iPort = iPort;
        }

        public void Run()
        {
            ThreadHelper.Execute(this, ListenerThread);
        }

        public List<SocketClient<T>> GetCurrentConnections()
        {
            return m_diCurrentConnections.GetValueList();
        }

        protected int GetNextId()
        {
            lock (m_objLocker)
            {
                return ++ m_iConnectedCount;
            }
        }

        protected int ConnectedCount
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_iConnectedCount;
                }
            }
        }

        public void Stop()
        {
            m_bIsToStop = true;
            lock (m_objLocker)
            {
                if (m_listener != null)
                {
                    m_listener.Stop();
                    m_listener = null;
                }
            }
        }

        protected bool CreateAndStartListener()
        {
            if (!m_bIsToStop)
            {
                lock (m_objLocker)
                {
                    if (m_listener == null)
                    {
                        m_listener = new TcpListener(m_ipAddress, m_iPort);
                        //m_listener = new TcpListener(localEndPoint);
                        m_listener.Start();

                        return true;
                    }
                }
            }

            return false;
        }

        protected void StopListener()
        {
            lock (m_objLocker)
            {
                if (m_listener != null)
                {
                    m_listener.Stop();
                    m_listener = null;
                }
            }
        }

        protected abstract SocketClient<T> CreateClient(TcpClient tc, Stream stream, int iUniqueId);

        protected void OnClientConnected(TcpClient tcpClient)
        {
            Debug.Assert(tcpClient != null);

            ThreadHelper.Execute(null, delegate (object objParam)
            {
                Stream stream = null;

                if (m_cert != null)
                {
                    // DK - TODO: Refactor this section
                    SslStream sslStream = new SslStream(tcpClient.GetStream(), false, VerifyClientCertificateCallback, CertificateSelectionCallback, EncryptionPolicy.RequireEncryption);
                    sslStream.AuthenticateAsServer(m_cert, false, SslProtocols.Tls12, false);

                    byte[] arrHello = Encoding.UTF8.GetBytes("Hello");
                    sslStream.Write(arrHello);

                    stream = sslStream;
                }
                else
                {
                    stream = tcpClient.GetStream();
                }

                SocketClient<T> client = CreateClient (tcpClient, stream, this.GetNextId());
                client.SocketClientClosed += SocketClientClosed;
                client.Run();

                m_diCurrentConnections[client.Id] = client;

                m_logger.Info(string.Concat("Started ", client));

                return null;
            });
        }

        private static bool VerifyClientCertificateCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private static X509Certificate2 CertificateSelectionCallback(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            Debug.Assert(0 < localCertificates.Count);
            X509Certificate2 cert = localCertificates[0] as X509Certificate2;
            Debug.Assert(cert != null);

            return cert;
        }

        protected virtual void SocketClientClosed(SocketClient<T> client, string sReason)
        {
            m_diCurrentConnections.Remove(client.Id);
            client.Dispose();
            m_logger.Info("{0} removed du to {1}", client, sReason);
        }

        private static object ListenerThread (Object objParam)
        {
            SocketServer<T> server = objParam as SocketServer<T>;
            Debug.Assert(server != null);

            try
            {
                if (!server.CreateAndStartListener())
                {
                    return null;
                }

                //_logger.Debug("Waiting for a client to connect...");

                while (!server.m_bIsToStop)
                {
                    TcpClient tcpClient = server.m_listener.AcceptTcpClient();
                    server.OnClientConnected(tcpClient);
                }
            }
            catch (Exception ex)
            {
                m_logger.Error("Exception in run thread outside while(alive && runAlive). Try to restart thread.", ex);
            }
            finally
            {
                server.StopListener();

                if (!server.m_bIsToStop)
                {
                    server.Run();
                }
            }

            return null;
        }

        public override string ToString()
        {
            return $"{this.GetType().Name}{{CurrentConnections={m_diCurrentConnections.Count}, AllConnections={this.ConnectedCount}}}";
        }
    }
}
