# BREAK NOTICE Showcase VFX Delivery Checklist v001

- Status: `REVIEW / DOCUMENTATION PACKAGE PREPARED / SOURCE DELIVERY MISSING`
- Ticket: `SHOWCASE-VFX-SOURCE-001`
- Relay: `EAR-CONTEST-0002-VFX-SOURCE-001`
- Baseline: `f51315aea91dd00293a4f5c387f1cb748865fd11`
- Runtime Integration: `NOT AUTHORIZED`

## 상태 표기

- `[x]`: 이번 documentation-only Relay에서 확인 완료
- `[ ]`: 후속 Source/Preview/Runtime Relay가 필요한 항목
- `MISSING`: 파일 또는 권리 증거가 아직 없음
- `REVIEW REQUIRED`: Human Owner 또는 Technical Review 전 확정 불가

## 1. Authority와 scope

- [x] `AGENTS.md` 전체 재독 및 SHA-256 일치
- [x] 최초 MASTER EXECUTION PROMPT 전체 재독 및 SHA-256 일치
- [x] 지정된 live-state, CR, ADR, ticket, Gameplay Lock, Asset Matrix와 Owner
  directive 전체 재독
- [x] `CONTEST-0001` C0–C4 accepted foundation 보존
- [x] `CONTEST-0002 / PRT-RM-002`에만 cue 적용
- [x] S1, C5, S9와 Main Product Gate는 닫힌 상태로 기록
- [x] `Game/**`, Package, ProjectSettings, Scene, Prefab, Runtime code 변경 없음
- [x] Texture, Flipbook, Binary, `.meta` 추가 없음
- [x] 파괴, runtime fracture, gameplay debris, 새 Damage/Break mechanic 없음
- [x] 마법, 초능력, 과도한 neon, 폭발 중심, screen-wide effect 없음

## 2. Cue coverage

- [x] Binder/Chair 획득 가능 상태
- [x] Throw Charge 단계
- [x] Throw Release와 Binder/Chair 물리 contact
- [x] Ally READY 느낌표 지원
- [x] Q Binder/Chair Supply
- [x] Binder Catch
- [x] Chair floor-supported feed와 wheel contact
- [x] supply rejection과 path rejection
- [x] PASS / BRACE / BREAK
- [x] TEAM READY
- [x] HANDOVER CRASH의 Chair→Runner→Drawer 인과
- [x] Drawer causal impact와 route-state 변화
- [x] Runner pressure/telegraph/contact/stagger readability
- [x] Reset start/complete
- [x] Exit activation
- [x] UI confirmation/rejection
- [x] Cue ID uniqueness validation 대상 정의

## 3. Source 후보 파일

현재 아래 파일은 모두 `MISSING`이며 이번 Relay에서 만들지 않는다.

### Editable Source

- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_IndustrialCueShapes_v001.svg`
- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_PaperBits_v001.svg`
- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_ContactDust_v001.blend`
- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_WheelContact_v001.blend`
- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_ImpactMarks_v001.svg`
- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_ExitLightMasks_v001.svg`
- [ ] `ArtSource/VFX/Showcase/Source/VFX_Showcase_UIConfirmReject_v001.svg`

### Texture / Flipbook

- [ ] `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_IndustrialCueShapes_Atlas_v001.png`
- [ ] `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_PaperBits_Atlas_v001.png`
- [ ] `ArtSource/VFX/Showcase/Flipbooks/T_VFX_Showcase_ContactDust_4x4_v001.png`
- [ ] `ArtSource/VFX/Showcase/Flipbooks/T_VFX_Showcase_WheelContact_4x4_v001.png`
- [ ] `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_ImpactMarks_Atlas_v001.png`
- [ ] `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_ExitLightMasks_v001.png`
- [ ] `ArtSource/VFX/Showcase/Textures/T_VFX_Showcase_UIConfirmReject_Atlas_v001.png`

### Preview

- [ ] `ArtSource/VFX/Showcase/Preview/VFX_Showcase_CueBoard_v001.png`
- [ ] `ArtSource/VFX/Showcase/Preview/VFX_Showcase_AccessibilityGrayscale_v001.png`
- [ ] `ArtSource/VFX/Showcase/Preview/VFX_Showcase_GameplayCameraReadability_v001.mp4`

### License / provenance

- [ ] `ArtSource/VFX/Showcase/Licenses/VFX_Showcase_Source_License_Ledger_v001.csv`
- [ ] `ArtSource/VFX/Showcase/Licenses/VFX_Showcase_Original_Work_Declaration_v001.md`
- [ ] `ArtSource/VFX/Showcase/Licenses/VFX_Showcase_External_Source_Evidence_v001.md`

## 4. Source 제작 검사

