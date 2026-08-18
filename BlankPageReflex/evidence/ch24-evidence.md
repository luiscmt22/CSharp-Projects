# Chapter 24 — Implementation Evidence

Repo: `scratchpad/book/BookIt`. Baseline verified before starting: HEAD `8e7c51e`
(ch23-M5), tags `bookit-ch20..bookit-ch23` present, suite green
(`Passed: 74` BookIt.Tests + `Passed: 7` ClinicIt.Tests). The "Assumed upstream
surface" table in `ch24-design.md` was re-validated against the actual tree before
M1: every pinned seam matched (desk ctor 2 args, `BookingDecision` statics,
`BookingSubmission` shape, `Resource.Floor`/`Booking.Floor`, `IRefusalLog`,
`BookingDeskTests.CreateDesk()`).

## Milestones

| Milestone | Commit | Message | BookIt.Tests | ClinicIt.Tests | Total |
|---|---|---|---|---|---|
| M1 | `a44c408` | ch24-M1: three reactions, wired by hand | 79 | 7 | 86 |
| M2 | `8922833` | ch24-M2: static event - the shortcut that ships | 80 | 7 | 87 |
| M3 | `01a5f16` | ch24-M3: instance event + disposable subscribers | 84 | 7 | 91 |
| M4 | `00e719e` | ch24-M4: utilization stats - zero desk edits | 87 | 7 | 94 |

Tag `bookit-ch24` on `00e719e`. Every commit green; the only red bars are the two
scripted runs inside M3 (plus one unscripted M2 observation recorded below because
it is book material).

M1 tally facts (for the writers): desk ctor args 2 → 4; files edited just to
compile: 3 — `BookingDeskTests.CreateDesk()` body, `RefusalLogTests.DeskOver()`
body, `FrontDeskRegistration.AddFrontDesk()` (+2 registrations); per-test edits
the two planted factory seams absorbed: 9 tests construct desks through them;
test-intent edits: 0. M2 tally: ctor 4 → 2, both factory bodies slimmed back,
registrations removed — churn in both directions inside one hour, both times
factory bodies only.

## Unscripted red observation (M2, before the M2 commit)

The design calls M2 "deceptively green." On this machine it was better than that:
deceptively FLAKY. xUnit runs test collections in parallel by default, and under
the static event every live display hears every desk in the process — including
desks in other test classes running on other threads. First full-suite run after
wiring the static event:

```
[xUnit.net 00:00:00.30]     BookIt.Tests.FrontOfficeReactionTests.An_Accepted_Booking_Refreshes_The_Front_Desk_Display [FAIL]
Failed!  - Failed:     1, Passed:    79, Skipped:     0, Total:    80, Duration: 29 ms - BookIt.Tests.dll (net10.0)
```

The next run passed. Race, not determinism: another class's accepted `Submit`
landed inside this test's subscribe-to-assert window. Resolution recorded under
Deviations item 1.

## Scripted red run #1 — cross-talk under the static event (M3)

