using ProVoiceLedger.Core.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProVoiceLedger.Core.Services
{
    public class SessionDatabase
    {
        private readonly SQLiteAsyncConnection _database;
        private readonly Task _initTask;

        public SessionDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _initTask = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Session>();
            await _database.CreateTableAsync<RecordedClipInfo>();
        }

        private async Task EnsureInitializedAsync()
        {
            await _initTask;
        }

        public async Task<List<Session>> GetSessionsAsync()
        {
            await EnsureInitializedAsync();
            return await _database
                .Table<Session>()
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Session?> GetSessionAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _database
                .Table<Session>()
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> SaveSessionAsync(Session session)
        {
            await EnsureInitializedAsync();
            return session.Id != 0
                ? await _database.UpdateAsync(session)
                : await _database.InsertAsync(session);
        }

        public async Task<int> DeleteSessionAsync(int id)
        {
            await EnsureInitializedAsync();
            var sessionToDelete = await GetSessionAsync(id);
            if (sessionToDelete != null)
            {
                return await _database.DeleteAsync(sessionToDelete);
            }
            return 0;
        }

        public async Task<List<Session>> GetSessionsInRangeAsync(DateTime from, DateTime to)
        {
            await EnsureInitializedAsync();
            return await _database
                .Table<Session>()
                .Where(s => s.StartTime >= from && s.StartTime <= to)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<int> SaveRecordingAsync(RecordedClipInfo clip)
        {
            await EnsureInitializedAsync();
            return await _database.InsertAsync(clip);
        }

        public async Task<RecordedClipInfo?> GetLastRecordingAsync()
        {
            await EnsureInitializedAsync();
            return await _database
                .Table<RecordedClipInfo>()
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<List<RecordedClipInfo>> GetRecordingsForSessionAsync(string sessionName)
        {
            await EnsureInitializedAsync();
            return await _database
                .Table<RecordedClipInfo>()
                .Where(r => r.SessionName == sessionName)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task<List<RecordedClipInfo>> GetAllRecordingsAsync()
        {
            await EnsureInitializedAsync();
            return await _database
                .Table<RecordedClipInfo>()
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task<int> UpdateRecordingAsync(RecordedClipInfo clip)
        {
            await EnsureInitializedAsync();
            return await _database.UpdateAsync(clip);
        }

        public async Task<int> DeleteRecordingAsync(int id)
        {
            await EnsureInitializedAsync();
            return await _database.DeleteAsync<RecordedClipInfo>(id);
        }
    }
}