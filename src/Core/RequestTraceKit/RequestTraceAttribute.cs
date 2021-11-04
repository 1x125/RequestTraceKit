using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RequestTraceKit
{
    public class RequestTraceAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var traceTraker = context.HttpContext.RequestServices.GetService<ITraceBehavior>();
            var traceOption = context.HttpContext.RequestServices.GetService<IOptions<TraceRecordOption>>();
            if (traceTraker.CheckNeedTrace(context.HttpContext) == false)
            {
                return;
            }
            TraceInfo info = new TraceInfo(traceOption);
            context.HttpContext.Items["RequestTrace"] = info;
            info.Start();
        }


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var traceTraker = context.HttpContext.RequestServices.GetService<ITraceBehavior>();
            if (traceTraker.CheckNeedTrace(context.HttpContext) == false)
            {
                return;
            }
            TraceInfo info = context.HttpContext.Items["RequestTrace"] as TraceInfo;
            if (info != null)
            {
                info.ActionEnd(context.Exception != null);
            }
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var traceTraker = context.HttpContext.RequestServices.GetService<ITraceBehavior>();
            if (traceTraker.CheckNeedTrace(context.HttpContext) == false)
            {
                return;
            }
            TraceInfo info = context.HttpContext.Items["RequestTrace"] as TraceInfo;
            if (info != null)
            {
                info.ViewBegin();
            }
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var traceTraker = context.HttpContext.RequestServices.GetService<ITraceBehavior>();
            //if (context.HttpContext.Response.StatusCode)
            //{
            //    return;
            //}
            if (traceTraker.CheckNeedTrace(context.HttpContext) == false)
            {
                return;
            }
            TraceInfo info = context.HttpContext.Items["RequestTrace"] as TraceInfo;
            if (info != null)
            {
                info.Stop(context.Exception != null);
                string urlReferrer = context.HttpContext.Request.Headers["Referer"];
                string url = context.HttpContext.Request.GetDisplayUrl();
                string clientType = traceTraker.DetectRequestClientType(context.HttpContext);
                if (string.IsNullOrWhiteSpace(clientType))
                {
                    clientType = "OT";
                }

                RequestTraceRecord msg = info.ToMessage(context.HttpContext, url, urlReferrer, clientType);
                traceTraker.SetExtentionData(context.HttpContext, msg);
                TaskPool.Instance.Enqueue(msg);
            }
        }
    }
}
