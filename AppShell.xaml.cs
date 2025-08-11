using Microsoft.Maui.Controls;
using System;
using System.Diagnostics;
using System.IO;

namespace ProVoiceLedger;

public partial class AppShell : Shell
{
    public AppShell()
    {
        try
        {
            Log("🚪 Entered AppShell constructor");

            InitializeComponent();

            Routing.RegisterRoute("login", typeof(Pages.LoginPage));
            Routing.RegisterRoute("main", typeof(Pages.RecordingPage));
            Routing.RegisterRoute("history", typeof(Pages.SessionHistoryPage));

            Log("✅ AppShell InitializeComponent succeeded");
        }
        catch (Exception ex)
        {
            LogFatal("⛔ AppShell crash", ex);
        }
    }

    private void Log(string message)
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string logPath = Path.Combine(FileSystem.AppDataDirectory, "launchlog.txt");
        File.AppendAllText(logPath, $"{timestamp} - {message}{Environment.NewLine}");
        Debug.WriteLine($"{timestamp}: {message}");
    }

    private void LogFatal(string prefix, Exception ex)
    {
        string fatalPath = Path.Combine(FileSystem.AppDataDirectory, "fatal.txt");
        File.WriteAllText(fatalPath, $"{prefix} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}\n");
        Debug.WriteLine($"{prefix}: {ex}");
    }
}