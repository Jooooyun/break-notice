# Object Taxonomy

> **G2 STATUS BOUNDARY**
>
> Light/Medium/Heavy의 기능 envelope와 class-based eligibility는
> [`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)에서
> Owner-approved다. Prototype의 exact object ID, count, eligibility와
> content role은
> [`PROTOTYPE_CONTENT_LOCK.md`](PROTOTYPE_CONTENT_LOCK.md)의
> `OWNER-APPROVED G2 PROTOTYPE CONTENT LOCK`을 따른다. exact physical
> value, Behavior Tag, final art와 technical field는 계속 `DRAFT`다.

## Prototype Object Set — Draft Content Candidate

이 역사적 heading은 RTM heading compatibility를 위해 유지한다.
아래 현재 Prototype 표 자체는 Owner-approved Content Lock이며 정확히
네 개의 definition과 일곱 개의 instance를 가진다. ID는 문서상 참조
식별자일 뿐 runtime ID, prefab, enum, ScriptableObject, 파일명 또는
database key가 아니다.

| ID | 물체 | Weight | 수량 | Direct | Ally | Catch | Strap | Content role |
|---|---|---|---:|---|---|---|---|---|
| `OBJ-P-L-001` | File Binder | `LIGHT` | 3 | 1 | 2 airborne | YES | NO | direct/relay throw와 causal initiator |
| `OBJ-P-M-001` | Wheeled Office Chair | `MEDIUM` | 2 | 1 | 1 grounded | NO | YES — valid metal Anchor | direct handling과 redirect |
| `OBJ-P-M-002` | Rolling Drawer Cabinet | `MEDIUM` | 1 | NO | NO | NO | NO | attributed displacement와 side-lane clearance |
| `OBJ-P-H-001` | Copier-Class Asset | `HEAVY` | 1 | NO | NO | NO | NO | recognition-only landmark |

Binder, Chair와 Drawer의 세 definition만 gameplay-interactive다.
Copier는 static, floor-supported Heavy recognition evidence이며
Prototype active interaction에 참여하지 않는다. 추가 interactive
object 또는 object type을 넣지 않는다. decorative clutter는
non-interactive이며 gameplay physics나 causal role을 갖지 않는다.

Prototype에는 required Behavior Tag, breakage, glass, debris, Structural
Feature 또는 Momentum/HeavyAction use가 없다. 역사적 keyboard, monitor,
glass partition과 Heavy `Momentum 사용` 행은
`SUPERSEDED PROTOTYPE DRAFT`이며 현재 Prototype content가 아니다.

## Vertical Slice Expansion — Draft Candidates

- 이동식 화이트보드: Medium + Slab
- 이동식 서랍장: Medium + Rolling/Crush
- 철제 캐비닛: Heavy + Crush
- 회의실 테이블: Heavy + Slab
- 기록 카트: Medium + Rolling

FIN_01 conference table은 이 예시로 제품 또는 Vertical Slice 범위에
편입되지 않는다.

## G2-Approved Design Dimensions

- Weight Class
- Handling Profile
- Direct Acquisition Eligibility
- Carry Eligibility
- Throw Eligibility
- Relay Delivery Profile
- Catch Eligibility
- Strap Eligibility
- HeavyAction Eligibility
- First-Order Impact Role
- Local Room-State Role

위 design dimension 중 Prototype object-specific assignment는
`PROTOTYPE_CONTENT_LOCK.md`가 고정한다. Vertical Slice object ID,
content count와 milestone inclusion은 계속 `DRAFT`다.

## Later Draft Fields

- Behavior Tags
- Mass Baseline
- Throw Speed Range
- Collision Detection Mode
- Break Threshold
- Break Prefab
- Chain Relay Multiplier
- Momentum Value
- Audio Material
- VFX Material
- Pooling Policy

위 값은 later G2 tuning, G3 technical budget/architecture 또는 G4
Graybox criterion에서 별도 결정한다. 이 문서는 C# field, enum,
ScriptableObject schema, database column, serialized asset 또는 Unity
component를 승인하지 않는다.
