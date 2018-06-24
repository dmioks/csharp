using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Entity;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public class Write : IWrite
    {
        public readonly Stream Stream;

        public Write(Stream stream)
        {
            this.Stream = stream;
        }

        public void WriteByte(byte bt)
        {
            this.Stream.WriteByte(bt);
        }

        public void WriteChar(char ch)
        {
            BinHelper.EncodeUtf8(this.Stream, ch);
        }

        public void WriteString(string sValue)
        {
            BinHelper.EncodeUtf8(this.Stream, sValue);
        }
    }

    public class BinWrite : Write, IBinWrite
    {
        public const int DECIMAL_ARRAY_COUNT = 4;

        protected readonly byte[] m_arrUlongBuffer = new byte[8];
        Dictionary<int, ObjectType> m_diSentObjectTypes = new Dictionary<int, ObjectType>();

        public BinWrite(Stream stream) : base(stream)
        {
        }

        public byte[] UlongBuffer
        {
            get { return m_arrUlongBuffer; }
        }

        public void WriteUshort(uint uiValue, bool bFlag)
        {
            BinHelper.SerializeUshort(this, uiValue, bFlag);
        }

        public void WriteShortInt(int iValue)
        {
            BinHelper.SerializeShortInt(this, iValue);
        }

        public void WriteUlong(ulong ulValue)
        {
            BinHelper.SerializeUlong(this, ulValue);
        }

        public void WriteLong(long lValue)
        {
            BinHelper.SerializeLong(this, lValue);
        }

        public void WriteStringBin(string sValue)
        {
            ExcpHelper.ThrowIf<InvalidDataException>(sValue == null, "WriteStringBin() ERROR. Argument is null.");

            BinHelper.SerializeUlong(this, (ulong) sValue.Length);
            BinHelper.EncodeUtf8(this.Stream, sValue);
        }

        public void WriteBinary(byte[] arr)
        {
            ExcpHelper.ThrowIf<InvalidDataException>(arr == null, "WriteBinary() ERROR. Byte array is null.");

            this.WriteBinarySlice(arr, arr.Length);
        }

        public void WriteBinarySlice(byte[] arr, int iLength)
        {
            BinHelper.SerializeUlong(this, (ulong) iLength);

            this.Stream.Write(arr, 0, iLength);
        }

        protected void WritePropertyKey(PropertyKey pk, bool bFlag)
        {
            Debug.Assert(pk != null);

            this.WriteUshort((uint) pk.BinName, bFlag);
            this.WriteStringBin(pk.Name);
            this.WriteByte((byte) pk.IdentityKey);
            this.WriteUshort((uint) pk.PropertyType.EType, false);
        }

        public void WriteObject(SimpleEntity se)
        {
            if (se.ObjectType == null || se.ObjectType.PropertyKeys == null)
            {
                throw new InvalidDataException("WriteObject() ERROR. SimpleEntity.ObjectType is null or SimpleEntity.ObjectType.PropertyKeys is null.");
            }

            // Write Object Type
            if (m_diSentObjectTypes.TryGetValue(se.ObjectType.BinName, out ObjectType ot))
            {
                Debug.Assert(ot.BinName == se.ObjectType.BinName);
                this.WriteUshort((uint)se.ObjectType.BinName, true); // DK - It is enough to send just bin class name
            }
            else
            {
                this.WriteUshort((uint)se.ObjectType.BinName, false); // DK - Full Object type will be send
                this.WriteStringBin(se.ObjectType.Name);
                this.WriteUshort((uint) se.ObjectType.PropertyKeys.Length, false);

                foreach (PropertyKey pk in se.ObjectType.PropertyKeys)
                {
                    this.WritePropertyKey(pk, false);
                }

                m_diSentObjectTypes.Add(se.ObjectType.BinName, se.ObjectType);
            }

            // Write Object
            foreach (PropertyKey pk in se.ObjectType.PropertyKeys)
            {
                if (se.Items.TryGetValue(pk, out object objValue))
                {
                    if (objValue == null)
                    {
                        this.WriteUshort((uint)pk.BinName, true); // Flag value true means property value is null
                    }
                    else
                    {
                        // Value is not null
                        this.WriteUshort((uint)pk.BinName, false); 
                        this.WriteObjectPropertyValue(pk, objValue);
                    }
                }
            }

            this.WriteUshort(0, false); // End of Current Object
        }

        protected void WriteDecimal(decimal d)
        {
            int[] arr = decimal.GetBits(d);
            Debug.Assert(arr.Length == DECIMAL_ARRAY_COUNT);

            for (int i = 0; i < DECIMAL_ARRAY_COUNT; i ++)
            {
                this.WriteLong(arr[i]);
            }
        }

        protected void WriteGuid(Guid guid)
        {
            byte[] arr = guid.ToByteArray();
            Debug.Assert(arr.Length == BinRead.SERIALIZED_GUID_BYTE_ARRAY_LENGTH);

            for (int i = 0; i < BinRead.SERIALIZED_GUID_BYTE_ARRAY_LENGTH; i++)
            {
                this.WriteByte(arr[i]);
            }
        }

        protected delegate void DelegateWriteArrayValue<T>(T value);

        protected void WriteArray<T>(T[] arr, DelegateWriteArrayValue<T> dwav)
        {
            ExcpHelper.ThrowIf<InvalidDataException>(arr == null, $"WriteArray<{typeof(T).Name}>() ERROR. Array is null.");

            this.WriteUlong((ulong) arr.Length);

            for (int i = 0; i < arr.Length; i++)
            {
                dwav(arr[i]);
            }
        }

        protected void WriteObjectPropertyValue(PropertyKey pk, object objValue)
        {
            switch (pk.PropertyType.EType)
            {
                case ePropertyType.Int16Array:

                    this.WriteArray<Int16>((Int16[]) objValue, delegate(Int16 iValue)
                    {
                        this.WriteLong(iValue);
                    });

                    break;

                case ePropertyType.Int32Array:

                    this.WriteArray<int>((int[]) objValue, delegate (int iValue)
                    {
                        this.WriteLong(iValue);
                    });

                    break;

                case ePropertyType.Int64Array:

                    this.WriteArray<long>((long[]) objValue, delegate (long lValue)
                    {
                        this.WriteLong(lValue);
                    });

                    break;

                case ePropertyType.DecimalArray:

                    this.WriteArray<decimal>((decimal[]) objValue, delegate (decimal dcValue)
                    {
                        this.WriteDecimal(dcValue);
                    });

                    break;

                case ePropertyType.BoolArray:

                    this.WriteArray<bool>((bool[])objValue, delegate (bool bValue)
                    {
                        this.WriteByte((byte)(bValue ? 1 : 0));
                    });

                    break;

                case ePropertyType.GuidArray:

                    this.WriteArray<Guid>((Guid[])objValue, delegate (Guid guid)
                    {
                        this.WriteGuid(guid);
                    });

                    break;

                case ePropertyType.DateTimeArray:

                    this.WriteArray<DateTime>((DateTime[]) objValue, delegate (DateTime dtValue)
                    {
                        this.WriteUlong((ulong) dtValue.Ticks);
                    });

                    break;

                case ePropertyType.StringArray:

                    this.WriteArray<string>((string[])objValue, delegate (string sValue)
                    {
                        this.WriteStringBin(sValue);
                    });

                    break;

                case ePropertyType.ObjectArray:

                    this.WriteArray<SimpleEntity>((SimpleEntity[])objValue, delegate (SimpleEntity seValue)
                    {
                        this.WriteObject(seValue);
                    });

                    break;


                case ePropertyType.Object:

                    SimpleEntity objEntity = objValue as SimpleEntity;
                    Debug.Assert(objEntity != null);

                    this.WriteObject(objEntity);

                    break;

                case ePropertyType.Int16:
                case ePropertyType.Int16Nullable:
                case ePropertyType.Int32:
                case ePropertyType.Int32Nullable:
                case ePropertyType.Int64:
                case ePropertyType.Int64Nullable:

                    this.WriteLong(Convert.ToInt64(objValue));
                    break;

                case ePropertyType.Decimal:
                case ePropertyType.DecimalNullable:

                    this.WriteDecimal((decimal) objValue);

                    break;

                case ePropertyType.Bool:
                case ePropertyType.BoolNullable:

                    bool b = (bool)objValue;
                    this.WriteByte((byte)(b ? 1 : 0));
                    break;

                case ePropertyType.Guid:
                case ePropertyType.GuidNullable:

                    Guid guid2 = (Guid)objValue;
                    this.WriteGuid(guid2);
                    break;

                case ePropertyType.DateTime:
                case ePropertyType.DateTimeNullable:

                    DateTime dt = (DateTime)objValue;
                    this.WriteUlong((ulong) dt.Ticks);
                    break;

                case ePropertyType.String:

                    this.WriteStringBin((string) objValue);
                    break;

                case ePropertyType.ByteArray:

                    this.WriteBinary(objValue as byte[]);
                    break;

                default:

                    Debug.Assert(false, $"Unknown or incorrect param {pk.PropertyType.EType}");
                    throw new InvalidDataException($"WriteObjectPropertyValue({pk}, {objValue}) ERROR: Unknown or incorrect param {pk.PropertyType.EType}");
                    break;
            }
        }
    }

    public class BinWriteToString : IWrite, IDisposable
    {
        private StringBuilder m_sb = StringBuilderPool.Instance.Get();

        public byte[] UlongBuffer
        {
            get { throw new NotSupportedException(); }
        }

        public void Dispose()
        {
            StringBuilderPool.Instance.Put(m_sb);
            m_sb = null;
        }

        public void WriteByte(byte bt)
        {
            m_sb.Append((char) bt);
        }

        public void WriteChar(char ch)
        {
            m_sb.Append(ch);
        }

        public void WriteString(string sValue)
        {
            m_sb.Append(sValue);
        }

        public void WriteUlong(ulong ulValue)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return m_sb.ToString();
        }
    }
}
