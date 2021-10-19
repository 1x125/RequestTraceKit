using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class PageStayRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TraceId { get; set; }
        public string RuleName { get; set; }

        public string PageUrl { get; set; }

        public string ClientIP { get; set; }

        public string UserAgent { get; set; }

        public string UserId { get; set; }

        public int StayTimes { get; set; }

        public DateTime EnterTime { get; set; }

        public DateTime LeaveTime { get; set; }
    }
}
