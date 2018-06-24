using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Utils
{
    public class StringBuilderPool : ObjectPool<StringBuilder>
    {
        private const int DEFAULT_SIZE = 65536;

        private StringBuilderPool()
        {
        }

        private static StringBuilderPool m_instance = new StringBuilderPool();

        public static StringBuilderPool Instance
        {
            get
            {
                return m_instance;
            }
        }

        protected override StringBuilder CreateNew()
        {
            return new StringBuilder(DEFAULT_SIZE);
        }

        protected override void OnPut(StringBuilder sb)
        {
            sb.Clear();
        }
    }
}
