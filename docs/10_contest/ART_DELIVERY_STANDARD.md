# Contest Art Delivery Standard

## Authority

모든 delivery는 다음을 먼저 따른다.

- `docs/03_art/ART_DIRECTION.md`
- `docs/03_art/OFFICE_CORE_KIT.md`
- 관련 Visual Spec과 Accepted ADR
- `docs/10_contest/VISUAL_REFERENCE_INVENTORY.md`
- tracked `references/approved/` reference

Art direction은 `Grounded Corporate Action Realism`과
`Grounded world. Absurd physics.`다. anime/cel shading, toy material,
과장 얼굴, superhero/military-SF, firearm, magic/telekinesis, neon energy
cable과 necktie-to-strap 연결을 금지한다.

## Source and Runtime Separation

- 편집 원본: `ArtSource/**`
- 기술 검수된 export만: `Game/Assets/_Project/Art/**` 또는
  `Game/Assets/_Project/Audio/**`
- 승인 reference 이미지를 runtime texture/background로 무단 사용하지 않는다.
- Unity가 만든 `.meta`는 대응 runtime asset과 함께 보존한다.

## Naming

- 영문/숫자/underscore, 공백·한글 금지
- 명시적 Asset ID와 `_v001` 형식의 version suffix
- `final`, `final2`, `real_final`, `new`, `수정본` 금지
- 예: `CHR00_Player_v001.fbx`, `PRP_OfficeChair_A_v001.fbx`

## Coordinate, Transform and Pivot

- Unity `1 unit = 1 meter`, Y-Up, forward `+Z`
- transform applied, scale `1,1,1`, rotation `0,0,0`, negative scale 금지
- Character pivot: 양발 사이 지면
- Binder: object center
- Chair: wheel-base 중심 지면
- Drawer/Copier: ground center
- Door: hinge
- Wall/Partition: lower module origin
- GEAR01: actual attachment position; clamp/carabiner connection axis 명시

## Geometry

깨진 normal, 겹친 면, 불필요한 내부 면과 modifier, 불필요한 N-gon을
정리한다. Polygon count와 material slot을 DELIVERY.md에 기록한다. 고정
decorative asset에 gameplay Rigidbody/interaction을 부여하지 않는다.

## Texture and Material

- Character/Hero Equipment: 기본 최대 `2K`
- Gameplay Prop: 기본 `1K`, 증명된 경우 `2K`
- Decorative: `512–1K`, 불필요한 `4K` 금지
- Base Color, Normal, Metallic/Smoothness 또는 Mask Map 중심
- URP/Lit, shared material/atlas 우선
- Simple Prop material slot `1`, Character `2–3` 우선
- 제한 없는 transparency/custom shader graph와 toy-like smoothness 금지

## Rig and Animation

Locomotion은 CharacterController가 소유하므로 별도 결정 전까지 in-place다.
승인 없는 root motion 전환을 금지한다.

### CHR00 Minimum

`Idle`, `Walk_F`, `Walk_B`, `Strafe_L`, `Strafe_R`, `Aim_Idle`,
`Interact`, `Carry_Light`, `Carry_Medium`, `Throw_Light`, `Throw_Medium`,
`Catch`, `Strap_Attach`, `Strap_Tension`, `Strap_Release`,
`Pressure_Reaction`, `Failure`.

### CHR01 Minimum

`Idle`, `Call_Response`, `Supply_Binder`, `Push_Chair`, `Reset`.

### ENY01 Minimum

`Idle`, `Move`, `Pressure`, `HitReaction`, `Stagger`, `Reset`.

clip name, start/end frame와 loop 여부를 명시한다. Jump, weapon, death,
finisher, second ally와 boss animation은 만들지 않는다.

## UI, VFX, Audio and Marketing

- UI source: PSD/AI/Figma 또는 편집 가능 원본; runtime PNG 우선
- SVG는 Unity import stability가 검증된 경우만; font license 필수
- VFX delivery: texture/flipbook/reference, color, blend, duration, loop,
  world/local space와 blocking feature 기록
- VFX는 material/impact/space-change를 표현하며 magic/neon energy 금지
- Audio: WAV, 48kHz, 16/24-bit 권장; mono/stereo, loop, trim, peak,
  creator/license와 usage 기록
- Thumbnail: 16:9; video: H.264 MP4, 3분 이내 실제 gameplay 중심

## DELIVERY.md Required Fields

모든 delivery root는 다음을 포함한다.

```text
Asset ID:
Asset Name:
Creator:
GitHub ID:
Contribution Date:
Work Classification: Original / Purchased / Open License / AI Generated / Mixed
Exact License:
Written Permission:
Reference Used:
Source Files:
Export Files:
Texture Files:
Animation Files:
Audio Files:
UI Files:
VFX Files:
Polygon Count:
Material Slots:
Texture Resolution:
Scale:
Pivot:
Forward Axis:
Rig Type:
Animation Type:
Loop Clips:
Known Issues:
Requested Integration:
Blocking Feature:
Suggested Runtime Path:
Preview Files:
```

Source만 있거나 export만 있는 delivery, 권리 정보 또는 DELIVERY.md가 없는
delivery는 완료가 아니다.

## Delivery State

`MISSING`, `BRIEFED`, `ASSIGNED`, `WORK IN PROGRESS`, `DELIVERED`,
`TECHNICAL REVIEW`, `REVISION REQUIRED`, `OWNER APPROVED`, `INTEGRATED`,
`CUT`만 사용한다. Codex는 `OWNER APPROVED`를 선언하지 않는다.

## PR and Integration

- 한 PR은 한 delivery batch만 포함한다.
- Art branch에서 contest integration branch로 PR한다.
- designer는 code, Packages, ProjectSettings, Input Actions, gameplay
  Scene/Prefab, tests와 build script를 수정하지 않는다.
- DELIVERY, preview, source, export, texture/animation, license와 known issue가
  있어야 technical review를 시작한다.
- Human Owner/Codex가 import, material, `.meta`, isolated art prefab,
  collider/rig/Animator, art scene/gameplay scene/tests/browser를 검증한다.

## Git LFS

현재 Git LFS `3.7.0`은 설치됐지만 `.gitattributes`에는 LFS filter가 없다.
기존 history는 재작성하지 않는다. 향후 큰 `.blend`, `.fbx`, `.psd`, `.tga`,
`.png`, `.wav`, `.mp4`, `.mov`, `.zip`은 delivery별 크기와 collaborator
support를 확인한 뒤 새 contest-period 파일에만 LFS를 적용한다.
