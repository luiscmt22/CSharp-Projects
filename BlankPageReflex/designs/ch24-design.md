# Design — Chapter 24

**Title (fixed by contract):** Observer: Three Places Hear One Booking
**Arc position:** closes Part III's BookIt build arc (Ch. 20–24). Tag at end: `bookit-ch24`.
**Chapter 8 owns delegate/event mechanics.** This chapter contains zero `+=` tutorials — one
pointer back to Ch. 8, then it spends all its pages on the architectural decision (when events
beat direct calls) and the operational failure modes (shared static state, subscribers that
outlive their owners).

**Repo ground truth at design time (verified, not assumed):**
- `scratchpad/book/BookIt` is at `bookit-ch20` (commit `1e81147`), 5 commits `M1`–`M5`,
  26 tests green (`dotnet test`: Passed 26, ~34 ms).
- Chapters 21–23 are NOT yet in the repo. No `src/Scheduling.Conflicts`, no `ClinicIt`,
  no `BookingDesk`, no `IBookingRule`, no `IRefusalLog`, no `WeeklyReport`.
- `bin/` and `obj/` are **tracked** in git (no `.gitignore` in the index), and the solution
  file is `BookIt.slnx`. Both facts affect the diff-based payoff proof; see Deviations.

This design therefore targets the **post-Ch. 23 state as fixed by the ch21–ch23 design docs**
(reconciled against them by the arc continuity gate, 27/07/2026) and pins every seam it
depends on in "Assumed upstream surface" below. Where an assumption could still drift in
execution, the design absorbs the drift inside Ch. 24's own types (event args carry whole
domain models, so only subscriber internals move if Ch. 22's execution shapes things
differently).

---

## Assumed upstream surface (Ch. 21–23 outputs this chapter builds on)

Binding on the Ch. 21–23 implementers only insofar as the arc contract already binds them;
listed here so any mismatch is caught at Ch. 24 implementation time instead of silently drifting.

| Seam | Pinned shape (from the ch22/ch23 design docs) | Ch. 24 actually needs |
|---|---|---|
| `BookingDesk` (Ch. 22) | `src/BookIt/FrontDesk/BookingDesk.cs`, `namespace BookIt.FrontDesk`; ctor `(BookingRulebook rulebook, IRefusalLog refusalLog)`; `BookingDecision Submit(BookingSubmission submission)`. **Ch. 22's desk does NOT store bookings or assign ids — ch24-M1 adds that** (internal id counter; accepted path materializes the `Booking` from the submission) as part of its own desk edit | A single `Submit` that either refuses or materializes a confirmed `Booking`; ctor with 2 existing deps (the bloat tally counts from here) |
| `BookingDecision` (Ch. 22) | `sealed record BookingDecision(bool Accepted, string? RefusedBy, string? Reason)` with statics `Accept()` / `Refuse(ruleName, reason)`. **Ch24-M1 widens it** with a trailing `Booking? ConfirmedBooking = null` positional member — trailing default, so zero churn in ch22's tests, and the chapter says so | Distinguish accepted/refused; reach the confirmed `Booking` |
| `BookingSubmission` (Ch. 22) | `(Member Member, Resource Resource, BookingRequest Request, int Floor, IReadOnlyList<Booking> ExistingBookings)` — `Floor` is the booking's target floor | `Resource` + the target floor for equipment |
| Floors (Ch. 22, rule 3) | `Resource.Floor int = 0` (home floor); `Booking.Floor int = 0` (where the booking puts it); the desk copies `submission.Floor` onto the materialized `Booking` | From-floor (`Resource.Floor`) and to-floor (`Booking.Floor`) for the porter |
| `IRefusalLog` (Ch. 22) | in-memory Singleton, `Record(Refusal)` + `All` | Only narratively (refusals are the log's business, not the stats') |
| Ch. 23 | `WeeklyReport` built from bookings + refusal log | Only narratively: `UtilizationStats` accumulates live the same numbers the Monday report re-derives weekly |
| Test helpers (Ch. 22 tests) | `BookingDeskTests.CreateDesk()` — the single construction point ch22 planted | Reused/mirrored by Ch. 24 test files — and it is WHY ch24's construction churn lands in factory bodies, not test bodies (see M1's tally) |
| Suite baseline (post-ch23) | BookIt.Tests **74** + ClinicIt.Tests **7** | The +13 arithmetic below sits on 74 |

