using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Core.Service
{
    public class SqlEngine
    {
        private readonly string _connectionString;
        private readonly string _companyCode;

        public SqlEngine(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _companyCode = configuration["AlmesSettings:CompanyCode"] ?? "000";
        }

        public List<T> ReadFromView<T>(string moduleName, string operationName, Func<IDataRecord, T> mapFunction)
        {
            string viewName = $"ALV_{_companyCode}_{moduleName}_{operationName}";
            string query = $"SELECT * FROM {viewName}";

            return ExecuteCustomQuery(query, mapFunction);
        }

        public List<T> ExecuteCustomQuery<T>(string query, Func<IDataRecord, T> mapFunction)
        {
            var list = new List<T>();

            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(mapFunction(reader));
                        }
                    }
                }
            }
            return list;
        }
    }
}