using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RequestTraceKit
{
    internal class MySqlHelper
    {
        /// <summary>
        /// 连接字符串
        /// </summary>
        public readonly string conf;

        public MySqlHelper(string connStr)
        {
            conf = connStr;
        }

        /// <summary>
        /// 返回首行首列
        /// </summary>
        /// <param name="sqltext"></param>
        /// <returns></returns>
        public object ExecuteScalar(string sqltext)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                MySqlCommand comm = new MySqlCommand(sqltext, conn);
                return comm.ExecuteScalar();
            }
        }

        /// <summary>
        /// 获得表名
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public string GetScalarTableName(string tableName, string database)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                string sqltext = $"select TABLE_NAME from information_schema.tables where TABLE_SCHEMA='{database}' AND table_name like '{tableName}%' ORDER BY TABLE_NAME desc limit 0,1;";
                MySqlCommand comm = new MySqlCommand(sqltext, conn);
                var obj = comm.ExecuteScalar();
                return obj?.ToString();
            }
        }

        /// <summary>
        /// 返回受影响行数
        /// </summary>
        /// <param name="sqltext"></param>
        /// <returns></returns>
        public int ExecuteNoQuery(string sqltext)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                MySqlCommand comm = new MySqlCommand(sqltext, conn);
                return comm.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 返回受影响行数
        /// </summary>
        /// <param name="sqltext"></param>
        /// <returns></returns>
        public DataSet ExecuteDataset(string sqltext)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(sqltext, conf);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }
        /// <summary>
        /// 返回dataset 传入sqlparameter
        /// </summary>
        /// <param name="sqltext"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public DataSet ExecuteDataset(string sqltext, MySqlParameter[] param)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                conn.Open();
                MySqlDataAdapter adapter = new MySqlDataAdapter(sqltext, conn);
                //adapter.SelectCommand.Connection = conn;
                adapter.SelectCommand.CommandType = CommandType.Text;
                //  adapter.SelectCommand.CommandText = sqltext;
                adapter.SelectCommand.Parameters.AddRange(param);
                DataSet ds = new DataSet();
                adapter.Fill(ds);
                return ds;
            }
        }

        public int BulkLoad(DataTable table)
        {
            using (MySqlConnection conn = new MySqlConnection(conf))
            {
                var columns = table.Columns.Cast<DataColumn>().Select(colum => colum.ColumnName).ToList();
                MySqlBulkLoader bulk = new MySqlBulkLoader(conn)
                {
                    FieldTerminator = ",",
                    FieldQuotationCharacter = '"',
                    EscapeCharacter = '"',
                    LineTerminator = "\r\n",
                    FileName = table.TableName + ".csv",
                    NumberOfLinesToSkip = 0,
                    TableName = table.TableName,
                    CharacterSet = "utf8"

                };

                bulk.Columns.AddRange(columns);
                return bulk.Load();
            }
        }

        public int TrackBatchInsert(string opTableName, string currentTableName, string sql, string createTbSql, string database)
        {
            int result = 0;
            try
            {
                result = ExecuteNoQuery(sql);
            }
            catch (Exception e)
            {
                MySqlException ex = e as MySqlException;
                if (ex.Number == 1146)
                {
                    //如果分表不存在则创建
                    //先判断是否是第一次创建表
                    string fromTableName = GetScalarTableName(opTableName, database);
                    if (string.IsNullOrEmpty(fromTableName))
                    {
                        //创建新表
                        CreateTable(currentTableName, createTbSql);
                    }
                    else
                    {
                        //复制表
                        CopyTable(fromTableName, currentTableName);
                    }
                    result = ExecuteNoQuery(sql);
                }
            }

            return result;
        }

        /// <summary>
        /// 复制表
        /// </summary>
        /// <param name="fromTableName"></param>
        /// <param name="toTableName"></param>
        /// <returns></returns>
        public bool CopyTable(string fromTableName, string toTableName)
        {
            try
            {
                string sql = $"CREATE TABLE IF NOT EXISTS {toTableName} (LIKE {fromTableName});";
                ExecuteNoQuery(sql);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="createSql"></param>
        /// <returns></returns>
        public bool CreateTable(string tableName, string createSql)
        {
            try
            {
                ExecuteNoQuery(createSql);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
