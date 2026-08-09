# Enemy Normal (ENY01 후보, elite와 동일 뼈대 재사용) — Animation Usage Guide

베이스: `enemy_normal_RIGGED_textured.fbx` (자체 리깅, 65 bone, mesh 1개(LOD0), 텍스처 4장 임베드).
애니메이션 45개는 **enemy_elite와 동일 클립 그대로 재사용** — 별도로 다시 안 받음. 이유: 두 캐릭터 다 Mixamo Standard Skeleton(65 bone, 동일 뼈 이름) 기준으로 리깅됐고, world-space 렌더로 리타겟팅 확인함(`standard run.fbx`를 normal 리그에 적용해 포즈 정상 확인, 텍스처도 정상 표시됨).

## 상태별 매핑

`character_elite_enemy/ANIMATION_USAGE_GUIDE.md`와 완전히 동일한 카테고리·용도 표를 그대로 따름 (Locomotion / Strafe 8종 / Combat·Weapon / Injured 17종). 클립 파일이 물리적으로 동일하기 때문에 중복 서술하지 않음 — 상세 표는 elite 쪽 문서 참조.

차이점은 딱 하나: **본 체형(bone length)이 각자 메쉬에 맞게 다름** — elite는 방탄복 입은 두꺼운 체형, normal은 일반 경찰 체형. 리타겟팅 자체는 정상 작동하지만, 체형 차이 큰 애니메이션(특히 strafe·격렬한 전투 동작)은 Unity에서 발 미끄러짐 여부 한번 육안 확인 권장.

## 중복 후보

elite 문서와 동일 — `strafe`/`strafe (2)`/`strafe left`/`strafe right` 4개가 `left/right strafe`, `left/right strafe walking`과 목적 겹침. elite에서 삭제 결정 나면 normal도 동일하게 적용(같은 클립 파일 공유하는 구조라 한쪽만 정리하면 반대쪽도 똑같이 정리 필요).

## 기술적 확인 필요

- Root Motion: elite와 동일 클립이므로 동일하게 대부분 존재. in-place 베이크 필요.
- 텍스처: diffuse/normal/metallic/roughness 4장 임베드 완료. (elite는 emissive 1장 추가로 있음 — normal 메쉬엔 emissive 텍스처 자체가 없어서 미포함, 정상)
- 정점 수(74992) 리깅본과 텍스처 소스 완전 일치 확인 후 재질 교체함 — 스킨 웨이트 안전.
