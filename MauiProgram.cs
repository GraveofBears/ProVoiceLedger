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
using SQLitePCL;

namespace ProVoiceLedger
{
    public static class MauiProgram
    {
        // Fix for CS8618: Non-nullable property 'Services' must contain a non-null value when exiting constructor.
        // Make the property nullable to satisfy the compiler and avoid startup exceptions.
        public static IServiceProvider? Services { get; private set; } // Expose the service provider

        public static MauiApp CreateMauiApp()
        {
            // Global unhandled exception handler for diagnostics
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[GLOBAL] Unhandled exception: {e.ExceptionObject}");
            };

            SQLitePCL.Batteries_V2.Init(); // SQLite initialization

            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseSkiaSharp();

            // Core services with try-catch for diagnostics
            builder.Services.AddSingleton<UserRepository>(provider =>
                CreateWithDiagnostics(() => new UserRepository(), nameof(UserRepository)));
            builder.Services.AddSingleton<AuthService>(provider =>
                CreateWithDiagnostics(() => new AuthService(provider.GetRequiredService<UserRepository>()), nameof(AuthService)));
            builder.Services.AddSingleton<FileStorageService>(provider =>
                CreateWithDiagnostics(() => new FileStorageService(), nameof(FileStorageService)));
            builder.Services.AddSingleton<CommunicationService>(provider =>
                CreateWithDiagnostics(() => new CommunicationService(
                    provider.GetRequiredService<AuthService>(),
                    provider.GetRequiredService<FileStorageService>()), nameof(CommunicationService)));
            builder.Services.AddSingleton<PipeServerService>(provider =>
                CreateWithDiagnostics(() => new PipeServerService(provider.GetRequiredService<CommunicationService>()), nameof(PipeServerService)));

            // Audio and recording services with try-catch
            builder.Services.AddSingleton<IAudioEngine>(provider =>
            {
                try { return new MockAudioEngine(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MockAudioEngine init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddSingleton<IAudioCaptureService>(provider =>
            {
                try { return new AudioCaptureService(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AudioCaptureService init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddSingleton<IAudioPlaybackService>(provider =>
            {
                try { return new AudioPlaybackService(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AudioPlaybackService init error: {ex}");
                    throw;
                }
            });

            // SQLite session database
            string dbDirectory = FileSystem.AppDataDirectory;
            if (!Directory.Exists(dbDirectory))
            {
                Directory.CreateDirectory(dbDirectory);
            }
            string dbPath = Path.Combine(dbDirectory, "sessions.db");
            builder.Services.AddSingleton(provider =>
            {
                try
                {
                    return new SessionDatabase(dbPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SessionDatabase init error: {ex}");
                    throw;
                }
            });

            // Recording service
            builder.Services.AddSingleton<IRecordingService>(provider =>
            {
                try
                {
                    var audioCapture = provider.GetRequiredService<IAudioCaptureService>();
                    return new RecordingService(audioCapture);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RecordingService init error: {ex}");
                    throw;
                }
            });

            builder.Services.AddSingleton<RecordingUploadService>(provider =>
                CreateWithDiagnostics(() => new RecordingUploadService(), nameof(RecordingUploadService)));

            // Pages (transient, with try-catch for diagnostics)
            builder.Services.AddTransient<RecordingPage>(provider =>
                CreateWithDiagnostics(() => new RecordingPage(), nameof(RecordingPage)));
            builder.Services.AddTransient<GradientPage>(provider =>
                CreateWithDiagnostics(() => new GradientPage(), nameof(GradientPage)));
            builder.Services.AddTransient<RecordingListPage>(provider =>
                CreateWithDiagnostics(() => new RecordingListPage(), nameof(RecordingListPage)));
            builder.Services.AddTransient<LoginPage>(provider =>
                CreateWithDiagnostics(() => new LoginPage(), nameof(LoginPage)));
            builder.Services.AddTransient<SessionHistoryPage>(provider =>
            {
                try
                {
                    var sessionDb = provider.GetRequiredService<SessionDatabase>();
                    return new SessionHistoryPage(sessionDb);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SessionHistoryPage init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddTransient<SplashPage>(provider =>
                CreateWithDiagnostics(() => new SplashPage(), nameof(SplashPage)));
            builder.Services.AddTransient<SettingsPage>(provider =>
                CreateWithDiagnostics(() => new SettingsPage(), nameof(SettingsPage)));
            builder.Services.AddTransient<MainTabbedPage>(provider =>
                CreateWithDiagnostics(() => new MainTabbedPage(), nameof(MainTabbedPage)));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            MauiProgram.SetServiceProvider(app.Services);

            // var pipeServer = app.Services.GetRequiredService<PipeServerService>();
            // Task.Run(() => pipeServer.StartListenerAsync());

            return app;
        }

        // Helper methods for service registration with diagnostics
        private static T CreateWithDiagnostics<T>(Func<T> factory, string name)
        {
            try { return factory(); }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{name} init error: {ex}");
                throw;
            }
        }

        // Expression-bodied member for SetServiceProvider
        public static void SetServiceProvider(IServiceProvider serviceProvider) => Services = serviceProvider;
    }
}