# BREAK NOTICE — Master Game Design Document

## 1. 한 문장 정의

해고 통보를 받은 사내 자산관리 직원이 산업용 운반 장비와 동료의 물체 패스를 이용해 사무용품을 받아 던지고, 적·가구·설비 사이의 연쇄 충돌로 회사 건물의 전투 공간을 재구성하는 3인칭 물리 액션 Roguelite.

## 2. 대표 장면

```text
시설 동료가 의자를 바닥으로 굴린다.
→ 플레이어가 허리 릴의 화물 스트랩을 의자 프레임에 연결한다.
→ 몸 전체로 회전해 진행 방향을 바꾼다.
→ 의자가 보안요원의 다리를 쓸어 넘어뜨린다.
→ 보안요원이 유리 칸막이를 깨뜨린다.
→ 프레임이 이동식 서랍장을 밀어 복사기에 충돌시킨다.
→ 복사기가 기울며 종이와 토너를 뿜고 새로운 통로가 열린다.
```

**No magic. Just teamwork, momentum, and office violence.**

## 3. 플레이어 판타지

주인공은 군인, 초능력자, 암살자가 아니다. 회사에서 가구·복사기·서버 장비를 옮기던 자산관리 직원이다. 전투 능력은 다음에서 나온다.

- 작업 경험과 물체 취급 기술
- 외부 장착형 하중 보조 브레이스
- 허리 수축식 화물 스트랩
- 실제 방 안의 사무용품
- 동료의 패스와 환경 조작
- 후속 충돌을 읽고 만드는 판단

## 4. 톤

- 세계와 인물은 현실적이다.
- 캐릭터는 상황을 진지하게 받아들인다.
- 코미디는 평범한 물체가 거대한 결과를 만드는 데서 나온다.
- 잔혹한 피와 절단보다 보호 장비 파손, 자세 붕괴, 에어백 전개 등 비살상 표현을 우선한다.

## 5. 핵심 Pillars

### 5.1 Physical Relay

동료는 공격력 버프가 아니라 실제 공간에서 물체를 전달한다. 패스는 가로막히고, 바닥을 구르고, 가구에 부딪히며, 플레이어가 받아야 한다.

### 5.2 Controlled Chaos

결과는 화려하지만 원인은 추적 가능해야 한다. 플레이어는 다음 시도에서 더 긴 Chain을 의도적으로 만든다.

### 5.3 Weight Has Meaning

Light, Medium, Heavy는 피해 숫자가 아니라 전투 역할과 움직임이 다르다.

### 5.4 Room Remembers

전투 종료 시 방의 통로, 가구, 시야, 설비가 시작 상태와 달라야 한다.

### 5.5 Fun Before Content

5분 Graybox가 반복해서 재미있다는 증거 없이 캐릭터 생산, 방 확장, 메타 성장, 보스를 진행하지 않는다.

## 6. 배경

대형 기업이 구조조정 당일 `Business Continuity Lockdown`을 발동한다. 자동 보안 시스템은 해고 대상 직원을 “미승인 자산”으로 분류하고 건물을 봉쇄한다. 주인공과 여러 부서 직원은 최상층 통제실로 올라가 봉쇄와 해고 처리를 직접 끝내려 한다.

## 7. 플레이 세션 구조

```text
Run 시작
→ 방 선택
→ 전투방 진입
→ 물체·동료·적·환경 기회 확인
→ 패스와 연쇄 충돌로 방 클리어
→ Policy Violation 3개 중 1개 선택
→ 다음 방
→ Elite/피니셔
→ Run 종료 또는 다음 층
```

버티컬 슬라이스는 10~15분, 3개 전투방을 목표로 한다.

## 8. 전투 루프

```text
SCOUT     사용할 물체, 동료 위치, 충돌 표면 확인
CALL      동료에게 Light 또는 Medium 전달 요청
CATCH     Normal Catch 또는 Perfect Catch
SETUP     Light로 공격·방패·약점을 끊음
FEED      Medium을 직접 집거나 동료가 굴림
REDIRECT  화물 스트랩과 몸의 회전으로 방향 수정
LAUNCH    적을 벽·가구·다른 적으로 이동
RELAY     후속 충돌과 환경 변화 발생
BANK      Momentum Pip 저장
CASH OUT  Heavy 또는 Structural Feature 사용
REARM     파괴된 가구와 남은 물체로 다음 행동 준비
```