Any mismatch found against the actual `bookit-ch23` tag at implementation time is upstream
execution drift — fix it upstream, don't absorb it silently here.

---

## Milestones (4)

Commit prefix `ch24-Mk:` per contract; every commit green except inside the two scripted red
runs (both inside M3). All new test files follow Chapter 20's house style: underscored
sentence names, one factory per file as the single construction point, `Monday = new(2026, 7, 13)`.

### M1 — Three emails, two wires (the coupled version)

**Commit:** `ch24-M1: three reactions, wired by hand`

**Story.** The owner sends three emails: (1) the lobby display must show a booking the moment
the desk accepts it; (2) the porter needs a move task when travelling equipment is confirmed on
a floor it doesn't live on; (3) she wants utilization numbers accumulating live — "the same ones
your Monday report keeps re-computing." The reader wires the first two the obvious way: two new
collaborator classes, two new constructor parameters on `BookingDesk`, two direct calls at the
end of `Submit`'s accepted path. The third email is priced but NOT implemented — that pricing is
the milestone's closing beat (see Felt-pain).

**Files:**
- added `src/BookIt/Notifications/FrontDeskDisplay.cs` (direct-call shape: public `ShowConfirmed`)
- added `src/BookIt/Notifications/PorterDispatch.cs` (+ `MoveTask` record in the same file)
- changed `src/BookIt/FrontDesk/BookingDesk.cs` — TWO edits, both tallied: (1) the accepted
  path now materializes the `Booking` (internal id counter, floor from the submission) and
  returns it via `BookingDecision.ConfirmedBooking` (trailing-default widening of the record —
  zero ch22 test churn, named as ch22-M2's own trailing-default lesson paying out); (2) ctor
  2 → 4 args, two direct calls after the booking is materialized
- changed `src/BookIt/FrontDesk/FrontDeskModels.cs` — `BookingDecision` gains the trailing
  `Booking? ConfirmedBooking = null`
- changed `src/BookIt/FrontDesk/FrontDeskRegistration.cs` — +2 registrations
  (`FrontDeskDisplay`, `PorterDispatch`) so ch22's `The_Desk_Itself_Resolves` smoke test stays
  green: the composition root pays for the coupling too, and that is a tally line
- added `tests/BookIt.Tests/FrontOfficeReactionTests.cs`
- changed ch22's `BookingDeskTests.cs` — ONLY `CreateDesk()`'s factory body: ch22's planted
  seam absorbs what would have been ~7 per-test construction edits, and the chapter names the
  tally that DIDN'T happen (see Felt-pain beat 1)
- `Program.cs` NOT touched (its one scene is saved for M4)

**Checkpoint tests** (in `FrontOfficeReactionTests.cs`; all constructed through one factory —
see "the seam" note below):
1. `An_Accepted_Booking_Refreshes_The_Front_Desk_Display` — accepted submission ⇒ one new display line naming the resource and slot.
2. `A_Refused_Booking_Leaves_The_Display_Untouched` — refusal (unpaid member, Ch. 22 rule 1) ⇒ `Assert.Empty(display.Lines)`.
3. `Equipment_Confirmed_On_Another_Floor_Creates_A_Porter_Move_Task` — home floor 1, booking floor 3 ⇒ one `MoveTask(from:1, to:3, neededBy: booking.Start)`.
4. `Equipment_Confirmed_On_Its_Home_Floor_Dispatches_No_Porter` — same floor ⇒ no task.
5. `A_Room_Booking_Never_Dispatches_A_Porter` — rooms are bolted to their floor ⇒ no task.

**The seam (Chapter 20's `CreateChecker`, replayed at desk scale).** The test file's only
construction point:

```csharp
private static (BookingDesk Desk, FrontDeskDisplay Display, PorterDispatch Porter) CreateFrontOffice()
```

All five tests assert on **observable subscriber state** (`Lines`, `PendingTasks`) — never on
calls. That decision is planted here and pays out twice: M2 and M3 each rewire construction, and
the only edit this file ever takes is this factory's body. The chapter says so out loud, the way
Chapter 20 did for `CreateChecker`.

**Your-turn box (note form):**
- *Goal:* make the 5 checkpoint tests pass the most obvious way — two collaborator classes, two ctor params, two direct calls on the accepted path only, after the booking is stored.
- *Constraints:* no events, no interfaces — you ship at 17:30 again. Reactions never fire on refusals. `CreateFrontOffice()` is the single construction point. Keep two paper tallies: (a) every FILE edited just to compile (factory body, registration method) — plus, in a second column, the ~7 per-test edits ch22's `CreateDesk()` seam absorbed; (b) `BookingDesk` ctor arg count before/after.
- *Order-if-you-freeze:* the `ConfirmedBooking` widening + id counter first (test 1 needs a materialized booking to display); `MoveTask` record next; `FrontDeskDisplay` is a `List<string>` and one method; the porter check is one `if` on two floors; desk ctor + two calls + registrations last; run per-test.
- *Done when:* 5 new tests green, whole suite green, both tallies written down, committed.

**Red run:** none in M1.

### M2 — The static shortcut (the "fix" that ships the bug)

**Commit:** `ch24-M2: static event - the shortcut that ships`

**Story.** A colleague "cleans up" the coupling: deletes the two ctor params, puts
`public static event EventHandler<BookingConfirmedEventArgs>? BookingConfirmed` on
`BookingDesk`, raises it in `Submit`, and has the subscribers attach themselves in their
constructors — constructors that still take a `BookingDesk` **and ignore it**, which is exactly
how the production version of this bug reads. The tally goes DOWN (ctor 4 → 2, tests slim
again), which is why this diff gets approved in real codebases. The reader ships it, green,
and commits — then the chapter tells them what they just shipped. Smells are noted on paper,
not fixed: the decorative ctor param; `Dispose` mutating a global; the question "which desk
said that?" having no answer.

**Files:**
- added `src/BookIt/Notifications/BookingConfirmedEventArgs.cs`
- changed `src/BookIt/FrontDesk/BookingDesk.cs` (drop 2 ctor params; static event; raise after the booking is materialized)
- changed `src/BookIt/FrontDesk/FrontDeskRegistration.cs` — the M1 subscriber registrations come OUT again (the composition root slims down too: part of why this diff reviews so well)
- changed `FrontDeskDisplay.cs`, `PorterDispatch.cs` (ctor takes-and-ignores desk; static subscribe; `Dispose` detaches the static event; public reaction methods become private handlers)
- changed `tests/BookIt.Tests/FrontOfficeReactionTests.cs` — `CreateFrontOffice()` body only
- changed ch22's `BookingDeskTests.cs` — `CreateDesk()`'s body slims back down (tally line: churn in BOTH directions inside one hour, both times exactly one factory body — the seam again)

**Checkpoint test (1 new, same file):**
6. `A_Desk_With_No_Listeners_Still_Accepts_Bookings` — no subscriber constructed; `Submit` accepts without throwing. (Trivially true under `static`; earns its keep in M3 pinning the null-conditional raise.)

**Your-turn box:**
- *Goal:* perform the colleague's cleanup faithfully: static event + self-subscribing constructors + `Dispose` that detaches. Adapt tests through `CreateFrontOffice()` only; add test 6.
- *Constraints:* do NOT fix what feels wrong — shipping the smell is the exercise. Subscribers' ctors must accept a `BookingDesk` and not use it. Write the three smells on the tally sheet instead of in code.
- *Order-if-you-freeze:* `BookingConfirmedEventArgs` first; event + raise; subscriber ctors + handlers; `Dispose`; factory body last.
- *Done when:* whole suite green (6 new so far), smells written down, committed — "you have just shipped the bug this chapter exists to teach."

**Red run:** none scripted (the suite is deceptively green — that IS the point, and the prose
says so: nothing on the bar distinguishes M2 from correct code; M3 manufactures the evidence).

### M3 — Two desks, one wire: condemn, then fix

**Commit:** `ch24-M3: instance event + disposable subscribers`

**Story.** The reader plays two Blazor circuits / two tenants inside xUnit: two desks, each with
its own display. Under the static event, desk B's booking appears on desk A's display — the
cross-talk test is typed FIRST and runs RED (scripted red run #1; contract's designer-pick:
invocation-count cross-talk over `WeakReference`, because it's deterministic — no GC
nondeterminism — and it reproduces the corpus's Blazor shared-state bug literally). The fix is
the extraction: the event moves from `static` to instance, subscribers attach to the desk they
are handed, and `IDisposable.Dispose` detaches. Scripted red run #2: leave `Dispose` empty,
watch the disposed-display test fail, then write the `-=` — the unsubscribe discipline enters
as a failing test, not a homily.

**Files:**
- added `tests/BookIt.Tests/DeskIsolationTests.cs` (constructs desk+subscriber pairs explicitly — pairing is the subject, so this file does NOT use `CreateFrontOffice`; it has its own `CreateDesk()` helper building an always-accepting desk, reusing Ch. 22's test fixtures)
- changed `src/BookIt/FrontDesk/BookingDesk.cs` (static → instance event; private `OnBookingConfirmed` raiser; the contract-required one-comment justification of `EventHandler<T>` — see type signatures)
- changed `FrontDeskDisplay.cs`, `PorterDispatch.cs` (ctor param now used; instance subscribe; `Dispose` detaches from the stored desk)
- changed `FrontOfficeReactionTests.cs` — factory body only, third and final time

**Checkpoint tests (4 new, in `DeskIsolationTests.cs`):**
7. `Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays` — desk A + display A, desk B + display B; accepted submit to B; `Assert.Empty(displayA.Lines)`. **Typed first; RED under M2.**
8. `A_Disposed_Display_Stops_Listening` — subscribe, dispose, submit; `Assert.Empty(display.Lines)`. **RED while `Dispose` is empty (scripted red #2).**
9. `Disposing_One_Subscriber_Leaves_The_Others_Attached` — dispose the porter; the display still hears.
10. `A_Late_Subscriber_Hears_Only_Bookings_After_It_Joined` — submit, then attach display, submit again ⇒ exactly one line. Pins no-replay semantics and sets up the M4/Ch. 23 contrast: reports re-derive history; events only see the future.

**Deliberate red runs — what exactly fails:**
- *Red #1 (after typing test 7, before touching production code):* exactly 1 failure.
  `Assert.Empty() Failure … Collection: ["EQ-PROJ-1 Mon 11:00-13:00 …"]` — the reader literally
  reads the OTHER desk's booking line inside their own assertion failure. Everything else green.
- *Red #2 (instance event in place, `Dispose` bodies still empty):* exactly 1 failure —
  `A_Disposed_Display_Stops_Listening`, same `Assert.Empty` shape: the disposed display's
  `Lines` contains the post-disposal booking. Fix is one `-=` line per subscriber.

**Your-turn box:**
- *Goal:* type test 7, watch it condemn M2, then perform the fix: instance `BookingConfirmed` on `BookingDesk`, subscribers that use their ctor param, `IDisposable` that detaches. Then red #2 and the remaining green checkpoints.
- *Constraints:* zero assertion edits in `FrontOfficeReactionTests.cs` — factory body only (say why that proves behavior held, as M3 of Ch. 20 did). Raise must be null-conditional (`?.Invoke`); test 6 pins it. The word `static` must not survive anywhere near the event — grep the desk file and mean it. The event carries `(sender, args)`; the one-comment justification for `EventHandler<T>` is mandatory.
- *Order-if-you-freeze:* red test first, run, read the failure; move the event to instance + raiser method; display ctor `+=`; run (test 7 green, 8 red-by-design next); `Dispose` with `-=`; porter same; tests 9–10 last.
- *Done when:* both scripted reds observed and written in the log-margin; whole suite green (10 new so far); `grep -n "static event" src/BookIt/FrontDesk/BookingDesk.cs` returns nothing; committed.

### M4 — The third subscriber: stats land without touching the desk

**Commit:** `ch24-M4: utilization stats - zero desk edits`

**Story.** The third email finally gets implemented — through the front door this time.
`UtilizationStats` self-subscribes like its siblings and accumulates hours + counts per resource
type, live: the same numbers the Ch. 23 Monday report re-derives by querying, now maintained by
listening. The M1 tally sheet's projected column ("+1 ctor arg, another round of factory-body
rewrites, one desk reopen") is set against the actual invoice: one new class, one new test file, one Program scene, zero
edits to `BookingDesk`, `FrontDeskDisplay`, or `PorterDispatch` — proved by diff, and the
constraint box forbids even opening those files (Chapter 20 M5's move, replayed). Closing beats:
(a) scale the shape honestly — same idea process-wide is an event bus, cross-process is SignalR;
the "In the wild" sections carry it (the platform's SignalR notification system; the static-event
bug that leaked state across circuits); (b) the ARC closes — `git log --oneline` read end to end
as the five-chapter story, and the bridge to Part IV (reading production systems this size).

**Files:**
- added `src/BookIt/Notifications/UtilizationStats.cs`
- added `tests/BookIt.Tests/UtilizationStatsTests.cs`
- changed `src/BookIt/Program.cs` — the installment's one scene (see Payoff proof for output)
- tag `bookit-ch24` after the commit

**Checkpoint tests (3 new):**
11. `Confirmed_Bookings_Accumulate_Hours_By_Resource_Type` — a 1h room + a 2h equipment booking ⇒ `TotalBookedFor(MeetingRoom) == 1h`, `TotalBookedFor(Equipment) == 2h`, counts 1 and 1.
12. `Refused_Bookings_Leave_The_Stats_Untouched` — refusal ⇒ zero everywhere (refusals are the refusal log's business, Ch. 22).
13. `A_Disposed_Stats_Collector_Stops_Accumulating` — dispose, submit ⇒ totals frozen; the discipline test 8 established, applied by reflex to a class the reader designs alone.

**Your-turn box:**
- *Goal:* third subscriber end-to-end: class + 3 tests + one Program scene, then the proof and the arc close.
- *Constraints:* you may not open `BookingDesk.cs`, `FrontDeskDisplay.cs`, or `PorterDispatch.cs` — not even to look; the diff will certify you didn't need to. Stats hear only confirmations. Dictionary keyed by resource type, `StringComparer.OrdinalIgnoreCase` (the Ch. 20 comparer lesson, applied unprompted).
- *Order-if-you-freeze:* tests first — they fix the public API (`TotalBookedFor`/`ConfirmedCountFor`); the handler is three lines (lookup, add duration, bump count); `Dispose` is the same `-=` you've now written twice; Program scene last.
- *Done when:* suite green (13 new across the chapter); the two payoff diffs below say zero; tagged `bookit-ch24`; the log read aloud.

**Red run:** none scripted (M3 carries both; a third would pad the budget).

---

## New/changed domain types (exact C# signatures)

All Ch. 24 types live in `namespace BookIt.Notifications` except the `BookingDesk`/
`BookingDecision` changes (`namespace BookIt.FrontDesk`, ch22's layout). No changes to
`BookIt.Domain` models and no changes to the `Scheduling.Conflicts` engine — Ch. 24 touches
the front desk and adds listeners.

```csharp
// src/BookIt/FrontDesk/FrontDeskModels.cs — ch24-M1's one-line widening of ch22's record
// (trailing default: Accept()/Refuse() and every ch22 call site keep compiling unchanged)
public sealed record BookingDecision(
    bool Accepted, string? RefusedBy, string? Reason, Booking? ConfirmedBooking = null);

// src/BookIt/Notifications/BookingConfirmedEventArgs.cs  (new in M2, survives unchanged;
// classic ctor + get-only properties — the arc's Ch. 20 syntax palette, no primary ctors)
public sealed class BookingConfirmedEventArgs : EventArgs
{
    public BookingConfirmedEventArgs(Resource resource, Booking booking)
    {
        Resource = resource;
        Booking = booking;
    }

    public Resource Resource { get; }
    public Booking Booking { get; }
}
```

Event args deliberately carry the whole `Resource` and `Booking` rather than plucked fields:
subscribers decide what matters to them, and upstream model drift (see Assumed upstream surface)
stays inside subscriber bodies.

```csharp
// src/BookIt/FrontDesk/BookingDesk.cs  (changed; final M3+ shape)
public sealed class BookingDesk
{
    // EventHandler<T> over Action<T>: subscribers get the sender, and the isolation
    // test needs to know WHICH desk spoke; EventArgs keeps the payload growable
    // without breaking every handler.                       [contract-required comment]
    public event EventHandler<BookingConfirmedEventArgs>? BookingConfirmed;

    public BookingDesk(BookingRulebook rulebook, IRefusalLog refusalLog);   // back to Ch. 22's shape

    public BookingDecision Submit(BookingSubmission submission);            // signature unchanged

    private void OnBookingConfirmed(Resource resource, Booking booking) =>
        BookingConfirmed?.Invoke(this, new BookingConfirmedEventArgs(resource, booking));
}
```

Raise ordering is a stated decision, one sentence in prose: `OnBookingConfirmed` runs **after**
the booking is stored — observers must never hear about a booking that then fails to save.

Transitional shapes the narrative needs (recorded so the implementer builds them, then deletes them):

```csharp
// M1 only — the bloat IS the lesson:
public BookingDesk(BookingRulebook rulebook, IRefusalLog refusalLog,
    FrontDeskDisplay display, PorterDispatch porter);

// M1 only — direct-call reaction API (becomes a private handler in M2):
public void ShowConfirmed(Resource resource, Booking booking);      // on FrontDeskDisplay
public void PlanMoveIfNeeded(Resource resource, Booking booking);   // on PorterDispatch

// M2 only — the trap, typed and shipped, then deleted in M3:
public static event EventHandler<BookingConfirmedEventArgs>? BookingConfirmed;
```

Final subscriber shapes (M3+; M4 for stats):

```csharp
// src/BookIt/Notifications/FrontDeskDisplay.cs
public sealed class FrontDeskDisplay : IDisposable
{
    public FrontDeskDisplay(BookingDesk desk);          // subscribes in ctor
    public IReadOnlyList<string> Lines { get; }         // what the lobby screen shows
    public void Dispose();                              // -= ; the discipline test 8 pins
    private void Handle(object? sender, BookingConfirmedEventArgs e);
}

// src/BookIt/Notifications/PorterDispatch.cs
public sealed record MoveTask(string ResourceId, int FromFloor, int ToFloor, DateTime NeededBy);

public sealed class PorterDispatch : IDisposable
{
    public PorterDispatch(BookingDesk desk);
    public IReadOnlyList<MoveTask> PendingTasks { get; }
    public void Dispose();
    private void Handle(object? sender, BookingConfirmedEventArgs e);
    // task iff e.Resource is Equipment AND booking floor != resource home floor;
    // FromFloor = e.Resource.Floor, ToFloor = booking floor, NeededBy = e.Booking.Start
}

// src/BookIt/Notifications/UtilizationStats.cs  (M4)
public sealed class UtilizationStats : IDisposable
{
    public UtilizationStats(BookingDesk desk);
    public TimeSpan TotalBookedFor(string resourceType);
    public int ConfirmedCountFor(string resourceType);
    public void Dispose();
    private void Handle(object? sender, BookingConfirmedEventArgs e);
}
```

DI note (one paragraph in the chapter, no code): subscribers are deliberately NOT container-
registered. They are owned, disposable listeners — the Blazor-shaped lifetime (a per-circuit
component subscribing to a scoped service) — and a Singleton subscriber on a Scoped publisher
is the lifetime mismatch that static events fake away. The desk keeps whatever registration
Ch. 22 gave it; `Program.cs` constructs subscribers around the resolved desk in its scene.

---

## Payoff proof procedure (exact commands and expected outputs)

Run from the repo root. Pathspec excludes make the proofs immune to the currently-tracked
`bin/`/`obj/` noise (see Deviations item 4); if hygiene is fixed in Ch. 21 the excludes are
harmless no-ops.

**1. Suite green, full count.**
```bash
dotnet test --nologo
# expected, two result lines (ch23 pins the baseline at 74 + 7; +13 is this chapter):
#   Passed! - Failed: 0, Passed: 87, Skipped: 0, Total: 87 … BookIt.Tests.dll
#   Passed! - Failed: 0, Passed:  7, Skipped: 0, Total:  7 … ClinicIt.Tests.dll
```

**2. The zero-diff certificate (the unfakeable check).** M4 is exactly one commit after M3:
```bash
git diff --stat HEAD~1..HEAD -- . ':!*/bin/*' ':!*/obj/*'
# expected — three files, all additions, none of them the desk or the existing subscribers:
#  src/BookIt/Notifications/UtilizationStats.cs   | ~45 +
#  src/BookIt/Program.cs                          | ~18 +
#  tests/BookIt.Tests/UtilizationStatsTests.cs    | ~75 +
#  3 files changed, ~138 insertions(+)
```

**3. The explicit negative (reader types the filenames, gets silence):**
```bash
git diff HEAD~1..HEAD --name-only -- \
  src/BookIt/FrontDesk/BookingDesk.cs \
  src/BookIt/Notifications/FrontDeskDisplay.cs \
  src/BookIt/Notifications/PorterDispatch.cs
# expected: no output at all
# (path discipline matters here: a typoed path also prints nothing — the reader is told to
#  cross-check the three filenames against step 2's --stat listing before trusting silence)
```

**4. The trap is dead:**
```bash
grep -n "static event" src/BookIt/FrontDesk/BookingDesk.cs
# expected: no output (exit code 1)
```

**5. Tag and the arc-closing read:**
```bash
git tag bookit-ch24
git log --oneline bookit-ch20..bookit-ch24
# expected: the ch21-M0 … ch24-M4 commits, newest first — the Part III story in one screen;
# the chapter walks it oldest-first as the closing beat.
```

**6. The living scene:**
```bash
dotnet run --project src/BookIt
# expected shape of the Ch. 24 scene (exact strings are the implementer's call;
# these four beats are not optional):
#   Accepted: #7 ROOM-ATLAS Mon 09:00-10:00
#   Display:  ROOM-ATLAS Mon 09:00-10:00
#   Accepted: #8 EQ-PROJ-1 Mon 11:00-13:00 (floor 3)
#   Porter:   move EQ-PROJ-1 floor 1 -> 3 by 11:00
#   Stats:    MeetingRoom 1h / 1 booking; Equipment 2h / 1 booking
#   -- porter clocks out (Dispose) --
#   Accepted: #9 EQ-WB-1 Mon 14:00-15:00 (floor 2)
#   Porter tasks pending: 1        <- the clocked-out porter heard nothing
# The last line is disposal demonstrated live, not just in a test.
```

**Mid-chapter red-run proofs** (scripted in M3, outputs the manuscript reprints):
```bash
dotnet test --filter Two_Desks_Do_Not_Broadcast_Into_Each_Others_Displays
# under M2's static event: Failed: 1 —
#   Assert.Empty() Failure … Collection: ["EQ-PROJ-1 Mon 11:00-13:00 …"]
#   (the reader reads the OTHER desk's booking inside their own failure message)
dotnet test --filter A_Disposed_Display_Stops_Listening
# with empty Dispose bodies: Failed: 1 — same Assert.Empty shape, post-disposal line present.
```

---

## Felt-pain narrative beats (what the reader suffers, and the paper tallies)

The tally sheet is one page with two columns — **paid** and **projected** — and it is the
chapter's connective tissue; every milestone writes on it.

1. **M1 — the same tally, third time (with the seam's discount showing).** Wiring two reactions
   would have edited every Ch. 22 desk test — the planted `CreateDesk()` seam absorbs all of it
   into one factory body. Tally lines: desk ctor args 2 → 4; files edited just to compile: 3
   (two factory bodies + the registration method); per-test edits the seam absorbed: ~7, written
   in a second column as the invoice the seam paid; edits that changed test *intent*: zero —
   written in large digits. The chapter names the echo: this is Chapter 20 M2's signature-churn
   tally and Chapter 22's setup-bloat tally, happening again — cheaper only because a seam was
   planted — which is how the reader learns the smell has a *shape* independent of the pattern
   that fixes it.
2. **M1 close — pricing the third email.** The stats request is read, costed, and refused:
   projected column gets "+1 ctor arg, a third rewrite of both factory bodies plus the
   registration method, reopen the desk between two working collaborators — and ~8 per-test
   edits in any suite without the seam." The reader has learned to invoice a change before
   making it. The sheet stays on the desk; M4 settles it.
3. **M2 — the fix that reviews well.** The static event makes both tallies go DOWN (ctor
   4 → 2; tests slim again). Honest admission in prose: this is why the bug ships — the diff
   reads as a cleanup, the suite is green, and nothing on the bar distinguishes it from correct
   code. Three smells go on paper instead of in code: a constructor parameter nobody reads;
   a `Dispose` that mutates a global; "which desk said that?" having no answer in the type system.
4. **M3 — reading someone else's screen.** Two desks stand in for two Blazor circuits / two
   tenants. The red run's failure message contains desk B's booking line inside desk A's
   assertion — the reader experiences the corpus bug (the care-workforce platform's front-desk
   display showing another circuit's state) as a literal string in their own terminal.
   In-the-wild #1 grounds it: `_Curated\CORE\frontend\blazor-static-events-shared-state-bug.md`.
5. **M3 — the subscriber that outlives its owner.** A disposed display still accumulating lines
   is the closed circuit that keeps handling events forever: memory that can't be collected,
   ghost work on every booking. The discipline arrives as red test 8 plus the `-=` that turns
   it green — a test, not a homily (contract's phrasing, honored literally).
6. **M4 — the invoice settled.** Projected column from beat 2 sits next to the actual: zero
   desk edits, zero subscriber edits, one new file — certified by the diff the reader runs.
   The two columns side by side are the Observer pattern's receipt, and the last thing the
   tally sheet ever records.

---

## Reader time budget (total 75–85 min, inside the 60–90 contract window)

| Milestone | Budget | Notes |
|---|---|---|
| M1 — three reactions, wired by hand | 25 min | 2 small classes, ctor churn, 5 tests; the churn is typing, not thinking |
| M2 — the static shortcut | 12 min | mostly moves; 1 new test; factory-body edit |
| M3 — condemn, then fix | 25 min | 2 scripted reds + extraction + 4 tests; the chapter's core |
| M4 — stats + proof + arc close | 13 min | 1 class, 3 tests, 1 scene, proof commands |
| Arc-closing read (git log, Part IV bridge) | 5 min | reading, not typing |

First-run stretch toward two hours is called out as calibration, per Chapter 20's precedent.
Practice rules (no AI, type everything, 10-minute timer) restated in the drills header as always.
Drills: 7 fresh (Ch. 24's INDEX slot is "planned" per contract), Chapter 20's exact format,
final drill "The Confirmed-Booking Event, From Memory" — instance event, two disposable
subscribers, null-conditional raise, from a blank file, tomorrow.

---

## Deviations from contract

1. **Pain phase grows two direct dependencies, not three.** The contract's felt-pain sketch has
   `BookingDesk` grow "three direct dependencies"; this design wires the display and porter
   directly and defers `UtilizationStats` to M4. Reason: the contract's own payoff requires
   UtilizationStats to land as the *third subscriber* with a zero-change diff against the desk
   and "both existing subscribers" — impossible if stats already existed pre-extraction, short
   of a build-delete-rebuild detour the 60–90 min budget can't fund. The pain quantum is
   preserved: the ctor still bloats to four collaborators (Ch. 22's two + the new two), and the
   third dependency's cost is *priced on the tally sheet* rather than paid (beat 2), which
   sharpens rather than weakens the M4 payoff.
2. **Designing ahead of an unimplemented Ch. 21–23.** The contract binds this design to the
   post-Ch. 23 repo; at design time the repo is verifiably at `bookit-ch20` (26 green) with
   none of Ch. 21–23 present. Recorded per the contract's no-silent-drift rule; mitigated by
   the "Assumed upstream surface" table (exact seams, with fallbacks) and by event args that
   carry whole domain models so upstream shape drift stays inside subscriber bodies. The Ch. 24
   implementer must re-validate that table against the actual post-ch23 tree before starting.
3. **Not a deviation, but a delegated pick recorded:** the contract offers a WeakReference-based
   leak test OR an invocation-count cross-talk test and asks the designer to choose. Chosen:
   cross-talk (test 7), red-first — deterministic (no GC nondeterminism inside xUnit), and it
   reproduces the corpus's Blazor shared-state bug verbatim. The leak is still demonstrated,
   via the disposed-subscriber-still-hears variant (test 8, also run red). Likewise the
   contract's `EventHandler<T>` vs `Action<…>` choice: `EventHandler<T>`, justified in the
   mandated one-line comment (the sender identity is load-bearing for test 7).
4. **Repo hygiene drift inherited from Ch. 20's implementation.** `bin/` and `obj/` are tracked
   at `bookit-ch20` and no `.gitignore` exists, although Chapter 20's manuscript states
   `dotnet new gitignore` ran and calls it load-bearing for diff proofs; the solution file is
   also `BookIt.slnx`, not the `.sln` the manuscript's setup block creates. Not Ch. 24's scope
   to fix — **ch21-M0 performs the repair** (`dotnet new gitignore` + `git rm -r --cached`, a
   scripted beat in `ch21-design.md`), so by this chapter the index is clean; this design's
   proof commands still carry pathspec excludes (`':!*/bin/*' ':!*/obj/*'`) as defense in depth,
   harmless no-ops on the repaired tree.
