using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dmioks.Common.Collections
{
    public class SyncBlockingQueue<T>
    {
        private readonly QueueArray<T> m_q;
        private readonly object m_Locker = new Object();

        private readonly EventWaitHandle m_ewhNotEmpty = new ManualResetEvent(false);
        private readonly EventWaitHandle m_ewhNotFull  = new ManualResetEvent(true);

        public SyncBlockingQueue(int iCapacity)
        {
            m_q = new QueueArray<T>(iCapacity);
        }

        /*
        public virtual void Clear()
        {
            lock (m_Locker)
            {
                m_q.Clear();
            }
        }

        public new virtual T Dequeue()
        {
            lock (m_Locker)
            {
                return m_q.Dequeue();
            }
        }
        */

        public virtual bool TryEnqueue(int iTimeOutMilles, T obj)
        {
            if (m_ewhNotFull.WaitOne(iTimeOutMilles))
            {
                lock (m_Locker)
                {
                    if (!m_q.IsFull())
                    {
                        m_q.Enqueue(obj);
                        m_ewhNotEmpty.Set();

                        if (m_q.IsFull())
                        {
                            m_ewhNotFull.Reset();
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool TryDequeue(int iTimeOutMilles, out T objOut)
        {
            if (m_ewhNotEmpty.WaitOne(iTimeOutMilles))
            {
                lock (m_Locker)
                {
                    if (0 < m_q.Count)
                    {
                        objOut = m_q.Dequeue();
                        m_ewhNotFull.Set();

                        if (m_q.Count == 0)
                        {
                            m_ewhNotEmpty.Reset();
                        }

                        return true;
                    }
                }
            }

            objOut = default(T);
            return false;
        }

        /*
        public virtual int Enqueue(T value)
        {
            lock (m_Locker)
            {
                m_q.Enqueue(value);
                m_ewhNotEmpty.Set();

                return m_q.Count;
            }
        }

        public virtual IEnumerator GetEnumerator()
        {
            lock (m_Locker)
            {
                return m_q.GetEnumerator();
            }
        }

        public virtual object Peek()
        {
            lock (m_Locker)
            {
                return m_q.Peek();
            }
        }

        public virtual bool TryPeek(int iTimeOutMilles, out T objOut)
        {
            lock (m_Locker)
            {
                if (m_q.Count == 0)
                {
                    m_ewhNotEmpty.WaitOne(iTimeOutMilles);
                }

                if (0 < m_q.Count)
                {
                    objOut = m_q.Peek();
                    return true;
                }

                objOut = default(T);
                return false;
            }
        }

        public virtual T[] ToArray()
        {
            lock (m_Locker)
            {
                return m_q.ToArray();
            }
        }
        */

        public int Count
        {
            get
            {
                lock (m_Locker)
                {
                    return m_q.Count;
                }
            }
        }

        public override string ToString()
        {
            int iCount = this.Count;
            return string.Format("SyncQueue<{0}> {{Count = {1}}}", typeof(T).Name, iCount);
        }
    }
}