- [ ] 파일명에 공백·한글·`final`·`new`·unversioned suffix가 없음
- [ ] editable source와 export가 1:1로 추적됨
- [ ] texture/flipbook frame order와 alpha edge가 문서화됨
- [ ] premultiplied/straight alpha 후보가 실제 Preview로 비교됨
- [ ] additive bloom 없이 office-light 배경에서 읽힘
- [ ] paper/dust/wheel silhouette가 서로 구분됨
- [ ] Binder paper가 gameplay projectile/debris로 오인되지 않음
- [ ] Chair wheel cue가 floor support를 명확히 유지함
- [ ] contact-normal 방향과 effect fan 방향이 일치함
- [ ] Runner contact cue가 actor silhouette를 가리지 않음
- [ ] Drawer cue가 breakage나 fracture로 보이지 않음
- [ ] Exit cue가 route-open Fact보다 먼저 나오지 않음

## 5. 접근성·가독성 Preview

- [ ] grayscale에서 eligibility, confirm, reject, charge band가 구분됨
- [ ] Light는 1 tick, Medium은 2 tick으로 class가 중복 전달됨
- [ ] PASS/BRACE/BREAK가 서로 다른 pictogram으로 구분됨
- [ ] TEAM READY가 두 badge 결합 형태로 읽힘
- [ ] rejection은 X/끊어진 bracket/lateral motion과 원인 text를 사용함
- [ ] 1366×768 gameplay-camera frame에서 contact 지점이 보임
- [ ] 1920×1080 gameplay-camera frame에서 과도하게 작아지지 않음
- [ ] color-vision simulation review 완료
- [ ] rapid flash 또는 photosensitivity risk 없음
- [ ] 목표, reticle, Q prompt, Exit lane을 가리는 frame 없음

## 6. License와 외부 Source

- [ ] creator와 canonical source URL 기록
- [ ] download date와 original SHA-256 기록
- [ ] exact license text의 local evidence 보존
- [ ] commercial use 허용 확인
- [ ] derivative work 허용 확인
- [ ] attribution 의무 확인
- [ ] raw source redistribution 허용 여부 확인
- [ ] contest build·영상·thumbnail 사용 허용 확인
- [ ] third-party IP 또는 editorial-only 제한 없음
- [ ] Human Owner rights review 완료

`NC`, `ND`, editorial-only, 출처 불명, license text 누락 또는 raw-source
재배포 조건이 delivery 방식과 충돌하는 경우:

`BLOCKED — PROVENANCE OR LICENSE INCOMPLETE`

## 7. Future Runtime Technical Review

- [ ] 실제 existing Runtime signal 감사
- [ ] cue hook 이름과 payload 승인
- [ ] Presentation-only dependency 확인
- [ ] Reset generation/stale cue guard 확인
- [ ] pooling/allocation 정책 승인
- [ ] texture import, compression, mipmap, color-space 승인
- [ ] depth/occlusion behavior 검증
- [ ] effect concurrency와 alpha overdraw 측정
- [ ] no gameplay collision / no causal-carrier 검증
- [ ] no VFX Graph / Shader Graph / custom package drift 검증
- [ ] Unity compile, automated tests, Web build와 browser smoke

위 Runtime 항목은 현재 실행 대상이 아니다.

## 8. Documentation package validation

- [x] exact allowlist 밖 파일을 만들지 않는 계획
- [x] Cue Matrix 필수 열 15개 포함
- [x] 모든 미확정 hook/timing에 `PROPOSED / REVIEW REQUIRED` 요구
- [x] 모든 실제 Source/License 상태를 `MISSING`으로 기록
- [x] 후보 파일명을 exact path로 기록
- [x] Graybox fallback 유지
- [ ] CSV parse와 duplicate-ID validation 실행
- [ ] UTF-8 validation 실행
- [ ] `git diff --check` 실행
- [ ] exact allowlist stage/commit/push

마지막 네 항목은 commit 전 자동 validation에서 수행하고 결과는 최종 Work
Package Report에 기록한다. 이 파일 자체를 후속 commit으로 수정해 완료 표기를
남기지는 않는다.

## 9. 후속 Relay 요청

제안 ID: `EAR-CONTEST-0002-VFX-SOURCE-002` (`PROPOSED ONLY / NOT ISSUED`)

요청 범위:

- 위 exact Source/Texture/Flipbook/Preview/License 후보 파일 생성
- gameplay-camera와 grayscale Preview 제작
- 외부 Source 사용 시 exact provenance/license 증거 등록
- Unity, `.meta`, Runtime, Scene, Prefab, Shader Graph, VFX Graph, build는 제외

Source/Preview를 Human Owner가 통과시킨 뒤에도 S9 Runtime Integration에는 별도
one-shot Relay가 필요하다.
