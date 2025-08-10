using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger.AudioBackup
{
    /// <summary>
    /// Defines the contract for capturing audio sessions and emitting waveform data.
    /// </summary>
    public interface IAudioCaptureService
    {
        /// <summary>
        /// Starts a new audio recording session.
        /// </summary>
        /// <param name="sessionName">A user-defined label for the session.</param>
        /// <param name="metadata">Optional metadata to associate with the session.</param>
        /// <returns>True if recording successfully started.</returns>
        Task<bool> StartRecordingAsync(string sessionName, Dictionary<string, string>? metadata = null);

        /// <summary>
        /// Stops the current recording and returns recorded clip info.
        /// </summary>
        /// <returns>A RecordedClipInfo object with session data, or null if no recording was active.</returns>
        Task<RecordedClipInfo?> StopRecordingAsync();

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

        /// <summary>
        /// Indicates whether recording is currently in progress.
        /// </summary>
        bool IsRecording { get; }

        /// <summary>
        /// Emits waveform samples during recording, as a float array.
        /// </summary>
        event Action<float[]>? OnAudioSampleCaptured;

        /// <summary>
        /// Emits real-time amplitude values for visual feedback.
        /// </summary>
        event Action<float>? OnAmplitude;
    }
}
