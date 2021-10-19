using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RequestTraceKit
{
    public class TaskPool
    {
        private static TaskPool s_TaskPool = new TaskPool();
        public static TaskPool Instance
        {
            get { return s_TaskPool; }
        }

        private class TaskOperator
        {
            public Action Flush { get; set; }

            public Action<object> Enqueue { get; set; }

            public DateTime NextExecuteTime { get; set; }

            public Func<IEnumerable> GetAllTasks { get; set; }

            public int PollingIntervalSeconds { get; set; }
        }

        private Dictionary<Type, TaskOperator> m_Operators;
        private Timer m_PollingTimer;

        private TaskPool()
        {
            m_Operators = new Dictionary<Type, TaskOperator>();
            m_PollingTimer = new Timer(new TimerCallback(Polling), null, 100, 100);
        }

        public TaskPool RegisterQueue<T>(Action<IEnumerable<T>> taskHandler, int pollingIntervalSeconds = 10, int queueCapacity = 10240, int maxBatchCount = 200) where T : class
        {
            Type t = typeof(T);
            TaskOperator op;
            if (m_Operators.TryGetValue(t, out op) == false)
            {
                op = new TaskOperator { NextExecuteTime = DateTime.Now.AddSeconds(-1), PollingIntervalSeconds = pollingIntervalSeconds };
                IQueue<T> queue = new LocalMemoryQueue<T>(queueCapacity);
                op.Enqueue = task => queue.Enqueue((T)task);
                op.GetAllTasks = () => queue.GetAllItems();
                op.Flush = () =>
                {
                    try
                    {
                        List<T> list = new List<T>(maxBatchCount);
                        do
                        {
                            T msg;
                            if (queue.Dequeue(out msg) < 0) // queue is empty.
                            {
                                if (list.Count > 0)
                                {
                                    taskHandler(list);
                                }
                                break;
                            }
                            list.Add(msg);
                            if (list.Count >= maxBatchCount)
                            {
                                taskHandler(list);
                                list.Clear();
                                Thread.Sleep(10);
                            }
                        } while (true);
                    }
                    catch (Exception ex)
                    {
                    }
                };
                m_Operators.Add(t, op);
            }
            return this;
        }

        public void Enqueue<T>(T task)
        {
            Type t = typeof(T);
            TaskOperator op;
            if (m_Operators.TryGetValue(t, out op) == false)
            {
                throw new ApplicationException("没有注册针对类型“" + t.FullName + "”的队列，请先调用TaskPool的实例方法RegisterQueue<" + t.Name + ">方法来进行注册。");
            }
            op.Enqueue(task);
        }


        public IList<T> GetUnprocessdTasks<T>()
        {
            Type t = typeof(T);
            TaskOperator op;
            if (m_Operators.TryGetValue(t, out op) == false)
            {
                throw new ApplicationException("没有注册针对类型“" + t.FullName + "”的队列，请先调用TaskPool的实例方法RegisterQueue<" + t.Name + ">方法来进行注册。");
            }
            return (IList<T>)op.GetAllTasks();
        }

        private void Polling(object state)
        {
            try
            {
                List<TaskOperator> list = new List<TaskOperator>(m_Operators.Values);
                foreach (var taskOperator in list)
                {
                    if (DateTime.Now >= taskOperator.NextExecuteTime)
                    {
                        taskOperator.Flush();
                        taskOperator.NextExecuteTime = taskOperator.NextExecuteTime.AddSeconds(taskOperator.PollingIntervalSeconds);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}
