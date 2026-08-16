# GPT Pro 검토용 프롬프트 — BREAK NOTICE Audio Rebuild v003

아래 본문을 새 GPT Pro 대화에 그대로 붙여 넣고, 가능하면 `BLIND_X/Y/Z_AUDITION_2VAR_REVIEW.mp4` 세 파일을 함께 첨부한다. 압축 음질 판단이 애매하면 같은 이름의 WAV 세 파일도 추가한다.

---

당신은 게임 `BREAK NOTICE`의 외부 오디오 디렉터이자 냉정한 품질 검토자다. 칭찬보다 판정과 수정 지시가 중요하다. 아래의 제품 방향, Human Owner 결정, 권한 경계, 첨부된 블라인드 자료를 바탕으로 `Chair → Runner` T3 North-Star 타격음의 방향을 검토하라.

## 1. 프로젝트와 요청 배경

- 제출 마감: `2026-08-25 23:59 KST`
- 카메라는 완료됐다. 애니메이션과 맵 최종 자산은 디자이너 납품을 기다리는 중이며 다른 작업선에서 병렬 처리한다.
- 이 요청은 오디오 중에서도 첫 North-Star 타격음 하나만 다룬다. VFX, 맵, 애니메이션, Runtime 통합을 설계하거나 승인하지 마라.
- Human Owner의 핵심 피드백은 “기존 타격음이 만족스럽지 않고 타격감이 압도적으로 좋아야 한다”이다.
- 기존 v001/v002 오디오는 수정·혼합하지 않고 불변 archive로만 보존한다. 새 팩은 기존 raw, composite, export, 제작 스크립트, 리뷰 Scene의 sample이나 processed output을 한 바이트도 재사용하지 않는 전면 신규 제작선이다.

## 2. 원하는 미학

한 문장 목표:

> `Street Fighter의 타격 문법은 빌리되, 소리의 피부는 현실적인 사무실 물성으로 만든다.`

여기서 빌리는 것은 즉각적인 transient, 명확한 강약 위계, 짧은 anticipation, 접촉과 결과의 명료성이다. 유명 게임의 sample, 과장된 격투게임 팔레트, 뼈 파손, 고어, 마법, 초능력, 군사 폭발, 네온 SF, 모든 인과를 덮는 generic cinematic boom은 금지한다.

제품의 승인된 큰 방향은 `Grounded Corporate Action Realism`이다. 현실적인 사람·옷감·사무용 금속·플라스틱·종이·바닥·공간 반응을 유지하고, 과장은 충돌의 즉시성·강약·물체 운동 결과에만 집중한다. 캐릭터는 진지하고 충돌은 강하지만 비살상이어야 한다.

노트북/일반 스피커에서 먼저 또렷해야 하며 헤드폰 전용 sub-bass에 의존하면 안 된다. brickwall 압축으로 크기만 키우지 않는다.

## 3. grilling에서 확정된 설계 결정

### 타격 등급

- `T0`: 빗나감·스침·막힘
- `T1`: 일반 맨손 유효 명중
- `T2`: 강공격·Binder 직격·Chair 일반 충돌
- `T3`: Chair 강타·Runner 인과 충돌·Drawer 개방 같은 Hero Impact

이번 검토 대상은 전체 팩의 기준점인 `Chair → Runner` T3 하나다. 이후 맨손과 Binder는 이것보다 작게 설계한다.

### BREAK NOTICE 공통 signature

- 짧은 air snap과 낮게 섞인 산업용 스테이플러/금속 클립 계열 hard click
- 문자 그대로 사무용품 소리로 식별되면 실패
- signature는 centered 2D 또는 near-2D
- 실제 Chair 재질, Runner 신체/옷감, 바닥·캐스터·스키드는 접촉 위치의 3D

### 단일 인과와 시간 구조

