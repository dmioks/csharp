using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Server
{
    public abstract class ClientPool<T> : ObjectPool<T> where T : BinSocketClient
    {
        private int m_iNextId = 0;
        protected object m_objLocker = new object();

        public delegate object DelegateProcessBinClient<T>(T binClient) where T : BinSocketClient;

        public readonly string Host;
        public readonly int Port;
        public readonly string BaseClientName;
        public readonly int AlivePeriodMilles;
        public readonly int WriteQueueCapacity;

        public ClientPool(string sHost, int iPort, string sBaseClientName, int iAlivePeriodMilles, int iWriteQueueCapacity)
        {
            this.Host = sHost;
            this.Port = iPort;
            this.BaseClientName = sBaseClientName;
            this.AlivePeriodMilles = iAlivePeriodMilles;
            this.WriteQueueCapacity = iWriteQueueCapacity;
        }

        protected int GetNextId()
        {
            lock (m_objLocker)
            {
                return ++ m_iNextId;
            }
        }

        public object ProcessClient(DelegateProcessBinClient<T> dpbc)
        {
            T binClient = this.Get();

            try
            {
                return dpbc(binClient);
            }
            catch (Exception excp)
            {
                throw new Exception("ProcessClient() ERROR", excp);
            }
            finally
            {
                this.Put(binClient);
            }
        }
    }
}
