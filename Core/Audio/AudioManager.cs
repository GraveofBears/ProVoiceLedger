using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Audio
{
    /// <summary>
    /// Centralized audio controller for recording, playback, and UI synchronization.
    /// </summary>
    public class AudioManager
    {
        public static AudioManager Instance { get; } = new();

        private readonly AudioCaptureService _captureService = new();
        private RecordedClipInfo? _lastClip;
        private CancellationTokenSource? _playbackCts;

        public bool IsRecording => _captureService.IsRecording;
        public bool IsPlaying { get; private set; }

        public float Amplitude { get; private set; }
        public double CurrentTime { get; private set; }
        public double TotalDuration { get; private set; }
        public float Progress => TotalDuration > 0 ? (float)(CurrentTime / TotalDuration) : 0;

        public event Action<float[]>? OnAudioSampleCaptured;

        private AudioManager()
        {
            _captureService.OnAmplitude += amp => Amplitude = amp;
            _captureService.OnAudioSampleCaptured += samples => OnAudioSampleCaptured?.Invoke(samples);
        }

        public async Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            bool started = await _captureService.StartRecordingAsync(sessionName, metadata);
            if (started)
            {
                Amplitude = 0;
                CurrentTime = 0;
                TotalDuration = 0;
                _lastClip = null;
                Console.WriteLine($"🎙️ Recording started: {sessionName}");
            }
            return started;
        }

        public async Task StopRecordingAsync()
        {
            _lastClip = await _captureService.StopRecordingAsync();
            if (_lastClip != null)
            {
                TotalDuration = _lastClip.Duration;
                Console.WriteLine($"🛑 Recording stopped. Duration: {TotalDuration:F2}s");
            }
        }

        public async void Play()
        {
            if (_lastClip == null || string.IsNullOrEmpty(_lastClip.FilePath))
            {
                Console.WriteLine("⚠️ No clip to play.");
                return;
            }

            _playbackCts?.Cancel();
            _playbackCts = new CancellationTokenSource();
            IsPlaying = true;
            CurrentTime = 0;

            _ = Task.Run(async () =>
            {
                await _captureService.PlayAudioAsync(_lastClip.FilePath, _playbackCts.Token);
                IsPlaying = false;
                CurrentTime = 0;
                Console.WriteLine("▶️ Playback finished.");
            });

            _ = Task.Run(async () =>
            {
                while (IsPlaying && !_playbackCts.Token.IsCancellationRequested)
                {
                    CurrentTime += 0.1;
                    await Task.Delay(100);
                }
            });
        }

        public void Pause()
        {
            _playbackCts?.Cancel();
            IsPlaying = false;
            Console.WriteLine("⏸️ Playback paused.");
        }

        public void Stop()
        {
            _playbackCts?.Cancel();
            IsPlaying = false;
            CurrentTime = 0;
            Console.WriteLine("⏹️ Playback stopped.");
        }

        public void SeekToStart() => CurrentTime = 0;
        public void SeekToEnd() => CurrentTime = TotalDuration;
        public void Rewind() => CurrentTime = Math.Max(0, CurrentTime - 5);
        public void FastForward() => CurrentTime = Math.Min(TotalDuration, CurrentTime + 5);

        public void SetCurrentTime(double time)
        {
            CurrentTime = time;
        }

        public void Finish()
        {
            Console.WriteLine("✅ Finished session.");
            // Optional: Save/export logic or navigation
        }

        public string? GetCurrentFilePath() => _lastClip?.FilePath;
        public string? GetSessionName() => _lastClip?.SessionName;

        public string GetSaveDirectory()
        {
#if ANDROID
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#elif IOS
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Recordings");
#endif
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return path;
        }
    }
}
