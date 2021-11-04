using Microsoft.AspNetCore.Http;
using RequestTraceKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demo.web.Behavior
{
    public class MyTraceBehavior : ITraceBehavior
    {
        /// <summary>
        /// 检测是否需要统计
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public bool CheckNeedTrace(HttpContext httpContext)
        {
            return true;
        }

        /// <summary>
        /// 获得客户端类型
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public string DetectRequestClientType(HttpContext httpContext)
        {
            string clientType = "Customer";
            return clientType;
        }

        /// <summary>
        /// 自定义设置扩展参数 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="msg"></param>
        public void SetExtentionData(HttpContext httpContext, RequestTraceRecord msg)
        {
            msg.ExtentionData.Add("ExtParam", httpContext.Items["ExtParam"]?.ToString());//ExtParam对应扩展字段名称
        }
    }
}
