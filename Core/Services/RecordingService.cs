using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Audio;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public class RecordingService : IRecordingService
    {
        private readonly List<RecordedClipInfo> _recordings = new();
        private RecordedClipInfo? _lastClip;
        private User? _currentUser;

        private readonly IAudioEngine _audioPlayer;

        public RecordingService(IAudioEngine audioPlayer)
        {
            _audioPlayer = audioPlayer ?? throw new ArgumentNullException(nameof(audioPlayer));
        }

        // 🎙️ Recording
        public async Task StartRecordingAsync(string sessionName, Dictionary<string, string> metadata)
        {
            var filePath = GenerateFilePath(sessionName);
            await _audioPlayer.StartRecordingAsync(filePath);

            _lastClip = new RecordedClipInfo
            {
                FilePath = filePath,
                Metadata = metadata,
                StartedAt = DateTime.UtcNow
            };
        }

        public async Task<RecordedClipInfo> StopRecordingAsync()
        {
            if (_lastClip == null)
                throw new InvalidOperationException("No recording in progress.");

            await _audioPlayer.StopRecordingAsync();

            _lastClip.StoppedAt = DateTime.UtcNow;

            // ✅ Fix: unwrap nullable DateTime before subtracting
            if (_lastClip.StartedAt != null && _lastClip.StoppedAt != null)
            {
                _lastClip.Duration = (_lastClip.StoppedAt.Value - _lastClip.StartedAt.Value).TotalSeconds;
            }
            else
            {
                _lastClip.Duration = 0;
            }

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
            await _audioPlayer.PlayAsync(filePath);
        }

        public void PausePlayback()
        {
            _audioPlayer.Pause();
        }

        public void StopPlayback()
        {
            _audioPlayer.Stop();
        }

        public void SeekBackward(TimeSpan amount)
        {
            _audioPlayer.SeekRelative(-amount);
        }

        public void SeekForward(TimeSpan amount)
        {
            _audioPlayer.SeekRelative(amount);
        }

        public void SeekTo(TimeSpan position)
        {
            _audioPlayer.SeekAbsolute(position);
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

        // 🔧 Helpers
        private string GenerateFilePath(string sessionName)
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"{sessionName}_{timestamp}.wav";
            var folder = Path.Combine(FileSystem.AppDataDirectory, "Recordings");

            Directory.CreateDirectory(folder);
            return Path.Combine(folder, fileName);
        }
    }
}
