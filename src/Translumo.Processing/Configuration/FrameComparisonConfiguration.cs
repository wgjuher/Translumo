using Translumo.Utils;

namespace Translumo.Processing.Configuration
{
    public class FrameComparisonConfiguration : BindableBase
    {
        public static FrameComparisonConfiguration Default => new FrameComparisonConfiguration()
        {
            EnableFrameComparison = false,
            StableFramesRequired = 3,
            ImageSimilarityThreshold = 0.95,
            UseGrayscaleComparison = true
        };

        public bool EnableFrameComparison
        {
            get => _enableFrameComparison;
            set
            {
                SetProperty(ref _enableFrameComparison, value);
            }
        }

        public int StableFramesRequired
        {
            get => _stableFramesRequired;
            set
            {
                SetProperty(ref _stableFramesRequired, value);
            }
        }

        public double ImageSimilarityThreshold
        {
            get => _imageSimilarityThreshold;
            set
            {
                SetProperty(ref _imageSimilarityThreshold, value);
            }
        }

        public bool UseGrayscaleComparison
        {
            get => _useGrayscaleComparison;
            set
            {
                SetProperty(ref _useGrayscaleComparison, value);
            }
        }

        private bool _enableFrameComparison;
        private int _stableFramesRequired;
        private double _imageSimilarityThreshold;
        private bool _useGrayscaleComparison;
    }
}