## 9. 기본 전투 동사

- 이동, 전력질주, 짧은 회피
- 낮은 가구 넘기
- Scoop
- Aim
- Quick Throw
- Charged Throw
- Shove
- Call & Catch
- Perfect Catch
- Queued Throw
- Cargo Strap Attach/Release/Redirect
- Heavy Overgrip

## 10. Scoop

- 범위 목표: 1.5~2m
- Light는 이동 중 한 손 획득
- Medium은 짧은 자세 보정 후 집기
- Heavy는 Momentum 없이는 즉시 집을 수 없음
- 바닥 물체는 손에 자연스럽게 들어오도록 자세·회전 보정 가능
- 물체가 공간을 순간이동하는 연출은 금지

## 11. Cargo Strap

붉은 넥타이와 완전히 별개의 검정·황색 산업용 스트랩이다.

가능:

- 연결 가능한 손잡이·프레임에 짧게 연결
- 굴러오는 의자·카트의 방향 변경
- 흔들리는 가구의 최종 전도 방향 보정
- 방패나 자세가 무너진 적을 짧게 당김

불가능:

- 방 전체의 물체를 손으로 끌어오기
- 공중 정지·비행
- Heavy를 원거리에서 당김
- 이동용 그래플링
- 넥타이 연결

## 12. Call & Catch

### Normal Catch

- 요청한 동료와 대략적인 방향을 보고 있으면 넓은 보조 판정
- 실패 스트레스보다 전투 템포 유지 우선

### Perfect Catch

- 도착 직전 짧은 입력 창
- 다음 투척 안정화와 Momentum 보너스
- 짧은 Contact Freeze와 기계적 충격음

### Queued Throw

- 물체가 오기 전 Throw를 유지
- 캐치 충격을 몸 회전으로 이어 즉시 재투척
- 캐치 후 정지·재조준 단계 최소화

## 13. 동료

- 일반 전투 활성 동료 1명
- 자유 추적 AI가 아니라 2~3개의 Relay Position 사용
- 실제 주변 물체를 소비
- 적 압박 시 Suppressed, 죽거나 영구 이탈하지 않음
- 플레이어 대신 적을 처치하지 않음

버티컬 슬라이스 첫 동료는 `Facilities Mover` 하나로 통합한다.

- Light 파일철/키보드 패스
- Medium 의자/카트 굴리기
- Heavy 고정부 해제 보조

## 14. 적

### Compliance Runner

빠른 근접 기본 적. 낮은 Stability, 연쇄 충돌 재료로 읽히지만 공격 압박은 명확하다.

### Partition Guard

파티션·화이트보드 방패 사용. 정면 투척을 막고 측면, Rolling, 환경 충돌을 유도한다.

### Asset Mover / Interceptor

버티컬 슬라이스 확장 후보. 물체 이동 또는 패스 방해 역할. 첫 프로토타입에는 제외 가능하다.

## 15. 물체 무게

### Light — Setup and Precision

파일철, 스테이플러, 키보드, 머그컵. 공격 차단, 약점, 손·장비 타격.

### Medium — Chain Maker

사무용 의자, 모니터, 이동식 서랍장, 화이트보드. 적·가구 위치 이동과 Chain 생성.

### Heavy — Space Breaker

복사기, 철제 캐비닛, 회의실 테이블. Momentum 소비, 공간 파괴, 다중 자세 붕괴.

## 16. 행동 태그

- Rolling
- Slab
- Rebound
- Fragile
- Powered
- Anchored
- Crush
- Tethered

프로토타입에서는 `Rolling`, `Fragile`, `Slab`만 사용한다.

## 17. Momentum

Owner-approved G2 시작 profile은
[`MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md`](MOMENTUM_PIP_STARTING_VALUE_CONTRACT.md)를
따른다.

- Momentum/Pip은 `VERTICAL SLICE ONLY`이며 Prototype Core에서는
  비활성이다.
- run은 Current Momentum `0`, Banked Pip `0`으로 시작한다.
- threshold `100 Momentum`에서 Pip으로 변환하며 최대
  `3 Banked Pips`를 저장한다.
