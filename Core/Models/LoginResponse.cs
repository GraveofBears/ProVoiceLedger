using System;
using System.Collections.Generic;

namespace ProVoiceLedger.Core.Models
{
    public class LoginResponse
    {
        /// <summary>
        /// Indicates whether the login attempt was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The authentication token returned upon successful login.
        /// </summary>
        public string? Token { get; set; }

        /// <summary>
        /// Optional message returned from the login attempt (e.g., error or status).
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// The unique identifier of the authenticated user.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// A list of roles or permissions associated with the user.
        /// </summary>
        public List<string>? Roles { get; set; }

        /// <summary>
        /// The expiration timestamp of the session token.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Default constructor for serialization or manual property assignment.
        /// </summary>
        public LoginResponse() { }

        /// <summary>
        /// Full constructor for initializing a complete login response.
        /// </summary>
        public LoginResponse(
            bool success,
            string? token = null,
            string? message = null,
            string? userId = null,
            List<string>? roles = null,
            DateTime? expiresAt = null)
        {
            Success = success;
            Token = token;
            Message = message;
            UserId = userId;
            ExpiresAt = expiresAt;
        }
    }
}
