using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProVoiceLedger.Core.Models;
using ProVoiceLedger.Graphics;
using System;
using System.Collections.Generic;

namespace ProVoiceLedger.Pages
{
    public partial class AudioEditorPage : ContentPage
    {
        private readonly RecordedClipInfo _clip;
        private readonly WaveformEditorDrawable _waveformDrawable = new();
        private bool _hasUnsavedChanges = false;

        public AudioEditorPage(RecordedClipInfo clip)
        {
            InitializeComponent();
            _clip = clip ?? throw new ArgumentNullException(nameof(clip));
            TitleEntry.Text = clip.Title;

            WaveformView.Drawable = _waveformDrawable;

            // 🔧 Adapter: Convert MAUI TouchEventArgs to CustomTouchEventArgs
            WaveformView.StartInteraction += (s, e) =>
            {
                var points = new List<PointF>();
                foreach (var touch in e.Touches)
                    points.Add(touch);

                var customArgs = new CustomTouchEventArgs(points);
                OnWaveformTapped(s, customArgs);
            };

            LoadWaveformFromFile(clip.FilePath);
        }

        private void LoadWaveformFromFile(string filePath)
        {
            // TODO: Replace with real decoding
            var rand = new Random();
            float[] samples = new float[512];
            for (int i = 0; i < samples.Length; i++)
                samples[i] = (float)rand.NextDouble();

            _waveformDrawable.Amplitudes = samples;
            WaveformView.Invalidate();
        }

        private void OnWaveformTapped(object sender, CustomTouchEventArgs e)
        {
            if (e.Touches.Count == 0 || WaveformView.Width <= 0)
                return;

            var point = e.Touches[0];
            _waveformDrawable.OnTap(point, (float)WaveformView.Width);
            _hasUnsavedChanges = true;
            WaveformView.Invalidate();
        }

        // 🎛 Playback and Editing Controls

        private void OnPlayClicked(object sender, EventArgs e) { /* TODO */ }
        private void OnPauseClicked(object sender, EventArgs e) { /* TODO */ }
        private void OnStopClicked(object sender, EventArgs e) { /* TODO */ }

        private void OnSeekStartClicked(object sender, EventArgs e)
        {
            _waveformDrawable.SetProgress(0f);
            WaveformView.Invalidate();
        }

        private void OnSeekEndClicked(object sender, EventArgs e)
        {
            _waveformDrawable.SetProgress(1f);
            WaveformView.Invalidate();
        }

        private void OnRewindClicked(object sender, EventArgs e)
        {
            _waveformDrawable.ShiftProgress(-0.05f);
            WaveformView.Invalidate();
        }

        private void OnFastForwardClicked(object sender, EventArgs e)
        {
            _waveformDrawable.ShiftProgress(0.05f);
            WaveformView.Invalidate();
        }

        private void OnCutClicked(object sender, EventArgs e)
        {
            _waveformDrawable.CutSelection();
            _hasUnsavedChanges = true;
            WaveformView.Invalidate();
        }

        private void OnClearSelectionClicked(object sender, EventArgs e)
        {
            _waveformDrawable.ClearSelection();
            _hasUnsavedChanges = true;
            WaveformView.Invalidate();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _clip.Title = TitleEntry.Text;
            _clip.SaveEditedWaveform(_waveformDrawable.Amplitudes);
            _hasUnsavedChanges = false;

            await DisplayAlert("Saved", "Changes have been saved.", "OK");
            await Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                bool discard = await DisplayAlert(
                    "Discard Changes?",
                    "You have unsaved edits. Exit without saving?",
                    "Discard", "Keep Editing");

                if (!discard)
                    return;
            }

            await Navigation.PopAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            OnCancelClicked(this, EventArgs.Empty);
            return true;
        }
    }
}
