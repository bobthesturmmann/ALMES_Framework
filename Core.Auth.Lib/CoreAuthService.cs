using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Service;

namespace Core.Auth.Lib
{
    public class CoreAuthService
    {
        private readonly SqlEngine _sqlEngine;
        private readonly string _companyCode;

        public CoreAuthService(SqlEngine sqlEngine, IConfiguration configuration)
        {
            _sqlEngine = sqlEngine;
            _companyCode = configuration["AlmesSettings:CompanyCode"] ?? "000";
        }

        public async Task<CoreAuthUser?> ValidateUserInDatabaseAsync(string username, string password)
        {
            var safeUsername = username.Trim().Replace("'", "''");
            var cleanPassword = password.Trim();

            string tableName = $"ALT_{_companyCode}_AUTH_USERS";
            string query = $"SELECT Id, Username, Email, PasswordHash FROM {tableName} WHERE Username = '{safeUsername}' AND IsActive = 1";

            List<CoreUserWithPassword> users = _sqlEngine.ExecuteCustomQuery(query, reader =>
            {
                return new CoreUserWithPassword
                {
                    User = new CoreAuthUser
                    {
                        UserId = Convert.ToInt32(reader["Id"]),
                        Username = reader["Username"].ToString() ?? string.Empty,
                        Email = reader["Email"].ToString() ?? string.Empty,
                        Roles = new List<string> { "Admin" }
                    },
                    PasswordHash = reader["PasswordHash"].ToString() ?? string.Empty
                };
            });

            if (users != null && users.Count > 0)
            {
                var matchedUser = users.First();
                if (cleanPassword == matchedUser.PasswordHash)
                {
                    return matchedUser.User;
                }
            }
            return null;
        }
    }

    public class CoreAuthUser
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    internal class CoreUserWithPassword
    {
        public CoreAuthUser User { get; set; } = null!;
        public string PasswordHash { get; set; } = string.Empty;
    }
}