Command (design's exact filter), with production code still at the M2 commit shape:

```
$ dotnet test tests/BookIt.Tests --nologo --filter Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays

[xUnit.net 00:00:00.30]     BookIt.Tests.DeskIsolationTests.Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays [FAIL]
  Failed BookIt.Tests.DeskIsolationTests.Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays [6 ms]
  Error Message:
   Assert.Empty() Failure: Collection was not empty
Collection: ["EQ-PROJ-1 Mon 11:00-13:00"]
  Stack Trace:
     at BookIt.Tests.DeskIsolationTests.Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays() in ...\tests\BookIt.Tests\DeskIsolationTests.cs:line 33
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 14 ms - BookIt.Tests.dll (net10.0)
```

Desk B's booking line, read inside desk A's assertion failure — the corpus's
Blazor shared-state bug as a literal string in the terminal. Full-suite run at the
same point: exactly 1 failure, everything else green:

```
[xUnit.net 00:00:00.18]     BookIt.Tests.DeskIsolationTests.Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays [FAIL]
Failed!  - Failed:     1, Passed:    83, Skipped:     0, Total:    84, Duration: 86 ms - BookIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 105 ms - ClinicIt.Tests.dll (net10.0)
```

## Scripted red run #2 — the subscriber that outlives its owner (M3)

Instance event in place, both `Dispose` bodies deliberately empty. Command
(design's exact filter):

```
$ dotnet test tests/BookIt.Tests --nologo --filter A_Disposed_Display_Stops_Listening

[xUnit.net 00:00:00.27]     BookIt.Tests.DeskIsolationTests.A_Disposed_Display_Stops_Listening [FAIL]
  Failed BookIt.Tests.DeskIsolationTests.A_Disposed_Display_Stops_Listening [7 ms]
  Error Message:
   Assert.Empty() Failure: Collection was not empty
Collection: ["EQ-PROJ-1 Mon 11:00-13:00"]
  Stack Trace:
     at BookIt.Tests.DeskIsolationTests.A_Disposed_Display_Stops_Listening() in ...\tests\BookIt.Tests\DeskIsolationTests.cs:line 45
Failed!  - Failed:     1, Passed:     0, Skipped:     0, Total:     1, Duration: 16 ms - BookIt.Tests.dll (net10.0)
```

The disposed display's `Lines` contains the post-disposal booking. Full-suite run
at the same point — TWO failures, not the design's predicted one (see Deviations
item 3):

```
[xUnit.net 00:00:00.17]     BookIt.Tests.DeskIsolationTests.A_Disposed_Display_Stops_Listening [FAIL]
[xUnit.net 00:00:00.17]     BookIt.Tests.DeskIsolationTests.Disposing_One_Subscriber_Leaves_The_Others_Attached [FAIL]
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 19 ms - ClinicIt.Tests.dll (net10.0)
Failed!  - Failed:     2, Passed:    82, Skipped:     0, Total:    84, Duration: 85 ms - BookIt.Tests.dll (net10.0)
```

Fix: one `-=` line per subscriber. Suite green (84 + 7) and stable across six
consecutive re-runs.

## Payoff proof procedure (run from repo root, verbatim output)

**1. Suite green, full count.**

```
$ dotnet test --nologo
Passed!  - Failed:     0, Passed:     7, Skipped:     0, Total:     7, Duration: 93 ms - ClinicIt.Tests.dll (net10.0)
Passed!  - Failed:     0, Passed:    87, Skipped:     0, Total:    87, Duration: 28 ms - BookIt.Tests.dll (net10.0)
```

**2. The zero-diff certificate.**

```
$ git diff --stat HEAD~1..HEAD -- . ':!*/bin/*' ':!*/obj/*'
 src/BookIt/Notifications/UtilizationStats.cs | 32 ++++++++++++
 src/BookIt/Program.cs                        | 36 ++++++++++++++
 tests/BookIt.Tests/UtilizationStatsTests.cs  | 74 ++++++++++++++++++++++++++++
 3 files changed, 142 insertions(+)
```

Three files, all additions, none of them the desk or the existing subscribers.

**3. The explicit negative.**

```
$ git diff HEAD~1..HEAD --name-only -- \
    src/BookIt/FrontDesk/BookingDesk.cs \
    src/BookIt/Notifications/FrontDeskDisplay.cs \
    src/BookIt/Notifications/PorterDispatch.cs
(no output)
```

**4. The trap is dead.**

```
$ grep -n "static event" src/BookIt/FrontDesk/BookingDesk.cs
(no output; exit code 1)
```

**5. Tag and the arc-closing read.**

```
$ git tag bookit-ch24
$ git log --oneline bookit-ch20..bookit-ch24
00e719e ch24-M4: utilization stats - zero desk edits
01a5f16 ch24-M3: instance event + disposable subscribers
8922833 ch24-M2: static event - the shortcut that ships
a44c408 ch24-M1: three reactions, wired by hand
8e7c51e ch23-M5: parking occupancy — one section, one method
c2b5ac3 ch23-M4: content and presentation facets
c225040 ch23-M3: fluent builder, staged entry, immutable report
bfa4c76 ch23-M2: options via parameters — the telescoping tally
6e56330 ch23-M1: the Monday report, fixed menu
c952fda ch22-M5: advance window - one class, one line
4491dcf ch22-M4: DI order + smoke net
cd85acf ch22-M3: rulebook extraction
17b2cd5 ch22-M2: transfer window - the method buckles
bc1146e ch22-M1: front desk rules, the obvious way
092796f ch21-M5: two hosts, one engine — proof and tag
04812dd ch21-M4: clinic adapters — their models, our engine
2de6019 ch21-M3: extract Scheduling.Conflicts — the engine owns its contracts
6b09eac ch21-M2: tally the damage, back out the mapping layer
700d79d ch21-M1: ClinicIt on the mapping road — green, and wrong
0fb9260 ch21-M0: evict build artifacts; every proof in this chapter is a diff
```

## Final `dotnet run --project src/BookIt` (verbatim, complete)

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

Front desk — Ana, Atlas next Monday 09:00: ACCEPTED
Front desk — Ana, projector to floor 3 next Monday 11:00: ACCEPTED
Display: ROOM-ATLAS Mon 09:00-10:00
Display: EQ-PROJ-1 Mon 11:00-13:00
Porter:  move EQ-PROJ-1 floor 1 -> 3 by 11:00
Stats:   MeetingRoom 1h / 1 booking(s); Equipment 2h / 1 booking(s)
-- porter clocks out (Dispose) --
Front desk — Ana, whiteboard to floor 2 next Monday 14:00: ACCEPTED
Porter tasks pending: 1   <- the clocked-out porter heard nothing
```

All four mandatory beats present: display refresh, porter move task, live stats,
and disposal demonstrated live (the clocked-out porter's pending count stays at 1
while a third booking is accepted).

## Deviations

1. **M2 required pinning xUnit collection behavior to stay committably green.**
   The design's M2 assumes a deterministically green (if deceptive) bar. In
   reality, xUnit's default parallel test collections made the static-event state
   flaky: any test class's accepted `Submit` raises into every live subscriber in
   the process, and `An_Accepted_Booking_Refreshes_The_Front_Desk_Display` failed
   intermittently (capture above). Since every commit must leave the suite green,
   M2 adds `tests/BookIt.Tests/TestCollectionBehavior.cs` —
   `[assembly: CollectionBehavior(DisableTestParallelization = true)]` with a
   decision comment — and M3 DELETES it as part of the fix: the instance event
   makes cross-collection isolation structural again, so the throttle comes out
   with the trap. This is a gift to the manuscript, not a cost: "we had to turn
   off parallel tests to make the static event look green" is the production
   pathology, verbatim, and the writer should use it.
2. **`RefusalLogTests.DeskOver()` is the second factory body.** The design's M1
   file list names only `BookingDeskTests.CreateDesk()`, but its tally line says
   "two factory bodies + the registration method" — the second is
   `RefusalLogTests.DeskOver()`, a Ch. 22 planted seam the design's file list
   omitted. Edited body-only in M1 (grew two collaborators, fully-qualified so the
   edit stays literally inside the body) and M2 (slimmed back). Test intent
   untouched.
3. **Red #2's full-suite count is 2, not 1.** With BOTH `Dispose` bodies empty —
   which is the state the design's own order-if-you-freeze produces —
   `Disposing_One_Subscriber_Leaves_The_Others_Attached` also fails (the
   undisposed porter accumulates a task). The design's prescribed filtered command
   shows exactly the predicted single failure and message shape; only the
   whole-suite count differs. Arguably sharper for the book: each missing `-=`
   gets its own accuser.
4. **M3's listed `FrontOfficeReactionTests` factory-body edit was a no-op.** The
   M2 body already constructs the desk first and hands it to both subscribers
   (their ctors demanded it, ignored it); switching to the instance event required
   no textual change there — which is itself the seam's final receipt: the third
   rewiring cost zero edits. `DeskIsolationTests` was still the file that changed
   in M3, as designed.
5. **Demo booking ids differ from the design's sketch.** The design's expected
   scene shows `#7/#8/#9`; the desk's internal counter (new in ch24-M1, starting
   at 1) yields different numbers and the reused `ReportDecision` helper does not
   print ids. The design marks exact strings as the implementer's call and its
   four mandatory beats as the contract; all four are present.

