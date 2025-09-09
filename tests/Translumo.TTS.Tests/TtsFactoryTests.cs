using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.Python;
using Translumo.TTS;
using Translumo.TTS.Engines;
using Xunit;

namespace Translumo.TTS.Tests;

public class TtsFactoryTests
{
    private readonly Mock<LanguageService> _mockLanguageService;
    private readonly Mock<PythonEngineWrapper> _mockPythonEngine;
    private readonly Mock<ILogger<TtsFactory>> _mockLogger;
    private readonly TtsFactory _ttsFactory;

    public TtsFactoryTests()
    {
        _mockLanguageService = new Mock<LanguageService>(Mock.Of<LanguageDescriptorFactory>());
        _mockPythonEngine = new Mock<PythonEngineWrapper>();
        _mockLogger = new Mock<ILogger<TtsFactory>>();
        
        _ttsFactory = new TtsFactory(
            _mockLanguageService.Object,
            _mockPythonEngine.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void CreateTtsEngine_WithNoneTTS_ReturnsNoneTTSEngine()
    {
        // Arrange
        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.None,
            TtsLanguage = Languages.English
        };

        // Act
        var engine = _ttsFactory.CreateTtsEngine(config);

        // Assert
        engine.Should().BeOfType<NoneTTSEngine>();
    }

    [Fact]
    public void CreateTtsEngine_WithWindowsTTS_ReturnsWindowsTTSEngine()
    {
        // Arrange
        var mockLanguageDescriptor = new Mock<LanguageDescriptor>();
        mockLanguageDescriptor.Setup(x => x.Code).Returns("en-US");
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(Languages.English))
            .Returns(mockLanguageDescriptor.Object);

        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.WindowsTTS,
            TtsLanguage = Languages.English
        };

        // Act
        var engine = _ttsFactory.CreateTtsEngine(config);

        // Assert
        engine.Should().BeOfType<WindowsTTSEngine>();
    }

    [Fact]
    public void CreateTtsEngine_WithUnsupportedEngine_ThrowsNotSupportedException()
    {
        // Arrange
        var config = new TtsConfiguration
        {
            TtsSystem = (TTSEngines)999, // Invalid engine
            TtsLanguage = Languages.English
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _ttsFactory.CreateTtsEngine(config));
    }

    [Fact]
    public void CreateTtsEngine_WithWindowsTTS_CallsLanguageService()
    {
        // Arrange
        var mockLanguageDescriptor = new Mock<LanguageDescriptor>();
        mockLanguageDescriptor.Setup(x => x.Code).Returns("ru-RU");
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(Languages.Russian))
            .Returns(mockLanguageDescriptor.Object);

        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.WindowsTTS,
            TtsLanguage = Languages.Russian
        };

        // Act
        var engine = _ttsFactory.CreateTtsEngine(config);

        // Assert
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(Languages.Russian), Times.Once);
        engine.Should().BeOfType<WindowsTTSEngine>();
    }

    [Fact]
    public void CreateTtsEngine_WithNullConfiguration_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _ttsFactory.CreateTtsEngine(null!));
    }

    [Theory]
    [InlineData(TTSEngines.None, typeof(NoneTTSEngine))]
    [InlineData(TTSEngines.WindowsTTS, typeof(WindowsTTSEngine))]
    public void CreateTtsEngine_WithVariousEngines_ReturnsCorrectType(TTSEngines engineType, Type expectedType)
    {
        // Arrange
        if (engineType == TTSEngines.WindowsTTS)
        {
            var mockLanguageDescriptor = new Mock<LanguageDescriptor>();
            mockLanguageDescriptor.Setup(x => x.Code).Returns("en-US");
            
            _mockLanguageService
                .Setup(x => x.GetLanguageDescriptor(It.IsAny<Languages>()))
                .Returns(mockLanguageDescriptor.Object);
        }

        var config = new TtsConfiguration
        {
            TtsSystem = engineType,
            TtsLanguage = Languages.English
        };

        // Act
        var engine = _ttsFactory.CreateTtsEngine(config);

        // Assert
        engine.Should().BeOfType(expectedType);
    }

    [Fact]
    public void CreateTtsEngine_MultipleCallsWithSameConfig_ReturnsNewInstances()
    {
        // Arrange
        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.None,
            TtsLanguage = Languages.English
        };

        // Act
        var engine1 = _ttsFactory.CreateTtsEngine(config);
        var engine2 = _ttsFactory.CreateTtsEngine(config);

        // Assert
        engine1.Should().NotBeSameAs(engine2);
        engine1.Should().BeOfType<NoneTTSEngine>();
        engine2.Should().BeOfType<NoneTTSEngine>();
    }

    [Fact]
    public void CreateTtsEngine_WithDifferentLanguages_CallsLanguageServiceCorrectly()
    {
        // Arrange
        var englishDescriptor = new Mock<LanguageDescriptor>();
        englishDescriptor.Setup(x => x.Code).Returns("en-US");
        
        var russianDescriptor = new Mock<LanguageDescriptor>();
        russianDescriptor.Setup(x => x.Code).Returns("ru-RU");
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(Languages.English))
            .Returns(englishDescriptor.Object);
            
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(Languages.Russian))
            .Returns(russianDescriptor.Object);

        var englishConfig = new TtsConfiguration
        {
            TtsSystem = TTSEngines.WindowsTTS,
            TtsLanguage = Languages.English
        };
        
        var russianConfig = new TtsConfiguration
        {
            TtsSystem = TTSEngines.WindowsTTS,
            TtsLanguage = Languages.Russian
        };

        // Act
        var englishEngine = _ttsFactory.CreateTtsEngine(englishConfig);
        var russianEngine = _ttsFactory.CreateTtsEngine(russianConfig);

        // Assert
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(Languages.English), Times.Once);
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(Languages.Russian), Times.Once);
        
        englishEngine.Should().BeOfType<WindowsTTSEngine>();
        russianEngine.Should().BeOfType<WindowsTTSEngine>();
    }

    [Fact]
    public void CreateTtsEngine_WithNoneEngine_DoesNotCallLanguageService()
    {
        // Arrange
        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.None,
            TtsLanguage = Languages.English
        };

        // Act
        var engine = _ttsFactory.CreateTtsEngine(config);

        // Assert
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(It.IsAny<Languages>()), Times.Never);
        engine.Should().BeOfType<NoneTTSEngine>();
    }

    [Fact]
    public void CreateTtsEngine_WithWindowsTTS_PassesCorrectLanguageCode()
    {
        // Arrange
        var mockLanguageDescriptor = new Mock<LanguageDescriptor>();
        mockLanguageDescriptor.Setup(x => x.Code).Returns("ja-JP");
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(Languages.Japanese))
            .Returns(mockLanguageDescriptor.Object);

        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.WindowsTTS,
            TtsLanguage = Languages.Japanese
        };

        // Act
        var engine = _ttsFactory.CreateTtsEngine(config);

        // Assert
        engine.Should().BeOfType<WindowsTTSEngine>();
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(Languages.Japanese), Times.Once);
    }

    // Note: SileroTTS is commented out in the original code, so we don't test it
    // If it gets uncommented, we would add tests like:
    /*
    [Fact]
    public void CreateTtsEngine_WithSileroTTS_ReturnsSileroTTSEngine()
    {
        // This test would be added when SileroTTS is enabled
    }
    */
}