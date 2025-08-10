using Microsoft.Maui.Controls;
using System;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.AudioBackup;

namespace ProVoiceLedger
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        public static IRecordingService RecordingService =>
            Services.GetRequiredService<IRecordingService>();

        public static SessionDatabase SessionDatabase =>
            Services.GetRequiredService<SessionDatabase>();

        public App(IServiceProvider services)
        {
            Services = services;
            InitializeComponent();

            // 🌒 Start with splash screen wrapped in navigation
            MainPage = new NavigationPage(new Pages.SplashPage());
        }
    }
}
