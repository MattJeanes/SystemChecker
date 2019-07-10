using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SystemChecker.Contracts.DTO;
using SystemChecker.Model.Data.Entities;
using SystemChecker.Model.Extensions;
using SystemChecker.Model.Helpers;

namespace SystemChecker.Model.Jobs
{
    public class DatabaseChecker : BaseChecker
    {
        public DatabaseChecker(ICheckerHelper helper) : base(helper) { }

        public class Settings
        {
            public int ConnectionString { get; set; }
            public string Query { get; set; }
        }

        public enum SubChecks
        {
            FieldEqualTo,
            FieldNotEqualTo,
            FieldGreaterThan,
            FieldLessThan
        }

        public class FieldCompare
        {
            public string FieldName { get; set; }
            public string Value { get; set; }
            public bool Exists { get; set; }
        }


        public override async Task<CheckResult> PerformCheck(CheckResult result)
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
            var connStringId = _check.Data.GetTypeOptions<Settings>().ConnectionString;
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
            var query = _check.Data.GetTypeOptions<Settings>().Query;
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
            switch (subCheck.Type.Identifier)
            {
                case nameof(SubChecks.FieldEqualTo):
                case nameof(SubChecks.FieldNotEqualTo):
                case nameof(SubChecks.FieldGreaterThan):
                case nameof(SubChecks.FieldLessThan):
                    var fieldCompareOptions = subCheck.GetOptions<FieldCompare>();
                    var fieldName = fieldCompareOptions.FieldName;
                    var exists = fieldCompareOptions.Exists;
                    var actualValue = GetFieldValue(fieldName, exists, jsonResult);
                    var expectedValue = fieldCompareOptions.Value;
                    switch (subCheck.Type.Identifier)
                    {
                        case nameof(SubChecks.FieldEqualTo):
                            VerifyAreEquals(expectedValue, actualValue, fieldName);
                            break;
                        case nameof(SubChecks.FieldNotEqualTo):
                            VerifyAreNotEquals(expectedValue, actualValue, fieldName);
                            break;
                        case nameof(SubChecks.FieldGreaterThan):
                            VerifyIsGreaterThan(expectedValue, actualValue, fieldName);
                            break;
                        case nameof(SubChecks.FieldLessThan):
                            VerifyIsLessThan(expectedValue, actualValue, fieldName);
                            break;
                    }
                    break;
                default:
                    _logger.Warn($"Unknown sub-check type {subCheck.TypeID} - ignoring");
                    break;
            }
        }

        private void VerifyIsLessThan(string expectedValueString, string actualValueString, string fieldName)
        {
            if (!int.TryParse(expectedValueString, out var expectedValue))
            {
                throw new SubCheckException($"Expected value of {expectedValueString} could not be parsed to int");
            }
            if (!int.TryParse(actualValueString, out var actualValue))
            {
                throw new SubCheckException($"Actual value of {actualValueString} could not be parsed to int");
            }
            {
                if (actualValue < expectedValue)
                {
                    _logger.Info($"Field '{fieldName}' is less than the expected value of '{expectedValue}'");
                }
                else
                {
                    throw new SubCheckException($"Field '{fieldName}' is not less than the expected value of '{expectedValue}', the actual value is '{actualValue}'");
                }
            }
        }

        private void VerifyIsGreaterThan(string expectedValueString, string actualValueString, string fieldName)
        {
            if (!int.TryParse(expectedValueString, out var expectedValue))
            {
                throw new SubCheckException($"Expected value of {expectedValueString} could not be parsed to int");
            }
            if (!int.TryParse(actualValueString, out var actualValue))
            {
                throw new SubCheckException($"Actual value of {actualValueString} could not be parsed to int");
            }
            {
                if (actualValue > expectedValue)
                {
                    _logger.Info($"Field '{fieldName}' is greater than the expected value of '{expectedValue}'");
                }
                else
                {
                    throw new SubCheckException($"Field '{fieldName}' is not greater than the expected value of '{expectedValue}', the actual value is '{actualValue}'");
                }
            }
        }

        private void VerifyAreEquals(string expectedValue, string actualValue, string fieldName)
        {
            if (actualValue == expectedValue)
            {
                _logger.Info($"Field '{fieldName}' equals the expected value of '{expectedValue}'");
            }
            else
            {
                throw new SubCheckException($"Field '{fieldName}' does not equal the expected value of '{expectedValue}', the actual value is '{actualValue}'");
            }
        }

        private void VerifyAreNotEquals(string nonExpectedValue, string actualValue, string fieldName)
        {
            if (actualValue != nonExpectedValue)
            {
                _logger.Info($"Field '{fieldName}' does not equal the non-expected value of '{nonExpectedValue}', the actual value is '{actualValue}'");
            }
            else
            {
                throw new SubCheckException($"Field '{fieldName}' equals the non-expected value of '{nonExpectedValue}'");
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