using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Utils;

namespace Dmioks.Common.Binary
{
    public sealed class BinaryWriterPool : ObjectPool<BinaryWriterImp>
    {
        private const int DEFAULT_SIZE = 4194304;

        private BinaryWriterPool()
        {
        }

        private static BinaryWriterPool m_instance = new BinaryWriterPool();

        public static BinaryWriterPool Instance
        {
            get
            {
                return m_instance;
            }
        }

        protected override BinaryWriterImp CreateNew()
        {
            return BinaryWriterImp.create(DEFAULT_SIZE);
        }

        protected override void OnPut(BinaryWriterImp bwsr)
        {
            bwsr.Reset();
        }
    }
}
