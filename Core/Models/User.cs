namespace ProVoiceLedger.Core.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsSuspended { get; set; }
    }
}
