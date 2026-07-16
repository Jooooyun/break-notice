# Cargo Strapping System — Visual and Functional Spec

## 승인 이미지

![GEAR_01](../../references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png)

`GEAR_01_Cargo_Strapping_System_v002_FINAL`을 GEAR_01 Production Visual Canon으로 승인한다.

승인 범위는 이미지와 아래 Canonical Elements에 한정한다. 정확한 스트랩 거리, 각도, 힘, 타이밍, 입력, 장력 임계값 또는 기술 구현은 승인하지 않는다.

## Canonical Elements

- 허리 장착형 수축 릴
- 검정·황색 직조 산업용 화물 스트랩
- 은색 금속 카라비너
- 보강 아이가 있는 교체식 금속 자산 고정 클램프
- 브레이스의 기계식 장력 잠금 레버
- 제한된 황색 기계식 장력·하중 경고 표시
- `릴 → 스트랩 → 카라비너 → 클램프 → 하중 지지 금속 Anchor`의 물리 연결 체인
- 의자 좌석 아래 금속 틸트 메커니즘 브래킷 또는 보강 허브와 같은 하중 지지 금속 Anchor
- 카트 바퀴 위쪽의 금속 프레임 Anchor
- Stored, Attached, Tensioned Redirect, Safety Release, Brace Lock, Warning Behavior의 여섯 시각 상태
- Attached와 Tensioned Redirect에서 바닥 지지를 유지하는 의자
- Safety Release에서 열린 기계식 래치와 제어된 회수 방향
- Brace Lock에서 레버 위치만으로 읽히는 체결 상태
- 암적색 넥타이와 릴·스트랩·카라비너·클램프·의자·카트의 완전한 분리
- 현실적인 산업용 재질과 1인 개발 가능한 부품 밀도

## Prohibited Visual Connections

- 캐스터, 휠 축, 플라스틱 휠 하우징 또는 취약 돌출부에 연결
- 넥타이 연결 또는 목에서 나오는 줄
- 공중 물체 소환 또는 부유
- 그래플링 이동 또는 로프 스윙
- 에너지 케이블
- Heavy 원거리 견인
- 무한 거리 또는 장애물 관통

## Non-Canonical Elements

- 정확한 스트랩 거리와 리디렉션 각도
- 힘, 타이밍, 입력, 장력 임계값과 밸런스 수치
- 자동 해제의 내부 작동 원리와 정확한 회수 속도
- 브레이스 레버의 내부 기구와 기술 구현
- 리깅, 물리, IK, 애니메이션 또는 Unity 구현 방식
- 이미지의 정확한 볼트·흠집·마모·패널 간격·스튜디오 조명
- 우연한 미세 비대칭과 렌더링 아티팩트

## Draft Functional Behavior — Not Approved by the Visual Lock

다음은 상세 게임플레이 설계에서 검증할 `DRAFT`이며 이번 Visual Lock의 승인 범위가 아니다.

- 연결 가능 표면의 상세 판정
- 짧고 화면 내에서 이해 가능한 최대 거리
- 움직이거나 굴러오는 Medium의 방향 보정
- 흔들리는 가구 전도 보조
- 과도한 장력에서 자동 해제

## Visual Language

- 스트랩은 릴에서 끝단 하드웨어까지 연속적으로 보여야 한다.
- 카라비너는 클램프의 보강 아이에 연결되고 클램프가 하중 지지 금속 Anchor에 실제 접촉해야 한다.
- 장력은 스트랩 직선, 몸의 기울기, 브레이스 자세로 표현한다.
- 기계 표시등은 작은 황색으로 제한한다.
- 넥타이는 장비와 완전히 분리한다.

## Locked State Coverage

- 릴 정면·측면·후면
- Stored
- Attached
- Tensioned Redirect
- Safety Release
- Brace Lock
- Warning Behavior
- 의자 금속 프레임 연결
- 카트 금속 프레임 연결
