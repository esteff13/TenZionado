using System.Data;
using System.Globalization;
using Microsoft.Data.SqlClient;
namespace CampusEvents;

// Task 4 refactor: normalized schema stores Email in Users, not Registrations.
public sealed class RegistrationService(string connectionString) {
    // Returns the most recent registration ID, or null if no match exists.
    public string? GetUserRegistration(string inputEmail) {
        if (!RegistrationValidator.IsStudentEmail(inputEmail))
            throw new ArgumentException("A valid university email is required.", nameof(inputEmail));
        using (var conn = new SqlConnection(connectionString)) {
            using (var cmd = new SqlCommand("""
                SELECT TOP (1) r.RegistrationId
                FROM dbo.Registrations AS r
                JOIN dbo.Users AS u ON u.UserId = r.UserId
                WHERE u.Email = @Email
                ORDER BY r.RegisteredAt DESC, r.RegistrationId DESC;
                """, conn)) {
                cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 254).Value = RegistrationValidator.NormalizeEmail(inputEmail);
                conn.Open();
                var result = cmd.ExecuteScalar();
                return result is null or DBNull ? null : Convert.ToString(result, CultureInfo.InvariantCulture);
            }
        }
    }
}
