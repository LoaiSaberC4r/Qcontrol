# QControl Kiosk backend stabilization report

Branch: `codex/kiosk-backend-stabilization`

Repository root: `E:\QControl`

Validation date: 2026-11-03

Implemented the seven approved fixes. The existing HTTP search contract supports historical lookup when the original input ID is retained. Lookup after permanent deletion nulls that ID remains blocked by the existing contract, as detailed below. No frontend or database model changes were made.

## A. Bug fix summary

### KSK-08 — Runtime service eligibility

**Root cause:** The runtime checker only read the selected service's activity, branch assignment, and schedule. It did not verify the branch, hierarchy, deletion, leaf status, or service scope.

**Files:**

- `QControl.infrastructure/Services/BranchServiceAvailabilityChecker.cs`
- `QControl.Application/Properties/AssemblyInfo.cs`
- Regression files listed under KSK-08 in section D.

Kiosk and reservation availability project the requested service by ID, check for children with an indexed existence predicate, and validate its ancestor chain with a parameterized recursive SQL Server query when needed. Strict eligibility requires an active existing branch, effective ancestor activity, no children, Global scope or matching BranchScoped ownership, and the existing branch assignment and schedule. Kiosk issuance, conversion, and search retain their explicit `IsTicketIssuable` checks. Reservation creation also checks that flag under KSK-11.

**Why it fixes the bug:** Direct runtime operations enforce the same hierarchy, scope, and assignment rules as discovery. Deleted children still make a node non-leaf, matching discovery's existing semantics. Existing filters on branch and assignment queries remain in place. Errors still use the operation's existing `ServiceUnavailable` domain result.

**Status:** Complete.

### KSK-02 — AllowedTime at arrival

**Root cause:** Discovery considered upcoming schedule starts within `AllowedTime`; runtime checks required the actual opening time. The same checker also validates future appointments, which must retain strict schedule validation.

**Files:**

- `QControl.Application/Abstraction/Services/ITicketRuntimeRepository.cs`
- `QControl.Application/Features/Services/Shared/ServiceScheduleAvailability.cs` (new)
- `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/GetBranchTicketIssuableServicesQueryHandler.cs`
- `QControl.infrastructure/Services/BranchServiceAvailabilityChecker.cs`
- `QControl.infrastructure/Repositories/TicketRuntimeRepository.cs`
- Relevant regression files in section D.

Discovery and runtime share the existing same-day, midnight-clamped allowed-until calculation. Direct kiosk issuance, kiosk reservation search, and reservation conversion opt into `applyAllowedTime`. The checker reads the existing branch configuration and uses zero when the configuration is absent. It uses the current UTC weekday and the exclusive `time < EndTime` condition.

Kiosk arrival operations open at the schedule start minus the configured allowance, including the exact boundary. Future appointments, manual transfer, and workflow availability use the ordinary schedule window.

**Status:** Complete.

### KSK-11 — Reservation creation eligibility

**Root cause:** Creation required availability and `HasReservation`, but omitted `IsTicketIssuable`.

**Files:**

- `QControl.infrastructure/Repositories/TicketRuntimeRepository.cs`
- KSK-08 checker changes above.
- Relevant regression files in section D.

**Change:** Require `IsTicketIssuable` alongside `HasReservation` and the strengthened availability result before input validation or persistence.

**Why it fixes the bug:** Non-issuable services cannot accept reservations that cannot produce tickets. Appointment scheduling, submitted SegmentId, snapshot creation, quota consumption, and Serializable transaction boundaries retain their existing implementations.

**Status:** Complete.

### KSK-09 — Conversion business date

**Root cause:** The different-day guard applied only to NoShow reservations, allowing Active reservations from other dates to convert.

**Files:**

- `QControl.infrastructure/Repositories/TicketRuntimeRepository.cs`
- Relevant regression files in section D.

**Change:** Apply `reservation.BusinessDate != DateOnly.FromDateTime(now)` to every otherwise-convertible reservation, after the existing duplicate and terminal-state guards. Retain the existing `Reservations.Convert.DifferentDay` error.

