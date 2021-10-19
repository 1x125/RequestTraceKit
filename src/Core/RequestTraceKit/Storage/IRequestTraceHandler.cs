using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public interface IRequestTraceHandler
    {
        /// <summary>
        /// 请求记录写入
        /// </summary>
        /// <param name="traces"></param>
        void BulkSaveTrace(IEnumerable<RequestTraceRecord> traces);

        /// <summary>
        /// 页面停留时间写入
        /// </summary>
        /// <param name="traces"></param>
        void BulkSaveTrace(IEnumerable<PageStayRecord> traces);

        /// <summary>
        /// 埋点统计写入
        /// </summary>
        /// <param name="traces"></param>
        void BulkSaveTrace(IEnumerable<ElementClickRecord> traces);
    }
}
