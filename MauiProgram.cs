using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp.Views.Maui.Controls.Hosting;
using System.IO;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;

namespace ProVoiceLedger;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>() // App.xaml.cs handles MainPage and splash flow
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .UseSkiaSharp(); // ✅ Registers SKCanvasView and other SkiaSharp handlers

        // 🔧 Core application services
        builder.Services.AddSingleton<UserRepository>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<FileStorageService>();
        builder.Services.AddSingleton<CommunicationService>();
        builder.Services.AddSingleton<PipeServerService>();

        // 🎙️ Audio and recording services
        builder.Services.AddSingleton<IAudioCaptureService, AudioCaptureService>();
        builder.Services.AddSingleton<IRecordingService, RecordingService>();
        builder.Services.AddSingleton<RecordingUploadService>();

        // 🗂️ SQLite session database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "sessions.db");
        builder.Services.AddSingleton(provider => new SessionDatabase(dbPath));

        // 📄 Active pages (transient for fresh instances)
        builder.Services.AddTransient<RecordingPage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SessionHistoryPage>();
        builder.Services.AddTransient<SplashPage>();

        // 🛠️ Central application setup
        builder.Services.AddSingleton<App>(provider =>
        {
            var db = provider.GetRequiredService<SessionDatabase>();
            var audioService = provider.GetRequiredService<IAudioCaptureService>();
            return new App(db, audioService);
        });

        var app = builder.Build();

        // 🚀 Start background pipe listener
        var pipeServer = app.Services.GetRequiredService<PipeServerService>();
        Task.Run(() => pipeServer.StartListenerAsync());

        return app;
    }
}
