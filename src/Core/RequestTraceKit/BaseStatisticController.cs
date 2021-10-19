using AngleSharp.Common;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class BaseStatisticController : ControllerBase
    {
        private IOptions<TraceRecordOption> _options;
        private readonly ICommQuery _commonQuery;

        public BaseStatisticController(IOptions<TraceRecordOption> options, ICommQuery commonQuery)
        {
            _options = options;
            _commonQuery = commonQuery;
        }

        #region virtual method

        /// <summary>
        /// 检测是否需要统计
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual bool CheckNeedTrace()
        {
            return _options.Value.Disable == false;
        }

        /// <summary>
        /// 获得用户ID
        /// </summary>
        /// <returns></returns>
        protected virtual string GetUserId()
        {
            return HttpContext.Items["UserId"]?.ToString();
        }

        /// <summary>
        /// 设置访问记录的额外参数
        /// </summary>
        /// <param name="msg"></param>
        protected virtual void SetTraceExtentionData(RequestTraceRecord msg, Dictionary<string, string> extentionDic)
        {

        }

        #endregion

        #region set record


        protected async Task SetTraceRecord()
        {
            var content = await HttpContext.Request.Body.ReadAsStringAsync();
            var pageTrace = JsonConvert.DeserializeObject<PageTraceEntity>(content);
            var tracker = new TraceBehavior(_options);
            var info = new TraceInfo(_options);
            string urlReferrer = pageTrace.FromUrl;
            string url = pageTrace.ToUrl;
            string clientType = tracker.DetectRequestClientType(HttpContext);
            if (string.IsNullOrWhiteSpace(clientType))
            {
                clientType = "OT";
            }

            RequestTraceRecord msg = info.ToMessage(HttpContext, url, urlReferrer, clientType);
            msg.TraceUid = pageTrace.TraceId;
            msg.SiteId = pageTrace.SiteId;
            msg.UserId = GetUserId();
            SetTraceExtentionData(msg, pageTrace.ExtentionData);
            TaskPool.Instance.Enqueue(msg);
        }

        protected async Task SetPageStayTime()
        {
            var now = DateTime.Now;
            var content = await HttpContext.Request.Body.ReadAsStringAsync();
            var pageStay = JsonConvert.DeserializeObject<PageStayEntity>(content);
            var pageStayRules = _options.Value.PageStayTimeRules;
            var pageRule =
                pageStayRules.FirstOrDefault(rule => pageStay.Url.Contains(rule) || new Regex(rule).IsMatch(pageStay.Url));
            if (!string.IsNullOrEmpty(pageRule))
            {
                IContext cxt = ContextManager.Current;
                var userAgent = HttpContext.Request.Headers["User-Agent"];
                var stayRecord = new PageStayRecord
                {
                    ClientIP = cxt.ClientIP,
                    UserAgent = userAgent,
                    PageUrl = pageStay.Url,
                    UserId = GetUserId(),
                    RuleName = pageRule,
                    LeaveTime = now,
                    StayTimes = pageStay.Time,
                    EnterTime = now.AddSeconds(-pageStay.Time),
                    TraceId = pageStay.TraceId
                };
                TaskPool.Instance.Enqueue(stayRecord);
            }
        }

        protected async Task SetElementClick(int siteId)
        {
            var content = await HttpContext.Request.Body.ReadAsStringAsync();
            var eleEntity = JsonConvert.DeserializeObject<ElementClickEntity>(content);
            var parser = new HtmlParser();
            var document = await parser.ParseDocumentAsync(eleEntity.Element);
            var el = document.Body.FirstElementChild;
            //var elName = el.GetAttribute("name");
            var configList = _commonQuery.GetClickConfigList(siteId, "");
            var clickRules = configList.Where(
                rule => (eleEntity.Url.Contains(rule.UrlRule) || new Regex(rule.UrlRule).IsMatch(eleEntity.Url))
            );

            if (clickRules.Count() <= 0)
                return;

            var peekConfigList = new List<string>();
            var ruleNameList = new List<string>();
            foreach (var rule in clickRules)
            {
                peekConfigList = peekConfigList.Concat(rule.PeekConfig).ToList();
                ruleNameList.Add(rule.Name);
            }
            IContext cxt = ContextManager.Current;
            var userAgent = HttpContext.Request.Headers["User-Agent"];
            var clickRecord = new ElementClickRecord
            {
                AttrId = el.Id,
                AttrName = el.GetAttribute("name"),
                ClassName = el.ClassName,
                ClientIP = cxt.ClientIP,
                CreateTime = DateTime.Now,
                PeekValue = GetPeekValue(peekConfigList, el),
                RequestUrl = eleEntity.Url,
                RuleName = string.Join('&', ruleNameList),
                UserAgent = userAgent,
                UserId = GetUserId()
            };
            TaskPool.Instance.Enqueue(clickRecord);
        }

        #endregion

        #region Action
        [HttpPost("stay")]
        public async Task<IActionResult> StayTime()
        {
            if (!CheckNeedTrace())
            {
                return Ok("statistic is disable");
            }
            await SetPageStayTime();
            return Ok("success");
        }

        [HttpPost("click")]
        public async Task<IActionResult> ElementClick(int siteId)
        {
            if (!CheckNeedTrace())
            {
                return Ok("statistic is disable");
            }
            await SetElementClick(siteId);
            return Ok("success");
        }

        [HttpPost("trace")]
        public async Task<IActionResult> PageTrace()
        {
            try
            {
                if (!CheckNeedTrace())
                {
                    return Ok("statistic is disable");
                }
                await SetTraceRecord();
                return Ok("success");
            }
            catch (Exception e)
            {
                return Content(e.Message);
            }
        }

        //[HttpGet, Route("trace.js")]
        //public IActionResult GetElementStatisticJs(int siteId)
        //{
        //    if (!CheckNeedTrace())
        //    {
        //        return Content(StatisticJsFile.STOPSTATISTICJS, "application/javascript");
        //    }

        //    // var traceId = SetTraceRecord();
        //    var url = HttpContext.Request.Headers["Referer"].ToString();

        //    var configList = _commonQuery.GetClickConfigList(siteId, "");

        //    var clickRules = configList.Where(rule => url.Contains(rule.UrlRule) || new Regex(rule.UrlRule).IsMatch(url)).ToList();
        //    var tagNameList = new List<string>();
        //    var rootUrl = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host.Value;
        //    var jsString = string.Empty;
        //    if (clickRules.Count > 0)
        //    {
        //        foreach (var rule in clickRules)
        //        {
        //            var attrNameList = rule.AttrNames.Split(',').ToList();
        //            tagNameList = tagNameList.Concat(attrNameList).ToList();
        //        }
        //        tagNameList = tagNameList.Distinct().ToList();
        //        var arrStr = JsonConvert.SerializeObject(tagNameList);
        //        jsString = StatisticJsFile.STATISTICJS.Replace("<WEBROOTURL>", rootUrl);
        //    }
        //    else
        //    {
        //        jsString = StatisticJsFile.STATISTICJS.Replace("<WEBROOTURL>", rootUrl);
        //    }
        //    return Content(jsString, "application/javascript");
        //}

        [HttpGet("clickelementselectors")]
        public IActionResult GetClickElementSelectors(int siteId)
        {
            var url = HttpContext.Request.Headers["Referer"].ToString();
            var configList = _commonQuery.GetClickConfigList(siteId, "");
            var clickRules = configList.Where(rule => url.Contains(rule.UrlRule) || new Regex(rule.UrlRule).IsMatch(url)).ToList();
            var tagNameList = new List<string>();
            if (clickRules.Count > 0)
            {
                foreach (var rule in clickRules)
                {
                    var attrNameList = rule.AttrNames.Split(',').ToList();
                    tagNameList = tagNameList.Concat(attrNameList).ToList();
                }
                tagNameList = tagNameList.Distinct().ToList();
            }

            return Ok(tagNameList);
        }
        #endregion

        #region private method

        private string GetPeekValue(List<string> peekConfig, IElement el)
        {
            var result = new List<string>();
            foreach (var conf in peekConfig)
            {
                result.Add($"{conf}={el.GetAttribute(conf)}");
            }

            return string.Join("&", result);
        }

        #endregion

    }
}
