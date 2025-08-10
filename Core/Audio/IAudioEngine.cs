using System;
using System.Threading.Tasks;

namespace ProVoiceLedger.Core.Audio
{
    public interface IAudioEngine
    {
        // 🎙️ Recording
        Task StartRecordingAsync(string filePath);
        Task StopRecordingAsync();

        // 🔊 Playback
        Task PlayAsync(string filePath);
        void Pause();
        void Stop();
        void SeekRelative(TimeSpan offset);
        void SeekAbsolute(TimeSpan position);

        // 📈 Real-time amplitude for waveform
        event Action<float> AmplitudeReceived;
    }
}
