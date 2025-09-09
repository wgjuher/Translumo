# Translumo Testing Guidelines

This document outlines the testing strategy, guidelines, and best practices for the Translumo project.

## Overview

The Translumo project now includes comprehensive unit and integration tests across all modules. The testing infrastructure is built using:

- **xUnit** - Primary testing framework
- **FluentAssertions** - Assertion library for readable tests
- **Moq** - Mocking framework for dependencies
- **Coverlet** - Code coverage collection
- **WireMock.Net** - HTTP API mocking for translation services

## Test Project Structure

```
tests/
├── Translumo.Utils.Tests/              # Utility function tests
├── Translumo.Infrastructure.Tests/     # Core infrastructure tests
├── Translumo.Processing.Tests/         # Processing pipeline tests
├── Translumo.OCR.Tests/                # OCR engine tests
├── Translumo.Translation.Tests/        # Translation service tests
├── Translumo.TTS.Tests/                # Text-to-speech tests
└── Translumo.Integration.Tests/        # End-to-end integration tests
```

## Test Categories

### Unit Tests
Test individual components in isolation with mocked dependencies.

**Examples:**
- String similarity algorithms ([`StringExtensionsTests.cs`](../tests/Translumo.Utils.Tests/Extensions/StringExtensionsTests.cs))
- HTTP utility functions ([`HttpHelperTests.cs`](../tests/Translumo.Utils.Tests/Http/HttpHelperTests.cs))
- Encryption services ([`AesEncryptionServiceTests.cs`](../tests/Translumo.Infrastructure.Tests/Encryption/AesEncryptionServiceTests.cs))
- Collection behaviors ([`LimitedDictionaryTests.cs`](../tests/Translumo.Infrastructure.Tests/Collections/LimitedDictionaryTests.cs))

### Integration Tests
Test component interactions and workflows with real or realistic dependencies.

**Examples:**
- Factory pattern implementations
- Service dependency injection
- Multi-component workflows

### Mock-Heavy Tests
Test components that depend on external services or resources.

**Examples:**
- Translation API calls
- OCR engine interactions
- File system operations

## Running Tests

### Local Development

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings

# Run specific test project
dotnet test tests/Translumo.Utils.Tests/

# Run tests in watch mode
dotnet watch test tests/Translumo.Utils.Tests/
```

### GitHub Actions

Tests run automatically on:
- Push to `main`, `master`, `develop` branches
- Pull requests to these branches
- Manual workflow dispatch

The CI pipeline:
1. Builds the solution in Debug and Release configurations
2. Runs all test projects
3. Collects code coverage
4. Uploads test results and coverage reports

## Writing Tests

### Naming Conventions

```csharp
public class ClassNameTests
{
    public class MethodNameTests
    {
        [Fact]
        public void MethodName_WithSpecificCondition_ExpectedBehavior()
        {
            // Test implementation
        }
    }
}
```

### Test Structure (AAA Pattern)

```csharp
[Fact]
public void Method_Condition_ExpectedResult()
{
    // Arrange
    var input = "test input";
    var expected = "expected output";
    
    // Act
    var result = methodUnderTest(input);
    
    // Assert
    result.Should().Be(expected);
}
```

### Mocking Guidelines

```csharp
// Create mocks for dependencies
private readonly Mock<ILogger<ServiceClass>> _mockLogger;
private readonly Mock<IDependency> _mockDependency;

public TestClass()
{
    _mockLogger = new Mock<ILogger<ServiceClass>>();
    _mockDependency = new Mock<IDependency>();
}