**Why it fixes the bug:** Active and NoShow reservations must belong to today's UTC business date. Their appointment time does not become an arrival opening boundary. All existing snapshot/Field copying, SegmentId preservation, numbering, transaction, row-version, and unique-reservation safeguards remain in place. Conversion still never calls quota consumption.

**Status:** Complete.

### KSK-12 — Historical reservation lookup

**Root cause:** Search chose its input mode and validated values using current active definitions. Deactivation, replacement, or changed value constraints could make saved lookup data unusable. Generic Field reservations could also stop being searchable after custom inputs were added.

**Files:**

- `QControl.infrastructure/Repositories/TicketRuntimeRepository.cs`
- `QControl.Application.Tests/Tickets/KioskHistoricalReservationSearchTests.cs` (new)
- `QControl.Application.Tests/Tickets/KioskBackendSqlServerTests.cs` (new)

**Change:** Search the stored Field when supplied without custom inputs. For custom input lookup, accept IDs belonging to current active definitions or retained snapshots in reservations scoped to the requested BranchId and ServiceId. Match every submitted ID/value pair using the existing exact equality and AND semantics. Reject empty, duplicate, blank, and inapplicable inputs through existing errors. Value constraints from today's definitions are no longer applied to historical lookup. Creation continues to use the unchanged full validator.

**Why it fixes the bug:** Historical identity and values remain usable after supported configuration edits without publishing historical customer values or guessing equivalent fields. Results remain scoped, ordered, and shaped as before; no status filter was changed.

**Partially complete — remaining contract dependency.** Rename, deactivation, replacement, and validation changes are supported when the original input ID is available. Generic Field records remain searchable after new Custom Inputs are added. Section H records the review cases and their results.

### KSK-01 — HasReservation in discovery

**Root cause:** Hierarchy items already carried `HasReservation`, but the kiosk tree response and builder omitted it.

**Files:**

- `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/GetBranchTicketIssuableServicesResponse.cs`
- `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/TicketIssuableServiceTreeBuilder.cs`
- `QControl.Application.Tests/BranchServices/GetBranchTicketIssuableServices/KioskServiceTreeMetadataTests.cs` (new)

**Change:** Add `HasReservation`; populate it as `isEligibleLeaf && item.HasReservation`.

**Why it fixes the bug:** Eligible leaves expose their actual configuration; structural parents remain non-bookable even when their stored property is true. Existing ordering, hierarchy, and custom input mapping are unchanged.

**Status:** Complete.

### KSK-04 — Discovery service images

**Root cause:** Kiosk tree nodes omitted existing service logo and icon URLs.

**Files:**

- `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/GetBranchTicketIssuableServicesResponse.cs`
- `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/GetBranchTicketIssuableServicesQueryHandler.cs`
- `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/TicketIssuableServiceTreeBuilder.cs`
- `QControl.Application.Tests/BranchServices/GetBranchTicketIssuableServices/KioskServiceTreeMetadataTests.cs` (new)
- `QControl.Application.Tests/BranchServices/GetBranchTicketIssuableServices/TicketIssuableServiceCustomInputTests.cs`
- `QControl.Application.Tests/Tickets/KioskBackendSqlServerTests.cs` (new)

**Change:** Add nullable `LogoUrl` and `IconUrl`. Collect included leaves and ancestors, project their selected active logo/icon paths in one batched query, and pass mapped URLs to the builder. Selection uses the same DisplayOrder/Id ordering as Service Details; mapping uses the existing `ServiceMediaUrlMapper`.

**Why it fixes the bug:** Every included node receives its own configured images without advertisement images, entity Includes, placeholders, or per-service database round trips. The custom input batching assertion now accounts for the additional single image batch.

**Status:** Complete.

## B. Changed files

Paths below are relative to `E:\QControl`.