## Independent verification pass (second implementer, 27/07/2026)

A second implementer re-ran every mechanical claim in this file from a clean clone
of the repo. Nothing was re-implemented; nothing needed correcting. Results:

- **Suite at HEAD (`00e719e`):** `Passed: 87` BookIt.Tests + `Passed: 7`
  ClinicIt.Tests. Matches.
- **Every ch24 commit checked out and tested in isolation** (clone, not the live
  tree): M1 `a44c408` → 79+7, M2 `8922833` → 80+7, M3 `01a5f16` → 84+7,
  M4 `00e719e` → 87+7. All green, all matching the milestone table above; the
  +5/+1/+4/+3 = +13 arithmetic on ch23's 74 baseline holds.
- **Red run #1 reproduced** by checking out M2's production code and copying M3's
  `DeskIsolationTests.cs` over it: identical failure — `Assert.Empty() Failure:
  Collection was not empty` / `Collection: ["EQ-PROJ-1 Mon 11:00-13:00"]` at
  `DeskIsolationTests.cs:line 33`, `Failed: 1, Passed: 0, Total: 1`. The captured
  message is real, not reconstructed.
- **Red run #2 reproduced** by emptying both `Dispose` bodies at M3: identical
  failure at `DeskIsolationTests.cs:line 45`, same collection contents. The
  full-suite count in that state is `Failed: 2, Passed: 82, Total: 84` with
  `A_Disposed_Display_Stops_Listening` and
  `Disposing_One_Subscriber_Leaves_The_Others_Attached` failing — Deviations
  item 3 confirmed exactly as written.
- **Payoff proofs 2–5 re-run verbatim on the live tree:** the three-file/142-insertion
  stat, the silent explicit negative, `grep -n "static event"` exiting 1, tag
  `bookit-ch24` on `00e719e`, and the 20-commit arc log. All identical.
- **`dotnet run --project src/BookIt`** re-run: output identical to the transcript
  above, including the closing `Porter tasks pending: 1` disposal beat.
- **Design conformance spot-checks:** M2 genuinely shipped
  `public static event EventHandler<BookingConfirmedEventArgs>? BookingConfirmed;`
  (line 9 of the desk at `8922833`); M3's commit deletes `TestCollectionBehavior.cs`
  and does NOT touch `FrontOfficeReactionTests.cs` (Deviations 1 and 4 confirmed
  from the commit stats); all 13 checkpoint tests carry the exact names the design
  specifies; the contract-required `EventHandler<T>` justification comment is
  present on `BookingDesk.BookingConfirmed`; `UtilizationStats` uses
  `StringComparer.OrdinalIgnoreCase` as mandated.

Repo hygiene note for whoever closes the arc: four scratch worktrees from the
Ch. 21–22 implementations are still registered (`git worktree list` shows
`ch21check`, `ch21m4`, `ch22m4`, `ch22verify` under `%TEMP%`). They are detached-HEAD
scratch checkouts, invisible to the manuscript's `git log`, and were left in place
rather than removed on someone else's behalf.
