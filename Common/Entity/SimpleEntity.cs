using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Cache;
using Dmioks.Common.Collections;
using Dmioks.Common.Json;
using Dmioks.Common.Utils;
using NLog.Targets;

namespace Dmioks.Common.Entity
{
    public enum eItemState
    {
        Unchanged = 0,
        New = 1,
        Changed = 2,
    }

    public class SimpleEntity : ICollectionItem
    {
        public readonly PropertyDictionary<object> Items;

        public const long VERSION_1 = 1;

        public static readonly PropertyKey<DateTime>    PK_CREATED_UTC          = new PropertyKey<DateTime>(92, "CreatedUtc", PropertyTypes.DATE_TIME);
        public static readonly PropertyKey<DateTime>    PK_LAST_MODIFIED_UTC    = new PropertyKey<DateTime>(94, "LastModifiedUtc", PropertyTypes.DATE_TIME);
        public static readonly PropertyKey<long>        PK_VERSION              = new PropertyKey<long>(98, "Version", PropertyTypes.INT_64);

        protected bool m_bToBeDeleted = false;

        public readonly ObjectType ObjectType;
        protected eItemState m_state = eItemState.Unchanged;

        // DK - Main Constructor
        public SimpleEntity(ObjectType ot, PropertyDictionary<object> diItems)
        {
            Debug.Assert(diItems != null);

            this.ObjectType = ot;
            this.Items = diItems;

            this.Items[PK_VERSION] = 0;
        }

        public SimpleEntity() : this (null, new PropertyDictionary<object>())
        {
            this.ObjectType = null;
        }

        public SimpleEntity(ObjectType ot) : this(ot, new PropertyDictionary<object>())
        {
        }

        public SimpleEntity(PropertyDictionary<object> di) : this(null, di)
        {
        }

        public eItemState State
        {
            get { return m_state; }
        }

        public bool Changed
        {
            get { return m_state != eItemState.Unchanged; }
        }

        public eSaveOptions SaveOptions
        {
            get
            {
                switch (m_state)
                {
                    case eItemState.Unchanged: return eSaveOptions.None;
                    case eItemState.New: return eSaveOptions.Insert;
                    case eItemState.Changed: return eSaveOptions.Update;
                    default:

                        Debug.Assert(false, $"SetChanged() missed {m_state}");
                        break;
                }

                return eSaveOptions.None;
            }
        }

        public bool ToBeDeleted
        {
            get { return m_bToBeDeleted; }
        }

        public void SetToBeDeleted()
        {
            m_bToBeDeleted = true;
        }

        public IParentCollection ParentCollection { get; set; }

        public eItemState SetChanged()
        {
            long lVersion = PK_VERSION.Value(this);
            Debug.Assert(0 <= lVersion);

            switch (m_state)
            {
                case eItemState.Unchanged:

                    DateTime dtUtc = DateTime.UtcNow;

                    if (lVersion == 0)
                    {
                        m_state = eItemState.New;
                        this.Items[PK_CREATED_UTC] = dtUtc;
                    }
                    else
                    {
                        m_state = eItemState.Changed;
                    }

                    this.Items[PK_LAST_MODIFIED_UTC] = dtUtc;

                    lVersion ++;
                    this.Items[PK_VERSION] = lVersion;

                    if (this.ParentCollection != null)
                    {
                        this.ParentCollection.SetChanged(this);
                    }

                    break;

                case eItemState.New:

                    Debug.Assert(VERSION_1 == lVersion);
                    break;

                case eItemState.Changed:

                    Debug.Assert(VERSION_1 < lVersion);
                    break;

                default:

                    Debug.Assert(false, $"SetChanged() missed {m_state}");
                    break;
            }

            return m_state;
        }

        /*
        public void UpdateLastModified(DateTime dtUtc)
        {
            switch (m_state)
            {
                case eEntityState.Unchanged:

                    // DK - Does nothing here
                    break;

                case eEntityState.New:

                    PK_CREATED_UTC.Set(this, dtUtc);
                    PK_LAST_MODIFIED_UTC.Set(this, dtUtc);
                    Debug.Assert(PK_VERSION.Value(this) == VERSION_1);

                    break;

                case eEntityState.Changed:

                    PK_LAST_MODIFIED_UTC.Set(this, dtUtc);
                    break;

                default:

                    Debug.Assert(false, $"UpdateLastModified() missed {m_state}");
                    break;
            }
        }
        */

