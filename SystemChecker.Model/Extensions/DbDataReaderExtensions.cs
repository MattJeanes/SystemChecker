using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace SystemChecker.Model.Extensions
{
    public static class DbDataReaderExtensions
    {
        /// <summary>
        /// Returns a json string, as a json array, from a SQL data reader row(s) dataset.
        /// Adapted from https://stackoverflow.com/questions/33450298/retrieving-data-from-a-database-through-sqldatareader-and-converting-it-to-json
        /// </summary>
        public async static Task<string> ToJson(this DbDataReader dbDataReader)
        {
            var serializedRows = await dbDataReader.GetSerializedRows();

            var json = JsonConvert.SerializeObject(serializedRows, Formatting.Indented);

            return json;
        }

        private async static Task<IEnumerable<Dictionary<string, object>>> GetSerializedRows(this DbDataReader dbDataReader)
        {
            var serializedRows = new List<Dictionary<string, object>>();
            var columns = new List<string>();

            for (var i = 0; i < dbDataReader.FieldCount; i++)
            {
                columns.Add(dbDataReader.GetName(i));
            }

            while (await dbDataReader.ReadAsync())
            {
                var serializedRow = GetSerializedRow(columns, dbDataReader);
                serializedRows.Add(serializedRow);
            }

            return serializedRows;
        }

        private static Dictionary<string, object> GetSerializedRow(IEnumerable<string> columns, DbDataReader dbDataReader)
        {
            var serializedRow = new Dictionary<string, object>();

            foreach (var column in columns)
            {
                serializedRow.Add(column, dbDataReader[column]);
            }

            return serializedRow;
        }
    }
}
