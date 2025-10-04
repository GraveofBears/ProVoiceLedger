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

            // Logo pulse animation
            LogoImage.Opacity = 0;
            await LogoImage.FadeTo(1, 1200, Easing.CubicInOut);

            await Task.Delay(2000); // Wait 2 seconds

            // For now, just show a message instead of navigating
            await DisplayAlert("Success", "App initialized successfully!", "OK");
        }
    }
}