# Design — Chapter 21: Adapter — Your Engine, Their Models

Designed against the ACTUAL repo at tag `bookit-ch20` (commit `1e81147`, 26 tests green, verified
with `dotnet test` during design). Structure, box anatomy, voice and code standard follow the
Chapter 20 exemplar (`book-demo/chapter-strategy.md`).

**Repo facts this design is built on (read, not assumed):**

- Solution file is `BookIt.slnx` (XML solution format), not `.sln`. All CLI commands below use it.
- `src/BookIt` (console, net10.0, `Microsoft.Extensions.Hosting` 10.0.9) + `tests/BookIt.Tests`
  (xunit 2.9.3). Test census: ConflictCheckerTests 12, RouterTests 3, SmokeTests 3,
  MeetingRoom 2, Equipment 2, Parking 4 = 26.
- **There is no `.gitignore` anywhere in history, and 198 `bin/`/`obj/` artifacts are tracked.**
  The Ch. 20 manuscript prescribes `dotnet new gitignore`; the executed repo drifted. Chapter 21's
  proofs are diffs; this must be repaired first (see M0 and the Deviations section).
- `Conflict` messages currently embed `booking.Id` (`"Overlaps booking #1 …"`). No test pins any
  message beyond `Contains("Wednesday")` and the exception's `Contains("HotDesk")`/`("Sauna")` —
  so messages can be reworded during extraction without touching behavior any test observes.
  This freedom is load-bearing for the identity teaching beat (below).
- `ConflictCheckerRouterTests` contains an inline test double (`RecordingStrategy`) that
  implements `IConflictStrategy` against the current concrete-record signature. It is the ONE
  pre-existing test that needs more than a `using` edit in M3 — counted below.

