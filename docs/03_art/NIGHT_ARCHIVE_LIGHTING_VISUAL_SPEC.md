# Night Archive Lighting Visual Specification

## 승인 이미지

![LGT_01](../../references/approved/lighting/LGT_01_Night_Archive_Lighting_v001_FINAL.png)

`LGT_01_Night_Archive_Lighting_v001_FINAL`을 LGT_01 Night Archive Lighting Production Visual Canon으로 승인한다.

승인 범위는 이미지와 아래 Canonical Elements에 한정한다. LGT_01은 Records Archive 지역 조명 변주이며 게임 전체의 Global Lighting 또는 Color Grading 기준이 아니다.

## Canonical Elements

- Records Archive에 한정된 after-hours 조명 변주
- 현실적인 차가운 중성 형광등 Practical을 Primary Source로 사용하는 관계
- 선반과 통로 암부에 적용된 절제된 Cool Neutral-to-Green-Blue Ambient
- Black Crush 없이 유지되는 Shadow Detail
- 거칠고 제한된 바닥·금속·유리·플라스틱 PBR Highlight
- 플레이어, 동료, 임시 적, Stored GEAR_01, 사용 가능 물체와 복수 통로의 Gameplay Readability
- 전경, 선반 Aisle과 후경 사이의 공간 깊이
- 공포나 초자연 표현이 아닌 진지한 After-hours Corporate Tension

## Continuity Constraints

- CHR_00, GEAR_01, CHR_01과 PGT_01은 각 Visual Spec과 ADR에 기록된 Canonical Elements만 따른다.
- 실제 3인칭 Gameplay View, 약간 높은 OTS 방향, 전신 또는 거의 전신 가시성, 바닥과 경로 가독성을 유지한다.
- 플레이어, 승인된 활성 동료 한 명과 익명 임시 적은 색상만이 아니라 실루엣, 명도, 자세, 재질과 공간 분리로 구분된다.
- GEAR_01은 Stored/Inactive 상태로 읽히며 릴, 수납된 검정·황색 스트랩과 금속 하드웨어는 emissive 표시 없이 구분된다.
- 암적색 넥타이는 복장 전용이며 장비나 물체와 연결되지 않는다.
- 조명은 플레이어, 동료, 임시 적, 장비, 사용 가능 물체, 충돌 표면과 복수 경로를 가리지 않는다.
- 공포, 초자연 효과, Flashlight-only presentation, Black Crush, monochrome wash, wet/mirror floor와 cinematic obscuration을 사용하지 않는다.
- 파괴, PGT_03 aftermath와 FIN_01 choreography를 이 조명 변주의 필수 상태로 사용하지 않는다.

## Local-Variant Boundary

LGT_01은 Records Archive에 한정된 local after-hours lighting variant다. BREAK NOTICE 전체 지역의 기본 Lighting, Global Color Grading, 전역 Exposure 또는 Tone Mapping을 승인하지 않는다.

이미지에 적색 Indicator가 나타나더라도 이는 Emergency Gameplay, 적 표시, Interactable 표시 또는 초자연 효과를 승인하지 않는다. 적색 Indicator는 LGT_01 Canon의 필수 요소가 아니다.

## Non-Canonical Elements

- 정확한 색온도, RGB, Lux, Exposure와 Contrast
- 조명 기구의 정확한 모델, 수, 위치와 Fixture Failure
- Flicker와 Emergency Gameplay 또는 전원 상태 동작
- 정확한 Shadow softness, range와 shadow-casting light count
- Reflection strength, LUT, Tone Mapping과 Global Color Grading
- Baked/Realtime/Mixed Lighting, GI, Probe와 Volumetric
- Post-processing, URP 설정과 Unity 구현
- 정확한 방 구조, 선반 수, Aisle 폭과 Level Layout
- 정확한 적·소품·카트 수와 위치, Actor 배치, Route와 Collision Surface
- 정확한 얼굴, 머리, 주름, 마모, 라벨, 조명 노출과 렌더링 아티팩트

## FIN_01 Boundary

LGT_01은 Conference Table Team Finisher, 두 번째 활성 동료, Heavy choreography, 피니셔 카메라 또는 적 수를 승인하지 않는다. 다음 Production Visual 대상은 `FIN_01 — Conference Table Team Finisher`다.

## Implementation Boundary

이 Visual Lock은 정확한 조명 수치, 렌더링 기술, Emergency Behavior, level design 또는 Unity 구현을 승인하지 않는다. Implementation Gate는 `CLOSED`다.
