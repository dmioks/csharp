using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public class ByteBufferPool : ObjectPool<ByteBuffer>
    {
        private const int DEFAULT_SIZE = 1024;

        private ByteBufferPool()
        {
        }

        private static ByteBufferPool m_instance = new ByteBufferPool();

        public static ByteBufferPool Instance
        {
            get
            {
                return m_instance;
            }
        }

        protected override ByteBuffer CreateNew()
        {
            return new ByteBuffer(DEFAULT_SIZE);
        }

        protected override void OnPut(ByteBuffer bb)
        {
            bb.Reset();
        }
    }
}
