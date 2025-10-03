using Microsoft.Maui.Graphics;
using System;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.AudioBackup;

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
            try
            {
                Services = services;
                InitializeComponent();

                // Start with splash screen wrapped in navigation
                MainPage = new NavigationPage(new Pages.SplashPage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"App constructor error: {ex}");
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProVoiceLedger_app_crash.txt"),
                    $"App crash at {DateTime.Now}\n{ex}"
                );
                throw;
            }
        }
    }
}