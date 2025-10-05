using Microsoft.UI.Xaml;
using System;
using System.Runtime.InteropServices;

namespace ProVoiceLedger.Platforms.Windows;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // Suppress WinRT exceptions before COM initialization
        AppDomain.CurrentDomain.FirstChanceException += (sender, e) =>
        {
            if (e.Exception is COMException comEx && comEx.HResult == unchecked((int)0x80004005))
            {
                var msg = e.Exception.Message;
                if (msg.Contains("AcrylicBackgroundFillColorDefaultBrush") ||
                    msg.Contains("Cannot find a resource"))
                {
                    // Suppress silently
                    return;
                }
            }
        };

        try
        {
            WinRT.ComWrappersSupport.InitializeComWrappers();
            Microsoft.UI.Xaml.Application.Start((p) =>
            {
                var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(
                    Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
                System.Threading.SynchronizationContext.SetSynchronizationContext(context);
                _ = new WinUIApp();
            });
        }
        catch (Exception ex)
        {
            // Only log non-WinUI resource exceptions
            if (!ex.Message.Contains("AcrylicBackgroundFillColorDefaultBrush"))
            {
                System.Diagnostics.Debug.WriteLine($"Fatal startup error: {ex}");
                System.IO.File.WriteAllText(
                    System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProVoiceLedger_crash.txt"),
                    $"Crash at {DateTime.Now}\n{ex}"
                );
                throw;
            }
        }
    }
}

public partial class WinUIApp : MauiWinUIApplication
{
    public WinUIApp()
    {
        // Suppress WinUI resource lookup exceptions at app level
        this.UnhandledException += (s, e) =>
        {
            if (e.Message.Contains("AcrylicBackgroundFillColorDefaultBrush") ||
                e.Message.Contains("Cannot find a resource"))
            {
                e.Handled = true;
            }
        };
    }

    protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
    {
        try
        {
            return MauiProgram.CreateMauiApp();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating MAUI app: {ex}");
            throw;
        }
    }
}