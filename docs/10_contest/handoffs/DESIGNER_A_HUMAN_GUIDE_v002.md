# Designer A Human Guide v002

- Your branch: `art/characters-animation`
- Start only after it cleanly equals: `6030c4af6b800d0d7e80013382babd7b1cfc58a0`
- Read first: `BREAK_NOTICE_GAMEPLAY_ANIMATION_COVERAGE_v003_FINAL.txt`
- Do not touch Unity Runtime or the Showcase Scene.

## 첫 작업

새 애니메이션을 대량 제작하기 전에 아래 CSV 한 개를 먼저 만드십시오.

`ArtSource/Characters/ANIMATION_COVERAGE_MATRIX_v001.csv`

기존 Animation FBX 151개를 전부 한 줄씩 기록하고 다음 셋 중 하나로
분류합니다.

- `REUSE CANDIDATE`: 재사용 가능성이 있어 Preview 검토할 후보
- `ARCHIVE ONLY`: 총기·수류탄·검·Elite 등 현재 방향에 쓰지 않을 기록
- `MISSING / NEW GENERATION REQUIRED`: 현재 파일로 충족하지 못하는 모션

재사용 후보는 승인본이 아닙니다. Human Owner가 Preview를 통과시킨 뒤에만
선택 FBX와 편집 가능한 원본을 전달합니다.

## 기존 파일 보호

- `0810_designerA/**`를 삭제·이동·이름 변경·덮어쓰기 하지 마십시오.
- 기존 batch는 계속 `REVIEW HOLD / DO NOT INTEGRATE`입니다.
- 권리와 생성 출처가 해결되지 않은 상태를 완료로 표시하지 마십시오.
- 총기, 수류탄, 검, Elite, Boss 관련 파일은 `ARCHIVE ONLY`입니다.

## 새 작업 허용 위치

- `ArtSource/Characters/CHR00_Player/**`
- `ArtSource/Characters/CHR01_Ally/**`
- `ArtSource/Characters/ENY01_ComplianceRunner/**`
- `ArtSource/Equipment/GEAR01_CargoStrap/**`

새 얼굴, 새 모델, 새 의상, 새 텍스처, 새 Character Identity는 만들지
마십시오. 기존 승인 비주얼을 바꾸는 작업도 금지합니다.

## AI 보조를 쓸 때

애니메이션 생성·리타게팅·키프레임/루프 정리·접촉 보정에는 AI 보조를
사용할 수 있습니다. 대신 사용 도구, 버전, 입력 소스, 라이선스/약관,
날짜, 사람이 수정한 내용과 남은 문제를 반드시 기록하십시오. 출처나
권리가 빠지면 해당 후보는 `BLOCKED`입니다.

## Preview 순서

1. 정면 Preview
2. 측면 Preview
3. 실제 gameplay-camera Preview
4. 필요한 접촉/짝동작 Preview
5. Human Owner 검토
6. 통과한 후보만 selected FBX + editable source export

Binder, Chair, GEAR01은 Preview용 소품으로만 사용하고 Character FBX에
영구 포함하지 마십시오.

## Git 규칙

한 목적당 한 commit만 만들고 본인의 art branch에 normal push하십시오.
PR·merge·rebase·amend·force push는 하지 마십시오. Animator, Runtime,
Scene, Prefab, Input, Package, Test, Build 파일은 수정하지 마십시오.

## 제출 보고

CSV 전체 행 수, REUSE/ARCHIVE/MISSING 각 개수, Preview 목록, 권리 차단
항목, 변경 파일, commit SHA와 push 결과를 보고하십시오. Owner Approved
또는 Integrated라고 스스로 표시하지 마십시오.
