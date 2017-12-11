using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Extensions;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public class DatabaseChecker : BaseChecker
    {
        public DatabaseChecker(ICheckerHelper helper) : base(helper) { }

        private const string column = "Column";

        private enum Settings
        {
            ConnString = 7,
            Query = 8,
        }

        /// <summary>
        /// This enum value MUST correlate to dbo.tblSubCheckType 'ID' column
        /// </summary>
        private enum SubCheckType
        {
            JSONProperty = 3,
        }

        /// <summary>
        /// These enum values MUST correlate to dbo.tblSubCheckTypeOption 'ID' column
        /// </summary>
        private enum SubCheckTypeOption
        {
            ValueEqualsSingleRow = 6,
            ColumnName = 7,
            Exists = 8,
            ValueEqualsMultipleRows = 9,
        }

        public async override Task<CheckResult> PerformCheck(CheckResult result)
        {
            // Get and validate db connection string
            int connStringId = _check.Data.TypeOptions[((int)Settings.ConnString).ToString()];
            var connString = _settings.ConnStrings.FirstOrDefault(x => x.ID == connStringId);
            if (connString == null || string.IsNullOrEmpty(connString.Value))
            {
                throw new Exception("Connection string invalid");
            }
            _logger.Info($"Using connection string {connString.Name}: {connString.Value}");

            // Get and validate sequel query
            string query = _check.Data.TypeOptions[((int)Settings.Query).ToString()];
            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query invalid");
            }

            // Run sub check, within a timer
            var timer = new Stopwatch();
            timer.Start();
            var jsonResult = await GetQueryResultAsJson(connString.Value, query);
            await _helper.RunSubChecks(_check, _logger, subCheck => RunSubCheck(subCheck, jsonResult, result));
            timer.Stop();
            result.TimeMS = (int)timer.ElapsedMilliseconds;

            // Log and return
            _logger.Info(JsonConvert.SerializeObject(jsonResult, Formatting.Indented));
            return result;
        }

        protected async Task<string> GetQueryResultAsJson(string connectionString, string query)
        {
            string jsonResult;

            using (var conn = new SqlConnection(connectionString))
            {
                using (var sqlCommand = new SqlCommand(query, conn))
                {
                    conn.Open();
                    try
                    {
                        var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                        jsonResult = await sqlDataReader.ToJson();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return jsonResult;
        }

        private void RunSubCheck(SubCheck subCheck, string jsonResult, CheckResult checkResult)
        {
            switch (subCheck.TypeID)
            {
                case (int)SubCheckType.JSONProperty:

                    // Get & validate data identifiers
                    string columnName = subCheck.Options[((int)SubCheckTypeOption.ColumnName).ToString()];
                    bool exists = subCheck.Options[((int)SubCheckTypeOption.Exists).ToString()];
                    if (string.IsNullOrEmpty(columnName) && exists)
                    {
                        throw new SubCheckException($"{column} '{columnName}' does not exist");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(columnName) && !exists)
                        {
                            throw new SubCheckException($"{column} '{columnName}' exists");
                        }
                    }

                    // Get dataset from query as json
                    var jArray = JArray.Parse(jsonResult);
                    dynamic jObject = JObject.Parse(jArray[0].ToString());//NOTE: GETTING FIRST ARRAY ENTRY ONLY AT THIS TIME
                    string actualValue = jObject.SelectToken(columnName)?.ToString();
                    _logger.Info(actualValue);

                    // Perform actual check
                    string expectedValue = subCheck.Options[((int)SubCheckTypeOption.ValueEqualsSingleRow).ToString()]; ;
                    if (actualValue == expectedValue)
                    {
                        _logger.Info($"{column} '{columnName}' equals the expected value of '{expectedValue}'");
                    }
                    else
                    {
                        throw new SubCheckException($"{column} '{columnName}' does not equal the expected value of '{expectedValue}', the actual value is '{actualValue}'");
                    }
                    break;
                default:
                    _logger.Warn($"Unknown sub-check type {subCheck.TypeID} - ignoring");
                    break;
            }
        }
    }
}