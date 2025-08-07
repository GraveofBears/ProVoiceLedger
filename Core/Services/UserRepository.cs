using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public class UserRepository
    {
        public User? GetUserByUsername(string username)
        {
            // Dummy for now
            return username == "admin"
                ? new User
                {
                    Id = "1",
                    Username = "admin",
                    PasswordHash = "password123",
                    DisplayName = "Administrator",
                    IsSuspended = false,
                    Role = "Admin"
                }
                : null;
        }
    }
}
