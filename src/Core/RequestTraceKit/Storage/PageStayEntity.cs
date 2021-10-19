using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class PageStayEntity
    {
        public string Url { get; set; }

        public int Time { get; set; }

        public string TraceId { get; set; }
    }
}
