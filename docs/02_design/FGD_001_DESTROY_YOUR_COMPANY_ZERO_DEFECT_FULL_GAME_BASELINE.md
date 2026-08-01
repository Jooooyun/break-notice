# FGD-001 — DESTROY YOUR COMPANY: ZERO DEFECT
## Full-Game Direction, Story, Campaign Map, Level Design, and G4-A Baseline

> **HUMAN OWNER-APPROVED CHANGE-CONTROL INPUT**
>
> Repository codename: `BREAK NOTICE`
>
> This document defines the intended full game. It does not authorize Unity,
> implementation, Graybox execution, G4 passage, production assets, or Gate opening.
> Exact runtime values, geometry, tuning, asset production, and implementation details
> remain owned by their later Gates.

---

## Authority Diff — Previous Limited Registration → Full-Game Baseline

```diff
- Register only Product Direction and G4-A.
- Keep detailed Story, Campaign, Map, and Level design frozen as post-G4 targets.
+ Register the complete Story Worldline, Campaign structure, Macro Map, Levels 0–6,
+ core narrative beats, rescue structure, Finale, and Ending as the Owner-approved
+ full-game design target.
+ Keep only exact implementation values, geometry, enemy counts, timings, camera
+ tuning, asset production, and technical realization deferred to G4/G5/G6.
+ G4-A remains the first production evidence gate and may block implementation
+ expansion, but does not erase or silently replace the approved full-game target.
```

---

# 1. Decision Summary

| Area | Owner-approved decision |
|---|---|
| Working title | `DESTROY YOUR COMPANY: ZERO DEFECT` |
| Repository codename | `BREAK NOTICE` |
| Public marketing title | Working title approved; final public lock remains open because of destruction-sandbox positioning risk |
| Primary genre | Single-campaign third-person corporate physics action adventure |
| Full-product target | 3.5–5 hour first-play campaign |
| Structure | One authored office building, four floor bands, central atrium, seven main levels |
| Core combat | Physical office-object combat, one active ally, Cargo Strap redirection, causal room-state change |
| Core narrative pillar | `OFFICIAL HANDOVER PLAN` versus `CHR_00 AS-BUILT NOTES` |
| Worldline | `ZERO DEFECT / HANDOVER` |
| Rescue population | Approximately 15 unaccounted people, separate from 12 authorized handover personnel |
| Active ally | CHR_01 only |
| Boss | No traditional HP boss |
| Finale | Central Atrium spatial objective and removal of a non-load-bearing corporate promotional wall |
| Ending | People survive; handover fails; company becomes insolvent; As-Built Map remains as evidence |
| Roguelite disposition | Not the primary format; retained only as a deferred replay layer candidate |
| G4-A | 5–10 minute single Core Room validation using the locked Prototype content |
| Detailed implementation | Not authorized by this document |

---

# 2. Product Identity

## 2.1 One-line concept

On the final night before a headquarters sale, approximately fifteen workers omitted
from the official occupancy record remain trapped inside a building reported as empty.
A facilities and asset-management employee and one outsourced relocation foreman fight
through the handover response operation using physical office objects, industrial cargo
straps, and the building’s own automated equipment, while recording the real building
that the company erased from its official plans.

## 2.2 Core promise

The player does not shoot, summon, or telekinetically control objects.

The player:

- directly acquires real office objects;
- receives real Light and Medium objects from CHR_01 through physical paths;
- catches, throws, redirects, and transfers them;
- attaches the approved Cargo Strap only to eligible physical anchors;
- changes enemy position, furniture placement, visibility, routes, and room state;
- creates a safe window to document the actual building;
- rescues people by opening routes rather than escorting them.

Core statement:

> Combat is not followed by a spatial puzzle. Combat is how the spatial problem is solved.

## 2.3 Tone

`Grounded Corporate Action Realism`

- Characters treat layoffs, unsafe occupancy, debt, and legal exposure seriously.
- Comedy comes from physical consequences, not quips or caricature.
- The building-management system is not sentient or malicious.
- The human organization created false inputs and evasive procedures.
- The story is tense, grounded, non-supernatural, and non-military.

## 2.4 Time target

- First Normal blind clear: `3.5–5 hours`
- Skilled replay target: `2–3 hours`
- Optional exploration may extend the first playthrough.
- Story time: approximately `20:00–00:00`
- No real-time countdown Game Over.

---

# 3. Preserved Existing Canon

The following remain unchanged.

- Unity 6, 3D third-person presentation, solo-development constraint.
- CHR_00 is an ordinary facilities and asset-management worker.
- No telekinesis, magic, firearm-centered combat, or superhero movement.
- The dark-red necktie is clothing only.
- Object control uses the separate approved black-yellow industrial Cargo Strap.
- Objects follow real trajectories and are affected by obstacles.
- General combat has exactly one active ally: CHR_01.
- CHR_01 supports but does not replace player intent.
- Prototype destruction remains zero.
- No runtime fracture or arbitrary wall destruction.
- Existing G2/G3 contracts remain authoritative unless explicitly changed by this
  Change Control.
