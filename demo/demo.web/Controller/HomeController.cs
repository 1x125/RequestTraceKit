using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RequestTraceKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace demo.web.Controller
{
    [RequestTrace]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        // GET: HomeController
        [HttpGet(nameof(Index))]
        public IActionResult Index()
        {
            HttpContext.Items["ExtParam"] = "my extParam";//设置自定义统计参数值
            return Ok("ok");
        }
    }
}
