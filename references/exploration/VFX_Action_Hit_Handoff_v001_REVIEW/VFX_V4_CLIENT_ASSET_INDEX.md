# VFX V4 Client Asset Index

실제 Unity 적용 파일: `VFX_V4_ClientReady_Trail_Hit.unitypackage`

`GIF`는 검수용입니다. runtime에서는 Trail/Hit prefab을 사용합니다.

| Action | Trail prefab | Hit prefab | Trail GIF | Hit GIF |
|---|---|---|---|---|
| Jab | `VFX_V4_Trail_Jab.prefab` | `VFX_V4_Hit_Jab.prefab` | `VFX_V4_Trail_Jab.gif` | `VFX_V4_Hit_Jab.gif` |
| Cross | `VFX_V4_Trail_Cross.prefab` | `VFX_V4_Hit_Cross.prefab` | `VFX_V4_Trail_Cross.gif` | `VFX_V4_Hit_Cross.gif` |
| Kick | `VFX_V4_Trail_Kick.prefab` | `VFX_V4_Hit_Kick.prefab` | `VFX_V4_Trail_Kick.gif` | `VFX_V4_Hit_Kick.gif` |
| BodyCheck | `VFX_V4_Trail_BodyCheck.prefab` | `VFX_V4_Hit_BodyCheck.prefab` | `VFX_V4_Trail_BodyCheck.gif` | `VFX_V4_Hit_BodyCheck.gif` |
| StrapSweepL | `VFX_V4_Trail_StrapSweepL.prefab` | `VFX_V4_Hit_StrapSweepL.prefab` | `VFX_V4_Trail_StrapSweepL.gif` | `VFX_V4_Hit_StrapSweepL.gif` |
| StrapSweepR | `VFX_V4_Trail_StrapSweepR.prefab` | `VFX_V4_Hit_StrapSweepR.prefab` | `VFX_V4_Trail_StrapSweepR.gif` | `VFX_V4_Hit_StrapSweepR.gif` |
| StrapSlam | `VFX_V4_Trail_StrapSlam.prefab` | `VFX_V4_Hit_StrapSlam.prefab` | `VFX_V4_Trail_StrapSlam.gif` | `VFX_V4_Hit_StrapSlam.gif` |
| KnockdownReaction | — | `VFX_V4_Hit_KnockdownReaction.prefab` | — | `VFX_V4_Hit_KnockdownReaction.gif` |

## 실제 호출

- 공격 궤적: `CombatVfxRuntime.BeginAttack`
- 실제 명중: `CombatVfxRuntime.ConfirmHit`
- 공격 종료 또는 취소: `CombatVfxRuntime.EndAttack`
- miss: `ConfirmHit`을 호출하지 않음
