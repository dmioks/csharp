using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using Dmioks.Common.Collections;
using Dmioks.Common.Entity;
using Dmioks.Common.Server;

namespace Dmioks.Common.Cache
{
    public class ChangedCache<K, V> : IChangedCache<K, V> where V : ICacheItem<K>
    {
        // New Items
        protected readonly List<V> m_lNewItems = new List<V>();
        // Changed items
        protected readonly HashSet<V> m_hsChangedItems = new HashSet<V>();
        // Ids of items to be deleted
        protected readonly HashSet<V> m_hsItemsToDelete = new HashSet<V>();

        protected internal ChangedCache()
        {

        }

        public List<V> NewItems { get { return m_lNewItems; } }
        public HashSet<V> ChangedItems { get { return m_hsChangedItems; } }
        public HashSet<V> ItemsToDelete { get { return m_hsItemsToDelete; } }

        public bool IsChanged
        {
            get { return 0 < m_hsChangedItems.Count || 0 < m_lNewItems.Count || 0 < m_hsItemsToDelete.Count; }
        }

        public K[] GetIdsToDelete()
        {
            V[] arrItems = m_hsItemsToDelete.ToArray();
            K[] arrIds = new K[arrItems.Length];

            for (int i = 0; i < arrItems.Length; i++)
            {
                arrIds[i] = arrItems[i].CacheKey;
            }

            return arrIds;
        }
    }

