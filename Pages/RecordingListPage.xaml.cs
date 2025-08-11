using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using System;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class RecordingListPage : ContentPage
    {
        private readonly SessionDatabase _sessionDatabase;
        private readonly IRecordingService _recordingService;

        public RecordingListPage() : this(App.SessionDatabase, App.RecordingService) { }

        public RecordingListPage(SessionDatabase sessionDatabase, IRecordingService recordingService)
        {
            InitializeComponent();
            _sessionDatabase = sessionDatabase ?? throw new ArgumentNullException(nameof(sessionDatabase));
            _recordingService = recordingService ?? throw new ArgumentNullException(nameof(recordingService));

            LoadRecordings();
        }

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

        // ✏️ Edit waveform
        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is RecordedClipInfo clip)
            {
                await Navigation.PushAsync(new AudioEditorPage(clip));
            }
        }

        // 📤 Send or export
        private async void OnSendClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is RecordedClipInfo clip)
            {
                try
                {
                    await DisplayAlert("Send", $"Stub: send {clip.Title}", "OK");
                    // TODO: Implement actual sharing/export logic
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send error: {ex}");
                    await DisplayAlert("Error", "Failed to send recording.", "OK");
                }
            }
        }

        // 📝 Rename recording
        private async void OnRenameClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is RecordedClipInfo clip)
            {
                string newTitle = await DisplayPromptAsync("Rename", "Enter new title:", initialValue: clip.Title);
                if (!string.IsNullOrWhiteSpace(newTitle))
                {
                    clip.Title = newTitle;
                    await _sessionDatabase.UpdateRecordingAsync(clip);
                    LoadRecordings();
                }
            }
        }
    }
}
