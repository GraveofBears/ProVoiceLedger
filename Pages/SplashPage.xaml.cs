using Microsoft.Maui.Graphics;
using System.Threading.Tasks;
using System.Reflection;

namespace ProVoiceLedger.Pages
{
    public partial class SplashPage : ContentPage
    {
        public string AppVersion { get; set; }

        public SplashPage()
        {
            InitializeComponent();

            // Bind version label
            AppVersion = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0"}";
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // 🌒 Logo spectral pulse animation
            LogoImage.Opacity = 0;
            await LogoImage.FadeTo(1, 1200, Easing.CubicInOut); // Fade in

            // Optional: ambient audio trigger or glow overlay here

            await Task.Delay(400); // Let animation settle

            // 🎯 Navigate to login page
            await Navigation.PushAsync(new LoginPage());
        }
    }
}
