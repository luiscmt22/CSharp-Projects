# Design — Chapter 22

**Chain of Responsibility: The Front Desk Rulebook** — BookIt build arc, installment 3 of 5.

Repo verified 2026-07-27 at tag `bookit-ch20` (commits `M1`–`M5`, 26 tests green in 41 ms,
`.NET 10`). This chapter sits on **Chapter 21's designed end state** (`ch21-design.md`, tag
`bookit-ch21`), which is binding upstream: the engine lives in `src/Scheduling.Conflicts`
(namespace `Scheduling.Conflicts`), its surface is
`ConflictChecker.FindConflicts(IBookableResource, BookingRequest, IReadOnlyList<IBookingRecord>)`
+ `AddConflictDetection()`; `BookingRequest`, `Conflict` and `ResourceTypes` are engine types;
`BookIt.Domain.Resource : IBookableResource` and `Booking : IBookingRecord`; the suite stands at
**BookIt.Tests 27 + ClinicIt.Tests 7**. The only *surviving* type in this chapter that *invokes* the engine
is `ConflictRule` (plus the one `AddConflictDetection()` registration call; the M1–M2 naive desk
calls the engine directly, and M3 moves that call into `ConflictRule`); engine-owned value
types (`BookingRequest`, `ResourceTypes`) ride through other files as data only — so if ch21's
execution drifts from its design, ch22 absorbs it in one surviving file.

**Premise (arrives as a forwarded email list, numbered by the owner).** Before any conflict math:
(1) members with unpaid invoices can't book; (2) bookings only on the space's opening days —
the space closes Sundays; (3) travelling equipment needs a 15-minute transfer window between
bookings on *different floors*; (4) whatever survives the rules still faces the Ch. 20 conflict
engine. Payoff requirement (5) lands in M5: nothing bookable more than 60 days out.

**Teaching spine.** Ch. 20 was "run ONE of these based on a key." This chapter is the sibling
slogan from Drill 7 coming home: "run ALL of these in order — any one can say no." The gauntlet,
not the router. Named beats: (a) contrast with Strategy selection, explicitly, once — the
consumption differs, not the DI shape; (b) Strategy *inside* Chain — the whole Ch. 20 engine
becomes one link; (c) order is behavior, which is why registration order lives in exactly one
place; (d) `IEnumerable<T>` DI injection preserves registration order — proved by a checkpoint
test, never asserted in prose.

**Chapter skeleton** mirrors Ch. 20 exactly: cold open (the care-workforce platform's check-in
story — geofence, time window, duplicate detection — told as far as the wall, resolution
deferred), project brief, M1–M5 with your-turn boxes and typed checkpoint tests, "The Pattern,
Named" between M2 and M3, In the Wild #1 (the validator pipeline: `ICheckInValidator` with
`Order`, from `_Curated\APPLICATIONS\scheduling\givingcare-validator-pipeline.html` — "the
care-workforce platform", never the product name; Ch. 20's Boundaries already PRINTED this
interface and its `foreach` as a preview, so this section must advance, not reprint: the
`Order` gap-numbering convention, the mid-pipeline insertion at 25, the veto-vs-collect policy,
and the resolution of the cold open the preview never gave) after M4, In the Wild #2 (the
notification-recipient resolver pipeline, `_Curated\CORE\patterns\notification-recipient-resolver-pipeline.md`)
plus Boundaries after M5, then drills. Boundaries section owns: Chain vs Strategy (revisited from
the builder's side), Chain vs middleware (`app.Use(...)` is this pattern with `next` made
explicit), GoF's linked-handler original vs the modern flattened `foreach` (Ch. 20's boundaries
already introduced this — now the reader has built it), `Order` property vs registration order
(the production system chose `Order`; BookIt chooses registration order; the trade is stated,
one is not holier), and when NOT to chain (two stable rules = two guard clauses, no pattern).

---

## Milestones (5)

Suite arithmetic, fixed for all Done-whens (BookIt.Tests counts; ClinicIt.Tests holds at 7
throughout — `dotnet test` prints both lines): start 27 → M1 31 → M2 34 → M3 48 → M4 51 →
M5 54. Totals across both hosts: 34 → 38 → 41 → 55 → 58 → 61.

---

### ch22-M1 — The Desk, the Obvious Way

**Commit:** `ch22-M1: front desk rules, the obvious way`

**Files:**
- ADD `src/BookIt/FrontDesk/FrontDeskModels.cs` — `BookingSubmission`, `BookingDecision`,
  `RuleNames` constants (planted in M1 the way `CreateChecker` was planted in Ch. 20 M1: today
  they're just refusal codes on the decision; in M3 they become the links' self-describing names,
  and no test churns when that happens). Note in file, Ch. 20 style: "in a real project these
  records get a file each."
- ADD `src/BookIt/FrontDesk/BookingDesk.cs` — naive: ctor `(ConflictChecker, OpeningDays)`,
  `Submit(BookingSubmission)` = two guard clauses + engine call. Readable. Correct. ~20 lines.
  The chapter refuses to apologize for it, same as Ch. 20 M1.
- CHANGE `src/BookIt/Domain/Models.cs` — add `Member` record, `[Flags] OpeningDays` +
  `OpeningDaysExtensions` (the "arrives from Ch. 5; built inline in ten minutes" piece — one
  sentence in the chapter says exactly that, per contract).
