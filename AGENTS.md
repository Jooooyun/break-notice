# AGENTS.md — BREAK NOTICE Preproduction Operating Rules

이 파일은 BREAK NOTICE 저장소 전체에 적용되는 최상위 Codex 작업 규칙이다.

Codex는 모든 작업을 시작하기 전에 이 파일을 읽어야 한다.

---

# 1. Current Project Phase

현재 단계:

`PRE-PRODUCTION / PRODUCTION VISUAL LOCK`

현재 프로젝트는 전체 Design Lock 상태가 아니다.

다음 항목은 아직 DRAFT다.

- 상세 게임플레이 설계
- 요구사항 명세
- 인수 테스트
- 밸런스 수치
- 기술 아키텍처
- Unity 구현 계획
- Vertical Slice 구현 범위

Implementation Gate는 `CLOSED`다.

명시적인 사용자 승인과 Design Readiness Gate 통과 전에는 Unity 구현을 시작하지 않는다.

---

# 2. Current Approved Visual Canon

현재 승인되고 LOCK된 Production Visual은 다음 두 개다.

- `references/approved/characters/CHR_00_Player_Identity_Lock_v002_FINAL.png`
- `references/approved/equipment/GEAR_01_Cargo_Strapping_System_v002_FINAL.png`

관련 승인 문서:

- `docs/03_art/PLAYER_VISUAL_SPEC.md`
- `docs/03_art/CARGO_STRAP_SPEC.md`
- `docs/09_adrs/ADR-0007-PLAYER-VISUAL-LOCK.md`
- `docs/09_adrs/ADR-0008-GEAR-01-PRODUCTION-VISUAL-LOCK.md`

승인된 CHR_00의 다음 요소는 임의로 변경하지 않는다.

- 현실적인 성인 남성 비율
- 지치고 집중된 회사원 인상
- 구겨진 흰색 셔츠
- 걷어 올린 소매
- 짧고 느슨한 암적색 넥타이
- 사원증
- 작업용 바지
- 작업 장갑
- 안전화
- 산업용 하중 분산 하네스
- 한쪽 팔의 외부 장착형 하중 보조 브레이스
- 허리 장착형 수축식 화물 스트랩 릴
- 검정·황색 화물 스트랩
- 금속 카라비너

이미지의 렌더링 품질, 미세 볼트 위치, 얼굴 모공, 흠집 위치는 Canon이 아니다.

승인된 GEAR_01의 다음 요소는 임의로 변경하지 않는다.

- 허리 장착형 수축 릴
- 검정·황색 직조 화물 스트랩
- 은색 금속 카라비너
- 보강 아이가 있는 교체식 자산 고정 클램프
- `릴 → 스트랩 → 카라비너 → 클램프 → 하중 지지 금속 Anchor` 연결 체인
- 캐스터·휠 축·플라스틱 하우징·취약 돌출부가 아닌 하중 지지 금속 Anchor
- Stored, Attached, Tensioned Redirect, Safety Release, Brace Lock, Warning Behavior의 여섯 시각 상태
- 바닥 지지 상태를 유지하는 의자 리디렉션
- 체결 위치가 읽히는 브레이스 잠금 레버
- 제한된 황색 기계식 경고 표시
- 넥타이와 장비의 완전한 분리

정확한 스트랩 거리, 각도, 힘, 타이밍, 입력, 장력 임계값, 내부 작동, 기술 구현, 미세 부품 위치는 GEAR_01 Canon이 아니다.

---

# 3. Production Visual Lock Progress

다음 순서로 Production Visual Lock을 진행한다.

1. `GEAR_01 — Cargo Strapping System` — `LOCKED`
2. `CHR_01 — First Ally Identity` — `NEXT`
3. `PGT_01 — Gameplay Camera`
4. `PGT_02 — Chair Redirect Core Loop`
5. `PGT_03 — Chain Reaction Aftermath`
6. `LGT_01 — Night Archive Lighting`
7. `FIN_01 — Conference Table Team Finisher`

현재 다음 작업 대상은:

`CHR_01 — First Ally Identity`

이다.

각 결과물은 사용자에게 명시적으로 승인받기 전까지 `REVIEW` 상태다.

Codex는 스스로 결과물을 `APPROVED`, `FINAL`, `LOCKED`로 선언하지 않는다.

---

# 4. Approved High-Level Constraints

다음은 현재 프리프로덕션 작업에서 유지할 상위 제약이다.

- 1인 개발
- 폭포수 기반 개발
- 명시적 단계별 승인 게이트
- Unity 6 예정
- 3D Third-Person 예정
- Grounded Corporate Action Realism
- Grounded world, absurd physics
- 마법과 염력 없음
- 초능력 Grab 없음
- 붉은 넥타이는 일반 복장과 시각적 상징일 뿐임
- 넥타이를 로프, 케이블, 무기, 연결 장치로 사용하지 않음
- 물체 제어에는 별도의 산업용 화물 스트랩 사용
- 일반 전투의 활성 동료는 한 명
- 파괴는 통제되고 1인 개발 가능한 범위로 제한
- 캐릭터는 상황을 진지하게 받아들임
- 코미디는 과장된 얼굴이 아니라 물리적 결과에서 발생

이 제약들은 세부 게임 설계가 모두 완료되었다는 뜻이 아니다.

---

# 5. Instruction Authority

작업 중 정보가 충돌하면 다음 순서로 판단한다.

1. 현재 사용자가 명시적으로 내린 작업 지시
2. 저장소 루트 `AGENTS.md`
3. 사용자가 승인한 ADR
4. 현재 Phase 및 Status 문서
5. 승인된 Visual Lock 문서
6. 상세 설계 및 기술 문서
7. 탐색 이미지
8. 과거 문서와 Archive

