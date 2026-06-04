using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Core.Service
{
    public class SqlEngine
    {
        public List<T> ExecuteRawQuery<T>(string connectionString, string sql, Func<IDataRecord, T> mapFunction)
        {
            var results = new List<T>();

            using (var connection = new SqlConnection(connectionString))
            {
                using (var command = new SqlCommand(sql, connection))
                {
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.Add(mapFunction(reader));
                        }
                    }
                }
            }

            return results;
        }
    }
}