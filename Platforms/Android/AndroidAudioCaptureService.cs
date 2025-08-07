#if ANDROID
using Android.Media;
using Microsoft.Maui.Storage;
using ProVoiceLedger.AudioBackup;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProVoiceLedger.Platforms.Android
{
    public class AndroidAudioCaptureService : IAudioCaptureService
    {
        private AudioRecord? _audioRecord;
        private FileStream? _outputStream;
        private CancellationTokenSource? _recordingTokenSource;
        private string? _pcmFilePath;
        private string? _wavFilePath;
        private DateTime _startTime;
        private bool _isRecording;
        private string? _sessionName;
        private Dictionary<string, string>? _metadata;

        public bool IsRecording => _isRecording;
        public event Action<float[]>? OnAudioSampleCaptured;

        private const int SampleRate = 44100;
        private const int BufferSize = 2048;

        public async Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null)
        {
            var status = await Permissions.RequestAsync<Permissions.Microphone>();
            if (status != PermissionStatus.Granted)
                return false;

            _sessionName = sessionName;
            _metadata = metadata;
            _startTime = DateTime.Now;
            _isRecording = true;

            var dir = Path.Combine(FileSystem.AppDataDirectory, "Recordings");
            Directory.CreateDirectory(dir);

            _pcmFilePath = Path.Combine(dir, $"{sessionName}_{_startTime:yyyyMMdd_HHmmss}.pcm");
            _wavFilePath = Path.ChangeExtension(_pcmFilePath, ".wav");

            _outputStream = new FileStream(_pcmFilePath, FileMode.Create, FileAccess.Write);
            _audioRecord = new AudioRecord(
                AudioSource.Mic,
                SampleRate,
                ChannelIn.Mono,
                Encoding.Pcm16bit,
                BufferSize
            );

            _audioRecord.StartRecording();
            _recordingTokenSource = new CancellationTokenSource();

            _ = Task.Run(() => CaptureLoop(_recordingTokenSource.Token));

            return true;
        }

        public async Task<RecordedClipInfo?> StopRecordingAsync()
        {
            if (!_isRecording) return null;

            _isRecording = false;
            _recordingTokenSource?.Cancel();

            _audioRecord?.Stop();
            _audioRecord?.Release();
            _audioRecord = null;

            await _outputStream?.FlushAsync()!;
            _outputStream?.Dispose();
            _outputStream = null;

            // Convert PCM to WAV for playback and duration support
            ConvertPcmToWav(_pcmFilePath!, _wavFilePath!);

            var duration = DateTime.Now - _startTime;
            var timestamp = DateTime.Now;

            return new RecordedClipInfo
            {
                FilePath = _wavFilePath ?? string.Empty,
                Duration = duration,
                SessionName = _sessionName ?? string.Empty,
                Timestamp = timestamp,
                Metadata = _metadata,
                RecordedAt = timestamp,
                DeviceUsed = "Android Mic"
            };
        }

        public async Task PlayAudioAsync(string filePath)
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.SetDataSource(filePath);
            mediaPlayer.Prepare();
            mediaPlayer.Start();

            while (mediaPlayer.IsPlaying)
            {
                await Task.Delay(100);
            }

            mediaPlayer.Release();
        }

        public Task<double> GetDurationAsync(string filePath)
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.SetDataSource(filePath);
            mediaPlayer.Prepare();
            double durationSeconds = mediaPlayer.Duration / 1000.0;
            mediaPlayer.Release();
            return Task.FromResult(durationSeconds);
        }

        private void CaptureLoop(CancellationToken token)
        {
            var buffer = new byte[BufferSize];

            while (!token.IsCancellationRequested)
            {
                int read = _audioRecord?.Read(buffer, 0, buffer.Length) ?? 0;
                if (read <= 0) continue;

                _outputStream?.Write(buffer, 0, read);

                float[] samples = new float[read / 2];
                for (int i = 0; i < samples.Length; i++)
                {
                    short sample = BitConverter.ToInt16(buffer, i * 2);
                    samples[i] = sample / 32768f;
                }

                OnAudioSampleCaptured?.Invoke(samples);
            }
        }

        private static void ConvertPcmToWav(string pcmPath, string wavPath, int sampleRate = SampleRate)
        {
            using var pcmStream = new FileStream(pcmPath, FileMode.Open, FileAccess.Read);
            using var wavStream = new FileStream(wavPath, FileMode.Create, FileAccess.Write);

            int byteRate = sampleRate * 2; // mono, 16-bit
            int dataLength = (int)pcmStream.Length;

            wavStream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            wavStream.Write(BitConverter.GetBytes(36 + dataLength));
            wavStream.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            wavStream.Write(BitConverter.GetBytes(16)); // PCM header size
            wavStream.Write(BitConverter.GetBytes((short)1)); // PCM format
            wavStream.Write(BitConverter.GetBytes((short)1)); // mono
            wavStream.Write(BitConverter.GetBytes(sampleRate));
            wavStream.Write(BitConverter.GetBytes(byteRate));
            wavStream.Write(BitConverter.GetBytes((short)2)); // block align
            wavStream.Write(BitConverter.GetBytes((short)16)); // bits per sample
            wavStream.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            wavStream.Write(BitConverter.GetBytes(dataLength));

            pcmStream.CopyTo(wavStream);
        }
    }
}
#endif
