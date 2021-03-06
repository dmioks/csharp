﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Json;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Entity
{
    public enum eKeyIdentity
    {
        None = 0,
        AutoGenerated = 1,
        ManualInsert = 2,
    }

    public abstract class PropertyKey
    {
        public const int MAX_BIN_NAME = BinHelper.MAX_USHORT_VALUE;

        public readonly int BinName;
        public readonly string Name;
        public readonly eKeyIdentity IdentityKey;
        public readonly PropertyType PropertyType;

        protected PropertyKey(int iBinName, string sName, eKeyIdentity ki, PropertyType pt)
        {
            Debug.Assert(0 < iBinName && iBinName <= MAX_BIN_NAME);
            Debug.Assert(!string.IsNullOrEmpty(sName));
            Debug.Assert(pt != null);

            this.BinName = iBinName;
            this.Name = sName;
            this.IdentityKey = ki;
            this.PropertyType = pt;
        }

        protected PropertyKey(int iBinName, string sName, PropertyType pt) : this(iBinName, sName, eKeyIdentity.None, pt)
        {
        }

        public abstract D NullConditionalValue<D>(SimpleEntity si, D objDefault, PropertyType<D> ptDefault);
        public abstract bool TryFromDto(SimpleEntity se, JsonObject joDto, PropertyType.DelegateTypeConverter dtc = null);
        public abstract bool TryFromDtoAsIdentity<T>(SimpleEntity se, JsonObject joDto);
        public abstract void TryToDto(SimpleEntity se, JsonObject joDto, PropertyType.DelegateTypeConverter dtc = null);

        public override bool Equals(object obj)
        {
            PropertyKey pk = obj as PropertyKey;

            if (pk != null)
            {
#if DEBUG
                if (pk.BinName == this.BinName)
                {
                    Debug.Assert(pk.Name.Equals(this.Name));
                    return true;
                }
#endif
                return pk.BinName == this.BinName;
            }

            int? objInt = obj as int?;

            if (objInt != null)
            {
                return objInt.Equals(this.BinName);
            }

            return false;
        }

        public virtual Type GetPropertyType()
        {
            return typeof(object);
        }

        public override int GetHashCode()
        {
            return this.BinName;
        }

        public string ToFullString()
        {
            return $"{this.Name}({this.BinName}){this.PropertyType}";
        }

        public override string ToString()
        {
            return $"{this.Name}({this.BinName})";
        }
    }

    public class PropertyKey<T> : PropertyKey
    {
        public readonly new PropertyType<T> PropertyType;

        public PropertyKey(int iBinName, string sName, eKeyIdentity ki, PropertyType<T> pt) : base (iBinName, sName, ki, pt)
        {
            Debug.Assert(pt != null);

            this.PropertyType = pt;
        }

        public PropertyKey(int iBinName, string sName, PropertyType<T> pt) : this(iBinName, sName, eKeyIdentity.None, pt)
        {
        }

        public T Value(SimpleEntity si)
        {
            if (si.Items.TryGetValue(this, out object objValue))
            {
                return this.PropertyType.Cast(objValue);
            }

            Debug.Assert(false);

            throw new KeyNotFoundException();
        }

        public T Value(SimpleEntity si, T objDefault)
        {
            if (si.Items.TryGetValue(this, out object objValue))
            {
                return this.PropertyType.Cast(objValue);
            }

            return objDefault;
        }

        public override D NullConditionalValue<D>(SimpleEntity si, D objDefault, PropertyType<D> ptDefault)
        {
            if (si.Items.TryGetValue(this, out object objValue))
            {
                if (objValue == null)
                {
                    return objDefault;
                }

                ptDefault.Cast(objValue);
            }

            return objDefault;
        }

        public SE ObjectValue<SE>(SimpleEntity si) where SE : SimpleEntity
        {
            if (si.Items.TryGetValue(this, out object objValue))
            {
                return objValue as SE;
            }

            Debug.Assert(false);

            return null;
        }

        public bool Set(SimpleEntity se, T objValue)
        {
            bool bChanged = false;

            if (!se.Items.ContainsKey(this) || objValue == null || !objValue.Equals(se.Items[this]))
            {
                se.SetChanged();
                bChanged = true;
            }

            se.Items[this] = objValue;

            return bChanged;
        }

        public override bool TryFromDto(SimpleEntity se, JsonObject joDto, PropertyType.DelegateTypeConverter dtc = null)
        {
            Debug.Assert(joDto != null);

            if (joDto.TryGetValue(this.Name, out object objValue))
            {
                T obj = dtc == null ? this.PropertyType.Cast(objValue) : (T) dtc(objValue);

                return this.Set(se, obj);
            }

            return false;
        }

        public override bool TryFromDtoAsIdentity<T>(SimpleEntity se, JsonObject joDto)
        {
            Debug.Assert(joDto != null);

            T objSourceId = joDto.TryGet<T>(this.Name);
            T objTargetId = default(T);

            if (se.Items.TryGetValue(this, out object objIdentity))
            {
                objTargetId = (T) objIdentity;
            }

            if (objTargetId.Equals(default(T)))
            {
                if (!objSourceId.Equals(0))
                {
                    se.Items[this] = objSourceId;
                    return true;
                }
            }
            else if (!objSourceId.Equals(0) && !objTargetId.Equals(0))
            {
                if (!objSourceId.Equals(objTargetId))
                {
                    throw new Exception($"TryFromDtoAsIdentity<{typeof(T).Name}>() ERROR. Cannot merge identity {objSourceId} to {objTargetId}\r\n{joDto}\r\n{se}");
                }
            }
            else
            {
                Debug.Assert(false);
            }

            return false;
        }

        public override void TryToDto(SimpleEntity se, JsonObject joDto, PropertyType.DelegateTypeConverter dtc = null)
        {
            Debug.Assert(joDto != null);

            if (se.Items.TryGetValue(this, out object objValue))
            {
                joDto.Set(this.Name, dtc == null ? objValue : dtc(objValue));
            }
        }

        public override Type GetPropertyType()
        {
            return typeof(T);
        }
    }
}
