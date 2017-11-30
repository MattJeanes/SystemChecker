using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemChecker.Model.Data;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public class DatabaseChecker : BaseChecker
    {
        public DatabaseChecker(ICheckerHelper helper) : base(helper) { }
        private enum Settings
        {
            ConnString = 7,
            Query = 8,
        }

        public async override Task<CheckResult> PerformCheck(CheckResult result)
        {
            int connStringId = _check.Data.TypeOptions[((int)Settings.ConnString).ToString()];
            var connString = _settings.ConnStrings.FirstOrDefault(x => x.ID == connStringId);
            string query = _check.Data.TypeOptions[((int)Settings.Query).ToString()];
            if (connString == null || string.IsNullOrEmpty(connString.Value))
            {
                throw new Exception("Connection string invalid");
            }
            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query invalid");
            }
            _logger.Info($"Using connection string {connString.Name}: {connString.Value}");
            var timer = new Stopwatch();
            timer.Start();
            var results = await RetrieveFromSQL(connString.Value, query);
            timer.Stop();
            result.TimeMS = (int)timer.ElapsedMilliseconds;
            _logger.Info(JsonConvert.SerializeObject(results, Formatting.Indented));
            return result;
        }

        protected async Task<IEnumerable<dynamic>> RetrieveFromSQL(string connectionString, string sql)
        {
            var resultList = new List<dynamic>();

            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(sql, conn))
                {
                    conn.Open();
                    try
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            resultList.Add(SqlDataReaderToExpando(reader));
                        }
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
            return resultList;
        }

        protected dynamic SqlDataReaderToExpando(SqlDataReader reader)
        {
            var expandoObject = new ExpandoObject() as IDictionary<string, object>;

            for (var i = 0; i < reader.FieldCount; i++)
                expandoObject.Add(reader.GetName(i), reader[i]);

            return expandoObject;
        }
    }
}