현재 작업 지시가 승인된 상위 결정과 충돌하면 임의로 실행하지 않는다.

충돌 내용을 보고하고 사용자 판단을 기다린다.

탐색 이미지의 우연한 디테일은 게임 요구사항이 아니다.

승인 이미지도 해당 Visual Lock 문서에 기록된 Canonical Element만 권한을 가진다.

---

# 6. Duplicate Documentation Warning

현재 저장소에는 서로 겹치는 문서 구조가 존재할 수 있다.

예:

- 대문자 기반 문서 폴더
- 소문자 기반 문서 폴더
- 유사한 요구사항 문서
- 유사한 인수 테스트 문서
- 유사한 Codex 프로토콜 문서

Codex는 별도 정리 작업 없이 다음 행동을 하지 않는다.

- 어느 문서가 Canon인지 임의 결정
- 중복 파일 삭제
- 문서 이동
- 내용 병합
- 요구사항 ID 변경
- 폴더 이름 통일
- Archive 처리

중복이나 충돌을 발견하면 보고만 하고, 명시적인 정리 작업이 주어질 때까지 수정하지 않는다.

---

# 7. One Task, One Outcome

하나의 Task는 하나의 명확한 결과만 만든다.

허용되는 예:

- 저장소 상태 감사
- GEAR_01 시안 검토
- 승인된 이미지 등록
- 특정 문서 한 개 정리
- 특정 시스템 하나 설계
- 특정 기능 하나 구현

금지되는 예:

- 설계 확정과 Unity 구현을 동시에 진행
- 이미지 생성과 저장소 전체 정리를 동시에 진행
- 요청받지 않은 리팩터링
- 관련 없는 파일 정리
- 새로운 기능 추가
- 프로젝트 전체를 알아서 완성

---

# 8. Before Editing

파일을 수정하기 전에 반드시 다음을 먼저 보고한다.

- 이해한 작업 목표
- 읽은 지침과 Canon 문서
- 변경 예정 파일
- 변경하지 않을 파일
- 발견한 모호함
- 범위 밖 항목
- 사용자 승인이 필요한 사항

모호함이 결과에 영향을 줄 경우 임의로 추측하지 않는다.

---

# 9. Repository Modification Rules

명시적으로 허용되지 않은 파일은 수정하지 않는다.

명시적인 요청 없이는 다음을 하지 않는다.

- 파일 생성
- 파일 삭제
- 파일 이동
- 파일 이름 변경
- 디렉터리 구조 변경
- 외부 의존성 설치
- Unity 패키지 설치
- 브랜치 생성
- Commit
- Push
- Pull Request 생성
- Pull Request Merge

자동 Merge는 금지한다.

사용자가 허용한 경우에도 변경사항과 Diff를 먼저 보고한다.

---

# 10. Visual Workflow Rules

비주얼 작업 상태는 다음을 사용한다.

- `EXPLORATION`
- `REVIEW`
- `REVISION_REQUIRED`
- `LOCKED`
- `ARCHIVED`

새로 생성된 시안의 기본 상태는 `REVIEW`다.

사용자만 다음 판정을 내릴 수 있다.

- `APPROVE`
- `REVISION REQUIRED`
- `REJECT`

사용자가 명시적으로 `APPROVE`하기 전에는:

- FINAL 파일명 사용 금지
- LOCK 문서 등록 금지
- ADR 승인 기록 금지
- 다음 단계의 Canon으로 사용 금지

이미지 생성은 사용자가 명시적으로 요청한 경우에만 수행한다.

---

# 11. Current Implementation Restrictions

Implementation Gate가 닫혀 있으므로 다음은 금지한다.

- Unity 프로젝트 초기화 또는 변경
- C# 게임플레이 코드 작성
- 패키지 설치
- Input System 설정
- Cinemachine 설정
- Rigidbody 시스템 구현
- 적 AI 구현
- 씬 제작
- Prefab 제작
- 빌드 파이프라인 구성
- 최적화 작업
- 테스트 코드 구현

현재 허용 범위는 명시적으로 요청된 프리프로덕션 작업뿐이다.

- 저장소 상태 확인
- 비주얼 시안 관리
- 승인 기록
- 문서 검토
- 설계 조율
- 범위 정의
- 변경 관리

---

# 12. Scope Protection

Codex는 다음을 임의로 추가하지 않는다.

- Co-op
- 총기
- 마법
- 염력
- 슈퍼히어로 장비
- 군사 SF 장비
- 다수 동료 동행 시스템
- 완전 절차적 파괴
- 모든 벽 자유 파괴
- 오픈월드
- 온라인 기능
- 추가 플랫폼
- 보스
- 메타 성장
- 새로운 캐릭터
- 새로운 게임 모드

좋아 보인다는 이유만으로 범위를 확장하지 않는다.

---

# 13. Completion Report

모든 작업 완료 보고에는 다음을 포함한다.

1. 작업 요약
2. 읽은 파일
3. 변경한 파일
4. 변경하지 않은 범위
5. 승인된 결정과의 일치 여부
6. 수행한 검사
7. 남은 모호함
8. 알려진 한계
9. 다음 권장 작업 한 개
10. 사용자 승인이 필요한 사항

파일을 수정하지 않은 작업에서는 반드시 다음을 명시한다.

`No files, branches, commits, pushes, or pull requests were created or modified.`

---

# 14. Final Operating Principle

설계되지 않은 것을 구현하지 않는다.

승인되지 않은 것을 LOCK하지 않는다.

요청받지 않은 범위를 확장하지 않는다.

문서 충돌을 조용히 해결하지 않는다.

사용자가 승인하기 전에는 다음 단계로 넘어가지 않는다.

**The specification is law.  
The image is evidence, not authority.  
Owner approval opens the next gate.**