- Approved Visuals preserve identity, silhouette, equipment relationship, camera intent,
  and prohibited drift; they do not impose AAA render fidelity.

---

# 4. Company and ZERO DEFECT

## 4.1 Company condition

The company accumulated severe debt through expansion, failed acquisitions, duplicated
organizations, excessive headquarters overhead, short-term borrowing, and repeated
reduction of facility and safety costs.

The board approves:

- sale of headquarters;
- reduction of roughly one third of staff;
- outsourcing of facilities, logistics, and support functions;
- relocation of remaining departments;
- bulk disposal of assets.

The sale proceeds are the company’s only plausible source for:

- final payroll;
- severance;
- short-term debt;
- contractor payments.

## 4.2 Handover contract

By midnight the building must be in handover condition.

The buyer may stop the transfer if it discovers:

- remaining occupants;
- unremoved company assets;
- active operating systems;
- undisclosed occupied spaces;
- unresolved facility or safety exceptions.

## 4.3 ZERO DEFECT system

`ZERO DEFECT` is the facility and asset-management system.

`HANDOVER MODE` is the automated property-transfer procedure.

Target state:

```text
OCCUPANCY: 0
ASSET VARIANCE: 0
OPEN SAFETY EXCEPTION: 0
```

The company did not resolve reality to reach zero. It excluded reality from the record.

- outsourced workers were excluded from occupancy;
- informal workspaces were omitted from drawings;
- personal property was treated as unclassified assets;
- safety issues were closed as local field actions;
- illegal modifications were classified as temporary facilities;
- contractors were registered as visitors rather than regular occupants.

## 4.4 What HANDOVER MODE does

Believing the building is empty, the system:

- reduces lighting and HVAC by zone;
- closes ordinary service access not included in the handover route;
- moves mobile partitions to presentation positions;
- dispatches automated carts to collect remaining assets;
- assigns freight elevators to removal operations;
- closes non-handover work routes;
- stages the central atrium for final transfer;
- progressively shuts down operating systems.

The system is functioning as specified.
The official model is false.

---

# 5. People and Spaces Missing from the Record

## 5.1 Authorized handover personnel

Approximately `12` authorized handover personnel remain in the official system.

CHR_00 and CHR_01 are part of this authorized operational population or are explicitly
tracked alongside it.

## 5.2 Unaccounted rescue population

Approximately `15` additional people remain unaccounted for.

Candidates include:

- night cleaning workers;
- outsourced logistics workers;
- contract technical-support staff;
- event installation and removal workers;
- partner-company staff working late;
- non-regular customer-support staff.

Production representation:

- approximately 9 encountered or directly acknowledged individuals;
- approximately 6 people revealed as a group in the Finale;
- only a small subset receives unique voice and facial treatment.

## 5.3 Unofficial spaces

The real building contains:

- an outsourced office built inside former storage;
- a support room hidden behind partitions;
- temporary seats installed in circulation space;
- unofficial storage in a closed floor zone;
- an electrical-support area outside the formal detector loop;
- a service secondary stair hidden by executive expansion;
- maintenance access behind the central promotional wall.

These are realistic illegal or undisclosed modifications, not supernatural secret floors.

---

# 6. Main Characters

## 6.1 CHR_00

Role:
Facilities and asset-management employee.

Knowledge:

- facility systems;
- maintenance routes;
- freight circulation;
- manual releases;
- original drawings;
- cart service lanes;
- differences between load-bearing structure and removable installation;
- which objects and fixtures can be safely moved or dismantled.

Starting state:

- It is his final workday.
- He wants to complete the handover inspection and leave.
- He initially believes the first trapped person is an administrative omission.
- He sees completing procedure as part of his identity.

### Past involvement

Months earlier, CHR_00 supervised installation of the executive expansion and central
promotional wall.

He noticed the access risk but processed it formally rather than stopping the work.

```text
TEMPORARY ACCESS VARIANCE
ALTERNATE EGRESS AVAILABLE PER APPROVED PLAN
RESTORE BEFORE PROPERTY HANDOVER
PRIORITY: NON-CRITICAL
```

He believed another approved route existed, accepted an assurance that restoration would
occur before handover, and chose not to delay the project.

The note does not absolve him.
He recorded a risk without verifying the physical reality.

### Arc

```text
employee completing procedure
→ employee correcting an omission
→ employee discovering that records conceal reality
→ employee refusing delegated indifference
→ worker physically undoing the structure he once approved
```

## 6.2 CHR_01

Role:
Outsourced relocation and asset-removal foreman.

