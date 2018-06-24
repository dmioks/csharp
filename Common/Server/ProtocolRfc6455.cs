using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Server
{
    public static class ProtocolRfc6455
    {
        private const int TWO_BYTE_LENGTH = 126;

        enum eMessageType
        {
            Text = 1,
        }

        public static object ReadMessage(IBinRead br)
        {
            byte btFirst = br.ReadByte();

            bool bIsLastFrame = 0 < (btFirst & 0x80);
            int iType = btFirst & 0x0F;

            byte btSecond = br.ReadByte();
            bool bMasked = 0 < (btSecond & 0x80);

            int iLength = btSecond & 0x7F;

            if (TWO_BYTE_LENGTH == iLength)
            {
                // Two bytes more   
                iLength = br.ReadByte();
                iLength <<= 8;
                iLength |= br.ReadByte();
            }
            else if (TWO_BYTE_LENGTH < iLength)
            {
                // Eight bytes more
                iLength = br.ReadByte();
            }

            byte[] arrKey = null;

            if (bMasked)
            {
                arrKey = new byte[4];

                for (int i = 0; i < 4; i++)
                {
                    arrKey[i] = br.ReadByte();
                }
            }

            StringBuilder sb = StringBuilderPool.Instance.Get();

            try
            {
                for (int i = 0; i < iLength; i++)
                {
                    byte bt = br.ReadByte();
                    int iMaskIndex = i % 4;

                    bt = (byte)(bt ^ arrKey[iMaskIndex]);
                    char ch = (char)bt;

                    sb.Append(ch);
                }

                return sb.ToString();
            }
            finally
            {
                StringBuilderPool.Instance.Put(sb);
            }
        }

        public static void WriteMessage(IBinWrite bw, string sMessage, byte[] arrMaskKey = null, bool bIsLastFrame = true)
        {
            byte[] arrBytes = Encoding.UTF8.GetBytes(sMessage);
            int iBodyLength = arrBytes.Length;
            Debug.Assert(iBodyLength < TWO_BYTE_LENGTH);

            int iType = (int)eMessageType.Text;

            byte btFirst = (byte)(bIsLastFrame ? 0x80 | iType : iType);
            bw.WriteByte(btFirst);

            byte btSecond = (byte)(arrMaskKey != null ? 0x80 | iBodyLength : iBodyLength);
            bw.WriteByte(btSecond);

            for (int i = 0; i < iBodyLength; i++)
            {
                bw.WriteByte(arrBytes[i]);
            }
        }
    }
}
