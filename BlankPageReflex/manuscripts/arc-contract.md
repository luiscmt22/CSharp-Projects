# BookIt Arc Contract — Part III, Chapters 21–24

This is the whole-arc design. Installment designers elaborate WITHIN it; implementers
execute it; writers narrate what was actually executed. Deviating from a contract item
requires recording the deviation and its reason in the design doc — silent drift is a defect.

## Canonical facts

- Repo: `scratchpad/book/BookIt` (git; tag `bookit-ch20` = Chapter 20 end state; 26 tests green).
- Chapter 20 left behind: `ConflictChecker` (pure router, public signature
  `FindConflicts(Resource, BookingRequest, IReadOnlyList<Booking>)`), `IConflictStrategy`
  (self-identifying `ResourceType`, pre-filtered `existingBookings`), three strategies
  (MeetingRoom: 15-min buffer ×2 when Capacity ≥ 12; Equipment: whole-day checkout + MaintenanceDay;
  Parking: plain half-open overlap), `AddConflictDetection()` as the single composition point,
  smoke tests incl. `Every_Expected_Resource_Type_Resolves_To_A_Strategy`, demo `Program.cs`.
- Domain (`Domain/Models.cs`): `ResourceTypes` constants; `Resource(Id, Type, Capacity=0, MaintenanceDay=null)`;
  `BookingRequest(Start, End)`; `Booking(Id, ResourceId, Start, End)`; `Conflict(Existing?, Reason)`.
- Practice rules apply to the whole build (no AI, type everything, 10-min timer).
- Reader budget per installment: 60–90 minutes. Each installment: 4–5 milestones, your-turn boxes
  (Goal / Constraints / Order-if-you-freeze / Done when), checkpoint tests the reader types,
  at least one deliberate red run, a payoff proved by `git diff --stat` or an equivalent
  mechanical, unfakeable check.
- Commit messages: `ch21-M1: …` through `ch24-M4: …`. Tag `bookit-ch21` … `bookit-ch24` at each
  installment's end. Every commit leaves ALL tests green except inside a scripted red-run step.
- Polite code throughout: intention-revealing names, early returns, no comments that narrate code.
  Comments only for decisions (lifetime choices, boundary trade-offs) — Chapter 20 set the tone.
- `Program.cs` is the demo harness; each installment may extend it with one scene. It is never
  a protected seam.
- Chapter titles are FIXED (TOC will be updated to match):
  - Ch. 21 — Adapter: Your Engine, Their Models
  - Ch. 22 — Chain of Responsibility: The Front Desk Rulebook
  - Ch. 23 — Builder: The Monday Report
  - Ch. 24 — Observer: Three Places Hear One Booking

## Chapter 21 — Adapter: Your Engine, Their Models

**Requirement.** A second client — a private physiotherapy clinic — rents therapy rooms by the
hour and wants BookIt's conflict engine. Their codebase already has load-bearing models with
different shapes: `TherapyRoom(string Code, …)`, `Appointment(Guid Id, string RoomCode,
DateTime StartsAt, int DurationMinutes)`. They will not adopt `BookIt.Domain`, and they are right
not to.

**Felt pain (naive first).** Two dead ends the reader must *see*, not be told about: (a) copy the
engine into the clinic solution — Chapter 20's copy-paste road at project scale; (b) make the
clinic's types inherit/convert to BookIt's records — invasive, and breaks the moment either
business evolves. The milestone that hurts: try (b) far enough to feel it (a mapping layer full of
lossy conversions), tally the damage, back it out.

**Extraction.** The engine moves to a new class library `src/Scheduling.Conflicts` that OWNS its
contracts: `IBookableResource { Id, Type, Capacity, MaintenanceDay }` and
`IBookingRecord { ResourceId, Start, End }` (exact members are the designer's call, but the
principle is fixed: the feature defines the interfaces; hosts adapt). `ConflictChecker`,
`IConflictStrategy`, the three strategies, and the registration extension move there and are
rewritten against the interfaces. The engine project has ZERO references to any host project —
this is checked mechanically (`grep -L`/csproj inspection is part of a checkpoint).
BookIt adapts its records (`sealed record`s can implement interfaces directly — that is itself a
teaching beat: sometimes the adapter is one line of `: IBookableResource`); the clinic needs a real
adapter class (`AppointmentBookingAdapter`: `End = StartsAt.AddMinutes(DurationMinutes)`).
New host: `src/ClinicIt` console + `tests/ClinicIt.Tests`.