        public virtual void ResetChanged()
        {
            m_state = eItemState.Unchanged;
        }

        public DateTime CreatedUtc
        {
            get { return PK_CREATED_UTC.Value(this); }
        }

        public DateTime LastModifiedUtc
        {
            get { return PK_LAST_MODIFIED_UTC.Value(this); }
        }

        public long Version
        {
            get { return PK_VERSION.Value(this); }
        }

        public int Count
        {
            get { return this.Items.Count; }
        }

        /*

        public object TryGet(string sName, object objDefault)
        {
            if (this.Items.TryGetValue(sName, out object objValue))
            {
                return objValue;
            }

            return objDefault;
        }

        public T Get<T>(PropertyKey pk)
        {
            return (T)this.Items[pk];
        }

        public T Get<T>(string sName)
        {
            return (T)this.Items[sName];
        }

        public object GetObject(PropertyKey pk)
        {
            return this.Items[pk];
        }

        public object GetObject(string sName)
        {
            return this.Items[sName];
        }

        public T GetObject<T>(PropertyKey pk) where T : SimpleEntity
        {
            return this.Items[pk] as T;
        }

        public T GetObject<T>(string sName) where T : SimpleEntity
        {
            return this.Items[sName] as T;
        }

        public void SetObject<T>(PropertyKey pk, T obj) where T : SimpleEntity
        {
            this.Items[pk] = obj;
        }

        public long GetLong(PropertyKey pk)
        {
            return Convert.ToInt64(this.Items[pk]);
        }

        public long GetLong(string sName)
        {
            return Convert.ToInt64(this.Items[sName]);
        }

        public int GetInt(PropertyKey pk)
        {
            return Convert.ToInt32(this.Items[pk]);
        }

        public int GetInt(string sName)
        {
            return Convert.ToInt32(this.Items[sName]);
        }

        public string GetString(PropertyKey pk)
        {
            return (string)this.Items[pk];
        }

        public string GetString(string sName)
        {
            return (string)this.Items[sName];
        }

        public bool GetBool(PropertyKey pk)
        {
            return Convert.ToBoolean(this.Items[pk]);
        }

        public bool GetBool(string sName)
        {
            return Convert.ToBoolean(this.Items[sName]);
        }

        public DateTime GetDateTime(PropertyKey pk)
        {
            return Convert.ToDateTime(this.Items[pk]);
        }

        public DateTime GetDateTime(string sName)
        {
            return Convert.ToDateTime(this.Items[sName]);
        }

        public byte[] GetBinary(PropertyKey pk)
        {
            byte[] arrBytes = (byte[])this.Items[pk];
            return arrBytes;
        }

        public byte[] GetBinary(string sName)
        {
            byte[] arrBytes = (byte[])this.Items[sName];
            return arrBytes;
        }

        public void Set<T>(PropertyKey pk, T objValue)
        {
            this.Items[pk] = objValue;
        }
        */

        protected static void SetJsonValue<T>(JsonObject jo, PropertyKey pk, object objValue) where T : JsonObject
        {
            if (objValue == null)
            {
                jo.Set<JsonObject>(pk.Name, null);
            }
            else
            {
                switch (pk.PropertyType.EType)
                {
                    case ePropertyType.DateTime:
                    case ePropertyType.DateTimeNullable:

                        DateTime dtValue = (DateTime)objValue;
                        jo.Set(pk.Name, dtValue);

                        break;

                    case ePropertyType.Object:

                        SimpleEntity seObject = objValue as SimpleEntity;
                        Debug.Assert(seObject != null);

                        T joChild = seObject.ToJsonObject<T>();
                        jo.Set<T>(pk.Name, joChild);
                        break;

                    default:

                        jo.Set(pk.Name, objValue);
                        break;
                }
            }
        }

        public virtual void MergeFromDto(JsonObject jo)
        {

        }