| File | Reason |
| --- | --- |
| `QControl.Application/Abstraction/Services/ITicketRuntimeRepository.cs` | Optional availability timing flag; no changes to ticket repository operation signatures. |
| `QControl.Application/Features/Services/Shared/ServiceScheduleAvailability.cs` | Shared existing UTC allowed-until calculation. |
| `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/GetBranchTicketIssuableServicesQueryHandler.cs` | Shared timing calculation and batched projected image read. |
| `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/GetBranchTicketIssuableServicesResponse.cs` | Three additive tree-node properties. |
| `QControl.Application/Features/BranchServices/Query/GetBranchTicketIssuableServices/TicketIssuableServiceTreeBuilder.cs` | Map reservation support and images; reuse included-node collection. |
| `QControl.Application/Properties/AssemblyInfo.cs` | Allow infrastructure to reuse the internal schedule timing helper. |
| `QControl.infrastructure/Services/BranchServiceAvailabilityChecker.cs` | Branch, hierarchy, scope, leaf eligibility and operation-specific schedule allowance. |
| `QControl.infrastructure/Repositories/TicketRuntimeRepository.cs` | Arrival timing opt-in, reservation issuance flag, same-day conversion, historical lookup. |
| `QControl.Application.Tests/Tickets/KioskTicketRuntimeRepositoryTests.cs` | Share existing fixture through partial test class and adapt checker stub signature. Existing assertions remain unchanged. |
| `QControl.Application.Tests/Tickets/KioskBackendStabilizationTests.cs` | Runtime eligibility, timing, booking, and conversion regression matrix. |
| `QControl.Application.Tests/Tickets/KioskHistoricalReservationSearchTests.cs` | Historical identity/value lookup, creation validation, scope, and snapshot regressions. |
| `QControl.Application.Tests/Tickets/KioskBackendSqlServerTests.cs` | Opt-in SQL Server integration using a unique disposable database. |
| `QControl.Application.Tests/BranchServices/GetBranchTicketIssuableServices/KioskServiceTreeMetadataTests.cs` | Discovery mapping, image association, URL parity, and serialization contract tests. |
| `QControl.Application.Tests/BranchServices/GetBranchTicketIssuableServices/TicketIssuableServiceCustomInputTests.cs` | Account for one custom input batch plus one image batch. |
| `KIOSK_BACKEND_CHANGE_REPORT.md` | Requested implementation, verification, and scope report. |

## C. API contract changes

Only these properties were added to `TicketIssuableServiceTreeNodeResponse`:

```csharp
public bool HasReservation { get; init; }
public string? LogoUrl { get; init; }
public string? IconUrl { get; init; }
```

With the existing web JSON naming convention, these are `hasReservation`, `logoUrl`, and `iconUrl`. No existing response property was removed or renamed. No route, HTTP method, authorization policy, permission, or controller changed. The existing anonymous service-discovery behavior is preserved. Reservation search's request and response contracts are unchanged.

## D. Initial tests and build

Added **48 xUnit test cases**. Eligibility and timing tests execute the real availability checker and repository using the existing EF InMemory test infrastructure; SQL integration additionally executes production SQL queries and persistence constraints.

| Bug | New coverage |
| --- | --- |
| KSK-08 | `KioskBackendStabilizationTests.cs`: 15 eligibility scenarios across direct kiosk issuance, booking, conversion, and search. Covers valid Global/BranchScoped, inactive/deleted service and ancestors, grandparent activity, non-leaf including deleted child, wrong scope, missing assignment, missing/inactive branch, missing service, and non-issuable service. |
| KSK-02 | Same file: 10 timing cases compare discovery, walk-in, and conversion; 30/20/zero allowance, exact opening, one minute before, normal opening, exclusive end, and after end. Every case also checks strict appointment validation. A separate future-day test verifies the appointment weekday, business date, and quota date. |
| KSK-11 | Same file: HasReservation true/false, plus all eligibility cases and ordinary appointment restrictions. Historical and SQL tests verify submitted segment and snapshots on valid bookings. |
| KSK-09 | Same file: nine status/date cases, duplicate rejection, original Field/segment, unchanged number, no conversion quota consumption, and unchanged state on rejection. |
| KSK-12 | `KioskHistoricalReservationSearchTests.cs`: rename, label edit, type/constraint change, deactivation, replacement, new-definition creation/search, historical Field, exact AND matching, scope, invalid inputs, conversion snapshots, and null-ID preservation without guessed matching. |
| KSK-01/KSK-04 | `KioskServiceTreeMetadataTests.cs`: real handler mapping, parent reservation gating, true/false leaves, both/logo-only/icon-only/no images, multiple services, inactive images and Ads exclusion, deterministic selection, Service Details URL parity, batching, and exact additive JSON properties. |
| SQL integration | `KioskBackendSqlServerTests.cs`: real SQL availability, strict booking versus early arrival, discovery image projection, deactivated-input lookup, snapshot/segment/quota preservation, second-context duplicate conversion rejection, unique-reservation index enforcement, and actual FK SET NULL behavior after input deletion. |

