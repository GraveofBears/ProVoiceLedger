using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Pages;
using ProVoiceLedger.AudioBackup;


namespace ProVoiceLedger;

public partial class App : Application
{
    public static SessionDatabase SessionDb { get; private set; } = default!;
    public static IAudioCaptureService AudioService { get; private set; } = default!;

    public App(SessionDatabase db, IAudioCaptureService audioCaptureService)
    {
        InitializeComponent();

        SessionDb = db;
        AudioService = audioCaptureService;

        // ✅ Wrap SplashPage in a NavigationPage
        MainPage = new NavigationPage(new SplashPage());
    }
}