- `HeavyAction` 시작 비용은 `2 Banked Pips`다.
- passive decay와 ordinary-damage resource loss는 없다.
- HeavyAction-origin result는 Momentum을 만들지 않는다.
- Structural Feature의 Pip cost는 승인되지 않았다.
- reward table, modifier, cap과 lifecycle의 정확한 시작값은 위
  계약에 있으며 G4/G6 evidence-based tuning 대상이다.

이 문서의 다른 Momentum·Chain·Heavy·Structural 수치 또는 보상 예시는
위 계약과 별도로 Owner-approved되지 않은 역사적 Draft다. Chain의
logical 생성·전파·종료는
[`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)를
따르며 runtime lineage, exact termination threshold, presentation과
final balance는 계속 Draft다.

## 18. Causal Chain

Owner-approved logical rule은
[`CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md`](CAUSAL_CHAIN_PROPAGATION_TERMINATION_CONTRACT.md)를
따른다.

- eligible player-owned 행동이 Causal Origin과 Initiating Carrier를
  Depth `0`에서 만든다.
- Initiating Carrier의 결과는 Direct, 물리적 Transfer를 받은
  Attributed Carrier의 결과는 Depth `1`~`4` Downstream이다.
- 최대 전파 깊이는 `4`이며 Depth `4` 뒤의 물리 simulation은 계속될 수
  있지만 Depth `5` 귀속 또는 reward를 만들지 않는다.
- Branch는 허용하지만 Branch와 Origin은 서로 merge하지 않는다.
- 같은 Origin의 같은 target/result-family 반복, resting contact,
  jitter와 cycle은 새 Result나 reward를 만들지 않는다.
- completed Reset과 lifecycle 종료는 Origin을 종료하며, 종료된 Origin은
  다시 열리지 않는다.
- Chain은 sequence-agnostic이다. PGT_03의 정확한 의자·적·유리·카트·
  복사기 순서는 gameplay rule이 아니다.

player-facing Chain Link, 문구, HUD, VFX, audio, exact natural-termination
threshold, runtime lineage와 Unity 구현은 Draft다.

## 19. 타격감

### Contact Freeze

- Light: 20~35ms 시작점
- Medium: 55~80ms 시작점
- Heavy: 90~130ms 시작점

### Impulse Release

정지 직후 적과 물체가 명확히 이동한다. 화면 흔들림보다 결과의 위치 변화가 우선이다.

### Sound Layers

1. 비행음
2. 물체 재질 접촉음
3. 보호 장비·신체 둔탁음
4. 벽·가구 후속 충돌음

## 20. 파괴

- Dynamic Prop
- Pre-fractured Breakable Furniture
- Authored Structural Feature

작은 유리·종이·먼지는 VFX다. 큰 프레임·상판·문·용지함만 물리 오브젝트다.

## 21. Roguelite 보상

업그레이드는 숫자보다 행동을 바꾼다.

예:

- `Delegated Authority`: 동료 패스 첫 충돌 강화
- `Flexible Working`: 회피 중 패스 캐치
- `Team Building Exercise`: Medium 적중 후 Light 추가 패스
- `Redirection Policy`: 스트랩 리디렉션 각도 확대
- `Rolling Forecast`: Medium 수평 속도 유지
- `Capital Expense`: Heavy 구조 파괴 시 Momentum 일부 환급

## 22. 버티컬 슬라이스 방

### Room 1 — Open Operations Office

Scoop, Light/Medium, 기본 패스, 첫 Chain.

### Room 2 — Copy and Meeting Hub

유리, 복사기, 토너, Partition Guard, 첫 Heavy.

### Room 3 — Records Archive

좁은 통로, 이동식 선반, 야간 조명 변주, Elite 전투.

## 23. 프로토타입 최소 범위

- Graybox 방 1개
- 플레이어 1명
- Relay 동료 1명
- 기본 적 3명
- Light 2종
- Medium 2종
- Heavy 1종
- 유리벽 1개
- Scoop, 패스, 캐치, 즉시 투척
- 의자 굴리기와 스트랩 리디렉션
- 기본 Stability, Momentum, 1-hop 이상 Chain
- 즉시 Reset

## 24. 절대 하지 않을 것

- Co-op
- 자유 절차 생성
- 모든 벽 완전 파괴
- 총기 중심 전투
- 동료 자유 추적 파티
- 작은 파편 전체 물리
- 재미 검증 전 보스·메타·스토리 캠페인 확대
- 포토리얼 AAA 아트 목표
- 넥타이를 케이블로 사용
