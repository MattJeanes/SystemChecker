using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.DTO;
using SystemChecker.Model.Extensions;
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

        /// <summary>
        /// This enum value MUST correlate to dbo.tblSubCheckType 'ID' column
        /// </summary>
        private enum SubCheckType
        {
            FieldEqualTo = 3,
            FieldNotEqualTo = 4,
        }

        /// <summary>
        /// These enum values MUST correlate to dbo.tblSubCheckTypeOption 'ID' column
        /// </summary>
        private enum FieldEqualTo
        {
            Value = 6,
            FieldName = 7,
            Exists = 8,
        }

        private enum FieldNotEqualTo
        {
            FieldName = 9,
            Value = 10,
            Exists = 11,
        }

        public async override Task<CheckResult> PerformCheck(CheckResult result)
        {
            var connStringDTO = GetConnStringDTO();
            var query = GetQuery();

            var timer = new Stopwatch();
            try
            {
                timer.Start();
                var jsonResult = await GetQueryResultAsJson(connStringDTO.Value, query);
                timer.Stop();
                _logger.Info(JsonConvert.SerializeObject(jsonResult, Formatting.Indented));
                await _helper.RunSubChecks(_check, _logger, subCheck => RunSubCheck(subCheck, jsonResult, result));
            }
            finally
            {
                if (timer.IsRunning)
                {
                    timer.Stop();
                }
                result.TimeMS = (int)timer.ElapsedMilliseconds;
            }
            return result;
        }

        private ConnStringDTO GetConnStringDTO()
        {
            int connStringId = _check.Data.TypeOptions[((int)Settings.ConnString).ToString()];
            var connString = _settings.ConnStrings.FirstOrDefault(x => x.ID == connStringId);
            if (connString == null || string.IsNullOrEmpty(connString.Value))
            {
                throw new Exception("Connection string invalid");
            }
            _logger.Info($"Using connection string {connString.Name}: {connString.Value}");
            return connString;
        }

        private string GetQuery()
        {
            string query = _check.Data.TypeOptions[((int)Settings.Query).ToString()];
            if (string.IsNullOrEmpty(query))
            {
                throw new Exception("Query invalid");
            }

            return query;
        }

        protected async Task<JArray> GetQueryResultAsJson(string connectionString, string query)
        {
            JArray jsonResult;

            using (var conn = new SqlConnection(connectionString))
            {
                using (var sqlCommand = new SqlCommand(query, conn))
                {
                    conn.Open();
                    try
                    {
                        var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                        jsonResult = await sqlDataReader.ToJArray();
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }

            return jsonResult;
        }

        private void RunSubCheck(SubCheck subCheck, JArray jsonResult, CheckResult checkResult)
        {
            switch (subCheck.TypeID)
            {
                case (int)SubCheckType.FieldEqualTo:
                    string fieldName = subCheck.Options[((int)FieldEqualTo.FieldName).ToString()];
                    bool exists = subCheck.Options[((int)FieldEqualTo.Exists).ToString()];
                    var actualValue = GetFieldValue(fieldName, exists, jsonResult);
                    string expectedValue = subCheck.Options[((int)FieldEqualTo.Value).ToString()];
                    VerifyAreEquals(expectedValue, actualValue, fieldName);
                    break;
                case (int)SubCheckType.FieldNotEqualTo:
                    fieldName = subCheck.Options[((int)FieldNotEqualTo.FieldName).ToString()];
                    exists = subCheck.Options[((int)FieldNotEqualTo.Exists).ToString()];
                    actualValue = GetFieldValue(fieldName, exists, jsonResult);
                    expectedValue = subCheck.Options[((int)FieldNotEqualTo.Value).ToString()];
                    VerifyAreNotEquals(expectedValue, actualValue, fieldName);
                    break;
                default:
                    _logger.Warn($"Unknown sub-check type {subCheck.TypeID} - ignoring");
                    break;
            }
        }

        private void VerifyAreEquals(string expectedValue, string actualValue, string columnName)
        {
            if (actualValue == expectedValue)
            {
                _logger.Info($"Field '{columnName}' equals the expected value of '{expectedValue}'");
            }
            else
            {
                throw new SubCheckException($"Field '{columnName}' does not equal the expected value of '{expectedValue}', the actual value is '{actualValue}'");
            }
        }

        private void VerifyAreNotEquals(string nonExpectedValue, string actualValue, string columnName)
        {
            if (actualValue != nonExpectedValue)
            {
                _logger.Info($"Field '{columnName}' does not equal the non-expected value of '{nonExpectedValue}', the actual value is '{actualValue}'");
            }
            else
            {
                throw new SubCheckException($"Field '{columnName}' equals the non-expected value of '{nonExpectedValue}'");
            }
        }

        private static string GetFieldValue(string fieldName, bool exists, JArray jsonResult)
        {
            var value = jsonResult.SelectToken(fieldName);

            if (value == null && exists)
            {
                throw new SubCheckException($"Field '{fieldName}' does not exist");
            }
            else if (value != null && !exists)
            {
                throw new SubCheckException($"Field '{fieldName}' exists");
            }

            return value?.ToString();
        }
    }
}