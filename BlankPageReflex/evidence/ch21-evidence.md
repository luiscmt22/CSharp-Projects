# Chapter 21 — Execution Evidence

Repo: `scratchpad/book/BookIt` · baseline `bookit-ch20` (1e81147, 26 tests green, verified) ·
final tag `bookit-ch21` (092796f). All captures below are verbatim from real runs
(dotnet SDK 10.0.300, Windows, Git Bash). Long absolute path prefixes are the only thing
shortened, marked `<repo>`.

## Milestone ledger

| Milestone | Commit | Message | Suite after commit |
|---|---|---|---|
| M0 | `0fb9260` | ch21-M0: evict build artifacts; every proof in this chapter is a diff | BookIt 26 green (unchanged) |
| M1 | `700d79d` | ch21-M1: ClinicIt on the mapping road — green, and wrong | BookIt 26 + ClinicIt 3 = 29 green |
| M2 | `6b09eac` | ch21-M2: tally the damage, back out the mapping layer | BookIt 26 + ClinicIt 0 (empty suite, exit 0) |
| M3 | `2de6019` | ch21-M3: extract Scheduling.Conflicts — the engine owns its contracts | BookIt 27 + ClinicIt 0 |
| M4 | `04812dd` | ch21-M4: clinic adapters — their models, our engine | BookIt 27 + ClinicIt 6 = 33 green |
| M5 | `092796f` | ch21-M5: two hosts, one engine — proof and tag | BookIt 27 + ClinicIt 7 = **34 green** |

Baseline check before M0:

```
1e81147 M5: parking - one class, one line
Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26, Duration: 35 ms - BookIt.Tests.dll (net10.0)
```

## M0 checkpoints (non-xUnit)

- Tracked `bin/`/`obj/` artifacts before: `git ls-files | grep -cE 'bin/|obj/'` → `198`.
- After `dotnet new gitignore` + `git rm -r --cached` + commit + `dotnet build BookIt.slnx`:
  `git status --short` → no output (clean); tracked artifacts → `0`.

## M1 — dead end (a), the copy that smuggles (performed, uncommitted)

```
$ cp -r src/BookIt/Conflicts src/ClinicIt-copy && ls src/ClinicIt-copy
ConflictChecker.cs
ConflictDetectionRegistration.cs
EquipmentConflictStrategy.cs
IConflictStrategy.cs
MeetingRoomConflictStrategy.cs
ParkingSpaceConflictStrategy.cs

$ grep -c 'using BookIt.Domain;' src/ClinicIt-copy/*.cs | grep -v ':0'
src/ClinicIt-copy/ConflictChecker.cs:1
src/ClinicIt-copy/EquipmentConflictStrategy.cs:1
src/ClinicIt-copy/IConflictStrategy.cs:1
src/ClinicIt-copy/MeetingRoomConflictStrategy.cs:1
src/ClinicIt-copy/ParkingSpaceConflictStrategy.cs:1
```

T1: files copied 6 · files to fix twice forever 6 · foreign models smuggled 1
(`Models.cs` would be the seventh file). Folder deleted; nothing committed.

M1 suite after mapper + 3 checkpoint tests:

```
Passed!  - Failed:     0, Passed:    26, Skipped:     0, Total:    26, Duration: 53 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     3, Skipped:     0, Total:     3, Duration: 214 ms - ClinicIt.Tests.dll (net10.0)
```

## Red run 1 (M2, flagship) — the borrowed policy leak

Test typed into `ClinicMappingTests.cs` (uncommitted edit, deleted with its file in the
back-out): `Group_Room_Twenty_Minute_Gap_Is_Allowed_Because_The_Clinic_Sanitises_In_Fifteen`.

