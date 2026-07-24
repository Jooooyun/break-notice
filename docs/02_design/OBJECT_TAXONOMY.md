# Object Taxonomy

> **G2 STATUS BOUNDARY**
>
> Light/Medium/Heavy의 기능 envelope와 class-based eligibility는
> [`WEIGHT_CLASS_DATA_RANGE_CONTRACT.md`](WEIGHT_CLASS_DATA_RANGE_CONTRACT.md)에서
> Owner-approved다. 아래 object ID, object count, Behavior Tag, exact
> milestone inclusion, mass, throw speed, break value와 technical field는
> 계속 `DRAFT`다.

## Prototype Object Set — Draft Content Candidate

이 표는 별도 Prototype content lock 전의 illustrative candidate다.
정확한 물체 목록·수·태그·파손 상태 또는 milestone inclusion을
승인하지 않는다.

| ID | 물체 | 무게 | 태그 | 전달 | 파손 |
|---|---|---|---|---|---|
| OBJ-L-001 | 파일철 | Light | Rebound | 동료 투척/직접 집기 | 선택적 |
| OBJ-L-002 | 키보드 | Light | Fragile | 동료 투척/직접 집기 | 키캡 VFX |
| OBJ-M-001 | 사무용 의자 | Medium | Rolling | 동료 굴림/직접 집기/스트랩 | 큰 부품 제한 |
| OBJ-M-002 | 모니터 | Medium | Fragile | 직접 집기 | 화면·스탠드 분리 |
| OBJ-H-001 | 복사기 | Heavy | Powered, Crush | Momentum 사용 | 용지함·덮개·토너 |
| STR-001 | 유리 파티션 | Structural | Fragile | 충돌 결과 | 프레임 큰 조각 + 유리 VFX |

위 Heavy 행의 `Momentum 사용`과 Structural 행의 exact relationship은
Owner-approved rule이 아닌 역사적 Draft다. Structural Feature는
Weight Class가 아니며 어떤 Heavy도 자동으로 Structural이 되지 않는다.

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

현재 exact ID 목록과 content count는 별도 Prototype content lock
전까지 Draft다.

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
