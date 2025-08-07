using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SQLite;
using ProVoiceLedger.Core.Models;
using Microsoft.Maui.Storage;
using ProVoiceLedger.Core.Services;


namespace ProVoiceLedger.Persistence
{
    public class RecordingDatabase
    {
        private readonly SQLiteAsyncConnection _connection;

        public RecordingDatabase()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "recordings.db");
            _connection = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitializeAsync()
        {
            await _connection.CreateTableAsync<RecordedClipInfo>(CreateFlags.None);
        }

        public async Task SaveClipAsync(RecordedClipInfo clip)
        {
            await InitializeAsync();
            await _connection.InsertAsync(clip);
        }

        public async Task<List<RecordedClipInfo>> GetAllClipsAsync()
        {
            await InitializeAsync();
            return await _connection.Table<RecordedClipInfo>().ToListAsync();
        }

        public async Task<RecordedClipInfo?> GetClipByIdAsync(int id)
        {
            await InitializeAsync();
            return await _connection.FindAsync<RecordedClipInfo>(id);
        }

        public async Task DeleteClipAsync(int id)
        {
            await InitializeAsync();
            await _connection.DeleteAsync<RecordedClipInfo>(id);
        }
    }
}
