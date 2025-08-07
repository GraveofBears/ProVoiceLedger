using System.Collections.ObjectModel;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;

namespace ProVoiceLedger.ViewModels;

public class SessionHistoryViewModel
{
    private readonly SessionDatabase _sessionDb;

    public ObservableCollection<Session> Sessions { get; set; } = new();

    public SessionHistoryViewModel(SessionDatabase sessionDb)
    {
        _sessionDb = sessionDb;
        _ = LoadSessionsAsync();
    }

    private async Task LoadSessionsAsync()
    {
        var saved = await _sessionDb.GetSessionsAsync();
        if (saved != null)
        {
            foreach (var s in saved)
                Sessions.Add(s);
        }
    }
}
