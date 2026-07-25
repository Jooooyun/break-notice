# Visual Reference Index

## Canon Hierarchy

1. 승인된 CHR_00 Player Identity, GEAR_01 Cargo Strapping System, CHR_01 First Ally Identity, PGT_01 Gameplay Camera, PGT_02 Chair Redirect Core Loop, PGT_03 Chain Reaction Aftermath, LGT_01 Night Archive Lighting, FIN_01 Conference Table Team Finisher 이미지
2. CHR_00 주 Identity 권위에 종속된 승인 Turnaround Production Support
3. Primary exploration references
4. Secondary scenario references
5. Non-canonical exploration

## Approved

| ID | 파일 | 상태 | 역할 |
|---|---|---|---|
| CHR-00-v002 | `references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png` | LOCKED | CHR_00 Player Identity Canon |
| GEAR-01-v002 | `references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png` | LOCKED | Cargo Strapping System Visual Canon |
| CHR-01-v001 | `references/approved/characters/CHR_01_First_Ally_Identity_v001_FINAL.png` | LOCKED | CHR_01 First Ally Identity Canon |
| PGT-01-v002 | `references/approved/gameplay/PGT_01_Gameplay_Camera_v002_FINAL.png` | LOCKED | Gameplay Camera Visual Canon |
| PGT-02-v010 | `references/approved/gameplay/PGT_02_Chair_Redirect_Core_Loop_v010_FINAL.png` | LOCKED | Chair Redirect Core Loop Visual Canon |
| PGT-03-v003 | `references/approved/gameplay/PGT_03_Chain_Reaction_Aftermath_v003_FINAL.png` | LOCKED | Chain Reaction Aftermath Visual Canon |
| LGT-01-v001 | `references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png` | LOCKED | Night Archive Lighting Visual Canon |
| FIN-01-v003 | `references/approved/gameplay/FIN_01_Conference_Table_Team_Finisher_v003_FINAL.png` | LOCKED | Conference Table Team Finisher Visual-only conditional Canon |

CHR_00 Player Identity, GEAR_01 Cargo Strapping System, CHR_01 First Ally Identity, PGT_01 Gameplay Camera, PGT_02 Chair Redirect Core Loop, PGT_03 Chain Reaction Aftermath, LGT_01 Night Archive Lighting과 FIN_01 Conference Table Team Finisher가 현재 `LOCKED`된 Production Visual이다.

PGT_01의 정확한 방 구조, 적 외형과 인원수, 소품 모델·배치·수량, 동료 위치, 조명과 collision surface는 incidental Non-Canon detail이다.

PGT_02의 정확한 방 구조, 조명, 목표 위치와 외형, 의자 모델, 캐스터 각도, 배우 위치와 세부 포즈는 incidental Non-Canon detail이다.

PGT_03의 정확한 Chain 순서와 hop 수, 개수, 방·소품·통로 치수, 목표·의자·자산 배치, 파편·Particle, 조명, 스트랩 해제 시점과 구현은 incidental Non-Canon 또는 Draft다. 깨진 유리는 새로운 visibility line을 만들지만 완전 통과 가능한 경로의 정확한 형태와 치수는 Canon이 아니다.

LGT_01은 Records Archive 지역의 after-hours 조명 변주이며 게임 전체의 Global Lighting 또는 Color Grading 기준이 아니다. 정확한 조명 수치·기구·Flicker·Emergency Behavior·렌더링 구현과 이미지의 방·선반·통로·배우·적·소품 배치는 incidental Non-Canon 또는 Draft다.

FIN_01은 PR-F-022 `COULD` 기능의 Visual-only conditional direction이다. 익명의 Finisher-only 보조 직원은 CHR_02 또는 지속 Party Member가 아니며, 정확한 신원·테이블·캐스터·목표·카메라·방·자세와 모든 기능·구현 세부는 incidental Non-Canon, Draft 또는 미승인이다. Visual Lock은 Vertical Slice 또는 최종 제품 편입을 의미하지 않는다.

## Production Visual Lock Sequence

1. `GEAR_01 — Cargo Strapping System` — `LOCKED`
2. `CHR_01 — First Ally Identity` — `LOCKED`
3. `PGT_01 — Gameplay Camera` — `LOCKED`
4. `PGT_02 — Chair Redirect Core Loop` — `LOCKED`
5. `PGT_03 — Chain Reaction Aftermath` — `LOCKED`
6. `LGT_01 — Night Archive Lighting` — `LOCKED`
7. `FIN_01 — Conference Table Team Finisher` — `LOCKED`

현재 Production Visual Lock sequence는 완료되었다. 다음 Production Visual 대상은 승인되지 않았으며 Full Design Lock과 Implementation Gate 상태는 별도 승인 대상이다.

## Approved Production Support

| ID | 파일 | 상태 | 역할 |
|---|---|---|---|
| CHR-00-TURN-v001 | `references/approved/characters/CHR_00_Player_Turnaround_v001_FINAL.png` | OWNER-APPROVED / LOCKED PRODUCTION SUPPORT | 정확한 다섯 뷰와 후면·측면 제작 가독성 지원 |

이 한 개의 Production Support는 승인된 Production Visual 여덟 개의
수를 늘리거나 Lock sequence를 변경하지 않는다. CHR_00 Identity의 주
권위는 CHR-00-v002, `PLAYER_VISUAL_SPEC.md`와 ADR-0007이며 Turnaround는
그 권위에 종속된다. 3D 모델, rig, texture, material, gameplay,
Unity asset, Graybox 또는 구현 권한은 없다.

## User-Selected Primary/Secondary References

| 번호 | 파일 | Canon 역할 | 가져갈 것 | 버릴 것 |
|---:|---|---|---|---|
| 01 | `EXP_01_Cinematic_Key_Art.png` | Primary Impact | 현실감, 무게, 유리 충돌 | 과도한 파편, 영화 카메라 |
| 05 | `EXP_05_Character_Team_Lineup.png` | Primary Character Ecosystem | 직군, 작업복, 도구 | 4인 추적 파티 인상 |
| 06 | `EXP_06_Night_Fluorescent_Noir.png` | Secondary Lighting | 기록실 조명과 긴장감 | 전체 게임 기본 톤, 과도한 암부 |
| 09 | `EXP_09_Chair_Redirect_Low_Angle.png` | Primary Mechanic | 물리 연결, 의자 리디렉션 | 넥타이 연결, 낮은 실제 카메라 |
| 10 | `EXP_10_Conference_Table_Finisher.png` | Secondary Finisher | 합동 Heavy 판타지 | 일반 전투 시스템, 많은 NPC |

## Non-Canonical Exploration

02 그래픽 코믹, 03 페인터리 파괴, 04 아이소메트릭, 07 애니메이션 풍자, 08 복사실 종이 폭풍은 부분 참고 또는 아카이브다. 08의 토너·종이 재질 반응만 VFX 참고로 사용한다.

## Image Rule

이미지에 문서에 없는 총, 장비, 동료, 폭발, 방 구조가 나타나도 자동으로 기능 요구사항이 되지 않는다.

**The specification is law. The image is evidence, not authority.**
