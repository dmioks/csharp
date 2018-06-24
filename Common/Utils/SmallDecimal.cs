using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;

namespace Dmioks.Common.Utils
{
    public class SmallDecimal
    {
        public static readonly SmallDecimal ZERO = new SmallDecimal(0L, 0);

        public long Unscaled { get; protected set; }
        public byte Scale { get; protected set; }

        public SmallDecimal(long lUnscaled, byte btScale)
        {
            this.Unscaled = lUnscaled;
            this.Scale = btScale;
        }

        public decimal DecimalValue()
        {
            return ToDecimal(this);
        }

        public static decimal ToDecimal(SmallDecimal sd)
        {
            Debug.Assert(sd != null);

            long l = Math.Abs(sd.Unscaled);

            long lLow = l & 0x00000000FFFFFFFF;
            long lMid = l >> 32;

            return new decimal((Int32)lLow, (Int32)lMid, 0, l < sd.Unscaled, sd.Scale);
        }

        public static SmallDecimal ToSmallDecimal(decimal dc)
        {
            int[] arr = Decimal.GetBits(dc);

            Debug.Assert(arr[2] == 0, string.Format("SmallDecimal.ToSmallDecimal({0}) ERROR. Value is too big", dc));

            long lLow = arr[0];
            long lMid = arr[1];

            byte btScale = (byte)((arr[3] >> 16) & 31);

            long lUnscaled = (lMid << 32) + lLow;

            return new SmallDecimal(lUnscaled, btScale);
        }

        public static SmallDecimal FromString(string sValue)
        {
            decimal dc = decimal.Parse(sValue);
            return ToSmallDecimal(dc);
        }

        /*
        public void Serialize(BinaryWriter wr)
        {
            BinHelper.SerializeNum(wr, this.Unscaled);
            wr.Write(this.Scale);
        }
        */

        public static SmallDecimal Deserialize(IBinRead br)
        {
            long lUnscaled = BinHelper.DeserializeLong(br);
            byte btScale = br.ReadByte();

            return new SmallDecimal(lUnscaled, btScale);
        }

        public override int GetHashCode()
        {
            return this.Unscaled.GetHashCode() ^ this.Scale.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            SmallDecimal sd = obj as SmallDecimal;

            if (sd != null)
            {
                return this.Unscaled == sd.Unscaled && this.Scale == sd.Scale;
            }

            return false;
        }

        public override string ToString()
        {
            decimal dc = ToDecimal(this);
            return dc.ToString();
        }
    }
}
