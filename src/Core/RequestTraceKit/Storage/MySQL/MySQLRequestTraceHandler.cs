using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class MySQLRequestTraceHandler : IRequestTraceHandler
    {
        private TraceRecordOption _option;
        private MySqlHelper _dbHelper;

        public MySQLRequestTraceHandler(TraceRecordOption option)
        {
            _option = option;
            _dbHelper = new MySqlHelper(_option.ConnectionString);
        }

        public void BulkSaveTrace(IEnumerable<RequestTraceRecord> traces)
        {
            if (traces == null || traces.Count() <= 0)
            {
                return;
            }
            var tableName = _option.TableNames.TraceTableName + DateTime.Now.ToString("yyyyMM");
            var traceList = traces.ToList();

            StringBuilder sqlCol = new StringBuilder(
                $"INSERT INTO `{_option.DatabaseName}`.`{tableName}`(TraceUid,SiteId,`RequestUrl`, `UserId`, UserName,`SessionId`, `UserAgent`, `DeviceType`, DeviceName,`ClientIP`,HostIP,HostPort,RequestMethod, `RequestStartTime`, `ElapsedSecond`, `ClientType`, `UrlReferrer`, `ActionElapsedSecond`, `ViewElapsedSecond`, `OS`, `OSVersion`, `Browser`, `BrowserVersion`, `HasException`,Cookie,ResponseStatus,IsNew,IsSpider,Area,Province,CityName,ControllerName,ActionName");

            //构建字段
            foreach (var key in traceList[0].ExtentionData.AllKeys)
            {
                sqlCol.Append("," + key);
            }
            sqlCol.Append(") values ");

            var insertList = new List<string>();
            foreach (var msg in traceList)
            {
                if (msg.RequestUrl != null && msg.RequestUrl.Length > 256)
                {
                    msg.RequestUrl = msg.RequestUrl.Substring(0, 256);
                }

                if (msg.ClientIP != null && msg.ClientIP.Length > 16)
                {
                    msg.ClientIP = msg.ClientIP.Substring(0, 16);
                }
                if (msg.ClientType != null && msg.ClientType.Length > 2)
                {
                    msg.ClientType = msg.ClientType.Substring(0, 2);
                }
                if (msg.UrlReferrer != null && msg.UrlReferrer.Length > 256)
                {
                    msg.UrlReferrer = msg.UrlReferrer.Substring(0, 256);
                }
                //获得城市
                //var ipInfo = await IPInfoHelper.GetIPData(msg.ClientIP, _option.IPDataServerUrl);
                //msg.Area = ipInfo?.data?.country;
                //msg.Province = ipInfo?.data?.region;
                //msg.CityName = ipInfo?.data?.city;

                StringBuilder valStr = new StringBuilder($"('{msg.TraceUid}',{msg.SiteId},'{msg.RequestUrl}', '{msg.UserId}', '{msg.UserName}','{msg.SessionId}', '{msg.UserAgent}', {(int)msg.DeviceType},'{msg.DeviceName}', '{msg.ClientIP}','{msg.HostIP}', {msg.HostPort},'{msg.RequestMethod}','{msg.RequestStartTime.ToString("yyyy-MM-dd HH:mm:ss")}', " +
                                                       $"{msg.ElapsedSecond}, '{msg.ClientType}', '{msg.UrlReferrer}', {msg.ActionElapsedSecond}, {msg.ViewElapsedSecond}, '{msg.OS}', '{msg.OS_Version}'," +
                                                       $" '{msg.Browser}', '{msg.Browser_Version}', {(msg.HasException ? 1 : 0)},'{msg.Cookie}',{msg.ResponseStatus},{msg.IsNew},{msg.IsSpider},'{msg.Area}','{msg.Province}','{msg.CityName}','{msg.ControllerName}','{msg.ActionName}'");
                foreach (var key in msg.ExtentionData.AllKeys)
                {
                    valStr.Append($",'{msg.ExtentionData[key]}'");
                }
                valStr.Append(")");
                insertList.Add(valStr.ToString());
            }

            sqlCol.Append(string.Join(',', insertList));
            string createSql = GetCreateTableSql(tableName);
            int result = _dbHelper.TrackBatchInsert(_option.TableNames.TraceTableName, tableName, sqlCol.ToString(), createSql, _option.DatabaseName);
        }

        public void BulkSaveTrace(IEnumerable<PageStayRecord> traces)
        {
            if (traces == null || traces.Count() <= 0)
            {
                return;
            }
            var tableName = _option.TableNames.TraceTableName + DateTime.Now.ToString("yyyyMM");
            var traceList = traces.ToList();

            StringBuilder updateSql = new StringBuilder($"UPDATE {tableName} SET StaySecond = CASE TraceUid");

            var updateTraceIds = new List<string>();
            foreach (var msg in traceList)
            {
                updateSql.AppendLine($" WHEN '{msg.TraceId}' THEN {msg.StayTimes}");
                updateTraceIds.Add($"'{msg.TraceId}'");
            }

            string inTraceIdStr = string.Join(',', updateTraceIds);
            updateSql.AppendLine($"END WHERE TraceUid IN ({inTraceIdStr});");
            int result = _dbHelper.ExecuteNoQuery(updateSql.ToString());
        }

        public void BulkSaveTrace(IEnumerable<ElementClickRecord> traces)
        {
            if (traces == null || traces.Count() <= 0)
            {
                return;
            }
            var tableName = _option.TableNames.ElementClickTableName + DateTime.Now.ToString("yyyyMM");
            var traceList = traces.ToList();

            StringBuilder sqlCol = new StringBuilder(
                $"INSERT INTO `{_option.DatabaseName}`.`{tableName}`(`RequestUrl`, `RuleName`, `AttrName`, `AttrId`, `ClassName`, `PeekValue`, `ClientIP`, `UserAgent`, `UserId`, `CreateTime`) values ");

            var insertList = new List<string>();
            foreach (var msg in traceList)
            {
                if (msg.RequestUrl != null && msg.RequestUrl.Length > 256)
                {
                    msg.RequestUrl = msg.RequestUrl.Substring(0, 256);
                }

                if (msg.ClientIP != null && msg.ClientIP.Length > 16)
                {
                    msg.ClientIP = msg.ClientIP.Substring(0, 16);
                }

                StringBuilder valStr = new StringBuilder($"('{msg.RequestUrl}', '{msg.RuleName}', '{msg.AttrName}', '{msg.AttrId}', '{msg.ClassName}', '{msg.PeekValue}', '{msg.ClientIP}', '{msg.UserAgent}', '{msg.UserId}', '{msg.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")}')");

                insertList.Add(valStr.ToString());
            }

            sqlCol.Append(string.Join(',', insertList));
            string createSql = GetElCreateTableSql(tableName);
            int result = _dbHelper.TrackBatchInsert(_option.TableNames.ElementClickTableName, tableName, sqlCol.ToString(), createSql, _option.DatabaseName);
        }

        private string GetCreateTableSql(string tableName)
        {
            string sql = $@"SET NAMES utf8mb4;
                                        SET FOREIGN_KEY_CHECKS = 0;
                                        DROP TABLE IF EXISTS `{tableName}`;
                                        CREATE TABLE `{tableName}`  (
                                          `Id` int(11) NOT NULL AUTO_INCREMENT,
                                          `TraceUid` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `SiteId` int(11) DEFAULT NULL,
                                          `RequestUrl` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `UserId` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `UserName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `SessionId` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `UserAgent` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `DeviceType` int(11) DEFAULT NULL,
                                          `DeviceName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ClientIP` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `HostIP` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `HostPort` int(11) DEFAULT NULL,
                                          `RequestMethod` varchar(20) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `RequestStartTime` datetime(0) DEFAULT NULL,
                                          `ElapsedSecond` double DEFAULT NULL,
                                          `ClientType` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `UrlReferrer` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ControllerName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ActionName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ActionElapsedSecond` double DEFAULT NULL,
                                          `ViewElapsedSecond` double DEFAULT NULL,
                                          `OS` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `OSVersion` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `Browser` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `BrowserVersion` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `Cookie` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ResponseStatus` int(11) DEFAULT NULL,
                                          `IsNew` bit(1) DEFAULT NULL,
                                          `IsSpider` bit(1) DEFAULT NULL,
                                          `Area` varchar(200) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `Province` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `CityName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `HasException` bit(1) DEFAULT NULL,
                                          `StaySecond` int(11) DEFAULT NULL,
                                          `ExtProductId` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          PRIMARY KEY (`Id`) USING BTREE
                                        ) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Compact;
                                        SET FOREIGN_KEY_CHECKS = 1;";
            return sql;
        }

        private string GetElCreateTableSql(string tableName)
        {
            string sql = $@"SET NAMES utf8mb4;
                                        SET FOREIGN_KEY_CHECKS = 0;
                                        DROP TABLE IF EXISTS `{tableName}`;
                                        CREATE TABLE `{tableName}`  (
                                          `Id` int(11) NOT NULL AUTO_INCREMENT,
                                          `RequestUrl` varchar(255) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `RuleName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `AttrName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `AttrId` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ClassName` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `PeekValue` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `ClientIP` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `UserAgent` varchar(500) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `UserId` varchar(50) CHARACTER SET utf8 COLLATE utf8_general_ci DEFAULT NULL,
                                          `CreateTime` datetime(0) DEFAULT NULL,
                                          PRIMARY KEY (`Id`) USING BTREE
                                        ) ENGINE = InnoDB AUTO_INCREMENT = 2 CHARACTER SET = utf8 COLLATE = utf8_general_ci ROW_FORMAT = Compact;

                                        SET FOREIGN_KEY_CHECKS = 1;";
            return sql;
        }
    }
}
