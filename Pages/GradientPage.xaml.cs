using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class GradientPage : ContentPage
    {
        private readonly IRecordingService _recordingService;
        private readonly PlaybackController _playbackController;
        private readonly ArcWaveformDrawable _arcDrawable;
        private readonly WaveformLineDrawable _waveformDrawable;
        private readonly AmplitudeBuffer _amplitudeBuffer;

        private bool _isRecording = false;
        private bool _isPlaying = false;
        private DateTime _recordingStartTime;

        public GradientPage() : this(App.RecordingService) { }

        public GradientPage(IRecordingService recordingService)
        {
            InitializeComponent();
            _recordingService = recordingService;

            _amplitudeBuffer = new AmplitudeBuffer(64);
            _arcDrawable = new ArcWaveformDrawable(_amplitudeBuffer);
            VisualizerCanvas.Drawable = _arcDrawable;

            _waveformDrawable = new WaveformLineDrawable();
            WaveformLineCanvas.Drawable = _waveformDrawable;

            _playbackController = new PlaybackController(_recordingService);
            _playbackController.PlaybackProgressUpdated += OnPlaybackProgressUpdated;

            UpdateMicImage();
        }

        private async void OnRecordButtonClicked(object sender, EventArgs e)
        {
            if (!_isRecording)
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
                    _arcDrawable.IsRecording = true;

                    SubscribeToAmplitude();
                    UpdateMicImage();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"StartRecordingAsync error: {ex}");
                    await DisplayAlert("Error", "Failed to start recording.", "OK");
                }
            }
            else
            {
                await StopRecordingAsync();
            }
        }

        private async Task StopRecordingAsync()
        {
            try
            {
                _isRecording = false;
                _arcDrawable.IsRecording = false;
                UnsubscribeFromAmplitude();
                UpdateMicImage();

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
        }

        private void SubscribeToAmplitude()
        {
            if (_recordingService is RecordingService rs &&
                rs.GetType().GetProperty("AudioCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(rs) is IAudioCaptureService capture)
            {
                capture.OnAmplitude += OnAmplitudeReceived;
            }
        }

        private void UnsubscribeFromAmplitude()
        {
            if (_recordingService is RecordingService rs &&
                rs.GetType().GetProperty("AudioCapture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(rs) is IAudioCaptureService capture)
            {
                capture.OnAmplitude -= OnAmplitudeReceived;
            }
        }

        private void OnAmplitudeReceived(float amplitude)
        {
            _amplitudeBuffer.Push(amplitude);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                VisualizerCanvas.Invalidate();
            });
        }

        private async void UpdateMicImage()
        {
            string newImage = _isRecording ? "mic_cyan.png" : "mic_purple.png";

            MicImageOverlay.Source = newImage;
            MicImageOverlay.Opacity = 0.0;
            MicImageOverlay.IsVisible = true;

            await MicImageOverlay.FadeTo(1.0, 600, Easing.CubicInOut);
            await Task.Delay(100);

            MicImageBase.Source = newImage;
            MicImageBase.Opacity = 1.0;
            MicImageOverlay.Opacity = 0.0;
            MicImageOverlay.IsVisible = false;
        }

        private async void OnPlayPauseButtonClicked(object sender, EventArgs e)
        {
            if (_isRecording)
                await StopRecordingAsync();

            try
            {
                if (_isPlaying)
                {
                    _playbackController.Pause();
                    PlayPauseImage.Source = "play_cr.png";
                    _isPlaying = false;
                }
                else
                {
                    var clip = await _recordingService.GetLastRecordingAsync();
                    if (clip != null)
                    {
                        _playbackController.Play(clip);
                        PlayPauseImage.Source = "pause_cr.png";
                        _isPlaying = true;
                    }
                    else
                    {
                        await DisplayAlert("No Recording", "No recording found to play.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Play/Pause error: {ex}");
                await DisplayAlert("Error", "Playback failed.", "OK");
            }
        }

        private async void OnRewindClicked(object sender, EventArgs e)
        {
            if (_isRecording)
                await StopRecordingAsync();

            _playbackController.Rewind(TimeSpan.FromSeconds(5));
        }

        private async void OnFastForwardClicked(object sender, EventArgs e)
        {
            if (_isRecording)
                await StopRecordingAsync();

            _playbackController.FastForward(TimeSpan.FromSeconds(5));
        }

        private void OnPlaybackProgressUpdated(TimeSpan elapsed)
        {
            Dispatcher.Dispatch(() =>
            {
                var clip = _playbackController.CurrentClip;
                if (clip != null && clip.Duration > 0)
                {
                    var totalDuration = TimeSpan.FromSeconds(clip.Duration);
                    float progress = (float)(elapsed.TotalSeconds / totalDuration.TotalSeconds);
                    _waveformDrawable.Progress = Math.Clamp(progress, 0f, 1f);
                    WaveformLineCanvas.Invalidate();

                    TimestampLeft.Text = $"{elapsed:mm\\:ss}";
                    TimestampRight.Text = $"{totalDuration:mm\\:ss}";
                }
            });
        }
    }
}
