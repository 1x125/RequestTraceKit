using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace RequestTraceKit
{
    public static class HttpRequestExtensions
    {
        private const string MOBILE_COOKIE_NAME = "x-ibb360-mobile-cookie";

        private static readonly List<string> s_SearchEngineSpiderIdentifiers = new List<string>
        {
            "bot",
            "spider",
            "crawler",
            "ask",
            "architext",
            "slurp",
            "yahoo",
            "ia_archiver",
            "infoseek",
            "scooter",
            "nutch",
            "wordpress",
            "urllib",
            "pycurl",
            "larbin",
            "acoon",
            "soft framework",
            "EC2LinkFinder",
            "Baiduspider",
            "DNSPod"
        };

        /// <summary>
        /// Is this current request coming from a search engine bot.
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static bool BySearchEngine(this HttpRequest httpRequest)
        {
            if (httpRequest == null || string.IsNullOrWhiteSpace(httpRequest.Headers["User-Agent"].ToString()))
            {
                return true;
            }

            string userAgent = httpRequest.Headers["User-Agent"].ToString().ToLowerInvariant();
            foreach (string identifier in s_SearchEngineSpiderIdentifiers)
            {
                if (httpRequest.Headers["User-Agent"].ToString().IndexOf(identifier, StringComparison.OrdinalIgnoreCase) > -1
                    || string.Equals(userAgent, identifier, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Is this current request using weixin browser
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static bool ByWeiXinBrowser(this HttpRequest httpRequest)
        {
            if (httpRequest == null || string.IsNullOrWhiteSpace(httpRequest.Headers["User-Agent"].ToString()))
            {
                return false;
            }
            return httpRequest.Headers["User-Agent"].ToString().IndexOf("micromessenger", StringComparison.OrdinalIgnoreCase) > -1
                && httpRequest.Headers["User-Agent"].ToString().IndexOf("wxwork", StringComparison.OrdinalIgnoreCase) <= -1;
        }


        public static bool ByWorkWeChatBrowser(this HttpRequest httpRequest)
        {
            if (httpRequest == null || string.IsNullOrWhiteSpace(httpRequest.Headers["User-Agent"].ToString()))
            {
                return false;
            }
            return httpRequest.Headers["User-Agent"].ToString().IndexOf("wxwork", StringComparison.OrdinalIgnoreCase) > -1;
        }

        /// <summary>
        /// Is this current request using weibo browser
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static bool ByWeiBoBrowser(this HttpRequest httpRequest)
        {
            if (httpRequest == null || string.IsNullOrWhiteSpace(httpRequest.Headers["User-Agent"].ToString()))
            {
                return false;
            }
            return httpRequest.Headers["User-Agent"].ToString().IndexOf("weibo", StringComparison.OrdinalIgnoreCase) > -1;
        }


        /// <summary>
        /// Is this current request using qq browser
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static bool ByQQBrowser(this HttpRequest httpRequest)
        {
            if (httpRequest == null || string.IsNullOrWhiteSpace(httpRequest.Headers["User-Agent"].ToString()))
            {
                return false;
            }
            return httpRequest.Headers["User-Agent"].ToString().IndexOf(" QQ/", StringComparison.OrdinalIgnoreCase) > -1;
        }



        /// <summary>
        /// Is this current request using qq browser
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static bool ByAlipayBrowser(this HttpRequest httpRequest)
        {
            if (httpRequest == null || string.IsNullOrWhiteSpace(httpRequest.Headers["User-Agent"].ToString()))
            {
                return false;
            }
            return httpRequest.Headers["User-Agent"].ToString().IndexOf("AlipayClient/", StringComparison.OrdinalIgnoreCase) > -1;
        }


        /// <summary>
        /// Is this current request from ibb360's app
        /// </summary>
        /// <param name="httpRequest"></param>
        /// <returns></returns>
        public static bool ByApp(this HttpRequest httpRequest)
        {
            if (httpRequest == null || httpRequest.Headers == null)
            {
                return false;
            }
            return httpRequest.Headers.Keys.Contains(MOBILE_COOKIE_NAME);
        }

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            return request.Headers != null && request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        private static string[] BubbleSort(string[] r)
        {
            int i, j;
            string temp;
            bool exchange;
            for (i = 0; i < r.Length; i++)
            {
                exchange = false;
                for (j = r.Length - 2; j >= i; j--)
                {
                    if (String.CompareOrdinal(r[j + 1], r[j]) < 0)
                    {
                        temp = r[j + 1];
                        r[j + 1] = r[j];
                        r[j] = temp;
                        exchange = true;
                    }
                }
                if (!exchange)
                {
                    break;
                }
            }
            return r;
        }

    }
}
