using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Collections;

namespace Dmioks.Common.Entity
{
    public delegate SimpleEntity DelegateCreateNewObject(object objParam);

    public class ObjectType
    {
        public readonly int BinName;
        public readonly string Name;
        protected readonly DelegateCreateNewObject m_dcno;
        public readonly PropertyKey[] PropertyKeys;
        public readonly PropertyKey[] IdentityKeys;
        public readonly PropertyKey SingleIdentityKey;
        protected Dictionary<int, PropertyKey> m_diIntToKey = null;

        public static readonly DelegateCreateNewObject DEFAULT_CREATE_OBJECT = delegate(object objParam)
        {
            return new SimpleEntity();
        };

        public ObjectType(int iBinName, string sName, DelegateCreateNewObject dcno, PropertyKey[] arrPropertyKeys) 
        {
            Debug.Assert(0 < iBinName && iBinName <= PropertyKey.MAX_BIN_NAME);
            Debug.Assert(!string.IsNullOrEmpty(sName));
            Debug.Assert(dcno != null);
            Debug.Assert(arrPropertyKeys != null && 0 < arrPropertyKeys.Length);

            this.BinName = iBinName;
            this.Name = sName;
            m_dcno = dcno;
            this.PropertyKeys = arrPropertyKeys;
            this.IdentityKeys = GetIdentityKeys(arrPropertyKeys);
            this.SingleIdentityKey = this.IdentityKeys.Length == 1 ? this.IdentityKeys[0] : null;
        }

        public static PropertyKey[] GetIdentityKeys (PropertyKey[] arrPropertyKeys)
        {
            List<PropertyKey> lIdentityKeys = new List<PropertyKey>();

            foreach (PropertyKey pk in arrPropertyKeys)
            {
                if (pk.IdentityKey != eKeyIdentity.None)
                {
                    lIdentityKeys.Add(pk);
                }
            }

            return lIdentityKeys.ToArray();
        }

        public static Dictionary<int, PropertyKey> KeyArrayToDictionary(PropertyKey[] arrPropertyKeys)
        {
            Debug.Assert(arrPropertyKeys != null && 0 < arrPropertyKeys.Length);

            Dictionary<int, PropertyKey> diPropertyKeys = new Dictionary<int, PropertyKey>();

            foreach (PropertyKey pk in arrPropertyKeys)
            {
                Debug.Assert(pk != null);
                diPropertyKeys.Add(pk.BinName, pk);
            }

            return diPropertyKeys;
        }

        public Dictionary<int, PropertyKey> EnsurePropertyKeysDictionary()
        {
            if (m_diIntToKey == null)
            {
                m_diIntToKey = KeyArrayToDictionary(this.PropertyKeys);
            }

            return m_diIntToKey;
        }

        public Dictionary<int, PropertyKey> IntToKey
        {
            get { return m_diIntToKey; }
        }

        public SimpleEntity CreateNewObject(object objParam)
        {
            return m_dcno(objParam);
        }

        public List<PropertyKey> GetSortedKeys()
        {
            List<PropertyKey> lResult = new List<PropertyKey>(this.PropertyKeys);

            lResult.Sort(delegate (PropertyKey pk1, PropertyKey pk2) { return pk1.BinName.CompareTo(pk2.BinName); });

            return lResult;
        }

        public override bool Equals(object obj)
        {
            ObjectType ot = obj as ObjectType;

            return ot != null && this.BinName == ot.BinName;
        }

        public override int GetHashCode()
        {
            return this.BinName;
        }

        public override string ToString()
        {
            return $"{this.Name}({this.BinName})";
        }
    }
}