Knowledge:

- which furniture can be disassembled;
- which objects can carry load;
- where industrial straps can be attached;
- automated-cart routes;
- unofficial storage and contractor workspaces;
- which outsourced workers remain inside.

Gameplay boundary:

- one active ally only;
- physical Light supply;
- floor-supported eligible Medium supply;
- no second combat ally;
- does not choose final direction or timing for the player;
- rescued people do not become additional relay agents.

## 6.3 CEO

The CEO is not initially a murderer or cartoon villain.

His logic:

- failed handover may make the company immediately insolvent;
- final salary and severance for hundreds may be endangered;
- the remaining people are reported as a small field problem;
- facilities personnel are still operating locally;
- a full stop is equivalent to destroying the last financial option.

Initial direction:

```text
Maintain handover.
Locate and remove remaining personnel in the field.
```

His wrongdoing is deliberate ignorance and delegated responsibility.

He receives only questions such as:

```text
VERIFIED REGISTERED OCCUPANT?
VERIFIED LIFE-SAFETY EXCEPTION?
LOCAL EXTRACTION CAPABLE?
HANDOVER IMPACT?
```

Unregistered people and unofficial rooms fall outside the questions.
He can inspect raw data, but formal inspection would record the discrepancy in the
buyer-facing system and halt the deal.

He does not merely lack knowledge.
He avoids the procedure that would make the knowledge official.

Final line:

> If this deal collapses, the people you think you saved may not get paid either.

## 6.4 Handover Response Team

They are contracted facility-control personnel, not soldiers.

Mission:

- protect handover routes;
- maintain asset-removal flow;
- prevent damage;
- protect buyer representatives;
- nonlethally restrain interference;
- direct personnel through official routes.

No firearms, ballistic armor, riot shields, or military silhouette.

Most believe CHR_00 is sabotaging a legitimate property transfer.
Some stop fighting in the Finale once they see the hidden people.

---

# 7. Time and Act Structure

## 7.1 Time progression

```text
20:00 HANDOVER MODE begins; first trapped worker found
20:30 Floor Seal; Operations closes
21:00 Asset Consolidation begins
21:30 Service Reduction exposes hidden contractor spaces
21:50 Electrical overheating and smoke
21:55 Emergency call
22:05 Fire and safety response arrives using official plans
22:15 CEO refuses full stop and records a local incident
22:20 Lobby commitment beat
22:45 Executive Seal
23:30 Buyer Arrival Window and Atrium Finale
00:00 Scheduled handover fails
```

Time advances through story beats, clocks, announcements, lighting, and system states.
There is no real-time failure countdown.

## 7.2 Act I — EMPTY ON PAPER

Target:
Approximately 70–90 minutes in the full campaign target.

Main progression:

1. HANDOVER MODE begins.
2. A fire door repeatedly attempts to open in a zero-occupancy zone.
3. CHR_00 finds a night cleaner.
4. Control rejects the report because the person is not a registered occupant.
5. CHR_00 joins CHR_01.
6. Open Operations contains more unnotified outsourced staff.
7. Automated asset removal begins.
8. Personal medicine and work property are classified as assets.
9. CHR_00 realizes the system can create immediate physical harm.

CHR_00’s belief:

> This is an administrative mistake. I can correct it.

## 7.3 Act II — NOT ON THE PLAN

Target:
Approximately 90–120 minutes.

Main progression:

1. Hidden Contractor Annex is discovered.
2. Official and real geometry diverge.
3. Actual occupancy and construction records are found.
4. Temporary electrical equipment overheats.
5. Smoke and partial power loss sever a route.
6. CHR_00 calls emergency services.
7. Fire and safety teams arrive and search the official building.
8. CHR_00 requests a full HANDOVER stop.
9. The CEO refuses and records the event as a local incident.
10. CHR_00 leaves the command structure.
11. The original secondary stair and promotional-wall construction record are found.

CHR_00’s belief:

> People were not missed. The company removed them from the record.

## 7.4 Act III — HANDOVER FAILED

Target:
Approximately 65–90 minutes.

Main progression:

1. CHR_00 and CHR_01 enter the executive floor.
2. CHR_00 finds his own past approval record.
3. A short in-engine flashback shows the installation decision.
4. The buyer and legal team arrive.
5. The CEO reports the building empty and ready.
6. CHR_00 and CHR_01 enter the Atrium.
7. Automated carts and guidance walls are redirected.
8. Maintenance-rail release is reached.
9. The corporate promotional wall is dismantled.
10. The secondary stair, hidden connection, and remaining people are revealed.
11. The buyer stops handover.
12. External responders enter using the As-Built information.
13. The company becomes insolvent the next day.
14. CHR_00 leaves his badge behind.

CHR_00’s belief:

> I cannot save the company. I will not leave people inside it.

---

