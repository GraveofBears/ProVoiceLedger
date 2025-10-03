using Microsoft.UI.Xaml;
using System;

namespace ProVoiceLedger.Platforms.Windows;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
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
            System.Diagnostics.Debug.WriteLine($"Fatal startup error: {ex}");
            System.IO.File.WriteAllText(
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ProVoiceLedger_crash.txt"),
                $"Crash at {DateTime.Now}\n{ex}"
            );
            throw;
        }
    }
}

public partial class WinUIApp : MauiWinUIApplication
{
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