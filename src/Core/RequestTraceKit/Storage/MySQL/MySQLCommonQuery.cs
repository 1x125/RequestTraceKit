using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    public class MySQLCommonQuery : ICommQuery
    {
        private readonly MySqlHelper _dbHelper;

        public MySQLCommonQuery(string connectionSgtring)
        {
            _dbHelper = new MySqlHelper(connectionSgtring);
        }

        public List<ClickConfigEntity> GetClickConfigList(int siteId, string urlRule)
        {
            var sql = $"SELECT * FROM clickconfig WHERE SiteId=@SiteId";
            if (!string.IsNullOrWhiteSpace(urlRule))
            {
                sql += " AND UrlRule=@UrlRule";
            }
            var paramArr = new MySqlParameter[]
            {
                new MySqlParameter("@SiteId",siteId),
                new MySqlParameter("@UrlRule",urlRule),
            };
            var ds = _dbHelper.ExecuteDataset(sql, paramArr);
            if (ds == null)
                return null;
            var tb = ds.Tables[0];
            var configList = new List<ClickConfigEntity>();
            foreach (DataRow row in tb.Rows)
            {
                configList.Add(new ClickConfigEntity
                {
                    Id = Convert.ToInt32(row["Id"]),
                    AttrNames = row["AttrNames"].ToString(),
                    Name = row["Name"].ToString(),
                    PeekConfig = row["PeekConfig"].ToString(),
                    SiteId = Convert.ToInt32(row["SiteId"]),
                    UrlRule = row["UrlRule"].ToString()
                });
            }

            return configList;
        }
    }
}
