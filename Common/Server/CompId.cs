using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Server
{
    /*
    public class CompId
    {
        protected const char CHAR_ID_SEPARATOR = '-';
        protected static readonly char[] SPLIT_CHARS = new char[] { CHAR_ID_SEPARATOR };

        protected readonly long m_lFirstId  = 0L;
        protected readonly long m_lSecondId = 0L;
        protected readonly int  m_iHashCode = 0;

        public CompId(long lFirstId, long lSecondId)
        {
            m_lFirstId  = lFirstId;
            m_lSecondId = lSecondId;
            m_iHashCode = m_lFirstId.GetHashCode() ^ m_lSecondId.GetHashCode();
        }

        public long FirstId { get { return m_lFirstId; } }
        public long SecondId { get { return m_lSecondId; } }

        public override int GetHashCode()
        {
            return m_iHashCode;
        }

        public static CompId FromString (string sId)
        {
            if (!string.IsNullOrEmpty(sId) && 0 < sId.IndexOf(CHAR_ID_SEPARATOR))
            {
                string[] arr = sId.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                Debug.Assert(arr.Length == 2);

                if (!long.TryParse(arr[0], out long l1))
                {
                    throw new Exception($"FromString({sId}) ERROR. First part ({arr[0]}) invalid format.");
                }

                if (!long.TryParse(arr[1], out long l2))
                {
                    throw new Exception($"FromString({sId}) ERROR. Second part ({arr[1]}) invalid format.");
                }

                return new CompId(l1, l2);
            }

            throw new Exception($"FromString({sId}) ERROR. Param either null or invalid format.");
        }

        public override bool Equals (Object obj)
        {
            CompId id = obj as CompId;

            return id != null ? m_lFirstId == id.m_lFirstId && m_lSecondId == id.m_lSecondId : false;
        }

        public static bool operator == (CompId srid1, CompId srid2)
        {
            if (System.Object.ReferenceEquals(srid1, srid2))
            {
                return true;
            }

            if (((object)srid1) != null)
            {
                return srid1.Equals(srid2);
            }

            return false;
        }

        public static bool operator != (CompId srid1, CompId srid2)
        {
            return !(srid1 == srid2);
        }

        public string ToJsonKey()
        {
            return string.Concat(m_lFirstId, CHAR_ID_SEPARATOR, m_lSecondId);
        }

        public override string ToString()
        {
            return $"CompId{{First={m_lFirstId}, Second={m_lSecondId}}}";
        }
    }
    */
}
