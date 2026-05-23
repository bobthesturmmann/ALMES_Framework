using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Core.Service;
using _Core.Shared.Lib;

namespace _Auth.Lib
{
    public class AuthManager
    {
        private readonly SqlEngine _sqlEngine;
        private readonly string _companyCode;

        public AuthManager(SqlEngine sqlEngine, IConfiguration configuration)
        {
            _sqlEngine = sqlEngine;
            _companyCode = configuration["AlmesSettings:CompanyCode"] ?? "000";
        }

        public async Task<CurrentUserContext?> ValidateUserAsync(string username, string password)
        {
            var safeUsername = username.Trim().Replace("'", "''");
            var cleanPassword = password.Trim();

            string tableName = $"ALT_{_companyCode}_AUTH_USERS";
            string query = $"SELECT Id, Username, Email, PasswordHash FROM {tableName} WHERE Username = '{safeUsername}' AND IsActive = 1";

            List<CurrentUserContextWithPassword> users = _sqlEngine.ExecuteCustomQuery(query, reader =>
            {
                return new CurrentUserContextWithPassword
                {
                    Context = new CurrentUserContext
                    {
                        UserId = Convert.ToInt32(reader["Id"]),
                        Username = reader["Username"].ToString() ?? string.Empty,
                        Email = reader["Email"].ToString() ?? string.Empty,
                        Roles = new List<string> { AuthConstants.RoleAdmin }
                    },
                    PasswordHash = reader["PasswordHash"].ToString() ?? string.Empty
                };
            });

            if (users != null && users.Count > 0)
            {
                var matchedUser = users.First();

                if (cleanPassword == matchedUser.PasswordHash)
                {
                    matchedUser.Context.AuthenticationTime = DateTime.UtcNow;
                    return matchedUser.Context;
                }
            }

            return null;
        }
    }

    internal class CurrentUserContextWithPassword
    {
        public CurrentUserContext Context { get; set; } = null!;
        public string PasswordHash { get; set; } = string.Empty;
    }
}