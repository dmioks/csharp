using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Dmioks.Common.Binary
{
    public static class BinHelper
    {
        public static int DEFAULT_BUFFER_SIZE = 4194304;
        public const int BYTE_NUM_MIN_VALUE = -128;
        public const int BYTE_NUM_MAX_VALUE = 127;
        public const int BYTE_OVERFLOW = 256;

        public static int ByteToInt(byte bt)
        {
            return BYTE_NUM_MAX_VALUE < bt ? ((int)bt) - BYTE_OVERFLOW : (int)bt;
        }

        public static byte IntToByte(int i)
        {
            Debug.Assert(BYTE_NUM_MIN_VALUE <= i && i <= BYTE_NUM_MAX_VALUE);
            return (byte)(i > BYTE_NUM_MAX_VALUE ? i - BYTE_OVERFLOW : i);
        }

        public static void WriteChar(BinaryWriter wr, char chValue)
        {
            byte bt = (byte)(chValue >> 8);
            wr.Write(bt);
            bt = (byte)chValue;
            wr.Write(bt);
        }

        public static char ReadChar(BinaryReader rd)
        {
            char ch = (char)rd.ReadByte();
            ch <<= 8;

            char chLow = (char)rd.ReadByte();
            ch |= chLow;

            return ch;
        }

        public static void SerializeNum(BinaryWriter wr, long lValue)
        {
            byte[] bytes = new byte[8];
            long lShift = Math.Abs(lValue);
            int iScale = 0;

            while (lShift > 0)
            {
                byte bt = (byte)lShift;
                bytes[iScale++] = bt;
                lShift >>= 8;
            }

            wr.Write(IntToByte((lValue < 0 ? -iScale : iScale)));

            for (int i = iScale - 1; i >= 0; i--)
            {
                wr.Write(bytes[i]);
                lShift >>= 8;
            }
        }

        public static long DeserializeNum(BinaryReader rd)
        {
            long lShift = 0;
            int iScale = ByteToInt(rd.ReadByte());

            for (int i = 0; i < Math.Abs(iScale); i++)
            {
                byte bt = rd.ReadByte();
                lShift <<= 8;
                lShift += bt;
            }

            return iScale < 0 ? -lShift : lShift;
        }

        public delegate void DelegateSerialize(BinaryWriter wr);

        public static ByteArray Serialize(DelegateSerialize ds)
        {
            Debug.Assert(ds != null);
            BinaryWriterSr bws = BinaryWriterPool.Instance.Get();

            try
            {
                ds(bws.BinaryWriter);

                return bws.ToByteArray();
            }
            catch (Exception excp)
            {
                throw excp;
            }
            finally
            {
                BinaryWriterPool.Instance.Put(bws);
            }
        }

        public static ByteArray Serialize(BinaryWriterSr bws, DelegateSerialize ds)
        {
            Debug.Assert(bws != null);
            Debug.Assert(ds != null);

            try
            {
                ds(bws.BinaryWriter);

                return bws.ToByteArray();
            }
            catch (Exception excp)
            {
                throw excp;
            }
            finally
            {
                bws.Reset();
            }
        }

        public static void SerializeStr(BinaryWriter wr, String sValue)
        {
            byte[] arr = Encoding.UTF8.GetBytes(sValue);
            BinHelper.SerializeNum(wr, arr.Length);
            wr.Write(arr, 0, arr.Length);
        }

        public static String DeserializeStr(BinaryReader rd)
        {
            int iSize = (int)BinHelper.DeserializeNum(rd);
            byte[] arr = new byte[iSize];
            rd.Read(arr, 0, iSize);

            return Encoding.UTF8.GetString(arr);
        }

        /*
        public static ByteArray Compress(ByteArray baDecompressed)
        {
            Debug.Assert(baDecompressed != null);

            DataOutputStreamSr doss = m_dosPool.get();
            GZIPOutputStream osGZip = new GZIPOutputStream(doss.getDataOutputStream());

            try
            {
                osGZip.write(baDecompressed.get(), 0, baDecompressed.length());
                osGZip.close();

                return doss.toByteArray();
            }
            catch (IOException excp)
            {
                String sError = ExcpHelper.getDetails(excp);

                System.out.println(sError);
            }
            finally
            {
                m_dosPool.put(doss);
            }

            return null;
        }
        */

        public static ByteArray Decompress(ByteArray baCompressed)
        {
            Debug.Assert(baCompressed != null);

            ByteBuffer btBuf = ByteBufferPool.Instance.Get();
            byte[] buffer = btBuf.GetBytes();

            BinaryWriterSr bws = BinaryWriterPool.Instance.Get();
            BinaryWriter wr = bws.BinaryWriter;

            try
            {
                using (GZipStream isGZip = new GZipStream(baCompressed.GetMemoryStream(), CompressionMode.Decompress))
                {
                    int iPos = 0;
                    int iRead = 0;

                    while ((iRead = isGZip.Read(buffer, iPos, DEFAULT_BUFFER_SIZE - iPos)) > 0)
                    {
                        wr.Write(buffer, iPos, iRead);
                        iPos += iRead;
                    }

                    ByteArray baDecompressed = bws.ToByteArray();

                    return baDecompressed;
                }
            }
            catch (IOException excp)
            {
                String sError = ExcpHelper.FormatException(excp, "BinaryHelper.Decompress() ERROR for {0}", baCompressed);

                throw new Exception(sError, excp);
            }
            finally
            {
                BinaryWriterPool.Instance.Put(bws);
                ByteBufferPool.Instance.Put(btBuf);
            }

            return null;
        }
    }
}
