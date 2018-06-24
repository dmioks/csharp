using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Binary
{
    public class NumberPacker
    {
        public static int MAX_LOW_VALUE = 1000000000;
        private static int MAX_INT_DEGREE = 9;
        private static int DECIMAL_BASE = 10;

        private static readonly Dictionary<int, int> m_di = new Dictionary<int, int>()
        {
            {0, 1},
            {1, 10},
            {2, 100},
            {3, 1000},
            {4, 10000},
            {5, 100000},
            {6, 1000000},
            {7, 10000000},
            {8, 100000000},
            {9, 1000000000},
        };

        private static int GetDegree(long lValue)
        {
            int iDegree = 0;

            while (lValue != 0)
            {
                lValue /= DECIMAL_BASE;
                iDegree += 1;
            }

            return iDegree;
        }

        public static long Pack(long lHigh, int iLow)
        {
            Debug.Assert(iLow < MAX_LOW_VALUE);
            Debug.Assert((lHigh * iLow * DECIMAL_BASE) < long.MaxValue);

            int iDegree = GetDegree(iLow);
            Debug.Assert(iDegree <= MAX_INT_DEGREE);

            long lFactor = m_di[iDegree];
            long lResult = lHigh * lFactor + iLow;

            return lResult * DECIMAL_BASE + iDegree;
        }

        public static long Unpack(long lValue, ref int iLow)
        {
            int iDegree = (int)(lValue % DECIMAL_BASE);
            int iFactor = m_di[iDegree];

            long lResult = lValue / DECIMAL_BASE;

            iLow = (int)(lResult % iFactor);

            return lResult / iFactor;
        }
    }
}