**Payoff.** The clinic host lands — new booking rules NOT needed, same three strategies? No:
the clinic uses only MeetingRoom-style rules for rooms; the point is the engine ran unmodified.
Proof: (1) `git diff --stat` from the milestone where the engine project stabilized to HEAD shows
zero changes inside `src/Scheduling.Conflicts/`; (2) the engine `.csproj` has no ProjectReference;
(3) both hosts' suites green (all pre-existing 26 tests survive, relocated or adapted only where
the design doc says so and counts it).

**Journeying On →** the front desk's rulebook (Ch. 22) — validation that isn't about time overlap
at all.

## Chapter 22 — Chain of Responsibility: The Front Desk Rulebook

**Requirement burst (arrives as a forwarded email list).** Before any conflict math runs:
(1) members with unpaid invoices can't book; (2) bookings only during the space's opening days —
model as `[Flags] enum OpeningDays` (in the full book this piece arrives from Ch. 5; here it is
built inline in ten minutes, and the chapter says so in one sentence); (3) travelling equipment
needs a 15-minute transfer window between bookings on *different floors* (add `Floor` to
`Resource`; equipment bookings know their floor); (4) everything that passes the rules still goes
through the Ch. 20/21 conflict engine.

**Felt pain.** The naive version: a `BookingDesk.Submit()` method that accumulates the rules as
a growing wall of ifs with inconsistent early-exits — the reader writes rules 1–2 that way, then
rule 3 makes the method unreadable and the tallies start (edits to the same method, test setup
bloat).

**Extraction.** `IBookingRule` links with a self-describing `RuleName` and
`RuleResult Check(BookingSubmission submission)` (designer fixes exact shapes); an ordered
pipeline `BookingRulebook` that runs links in REGISTRATION ORDER and stops at the first veto
(gauntlet semantics — contrast with Ch. 20's Strategy selection is a named teaching beat: Drill 7's
two slogans come home). The conflict engine joins as the LAST link (`ConflictRule` wrapping
`ConflictChecker`) — Strategy inside Chain, patterns composing, said explicitly. Refused
submissions are recorded to an `IRefusalLog` (in-memory) — one line of foreshadowing to Ch. 23,
no more. DI: `IEnumerable<IBookingRule>` preserves registration order — the chapter proves this
with a checkpoint test rather than asserting it in prose.

**Payoff.** A fifth rule (advance-booking window: nothing more than 60 days out) lands as one
class + one registration line; `git diff --stat` shows zero changes to the rulebook and the four
existing rules. Plus an order-matters demonstration: a checkpoint test moves the cheap
member-standing rule after the expensive conflict rule and asserts the veto *reason* changes —
order is behavior, which is why registration order lives in one place.

**Journeying On →** the owner reads refusals and asks for the Monday report (Ch. 23).

## Chapter 23 — Builder: The Monday Report

**Requirement.** The owner: "Every Monday morning I want last week's story: which rooms earn
their keep, which equipment travels most, what we refused and why. Some weeks I want the detail,
some weeks just the headline." A `WeeklyReport` with genuinely optional parts.

**Felt pain.** The telescoping constructor, honestly grown: the reader ships
`new WeeklyReport(weekStart, includeRooms, includeEquipment, includeRefusals, topN, title, …)`
— then meets it at a call site three weeks later and can't read it (`true, false, true, 5, null`).
The mutable-setters escape hatch is tried and rejected on-screen: a half-configured report object
escapes into a method and produces a wrong report — the bug IS the milestone.

**Extraction.** A fluent builder with a staged entry (`WeeklyReportBuilder.ForWeekStarting(DateOnly)`
returns the builder; `Build()` validates completeness and returns an IMMUTABLE `WeeklyReport`).
Then the faceted move, small and honest: `.Content` and `.Presentation` facets grouping related
choices (the corpus's fluent → generic → faceted progression, scaled to this project's size —
faceted builder only where two facets genuinely exist, and the Boundaries section says when NOT
to build a builder: two constructor args need no pattern). Data comes from what the arc already
produces: bookings (Ch. 20/21) and the refusal log (Ch. 22).