# 8. Core Gameplay Structure

## 8.1 Main loop

```text
DETECT
→ CONTEST
→ FIGHT
→ RECONFIGURE
→ RECORD
→ RELEASE
```

- `DETECT`: discover a person, hidden space, or record discrepancy.
- `CONTEST`: Response Team or automated equipment blocks the route.
- `FIGHT`: use physical objects, CHR_01 relay, Catch, Throw, Strap, and causal results.
- `RECONFIGURE`: change furniture, carts, partitions, shutters, or access.
- `RECORD`: mark the verified actual state on the As-Built layer.
- `RELEASE`: rescued people leave through the opened route without escort gameplay.

## 8.2 Combat share target

Long-term campaign target:

- physical combat and combat objectives: 55–60%;
- navigation, mapping, and route construction: 25–30%;
- dialogue, transitions, flashback, and story staging: 10–15%.

This is a target, not an approved runtime metric.

## 8.3 Combat objective families

- `DISLODGE`
- `INTERRUPT`
- `REDIRECT`
- `TRANSFER`
- `HOLD`
- `REVEAL`
- `SECURE`

Enemy elimination is not the universal win condition.

## 8.4 Encounter completion

An encounter may complete when:

- a route is opened;
- a rescue lane becomes safe;
- a required device becomes reachable;
- an enemy is moved outside the objective lane;
- a recording window is created;
- automated-equipment routing is changed.

---

# 9. Official Plan and As-Built Notes

## 9.1 Map duality

```text
OFFICIAL HANDOVER PLAN
↕
CHR_00 AS-BUILT NOTES
```

The Official Plan is the building submitted to the buyer and emergency responders.

The As-Built Notes record:

- actual doors;
- hidden rooms;
- blocked stairs;
- temporary electrical feeds;
- partition changes;
- verified occupancy points;
- manual releases;
- cart service lanes;
- safe exit routes.

## 9.2 Player action

Working context name:

`MARK AS-BUILT`

- no new logical action is required;
- intended as an `InteractPrimary` context;
- final arbitration is not approved in this document;
- no production Map UI is approved before G4 evidence.

## 9.3 Production boundary

Full-campaign target:

- approximately 6–8 authored survey points;
- no free photography;
- no freeform annotation;
- short contextual interaction;
- visible connection between each record and a route, shortcut, rescue, or later reveal.

## 9.4 Ending payoff

The As-Built Map survives the company.

External responders and investigators use it to locate hidden spaces, temporary feeds,
blocked exits, and real occupancy.

---

# 10. Emergency Response Logic

## 10.1 Emergency call

The call occurs after the electrical incident.

Condensed working dialogue:

```text
Dispatcher:
The address is confirmed.
The space you described does not appear on the submitted plan.
Give me the nearest official room or stair number.

CHR_00:
...That space has no number.

Dispatcher:
We are sending crews to the official access points.
Do not force entry into the energized area.
Continue reporting any verifiable access and isolation point.
```

The dispatcher is competent.
The coordinate system is false.

## 10.2 Why responders cannot immediately reach the Annex

Three distinct conditions:

1. `Main Fire Egress`
   - remains legally open;
   - not locked by HANDOVER MODE.

2. `Ordinary Service Access`
   - was used by workers before handover;
   - HANDOVER MODE closes it because it is not an approved handover route.

3. `Service Secondary Stair`
   - was physically concealed by past illegal modification;
   - is not simply locked by the automated system;
   - its actual relationship to the Annex does not exist on the official drawing.

External responders search and isolate the official building correctly.
CHR_00 must supply the actual geometry.

---

# 11. Rescue Population and Clusters

## 11.1 Cluster 1 — They do not know they must leave

Location:
Facilities and Open Operations.

People:
Approximately 3.

Function:
First proof that exit communication excluded outsourced staff.

Combat question:
Can the player use direct acquisition and CHR_01 relay to establish space control?

## 11.2 Cluster 2 — They refuse to leave

Location:
Automated Asset Dispatch.

People:
Approximately 2.

Situation:

- They believe failed handover may destroy wages.
- Personal medicine and work property were moved into an asset cage.
- They will not leave until the medicine is recovered.

Tone:
Matter-of-fact, not melodramatic.

Story function:
Act I to Act II transition.

## 11.3 Cluster 3 — They cannot be physically reached

Location:
Hidden Contractor Annex.

People:
Approximately 3.

Situation:

- ordinary access has been closed by HANDOVER;
- the official map omits the space;
- partitions, shelving, and temporary infrastructure block alternate access.

Combat question:
Can the player create access using physical transfer and room reconfiguration?

## 11.4 Cluster 4 — Someone returned inside

Location:
Records / Electrical Support.

People:
Approximately 1 directly encountered.

Situation:
The person had exited, then returned for an omitted colleague and the original list.

