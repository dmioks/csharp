using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Binary
{
    public class BinaryWriterImp
    {
        protected MemoryStream m_ms = null;
        protected BinaryWriter m_wr = null;

        protected BinaryWriterImp()
        {
            
        }

        protected void initialize(int iSize)
        {
            m_ms = new MemoryStream(iSize);
            m_wr = new BinaryWriter(m_ms);
        }

        public static BinaryWriterImp create(int iSize)
        {
            BinaryWriterImp bwsr = new BinaryWriterImp();
            bwsr.initialize(iSize);

            return bwsr;
        }

        public MemoryStream MemoryStream
        {
            get { return m_ms; }
        }

        public BinaryWriter BinaryWriter
        {
            get { return m_wr; }
        }

        public void Reset()
        {
            m_ms.SetLength(0);
        }

        public byte[] GetBytes()
        {
            return m_ms.ToArray();
        }

        public ByteArray ToByteArray()
        {
            return new ByteArray(m_ms.ToArray());
        }
    }
}
