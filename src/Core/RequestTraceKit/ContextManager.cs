using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    internal static class ContextManager
    {
        private static Type s_Type = null;

        public static void SetAsDefault(string userId, string sessionId, Dictionary<string, string> contextData = null)
        {
            DefaultTraceContext.SetContextData(userId, sessionId, contextData);
            s_Type = typeof(DefaultTraceContext);
        }

        public static void SetContextType(Type type)
        {
            if (typeof(IContext).IsAssignableFrom(type) == false)
            {
                throw new ApplicationException(string.Format("The type '{0}' has not implemented the interface '{1}'.", type.AssemblyQualifiedName, typeof(IContext).AssemblyQualifiedName));
            }
            s_Type = type;
        }

        public static IContext Current
        {
            get
            {
                return new DefaultTraceContext();
            }
        }
    }
}