Function:
Shows that people are not a single morally uniform crowd.

## 11.5 Remaining group

Approximately 6 people are verified indirectly and revealed through the secondary stair
during the Finale.

## 11.6 Rescue NPC boundary

- no escort missions;
- no rescue NPC combat AI;
- no additional persistent ally;
- no permanent NPC death system;
- limited fixed-position scripted support only.

---

# 12. Campaign Map

## 12.1 Structure

`HYBRID VERTICAL LOOP`

- one central atrium visible early;
- service core and freight lift form vertical progression;
- each floor has a short authored loop;
- one meaningful shortcut per floor band;
- limited backtracking;
- no open world;
- no procedural rooms.

## 12.2 Macro Map

```text
                         3F — EXECUTIVE
        ┌────────────────────────────────────┐
        │ Executive Reception                │
        │ Handover / Legal Suite             │
        │ Blocked Service Secondary Stair ───┼────┐
        │ Atrium Upper Ring                  │    │
        └────────┬───────────────────────────┘    │
                 │                                │
                 │                      CENTRAL ATRIUM
                 │                      [FINAL LANDMARK]
                 │                                │
        ┌────────▼───────────────────────────┐    │
        │ 2F — CONTRACTOR / SUPPORT          │    │
        │ Service Junction [SAFE 2]          │    │
        │ Hidden Contractor Annex            │    │
        │ Records / Electrical               │    │
        │ Service Stair Shortcut ────────────┼────┘
        └────────┬───────────────────────────┘
                 │
        ┌────────▼───────────────────────────┐
        │ 1F — OPERATIONS                    │
        │ Open Operations Office             │
        │ Copy / Meeting Hub                 │
        │ Atrium Perimeter Corridor          │
        │ Service Stair Shortcut             │
        └────────┬───────────────────────────┘
                 │
        ┌────────▼───────────────────────────┐
        │ B1 — FACILITIES / DISPATCH         │
        │ Facilities Control [SAFE 1]        │
        │ Automated Asset Dispatch           │
        │ Loading Dock / External Access     │
        │ Freight Lift                       │
        └────────────────────────────────────┘
```

## 12.3 Route families

Every major level uses:

- `OFFICIAL ROUTE`
- `MAINTENANCE ROUTE`
- `IMPROVISED PHYSICAL ROUTE`

## 12.4 Safe rooms

1. Facilities Control
2. Service Junction
3. Executive Fire Refuge

No shop, weapon upgrade, crafting, or grid inventory.

## 12.5 Shortcuts

1. Copy Hub ↔ Facilities Service Stair
2. Loading Dock Service Gate ↔ Facilities Control
3. Hidden Annex Service Passage ↔ Service Junction
4. Executive Upper Ring ↔ 2F Service Stair

---

# 13. Common Level Grammar

Every major level targets:

1. one official-record discrepancy;
2. one rescue cluster or rescue objective;
3. at least one full physical combat encounter;
4. one major room-state change;
5. one survey or As-Built record point;
6. one shortcut or access-state change;
7. no more than one newly taught gameplay concept;
8. one clear exit objective.

Every major combat space requires:

- readable entry zone;
- direct object pool;
- CHR_01 supply lane;
- primary causal spine;
- at least two plausible physical approaches;
- clear room-state endpoint;
- safe recording window.

Prohibited:

- single fixed Rube Goldberg answer;
- mandatory precision platforming;
- maze backtracking;
- enemy elimination as the only objective;
- arbitrary destruction of all walls or furniture.

---

# 14. Levels 0–6

## LEVEL 0 — OCCUPANCY ZERO

Location:
B1 Facilities Control / First Rescue.

Target role:
Introduction and first discrepancy.

Story:

- fire door repeatedly attempts to open in a zero-occupancy zone;
- night cleaner found inside;
- control rejects the report because the person is not registered.

Core gameplay:

- Move, Look, InteractPrimary;
- small direct object;
- manual door release;
- one short physical-pressure encounter;
- first Reset introduction.

Room-state result:
Open first safe route.

Record:
First occupancy discrepancy.

## LEVEL 1 — CLEARED FLOOR

Location:
1F Open Operations / Copy & Meeting Hub.

Target role:
Complete Prototype Core encounter and CHR_01 introduction.

Locked content foundation:

- CHR_00 ×1
- CHR_01 ×1
- Compliance Runner ×2
- Binder ×3
- Wheeled Chair ×2
- Rolling Drawer Cabinet ×1
- Copier ×1 recognition-only

Core gameplay:

- physical ally relay;
- Light Catch;
- Queued Throw;
- Direct Medium;
- first Cargo Strap use;
- Drawer room-state change;
- Reset.

Rescue:
Outsourced staff excluded from exit communication.

Room-state:
Open Side Lane, Service Stair, or supply lane.

