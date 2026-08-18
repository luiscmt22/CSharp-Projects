# Chapter 22 Evidence — Chain of Responsibility: The Front Desk Rulebook

Executed 27/07/2026 against `bookit-ch21` (verified baseline: BookIt.Tests 27 + ClinicIt.Tests 7,
all green). Design: `designs/ch22-design.md`. Every output below is real, captured from the
commands shown.

## Milestones

| Milestone | Commit | Message | BookIt.Tests | ClinicIt.Tests | Total |
|---|---|---|---|---|---|
| M1 | `bc1146e` | ch22-M1: front desk rules, the obvious way | 31 | 7 | 38 |
| M2 | `17b2cd5` | ch22-M2: transfer window - the method buckles | 34 | 7 | 41 |
| M3 | `cd85acf` | ch22-M3: rulebook extraction | 48 | 7 | 55 |
| M4 | `4491dcf` | ch22-M4: DI order + smoke net | 51 | 7 | 58 |
| M5 | `c952fda` | ch22-M5: advance window - one class, one line | 54 | 7 | 61 |

Tag `bookit-ch22` on `c952fda`. Every commit left the full suite green; the only reds were the
two scripted red runs below, both resolved before committing.

Suite arithmetic matches the design exactly: 27 → 31 → 34 → 48 → 51 → 54 (BookIt.Tests);
ClinicIt.Tests held at 7 throughout.

