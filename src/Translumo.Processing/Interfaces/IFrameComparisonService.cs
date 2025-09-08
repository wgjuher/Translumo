using System;

namespace Translumo.Processing.Interfaces
{
    /// <summary>
    /// Service for comparing captured frames to determine if translation should be triggered.
    /// Only processes translation when frames have been stable for a configured number of iterations.
    /// </summary>
    public interface IFrameComparisonService : IDisposable
    {
        /// <summary>
        /// Determines if the current frame should trigger OCR and translation processing.
        /// </summary>
        /// <param name="currentFrame">The current captured frame as byte array</param>
        /// <returns>True if the frame should be processed (stable for required iterations), false otherwise</returns>
        bool ShouldProcessFrame(byte[] currentFrame);

        /// <summary>
        /// Resets the frame comparison state, clearing stored frames and counters.
        /// Should be called when translation processing starts or stops.
        /// </summary>
        void Reset();
    }
}