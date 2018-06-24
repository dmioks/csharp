using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmioks.Common.Binary;

namespace Dmioks.Common.Utils
{
    public sealed class TimeStamp
    {
        public static readonly TimeStamp DEFAULT = new TimeStamp(0L);
        public const long SECOND_MILLISECONDS = 1000;
        public const long MINUTE_MILLISECONDS = SECOND_MILLISECONDS * 60;
        public const long HOUR_MILLISECONDS = MINUTE_MILLISECONDS * 60;
        public const long DAY_MILLISECONDS = HOUR_MILLISECONDS * 24;

        public const int TICKS_IN_MILLISECOND = 10000;
        public static readonly DateTime DATETIME_BASE_UTC = new DateTime(1970, 01, 01, 0, 0, 0, 0, System.DateTimeKind.Utc);

        public static readonly TimeStamp TIMESTAMP_BASE_UTC = new TimeStamp(0);
        public static readonly long DATETIME_BASE_UTC_MILLISECONDS = DATETIME_BASE_UTC.Ticks / TICKS_IN_MILLISECOND;

        public static readonly TimeZoneInfo LOCAL_TIMEZONE = TimeZoneInfo.Local;
        //public static readonly TimeSpan UTC_OFFSET = LOCAL_TIMEZONE.GetUtcOffset();

        private long m_lTimeStamp = 0L;

        public TimeStamp(long lTimeStamp)
        {
            m_lTimeStamp = lTimeStamp;
        }

        public long TimeStampValue
        {
            get
            {
                return m_lTimeStamp;
            }
        }

        public DateTime UtcDateTime()
        {
            return ToUtcDateTime(this);
        }

        public DateTime LocalDateTime()
        {
            return ToLocalDateTime(this);
        }

        public static TimeStamp UtcNow()
        {
            DateTime dtUtcNow = DateTime.UtcNow;
            return UtcToTimeStamp(dtUtcNow);
        }

        public static TimeStamp FromLocalTime(String sLocalTime)
        {
            DateTime dtLocal = DateTime.Parse(sLocalTime);

            return LocalToTimeStamp(dtLocal);
        }

        public static TimeStamp FromLocalTime(DateTime dtLocal)
        {
            return LocalToTimeStamp(dtLocal);
        }

        public static TimeStamp FromUtcTime(String sUtcTime)
        {
            DateTime dtUtc = DateTime.Parse(sUtcTime);

            return UtcToTimeStamp(dtUtc);
        }

        public static DateTime ToUtcDateTime(TimeStamp ts)
        {
            return new DateTime((DATETIME_BASE_UTC_MILLISECONDS + ts.m_lTimeStamp) * TICKS_IN_MILLISECOND);
        }

        public static DateTime ToLocalDateTime(TimeStamp ts)
        {
            DateTime dtUtc = ToUtcDateTime(ts);
            return dtUtc.ToLocalTime();
        }

        public static TimeStamp LocalToTimeStamp(DateTime dtLocal)
        {
            DateTime dtUtc = dtLocal.ToUniversalTime();
            return new TimeStamp(DateTimeToLong(dtUtc));
        }

        public static long DateTimeToLong(DateTime dt)
        {
            return dt.Ticks / TICKS_IN_MILLISECOND - DATETIME_BASE_UTC_MILLISECONDS;
        }

        public static DateTime LongToDateTime(long lDateTime)
        {
            return new DateTime((DATETIME_BASE_UTC_MILLISECONDS + lDateTime) * TICKS_IN_MILLISECOND);
        }

        public static TimeStamp UtcToTimeStamp(DateTime dtUtc)
        {
            return new TimeStamp(DateTimeToLong(dtUtc));
        }

        public void Serialize(IBinWrite wr)
        {
            BinHelper.SerializeUlong(wr, (ulong) m_lTimeStamp);
        }

        public static long GetUnixTimestamp()
        {
            TimeSpan tsNow = DateTime.UtcNow - DATETIME_BASE_UTC;

            return (long)tsNow.TotalMilliseconds;
        }

        public static TimeStamp Deserialize(IBinRead br)
        {
            long lTimeStamp = (long) BinHelper.DeserializeUlong(br);
            return new TimeStamp(lTimeStamp);
        }

        public static string FormatUtc (DateTime dtUtc)
        {
            Debug.Assert(dtUtc != null);

            return dtUtc.ToString("dd-MMM-yyyy HH:mm:ss.fff UTC");
        }

        public override int GetHashCode()
        {
            return m_lTimeStamp.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            TimeStamp ts = obj as TimeStamp;

            if (ts != null)
            {
                return m_lTimeStamp == ts.m_lTimeStamp;
            }

            return false;
        }

        public override String ToString()
        {
            DateTime dt = this.LocalDateTime();
            return dt.ToString();
        }
    }
}
