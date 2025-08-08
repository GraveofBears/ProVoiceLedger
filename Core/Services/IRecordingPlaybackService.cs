using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.Core.Services
{
    public interface IRecordingPlaybackService
    {
        Task PlayRecordingAsync(string filePath);
        Task PlayLastRecordingAsync();
        Task<double> GetLastRecordingDurationAsync();
        RecordedClipInfo? GetLastClipInfo();
    }
}
