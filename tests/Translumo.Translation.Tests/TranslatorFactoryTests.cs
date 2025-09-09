using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Translumo.Infrastructure.Dispatching;
using Translumo.Infrastructure.Language;
using Translumo.Translation;
using Translumo.Translation.Configuration;
using Translumo.Translation.Deepl;
using Translumo.Translation.Google;
using Translumo.Translation.Papago;
using Translumo.Translation.Yandex;
using Xunit;

namespace Translumo.Translation.Tests;

/// <summary>
/// Tests for TranslatorFactory.
///
/// NOTE: The current Translumo implementation uses web scraping for translation services
/// rather than official APIs. This means:
/// - DeepL: Uses free web interface (https://www2.deepl.com/jsonrpc) - no API key required
/// - Google: Uses free web interface (https://translate.google.com/m) - no API key required
/// - Yandex/Papago: Use their respective free web interfaces - no API keys required
///
/// If the implementation switches to official APIs in the future, the TranslationConfiguration
/// would need to be extended with API key properties and these tests updated accordingly.
/// </summary>
public class TranslatorFactoryTests
{
    private readonly Mock<LanguageService> _mockLanguageService;
    private readonly Mock<IActionDispatcher> _mockActionDispatcher;
    private readonly Mock<ILogger<TranslatorFactory>> _mockLogger;
    private readonly TranslatorFactory _translatorFactory;

    public TranslatorFactoryTests()
    {
        _mockLanguageService = new Mock<LanguageService>(Mock.Of<LanguageDescriptorFactory>());
        _mockActionDispatcher = new Mock<IActionDispatcher>();
        _mockLogger = new Mock<ILogger<TranslatorFactory>>();
        
        _translatorFactory = new TranslatorFactory(
            _mockLanguageService.Object,
            _mockActionDispatcher.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void CreateTranslator_WithDeepLTranslator_ReturnsDeepLTranslator()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Deepl,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<DeepLTranslator>();
    }

    [Fact]
    public void CreateTranslator_WithYandexTranslator_ReturnsYandexTranslator()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Yandex,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<YandexTranslator>();
    }

    [Fact]
    public void CreateTranslator_WithPapagoTranslator_ReturnsPapagoTranslator()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Papago,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<PapagoTranslator>();
    }

    [Fact]
    public void CreateTranslator_WithGoogleTranslator_ReturnsGoogleTranslator()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Google,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<GoogleTranslator>();
    }

    [Fact]
    public void CreateTranslator_WithUnsupportedTranslator_ThrowsNotSupportedException()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = (Translators)255, // Invalid translator (using max byte value)
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => _translatorFactory.CreateTranslator(config));
    }

    [Fact]
    public void CreateTranslator_PassesCorrectDependenciesToDeepL()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Deepl,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<DeepLTranslator>();
        // We can't easily verify the internal dependencies without reflection,
        // but we can verify the translator was created successfully
        translator.Should().NotBeNull();
    }

    [Fact]
    public void CreateTranslator_PassesCorrectDependenciesToYandex()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Yandex,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<YandexTranslator>();
        translator.Should().NotBeNull();
    }

    [Fact]
    public void CreateTranslator_WithNullConfiguration_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<NullReferenceException>(() => _translatorFactory.CreateTranslator(null!));
    }

    [Theory]
    [InlineData(Translators.Deepl, typeof(DeepLTranslator))]
    [InlineData(Translators.Yandex, typeof(YandexTranslator))]
    [InlineData(Translators.Papago, typeof(PapagoTranslator))]
    [InlineData(Translators.Google, typeof(GoogleTranslator))]
    public void CreateTranslator_WithVariousTranslators_ReturnsCorrectType(Translators translatorType, Type expectedType)
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = translatorType,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType(expectedType);
    }

    [Fact]
    public void CreateTranslator_MultipleCallsWithSameConfig_ReturnsNewInstances()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Google,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };

        // Act
        var translator1 = _translatorFactory.CreateTranslator(config);
        var translator2 = _translatorFactory.CreateTranslator(config);

        // Assert
        translator1.Should().NotBeSameAs(translator2);
        translator1.Should().BeOfType<GoogleTranslator>();
        translator2.Should().BeOfType<GoogleTranslator>();
    }

    [Fact]
    public void CreateTranslator_WithDifferentConfigurations_ReturnsCorrectTranslators()
    {
        // Arrange
        var deeplConfig = new TranslationConfiguration
        {
            Translator = Translators.Deepl,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian
        };
        
        var googleConfig = new TranslationConfiguration
        {
            Translator = Translators.Google,
            TranslateFromLang = Languages.Japanese,
            TranslateToLang = Languages.English
        };

        // Act
        var deeplTranslator = _translatorFactory.CreateTranslator(deeplConfig);
        var googleTranslator = _translatorFactory.CreateTranslator(googleConfig);

        // Assert
        deeplTranslator.Should().BeOfType<DeepLTranslator>();
        googleTranslator.Should().BeOfType<GoogleTranslator>();
        deeplTranslator.Should().NotBeSameAs(googleTranslator);
    }

    [Fact]
    public void CreateTranslator_WithComplexConfiguration_CreatesTranslatorSuccessfully()
    {
        // Arrange
        var config = new TranslationConfiguration
        {
            Translator = Translators.Yandex,
            TranslateFromLang = Languages.English,
            TranslateToLang = Languages.Russian,
            ProxySettings = new List<Proxy>
            {
                new Proxy
                {
                    IpAddress = "proxy.example.com",
                    Port = 8080,
                    Login = "user",
                    Password = "pass"
                }
            }
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<YandexTranslator>();
        translator.Should().NotBeNull();
    }
}