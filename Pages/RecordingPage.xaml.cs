using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Audio;
using System;
using System.Collections.Generic;
using System.Timers;

namespace ProVoiceLedger.Pages
{
    public partial class RecordingPage : ContentPage
    {
        private readonly IRecordingService _recordingService;
        private readonly System.Timers.Timer _waveformTimer;
        private readonly PlaybackController _playbackController;
        private DateTime _recordingStartTime;
        private bool _isRecording;

        public RecordingPage() : this(App.RecordingService) { }

        public RecordingPage(IRecordingService recordingService)
        {
            InitializeComponent();
            _recordingService = recordingService;

            _waveformTimer = new System.Timers.Timer(500);
            _waveformTimer.Elapsed += OnWaveformTimerElapsed;

            _playbackController = new PlaybackController(_recordingService);
            _playbackController.PlaybackProgressUpdated += OnPlaybackProgressUpdated;
        }

        private async void OnRecordButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var metadata = new Dictionary<string, string>
                {
                    { "Session", "Dictation" },
                    { "StartedAt", DateTime.UtcNow.ToString("o") },
                    { "Device", DeviceInfo.Name }
                };

                await _recordingService.StartRecordingAsync("DictateSession", metadata);
                _recordingStartTime = DateTime.UtcNow;
                _isRecording = true;
                _waveformTimer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartRecordingAsync error: {ex}");
                await DisplayAlert("Error", "Failed to start recording.", "OK");
            }
        }

        private async void OnStopButtonClicked(object sender, EventArgs e)
        {
            try
            {
                _waveformTimer.Stop();
                _isRecording = false;

                var clip = await _recordingService.StopRecordingAsync();
                clip.Metadata["StoppedAt"] = DateTime.UtcNow.ToString("o");

                await _recordingService.SaveRecordingAsync(clip);
                await DisplayAlert("Saved", "Recording saved successfully.", "OK");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StopRecordingAsync error: {ex}");
                await DisplayAlert("Error", "Failed to save recording.", "OK");
            }

            _playbackController.Stop();
        }

        private async void OnPlayButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var clip = await _recordingService.GetLastRecordingAsync();
                if (clip != null)
                {
                    _playbackController.Play(clip);
                }
                else
                {
                    await DisplayAlert("No Recording", "No recording found to play.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Play error: {ex}");
                await DisplayAlert("Error", "Failed to play recording.", "OK");
            }
        }

        private void OnPauseClicked(object sender, EventArgs e) => _playbackController.Pause();
        private void OnRewindClicked(object sender, EventArgs e) => _playbackController.Rewind(TimeSpan.FromSeconds(5));
        private void OnFastForwardClicked(object sender, EventArgs e) => _playbackController.FastForward(TimeSpan.FromSeconds(5));

        private void OnPlaybackProgressUpdated(TimeSpan elapsed)
        {
            Dispatcher.Dispatch(() =>
            {
                TimestampLabel.Text = $"{elapsed:mm\\:ss} / ∞";
                WaveformBar.Progress = new Random().NextDouble();
            });
        }

        private void OnWaveformTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_isRecording)
                return;

            Dispatcher.Dispatch(() =>
            {
                var elapsed = DateTime.UtcNow - _recordingStartTime;
                WaveformBar.Progress = new Random().NextDouble();
                TimestampLabel.Text = $"{elapsed:mm\\:ss} / ∞";
            });
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }
    }
}