Executed commands:

```powershell
dotnet build Qcontrol.slnx --no-restore -v minimal

$env:QCONTROL_KIOSK_TEST_SQLSERVER = 'Server=.\SQLEXPRESS;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5'
dotnet test QControl.Application.Tests/QControl.Application.Tests.csproj --no-restore --filter 'FullyQualifiedName~KioskTicketRuntimeRepositoryTests|FullyQualifiedName~GetBranchTicketIssuableServices|FullyQualifiedName~KioskBackendSqlServerTests' --logger 'trx;LogFileName=kiosk-focused.trx' --results-directory .test-results -v minimal
dotnet test Qcontrol.slnx --no-restore --logger 'trx;LogFileName=kiosk-full-suite.trx' --results-directory .test-results -v minimal
git diff --check
```

- **Final build:** succeeded; 0 errors, 5 warnings. Warnings are from unchanged MailKit/MimeKit package advisories and the existing Serilog.Expressions version resolution. No dependency changes were made.
- **Focused run:** 135 passed, 0 failed, 0 skipped. The additional future-day case was added afterward and passed in the full suite.
- **Final full suite:** **630 passed, 0 failed, 0 skipped**; 630 executed.
- **Whitespace/diff check:** passed.
- **Unexecuted tests:** none in the full suite. The SQL integration test ran with the opt-in connection set. Without that environment variable, only that integration test is explicitly skipped.
- The SQL test replaced the supplied connection's database name with a unique `QControl_KioskRegression_<guid>` database, used the existing EF model, and removed the database in `finally`. Verified that no such database remained. No existing database was modified and no migration was created.

Validation logs and TRX files were moved out of the repository to:

`C:\Users\p\.codex\visualizations\2026\11\03\01a1a24a-5712-79d1-ae44-3870b56832e7\qcontrol-validation`

## E. Business regression verification

Each preserved behavior from section 4 of the brief was reviewed:

| # | Behavior | Verification |
| --- | --- | --- |
| 1 | Walk-in directly creates a Ticket | Unchanged; existing repository/controller tests pass. No Reservation is created. |
| 2 | Walk-in uses default Service/Branch Segment | Unchanged; default resolution code untouched. Existing client-SegmentId override, missing/ambiguous default, and quota tests pass. |
| 3 | Conversion preserves SegmentId | Unchanged; existing, new status/date, and SQL tests pass. |
| 4 | Conversion preserves historical snapshots | Unchanged; edited/deactivated/replaced/null-ID snapshot tests and SQL integration pass. |
| 5 | Generic Field mandatory with no configured inputs | Unchanged for creation; existing creation tests pass. Empty historical search also remains rejected. |
| 6 | Existing custom input validation | Creation validator unchanged; existing tests and new stale-definition/type-constraint creation tests pass. Historical lookup uses saved values as approved. |
| 7 | Same-day NoShow conversion | Preserved; new same-day NoShow regression passes. |
| 8 | Ticket quota behavior | Unchanged; quota implementation untouched; existing default-assignment quota tests pass. |
| 9 | Reservation quota behavior | Unchanged; creation still consumes once on the appointment business date; existing/new/SQL tests pass. |
| 10 | Conversion consumes no further quota | Unchanged; existing tests, historical tests, and SQL integration pass. |
| 11 | Ticket numbering and prefix/range rules | Implementation untouched; existing tests and conversion number assertion pass. |
| 12 | Ticket/Reservation state transitions | Domain state machines unchanged. Only approved conversion eligibility guards changed. Terminal rejection and Active/NoShow conversion tests pass. |
| 13 | Archive/retention | Code untouched; full existing suite, including archive Field-copy assertions, passes. |
| 14 | JWT, permissions, policies, handlers, branch access | Code untouched; existing kiosk protection and request guards preserved. Existing contract tests pass. No deployed JWT end-to-end session was exercised. |
| 15 | Routes and Result/ProblemDetails mapping | Controllers, routes, mapping and error resources untouched; existing controller contract tests pass. |
| 16 | Public discovery authorization | Unchanged; existing anonymous-access tests and new anonymous discovery tests pass. |
| 17 | Tree ordering and unlimited hierarchy depth | Existing tree traversal/ordering and hierarchy calculator unchanged. Existing hierarchy/depth tests and new multi-service metadata tests pass. |
| 18 | Unrelated modules | No Terminal, Display, Window, Operator, CallAlgorithm, Identity, archiving, or other unrelated production module changed. Full suite passes. |

