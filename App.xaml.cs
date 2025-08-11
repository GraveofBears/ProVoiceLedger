using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;
using ProVoiceLedger.AudioBackup;

namespace ProVoiceLedger;

public partial class App : Application
{
    // 🔒 Directly injected services
    public static SessionDatabase SessionDb { get; private set; } = default!;
    public static IAudioCaptureService AudioService { get; private set; } = default!;

    public App(SessionDatabase db, IAudioCaptureService audioCaptureService)
    {
        InitializeComponent();

        // 🧩 Assign injected services
        SessionDb = db;
        AudioService = audioCaptureService;

        // 🧭 Set initial navigation
        MainPage = new NavigationPage(new SplashPage());
    }

    // 🧰 Access DI container
    private static IServiceProvider? Services =>
        Current?.Handler?.MauiContext?.Services;

    // 🎙️ Recording service (DI-resolved)
    public static IRecordingService? RecordingService =>
        Services?.GetService<IRecordingService>();

    // 🔊 Playback service (DI-resolved)
    public static IAudioPlaybackService? PlaybackService =>
        Services?.GetService<IAudioPlaybackService>();

    // 📦 Session database (DI-resolved fallback)
    public static SessionDatabase? SessionDatabase =>
        Services?.GetService<SessionDatabase>();
}