Record:
Unofficial seating area and actual door.

Foreshadow:
Central promotional wall visible from the atrium perimeter.

## LEVEL 2 — ASSET FLOW

Location:
B1 Automated Asset Dispatch / Loading Dock.

Target role:
Moving logistics flow and immediate human stakes.

Core gameplay:

- moving cart route;
- Medium redirection;
- Cargo Strap route change;
- Interrupt / Redirect / Transfer objectives.

Rescue:
Workers refuse to leave until personal medicine and work property are recovered.

Room-state:
Redirect cart flow to open Freight Control and asset cage.

Story:
The system classifies personal necessities as removable assets.

Shortcut:
Loading Dock Service Gate ↔ Facilities Control.

## LEVEL 3 — NOT ON THE PLAN

Location:
2F Hidden Contractor Annex.

Target role:
First major hidden-space reveal and As-Built expansion.

Core gameplay:

- sound and physical evidence;
- partition movement;
- supply-lane reconstruction;
- transfer between split lanes;
- Reveal / Transfer objectives.

Rescue:
Three contract support workers.

Room-state:
Expose the real Annex access through shelving or non-load-bearing partition movement.

Record:
First major hidden Annex.

Shortcut:
Hidden Service Passage ↔ Service Junction.

## LEVEL 4 — LOCAL INCIDENT

Location:
2F Records Archive / Electrical Support.

Target role:
Emergency, external responders, and moral commitment.

Core gameplay target:

- localized smoke volume;
- partial lighting;
- secure and hold objectives;
- Cabinet movement;
- Service Breaker access;
- limited late-game system combinations subject to G4/G5 evidence.

Story:

- temporary electrical equipment overheats;
- CHR_00 calls emergency services;
- responders arrive using official plans;
- CEO refuses a full handover stop;
- event is recorded as a local incident.

As-Built record:

- Annex access;
- temporary feed;
- secondary stair;
- responder isolation point.

### Lobby commitment beat

CHR_00 reaches an interior lobby threshold.

Outside are:

- incident-control barrier;
- muster line;
- accountability table;
- safety personnel recording actual names and counts.

The official system still reports `OCCUPANCY: 0`.

The player retains control.
No choice UI and no automatic turn.

The player may approach the exit threshold.
Crossing the external controlled boundary would end re-entry for that shift, but no
separate exterior level or alternate ending is produced.

To continue the story, the player physically turns CHR_00 and returns toward the
Executive Service Core.

The camera and route must clearly frame the Accountability Table and the act of writing
real names, contrasting with the system’s false zero.

## LEVEL 5 — HANDOVER SUITE

Location:
3F Executive Reception / Board Support.

Target role:
Past responsibility, freight preparation, and access to Atrium release.

Core gameplay:

- sparse executive environment;
- actual physical objects transferred upward through the restored freight lift;
- Transfer / Secure encounter;
- maintenance-rail access.

Flashback:

- 45–60 second in-engine match cut;
- past promotional-wall installation;
- alternate-egress assurance;
- CHR_00’s low-priority variance record;
- approval of the work.

Room-state:
Open maintenance control, upper ring, and secondary-stair release access.

Record:
Connect original drawings and deleted warning to the As-Built layer.

Safe room:
Executive Fire Refuge.

## LEVEL 6 — ZERO DEFECT

Location:
Central Atrium.

Target role:
No-boss spatial Finale and public proof.

### Phase 1 — BREAK THE PERIMETER

- physical combat with Handover Response Team;
- redirect carts;
- move guidance walls;
- keep shutters and barriers from re-closing;
- open central lane.

### Phase 2 — BRING THE LOAD

- move real objects delivered through Freight Lift;
- CHR_01 supplies Light and eligible Medium objects;
- rescued staff provide only fixed scripted support;
- no second active ally.

### Phase 3 — RELEASE THE WALL

Promotional wall:

- non-load-bearing;
- top maintenance rail;
- lower anchors;
- side guides;
- separable panels;
- floor brackets.

The player combines:

- maintenance release;
- cart mass;
- approved Cargo Strap;
- mobile guidance structures.

The wall leaves its guide and settles down.
No explosion, room collapse, or runtime fracture.

Reveal:

- service secondary stair;
- erased exit sign;
- hidden Annex connection;
- illegal modifications;
- final six unaccounted people.

Witnesses:

- CEO;
- buyer technical representative;
- legal team;
- fire and safety responders;
- rescued workers.

Victory means making the denied reality simultaneously visible, not reducing a boss’s
health to zero.

---

# 15. Player Failure Direction

## 15.1 Meaning

Player failure is nonlethal removal from the contested space.

## 15.2 Minimal G4-A direction

```text
Stability depletion
→ nonlethal takedown
→ short removal Bark
→ Fade
→ Encounter Entry Baseline reload
```

