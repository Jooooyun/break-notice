# BREAK NOTICE Contest Showcase Audio Direction v001

- 상태: `PROPOSED / REVIEW REQUIRED / SOURCE-DESIGN-LICENSE PREPARATION ONLY`
- Ticket: `SHOWCASE-AUDIO-SOURCE-001`
- Execution Relay: `EAR-CONTEST-0002-AUDIO-SOURCE-001`
- Branch: `codex/showcase-audio`
- Immutable Base: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- 적용 범위: `CONTEST-0002 / PRT-RM-002 — Operations Floor Handover Lockdown`
- Runtime Integration: `NOT AUTHORIZED / S9-BOUND`
- Audio Binary: `0 / NOT DELIVERED`

## 1. 목적과 권위 경계

이 문서는 잠긴 Showcase Gameplay를 새로 해석하거나 확장하지 않고, 이미
승인된 이벤트가 실제 물리 인과와 팀 협동으로 읽히도록 하는 오디오 제작
방향을 정의한다. 다음 권위를 보존한다.

- `CONTEST-0001` C0–C4 accepted foundation과 기존 Graybox는 변경하지 않는다.
- `CONTEST-0002`의 48 m × 34 m Operations Floor, Binder 5, Chair 3,
  Drawer 1, Copier 1, 동일 Runner 2명, mobile CHR_01 한 명을 그대로 따른다.
- Q 우선순위, PASS/BRACE/BREAK, TEAM READY, 네 Drawer 해결법과
  `FIN-P-001 — HANDOVER CRASH`를 그대로 따른다.
- 새로운 Gameplay, Room, Enemy, Object, Finisher, 피해, 파괴 또는 대사를
  만들지 않는다.

이 문서의 cue 이름은 제작 식별자다. 확정된 Runtime event나 API 이름이
아니다. 모든 `Future Runtime Hook`과 세부 timing은
`PROPOSED / REVIEW REQUIRED`다.

## 2. 핵심 음향 문장

`Grounded world. Absurd physics.`

재미는 만화식 효과음이나 과장된 캐릭터 음성이 아니라, 평범한 사무용
물체가 예상보다 큰 운동량과 결과를 만드는 물리적 소리에서 나온다.

1. 공간은 현실적인 야간 Operations Floor로 들린다.
2. 플레이어가 무엇을 잡고, 얼마나 충전하고, 언제 놓았는지 명확히 들린다.
3. 동료 행동은 UI 알림보다 먼저 물리적 접근·그립·Release로 읽힌다.
4. PASS/BRACE/BREAK는 서로 다른 재질 모티프로 구분하되 Gameplay 정보를
   오디오에만 의존하지 않는다.
5. 큰 충돌은 저역과 잔향보다 접촉 순서와 재질 구분을 우선한다.
6. 공포, 군사, 초능력, 네온 에너지, 슬랩스틱 cartoon 방향은 사용하지 않는다.

## 3. 음향 팔레트

### 3.1 Operations Floor ambience

기본 ambience는 다음 세 층으로 구성한다.

- 넓고 안정적인 HVAC/room-tone stereo bed.
- 실제 fluorescent fixture의 낮고 얇은 electrical hum.
- 먼 곳의 제한된 alarm/시설 기계음. 반복 주기를 쉽게 인지할 수 있는
  짧은 loop는 피한다.

중앙 Bowl은 가장 비워 물리 cue가 읽히게 하고, West Loop는 종이·수납·바퀴
질감을, East Lane은 금속 프레임·caster·Strap 기계음을 조금 더 드러낸다.
Z6 Drawer Gate에서는 alarm 존재감을 유지하되 충돌과 Q prompt를 가리지
않는다. Drawer 해결 후 alarm이 약해지고 Exit-light cue가 전면으로 나온다.

음악은 현재 잠긴 Gameplay 이벤트가 아니므로 이 작업에서 제안하거나
소싱하지 않는다.

### 3.2 Player movement와 물리 상호작용

- Footstep은 안전화와 평평한 사무실 바닥의 짧고 건조한 접촉으로 만든다.
- Binder는 판지/종이 shell, 내부 종이 flutter와 모서리 접촉을 분리한다.
- Chair는 caster roll, 프레임 rattle, skid와 금속/플라스틱 impact를 분리한다.
- Carry 상태는 과도한 loop 대신 방향 전환·정지 때의 작은 settle만 사용한다.
- 물체가 손에 닿기 전 pickup sound를 재생하지 않는다.
- Release 이전에 impact나 airborne cue가 시작되지 않는다.

### 3.3 LMB Throw Charge

수락된 기반의 full charge는 1.2초지만, Audio hook과 내부 분할 시점은 아직
확정되지 않았다. 다음 3단 구조만 제안한다.

1. `Charge Start`: LMB press를 확인하는 짧은 tactile cue.
2. `Charge Build`: hold 동안 물체 재질과 신체 하중이 점진적으로 증가하는
   비선율적 layer. 짧은 click에서는 들리지 않거나 즉시 종료한다.
3. `Charge Full`: full charge 도달을 한 번만 알리는 작은 lock cue.

