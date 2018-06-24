using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Collections
{
    public class SyncList<T> : IList<T>
    {
        protected List<T> m_list = new List<T>();
        protected readonly ReaderWriterLock m_rwl = new ReaderWriterLock();

        public SyncList(IEnumerable<T> collection)
        {
            m_list = new List<T>(collection);
        }

        public SyncList(List<T> toList)
        {
            m_list = new List<T>(toList);
        }

        public SyncList()
        {
        }

        public SyncList<T> Clone()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return new SyncList<T>(new List<T>(m_list));
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual int IndexOf(T item)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_list.IndexOf(item);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual void Insert(int iIndex, T item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_list.Insert(iIndex, item);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual void RemoveAt(int iIndex)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_list.RemoveAt(iIndex);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual T this[int iIndex]
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_list[iIndex];
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
            set
            {
                m_rwl.AcquireWriterLock(Timeout.Infinite);

                try
                {
                    m_list[iIndex] = value;
                }
                finally
                {
                    m_rwl.ReleaseWriterLock();
                }
            }
        }

        public virtual void Add(T item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_list.Add(item);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual void Clear()
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_list.Clear();
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual bool Contains(T item)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_list.Contains(item);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual void CopyTo(T[] array, int iIndex)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                m_list.CopyTo(array, iIndex);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual int Count
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_list.Count;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
        }

        public virtual bool Remove(T item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return m_list.Remove(item);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_list.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_list.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_list.Sort(comparer);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_list.Sort(comparison);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}> {{Count={2}}}", this.GetType().Name, typeof(T).Name, this.m_list.Count);
        }
    }
}