First-failure working Bark:

> Sir, this way. You are not supposed to be here today.

Repeated failures use a shortened cue or immediate Fade to avoid repetition.

Retry is a gameplay abstraction, not a canon sequence of repeated removals.

## 15.3 Persistence

Preserved before the current encounter:

- completed story beats;
- completed rescue clusters;
- completed As-Built records;
- unlocked shortcuts.

Reset within the current encounter:

- enemies;
- physical objects;
- current room state;
- carry, relay, strap, and current causal-origin state;
- incomplete rescue objective.

Exact Stability values, recovery, animation, and timing remain G4 tuning.

---

# 16. Progression and Roguelite Disposition

## 16.1 Primary campaign

The primary game is not a repeating Roguelite run structure.

There is:

- one authored campaign;
- one ending;
- no permanent stat tree;
- no weapon rarity;
- no resource grinding;
- no procedural room generation.

## 16.2 Existing systems

Existing approved Momentum, Pip, HeavyAction, Perfect Catch, Relay Position, and other
G2 contracts are preserved as system baselines.

Their exact campaign activation, tuning, and production inclusion remain subject to G4/G5.

## 16.3 Policy Violation

Status:

`DEFERRED REPLAY LAYER / NOT REQUIRED FOR G4-A OR PRIMARY CAMPAIGN`

The existing approved contract is not deleted.
Its Applicability is moved out of the required primary-campaign baseline by this Change
Control.

Future options:

- limited post-clear replay modifier;
- later expansion;
- permanent deferment.

No implementation is authorized now.

---

# 17. Finale and Ending

## 17.1 Finale tension

No traditional boss.

Tension comes from:

- Response Team;
- automated equipment;
- buyer and legal team progressing through handover;
- physical transfer of required objects;
- access to maintenance release;
- hidden people’s route.

## 17.2 Ending

- Buyer stops handover.
- External responders receive the As-Built Map.
- Approximately fifteen unaccounted people are confirmed.
- Illegal modifications and unofficial occupancy enter the record.
- The company declares insolvency the next day.
- Final salary and severance remain uncertain.
- The CEO is not immediately arrested on screen.
- CHR_00 faces investigation for interference and property damage.
- Workers respond differently: gratitude, anger, silence.

Final image:

- CHR_00 leaves his badge at the entrance.
- Incident command spreads the As-Built Map across a vehicle hood.
- The map contains hidden rooms, temporary feeds, blocked stairs, occupancy, and routes.
- The promotional wall settles behind them.
- The old secondary-stair sign is visible.

Cut to black.

`DESTROY YOUR COMPANY: ZERO DEFECT`

---

# 18. Full-Game Content Target and Hard Caps

| Area | Target / cap |
|---|---:|
| Campaign | 1 |
| First play | 3.5–5 hours |
| Floor bands | 4 |
| Main levels | 7 |
| Major physical encounters | 6–7 |
| Safe rooms | 3 |
| Major shortcuts | 4 |
| General active allies | 1 |
| General enemy roles | up to 3 |
| Traditional bosses | 0 |
| Rescue clusters | 4 |
| Unaccounted rescue population | about 15 |
| Unique major voice roles | tightly limited |
| Story ending | 1 |
| Procedural levels | 0 |
| Permanent upgrade tree | 0 |
| FIN_01 | OUT |
| Final large authored set piece | promotional wall, conditional on later production approval |

These are full-game design caps, not immediate implementation authorization.

---

# 19. G4-A Core Room Validation Brief

## 19.1 Objective

Determine whether the locked physical core is readable, controllable, repeatable, and
worth expanding into the approved full game.

## 19.2 Target experience

One repeatable `5–10 minute` Core Room encounter.

G4-A is not a Story chapter, micro vertical slice, or campaign production.

## 19.3 Included content

Exactly the locked Prototype foundation:

- CHR_00 ×1
- CHR_01 ×1
- Compliance Runner ×2
- Binder ×3
- Wheeled Chair ×2
- Rolling Drawer Cabinet ×1
- Copier ×1 recognition-only
- one authored room
- one fixed CHR_01 support position
- one Drawer room-state objective

Included actions and systems:

- Move
- Look
- Aim
- InteractPrimary
- Direct Acquisition
- Normal Catch
- Queued Throw
- Direct Throw
- physical ally Light supply
- floor-supported ally Medium supply
- Cargo Strap redirect
- causal result ownership
- ResetEncounter
- functional third-person camera
- minimum nonlethal player pressure and failure direction

## 19.4 Excluded content

- full Story scenes;
- rescue NPC production;
- As-Built production UI;
- Survey system;
- Cart AI;
- Freight Lift persistence;
- Momentum/Pip;
- HeavyAction;
- Perfect Catch;
- Policy Violation;
- Relay Position transition;
- Suppressed;
- destruction;
- glass;
- debris;
- Structural Feature;
- Safe Room;
- save system;
- final art;
- production animation;
- audio polish;
- campaign levels;
- promotional-wall Finale.

