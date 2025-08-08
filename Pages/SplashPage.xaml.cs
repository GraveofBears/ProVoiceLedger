using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
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

            var recordingService = App.RecordingService;
            User? restoredUser = null;

            // 🔧 Cast to concrete type to access internal methods
            if (recordingService is RecordingService concrete)
            {
                try
                {
                    restoredUser = await concrete.TryRestoreUserAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ User restoration failed: {ex.Message}");
                }

                if (restoredUser != null)
                {
                    concrete.SetCurrentUser(restoredUser);
                    NavigateTo(new RecordingPage(concrete));
                    return;
                }
            }

            NavigateTo(new LoginPage());
        }

        private void NavigateTo(Page destination)
        {
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
