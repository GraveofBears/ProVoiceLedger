using System.Threading;
using System.Threading.Tasks;

namespace ProVoiceLedger.AudioBackup
{
    /// <summary>
    /// Defines the contract for audio playback and duration inspection.
    /// </summary>
    public interface IAudioPlaybackService
    {
        /// <summary>
        /// Plays back a recorded audio file.
        /// </summary>
        /// <param name="filePath">The full path to the audio file.</param>
        /// <param name="cancellationToken">Optional cancellation token to stop playback.</param>
        Task PlayAudioAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the duration of a recorded audio file in seconds.
        /// </summary>
        /// <param name="filePath">The full path to the audio file.</param>
        /// <returns>The duration in seconds.</returns>
        Task<double> GetDurationAsync(string filePath);
    }
}
