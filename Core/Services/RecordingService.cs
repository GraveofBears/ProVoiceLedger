using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.AudioBackup;

namespace ProVoiceLedger.Core.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly List<RecordedClipInfo> _recordings = new();
        private RecordedClipInfo? _lastClip;
        private User? _currentUser;

        public IAudioCaptureService AudioCapture { get; }

        public RecordingService(IAudioCaptureService audioCapture)
        {
            AudioCapture = audioCapture ?? throw new ArgumentNullException(nameof(audioCapture));
        }

        // 🎙️ Recording
        public async Task StartRecordingAsync(string sessionName, Dictionary<string, string> metadata)
        {
            bool started = await AudioCapture.StartRecordingAsync(sessionName, metadata);
            if (!started)
                throw new InvalidOperationException("Recording could not be started.");

            _lastClip = new RecordedClipInfo
            {
                SessionName = sessionName,
                Metadata = metadata,
                StartedAt = DateTime.UtcNow
            };
        }

        public async Task<RecordedClipInfo> StopRecordingAsync()
        {
            if (_lastClip == null)
                throw new InvalidOperationException("No recording in progress.");

            var clip = await AudioCapture.StopRecordingAsync();
            if (clip == null)
                throw new InvalidOperationException("Recording failed to stop properly.");

            _lastClip.FilePath = clip.FilePath;
            _lastClip.StoppedAt = clip.Timestamp;
            _lastClip.Duration = clip.Duration;
            _lastClip.DeviceUsedOverride = clip.DeviceUsedOverride;
            _lastClip.RecordedAtOverride = clip.RecordedAtOverride;

            return _lastClip;
        }

        public async Task FinishRecordingAsync()
        {
            var clip = await StopRecordingAsync();
            clip.Metadata["FinishedAt"] = DateTime.UtcNow.ToString("o");
            await SaveRecordingAsync(clip);
        }

        public async Task SaveRecordingAsync(RecordedClipInfo clip)
        {
            _recordings.Add(clip);
            _lastClip = clip;

            await Task.CompletedTask; // Replace with actual persistence logic
        }

        // ▶️ Playback
        public async Task PlayRecordingAsync(string filePath)
        {
            await AudioCapture.PlayAudioAsync(filePath);
        }

        public void PausePlayback()
        {
            // Optional: implement if your audio engine supports it
        }

        public void StopPlayback()
        {
            // Optional: implement if your audio engine supports it
        }

        public void SeekBackward(TimeSpan amount)
        {
            // Optional: implement if your audio engine supports it
        }

        public void SeekForward(TimeSpan amount)
        {
            // Optional: implement if your audio engine supports it
        }

        public void SeekTo(TimeSpan position)
        {
            // Optional: implement if your audio engine supports it
        }

        // 📦 Retrieval
        public Task<RecordedClipInfo?> GetLastRecordingAsync()
        {
            return Task.FromResult(_lastClip);
        }

        public Task<IList<RecordedClipInfo>> GetAllRecordingsAsync()
        {
            return Task.FromResult<IList<RecordedClipInfo>>(_recordings);
        }

        public Task<double> GetLastRecordingDurationAsync()
        {
            return Task.FromResult(_lastClip?.Duration ?? 0);
        }

        public RecordedClipInfo? GetLastClipInfo()
        {
            return _lastClip;
        }

        // 👤 User session
        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        public User? GetCurrentUser()
        {
            return _currentUser;
        }

        public Task<User?> TryRestoreUserAsync()
        {
            return Task.FromResult(_currentUser);
        }
    }
}
