using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dmioks.Common.Collections
{
    public class SyncHashSet<T> : ISet<T>
    {
        protected readonly ReaderWriterLock m_rwl = new ReaderWriterLock();
        protected readonly HashSet<T> m_hs = new HashSet<T>();

        /*
        m_rwl.AcquireReaderLock(Timeout.Infinite);

        try
        {
        }
        finally
        {
            m_rwl.ReleaseReaderLock();
        }

        m_rwl.AcquireWriterLock(Timeout.Infinite);

        try
        {
        }
        finally
        {
            m_rwl.ReleaseWriterLock();
        }
        */


        public int Count
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_hs.Count;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Add(T item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return m_hs.Add(item);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public void Clear()
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_hs.Clear();
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public bool Contains(T item)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_hs.Contains(item);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_hs.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_hs.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return m_hs.Remove(item);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_hs.Add(item);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }
    }
}
