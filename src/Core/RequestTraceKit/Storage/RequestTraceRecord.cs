using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class RequestTraceRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TraceUid { get; set; }

        public string RequestUrl { get; set; }

        //public string RequestRawUrl { get; set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string SessionId { get; set; }

        public string UserAgent { get; set; }

        public int SiteId { get; set; }

        public DeviceType DeviceType { get; set; }

        public string DeviceName { get; set; }

        public string ClientIP { get; set; }

        public string HostIP { get; set; }

        public int HostPort { get; set; }

        public string RequestMethod { get; set; }

        //public float? Latitude { get; set; }

        //public float? Longitude { get; set; }

        public DateTime RequestStartTime { get; set; }

        public double ElapsedSecond { get; set; }

        //public bool IsAjax { get; set; }

        public string ClientType { get; set; }

        public string UrlReferrer { get; set; }

        //public string DomainReferrer { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public double ActionElapsedSecond { get; set; }

        public double ViewElapsedSecond { get; set; }

        public string OS { get; set; }

        public string OS_Version { get; set; }

        public string Browser { get; set; }

        public string Browser_Version { get; set; }

        public string Cookie { get; set; }

        public int ResponseStatus { get; set; }

        public bool IsNew { get; set; }

        /// <summary>
        /// 是否机器人
        /// </summary>
        public bool IsSpider { get; set; }

        /// <summary>
        /// 国家或地区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        public string CityName { get; set; }

        public int StaySecond { get; set; }

        public bool HasException { get; set; }

        private NameValueCollection _extentionData;

        public NameValueCollection ExtentionData
        {
            get
            {
                if (_extentionData == null)
                {
                    _extentionData = new NameValueCollection();
                }
                return _extentionData;
            }
        }


    }
}
