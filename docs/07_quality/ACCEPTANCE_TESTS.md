# Acceptance Tests

## AT-SCOOP-001 — 근거리 직접 집기

**Given** 플레이어 1.8m 안에 유효한 Light 물체가 있고 이동 중이다.  
**When** InteractPrimary를 누른다.  
**Then** 이동을 장시간 정지하지 않고 물체를 획득한다.  
**And** 물체가 먼 거리에서 순간이동하지 않는다.

## AT-PHY-001 — 실제 궤적

**Given** 동료가 Light를 전달한다.  
**When** 물체를 방출한다.  
**Then** Rigidbody가 출발점부터 Catch 영역까지 연속된 공간 경로를 따른다.  
**And** 적·가구가 경로를 가로막을 수 있다.

## AT-ALLY-001 — Medium 굴리기

**Given** Facilities Mover가 의자를 보유하고 패스 경로가 열려 있다.  
**When** 플레이어가 Medium 패스를 요청한다.  
**Then** 동료가 의자를 바닥에 접촉한 상태로 밀어 보낸다.  
**And** 의자는 공중 소환되지 않는다.

## AT-ALLY-002 — Relay Position

**Given** 전투 중 플레이어가 방을 이동한다.  
**When** 동료가 위치를 바꿀 필요가 있다.  
**Then** 승인된 Relay Position 중 하나로만 이동한다.  
**And** 플레이어를 자유 추적하지 않는다.

## AT-ALLY-003 — Suppressed

**Given** 적이 동료의 지원 위치를 압박한다.  
**When** Suppressed 조건이 충족된다.  
**Then** 패스가 일시 중단된다.  
**And** 동료는 사망하거나 Run에서 제거되지 않는다.  
**When** 압박이 해소된다.  
**Then** 지원이 재개된다.

## AT-CATCH-001 — Normal Catch

**Given** 요청한 물체가 Catch 영역으로 진입한다.  
**When** 플레이어가 올바른 방향을 보고 있다.  
**Then** 넓은 보조 판정으로 물체를 받는다.

## AT-CATCH-002 — Perfect Catch

**Given** 물체가 Perfect Catch 창에 진입한다.  
**When** 플레이어가 타이밍 입력한다.  
**Then** Perfect 결과, 짧은 Contact Freeze, Momentum 보너스가 발생한다.

## AT-THROW-003 — Queued Throw

**Given** 물체가 패스 중이고 Throw가 유지되어 있다.  
**When** Catch에 성공한다.  
**Then** 별도 정지·재조준 없이 캐치 동작이 투척으로 이어진다.

## AT-STRAP-001 — 의자 리디렉션

**Given** 굴러오는 의자에 유효 Anchor가 있고 최대 거리 안이다.  
**When** 화물 스트랩을 연결하고 플레이어가 회전한다.  
**Then** 의자는 바닥 운동량을 유지하면서 허용 범위 내에서 방향을 바꾼다.  
**And** 넥타이는 의자에 연결되지 않는다.  
**And** 의자는 공중에 정지하지 않는다.

## AT-ART-001 — 넥타이 기능 분리

승인된 캐릭터·애니메이션·VFX·게임플레이에서 붉은 넥타이가 물체 연결, 당김, 공격, 이동에 사용되지 않는다.

## AT-WEIGHT-001 — 무게 계층

동일 조건에서 Light는 공격 차단, Medium은 위치 이동과 Chain, Heavy는 공간 변화가 명확히 발생한다.

## AT-CHAIN-001 — 계보 전파

**Given** 의자가 적을 밀고 적이 유리벽을 깨고 프레임이 가구를 민다.  
**Then** 하나의 Chain ID에서 깊이가 증가한다.  
**And** 같은 충돌이 중복 보상되지 않는다.

## AT-MOM-001 — 보상 차등

직접 적중보다 2-hop 이상의 Chain이 더 많은 Momentum을 지급한다.

## AT-HEAVY-001 — Heavy 소비

Pip이 부족하면 Heavy를 사용할 수 없고 명확한 피드백을 준다. 충분하면 Pip을 정확히 소비하고 공간 변화가 발생한다.

## AT-IMPACT-001 — 결과

강한 충돌은 적 위치, 자세, 가구, 파손, 통로 중 최소 하나를 바꾼다.

## AT-SCOPE-001 — 동료 수

일반 전투 중 활성 지원 동료는 한 명을 초과하지 않는다.

## AT-ROOM-001 — 방의 잔상

전투 종료 후 시작 스냅샷과 비교했을 때 최소 두 개의 의미 있는 공간 상태가 변경되어 있다.

## AT-FLOW-001 — Reset

Reset 후 물체, 적, 동료, Momentum, Chain, 파손, 이벤트 구독이 기준 상태로 돌아간다.

## AT-ROGUE-001 — 보상 선택

방 클리어 후 서로 다른 행동 변화를 주는 Policy Violation 3개가 제시되고 하나만 적용된다.

## AT-PROP-001 — 행동 태그

Rolling 의자는 일반 Medium보다 첫 충돌 후 수평 속도를 더 오래 유지한다.

## AT-FIN-001 — 특수 합동 피니셔

특수 조건에서만 동료 2명이 Heavy 테이블에 직접 접촉해 밀며, 일반 전투 동료 제한을 영구 변경하지 않는다.
