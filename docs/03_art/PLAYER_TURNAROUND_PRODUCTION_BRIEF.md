# CHR_00 Player Turnaround Production Brief

`OWNER-APPROVED / CONSUMED BY APPROVED TURNAROUND`

Decision Source: `TASK-0057 delegated Design Authority decision`; registration
source: `TASK-0059` after GPT Pro `PASS` and Human Owner `APPROVE`

## Authority and Gate Boundary

이 문서는 CHR_00 Player Turnaround 이미지 한 장의 제작 구성과 이진
검토 기준, 그 기준을 통과한 승인본의 등록 증거를 보존한다. 등록된
Turnaround는 `LOCKED PRODUCTION SUPPORT`이며 3D 모델, 리깅, texture,
material, Unity asset, Graybox 또는 구현을 생성하거나 승인하지 않는다.
G4는 `NOT PASSED / NOT STARTED / NOT AUTHORIZED`, G5는 `NOT PASSED /
FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`, Full Design Lock은
`INCOMPLETE / FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`, Graybox는
`NOT STARTED`, Implementation Gate는 `CLOSED`다.

## Sole Identity Reference

CHR_00의 유일한 Identity 권위는 다음 세 항목이다.

- `references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png`
- `docs/03_art/PLAYER_VISUAL_SPEC.md`
- `docs/09_adrs/ADR-0007-PLAYER-VISUAL-LOCK.md`

다른 승인 gameplay 이미지는 이미 문서화된 continuity 확인에만 사용할
수 있으며 위 세 권위를 덮어쓰지 않는다. 탐색 이미지, 역사적 prompt,
incidental detail과 미승인 생성 결과는 Identity 권위가 아니다.

## Approved Registration Evidence

- 검토 소스:
  `references/exploration/CHR_00_Player_Turnaround_v001_REVIEW.png`
- 승인 등록본:
  `references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png`
- 검토 결과: `PASS — REVIEW CONTRACT SATISFIED`
- Human Owner disposition: `APPROVE`
- 등록 상태:
  `OWNER-APPROVED / REGISTERED / LOCKED PRODUCTION SUPPORT`
- SHA-256:
  `8a852d77296ebf39cb3cf81c51d931d141dff63f358182ab5638bbbd4329d214`
- 픽셀 크기: `1774 × 887`
- 파일 크기: `2,096,375 bytes`

승인 등록본은 검토 소스의 byte-identical copy다. 검토 소스는 기존
untracked `REVIEW` 증거로 보존되며 승인 등록본만 tracked Production
Support다.

## Exact Five-View Layout

한 장에는 같은 바닥선과 같은 축척을 공유하는 전신 뷰를 정확히 다섯
개만 둔다.

1. `FRONT ORTHOGRAPHIC`
2. `LEFT PROFILE`
3. `REAR ORTHOGRAPHIC`
4. `RIGHT PROFILE`
5. `FRONT THREE-QUARTER CONTINUITY VIEW`

앞·왼쪽·뒤·오른쪽의 네 modeling view는 orthographic이어야 한다.
Front three-quarter view는 Identity와 장비 continuity 확인 전용이며
모델링 orthographic view를 대체하지 않는다. 다섯 뷰 모두 같은 인물,
신체 비율, 의복, 장비, 승인 이미지와 동일한 브레이스 신체 측, 축척과
바닥선을 유지한다. crop, action pose, cinematic framing, perspective
exaggeration, depth of field와 motion blur는 허용하지 않는다.

## Neutral Modeling Pose

- 현실적인 성인 비율의 직립 자세
- 발은 대략 어깨너비, 양쪽 발에 고른 체중
- 하네스와 셔츠 옆선을 읽을 수 있을 만큼 몸에서 떨어진 팔
- 힘을 빼고 손가락이 보이는 손
- 내려간 어깨와 중립적인 골반·상체

combat stance, 물체 상호작용, 스트랩 장력, superhero pose, 과장된
contrapposto는 금지한다. 이 자세는 rig bind pose 또는 정확한 관절 각도
승인이 아니다.

## Body and Identity Continuity

현실적인 성인 남성, 평범한 시설·자산관리 직원 체형, 지치고 집중된
인상을 유지한다. athlete, bodybuilder, superhero 체형으로 확대하거나
나이·스타일을 재설계하지 않는다. 과장된 animated face, hero jaw,
새 흉터·문신·cybernetic 요소를 추가하지 않는다. 정확한 모공, 머리카락
가닥과 studio-lighting 반응은 Canon이 아니다.

## Garment Continuity

- 구겨지고 사용감 있는 흰색 셔츠와 걷어 올린 소매
- 짧고 느슨한 암적색 넥타이
- 일반 회사 사원증
- 어두운 작업용 바지
- 사용감 있는 작업 장갑과 안전화
- 현실적인 작업장 마모와 제한된 오염

넥타이는 일반 복장일 뿐 장비, 물체 또는 신체 하네스와 연결되지 않는다.
military·ballistic·ammo·superhero 요소, jacket, 새 logo를 추가하지
않으며 사원증 문구는 읽을 수 없는 일반 표기로 유지한다. 정확한 주름,
얼룩, stitch와 label 위치는 Canon이 아니다.

## Equipment Continuity

- 산업용 하중 분산 하네스
- 한쪽 팔의 외부 장착형 전완 하중 보조 브레이스
- 허리 장착형 수축식 화물 스트랩 릴
- 검정·황색 화물 스트랩
- 은색 금속 카라비너
- 제한된 안전 표시

