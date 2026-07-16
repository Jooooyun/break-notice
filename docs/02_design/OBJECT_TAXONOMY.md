# Object Taxonomy

## Prototype Object Set

| ID | 물체 | 무게 | 태그 | 전달 | 파손 |
|---|---|---|---|---|---|
| OBJ-L-001 | 파일철 | Light | Rebound | 동료 투척/직접 집기 | 선택적 |
| OBJ-L-002 | 키보드 | Light | Fragile | 동료 투척/직접 집기 | 키캡 VFX |
| OBJ-M-001 | 사무용 의자 | Medium | Rolling | 동료 굴림/직접 집기/스트랩 | 큰 부품 제한 |
| OBJ-M-002 | 모니터 | Medium | Fragile | 직접 집기 | 화면·스탠드 분리 |
| OBJ-H-001 | 복사기 | Heavy | Powered, Crush | Momentum 사용 | 용지함·덮개·토너 |
| STR-001 | 유리 파티션 | Structural | Fragile | 충돌 결과 | 프레임 큰 조각 + 유리 VFX |

## Vertical Slice Expansion

- 이동식 화이트보드: Medium + Slab
- 이동식 서랍장: Medium + Rolling/Crush
- 철제 캐비닛: Heavy + Crush
- 회의실 테이블: Heavy + Slab
- 기록 카트: Medium + Rolling

## 데이터 필드

- ID
- Display Name
- Weight Class
- Behavior Tags
- Mass Baseline
- Pickup Rules
- Ally Delivery Mode
- Throw Speed Range
- Collision Detection Mode
- Break Threshold
- Break Prefab
- Chain Relay Multiplier
- Momentum Value
- Audio Material
- VFX Material
- Pooling Policy
