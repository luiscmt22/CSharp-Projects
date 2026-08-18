# Chapter 23 Evidence — Builder: The Monday Report

Executed 27/07/2026 against `bookit-ch22` (verified baseline: BookIt.Tests 54 + ClinicIt.Tests 7,
all green in ~1s). Design: `designs/ch23-design.md`. Every output below is real, captured from the
commands shown.

Preconditions verified before ch23-M1 (D1 gate): `BookIt.FrontDesk.Refusal(string MemberId,
string ResourceId, string RuleName, string Reason, DateTime RequestedStart)` and
`IRefusalLog.All { get; }` exist in the repo exactly as pinned; suite baseline 54 + 7 confirmed
by `dotnet test`. No upstream drift found. The `.gitignore` repair from ch21-M0 held: `git status`
stayed clean of `bin/`/`obj/` throughout; no hygiene step was needed (D2 as designed).

## Milestones

| Milestone | Commit | Message | BookIt.Tests | ClinicIt.Tests | Total |
|---|---|---|---|---|---|
| M1 | `6e56330` | ch23-M1: the Monday report, fixed menu | 60 | 7 | 67 |
| M2 | `bfa4c76` | ch23-M2: options via parameters — the telescoping tally | 63 | 7 | 70 |
| M3 | `c225040` | ch23-M3: fluent builder, staged entry, immutable report | 68 | 7 | 75 |
| M4 | `c2b5ac3` | ch23-M4: content and presentation facets | 70 | 7 | 77 |
| M5 | `8e7c51e` | ch23-M5: parking occupancy — one section, one method | 74 | 7 | 81 |

Tag `bookit-ch23` on `8e7c51e`. Every commit left the full suite green; the only reds were the
three scripted red runs below, all resolved (or backed out, per script) before committing.

