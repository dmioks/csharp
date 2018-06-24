using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Binary
{
    public class ByteArray
    {
    public const int MAX_DISPLAY_COUNT = 32;
    public static readonly ByteArray EMPTY = new ByteArray();
    
    byte[] m_arr = null;
    
    private ByteArray()
    {
        m_arr = new byte[0];
    }
    
    public ByteArray(byte[] arr)
    {
        m_arr = arr;
    }
    
    public ByteArray(int[] arr)
    {
        m_arr = IntArrayToByteArray (arr);
    }

    public byte[] Bytes { get { return m_arr; } }
    public int Length { get { return m_arr.Length; } }
    
    public static byte[] IntArrayToByteArray (int[] arrInt)
    {
        Debug.Assert(arrInt != null);
        
        byte[] arrByte = new byte[arrInt.Length];
        
        for (int i = 0; i < arrInt.Length; i ++)
        {
            Debug.Assert(arrInt[i] < BinHelper.BYTE_OVERFLOW);
            arrByte[i] = (byte) arrInt[i];
        }
        
        return arrByte;
    }
    
    public long GetCheckSum () 
    {
        /*
        CRC32 crc32 = new CRC32();
        crc32.update(m_arr, 0, m_arr.length);
        
        return crc32.getValue();
        */

        return 0;
    }
    
    /*
    public int getCheckSum () throws IOException
    {
        int iCheckSum = 0;
        DataInputStream dis = this.getDataInputStream();
        
        int iRest = m_arr.length;
        
        while (iRest >= INTEGER_SIZE)
        {
            iCheckSum ^= dis.readInt();
            
            iRest -= INTEGER_SIZE;
        }
        
        while (iRest > 0)
        {
            int i = dis.readUnsignedByte() << iRest * 8;
            iCheckSum ^= i;
            
            iRest --;
        }
        
        return iCheckSum;
    }
    
    public void Serialize (IBinWrite wr) 
    {
        byte[] arrBytes = new byte[8];
        BinHelper.SerializeNum(wr, m_arr.Length, arrBytes);        
        wr.Write(m_arr);
    }
    
    public static ByteArray Deserialize (ByteBuffer rd)
    {
        int iSize = (int) BinHelper.DeserializeNum(rd);
        
        byte[] arr = new byte[iSize];
        rd.Read(arr, 0, iSize);
        
        ByteArray ba = new ByteArray (arr);
        
        return ba;
    }
    */

    public BinaryReader GetBinaryReader()
    {
        return new BinaryReader(new MemoryStream(m_arr));
    }

    public MemoryStream GetMemoryStream()
    {
        return new MemoryStream(m_arr);
    }
    
    public override bool Equals(object obj)
    {
        ByteArray ba = obj as ByteArray;
        
        if (ba != null && m_arr.Length == ba.m_arr.Length)
        {
            for (int i = 0; i < m_arr.Length; i ++)
            {
                if (m_arr[i] != ba.m_arr[i])
                {
                    return false;
                }
            }
            
            return true;
        }
        
        return false;
    }
    
    public override int GetHashCode ()
    {
        return m_arr.Length;
    }
    
    public override string ToString ()
    {
        List<String> l = new List<String>();

        int iLength = m_arr.Length;

        for (int i = 0; i < iLength && i < MAX_DISPLAY_COUNT; i++)
        {
            l.Add(m_arr[i].ToString("G"));
        }

        String sData = iLength > 0 ? string.Join(", ", l.ToArray()) : "None";
        String sMore = iLength > MAX_DISPLAY_COUNT ? String.Format(", ... more {0} byte(s)", m_arr.Length - MAX_DISPLAY_COUNT) : string.Empty;
        
        return string.Format("ByteArray {{Length = {0}, Data = {1}}}", m_arr.Length, sData + sMore);
    }
    }
}
