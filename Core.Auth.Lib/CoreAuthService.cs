using Core.Service;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core.Auth.Lib
{
    public class CoreAuthService(SqlEngine sqlEngine)
    {
        private readonly SqlEngine _sqlEngine = sqlEngine;

        public CoreAuthUser? ValidateUserInDatabase(string username, string password)
        {
            var cleanUsername = username.Trim();
            var cleanPassword = password.Trim();

            List<SqlParameter> parameters = [
                new SqlParameter("@IslemTipi", 1),
                new SqlParameter("@Username", cleanUsername)
            ];

            List<CoreUserWithPassword> users = _sqlEngine.ExecuteProcedure(
                "AUTH",
                "USERS",
                (IDataRecord row) => new CoreUserWithPassword
                {
                    User = new CoreAuthUser
                    {
                        UserId = Convert.ToInt32(row["Id"]),
                        Username = row["Username"].ToString() ?? string.Empty,
                        Email = row["Email"].ToString() ?? string.Empty,
                        Roles = ["Admin"],
                        CookieScheme = "AlmesSecureCookie"
                    },
                    PasswordHash = row["PasswordHash"].ToString() ?? string.Empty
                },
                parameters
            );

            if (users is { Count: > 0 })
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
        public List<string> Roles { get; set; } = [];
        public string CookieScheme { get; set; } = string.Empty;
    }

    public class CoreUserWithPassword
    {
        public CoreAuthUser User { get; set; } = null!;
        public string PasswordHash { get; set; } = string.Empty;
    }
}