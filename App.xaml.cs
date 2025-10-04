using Microsoft.Maui.Graphics;
using System;
using ProVoiceLedger.Core.Services;
using ProVoiceLedger.Core.Models;

namespace ProVoiceLedger
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        public static IRecordingService RecordingService =>
            Services.GetRequiredService<IRecordingService>();

        public static SessionDatabase SessionDatabase =>
            Services.GetRequiredService<SessionDatabase>();

        public App()
        {
            InitializeComponent();
            MainPage = new ContentPage
            {
                Content = new Label
                {
                    Text = "ProVoiceLedger Started!",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 24,
                    TextColor = Colors.White
                },
                BackgroundColor = Colors.Black
            };
        }

        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
            Services = serviceProvider;
        }
    }
}