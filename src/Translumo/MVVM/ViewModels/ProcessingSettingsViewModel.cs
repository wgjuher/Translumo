using Translumo.Processing.Configuration;
using Translumo.Utils;

namespace Translumo.MVVM.ViewModels
{
    public sealed class ProcessingSettingsViewModel : BindableBase
    {
        private readonly TextProcessingConfiguration _textProcessingConfiguration;

        public ProcessingSettingsViewModel(TextProcessingConfiguration textProcessingConfiguration)
        {
            _textProcessingConfiguration = textProcessingConfiguration;
        }

        public bool KeepFormatting
        {
            get => _textProcessingConfiguration.KeepFormatting;
            set
            {
                _textProcessingConfiguration.KeepFormatting = value;
                OnPropertyChanged();
            }
        }

        public bool AutoClearTexts
        {
            get => _textProcessingConfiguration.AutoClearTexts;
            set
            {
                _textProcessingConfiguration.AutoClearTexts = value;
                OnPropertyChanged();
            }
        }

        public uint AutoClearTextsDelayMs
        {
            get => _textProcessingConfiguration.AutoClearTextsDelayMs;
            set
            {
                _textProcessingConfiguration.AutoClearTextsDelayMs = value;
                OnPropertyChanged();
            }
        }

        // Frame Comparison Settings
        public bool EnableFrameComparison
        {
            get => _textProcessingConfiguration.FrameComparison.EnableFrameComparison;
            set
            {
                _textProcessingConfiguration.FrameComparison.EnableFrameComparison = value;
                OnPropertyChanged();
            }
        }

        public int StableFramesRequired
        {
            get => _textProcessingConfiguration.FrameComparison.StableFramesRequired;
            set
            {
                _textProcessingConfiguration.FrameComparison.StableFramesRequired = value;
                OnPropertyChanged();
            }
        }

        public double ImageSimilarityThreshold
        {
            get => _textProcessingConfiguration.FrameComparison.ImageSimilarityThreshold;
            set
            {
                _textProcessingConfiguration.FrameComparison.ImageSimilarityThreshold = value;
                OnPropertyChanged();
            }
        }

        public bool UseGrayscaleComparison
        {
            get => _textProcessingConfiguration.FrameComparison.UseGrayscaleComparison;
            set
            {
                _textProcessingConfiguration.FrameComparison.UseGrayscaleComparison = value;
                OnPropertyChanged();
            }
        }

        // Helper properties for UI binding
        public double ImageSimilarityThresholdPercent
        {
            get => ImageSimilarityThreshold * 100;
            set
            {
                ImageSimilarityThreshold = value / 100;
                OnPropertyChanged();
            }
        }
    }
}