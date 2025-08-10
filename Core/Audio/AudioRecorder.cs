using System.Threading.Tasks;
using ProVoiceLedger.Core.Audio;

namespace ProVoiceLedger.Core.Audio
{
    public class AudioRecorder
    {
        public Task StartAsync(string filePath)
        {
            return AudioManager.Instance.StartRecordingAsync(filePath);
        }

        public Task StopAsync()
        {
            return AudioManager.Instance.StopRecordingAsync();
        }

        public void Cancel()
        {
            AudioManager.Instance.Stop();
        }
    }
}
