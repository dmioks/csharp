using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Entity;

namespace Dmioks.Common.Collections
{
    public class PropertyDictionary<TValue> : IDictionary<PropertyKey, TValue> 
    {
        protected readonly ReaderWriterLock m_rwl = new ReaderWriterLock();

        protected readonly Dictionary<PropertyKey, TValue>   m_diKeyToObj = new Dictionary<PropertyKey, TValue>();
        protected readonly Dictionary<string, PropertyKey>   m_diStrToKey = new Dictionary<string, PropertyKey>();
        protected readonly Dictionary<int, PropertyKey>      m_diIntToKey = new Dictionary<int, PropertyKey>();

        protected int m_iNext = 0;

        protected void SetImp(PropertyKey pk, TValue objValue)
        {
            m_diStrToKey[pk.Name] = pk;
            m_diKeyToObj[pk] = objValue;
            m_diIntToKey[pk.BinName] = pk;

            Debug.Assert(m_diStrToKey.Count == m_diKeyToObj.Count);
            Debug.Assert(m_diIntToKey.Count == m_diKeyToObj.Count);
        }

        public PropertyDictionary<TValue> Clone()
        {
            PropertyDictionary<TValue> diNew = new PropertyDictionary<TValue>();

            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                foreach (KeyValuePair<PropertyKey, TValue> kvp in m_diKeyToObj)
                {
                    diNew.AddImp(kvp.Key, kvp.Value);
                }
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }

            return diNew;
        }

