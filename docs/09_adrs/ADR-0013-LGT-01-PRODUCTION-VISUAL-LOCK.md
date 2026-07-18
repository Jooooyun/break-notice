# ADR-0013: LGT_01 Night Archive Lighting Production Visual Lock v001

- Status: Accepted
- Date: 2026-07-18

## Decision

`references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png`를 LGT_01 Night Archive Lighting Production Visual Canon으로 승인한다.

LGT_01은 Records Archive에 한정된 after-hours lighting variant이며 게임 전체의 Global Lighting 또는 Color Grading 기준이 아니다.

## Canon

- 현실적인 차가운 중성 형광등 Practical을 Primary Source로 사용하는 관계
- 선반과 통로 암부에 적용된 절제된 Cool Neutral-to-Green-Blue Ambient
- Black Crush 없이 유지되는 Shadow Detail
- 거칠고 제한된 바닥·금속·유리·플라스틱 PBR Highlight
- 플레이어, 동료, 임시 적, Stored GEAR_01, 사용 가능 물체와 복수 통로의 Gameplay Readability
- 전경, 선반 Aisle과 후경 사이의 공간 깊이
- 공포나 초자연 표현이 아닌 진지한 After-hours Corporate Tension

세부 Canonical Elements는 `docs/03_art/NIGHT_ARCHIVE_LIGHTING_VISUAL_SPEC.md`를 따른다.

## Local-Variant Boundary

이 결정은 Records Archive 지역 조명 변주만 승인한다. 전역 Lighting, Global Color Grading, 다른 지역의 조명 방향, Emergency Gameplay 또는 전원 상태 동작을 승인하지 않는다.

## Non-Canon

- 정확한 색온도, RGB, Lux, Exposure와 Contrast
- 조명 기구 수와 위치, Fixture Failure와 Flicker
- Emergency Gameplay, Shadow와 Reflection 세부
- LUT, Tone Mapping, Baked/Realtime/Mixed Lighting, GI, Probe와 Volumetric
- Post-processing, URP 설정과 Unity 구현
- 정확한 방 구조, 선반 수, Aisle 폭과 Level Layout
- 적·소품·카트 수와 위치, Actor 배치와 Collision Surface
- 정확한 조명 노출, 마모, 배치와 렌더링 아티팩트

## Consequences

- LGT_01의 문서화된 Visual Canon은 `LOCKED`이며 정확한 조명 수치·렌더링 설계와 구현은 `DRAFT`다.
- 다음 Production Visual 대상은 `FIN_01 — Conference Table Team Finisher`다.
- FIN_01은 이 ADR에서 승인하지 않는다.
- LGT_01 이미지의 정확한 방·선반·통로·배우·적·소품·조명 배치는 후속 Canon으로 자동 승격되지 않는다.
- Implementation Gate는 `CLOSED`로 유지한다.
