using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class MongoRequestTraceHandler : IRequestTraceHandler
    {
        private TraceRecordOption _option;
        private readonly IMongoCollection<RequestTraceRecord> _record;
        private readonly IMongoCollection<ElementClickRecord> _elRecord;

        public MongoRequestTraceHandler(TraceRecordOption option)
        {
            _option = option;

            var client = new MongoClient(_option.ConnectionString);
            var database = client.GetDatabase(_option.DatabaseName);
            _record = database.GetCollection<RequestTraceRecord>(_option.TableNames.TraceTableName);
            _elRecord = database.GetCollection<ElementClickRecord>(_option.TableNames.ElementClickTableName);
        }

        public void BulkSaveTrace(IEnumerable<RequestTraceRecord> traces)
        {
            if (traces == null || traces.Count() <= 0)
            {
                return;
            }
            try
            {
                //foreach (var msg in traces)
                //{
                //    var ipInfo = await IPInfoHelper.GetIPData(msg.ClientIP, _option.IPDataServerUrl);
                //    msg.Area = ipInfo?.data?.country;
                //    msg.Province = ipInfo?.data?.region;
                //    msg.CityName = ipInfo?.data?.city;
                //}
                _record.InsertMany(traces);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void BulkSaveTrace(IEnumerable<PageStayRecord> traces)
        {
            if (traces == null || traces.Count() <= 0)
            {
                return;
            }
            foreach (var stay in traces)
            {
                var filter = Builders<RequestTraceRecord>.Filter.Eq(trace => trace.TraceUid, stay.TraceId);
                var update = Builders<RequestTraceRecord>.Update.Inc(trace => trace.StaySecond, stay.StayTimes);
                _record.UpdateOne(filter, update);
            }
        }

        public void BulkSaveTrace(IEnumerable<ElementClickRecord> traces)
        {
            if (traces == null || traces.Count() <= 0)
            {
                return;
            }
            _elRecord.InsertMany(traces);
        }
    }
}
