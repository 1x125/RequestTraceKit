using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class RequestTraceBuilder
    {
        public RequestTraceBuilder(IServiceCollection services,TraceRecordOption options)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the services.
        /// </summary>
        /// <value>
        /// The services.
        /// </value>
        public IServiceCollection Services { get; }

        public TraceRecordOption Options { get; set; }
    }
}
