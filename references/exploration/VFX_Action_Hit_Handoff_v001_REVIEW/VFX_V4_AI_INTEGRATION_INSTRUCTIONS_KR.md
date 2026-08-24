# VFX V4 Client Integration Instructions

이 문서는 클라이언트 프로젝트를 수정하는 AI 또는 Unity 개발자가 `VFX_V4_ClientReady_Trail_Hit.unitypackage`를 안전하게 적용하기 위한 실행 지침입니다.

## 1. 파일 역할

- `.unitypackage`: 실제 적용 단위입니다. Action FBX, Trail prefab, Hit prefab, material, shader, runtime script, catalog를 함께 가져옵니다.
- `.prefab`: 게임에서 생성하거나 character socket에 부착하는 실제 VFX asset입니다.
- `.fbx`: 캐릭터 공격 및 피격 animation입니다. VFX renderer가 아닙니다.
- `.png`: prefab의 particle 또는 shader가 참조하는 texture입니다. PNG sequence를 직접 재생하지 않습니다.
- `.gif`와 `.mp4`: 검수용 preview입니다. 게임 runtime asset으로 사용하지 않습니다.

Unity 3D 게임에서 권장 구성은 다음과 같습니다.

1. 공격 animation은 FBX clip을 Humanoid retarget하여 재생합니다.
2. 공격 궤적은 `TrailRenderer` 두 개가 들어 있는 `VFX_V4_Trail_*` prefab을 socket에 부착합니다.
3. 실제 damage/contact 판정이 성공한 순간에만 `VFX_V4_Hit_*` prefab을 world contact point에 생성합니다.

## 2. 지원 환경

- 제작 및 검증: Unity `6000.3.20f1`
- Render Pipeline: URP `17.3.0`
- 모든 제공 animation: Humanoid, non-loop, in-place
- 모든 Hit particle: one-shot, non-loop

다른 Unity 또는 URP 버전에서는 별도 branch에서 import한 뒤 shader compile 상태를 먼저 확인하십시오. 기존 프로젝트의 `Packages/manifest.json`은 이 package가 변경하지 않습니다.

## 3. Import 순서

1. 현재 프로젝트를 commit하거나 backup합니다.
2. `VFX_V4_ClientReady_Trail_Hit.unitypackage`만 import합니다.
3. 이전 `VFX_V4_HighPixel_Mono_Hit_Motion.unitypackage`는 함께 import하지 않습니다. 이전 package는 구형 결합 prefab입니다.
4. Unity menu에서 `Tools > VFX V4 > Configure Client Action FBX`를 실행합니다.
5. Console에서 `VFX V4 CLIENT ACTION IMPORT PASS: configured=8`을 확인합니다.
6. `Assets/VFX_V4_Client/Config/VFX_V4_Client_Catalog.asset`을 확인합니다.

## 4. Runtime Module Interface

외부 combat code는 `CombatVfxRuntime`의 다음 세 method만 호출합니다.

```csharp
AttackVfxToken BeginAttack(AttackVfxId attack);
void ConfirmHit(AttackVfxToken token, Vector3 worldPoint, Vector3 surfaceNormal);
void EndAttack(AttackVfxToken token);
```

필수 호출 규칙:

1. 궤적 시작 frame에서 `BeginAttack`을 호출합니다.
2. 실제 damage/contact 판정 성공 시에만 `ConfirmHit`을 호출합니다.
3. 궤적 종료 frame, animation cancel, state interruption에서 `EndAttack`을 호출합니다.
4. 공격이 빗나가면 `ConfirmHit`을 호출하지 않습니다.
5. `EndAttack` 중복 호출은 허용됩니다.

## 5. Character 설정

1. 공격 character root에 `CombatVfxRuntime` component를 추가합니다.
2. `Catalog`에 `VFX_V4_Client_Catalog.asset`을 배정합니다.
3. `Animator`에 해당 character의 Humanoid Animator를 배정합니다.
4. 가능하면 `Socket Bindings`에 실제 effect anchor를 명시적으로 배정합니다.

Humanoid bone fallback:

| Action | 기본 socket | fallback bone |
|---|---|---|
| Jab | `FX_TrailSocket_Jab` | LeftHand |
| Cross | `FX_TrailSocket_Cross` | RightHand |
| Kick | `FX_TrailSocket_Kick` | RightFoot |
| Body Check | `FX_ContactSocket` | Chest |
| Strap Sweep L/R | `FX_StrapTip` | RightHand |
| Strap Slam | `FX_StrapTip` | RightHand |