        public TValue this[PropertyKey pk]
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_diKeyToObj[pk];
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
                    SetImp(pk, value);
                }
                finally
                {
                    m_rwl.ReleaseWriterLock();
                }
            }
        }

        public PropertyKey SetByName(string sName, TValue objValue)
        {
            Debug.Assert(!string.IsNullOrEmpty(sName));

            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                // Check if Property Key with such name already exists in the collections
                if (m_diStrToKey.TryGetValue(sName, out PropertyKey pk))
                {
#if DEBUG
                    // Assert if existing type correct
                    {
                        if (PropertyTypes.GetEType(objValue.GetType(), out ePropertyType ept))
                        {
                            Debug.Assert(pk.PropertyType.EType == ept);
                        }
                        
                        Debug.Assert(false);
                    }
#endif

                    SetImp(pk, objValue);

                    return pk;
                }

                if (objValue == null)
                {
                    throw new ArgumentException($"SetByName('{sName}', null) ERROR. Cannot set new property with null value because value type is unknown.");
                }

                // No, it is new name, so let's find next available bin name
                while (m_diIntToKey.ContainsKey(++ m_iNext))
                {
                }

                {
                    // Let's get Property Type
                    if (PropertyTypes.GetEType(objValue.GetType(), out ePropertyType ept))
                    {
                        PropertyType pt = PropertyTypes.GetPropertyType(ept);

                        PropertyKey pkNew = pt.CreatePropertyKey(m_iNext, sName);
                        SetImp(pkNew, objValue);
                    }
                    else
                    {
                        throw new InvalidEnumArgumentException($"SetByName('{sName}', {objValue.GetType()}) ERROR. Value type is not supported.");
                    }
                }

                return pk;
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public TValue this[string sName]
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (m_diStrToKey.TryGetValue(sName, out PropertyKey pk))
                    {
                        return m_diKeyToObj[pk];
                    }

                    return default(TValue);
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
            set { this.SetByName(sName, value); }
        }

        public PropertyKey SetByInt(int iBinName, TValue objValue)
        {
            Debug.Assert(0 < iBinName);

            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                // Check if Property Key with such bin name already exists in the collections
                if (m_diIntToKey.TryGetValue(iBinName, out PropertyKey pk))
                {
#if DEBUG
                    // Assert if existing type correct
                    {
                        if (PropertyTypes.GetEType(objValue.GetType(), out ePropertyType ept))
                        {
                            Debug.Assert(pk.PropertyType.EType == ept);
                        }

                        Debug.Assert(false);
                    }
#endif

                    SetImp(pk, objValue);

                    return pk;
                }

                if (objValue == null)
                {
                    throw new ArgumentException($"SetByInt('{iBinName}', null) ERROR. Cannot set new property with null value because value type is unknown.");
                }

                {
                    // Let's get Property Type
                    // Let's get Property Type
                    if (PropertyTypes.GetEType(objValue.GetType(), out ePropertyType ept))
                    {
                        PropertyType pt = PropertyTypes.GetPropertyType(ept);
                        PropertyKey pkNew = pt.CreatePropertyKey(iBinName, iBinName.ToString());
                        SetImp(pkNew, objValue);
                    }
                    else
                    {
                        throw new InvalidEnumArgumentException($"SetByInt('{iBinName}', {objValue.GetType()}) ERROR. Value type is not supported.");
                    }

                    // Let's create new PropertyKey
                }

                return pk;
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public TValue this[int iBinName]
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    if (m_diIntToKey.TryGetValue(iBinName, out PropertyKey pk))
                    {
                        return m_diKeyToObj[pk];
                    }

                    return default(TValue);
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
            set { this.SetByInt(iBinName, value); }
        }



        public PropertyKey GetKeyByName(string sName)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_diStrToKey[sName];
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public ICollection<PropertyKey> Keys
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_diKeyToObj.Keys;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    return m_diKeyToObj.Values;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public int Count
        {
            get
            {
                m_rwl.AcquireReaderLock(Timeout.Infinite);

                try
                {
                    Debug.Assert(m_diStrToKey.Count == m_diKeyToObj.Count);
                    return m_diKeyToObj.Count;
                }
                finally
                {
                    m_rwl.ReleaseReaderLock();
                }
            }
        }

        public List<PropertyKey> GetSortedKeys ()
        {
            List<PropertyKey> lResult = null;

            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                lResult = m_diKeyToObj.Keys.ToList();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }

            lResult.Sort(delegate (PropertyKey pk1, PropertyKey pk2) { return pk1.BinName.CompareTo(pk2.BinName); });

            return lResult;
        }

        public List<string> GetSortedNames ()
        {
            List<string> lSortedNames = new List<string>();
            List<PropertyKey> lSortedKeys = this.GetSortedKeys();

            foreach (PropertyKey pk in lSortedKeys)
            {
                lSortedNames.Add(pk.Name);
            }

            return lSortedNames;
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        protected void AddImp(PropertyKey pk, TValue value)
        {
            Debug.Assert(!m_diStrToKey.ContainsKey(pk.Name));
            Debug.Assert(!m_diKeyToObj.ContainsKey(pk));
            Debug.Assert(!m_diIntToKey.ContainsKey(pk.BinName));

            m_diStrToKey.Add(pk.Name, pk);
            m_diKeyToObj.Add(pk, value);
            m_diIntToKey.Add(pk.BinName, pk);

            Debug.Assert(m_diStrToKey.Count == m_diKeyToObj.Count);
            Debug.Assert(m_diIntToKey.Count == m_diKeyToObj.Count);
        }

        public virtual void Add(PropertyKey pk, TValue value)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                AddImp(pk, value);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public void Add(KeyValuePair<PropertyKey, TValue> item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                AddImp(item.Key, item.Value);
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
                m_diStrToKey.Clear();
                m_diKeyToObj.Clear();
                m_diIntToKey.Clear();
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public bool Contains(KeyValuePair<PropertyKey, TValue> item)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                if (m_diKeyToObj.TryGetValue(item.Key, out TValue objValue))
                {
                    return objValue != null ? objValue.Equals(item.Value) : item.Value == null;
                }

                return false;
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public bool ContainsKey(PropertyKey pk)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_diKeyToObj.ContainsKey(pk);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public bool ContainsName(string sName)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_diStrToKey.ContainsKey(sName);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public void CopyTo(KeyValuePair<PropertyKey, TValue>[] array, int arrayIndex)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public IEnumerator<KeyValuePair<PropertyKey, TValue>> GetEnumerator()
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_diKeyToObj.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        protected bool RemoveImp(PropertyKey pk)
        {
            Debug.Assert(pk != null);

            bool bResult = m_diKeyToObj.Remove(pk);
            m_diStrToKey.Remove(pk.Name);
            m_diIntToKey.Remove(pk.BinName);

            Debug.Assert(m_diStrToKey.Count == m_diKeyToObj.Count);
            Debug.Assert(m_diIntToKey.Count == m_diKeyToObj.Count);

            return bResult;
        }

        public bool Remove(PropertyKey pk)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return RemoveImp(pk);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public bool Remove(KeyValuePair<PropertyKey, TValue> item)
        {
            m_rwl.AcquireWriterLock(Timeout.Infinite);

            try
            {
                return RemoveImp(item.Key);
            }
            finally
            {
                m_rwl.ReleaseWriterLock();
            }
        }

        public bool TryGetValue(PropertyKey pk, out TValue objValue)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                return m_diKeyToObj.TryGetValue(pk, out objValue);
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }

        public bool TryGetValue(string sName, out TValue objValue)
        {
            m_rwl.AcquireReaderLock(Timeout.Infinite);

            try
            {
                PropertyKey pk = m_diStrToKey[sName];

                if (pk != null)
                {
                    return m_diKeyToObj.TryGetValue(pk, out objValue);
                }

                objValue = default(TValue);

                return false;
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
                return m_diKeyToObj.GetEnumerator();
            }
            finally
            {
                m_rwl.ReleaseReaderLock();
            }
        }
    }
}
