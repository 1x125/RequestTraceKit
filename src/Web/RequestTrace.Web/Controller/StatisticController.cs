using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RequestTraceKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RequestTrace.Web.Controller
{
    [Route("basestatistic")]
    public class StatisticController : BaseStatisticController
    {
        public StatisticController(IOptions<TraceRecordOption> options, ICommQuery commonQuery) : base(options, commonQuery)
        {
        }
    }
}
