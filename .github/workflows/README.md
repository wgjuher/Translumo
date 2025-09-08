# GitHub Actions Build Workflow Documentation

## Overview

This repository uses a comprehensive GitHub Actions workflow for building, testing, and releasing the Translumo application. The workflow is designed to handle the Windows-specific requirements and skip external binary extraction during CI/CD builds.

## Workflow Files

- **[`build.yml`](build.yml)** - Main build and release workflow
- **[`PR.check.yml`](PR.check.yml)** - Pull request validation workflow (existing)

## Key Features

### 🚫 **Binary Extraction Prevention**
The workflow uses `SkipBinariesExtract: true` to prevent execution of [`binaries_extract.bat`](../../binaries_extract.bat) which would normally download:
- Python 3.9 runtime (~50MB)
- EasyOCR models (~100MB)
- Tesseract language data (~50MB)
- ML prediction models (~20MB)

### 🔄 **Trigger Conditions**

| Trigger | Branches | Action |
|---------|----------|--------|
| **Push** | `main`, `master`, `develop` | Build validation |
| **Pull Request** | `main`, `master`, `develop` | Build validation |
| **Tag Push** | `v*` (e.g., `v1.0.0`) | Full build + GitHub release |
| **Manual** | Any branch | Build validation |

### 🏗️ **Build Matrix**

The workflow runs parallel builds for:
- **Debug Configuration**: Fast validation, no artifacts
- **Release Configuration**: Optimized build with single-file executable

### 📦 **Artifacts**

#### Build Artifacts (Release only)
- **Complete Application Directory Structure**:
  - `Translumo.exe` - Main application executable
  - `Translumo.dll` - Application libraries
  - `*.dll` - All required dependencies
  - `Translumo.pdb` - Debug symbols for troubleshooting
  - `models/` - Directory for OCR and ML models (created empty, populated at runtime)
    - `easyocr/` - EasyOCR models directory
    - `tessdata/` - Tesseract language data directory
    - `prediction/` - ML prediction models directory
  - `python/` - Python runtime directory (populated at runtime)
  - `logs/` - Application logs directory
  - `README.txt` - Installation and usage instructions
- **Retention**: 30 days

#### Test Results
- `TestResults/` - Test execution results (if tests exist)
- **Retention**: 7 days

#### Release Packages (Tags only)
- `Translumo-{version}-win10-x64.zip` - Complete application package with directory structure
- Contains ready-to-run application with all necessary folders
- Automatically attached to GitHub releases

## Usage Examples

### Development Workflow

```bash
# Regular development - triggers build validation
git checkout develop
git add .
git commit -m "Add new feature"
git push origin develop
```

### Pull Request Workflow

```bash
# Create feature branch
git checkout -b feature/new-translation-engine
git add .
git commit -m "Implement new translation engine"
git push origin feature/new-translation-engine

# Create PR to develop/main - triggers build validation
```

### Release Workflow

```bash
# Create and push version tag - triggers full release
git checkout main
git tag v1.2.0
git push origin v1.2.0

# This automatically:
# 1. Builds Release configuration
# 2. Creates single-file executable
# 3. Packages into ZIP file
# 4. Creates GitHub release with release notes
# 5. Attaches ZIP file to release
```

## Environment Variables

The workflow uses these environment variables:

```yaml
env:
  SOLUTION_NAME: Translumo.sln
  MAIN_PROJECT: src/Translumo/Translumo.csproj
  SKIP_BINARIES_EXTRACT: true
  SkipBinariesExtract: true  # Prevents PreBuild target execution
```

## Build Process

### 1. **Setup Phase**
- Checkout repository with full history
- Setup .NET 7 SDK
- Cache NuGet packages for faster builds

### 2. **Build Phase**
- Restore NuGet dependencies
- Build solution with `SkipBinariesExtract=true`
- Skip external binary downloads

### 3. **Test Phase**
- Automatically detect test projects
- Run tests if found, skip gracefully if none exist
- Upload test results as artifacts

### 4. **Publish Phase** (Release builds only)
- Create self-contained single-file executable
- Target: `win10-x64` runtime
- Include all content for self-extraction

### 5. **Release Phase** (Tags only)
- Download build artifacts
- Create versioned ZIP package
- Generate release notes with system requirements
- Create GitHub release with attachments

## System Requirements

The built application requires:
- **OS**: Windows 10 build 19041 (20H1) / Windows 11
- **Runtime**: Self-contained (no .NET installation required)
- **DirectX**: DirectX 11 for screen capture
- **Memory**: 8 GB RAM (for EasyOCR mode)
- **Storage**: 5 GB free space (for EasyOCR mode)

## Troubleshooting

### Build Failures

1. **"PreBuild target failed"**
   - Ensure `SkipBinariesExtract: true` is set in environment
   - Check that `/p:SkipBinariesExtract=true` is passed to build commands

2. **"NuGet restore failed"**
   - Check internet connectivity on runner
   - Verify package sources are accessible

3. **"Publish failed"**
   - Ensure target framework `net7.0-windows10.0.19041.0` is available
   - Check runtime identifier `win10-x64` is valid

### Release Issues

1. **"Release creation failed"**
   - Ensure tag follows `v*` pattern (e.g., `v1.0.0`)
   - Check repository permissions for release creation

2. **"Artifact upload failed"**
   - Verify build artifacts were created successfully
   - Check artifact size limits (GitHub has 2GB limit per artifact)

## Security

- Uses `GITHUB_TOKEN` (automatically provided by GitHub)
- No external secrets required
- Windows-only builds prevent cross-platform security issues
- Artifacts are automatically cleaned up after retention period

## Performance Optimizations

- **NuGet Caching**: Reduces dependency download time
- **Matrix Builds**: Parallel execution of Debug/Release
- **Conditional Steps**: Skip unnecessary steps based on configuration
- **Artifact Cleanup**: Automatic removal of old artifacts to save storage

## Monitoring

Monitor workflow execution via:
- **GitHub Actions tab** in repository
- **Email notifications** for failed builds (if configured)
- **Status badges** in README (can be added)

## Future Enhancements

Potential improvements:
- Add code coverage reporting
- Implement security scanning
- Add performance benchmarking
- Create deployment workflows for different environments