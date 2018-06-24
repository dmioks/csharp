using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dmioks.Common.Collections
{
    public class SyncDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        protected readonly Dictionary<TKey, TValue> m_di;
        protected readonly ReaderWriterLock m_rwl = new ReaderWriterLock();

        public SyncDictionary(Dictionary<TKey, TValue> diItems)
        {
            m_di = diItems;
        }

        public SyncDictionary() : this(new Dictionary<TKey, TValue>())
        {

        }

        public virtual Dictionary<TKey, TValue> Clone()
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                Dictionary<TKey, TValue> diNew = new Dictionary<TKey, TValue>();

                foreach (KeyValuePair<TKey, TValue> kvp in m_di)
                {
                    diNew.Add(kvp.Key, kvp.Value);
                }

                return diNew;
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual void Add(TKey key, TValue value)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_di.Add(key, value);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual bool ContainsKey(TKey key)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_di.ContainsKey(key);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual ICollection<TKey> Keys
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_di.Keys;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public virtual List<KeyValuePair<TKey, TValue>> PairsToList()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_di.ToList();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual bool Remove(TKey key)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return m_di.Remove(key);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_di.TryGetValue(key, out value);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual ICollection<TValue> Values
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_di.Values;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public virtual List<TValue> GetValueList()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return new List<TValue>(m_di.Values);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual TValue this[TKey key]
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_di[key];
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
                    m_di[key] = value;
                }
                finally
                {
                    m_rwl.ReleaseWriterLock();
                }
            }
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                m_di.Add(item.Key, item.Value);
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
                m_di.Clear();
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_di.Contains(item);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int iIndex)
        {
            throw new NotImplementedException();
        }

        public virtual int Count
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_di.Count;
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

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return m_di.Remove(item.Key);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_di.GetEnumerator();
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
                return m_di.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public override string ToString()
        {
            return string.Format("{0}<{1}, {2}> {{Count={3}}}", this.GetType().Name, typeof(TKey).Name, typeof(TValue).Name, this.Count);
        }

        public TValue GetValueWithLock(TKey iUserAccountId, Func<TValue, TValue> func)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);
            try
            {
                m_di.TryGetValue(iUserAccountId, out TValue value);
                return func(value);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }

        }
    }
}
