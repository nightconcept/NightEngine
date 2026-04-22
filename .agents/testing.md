# Testing

## Overview

Tests use xUnit orchestration with custom base classes. Three test case types:

- **`GameTestCase`** — automated, runs inside the engine's game loop
- **`ManualTestCase`** — runs inside the game loop, requires user confirmation
- **`ModTestCase`** — isolated unit test, no game window needed

**Run automated tests (development default):**
```
mise test
```

The test runner (`scripts/run_tests.py`) automatically sets `SDL_VIDEODRIVER=dummy` and filters to `TestType=Automated` by default. Common flags:

| Flag | Purpose |
|---|---|
| `--headed` | Use real SDL video (no dummy driver) |
| `--all` | Include manual/skipped tests (no TestType filter) |
| `--filter EXPR` | Additional dotnet filter expression |
| `--group NAME` | Run a single test group by name fragment |
| `--find PATTERN` | List tests matching a substring, then exit |
| `--failures-only` | Re-run only previously failed tests |
| `--dry-run` | Print the command without executing |
| `--verbose` | Pass `--verbosity normal` to dotnet test |
| `--no-build` | Skip the build step |

Pass flags through mise: `mise test -- --group Timer`.

## Directory Structure

```
tests/
├── Core/                 # Base classes and interfaces
│   ├── BaseTestCase.cs
│   ├── GameTestCase.cs
│   ├── ManualTestCase.cs
│   ├── ModTestCase.cs
│   ├── ITestCase.cs
│   ├── TestGroup.cs
│   └── TestTypes.cs      # TestStatus, TestType enums
└── Groups/
    └── [ModuleName]/
        ├── [ModuleName]Group.cs
        ├── [Feature1]Tests.cs
        └── [Feature2]Tests.cs
```

## Naming Conventions

- **Group files:** `[ModuleName]Group.cs` (e.g., `FilesystemGroup.cs`)
- **Test case files:** `[FeatureName]Tests.cs` or `[SpecificName]Test.cs`
- **Group classes:** `[ModuleName]Group : TestGroup`
- **GameTestCase/ManualTestCase classes:** `[Module][Feature]_[Behavior]Test`
- **ModTestCase classes:** `[Module][Class]_[Method]Test`
- **Test `Name` property format:** `"Module.Feature.Behavior"` (e.g., `"Graphics.Clear"`)

## Creating Tests

### Step 1 — Choose the test type

- `ModTestCase`: logic testable without a window → prefer this
- `GameTestCase`: needs engine loop, graphics context, or input system
- `ManualTestCase`: requires human visual confirmation

### Step 2 — Create the test case class

**`GameTestCase` example:**
```csharp
public class MyModule_FeatureBehaviorTest : GameTestCase
{
    public override string Name => "MyModule.Feature.Behavior";
    public override string Description => "Tests that Feature does X.";

    protected override void Load()
    {
        base.Load(); // always call base
    }

    protected override void Update(double deltaTime)
    {
        if (this.IsDone) return;
        bool ok = Night.MyModule.Feature.DoSomething();
        this.CurrentStatus = ok ? TestStatus.Passed : TestStatus.Failed;
        this.Details = ok ? "DoSomething returned true." : "DoSomething returned false.";
        this.EndTest();
    }
}
```

**`ManualTestCase` example:**
```csharp
public class MyModule_VisualTest : ManualTestCase
{
    public override string Name => "MyModule.Visual";
    public override string Description => "User confirms visual output.";

    protected override void Update(double deltaTime)
    {
        if (this.IsDone) return;
        if (this.TestStopwatch.ElapsedMilliseconds > this.ManualTestPromptDelayMilliseconds)
            this.RequestManualConfirmation("Does the screen show X correctly?");
    }
}
```

**`ModTestCase` example:**
```csharp
public class MyModule_LogicTest : ModTestCase
{
    public override string Name => "MyModule.Logic";
    public override string Description => "Tests logic in isolation.";
    public override string SuccessMessage => "Logic tested successfully.";

    public override void Run()
    {
        var result = new MyClass().Method("input");
        Assert.Equal("expected", result);
    }
}
```

