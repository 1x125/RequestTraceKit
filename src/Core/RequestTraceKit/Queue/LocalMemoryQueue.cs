using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RequestTraceKit
{
    /// <summary>
    /// 基于本地内存的队列存储结构（先进先出）
    /// </summary>
    /// <typeparam name="T">队列中元素的类型</typeparam>
    public class LocalMemoryQueue<T> : IQueue<T>
    {
        private T[] m_MessageList;
        private byte[] m_UnreadFlagList;
        private long m_EnqueueIndex = -1;
        private long m_DequeueIndex = -1;

        /// <summary>
        /// 初始化一个<c>LocalMemoryQueue</c>类型的实例。
        /// </summary>
        /// <param name="boundedCapacity">指定队列可存储的元素的最大长度；
        /// 如果队列元素的数量超过了该指定长度，那么新入队列的数据将覆盖掉队列中最先进入的数据元素</param>
        public LocalMemoryQueue(int boundedCapacity)
        {
            m_MessageList = new T[boundedCapacity];
            m_UnreadFlagList = new byte[boundedCapacity];
        }

        /// <summary>
        /// 将数据放入队列的尾部；该方法线程安全，支持多线程并发操作
        /// </summary>
        /// <param name="msg">待放入队列的数据</param>
        public void Enqueue(T msg)
        {
            long initialValue;
            long computedValue;
            do
            {
                // Save the current running 's_Index' in a local variable.
                initialValue = m_EnqueueIndex;

                // compute the new value and stored it into the local variable 'computedValue'.
                computedValue = initialValue + 1;
                if (computedValue >= m_MessageList.Length)
                {
                    computedValue = 0;
                }

                // CompareExchange compares 's_Index' to 'initialValue'. If
                // they are not equal, then another thread has updated the
                // running 's_Index' since this loop started. CompareExchange
                // does not update 's_Index'. CompareExchange returns the
                // contents of 's_Index', which do not equal 'initialValue',
                // so the loop executes again.
            }
            while (initialValue != Interlocked.CompareExchange(ref m_EnqueueIndex, computedValue, initialValue));
            // If no other thread updated the running 's_Index', then 
            // 's_Index' and 'initialValue' are equal when CompareExchange
            // compares them, and 'computedValue' is stored in 's_Index'.
            // CompareExchange returns the value that was in 's_Index'
            // before the update, which is equal to initialValue, so the 
            // loop ends.
            m_MessageList[computedValue] = msg;
            m_UnreadFlagList[computedValue] = (byte)1;
        }

        /// <summary>
        /// 从队列头部取出数据元素（该数据元素为当前队列元素中最先进入的队列的）；该方法线程安全，支持多线程并发操作
        /// </summary>
        /// <param name="msg">从队列头部所取出的数据，只有当方法返回值为true时才有意义</param>
        /// <returns>指示本次取出的数据在队列中的索引，如果队列为空，则返回-1，表示本次操作没有取到数据</returns>
        public long Dequeue(out T msg)
        {
            long initialValue;
            long computedValue;
            do
            {
                initialValue = m_DequeueIndex;
                computedValue = initialValue + 1;
                if (computedValue >= m_MessageList.Length)
                {
                    computedValue = 0;
                }
                // if the element at computed index has no unread data,
                // and the 'm_DequeueIndex' is not changed (equal to 'initialValue'),
                // then the queue is empty.
                if (m_UnreadFlagList[computedValue] == (byte)0
                    && initialValue == Interlocked.CompareExchange(ref m_DequeueIndex, initialValue, initialValue))
                {
                    msg = default(T);
                    return -1;
                }
            }
            while (initialValue != Interlocked.CompareExchange(ref m_DequeueIndex, computedValue, initialValue));
            byte tmp = m_UnreadFlagList[computedValue];
            m_UnreadFlagList[computedValue] = 0;
            msg = m_MessageList[computedValue];
            if (tmp == (byte)1)
            {
                return computedValue;
            }
            return -1;
        }

        /// <summary>
        /// 当前队列中数据元素的个数；该属性不是线程安全
        /// </summary>
        public int Count
        {
            get
            {
                int total = 0;
                for (int i = 0; i < m_UnreadFlagList.Length; i++)
                {
                    total += (int)m_UnreadFlagList[i];
                }
                return total;
            }
        }

        /// <summary>
        /// 当前队列可存储的元素的最大长度；该属性是线程安全的
        /// </summary>
        public int Capacity
        {
            get { return m_UnreadFlagList.Length; }
        }

        public IList<T> GetAllItems()
        {
            List<T> list = new List<T>(m_UnreadFlagList.Length);
            byte[] tmp = new byte[m_UnreadFlagList.Length];
            Array.Copy(m_UnreadFlagList, tmp, m_UnreadFlagList.Length);
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] == (byte)1)
                {
                    list.Add(m_MessageList[i]);
                }
            }
            return list;
        }
    }
}
