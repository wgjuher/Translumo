using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Translumo.Infrastructure.Language;
using Translumo.Infrastructure.Python;
using Translumo.OCR;
using Translumo.OCR.Configuration;
using Translumo.OCR.WindowsOCR;
using Translumo.OCR.Tesseract;
using Translumo.Translation;
using Translumo.Translation.Configuration;
using Translumo.TTS;
using Xunit;

namespace Translumo.Integration.Tests;

public class BasicIntegrationTests
{
    private readonly IServiceProvider _serviceProvider;

    public BasicIntegrationTests()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder => builder.AddConsole());

        // Mock dependencies that require external resources
        var mockPythonEngine = new Mock<PythonEngineWrapper>();
        services.AddSingleton(mockPythonEngine.Object);

        // Add language services
        services.AddSingleton<LanguageDescriptorFactory>();
        services.AddSingleton<LanguageService>();

        // Add factories
        services.AddSingleton<OcrEnginesFactory>();
        services.AddSingleton<TranslatorFactory>();
        services.AddSingleton<TtsFactory>();

        // Mock action dispatcher for translation services
        var mockActionDispatcher = new Mock<Translumo.Infrastructure.Dispatching.IActionDispatcher>();
        services.AddSingleton(mockActionDispatcher.Object);
    }

    [Fact]
    public void ServiceProvider_CanResolveAllFactories()
    {
        // Act & Assert
        var ocrFactory = _serviceProvider.GetService<OcrEnginesFactory>();
        var translatorFactory = _serviceProvider.GetService<TranslatorFactory>();
        var ttsFactory = _serviceProvider.GetService<TtsFactory>();

        ocrFactory.Should().NotBeNull();
        translatorFactory.Should().NotBeNull();
        ttsFactory.Should().NotBeNull();
    }

    [Fact]
    public void LanguageService_CanResolveLanguageDescriptors()
    {
        // Arrange
        var languageService = _serviceProvider.GetRequiredService<LanguageService>();

        // Act
        var englishDescriptor = languageService.GetLanguageDescriptor(Languages.English);
        var russianDescriptor = languageService.GetLanguageDescriptor(Languages.Russian);

        // Assert
        englishDescriptor.Should().NotBeNull();
        englishDescriptor.Language.Should().Be(Languages.English);
        
        russianDescriptor.Should().NotBeNull();
        russianDescriptor.Language.Should().Be(Languages.Russian);
    }

    [Fact]
    public void OcrEnginesFactory_CanCreateEnginesWithDependencies()
    {
        // Arrange
        var ocrFactory = _serviceProvider.GetRequiredService<OcrEnginesFactory>();
        var configurations = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true },
            new TesseractOCRConfiguration { Enabled = true }
        };

        // Act
        var engines = ocrFactory.GetEngines(configurations, Languages.English).ToList();

        // Assert
        engines.Should().NotBeEmpty();
        engines.Should().HaveCount(4); // 2 Windows OCR + 2 Tesseract engines
    }

    [Fact]
    public void TranslatorFactory_CanCreateTranslatorsWithDependencies()
    {
        // Arrange
        var translatorFactory = _serviceProvider.GetRequiredService<TranslatorFactory>();
        var config = new TranslationConfiguration
        {
            Translator = Translators.Google,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().NotBeNull();
        translator.Should().BeAssignableTo<ITranslator>();
    }

    [Fact]
    public void TtsFactory_CanCreateEnginesWithDependencies()
    {
        // Arrange
        var ttsFactory = _serviceProvider.GetRequiredService<TtsFactory>();
        var config = new TtsConfiguration
        {
            TtsSystem = TTSEngines.None,
            TtsLanguage = Languages.English
        };

        // Act
        var engine = ttsFactory.CreateTtsEngine(config);

        // Assert
        engine.Should().NotBeNull();
        engine.Should().BeAssignableTo<Translumo.TTS.Engines.ITTSEngine>();
    }

    [Fact]
    public void LanguageService_GetAll_ReturnsMultipleLanguages()
    {
        // Arrange
        var languageService = _serviceProvider.GetRequiredService<LanguageService>();

        // Act
        var allLanguages = languageService.GetAll().ToList();
        var allLanguagesIncludingTranslationOnly = languageService.GetAll(includeTranslationOnly: true).ToList();

        // Assert
        allLanguages.Should().NotBeEmpty();
        allLanguagesIncludingTranslationOnly.Should().NotBeEmpty();
        allLanguagesIncludingTranslationOnly.Count.Should().BeGreaterThanOrEqualTo(allLanguages.Count);
    }

    [Fact]
    public void MultipleFactories_CanWorkTogether()
    {
        // Arrange
        var ocrFactory = _serviceProvider.GetRequiredService<OcrEnginesFactory>();
        var translatorFactory = _serviceProvider.GetRequiredService<TranslatorFactory>();
        var ttsFactory = _serviceProvider.GetRequiredService<TtsFactory>();

        var ocrConfigs = new List<OcrConfiguration>
        {
            new WindowsOCRConfiguration { Enabled = true }
        };

        var translationConfig = new TranslationConfiguration
        {
            Translator = Translators.Google,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        var ttsConfig = new TtsConfiguration
        {
            TtsSystem = TTSEngines.None,
            TtsLanguage = Languages.English
        };

        // Act
        var ocrEngines = ocrFactory.GetEngines(ocrConfigs, Languages.English).ToList();
        var translator = translatorFactory.CreateTranslator(translationConfig);
        var ttsEngine = ttsFactory.CreateTtsEngine(ttsConfig);

        // Assert
        ocrEngines.Should().NotBeEmpty();
        translator.Should().NotBeNull();
        ttsEngine.Should().NotBeNull();

        // Verify they can all be created without conflicts
        ocrEngines.Should().HaveCount(2); // Windows OCR engines
        translator.Should().BeAssignableTo<ITranslator>();
        ttsEngine.Should().BeAssignableTo<Translumo.TTS.Engines.ITTSEngine>();
    }

    [Theory]
    [InlineData(Languages.English)]
    [InlineData(Languages.Russian)]
    [InlineData(Languages.Japanese)]
    [InlineData(Languages.Chinese)]
    public void LanguageService_SupportsMultipleLanguages(Languages language)
    {
        // Arrange
        var languageService = _serviceProvider.GetRequiredService<LanguageService>();

        // Act
        var descriptor = languageService.GetLanguageDescriptor(language);

        // Assert
        descriptor.Should().NotBeNull();
        descriptor.Language.Should().Be(language);
        descriptor.Code.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ServiceProvider_HandlesDisposal()
    {
        // Arrange
        var scope = _serviceProvider.CreateScope();
        var ocrFactory = scope.ServiceProvider.GetRequiredService<OcrEnginesFactory>();

        // Act & Assert - Should not throw
        Action act = () => scope.Dispose();
        act.Should().NotThrow();
    }
}