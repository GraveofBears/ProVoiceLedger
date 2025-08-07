using ProVoiceLedger.Core.Models;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace ProVoiceLedger.Core.Services
{
    public class SessionDatabase
    {
        private readonly SQLiteAsyncConnection _database;

        public SessionDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _ = InitializeAsync(); // fire-and-forget setup
        }

        private async Task InitializeAsync()
        {
            await _database.CreateTableAsync<Session>();
            await _database.CreateTableAsync<RecordedClipInfo>();
        }

        // 🔍 Get all sessions, newest first
        public async Task<List<Session>> GetSessionsAsync()
        {
            return await _database
                .Table<Session>()
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        // 🎯 Get a specific session by ID
        public async Task<Session?> GetSessionAsync(int id)
        {
            return await _database
                .Table<Session>()
                .Where(s => s.Id == id)
                .FirstOrDefaultAsync();
        }

        // 💾 Save a session (insert or update)
        public async Task<int> SaveSessionAsync(Session session)
        {
            if (session.Id != 0)
                return await _database.UpdateAsync(session);
            else
                return await _database.InsertAsync(session);
        }

        // 🗑️ Delete a session by ID
        public async Task<int> DeleteSessionAsync(int id)
        {
            var sessionToDelete = await GetSessionAsync(id);
            if (sessionToDelete != null)
            {
                return await _database.DeleteAsync(sessionToDelete);
            }
            return 0;
        }

        // 📆 Optional: get sessions within a date range
        public async Task<List<Session>> GetSessionsInRangeAsync(DateTime from, DateTime to)
        {
            return await _database
                .Table<Session>()
                .Where(s => s.StartTime >= from && s.StartTime <= to)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        // 🎙️ Save a recorded clip
        public async Task<int> SaveRecordingAsync(RecordedClipInfo clip)
        {
            return await _database.InsertAsync(clip);
        }

        // 🕵️ Get the most recent recording
        public async Task<RecordedClipInfo?> GetLastRecordingAsync()
        {
            return await _database
                .Table<RecordedClipInfo>()
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        // 📁 Get recordings for a specific session
        public async Task<List<RecordedClipInfo>> GetRecordingsForSessionAsync(string sessionName)
        {
            return await _database
                .Table<RecordedClipInfo>()
                .Where(r => r.SessionName == sessionName)
                .OrderByDescending(r => r.Timestamp)
                .ToListAsync();
        }
    }
}
