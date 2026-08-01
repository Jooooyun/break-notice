# Project Charter

> **OWNER-APPROVED PRODUCT BASELINE / EXTENDED BY CR-0001 AND ADR-0021**
>
> FGD-001은 Story, Campaign, Map, Levels 0–6, Finale, Ending과 G4-A의
> full-game target authority다. 이 Charter는 concise product baseline이며
> 구현, Graybox, Gate passage 또는 일정 승인이 아니다.

## 1. 프로젝트명

- 코드명: `BREAK NOTICE`
- Working Title: `DESTROY YOUR COMPANY: ZERO DEFECT`
- 장르: `3D third-person corporate physics action adventure`
- Primary format: `single authored campaign`
- 개발 형태: 1인 개발
- 엔진: Unity 6
- G1 기준 플랫폼: `Windows PC`
- 모바일 동시 출시: `OUT / WON'T`
- 그 밖의 미승인 플랫폼: Change Control 전까지 `DEFERRED`
- 정확한 하드웨어, 해상도와 성능 기준: `G3/G6 DRAFT`
- 개발 방식: 폭포수 모델 + Graybox 검증 게이트

## 2. 문제 정의

일반적인 사무실 액션 게임은 총기나 초능력에 의존하며, 주변 사물이 실질적인 전투 시스템이 아니라 장식에 머무르는 경우가 많다. 본 프로젝트는 평범한 사무용품, 동료의 전달 행동, 실제 궤적과 충돌 결과를 중심으로 전투를 구성한다.

## 3. 제품 비전

BREAK NOTICE / DESTROY YOUR COMPANY: ZERO DEFECT는 본사 매각 전 마지막
밤, ZERO DEFECT/HANDOVER 체계에서 누락된 인원과 비공식 공간을 찾아
탈출시키는 단일 authored campaign이다. 플레이어는 산업용 화물 장비,
동료의 실제 물리 릴레이와 읽을 수 있는 인과 결과로 건물의 Official
Plan과 As-Built reality 사이를 돌파한다. 정확한 Story와 Campaign
authority는 [`FGD-001`](../02_design/FGD_001_DESTROY_YOUR_COMPANY_ZERO_DEFECT_FULL_GAME_BASELINE.md)이다.

### Target Player

읽기 쉬운 물리 액션과 authored-building exploration을 즐기고,
하나의 기업 건물에서 인물·공간·기록의 관계를 따라가며 다른 물체,
각도와 경로의 인과 결과를 발견하려는 플레이어다. Roguelite 경험은
primary campaign의 전제가 아니다.

## 4. 핵심 성공 조건

### Owner-Approved G1 Qualitative Intent

짧은 첫 플레이에서 플레이어는 직접 획득, 동료의 물리적 릴레이와
화물 스트랩 리디렉션이 실제 경로와 기계적 연결로 작동함을
이해하고, 이해 가능한 국소 room-state change를 만든 뒤
다른 물체·각도·순서를 자발적으로 시험하거나 Reset하며,
이를 염력이 아닌 물리적 팀워크로 설명할 수 있어야 한다.

### G4-A Evidence Questions

첫 제작 확장 결정은 FGD-001 §19의 `Q1`~`Q5`를 사용한다. `Q1`,
`Q3`, `Q5`는 mandatory이며 Q2의 conditional G4-B boundary,
한 번의 retune/retest와 최대 한 exploratory round를 따른다.
Full-campaign production은 G4-A PASS 전까지 차단된다.
`3.5–5 hours`는 long-term play target이지 schedule estimate가 아니다.

## 5. 제약 조건

- 핵심 개발자 1명
- Codex를 주요 구현 보조 도구로 사용
- 네트워크 멀티플레이 제외
- 실시간 완전 파괴 제외
- 포토리얼 AAA 품질 제외
- 일반 전투 동료 1명
- G4-A PASS 전까지 full-campaign production expansion 금지

## 6. 제품 원칙

1. Fun first, but every feature must be specifiable and testable.
2. 물체는 순간이동하지 않는다.
3. 충돌은 위치·자세·공간 중 최소 하나를 바꾼다.
4. 동료는 플레이어를 대체하지 않는다.
5. 파괴는 큰 조각과 명확한 결과를 우선한다.
6. 캐릭터는 진지하고, 물리 결과가 코미디를 만든다.
7. 구현량이 아니라 통합·검증 가능성을 기준으로 범위를 정한다.

## 7. 승인 권한

1인 프로젝트이므로 제품 책임자, 설계 책임자, 기술 책임자, QA 승인자는 개발자 본인이다. 단, 역할을 분리하여 문서와 게이트를 생략하지 않는다.
