using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ORApp.Web.Models
{
    public class BettingViewModel : ORAppWebBaseModel, INotifyPropertyChanged
    {
        private string _inputText = string.Empty;
        private string _predictionsResult = string.Empty;
        private string _oddsResult = string.Empty;
        private bool _isProcessing = false;

        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public string PredictionsResult
        {
            get => _predictionsResult;
            set => SetProperty(ref _predictionsResult, value);
        }

        public string OddsResult
        {
            get => _oddsResult;
            set => SetProperty(ref _oddsResult, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => SetProperty(ref _isProcessing, value);
        }

        public async Task ProcessPredictions()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                PredictionsResult = "Please enter match details.";
                return;
            }

            IsProcessing = true;
            await Task.Delay(1000); // Simulate processing
            PredictionsResult = $"Predictions for '{InputText}': Home team wins with 65% confidence.";
            IsProcessing = false;
        }

        public async Task CalculateOdds()
        {
            if (string.IsNullOrWhiteSpace(InputText))
            {
                OddsResult = "Please enter match details.";
                return;
            }

            IsProcessing = true;
            await Task.Delay(1000); // Simulate processing
            OddsResult = $"Odds for '{InputText}': Home 1.85, Draw 3.40, Away 4.20.";
            IsProcessing = false;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}