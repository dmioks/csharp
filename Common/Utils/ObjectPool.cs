using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dmioks.Common.Utils
{
    public abstract class ObjectPool<T>
    {
        protected readonly List<T>      m_pool     = new List<T>();
        protected readonly HashSet<T>   m_hs       = new HashSet<T>();
        protected readonly Object       m_oLocker  = new Object();

        protected int m_iCreatedCount  = 0;
        protected int m_iExecutedCount = 0;

        protected abstract T CreateNew();

        protected virtual void OnPut(T obj)
        {

        }

        public int Count
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_pool.Count;
                }
            }
        }

        public int CreatedCount
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_iCreatedCount;
                }
            }
        }

        public int ExecutedCount
        {
            get
            {
                lock (m_oLocker)
                {
                    return m_iCreatedCount;
                }
            }
        }

        protected T CreateNewImp()
        {
            T obj = this.CreateNew();
            m_iCreatedCount++;
            return obj;
        }

        public void AddToPool(int iCount)
        {
            lock (m_oLocker)
            {
                for (int i = 0; i < iCount; i++)
                {
                    T obj = CreateNewImp();
                    this.OnPut(obj);

                    m_hs.Add(obj);
                    m_pool.Add(obj);

                    Debug.Assert(m_hs.Count == m_pool.Count, $"AddToPool() ERROR: Inconsistance in ObjectPool<{typeof(T)}>");
                }
            }
        }

        protected T GetIfAny()
        {
            lock (m_oLocker)
            {
                int iSize = m_pool.Count;

                if (0 < iSize)
                {
                    int iLast = iSize - 1;

                    T obj = m_pool[iLast];
                    m_pool.RemoveAt(iLast);
                    m_hs.Remove(obj);

                    Debug.Assert(m_hs.Count == m_pool.Count, $"GetIfAny() ERROR: Inconsistance in ObjectPool<{typeof(T)}>");

                    return obj;
                }

                Debug.Assert(m_hs.Count == 0);
            }

            return default(T);
        }

        public T Get()
        {
            lock (m_oLocker)
            {
                int iSize = m_pool.Count;

                if (iSize == 0)
                {
                    return CreateNewImp();
                }

                int iLast = iSize - 1;

                T obj = m_pool[iLast];
                m_pool.RemoveAt(iLast);
                m_hs.Remove(obj);

                Debug.Assert(m_hs.Count == m_pool.Count, $"Get() ERROR: Inconsistance in ObjectPool<{typeof(T)}>");

                return obj;
            }
        }

        public void Put(T obj)
        {
            lock (m_oLocker)
            {
                this.OnPut(obj);

                m_pool.Add(obj);
                m_hs.Add(obj);

                Debug.Assert(m_hs.Count == m_pool.Count, $"Put() ERROR: Inconsistance in ObjectPool<{typeof(T).Name}> for {obj}");

                m_iExecutedCount ++;
            }
        }

        public override string ToString()
        {
            int iCreated = 0;
            int iPool = 0;
            int iExecuted = 0;

            lock (m_oLocker)
            {
                iCreated = m_iCreatedCount;
                iPool = m_pool.Count;
                iExecuted = m_iExecutedCount;
            }

            return string.Format("{0}{{All/Pool/Executing/Executed={1}/{2}/{3}/{4}}}", this.GetType().Name, iCreated, iPool, iCreated - iPool, iExecuted);
        }

    }
}
