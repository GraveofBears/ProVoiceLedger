using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ProVoiceLedger.Core.Models;
using System;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class SplashPage : ContentPage
    {
        public string AppVersion => $"v{AppInfo.VersionString}";

        public SplashPage()
        {
            InitializeComponent();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await AnimateLogoIntro();
            await Task.Delay(1000);

            string? token = null;
            try
            {
                token = await SecureStorage.GetAsync("auth_token");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SecureStorage error: {ex.Message}");
            }

            Page destination;
            if (!string.IsNullOrEmpty(token))
            {
                var restoredUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Username = "RestoredUser",
                    PasswordHash = string.Empty,
                    DisplayName = "Restored User",
                    Role = "User",
                    IsSuspended = false
                };

                destination = new RecordingPage(App.AudioService, App.SessionDb, restoredUser);
            }
            else
            {
                destination = new LoginPage();
            }

            Application.Current?.Dispatcher.Dispatch(() =>
            {
                Application.Current.MainPage = new NavigationPage(destination);
            });
        }

        private async Task AnimateLogoIntro()
        {
            if (LogoImage == null) return;

            LogoImage.Opacity = 0;
            LogoImage.Scale = 0.5;

            await LogoImage.FadeTo(1, 600, Easing.CubicInOut);
            await LogoImage.ScaleTo(1.0, 600, Easing.CubicInOut);
            await LogoImage.ScaleTo(1.05, 120, Easing.SinOut);
            await LogoImage.ScaleTo(1.0, 120, Easing.SinIn);
        }
    }
}