        public virtual void MergeFromDto(JsonObject jo, PropertyKey[] arrKeys)
        {
            foreach (PropertyKey pk in arrKeys)
            {
                pk.TryFromDto(this, jo);
            }
        }

        public virtual T ToJsonObject<T>() where T : JsonObject
        {
            return (T)this.ToJsonObject(new JsonObject());
        }

        public virtual T ToJsonObject<T>(T jo) where T : JsonObject
        {
            Debug.Assert(jo != null);
            PropertyDictionary<object> diCloned = this.Items.Clone();

            foreach (KeyValuePair<PropertyKey, object> kvp in diCloned)
            {
                PropertyKey pk = kvp.Key;
                object objValue = kvp.Value;

                try
                {
                    SetJsonValue<T>(jo, pk, objValue);
                }
                catch (Exception excp)
                {
                    throw new Exception($"ToJsonObject<{typeof(T).Name}>() ERROR for {pk} value {objValue}", excp);
                }
            }

            return jo;
        }

        public virtual JsonObject ToJsonObject(PropertyKey[] arrKeys)
        {
            return this.ToJsonObject(new JsonObject(), arrKeys);
        }

        public virtual T ToJsonObject<T>(T jo, PropertyKey[] arrKeys) where T : JsonObject
        {
            Debug.Assert(jo != null);
            Debug.Assert(arrKeys != null && 0 < arrKeys.Length);

            PropertyDictionary<object> diCloned = this.Items.Clone();

            foreach (PropertyKey pk in arrKeys)
            {
                object objValue = diCloned[pk];

                SetJsonValue<T>(jo, pk, objValue);
            }

            return jo;
        }

        public static SimpleEntity FromJsonObject(JsonObject jo, ObjectType ot, object objParam = null)
        {
            Debug.Assert(jo != null);
            Debug.Assert(ot != null);

            SimpleEntity dbObj = ot.CreateNewObject(objParam) as SimpleEntity;
            Debug.Assert(dbObj != null);

            foreach (PropertyKey pk in ot.PropertyKeys)
            {
                object objValue = jo.GetObject(pk.Name);
                dbObj.Items[pk] = objValue;
            }

            return dbObj;
        }

        public bool HasKeys(PropertyKey[] arrPropertyKeys)
        {
            foreach (PropertyKey pk in arrPropertyKeys)
            {
                if (!this.Items.ContainsKey(pk))
                {
                    return false;
                }
            }

            return true;
        }

        public static List<T> CastArrayToList<T>(SimpleEntity[] arrSource) where T : SimpleEntity
        {
            if (arrSource != null)
            {
                List<T> lResult = new List<T>();

                for (int i = 0; i < arrSource.Length; i++)
                {
                    T obj = arrSource[i] as T;
                    lResult.Add(obj);
                }

                return lResult;
            }

            return null;
        }

        public static List<T> CastArrayToList<T>(SimpleEntity seArrayOwner, PropertyKey<SimpleEntity[]> pk) where T : SimpleEntity
        {
            Debug.Assert(seArrayOwner != null);

            SimpleEntity[] arrSource = pk.Value(seArrayOwner, null);

            return CastArrayToList<T>(arrSource);
        }

        public static SimpleEntity[] ToSimpleEntityArray<T>(T[] arrSource) where T : SimpleEntity
        {
            Debug.Assert(arrSource != null);

            SimpleEntity[] arrTarget = new SimpleEntity[arrSource.Length];

            for (int i = 0; i < arrSource.Length; i++)
            {
                arrTarget[i] = arrSource[i];
            }

            return arrTarget;
        }

        public override string ToString()
        {
            List<string> lItems = new List<string>();

            foreach (PropertyKey pk in this.Items.GetSortedKeys())
            {
                object objValue = this.Items[pk];
                bool bUsingCommas = pk.PropertyType.EType == ePropertyType.String;
                string sItem = bUsingCommas ? $"{pk.Name}='{objValue}'" : $"{pk.Name}={objValue}";
                lItems.Add(sItem);
            }

            string sItems = string.Join(", ", lItems);

            return $"{this.GetType().Name}{{Count={lItems.Count}, State={m_state}, {sItems}}}";
        }
    }
}