```
[xUnit.net 00:00:00.20]     ClinicIt.Tests.ClinicMappingTests.Group_Room_Twenty_Minute_Gap_Is_Allowed_Because_The_Clinic_Sanitises_In_Fifteen [FAIL]
  Failed ClinicIt.Tests.ClinicMappingTests.Group_Room_Twenty_Minute_Gap_Is_Allowed_Because_The_Clinic_Sanitises_In_Fifteen [82 ms]
  Error Message:
   Assert.Empty() Failure: Collection was not empty
Collection: [Conflict { Existing = Booking { Id = -620002672, ResourceId = TR-GROUP, Start = 13/07/2026 09:00:00, End = 13/07/2026 09:45:00 }, Reason = Overlaps booking #-620002672 once the 30-minute cleaning buffer is applied. }]

Failed!  - Failed:     1, Passed:     3, Skipped:     0, Total:     4, Duration: 101 ms - ClinicIt.Tests.dll (net10.0)
```

The failure names the leak twice: a **30-minute cleaning buffer** the clinic never asked for
(TreatmentChairs 14 crossed BookIt's `EventRoomCapacityThreshold = 12`), and the mangled
identity `#-620002672` — `appointment.Id.GetHashCode()`, the one-way trip. The unwritable
test's name went on paper: `The_Refusal_Identifies_The_Blocking_Appointment` (returns in M4).

M2 back-out checks:

```
$ git grep BookIt -- src/ClinicIt        → no output (exit 1)
$ dotnet test BookIt.slnx                → BookIt 26 green; ClinicIt.Tests: no tests, run exit code 0
```

## Red run 2 (M3, compile kind) — the extraction wall

Stage 1 — right after `git mv` + engine rewrite, before touching any host code
(`dotnet build BookIt.slnx`), BookIt itself falls first:

```
Build FAILED.    1 Error(s)
src\BookIt\Program.cs(2,14): error CS0234: The type or namespace name 'Conflicts' does not exist in the namespace 'BookIt' (are you missing an assembly reference?)
```

Stage 2 — after adapting BookIt (`Domain/Models.cs` one-line adapters + `Program.cs` using),
the test project's wall is the adaptation worksheet (deduplicated list):

```
tests\BookIt.Tests\ConflictCheckerRouterTests.cs(2,14): error CS0234: The type or namespace name 'Conflicts' does not exist in the namespace 'BookIt'
tests\BookIt.Tests\ConflictCheckerRouterTests.cs(11,46): error CS0246: The type or namespace name 'IConflictStrategy' could not be found
tests\BookIt.Tests\ConflictCheckerRouterTests.cs(17,30): error CS0246: The type or namespace name 'Conflict' could not be found
tests\BookIt.Tests\ConflictCheckerRouterTests.cs(18,32): error CS0246: The type or namespace name 'BookingRequest' could not be found
tests\BookIt.Tests\ConflictCheckerTests.cs(2,14): error CS0234: The type or namespace name 'Conflicts' does not exist in the namespace 'BookIt'
tests\BookIt.Tests\ConflictCheckerTests.cs(9,20): error CS0246: The type or namespace name 'ConflictChecker' could not be found
tests\BookIt.Tests\ConflictDetectionSmokeTests.cs(2,14): error CS0234: ...
tests\BookIt.Tests\EquipmentConflictStrategyTests.cs(2,14): error CS0234: ...
tests\BookIt.Tests\MeetingRoomConflictStrategyTests.cs(2,14): error CS0234: ...
tests\BookIt.Tests\ParkingSpaceConflictStrategyTests.cs(2,14): error CS0234: ...
```

Exactly the counted churn: six files' using lines, plus `RecordingStrategy` (the CS0246
cluster in `ConflictCheckerRouterTests.cs`). Green after: **27** (26 survivors + purity test).

M3 refactor certificate — `git diff 6b09eac..2de6019 -- tests/BookIt.Tests/` content lines,
complete:

