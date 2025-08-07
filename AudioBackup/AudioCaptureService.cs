using NAudio.Wave;
using ProVoiceLedger.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProVoiceLedger.AudioBackup
{
    public class AudioCaptureService : IAudioCaptureService
    {
        private bool _isRecording;
        private DateTime _startTime;
        private string _sessionName = string.Empty;
        private Dictionary<string, string>? _metadata;
        private WaveInEvent? _waveIn;
        private WaveFileWriter? _writer;
        private string _currentFilePath = string.Empty;
        private TaskCompletionSource<bool>? _recordingStoppedTcs;

        public bool IsRecording => _isRecording;
        public event Action<float[]>? OnAudioSampleCaptured;

        public Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            if (_isRecording)
            {
                Console.WriteLine("⚠️ Recording already in progress.");
                return Task.FromResult(false);
            }

            _isRecording = true;
            _startTime = DateTime.Now;
            _sessionName = sessionName;
            _metadata = metadata;

            string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VoiceRecordings");
            Directory.CreateDirectory(directory);

            _currentFilePath = Path.Combine(directory, $"{sessionName}_{_startTime:yyyyMMdd_HHmmss}.wav");

            var format = new WaveFormat(8000, 16, 1); // 8kHz, 16-bit, mono
            _waveIn = new WaveInEvent
            {
                WaveFormat = format,
                BufferMilliseconds = 100
            };

            _writer = new WaveFileWriter(_currentFilePath, format);
            _recordingStoppedTcs = new TaskCompletionSource<bool>();

            _waveIn.DataAvailable += (s, e) =>
            {
                _writer?.Write(e.Buffer, 0, e.BytesRecorded);

                if (OnAudioSampleCaptured is not null)
                {
                    float[] samples = ConvertToSamples(e.Buffer, e.BytesRecorded);
                    OnAudioSampleCaptured.Invoke(samples);
                }
            };

            _waveIn.RecordingStopped += (s, e) =>
            {
                try
                {
                    _writer?.Dispose();
                    _writer = null;
                    _waveIn?.Dispose();
                    _waveIn = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Error during disposal: {ex}");
                }
                finally
                {
                    _recordingStoppedTcs?.TrySetResult(true);
                }
            };

            _waveIn.StartRecording();
            Console.WriteLine($"🎙️ Recording started: {_currentFilePath}");
            return Task.FromResult(true);
        }

        public async Task<RecordedClipInfo?> StopRecordingAsync()
        {
            if (!_isRecording)
            {
                Console.WriteLine("⚠️ Stop called with no active recording.");
                return null;
            }

            _isRecording = false;
            _waveIn?.StopRecording();

            if (_recordingStoppedTcs is not null)
                await _recordingStoppedTcs.Task;

            var duration = DateTime.Now - _startTime;
            var timestamp = DateTime.Now;

            Console.WriteLine($"🛑 Recording stopped: {_currentFilePath} ({duration})");

            return new RecordedClipInfo
            {
                FilePath = _currentFilePath,
                Duration = duration,
                SessionName = _sessionName,
                Timestamp = timestamp,
                Metadata = _metadata,
                RecordedAt = timestamp,
                DeviceUsed = "NAudio WaveInEvent"
            };
        }

        public async Task PlayAudioAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"⚠️ File not found: {filePath}");
                return;
            }

            using var audioFile = new AudioFileReader(filePath);
            using var outputDevice = new WaveOutEvent();
            outputDevice.Init(audioFile);
            outputDevice.Play();

            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }
        }

        public Task<double> GetDurationAsync(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"⚠️ File not found: {filePath}");
                return Task.FromResult(0.0);
            }

            using var reader = new AudioFileReader(filePath);
            return Task.FromResult(reader.TotalTime.TotalSeconds);
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
    }
}
