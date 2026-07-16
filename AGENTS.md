# AGENTS.md — BREAK NOTICE Codex Constitution

이 파일은 저장소 전체에 적용되는 최상위 작업 규칙이다. Codex와 모든 자동화 도구는 작업을 시작하기 전에 반드시 읽는다.

## 1. 프로젝트 목표

`BREAK NOTICE`는 1인 개발자가 Unity 6로 제작하는 3D TPS 물리 액션 Roguelite다. 핵심 재미는 동료가 실제 공간에서 물체를 던지거나 굴려주고, 플레이어가 직접 집기·캐치·산업용 스트랩 리디렉션·투척으로 연쇄 충돌을 만드는 것이다.

## 2. Canon 우선순위

충돌이 있을 때 아래 순서가 우선한다.

1. `AGENTS.md`
2. 승인된 ADR (`docs/09_adrs/`)
3. 요구사항 (`docs/01_requirements/`)
4. 기술·게임·아트 명세 (`docs/02_design/` ~ `docs/04_technical/`)
5. 인수 테스트 (`docs/07_quality/ACCEPTANCE_TESTS.md`)
6. 승인된 비주얼 (`references/approved/`)
7. 탐색 이미지와 과거 문서

탐색 이미지의 우연한 디테일은 요구사항이 아니다.

## 3. Locked Decisions

다음 결정을 임의로 변경하지 않는다.

- 3D TPS
- Unity 6 + URP + GameObject PhysX
- Windows PC 우선
- 초능력·염력 Grab 없음
- Scoop + Cargo Strap Redirect + Ally Call & Catch
- 붉은 넥타이는 물체에 연결하지 않음
- 일반 전투 활성 동료 1명
- Grounded Corporate Action Realism
- 미리 제작된 파손 상태와 제한된 큰 물리 파편
- 1인 개발 범위 우선
- 폭포수 기반 개발과 명시적 검증 게이트

## 4. 작업 시작 조건

Codex는 아래가 없는 기능을 구현하지 않는다.

- 작업 티켓 ID
- 연결된 요구사항 ID
- 변경 가능한 파일 목록
- 금지된 변경 목록
- 인수 조건
- 필요한 테스트
- 완료 후 보고 형식

티켓은 `tasks/TASK_TEMPLATE.md`를 복사해 작성한다.

## 5. 작업 방식

1. 관련 Canon 문서를 읽는다.
2. 요청이 문서와 충돌하는지 확인한다.
3. 충돌하면 코드를 작성하지 말고 Change Request 필요성을 보고한다.
4. 구현 계획을 파일 단위로 작성한다.
5. 승인된 범위만 수정한다.
6. 테스트를 작성하거나 기존 테스트를 갱신한다.
7. 인수 조건을 직접 검증한다.
8. 변경 파일, 테스트 결과, 남은 위험을 보고한다.

## 6. 금지 사항

- 요청받지 않은 리팩터링
- 새 패키지나 외부 의존성의 임의 추가
- 싱글턴 남발
- `FindObjectOfType`, 문자열 기반 이벤트, 전역 mutable state의 무분별한 사용
- 모든 물체에 상시 Dynamic Rigidbody 적용
- 모든 파편에 물리 계산 적용
- 총기·마법·에너지 오라·염력 기능 추가
- 일반 전투 동료 수 확대
- 설계 문서와 테스트를 갱신하지 않은 동작 변경
- “나중에 고친다”는 이유로 null 처리, 예외 처리, 로그를 생략

## 7. 코드 방향

구현이 시작되면 다음을 따른다.

- C# 식별자는 영어
- 문서와 주석은 한국어 또는 명확한 영어
- 컴포넌트는 단일 책임
- ScriptableObject는 데이터 정의에 사용하고 런타임 상태를 저장하지 않음
- 시스템 간 통신은 명시적 인터페이스와 타입 안전 이벤트 사용
- 물리 이벤트와 게임 판정 이벤트를 분리
- 설정값은 하드코딩하지 않고 승인된 데이터 에셋으로 관리
- EditMode와 PlayMode 테스트를 목적에 맞게 분리

## 8. Definition of Done

기능은 다음을 모두 만족해야 완료다.

- 요구사항 ID 충족
- 인수 테스트 통과
- 신규 경고와 오류 없음
- 테스트 추가 또는 갱신
- 문서 동기화
- 성능 예산 위반 없음
- 작업 범위 밖 파일의 불필요한 변경 없음
- 변경 요약과 리스크 보고 완료

## 9. 설계 변경

Locked Decision을 바꾸려면 반드시:

1. `docs/00_project/CHANGE_CONTROL.md`의 Change Request 작성
2. 영향 분석
3. 관련 ADR 작성 또는 기존 ADR 대체
4. 요구사항과 추적표 갱신
5. 인수 테스트 갱신
6. 승인 후 구현

**The specification is law. The image is evidence, not authority.**