```
ConflictCheckerRouterTests.cs   -using BookIt.Conflicts;  +using Scheduling.Conflicts;
                                -public IReadOnlyList<Booking>? ReceivedBookings ...
                                +public IReadOnlyList<IBookingRecord>? ReceivedBookings ...
                                -    Resource resource, BookingRequest request, IReadOnlyList<Booking> existingBookings)
                                +    IBookableResource resource, BookingRequest request, IReadOnlyList<IBookingRecord> existingBookings)
ConflictCheckerTests.cs         -using BookIt.Conflicts;  +using Scheduling.Conflicts;
ConflictDetectionSmokeTests.cs  -using BookIt.Conflicts;  +using Scheduling.Conflicts;
EquipmentConflictStrategyTests.cs    (same one-line using swap)
MeetingRoomConflictStrategyTests.cs  (same one-line using swap)
ParkingSpaceConflictStrategyTests.cs (same one-line using swap)
+ tests/BookIt.Tests/EnginePurityTests.cs (new file)
```

No test was relocated; no assert changed.

## Red run 3 (M4) — tests typed before the adapters exist

`dotnet build tests/ClinicIt.Tests` with `ClinicAdapterTests.cs` + `ClinicConflictTests.cs`
written and no adapter classes:

```
Build FAILED.
tests\ClinicIt.Tests\ClinicAdapterTests.cs(2,16): error CS0234: The type or namespace name 'Adapters' does not exist in the namespace 'ClinicIt' (are you missing an assembly reference?)
tests\ClinicIt.Tests\ClinicConflictTests.cs(2,16): error CS0234: The type or namespace name 'Adapters' does not exist in the namespace 'ClinicIt' (are you missing an assembly reference?)
```

(4 error lines total, all CS0234 — see Deviations #2: the design scripted this red as
CS0246; the compiler stops at the `using ClinicIt.Adapters;` line because the whole
namespace is missing, not just the two types. Same cause, earlier symptom.)

Resolved by writing the adapters, never by editing the tests. Green after:

```
Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27, Duration: 74 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     6, Skipped:     0, Total:     6, Duration: 82 ms - ClinicIt.Tests.dll (net10.0)
```

`git status --short -- src/Scheduling.Conflicts/` before the M4 commit: no output —
zero engine files touched; the M4 commit contains only ClinicIt + ClinicIt.Tests files.

## Payoff proof procedure (verbatim, run after the M5 commit)

```
# 0 — orientation
$ git log --oneline
092796f ch21-M5: two hosts, one engine — proof and tag
04812dd ch21-M4: clinic adapters — their models, our engine
2de6019 ch21-M3: extract Scheduling.Conflicts — the engine owns its contracts
6b09eac ch21-M2: tally the damage, back out the mapping layer
700d79d ch21-M1: ClinicIt on the mapping road — green, and wrong
0fb9260 ch21-M0: evict build artifacts; every proof in this chapter is a diff
1e81147 M5: parking - one class, one line
f8c2192 M4: DI wiring + smoke tests
6b7489e M3: strategy extraction
e2a1c61 M2: capacity buffer + maintenance day
740efb2 M1: naive checker

# 1 — the engine has not changed since it stabilized
$ M3=2de6019
$ git diff --stat "$M3"..HEAD -- src/Scheduling.Conflicts/
(no output at all — exit 0; two commits of clinic work touched nothing in the engine)

# 2 — the engine references no host: metadata and text, independently
$ grep ProjectReference src/Scheduling.Conflicts/Scheduling.Conflicts.csproj
(no output — exit 1)
$ git grep -nE "BookIt|ClinicIt" -- src/Scheduling.Conflicts/
(no output — exit 1; and Engine_Assembly_References_No_Host_Assembly keeps this true forever)

# 3 — both hosts' suites, one command
$ dotnet test BookIt.slnx
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 98 ms - ClinicIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:    27, Skipped:     0, Total:    27, Duration: 78 ms - BookIt.Tests.dll (net10.0)

# 5 — seal it
$ git tag bookit-ch21
```

## Final demo runs (verbatim)