## 19.5 Runner starting-layout variants

Test Harness only.
No random generation and no new AI behavior.

- `Variant A`: Runner 1 begins between the player and the direct Drawer approach.
  Runner 2 pressures the central movement lane.
- `Variant B`: Runner 1 begins on CHR_01’s supply lane.
  Runner 2 begins near the Drawer objective.
- `Variant C`: Runner 1 begins between the player and the Direct Pool.
  Runner 2 begins between the player and the Strap Anchor lane.

## 19.6 Evidence questions

- `Q1`: Under pressure, does the player voluntarily use the core Relay, Throw, and Strap actions?
- `Q2`: Can the same Drawer objective be solved through at least two valid physical approaches?
- `Q3`: Is Player–Ally–Object causality readable without explanation?
- `Q4`: After failure or Reset, do the layout variants create a reason to try another approach?
- `Q5`: Can the camera simultaneously keep the player, active object, enemy pressure, and exit objective readable?

Mandatory questions:

`Q1`, `Q3`, and `Q5`

## 19.7 Decision rule

### PASS

```text
4–5 YES
AND Q1 = YES
AND Q3 = YES
AND Q5 = YES
```

Result:

- `G4-A PASS`
- Owner may separately decide whether G4-B may begin.

### Conditional Q2 boundary

If the PASS rule is met while `Q2 = NO`:

- G4-A retains PASS for core viability;
- G4-B entry remains blocked;
- revise the Core Room layout exactly once;
- recheck only Q2;
- if Q2 remains NO, do not enter G4-B and keep Story/Campaign/Map/Level production integration blocked.

### One retune round

If:

- total YES = 2–3; or
- any mandatory question is NO;

then:

- tune only within the same G4-A scope;
- perform exactly one retest;
- no new mechanic, content type, enemy role, story scene, or system may be added.

### Core redesign

If:

- total YES = 0–1; or
- the single retest still fails the PASS rule;

then:

- `CORE LOOP REDESIGN REQUIRED`;
- G4-B remains blocked;
- production implementation of Levels 0–6 remains blocked;
- additional Story/Campaign/Map/Level expansion remains frozen;
- the approved full-game target remains the intended product unless the Owner later changes it through Change Control.

## 19.8 Exploratory round

At most one exploratory evidence round is allowed.

The next round must use the binary rule above.
No indefinite exploratory state.

## 19.9 Production-speed evidence

G4-A must record:

1. actual work time from empty project state to playable Core Room;
2. actual weekly development time available during the round;
3. rework time, tool/asset wait time, reusable outputs, and external-asset dependency.

Do not multiply one sample directly into a full-campaign schedule.

Character locomotion production-time measurement belongs to G4-B, not G4-A.
G4-A may use capsule or placeholder locomotion.

---

# 20. Locked versus Deferred

## 20.1 Locked full-game design target

The following are approved as the intended full game:

- working title;
- ZERO DEFECT/HANDOVER worldline;
- single-campaign product direction;
- 3.5–5-hour first-play target;
- one building and Hybrid Vertical Loop;
- Levels 0–6 and their narrative/gameplay roles;
- three-act story;
- CHR_00 and CEO arcs;
- CHR_01 role;
- approximately fifteen unaccounted rescue people;
- Official Plan versus As-Built Notes;
- external-response logic;
- lobby commitment beat;
- no traditional boss;
- Atrium promotional-wall Finale;
- insolvency ending;
- As-Built Map remaining as evidence.

## 20.2 Later-Gate evidence and tuning

Not fixed here:

- exact room dimensions;
- corridor width;
- exact enemy counts and placement;
- exact object placement;
- camera numbers;
- Stability values;
- Catch, Throw, and Strap timings;
- smoke-volume dimensions;
- survey interaction duration;
- final wall physics parameters;
- exact level runtime;
- performance budgets;
- final asset count and production schedule.

## 20.3 Implementation boundary

This document does not:

- install Unity;
- open the Implementation Gate;
- authorize G4 execution;
- create Game/** content;
- authorize Levels 0–6 implementation;
- approve final art or production assets;
- issue an Execution Authorization Relay.

---

# 21. Next Authorization Boundary

After repository registration of this Change Control:

`OWNER EXPLICIT UNITY TOOLCHAIN PROVISIONING INSTRUCTION REQUIRED`

Then:

```text
Toolchain provisioning
→ empty Unity bootstrap authorization
→ G4-A Core Room implementation
→ G4-A evidence and decision
→ only after PASS, staged production of the approved full-game target
```

No additional speculative Story, Campaign, Map, or Level design document is required
before G4-A.
