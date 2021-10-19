using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UAParser;

namespace RequestTraceKit
{
    public class TraceBehavior : ITraceBehavior
    {
        private static bool Disabled = false;
        public TraceBehavior(IOptions<TraceRecordOption> options)
        {
            if (options.Value.Disable.HasValue)
                Disabled = options.Value.Disable.Value;
        }

        public bool CheckNeedTrace(HttpContext httpContext)
        {
            return Disabled == false;
        }

        public void SetExtentionData(HttpContext httpContext, RequestTraceRecord msg)
        {

        }

        public string DetectRequestClientType(HttpContext httpContext)
        {
            string clientType = "OT";
            if (httpContext.Request.ByApp())
            {
                clientType = "AP";
            }
            else if (httpContext.Request.ByQQBrowser())
            {
                clientType = "QQ";
            }
            else if (httpContext.Request.BySearchEngine())
            {
                clientType = "SE";
            }
            else if (httpContext.Request.ByWeiBoBrowser())
            {
                clientType = "WB";
            }
            else if (httpContext.Request.ByWeiXinBrowser())
            {
                clientType = "WX";
            }
            else if (httpContext.Request.ByAlipayBrowser())
            {
                clientType = "AL";
            }
            return clientType;
        }
    }

    internal class TraceInfo
    {
        private DateTime m_StartTime;
        private Stopwatch m_Watch;
        private Stopwatch m_WatchView;
        private long m_ElapsedMilliseconds;
        private long m_ActionElapsedMilliseconds;
        private long m_ViewElapsedMilliseconds;
        private bool m_HasException = false;

        private static string s_ServerIP;
        private static string s_AppId;

        private IOptions<TraceRecordOption> _option;

        public TraceInfo(IOptions<TraceRecordOption> option)
        {
            _option = option;
        }
        private static string GetServerIP() // 不需要加锁，因为即使多线程时发生了并发冲突也不会有问题，加锁反而带来更大的性能开销
        {
            if (s_ServerIP == null)
            {
                IPAddress ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(i => i.AddressFamily == AddressFamily.InterNetwork);
                s_ServerIP = ip.ToString();
            }
            return s_ServerIP;
        }

        public void Start()
        {
            m_StartTime = DateTime.Now;
            m_Watch = Stopwatch.StartNew();
        }

        public void ActionEnd(bool hasException)
        {
            m_ActionElapsedMilliseconds = m_Watch.ElapsedMilliseconds;
            m_HasException = m_HasException || hasException;
        }

        public void ViewBegin()
        {
            m_WatchView = Stopwatch.StartNew();
        }

        public void Stop(bool hasException)
        {
            m_WatchView.Stop();
            m_Watch.Stop();
            m_ViewElapsedMilliseconds = m_WatchView.ElapsedMilliseconds;
            m_ElapsedMilliseconds = m_Watch.ElapsedMilliseconds;
            m_HasException = m_HasException || hasException;
        }

        public RequestTraceRecord ToMessage(HttpContext httpContext, string url, string urlReferrer, string clientType)
        {
            IContext cxt = ContextManager.Current;
            var userAgent = httpContext.Request.Headers["User-Agent"];
            var uaParser = Parser.GetDefault();
            ClientInfo client = uaParser.Parse(userAgent);
            var routeDatas = cxt.GetRouteDatas(url);


            var msg = new RequestTraceRecord()
            {
                //AppServerIP = GetServerIP(),
                //RequestRawUrl = url,
                RequestUrl = url,
                UserId = cxt.UserId,
                SessionId = GetSessionId(httpContext),
                UserAgent = userAgent,
                DeviceType = cxt.DeviceType,
                ClientIP = cxt.ClientIP,
                // Latitude = null, //cxt.ClientLocation.Latitude == 0 ? null : new float?(cxt.ClientLocation.Latitude),
                //Longitude = null, //cxt.ClientLocation.Longitude == 0 ? null : new float?(cxt.ClientLocation.Longitude),
                RequestStartTime = (m_StartTime == DateTime.MinValue) ? DateTime.Now : m_StartTime,
                ElapsedSecond = m_ElapsedMilliseconds / 1000d,
                ClientType = clientType,
                UrlReferrer = urlReferrer,
                ActionElapsedSecond = m_ActionElapsedMilliseconds / 1000d,
                ViewElapsedSecond = m_ViewElapsedMilliseconds / 1000d,
                HasException = m_HasException,
                OS = client.OS.Family,
                OS_Version = client.OS.Major,
                Browser_Version = client.UA.Major,
                Browser = client.UA.Family,
                DeviceName = client.Device.Family,
                Cookie = httpContext.Request.Cookies.ToString(),
                IsSpider = client.Device.IsSpider,
                ControllerName = routeDatas["controller"],
                ActionName = routeDatas["action"],
                UserName = httpContext.User.ToString(),
                ResponseStatus = httpContext.Response.StatusCode,
                RequestMethod = httpContext.Request.Method,
                HostIP = httpContext.Connection.LocalIpAddress.MapToIPv4().ToString(),
                HostPort = httpContext.Connection.LocalPort
            };
            if (!httpContext.Request.Cookies.ContainsKey(_option.Value.CookieName))
            {
                var cookie = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                httpContext.Response.Cookies.Append(_option.Value.CookieName, cookie, new CookieOptions()
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.MaxValue,
                    SameSite = SameSiteMode.None
                });
                msg.IsNew = true;
                msg.Cookie = cookie;
            }
            else
            {
                msg.IsNew = false;
                msg.Cookie = httpContext.Request.Cookies[_option.Value.CookieName];
            }

            return msg;
        }

        private string GetSessionId(HttpContext httpContext)
        {
            try
            {
                return httpContext.Session.Id;
            }
            catch (Exception e)
            {
                return null;
            }
        }


    }
}