## F. Risks, dependencies, and verification limits

1. **Historical search identifier dependency:** Renamed/deactivated/replaced definitions remain searchable when callers retain their original `ServiceCustomInputId`. Discovery continues to expose only current input configuration; it does not publish historical customer values or historical lookup metadata. A UI that discards the old identifier cannot construct that historical lookup request. No frontend changes were made.
2. **Permanent deletion is a contract blocker:** The existing FK uses `DeleteBehavior.SetNull`. SQL integration verified that hard deletion preserves names/labels/types/values but nulls `ServiceCustomInputId`. The current request contains only an input ID and value, so it cannot safely identify such snapshots. No name/value guessing, generic Field fallback for custom snapshots, new endpoint, or schema change was introduced. Supporting this case requires separately approved identity/contract work.
3. Kiosk issuance, search, and conversion use early-arrival timing and strict eligibility. Reservation creation uses strict eligibility at the requested appointment time. Normal issuance, transfer, and workflow continuation use the original availability rules and ordinary schedule window.
4. SQL Server availability reads the requested service and uses one recursive command for its ancestor chain when needed. The chain query returns a single eligibility flag. Discovery reads one projected image batch for included nodes. Historical input IDs outside the current configuration require one additional scoped identity projection.
5. The HTTP changes are additive. The .NET availability interface uses optional timing and kiosk eligibility flags. All in-repository implementations use the updated signature. External implementations would need to adopt that signature.
6. **Concurrency:** Serializable transactions, row versions, duplicate checks, and the unique Ticket.ReservationId index are unchanged. Real SQL tests verify duplicate checks across contexts and direct unique-index enforcement. A simultaneous-conversion stress/deadlock test was not performed.
7. **Other unverified environments:** No browser/frontend exercise, deployment, production database execution, or HTTP JWT end-to-end test was performed. SQL verification used local SQL Server Express in a disposable database. Existing build dependency warnings remain outside this task's scope.

## G. Scope confirmation

| Question | Answer |
| --- | --- |
| Changed frontend files? | **NO** |
| Created a database migration? | **NO** |
| Added an API endpoint? | **NO** |
| Changed any Permission/Role/Authorization policy? | **NO** |
| Changed Ticket numbering? | **NO** |
| Changed quota logic? | **NO** |
| Changed Segment selection? | **NO** |
| Changed Ticket/Reservation transition definitions? | **NO** — only approved eligibility/date guards were added. |
| Modified any unrelated module? | **NO** |

No entity, EF model, database schema definition, reservation lifecycle, calling algorithm, retention, or archiving implementation changed. No deployment was performed.

## H. PR #35 review follow-up

| Finding | Result | Verification |
| --- | --- | --- |
| KSK-02 | Complete. Kiosk reservation search uses the same early-arrival mode as discovery, issuance, and conversion. | The timing matrix checks all four paths at the allowance boundary, before it, at normal opening, and at or after closing. Strict appointment and conversion date/status regressions passed. SQL Server also found a saved reservation at 15:00 for a 15:30 service opening with a 30-minute allowance. |
| KSK-08 | Complete. One parameterized lookup projects the target and its child-existence flag. One recursive command checks ancestors for an eligible leaf with a parent. | SQL Server command instrumentation observed exactly two Service reads for a leaf with 105 ancestors and 500 unrelated services. The target and ancestor anchor used ID parameters. A cycle and inactive/deleted ancestors were rejected, reactivation was observed on the next check, and a soft-deleted child still made the target a non-leaf. Repository tests covered a shallow chain, a missing parent, branch state, scope, and assignment. |
| KSK-12 | **Partially complete — remaining contract dependency.** The existing search implementation covers cases A–E. | Historical search, current-definition booking, exact equality and AND matching, branch/service isolation, invalid input, and conversion snapshot regressions passed. SQL Server confirmed the FK clears the input ID and preserves the snapshot on hard deletion. |

