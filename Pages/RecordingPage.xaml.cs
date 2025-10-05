using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Graphics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages;

public partial class RecordingPage : ContentPage
{
    private readonly IRecordingService _recordingService;
    private readonly IAudioCaptureService _audioCaptureService;
    private readonly SessionDatabase _sessionDatabase;
    private readonly AmplitudeBuffer _amplitudeBuffer;
    private readonly ArcWaveformDrawable _arcDrawable;

    private bool _isRecording;
    private DateTime _recordingStartTime;
    private System.Timers.Timer? _durationTimer;
    private RecordedClipInfo? _currentClip;
    private bool _isPlaying;

    public RecordingPage()
    {
        InitializeComponent();

        // Get services from DI
        _recordingService = App.RecordingService;
        _sessionDatabase = App.SessionDatabase;
        _audioCaptureService = MauiProgram.Services!.GetRequiredService<IAudioCaptureService>();

        // Setup visualizer
        _amplitudeBuffer = new AmplitudeBuffer(64);
        _arcDrawable = new ArcWaveformDrawable(_amplitudeBuffer);
        VisualizerCanvas.Drawable = _arcDrawable;

        // Subscribe to amplitude events
        _audioCaptureService.OnAmplitude += OnAmplitudeReceived;

        // Setup duration timer
        _durationTimer = new System.Timers.Timer(100);
        _durationTimer.Elapsed += (s, e) =>
        {
            if (_isRecording)
            {
                var elapsed = DateTime.Now - _recordingStartTime;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DurationLabel.Text = $"{elapsed:hh\\:mm\\:ss} / 0:00:00";
                });
            }
        };

        // Use Unloaded event instead of OnDisappearing for TabbedPage compatibility
        this.Unloaded += OnPageUnloaded;
    }

    private void OnAmplitudeReceived(float amplitude)
    {
        _amplitudeBuffer.Push(amplitude);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            VisualizerCanvas.Invalidate();
        });
    }

    private async void OnRecordButtonClicked(object? sender, EventArgs e)
    {
        if (!_isRecording)
        {
            await StartRecordingAsync();
        }
        else
        {
            await StopRecordingAsync();
        }
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            string sessionName = string.IsNullOrWhiteSpace(FilenameLabel.Text) || FilenameLabel.Text == "Untitled"
                ? $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}"
                : FilenameLabel.Text;

            var metadata = new Dictionary<string, string>
            {
                { "Session", "Dictation" },
                { "StartedAt", DateTime.UtcNow.ToString("o") },
                { "Device", DeviceInfo.Name }
            };

            await _recordingService.StartRecordingAsync(sessionName, metadata);

            _recordingStartTime = DateTime.Now;
            _isRecording = true;
            _arcDrawable.IsRecording = true;
            _durationTimer?.Start();

            // Animate mic to cyan
            await AnimateMicToCyan();
            _ = StartPulseAnimationAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to start recording: {ex.Message}", "OK");
        }
    }

    private async Task StopRecordingAsync()
    {
        try
        {
            _isRecording = false;
            _arcDrawable.IsRecording = false;
            _durationTimer?.Stop();

            var clip = await _recordingService.StopRecordingAsync();
            clip.Title = FilenameLabel.Text;
            clip.Metadata["StoppedAt"] = DateTime.UtcNow.ToString("o");

            await _sessionDatabase.SaveRecordingAsync(clip);
            _currentClip = clip;

            // Animate mic back to white
            await AnimateMicToWhite();
            await StopPulseAnimationAsync();

            await DisplayAlert("Saved", "Recording saved successfully.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save recording: {ex.Message}", "OK");
        }
    }

    private async Task AnimateMicToCyan()
    {
        MicImageOverlay.Source = "mic_cyan.png";
        MicImageOverlay.Opacity = 0.0;
        await MicImageOverlay.FadeTo(1.0, 600, Easing.CubicInOut);
        MicImageBase.Source = "mic_cyan.png";
        MicImageOverlay.Opacity = 0.0;
    }

    private async Task AnimateMicToWhite()
    {
        MicImageOverlay.Source = "mic_white.png";
        MicImageOverlay.Opacity = 0.0;
        await MicImageOverlay.FadeTo(1.0, 600, Easing.CubicInOut);
        MicImageBase.Source = "mic_white.png";
        MicImageOverlay.Opacity = 0.0;
    }

    private async Task StartPulseAnimationAsync()
    {
        MicPulseImage.Opacity = 0.7;
        while (_isRecording)
        {
            await MicPulseImage.FadeTo(0.0, 1000);
            if (_isRecording)
            {
                await MicPulseImage.FadeTo(0.7, 1000);
            }
        }
    }

    private async Task StopPulseAnimationAsync()
    {
        await MicPulseImage.FadeTo(0.0, 300);
    }

    private async void OnRenameButtonClicked(object sender, EventArgs e)
    {
        var newName = await DisplayPromptAsync("Rename", "Enter new filename:",
            initialValue: FilenameLabel.Text,
            placeholder: "Untitled");

        if (!string.IsNullOrWhiteSpace(newName))
        {
            FilenameLabel.Text = newName;
        }
    }

    private async void OnPlayPauseButtonClicked(object? sender, EventArgs e)
    {
        if (_isRecording)
        {
            await StopRecordingAsync();
            return;
        }

        if (_currentClip == null)
        {
            await DisplayAlert("No Recording", "Record something first", "OK");
            return;
        }

        if (!_isPlaying)
        {
            _isPlaying = true;
            PlayPauseImage.Source = "pause_cu.png";
            await _audioCaptureService.PlayAudioAsync(_currentClip.FilePath);
            _isPlaying = false;
            PlayPauseImage.Source = "play_cu.png";
        }
        else
        {
            _isPlaying = false;
            PlayPauseImage.Source = "play_cu.png";
        }
    }

    private void OnRewindClicked(object sender, EventArgs e)
    {
        // TODO: Implement rewind functionality with service
    }

    private void OnFastForwardClicked(object sender, EventArgs e)
    {
        // TODO: Implement fast forward functionality with service
    }

    private async void OnNewButtonClicked(object sender, EventArgs e)
    {
        if (_isRecording)
        {
            await StopRecordingAsync();
        }

        FilenameLabel.Text = "Untitled";
        DurationLabel.Text = "0:00:00 / 0:00:00";
        _currentClip = null;
        _isPlaying = false;
        PlayPauseImage.Source = "play_cu.png";

        MicImageBase.Source = "mic_white.png";
        MicImageOverlay.Opacity = 0;
        MicPulseImage.Opacity = 0;
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (_currentClip == null)
        {
            await DisplayAlert("Delete", "No recording to delete", "OK");
            return;
        }

        var confirm = await DisplayAlert("Delete Recording",
            "Are you sure you want to delete this recording?",
            "Delete", "Cancel");

        if (confirm)
        {
            try
            {
                if (System.IO.File.Exists(_currentClip.FilePath))
                {
                    System.IO.File.Delete(_currentClip.FilePath);
                }

                await _sessionDatabase.DeleteRecordingAsync(_currentClip.Id);
                OnNewButtonClicked(sender, e);
                await DisplayAlert("Success", "Recording deleted", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }
        }
    }

    private void OnPageUnloaded(object? sender, EventArgs e)
    {
        // Cleanup when page is unloaded (works better with TabbedPage)
        _audioCaptureService.OnAmplitude -= OnAmplitudeReceived;
        _durationTimer?.Stop();
        _durationTimer?.Dispose();

        if (_isRecording)
        {
            // Fire and forget - stop recording if still active
            _ = StopRecordingAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Keep this lighter - TabbedPage keeps pages in memory
    }
}