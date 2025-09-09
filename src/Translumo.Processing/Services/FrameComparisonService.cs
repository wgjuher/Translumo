using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using Translumo.Processing.Configuration;
using Translumo.Processing.Interfaces;

namespace Translumo.Processing.Services
{
    public class FrameComparisonService : IFrameComparisonService, IDisposable
    {
        private readonly FrameComparisonConfiguration _configuration;
        private readonly ILogger<FrameComparisonService> _logger;
        
        private Mat _previousFrame;
        private int _stableFrameCount;
        private readonly object _lockObject = new object();

        public FrameComparisonService(FrameComparisonConfiguration configuration, ILogger<FrameComparisonService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _stableFrameCount = 0;
        }

        public bool ShouldProcessFrame(byte[] currentFrame)
        {
            if (!_configuration.EnableFrameComparison)
            {
                return true; // Always process when frame comparison is disabled
            }

            if (currentFrame == null || currentFrame.Length == 0)
            {
                _logger.LogWarning("Received null or empty frame for comparison");
                return true; // Process frame if we can't compare
            }

            lock (_lockObject)
            {
                try
                {
                    using var currentMat = ConvertToMat(currentFrame);
                    
                    if (_previousFrame == null)
                    {
                        // First frame - store it and don't process
                        _previousFrame = currentMat.Clone();
                        _stableFrameCount = 0;
                        _logger.LogTrace("First frame stored, skipping processing");
                        return false;
                    }

                    double similarity = CalculateFrameSimilarity(_previousFrame, currentMat);
                    bool framesAreSimilar = similarity >= _configuration.ImageSimilarityThreshold;

                    if (framesAreSimilar)
                    {
                        _stableFrameCount++;
                        _logger.LogTrace($"Frame similarity: {similarity:F3}, stable count: {_stableFrameCount}");
                    }
                    else
                    {
                        _stableFrameCount = 0;
                        _logger.LogTrace($"Frame changed (similarity: {similarity:F3}), resetting stable count");
                    }

                    // Always update the previous frame with current frame
                    _previousFrame?.Dispose();
                    _previousFrame = currentMat.Clone();

                    // Only process if we've had enough stable frames
                    bool shouldProcess = _stableFrameCount >= _configuration.StableFramesRequired;
                    
                    if (shouldProcess)
                    {
                        _logger.LogTrace($"Processing frame after {_stableFrameCount} stable iterations");
                        // Reset counter after processing to avoid continuous processing of the same stable content
                        _stableFrameCount = 0;
                    }

                    return shouldProcess;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during frame comparison, defaulting to process frame");
                    return true; // Process frame if comparison fails
                }
            }
        }

        public void Reset()
        {
            lock (_lockObject)
            {
                _previousFrame?.Dispose();
                _previousFrame = null;
                _stableFrameCount = 0;
                _logger.LogTrace("Frame comparison state reset");
            }
        }

        private Mat ConvertToMat(byte[] frameData)
        {
            using var bitmap = new Bitmap(new MemoryStream(frameData));
            var mat = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);
            
            if (_configuration.UseGrayscaleComparison && mat.Channels() > 1)
            {
                var grayMat = new Mat();
                Cv2.CvtColor(mat, grayMat, ColorConversionCodes.BGR2GRAY);
                mat.Dispose();
                return grayMat;
            }
            
            return mat;
        }

        private double CalculateFrameSimilarity(Mat frame1, Mat frame2)
        {
            if (frame1.Size() != frame2.Size())
            {
                _logger.LogWarning("Frame sizes don't match, resizing for comparison");
                using var resized = new Mat();
                Cv2.Resize(frame2, resized, frame1.Size());
                return CalculateFrameSimilarityInternal(frame1, resized);
            }

            return CalculateFrameSimilarityInternal(frame1, frame2);
        }

        private double CalculateFrameSimilarityInternal(Mat frame1, Mat frame2)
        {
            // Use Structural Similarity Index (SSIM) approach with mean squared error
            using var diff = new Mat();
            Cv2.Absdiff(frame1, frame2, diff);
            
            // Calculate mean squared error
            var mse = Cv2.Mean(diff).Val0;
            
            // Convert MSE to similarity score (0-1, where 1 is identical)
            // For 8-bit images, max possible MSE is 255^2 = 65025
            var maxMse = 255.0 * 255.0;
            var similarity = 1.0 - (mse / maxMse);
            
            return Math.Max(0.0, similarity); // Ensure non-negative
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                _previousFrame?.Dispose();
                _previousFrame = null;
            }
        }
    }
}