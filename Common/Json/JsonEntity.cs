using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Json
{
    public abstract class JsonEntity
    {
        protected JsonEntity()
        {
        }

        public static readonly Dictionary<char, byte> MAP_OF_SPECIAL_CHARS = new Dictionary<char, byte>()
        {
            {(char) 8, (byte) 'b'},
            {(char) 9, (byte) 't'},
            {(char)12, (byte) 'f'},
            {(char)10, (byte) 'n'},
            {(char)13, (byte) 'r'},
            {(char)34, (byte) '"'},
            {(char)92, (byte) '\\'},
        };


        private static readonly Type TYPE_STRING    = typeof(string);
        private static readonly Type TYPE_INT       = typeof(int);
        private static readonly Type TYPE_LONG      = typeof(long);
        private static readonly Type TYPE_BOOLEAN   = typeof(bool);
        private static readonly Type TYPE_DECIMAL   = typeof(decimal);

        private delegate void DelegateSerializeObject(IWrite bw, object obj);

        private static void SerializeString(IWrite bw, object obj)
        {
            string s = (string)obj;
            bw.WriteByte((byte)'"');

            foreach (char ch in s)
            {
                if (MAP_OF_SPECIAL_CHARS.TryGetValue(ch, out byte btSpecial))
                {
                    bw.WriteByte((byte)'\\');
                    bw.WriteByte(btSpecial);
                }
                else
                {
                    bw.WriteChar(ch);
                }
            }

            bw.WriteByte((byte)'"');
        }

        private static readonly Dictionary<Type, DelegateSerializeObject> TYPE_MAP = new Dictionary<Type, DelegateSerializeObject>() {
            { TYPE_STRING, SerializeString},
            { TYPE_INT, delegate(IWrite bw, object obj){
                bw.WriteString(Convert.ToString((int)obj));
            }},
            { TYPE_LONG, delegate(IWrite bw, object obj){
                bw.WriteString(Convert.ToString((long)obj));
            }},
            { TYPE_BOOLEAN, delegate(IWrite bw, object obj){
                bool b = (bool) obj;
                bw.WriteString(b ? "true" : "false");
            }},
            { TYPE_DECIMAL, delegate(IWrite bw, object obj){
                decimal d = (decimal) obj;
                bw.WriteString(d.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture));
            }},
        };

        protected delegate void DelegateSerializeEntity(JsonEntity je);

        protected static void FormatSpace(IWrite bw, int iFormat, int iDepth)
        {
            bw.WriteByte((byte)'\r');
            bw.WriteByte((byte)'\n');

            int iSpaceChars = iFormat * iDepth;

            for (int i = 0; i < iSpaceChars; i++)
            {
                bw.WriteByte((byte)' ');
            }
        }

        protected static void SerializeType(IWrite bw, object obj, DelegateSerializeEntity dse)
        {
            if (obj == null)
            {
                bw.WriteString("null");
            }
            else
            {
                Type t = obj.GetType();

                if (TYPE_MAP.TryGetValue(t, out DelegateSerializeObject d))
                {
                    d(bw, obj);
                }
                else 
                {
                    JsonEntity je = obj as JsonEntity;

                    if (je != null)
                    {
                        Debug.Assert(dse != null);
                        dse(je);
                        //je.Serialize(bw, iFormat);
                    }
                    else
                    {
                        //Debug.Assert(false);
                    }
                }
            }
        }

        public abstract void SerializeImp(IWrite bw, int iFormat, int iDepth);
        public abstract void Serialize(IWrite bw, int iFormat);

        public string ToString(int iFormat)
        {
            using (BinWriteToString bw = new BinWriteToString())
            {
                this.SerializeImp(bw, iFormat, 0);
                return bw.ToString();
            }
        }

        public override string ToString()
        {
            return this.ToString(0);
        }
    }
}
