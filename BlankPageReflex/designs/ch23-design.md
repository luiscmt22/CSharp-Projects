# Design — Chapter 23

**Chapter 23 — Builder: The Monday Report** (BookIt arc, Part III). Commits `ch23-M1` … `ch23-M5`; tag `bookit-ch23` at the end. Reader budget 65–90 minutes. All practice rules from Chapter 20 apply (no AI, type everything, 10-minute timer, checkpoint tests typed not pasted).

## Repo-state preconditions (verified 27/07/2026)

Designed against the ACTUAL repo plus the arc contract's declared ch21/ch22 end state:

- Verified now: repo at `bookit-ch20` (commits M1–M5), 26 tests green in 35 ms; `BookIt.Domain` has `ResourceTypes` (MeetingRoom/Equipment/Parking), `Resource(Id, Type, Capacity=0, MaintenanceDay=null)`, `BookingRequest(Start, End)`, `Booking(Id, ResourceId, Start, End)`, `Conflict(Existing?, Reason)`. `Program.cs` is a four-verdict demo through `Host.CreateApplicationBuilder`.
- Verified now, and a problem: **the repo has no `.gitignore`; `bin/` and `obj/` are tracked** (215 tracked files; every commit's `--stat` is polluted). Chapter 20's printed setup ran `dotnet new gitignore`, so this is executed-repo drift. See Deviations D2 for how ch23 defends its diff proofs either way.
- Bound upstream (ch21/ch22 not yet executed in the repo, but their designs are binding — see Deviations D1): after ch22, the engine lives in `src/Scheduling.Conflicts`; the BookIt host keeps `src/BookIt` with its `Domain` records (now `Resource : IBookableResource`, `Booking : IBookingRecord`, both with a host-only `Floor`); ch22 leaves behind `BookingRulebook`, `IBookingRule`, and an **in-memory `IRefusalLog`** whose read side ch23 consumes. Exact shapes, pinned from `ch22-design.md` (namespace `BookIt.FrontDesk`):
  - `Refusal(string MemberId, string ResourceId, string RuleName, string Reason, DateTime RequestedStart)` — `RequestedStart` is the refused slot's start; it is what lets refusals be filtered to the report's week.
  - `IRefusalLog` exposing `IReadOnlyList<Refusal> All { get; }`, registered Singleton, resolvable from the container.
  - Suite baseline at `bookit-ch22`: BookIt.Tests 54 + ClinicIt.Tests 7.
- Everything ch23 builds is host-side and new: `src/BookIt/Reports/`. It reads `Resource`/`Booking` from `BookIt.Domain` (untouched since ch20) and `Refusal` from ch22. It does NOT touch `src/Scheduling.Conflicts`, the rulebook, or any strategy — deliberate: the report is a consumer, and its `git diff` footprint stays inside `Reports/` + `Program.cs` + tests for the whole installment. ch23 also deliberately avoids ch22's `Floor`/`OpeningDays` additions (equipment "travel" is counted as checkouts, not floor moves) to keep the cross-chapter surface at exactly one interface: the refusal log.

**The requirement (owner's words, cold open):** "Every Monday morning I want last week's story: which rooms earn their keep, which equipment travels most, what we refused and why. Some weeks I want the detail, some weeks just the headline." Genuinely optional parts; one report record; the Builder chapter.

---

## Milestones

### M1 — The Monday report, fixed menu (~18 min)

**Commit:** `ch23-M1: the Monday report, fixed menu`

**Files added:**
- `src/BookIt/Reports/WeeklyReport.cs` — a `sealed class` that computes itself in its constructor (naive on purpose: report = object that builds itself), plus `sealed record SectionContent`.
- `src/BookIt/Reports/ReportSources.cs` — `ReportSources(Resources, Bookings, Refusals)` record bundling the three inputs. Three args, no builder — planted now so the Boundaries section can point at it later: *this* record never needs the pattern.
- `tests/BookIt.Tests/WeeklyReportTests.cs`

**Files changed:**
- `src/BookIt/Program.cs` — the installment's one demo scene: after the ch22 submissions run, pull `IRefusalLog` from the container, assemble `ReportSources`, print the Monday report (a local `Print(WeeklyReport)` helper; dd/MM/yyyy dates).
- *(Hygiene: ch21-M0 already evicted `bin/`/`obj/` and added the `.gitignore`; ch23 adds no hygiene step. See Deviations D2.)*

**Behavior fixed by M1:** the week is the half-open window `[weekStart, weekStart+7d)`; a booking is in the week iff its `Start` falls inside it, a refusal iff its `RequestedStart` does (boundary-straddling bookings count whole — one honest sentence in the text, not code). Week filtering happens ONCE, before sections — the invariant-filter beat from ch20's router, called back by name. Sections, in order: room usage (booked hours per room, busiest first), equipment travel (checkouts per item, most-travelled first), refusals (grouped by rule, with counts). Headline always computed: totals of bookings and refusals. Default title `"Monday report — week of dd/MM/yyyy"`.

**Checkpoint tests (reader types; the construction seam `FullWeekReport(sources)` is planted here exactly like ch20's `CreateChecker` — it becomes the only surviving call shape in M3):**
1. `Full_Report_Contains_Room_Usage_Equipment_Travel_And_Refusals_In_That_Order` — the fixed menu and its order are pinned.
2. `Room_Usage_Sums_Booked_Hours_Per_Room_Most_Used_First` — the hours math and descending order.
3. `Bookings_Outside_The_Week_Are_Ignored` — half-open week window (a Sunday-23:00 booking of the *previous* week stays out).
4. `Equipment_Travel_Counts_Checkouts_Per_Item_Most_Travelled_First` — per-item counting from ch20-shaped bookings.
5. `Refusals_Section_Groups_Refusals_By_Rule_With_Counts` — reads ch22's `Refusal` list; groups by `RuleName`.
6. `Headline_Totals_Bookings_And_Refusals_For_The_Week` — headline math independent of sections.

**Your-turn box (note form):**
- *Goal:* six checkpoint tests green with the most obvious code: one class, constructor takes `(DateOnly weekStart, ReportSources sources)`, computes headline + three sections, exposes get-only properties.
- *Constraints:* no interfaces, no builder, no patterns — you ship at 17:30 again. All section math private methods of `WeeklyReport`. `SectionContent(Title, Headline, Lines)` as given. Week filter runs once in the constructor.
- *Order if you freeze:* type `SectionContent` and `ReportSources` first (two decisions made for you); then the week filter; then one section at a time against its test, rooms → equipment → refusals; headline last.
- *Done when:* 6 tests green on top of the ch22 suite; `dotnet run` prints the report scene after the booking verdicts; committed.

**Red run:** none (M1 is the calm before).

---

### M2 — Some weeks just the headline (~16 min)

**Commit:** `ch23-M2: options via parameters — the telescoping tally`

**Files changed:**
- `src/BookIt/Reports/WeeklyReport.cs` — the constructor grows to seven parameters; the old two-arg constructor stays and chains (`: this(weekStart, sources, true, true, true)`) — the literal telescoping-constructor formation, two ctors and counting.
- `tests/BookIt.Tests/WeeklyReportTests.cs` — +3 tests that bind to the positional option list ON PURPOSE (they are M3's churn exhibit).
- `src/BookIt/Program.cs` — second call site: the investor headline (`new WeeklyReport(monday, sources, false, false, false, 5, "Investor weekly")`) printed after the owner's full report. The call site is the exhibit: `false, false, false, 5` means nothing at a glance.

**The scripted excursion (in-milestone, backed out before commit):** the mutable-setters escape hatch. ADD a parameterless constructor + public setters + `Compute()` to `WeeklyReport` (the ctor tests still compile — the additions coexist). Reuse one report object for both the owner's report and the investor pack; forget to reset `IncludeRefusals` between them. **The bug IS the milestone: the investor pack prints the refusal log.**

**Checkpoint tests:**
7. `Report_Without_Refusals_Omits_The_Section_But_Keeps_The_Headline` — options actually remove sections; headline is unconditional.
8. `Top_Count_Caps_Every_Ranked_Section` — `topCount: 2` trims rooms AND equipment lists.
9. `Custom_Title_Replaces_The_Default` — `title:` wins; null keeps the default.
- *(Excursion test, typed, run RED, then deleted with the excursion:)* `A_Reused_Report_Object_Does_Not_Leak_Choices_Into_The_Next_Report` — configure full report, `Compute()`, reconfigure for investor headline without resetting `IncludeRefusals`, `Compute()` again; assert no refusals section. **Fails:** `Assert.DoesNotContain` finds `"Refusals"` among the second report's section titles. Read the failure, then `git restore src/BookIt/Reports/WeeklyReport.cs` and delete the test — the assertion returns, green, in M3 test #13.

**Your-turn box (note form):**
- *Goal:* absorb the owner's "some weeks just the headline / skip the refusals for outsiders / top 3 not top 5 / my own title" email with parameters only; make tests 7–9 pass; then run the mutable excursion to its failing end and back it out.
- *Constraints:* no builder, no options record, no new types — parameters are the tool you have. Keep two paper tallies (below). The excursion is mandatory, its deletion is mandatory, and the red test's name goes in the margin — you will meet it again.
- *Order if you freeze:* grow the ctor and chain the old one first (compiler stays green the whole way); tests 7–9 top to bottom; Program's investor call site; excursion last, timer set.
- *Done when:* 9 installment tests green; both report variants print; excursion backed out (`git status` clean of it); tallies written; committed.

**Deliberate red run #1:** the excursion test above — one red assertion proving a half-configured/reused mutable report produces a *wrong* report, not a crash. That distinction is said out loud: the compiler was happy, the tests that existed were happy, and the output was a business incident (refusals shown to investors).

---

### M3 — The extraction: a builder with a staged entry (~22 min)

**Commit:** `ch23-M3: fluent builder, staged entry, immutable report`

**Files added:**
- `src/BookIt/Reports/IReportSection.cs` — one method, `SectionContent Compute(ReportContext context)`. (Footnote beat: a `Func<ReportContext, SectionContent>` would also do — ch20's delegate-vs-interface footnote transfers; classes win here because each section is separately named and separately tested.)
- `src/BookIt/Reports/ReportContext.cs` — week-filtered data + `TopCount`, computed once in `Build()`.
- `src/BookIt/Reports/WeeklyReportBuilder.cs` — **private constructor; the only door is `static WeeklyReportBuilder ForWeekStarting(DateOnly weekStart)`**, which throws `ArgumentException` naming the offending day unless `weekStart.DayOfWeek == DayOfWeek.Monday` (it's the *Monday* report). Flat fluent surface this milestone: `WithTitle`, `Top` (throws on < 1 at the call, not at `Build` — fail where the mistake is), `IncludeRoomUsage`, `IncludeEquipmentTravel`, `IncludeRefusals`, `Build(ReportSources)`.
- `src/BookIt/Reports/Sections/RoomUsageSection.cs`, `Sections/EquipmentTravelSection.cs`, `Sections/RefusalsSection.cs` — the M1 private methods MOVE here (cut-paste-adjust, ch20's M3 discipline: don't re-derive).
- `tests/BookIt.Tests/WeeklyReportBuilderTests.cs`

**Files changed:**
- `src/BookIt/Reports/WeeklyReport.cs` — the class that computed itself becomes a `sealed record` that holds data and nothing else. The type's journey (self-building class → immutable record) is narrated as the chapter's spine.
- `tests/BookIt.Tests/WeeklyReportTests.cs` — `FullWeekReport`'s body becomes a builder chain (ONE edit, tests 1–6 untouched: the seam pays out exactly as `CreateChecker` did); tests 7–9 are rewritten to builder calls and COUNTED — they bound to the positional option list, and the churn is the lesson, not an accident (tally 3).
- `src/BookIt/Program.cs` — both call sites become builder chains; read them aloud against M2's `false, false, false, 5`.

**Checkpoint tests:**
10. `ForWeekStarting_Any_Day_But_Monday_Is_Refused_And_The_Day_Is_Named` — staged entry validates at the door; `Assert.Contains("Thursday", ex.Message)`.
11. `Top_Below_One_Is_Refused_At_The_Call_Not_At_Build` — fail-fast placement is a decision, pinned.
12. `Build_With_No_Content_Chosen_Returns_Headline_Only` — "just the headline" is now the builder's natural default, not a flag combination.
13. `A_Built_Report_Is_Immune_To_Later_Builder_Configuration` — build A; call `IncludeRefusals()` on the same builder; build B; A still has no refusals section. **M2's red test, reborn green** — named as such in the text.
14. `Two_Builds_From_One_Builder_Produce_Independent_Reports` — no shared section lists between built reports.

**Deliberate red run #2 (the contract's designer-pick, executed):** the contract offered compile-time unrepresentability OR one validation test — **this design picks compile-time and proves it mechanically.** A "does not compile" box: the reader types, in a scratch test,
```csharp
var report = new WeeklyReportBuilder().Build(sources);           // CS1729: no 0-arg ctor
var sneaky = new WeeklyReportBuilder(new DateOnly(2026, 7, 13)); // CS0122: ctor inaccessible
```
runs `dotnet build`, reads both compiler errors (exit code non-zero — unfakeable), deletes the two lines, rebuilds green. "A report without a week is not an invalid state you check for; it is a sentence the language no longer lets you say." (Test #10's Monday rule is the runtime *validation* the staged entry still owns — the two guards are contrasted, not conflated.)

**Your-turn box (note form):**
- *Goal:* builder + three section classes + immutable record; tests 1–6 pass with only `FullWeekReport`'s body edited; tests 10–14 green; the does-not-compile box witnessed.
- *Constraints:* `WeeklyReportBuilder`'s constructor is private — `ForWeekStarting` is the only entry; `Build()` assembles `ReportContext` once (week filter lives there, nowhere else — grep the sections for `AddDays(7)` and find nothing); `WeeklyReport` ends the milestone with zero methods; section bodies are MOVED, not rewritten. Mutable state is allowed in exactly one place: inside the builder (that containment IS the pattern — say it in a one-line comment).
- *Order if you freeze:* `SectionContent`/`ReportContext` compile first; `IReportSection` + `RoomUsageSection` (move the method, run tests 2–3); the other two sections; the builder skeleton with `Build`; swap `FullWeekReport`'s body; rewrite 7–9; Program last. Never more than one step from green.
- *Done when:* 14 installment tests green; tests 1–6 untouched outside the seam (diff the file and check); compile box done and deleted; committed.

---

### M4 — Two facets: Content and Presentation (~10 min)

**Commit:** `ch23-M4: content and presentation facets`

**Files added:**
- `src/BookIt/Reports/ContentFacet.cs` — `RoomUsage()`, `EquipmentTravel()`, `Refusals()`, each returning the facet for chaining; writes into the builder's internal `ReportSpec`.
- `src/BookIt/Reports/PresentationFacet.cs` — `Title(string)`, `Top(int)` (the < 1 throw moves with it).

**Files changed:**
- `src/BookIt/Reports/WeeklyReportBuilder.cs` — flat `Include*/WithTitle/Top` surface REPLACED by `Content(Action<ContentFacet>)` and `Presentation(Action<PresentationFacet>)`; internal `sealed class ReportSpec` (mutable, never escapes) becomes the shared state both facets write.
- `tests/BookIt.Tests/WeeklyReportTests.cs` + `WeeklyReportBuilderTests.cs` — helper bodies, PLUS the three builder tests that name flat methods (11: `Top`, 13: `IncludeRefusals`, 14: `Include*` between builds) re-spelled to the facet surface. Counted out loud as tally (c)'s coda: deleting-not-aliasing has a price, these three call-shape edits are the whole invoice, and every assertion survives unchanged. +2 new tests.
- `src/BookIt/Program.cs` — the owner call site reads as two grouped clauses:
  `WeeklyReportBuilder.ForWeekStarting(monday).Content(c => c.RoomUsage().EquipmentTravel().Refusals()).Presentation(p => p.Title("Week 29 — full").Top(3)).Build(sources)`.

**Design decision, argued in the text:** facets are lambda-configured, not property-hopped (`.Content(c => …)` rather than `.Content.RoomUsage().And.…`). The lambda's braces make the grouping visible at the call site, return-to-root is automatic (no `And`/`Done` gymnastics), and the reader already knows the idiom from `services.AddCors(o => …)`. Recorded as Deviation D3 (interpretation of the contract's `.Content`/`.Presentation` notation).

**Checkpoint tests:**
15. `Content_And_Presentation_Facets_Compose_In_A_Single_Chain` — both lambdas, one `Build`; title, top and section set all land.
16. `An_Empty_Content_Lambda_Still_Builds_The_Headline_Report` — `Content(c => { })` equals not calling it; defaults stay honest.

**Your-turn box (note form):**
- *Goal:* regroup the builder's surface into the two facets with zero behavior change; tests 15–16 green; every pre-existing test green with edits confined to helper bodies plus the three counted call-shape re-spellings (tests 11, 13, 14 — assertions untouched).
- *Constraints:* facet classes hold no state of their own — both write the one internal `ReportSpec`; the flat methods are deleted, not kept as aliases (two spellings of one API is how docs rot); `ForWeekStarting`/`Build` stay on the root.
- *Order if you freeze:* extract `ReportSpec` first (rename-refactor, still flat, still green); then `ContentFacet` and delete the three `Include*`; then `PresentationFacet`; helpers; Program.
- *Done when:* 16 installment tests green; `git diff` for this milestone touches no file under `Sections/`; committed.

**Red run:** none scripted (the milestone is a pure-surface refactor; its proof is the untouched `Sections/` diff).

---

### M5 — Parking occupancy: one section, one method (~13 min)

**Commit:** `ch23-M5: parking occupancy — one section, one method`
**Tag after commit:** `bookit-ch23`

The owner again: "How full is the parking, actually? I'm thinking of renting two more spaces." A NEW optional part arrives — the payoff milestone, run test-first.

**Files added:**
- `src/BookIt/Reports/Sections/ParkingOccupancySection.cs` — occupied hours per space from week bookings of `ResourceTypes.Parking` resources, most-occupied first, honours `TopCount`.
- `tests/BookIt.Tests/ParkingOccupancySectionTests.cs`

**Files changed:**
- `src/BookIt/Reports/ContentFacet.cs` — ONE method: `public ContentFacet ParkingOccupancy()` (adds the section to the spec; ~3 lines).
- `src/BookIt/Program.cs` — `.ParkingOccupancy()` joins the owner's content lambda; one new output line.

**Checkpoint tests:**
17. `Parking_Occupancy_Sums_Occupied_Hours_Per_Space_Most_Occupied_First` — the math and the order.
18. `Parking_Occupancy_Ignores_Bookings_For_Other_Resource_Types` — a room booking never counts as parking.
19. `Parking_Occupancy_Honours_Top_Count` — the presentation knob reaches the new section for free.
20. `Parking_Occupancy_Arrives_Through_The_Content_Facet` — one builder chain with `c.ParkingOccupancy()`; the section title appears in the built report.

**Deliberate red run #3 (light, ch20-M5 rhythm):** type test file 17–19 FIRST; `dotnet build` fails with `CS0246: ParkingOccupancySection not found` — the spec exists before the code. Create the class, go green, then test 20, then the facet method.

**Your-turn box (note form):**
- *Goal:* the new section end-to-end, in this order: tests (red) → section class (green) → facet method → demo line → the diff proof.
- *Constraints:* you may not open `WeeklyReportBuilder.cs`, `WeeklyReport.cs`, `PresentationFacet.cs`, or any existing file under `Sections/` — not even to look; the proof below verifies you didn't need to.
- *Order if you freeze:* copy `EquipmentTravelSection`'s skeleton shape from memory (it's the same fold: group → aggregate → order → take); hours instead of counts is the only new math.
- *Done when:* 20 installment tests green on top of the ch22 baseline; `dotnet run` prints the parking line; the payoff proof procedure below runs clean; committed and tagged.

---

## New/changed domain types (exact C# signatures)

```csharp
// src/BookIt/Reports/ReportSources.cs (M1, never changes)
// Refusal comes from ch22: BookIt.FrontDesk.Refusal (see preconditions for its pinned shape).
public sealed record ReportSources(
    IReadOnlyList<Resource> Resources,
    IReadOnlyList<Booking> Bookings,
    IReadOnlyList<Refusal> Refusals);

// src/BookIt/Reports/WeeklyReport.cs (M1, and grown in M2 — the naive shape)
public sealed record SectionContent(
    string Title,
    string Headline,
    IReadOnlyList<string> Lines);

public sealed class WeeklyReport                       // M1
{
    public WeeklyReport(DateOnly weekStart, ReportSources sources);
    public DateOnly WeekStart { get; }
    public string Title { get; }
    public string Headline { get; }
    public IReadOnlyList<SectionContent> Sections { get; }
}

public WeeklyReport(                                   // M2 addition — telescoping; 2-arg ctor chains to it
    DateOnly weekStart,
    ReportSources sources,
    bool includeRoomUsage,
    bool includeEquipmentTravel,
    bool includeRefusals,
    int topCount = 5,
    string? title = null);

// M2 excursion only — added, condemned red, backed out before commit:
public WeeklyReport();                                 // + setters on all options + public void Compute()

// src/BookIt/Reports/WeeklyReport.cs (M3 — final shape, unchanged through M5)
public sealed record WeeklyReport(
    DateOnly WeekStart,
    string Title,
    string Headline,
    IReadOnlyList<SectionContent> Sections);

// src/BookIt/Reports/IReportSection.cs (M3)
public interface IReportSection
{
    SectionContent Compute(ReportContext context);
}

// src/BookIt/Reports/ReportContext.cs (M3) — week-filtered once, in Build()
public sealed record ReportContext(
    DateOnly WeekStart,
    IReadOnlyList<Resource> Resources,
    IReadOnlyList<Booking> WeekBookings,
    IReadOnlyList<Refusal> WeekRefusals,
    int TopCount);

// src/BookIt/Reports/WeeklyReportBuilder.cs (M3 flat surface)
public sealed class WeeklyReportBuilder
{
    private WeeklyReportBuilder(DateOnly weekStart);                  // the staged entry's teeth
    public static WeeklyReportBuilder ForWeekStarting(DateOnly weekStart); // throws unless Monday
    public WeeklyReportBuilder WithTitle(string title);
    public WeeklyReportBuilder Top(int count);                        // throws if < 1
    public WeeklyReportBuilder IncludeRoomUsage();
    public WeeklyReportBuilder IncludeEquipmentTravel();
    public WeeklyReportBuilder IncludeRefusals();
    public WeeklyReport Build(ReportSources sources);
}

// src/BookIt/Reports/WeeklyReportBuilder.cs (M4 — flat surface replaced)
public sealed class WeeklyReportBuilder
{
    private WeeklyReportBuilder(DateOnly weekStart);
    public static WeeklyReportBuilder ForWeekStarting(DateOnly weekStart);
    public WeeklyReportBuilder Content(Action<ContentFacet> configure);
    public WeeklyReportBuilder Presentation(Action<PresentationFacet> configure);
    public WeeklyReport Build(ReportSources sources);

    internal sealed class ReportSpec       // mutable on purpose, and only here —
    {                                      // containment behind Build() is the trade
        public string? Title;
        public int TopCount = 5;
        public List<IReportSection> Sections = [];
    }
}

// src/BookIt/Reports/ContentFacet.cs (M4; +ParkingOccupancy in M5)
public sealed class ContentFacet
{
    internal ContentFacet(WeeklyReportBuilder.ReportSpec spec);
    public ContentFacet RoomUsage();
    public ContentFacet EquipmentTravel();
    public ContentFacet Refusals();
    public ContentFacet ParkingOccupancy();            // M5 — the one fluent method
}

// src/BookIt/Reports/PresentationFacet.cs (M4)
public sealed class PresentationFacet
{
    internal PresentationFacet(WeeklyReportBuilder.ReportSpec spec);
    public PresentationFacet Title(string title);
    public PresentationFacet Top(int count);           // throws if < 1
}

// src/BookIt/Reports/Sections/*.cs (M3; parking in M5)
public sealed class RoomUsageSection : IReportSection        { public SectionContent Compute(ReportContext context); }
public sealed class EquipmentTravelSection : IReportSection  { public SectionContent Compute(ReportContext context); }
public sealed class RefusalsSection : IReportSection         { public SectionContent Compute(ReportContext context); }
public sealed class ParkingOccupancySection : IReportSection { public SectionContent Compute(ReportContext context); }
```

No DI registrations: the builder is a local object with the lifetime of one expression — a named teaching beat (not everything belongs in the container; contrast with ch20's strategies, which DI *selects* among; nothing selects a builder). The only container touch is reading `IRefusalLog` in `Program.cs`.

## Payoff proof procedure (exact commands and expected outputs)

Run from the repo root after the `ch23-M5` commit. Pathspecs are explicit so the proof is immune to the tracked-`bin/` defect even if it survives (Deviations D2).

```bash
M4=$(git log --format=%h --grep='ch23-M4' -1)     # or read it off `git log --oneline -6`

# 1. The whole cost of a new optional report part:
git diff --stat $M4..HEAD -- src/BookIt/Reports src/BookIt/Program.cs tests/BookIt.Tests
# Expected shape (±line counts):
#  src/BookIt/Program.cs                                  |  3 +-
#  src/BookIt/Reports/ContentFacet.cs                     |  7 ++++
#  src/BookIt/Reports/Sections/ParkingOccupancySection.cs | 3x ++++++++++++ (new)
#  tests/BookIt.Tests/ParkingOccupancySectionTests.cs     | 7x ++++++++++++ (new)
#  4 files changed …

# 2. The sharper, negative proof — the machinery was not even opened:
git diff $M4..HEAD -- \
  src/BookIt/Reports/WeeklyReport.cs \
  src/BookIt/Reports/WeeklyReportBuilder.cs \
  src/BookIt/Reports/PresentationFacet.cs \
  src/BookIt/Reports/Sections/RoomUsageSection.cs \
  src/BookIt/Reports/Sections/EquipmentTravelSection.cs \
  src/BookIt/Reports/Sections/RefusalsSection.cs
# Expected output: NOTHING. Empty. Build(), the record, the other facet,
# every existing section — untouched, and git is the notary.

# 3. Suite and scene:
dotnet test          # all green, still ~1s:
# Passed! - Failed: 0, Passed: 74, Skipped: 0, Total: 74 … BookIt.Tests.dll   (54 + 20 ch23)
# Passed! - Failed: 0, Passed:  7, Skipped: 0, Total:  7 … ClinicIt.Tests.dll
dotnet run --project src/BookIt
# Expected tail of output (dates dd/MM/yyyy):
#   === Monday report — week of 13/07/2026 ===
#   Week of 13/07: 3 bookings, 3 refusals      <- the three ch22-scene refusals, week-filtered
#   Room usage — busiest: ROOM-ATLAS (1.0h)
#     …
#   Parking occupancy — busiest space: PARK-12 (4.0h)
#     …

git tag bookit-ch23
```

Mid-chapter mechanical checks already scripted above: the M3 does-not-compile box (`dotnet build` exits non-zero on CS1729 + CS0122, then green after deletion) and the M5 test-first CS0246 red.

## Felt-pain narrative beats (with the paper tallies)

1. **M2, tally (a) — "trips to the signature."** Three call sites of the seven-parameter constructor exist by mid-M2 (two in `Program.cs`, one per new test). The reader keeps a tick every time they must jump to or hover the constructor to decode a positional argument. The scripted moment: cover the signature with a hand and read `new WeeklyReport(monday, sources, false, false, false, 5, null)` aloud — *which* false is refusals? Expected tally: 4–6 ticks in fifteen minutes, in a codebase they wrote that same hour. The text then names the free counterweight — C# named arguments — and is honest that they cure the *reading* problem entirely… and nothing else on this list.
2. **M2, tally (b) — "bugs the compiler waved through."** The reader writes the investor call site from memory, then diffs against intent: the two adjacent booleans (`includeEquipmentTravel`, `includeRefusals`) transpose without a squiggle — same type, no defense. One tick minimum, guaranteed by construction (the box instructs writing it from memory first). This is the felt version of "a parameter list is a positional protocol with no schema."
3. **M2, the excursion — "states that are not a report."** Before running the red test, the reader lists on paper every state the mutable object can occupy that is not a valid report: unset week, `TopCount = 0`, configured-but-not-computed, computed-then-reconfigured (stale), reused-across-audiences. Expected: 5+. Then the red run makes one of them concrete: **the investor pack prints the refusal log** — not an exception, a wrong report with a business sting, caught only because a test looked. The chapter's thesis lands here: the builder's job is to make every item on that paper list either unrepresentable (no week → no builder exists) or transient inside `Build()`.
4. **M3, tally (c) — "which tests churned, and why."** The seam-guarded tests 1–6 survive the API replacement with one helper-body edit (ch20's certificate, re-earned); tests 7–9, which bound to the positional option list, all have to be rewritten. Count: 6 survivors, 3 casualties, and the casualties all share one property — they knew the parameter order. Coupling to a telescoping signature is contagious; the tally makes it a number instead of a sermon.
5. **M4/M5, the payback ledger.** M2's tallies are re-read against the M5 diff: the same owner-email class of change ("one more optional part") cost, in M2's world, edits to a constructor every caller shares — and costs, in M5's world, one new file plus three lines in a facet, with the negative diff proving nothing else moved. The reader has both invoices in their own handwriting, ninety minutes apart — the ch20 M2-vs-M5 rhyme, replayed on a different pattern.

Boundary beats carried by the chapter (contract-mandated "when NOT to build a builder"): `ReportSources` — three args, a record, no builder, on purpose, sitting in the same folder; named+optional arguments as the honest first remedy (they fix beat 1 for free; they cannot fix beats 2–3, staging, or open-ended section growth); records' `with` as immutability's escape valve (copies, never mutations); and the rule of thumb said with a straight face — *two constructor args need no pattern, and most types with five don't either; the builder earned its keep here only when parts became genuinely optional, growing, and dangerous to half-configure.*

## Reader time budget

| Milestone | Content | Minutes |
|---|---|---|
| M1 | Fixed-menu report, 6 tests, demo scene | 18 |
| M2 | Telescoping growth, 3 tests, mutable excursion + red run + back-out, tallies | 16 |
| M3 | Builder + staged entry + 3 section classes + immutable record, 5 tests, compile box | 22 |
| M4 | Facet refactor, 2 tests | 10 |
| M5 | Parking section test-first, 4 tests, diff proof, tag | 13 |
| **Total** | | **79** (range 65–90; first run may stretch to ~110 — calibration, not failure) |

## Drills (outline — full drafting belongs to the writer, per contract format)

Descend in spirit from the Builder exercise set
(`C:\Users\User\source\repos\luiscmt22\CSharpProjects\GeneralExercises`), per contract. 7 drills +
staged hints, Chapter 20's exact format (timed, goal, self-check `- [ ]`, hints at the end);
final drill from memory. Sketch: (1) `SandwichBuilder` — smallest fluent builder, immutable
result, 10 min; (2) telescoping-to-builder refactor on a given 6-arg constructor, keeping its
tests green through one seam edit — 12 min; (3) staged entry: make "no week chosen" a sentence
the compiler refuses (`private` ctor + static door), witness the CS error — 8 min; (4) the
mutable-escape bug reproduced in miniature: reused config object leaks a flag; then contain the
mutability inside `Build()` — 12 min; (5) facet split: regroup a 6-method flat surface into two
lambda-configured facets, zero behavior change — 12 min; (6) judgment, no code: six types, decide
builder / named-args / plain ctor, with the two-args-need-no-pattern rule applied at least twice
— 8 min; (7) The Monday Report Builder, From Memory: staged entry, two facets, one section
interface, `Build()`, blank file, tomorrow — 15 min.

## Deviations from contract

- **D1 — Designed ahead of ch21/ch22 execution; reconciled to their design docs (arc continuity gate, 27/07/2026).** The repo verifiably sits at `bookit-ch20` (no engine extraction, no rulebook, no refusal log). This design binds to the ch21/ch22 designs' declared end states, with the consumed shapes pinned verbatim in "Repo-state preconditions": `BookIt.FrontDesk.Refusal(MemberId, ResourceId, RuleName, Reason, RequestedStart)` and `IRefusalLog.All`, suite baseline 54 + 7. The ch23 implementer verifies those pins against the actual `bookit-ch22` tag before `ch23-M1`; any mismatch is an upstream execution drift to fix upstream, not to absorb here. *Reason:* installments are designed in parallel against the binding arc contract; silent guessing would be drift, so the pins are explicit instead.
- **D2 — Repo tracks `bin/`/`obj/` with no `.gitignore` at `bookit-ch20`, contradicting Chapter 20's printed setup** (215 tracked files, 198 of them build artifacts; every historical `--stat` is polluted). **Ch21-M0 performs the repair** as a scripted beat, so ch23 inherits a clean index and carries no hygiene step. Independently, every proof command in this design uses explicit pathspecs so the payoff diffs stay clean against any tree. *Reason:* the arc's proofs are diffs; a noisy history fakes nothing but obscures everything.
- **D3 — Facets are lambda-configured (`Content(Action<ContentFacet>)`), not property-hopped (`.Content` returning a facet).** The contract writes "`.Content` and `.Presentation` facets"; this design keeps the names and the two-facet split but configures each through a lambda. *Reason:* return-to-root is automatic (no `And`/`Done` connective ceremony at this project's size), the call-site braces make the grouping *visible*, and the reader already owns the idiom from `IServiceCollection` options lambdas. The red-run choice inside the contract's own either/or (compile-time unrepresentable over validation-test, M3) is a mandated designer pick, recorded there, not a deviation.
