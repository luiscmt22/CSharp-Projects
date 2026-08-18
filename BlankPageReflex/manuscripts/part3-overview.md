This page is the rest of Part III — the four chapters that continue the BookIt build Chapter 20 opened (Chapter 20 itself lives in the Book Demo, alongside the proposal and the full table of contents). Together the five chapters are one arc: a single system that meets a new requirement in every chapter and gets out of trouble the honest way each time.

The arc in numbers, every one of them executed rather than estimated:

| Ch. | Installment | Pattern earns its keep when… | Tag | Suite after |
|---|---|---|---|---|
| 20 | The conflict checker | three kinds of resource refuse bookings for different reasons | `bookit-ch20` | 26 green |
| 21 | Your engine, their models | a second business wants the engine without adopting your types | `bookit-ch21` | 34 green |
| 22 | The front desk rulebook | the rules that aren't about time overlap outgrow one method | `bookit-ch22` | 61 green |
| 23 | The Monday report | a constructor's argument list stops being readable | `bookit-ch23` | 81 green |
| 24 | Three places hear one booking | the desk collects dependencies that only want to listen | `bookit-ch24` | 94 green |

**What the continuity buys.** A project per chapter would have produced four throwaways teaching the same lesson four times: here is a pattern, here is a toy that needs it. An arc teaches what a throwaway cannot — that a design decision is a debt or a dividend, paid in the chapters that follow. Chapter 21's engine extraction is what makes Chapter 22's rulebook composable. Chapter 22's refusal log is what gives Chapter 23's report something worth reporting. Chapter 23's utilization numbers are what Chapter 24's third subscriber accumulates. By the time that subscriber lands without touching the booking desk, the reader has watched open/closed pay out four times on code they typed themselves — and the git log is the receipt.

The cost is the reader who joins at Chapter 22 with no repository, and the book pays it explicitly rather than hoping: tagged checkpoints, a state-check box at every chapter opening, and a *Journeying onwards* note naming exactly what the next chapter assumes. The arc rewards continuity; it doesn't punish arrival.

**How to work these chapters.** The practice rules from the front matter apply to the whole build: no AI, no completions, type everything — including the checkpoint tests. Each chapter opens from the previous tag, so the arc can be joined anywhere: `git checkout bookit-ch22` hands you a working system with 61 green tests and no questions asked. If you built along from Chapter 20, your own repo is the better starting point — the chapters' state-check boxes tell you how to confirm you're standing where the text assumes.

**Provenance.** Every listing on this page was extracted from the commit where the reader produces it, every red run was actually run red, and every payoff is certified the same way Chapter 20's was — `git diff --stat`, with the router, the engine, or the desk provably untouched. The full commit history (`M1`…`M5`, then `ch21-M0`…`ch24-M4`) and all five tags ship in the companion repository.
