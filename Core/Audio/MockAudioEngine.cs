using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ProVoiceLedger.Core.Audio
{
    public class MockAudioEngine : IAudioEngine
    {
        private string? _currentFile;
        private TimeSpan _playbackPosition = TimeSpan.Zero;

        private System.Timers.Timer? _amplitudeTimer;
        private double _phase = 0;

        public event Action<float>? AmplitudeReceived;

        public Task StartRecordingAsync(string filePath)
        {
            _currentFile = filePath;
            Debug.WriteLine($"🎙️ Start recording: {filePath}");

            StartAmplitudeSimulation();

            return Task.CompletedTask;
        }

        public Task StopRecordingAsync()
        {
            Debug.WriteLine($"⏹️ Stop recording: {_currentFile}");

            StopAmplitudeSimulation();

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

        private void StartAmplitudeSimulation()
        {
            _amplitudeTimer = new System.Timers.Timer(33); // ~30 FPS
            _amplitudeTimer.Elapsed += (s, e) =>
            {
                // Simulate a smooth sine wave amplitude
                _phase += 0.1;
                float amplitude = (float)(0.5 + 0.5 * Math.Sin(_phase));

                AmplitudeReceived?.Invoke(amplitude);
            };
            _amplitudeTimer.Start();
        }

        private void StopAmplitudeSimulation()
        {
            _amplitudeTimer?.Stop();
            _amplitudeTimer?.Dispose();
            _amplitudeTimer = null;
            _phase = 0;
        }
    }
}
