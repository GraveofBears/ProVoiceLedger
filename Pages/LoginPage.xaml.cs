using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginService _loginService;

        public LoginPage()
        {
            InitializeComponent();

            var httpClient = new HttpClient();
            _loginService = new LoginService(httpClient);
        }

        private async void OnLoginButtonClicked(object sender, EventArgs e)
        {
            string username = UsernameEntry?.Text?.Trim() ?? string.Empty;
            string rawPassword = PasswordEntry?.Text ?? string.Empty;
            string password = rawPassword.Trim().Normalize(NormalizationForm.FormC);

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Missing Info", "Please enter both username and password.", "OK");
                return;
            }

            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            try
            {
                var result = await _loginService.AttemptLoginAsync(request);

                if (!string.IsNullOrEmpty(result?.Token))
                {
                    var user = new User
                    {
                        Id = result.UserId ?? Guid.NewGuid().ToString(),
                        Username = username,
                        PasswordHash = string.Empty,
                        DisplayName = username,
                        IsSuspended = false
                    };

                    try
                    {
                        await SecureStorage.SetAsync("auth_token", result.Token ?? string.Empty);
                    }
                    catch (Exception tokenEx)
                    {
                        Console.WriteLine($"Token save failed: {tokenEx.Message}");
                    }

                    App.RecordingService.SetCurrentUser(user); // ✅ Works if interface includes it

                    var mainTabs = new MainTabbedPage();

                    Application.Current?.Dispatcher.Dispatch(() =>
                    {
                        Application.Current.MainPage = new NavigationPage(mainTabs);
                    });
                }
                else
                {
                    await DisplayAlert("Login Failed", result?.Message ?? "Invalid credentials", "Try Again");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Login error: {ex.Message}", "Close");
            }
        }
    }
}
