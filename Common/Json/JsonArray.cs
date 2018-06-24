using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;

namespace Dmioks.Common.Json
{
    public abstract class JsonArray : JsonEntity
    {
        public abstract int Count { get; }
        public abstract void Add (object obj);

        public abstract JsonEntity CreateObject();
    }

    public abstract class JsonArray<T> : JsonArray, IEnumerable<T>
    {
        public readonly List<T> Collection;
        public readonly Type GenericType;

        public JsonArray(IEnumerable<T> collection) : base()
        {
            this.GenericType = typeof(T);
            this.Collection = new List<T>(collection);
        }

        public JsonArray() : this(new List<T>())
        {
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Collection.GetEnumerator();
        }

        public override int Count
        {
            get { return this.Collection.Count; }
        }

        public T this[int iIndex]
        {
            get { return this.Collection[iIndex]; }
            set { this.Collection[iIndex] = value; }
        }

        public override void Add(object obj)
        {
            try
            {
                this.Collection.Add((T)obj);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Add (T obj)
        {
            this.Collection.Add(obj);
        }

        public override void SerializeImp (IWrite bw, int iFormat, int iDepth)
        {
            int iCount = this.Collection.Count;
            int iLast = iCount - 1;

            bw.WriteByte((byte)'[');

            for (int i = 0; i < iCount; i++)
            {
                if (0 < iFormat)
                {
                    FormatSpace(bw, iFormat, iDepth + 1);
                }

                Object obj = this.Collection[i];

                SerializeType(bw, obj, delegate(JsonEntity je)
                {
                    je.SerializeImp(bw, iFormat, iDepth + 1);
                });

                if (i < iLast)
                {
                    bw.WriteByte((byte)',');
                }
            }

            if (0 < iCount && 0 < iFormat)
            {
                FormatSpace(bw, iFormat, iDepth);
            }

            bw.WriteByte((byte)']');
        }

        public override void Serialize(IWrite bw, int iFormat)
        {
            this.SerializeImp(bw, iFormat, 0);
        }
    }

    public class JsonObjectArray : JsonArray<JsonObject>
    {
        public JsonObjectArray() : base()
        {
        }

        public JsonObjectArray(IEnumerable<JsonObject> collection) : base(collection)
        {
        }

        public override JsonEntity CreateObject()
        {
            return new JsonObject();
        }
    }

    public class JsonSimpleArray : JsonArray<object>
    {
        public JsonSimpleArray() : base()
        {
        }

        public JsonSimpleArray(IEnumerable<JsonObject> collection) : base(collection)
        {
        }

        public override JsonEntity CreateObject()
        {
            return new JsonObject();
        }

        public static JsonSimpleArray CastFrom<T>(T[] arrSource)
        {
            Debug.Assert(arrSource != null);

            JsonSimpleArray ja = new JsonSimpleArray();

            for (int i = 0; i < arrSource.Length; i++)
            {
                ja.Collection[i] = (object) arrSource[i];
            }

            return ja;
        }

        public delegate T DelegateCast<T>(object obj);

        public T[] CastTo<T>(DelegateCast<T> dc) 
        {
            Debug.Assert(dc != null);

            T[] arr = new T[this.Count];

            for (int i = 0; i < this.Collection.Count; i++)
            {
                object obj = this.Collection[i];
                arr[i] = dc(obj);
            }

            return arr;
        }
    }
}
