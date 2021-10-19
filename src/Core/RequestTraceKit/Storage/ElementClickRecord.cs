using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class ElementClickRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string RequestUrl { get; set; }

        public string RuleName { get; set; }

        public string AttrName { get; set; }

        public string AttrId { get; set; }

        public string ClassName { get; set; }

        public string PeekValue { get; set; }

        public string ClientIP { get; set; }

        public string UserAgent { get; set; }

        public string UserId { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
