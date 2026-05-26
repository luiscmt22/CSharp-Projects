# GeneralExercises

Drill projects for blank-page practice. See `INDEX.html` for the curriculum.

---

## `dotnet new` cheat sheet

The three project types you'll create over and over. **Always pass `-f net10.0`** so you pin the framework and don't get bitten by template defaults.

### Class library (`Domain`, helpers, anything without an entry point)

```bash
dotnet new classlib -n MyLib -f net10.0
```

Produces a `.csproj` with `<TargetFramework>net10.0</TargetFramework>`, nullable + implicit usings enabled. No `Program.cs` — it's a library.

### Console app (executable; the thing with `Main`)

```bash
dotnet new console -n MyApp -f net10.0
```

Produces a `Program.cs` with `Console.WriteLine("Hello, World!");`. Modern templates use top-level statements — no explicit `Main` method needed.

### xUnit test project

```bash
dotnet new xunit -n MyApp.Tests -f net10.0
```

Comes with xUnit, the test SDK, and a sample test file. Add Shouldly separately:

```bash
dotnet add MyApp.Tests/MyApp.Tests.csproj package Shouldly
```

---

## Wiring projects together

### Add a project reference (one project uses another)

```bash
# Tests references the lib it's testing
dotnet add MyApp.Tests/MyApp.Tests.csproj reference MyLib/MyLib.csproj

# Console app references the domain lib
dotnet add MyApp/MyApp.csproj reference MyLib/MyLib.csproj
```

### Create a solution and add projects

```bash
dotnet new sln -n MySolution --format slnx
dotnet sln add MyLib/MyLib.csproj MyApp/MyApp.csproj MyApp.Tests/MyApp.Tests.csproj
```

### Make internals visible to tests (for testing internal types)

Add this `ItemGroup` inside the library's `.csproj`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="MyApp.Tests" />
</ItemGroup>
```

---

## A full Chapter-style scaffold (one block, copy-paste-friendly)

If you're setting up a chapter that has the standard three projects (Domain + Console + Tests):

```bash
dotnet new sln -n CX --format slnx

dotnet new classlib -n MyApp.Domain -f net10.0
dotnet new console  -n MyApp        -f net10.0
dotnet new xunit    -n MyApp.Tests  -f net10.0

dotnet add MyApp/MyApp.csproj             reference MyApp.Domain/MyApp.Domain.csproj
dotnet add MyApp.Tests/MyApp.Tests.csproj reference MyApp.Domain/MyApp.Domain.csproj
dotnet add MyApp.Tests/MyApp.Tests.csproj package Shouldly

dotnet sln add MyApp.Domain/MyApp.Domain.csproj MyApp/MyApp.csproj MyApp.Tests/MyApp.Tests.csproj

dotnet build
```

Then add to `MyApp/MyApp.csproj` whatever NuGet packages that app needs (`Spectre.Console`, `Microsoft.Extensions.DependencyInjection`, etc.):

```bash
dotnet add MyApp/MyApp.csproj package Spectre.Console
```

---

## Drills project setup (this folder specifically)

```bash
# From CSharpProjects/GeneralExercises/
dotnet new console -n Drills       -f net10.0
dotnet new xunit   -n Drills.Tests -f net10.0
dotnet add Drills.Tests/Drills.Tests.csproj reference Drills/Drills.csproj
dotnet add Drills.Tests/Drills.Tests.csproj package Shouldly

dotnet new sln -n GeneralExercises --format slnx
dotnet sln add Drills/Drills.csproj Drills.Tests/Drills.Tests.csproj
```

---

## Working pattern (drills)

- One file per drill: `Drill_01_LastElement.cs`, `Drill_02_EvensOnly.cs`, ...
- Each file: one `static class` with one method matching the drill's signature.
- `Program.cs` calls whichever drill you're testing today. Swap as you go.
- Stretch tests live in `Drills.Tests/` mirroring the drill name: `Drill_01_LastElementTests.cs`.

## The rules (re-read every session)

- No AI during the drill. Disable Copilot completions.
- Only `learn.microsoft.com` allowed as a reference.
- 10-minute timer. When it rings, write down the question you'd ask before opening the hint.
- Type, don't paste — even your own previous code.
- Ask for *review*, never solutions. Reviews are how I help without breaking the rep.
