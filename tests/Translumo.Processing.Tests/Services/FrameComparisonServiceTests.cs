using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Drawing;
using System.Drawing.Imaging;
using Translumo.Processing.Configuration;
using Translumo.Processing.Services;
using Xunit;

namespace Translumo.Processing.Tests.Services;

public class FrameComparisonServiceTests : IDisposable
{
    private readonly Mock<ILogger<FrameComparisonService>> _mockLogger;
    private readonly FrameComparisonConfiguration _defaultConfig;
    private readonly FrameComparisonService _frameComparisonService;

    public FrameComparisonServiceTests()
    {
        _mockLogger = new Mock<ILogger<FrameComparisonService>>();
        _defaultConfig = new FrameComparisonConfiguration
        {
            EnableFrameComparison = true,
            ImageSimilarityThreshold = 0.95,
            StableFramesRequired = 3,
            UseGrayscaleComparison = true
        };
        _frameComparisonService = new FrameComparisonService(_defaultConfig, _mockLogger.Object);
    }

    [Fact]
    public void ShouldProcessFrame_WhenFrameComparisonDisabled_ReturnsTrue()
    {
        // Arrange
        var config = new FrameComparisonConfiguration { EnableFrameComparison = false };
        var service = new FrameComparisonService(config, _mockLogger.Object);
        var frameData = CreateTestImageBytes(100, 100, Color.Red);

        // Act
        var result = service.ShouldProcessFrame(frameData);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldProcessFrame_WithNullFrame_ReturnsTrue()
    {
        // Act
        var result = _frameComparisonService.ShouldProcessFrame(null);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldProcessFrame_WithEmptyFrame_ReturnsTrue()
    {
        // Act
        var result = _frameComparisonService.ShouldProcessFrame(Array.Empty<byte>());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldProcessFrame_FirstFrame_ReturnsFalse()
    {
        // Arrange
        var frameData = CreateTestImageBytes(100, 100, Color.Red);

        // Act
        var result = _frameComparisonService.ShouldProcessFrame(frameData);

        // Assert
        result.Should().BeFalse(); // First frame should not be processed
    }

    [Fact]
    public void ShouldProcessFrame_WithIdenticalFrames_EventuallyReturnsTrue()
    {
        // Arrange
        var frameData = CreateTestImageBytes(100, 100, Color.Red);
        
        // Act & Assert
        // First frame
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeFalse();
        
        // Second frame (identical)
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeFalse();
        
        // Third frame (identical)
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeFalse();
        
        // Fourth frame (identical) - should trigger processing after 3 stable frames
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeTrue();
    }

    [Fact]
    public void ShouldProcessFrame_WithDifferentFrames_ResetsStableCount()
    {
        // Arrange
        var frameData1 = CreateTestImageBytes(100, 100, Color.Red);
        var frameData2 = CreateTestImageBytes(100, 100, Color.Blue);
        
        // Act & Assert
        // First frame
        _frameComparisonService.ShouldProcessFrame(frameData1).Should().BeFalse();
        
        // Second frame (identical)
        _frameComparisonService.ShouldProcessFrame(frameData1).Should().BeFalse();
        
        // Third frame (different) - should reset stable count
        _frameComparisonService.ShouldProcessFrame(frameData2).Should().BeFalse();
        
        // Fourth frame (same as third)
        _frameComparisonService.ShouldProcessFrame(frameData2).Should().BeFalse();
        
        // Fifth frame (same as third and fourth)
        _frameComparisonService.ShouldProcessFrame(frameData2).Should().BeFalse();
        
        // Sixth frame (same) - should trigger processing
        _frameComparisonService.ShouldProcessFrame(frameData2).Should().BeTrue();
    }

    [Fact]
    public void ShouldProcessFrame_AfterProcessing_ResetsStableCount()
    {
        // Arrange
        var frameData = CreateTestImageBytes(100, 100, Color.Red);
        
        // Build up stable frames
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeFalse(); // First
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeFalse(); // Second
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeFalse(); // Third
        _frameComparisonService.ShouldProcessFrame(frameData).Should().BeTrue();  // Fourth - triggers processing
        
        // Act - Next identical frame should not immediately trigger processing
        var result = _frameComparisonService.ShouldProcessFrame(frameData);
        
        // Assert
        result.Should().BeFalse(); // Should reset and require stable frames again
    }

    [Fact]
    public void Reset_ClearsInternalState()
    {
        // Arrange
        var frameData = CreateTestImageBytes(100, 100, Color.Red);
        _frameComparisonService.ShouldProcessFrame(frameData); // Store first frame
        
        // Act
        _frameComparisonService.Reset();
        
        // Assert - Next frame should be treated as first frame again
        var result = _frameComparisonService.ShouldProcessFrame(frameData);
        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldProcessFrame_WithCustomSimilarityThreshold_WorksCorrectly()
    {
        // Arrange
        var config = new FrameComparisonConfiguration
        {
            EnableFrameComparison = true,
            ImageSimilarityThreshold = 0.5, // Lower threshold
            StableFramesRequired = 2,
            UseGrayscaleComparison = true
        };
        var service = new FrameComparisonService(config, _mockLogger.Object);
        
        var frameData1 = CreateTestImageBytes(100, 100, Color.Red);
        var frameData2 = CreateTestImageBytes(100, 100, Color.FromArgb(200, 0, 0)); // Slightly different red
        
        // Act & Assert
        service.ShouldProcessFrame(frameData1).Should().BeFalse(); // First frame
        service.ShouldProcessFrame(frameData2).Should().BeFalse(); // Similar enough
        service.ShouldProcessFrame(frameData2).Should().BeTrue();  // Should trigger processing
    }

    [Fact]
    public void ShouldProcessFrame_WithCustomStableFramesRequired_WorksCorrectly()
    {
        // Arrange
        var config = new FrameComparisonConfiguration
        {
            EnableFrameComparison = true,
            ImageSimilarityThreshold = 0.95,
            StableFramesRequired = 1, // Only 1 stable frame required
            UseGrayscaleComparison = true
        };
        var service = new FrameComparisonService(config, _mockLogger.Object);
        var frameData = CreateTestImageBytes(100, 100, Color.Red);
        
        // Act & Assert
        service.ShouldProcessFrame(frameData).Should().BeFalse(); // First frame
        service.ShouldProcessFrame(frameData).Should().BeTrue();  // Should trigger immediately
    }

    [Fact]
    public void ShouldProcessFrame_WithException_ReturnsTrue()
    {
        // Arrange - Create invalid image data that should cause an exception
        var invalidFrameData = new byte[] { 1, 2, 3, 4, 5 };
        
        // Act
        var result = _frameComparisonService.ShouldProcessFrame(invalidFrameData);
        
        // Assert
        result.Should().BeTrue(); // Should default to processing when comparison fails
    }

    [Fact]
    public void Dispose_CleansUpResources()
    {
        // Arrange
        var frameData = CreateTestImageBytes(100, 100, Color.Red);
        _frameComparisonService.ShouldProcessFrame(frameData); // Store a frame
        
        // Act
        _frameComparisonService.Dispose();
        
        // Assert - Should not throw when disposed
        Action act = () => _frameComparisonService.Dispose();
        act.Should().NotThrow();
    }

    [Fact]
    public void ShouldProcessFrame_WithDifferentImageSizes_HandlesCorrectly()
    {
        // Arrange
        var frameData1 = CreateTestImageBytes(100, 100, Color.Red);
        var frameData2 = CreateTestImageBytes(200, 200, Color.Red); // Different size, same color
        
        // Act & Assert
        _frameComparisonService.ShouldProcessFrame(frameData1).Should().BeFalse(); // First frame
        
        // Different size should still be compared (with resizing)
        var result = _frameComparisonService.ShouldProcessFrame(frameData2);
        result.Should().BeFalse(); // Should handle size difference
    }

    private static byte[] CreateTestImageBytes(int width, int height, Color color)
    {
        using var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        using var brush = new SolidBrush(color);
        
        graphics.FillRectangle(brush, 0, 0, width, height);
        
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    public void Dispose()
    {
        _frameComparisonService?.Dispose();
    }
}