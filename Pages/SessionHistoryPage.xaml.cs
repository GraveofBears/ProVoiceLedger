using Microsoft.Maui.Controls;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Core.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProVoiceLedger.Pages
{
    public partial class SessionHistoryPage : ContentPage, INotifyPropertyChanged
    {
        private readonly SessionDatabase _sessionDb;

        private ObservableCollection<Session> _sessions = new();
        public ObservableCollection<Session> Sessions
        {
            get => _sessions;
            set
            {
                if (_sessions != value)
                {
                    _sessions = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public SessionHistoryPage(SessionDatabase sessionDb)
        {
            InitializeComponent();
            _sessionDb = sessionDb;
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadSessions();
        }

        private async void LoadSessions()
        {
            try
            {
                var savedSessions = await _sessionDb.GetSessionsAsync();
                if (savedSessions != null)
                    Sessions = new ObservableCollection<Session>(savedSessions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading sessions: {ex.Message}");
                await DisplayAlert("Error", "Failed to load saved sessions.", "OK");
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
