using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public static class StreamExtension
    {
        public static async Task<string> ReadAsStringAsync(this Stream stream)
        {
            if (!stream.CanRead) return null;
            var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }

        public static async Task<T> ReadAs<T>(this Stream stream)
        {
            var json = await ReadAsStringAsync(stream);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
