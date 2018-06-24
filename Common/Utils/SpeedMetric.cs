using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmioks.Common.Utils
{
    public class SpeedMetric
    {
        public const int DEFAULT_MAX_MESSAGES_CALCULATE_COUNT = 20;
        public readonly int MaxMessagesCalculateCount;

        protected long m_lStartCalcTime = TimeStamp.UtcNow().TimeStampValue;
        protected long m_lCalcBytesCount = 0;

        protected long m_lTotalMessages = 0;

        protected double m_dbSpeed = 0.0f;

        protected Queue<SpeedMetricRecord> m_queue = new Queue<SpeedMetricRecord>();
        protected object m_objLocker = new object ();

        public SpeedMetric (int iMaxMessagesCalculateCount)
        {
            this.MaxMessagesCalculateCount = iMaxMessagesCalculateCount;
        }

        public SpeedMetric() : this (DEFAULT_MAX_MESSAGES_CALCULATE_COUNT)
        {
        }

        public void Add(int iBytes)
        {
            this.Add(TimeStamp.UtcNow().TimeStampValue, iBytes);
        }

        public void Add (long lUnixTime, int iBytes)
        {
            SpeedMetricRecord smrNew = new SpeedMetricRecord(lUnixTime, iBytes);

            m_lCalcBytesCount += iBytes;
            m_queue.Enqueue(smrNew);

            while (m_queue.Count > this.MaxMessagesCalculateCount)
            {
                SpeedMetricRecord smrOld = m_queue.Dequeue();
                m_lStartCalcTime = smrOld.UnixTime;
                m_lCalcBytesCount -= smrOld.BytesCount;
            }

            // calculate
            lock (m_objLocker){
                m_dbSpeed = ((double)m_lCalcBytesCount) / ((double)(smrNew.UnixTime - m_lStartCalcTime)) * 1000.0f;
                /*
                if (m_dbSpeed < 0)
                {
                    int i = 0;
                }
                */
            }

            m_lTotalMessages ++;
        }

        public double Speed
        {
            get
            {
                lock (m_objLocker)
                {
                    return m_dbSpeed;
                }
            }
        }

        public override string ToString()
        {
            return string.Format("Speed {0} bytes/sec", (int)this.Speed);
        }
    }

    public class SpeedMetricRecord
    {
        public readonly long UnixTime;
        public readonly int BytesCount;

        public SpeedMetricRecord(long lUnixTime, int iBytesCount)
        {
            this.UnixTime = lUnixTime;
            this.BytesCount = iBytesCount;
        }

        /*
        public SpeedMetricRecord (int iBytesCount) : this (TimeStamp.UtcNow().TimeStampValue, iBytesCount)
        {
        }
        */
    }

    public class SpeedMetricPair
    {
        public readonly SpeedMetric ActualSpeedMetric = new SpeedMetric();
        public readonly SpeedMetric NeededSpeedMetric = new SpeedMetric();
    }
}


