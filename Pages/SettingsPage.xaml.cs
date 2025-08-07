using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            try
            {
                await SecureStorage.SetAsync("auth_token", string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SecureStorage error: {ex.Message}");
            }

            Application.Current?.Dispatcher.Dispatch(() =>
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            });
        }
    }
}