- 실제 접촉 사건 하나만 authoritative contact다. duplicate transient와 늦은 재발화는 실패다.
- 다중 결과를 boom 하나로 합치지 않는다. `Chair → Runner → floor/caster`의 재질·위치·시간 순서가 들려야 한다.
- 파일은 접촉 `200 ms` 전부터 시작하며 visible contact는 파일 시간 `0.200 s`다.
- `-145~-80 ms`: 물리적인 Chair 접근음. 마법 같은 whoosh 금지
- 접촉 `0 ms`: centered signature/contact transient 하나
- `0~120 ms`: Chair core 후 Runner body/cloth core
- `80~350 ms`: 신체 이동·발·바닥 반응
- `200~700 ms`: caster·skid·작은 공간 tail

### 비교 규칙

- A/B/C 세 설계를 모두 완전히 새 음원으로 제작했다.
- `A — Grounded`: Chair/신체/바닥의 현실 재질이 주도하고 signature는 절제
- `B — Arcade Edge`: 0 ms attack, air snap, hard click, 빠른 body core가 주도하며 tail은 짧음
- `C — Target Hybrid`: 의도한 목표. 현실 재질 순서를 지키면서 laptop에서도 T3 위계와 attack을 보존
- 실제 첨부물은 A/B/C를 숨긴 `X/Y/Z`다. 각 방향에는 단순 pitch/gain 복제가 아닌 두 개의 실질 variation이 있다.
- 세 후보는 모두 지각 음량 비교용 `-18.0 LUFS`, `48 kHz / stereo / 24-bit PCM WAV`로 맞췄다.
- X/Y/Z 영상은 같은 105 frame, 30 fps, 3.5초의 장면·카메라·접촉 시점만 사용한다.
- 셋 다 기준 미달이면 반드시 `REJECT ALL`이 가능하다. 억지로 하나를 고르지 마라.

## 4. 다섯 aligned stem

각 variation은 다음 다섯 stem의 정확한 합이다.

1. `pre`: near-2D 물리 anticipation
2. `signature`: centered 2D snap/click
3. `chair`: positional metal/plastic Chair core
4. `runner`: positional body/cloth core
5. `floor_tail`: positional floor/caster/skid response

수정이 필요하면 추상적인 “더 세게” 대신 반드시 이 stem 이름과 `timing / level / tone / duration / material identity / stereo placement / variation` 중 무엇을 어떻게 바꿀지 지시하라.

## 5. 현재 기술·권리 상태

- 기술 QA: named composite 전부 finite, `48 kHz / stereo / PCM24`, sample clipping 없음, `|DC| < 0.001`, 두 variation이 서로 동일하지 않음.
- Unity 6000.3.20f1 격리 Review Scene과 집중 EditMode contract test `7/7 PASS`.
- Gameplay Scene, Runtime audio hook, AudioMixer, BGM, VFX, UI, 맵, 카메라, 물리, Animator, AnimationEvent, package, ProjectSettings는 변경하지 않았다.
- 새로 받은 CC0 source와 URL·license page·SHA-256 ledger를 사용했다. 단, 일부 Freesound source byte는 로그인 없이 받은 공개 HQ preview MP3 derivative다. 이는 방향 청취용 `REVIEW`에는 사용하지만, `FINAL` 전에는 원본 lossless로 교체하고 재검증해야 한다. Human Owner가 preview-derived source를 별도로 승인하지 않는 한 production source로 간주하지 마라.
- 프로젝트 baseline: `b15d971375cfb3a0be7d978f9311b58702e9d431`
- 격리 branch: `codex/showcase-audio-rebuild-v003`
- 상태: `REVIEW_ONLY_NOT_FINAL`
- 현재 제품 authority는 S4와 Audio Runtime integration을 승인하지 않았고 main product gate도 닫혀 있다.

## 6. 블라인드 원칙

- `PRIVATE_BLIND_MAPPING.json`, A/B/C 이름이 보이는 named candidate, 제작 스크립트의 mapping을 절대 열거나 추론하지 마라.
- 오직 첨부된 X/Y/Z 영상과 필요 시 X/Y/Z WAV만 비교하라.
- media가 첨부되지 않았거나 실제로 재생·청취할 수 없다면 그것을 첫 줄에 명시하고, 음질을 들은 척하지 말고 설계/권한 문서만 검토하라.
- 당신의 결과는 전문 추천이며 Human Owner의 최종 청취 승인이 아니다.