The infrastructure friend assembly access remains necessary for the internal `ServiceScheduleAvailability` helper shared by discovery and runtime. Runtime eligibility no longer calls the discovery hierarchy calculator. The SQL Server query follows parent IDs through the existing primary key, and the leaf check uses the existing parent index. The nonrelational test provider checks ancestors by ID without database round trips. A missing parent ends the walk, and a cycle invalidates the chain.

| KSK-12 case | Result with the existing contract |
| --- | --- |
| A. Rename | The original input ID and saved value find the reservation. The snapshot keeps the original names and labels. |
| B. Deactivation | The caller can use the retained original ID and saved value. |
| C. Replacement | The retained original ID still works. A replacement ID identifies its own definition. |
| D. Changed validation restrictions | Search compares the saved value. New reservations use the current validation rules. |
| E. Custom Inputs added after a generic Field booking | The saved Field remains searchable in the same branch and service. |
| F. Permanent deletion with a null input ID | The snapshot survives, but the request cannot identify the field. This requires a separately approved contract solution. |

Discovery exposes current definitions, so callers need another existing source for an original ID that discovery no longer returns. Cases B and C work when callers already have that ID.

The follow-up passed `dotnet restore Qcontrol.slnx` and `dotnet build Qcontrol.slnx --no-restore`. The focused kiosk run passed **147 tests**, and `dotnet test Qcontrol.slnx --no-restore` passed **634 tests** with zero failures or skips. Both runs included the two opt-in SQL Server tests using unique disposable databases.

## I. Final regression pass

The final pass compared every availability call site with `master`. The shared creation method applied `AllowedTime` to normal issuance, and the shared checker imposed kiosk leaf, ancestor, and scope restrictions on manual transfer and workflow continuation. Completion without a next workflow step had no availability check.

| Checker consumer | Schedule | Eligibility |
| --- | --- | --- |
| Normal direct issuance | Ordinary schedule at current UTC time | Original availability plus the existing ticket-issuable requirement |
| Kiosk direct issuance | Early-arrival window at current UTC time | Strict kiosk eligibility |
| Reservation creation | Ordinary schedule at requested appointment time | Strict kiosk eligibility plus ticket and reservation flags |
| Reservation conversion via either handler | Early-arrival window at current UTC time | Strict kiosk eligibility and existing conversion guards |
| Kiosk reservation search | Early-arrival window at current UTC time | Strict kiosk eligibility and existing search filters |
| Manual transfer | Ordinary schedule at current UTC time | Original destination checks |
| Complete service with a next workflow step | Ordinary schedule at current UTC time | Original destination checks for the next step |
| Complete service without a next workflow step | No availability check | Existing ticket completion guards |

Normal issuance, transfer, and workflow continuation use the original active-service, assignment, and schedule checks, including the existing EF filters for deletion and branch access. The request handlers continue to enforce their existing branch and permission rules. Strict callers opt into `requireKioskEligibility` and still use the scoped SQL Server ancestor query.

The expanded timing matrix covers kiosk and normal issuance independently, including arrival at 15:00 with a 15:30 opening and a 30-minute allowance. New transfer and workflow cases accept scheduled non-leaf destinations and destinations with inactive ancestors, and reject inactive, deleted, unassigned, and out-of-schedule destinations. Completion and transfer tests exercise the branch guards through the real repository and request handlers. SQL Server verification checks that standard availability uses a single Service read even when kiosk eligibility requires an ancestor check.

Restore and build passed. The focused kiosk suite passed **160 tests**. The full solution passed **647 tests** with **0 failures and 0 skips**, including both SQL Server integration tests against disposable databases.

KSK-12 retains the approved search behavior. Permanent deletion still clears the historical input ID, so lookup of that snapshot requires a separately approved contract solution.
