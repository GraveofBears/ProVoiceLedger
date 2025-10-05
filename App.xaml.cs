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
            // Intercept and provide fallback for missing WinUI resources
            try
            {
                // Create fallback resources before InitializeComponent
                var fallbackResources = new ResourceDictionary();
                fallbackResources.Add("AcrylicBackgroundFillColorDefaultBrush", new SolidColorBrush(Colors.Black));
                fallbackResources.Add("AcrylicInAppFillColorDefaultBrush", new SolidColorBrush(Colors.Black));
                fallbackResources.Add("SystemControlAcrylicWindowBrush", new SolidColorBrush(Colors.Black));

                this.Resources.MergedDictionaries.Add(fallbackResources);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Resource fallback warning: {ex.Message}");
            }
#endif

            InitializeComponent();

#if WINDOWS
            Resources["WindowBackgroundBrush"] = new SolidColorBrush(Colors.Black);
#endif

            MainPage = new NavigationPage(new Pages.SplashPage())
            {
                BackgroundColor = (Color)Resources["WindowBackgroundColor"]
            };
        }
    }
}
