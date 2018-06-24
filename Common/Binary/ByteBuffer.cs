using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Binary
{
    public class ByteBuffer
    {
        protected readonly byte[] m_bytes = null;
        protected int m_iReadPosition;
        protected int m_iDataLength;

        public ByteBuffer(int iSize)
        {
            m_bytes = new byte[iSize];
        }

        public ByteBuffer(byte[] arrBytes, int iDataLength)
        {
            m_bytes = arrBytes;
            m_iDataLength = iDataLength;
        }

        public int ReadPosition
        {
            get { return m_iReadPosition; }
        }

        public void Reset()
        {
            m_iReadPosition = 0;
            m_iDataLength = 0;
        }

        public void SetDataLength(int iDataLength)
        {
            m_iDataLength = iDataLength;
        }

        public byte[] GetBytes()
        {
            return m_bytes;
        }

        public bool CanRead()
        {
            return m_iReadPosition < m_iDataLength;
        }

        public byte ReadByte()
        {
            byte b = m_bytes[m_iReadPosition];
            m_iReadPosition++;

            return b;
        }
    }
}
