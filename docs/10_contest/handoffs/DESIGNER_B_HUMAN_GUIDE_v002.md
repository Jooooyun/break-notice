# Designer B Human Guide v002

- Your branch: `art/environment-ui-vfx`
- Remote baseline: `7536f551fca146e225794219bc219d7f07dd5314`
- Read first: `BREAK_NOTICE_CONTEST_SHOWCASE_MAP_DESIGN_v001.txt`
- Do not touch Unity Runtime or either Gameplay Scene.

## 먼저 할 일

현재 9개 Blender 파일을 바로 완성품으로 취급하지 말고, Showcase Asset
Matrix에 연결해 분류하십시오.

- Chair의 `PRP_OfficeChair_A_Blockout_v001.blend`: 재사용 검토 후보
- `officeChair.blend`, `DrawerCabinet.blend`: 이름·버전 교정 필요
- `Copier.blend`, `copier_2.blend`: 어느 것이 어떤 후보인지 확인 필요
- `Untitled.blend`: 최종 파일명으로 사용 금지
- 모든 `.blend1`: 백업/Archive이며 최종 납품 아님

이 9개 파일은 삭제·이동·이름 변경·덮어쓰기 하지 마십시오. 현재 Runtime
승인 파일은 0개입니다.

## 맵 목록을 기존 16개 Family에 매핑

새 Family를 만들기 전에 `OFFICE_CORE_KIT.md`의 기존 16개 Family 안에서
모듈 조각이나 비상호작용 변형으로 분류하십시오.

특히 `ENV_SecurityShutter_VistaOnly`는 새 Family가 아닙니다.
`OCK-A-002 Fixed Office Door and Frame` 아래의 고정된 Vista용 모듈입니다.

먼저 확인할 순서:

1. Chair / Drawer / Copier / Binder 후보와 누락 항목
2. Wall / Door / Partition / Floor / Ceiling / Column / Light / Desk 모듈
3. Monitor / Paper / Plant / Waste / Signage / Tape / Wear 변형
4. PASS/BRACE/BREAK, Drawer, Exit에 필요한 UI/VFX Preview 목록

## 전달 방식

Designer B는 편집 가능한 Source와 Preview를 만듭니다. 새 후보는 공백이나
한글 없이 clean versioned name을 사용하고, DELIVERY와 권리 정보를 함께
기록하십시오. 기존 파일을 조용히 이름 변경하지 말고 별도 교정 후보로
제출하십시오.

Unity Import, Material, Collider, Prefab, Runtime 폴더 이동, Gameplay Scene
배치, Test와 Browser Build는 Human Owner/Codex의 후속 통합 작업입니다.

## Git 규칙

로컬 branch가 원격보다 한 commit 뒤였으므로, 작업 시작 전 clean 상태와
별도 normal fast-forward 승인을 확인하십시오. S0에서는 branch switch나
fast-forward를 하지 않습니다.

한 목적당 한 commit만 만들고 art branch에 normal push하십시오. PR·merge·
rebase·amend·force push는 하지 마십시오. Code, Scene, Prefab, Input,
Package, Test, Build 파일은 수정하지 마십시오.

## 제출 보고

9개 원본 보존 여부, 매핑된 항목 수, REUSE/REVISION/MISSING 개수, Preview
목록, 누락 DELIVERY/권리, 변경 파일, commit SHA와 push 결과를
보고하십시오. Owner Approved 또는 Integrated라고 스스로 표시하지
마십시오.
