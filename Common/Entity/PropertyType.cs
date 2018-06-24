using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Collections;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Entity
{
    public enum ePropertyType
    {
        Int16 = 2,
        Int16Array = 3,
        Int16Nullable = 4,
        Int32 = 6,
        Int32Array = 7,
        Int32Nullable = 8,
        Int64 = 12,
        Int64Array = 13,
        Int64Nullable = 14,
        Decimal = 16,
        DecimalArray = 17,
        DecimalNullable = 18,
        Bool = 22,
        BoolArray = 23,
        BoolNullable = 24,
        Guid = 26,
        GuidArray = 27,
        GuidNullable = 28,
        DateTime = 32,
        DateTimeArray = 33,
        DateTimeNullable = 34,
        String = 42,
        StringArray = 43,
        ByteArray = 46,
        Object = 52,
        ObjectArray = 53,
    }

    public abstract class PropertyType
    {
        public readonly ePropertyType EType;
        public readonly System.Type ValueType;

        protected PropertyType(ePropertyType ept, Type valueType)
        {
            this.EType = ept;
            this.ValueType = valueType;
        }

        public delegate object DelegateTypeConverter(object objValue);

        

        public override bool Equals(object obj)
        {
            PropertyType pt = obj as PropertyType;

            return pt != null && this.EType == pt.EType;
        }

        public override int GetHashCode()
        {
            return (int) this.EType;
        }

        public override string ToString()
        {
            return this.EType.ToString();
        }

        public abstract PropertyKey CreatePropertyKey(int iBinName, string sName, eKeyIdentity ki = eKeyIdentity.None);
    }

    public class PropertyType<T> : PropertyType
    {
        //public static readonly PropertyType<SimpleEntity> OBJECT = new PropertyType<SimpleEntity>(new ObjectType(1, typeof(SimpleEntity).Name, ObjectType.DEFAULT_CREATE_NEW_OBJECT));

        internal protected PropertyType(ePropertyType ept) : base (ept, typeof(T))
        {
        }

        public T Cast(object objValue)
        {
            if (objValue == null || objValue == DBNull.Value)
            {
                if (!PropertyTypes.NULLABLE_TYPES.Contains(this.EType))
                {
                    throw new InvalidCastException($"PropertyType<{typeof(T).Name}>.Cast(null) ERROR. This PropertyType is not nullable.");
                }

                return default(T);
            }

            try
            {
                switch (this.EType)
                {
                    case ePropertyType.Int16Nullable: return (T) Convert.ChangeType(objValue, PropertyTypes.TYPE_INT_16);
                    case ePropertyType.Int32Nullable: return (T)Convert.ChangeType(objValue, PropertyTypes.TYPE_INT_32);
                    case ePropertyType.Int64Nullable: return (T)Convert.ChangeType(objValue, PropertyTypes.TYPE_LONG);
                    case ePropertyType.DecimalNullable: return (T)Convert.ChangeType(objValue, PropertyTypes.TYPE_DECIMAL);
                    case ePropertyType.BoolNullable: return (T)Convert.ChangeType(objValue, PropertyTypes.TYPE_BOOL);
                    case ePropertyType.GuidNullable: return (T)Convert.ChangeType(objValue, PropertyTypes.TYPE_GUID);
                    case ePropertyType.DateTimeNullable: return (T)Convert.ChangeType(objValue, PropertyTypes.TYPE_DATETIME);
                    case ePropertyType.Object: return (T) objValue;

                    default: return (T)Convert.ChangeType(objValue, this.ValueType);
                }
            }
            catch (Exception excp)
            {
                throw new Exception($"{this}.Cast() ERROR", excp);
            }
        }

        public override PropertyKey CreatePropertyKey(int iBinName, string sName, eKeyIdentity ki = eKeyIdentity.None)
        {
            return new PropertyKey<T>(iBinName, sName, ki, this);
        }
    }

    public static class PropertyTypes
    {
        public static readonly Type TYPE_INT_16 = typeof(Int16);
        public static readonly Type TYPE_INT_16_ARRAY = typeof(Int16[]);
        public static readonly Type TYPE_INT_16_NULLABLE = typeof(Int16?);

        public static readonly Type TYPE_INT_32 = typeof(Int32);
        public static readonly Type TYPE_INT_32_ARRAY = typeof(Int32[]);
        public static readonly Type TYPE_INT_32_NULLABLE = typeof(Int32?);

        public static readonly Type TYPE_LONG = typeof(long);
        public static readonly Type TYPE_LONG_ARRAY = typeof(long[]);
        public static readonly Type TYPE_LONG_NULLABLE = typeof(long?);

        public static readonly Type TYPE_DECIMAL = typeof(decimal);
        public static readonly Type TYPE_DECIMAL_ARRAY = typeof(decimal[]);
        public static readonly Type TYPE_DECIMAL_NULLABLE = typeof(decimal?);

        public static readonly Type TYPE_BOOL = typeof(bool);
        public static readonly Type TYPE_BOOL_ARRAY = typeof(bool[]);
        public static readonly Type TYPE_BOOL_NULLABLE = typeof(bool?);

        public static readonly Type TYPE_GUID = typeof(Guid);
        public static readonly Type TYPE_GUID_ARRAY = typeof(Guid[]);
        public static readonly Type TYPE_GUID_NULLABLE = typeof(Guid?);

        public static readonly Type TYPE_DATETIME = typeof(DateTime);
        public static readonly Type TYPE_DATETIME_ARRAY = typeof(DateTime[]);
        public static readonly Type TYPE_DATETIME_NULLABLE = typeof(DateTime?);

        public static readonly Type TYPE_STRING = typeof(string);
        public static readonly Type TYPE_STRING_ARRAY = typeof(string[]);

        public static readonly Type TYPE_BYTE = typeof(byte);
        public static readonly Type TYPE_BYTE_ARRAY = typeof(byte[]);

        public static readonly Type TYPE_OBJECT = typeof(SimpleEntity);
        public static readonly Type TYPE_OBJECT_ARRAY = typeof(SimpleEntity[]);

        public static readonly PropertyType<Int16>          INT_16                = new PropertyType<Int16>(ePropertyType.Int16);
        public static readonly PropertyType<Int16[]>        INT_16_ARRAY          = new PropertyType<Int16[]>(ePropertyType.Int16Array);
        public static readonly PropertyType<Int16?>         INT_16_NULLABLE       = new PropertyType<Int16?>(ePropertyType.Int16Nullable);

        public static readonly PropertyType<int>            INT_32                = new PropertyType<int>(ePropertyType.Int32);
        public static readonly PropertyType<int[]>          INT_32_ARRAY          = new PropertyType<int[]>(ePropertyType.Int32Array);
        public static readonly PropertyType<int?>           INT_32_NULLABLE       = new PropertyType<int?>(ePropertyType.Int32Nullable);

        public static readonly PropertyType<long>           INT_64                = new PropertyType<long>(ePropertyType.Int64);
        public static readonly PropertyType<long[]>         INT_64_ARRAY          = new PropertyType<long[]>(ePropertyType.Int64Array);
        public static readonly PropertyType<long?>          INT_64_NULLABLE       = new PropertyType<long?>(ePropertyType.Int64Nullable);

        public static readonly PropertyType<decimal>        DECIMAL               = new PropertyType<decimal>(ePropertyType.Decimal);
        public static readonly PropertyType<decimal[]>      DECIMAL_ARRAY         = new PropertyType<decimal[]>(ePropertyType.DecimalArray);
        public static readonly PropertyType<decimal?>       DECIMAL_NULLABLE      = new PropertyType<decimal?>(ePropertyType.DecimalNullable);

        public static readonly PropertyType<bool>           BOOL                  = new PropertyType<bool>(ePropertyType.Bool);
        public static readonly PropertyType<bool[]>         BOOL_ARRAY            = new PropertyType<bool[]>(ePropertyType.BoolArray);
        public static readonly PropertyType<bool?>          BOOL_NULLABLE         = new PropertyType<bool?>(ePropertyType.BoolNullable);

        public static readonly PropertyType<Guid>           GUID                  = new PropertyType<Guid>(ePropertyType.Guid);
        public static readonly PropertyType<Guid[]>         GUID_ARRAY            = new PropertyType<Guid[]>(ePropertyType.GuidArray);
        public static readonly PropertyType<Guid?>          GUID_NULLABLE         = new PropertyType<Guid?>(ePropertyType.GuidNullable);

        public static readonly PropertyType<DateTime>       DATE_TIME             = new PropertyType<DateTime>(ePropertyType.DateTime);
        public static readonly PropertyType<DateTime[]>     DATE_TIME_ARRAY       = new PropertyType<DateTime[]>(ePropertyType.DateTimeArray);
        public static readonly PropertyType<DateTime?>      DATE_TIME_NULLABLE    = new PropertyType<DateTime?>(ePropertyType.DateTimeNullable);

        public static readonly PropertyType<string>         STRING                = new PropertyType<string>(ePropertyType.String);
        public static readonly PropertyType<string[]>       STRING_ARRAY          = new PropertyType<string[]>(ePropertyType.StringArray);

        public static readonly PropertyType<byte[]>         BYTE_ARRAY            = new PropertyType<byte[]>(ePropertyType.ByteArray);

        public static readonly PropertyType<SimpleEntity>   OBJECT                = new PropertyType<SimpleEntity>(ePropertyType.Object);
        public static readonly PropertyType<SimpleEntity[]> OBJECT_ARRAY          = new PropertyType<SimpleEntity[]>(ePropertyType.ObjectArray);

        // Static
        public static readonly HashSet<ePropertyType> NULLABLE_TYPES = new HashSet<ePropertyType>()
        {
            ePropertyType.Int16Nullable,
            ePropertyType.Int16Array,
            ePropertyType.Int32Nullable,
            ePropertyType.Int32Array,
            ePropertyType.Int64Nullable,
            ePropertyType.Int64Array,
            ePropertyType.DecimalNullable,
            ePropertyType.DecimalArray,
            ePropertyType.BoolNullable,
            ePropertyType.BoolArray,
            ePropertyType.GuidNullable,
            ePropertyType.GuidArray,
            ePropertyType.DateTimeNullable,
            ePropertyType.DateTimeArray,
            ePropertyType.String,
            ePropertyType.StringArray,
            ePropertyType.ByteArray,
            ePropertyType.Object,
            ePropertyType.ObjectArray,
        };

        // Static
        public static readonly HashSet<ePropertyType> DB_NULLABLE_TYPES = new HashSet<ePropertyType>()
        {
            ePropertyType.Int16Nullable,
            ePropertyType.Int32Nullable,
            ePropertyType.Int64Nullable,
            ePropertyType.DecimalNullable,
            ePropertyType.BoolNullable,
            ePropertyType.GuidNullable,
            ePropertyType.DateTimeNullable,
        };

        // Static
        public static readonly HashSet<ePropertyType> ARRAY_TYPES = new HashSet<ePropertyType>()
        {
            ePropertyType.ByteArray,
            ePropertyType.Int16Array,
            ePropertyType.Int32Array,
            ePropertyType.Int64Array,
            ePropertyType.DecimalArray,
            ePropertyType.BoolArray,
            ePropertyType.GuidArray,
            ePropertyType.DateTimeArray,
            ePropertyType.StringArray,
            ePropertyType.ObjectArray,
        };

        private static readonly SyncDictionary<Type, ePropertyType> m_diTypes = new SyncDictionary<Type, ePropertyType>()
        {
            {TYPE_INT_16, ePropertyType.Int16 },
            {TYPE_INT_16_ARRAY, ePropertyType.Int16Array },
            {TYPE_INT_16_NULLABLE, ePropertyType.Int16Nullable },

            {TYPE_INT_32, ePropertyType.Int32 },
            {TYPE_INT_32_ARRAY, ePropertyType.Int32Array },
            {TYPE_INT_32_NULLABLE, ePropertyType.Int32Nullable },

            {TYPE_LONG, ePropertyType.Int64 },
            {TYPE_LONG_ARRAY, ePropertyType.Int64Array },
            {TYPE_LONG_NULLABLE, ePropertyType.Int64Nullable },

            {TYPE_DECIMAL, ePropertyType.Decimal},
            {TYPE_DECIMAL_ARRAY, ePropertyType.DecimalArray},
            {TYPE_DECIMAL_NULLABLE, ePropertyType.DecimalNullable},

            {TYPE_BOOL, ePropertyType.Bool},
            {TYPE_BOOL_ARRAY, ePropertyType.BoolArray},
            {TYPE_BOOL_NULLABLE, ePropertyType.BoolNullable},

            {TYPE_GUID, ePropertyType.Guid},
            {TYPE_GUID_ARRAY, ePropertyType.GuidArray},
            {TYPE_GUID_NULLABLE, ePropertyType.GuidNullable},

            {TYPE_DATETIME, ePropertyType.DateTime},
            {TYPE_DATETIME_ARRAY, ePropertyType.DateTimeArray},
            {TYPE_DATETIME_NULLABLE, ePropertyType.DateTimeNullable},

            {TYPE_STRING, ePropertyType.String},
            {TYPE_STRING_ARRAY, ePropertyType.StringArray},

            {TYPE_BYTE_ARRAY, ePropertyType.ByteArray},

            {TYPE_OBJECT, ePropertyType.Object},
            {TYPE_OBJECT_ARRAY, ePropertyType.ObjectArray},
        };

        public static bool GetEType(System.Type type, out ePropertyType ept)
        {
            return m_diTypes.TryGetValue(type, out ept);
        }

        public static Type GetArrayMemberType(ePropertyType eptOfArray)
        {
            if (!ARRAY_TYPES.Contains(eptOfArray))
            {
                throw new ArgumentException ($"GetArrayMemberType({eptOfArray}) ERROR. The given type is not an array.");
            }

            switch (eptOfArray)
            {
                case ePropertyType.ByteArray: return TYPE_BYTE;
                case ePropertyType.Int16Array: return TYPE_INT_16;
                case ePropertyType.Int32Array: return TYPE_INT_32;
                case ePropertyType.Int64Array: return TYPE_LONG;
                case ePropertyType.DecimalArray: return TYPE_DECIMAL;
                case ePropertyType.BoolArray: return TYPE_BOOL;
                case ePropertyType.GuidArray: return TYPE_GUID;
                case ePropertyType.DateTimeArray: return TYPE_DATETIME;
                case ePropertyType.StringArray: return TYPE_STRING;
                case ePropertyType.ObjectArray: return TYPE_OBJECT;
            }

            Debug.Assert(false);

            return TYPE_OBJECT;
        }

        public static PropertyType GetPropertyType(ePropertyType ept)
        {
            switch (ept)
            {
                case ePropertyType.Int16: return PropertyTypes.INT_16;
                case ePropertyType.Int16Array: return PropertyTypes.INT_16_ARRAY;
                case ePropertyType.Int16Nullable: return PropertyTypes.INT_16_NULLABLE;
                case ePropertyType.Int32: return PropertyTypes.INT_32;
                case ePropertyType.Int32Array: return PropertyTypes.INT_32_ARRAY;
                case ePropertyType.Int32Nullable: return PropertyTypes.INT_32_NULLABLE;
                case ePropertyType.Int64: return PropertyTypes.INT_64;
                case ePropertyType.Int64Array: return PropertyTypes.INT_64_ARRAY;
                case ePropertyType.Int64Nullable: return PropertyTypes.INT_64_NULLABLE;
                case ePropertyType.Decimal: return PropertyTypes.DECIMAL;
                case ePropertyType.DecimalArray: return PropertyTypes.DECIMAL_ARRAY;
                case ePropertyType.DecimalNullable: return PropertyTypes.DECIMAL_NULLABLE;
                case ePropertyType.Bool: return PropertyTypes.BOOL;
                case ePropertyType.BoolArray: return PropertyTypes.BOOL_ARRAY;
                case ePropertyType.BoolNullable: return PropertyTypes.BOOL_NULLABLE;
                case ePropertyType.Guid: return PropertyTypes.GUID;
                case ePropertyType.GuidArray: return PropertyTypes.GUID_ARRAY;
                case ePropertyType.GuidNullable: return PropertyTypes.GUID_NULLABLE;
                case ePropertyType.DateTime: return PropertyTypes.DATE_TIME;
                case ePropertyType.DateTimeArray: return PropertyTypes.DATE_TIME_ARRAY;
                case ePropertyType.DateTimeNullable: return PropertyTypes.DATE_TIME_NULLABLE;
                case ePropertyType.String: return PropertyTypes.STRING;
                case ePropertyType.StringArray: return PropertyTypes.STRING_ARRAY;
                case ePropertyType.ByteArray: return PropertyTypes.BYTE_ARRAY;
                case ePropertyType.Object: return PropertyTypes.OBJECT;
                case ePropertyType.ObjectArray: return PropertyTypes.OBJECT_ARRAY;

                default:

                    Debug.Assert(false, $"Unknown or incorrect param {ept}");
                    break;
            }

            throw new ArgumentException($"GetPropertyType() ERROR. Unknown or incorrect param {ept}");
        }
    }
}
