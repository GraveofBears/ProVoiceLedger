// File: AudioBackup/AudioPlaybackService.cs
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProVoiceLedger.AudioBackup
{
    public class AudioPlaybackService : IAudioPlaybackService
    {
        public async Task PlayAudioAsync(string filePath, CancellationToken cancellationToken = default)
        {
            // Stubbed playback logic
            Console.WriteLine($"[Stub] Playing audio: {filePath}");
            await Task.CompletedTask;
        }

        public async Task<double> GetDurationAsync(string filePath)
        {
            // Stubbed duration logic
            Console.WriteLine($"[Stub] Getting duration for: {filePath}");
            return await Task.FromResult(0.0);
        }
    }
}