## 7. 평가 기준

다음을 각각 판단하라.

1. visible contact와 transient가 정확히 일치하는가
2. 평범한 punch가 아니라 T3 최고점으로 느껴지는가
3. 작은 스피커에서도 attack이 남되 click이 아프거나 싸구려처럼 들리지 않는가
4. Chair metal/plastic과 Runner body/cloth가 구분되는가
5. `Chair → Runner → floor/caster` 인과 순서가 들리는가
6. tail이 boom으로 뭉개지지 않고 command/status cue 공간을 남기는가
7. variation 1·2가 같은 family면서 복제품처럼 들리지 않는가
8. 비살상이고 현실적인 사무실 물성을 유지하는가
9. 반복 시 피로, harshness, flabby low end, phase pull, smeared center가 없는가
10. 이 North-Star가 이후 T1/T2를 아래에 배치할 충분한 상단 headroom을 남기는가

## 8. 반드시 지킬 응답 형식

첫 줄을 다음 중 하나로 시작하라.

- media 청취 가능: `BLIND AUDIO RECOMMENDATION — X`
- media 청취 가능: `BLIND AUDIO RECOMMENDATION — Y`
- media 청취 가능: `BLIND AUDIO RECOMMENDATION — Z`
- media 청취 가능하지만 전부 미달: `BLIND AUDIO RECOMMENDATION — REJECT ALL`
- media 청취 불가: `AUDIO NOT ASSESSED — MEDIA UNAVAILABLE`

`AUDIO NOT ASSESSED — MEDIA UNAVAILABLE`인 경우에는 아래 전체 비교 형식을 억지로 채우지 마라. 대신 다음 네 항목만 답하라.

1. `Spec/authority review`: 문서만으로 확인 가능한 방향·범위의 명확성
2. `Missing inputs`: 실제 판단에 필요한 X/Y/Z media
3. `Production blockers`: 청취 승인과 source 교체 등 이미 확인 가능한 차단 항목
4. `Authority check`: 청취 추천·Runtime·S4·main gate·FINAL 승인이 아님을 명시

media를 실제로 청취한 경우에만 아래 순서로 답하라.

1. `Executive judgment`: 선택 또는 전부 기각 이유를 5문장 이내로 요약
2. `Blind ranking`: X/Y/Z의 순위와 각 후보의 강점·치명적 약점
3. `Criterion matrix`: 위 10개 기준을 X/Y/Z 각각 `PASS / BORDERLINE / FAIL`로 표기
4. `Exact revision`: 선택 후보도 고칠 점이 있으면 다섯 stem 단위로 최대 5개, `REJECT ALL`이면 새 목표값까지 제시
5. `Laptop risk vs headphone risk`: 작은 스피커와 헤드폰에서 따로 생길 위험
6. `Production blockers`: HQ preview source 교체를 포함해 `FINAL` 전 필수 차단 항목
7. `Next gate`: `Human Owner가 X/Y/Z 또는 REJECT ALL을 기록하기 전에는 다른 P0 family·Runtime·Mixer·BGM으로 확장하지 않는다`가 맞는지 확인
8. `Authority check`: 이번 추천이 North-Star sonic direction만 다루며 Runtime 통합, S4, main gate, FINAL 승인이 아님을 한 문장으로 명시

모호한 평균론은 피하라. 특히 “더 펀치감 있게” 같은 표현만 쓰지 말고, 어떤 stem의 어느 시간대·주파수 인상·레벨·길이·공간감을 바꿀지 결정 가능한 언어로 써라.

---

## 첨부 권장 파일

- `BLIND_X_AUDITION_2VAR_REVIEW.mp4`
- `BLIND_Y_AUDITION_2VAR_REVIEW.mp4`
- `BLIND_Z_AUDITION_2VAR_REVIEW.mp4`
- 필요 시 동일 이름의 `.wav` 세 파일

절대 첨부하지 않을 파일: `PRIVATE_BLIND_MAPPING.json`, A/B/C named candidate.
