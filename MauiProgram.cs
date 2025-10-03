using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.IO;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Pages;

namespace ProVoiceLedger
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>() // ✅ This will use factory method below
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseSkiaSharp(); // ✅ SkiaSharp support for waveform rendering

            // 🔧 Core services
            builder.Services.AddSingleton<UserRepository>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<FileStorageService>();
            builder.Services.AddSingleton<CommunicationService>();
            builder.Services.AddSingleton<PipeServerService>();

            // 🎙️ Audio and recording services
            builder.Services.AddSingleton<IAudioEngine, MockAudioEngine>();
            builder.Services.AddSingleton<IAudioCaptureService, AudioCaptureService>();
            builder.Services.AddSingleton<IAudioPlaybackService, AudioPlaybackService>();

            // 🗂️ SQLite session database
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.db");
            builder.Services.AddSingleton(provider => new SessionDatabase(dbPath));

            // 🎛️ Recording service with injected audio capture
            builder.Services.AddSingleton<IRecordingService>(provider =>
            {
                var audioCapture = provider.GetRequiredService<IAudioCaptureService>();
                return new RecordingService(audioCapture);
            });

            builder.Services.AddSingleton<RecordingUploadService>();

            // 📄 Pages (transient for fresh state)
            builder.Services.AddTransient<RecordingPage>();
            builder.Services.AddTransient<GradientPage>(); // ✅ Added GradientPage
            builder.Services.AddTransient<RecordingListPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SessionHistoryPage>();
            builder.Services.AddTransient<SplashPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<MainTabbedPage>();

            // 🛠️ App entry point with dependency injection
            builder.Services.AddSingleton<App>(provider => new App(provider));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // 🚀 Start background pipe listener
            var pipeServer = app.Services.GetRequiredService<PipeServerService>();
            Task.Run(() => pipeServer.StartListenerAsync());

            return app;
        }
    }
}