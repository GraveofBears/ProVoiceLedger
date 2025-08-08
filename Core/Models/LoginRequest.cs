using System.Collections.Generic;

namespace ProVoiceLedger.Core.Models
{
    public class LoginRequest
    {
        /// <summary>
        /// The username or identifier used for login.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password or credential associated with the username.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Optional metadata for login context (e.g., device info, client version).
        /// </summary>
        public Dictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Default constructor for serialization or manual property assignment.
        /// </summary>
        public LoginRequest()
        {
            Username = string.Empty;
            Password = string.Empty;
        }

        /// <summary>
        /// Full constructor for initializing a login request.
        /// </summary>
        public LoginRequest(string username, string password, Dictionary<string, string>? metadata = null)
        {
            Username = username;
            Password = password;
            Metadata = metadata;
        }
    }
}
