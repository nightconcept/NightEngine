# macOS Manual Testing Constraints and Solutions

## Overview

This document explains the constraints and solutions for running manual tests on macOS, including the architectural reasons behind these limitations and practical workarounds for developers.

## Problem Summary

Manual tests that require graphics display fail when run through `dotnet test` on macOS, even with proper system permissions. This is due to fundamental differences between how xUnit test runners and regular applications access the macOS graphics subsystem.

## Root Cause Analysis

### The Core Issue

The problem is **not** with SDL3, permissions, or our test code, but with the **execution context** differences:

- **`dotnet test` (xUnit)**: Runs in a restricted test host process that cannot access macOS graphics
- **`dotnet run` (Game)**: Runs as a proper macOS application with full graphics access

### Technical Details

1. **xUnit Test Host Process**: Limited sandbox environment without proper macOS entitlements
2. **AppHost vs Library Context**: Test projects with `OutputType=Library` run in `dotnet` process context, while `OutputType=Exe` projects get their own appHost context
3. **macOS Graphics Access**: Requires proper app entitlements and process context that xUnit test host doesn't provide

## Solutions Implemented

### 1. Fixed Test Project Configuration

**Problem**: Multiple entry point conflicts preventing test compilation.

**Solution**: Changed test project to executable format while maintaining xUnit compatibility:

```xml
<!-- tests/NightTest.csproj -->
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <StartupObject>Program</StartupObject>
</PropertyGroup>
```

**Benefits**:
- Resolves compilation errors
- Enables appHost context for better macOS compatibility
- Maintains full xUnit test runner compatibility

### 2. Enhanced SDL Initialization

**Problem**: SDL3 initialization failures on macOS in different environments.

**Solution**: Added intelligent fallback logic in `Framework.cs`:

```csharp
// Special handling for macOS headed mode when video init fails
if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && !isHeadlessEnv)
{
    Logger.Info("macOS headed mode video init failed. Attempting workarounds...");

    // Clean up and try with different configuration
    SDL.Quit();
    _ = SDL.SetHint(SDL.Hints.VideoDriver, string.Empty);
    _ = SDL.SetHint(SDL.Hints.MacBackgroundApp, "0");

    // Retry with auto-detection
    if (!SDL.Init(initializedSubsystemsFlags))
    {
        // Provide helpful error guidance
        Logger.Error("macOS manual testing requires:");
        Logger.Error("1. Screen Recording permission for your terminal/IDE");
        Logger.Error("2. Running from a GUI application with proper entitlements");
        Logger.Error("3. Consider running: SDL_VIDEODRIVER=dummy mise man-test (headless)");
        return;
    }
}
```

**Benefits**:
- Graceful handling of macOS video initialization failures
- Clear error messages with actionable guidance
- Automatic fallback attempts for different configurations

## Current Status

### ✅ What Works
- **Automated tests**: All 79 automated tests pass on macOS
- **Headless manual tests**: `SDL_VIDEODRIVER=dummy mise man-test` works for logic testing
- **Game execution**: `mise game` shows real graphics window perfectly
- **Test compilation**: No more multiple entry point errors

### ❌ Current Constraint
- **xUnit manual tests with graphics**: Cannot show real windows due to xUnit test host limitations

## Workarounds for Manual Testing

### Option 1: Use SampleGame for Manual Testing ⭐ (Recommended)

Since `mise game` works perfectly with graphics, modify your game to run manual tests:

```bash
# Shows real graphics window - perfect for manual testing
mise game
```

**Benefits**:
- Full graphics display capability
- Real user interaction
- Same execution context as production
- No xUnit limitations

### Option 2: Headless Manual Testing

For logic verification without visual confirmation:

```bash
# Tests the logic flow without graphics
SDL_VIDEODRIVER=dummy mise man-test
```

**Benefits**:
- Validates test logic and flow
- Works in CI environments
- Fast execution
- No graphics dependencies

### Option 3: Create Standalone Manual Test Runner

Create a dedicated manual test application:

```csharp
// ManualTestRunner/Program.cs
using Night;
using NightTest.Groups.Graphics;

var test = new GraphicsClearTest();
Framework.Run(test);
```

```bash
# Run specific manual tests directly
dotnet run --project ManualTestRunner -- Graphics.Clear
```

**Benefits**:
- Full control over execution environment
- Direct SDL access like SampleGame
- Can be integrated into development workflow

## Development Recommendations

### For CI/CD Pipelines
- Use automated tests only: `mise test`
- Include headless manual tests for logic validation: `SDL_VIDEODRIVER=dummy dotnet test --filter TestType=Manual`

### For Local Development
- **Quick graphics verification**: Use `mise game` with test modifications
- **Logic testing**: Use headless manual tests with dummy driver
- **Production validation**: Create standalone test runner for critical manual tests

### For Manual Test Design
1. **Design for headless**: Ensure manual tests can validate logic without requiring visual confirmation
2. **Clear success criteria**: Make test outcomes programmatically verifiable when possible
3. **Fallback modes**: Provide alternative validation methods for CI environments

## Technical Notes

### macOS Permissions Required
Even with these solutions, running any graphics application on macOS requires:

1. **Screen Recording permission** for your terminal/IDE in System Preferences → Security & Privacy
2. **Proper app entitlements** (automatically handled by `dotnet run` but not `dotnet test`)
3. **GUI application context** (not available in xUnit test host)

### Alternative Approaches Considered

1. **Disable top-level statements**: Not applicable - this is about execution context, not compilation
2. **Change project SDK**: Would break xUnit integration
3. **Custom test runners**: Adds complexity without solving the fundamental issue
4. **Mock graphics subsystem**: Defeats the purpose of manual testing

## Conclusion

The macOS manual testing constraint is an **architectural limitation** of xUnit test runners, not a bug in our code. The implemented solutions provide:

1. **Robust automated testing** that works across all platforms
2. **Flexible manual testing options** for different development scenarios
3. **Clear guidance** for developers encountering this constraint

This approach maintains the benefits of both xUnit integration for automated tests and native application capabilities for manual graphics testing.

## Related Files

- `tests/NightTest.csproj` - Test project configuration
- `tests/Program.cs` - Test executable entry point
- `src/Night/Framework.cs` - SDL initialization with macOS handling
- `mise.toml` - Task definitions for different testing modes
