using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Binary;

namespace Dmioks.Common.Server
{
    public class AsyncResponse
    {
        protected EventWaitHandle m_ewh = new ManualResetEvent(false);
        protected IMessage m_ResponseMessage = null;

        public void WaitResponse(int iMillesecondsTimeout)
        {
            m_ewh.WaitOne(iMillesecondsTimeout);
        }

        public void WaitResponse()
        {
            m_ewh.WaitOne();
        }

        public void Set()
        {
            m_ewh.Set();
        }

        public void Reset()
        {
            m_ewh.Reset();
            m_ResponseMessage = null;
        }

        public IMessage ResponseMessage
        {
            get
            {
                return m_ResponseMessage;
            }
            set
            {
                m_ResponseMessage = value;
                m_ewh.Set();
            }
        }
    }
}
