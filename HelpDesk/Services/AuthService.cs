using HelpDesk.Data;
using HelpDesk.Helpers;
using HelpDesk.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace HelpDesk.Services
{
    public class AuthService
    {
        public User Authenticate(string login, string password)
        {
            const string sql = @"
                SELECT TOP (1)
                    u.Id, u.Login, u.FullName, u.RoleId,
                    r.Name AS RoleName, u.PasswordHash, u.IsActive
                FROM dbo.Users AS u
                INNER JOIN dbo.Roles AS r ON r.Id = u.RoleId
                WHERE u.Login = @Login;";

            using (SqlConnection connection = Database.GetConnection())
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add(
                    "@Login", SqlDbType.NVarChar, 50).Value = login;

                connection.Open();

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    if (!Convert.ToBoolean(reader["IsActive"]))
                        return null;

                    string storedHash =
                        Convert.ToString(reader["PasswordHash"]);

                    if (!PasswordHasher.VerifyPassword(password, storedHash))
                        return null;

                    User user = new User();
                    user.Id = Convert.ToInt32(reader["Id"]);
                    user.Login = Convert.ToString(reader["Login"]);
                    user.FullName = Convert.ToString(reader["FullName"]);
                    user.RoleId = Convert.ToInt32(reader["RoleId"]);
                    user.RoleName = Convert.ToString(reader["RoleName"]);
                    user.IsActive = true;

                    return user;
                }
            }
        }
    }
}