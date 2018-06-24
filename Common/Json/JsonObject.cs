using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Server;
using Dmioks.Common.Collections;

namespace Dmioks.Common.Json
{
    public class JsonDictionary : SyncDictionary<string, object>
    {
        public JsonDictionary() : base()
        {

        }

        public JsonDictionary(Dictionary<string, object> di) : base(di)
        {

        }
    }

    public class JsonObject : JsonEntity
    {
        public const string KEY_ID = "Id";

        private readonly JsonDictionary Items;

        public JsonObject(JsonDictionary diItems) : base()
        {
            this.Items = diItems;
        }

        public JsonObject() : this(new JsonDictionary())
        {
        }

      
      

        public object TryGet(string sName, object objDefault)
        {
            if (this.Items.TryGetValue(sName, out object objValue))
            {
                return objValue;
            }

            return objDefault;
        }

        public T TryGet<T>(string sName)
        {
            if (this.Items.TryGetValue(sName, out object objValue))
            {
                return (T)objValue;
            }

            return default(T);
        }

        public T TryGet<T>(string sName, T objDefaultValue)
        {
            if (this.Items.TryGetValue(sName, out object objValue))
            {
                return (T)objValue;
            }

            return objDefaultValue;
        }

        public JsonArray<T> GetJsonArray<T>(string sName)
        {
            object objJsonArray = this.TryGet(sName, null);
            return objJsonArray as JsonArray<T>;
        }

        public JsonSimpleArray GetJsonSimpleArray(string sName)
        {
            object objJsonArray = this.TryGet(sName, null);
            return objJsonArray as JsonSimpleArray;
        }

        public void AddJsonArray<T>(string sName, JsonArray<T> ja)
        {
            Debug.Assert(!this.Items.ContainsKey(sName), $"AddJsonArray<{typeof(T).Name}>('{sName}') ERROR. The key already exists.");
            this.Items.Add(sName, ja);
        }

        public void SetJsonArray<T>(string sName, JsonArray<T> ja)
        {
            this.Items[sName] = ja;
        } 
        public void SetJsonSimpleArray(string sName, JsonSimpleArray ja)
        {
            this.Items[sName] = ja;
        }

        public T Get<T>(string sName)
        {
            var item = this.Items[sName];
            if (item == null)
                return default(T);
            return (T)item;
        }

        public object GetObject(string sName)
        {
            return this.Items[sName];
        }

        public T GetObject<T>(string sName) where T : JsonObject
        {
            return this.Items[sName] as T;
        }

        public void SetObject<T>(string sName, T obj) where T : JsonObject
        {
            this.Items[sName] = obj;
        }

        public long GetLong(string sName)
        {
            if (this.Items.TryGetValue(sName, out object obj) && obj != null)
            {
                return (long) obj;
            }

            return 0L;
        }

        protected static readonly Type INT_TYPE = typeof(int);
        protected static readonly Type LONG_TYPE = typeof(long);

        public int GetInt(string sName)
        {
            if (this.Items.TryGetValue(sName, out object obj) && obj != null)
            {
                Type type = obj.GetType();

                if (type.Equals(INT_TYPE))
                {
                    return (int)obj;
                }

                if (type.Equals(LONG_TYPE))
                {
                    return (int)(long)obj;
                }

                Debug.Assert(false);
            }

            return 0;
        }

        public string GetString(string sName)
        {
            return (string)this.Items[sName];
        }

        public bool GetBool(string sName)
        {
            return (bool)this.Items[sName];
        }

        public void Set<T>(string sName, T objValue)
        {
            this.Items[sName] = objValue;
        }

        public void MergeFrom<T>(T jo) where T : JsonObject
        {
            Debug.Assert(jo != null);
            Dictionary<string, object> diCloned = jo.Items.Clone();

            foreach (KeyValuePair<string, object> kvp in diCloned)
            {
                try
                {
                    this.Items[kvp.Key] = kvp.Value;
                }
                catch (Exception excp)
                {
                    throw new Exception($"MergeFrom() ERROR for '{kvp.Key}' value {kvp.Value}", excp);
                }
            }
        }

        public override void SerializeImp(IWrite bw, int iFormat, int iDepth)
        {
            int iCount = this.Items.Count;
            int iLastIdx = iCount - 1;

            bw.WriteByte((byte)'{');

            for (int i = 0; i < iCount; i++)
            {
                if (0 < iFormat)
                {
                    FormatSpace(bw, iFormat, iDepth + 1);
                }

                KeyValuePair<string, object> kvp = this.Items.ElementAt(i);
                string sKey = kvp.Key;
                object obj = kvp.Value;

                bw.WriteByte((byte)'"');

                bw.WriteString(sKey);

                bw.WriteByte((byte)'"');
                bw.WriteByte((byte)':');

                if (0 < iFormat)
                {
                    bw.WriteByte((byte)' ');
                }

                SerializeType(bw, obj, delegate (JsonEntity je)
                {
                    je.SerializeImp(bw, iFormat, iDepth + 1);
                });

                if (i < iLastIdx)
                {
                    bw.WriteByte((byte)',');
                }
            }

            if (0 < iCount && 0 < iFormat)
            {
                FormatSpace(bw, iFormat, iDepth);
            }

            bw.WriteByte((byte)'}');
        }

        public override void Serialize(IWrite bw, int iFormat)
        {
            this.SerializeImp(bw, iFormat, 0);
        }

        public bool TryGetValue(string name, out object objValue)
        {
            return Items.TryGetValue(name, out objValue);
        }
    }
}
