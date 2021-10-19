using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class PageTraceEntity
    {
        public string ToUrl { get; set; }

        public string FromUrl { get; set; }

        public string TraceId { get; set; }

        public int SiteId { get; set; }

        public Dictionary<string, string> ExtentionData { get; set; }
    }
}
