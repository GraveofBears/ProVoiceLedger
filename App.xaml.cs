using Microsoft.Maui.Controls;
using System;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.AudioBackup;

#if WINDOWS
using Microsoft.Maui.Graphics;
#endif

namespace ProVoiceLedger
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public static IRecordingService RecordingService =>
            Services.GetRequiredService<IRecordingService>();

        public static SessionDatabase SessionDatabase =>
            Services.GetRequiredService<SessionDatabase>();

        public App(IServiceProvider services)
        {
            Services = services;

#if WINDOWS
            // Inject fallback brushes to prevent WinUI resource errors
            try
            {
                var fallbackResources = new ResourceDictionary
                {
                    { "AcrylicBackgroundFillColorDefaultBrush", new SolidColorBrush(Color.FromArgb("#CC000000")) },
                    { "AcrylicInAppFillColorDefaultBrush", new SolidColorBrush(Color.FromArgb("#CC000000")) },
                    { "SystemControlAcrylicWindowBrush", new SolidColorBrush(Color.FromArgb("#CC000000")) }
                };

                Application.Current.Resources.MergedDictionaries.Add(fallbackResources);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Resource fallback warning: {ex.Message}");
            }
#endif

            InitializeComponent();

#if WINDOWS
            // Optional: set a default window background brush
            Resources["WindowBackgroundBrush"] = new SolidColorBrush(Colors.Black);
#endif

            MainPage = new NavigationPage(new Pages.SplashPage())
            {
                BackgroundColor = (Color)Resources["WindowBackgroundColor"]
            };
        }
    }
}
