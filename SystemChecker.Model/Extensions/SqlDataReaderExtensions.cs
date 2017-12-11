using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SystemChecker.Model.Extensions
{
    public static class SqlDataReaderExtensions
    {
        /// <summary>
        /// Returns a json string, as a json array, from a sql data reader row(s) dataset.
        /// Adapted from https://stackoverflow.com/questions/33450298/retrieving-data-from-a-database-through-sqldatareader-and-converting-it-to-json
        /// </summary>
        public async static Task<string> ToJson(this SqlDataReader sqlDataReader)//gregt replace with DbDataReader ?
        {
            var serializedRows = await sqlDataReader.GetSerializedRows();

            var json = JsonConvert.SerializeObject(serializedRows, Formatting.Indented);

            return json;
        }

        private async static Task<IEnumerable<Dictionary<string, object>>> GetSerializedRows(this SqlDataReader sqlDataReader)
        {
            var serializedRows = new List<Dictionary<string, object>>();
            var columns = new List<string>();

            for (var i = 0; i < sqlDataReader.FieldCount; i++)
            {
                columns.Add(sqlDataReader.GetName(i));
            }

            while (await sqlDataReader.ReadAsync())
            {
                var serializedRow = GetSerializedRow(columns, sqlDataReader);
                serializedRows.Add(serializedRow);
            }

            return serializedRows;
        }

        private static Dictionary<string, object> GetSerializedRow(IEnumerable<string> columns, SqlDataReader sqlDataReader)
        {
            var serializedRow = new Dictionary<string, object>();

            foreach (var column in columns)
            {
                serializedRow.Add(column, sqlDataReader[column]);
            }

            return serializedRow;
        }
    }
}
