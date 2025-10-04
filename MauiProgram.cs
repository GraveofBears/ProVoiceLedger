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
        public static IServiceProvider Services { get; private set; } // Expose the service provider

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
            {
                try { return new UserRepository(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"UserRepository init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddSingleton<AuthService>(provider =>
            {
                try
                {
                    var userRepository = provider.GetRequiredService<UserRepository>();
                    return new AuthService(userRepository);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"AuthService init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddSingleton<FileStorageService>(provider =>
            {
                try { return new FileStorageService(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FileStorageService init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddSingleton<CommunicationService>(provider =>
            {
                try
                {
                    var authService = provider.GetRequiredService<AuthService>();
                    var fileStorageService = provider.GetRequiredService<FileStorageService>();
                    return new CommunicationService(authService, fileStorageService);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"CommunicationService init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddSingleton<PipeServerService>(provider =>
            {
                try
                {
                    var commService = provider.GetRequiredService<CommunicationService>();
                    return new PipeServerService(commService);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"PipeServerService init error: {ex}");
                    throw;
                }
            });

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
            {
                try { return new RecordingUploadService(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RecordingUploadService init error: {ex}");
                    throw;
                }
            });

            // Pages (transient, with try-catch for diagnostics)
            builder.Services.AddTransient<RecordingPage>(provider =>
            {
                try { return new RecordingPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RecordingPage init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddTransient<GradientPage>(provider =>
            {
                try { return new GradientPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"GradientPage init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddTransient<RecordingListPage>(provider =>
            {
                try { return new RecordingListPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"RecordingListPage init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddTransient<LoginPage>(provider =>
            {
                try { return new LoginPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"LoginPage init error: {ex}");
                    throw;
                }
            });
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
            {
                try { return new SplashPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SplashPage init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddTransient<SettingsPage>(provider =>
            {
                try { return new SettingsPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"SettingsPage init error: {ex}");
                    throw;
                }
            });
            builder.Services.AddTransient<MainTabbedPage>(provider =>
            {
                try { return new MainTabbedPage(); }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"MainTabbedPage init error: {ex}");
                    throw;
                }
            });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            // var pipeServer = app.Services.GetRequiredService<PipeServerService>();
            // Task.Run(() => pipeServer.StartListenerAsync());

            return app;
        }

        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }
    }
}