Humanoid retarget는 source FBX의 custom socket object를 대상 character hierarchy로 자동 복사하지 않습니다. Strap이 이미 구현되어 있으면 기존 끈의 tip/end Transform을 `Socket Bindings > FX_StrapTip`에 명시적으로 배정합니다. tip anchor가 없을 때만 끈 끝에 새 Transform을 만듭니다. `RightHand` fallback은 임시 확인용이며 최종 연결로 사용하지 않습니다.

## 6. Combat 연결 예시

```csharp
using BreakNotice.VfxV4;
using UnityEngine;

public sealed class PlayerCombatVfxAdapter : MonoBehaviour
{
    [SerializeField] private CombatVfxRuntime combatVfx;
    private AttackVfxToken activeVfx;

    // Animation Event 또는 공격 state 진입 지점
    public void OnJabTrailBegin()
    {
        activeVfx = combatVfx.BeginAttack(AttackVfxId.Jab);
    }

    // 실제 damage 적용 성공 callback에서만 호출
    public void OnDamageConfirmed(Vector3 point, Vector3 normal)
    {
        combatVfx.ConfirmHit(activeVfx, point, normal);
    }

    // Animation Event, cancel, state exit 공통
    public void OnTrailEnd()
    {
        combatVfx.EndAttack(activeVfx);
    }
}
```

`ConfirmHit`을 예상 contact frame의 Animation Event에서 호출하지 마십시오. Animation Event는 공격이 명중했는지 알 수 없으므로 miss에도 Hit VFX가 출력될 수 있습니다.

## 7. Timing 표

모든 frame은 30 fps 기준입니다.

| Action | Trail Begin | Contact | Trail End | Hit prefab |
|---|---:|---:|---:|---|
| Jab | F3 | F8 | F10 | `VFX_V4_Hit_Jab` |
| Cross | F10 | F15 | F17 | `VFX_V4_Hit_Cross` |
| Kick | F8 | F14 | F17 | `VFX_V4_Hit_Kick` |
| Body Check | F10 | F17 | F19 | `VFX_V4_Hit_BodyCheck` |
| Strap Sweep L | F1 | F8 | F11 | `VFX_V4_Hit_StrapSweepL` |
| Strap Sweep R | F1 | F8 | F11 | `VFX_V4_Hit_StrapSweepR` |
| Strap Slam | F14 | F19 | F22 | `VFX_V4_Hit_StrapSlam` |

Animation speed가 변경되는 게임에서는 시간(초)을 hard-code하지 말고 clip의 Animation Event 또는 normalized time을 사용하십시오.

## 8. Asset 경로

- Trail: `Assets/VFX_V4_Client/Prefabs/Trails/`
- Hit: `Assets/VFX_V4_Client/Prefabs/Hits/`
- Action FBX: `Assets/VFX_V4_Client/Animations/`
- Runtime scripts: `Assets/VFX_V4_Client/Runtime/`
- Catalog: `Assets/VFX_V4_Client/Config/VFX_V4_Client_Catalog.asset`
- License: `Assets/VFX_V4_Client/Documentation/Licenses/`

## 9. 완료 조건

- [ ] Action FBX 8개가 Humanoid로 import되고 avatar가 valid입니다.
- [ ] Trail prefab 7개 각각 `TrailRenderer`가 정확히 2개입니다.
- [ ] Hit prefab 8개 각각 ParticleSystem이 있으며 `TrailRenderer`가 없습니다.
- [ ] Miss에서는 Trail만 보이고 Hit은 보이지 않습니다.
- [ ] 실제 hit에서는 contact point에 Hit이 한 번 생성됩니다.
- [ ] cancel 또는 state interruption 후 Trail emission이 종료됩니다.
- [ ] Strap 3종의 `FX_StrapTip` binding이 기존 끈의 실제 tip/end Transform을 가리킵니다.
- [ ] URP shader가 pink로 표시되지 않습니다.

## 10. 비포함 범위

- damage와 hitbox 판정
- hit-stop, camera shake, sound
- network replication
- 프로젝트 고유 object pool adapter
- 실제 game rig의 strap mesh와 physics

기본 runtime은 `Instantiate/Destroy`를 사용합니다. 클라이언트 프로젝트에 기존 pool이 있을 때만 `CombatVfxRuntime` 내부 생성·반환 부분을 해당 pool adapter로 교체하고 세 method interface는 유지하십시오.
