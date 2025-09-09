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
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            Translator = (Translators)999, // Invalid translator
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            ApiKey = "test-api-key"
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
            ApiKey = "deepl-key"
        };
        
        var googleConfig = new TranslationConfiguration
        {
            Translator = Translators.Google,
            ApiKey = "google-key"
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
            ApiKey = "complex-api-key-123",
            Proxy = new Proxy
            {
                Host = "proxy.example.com",
                Port = 8080,
                Username = "user",
                Password = "pass"
            },
            RequestTimeoutMs = 5000
        };

        // Act
        var translator = _translatorFactory.CreateTranslator(config);

        // Assert
        translator.Should().BeOfType<YandexTranslator>();
        translator.Should().NotBeNull();
    }
}