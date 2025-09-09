using FluentAssertions;
using System.Text;
using Translumo.Infrastructure.Encryption;
using Xunit;

namespace Translumo.Infrastructure.Tests.Encryption;

public class AesEncryptionServiceTests : IDisposable
{
    private readonly AesEncryptionService _encryptionService;

    public AesEncryptionServiceTests()
    {
        _encryptionService = new AesEncryptionService();
    }

    [Fact]
    public void Encrypt_WithValidData_ReturnsEncryptedBytes()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password = "TestPassword123";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));

        // Act
        var encryptedData = _encryptionService.Encrypt(stream, password);

        // Assert
        encryptedData.Should().NotBeNull();
        encryptedData.Length.Should().BeGreaterThan(0);
        encryptedData.Should().NotEqual(Encoding.UTF8.GetBytes(originalText));
    }

    [Fact]
    public void Decrypt_WithValidEncryptedData_ReturnsOriginalText()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password = "TestPassword123";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void EncryptDecrypt_WithEmptyString_WorksCorrectly()
    {
        // Arrange
        const string originalText = "";
        const string password = "TestPassword123";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void EncryptDecrypt_WithLongText_WorksCorrectly()
    {
        // Arrange
        var originalText = string.Join("", Enumerable.Repeat("This is a long text that should be encrypted and decrypted correctly. ", 100));
        const string password = "TestPassword123";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void EncryptDecrypt_WithUnicodeCharacters_WorksCorrectly()
    {
        // Arrange
        const string originalText = "Hello 世界! Привет мир! 🌍🔒";
        const string password = "TestPassword123";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void Encrypt_WithDifferentPasswords_ProducesDifferentResults()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password1 = "Password1";
        const string password2 = "Password2";
        
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(originalText));

        // Act
        var encryptedData1 = _encryptionService.Encrypt(stream1, password1);
        var encryptedData2 = _encryptionService.Encrypt(stream2, password2);

        // Assert
        encryptedData1.Should().NotEqual(encryptedData2);
    }

    [Fact]
    public void Encrypt_SameDataMultipleTimes_ProducesDifferentResults()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password = "TestPassword123";
        
        using var stream1 = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        using var stream2 = new MemoryStream(Encoding.UTF8.GetBytes(originalText));

        // Act
        var encryptedData1 = _encryptionService.Encrypt(stream1, password);
        var encryptedData2 = _encryptionService.Encrypt(stream2, password);

        // Assert
        encryptedData1.Should().NotEqual(encryptedData2); // Due to random IV and salt
    }

    [Fact]
    public void Decrypt_WithWrongPassword_ThrowsException()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string correctPassword = "CorrectPassword";
        const string wrongPassword = "WrongPassword";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, correctPassword);

        // Act & Assert
        using var encryptedStream = new MemoryStream(encryptedData);
        Assert.Throws<System.Security.Cryptography.CryptographicException>(() => 
            _encryptionService.Decrypt(encryptedStream, wrongPassword));
    }

    [Fact]
    public void Encrypt_WithSpecialCharactersInPassword_WorksCorrectly()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password = "P@ssw0rd!@#$%^&*()_+-=[]{}|;':\",./<>?";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void Encrypt_WithVeryShortPassword_WorksCorrectly()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password = "a";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void Encrypt_WithVeryLongPassword_WorksCorrectly()
    {
        // Arrange
        const string originalText = "Hello, World!";
        var password = string.Join("", Enumerable.Repeat("VeryLongPassword", 100));
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    [Fact]
    public void EncryptedData_ContainsExpectedComponents()
    {
        // Arrange
        const string originalText = "Hello, World!";
        const string password = "TestPassword123";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));

        // Act
        var encryptedData = _encryptionService.Encrypt(stream, password);

        // Assert
        // Encrypted data should contain: salt (16 bytes) + IV (16 bytes) + cipher text
        encryptedData.Length.Should().BeGreaterThan(32); // At least salt + IV
        
        // The first 16 bytes should be the salt, next 16 bytes should be IV
        // We can't test the exact values since they're random, but we can test structure
        encryptedData.Take(16).Should().NotBeEquivalentTo(new byte[16]); // Salt shouldn't be all zeros
        encryptedData.Skip(16).Take(16).Should().NotBeEquivalentTo(new byte[16]); // IV shouldn't be all zeros
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("Hello, World!")]
    [InlineData("This is a longer text with multiple sentences. It contains various characters and should be encrypted properly.")]
    public void EncryptDecrypt_WithVariousTextLengths_WorksCorrectly(string originalText)
    {
        // Arrange
        const string password = "TestPassword123";
        
        using var originalStream = new MemoryStream(Encoding.UTF8.GetBytes(originalText));
        var encryptedData = _encryptionService.Encrypt(originalStream, password);

        // Act
        using var encryptedStream = new MemoryStream(encryptedData);
        var decryptedText = _encryptionService.Decrypt(encryptedStream, password);

        // Assert
        decryptedText.Should().Be(originalText);
    }

    public void Dispose()
    {
        // AesEncryptionService doesn't implement IDisposable, but we follow the pattern for consistency
    }
}