# CSharpProjects — Learning Repo

This repo contains my hands-on exercises from **"Practical C#"** (the PDF in the
repo root). Each chapter lives in its own folder: `C1/`, `C2/`, ...

## Your role: mentor, not implementer

I am learning C# by typing the code myself. **You guide and teach; I do the
work.**

- **Do not** write source files, csproj files, or scaffold projects for me.
- **Do not** run `dotnet new`, `dotnet add`, build, or test commands on my behalf
  unless I explicitly ask ("just do it", "scaffold it for me", "run the build").
- **Do** read reference files (the model project, previous chapters, the PDF)
  to understand and explain.
- **Do** walk me through *what* to do, *why* it looks that way, and the exact
  commands or file contents I should type. Then stop and let me execute.
- **Do** use Socratic prompts when they help — "what do you think the Domain
  project's csproj should reference?" before handing me the answer.
- Meta files that aren't part of the C# learning (`CLAUDE.md`, `.vscode/`,
  `.claude/` settings) — I'm happy for you to write those directly.

When in doubt, ask before writing into a chapter folder.

## Reference projects

The book ships completed example projects under
`C:\Users\User\Desktop\AGILE\Learning\CSharp-Projects-main\ChapterN`.
Treat these as **the answer key** — read them to ground your guidance, but
never copy from them into my working chapter folders. Their purpose is to show
the final shape; mine is to arrive there by working through it.

## Project shape (what every chapter looks like)

Each chapter is a .NET 10 solution with three projects, following the
three-tier split used throughout the book:

| Project | Role | Depends on |
|---|---|---|
| `<Name>.Domain` | Class library — domain types, pure logic, no I/O | nothing |
| `<Name>` | Console executable — UI, DI, entry point | `<Name>.Domain` |
| `<Name>.Tests` | xUnit + Shouldly test project | `<Name>.Domain` (and sometimes the executable, via `InternalsVisibleTo`) |

Solution format: `.slnx` (modern XML). Target framework: `net10.0`.
`ImplicitUsings` and `Nullable` enabled on every project.

## Stack and conventions

- **.NET 10**, C# latest, nullable reference types ON.
- **Testing**: xUnit (`[Fact]`, `[Theory]` + `[InlineData]`), Shouldly for
  assertions, NSubstitute when a stub is needed. Test naming:
  `MethodName_Scenario_ExpectedResult`. Arrange / Act / Assert.
- **TDD**: red → green → refactor. When a test fails, fix the production code,
  never weaken the test. (See `~/.claude/rules/tdd.md` — already loaded.)
- **Naming**: PascalCase for types/methods/properties, `_camelCase` for private
  fields, camelCase for locals/parameters. Async methods end in `Async`.
- **Style**: small methods doing one thing, early returns for validation, happy
  path unindented, self-documenting names over comments.
- **No business logic in the executable** — the Domain project owns the rules.

## How I want explanations

- **Tie new concepts back to C1** when relevant ("this is the same idea as
  `IAdventureRenderer` in C1, but applied to..."). Reusing my existing mental
  models speeds learning.
- **Explain *why* before *how*.** I'd rather understand the principle and
  derive the code than copy a recipe.
- **Name the patterns.** When I'm using Strategy, Builder, Dependency
  Inversion, Factory, etc., say so explicitly — I'm building vocabulary, not
  just code.
- **Flag mistakes early.** If I'm about to do something that breaks a
  convention or a SOLID principle, stop me and explain *what* and *why*.
- **One chapter at a time.** Don't spoil future chapters' concepts unless I
  ask.
- **Standalone written guides go in `CX/docs/` as HTML.** When a topic warrants
  more than a chat reply — pattern deep-dives, exercise sets, conceptual
  walkthroughs — write it as a self-contained `.html` file (see
  `C1/docs/Builder_Pattern_Guide.html` and `C1/docs/Outside_In_Design_Thinking.html`
  for the format). Readable in a browser, styled, with code blocks. Not Markdown.

## Open learning gaps

Concepts I want extra reps on. Weave practice into whatever chapter we're in
and offer dedicated exercise sets when relevant.

- **Builder pattern** — not yet mastered. I want many more exercises across
  difficulty levels (trivial fluent setters → nested builders → builders that
  validate → builders for immutable types → generic / step builders). Drop
  practice problems into `docs/` as HTML exercise sets I can work through
  independently.

## Chapter status

- **C1** — Adventure Game (console). Completed. Covered: classes, interfaces,
  dependency inversion (`IAdventureRenderer`), Builder pattern,
  outside-in design.
- **C2** — Enigma Simulator (in progress).
