using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.AudioBackup;

namespace ProVoiceLedger.AudioBackup
{
    /// <summary>
    /// Captures microphone input and emits waveform data for visualization.
    /// </summary>
    public class AudioCaptureService : IAudioCaptureService
    {
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private TaskCompletionSource<bool>? _recordingStoppedTcs;

        private bool _isRecording;
        private DateTime _startTime;
        private string _sessionName = string.Empty;
        private string _currentFilePath = string.Empty;
        private Dictionary<string, string>? _metadata;

        public bool IsRecording => _isRecording;

        public event Action<float[]>? OnAudioSampleCaptured;
        public event Action<float>? OnAmplitude;

        public Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            if (_isRecording)
            {
                Console.WriteLine("⚠️ Already recording.");
                return Task.FromResult(false);
            }

            _isRecording = true;
            _startTime = DateTime.Now;
            _sessionName = sessionName;
            _metadata = metadata;

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoiceRecordings");
            Directory.CreateDirectory(directory);

            _currentFilePath = Path.Combine(directory, $"{sessionName}_{_startTime:yyyyMMdd_HHmmss}.wav");

            var format = new WaveFormat(8000, 16, 1); // Mono, 8kHz, 16-bit
            _waveIn = new WaveInEvent
            {
                WaveFormat = format,
                BufferMilliseconds = 100
            };

            _writer = new WaveFileWriter(_currentFilePath, format);
            _recordingStoppedTcs = new TaskCompletionSource<bool>();

            _waveIn.DataAvailable += OnDataAvailable;
            _waveIn.RecordingStopped += OnRecordingStopped;

            _waveIn.StartRecording();
            Console.WriteLine($"🎙️ Recording started: {_currentFilePath}");
            return Task.FromResult(true);
        }

        public async Task<RecordedClipInfo?> StopRecordingAsync()
        {
            if (!_isRecording)
            {
                Console.WriteLine("⚠️ No active recording.");
                return null;
            }

            _isRecording = false;
            _waveIn?.StopRecording();

            if (_recordingStoppedTcs is not null)
                await _recordingStoppedTcs.Task;

            var duration = DateTime.Now - _startTime;
            var timestamp = DateTime.Now;

            Console.WriteLine($"🛑 Recording stopped: {_currentFilePath} ({duration})");

            return new RecordedClipInfo(
                filePath: _currentFilePath,
                duration: duration.TotalSeconds,
                sessionName: _sessionName,
                timestamp: timestamp,
                metadata: _metadata,
                recordedAtOverride: timestamp,
                deviceUsedOverride: "NAudio WaveInEvent"
            );
        }

        public async Task PlayAudioAsync(string filePath, CancellationToken cancellationToken = default)
        {
#if WINDOWS
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"⚠️ File not found: {filePath}");
                return;
            }

            using var audioFile = new AudioFileReader(filePath);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();

            while (outputDevice.PlaybackState == PlaybackState.Playing && !cancellationToken.IsCancellationRequested)
                await Task.Delay(100, cancellationToken);
#else
            Console.WriteLine("⚠️ Playback only supported on Windows.");
            await Task.CompletedTask;
#endif
        }

        public Task<double> GetDurationAsync(string filePath)
        {
#if WINDOWS
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"⚠️ File not found: {filePath}");
                return Task.FromResult(0.0);
            }

            using var reader = new AudioFileReader(filePath);
            return Task.FromResult(reader.TotalTime.TotalSeconds);
#else
            Console.WriteLine("⚠️ Duration check only supported on Windows.");
            return Task.FromResult(0.0);
#endif
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded <= 0 || _writer == null)
                return;

            _writer.Write(e.Buffer, 0, e.BytesRecorded);

            float[] samples = ConvertToSamples(e.Buffer, e.BytesRecorded);
            if (samples.Length > 0)
            {
                OnAudioSampleCaptured?.Invoke(samples);

                float amplitude = CalculateRMS(samples);
                OnAmplitude?.Invoke(amplitude);
            }
        }

        private void OnRecordingStopped(object? sender, StoppedEventArgs e)
        {
            try
            {
                _writer?.Dispose();
                _waveIn?.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Disposal error: {ex}");
            }
            finally
            {
                _writer = null;
                _waveIn = null;
                _recordingStoppedTcs?.TrySetResult(true);
            }
        }

        private static float[] ConvertToSamples(byte[] buffer, int bytesRecorded)
        {
            int sampleCount = bytesRecorded / 2;
            float[] samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(buffer, i * 2);
                samples[i] = sample / 32768f;
            }
            return samples;
        }

        private static float CalculateRMS(float[] samples)
        {
            if (samples.Length == 0)
                return 0;

            double sum = 0;
            foreach (var s in samples)
                sum += s * s;

            return (float)Math.Sqrt(sum / samples.Length);
        }
    }
}
