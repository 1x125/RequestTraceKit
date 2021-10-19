using System;
using System.Collections.Generic;
using System.Text;

namespace RequestTraceKit
{
    public class ClickConfigEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// 站点ID
        /// </summary>
        public int SiteId { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 应用页面
        /// </summary>
        public string UrlRule { get; set; }

        /// <summary>
        /// 元素选择器
        /// </summary>
        public string AttrNames { get; set; }

        /// <summary>
        /// 需要记录的元素属性值
        /// </summary>
        public string PeekConfig { get; set; }
    }
}
