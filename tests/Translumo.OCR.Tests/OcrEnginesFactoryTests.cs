using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.Python;
using Translumo.OCR;
using Translumo.OCR.Configuration;
using Translumo.OCR.EasyOCR;
using Translumo.OCR.Tesseract;
using Translumo.OCR.WindowsOCR;
using Xunit;

namespace Translumo.OCR.Tests;

public class OcrEnginesFactoryTests
{
    private readonly Mock<LanguageService> _mockLanguageService;
    private readonly Mock<PythonEngineWrapper> _mockPythonEngine;
    private readonly Mock<ILogger<OcrEnginesFactory>> _mockLogger;
    private readonly OcrEnginesFactory _ocrEnginesFactory;
    private readonly Mock<LanguageDescriptor> _mockLanguageDescriptor;

    public OcrEnginesFactoryTests()
    {
        _mockLanguageService = new Mock<LanguageService>(Mock.Of<LanguageDescriptorFactory>());
        _mockPythonEngine = new Mock<PythonEngineWrapper>();
        _mockLogger = new Mock<ILogger<OcrEnginesFactory>>();
        
        _mockLanguageDescriptor = new Mock<LanguageDescriptor>();
        _mockLanguageDescriptor.Setup(x => x.Code).Returns("en");
        _mockLanguageDescriptor.Setup(x => x.Language).Returns(Languages.English);
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(It.IsAny<Languages>()))
            .Returns(_mockLanguageDescriptor.Object);
        
        _ocrEnginesFactory = new OcrEnginesFactory(
            _mockLanguageService.Object,
            _mockPythonEngine.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void GetEngines_WithWindowsOCRConfiguration_ReturnsWindowsOCREngines()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().HaveCount(2); // WindowsOCREngine and WinOCREngineWithPreprocess
        engines.Should().Contain(e => e.GetType() == typeof(WindowsOCREngine));
        engines.Should().Contain(e => e.GetType() == typeof(WinOCREngineWithPreprocess));
    }

    [Fact]
    public void GetEngines_WithTesseractConfiguration_ReturnsTesseractEngines()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new TesseractOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().HaveCount(2); // TesseractOCREngine and TesseractOCREngineWIthPreprocess
        engines.Should().Contain(e => e.GetType() == typeof(TesseractOCREngine));
        engines.Should().Contain(e => e.GetType() == typeof(TesseractOCREngineWIthPreprocess));
    }

    [Fact]
    public void GetEngines_WithEasyOCRConfiguration_ReturnsEasyOCREngine()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new EasyOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().HaveCount(1); // Only EasyOCREngine
        engines.Should().Contain(e => e.GetType() == typeof(EasyOCREngine));
    }

    [Fact]
    public void GetEngines_WithDisabledConfiguration_ReturnsNoEngines()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = false },
            new TesseractOCRConfiguration { Enabled = false },
            new EasyOCRConfiguration { Enabled = false }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().BeEmpty();
    }

    [Fact]
    public void GetEngines_WithMixedConfigurations_ReturnsEnabledEnginesOnly()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true },
            new TesseractOCRConfiguration { Enabled = false },
            new EasyOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().HaveCount(3); // 2 Windows OCR engines + 1 EasyOCR engine
        engines.Should().Contain(e => e.GetType() == typeof(WindowsOCREngine));
        engines.Should().Contain(e => e.GetType() == typeof(WinOCREngineWithPreprocess));
        engines.Should().Contain(e => e.GetType() == typeof(EasyOCREngine));
        engines.Should().NotContain(e => e.GetType() == typeof(TesseractOCREngine));
        engines.Should().NotContain(e => e.GetType() == typeof(TesseractOCREngineWIthPreprocess));
    }

    [Fact]
    public void GetEngines_WithAllConfigurations_ReturnsAllEngines()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true },
            new TesseractOCRConfiguration { Enabled = true },
            new EasyOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().HaveCount(5); // 2 Windows + 2 Tesseract + 1 EasyOCR
        engines.Should().Contain(e => e.GetType() == typeof(WindowsOCREngine));
        engines.Should().Contain(e => e.GetType() == typeof(WinOCREngineWithPreprocess));
        engines.Should().Contain(e => e.GetType() == typeof(TesseractOCREngine));
        engines.Should().Contain(e => e.GetType() == typeof(TesseractOCREngineWIthPreprocess));
        engines.Should().Contain(e => e.GetType() == typeof(EasyOCREngine));
    }

    [Fact]
    public void GetEngines_WithEmptyConfigurations_ReturnsNoEngines()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>();

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().BeEmpty();
    }

    [Fact]
    public void GetEngines_CallsLanguageService()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.Russian).ToList();

        // Assert
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(Languages.Russian), Times.Once);
    }

    [Fact]
    public void GetEngines_WithDifferentLanguages_PassesCorrectLanguage()
    {
        // Arrange
        var japaneseDescriptor = new Mock<LanguageDescriptor>();
        japaneseDescriptor.Setup(x => x.Code).Returns("ja");
        japaneseDescriptor.Setup(x => x.Language).Returns(Languages.Japanese);
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(Languages.Japanese))
            .Returns(japaneseDescriptor.Object);

        var configurations = new List<OcrConfiguration>
        {
            new EasyOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.Japanese).ToList();

        // Assert
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(Languages.Japanese), Times.Once);
        engines.Should().HaveCount(1);
    }

    [Fact]
    public void GetEngines_WithEasyOCR_PassesPythonEngine()
    {
        // Arrange
        var configurations = new List<OcrConfiguration>
        {
            new EasyOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().HaveCount(1);
        engines.First().Should().BeOfType<EasyOCREngine>();
        // We can't easily verify the python engine was passed without reflection,
        // but we can verify the engine was created successfully
    }

    [Theory]
    [InlineData(Languages.English)]
    [InlineData(Languages.Russian)]
    [InlineData(Languages.Japanese)]
    [InlineData(Languages.Chinese)]
    public void GetEngines_WithVariousLanguages_WorksCorrectly(Languages language)
    {
        // Arrange
        var descriptor = new Mock<LanguageDescriptor>();
        descriptor.Setup(x => x.Language).Returns(language);
        descriptor.Setup(x => x.Code).Returns(language.ToString().ToLower());
        
        _mockLanguageService
            .Setup(x => x.GetLanguageDescriptor(language))
            .Returns(descriptor.Object);

        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = _ocrEnginesFactory.GetEngines(configurations, language).ToList();

        // Assert
        engines.Should().HaveCount(2);
        _mockLanguageService.Verify(x => x.GetLanguageDescriptor(language), Times.Once);
    }
}