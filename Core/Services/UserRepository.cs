using ProVoiceLedger.Core.Models;
using BCrypt.Net;

namespace ProVoiceLedger.Core.Services
{
    public class UserRepository
    {
        public User? GetUserByUsername(string username)
        {
            // Dummy user with properly hashed password
            // Password is "password123"
            return username == "admin"
                ? new User
                {
                    Id = "1",
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    DisplayName = "Administrator",
                    IsSuspended = false,
                }
                : null;
        }
    }
}