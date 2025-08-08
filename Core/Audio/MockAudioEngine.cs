using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ProVoiceLedger.Core.Audio
{
    public class MockAudioEngine : IAudioEngine
    {
        private string? _currentFile;
        private TimeSpan _playbackPosition = TimeSpan.Zero;

        public Task StartRecordingAsync(string filePath)
        {
            _currentFile = filePath;
            Debug.WriteLine($"🎙️ Start recording: {filePath}");
            return Task.CompletedTask;
        }

        public Task StopRecordingAsync()
        {
            Debug.WriteLine($"⏹️ Stop recording: {_currentFile}");
            return Task.CompletedTask;
        }

        public Task PlayAsync(string filePath)
        {
            _currentFile = filePath;
            _playbackPosition = TimeSpan.Zero;
            Debug.WriteLine($"▶️ Playing: {filePath}");
            return Task.CompletedTask;
        }

        public void Pause()
        {
            Debug.WriteLine("⏸️ Paused playback");
        }

        public void Stop()
        {
            _playbackPosition = TimeSpan.Zero;
            Debug.WriteLine("⏹️ Stopped playback");
        }

        public void SeekRelative(TimeSpan offset)
        {
            _playbackPosition += offset;
            Debug.WriteLine($"↔️ Seek relative: {offset}, new position: {_playbackPosition}");
        }

        public void SeekAbsolute(TimeSpan position)
        {
            _playbackPosition = position;
            Debug.WriteLine($"📍 Seek absolute: {position}");
        }
    }
}
