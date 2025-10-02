using Microsoft.Maui.Controls;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages;

public partial class RecordingPage : ContentPage
{
    // Audio recording fields
    private WaveInEvent? _waveIn;
    private WaveFileWriter? _writer;
    private string? _currentRecordingPath;
    private bool _isRecording;
    private bool _isPaused;
    private DateTime _recordingStartTime;
    private readonly System.Timers.Timer _durationTimer;

    public RecordingPage()
    {
        InitializeComponent();

        _durationTimer = new System.Timers.Timer(100);
        _durationTimer.Elapsed += (s, e) =>
        {
            if (_isRecording && !_isPaused)
            {
                var elapsed = DateTime.Now - _recordingStartTime;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DurationLabel.Text = $"{elapsed:hh\\:mm\\:ss} / 0:00:00";
                });
            }
        };

        InitializeAudio();
    }

    private void InitializeAudio()
    {
        try
        {
            _waveIn = new WaveInEvent
            {
                WaveFormat = new WaveFormat(44100, 1)
            };

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;
        }
        catch (Exception ex)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Audio Error", $"Failed to initialize audio: {ex.Message}", "OK");
            });
        }
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        if (_writer != null && _isRecording && !_isPaused)
        {
            _writer.Write(e.Buffer, 0, e.BytesRecorded);
            _writer.Flush();
        }
    }

    private void OnRecordingStopped(object? sender, StoppedEventArgs e)
    {
        _writer?.Dispose();
        _writer = null;

        if (e.Exception != null)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Recording Error", e.Exception.Message, "OK");
            });
        }
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

    private void OnPlayPauseButtonClicked(object? sender, EventArgs e)
    {
        var currentSource = PlayPauseImage.Source.ToString();

        if (currentSource.Contains("play"))
        {
            PlayPauseImage.Source = "pause_cu.png";
        }
        else
        {
            PlayPauseImage.Source = "play_cu.png";
        }
    }

    private void OnRewindClicked(object sender, EventArgs e)
    {
        // TODO: Implement rewind functionality
    }

    private void OnFastForwardClicked(object sender, EventArgs e)
    {
        // TODO: Implement fast forward functionality
    }

    private async void OnNewButtonClicked(object sender, EventArgs e)
    {
        if (_isRecording)
        {
            await StopRecordingAsync();
        }

        FilenameLabel.Text = "Untitled";
        DurationLabel.Text = "0:00:00 / 0:00:00";
        _currentRecordingPath = null;

        MicImageOverlay.Opacity = 0;
        MicPulseImage.Opacity = 0;
    }

    private async void OnDeleteButtonClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentRecordingPath))
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
                if (File.Exists(_currentRecordingPath))
                {
                    File.Delete(_currentRecordingPath);
                }

                OnNewButtonClicked(sender, e);
                await DisplayAlert("Success", "Recording deleted", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete: {ex.Message}", "OK");
            }
        }
    }

    private async Task StartRecordingAsync()
    {
        try
        {
            var recordingsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ProVoiceLedger",
                "Recordings"
            );
            Directory.CreateDirectory(recordingsPath);

            var filename = string.IsNullOrWhiteSpace(FilenameLabel.Text) || FilenameLabel.Text == "Untitled"
                ? $"Recording_{DateTime.Now:yyyyMMdd_HHmmss}.wav"
                : $"{FilenameLabel.Text}_{DateTime.Now:yyyyMMdd_HHmmss}.wav";

            _currentRecordingPath = Path.Combine(recordingsPath, filename);
            _writer = new WaveFileWriter(_currentRecordingPath, _waveIn!.WaveFormat);

            _recordingStartTime = DateTime.Now;
            _waveIn!.StartRecording();
            _isRecording = true;
            _durationTimer.Start();

            await MicImageOverlay.FadeTo(1.0, 300);
            MicPulseImage.Opacity = 0.7;

            _ = PulseAnimationAsync();
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
            _waveIn?.StopRecording();
            _isRecording = false;
            _durationTimer.Stop();

            await MicImageOverlay.FadeTo(0.0, 300);
            await MicPulseImage.FadeTo(0.0, 300);

            if (!string.IsNullOrEmpty(_currentRecordingPath))
            {
                await DisplayAlert("Success", "Recording saved!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to stop recording: {ex.Message}", "OK");
        }
    }

    private async Task PulseAnimationAsync()
    {
        while (_isRecording)
        {
            await MicPulseImage.FadeTo(0.0, 1000);
            if (_isRecording)
            {
                await MicPulseImage.FadeTo(0.7, 1000);
            }
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (_isRecording)
        {
            _waveIn?.StopRecording();
        }

        _durationTimer?.Stop();
        _durationTimer?.Dispose();
        _waveIn?.Dispose();
        _writer?.Dispose();
    }
}