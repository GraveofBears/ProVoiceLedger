using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using System;

namespace ProVoiceLedger.Pages
{
    public partial class RecordingListPage : ContentPage
    {
        private readonly SessionDatabase _sessionDatabase;
        private readonly IRecordingService _recordingService;

        // ✅ Parameterless constructor for XAML instantiation
        public RecordingListPage() : this(App.SessionDatabase, App.RecordingService) { }

        // ✅ Constructor with DI for manual instantiation or testing
        public RecordingListPage(SessionDatabase sessionDatabase, IRecordingService recordingService)
        {
            InitializeComponent();
            _sessionDatabase = sessionDatabase ?? throw new ArgumentNullException(nameof(sessionDatabase));
            _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));

            LoadRecordings();
        }

        // 📦 Load all saved recordings into the view
        private async void LoadRecordings()
        {
            try
            {
                var clips = await _sessionDatabase.GetAllRecordingsAsync();
                RecordingsView.ItemsSource = clips;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading recordings: {ex}");
                await DisplayAlert("Error", "Failed to load recordings.", "OK");
            }
        }

        // ▶️ Play selected recording
        private async void OnPlayClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is RecordedClipInfo clip)
            {
                try
                {
                    await _recordingService.PlayRecordingAsync(clip.FilePath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Playback error: {ex}");
                    await DisplayAlert("Error", "Failed to play recording.", "OK");
                }
            }
        }
    }
}
