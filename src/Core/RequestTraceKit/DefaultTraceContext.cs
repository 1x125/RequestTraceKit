using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    internal class DefaultTraceContext: IContext
    {
        private static Dictionary<string, object> s_ContextData = new Dictionary<string, object>();
        private static string s_UserId;
        private static string s_SessionId;
        private static string s_ClientIP;

        internal static void SetContextData(string userId, string sessionId, Dictionary<string, string> contextData)
        {
            s_UserId = userId;
            s_SessionId = sessionId;
            if (contextData != null && contextData.Count > 0)
            {
                foreach (var entry in contextData)
                {
                    if (s_ContextData.ContainsKey(entry.Key))
                    {
                        s_ContextData[entry.Key] = entry.Value;
                    }
                    else
                    {
                        s_ContextData.Add(entry.Key, entry.Value);
                    }
                }
            }
        }

        public bool HasLogined { get { return true; } }
        public string UserId
        {
            get { return s_UserId; }
        }

        public string SessionId
        {
            get { return s_SessionId; }
        }

        public int SalesChannelNumber
        {
            get
            {
                return 1;
            }
        }

        public string SalesAgentId
        {
            get { return null; }
        }

        public string VendorId
        {
            get { return null; }
        }

        public string MerchantId { get { return null; } }

        public string ClientIP
        {
            get
            {
                if (string.IsNullOrWhiteSpace(s_ClientIP))
                {
                    try
                    {
                        IPAddress[] address = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                        if (address != null)
                        {
                            foreach (IPAddress addr in address)
                            {
                                if (addr == null)
                                {
                                    continue;
                                }
                                string tmp = addr.ToString().Trim();
                                //过滤IPv6的地址信息
                                if (tmp.Length <= 16 && tmp.Length > 5)
                                {
                                    s_ClientIP = tmp;
                                    break;
                                }
                            }
                        }
                    }
                    catch
                    {
                        //s_ServerIP = string.Empty;
                    }
                }
                if (string.IsNullOrWhiteSpace(s_ClientIP))
                {
                    return string.Empty;
                }
                return s_ClientIP;
            }
        }

        public void Attach(IContext owner)
        {

        }

        public object this[string key]
        {
            get
            {
                object v;
                if (s_ContextData.TryGetValue(key, out v))
                {
                    return v;
                }
                return null;
            }
            set
            {
                s_ContextData[key] = value;
            }
        }


        public string RequestUserAgent
        {
            get { return "no agent"; }
        }

        public DeviceType DeviceType
        {
            get { return DeviceType.Desktop; }
        }

        public Geography ClientLocation
        {
            get
            {
                return new Geography { Latitude = 0, Longitude = 0 };
            }
        }

        public Dictionary<string, string> GetRouteDatas(string url)
        {
            var uri = new Uri(url);
            var dic = new Dictionary<string, string>();
            if (uri.Segments.Length == 2)
            {
                dic["controller"] = uri.Segments[1].Replace("/", "");
                dic["action"] = String.Empty;
                dic["query"] = String.Empty;
            }
            else if (uri.Segments.Length == 3)
            {
                dic["controller"] = uri.Segments[1].Replace("/", "");
                dic["action"] = uri.Segments[2].Replace("/", "");
                dic["query"] = String.Empty;
            }
            else if (uri.Segments.Length == 4)
            {
                dic["controller"] = uri.Segments[1].Replace("/", "");
                dic["action"] = uri.Segments[2].Replace("/", "");
                dic["query"] = uri.Segments[3];
            }
            else
            {
                dic["controller"] = String.Empty;
                dic["action"] = String.Empty;
                dic["query"] = String.Empty;
            }
            return dic;
        }
    }
}