M3 certificate of refactor (the only edit in `BookingDeskTests.cs` is `CreateDesk()`'s body):

```
$ git diff --stat tests/BookIt.Tests/BookingDeskTests.cs   # before committing M3
 tests/BookIt.Tests/BookingDeskTests.cs | 11 +++++++++--
 1 file changed, 9 insertions(+), 2 deletions(-)
```
(the full hunk shows only the `CreateDesk()` body: naive checker+enum out, rulebook+log in)

## Red run 1 — M4 scripted: ConflictRule registration moved to the TOP of AddFrontDesk

`dotnet test` — exactly ONE test fails; every rule unit test stays green:

```
[xUnit.net 00:00:00.21]     BookIt.Tests.FrontDeskSmokeTests.Rules_Resolve_In_Registration_Order_And_That_Order_Is_The_Spec [FAIL]
  Failed BookIt.Tests.FrontDeskSmokeTests.Rules_Resolve_In_Registration_Order_And_That_Order_Is_The_Spec [6 ms]
  Error Message:
   Assert.Equal() Failure: Collections differ
                        ↓ (pos 0)
Expected: string[]     ["MemberStanding", "OpeningDays", "EquipmentTransfer", "ResourceConflicts"]
Actual:   List<string> ["ResourceConflicts", "MemberStanding", "OpeningDays", "EquipmentTransfer"]
                        ↑ (pos 0)

Failed!  - Failed:     1, Passed:    50, Skipped:     0, Total:    51, Duration: 27 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 23 ms - ClinicIt.Tests.dll (net10.0)
```

`dotnet run` under the broken order — same inputs, different verdict text; the unpaid member is
told about a room clash instead of his invoices:

```
Front desk — Rui (unpaid), Atlas Monday 09:30: REFUSED [ResourceConflicts]
  - Overlaps an existing booking (09:00–10:00) once the 30-minute cleaning buffer is applied.
Front desk — Ana, Atlas Sunday 10:00: REFUSED [OpeningDays]
  - The space is closed on Sundays; this request touches 19/07/2026.
Front desk — Ana, whiteboard on floor 3 at 10:10: REFUSED [ResourceConflicts]
  - Equipment is checked out per day; an existing booking (09:00–10:00) already claims it.
Front desk — Ana, ROOM-1 Tuesday 14:00: ACCEPTED

Refusal log (3) — the owner will ask about these on Monday:
  - M-2, ROOM-ATLAS: [ResourceConflicts] Overlaps an existing booking (09:00–10:00) once the 30-minute cleaning buffer is applied.
  - M-1, ROOM-ATLAS: [OpeningDays] The space is closed on Sundays; this request touches 19/07/2026.
  - M-1, EQ-WB-1: [ResourceConflicts] Equipment is checked out per day; an existing booking (09:00–10:00) already claims it.
```

(Note the third scene flipped too — the whiteboard member now hears the whole-day checkout
reason instead of the transfer window.) Line restored, re-run: 51 + 7 green. Committed.

## Red run 2 — M5 spec-first (Step 1 → Step 3 IS the red run, watched twice)

**Step 1** — `RuleNames.AdvanceWindow` added to `ExpectedRuleOrder` (between OpeningDays and
EquipmentTransfer) and the constant to `RuleNames`. `dotnet test` — exactly TWO tests fail; the
system now promises a rule it doesn't have:

```
[xUnit.net 00:00:00.25]     BookIt.Tests.FrontDeskSmokeTests.Rules_Resolve_In_Registration_Order_And_That_Order_Is_The_Spec [FAIL]
[xUnit.net 00:00:00.25]     BookIt.Tests.FrontDeskSmokeTests.Every_Expected_Rule_Resolves_From_The_Composition_Root [FAIL]
  Failed BookIt.Tests.FrontDeskSmokeTests.Rules_Resolve_In_Registration_Order_And_That_Order_Is_The_Spec [6 ms]
  Error Message:
   Assert.Equal() Failure: Collections differ
                                                         ↓ (pos 2)
Expected: string[]     ["MemberStanding", "OpeningDays", "AdvanceWindow", "EquipmentTransfer", "ResourceConflicts"]
Actual:   List<string> ["MemberStanding", "OpeningDays", "EquipmentTransfer", "ResourceConflicts"]
                                                         ↑ (pos 2)

  Failed BookIt.Tests.FrontDeskSmokeTests.Every_Expected_Rule_Resolves_From_The_Composition_Root [1 ms]
  Error Message:
   Assert.Contains() Failure: Item not found in collection
Collection: ["MemberStanding", "OpeningDays", "EquipmentTransfer", "ResourceConflicts"]
Not found:  "AdvanceWindow"

Failed!  - Failed:     2, Passed:    49, Skipped:     0, Total:    51, Duration: 28 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 21 ms - ClinicIt.Tests.dll (net10.0)
```

**Step 2** — `AdvanceWindowRule` + its two tests written. The rule passes its own tests; the
smoke net is STILL red — the class exists, compiles, passes, and the running system has never
heard of it:

```
$ dotnet test
Failed!  - Failed:     2, Passed:    51, Skipped:     0, Total:    53, Duration: 26 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 20 ms - ClinicIt.Tests.dll (net10.0)

$ dotnet test --filter "FullyQualifiedName~AdvanceWindowRuleTests"
Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 15 ms - BookIt.Tests.dll (net10.0)
```

**Step 3** — the one registration line
(`services.AddSingleton<IBookingRule>(_ => new AdvanceWindowRule(TimeProvider.System));`):

```
Passed!  - Failed:     0, Passed:    53, Skipped:     0, Total:    53, Duration: 22 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 19 ms - ClinicIt.Tests.dll (net10.0)
```

Then `RuleOrderTests` typed → 54 green. Committed and tagged.

## Payoff proof procedure (verbatim)

**1. History and tag tell the story:**

```
$ git log --oneline -5
c952fda ch22-M5: advance window - one class, one line
4491dcf ch22-M4: DI order + smoke net
cd85acf ch22-M3: rulebook extraction
17b2cd5 ch22-M2: transfer window - the method buckles
bc1146e ch22-M1: front desk rules, the obvious way

$ git tag --list 'bookit-ch2*'
bookit-ch20
bookit-ch21
bookit-ch22
```

**2. Whole suite green at the promised count:**

```
$ dotnet test
Passed!  - Failed:     0, Passed:    54, Skipped:     0, Total:    54, Duration: 24 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 108 ms - ClinicIt.Tests.dll (net10.0)
```

**3. The Open/Closed diff — the fifth rule cost one class, one name, one line:**

```
$ M4=$(git rev-list -1 --grep='^ch22-M4:' HEAD)
$ git diff --stat $M4..HEAD -- src/BookIt/FrontDesk
 src/BookIt/FrontDesk/AdvanceWindowRule.cs     | 29 +++++++++++++++++++++++++++
 src/BookIt/FrontDesk/FrontDeskModels.cs       |  1 +
 src/BookIt/FrontDesk/FrontDeskRegistration.cs |  1 +
 3 files changed, 31 insertions(+)
```

(The design's printed expectation listed two files; the third is the one-line `AdvanceWindow`
name constant in `RuleNames` — see Deviations #2. Exactly parallel to Ch. 20 M5's "one constant"
line item.)

**4. The unfakeable zero — nothing that worked was even opened:**

```
$ git diff --stat $M4..HEAD -- \
    src/BookIt/FrontDesk/BookingRulebook.cs \
    src/BookIt/FrontDesk/BookingDesk.cs \
    src/BookIt/FrontDesk/MemberStandingRule.cs \
    src/BookIt/FrontDesk/OpeningDaysRule.cs \
    src/BookIt/FrontDesk/EquipmentTransferRule.cs \
    src/BookIt/FrontDesk/ConflictRule.cs
(empty output — the proof)
```

**5. The engine was consumed, never modified:**

```
$ CH21=$(git rev-list -1 bookit-ch21)
$ git diff --stat $CH21..HEAD -- src/Scheduling.Conflicts
(empty output — the whole chapter composed the engine without opening it)
```

**6. Ignorance greps (M3's constraint, mechanically checked):**

```
$ grep -cE 'Invoice|Floor|OpeningDays|TimeSpan' src/BookIt/FrontDesk/BookingRulebook.cs src/BookIt/FrontDesk/BookingDesk.cs
src/BookIt/FrontDesk/BookingRulebook.cs:0
src/BookIt/FrontDesk/BookingDesk.cs:0
```

**7. Order is behavior, twice over:**

```
$ dotnet test --filter "FullyQualifiedName~Rules_Resolve_In_Registration_Order|FullyQualifiedName~Moving_Member_Standing"
Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2, Duration: 26 ms - BookIt.Tests.dll (net10.0)
```

(plus the M4 red-run transcript above: with ConflictRule registered first, exactly one test
failed — the SequenceEqual order test — and `dotnet run` told the unpaid member about a room
clash instead of his invoices.)

## Final `dotnet run --project src/BookIt` (verbatim)

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
```

## Deviations

1. **`ToOpeningDay` has 8 arms, not 7.** The design specifies "switch expression, 7 arms", but
   covering all seven named `DayOfWeek` values still triggers compiler warning CS8524 (unnamed
   enum values, e.g. `(DayOfWeek)7`). An eighth arm was added —
   `_ => throw new ArgumentOutOfRangeException(nameof(day))` — to keep every build warning-free,
   which the arc treats as non-negotiable. One candidate sentence for the chapter: the compiler
   knows an enum is just an int in a costume, and the discard arm is where you say so.
2. **Payoff proof 3 shows three files, not two.** The `AdvanceWindow` name constant lives in
   `RuleNames` (in `FrontDeskModels.cs`, per the design's own file layout) and is marked "// M5"
   in the design's code listing, so adding it in M5 Step 1 necessarily touches that file for one
   insertion. The design's expected `git diff --stat` output listed only `AdvanceWindowRule.cs`
   and `FrontDeskRegistration.cs`; the honest diff adds `FrontDeskModels.cs | 1 +`. This matches
   Ch. 20 M5's own cost accounting ("one new production file, one constant, one registration
   line") and does not weaken the claim: rulebook, desk, and all four existing rules show the
   unfakeable zero (proof 4).
3. **Engine conflict-reason wording inherited from ch21 execution, not ch20's exemplar text.**
   The ch21 build reworded strategy reasons to booking-time form (e.g. "Overlaps an existing
   booking (09:00–10:00)…" instead of "Overlaps booking #1…") because `Conflict.Existing` became
   `IBookingRecord`, which carries no host id. Ch22 consumes those reasons as data (via
   `ConflictRule`'s join); the desk-level test asserting on engine text pins "cleaning buffer",
   which is stable across both wordings. No ch22 file touched the engine (proof 5).

## Elaborations within the designer's mandate (not deviations, recorded for the writer)

- `RuleNames` planted in M1 with the four rules the forwarded email list names (M1–M4); the
  fifth name arrives with email #5 in M5 Step 1, mirroring Ch. 20's `ResourceTypes.Parking`.
- Reason strings: member standing mentions "unpaid invoices"; opening days names the closed
  weekday ("The space is closed on Sundays; this request touches 19/07/2026"); transfer names
  the 15 minutes and both floors; ConflictRule joins engine reasons with "; ".
- The M2 naive desk's inline transfer check is guarded by
  `Resource.Type == ResourceTypes.Equipment` (rooms don't travel), which M3 lifts verbatim into
  `EquipmentTransferRule` and test `Ignores_Resources_That_Do_Not_Travel` then pins.
- Demo scene members are Ana (paid up) and Rui (unpaid), consistent with the test suite.
