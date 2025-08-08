using System;
using System.Threading.Tasks;

namespace ProVoiceLedger.Core.Audio
{
    public interface IAudioEngine
    {
        Task StartRecordingAsync(string filePath);
        Task StopRecordingAsync();

        Task PlayAsync(string filePath);
        void Pause();
        void Stop();

        void SeekRelative(TimeSpan offset);
        void SeekAbsolute(TimeSpan position);
    }
}
