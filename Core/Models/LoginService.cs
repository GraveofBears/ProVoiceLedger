using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ProVoiceLedger.Core.Models;
using System.Diagnostics;
using System.Linq;

namespace ProVoiceLedger.Core.Services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;
        private const string LoginEndpoint = "http://192.168.1.58:7290/api/auth/login";

        public LoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> AttemptLoginAsync(LoginRequest request)
        {
            try
            {
                // 🔍 Diagnostic Logging
                Debug.WriteLine($"🔍 Attempting login for: {request.Username}");
                Debug.WriteLine($"🔍 Raw password: '{request.Password}'");
                Debug.WriteLine($"🔍 Length: {request.Password?.Length}");
                Debug.WriteLine($"🔍 Char codes: {string.Join(",", request.Password?.Select(c => (int)c) ?? Enumerable.Empty<int>())}");

                var response = await _httpClient.PostAsJsonAsync(LoginEndpoint, request);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResponse(
                        success: false,
                        message: $"Server Error: {response.StatusCode}"
                    );
                }

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result?.Success == true && !string.IsNullOrEmpty(result.Token))
                {
                    await SecureStorage.SetAsync("auth_token", result.Token);
                }

                return result ?? new LoginResponse(
                    success: false,
                    message: "Unexpected server response"
                );
            }
            catch (Exception ex)
            {
                return new LoginResponse(
                    success: false,
                    message: $"Network Error: {ex.Message}"
                );
            }
        }

        public async Task<string?> TryGetStoredTokenAsync()
        {
            try
            {
                return await SecureStorage.GetAsync("auth_token");
            }
            catch
            {
                return null;
            }
        }

        public Task LogoutAsync()
        {
            SecureStorage.Remove("auth_token");
            return Task.CompletedTask;
        }
    }
}
