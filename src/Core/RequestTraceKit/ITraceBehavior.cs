using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public interface ITraceBehavior
    {
        bool CheckNeedTrace(HttpContext httpContext);


        void SetExtentionData(HttpContext httpContext, RequestTraceRecord msg);

        string DetectRequestClientType(HttpContext httpContext);
    }
}