브레이스는 모든 뷰에서 승인 이미지와 동일한 신체 측에 있어야 하며
좌우를 뒤집거나 새 명칭으로 추론하지 않는다. 릴 위치도 모든 뷰에서
일관되어야 한다. 스트랩과 카라비너는 보관·고정 상태이고 가구에
연결되지 않는다. active 또는 floating 상태, 마법·광원 효과, 전투용
손, firearm, 완전한 rope simulation과 새 gameplay 기능 추론은
금지한다. 정확한 bolt, label, 내부 작동과 소형 construction은
Canon이 아니다.

## Rear and Profile Readability

Rear view는 하네스의 등쪽 routing, 어깨·허리 하중 분산, belt·harness에
대한 릴 상대 위치, 셔츠·바지 실루엣과 안전화 후면 실루엣을 읽게 한다.

두 Profile view는 브레이스 두께와 팔 분리, 하네스 깊이, 릴 돌출,
보관된 스트랩·카라비너 실루엣, 넥타이 분리, 장갑과 안전화 측면을
읽게 한다. 승인 이미지에 보이지 않는 내부 구조는 발명하지 않는다.

## Authorized Detail Callouts

허용되는 detail callout은 다음 정확히 세 개가 최대다.

1. `FOREARM BRACE AND GLOVE SEPARATION`
2. `WAIST REEL, STORED STRAP AND CARABINER`
3. `REAR HARNESS ROUTING`

callout은 modeling readability만 보조하며 새 tool, 기능 또는 mechanism을
추가하지 않는다. 정확한 screw, label, 제조 방식은 승인하지 않고,
callout은 main view를 대체하지 않는다. 네 번째 callout은 허용하지 않는다.

## 3D Simplification Boundary

작은 장비 부품은 큰 기능 단위로 통합하고, 미세 주름은 normal map
후보로 취급하며, 머리카락은 card 또는 큰 mass로 단순화할 수 있다.
실루엣에 중요한 도구만 후속 geometry 후보가 될 수 있고, 넥타이는
제한된 bone chain을 전제로 하며 과도한 cloth simulation은 금지한다.
브레이스 측 손과 장갑은 별개의 형태로 읽혀야 한다.

이는 topology, polygon count, texture·UV·material 수, LOD, skeleton,
rig, skin weight, blendshape 또는 Unity import를 승인하지 않는다.

## Background and Presentation

중성의 밝은 회색 또는 중간 회색 배경, 고른 중립 조명과 하나의
바닥선만 사용한다. environment, furniture, ally, enemy, effect, HUD,
logo와 brand는 금지한다. 작은 view·callout label은 인물과 겹치지
않아야 한다.

## Canon and Non-Canon Boundary

이 문서의 Identity, 다섯 뷰 구성, pose·의복·장비 continuity,
readability와 이진 검토 항목만 제작 계약이다. 다음은 Non-Canon이다.

- 정확한 모공과 머리카락 가닥
- 얼룩·긁힘·주름의 정확한 위치
- bolt와 label 문구
- 미세 construction
- 조명·그림자의 정확한 값과 형태
- 생성 과정의 미세 비대칭
- typography와 spacing

## Binary Review Contract and Result

후보는 아래 항목을 모두 만족할 때만 `PASS`다. 하나라도
불충족이면 `REVISION REQUIRED`이며, 단순히 매력적이거나 렌더링
품질이 높은 것만으로는 통과할 수 없다.

- 승인 이미지·Player Spec·ADR-0007과 Identity, 신체, 얼굴 continuity
- 정확히 다섯 개의 전신 뷰와 동일 축척·바닥선
- Front, Rear, Left Profile, Right Profile, Front Three-Quarter의 완전성
- 네 modeling view의 orthographic 구성
- 승인 이미지와 동일한 브레이스 신체 측
- 하네스, 허리 릴, 보관 스트랩과 은색 카라비너의 일관된 존재
- 붉은 넥타이와 모든 장비·물체·하네스의 완전한 분리
- 장갑과 브레이스 측 손의 분리 및 양발의 완전한 가시성
- crop, action pose, cinematic perspective exaggeration 없음
- military, superhero, cybernetic hand, firearm 없음
- glow, supernatural 요소와 발명된 gameplay 기능 없음
- detail callout 최대 세 개 및 승인된 세 주제만 사용
- Non-Canon detail을 기능·Identity 권위로 승격하지 않음
- 생성된 후보가 계속 `REVIEW`임을 명시
- 명시적 Owner 승인·등록 전 Design Freeze의 Turnaround 항목 미완료 유지

`CHR_00_Player_Turnaround_v001_REVIEW.png`는 위 계약을
`PASS — REVIEW CONTRACT SATISFIED`했고 Human Owner가 `APPROVE`했다.
승인 전 REVIEW 상태 유지와 등록 전 checklist 미완료 조건도 준수했다.

## Image-Generation Authorization Result

Human Owner의 명시적 이미지 생성 요청은 승인 후보 한 장의 생성과
검토에 사용되어 소비되었다. 해당 후보는 이후 명시적 Human Owner
`APPROVE`로 등록되었다. 이 이력은 추가 이미지 생성, 변형, 재생성,
3D 제작 또는 구현 권한으로 확장되지 않는다.

## Design Freeze Boundary

Production Brief 승인만으로는 `주인공 Turnaround` checklist를
완료하지 않는다. 실제 `REVIEW` 후보가 이 계약을 통과하고 Human
Owner가 명시적으로 승인했으며 승인본과 ADR-0018이 등록되었으므로
해당 checkbox를 체크한다. Design Freeze는 `32 / 32`와 `COMPLETE /
FINAL PRE-IMPLEMENTATION REVIEW REQUIRED`다. 이는 G5 passage, Full
Design Lock 완료, Graybox, 3D 제작, Unity 또는 구현 권한이 아니다.
