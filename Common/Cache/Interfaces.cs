using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;

namespace Dmioks.Common.Cache
{
    public class CacheConsts
    {
        public const int DEFAULT_TIMEOUT_MILLES = 500;
    }

    public enum eSaveOptions
    {
        None = 0,
        Insert = 1,
        Update = 2,
        InsertIfNotExist = 3,
        InsertIfNotExistOrUpdate = 4,
    }

    /*
    public interface IParentCache<P>
    {
        P Instance { get; }
    }

    public interface IParentRelatedCache<P,K> : IParentCache<P>
    {
        K ParentId { get; }
    }
    */

    public interface IParentCollection
    {
        void SetChanged(object obj);
    }

    public interface ICollectionItem
    {
        IParentCollection ParentCollection { get; set; }
    }

    public interface ICacheItem<K> : ICollectionItem
    {
        K CacheKey { get; }
        eItemState State { get; }
        void ResetChanged();
        void SetToBeDeleted();
    }

    public interface ICacheItemEx<K> : ICacheItem<K>
    {
        void EnsureRelatedObjects();
        bool ToBeDeleted { get; }
        long Version { get; }
    }

    public interface IRelatedCacheItem<K> : ICacheItem<K>
    {
        void SetRelation(int iParentId, int iRelatedId);
    }

    public interface IChangedCache<K,V> where V : ICacheItem<K>
    {
        // New Items
        List<V> NewItems { get; }
        // Changed items
        HashSet<V> ChangedItems { get; }
        // Items to be deleted
        HashSet<V> ItemsToDelete { get; }
        // Items IDs to be deleted
        K[] GetIdsToDelete();
    }
}
