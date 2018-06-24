using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public sealed class BinHelperException : Exception
    {
        internal protected BinHelperException(string sError) : base(sError)
        {

        }

        public static void CheckCondition(bool bNecessaryCondition, string sErrorFormat, params object[] arrParams)
        {
            if (!bNecessaryCondition)
            {
                throw new BinHelperException(string.Format(sErrorFormat, arrParams));
            }
        }
    }

    public static class BinHelper
    {
        //public static long MAX_SERIALIZABLE_DWORD_VALUE   = 0x1FFFFFFF;
        public static int DEFAULT_BUFFER_SIZE = 4194304;
        public const int BYTE_NUM_MIN_VALUE = -128;
        public const int BYTE_NUM_MAX_VALUE = 127;
        public const int BYTE_OVERFLOW = 256;
        public const long MIN_LONG_VALUE = long.MinValue + 1;

        public const int MAX_DWORD_SHIFT_VALUE = 8;

        public static int ByteToInt(byte bt)
        {
            return BYTE_NUM_MAX_VALUE < bt ? ((int)bt) - BYTE_OVERFLOW : (int)bt;
        }

        public static byte IntToByte(int i)
        {
            Debug.Assert(BYTE_NUM_MIN_VALUE <= i && i <= BYTE_NUM_MAX_VALUE);
            return (byte)(i > BYTE_NUM_MAX_VALUE ? i - BYTE_OVERFLOW : i);
        }

        /*
        public static int EncodeUtf8(char chr, byte[] arrBuf)
        {
            Debug.Assert(arrBuf != null);
            Debug.Assert(3 <= arrBuf.Length);

            if (chr <= 0x7F)
            {
                arrBuf[0] = (byte)(chr & 0x7F);

                return 1;
            }
            else if (chr <= 0x7FF)
            {
                arrBuf[0] = (byte)(((chr & 0x07C0) >> 6) | 0xC0);
                arrBuf[1] = (byte)((chr & 0x003F) | 0x80);

                return 2;
            }

            Debug.Assert(chr <= 0xFFFF);

            arrBuf[0] = (byte)(((chr & 0xF000) >> 12) | 0xE0);
            arrBuf[1] = (byte)(((chr & 0x0FC0) >> 6) | 0x80);
            arrBuf[2] = (byte)((chr & 0x003F) | 0x80);

            return 3;
        }
        */

        public static void EncodeUtf8(Stream stream, string s)
        {
            foreach (char ch in s)
            {
                EncodeUtf8(stream, ch);
            }
        }

        public static void EncodeUtf8(Stream stream, char chr)
        {
            if (chr <= 0x7F)
            {
                stream.WriteByte((byte)(chr & 0x7F));
            }
            else if (chr <= 0x7FF)
            {
                stream.WriteByte((byte)(((chr & 0x07C0) >> 6) | 0xC0));
                stream.WriteByte((byte)((chr & 0x003F) | 0x80));
            }
            else
            {
                Debug.Assert(chr <= 0xFFFF);

                stream.WriteByte((byte)(((chr & 0xF000) >> 12) | 0xE0));
                stream.WriteByte((byte)(((chr & 0x0FC0) >> 6) | 0x80));
                stream.WriteByte((byte)((chr & 0x003F) | 0x80));
            }
        }

        public static char DecodeUtf8(Stream stream)
        {
            byte bt = (byte) stream.ReadByte();

            if ((bt & 0x80) == 0)
            {
                // 1 byte
                return (char)bt;
            }

            if ((bt & 0xE0) == 0xC0)
            {
                // 2 bytes
                int i2 = (int)(bt & 0x1F);
                i2 <<= 6;

                bt = (byte)stream.ReadByte();
                Debug.Assert((bt & 0xC0) == 0x80);

                i2 |= (bt & 0x3F);

                Debug.Assert(i2 <= 0x7FF);

                return (char)i2;
            }

            if ((bt & 0xF0) != 0xE0)
            {
                throw new BinHelperException($"Incorrect first byte of char (0x{bt:X02})");
            }
            
            // 3 bytes
            int i3 = (int) (bt & 0x0F);
            i3 <<= 6;

            bt = (byte)stream.ReadByte();
            Debug.Assert((bt & 0xC0) == 0x80);

            i3 |= (bt & 0x3F);
            i3 <<= 6;

            bt = (byte)stream.ReadByte();
            Debug.Assert((bt & 0xC0) == 0x80);

            i3 |= (bt & 0x3F);

            return (char) i3;
        }

        //////////////////////////////////////////////////////////////////////////////
        // SerializeUshort / DeserializeUshort
        //////////////////////////////////////////////////////////////////////////////
        // 
        // Bits 0 - 5 value
        // Bit  6 - flag bit.
        // Bit  7 - one more byte will follow.

        public const int MAX_USHORT_VALUE = 0x3FFF; // 16383
        private const uint MAX_USHORT_SINGLE_BYTE_VALUE = 0x3F; // 63
        private const int ONE_BYTE_SHIFT = 8;
        private const int MAX_BYTE_VALUE = 255;

        public static void SerializeUshort(IBinWrite wr, uint uiValue, bool bFlag)
        {
            Debug.Assert(uiValue <= MAX_USHORT_VALUE);

            uint uiFirst = (uint) (bFlag ? 0x40 : 0x00); // Set 6th bit if flag is true

            if (MAX_USHORT_SINGLE_BYTE_VALUE < uiValue)
            {
                // It is necessary two bytes to serialize uiValue
                uiFirst |= 0x80; // Set 7th bit

                uint iFirstAdd = uiValue >> ONE_BYTE_SHIFT;
                Debug.Assert(iFirstAdd <= MAX_USHORT_SINGLE_BYTE_VALUE);

                uiFirst += iFirstAdd;
                Debug.Assert(uiFirst <= MAX_BYTE_VALUE);

                wr.WriteByte((byte)uiFirst);

                uint uiSecond = uiValue & MAX_BYTE_VALUE;
                wr.WriteByte((byte) uiSecond);
            }
            else
            {
                // It is necessary single byte to serialize uiValue
                uiFirst |= uiValue;
                Debug.Assert(uiFirst <= MAX_BYTE_VALUE);

                wr.WriteByte((byte)uiFirst);
            }
        }

        public static uint DeserializeUshort(IBinRead br, out bool bFlag)
        {
            byte btFirst = br.ReadByte();
            bFlag = 0 < (btFirst & 0x40); // Set flag value
            uint uiFirstValue = btFirst & MAX_USHORT_SINGLE_BYTE_VALUE;

            if (0 < (btFirst & 0x80))
            {
                // 7th bit is set so there is one byte more
                byte btSecond = br.ReadByte();

                uint uiValue = (uiFirstValue << 8) + btSecond;

                return uiValue;
            }

            return uiFirstValue;
        }

        //////////////////////////////////////////////////////////////////////////////
        // SerializeShortInt / DeserializeShortInt
        //////////////////////////////////////////////////////////////////////////////

        public static void SerializeShortInt(IBinWrite wr, int iValue)
        {
            uint uiValue = (uint)Math.Abs(iValue);
            SerializeUshort(wr, uiValue, iValue < 0);
        }

        public static int DeserializeShortInt(IBinRead br)
        {
            uint uiValue = DeserializeUshort(br, out bool bIsNegative);

            if (bIsNegative)
            {
                return -(int)uiValue;
            }

            return (int)uiValue;
        }

        //////////////////////////////////////////////////////////////////////////////
        // SerializeUlong / DeserializeUlong
        //////////////////////////////////////////////////////////////////////////////

        private const byte ULONG_HEADER_MASK   = 0xF0;
        public const byte  MAX_FIRST_BYTE_MASK = 0x0F;
        private const int  ULONG_HEADER_SHIFT  = 4;
        public static void SerializeUlong (IBinWrite bw, ulong ulValue)
        {
            ulong ulShift = ulValue;

            int iScale = 0;

            while (MAX_FIRST_BYTE_MASK < ulShift)
            {
                byte bt = (byte) ulShift;
                bw.UlongBuffer[iScale ++] = bt;
                ulShift >>= ONE_BYTE_SHIFT;
            }

            byte btHeader = (byte)(iScale << ULONG_HEADER_SHIFT);
            btHeader |= (byte)ulShift;

            bw.WriteByte(btHeader);

            for (int i = iScale - 1; 0 <= i; i--)
            {
                bw.WriteByte(bw.UlongBuffer[i]);
            }
        }

        public static ulong DeserializeUlong(IBinRead br)
        {
            byte btFirst = br.ReadByte();

            int iScale = (btFirst & ULONG_HEADER_MASK) >> ULONG_HEADER_SHIFT;

            ulong ulShift = (byte) (btFirst & MAX_FIRST_BYTE_MASK);

            for (int i = 0; i < iScale; i ++)
            {
                byte bt = br.ReadByte();

                ulShift <<= ONE_BYTE_SHIFT;
                ulShift += bt;
            }

            return ulShift;
        }

        //////////////////////////////////////////////////////////////////////////////
        // Serializelong / Deserializelong
        //////////////////////////////////////////////////////////////////////////////

        public static void SerializeLong(IBinWrite bw, long lValue)
        {
            Debug.Assert(MIN_LONG_VALUE <= lValue);

            ulong ulValue = (ulong) Math.Abs(lValue);
            ulValue <<= 1;

            if (lValue < 0)
            {
                ulValue |= 1;
            }

            SerializeUlong(bw, ulValue);
        }

        public static long DeserializeLong(IBinRead br)
        {
            ulong ulValue = DeserializeUlong(br);

            bool bMinus = 0 < (ulValue & 1);
            ulValue >>= 1;

            if (bMinus)
            {
                return - (long) ulValue;
            }

            return (long) ulValue;
        }

        public static int GetHashCode(long l)
        {
            return ((int)l) ^ ((int)(l >> 32));
        }
    }
}