**Installment skeleton** (mirrors Ch. 20's): cold open (the second client) → state check + M0 →
M1 (both dead ends, felt) → M2 (tally, red run, back-out) → "The Pattern, Named" interlude →
M3 (extraction) → M4 (the real adapter) → M5 (proof, demo, tag) → In the Wild → Boundaries →
checklist → drills → Journeying On (to Ch. 22's front-desk rulebook).

---

## Milestones

### M0 — State check and repo hygiene (pre-milestone, ~5 min)

Not a numbered milestone; a "Before the clinic walks in" section with a state-check box, a shape
future installments will reuse. Reader verifies: `git tag` shows `bookit-ch20`, `dotnet test`
shows 26 green. Then the repair — narrated in three sentences as the kind of debt a second
client exposes ("the first thing a new host does to your repo is read its history; make the
history readable"):

```bash
dotnet new gitignore
git rm -r --cached src/BookIt/bin src/BookIt/obj tests/BookIt.Tests/bin tests/BookIt.Tests/obj
git commit -m "ch21-M0: evict build artifacts; every proof in this chapter is a diff"
```

- **Files added/changed:** `.gitignore` added; 198 tracked artifacts deleted from the index
  (working tree untouched).
- **Checkpoint (non-xUnit):** `git status --short` is clean after a `dotnet build`;
  `git ls-files | grep -cE 'bin/|obj/'` prints `0`.
- **No your-turn box, no tests.** This is hygiene, and the chapter says so — one paragraph.

### M1 — The clinic arrives; walk both dead-end roads (~18 min)

**Commit:** `ch21-M1: ClinicIt on the mapping road — green, and wrong`

The second client: a private physiotherapy clinic rents therapy rooms by the hour, wants
BookIt's conflict engine, and will not adopt `BookIt.Domain` — their `Appointment` is
load-bearing (persistence, UI, letters to patients all hang off it), and the chapter says
plainly that they are right to refuse. Their models arrive as given, to be typed as-is:
`TherapyRoom(string Code, string Name, int TreatmentChairs)` and
`Appointment(Guid Id, string RoomCode, DateTime StartsAt, int DurationMinutes)`.
Note the shapes: no `End` (duration instead), `Guid` not `int`, `RoomCode` not `ResourceId`.

**Dead end (a) — performed, not narrated (4 min, uncommitted).** The reader literally runs
`cp -r src/BookIt/Conflicts src/ClinicIt-copy/` (or Explorer-drags it) and tries to imagine it
compiling: it can't — `namespace BookIt.Conflicts` leans on `BookIt.Domain`, so the copy must
smuggle `Models.cs` along, which means the clinic just adopted the exact model it refused, plus
a fork. Paper tally T1: *files copied: 6; files to fix twice next Tuesday: 6; foreign models
smuggled in: 1 (`Models.cs`, the seventh file).* Delete the folder. Two paragraphs, one
visceral moment — Chapter 20's copy-paste road at project scale, seen not told.

**Dead end (b) — shipped honestly.** Scaffold and take the reference road:

```bash
dotnet new console -n ClinicIt -o src/ClinicIt
dotnet new xunit -n ClinicIt.Tests -o tests/ClinicIt.Tests
dotnet sln BookIt.slnx add src/ClinicIt/ClinicIt.csproj tests/ClinicIt.Tests/ClinicIt.Tests.csproj
dotnet add tests/ClinicIt.Tests/ClinicIt.Tests.csproj reference src/ClinicIt/ClinicIt.csproj
dotnet add src/ClinicIt/ClinicIt.csproj reference src/BookIt/BookIt.csproj   # the smell, typed knowingly
```

Then `ClinicModelMapper` — a static conversion layer, every line locally plausible:

```csharp
// src/ClinicIt/ClinicModelMapper.cs  (transient — dies in M2)
public static class ClinicModelMapper
{
    public static Resource ToResource(TherapyRoom room) =>
        new(room.Code, ResourceTypes.MeetingRoom, Capacity: room.TreatmentChairs);

    public static Booking ToBooking(Appointment appointment) =>
        new(appointment.Id.GetHashCode(),          // a Guid does not fit in an int
            appointment.RoomCode,
            appointment.StartsAt,
            appointment.StartsAt.AddMinutes(appointment.DurationMinutes));
}
```

The seduction beat: three checkpoint tests go green in twenty minutes and the milestone
commits — the dead end must be *shipped*, not sketched, or M2 has nothing to hurt.

- **Files added/changed:** `src/ClinicIt/ClinicIt.csproj` (+ BookIt ProjectReference),
  `src/ClinicIt/Domain/ClinicModels.cs`, `src/ClinicIt/ClinicModelMapper.cs`,
  `src/ClinicIt/Program.cs` (template stub, untouched),
  `tests/ClinicIt.Tests/ClinicIt.Tests.csproj`, `tests/ClinicIt.Tests/ClinicMappingTests.cs`;
  `BookIt.slnx` gains two projects.
- **Checkpoint tests** (`ClinicMappingTests.cs`; checker hand-built:
  `new ConflictChecker([new MeetingRoomConflictStrategy()])`):
  1. `Overlapping_Appointments_In_The_Same_Room_Conflict_Through_The_Mapper` — the mapping road
     genuinely works for the happy path; the trap is baited with a passing suite.
  2. `Appointments_In_Different_Rooms_Are_Ignored_Through_The_Mapper` — the router's
     same-resource filter survives the conversion.
  3. `A_Ten_Minute_Gap_Conflicts_Because_Of_The_Borrowed_Cleaning_Buffer` — the 15-minute rule
     arrives, but note the test name says *cleaning* (the coworking word), not *sanitising*
     (the clinic's) — borrowed vocabulary foreshadows borrowed policy.
- **Your-turn box (notes):**
  - *Goal:* both dead ends with your own hands — copy the Conflicts folder and watch it demand
    the domain; then the ProjectReference + `ClinicModelMapper` road until the three checkpoint
    tests pass.
  - *Constraints:* the clinic's two records are typed exactly as given — you may not add `End`,
    change `Guid`, or rename `RoomCode`; the mapper is the only new logic; no interfaces, no
    new projects beyond the two hosts; keep tally T1 (copy road) and start T2 (every mapper
    line that invents or destroys information — count them as you type them).
  - *Order-if-you-freeze:* copy experiment first (it fails fast); scaffold commands verbatim;
    type the two clinic records; then write `ToBooking` before `ToResource` — the
    `StartsAt.AddMinutes` line is the whole reason the mapper feels necessary.
  - *Done when:* 26 + 3 tests green across two test projects; the copy-road folder is deleted;
    T1 written down; T2 has at least two entries (`GetHashCode`, `TreatmentChairs→Capacity`);
    committed.
- **Deliberate red run:** none in M1 — M1's job is the false green. (The chapter says this:
  "the most dangerous suite is a green one testing the wrong design.")

### M2 — Tuesday at the clinic: tally the damage, back it out (~12 min)

**Commit:** `ch21-M2: tally the damage, back out the mapping layer`

Two clinic requests arrive. One: the group-therapy room (14 treatment chairs) books with
20-minute gaps — the clinic sanitises any room in 15 minutes flat; chair count has nothing to
do with it. Two: when a slot is refused, the front desk must see *which appointment* blocks it,
to phone that patient.

**The scripted red run (the flagship of the chapter).** Reader types, into
`ClinicMappingTests.cs`:

`Group_Room_Twenty_Minute_Gap_Is_Allowed_Because_The_Clinic_Sanitises_In_Fifteen` —
group room ("TR-GROUP", 14 chairs), existing appointment 09:00 + 45 min, request 10:05–10:50
(a 20-minute gap). Expected `Assert.Empty(conflicts)`. **Run: RED.** Actual: one conflict,
reason text `"…once the 30-minute cleaning buffer is applied."` — the mapper's plausible
`Capacity: room.TreatmentChairs` line crossed BookIt's `EventRoomCapacityThreshold = 12`, and
the coworking space's event-room policy silently became clinic policy. The failure message
*names the leak*: a 30-minute buffer nobody at the clinic ever asked for. Borrowed models
import borrowed rules.

**The unwritable test (paper beat, no code).** The reader attempts the second requirement and
discovers it cannot even be typed: `Conflict.Existing` is a BookIt `Booking` whose `Id` is
`appointment.Id.GetHashCode()` — a one-way trip. There is no assert that recovers the `Guid`.
The test's *name* goes on paper — `The_Refusal_Identifies_The_Blocking_Appointment` — with a
note: "returns in M4." A test you can't write is an architecture verdict, not a testing
problem.

**The back-out.** Tally T2 read aloud (lossy id, leaked policy, invented `MaintenanceDay`,
an exe referencing a competitor's exe, clinic recompiles on every BookIt edit), then:

```bash
git rm -f src/ClinicIt/ClinicModelMapper.cs tests/ClinicIt.Tests/ClinicMappingTests.cs
dotnet remove src/ClinicIt/ClinicIt.csproj reference src/BookIt/BookIt.csproj
```

(`-f` is not decoration: the red test is an *uncommitted* edit to `ClinicMappingTests.cs`, and
plain `git rm` refuses files with local modifications. We mean the deletion.)

The clinic keeps its domain records (they were always theirs); everything that pointed at
BookIt goes. Commit green: BookIt 26, ClinicIt.Tests 0 tests (empty suite passes — one wry
sentence acknowledging it).

- **Files added/changed:** deletes `ClinicModelMapper.cs` + `ClinicMappingTests.cs`;
  `ClinicIt.csproj` loses the BookIt reference. (The red test is typed, run, and deleted with
  its file — its name survives on paper.)
- **Checkpoint tests:** the red one above (typed, never committed green):
  4. `Group_Room_Twenty_Minute_Gap_Is_Allowed_Because_The_Clinic_Sanitises_In_Fifteen` —
     proves policy leaks through borrowed models; **fails with**: expected empty, got 1
     conflict, reason containing `"30-minute cleaning buffer"`.
- **Your-turn box (notes):**
  - *Goal:* run the red, attempt the unwritable test, finish tally T2, back the mapping road
    out with git doing the remembering.
  - *Constraints:* do NOT fix the red by editing the mapper (`Capacity: 0` would "work" —
    write one sentence on why that's another invented value, not a fix); do not keep the
    mapper "just in case"; the clinic's records stay.
  - *Order-if-you-freeze:* type the red test verbatim; run; read the failure reason out loud;
    try to write the identity assert until it stalls (give it three minutes, not ten); then
    the two back-out commands.
  - *Done when:* T2 has ≥ 4 entries; the red run's failure text is copied into your notes;
    `git grep BookIt -- src/ClinicIt` returns nothing; both suites green (26 + 0); committed.
- **Deliberate red run:** yes — the milestone IS the red run (exact failure specified above).

**Interlude — "The Pattern, Named"** (between M2 and M3, as in Ch. 20): the Adapter pattern,
with the direction inversion stated as the working rule: **the feature defines the interfaces;
hosts adapt.** The mapping road failed because the *dependency pointed at a host's models*;
Adapter succeeds by making both hosts point at contracts the engine owns. Two costumes
previewed: sometimes the adapter is one line (`: IBookableResource` on a record you own);
sometimes it's a real class wrapping a model you must not touch. Both get built in the next
two milestones.

### M3 — Extract the engine: Scheduling.Conflicts owns its contracts (~23 min)

**Commit:** `ch21-M3: extract Scheduling.Conflicts — the engine owns its contracts`

New class library; the whole `Conflicts/` folder moves and is rewritten against engine-owned
interfaces; BookIt becomes the first adapted host — via the one-line record adapters.

```bash
dotnet new classlib -n Scheduling.Conflicts -o src/Scheduling.Conflicts
dotnet sln BookIt.slnx add src/Scheduling.Conflicts/Scheduling.Conflicts.csproj
dotnet add src/Scheduling.Conflicts/Scheduling.Conflicts.csproj package Microsoft.Extensions.DependencyInjection.Abstractions
dotnet add src/BookIt/BookIt.csproj reference src/Scheduling.Conflicts/Scheduling.Conflicts.csproj
git mv src/BookIt/Conflicts/*.cs src/Scheduling.Conflicts/   # then delete Class1.cs, rewrite namespaces
```

(The package reference is narrated in one sentence: zero references to any *host* is the law;
a framework abstractions package is not a host. The registration extension lives with the
feature — Ch. 20 already made `AddConflictDetection` the single composition point, and it
stays that, now shared by two hosts.)

What moves and mutates (full inventory — the design counts everything the contract requires):

- Engine files after M3: `Contracts.cs` (IBookableResource, IBookingRecord), `Models.cs`
  (BookingRequest, Conflict, ResourceTypes), `IConflictStrategy.cs`, the three strategy files,
  `ConflictChecker.cs`, `ConflictDetectionRegistration.cs` — all `namespace Scheduling.Conflicts`.
- `BookingRequest`, `Conflict`, `ResourceTypes` **leave `BookIt.Domain`** and become engine
  types (a request is a time window and a verdict is the engine's currency — neither is host
  business). `BookIt.Domain` keeps `Resource` and `Booking`, now implementing the interfaces —
  the one-line-adapter beat, verbatim in the manuscript:
  positional records satisfy interface getters for free.
- **Conflict messages reworded to drop `booking.Id`** (`"Overlaps booking #1 …"` →
  `"Overlaps an existing booking (09:00–10:00) …"`). Teaching beat, planted by M2's Guid pain:
  the engine's contract asks hosts only for what conflict *math* needs — a resource key and a
  time window. Identity is host business; `Conflict.Existing` hands the host back the very
  record it supplied, and the host recovers its own identity from it (M4 proves this).
  No test pins these messages (verified against the real suite).
- The scripted compile wall: run `dotnet build` right after `git mv` + namespace change and
  read the error list — it *is* the adaptation worksheet (Ch. 20's "let the compiler enumerate
  the fall-out", now at solution scale).
- Test-suite churn, counted exactly (contract requires the design doc to count it):
  all 26 tests survive **in place** (no relocation); 6 test files change their `using
  BookIt.Conflicts;`/`using BookIt.Domain;` lines to add `using Scheduling.Conflicts;`;
  **one** substantive edit — `RecordingStrategy` in `ConflictCheckerRouterTests` re-types its
  members to the new interface signature (`IBookableResource`, `IReadOnlyList<IBookingRecord>`,
  ~4 lines). Named in prose: *test doubles are adapters' neighbors — when a seam moves, the
  fakes standing on it move too.* Nothing else: `List<Booking>` still flows into
  `IReadOnlyList<IBookingRecord>` parameters because `IReadOnlyList<out T>` is covariant —
  one paragraph, because it looks like magic and is a language guarantee.
- `Program.cs` (BookIt): `using` line only.

- **Files added/changed:** adds `src/Scheduling.Conflicts/` (csproj + 8 .cs files, six of
  them `git mv`ed — `Contracts.cs` and `Models.cs` are the two new ones); changes `src/BookIt/BookIt.csproj`, `src/BookIt/Domain/Models.cs`,
  `src/BookIt/Program.cs`, 6 test files (usings), `ConflictCheckerRouterTests.cs`
  (RecordingStrategy), adds `tests/BookIt.Tests/EnginePurityTests.cs`; `BookIt.slnx`.
- **Checkpoint tests:**
  5. `Engine_Assembly_References_No_Host_Assembly` (new, `EnginePurityTests.cs`, in
     BookIt.Tests as a pragmatic home — noted as a designer call) — a permanent tripwire:
     `typeof(ConflictChecker).Assembly.GetReferencedAssemblies()` contains neither `BookIt`
     nor `ClinicIt`; the mechanical independence claim, encoded in the suite forever.
  - Non-xUnit checkpoint (contract-mandated mechanical check):
    `grep ProjectReference src/Scheduling.Conflicts/Scheduling.Conflicts.csproj` → no output;
    `git grep -nE "BookIt|ClinicIt" -- src/Scheduling.Conflicts/` → no output.
  - The refactor certificate, Ch. 20-style: `git diff ch21-M2..HEAD -- tests/BookIt.Tests/`
    read aloud — using-lines, RecordingStrategy, and the new purity file; nothing else.
- **Your-turn box (notes):**
  - *Goal:* engine library extracted; contracts owned by the engine; strategies + router +
    registration rewritten against `IBookableResource`/`IBookingRecord`; BookIt adapted with
    two `: interface` clauses; 27 green.
  - *Constraints:* MOVE files with `git mv` (history is the story); the router's public method
    shape survives (`FindConflicts(resource, request, allBookings)` — parameter *types* widen
    to the interfaces, names and order hold); no test relocation; the ONLY non-using test edit
    allowed is `RecordingStrategy`; message rewording must remove every `booking.Id` use in
    the engine — `git grep -n "\.Id" src/Scheduling.Conflicts/` afterwards shows only
    `resource.Id`.
  - *Order-if-you-freeze:* create the classlib and move files first; type `Contracts.cs` (the
    two interfaces are 12 lines and M2's tally dictates every member); build and let the
    errors list the rewrite sites; strategies before router; `: IBookableResource` /
    `: IBookingRecord` on the two BookIt records; usings last.
  - *Done when:* 27 green (26 survivors + purity test); both greps empty; the tests/ diff
    reads as counted above; committed.
- **Deliberate red run:** scripted red *build* (not a red test): the post-move `dotnet build`
  failure wall, read as a worksheet. The design flags it as a "red run" of the compile kind;
  the flagship red test run already happened in M2.

### M4 — The clinic adapter: their models, our engine (~16 min)

**Commit:** `ch21-M4: clinic adapters — their models, our engine`

`dotnet add src/ClinicIt/ClinicIt.csproj reference src/Scheduling.Conflicts/Scheduling.Conflicts.csproj`
— ClinicIt references the engine and nothing else. Two real adapter classes (the second
costume), then M2's two impossible tests come back and go green.

Type-first discipline: the reader types the six checkpoint tests before either adapter class
exists — **scripted red: the build fails with CS0246** (`AppointmentBookingAdapter` not found).
Then the adapters, then green. `TherapyRoomAdapter.Capacity => 0` gets the chapter's most
important comment: in M1 a borrowed model *silently decided* the clinic's buffer policy; here
the clinic *visibly decides* — same value a mapper might have picked, opposite epistemics.
`Type => ResourceTypes.MeetingRoom` gets the honesty paragraph: strategy keys name *rule
families*, not business nouns; the clinic buys the buffered-room rules, and the awkward name
is recorded as cheap naming debt (Boundaries revisits when a key rename earns its cost).

- **Files added/changed:** `src/ClinicIt/Adapters/TherapyRoomAdapter.cs`,
  `src/ClinicIt/Adapters/AppointmentBookingAdapter.cs`,
  `tests/ClinicIt.Tests/ClinicAdapterTests.cs`, `tests/ClinicIt.Tests/ClinicConflictTests.cs`;
  `ClinicIt.csproj` (engine reference). **Zero files under `src/Scheduling.Conflicts/`.**
- **Checkpoint tests:**
  6. `Adapter_Derives_End_From_StartsAt_Plus_Duration` — the duration-to-End math exists in
     exactly one place, and this pins it.
  7. `Adapter_Maps_RoomCode_To_ResourceId` — the identity translation is explicit, not
     incidental.
  8. `Overlapping_Appointments_In_The_Same_Room_Conflict` — the unmodified engine delivers a
     verdict on models it has never heard of.
  9. `A_Ten_Minute_Gap_Conflicts_Because_The_Room_Is_Sanitised_For_Fifteen` — the 15-minute
     buffer, renamed in clinic vocabulary: now a policy the clinic *chose* to buy.
  10. `Group_Room_Twenty_Minute_Gap_Is_Allowed_Whatever_The_Chair_Count` — M2's red,
      resurrected green: 14 chairs, 20-minute gap, `Assert.Empty` — the leak is sealed by a
      decision, not a coincidence.
  11. `The_Refusal_Identifies_The_Blocking_Appointment` — the unwritable test, now three
      lines: `Assert.IsType<AppointmentBookingAdapter>(conflict.Existing)` →
      `Assert.Equal(blocking.Id, adapter.Source.Id)` — the `Guid` round-trips because the
      host gets back the very adapter it handed in.
- **Your-turn box (notes):**
  - *Goal:* two adapter classes; six tests green; the engine untouched — check with
    `git status` before committing: nothing under `src/Scheduling.Conflicts/`.
  - *Constraints:* tests first, watch the CS0246 red; adapters hold ONE readonly field and
    contain zero conditionals (an adapter that starts deciding things is a strategy trying to
    be born — one sentence, pointing at Ch. 20); `AppointmentBookingAdapter` must expose
    `Source` — test 11 is unwritable again without it; the two clinic records remain
    byte-for-byte as typed in M1.
  - *Order-if-you-freeze:* tests 6–7 first (pure adapter math, no engine);
    `AppointmentBookingAdapter` before `TherapyRoomAdapter` (End-from-duration is the
    contract-mandated line); then 8–11 against a hand-built
    `new ConflictChecker([new MeetingRoomConflictStrategy()])`.
  - *Done when:* ClinicIt.Tests 6 green, BookIt.Tests 27 still green (33 total);
    `git status` shows no engine files; T2's every line item answered on paper (lossy id →
    round-trip green; leaked policy → decision test green; exe→exe reference → gone);
    committed.
- **Deliberate red run:** the type-first CS0246 build failure — scripted, then resolved by
  writing the adapters (never by editing the tests).

### M5 — Two hosts, one engine: the demo, the proof, the tag (~11 min)

**Commit:** `ch21-M5: two hosts, one engine — proof and tag` · **Tag:** `bookit-ch21`

ClinicIt gets its composition root and demo scene (`Microsoft.Extensions.Hosting` added to
ClinicIt, `Host.CreateApplicationBuilder`, the *same* `AddConflictDetection()` extension BookIt
calls — one honest sentence: the clinic's container carries two strategies it never routes to;
two dormant singletons are today's price, and a registration-granularity overload is the
documented upgrade the day the price changes). Program prints two scenes:
a sanitising refusal that names the blocking appointment's `Guid` (the M4 round-trip, now on
screen), and the group room's 20-minute gap accepted. Then the payoff ritual (next section)
is *performed by the reader as the your-turn*, and the tag lands.

- **Files added/changed:** `src/ClinicIt/Program.cs` (replaces template stub),
  `src/ClinicIt/ClinicIt.csproj` (Hosting package),
  `tests/ClinicIt.Tests/ClinicCompositionSmokeTests.cs`.
- **Checkpoint tests:**
  12. `The_Clinic_Composition_Root_Resolves_The_Checker` — Ch. 20's smoke-test habit crosses
      hosts: the clinic's wiring is verified against the real, shared composition point.
- **Your-turn box (notes):**
  - *Goal:* clinic demo running through DI; smoke test green; then run every proof command
    yourself and read every empty output.
  - *Constraints:* Program.cs may hold scenes only — construction and `Report`-style printing
    (Ch. 20's demo-harness rule; it is never a protected seam); no new engine or adapter code.
  - *Order-if-you-freeze:* smoke test first (it dictates the composition root); Program is
    BookIt's Program.cs with clinic nouns; proofs verbatim from the chapter.
  - *Done when:* 34 green (27 + 7); both `dotnet run`s print their scenes; all five proof
    outputs match the chapter's printed expectations; committed; tagged.
- **Deliberate red run:** none — M5 is the green the chapter has been buying.

---

## New/changed domain types (exact C# signatures)

**Engine — `src/Scheduling.Conflicts`, all `namespace Scheduling.Conflicts;`** (new project;
strategies/router/registration keep their Ch. 20 bodies, re-typed):

```csharp
// Contracts.cs — the seam the feature OWNS; hosts adapt to it, never the reverse
public interface IBookableResource
{
    string Id { get; }
    string Type { get; }
    int Capacity { get; }
    DayOfWeek? MaintenanceDay { get; }
}

public interface IBookingRecord
{
    string ResourceId { get; }
    DateTime Start { get; }
    DateTime End { get; }
}

// Models.cs — engine-owned value types (moved out of BookIt.Domain)
public static class ResourceTypes
{
    public const string MeetingRoom = "MeetingRoom";
    public const string Equipment = "Equipment";
    public const string Parking = "Parking";
}

public sealed record BookingRequest(DateTime Start, DateTime End);

public sealed record Conflict(IBookingRecord? Existing, string Reason);

// IConflictStrategy.cs — same contract, widened to the engine's interfaces
public interface IConflictStrategy
{
    string ResourceType { get; }

    IReadOnlyList<Conflict> FindConflicts(
        IBookableResource resource,
        BookingRequest request,
        IReadOnlyList<IBookingRecord> existingBookings);
}

// ConflictChecker.cs — public surface (body unchanged apart from types)
public sealed class ConflictChecker
{
    public ConflictChecker(IEnumerable<IConflictStrategy> strategies);

    public IReadOnlyList<Conflict> FindConflicts(
        IBookableResource resource,
        BookingRequest request,
        IReadOnlyList<IBookingRecord> allBookings);
}

// ConflictDetectionRegistration.cs — moves as-is; now the composition point for BOTH hosts
public static class ConflictDetectionRegistration
{
    public static IServiceCollection AddConflictDetection(this IServiceCollection services);
}
```

Strategy classes keep names/keys: `MeetingRoomConflictStrategy`, `EquipmentConflictStrategy`,
`ParkingSpaceConflictStrategy : IConflictStrategy` — bodies identical except parameter types
and the id-free conflict messages (e.g.
`$"Overlaps an existing booking ({booking.Start:HH:mm}–{booking.End:HH:mm}) once the {buffer.TotalMinutes:0}-minute cleaning buffer is applied."`).

**BookIt — `src/BookIt/Domain/Models.cs` after M3** (the one-line adapters; `BookingRequest`,
`Conflict`, `ResourceTypes` deleted here — they are engine types now):

```csharp
using Scheduling.Conflicts;

namespace BookIt.Domain;

public sealed record Resource(
    string Id, string Type, int Capacity = 0, DayOfWeek? MaintenanceDay = null)
    : IBookableResource;

public sealed record Booking(
    int Id, string ResourceId, DateTime Start, DateTime End)
    : IBookingRecord;
```

**ClinicIt — clinic-owned models (M1, byte-stable for the whole chapter),
`namespace ClinicIt.Domain;`:**

```csharp
public sealed record TherapyRoom(string Code, string Name, int TreatmentChairs);

public sealed record Appointment(
    Guid Id, string RoomCode, DateTime StartsAt, int DurationMinutes);
```

**ClinicIt — transient dead end (M1, deleted in M2), `namespace ClinicIt;`:**

```csharp
public static class ClinicModelMapper
{
    public static Resource ToResource(TherapyRoom room);      // Capacity: room.TreatmentChairs — the leak
    public static Booking ToBooking(Appointment appointment); // Id: appointment.Id.GetHashCode() — the loss
}
```

**ClinicIt — the real adapters (M4), `namespace ClinicIt.Adapters;`** (classic ctor + readonly
field, matching Ch. 20's syntax palette — no primary constructors mid-arc):

```csharp
public sealed class TherapyRoomAdapter : IBookableResource
{
    public TherapyRoomAdapter(TherapyRoom room);

    public string Id { get; }                 // => _room.Code
    public string Type { get; }               // => ResourceTypes.MeetingRoom — rule family, not furniture
    public int Capacity { get; }              // => 0 — decision: no clinic room ever takes the event-room buffer
    public DayOfWeek? MaintenanceDay { get; } // => null
}

public sealed class AppointmentBookingAdapter : IBookingRecord
{
    public AppointmentBookingAdapter(Appointment appointment);

    public Appointment Source { get; }        // the round-trip: hosts get their own record back
    public string ResourceId { get; }         // => _appointment.RoomCode
    public DateTime Start { get; }            // => _appointment.StartsAt
    public DateTime End { get; }              // => StartsAt.AddMinutes(DurationMinutes) — the contract-mandated line
}
```

**Changed test double (M3, the one counted non-using test edit):** `RecordingStrategy` in
`ConflictCheckerRouterTests` re-types to
`IReadOnlyList<IBookingRecord>? ReceivedBookings` and the new `FindConflicts` signature.

Compatibility note the manuscript must carry (verified against real code): every existing
`List<Booking>` argument keeps compiling against `IReadOnlyList<IBookingRecord>` parameters
because `IReadOnlyList<out T>` is covariant and `Booking : IBookingRecord`.

## Payoff proof procedure (exact commands and expected outputs)

Performed by the reader as M5's your-turn; the writer prints expected outputs verbatim.
Run from the repo root, bash (Windows readers: Git Bash, as in Ch. 20).

```bash
# 0 — orientation: the story so far, as git tells it
git log --oneline
# expected: ch21-M5 … ch21-M0 on top of M5..M1 and tag bookit-ch20's history

# 1 — the engine has not changed since the milestone where it stabilized
M3=$(git log --oneline | grep "ch21-M3" | cut -d' ' -f1)
git diff --stat "$M3"..HEAD -- src/Scheduling.Conflicts/
# expected: NO output at all. Not "0 files changed" — nothing. Two commits of clinic work
# (M4, M5) happened after M3; none of it touched the engine directory.

# 2 — the engine references no host: metadata and text, independently
grep ProjectReference src/Scheduling.Conflicts/Scheduling.Conflicts.csproj
# expected: no output (exit code 1 — the csproj has zero ProjectReference elements)
git grep -nE "BookIt|ClinicIt" -- src/Scheduling.Conflicts/
# expected: no output — neither host's name exists anywhere in the engine's tracked sources
# (and the suite's Engine_Assembly_References_No_Host_Assembly keeps this true forever)

# 3 — both hosts' suites, one command
dotnet test BookIt.slnx
# expected, two result lines:
#   Passed! - Failed: 0, Passed: 27, Skipped: 0, Total: 27 … BookIt.Tests.dll
#   Passed! - Failed: 0, Passed:  7, Skipped: 0, Total:  7 … ClinicIt.Tests.dll

# 4 — two demos, one engine binary
dotnet run --project src/BookIt
# expected: the four Ch. 20 verdicts, same verdicts — with conflict reasons now citing
# time windows instead of booking #ids (the M3 rewording, visible and named)
dotnet run --project src/ClinicIt
# expected (shape):
#   Bruno 09:50 after Ana 09:00–09:45 in TR-1: 1 conflict(s)
#     - Overlaps an existing booking (09:00–09:45) once the 15-minute cleaning buffer is applied.
#       Blocking appointment: 3f2a… (Ana)          <- the Guid round-trip, on screen
#   Group room, 20-minute gap: OK

# 5 — seal it
git tag bookit-ch21
```

Why this is unfakeable, said in the chapter: (1) is a diff over a commit range the reader
created themselves; (2) is checked twice through independent mechanisms (MSBuild metadata and
full-text search) plus a compiled-in assembly test; (3) runs both suites from one solution
file, so a quietly-broken host cannot hide.

## Felt-pain narrative beats (with the paper tallies)

1. **The referral, and the refusal that is correct.** Cold open: a physiotherapy clinic wants
   the conflict engine — and will not adopt `BookIt.Domain`. The chapter takes the clinic's
   side immediately: their `Appointment` is load-bearing (rows in their database, fields on
   their screens); models are the most expensive thing in a codebase to change, and "adopt my
   models" is the most expensive sentence in integration work. The reader's two instincts —
   copy the engine, or convert their models to ours — are both named, then both walked.
2. **The copy that smuggles (dead end a, M1).** The Conflicts folder won't compile alone; it
   drags `Models.cs` with it, so "just copy the engine" turns out to mean "also adopt the
   models — plus a fork". **Tally T1:** *files copied: 6 · files to fix twice forever: 6 ·
   foreign models smuggled: 1.* Ch. 20's copy-paste road, one project-size step later.
3. **The seduction of the mapper (M1).** The ProjectReference road goes green in twenty
   minutes, and the chapter lets it feel good — three passing tests, an honest commit. The
   trap is that every mapper line is *locally* reasonable: of course `TreatmentChairs` maps to
   `Capacity`; of course a `Guid` becomes *some* int. **Tally T2 starts at the keyboard:**
   each line that invents or destroys information gets a tick as it is typed
   (`GetHashCode()` — destroys; `TreatmentChairs→Capacity` — invents meaning;
   `MaintenanceDay` — silently asserted null; the csproj line — an exe referencing a
   competitor's exe).
4. **Tuesday, clinic edition (M2).** Two innocent requests detonate the mapper. The group
   room's 20-minute gap is refused by a *30-minute event-room buffer* the clinic never heard
   of — the red run, with the leak named in the failure text the reader must copy into their
   notes. Then the harder pain: the "which patient blocks this slot?" test **cannot be
   written** — `GetHashCode()` is a one-way door. A red test hurts; an unwritable test
   condemns. **T2 closes with the structural item:** *every future BookIt release recompiles
   the clinic; the dependency arrow points at a business that owes the clinic nothing.*
5. **The reframe (interlude).** All four T2 lines are one disease: the dependency points at a
   *host's* models. Adapter, properly told, is a direction decision first and a class shape
   second: the feature owns interfaces sized to what its math needs — and nothing else (no
   identity in `IBookingRecord`, *because of* beat 4). Hosts adapt: BookIt with one line per
   record, the clinic with two small classes. The tallies become the interface's member list.
6. **The payoff, twice felt (M4–M5).** M2's two impossible tests return and go green —
   `Assert.Empty` for the chair count, `Guid`-equality through `Source` for the round-trip —
   and then the proof ritual: an empty diff, two silent greps, 34 green in one command, and a
   clinic demo naming a blocking appointment by the `Guid` the mapper used to destroy.

## Reader time budget

Nominal 85 minutes; 90 with slack. First run may stretch toward two hours — calibration, not
failure (same framing as Ch. 20). Practice rules apply: no AI, type everything including
checkpoint tests, 10-minute timer before hints.

| Segment | Content | Minutes |
|---|---|---|
| M0 + state check | verify tag/26 green; gitignore repair | 5 |
| M1 | copy experiment (4) + scaffold (4) + mapper & 3 tests (10) | 18 |
| M2 | red run, unwritable test, tally, back-out | 12 |
| M3 | extraction: classlib, contracts, re-typing, usings, purity test | 23 |
| M4 | two adapters, six tests (CS0246 red first) | 16 |
| M5 | clinic Program + smoke test + proof ritual + tag | 11 |
| **Total** | | **85 (ceiling 90)** |

Slack lives in M3 (the compile wall is where first-timers stall) and the two scripted reds.

## Deviations from contract

1. **Extra commit `ch21-M0` outside the `ch21-M1…Mk` naming scheme.** Reason: the actual
   `bookit-ch20` tag state has no `.gitignore` and tracks 198 `bin/`/`obj/` artifacts — the
   executed Ch. 20 drifted from its own manuscript (which prescribed `dotnet new gitignore`).
   Chapter 21's payoff is diff-based; without the repair, every `git diff --stat` proof drowns
   in artifact noise. A dedicated housekeeping commit keeps M1's diff readable (folding the
   198 deletions into M1 would bury the milestone's actual story). Alternative considered and
   rejected: silently fixing the repo before publication — the contract calls silent drift a
   defect, and the state-check box makes the repair reproducible for readers starting from
   the tag.

2. **Ch. 20 demo drift: one projector in the executed repo, two in the Ch. 20 strategy doc.**
   The `bookit-ch20` tag's `Program.cs` holds a single projector (`EQ-PROJ-1` with
   `MaintenanceDay: DayOfWeek.Wednesday`; Wednesday scene labeled "Projector, Wednesday
   (maintenance day)"), while the Ch. 20 strategy doc scripted a `projector`/`servicedProjector`
   (`EQ-PROJ-2`) split labeled "Serviced projector, Wednesday (maintenance day)". Ch. 21's
   proof ritual prints the executed output, and its Done-when demands that proof outputs match
   the printed expectations — so the drift would have broken the ritual for a faithful Ch. 20
   reader. Resolution: the repo is canonical; the Ch. 20 strategy doc's M2 Program migration
   and M4 Program.cs were re-synced to the single-projector shape, and Ch. 21's printed output
   stands unchanged. (Ch. 20's checkpoint tests are unaffected — their `EQ-PROJ-2` fixtures are
   test-local and match the repo.) Same class of drift as item 1, recorded here per the arc
   contract's silent-drift rule.

No other deviations. Contract items honored as specified, with designer-latitude calls
recorded inline above: interface members exactly as the contract sketches
(`IBookableResource { Id, Type, Capacity, MaintenanceDay }`, `IBookingRecord { ResourceId,
Start, End }`); `AppointmentBookingAdapter` computes `End = StartsAt.AddMinutes(DurationMinutes)`;
engine has zero ProjectReferences (checked three ways); all 26 pre-existing tests survive
unrelocated, adapted only by `using` lines plus the counted `RecordingStrategy` re-typing;
`Program.cs` extended by one scene per host; clinic uses only the MeetingRoom-family rules.

---

*Writer notes (not part of the reader-facing design):* In-the-wild sections ground on
`_Curated\CORE\patterns\adapter-pattern-module-decoupling.md` and
`_Curated\_extracted\Education__03-Adapter-Pattern-Explained.md.txt`; use placeholder names
("the care-workforce platform", "the HR module") — never real product names. Ch. 21's drills
descend from `DesignPatterns_Drills`; final drill is "The Engine Contract, From Memory" —
interfaces + one adapter from a blank file, tomorrow. Journeying On hands to Ch. 22: the front
desk's rulebook (validation that isn't about time overlap), with `BookingRulebook` standing in
front of the engine both hosts now share.