- ADD `tests/BookIt.Tests/BookingDeskTests.cs` — 4 checkpoint tests + the planted seam
  `private static BookingDesk CreateDesk()` (called out in prose: "you know why this helper
  exists; you watched it pay for itself in Chapter 20 Milestone 3").

**Checkpoint tests (reader types them first):**
1. `Member_With_Unpaid_Invoices_Is_Refused_Before_Any_Conflict_Math` — unpaid member, empty
   bookings list, decision refused with `RefusedBy == RuleNames.MemberStanding` and a reason
   mentioning invoices.
2. `Booking_On_A_Sunday_Is_Refused_When_The_Space_Closes_Sundays` — desk built with
   `Weekdays | Saturday`; Sunday request refused, reason names Sunday.
3. `A_Paid_Up_Member_On_An_Open_Day_Still_Faces_The_Conflict_Engine` — clean member, open day,
   overlapping ROOM-1 slot → refused with `RefusedBy == RuleNames.ResourceConflicts` (rule 4:
   the engine is the last word).
4. `A_Clean_Submission_Is_Accepted` — paid member, Tuesday, free slot → `Accepted` is true,
   `RefusedBy` and `Reason` both null.

**Your-turn box (notes):**
- *Goal.* Make the 4 checkpoint tests pass with the most obvious `Submit` you can write: guard
  clause for invoices, guard clause for opening days, delegate to the Ch. 20 `ConflictChecker`
  you already own, `Accept()` at the bottom. `BookingDecision` given (statics `Accept()` /
  `Refuse(ruleName, reason)`); refusal codes from `RuleNames`, never literals.
- *Constraints.* No interfaces, no pipeline, no pattern — the developer who ships at 17:30
  again. `OpeningDays` is a `[Flags]` enum with `Weekdays`/`EveryDay` composites; the
  day-touches check must walk every date the request spans (steal the loop shape from
  `TouchesDayOfWeek` — you wrote it in Ch. 20 M2). The desk does NOT re-query anything: all
  facts ride in on `BookingSubmission`.
- *Order-if-you-freeze.* Type `Member`, `OpeningDays`, `BookingSubmission`, `BookingDecision`
  first — the tests dictate all four. Then the invoice guard against test 1. Then the opening-day
  guard against test 2. The engine call is Ch. 20 code you've already written three times.
- *Done when.* `dotnet test` → BookIt.Tests 31 (38 with ClinicIt); committed.

**Red run:** none (M1 is the calm before).

---

### ch22-M2 — The Third Email Breaks the Method

**Commit:** `ch22-M2: transfer window - the method buckles`

**Files:**
- CHANGE `src/BookIt/Domain/Models.cs` — `Resource` gains `int Floor = 0`; `Booking` gains
  `int Floor = 0` (trailing defaults: ZERO edits to the 26 Ch. 20 tests — the chapter names
  this out loud as the counter-example to Ch. 20 M2's churn, then immediately delivers churn
  anyway, see next line).
- CHANGE `src/BookIt/FrontDesk/FrontDeskModels.cs` — `BookingSubmission` gains a REQUIRED
  positional `int Floor` (before `ExistingBookings`, no default — deliberately). Every
  construction site breaks. That's tally (a).
- CHANGE `src/BookIt/FrontDesk/BookingDesk.cs` — the transfer-window logic inline: loop over
  same-resource bookings, floor comparison, gap arithmetic, `TransferWindow` constant now a
  roommate of invoice logic. `Submit` goes ~20 → ~45 lines with four business vocabularies.
- CHANGE `tests/BookIt.Tests/BookingDeskTests.cs` — +3 tests; all 4 existing submissions edited
  for the new `Floor` argument.

**Checkpoint tests:**
5. `Equipment_Changing_Floors_With_Only_Ten_Minutes_Between_Bookings_Is_Refused` — whiteboard
   booked 09:00–10:00 on floor 1; request 10:10 on floor 3 → refused,
   `RefusedBy == RuleNames.EquipmentTransfer`.
6. `Equipment_Staying_On_Its_Floor_Is_Refused_By_The_Conflict_Engine_Not_The_Transfer_Check` —
   same times, same floor → still refused (whole-day checkout, Ch. 20 rule) but
   `RefusedBy == RuleNames.ResourceConflicts`. Two rules can doom the same booking; WHICH reason
   the member hears is decided by code order — the sentence the whole chapter is about, planted
   here as a passing observation.
7. `An_Overnight_Floor_Change_With_A_Ten_Minute_Gap_Is_Refused_Even_Though_The_Days_Differ` —
   evening event runs to 23:55 on floor 3; a 00:05 next-day setup on floor 1 → refused by the
   transfer check. Different calendar days, so the per-day engine would have ALLOWED it — the
   transfer rule sees intervals where the engine sees days. Proves the rule is not redundant
   with rule 4.

**Your-turn box (notes):**
- *Goal.* Absorb email #3 without refactoring. Add `Floor` where the tests demand it, write the
  transfer check inline in `Submit`, between the opening-days guard and the engine call. The gap
  test is interval math you have typed twice already (Ch. 20 M1 buffer, Ch. 20 M3 move):
  `request.Start < b.End + window && b.Start - window < request.End`, now guarded by
  `b.Floor != submission.Floor`. Exactly 15 minutes is allowed — strict `<`, half-open
  discipline, third appearance, the chapter says so.
- *Constraints.* Stay in the one method. Do NOT extract anything. Two paper tallies:
  (a) construction sites you edited only because `BookingSubmission` changed shape;
  (b) `Submit`'s line count before/after, and the count of distinct business vocabularies now
  living in it (invoices, calendars, floors, conflict math — you should reach 4).
- *Order-if-you-freeze.* Models first, let the compiler enumerate the fall-out (every red
  squiggle is a line item for tally (a)). Then test 5 only. Then run test 6 — if it fails,
  your transfer check runs AFTER the engine call or doesn't check floors; the ORDER of your
  inline checks is already load-bearing and nobody chose it on purpose. Test 7 last.
- *Done when.* BookIt.Tests 34 green (41 with ClinicIt); tallies written down; committed.

**Red run:** none scripted, but the box predicts the honest accidental one (test 6 failing when
the inline checks are in the wrong order) and mines it: order was behavior before we ever named
the pattern.

---

### ch22-M3 — The Rulebook Extraction

**Commit:** `ch22-M3: rulebook extraction`

*"The Pattern, Named" section sits immediately before this milestone:* the gauntlet — an
interface per link, an ordered runner that stops at the first veto, links that don't know each
other exist. Contrast box with Strategy: same DI shape (`IEnumerable<T>` of one interface),
opposite consumption (select ONE by key vs run ALL in order). Drill 7's two slogans, quoted.

**Files:**
- ADD `src/BookIt/FrontDesk/IBookingRule.cs` — interface, XML doc states the contract: rules
  run in registration order; first veto stops the run; a rule that doesn't apply returns Pass.
- CHANGE `src/BookIt/FrontDesk/FrontDeskModels.cs` — add `RuleResult` (statics `Pass` /
  `Veto(reason)`), add `Refusal`.
- ADD `src/BookIt/FrontDesk/MemberStandingRule.cs`, `OpeningDaysRule.cs`,
  `EquipmentTransferRule.cs`, `ConflictRule.cs` — MOVED branch bodies (cut, paste, adjust —
  same discipline as Ch. 20 M3: do not re-derive). `ConflictRule` is the ONLY type in the
  chapter that invokes the engine (engine value types ride through other files as data only);
  the chapter names the composition beat here: the entire
  Chapter 20 pattern — router, strategies, registrations — just became one link in a chain.
  Patterns compose; that sentence is the section's title.
- ADD `src/BookIt/FrontDesk/BookingRulebook.cs` — the runner. Owns NO rule logic; `foreach`
  in registration order, first veto → `Refuse(rule.RuleName, result.Reason)`.
- ADD `src/BookIt/FrontDesk/IRefusalLog.cs`, `InMemoryRefusalLog.cs` — one line of prose
  foreshadowing Ch. 23 ("the owner will want to read these on a Monday"), no more, per contract.
- CHANGE `src/BookIt/FrontDesk/BookingDesk.cs` — thin: ctor `(BookingRulebook, IRefusalLog)`;
  `Submit` = evaluate, record refusal if refused, return. ~12 lines.
- ADD tests: `MemberStandingRuleTests.cs`, `OpeningDaysRuleTests.cs`,
  `EquipmentTransferRuleTests.cs`, `ConflictRuleTests.cs`, `BookingRulebookTests.cs`,
  `RefusalLogTests.cs`.
- CHANGE `tests/BookIt.Tests/BookingDeskTests.cs` — the ONLY edit is the body of
  `CreateDesk()`. All 7 desk tests pass untouched: the certificate of refactor, second issue.

**Checkpoint tests (14):**
- `MemberStandingRuleTests`:
  1. `Vetoes_A_Member_With_Unpaid_Invoices_And_Names_Itself` — veto, reason mentions invoices.
  2. `Passes_A_Member_In_Good_Standing` — `RuleResult.Pass`.
- `OpeningDaysRuleTests`:
  3. `Vetoes_A_Multi_Day_Booking_That_Touches_A_Closed_Day` — Fri→Sun span, space closed
     Sundays → veto naming Sunday.
  4. `Passes_A_Submission_Inside_Opening_Days` — Tuesday passes.
- `EquipmentTransferRuleTests`:
  5. `Vetoes_A_Cross_Floor_Gap_Shorter_Than_The_Transfer_Window` — isolation twin of desk test 5.
  6. `Allows_A_Cross_Floor_Gap_Of_Exactly_Fifteen_Minutes` — boundary; strict `<` pinned.
  7. `Ignores_Resources_That_Do_Not_Travel` — a ROOM submission returns Pass untouched; a link
     that doesn't apply passes, it doesn't abstain-with-an-error. (The rule early-returns on
     `Resource.Type != ResourceTypes.Equipment`.)
- `ConflictRuleTests`:
  8. `Vetoes_With_The_Engines_Reason_When_A_Conflict_Exists` — the veto reason contains the
     engine's conflict reason text (Strategy inside Chain, observable).
  9. `Passes_When_The_Engine_Finds_Nothing` — free slot → Pass.
- `BookingRulebookTests` (with a `RecordingRule` test double, direct descendant of Ch. 20's
  `RecordingStrategy` — the chapter says so):
  10. `The_First_Veto_Stops_The_Run_And_Later_Rules_Never_Execute` — vetoing rule first,
      recording rule second; assert recording rule was never called. Gauntlet semantics, half 1.
  11. `A_Clean_Submission_Runs_The_Whole_Gauntlet` — two recording rules, both called. Half 2.
  12. `The_Decision_Names_The_Rule_That_Said_No` — `RefusedBy` equals the vetoing link's
      `RuleName`.
- `RefusalLogTests` (desk built by hand with its own `InMemoryRefusalLog`):
  13. `A_Refusal_Is_Recorded_With_The_Rule_And_The_Reason` — refusal row carries member id,
      resource id, rule name, reason, and the refused slot's start.
  14. `An_Accepted_Submission_Records_Nothing` — log stays empty.

**Your-turn box (notes):**
- *Goal.* Four rule classes, one runner, one thin desk. `IBookingRule` = self-describing
  `RuleName` (values from `RuleNames`) + `RuleResult Check(BookingSubmission)`. `BookingRulebook`
  takes `IEnumerable<IBookingRule>`, runs in the order given, stops at first veto. Desk keeps its
  public `Submit` signature; refusals go to `IRefusalLog`.
- *Constraints.* Public `Submit(BookingSubmission)` unchanged. The only edit permitted in
  `BookingDeskTests.cs` is `CreateDesk()`'s body — new rule tests live in new files. MOVE code
  into the rules; don't rewrite it. When done, grep and mean it: `BookingRulebook.cs` and
  `BookingDesk.cs` must contain no `Invoice`, no `Floor`, no `OpeningDays`, no `TimeSpan`, and
  no rule name — the runner and the desk know only `IBookingRule`.
- *Order-if-you-freeze.* Interface first (ten lines, tests dictate it). Then `MemberStandingRule`
  alone — smallest move, run only its 2 tests. Then `OpeningDaysRule`, then `EquipmentTransferRule`
  (the M2 mess lifts out whole), then `ConflictRule` (wrap the engine call, join conflict reasons
  with `"; "`). Then the rulebook against tests 10–12. Then gut the desk. Then the `CreateDesk`
  edit. Never more than one step from green.
- *Done when.* BookIt.Tests 48 green (55 with ClinicIt) with `BookingDeskTests.cs` untouched
  outside `CreateDesk`; grep comes back empty; committed.

**Red run:** none scripted (M4 and M5 own the scripted reds).

---

### ch22-M4 — DI Order and the Net Under It

**Commit:** `ch22-M4: DI order + smoke net`

**Files:**
- ADD `src/BookIt/FrontDesk/FrontDeskRegistration.cs` — `AddFrontDesk()`: calls
  `AddConflictDetection()` first, then registers the four rules IN ORDER (cheap and precise
  first, the expensive engine last — a comment states the ordering policy, Ch. 20's
  "Singleton is a decision" style), then rulebook, refusal log, desk. All Singleton, defended
  in one comment line (stateless links; the refusal log is a deliberate exception discussed in
  prose: in-memory Singleton is exactly what Ch. 24's "static state across circuits" bug will
  interrogate — one foreshadowing clause, no more).
- CHANGE `src/BookIt/Program.cs` — the installment's one scene: resolve `BookingDesk`, submit
  four stories (an unpaid member whose requested slot ALSO clashes with an existing booking —
  doubly doomed on purpose, because M4's red run and M5's order test both lean on that
  submission; a Sunday request; the 10-minute floor change; a clean acceptance), print each
  verdict with `RefusedBy`, end by dumping the refusal log ("three refusals the owner will
  ask about on Monday" — the Ch. 23 hook, one line).
- ADD `tests/BookIt.Tests/FrontDeskSmokeTests.cs` — spec array + 3 tests. The ordered spec
  array `ExpectedRuleOrder` serves both membership and order tests — one list, two claims,
  same `ExpectedResourceTypes` philosophy as Ch. 20 ("it looks like duplication; it's a spec").

**Checkpoint tests:**
1. `Every_Expected_Rule_Resolves_From_The_Composition_Root` — resolve
   `IEnumerable<IBookingRule>` from a provider built via `AddFrontDesk()`; every name in
   `ExpectedRuleOrder` is present.
2. `Rules_Resolve_In_Registration_Order_And_That_Order_Is_The_Spec` —
   `Assert.Equal(ExpectedRuleOrder, resolved.Select(r => r.RuleName))` (SequenceEqual). THIS is
   the test the contract demands instead of prose: `IEnumerable<T>` injection preserves
   registration order — now it's pinned, and any container that ever stops honoring it breaks
   a test instead of a member's booking.
3. `The_Desk_Itself_Resolves` — `GetRequiredService<BookingDesk>()` not null; the whole graph
   (desk → rulebook → rules → ConflictRule → engine → strategies) constructs from one
   composition root.

**Your-turn box (notes):**
- *Goal.* One composition point, one Program scene, three smoke tests green.
- *Constraints.* `Program.cs` calls ONLY `AddFrontDesk()` — calling `AddConflictDetection()`
  there as well would double-register the strategies and the Ch. 20 router's `ToDictionary`
  would throw at first resolution (the reader is invited to try it for ten seconds if they
  don't believe it: Ch. 20's duplicate-key net firing for a brand-new reason). Rule
  registrations appear in exactly one method, or the order test starts lying.
- *Order-if-you-freeze.* Registration method first — it's eleven lines and four of them are
  Ch. 20's. Then the smoke tests (steal the `BuildProvider` shape from
  `ConflictDetectionSmokeTests`). Program scene last.
- *Done when.* BookIt.Tests 51 green (58 with ClinicIt); `dotnet run` prints four front-desk
  verdicts and the refusal-log dump; the scripted red below has been run and read; committed.

**Deliberate red run (scripted, the milestone's centerpiece):** move the
`ConflictRule` registration line to the TOP of `AddFrontDesk`. Run `dotnet test`. Exactly one
test fails — `Rules_Resolve_In_Registration_Order_And_That_Order_Is_The_Spec`, its
SequenceEqual diff printing `ResourceConflicts` where `MemberStanding` should be. Then
`dotnet run` and read the demo: the unpaid member is now told about a *room clash* instead of
their invoices — same inputs, different verdict text; nothing else failed; every rule unit test
is still green. Order is behavior invisible to every test except the one that pins it. Restore
the line, re-run, 51 green. (A one-sentence echo notes the Ch. 20 cousin: comment out a rule
registration instead, and test 1 fires exactly like `Every_Expected_Resource_Type…` did — the
reader may try it, the chapter doesn't script it twice.)

---

### ch22-M5 — The Fifth Rule: The Payoff

**Commit:** `ch22-M5: advance window - one class, one line` · then tag `bookit-ch22`

**Files:**
- CHANGE `tests/BookIt.Tests/FrontDeskSmokeTests.cs` — Step 1: add `RuleNames.AdvanceWindow`
  to `ExpectedRuleOrder` (between `OpeningDays` and `EquipmentTransfer` — cheap rules stay
  ahead of the travel loop and the engine). Spec first, red on purpose.
- ADD `src/BookIt/FrontDesk/AdvanceWindowRule.cs` — Step 2. Ctor takes `TimeProvider` (one
  sentence on why: "today" is an input, and inputs get injected — the test types a five-line
  `FixedClock`).
- ADD `tests/BookIt.Tests/AdvanceWindowRuleTests.cs` — Step 2, with the `FixedClock` double.
- CHANGE `src/BookIt/FrontDesk/FrontDeskRegistration.cs` — Step 3: ONE line,
  `services.AddSingleton<IBookingRule>(_ => new AdvanceWindowRule(TimeProvider.System));`.
- ADD `tests/BookIt.Tests/RuleOrderTests.cs` — the order-matters demonstration the contract
  mandates, after Step 3.

**Checkpoint tests:**
1. `Vetoes_A_Booking_More_Than_Sixty_Days_Out` — clock fixed at 2026-07-13; request starting
   2026-09-12 (61 days) → veto, reason names the 60-day window.
2. `Allows_A_Booking_Exactly_Sixty_Days_Out` — 2026-09-11 → Pass. Boundary pinned; inclusive
   60 is a decision, the test is where it lives.
3. `Moving_Member_Standing_After_The_Conflict_Rule_Changes_Who_Says_No` (in `RuleOrderTests`) —
   ONE doubly-doomed submission (unpaid member AND clashing room slot) evaluated by two
   hand-built rulebooks: spec order → `RefusedBy == RuleNames.MemberStanding`; reversed →
   `RefusedBy == RuleNames.ResourceConflicts`. Same rules, same submission, different verdict —
   the permanent, always-green exhibit of what M4's red run showed transiently, and the reason
   registration order lives in one method.

**Your-turn box (notes):**
- *Goal.* Email #5 ("stop people camping on the calendar — nothing more than 60 days out"),
  end to end, in the order printed. Step 1: spec line → run → exactly two smoke tests fail
  (membership and order), read both messages: the system now promises a rule it doesn't have.
  Step 2: rule + its 2 tests → green locally, smoke still red — the class exists, compiles,
  passes its own tests, and the running system has never heard of it; you have now seen this
  exact gap three times across two chapters, and it is why composition-root smoke tests exist.
  Step 3: the one registration line → 54 green. Then type the order-matters test.
- *Constraints.* You may not open `BookingRulebook.cs`, `BookingDesk.cs`, or any of the four
  existing rule files — not even to look. The diff proves you didn't need to.
- *Order-if-you-freeze.* The rule body is two early returns and a subtraction; steal
  `MemberStandingRule`'s file as the template — that's what a rulebook is FOR.
- *Done when.* BookIt.Tests 54 green (61 with ClinicIt); the payoff diff (below) shows the
  rulebook and all four existing rules untouched; `git tag bookit-ch22`; committed.

**Deliberate red run:** Step 1→Step 3 IS the red run (spec-first, two named failures, watched
twice). Failure text the reader must see at Step 1/Step 2, verbatim shape:
`Rules_Resolve_In_Registration_Order… Assert.Equal() Failure` with `AdvanceWindow` present in
the expected sequence and absent from the actual, and
`Every_Expected_Rule_Resolves… Assert.Contains() Failure: Item "AdvanceWindow" not found`.

---

## New/changed domain types (exact C# signatures)

```csharp
// ── src/BookIt/Domain/Models.cs (additions / changes) ──────────────────────
namespace BookIt.Domain;

[Flags]
public enum OpeningDays
{
    None      = 0,
    Monday    = 1,
    Tuesday   = 2,
    Wednesday = 4,
    Thursday  = 8,
    Friday    = 16,
    Saturday  = 32,
    Sunday    = 64,
    Weekdays  = Monday | Tuesday | Wednesday | Thursday | Friday,
    EveryDay  = Weekdays | Saturday | Sunday,
}

public static class OpeningDaysExtensions
{
    public static OpeningDays ToOpeningDay(this DayOfWeek day);   // switch expression, 7 arms
    public static bool IsOpenOn(this OpeningDays days, DayOfWeek day);
        // => (days & day.ToOpeningDay()) != 0;
}

public sealed record Member(
    string Id,
    string Name,
    bool HasUnpaidInvoices = false);

// M2: trailing defaults — zero churn in the 26 surviving ch20 tests, and the chapter says so.
// The `: IBookableResource` / `: IBookingRecord` clauses are ch21's one-line adapters and stay.
// Floor is HOST-ONLY: it does not join the engine interfaces — the transfer rule is front-desk
// policy, not conflict math, and the engine's contract stays untouched (one named sentence).
public sealed record Resource(
    string Id,
    string Type,
    int Capacity = 0,
    DayOfWeek? MaintenanceDay = null,
    int Floor = 0)
    : IBookableResource;

public sealed record Booking(
    int Id,
    string ResourceId,
    DateTime Start,
    DateTime End,
    int Floor = 0)
    : IBookingRecord;

// BookingRequest and Conflict: UNCHANGED — engine types in Scheduling.Conflicts since ch21.
// The engine's surface is not touched by this chapter.
```

```csharp
// ── src/BookIt/FrontDesk/FrontDeskModels.cs ────────────────────────────────
namespace BookIt.FrontDesk;

public static class RuleNames
{
    public const string MemberStanding    = "MemberStanding";
    public const string OpeningDays       = "OpeningDays";
    public const string AdvanceWindow     = "AdvanceWindow";      // M5
    public const string EquipmentTransfer = "EquipmentTransfer";
    public const string ResourceConflicts = "ResourceConflicts";
}

// M1 shape has NO Floor; M2 inserts it as a required positional parameter — deliberately.
public sealed record BookingSubmission(
    Member Member,
    Resource Resource,
    BookingRequest Request,
    int Floor,                              // M2
    IReadOnlyList<Booking> ExistingBookings);

public sealed record BookingDecision(bool Accepted, string? RefusedBy, string? Reason)
{
    public static BookingDecision Accept() => new(true, null, null);
    public static BookingDecision Refuse(string ruleName, string reason) => new(false, ruleName, reason);
}

// M3:
public sealed record RuleResult(bool Passed, string? Reason)
{
    public static readonly RuleResult Pass = new(true, null);
    public static RuleResult Veto(string reason) => new(false, reason);
}

public sealed record Refusal(
    string MemberId,
    string ResourceId,
    string RuleName,
    string Reason,
    DateTime RequestedStart);   // the refused slot's start (submission.Request.Start) —
                                // Ch. 23's Monday report filters refusals to a week; a refusal
                                // without its slot would be unreportable. Set by the desk.
```

```csharp
// ── src/BookIt/FrontDesk/IBookingRule.cs (M3) ──────────────────────────────
namespace BookIt.FrontDesk;

/// <summary>
/// One link in the front desk's rulebook. Links run in registration order;
/// the first veto stops the run. A rule that does not apply returns Pass.
/// </summary>
public interface IBookingRule
{
    /// <summary>Self-describing name. Values come from <see cref="RuleNames"/>.</summary>
    string RuleName { get; }

    RuleResult Check(BookingSubmission submission);
}
```

```csharp
// ── the links (M3, M5) ─────────────────────────────────────────────────────
public sealed class MemberStandingRule : IBookingRule
{
    public string RuleName => RuleNames.MemberStanding;
    public RuleResult Check(BookingSubmission submission);
}

public sealed class OpeningDaysRule : IBookingRule
{
    public OpeningDaysRule(OpeningDays openingDays);
    public string RuleName => RuleNames.OpeningDays;
    public RuleResult Check(BookingSubmission submission);   // walks every date the request spans
}

public sealed class EquipmentTransferRule : IBookingRule
{
    private static readonly TimeSpan TransferWindow = TimeSpan.FromMinutes(15);
    public string RuleName => RuleNames.EquipmentTransfer;
    public RuleResult Check(BookingSubmission submission);
    // Early return unless submission.Resource.Type is ResourceTypes.Equipment.
    // Veto when any same-resource booking on a DIFFERENT floor sits closer than
    // TransferWindow:  submission.Request.Start < b.End + TransferWindow
    //               && b.Start - TransferWindow < submission.Request.End
    // Strict '<': a gap of exactly 15 minutes is allowed (pinned by test).
}

public sealed class ConflictRule : IBookingRule            // Strategy inside Chain
{
    public ConflictRule(ConflictChecker conflictChecker);  // the only type that CALLS the engine
    public string RuleName => RuleNames.ResourceConflicts;
    public RuleResult Check(BookingSubmission submission);
    // FindConflicts(submission.Resource, submission.Request, submission.ExistingBookings);
    // veto with conflict reasons joined by "; ".
}

public sealed class AdvanceWindowRule : IBookingRule       // M5
{
    private static readonly TimeSpan MaxAdvance = TimeSpan.FromDays(60);
    public AdvanceWindowRule(TimeProvider clock);
    public string RuleName => RuleNames.AdvanceWindow;
    public RuleResult Check(BookingSubmission submission);
}
```

```csharp
// ── src/BookIt/FrontDesk/BookingRulebook.cs (M3) ───────────────────────────
/// <summary>Runs the rules in registration order; the first veto ends the run. No rules live here.</summary>
public sealed class BookingRulebook
{
    public BookingRulebook(IEnumerable<IBookingRule> rules);
    public BookingDecision Evaluate(BookingSubmission submission);
}

// ── src/BookIt/FrontDesk/IRefusalLog.cs / InMemoryRefusalLog.cs (M3) ───────
public interface IRefusalLog
{
    void Record(Refusal refusal);
    IReadOnlyList<Refusal> All { get; }
}

public sealed class InMemoryRefusalLog : IRefusalLog;

// ── src/BookIt/FrontDesk/BookingDesk.cs ────────────────────────────────────
// M1–M2 (naive):  public BookingDesk(ConflictChecker conflictChecker, OpeningDays openingDays)
// M3 onward:
public sealed class BookingDesk
{
    public BookingDesk(BookingRulebook rulebook, IRefusalLog refusalLog);
    public BookingDecision Submit(BookingSubmission submission);   // evaluate → record refusal → return
}

// ── src/BookIt/FrontDesk/FrontDeskRegistration.cs (M4, +1 line M5) ─────────
public static class FrontDeskRegistration
{
    public static IServiceCollection AddFrontDesk(this IServiceCollection services);
    // AddConflictDetection();
    // ORDER IS BEHAVIOR — cheap, precise rules first; the engine last:
    //   AddSingleton<IBookingRule, MemberStandingRule>();
    //   AddSingleton<IBookingRule>(new OpeningDaysRule(OpeningDays.Weekdays | OpeningDays.Saturday));
    //   AddSingleton<IBookingRule>(_ => new AdvanceWindowRule(TimeProvider.System));   // M5
    //   AddSingleton<IBookingRule, EquipmentTransferRule>();
    //   AddSingleton<IBookingRule, ConflictRule>();
    //   AddSingleton<BookingRulebook>(); AddSingleton<IRefusalLog, InMemoryRefusalLog>(); AddSingleton<BookingDesk>();
}
```

Test double typed by the reader in M5 (six lines, in the test file — classic ctor + readonly
field, matching the arc's Ch. 20 syntax palette: no primary constructors mid-arc, per ch21):

```csharp
private sealed class FixedClock : TimeProvider
{
    private readonly DateTimeOffset _now;
    public FixedClock(DateTimeOffset now) => _now = now;
    public override DateTimeOffset GetUtcNow() => _now;
}
```

---

## Payoff proof procedure (exact commands and expected outputs)

All commands from the repo root (Bash path
`/c/Users/User/AppData/Local/Temp/claude/C--Users-User-source-repos-luiscmt22-SkillCanvas/f2d203ae-e0d5-41a1-851d-982987555e1a/scratchpad/book/BookIt`).
Diffs are path-scoped to source folders throughout — the repo currently tracks `bin/`/`obj/`
(see Deviations), and path-scoping keeps every proof honest regardless.

**1. History and tag tell the story:**

```bash
git log --oneline -5
# ch22-M5: advance window - one class, one line
# ch22-M4: DI order + smoke net
# ch22-M3: rulebook extraction
# ch22-M2: transfer window - the method buckles
# ch22-M1: front desk rules, the obvious way
git tag --list 'bookit-ch2*'
# bookit-ch20
# bookit-ch21
# bookit-ch22
```

**2. Whole suite green at the promised count (both hosts, one command):**

```bash
dotnet test
# Passed!  - Failed: 0, Passed: 54, Skipped: 0, Total: 54 … BookIt.Tests.dll
# Passed!  - Failed: 0, Passed:  7, Skipped: 0, Total:  7 … ClinicIt.Tests.dll
```

**3. The Open/Closed diff — the fifth rule cost one class and one line:**

```bash
M4=$(git rev-list -1 --grep='^ch22-M4:' HEAD)
git diff --stat $M4..HEAD -- src/BookIt/FrontDesk
#  src/BookIt/FrontDesk/AdvanceWindowRule.cs      | ~28 ++++++++++++++
#  src/BookIt/FrontDesk/FrontDeskRegistration.cs  |  1 +
#  2 files changed, ~29 insertions(+), 0 deletions(-)
```

**4. The unfakeable zero — nothing that worked was even opened:**

```bash
git diff --stat $M4..HEAD -- \
  src/BookIt/FrontDesk/BookingRulebook.cs \
  src/BookIt/FrontDesk/BookingDesk.cs \
  src/BookIt/FrontDesk/MemberStandingRule.cs \
  src/BookIt/FrontDesk/OpeningDaysRule.cs \
  src/BookIt/FrontDesk/EquipmentTransferRule.cs \
  src/BookIt/FrontDesk/ConflictRule.cs
# (prints NOTHING — empty output IS the proof)
```

**5. The engine was consumed, never modified — the extracted engine survives untouched:**

```bash
CH21=$(git rev-list -1 bookit-ch21)
git diff --stat $CH21..HEAD -- src/Scheduling.Conflicts
# (empty — the whole chapter composed the engine without opening it; the baseline is
# bookit-ch21 because ch21 is the commit where the engine's files last legitimately moved)
```

**6. Ignorance greps (M3's constraint, mechanically checked):**

```bash
grep -cE 'Invoice|Floor|OpeningDays|TimeSpan' src/BookIt/FrontDesk/BookingRulebook.cs src/BookIt/FrontDesk/BookingDesk.cs
# src/BookIt/FrontDesk/BookingRulebook.cs:0
# src/BookIt/FrontDesk/BookingDesk.cs:0
```

**7. Order is behavior, twice over:**

```bash
dotnet test --filter "FullyQualifiedName~Rules_Resolve_In_Registration_Order|FullyQualifiedName~Moving_Member_Standing"
# Passed! - 2 tests
# …plus the M4 red-run transcript: with ConflictRule registered first, exactly ONE test fails
# (the SequenceEqual order test, showing ResourceConflicts where MemberStanding belongs), and
# `dotnet run` tells the unpaid member about a room clash instead of their invoices.
```

**8. The demo scene:**

```bash
dotnet run --project src/BookIt
# ...four Ch.20 conflict verdicts, then the front-desk scene:
# Front desk — Rui (unpaid), Atlas Monday 09:30:   REFUSED [MemberStanding] …invoices…
#   (Rui's slot also clashes with Atlas's 09:00–10:00 booking — doubly doomed; only rule
#    order decides which refusal he hears, which is what M4's red run demonstrates live)
# Front desk — Ana, Atlas Sunday 10:00:            REFUSED [OpeningDays] …Sunday…
# Front desk — Ana, whiteboard floor 3 at 10:10:   REFUSED [EquipmentTransfer] …15 minutes…
# Front desk — Ana, ROOM-1 Tuesday 14:00:          ACCEPTED
# Refusal log (3): …one line each — the owner will ask about these on Monday.
```

---

## Felt-pain narrative beats (what the reader suffers, and the paper tallies)

The pain design deliberately differs from Ch. 20's. There the disease was *branch accretion by
type*; here it is *guard accretion by policy* — the same method absorbing rules that share
nothing but the word "no". The chapter says this in one sentence at M2's close: same disease,
different organ.

**Beat 1 — M1 feels fine, and the chapter admits it.** Two guard clauses and a delegate is
*polite code*. Early returns, happy path unindented — by every rule this book has taught, `Submit`
at M1 is finished. The trap is not ugliness; it's that the method has become the place where
refusals go, and refusals are what this business generates. (No tally yet; the reader is set up
to defend code the next email will indict.)

**Beat 2 — the forwarded email list itself.** The requirements arrive as the owner forwarded
them: numbered, chatty, out of order, one of them ("only when we're open — obviously??") half a
sentence long. The reader implements 1 and 2 in M1 and thinks the list is a formality. Item 3
is quietly different in kind — it needs *history* (other bookings), *geometry* (floors), and
*arithmetic* (gaps) — and the chapter lets the reader discover that instead of announcing it.

**Beat 3 — M2, the buckle, tallies out.** Two paper tallies, kept while working:
- **Tally (a) — churn:** construction sites edited only because `BookingSubmission` grew a
  `Floor`. Expected count: 5 (four M1 tests + the naive desk); not one changed behavior.
  The echo is explicit: this is Ch. 20 M2's signature-churn tally, re-earned — while `Resource`
  and `Booking` grew the same fact with a trailing default and cost zero edits. Where a new fact
  lands decides who pays for it; the reader now has both invoices in the same hour.
- **Tally (b) — the method:** `Submit`'s line count before/after (~20 → ~45) and the count of
  distinct business vocabularies in one method: invoices, calendars, floors, conflict math = 4.
  Four vocabularies means four reasons to edit, four reviewers who each understand a quarter of
  the diff, and a `TransferWindow` constant living beside invoice checks that will never use it.
- **The unchosen decision (narrated, not tallied):** test 6 only passes if the transfer check
  sits *before* the engine call — the member with a same-floor clash must hear the engine's
  reason, the cross-floor member must hear the transfer reason. The reader almost certainly got
  this right by accident. The chapter stops on it: *the order of your `if`s is already product
  behavior, and nothing in the code marks it as deliberate.* That sentence is the doorway to
  "The Pattern, Named".

**Beat 4 — test setup bloat (the contract's second tally, folded into (a)+(b)):** the M1
happy-path test arranged a member and a resource; by M2 every desk test arranges member,
resource, floors, and a booking history even when it's testing invoices. The arrange block of
the simplest test grew from 3 lines to 7 — counted on paper next to tally (b) — because in a
one-method design, every test pays entry fees for every rule.

**Beat 5 — the resolution readback (M3):** after extraction the reader re-reads the tallies the
way Ch. 20 did: the four vocabularies now live in four files that can each be read in fifteen
seconds; the invoice test arranges a member and nothing else; the rulebook greps clean of all
four vocabularies. And the unchosen decision from Beat 3 is now a *visible list of registration
lines with a comment explaining the ordering policy* — the thing that was accidental is now the
most legible line of the design. M4's red run and M5's order test then prove it's not just
legible but *guarded*.

**Beat 6 — the payoff contrast (M5):** email #5 lands and the reader never opens working code.
Ch. 20's closing move replayed one level up: back then a new resource type; now a new *policy*.
Two chapters, two axes of growth, same shape of diff — one class, one line, and a spec that went
red first.

---

## Reader time budget

Total nominal **81 minutes**; honest range 65–95 (first run may stretch to ~2h; the chapter
repeats Ch. 20's calibration-not-failure line). Practice rules apply to the whole build: no AI,
type everything including checkpoint tests, 10-minute timer before any hint.

| Milestone | Content | Minutes |
|---|---|---|
| M1 | models + naive desk + 4 tests | 20 |
| M2 | floors + inline transfer + 3 tests + tallies | 12 |
| M3 | interface, 4 rules, rulebook, log, thin desk + 14 tests | 25 |
| M4 | registration + Program scene + 3 smoke tests + scripted red run | 12 |
| M5 | spec-first red, rule + 2 tests, one line, order test, diff proof, tag | 12 |

Slack: ~9 minutes inside the 90 ceiling, absorbed mostly by M3 (the typing-heaviest milestone)
and the two red-run read-and-restore cycles.

---

## Drills (outline — full drafting belongs to the writer, per contract format)

Descend from `DesignPatterns_Drills` (Ch. 21–22 slot). 7 drills + staged hints; final drill
from memory, per contract. Sketch: (1) `IUploadCheck` mini-gauntlet, 3 links, first-veto runner
— 10 min; (2) same pipeline via `Func<Context, Result?>` delegates — the delegate costume,
8 min; (3) add a link *between* two existing links + the order test that proves placement —
10 min; (4) `Order` property variant (the production flavor): sort in the runner, argue
registration-order vs `Order` in two sentences — 12 min; (5) veto vs collect-all-failures:
rewrite the runner to gather every refusal, then answer when each policy is right (validation
UX vs gatekeeping) — 12 min; (6) classification drill, Chain / Strategy / Neither, six
scenarios with key (mirrors Ch. 20 Drill 7 from the other side) — 8 min; (7) The Front Desk
Rulebook, From Memory: interface, three links, runner, registrations, order smoke test, blank
file, tomorrow — 12 min.

---

## Deviations from contract

1. **Designed one installment ahead of execution; reconciled to ch21's designed end state
   (arc continuity gate, 27/07/2026).** The repo verifiably sits at `bookit-ch20`; this design
   binds to `bookit-ch21` as fixed by `ch21-design.md` (engine in `src/Scheduling.Conflicts`
   with `IBookableResource`/`IBookingRecord`, host records carrying the one-line adapter
   clauses, BookIt.Tests 27 + ClinicIt.Tests 7). All suite arithmetic and proofs above assume
   that state. Residual insurance: the only type that invokes the engine is `ConflictRule.cs`
   plus the one `AddConflictDetection()` call inside `AddFrontDesk()`, so any drift in ch21's
   *execution* is absorbed in one file. The ch22 implementer re-verifies the header's engine
   surface against the actual `bookit-ch21` tag before `ch22-M1`.
2. **Repo hygiene: at `bookit-ch20` there is no `.gitignore` and `bin/`/`obj/` are tracked**
   (verified via `git ls-files`; Chapter 20's text calls `dotnet new gitignore` load-bearing,
   but the reference implementation skipped it). **Ch21-M0 performs the repair** (`dotnet new
   gitignore` + `git rm -r --cached …`), so ch22 inherits a clean index and adds no hygiene
   commit of its own. All diff-based proofs here remain path-scoped to source folders anyway —
   defense in depth, and it keeps the printed commands honest against any tree.
3. **Elaborations within the designer's mandate (recorded for transparency, not deviations):**
   `RuleResult` shaped as `(bool Passed, string? Reason)` with `Pass`/`Veto` statics;
   member standing carried as a fact on a `Member` record rather than an invoice-lookup service
   (keeps the chapter about the chain, not about repositories); `Floor` added to `Booking` and
   `BookingSubmission` in addition to the contract-named `Resource` (the transfer rule needs
   both ends of the move); the fifth rule's clock injected as `TimeProvider`; the order-matters
   payoff realized as a permanent two-rulebook test (`RuleOrderTests`) plus a scripted transient
   red run in M4 — the contract's "moves the rule and asserts the reason changes," delivered
   both as evidence and as regression guard.
