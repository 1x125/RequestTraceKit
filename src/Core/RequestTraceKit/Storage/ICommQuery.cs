using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public interface ICommQuery
    {
        List<ClickConfigEntity> GetClickConfigList(int siteId, string urlRule);
     }
}
