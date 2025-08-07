using System;
using System.IO;
using ProVoiceLedger.Core.Models;


namespace ProVoiceLedger.Core.Services;
public class RecordingUploadService
{
    public bool SaveRecording(User currentUser, byte[] audioBuffer, string recordingName = "")
    {
        if (currentUser == null || string.IsNullOrWhiteSpace(currentUser.Username))
            throw new ArgumentException("Invalid user.");

        if (currentUser.IsSuspended)
        {
            Console.WriteLine("Upload denied: user is suspended.");
            return false;
        }

        try
        {
            string userFolder = Path.Combine("Recordings", currentUser.Username);
            Directory.CreateDirectory(userFolder); // Ensures folder exists

            string safeName = string.IsNullOrWhiteSpace(recordingName) ? "Recording" : recordingName;
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");

            string fileName = $"{safeName}_{timestamp}.wav";
            string fullPath = Path.Combine(userFolder, fileName);

            File.WriteAllBytes(fullPath, audioBuffer);
            Console.WriteLine($"Recording saved: {fullPath}");

            // Optionally write a metadata file alongside
            File.WriteAllText(Path.Combine(userFolder, $"{safeName}_{timestamp}.meta"),
                $"RecordedBy: {currentUser.Username}\nTimestamp: {timestamp}\nLength: {audioBuffer.Length} bytes");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Upload failed: {ex.Message}");
            return false;
        }
    }
}