**Payoff.** A new section — parking occupancy — arrives as one section class + one fluent method,
zero edits to `Build()`, existing sections, or the report record. Red run: `Build()` without
`ForWeekStarting` is unrepresentable by design (compile-time, shown in a "does not compile" box)
OR fails one precise validation test — designer picks ONE and proves it.

**Journeying On →** the report is pulled every Monday; the owner wants the front desk display,
the porter, and the stats to react the moment a booking lands (Ch. 24).

## Chapter 24 — Observer: Three Places Hear One Booking

**Requirement.** When a booking is confirmed, three places must react: the front-desk display
refreshes, a porter gets a move task when travelling equipment changes floors, and utilization
stats accumulate (the same numbers Ch. 23 reports weekly). Chapter 8 owns delegate/event
mechanics; THIS chapter owns the architectural decision — when events beat direct calls, and
what goes wrong operationally.

**Felt pain.** `BookingDesk` (from Ch. 22) grows three direct dependencies and three sequential
calls in `Submit()`; the tally: constructor bloat, test setup requiring all three collaborators,
the porter change forcing a desk recompile. Then the trap the corpus documents: someone "fixes"
coupling with a `static event` — and the chapter REPRODUCES the production bug in xUnit: two desk
instances (standing in for two Blazor circuits / two tenants) cross-talk through the static event,
and a subscriber outlives its owner (leak demonstrated via a WeakReference-based checkpoint test
or an invocation-count cross-talk test — designer picks the sharper one and executes it red first).

**Extraction.** An instance event `BookingConfirmed` on `BookingDesk` (`event EventHandler<BookingConfirmedEventArgs>`
or `Action<…>` — designer justifies the choice in one comment), subscribers as small classes
(`FrontDeskDisplay`, `PorterDispatch`, `UtilizationStats`) each self-subscribing and implementing
`IDisposable` to unsubscribe — the unsubscribe discipline is a checkpoint test, not a homily.
The static-event version is written, condemned by the failing tests, and replaced — the reader
performs the fix.

**Payoff.** `UtilizationStats` (third subscriber) lands with `git diff --stat` showing zero
changes to `BookingDesk` and both existing subscribers. Closing beat scales the idea up honestly:
same shape, process-wide, is an event bus; cross-process, it's SignalR — the "In the wild"
sections carry it (notification system; the static-event bug that leaked state across circuits).

**Journeying On →** closes the ARC: what the reader built across five chapters, the git log as
the story, and the bridge to Part IV (reading production systems this size for real).

## In-the-wild sources (writers: ground every production claim HERE, invent nothing)

- Ch. 21: `_Curated\CORE\patterns\adapter-pattern-module-decoupling.md`;
  `_Curated\_extracted\Education__03-Adapter-Pattern-Explained.md.txt`
- Ch. 22: `_Curated\APPLICATIONS\scheduling\givingcare-validator-pipeline.html` (or its
  `_extracted` .txt twin `Education__29-Validator-Pipeline-Chain-Of-Responsibility.html.txt`);
  `_Curated\CORE\patterns\notification-recipient-resolver-pipeline.md`
- Ch. 23: `_Curated\CORE\language\csharp-generics-and-functional-builders.html`;
  `_Curated\_extracted\Education__generics-builders-deep-dive.html.txt`
- Ch. 24: `_Curated\CORE\frontend\blazor-static-events-shared-state-bug.md`;
  `_Curated\CORE\language\csharp-events-godot-bridge.html`
- Corpus root: `C:\Users\User\Desktop\AGILE\Learning\_Curated`
- Anonymization: use the book's placeholder names — "the care-workforce platform", "the HR module",
  "the factory-floor tracker", "the stock service". NEVER the real product names (GivingCare,
  Furnor, Joti, HRModule) — they appear in corpus docs but must not appear in manuscripts.

## Drills

Each chapter closes with 6–8 drills + staged hints in exactly Chapter 20's format (timed, goal,
self-check checkboxes `- [ ]`, hints section, and a key only where a drill is classification-style).
Where an existing drill set covers the pattern (`DesignPatterns_Drills` for 21–22; the Builder
exercise set for 23 — see `C:\Users\User\source\repos\luiscmt22\CSharpProjects\GeneralExercises`),
drills should descend from it in spirit; Ch. 24's set is authored fresh (its INDEX slot is
"planned"). Final drill of every chapter is always "…, From Memory" — the full pattern from a
blank file, tomorrow.
