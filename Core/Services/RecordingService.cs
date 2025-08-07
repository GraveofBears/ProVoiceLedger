using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Persistence;

namespace ProVoiceLedger.Core.Services
{
    public class RecordingService
    {
        private readonly IAudioCaptureService _audioCapture;
        private readonly RecordingDatabase _database;
        private RecordedClipInfo? _lastClip;

        public RecordingService(IAudioCaptureService audioCapture)
        {
            _audioCapture = audioCapture;
            _database = new RecordingDatabase();
        }

        public async Task StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            var started = await _audioCapture.StartRecordingAsync(sessionName, metadata);
            if (!started)
                throw new InvalidOperationException("Failed to start recording.");
        }

        public async Task<RecordedClipInfo> StopRecordingAsync()
        {
            var clip = await _audioCapture.StopRecordingAsync();
            if (clip == null)
                throw new InvalidOperationException("No recording was active.");

            _lastClip = clip;
            await _database.SaveClipAsync(clip);
            return clip;
        }

        public async Task PlayLastRecordingAsync()
        {
            if (_lastClip == null || string.IsNullOrWhiteSpace(_lastClip.FilePath))
                throw new InvalidOperationException("No recording available to play.");

            await _audioCapture.PlayAudioAsync(_lastClip.FilePath);
        }

        public async Task<double> GetLastRecordingDurationAsync()
        {
            if (_lastClip == null || string.IsNullOrWhiteSpace(_lastClip.FilePath))
                return 0;

            return await _audioCapture.GetDurationAsync(_lastClip.FilePath);
        }

        public RecordedClipInfo? GetLastClipInfo()
        {
            return _lastClip;
        }
    }
}