취소·pointer loss·상태 rejection은 Release와 혼동되지 않는 짧은 감쇠로
끝낸다. Charge 단계에 전자 에너지 상승음이나 마법성 pitch sweep을 쓰지
않는다. exact threshold, pitch curve, loop start/end와 release-frame 연동은
모두 `PROPOSED / REVIEW REQUIRED`다.

### 3.4 Ally READY, Q Supply와 공급 거절

- Ally READY는 동료 위치의 작은 작업복/장갑/물체 settle과 절제된 UI tick을
  조합한다. 새 voice line을 만들지 않는다.
- Q Supply 수락은 UI confirm보다 동료의 실제 grip과 이동·Release가 중심이다.
- Binder supply는 hand release → airborne paper/edge → Catch 또는 Miss의
  순서를 보존한다.
- Chair feed는 two-hand push → grounded caster movement → visible release →
  floor-supported roll 순서를 보존한다. 공중 Throw처럼 들리면 실패다.
- Candidate 없음과 path/supply-lane 차단은 같은 rejection family를 쓰되,
  path 차단은 더 둔한 world cue를 겹쳐 원인을 구분한다.
- Q prompt가 바뀔 때마다 소리를 내지 않는다. valid action 상태 진입 또는
  player input 결과에만 제한한다.

### 3.5 PASS / BRACE / BREAK / TEAM READY

각 badge는 기존 물리 재질을 축약한 motif로 구성한다.

- PASS: Binder Catch의 종이 snap + 짧은 secure click.
- BRACE: Carabiner/Clamp의 금속 lock + 낮은 tension settle.
- BREAK: Grab Rescue 또는 Combat Tag의 짧은 body-contact thump + release breath.
- TEAM READY: 획득된 두 motif를 겹치고 세 번째 중립 pulse로 닫는다.

badge cue는 실제 성공 확정 뒤 한 번만 재생한다. 실패·예고·prompt 표시에서
미리 재생하지 않는다. badge는 decay, spend 또는 run persistence를 암시하지
않는다.

### 3.6 Runner pressure

Runner는 군인이나 괴물이 아니라 평범한 회사 인력이 거칠게 밀어붙이는
존재로 들려야 한다.

- 접근 압박은 신발 cadence, 작업복과 호흡의 제한된 layer로 만든다.
- Body Check는 발 디딤과 어깨/상체 weight shift를 telegraph로 사용한다.
- Shove, short Grab, Stagger와 Rescue는 접촉과 해제 순서를 명확히 한다.
- 동일 Runner 두 명은 새 archetype 없이 variation과 공간 위치로만 구분한다.
- 항상 한 공격 token이라는 Gameplay 제약을 오디오 polyphony에도 반영하여
  동시에 두 개의 공격 telegraph가 전면으로 나오지 않게 한다.

### 3.7 Drawer, route state와 HANDOVER CRASH

Drawer는 얇은 filing cabinet이 아니라 route를 막는 무거운 rolling asset으로
들려야 한다. impact, caster/foot scrape, 지속 이동, Parking Bay settle을
분리한다. Route Open cue는 충돌 자체가 아니라 권위 있는 room-state 변화가
확정된 뒤 재생한다.

`HANDOVER CRASH`는 새 결과를 만들지 않고 잠긴 Chair → Runner → Drawer 인과를
다음 순서로 읽힌다.

1. commit의 짧은 UI/world lock.
2. Ally setup과 Chair line 정렬.
3. Chair release/skid.
4. Chair → Runner body contact.
5. Runner → Drawer heavy impact와 Drawer movement.
6. paper aftermath, Exit-light activation, control return.

각 단계의 exact frame과 약 2.5초 sequence 내부 timing은
`PROPOSED / REVIEW REQUIRED`다. 한 개의 과도한 cinematic boom으로 접촉
순서를 덮지 않는다.

### 3.8 Reset, Exit, clear와 UI

- Reset 입력 확인은 즉시 읽히는 2D cue를 사용한다.
- Reset barrier 동안 진행 중 charge, loop, supply, tension과 impact tail을
  정리하고 ambience를 authored baseline으로 복구한다.
- Reset에 마법성 rewind sweep을 사용하지 않는다.
- Drawer 해결 후 Exit activation은 현실적인 전기 relay/fixture cue와 절제된
  UI confirmation을 조합한다.
- Exit crossing과 result card는 짧고 명확하게 닫으며 다음 wave나 CONTINUE를
  암시하지 않는다.
- UI confirm과 reject는 각각 하나의 family로 통일하고, Q context label마다
  별도 theme를 만들지 않는다.

## 4. 공간화 원칙

| 범주 | 기본 방식 | 원칙 |
|---|---|---|
| UI confirm/reject, badge, result | 2D | 짧고 중앙에 고정하되 world causality를 대체하지 않음 |
| Player body/gear | 3D near-field 또는 player-relative | 카메라 회전에 과도하게 흔들리지 않음 |
| Ally, Runner, objects, Drawer | 3D | 실제 발생 위치와 거리·차폐를 보존 |
| Operations Floor bed | Stereo 2D/area bed | seam 없는 loop, 방향성을 과장하지 않음 |
| Local fluorescent/mechanical layer | 3D loop | fixture/zone 위치에서만 들리며 gameplay cue를 가리지 않음 |

