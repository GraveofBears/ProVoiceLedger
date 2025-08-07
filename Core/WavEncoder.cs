using System;
using System.IO;

public static class WavEncoder
{
    /// <summary>
    /// Converts raw PCM data to a properly formatted WAV file.
    /// This version is optimized for voice recordings at 8kHz, 16-bit, mono.
    /// </summary>
    /// <param name="pcmPath">Path to raw PCM input</param>
    /// <param name="wavPath">Path for output WAV</param>
    /// <param name="sampleRate">Sample rate in Hz (default: 8000)</param>
    /// <param name="bitsPerSample">Bit depth (default: 16)</param>
    /// <param name="channels">Number of audio channels (default: 1 for mono)</param>
    public static void ConvertPcmToWav(string pcmPath, string wavPath, int sampleRate = 8000, int bitsPerSample = 16, int channels = 1)
    {
        using var pcmStream = File.OpenRead(pcmPath);
        using var wavStream = File.Create(wavPath);

        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        int dataLength = (int)pcmStream.Length;
        int totalLength = 36 + dataLength;

        using var writer = new BinaryWriter(wavStream);

        // RIFF Header
        writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(totalLength); // File size - 8 bytes
        writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));

        // fmt Chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16); // PCM chunk size
        writer.Write((short)1); // PCM format
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)(channels * bitsPerSample / 8)); // Block align
        writer.Write((short)bitsPerSample);

        // data Chunk
        writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);

        // Raw PCM data copy
        pcmStream.CopyTo(wavStream);
    }
}
