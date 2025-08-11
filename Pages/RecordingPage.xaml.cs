using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Graphics;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProVoiceLedger.Pages
{
    public partial class RecordingPage : ContentPage
    {
        private readonly AudioManager _audio = AudioManager.Instance;
        private readonly RecordingStateManager _state = RecordingStateManager.StateManager;
        private readonly ArcWaveformDrawable _arcDrawable;
        private readonly AmplitudeBuffer _buffer = new AmplitudeBuffer();

        private string? _currentFilePath;
        private DateTime _recordingStartTime;
        private CancellationTokenSource? _recordingCts;

        public RecordingPage()
        {
            InitializeComponent();

            _arcDrawable = new ArcWaveformDrawable(_buffer);
            VisualizerCanvas.Drawable = _arcDrawable;

            _state.OnStateChanged += OnStateChanged;
            _state.OnTimeUpdated += OnTimeUpdated;
            _state.OnAmplitudeUpdated += amp =>
            {
                _buffer.UpdateFromMic(amp);
                _arcDrawable.IsRecording = (_state.CurrentState == RecordingState.Recording);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    VisualizerCanvas.Invalidate();
                });
            };

            ResetUI();
        }


        private void OnStateChanged(RecordingState state)
        {
            switch (state)
            {
                case RecordingState.Recording:
                    CrossfadeMicImage(0.0, 1.0);
                    _arcDrawable.IsRecording = true;
                    break;
                case RecordingState.Playing:
                    CrossfadeMicImage(1.0, 0.0);
                    _arcDrawable.IsRecording = false;
                    break;
                case RecordingState.Paused:
                case RecordingState.Idle:
                    CrossfadeMicImage(1.0, 0.0);
                    _arcDrawable.IsRecording = false;
                    break;
            }

            VisualizerCanvas.Invalidate();
        }

        private void OnTimeUpdated(TimeSpan time)
        {
            DurationLabel.Text = $"{FormatTime(TimeSpan.FromSeconds(_audio.CurrentTime))} / {FormatTime(TimeSpan.FromSeconds(_audio.TotalDuration))}";
        }

        private async void CrossfadeMicImage(double baseOpacity, double pulseOpacity)
        {
            await MicImageBase.FadeTo(baseOpacity, 250, Easing.CubicInOut);
            await MicImageOverlay.FadeTo(1.0 - baseOpacity, 250, Easing.CubicInOut);
            await MicPulseImage.FadeTo(pulseOpacity, 250, Easing.CubicInOut);
        }

        private void ResetUI()
        {
            DurationLabel.Text = "00:00 / 00:00";
            CrossfadeMicImage(1.0, 0.0);
            _arcDrawable.IsRecording = false;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                VisualizerCanvas.Invalidate();
            });

            _state.Reset();
        }


        private string GenerateDefaultFilename()
        {
            string author = Environment.UserName ?? "User";
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return $"{author}_{timestamp}.wav";
        }

        private string FormatTime(TimeSpan time)
        {
            return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private void OnRewindClicked(object sender, EventArgs e)
        {
            _audio.Rewind();
            OnTimeUpdated(TimeSpan.FromSeconds(_audio.CurrentTime));
        }

        private void OnFastForwardClicked(object sender, EventArgs e)
        {
            _audio.FastForward();
            OnTimeUpdated(TimeSpan.FromSeconds(_audio.CurrentTime));
        }

        private void OnPlayPauseButtonClicked(object sender, EventArgs e)
        {
            if (_state.CurrentState == RecordingState.Playing)
            {
                _audio.Pause();
                _state.SetState(RecordingState.Paused);
                PlayPauseImage.Source = "play_cu.png";
            }
            else
            {
                if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
                    return;

                if (_audio.CurrentTime >= _audio.TotalDuration)
                    _audio.Rewind();

                _audio.Play();
                _state.SetState(RecordingState.Playing);
                PlayPauseImage.Source = "pause_cu.png";
                OnTimeUpdated(TimeSpan.FromSeconds(_audio.CurrentTime));
            }
        }
        private async void OnRecordButtonClicked(object sender, EventArgs e)
        {
            if (_state.CurrentState == RecordingState.Recording)
            {
                await StopRecordingAsync();
                PlayPauseImage.Source = "play_cu.png";
            }
            else
            {
                await StartRecordingAsync();
                PlayPauseImage.Source = "pause_cu.png";
            }
        }
        private async void OnNewButtonClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Start New?", "Save current dictation before starting a new one?", "Save & New", "Discard");
            if (confirm)
            {
                await StopRecordingAsync(); // Optional: export/save logic
            }

            ResetUI();
            _currentFilePath = Path.Combine(_audio.GetSaveDirectory(), GenerateDefaultFilename());
            FilenameLabel.Text = Path.GetFileName(_currentFilePath);
        }

        private async void OnDeleteButtonClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Delete Recording?", "This will permanently delete the current file.", "Delete", "Cancel");
            if (!confirm) return;

            _audio.Stop();
            _state.SetState(RecordingState.Idle);

            if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
            {
                File.Delete(_currentFilePath);
            }

            ResetUI();
            _currentFilePath = null;
            FilenameLabel.Text = "Untitled.wav";
        }
        private async void OnRenameButtonClicked(object sender, EventArgs e)
        {
            string currentName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "Untitled");
            string? newName = await DisplayPromptAsync("Rename Clip", "Enter new filename:", initialValue: currentName, maxLength: 64);

            if (!string.IsNullOrWhiteSpace(newName))
            {
                string newFilename = newName.Trim() + ".wav";
                string directory = _audio.GetSaveDirectory();

                if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
                {
                    string newPath = Path.Combine(directory, newFilename);
                    File.Move(_currentFilePath, newPath);
                    _currentFilePath = newPath;
                }
                else
                {
                    _currentFilePath = Path.Combine(directory, newFilename);
                }

                FilenameLabel.Text = newFilename;
            }
        }
        private async Task StartRecordingAsync()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                string filename = GenerateDefaultFilename();
                _currentFilePath = Path.Combine(_audio.GetSaveDirectory(), filename);
                FilenameLabel.Text = filename;
            }

            string sessionName = Path.GetFileNameWithoutExtension(_currentFilePath);
            await _audio.StartRecordingAsync(sessionName);
            _state.SetState(RecordingState.Recording);
            _recordingStartTime = DateTime.Now;

            _arcDrawable.IsRecording = true;
            _buffer.UpdateFromMic(_audio.Amplitude); 
            VisualizerCanvas.Invalidate();

            _recordingCts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                while (!_recordingCts.Token.IsCancellationRequested)
                {
                    double elapsed = (DateTime.Now - _recordingStartTime).TotalSeconds;
                    _audio.SetCurrentTime(elapsed);

                    float amp = _audio.Amplitude;
                    _buffer.UpdateFromMic(amp);

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        string formatted = FormatTime(TimeSpan.FromSeconds(elapsed));
                        DurationLabel.Text = $"{formatted} / {formatted}";
                        VisualizerCanvas.Invalidate();
                    });

                    await Task.Delay(100);
                }
            });
        }

        private async Task StopRecordingAsync()
        {
            _recordingCts?.Cancel();
            await _audio.StopRecordingAsync();
            _state.SetState(RecordingState.Idle);
            _audio.SetCurrentTime(0);
            OnTimeUpdated(TimeSpan.Zero);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _recordingCts?.Cancel();
            _state.Reset();
        }
    }
}
