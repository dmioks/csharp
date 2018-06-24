using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Collections;
using Dmioks.Common.Entity;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public class Read : IRead
    {
        private const int ARRAY_CAPACITY = 256;
        QueueArray<char> m_queue = new QueueArray<char>(ARRAY_CAPACITY);

        public readonly Stream Stream;

        public Read(Stream stream)
        {
            this.Stream = stream;
        }

        public char PeekChar()
        {
            char ch = BinHelper.DecodeUtf8(this.Stream);

            m_queue.Enqueue(ch);

            return ch;
        }

        public char ReadChar()
        {
            if (0 < m_queue.Count)
            {
                return m_queue.Dequeue();
            }

            return BinHelper.DecodeUtf8(this.Stream);
        }
    }

    public class BinRead : Read, IBinRead, IDisposable
    {
        public const int SERIALIZED_GUID_BYTE_ARRAY_LENGTH = 16;

        public readonly Stream Stream;
        Dictionary<int, ObjectType> m_diReadObjectTypes = new Dictionary<int, ObjectType>();

        public readonly SyncDictionary<int, DelegateCreateNewObject> ClassFactory = new SyncDictionary<int, DelegateCreateNewObject>();
        protected StringBuilder m_sb = StringBuilderPool.Instance.Get();

        public BinRead(Stream stream) : base (stream)
        {
            this.Stream = stream;
        }

        public void AddObjectType(ObjectType ot)
        {
            m_diReadObjectTypes.Add(ot.BinName, ot);
        }

        public byte ReadByte()
        {
            return (byte)this.Stream.ReadByte();
        }

        public uint ReadUshort(out bool bFlag)
        {
            return BinHelper.DeserializeUshort(this, out bFlag);
        }

        public int ReadShortInt()
        {
            return BinHelper.DeserializeShortInt(this);
        }

        public ulong ReadUlong()
        {
            return BinHelper.DeserializeUlong(this);
        }

        public long ReadLong()
        {
            return BinHelper.DeserializeLong(this);
        }

        public string ReadStringBin()
        {
            ulong ulLength = BinHelper.DeserializeUlong(this);

            try
            {
                for (int i = 0; i < (int)ulLength; i++)
                {
                    char ch = this.ReadChar();
                    m_sb.Append(ch);
                }

                return m_sb.ToString();
            }
            finally
            {
                m_sb.Clear();
            }
        }

        public byte[] ReadBinary()
        {
            int iLength = (int) BinHelper.DeserializeUlong(this);

            byte[] arrBin = new byte[iLength];

            int iPos = 0;
            int iRead = this.Stream.Read(arrBin, iPos, iLength - iPos);

            iPos += iRead;

            while (iPos < iLength)
            {
                iRead = this.Stream.Read(arrBin, iPos, iLength - iPos);
                iPos += iRead;
            }

            Debug.Assert(iPos == iLength);

            return arrBin;
        }

        protected PropertyKey ReadPropertyKey(out bool bFlag)
        {
            int iBinName = (int) this.ReadUshort(out bFlag);
            string sName = ReadStringBin();
            eKeyIdentity ki = (eKeyIdentity) (int) this.ReadByte();

            int iPropertyType = (int)this.ReadUshort(out bool bNotUsed);

            if (Enum.IsDefined(typeof(ePropertyType), iPropertyType))
            {
                ePropertyType ept = (ePropertyType) iPropertyType;

                PropertyType pt = PropertyTypes.GetPropertyType(ept);

                PropertyKey pk = pt.CreatePropertyKey(iBinName, sName, ki);

                return pk;
            }

            throw new InvalidDataException($"ReadPropertyKey() ERROR. unknown ePropertyType int value ({iPropertyType})");
        }

        public SimpleEntity ReadObject()
        {
            // Read Object Type
            int iObjectTypeBinName = (int) this.ReadUshort(out bool bFlag);
            ObjectType ot = null;

            if (bFlag)
            {
                // This object type was already received before
                if (m_diReadObjectTypes.TryGetValue(iObjectTypeBinName, out ot))
                {
                    Debug.Assert(ot != null && ot.BinName == iObjectTypeBinName);
                }
                else
                {
                    throw new InvalidDataException($"ReadObject() ERROR. Received uknown object type ({iObjectTypeBinName})");
                }
            }
            else
            {
                // This is new object type
                string sName = this.ReadStringBin();
                int iPropertyKeyCount = (int) this.ReadUshort(out bool bNotUsed);

                PropertyKey[] arrPropertyKeys = new PropertyKey[iPropertyKeyCount];

                for (int i = 0; i < iPropertyKeyCount; i ++)
                {
                    arrPropertyKeys[i] = this.ReadPropertyKey(out bool bNotUsed2);
                }

                this.ClassFactory.TryGetValue(iObjectTypeBinName, out DelegateCreateNewObject dcno);

                ot = new ObjectType(iObjectTypeBinName, sName, dcno ?? ObjectType.DEFAULT_CREATE_OBJECT, arrPropertyKeys);
                ot.EnsurePropertyKeysDictionary();

                if (!m_diReadObjectTypes.ContainsKey(ot.BinName))
                {
                    m_diReadObjectTypes.Add(ot.BinName, ot);
                }
            }

            Debug.Assert(ot != null);

            SimpleEntity se = ot.CreateNewObject(null);
            Debug.Assert(se != null);

            while (true)
            {
                int iPropertyKeyBinName = (int)this.ReadUshort(out bool bIsNullValue);

                if (iPropertyKeyBinName == 0)
                {
                    // End of object detected
                    se.ResetChanged();
                    return se;
                }

                if (ot.IntToKey.TryGetValue(iPropertyKeyBinName, out PropertyKey pk))
                {
                    if (bIsNullValue)
                    {
                        se.Items[pk] = null;
                    }
                    else
                    {
                        object objValue = this.ReadObjectPropertyValue(pk.PropertyType);
                        se.Items[pk] = objValue;
                    }
                }
                else
                {
                    throw new InvalidDataException($"ReadObject() ERROR. Received uknow PropertyKey ({iPropertyKeyBinName}) for {ot}");
                }
            }

            // Unreachebale code
            Debug.Assert(false);

            return null;
        }

        protected decimal ReadDecimal()
        {
            int[] arr = new int[BinWrite.DECIMAL_ARRAY_COUNT];

            arr[0] = (int) this.ReadLong();
            arr[1] = (int) this.ReadLong();
            arr[2] = (int) this.ReadLong();
            arr[3] = (int) this.ReadLong();

            return new decimal(arr);
        }

        protected Guid ReadGuid()
        {
            byte[] arr = new byte[SERIALIZED_GUID_BYTE_ARRAY_LENGTH];

            for (int i = 0; i < SERIALIZED_GUID_BYTE_ARRAY_LENGTH; i++)
            {
                arr[i] = this.ReadByte();
            }

            return new Guid(arr);
        }

        protected delegate T DelegateReadArrayValue<T>();

        protected T[] ReadArray<T>(DelegateReadArrayValue<T> dwav)
        {
            int iLength = (int) BinHelper.DeserializeUlong(this);

            T[] arr = new T[iLength];

            for (int i = 0; i < iLength; i++)
            {
                arr[i] = dwav();
            }

            return arr;
        }

        protected object ReadObjectPropertyValue(PropertyType pt)
        {
            switch (pt.EType)
            {
                case ePropertyType.Int16Array:

                    return this.ReadArray<Int16>(delegate ()
                    {
                        return (Int16) this.ReadLong();
                    });

                case ePropertyType.Int32Array:

                    return this.ReadArray<int>(delegate ()
                    {
                        return (int)this.ReadLong();
                    });

                case ePropertyType.Int64Array:

                    return this.ReadArray<long>(delegate ()
                    {
                        return (long)this.ReadLong();
                    });

                case ePropertyType.DecimalArray:

                    return this.ReadArray<decimal>(delegate ()
                    {
                        return (decimal)this.ReadDecimal();
                    });

                case ePropertyType.BoolArray:

                    return this.ReadArray<bool>(delegate ()
                    {
                        return 0 < this.ReadByte();
                    });

                case ePropertyType.GuidArray:

                    return this.ReadArray<Guid>(delegate ()
                    {
                        return this.ReadGuid();
                    });

                case ePropertyType.DateTimeArray:

                    return this.ReadArray<DateTime>(delegate ()
                    {
                        return new DateTime((long)this.ReadUlong());
                    });

                    break;

                case ePropertyType.StringArray:

                    return this.ReadArray<string>(delegate ()
                    {
                        return this.ReadStringBin();
                    });

                    break;

                case ePropertyType.ObjectArray:

                    return this.ReadArray<SimpleEntity>(delegate ()
                    {
                        return this.ReadObject();
                    });

                    break;

                case ePropertyType.Object:

                    return this.ReadObject();

                case ePropertyType.Int16:
                case ePropertyType.Int16Nullable:

                    return (Int16) this.ReadLong();

                case ePropertyType.Int32:
                case ePropertyType.Int32Nullable:

                    return (int) this.ReadLong();

                case ePropertyType.Int64:
                case ePropertyType.Int64Nullable:

                    return this.ReadLong();

                case ePropertyType.Decimal:
                case ePropertyType.DecimalNullable:

                    return this.ReadDecimal();

                case ePropertyType.Bool:
                case ePropertyType.BoolNullable:

                    return 0 < this.ReadByte();

                case ePropertyType.Guid:
                case ePropertyType.GuidNullable:

                    return this.ReadGuid();

                case ePropertyType.DateTime:
                case ePropertyType.DateTimeNullable:

                    ulong ulTicks = this.ReadUlong();
                    return new DateTime((long)ulTicks);

                case ePropertyType.String:

                    return this.ReadStringBin();

                case ePropertyType.ByteArray:

                    return this.ReadBinary();

                default:

                    Debug.Assert(false, $"Unknown or incorrect param {pt.EType}");
                    break;
            }

            throw new InvalidDataException($"ReadObjectPropertyValue() ERROR. Unknown or incorrect param {pt.EType}");
        }

        public void Dispose()
        {
            StringBuilderPool.Instance.Put(m_sb);
            m_sb = null;
        }
    }

    public class BinReadFromString : IRead
    {
        public readonly string Value;
        protected int m_i = 0;

        public BinReadFromString(string sValue)
        {
            this.Value = sValue;
        }

        public char PeekChar()
        {
            return Value[m_i];
        }

        public char ReadChar()
        {
            return Value[m_i++];
        }

        public bool IsToStop()
        {
            return this.Value.Length <= m_i;
        }
    }
}