정확한 min/max distance, spatial blend, rolloff, occlusion과 reverb 값은
Runtime 구현 권한이 없으므로 `PROPOSED / REVIEW REQUIRED`다.

## 5. Priority, silence와 ducking

| Priority | 목적 | 대표 cue | 처리 원칙 |
|---|---|---|---|
| P0 CRITICAL | Reset/clear와 authoritative state 전환 | Reset, Route Open, Exit/Clear | 다른 non-critical one-shot보다 우선하며 중복 금지 |
| P1 ACTION | player input 결과와 협동 성공 | Release, Catch, Q accept/reject, badge, TEAM READY | P2–P4를 짧게 duck |
| P2 PHYSICS | 실제 물리 인과 | Binder/Chair/Runner/Drawer impact | 가까운 큰 impact만 우선; 동일 frame stack 제한 |
| P3 MOTION | 이동과 telegraph | Footstep, caster loop, Runner approach | P0–P2에서 자연스럽게 뒤로 물러남 |
| P4 BED | 공간 연속성 | HVAC, fluorescent, distant alarm | 항상 가장 먼저 duck되고 Reset 후 baseline 복귀 |

제안 ducking 규칙은 다음과 같으며 모두 `PROPOSED / REVIEW REQUIRED`다.

- P0 재생 중 P4를 약 6–9 dB, P3를 약 3–6 dB 낮춘다.
- P1 Catch/Release/Badge 중 P4를 약 3–6 dB 짧게 낮춘다.
- HANDOVER CRASH의 핵심 impact 직전 매우 짧은 micro-gap을 둘 수 있으나
  정확한 길이는 Human playtest로 정한다.
- Reset barrier 진입 시 charge/tension/caster/grab loop는 즉시 종료하고,
  stale tail이 baseline 뒤에 남지 않게 한다.
- 동일 cue의 반복 입력은 retrigger window와 voice limit가 필요하나 정확한
  수치와 stealing 정책은 Runtime review에서 정한다.
- critical gameplay 정보는 silence나 ducking만으로 전달하지 않는다.

## 6. Source와 license 방향

- 우선순위 1: Human Owner 또는 승인된 제작자가 직접 녹음한 original source.
- 우선순위 2: 개별 asset page와 license evidence가 고정된 CC0 source.
- 우선순위 3: attribution과 변경 고지를 완전히 이행할 수 있는 CC BY 4.0.
- 별도 상업 EULA pack은 open-source로 부르지 않으며, 계약 증거와 사용 범위가
  검토된 경우에만 후보로 유지한다.
- CC BY-NC, Sampling+, editorial-only, ripped game/film audio, 출처 불명
  recording, 유명 IP/음악/음성, 라이선스가 바뀌었거나 상충하는 page는 제외한다.

현재 발견한 source는 모두 `CANDIDATE / NOT DOWNLOADED / NOT APPROVED`다.
정확한 후보와 증거 요구사항은 `SHOWCASE_AUDIO_LICENSE_PLAN_v001.md`를 따른다.

## 7. 기술 납품 제안

- Runtime 후보 format: WAV, 48 kHz, 24-bit preferred 또는 16-bit accepted.
- 3D one-shot/loop는 기본 mono source를 우선하고, ambience와 의도된 넓은
  결과 cue만 stereo를 사용한다.
- Source 원본을 보존하고 trim, denoise, EQ, layering, resample, normalization
  이력을 별도 manifest에 기록한다.
- clipping, DC offset, broken loop seam, phase cancellation과 과도한 noise
  reduction artifact를 허용하지 않는다.
- exact peak/LUFS target, compression setting, import compression과 mixer routing은
  `PROPOSED / REVIEW REQUIRED`다.

## 8. 승인 전 체크

- Cue Matrix의 locked event와 실제 Gameplay Lock이 모순되지 않는다.
- 실제 trigger 이름·timing을 승인된 것처럼 쓰지 않는다.
- Binder, Chair, Drawer와 Strap의 재질·접촉 순서가 구분된다.
- Ally cue는 physical action을 대체하거나 teleport delivery를 암시하지 않는다.
- PASS/BRACE/BREAK는 성공 후에만 울리고 TEAM READY는 두 distinct badge 뒤에만
  울린다.
- Finisher가 없어도 Direct Chair와 Strap 경로 오디오가 완결된다.
- Reset 후 stale loop가 0이고 ambience baseline이 복구된다.
- 오디오 없이도 모든 중요한 상태가 시각적으로 읽힌다.
- 각 실제 source에 provenance, license snapshot, URL, creator, download date,
  original filename, byte size와 SHA-256이 존재한다.

## 9. 비권한 선언

이 문서는 AudioSource, AudioMixer, Runtime event, code, Scene, Prefab, Import
Settings, `.meta`, Test, Build 또는 Browser validation을 만들지 않는다.
실제 WAV/OGG와 license evidence file은 추가되지 않았다. 실제 source 취득과
S9 Runtime integration에는 각각 새 Human Owner Relay가 필요하다.