```
$ dotnet run --project src/BookIt
Atlas room 10:20, twenty minutes after the previous meeting: 1 conflict(s)
  - Overlaps an existing booking (09:00–10:00) once the 30-minute cleaning buffer is applied.
Projector, Monday morning (already out Monday afternoon): 1 conflict(s)
  - Equipment is checked out per day; an existing booking (14:00–16:00) already claims it.
Projector, Wednesday (maintenance day): 1 conflict(s)
  - EQ-PROJ-1 is serviced every Wednesday; bookings touching a Wednesday are refused.
Parking space 12, back-to-back at noon (no buffer needed): OK

$ dotnet run --project src/ClinicIt
Bruno 09:50 after Ana 09:00–09:45 in TR-1: 1 conflict(s)
  - Overlaps an existing booking (09:00–09:45) once the 15-minute cleaning buffer is applied.
    Blocking appointment: fd6af0bc-e29a-458f-b3d3-1859598c3326 (Ana)
Group room, 20-minute gap: OK
```

Same four BookIt verdicts as Chapter 20, with conflict reasons now citing time windows
instead of booking #ids (the M3 rewording, visible). The clinic's refusal names the blocking
appointment by the `Guid` the M1 mapper used to destroy (the Guid varies per run —
`Guid.NewGuid()` in the demo — the round-trip is what the smoke of it proves; test 11 pins
it deterministically).

## Engine identity check (M3 constraint)

`grep -rn '\.Id' src/Scheduling.Conflicts/*.cs` after M3 (unchanged through HEAD):

```
ConflictChecker.cs:25:            .Where(b => b.ResourceId == resource.Id)
EquipmentConflictStrategy.cs:17:  $"{resource.Id} is serviced every {maintenanceDay}; ..."
```

Only `resource.Id`; no booking identity anywhere in the engine.

## Deviations

1. **(Carried from the design doc, executed as designed)** Extra commit `ch21-M0` outside the
   `ch21-M1…Mk` scheme — the `bookit-ch20` state tracked 198 `bin/`/`obj/` artifacts and had
   no `.gitignore`; every diff-based proof in this chapter needed the repair. Already recorded
   in ch21-design.md's Deviations section; executed exactly as written there.
2. **M4 scripted red is CS0234, not CS0246.** The design predicted
   `CS0246 (AppointmentBookingAdapter not found)`. In reality the compiler fails first at
   `using ClinicIt.Adapters;` with CS0234 (the namespace does not exist before the first
   adapter file is created), which suppresses the downstream type errors. Same cause — the
   adapters don't exist yet — earlier symptom. The manuscript should quote CS0234 at the
   using line; no code or ordering changed.
3. **Stage-1/stage-2 shape of the M3 compile wall.** The design describes one error wall;
   MSBuild delivers it in two stages because BookIt.Tests is not compiled while BookIt itself
   fails (stage 1: `Program.cs` CS0234; stage 2, after adapting BookIt: the six test files +
   RecordingStrategy). Both stages captured above; the worksheet content matches the design's
   count exactly. Worth one honest sentence in the manuscript — solution-scale walls arrive
   in dependency order.
4. **Package versions pinned to 10.0.9** (`Microsoft.Extensions.DependencyInjection.Abstractions`
   in the engine, `Microsoft.Extensions.Hosting` in ClinicIt) to match BookIt's existing
   Hosting 10.0.9 — the design left versions unspecified; a mixed-version repo would be noise
   in a book listing.
5. **Cosmetic:** the six test files' using swap was normalized to sorted order
   (`using BookIt.Domain;` before `using Scheduling.Conflicts;`), matching Chapter 20's
   sorted-usings style; the diff remains confined to using lines.

## Final state

- Tags: `bookit-ch20` (1e81147), `bookit-ch21` (092796f). Working tree clean.
- Suite: 34 green (BookIt.Tests 27, ClinicIt.Tests 7); zero red, zero skipped.
- Engine `src/Scheduling.Conflicts/`: 8 .cs files + csproj, zero ProjectReferences,
  zero host names in sources, unchanged since `2de6019`.
- Red runs executed: 3 (M2 test red · M3 compile wall · M4 tests-first red).
