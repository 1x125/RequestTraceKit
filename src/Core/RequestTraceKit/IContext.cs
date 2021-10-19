using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    internal interface IContext
    {
        bool HasLogined { get; }

        string UserId { get; }

        string SessionId { get; }

        int SalesChannelNumber { get; }

        string SalesAgentId { get; }

        string VendorId { get; }

        string MerchantId { get; }

        string ClientIP { get; }

        string RequestUserAgent { get; }

        DeviceType DeviceType { get; }

        Geography ClientLocation { get; }

        void Attach(IContext owner);

        Dictionary<string, string> GetRouteDatas(string url);

        object this[string key]
        {
            get;
            set;
        }
    }
}
