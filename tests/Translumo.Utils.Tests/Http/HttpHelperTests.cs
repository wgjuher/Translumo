using FluentAssertions;
using Translumo.Utils.Http;
using Xunit;

namespace Translumo.Utils.Tests.Http;

public class HttpHelperTests
{
    public class TestEntity
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public string? NullProperty { get; set; }
    }

    public class EmptyEntity
    {
    }

    public class BuildFormDataTests
    {
        [Fact]
        public void BuildFormData_WithValidEntity_ReturnsCorrectFormData()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "John Doe",
                Age = 30,
                Email = "john@example.com",
                IsActive = true,
                NullProperty = null
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().Contain("name=John%20Doe");
            result.Should().Contain("age=30");
            result.Should().Contain("email=john%40example.com");
            result.Should().Contain("isActive=True");
            result.Should().NotContain("nullProperty");
        }

        [Fact]
        public void BuildFormData_WithEmptyEntity_ReturnsEmptyString()
        {
            // Arrange
            var entity = new EmptyEntity();

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void BuildFormData_WithAllNullProperties_ReturnsEmptyString()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = null,
                Age = 0,
                Email = null,
                IsActive = false,
                NullProperty = null
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().Contain("age=0");
            result.Should().Contain("isActive=False");
            result.Should().NotContain("name=");
            result.Should().NotContain("email=");
            result.Should().NotContain("nullProperty=");
        }

        [Fact]
        public void BuildFormData_WithSpecialCharacters_EncodesCorrectly()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "John & Jane",
                Email = "test+email@domain.com",
                Age = 25,
                IsActive = true
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().Contain("name=John%20%26%20Jane");
            result.Should().Contain("email=test%2Bemail%40domain.com");
        }

        [Fact]
        public void BuildFormData_PropertyNameCasing_ConvertsToLowerCamelCase()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "Test",
                Age = 30
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().Contain("name=Test");
            result.Should().Contain("age=30");
            result.Should().NotContain("Name=");
            result.Should().NotContain("Age=");
        }

        [Fact]
        public void BuildFormData_WithUnicodeCharacters_EncodesCorrectly()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "José María",
                Email = "josé@example.com",
                Age = 35
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().Contain("name=Jos%C3%A9%20Mar%C3%ADa");
            result.Should().Contain("email=jos%C3%A9%40example.com");
        }

        [Fact]
        public void BuildFormData_DoesNotEndWithAmpersand()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "Test",
                Age = 30
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().NotEndWith("&");
        }
    }

    public class BuildQueryStringTests
    {
        [Fact]
        public void BuildQueryString_WithValidEntity_ReturnsCorrectQueryString()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "John Doe",
                Age = 30,
                Email = "john@example.com",
                IsActive = true
            };

            // Act
            var result = HttpHelper.BuildQueryString(entity);

            // Assert
            result.Should().StartWith("?");
            result.Should().Contain("name=John%20Doe");
            result.Should().Contain("age=30");
            result.Should().Contain("email=john%40example.com");
            result.Should().Contain("isActive=True");
        }

        [Fact]
        public void BuildQueryString_WithEmptyEntity_ReturnsQuestionMarkOnly()
        {
            // Arrange
            var entity = new EmptyEntity();

            // Act
            var result = HttpHelper.BuildQueryString(entity);

            // Assert
            result.Should().Be("?");
        }

        [Fact]
        public void BuildQueryString_WithNullProperties_ExcludesNullValues()
        {
            // Arrange
            var entity = new TestEntity
            {
                Name = "Test",
                Age = 25,
                Email = null,
                IsActive = false,
                NullProperty = null
            };

            // Act
            var result = HttpHelper.BuildQueryString(entity);

            // Assert
            result.Should().StartWith("?");
            result.Should().Contain("name=Test");
            result.Should().Contain("age=25");
            result.Should().Contain("isActive=False");
            result.Should().NotContain("email=");
            result.Should().NotContain("nullProperty=");
        }
    }

    public class PropertyNameConversionTests
    {
        public class CasingTestEntity
        {
            public string? FirstName { get; set; }
            public string? lastName { get; set; }
            public string? UPPERCASE { get; set; }
            public string? A { get; set; }
            public string? aB { get; set; }
        }

        [Fact]
        public void BuildFormData_PropertyNameCasing_HandlesVariousCases()
        {
            // Arrange
            var entity = new CasingTestEntity
            {
                FirstName = "John",
                lastName = "Doe",
                UPPERCASE = "TEST",
                A = "single",
                aB = "mixed"
            };

            // Act
            var result = HttpHelper.BuildFormData(entity);

            // Assert
            result.Should().Contain("firstName=John");
            result.Should().Contain("lastName=Doe"); // Already lowercase first letter
            result.Should().Contain("uPPERCASE=TEST");
            result.Should().Contain("a=single");
            result.Should().Contain("aB=mixed"); // Already lowercase first letter
        }
    }
}