using System;
using System.Collections.Generic;
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

        public List<T> ReadFromView<T>(string moduleName, string operationName, Func<IDataRecord, T> mapFunction, List<SqlParameter>? parameters = null)
        {
            string viewName = $"ALV_{_companyCode}_{moduleName}_{operationName}";
            return ExecuteQueryInternal($"SELECT * FROM {viewName}", mapFunction, parameters);
        }

        public List<T> ReadFromTable<T>(string moduleName, string operationName, Func<IDataRecord, T> mapFunction, List<SqlParameter>? parameters = null)
        {
            string tableName = $"ALT_{_companyCode}_{moduleName}_{operationName}";
            return ExecuteQueryInternal($"SELECT * FROM {tableName}", mapFunction, parameters);
        }

        private List<T> ExecuteQueryInternal<T>(string baseQuery, Func<IDataRecord, T> mapFunction, List<SqlParameter>? parameters)
        {
            string query = baseQuery;

            if (parameters is { Count: > 0 })
            {
                List<string> clauses = [];
                foreach (var param in parameters)
                {
                    string columnName = param.ParameterName.Replace("@", "");
                    clauses.Add($"{columnName} = {param.ParameterName}");
                }
                query += " WHERE " + string.Join(" AND ", clauses);
            }

            List<T> list = [];
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(query, connection);

            if (parameters is { Count: > 0 })
            {
                command.Parameters.AddRange([.. parameters]);
            }

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                list.Add(mapFunction(reader));
            }
            return list;
        }
    }
}