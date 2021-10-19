using System;
using System.Collections.Generic;
using System.Text;

namespace RequestTraceKit
{
    /// <summary>
    /// 先进先出的队列存储结构
    /// </summary>
    public interface IQueue<T>
    {
        /// <summary>
        /// 将数据放入队列的尾部
        /// </summary>
        /// <param name="msg">待放入队列的数据</param>
        void Enqueue(T msg);

        /// <summary>
        /// 从队列头部取出数据元素（该数据元素为当前队列元素中最先进入的队列的）；该方法线程安全，支持多线程并发操作
        /// </summary>
        /// <param name="msg">从队列头部所取出的数据，只有当方法返回值为true时才有意义</param>
        /// <returns>指示本次取出的数据在队列中的索引，如果队列为空，则返回-1，表示本次操作没有取到数据</returns>
        long Dequeue(out T msg);

        /// <summary>
        /// 获取队列中所有数据，但不出队列
        /// </summary>
        /// <returns></returns>
        IList<T> GetAllItems();
    }
}