### Step 3 — Create or update the Test Group

```csharp
[Collection("SequentialTests")] // required for GameTestCase/ManualTestCase groups
public class MyModuleGroup : TestGroup
{
    public MyModuleGroup(ITestOutputHelper outputHelper) : base(outputHelper) { }

    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_MyModule_GameTests()
    {
        this.Run_GameTestCase(new MyModule_FeatureBehaviorTest());
    }

    [Fact]
    [Trait("TestType", "Automated")]
    public void Run_MyModule_ModTests()
    {
        this.Run_ModTestCase(new MyModule_LogicTest());
    }

    [Fact]
    [Trait("TestType", "Manual")]
    public void Run_MyModule_VisualTest()
    {
        this.Run_GameTestCase(new MyModule_VisualTest());
    }
}
```

## Key Base Classes

| Class | Purpose |
|---|---|
| `BaseTestCase` | Shared properties: `Name`, `Type`, `CurrentStatus`, `Details`, `TestStopwatch` |
| `GameTestCase` | Automated, `IGame` lifecycle: `Load`, `Update`, `Draw`. Provides `EndTest()`, `CheckCompletionAfterDuration()`, `CheckCompletionAfterFrames()` |
| `ManualTestCase` | Extends `GameTestCase`; provides `RequestManualConfirmation()`, pass/fail UI, timeout |
| `ModTestCase` | Isolated; override `Run()` with xUnit `Assert` calls; define `SuccessMessage` |
| `TestGroup` | xUnit class base; provides `Run_GameTestCase()` and `Run_ModTestCase()` |

## Lifecycle (GameTestCase/ManualTestCase)

- `Load()`: one-time setup. Always call `base.Load()`.
- `Update(double deltaTime)`: main logic. Guard with `if (this.IsDone) return;`
- `Draw()`: rendering. For `ManualTestCase`, base handles UI and `Present()`.
- On error: call `this.RecordFailure("message", exception)` — it calls `EndTest()`.
- Cleanup resources in `finally` block before `EndTest()`.

## Test Member Order (within test case classes)

1. Fields → Constructors → Finalizers → Delegates → Events → Enums → Interfaces → Properties → Indexers → Methods (Public → Internal → Protected → Private) → Structs → Nested Classes

## Best Practices

- Prefer `ModTestCase` for pure logic — faster, no window required
- Use `[Collection("SequentialTests")]` on groups with `GameTestCase` or `ManualTestCase`
- Automated tests must be idempotent
- Clean up resources (temp files, etc.) in a `finally` block
- Test both success and failure cases
- Reserve `ManualTestCase` for genuine visual or interaction verification

---

## macOS Constraints

Manual tests (those requiring a real graphics window) cannot run via `dotnet test` on macOS. This is an architectural limitation of the xUnit test host process, not a bug.

**Root cause:** `dotnet test` runs in a restricted sandbox without proper macOS entitlements for window creation. `dotnet run` works fine.

**What works:**
- All automated tests: `mise test` (headless, `SDL_VIDEODRIVER=dummy`)
- All automated tests (raw dotnet): `SDL_VIDEODRIVER=dummy dotnet test --filter TestType=Automated`
- Headless manual tests: `SDL_VIDEODRIVER=dummy dotnet test --filter TestType=Manual`
- Real graphics: `mise game` (modify SampleGame to test visuals)
- Find/list tests: `mise test -- --find Timer`
- Re-run failures: `mise test -- --failures-only`

**For CI/CD:** Use automated + headless manual only.

**macOS permissions required for headed mode:** Screen Recording permission for terminal/IDE in System Preferences → Security & Privacy.

**Test project config** (`tests/NightTest.csproj`) sets `OutputType=Exe` to get appHost context — this is intentional and must not be reverted.
