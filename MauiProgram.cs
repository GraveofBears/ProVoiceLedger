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
using Microsoft.Maui.LifecycleEvents;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Microsoft.UI.Windowing;
#endif

namespace ProVoiceLedger
{
    public static class MauiProgram
    {
        public static IServiceProvider? Services { get; private set; }

        public static MauiApp CreateMauiApp()
        {
            // Global unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"[GLOBAL] Unhandled exception: {e.ExceptionObject}");
            };

#if WINDOWS
            // First-chance exception handler to suppress WinUI resource errors
            AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
            {
                if (e.Exception is System.Runtime.InteropServices.COMException comEx)
                {
                    if (comEx.HResult == unchecked((int)0x80004005) && // E_FAIL
                        (e.Exception.Message.Contains("AcrylicBackgroundFillColorDefaultBrush") ||
                         e.Exception.Message.Contains("Cannot find a resource")))
                    {
                        // Silently suppress - this is WinUI looking for theme resources we don't use
                        return;
                    }
                }
            };
#endif

            Batteries_V2.Init();

            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
                    // Prevent automatic loading of WinUI XamlControlsResources
                    handlers.AddHandler(typeof(Microsoft.Maui.Controls.Application), typeof(Microsoft.Maui.Handlers.ApplicationHandler));
#endif
                })
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if WINDOWS
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(windows =>
                {
                    windows.OnWindowCreated(window =>
                    {
                        try
                        {
                            window.SystemBackdrop = null;

                            // Prevent WinUI from looking up missing system brushes
                            if (window.Content is FrameworkElement root)
                            {
                                root.Resources.MergedDictionaries.Clear();
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Window backdrop init error: {ex}");
                        }
                    });
                });
            });
#endif

            // Core services
            builder.Services.AddSingleton<UserRepository>(provider =>
                CreateWithDiagnostics(() => new UserRepository(), nameof(UserRepository)));

            builder.Services.AddSingleton<AuthService>(provider =>
                CreateWithDiagnostics(() => new AuthService(provider.GetRequiredService<UserRepository>()), nameof(AuthService)));

            builder.Services.AddSingleton<FileStorageService>(provider =>
                CreateWithDiagnostics(() => new FileStorageService(), nameof(FileStorageService)));

            builder.Services.AddSingleton<CommunicationService>(provider =>
                CreateWithDiagnostics(() =>
                    new CommunicationService(
                        provider.GetRequiredService<AuthService>(),
                        provider.GetRequiredService<FileStorageService>()),
                    nameof(CommunicationService)));

            builder.Services.AddSingleton<PipeServerService>(provider =>
                CreateWithDiagnostics(() =>
                    new PipeServerService(provider.GetRequiredService<CommunicationService>()),
                    nameof(PipeServerService)));

            // Audio services
            builder.Services.AddSingleton<IAudioEngine>(provider =>
                CreateWithDiagnostics(() => new MockAudioEngine(), nameof(MockAudioEngine)));

            builder.Services.AddSingleton<IAudioCaptureService>(provider =>
                CreateWithDiagnostics(() => new AudioCaptureService(), nameof(AudioCaptureService)));

            builder.Services.AddSingleton<IAudioPlaybackService>(provider =>
                CreateWithDiagnostics(() => new AudioPlaybackService(), nameof(AudioPlaybackService)));

            // Database
            string dbDirectory = FileSystem.AppDataDirectory;
            Directory.CreateDirectory(dbDirectory);
            string dbPath = Path.Combine(dbDirectory, "sessions.db");

            builder.Services.AddSingleton(provider =>
                CreateWithDiagnostics(() => new SessionDatabase(dbPath), nameof(SessionDatabase)));

            // Recording services
            builder.Services.AddSingleton<IRecordingService>(provider =>
                CreateWithDiagnostics(() =>
                    new RecordingService(provider.GetRequiredService<IAudioCaptureService>()),
                    nameof(RecordingService)));

            builder.Services.AddSingleton<RecordingUploadService>(provider =>
                CreateWithDiagnostics(() => new RecordingUploadService(), nameof(RecordingUploadService)));

            // Pages - Singletons for TabbedPage children, Transient for others
            builder.Services.AddSingleton<RecordingPage>(provider =>
                CreateWithDiagnostics(() => new RecordingPage(), nameof(RecordingPage)));

            builder.Services.AddSingleton<RecordingListPage>(provider =>
                CreateWithDiagnostics(() => new RecordingListPage(), nameof(RecordingListPage)));

            builder.Services.AddSingleton<SettingsPage>(provider =>
                CreateWithDiagnostics(() => new SettingsPage(), nameof(SettingsPage)));

            // Transient pages (not in tabbed view)
            builder.Services.AddTransient<GradientPage>(provider =>
                CreateWithDiagnostics(() => new GradientPage(), nameof(GradientPage)));

            builder.Services.AddTransient<LoginPage>(provider =>
                CreateWithDiagnostics(() => new LoginPage(), nameof(LoginPage)));

            builder.Services.AddTransient<SessionHistoryPage>(provider =>
                CreateWithDiagnostics(() => new SessionHistoryPage(provider.GetRequiredService<SessionDatabase>()), nameof(SessionHistoryPage)));

            builder.Services.AddTransient<SplashPage>(provider =>
                CreateWithDiagnostics(() => new SplashPage(), nameof(SplashPage)));

            builder.Services.AddTransient<MainTabbedPage>(provider =>
                CreateWithDiagnostics(() => new MainTabbedPage(), nameof(MainTabbedPage)));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();
            SetServiceProvider(app.Services);

            return app;
        }

        private static T CreateWithDiagnostics<T>(Func<T> factory, string name)
        {
            try
            {
                return factory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"{name} init error: {ex}");
                throw;
            }
        }

        public static void SetServiceProvider(IServiceProvider serviceProvider) => Services = serviceProvider;
    }
}