[Fact]
public void TestMethod()
{
    // Setup mock behavior
    _mockDependency
        .Setup(x => x.Method(It.IsAny<string>()))
        .Returns("mocked result");
    
    // Verify mock was called
    _mockDependency.Verify(x => x.Method("expected input"), Times.Once);
}
```

## Test Coverage Goals

### Current Coverage Areas

✅ **High Priority (Implemented)**
- String similarity algorithms (Jaro, Dice)
- HTTP utilities (form data, query strings)
- Encryption services (AES encryption/decryption)
- Limited dictionary collection behavior
- Frame comparison service logic
- Factory pattern implementations

✅ **Medium Priority (Basic Implementation)**
- OCR engine factory
- Translation service factory
- TTS engine factory
- Integration test framework

### Coverage Targets

- **Utils Module**: 90%+ (critical algorithms)
- **Infrastructure Module**: 85%+ (core services)
- **Processing Module**: 80%+ (complex logic)
- **Translation Module**: 75%+ (API integrations)
- **OCR Module**: 70%+ (external dependencies)
- **TTS Module**: 70%+ (platform-specific)

## Testing Strategies by Module

### Translumo.Utils
- **Focus**: Pure functions, algorithms, extensions
- **Strategy**: Comprehensive unit tests with edge cases
- **Key Areas**: String similarity, HTTP helpers, extensions

### Translumo.Infrastructure
- **Focus**: Core services, collections, encryption
- **Strategy**: Unit tests with mocked dependencies
- **Key Areas**: Caching, security, language services

### Translumo.Processing
- **Focus**: Image processing, ML predictions, workflows
- **Strategy**: Unit tests with mock data, integration tests
- **Key Areas**: Frame comparison, text validation, processing pipeline

### Translumo.OCR
- **Focus**: Engine management, configuration
- **Strategy**: Factory tests, mock engines for external dependencies
- **Key Areas**: Engine selection, lifecycle management

### Translumo.Translation
- **Focus**: Service integration, API calls
- **Strategy**: Mock HTTP responses, factory pattern tests
- **Key Areas**: Service selection, request/response handling

### Translumo.TTS
- **Focus**: Engine creation, platform integration
- **Strategy**: Factory tests, mock platform dependencies
- **Key Areas**: Engine selection, language support

## Best Practices

### Test Organization
- Group related tests in nested classes
- Use descriptive test names that explain the scenario
- Keep tests focused on single behaviors
- Use `Theory` for parameterized tests

### Assertions
- Use FluentAssertions for readable assertions
- Test both positive and negative cases
- Include edge cases and boundary conditions
- Verify exception scenarios

### Mocking
- Mock external dependencies (APIs, file system, databases)
- Don't mock the system under test
- Use strict mocks when behavior verification is important
- Reset mocks between tests if sharing instances

### Test Data
- Use realistic test data
- Create helper methods for common test objects
- Consider using builders for complex objects
- Keep test data minimal but representative

## Continuous Integration

### GitHub Actions Workflow
The [`build.yml`](../.github/workflows/build.yml) workflow:
- Runs on Windows (required for Windows-specific features)
- Tests both Debug and Release configurations
- Collects code coverage using Coverlet
- Uploads test results and coverage reports
- Fails the build if tests fail

### Coverage Reporting
- Uses Coverlet for cross-platform coverage collection
- Generates multiple formats: OpenCover, Cobertura, JSON, LCOV
- Excludes test projects and generated code
- Configured via [`coverlet.runsettings`](../coverlet.runsettings)

## Troubleshooting

### Common Issues

**Tests not discovered:**
- Ensure test projects follow `*.Tests.csproj` naming convention
- Verify xUnit packages are properly referenced
- Check that test methods are public and have `[Fact]` or `[Theory]` attributes

**Mock setup issues:**
- Verify mock setup matches actual method signatures
- Use `It.IsAny<T>()` for flexible parameter matching
- Check that mocked methods are virtual or from interfaces

**Coverage not collected:**
- Ensure `coverlet.collector` package is referenced in test projects
- Verify `coverlet.runsettings` is in the solution root
- Check that the `--collect:"XPlat Code Coverage"` argument is used

**Windows-specific test failures:**
- Some tests may require Windows-specific features (OCR, TTS)
- Use conditional compilation or runtime checks for platform-specific code
- Mock platform-specific dependencies in cross-platform scenarios

## Future Enhancements

### Planned Improvements
- [ ] Performance benchmarking tests
- [ ] Load testing for processing pipeline
- [ ] UI automation tests for WPF components
- [ ] API contract tests for translation services
- [ ] Property-based testing for algorithms
- [ ] Mutation testing for test quality assessment

### Test Infrastructure
- [ ] Custom test attributes for categorization
- [ ] Shared test utilities library
- [ ] Test data builders and factories
- [ ] Custom assertions for domain-specific scenarios
- [ ] Automated test report generation

## Contributing

When adding new features:
1. Write tests first (TDD approach recommended)
2. Ensure new code has appropriate test coverage
3. Update this documentation if adding new test patterns
4. Run tests locally before submitting PRs
5. Include test scenarios in PR descriptions

When modifying existing code:
1. Update corresponding tests
2. Ensure all tests still pass
3. Add tests for new edge cases or scenarios
4. Maintain or improve coverage percentages

---

For questions about testing practices or issues with the test suite, please refer to the project's issue tracker or contact the development team.