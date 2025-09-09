using FluentAssertions;
using Translumo.Utils.Extensions;
using Xunit;

namespace Translumo.Utils.Tests.Extensions;

public class StringExtensionsTests
{
    public class GetJaroSimilarityTests
    {
        [Fact]
        public void GetJaroSimilarity_WithIdenticalStrings_ReturnsOne()
        {
            // Arrange
            const string str1 = "hello";
            const string str2 = "hello";

            // Act
            var result = str1.GetJaroSimilarity(str2);

            // Assert
            result.Should().Be(1.0);
        }

        [Fact]
        public void GetJaroSimilarity_WithCompletelyDifferentStrings_ReturnsZero()
        {
            // Arrange
            const string str1 = "abc";
            const string str2 = "xyz";

            // Act
            var result = str1.GetJaroSimilarity(str2);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void GetJaroSimilarity_WithEmptySecondString_ReturnsZero()
        {
            // Arrange
            const string str1 = "hello";
            const string str2 = "";

            // Act
            var result = str1.GetJaroSimilarity(str2);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void GetJaroSimilarity_WithNullSecondString_ReturnsZero()
        {
            // Arrange
            const string str1 = "hello";
            const string? str2 = null;

            // Act
            var result = str1.GetJaroSimilarity(str2!);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void GetJaroSimilarity_WithSimilarStrings_ReturnsExpectedValue()
        {
            // Arrange
            const string str1 = "martha";
            const string str2 = "marhta";

            // Act
            var result = str1.GetJaroSimilarity(str2);

            // Assert
            result.Should().BeApproximately(0.944, 0.001); // Known Jaro similarity for these strings
        }

        [Fact]
        public void GetJaroSimilarity_WithCaseInsensitiveComparison_WorksCorrectly()
        {
            // Arrange
            const string str1 = "Hello";
            const string str2 = "hello";

            // Act
            var result = str1.GetJaroSimilarity(str2);

            // Assert
            result.Should().Be(1.0); // Should be case-insensitive based on implementation
        }

        [Theory]
        [InlineData("dwayne", "duane", 0.822)]
        [InlineData("dixon", "dicksonx", 0.767)]
        [InlineData("jellyfish", "smellyfish", 0.896)]
        public void GetJaroSimilarity_WithKnownTestCases_ReturnsExpectedValues(string str1, string str2, double expected)
        {
            // Act
            var result = str1.GetJaroSimilarity(str2);

            // Assert
            result.Should().BeApproximately(expected, 0.01);
        }
    }

    public class GetDiceSimilarityTests
    {
        [Fact]
        public void GetDiceSimilarity_WithIdenticalStrings_ReturnsOne()
        {
            // Arrange
            const string str1 = "hello";
            const string str2 = "hello";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(1.0);
        }

        [Fact]
        public void GetDiceSimilarity_WithCompletelyDifferentStrings_ReturnsZero()
        {
            // Arrange
            const string str1 = "abc";
            const string str2 = "xyz";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void GetDiceSimilarity_WithEmptyStrings_ReturnsZero()
        {
            // Arrange
            const string str1 = "";
            const string str2 = "";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void GetDiceSimilarity_WithOneEmptyString_ReturnsZero()
        {
            // Arrange
            const string str1 = "hello";
            const string str2 = "";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(0.0);
        }

        [Fact]
        public void GetDiceSimilarity_WithSingleCharacterStrings_ReturnsZero()
        {
            // Arrange
            const string str1 = "a";
            const string str2 = "b";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(0.0); // No bigrams can be formed from single characters
        }

        [Fact]
        public void GetDiceSimilarity_WithTwoCharacterStrings_WorksCorrectly()
        {
            // Arrange
            const string str1 = "ab";
            const string str2 = "ab";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(1.0);
        }

        [Theory]
        [InlineData("night", "nacht", 0.25)] // "ni", "ig", "gh", "ht" vs "na", "ac", "ch", "ht" - only "ht" matches
        [InlineData("context", "contact", 0.615)] // More overlap
        [InlineData("hello", "hallo", 0.5)] // "he", "el", "ll", "lo" vs "ha", "al", "ll", "lo" - "ll", "lo" match
        public void GetDiceSimilarity_WithKnownTestCases_ReturnsExpectedValues(string str1, string str2, double expected)
        {
            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().BeApproximately(expected, 0.01);
        }

        [Fact]
        public void GetDiceSimilarity_WithRepeatedBigrams_HandlesCorrectly()
        {
            // Arrange
            const string str1 = "aaa";
            const string str2 = "aaa";

            // Act
            var result = str1.GetDiceSimilarity(str2);

            // Assert
            result.Should().Be(1.0); // Should handle repeated bigrams correctly
        }
    }
}