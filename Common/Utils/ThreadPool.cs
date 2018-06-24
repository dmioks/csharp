using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Logs;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Utils
{
    public delegate object DelegateExecuteThreadWithParam(object objParam);
    public delegate void DelegateExecuteThread();

    public class ThreadEnvelope
    {
        protected readonly Thread m_t;
        protected DelegateExecuteThreadWithParam m_delegate = null;
        protected EventWaitHandle m_ewh = new AutoResetEvent(false);
        protected object m_objParam = null;

        private static readonly ILog m_logger = LogFactory.GetLogger(typeof(ThreadEnvelope));

        protected ThreadEnvelope(Thread t)
        {
            Debug.Assert(t != null);

            m_t = t;
        }

        internal void Execute (DelegateExecuteThreadWithParam det, object objParam)
        {
            Debug.Assert(det != null);

            m_delegate = det;
            m_objParam = objParam;
            m_ewh.Set();
        }

        public void Stop()
        {
            m_delegate = null;
            m_objParam = null;
            m_ewh.Set();
        }

        internal static ThreadEnvelope Create()
        {
            Thread t = new Thread(new ParameterizedThreadStart(ExecuteThread));
            t.IsBackground = true;

            ThreadEnvelope te = new ThreadEnvelope(t);

            t.Start(te);

            return te;
        }

        protected static void ExecuteThread(object objParam)
        {
            ThreadEnvelope te = objParam as ThreadEnvelope;
            Debug.Assert(te != null);

            while (true)
            {
                te.m_ewh.WaitOne();

                if (te.m_delegate != null)
                {
                    try
                    {
                        te.m_delegate(te.m_objParam);
                    }
                    catch (Exception excp)
                    {
                        m_logger.Error(excp, "ExecuteThread() ERROR");
                    }

                    ThreadHelper.Instance.Put(te);
                }
            }
        }

        internal protected void Reset()
        {
            m_ewh.Reset();
        }
    }

    public class ThreadHelper : ObjectPool<ThreadEnvelope>
    {
        public readonly static ThreadHelper Instance = new ThreadHelper();

        protected override ThreadEnvelope CreateNew()
        {
            ThreadEnvelope te = ThreadEnvelope.Create();

            return te;
        }

        public static void StopAllInPool()
        {
            ThreadEnvelope te = ThreadHelper.Instance.GetIfAny();

            while (te != null)
            {
                te.Stop();
                ThreadHelper.Instance.m_iCreatedCount --;

                te = ThreadHelper.Instance.GetIfAny();
            }
        }

        public static void Execute (object objParam, DelegateExecuteThreadWithParam det)
        {
            if (det != null)
            {
                ThreadEnvelope te = ThreadHelper.Instance.Get();
                te.Execute(det, objParam);
            }
        }

        protected override void OnPut(ThreadEnvelope obj)
        {
            obj.Reset();
        }
    }
}
