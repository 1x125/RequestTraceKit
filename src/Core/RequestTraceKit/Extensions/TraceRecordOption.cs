using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    /// <summary>
    /// 网站统计配置项
    /// </summary>
    public class TraceRecordOption : IOptions<TraceRecordOption>
    {
        public TraceRecordOption Value => this;

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool? Disable { get; set; } = false;

        /// <summary>
        /// 存储数据库
        /// </summary>
        public StorageDataBaseType StorageDataBase { get; set; } = StorageDataBaseType.MySql;

        /// <summary>
        /// 链接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// MongoDB 数据库名称
        /// </summary>
        public string DatabaseName { get; set; } = "Trace";


        /// <summary>
        /// 表或集合名称
        /// </summary>
        public TableNameConfig TableNames { get; set; } = new TableNameConfig();


        public QueueConfig QueueConfig { get; set; } = new QueueConfig();

        /// <summary>
        /// 需要记录的Cookie名称
        /// </summary>
        public string CookieName { get; set; } = "TraceDefault";

        /// <summary>
        /// 需要统计停留时间的URL地址
        /// </summary>
        public List<string> PageStayTimeRules { get; set; } = new List<string> { "/" };

        ///// <summary>
        ///// IP服务地址
        ///// </summary>
        //public string IPDataServerUrl { get; set; }
    }

    /// <summary>
    /// 记录表名配置
    /// </summary>
    public class TableNameConfig
    {
        /// <summary>
        /// 访问记录表名
        /// </summary>
        public string TraceTableName { get; set; } = "tracerecord";

        /// <summary>
        /// 停留时间记录表名
        /// </summary>
        public string PageStayTableName { get; set; } = "pagestayrecord";

        /// <summary>
        /// 埋点统计表名
        /// </summary>
        public string ElementClickTableName { get; set; } = "elementclickrecord";
    }

    /// <summary>
    /// 存储队列配置
    /// </summary>
    public class QueueConfig
    {
        /// <summary>
        /// 队列执行间隔秒数
        /// </summary>
        public int PollingIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// 队列容量
        /// </summary>
        public int QueueCapacity { get; set; } = 10240;

        /// <summary>
        /// 每次批量插入数量
        /// </summary>
        public int MaxBatchCount { get; set; } = 200;
    }

    /// <summary>
    /// 埋点统计配置
    /// </summary>
    public class ElementClickRule
    {
        /// <summary>
        /// 统计规则名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 需要统计的URL的相对地址
        /// </summary>
        public string UrlRule { get; set; }

        /// <summary>
        /// 统计规则（参照jquery选择器） 
        /// </summary>
        public List<string> AttrNames { get; set; }

        /// <summary>
        /// 需要获取值的html属性
        /// </summary>
        public List<string> PeekConfig { get; set; }
    }
}
