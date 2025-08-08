using BCrypt.Net;
using ProVoiceLedger.Core.Models;
using System;

namespace ProVoiceLedger.Core.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepository;

        public AuthService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public LoginResponse ValidateCredentials(LoginRequest request)
        {
            var user = _userRepository.GetUserByUsername(request.Username);

            if (user == null)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "User not found",
                    Token = string.Empty
                };
            }

            if (user.IsSuspended)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Account is suspended",
                    Token = string.Empty
                };
            }

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid password",
                    Token = string.Empty
                };
            }

            return new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                Token = GenerateSessionToken(user.Username),
            };
        }

        public static string GenerateSessionToken(string username)
        {
            // You’ll likely replace this with a JWT or encrypted token later
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public static bool ValidateSessionToken(string token)
        {
            // TODO: Add real validation with expiration and hash check
            return !string.IsNullOrWhiteSpace(token);
        }
    }
}