Suite arithmetic matches the design exactly: 54 → 60 → 63 → 68 → 70 → 74 (BookIt.Tests;
+6, +3, +5, +2, +4 = the installment's 20 tests); ClinicIt.Tests held at 7 throughout.

M3 certificate of refactor (captured before committing M3 — `WeeklyReportTests.cs` absorbs the
API replacement through the `FullWeekReport` seam plus three counted helper additions; tests 1–6
bodies untouched, tests 7–9 rewritten to helper calls, tally (c): 6 survivors, 3 casualties):

```
$ git diff --stat tests/BookIt.Tests/WeeklyReportTests.cs   # before committing M3
 tests/BookIt.Tests/WeeklyReportTests.cs | 36 ++++++++++++++++++++++++++++-----
 1 file changed, 31 insertions(+), 5 deletions(-)
```

M3 mechanical check from the your-turn box: `grep -rn 'AddDays(7)' src/BookIt/Reports/Sections/`
returns 0 hits — the week filter lives in `Build()` and nowhere else.

M4 done-when check: `git diff --stat -- src/BookIt/Reports/Sections` for the M4 working tree was
EMPTY — the facet refactor touched no file under `Sections/`.

## Red run 1 — M2 scripted excursion: the mutable-setters escape hatch

Procedure executed as designed: M2 proper (telescoping ctor + tests 7–9 + investor call site) went
green and was STAGED (`git add -A`); the excursion (parameterless ctor + public setters +
`Compute()`, plus the excursion test) was then written on top, worktree-only. `dotnet test` —
exactly ONE test fails; the compiler was happy, the other 63 tests were happy, and the investor
pack printed the refusal log:

```
[xUnit.net 00:00:00.26]     BookIt.Tests.WeeklyReportTests.A_Reused_Report_Object_Does_Not_Leak_Choices_Into_The_Next_Report [FAIL]
  Failed BookIt.Tests.WeeklyReportTests.A_Reused_Report_Object_Does_Not_Leak_Choices_Into_The_Next_Report [20 ms]
  Error Message:
   Assert.DoesNotContain() Failure: Item found in collection
             ↓ (pos 0)
Collection: ["Refusals"]
Found:      "Refusals"

Failed!  - Failed:     1, Passed:    63, Skipped:     0, Total:    64, Duration: 27 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 22 ms - ClinicIt.Tests.dll (net10.0)
```

Back-out, as scripted: `git restore src/BookIt/Reports/WeeklyReport.cs
tests/BookIt.Tests/WeeklyReportTests.cs` (restore-from-index returns the files to the staged
M2-proper state, deleting the excursion and its test in the same stroke). Suite green again
(63 + 7), then committed as `bfa4c76`.

## Red run 2 — M3 does-not-compile box: a report without a week

Scratch test typed into `WeeklyReportBuilderTests.cs`:

```csharp
var report = new WeeklyReportBuilder().Build(Sources());
var sneaky = new WeeklyReportBuilder(new DateOnly(2026, 7, 13));
```

`dotnet build` — exit code 1 (captured):

```
tests\BookIt.Tests\WeeklyReportBuilderTests.cs(29,26): error CS1729: 'WeeklyReportBuilder' does not contain a constructor that takes 0 arguments
tests\BookIt.Tests\WeeklyReportBuilderTests.cs(30,26): error CS1729: 'WeeklyReportBuilder' does not contain a constructor that takes 1 arguments
Build FAILED.
exit code: 1
```

Both lines deleted; `dotnet build` green; suite 68 + 7 green. (See Deviations D-A for the
CS1729-vs-CS0122 note on the second line.) Test #10
(`ForWeekStarting_Any_Day_But_Monday_Is_Refused_And_The_Day_Is_Named`) pins the runtime Monday
validation the staged entry still owns — the two guards are distinct, as the design requires.

## Red run 3 — M5 test-first: the spec exists before the code

`ParkingOccupancySectionTests.cs` typed with tests 17–19 FIRST; `dotnet build` — exit code 1
(captured):

```
tests\BookIt.Tests\ParkingOccupancySectionTests.cs(30,27): error CS0246: The type or namespace name 'ParkingOccupancySection' could not be found (are you missing a using directive or an assembly reference?)
tests\BookIt.Tests\ParkingOccupancySectionTests.cs(46,27): error CS0246: The type or namespace name 'ParkingOccupancySection' could not be found (are you missing a using directive or an assembly reference?)
tests\BookIt.Tests\ParkingOccupancySectionTests.cs(61,27): error CS0246: The type or namespace name 'ParkingOccupancySection' could not be found (are you missing a using directive or an assembly reference?)
Build FAILED.
exit code: 1
```

`ParkingOccupancySection` created (EquipmentTravelSection's fold, hours instead of counts) →
suite green at 73 + 7 with tests 17–19 passing. Then test 20 + the one facet method
(`ContentFacet.ParkingOccupancy()`) in one step → 74 + 7 green (see Deviations D-D).

## Payoff proof (run after `ch23-M5`, from the repo root)

```
$ M4=$(git log --format=%h --grep='ch23-M4' -1)      # c2b5ac3

$ git diff --stat $M4..HEAD -- src/BookIt/Reports src/BookIt/Program.cs tests/BookIt.Tests
 src/BookIt/Program.cs                              |  2 +-
 src/BookIt/Reports/ContentFacet.cs                 |  6 ++
 .../Reports/Sections/ParkingOccupancySection.cs    | 35 +++++++++
 tests/BookIt.Tests/ParkingOccupancySectionTests.cs | 82 ++++++++++++++++++++++
 4 files changed, 124 insertions(+), 1 deletion(-)

$ git diff $M4..HEAD -- \
    src/BookIt/Reports/WeeklyReport.cs \
    src/BookIt/Reports/WeeklyReportBuilder.cs \
    src/BookIt/Reports/PresentationFacet.cs \
    src/BookIt/Reports/Sections/RoomUsageSection.cs \
    src/BookIt/Reports/Sections/EquipmentTravelSection.cs \
    src/BookIt/Reports/Sections/RefusalsSection.cs
(no output — EMPTY. Build(), the record, the other facet, every existing section: untouched.)

$ dotnet test --nologo -v q
Passed!  - Failed:     0, Passed:    74, Skipped:     0, Total:    74, Duration: 33 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 95 ms - ClinicIt.Tests.dll (net10.0)

$ git tag bookit-ch23     # on 8e7c51e
```

## Final `dotnet run --project src/BookIt` output (verbatim, after ch23-M5)

```
Atlas room 10:20, twenty minutes after the previous meeting: 1 conflict(s)
  - Overlaps an existing booking (09:00–10:00) once the 30-minute cleaning buffer is applied.
Projector, Monday morning (already out Monday afternoon): 1 conflict(s)
  - Equipment is checked out per day; an existing booking (14:00–16:00) already claims it.
Projector, Wednesday (maintenance day): 1 conflict(s)
  - EQ-PROJ-1 is serviced every Wednesday; bookings touching a Wednesday are refused.
Parking space 12, back-to-back at noon (no buffer needed): OK

Front desk — Rui (unpaid), Atlas Monday 09:30: REFUSED [MemberStanding]
  - Rui has unpaid invoices; bookings are blocked until they are settled.
Front desk — Ana, Atlas Sunday 10:00: REFUSED [OpeningDays]
  - The space is closed on Sundays; this request touches 19/07/2026.
Front desk — Ana, whiteboard on floor 3 at 10:10: REFUSED [EquipmentTransfer]
  - EQ-WB-1 needs 15 minutes to travel between floor 1 and floor 3; booking #4 is too close.
Front desk — Ana, ROOM-1 Tuesday 14:00: ACCEPTED

Refusal log (3) — the owner will ask about these on Monday:
  - M-2, ROOM-ATLAS: [MemberStanding] Rui has unpaid invoices; bookings are blocked until they are settled.
  - M-1, ROOM-ATLAS: [OpeningDays] The space is closed on Sundays; this request touches 19/07/2026.
  - M-1, EQ-WB-1: [EquipmentTransfer] EQ-WB-1 needs 15 minutes to travel between floor 1 and floor 3; booking #4 is too close.

=== Week 29 — full ===
Week of 13/07: 4 bookings, 3 refusals
Room usage — busiest: ROOM-ATLAS (1.0h)
  ROOM-ATLAS: 1.0h
Equipment travel — most travelled: EQ-PROJ-1 (1 checkout(s))
  EQ-PROJ-1: 1 checkout(s)
  EQ-WB-1: 1 checkout(s)
Refusals — 3 refusal(s) across 3 rule(s)
  MemberStanding: 1
  OpeningDays: 1
  EquipmentTransfer: 1
Parking occupancy — busiest space: PARK-12 (4.0h)
  PARK-12: 4.0h

=== Investor weekly ===
Week of 13/07: 4 bookings, 3 refusals
```

## Deviations

- **D-A — Compile box errors are CS1729 + CS1729, not CS1729 + CS0122.** The design predicted
  CS0122 ("inaccessible due to its protection level") for the `sneaky` line. The current Roslyn
  (SDK 10.0.300) excludes the inaccessible private constructor from the candidate set entirely
  and reports CS1729 ("does not contain a constructor that takes 1 arguments") instead. The
  teaching point is unchanged — both sentences are ones the language refuses to compile, proven
  by `dotnet build` exiting 1 — but the manuscript's "does not compile" box must quote the real
  CS1729 pair above, not the predicted CS0122.
- **D-B — Demo headline reads "4 bookings, 3 refusals", not the design sketch's "3 bookings".**
  The ch22 demo scene seeds FOUR week bookings (ROOM-ATLAS, EQ-PROJ-1, PARK-12, EQ-WB-1 — the
  fourth was added by ch22-M2's transfer-window scene). The design's expected-tail sketch was
  written from the ch22 design, which at that point listed three. Code is correct; the sketch was
  stale. The three ch22-scene refusals ARE week-filtered in, exactly as predicted.
- **D-C — Demo tail title is "Week 29 — full", not "Monday report — week of 13/07/2026".** The
  design pins the M4 owner call site as `.Presentation(p => p.Title("Week 29 — full").Top(3))`
  and simultaneously sketches the payoff tail with the default title; both cannot hold. The
  pinned call site wins (it is the exhibit read aloud against M2's `false, false, false, 5`).
  The default title stays pinned green by `Custom_Title_Replaces_The_Default`'s untitled path.
- **D-D — M5 micro-order: test 20 and the facet method landed in one step.** The design's
  order-if-you-freeze reads "then test 20, then the facet method"; typing test 20 alone would
  have produced an unscripted CS1061 red (no `ParkingOccupancy` on `ContentFacet` yet). The
  scripted CS0246 red was executed exactly as designed; test 20 and the 6-line facet method were
  written together and went green in one run.
