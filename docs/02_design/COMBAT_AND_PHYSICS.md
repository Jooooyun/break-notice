# Combat and Physics Design

## 1. 적 상태

- `Health`: 전투 지속력
- `Stability`: 균형과 자세
- `Guard`: 방패·장비 방어 상태(해당 적만)

Light는 공격·Guard·Stability를 끊고, Medium은 위치와 Stability를 크게 바꾸며, Heavy는 강제 자세 붕괴와 공간 변화를 만든다.

## 2. 무게별 결과

| 등급 | 접촉 정지 시작값 | 기대 결과 |
|---|---:|---|
| Light | 20~35ms | 공격 끊기, 부분 반응, 작은 파편 |
| Medium | 55~80ms | 2~5m 이동, 벽·가구 후속 충돌 |
| Heavy | 90~130ms | 다중 자세 붕괴, 구조 파괴, 통로 변화 |

값은 Graybox에서 튜닝하지만 상대적 계층은 고정한다.

## 3. 물체 행동 태그

| 태그 | 기능 | 예시 |
|---|---|---|
| Rolling | 바닥 속도를 오래 유지 | 의자, 카트 |
| Slab | 넓은 면적으로 여러 대상 밀침 | 화이트보드, 상판 |
| Rebound | 벽·방패에서 크게 반사 | 전화기, 스테이플러 |
| Fragile | 첫 충돌 후 후속 탄약 생성 | 머그컵, 모니터 |
| Powered | 파손 시 전기·종이·토너 결과 | 선풍기, 복사기 |
| Anchored | 고정부 해제 전 이동 제한 | 벽걸이 TV, 설비 |
| Crush | 전도·낙하 압착 | 캐비닛, 책장 |

## 4. 적 물리 반응

- 일반 이동·공격 중에는 애니메이션 기반
- 강한 Medium/Heavy에서 제한된 physical animation 또는 ragdoll
- 벽 충돌 후 회복 상태 제공
- 무한 회전, 관통, 접힘을 막기 위해 각속도·관절 제한
- 전투 불능은 안전복·보호 장비 파손과 자세 고정으로 표현 가능

## 5. 파괴 계층

### Dynamic Prop

처음부터 움직일 수 있는 단순 물체.

### Breakable Furniture

충격 임계값에서 정상 메시를 끄고 사전 파손 프리팹을 활성화한다.

### Structural Feature

방별 1~2개. 파괴 시 통로·시야·전투 위치가 바뀐다.

## 6. 타격감 순서

```text
Anticipation
→ Contact Freeze
→ Impulse Release
→ Secondary Collision
→ Material Burst
→ Aftermath
→ Momentum/Chain Confirmation
```

## 7. 실패 기준

- Heavy가 적을 제자리 움찔만 시킴
- Light가 방 끝까지 적을 날림
- 파편이 주 행동을 가림
- 카메라 흔들림이 결과보다 강함
- 동일 물체가 매번 예측 불가능하게 폭발함
