# VFX V4 Action FBX + Trajectory Map

## 결론

`Jab / Cross / Kick / Body Check / Strap Sweep L / Strap Sweep R / Strap Slam / Knockdown Reaction` 8종을 실제 CC0 FBX clip에서 분리했습니다. 각 FBX에는 retarget용 skeleton/mesh/animation과 역할별 `FX_TrailSocket / FX_ContactSocket / FX_ImpactSocket`이 들어 있습니다.

FBX 자체에는 Unity `TrailRenderer`를 넣을 수 없습니다. 실제 궤적과 hit는 표의 companion prefab을 socket/contact event에 붙입니다. Comparison preview의 `TPS Rear 3/4`는 공격 7종의 연출용 2중 암시선, `Side Debug`는 실제 socket tracking입니다. `Knockdown Reaction`은 피격 반응이므로 공격선을 표시하지 않습니다.

| Action | FBX | Reference clip | Preview / contact | Trajectory | Companion VFX | 적용 메모 |
|---|---|---|---|---|---|---|
| Jab | `BN_Action_Jab.fbx` | Quaternius / `Punch_Jab` | 0.867s / F8 (0.233s) | `FX_TrailSocket` | `VFX_V4_Jab` | 살릴 Jab hit를 접점에 합성 |
| Cross | `BN_Action_Cross.fbx` | Quaternius / `Punch_Cross` | 1.000s / F15 (0.467s) | `FX_TrailSocket` | `VFX_V4_Cross` | 살릴 Cross hit를 접점에 합성 |
| Kick | `BN_Action_Kick.fbx` | Kay Lousberg / `Melee_Unarmed_Attack_Kick` | 0.933s / F14 (0.433s) | `FX_TrailSocket` | `VFX_V4_Kick` | 살릴 Kick hit를 접점에 합성 |
| Body Check | `BN_Action_BodyCheck.fbx` | Quaternius / `Shield_Dash` | 1.100s / F17 (0.533s) | `FX_ContactSocket` | `VFX_V4_BodyCheck` | 살릴 BodyCheck ring을 몸통 접점에 합성 |
| Strap Sweep L | `BN_Action_Strap_Sweep_L.fbx` | Quaternius / `Sword_Regular_A + Sword_Regular_A_Rec` | 1.433s / F8 (0.233s) | `FX_TrailSocket` | `VFX_V4_Kick` | Kick streak를 좌측 sweep 궤적으로 재사용 |
| Strap Sweep R | `BN_Action_Strap_Sweep_R.fbx` | Quaternius / `Sword_Regular_B + Sword_Regular_B_Rec` | 1.600s / F8 (0.233s) | `FX_TrailSocket` | `VFX_V4_Kick` | Kick streak를 우측 sweep 궤적으로 재사용 |
| Strap Slam | `BN_Action_Strap_Slam.fbx` | Quaternius / `OverhandThrow` | 1.067s / F19 (0.600s) | `FX_TrailSocket` | `VFX_V4_Knockdown` | 뒤로 당긴 strap을 머리 위로 넘겨 전방 하강 slam; Knockdown burst를 접점에 재사용 |
| Knockdown Reaction | `BN_Action_Knockdown_Reaction.fbx` | Quaternius / `Death01` | 2.400s / F8 (0.233s) | `FX_ImpactSocket` | `VFX_V4_Knockdown` | 살릴 Knockdown hit를 피격 시작점에 합성 |

## Unity 적용

1. 포함된 `VFXV4ActionImporter.cs`가 FBX를 `Humanoid / non-loop / in-place / Compression Off`로 자동 설정하고 Avatar/clip 유효성을 검사합니다.
2. 게임 캐릭터의 `Animator`에서 해당 clip을 retarget합니다.
3. 공격 event에 표의 `VFX_V4_*` prefab을 spawn합니다.
4. 긴 궤적은 `FX_TrailSocket`, 몸통 충돌은 `FX_ContactSocket`, 피격점은 `FX_ImpactSocket`에 연결합니다.
5. `Knockdown_Reaction`은 공격 animation이 아니라 피격자 reaction입니다.
6. 모든 clip은 `in-place`입니다. 실제 이동은 gameplay controller가 담당합니다.

## 원본 및 라이선스

모두 CC0입니다. 원본 archive는 `00_Source_Packages/AnimationReferences`, license는 `Assets/VFX_V4/Licenses/AnimationReferences`와 `03_Documentation`에 보존했습니다.

- Quaternius — [Universal Animation Library 1 Standard](https://quaternius.com/packs/universalanimationlibrary.html), `CC0-1.0`, `Universal Animation Library[Standard].zip`, SHA-256 `CC73FC4E495B82958207316596317A3F40B9FA38065BDE1027937452DA537724`
- Quaternius — [Universal Animation Library 2 Standard](https://quaternius.com/packs/universalanimationlibrary2.html), `CC0-1.0`, `Universal Animation Library 2[Standard].zip`, SHA-256 `4008EA208A604773A2B2177D965F0F5D3195498B5BF838C3F5785D68E95F2A68`
- Kay Lousberg — [KayKit Character Animations Free 1.1](https://kaylousberg.com/game-assets/character-animations), `CC0-1.0`, `KayKit_Character_Animations_Free_1.1.zip`, SHA-256 `65882F31F905AD2E953819648A59287CDEAB8F623908D5EF701971D3758BE20F`

## 한계

게임 humanoid rig 원본을 받지 못했으므로 본 FBX는 최종 game-rig bake가 아니라 retargetable reference입니다. 손/발 접점과 strap 길이는 실제 캐릭터 비율에 맞춰 Unity에서 마지막 조정이 필요합니다.
