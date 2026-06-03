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

        public SqlEngine(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<T> ExecuteProcedure<T>(
            string moduleName,
            string operationName,
            Func<IDataRecord, T> mapFunction,
            List<SqlParameter>? parameters = null)
        {
            string procName = $"ALP_{moduleName}_{operationName}";

            List<T> list = [];
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand(procName, connection);
            command.CommandType = CommandType.StoredProcedure;

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