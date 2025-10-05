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
            System.Diagnostics.Debug.WriteLine(">>> SplashPage: OnAppearing called");

            LogoImage.Opacity = 0;
            await LogoImage.FadeTo(1, 1200, Easing.CubicInOut);
            System.Diagnostics.Debug.WriteLine(">>> SplashPage: Logo fade complete");

            await Task.Delay(2000);
            System.Diagnostics.Debug.WriteLine(">>> SplashPage: Navigating to LoginPage");

            await Navigation.PushAsync(new LoginPage());
            System.Diagnostics.Debug.WriteLine(">>> SplashPage: Navigation complete");
        }
    }
}