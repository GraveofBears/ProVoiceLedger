using System;
using System.IO;
using System.Text.Json;
using System.IO.Pipes;
using System.Threading.Tasks;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
public class FileStorageService
{
    public bool SaveFile(string filename, byte[] bytes)
    {
        var path = Path.Combine(FileSystem.AppDataDirectory, filename);
        File.WriteAllBytes(path, bytes);
        return true;
    }
}
