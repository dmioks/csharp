using System;
using System.Diagnostics;

namespace Dmioks.Common.Collections
{
    public class QueueArray<T>
    {
        private readonly T[] m_arrItems;

        private int m_iTakeIndex  = 0;
        private int m_iPutIndex   = 0;
        private int m_iCount      = 0;

        public QueueArray(int iCapacity)
        {
            m_arrItems = new T[iCapacity];
        }

        public int Count
        {
            get
            {
                return m_iCount;
            }
        }

        public void Enqueue(T obj)
        {
            m_arrItems[m_iPutIndex] = obj;

            if (++ m_iPutIndex == m_arrItems.Length)
            {
                m_iPutIndex = 0;
            }

            m_iCount++;
        }

        public bool IsFull()
        {
            return m_arrItems.Length == m_iCount;
        }

        public T First
        {
            get
            {
                Debug.Assert(0 < m_iCount);
                return m_arrItems[m_iTakeIndex];
            }
        }

        public T Dequeue()
        {
            T obj = m_arrItems[m_iTakeIndex];

            m_arrItems[m_iTakeIndex] = default(T);

            if (++ m_iTakeIndex == m_arrItems.Length)
            {
                m_iTakeIndex = 0;
            }

            m_iCount --;

            return obj;
        }
    }
}