    public abstract class ItemCache<K, V> : IParentCollection
        where V : ICacheItem<K>
    {
        // Dictionary of items that have not default keys
        protected SyncDictionary<K, V> m_di = new SyncDictionary<K, V>();

        public delegate void DelegateSaveCallback<V>();

        protected readonly ChangedCache<K, V> m_ChangedCache = new ChangedCache<K, V>();

        public int DefaultTimeout;

        public ItemCache(int iDefaultTimeout)
        {
            this.DefaultTimeout = iDefaultTimeout;
        }

        public int Count { get { return m_di.Count; } }

        /*
        protected abstract List<V> RetrieveItems(K[] arrKeys, int iMillesTimeout);
        protected abstract List<V> RetrieveAll(int iMillesTimeout);

        protected virtual void SaveChangesImp(int iMillesTimeout)
        {
            throw new NotImplementedException("Please override the method in derived class");
        }
        */

        public IChangedCache<K, V> ChangedCache { get { return m_ChangedCache; } }

        public bool IsChanged
        {
            get { return m_ChangedCache.IsChanged; }
        }

        public virtual K[] GetAllIds()
        {
            // DK - returns ONLY ids of Dictionary but does not return ids of new items
            return m_di.Keys.ToArray();
        }

        public List<V> GetAll()
        {
            List<V> lAll = new List<V>(m_ChangedCache.NewItems);
            lAll.AddRange(m_di.Values);
            return lAll;
        }

        public List<V> GetItems(K[] arrKeys)
        {
            return GetItems(arrKeys, this.DefaultTimeout);
        }

        public virtual List<V> GetItems(K[] arrKeys, int iMillesTimeout)
        {
            Debug.Assert(arrKeys != null && 0 < arrKeys.Length);

            List<V> lItems = new List<V>();
            //HashSet<K> hsIdsToRequest = new HashSet<K>();

            foreach (K key in arrKeys)
            {
                if (m_di.TryGetValue(key, out V objItem))
                {
                    lItems.Add(objItem);
                }
                //else
                //{
                //    hsIdsToRequest.Add(key);
                //}
            }

            /*
            if (0 < hsIdsToRequest.Count)
            {
                List<V> lRetievedItems = RetrieveItems(hsIdsToRequest.ToArray(), iMillesTimeout);

                foreach (V objRetrievedItem in lRetievedItems)
                {
                    ICacheItemEx<K> ciEx = objRetrievedItem as ICacheItemEx<K>;

                    if (ciEx != null)
                    {
                        ciEx.EnsureRelatedObjects();
                    }

                    this.SetItem(objRetrievedItem);

                    lItems.Add(objRetrievedItem);
                }
            }
            */

            return lItems;
        }

        public void MarkItemToDelete(K key)
        {
            Debug.Assert(key != null);

            if (m_di.TryGetValue(key, out V objValue))
            {
                Debug.Assert(key.Equals(objValue.CacheKey));

                m_di.Remove(key);
                objValue.SetToBeDeleted();
                m_ChangedCache.ItemsToDelete.Add(objValue);
            }
        }

        public void MarkItemsToDelete(K[] arrKeys)
        {
            foreach (K key in arrKeys)
            {
                this.MarkItemToDelete(key);
            }
        }

        public virtual V GetItem(K key)
        {
            if (m_di.TryGetValue(key, out V objItem))
            {
                return objItem;
            }

            /*
            List<V> lRetievedItems = RetrieveItems(new K[] { key }, iMillesTimeout);

            if (lRetievedItems != null && 0 < lRetievedItems.Count)
            {
                Debug.Assert(0 < lRetievedItems.Count);
                V objRetrievedItem = lRetievedItems[0];
                Debug.Assert(!objRetrievedItem.CacheKey.Equals(default(K)));

                this.SetItem(objRetrievedItem);

                return objRetrievedItem;
            }
            */

            return default(V);
        }

        public delegate V DelegateCreateCacheItem();

        public void SetItem(V objItem)
        {
            Debug.Assert(objItem != null && objItem.CacheKey != null);

            if (objItem.State == eItemState.New)
            {
                m_ChangedCache.NewItems.Add(objItem);
            }
            else
            {
                Debug.Assert(!objItem.CacheKey.Equals(default(K)));

                m_di[objItem.CacheKey] = objItem;

                if (objItem.State == eItemState.Changed)
                {
                    m_ChangedCache.ChangedItems.Add(objItem);
                }
            }

            objItem.ParentCollection = this;
        }

        public void SetChanged(object objItem)
        {
            ICacheItem<K> objCacheItem = objItem as ICacheItem<K>;

            if (objCacheItem != null)
            {
                if (objCacheItem.State == eItemState.Changed)
                {
                    m_ChangedCache.ChangedItems.Add((V) objCacheItem);
                }
            }
        }

        public void SetItems(List<V> lItems)
        {
            foreach (V obj in lItems)
            {
                this.SetItem(obj);
            }
        }

        public V EnsureItem(K key, DelegateCreateCacheItem dcsc)
        {
            if (m_di.TryGetValue(key, out V objItem))
            {
                return objItem;
            }

            objItem = dcsc();
            Debug.Assert(objItem != null);

            this.SetItem(objItem);

            return this.GetItem(key);
        }

        /*
        public void SaveChanges()
        {
            this.SaveChanges(this.DefaultTimeout);
        }

        public void SaveChanges(int iMillesTimeout)
        {
            try
            {
                this.SaveChangesImp(iMillesTimeout);

                this.ResetChanged();
            }
            catch (Exception excp)
            {
                throw new Exception($"{this} SaveChanges() ERROR", excp);
            }
        }
        */

        public void ResetChanged()
        {
            // Clear Deleted Ids
            m_ChangedCache.ItemsToDelete.Clear();

            // Move new objects to dictionary
            foreach (V obj in m_ChangedCache.NewItems)
            {
                Debug.Assert(!obj.CacheKey.Equals(default(K)));
                Debug.Assert(!m_di.ContainsKey(obj.CacheKey));

                obj.ResetChanged();

                this.SetItem(obj);
            }

            m_ChangedCache.NewItems.Clear();

            // Changed Items
            foreach (V obj in m_ChangedCache.ChangedItems)
            {
                Debug.Assert(!obj.CacheKey.Equals(default(K)));
                Debug.Assert(m_di.ContainsKey(obj.CacheKey));

                obj.ResetChanged();
            }

            m_ChangedCache.ChangedItems.Clear();
        }

        public override string ToString()
        {
            return $"ItemCache<{typeof(V).Name}>{{All={m_di.Count + m_ChangedCache.NewItems.Count}, New={m_ChangedCache.NewItems.Count}, Changed={m_ChangedCache.ChangedItems.Count}, ToBeDeleted={m_ChangedCache.ItemsToDelete.Count}}}";
        }
    }
}
