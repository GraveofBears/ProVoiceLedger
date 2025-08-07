using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using ProVoiceLedger.Core.Models;


namespace ProVoiceLedger.Pages
{
    public partial class RecordingPage : ContentPage
    {
        private readonly RecordingService _recordingService;
        private readonly Timer _waveformTimer;
        private DateTime _recordingStartTime;
        private bool _isRecording;

        public RecordingPage(RecordingService recordingService)
        {
            InitializeComponent();
            _recordingService = recordingService;

            _waveformTimer = new Timer(500); // update every 0.5s
            _waveformTimer.Elapsed += UpdateWaveform;
        }

        private async void OnRecordButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var metadata = new Dictionary<string, string>
                {
                    { "Session", "Dictation" },
                    { "StartedAt", DateTime.UtcNow.ToString("o") }
                };

                await _recordingService.StartRecordingAsync("DictateSession", metadata);
                _recordingStartTime = DateTime.UtcNow;
                _isRecording = true;
                _waveformTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartRecordingAsync error: {ex.Message}");
                await DisplayAlert("Error", "Failed to start recording.", "OK");
            }
        }

        private async void OnStopButtonClicked(object sender, EventArgs e)
        {
            try
            {
                _waveformTimer.Stop();
                _isRecording = false;

                using var stream = await _recordingService.GetLastRecordingAsync();

                var metadata = new Dictionary<string, string>
                {
                    { "StoppedAt", DateTime.UtcNow.ToString("o") }
                };

                await _recordingService.SaveRecordingAsync("DictateSession", stream, metadata);
                await DisplayAlert("Saved", "Recording saved successfully.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveRecordingAsync error: {ex.Message}");
                await DisplayAlert("Error", "Failed to save recording.", "OK");
            }
        }

        private async void OnPlayButtonClicked(object sender, EventArgs e)
        {
            try
            {
                using var stream = await _recordingService.GetLastRecordingAsync();
                await _recordingService.PlayRecordingAsync(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PlayRecordingAsync error: {ex.Message}");
                await DisplayAlert("Error", "Failed to play recording.", "OK");
            }
        }

        private void UpdateWaveform(object sender, ElapsedEventArgs e)
        {
            if (!_isRecording)
                return;

            Dispatcher.Dispatch(() =>
            {
                var elapsed = DateTime.UtcNow - _recordingStartTime;
                WaveformBar.Progress = new Random().NextDouble(); // Simulated waveform
                TimestampLabel.Text = $"{elapsed:mm\\:ss} / ∞";
            });
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
