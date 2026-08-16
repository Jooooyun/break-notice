# updatedAnimation 배포 노트

CHR00_Player 애니메이션 전체(37개) 재배포. `ArtSource/Characters/CHR00_Player/Animations/`에 있던 기존 애니메이션 전용(스킨 없는) 클립들과 달리, **이 폴더의 fbx는 전부 스킨 메쉬 + Armature + 애니메이션이 한 파일에 통째로 들어있음** — 각 fbx가 완전히 독립적/자급자족, 다른 파일과 Avatar를 맞출 필요 없음.

## Unity Import 권장 설정
- Animation Type: **Generic** 또는 **Legacy** 권장. Humanoid Avatar를 파일 간에 "Copy From Other Avatar"로 맞추는 방식은 뼈 길이 미스매치(Bone length mismatch) 에러가 잘 남 — 이번 작업 중 실제로 재현됨. 각 fbx가 이미 스킨+본을 자체 포함하고 있으므로 Humanoid로 안 묶어도 됨.
- 메쉬가 필요 없고 애니메이션만 쓸 거면, 그냥 해당 fbx의 AnimationClip 서브에셋만 뽑아서 기존 캐릭터 프리팹에 붙이면 됨(메쉬는 무시 가능).

## 모델 상태
- 머티리얼은 있지만 텍스처 이미지는 아직 안 씌워짐(언텍스처드). 스켈레톤/스킨 바인딩은 정상.

## 알려진 미해결 이슈
- `CHR00_Catch_Medium` / `CHR00_Catch_Large` / `CHR00_Throw_Medium` / `CHR00_Throw_Large`: 웨이트 재적용 필요(관절 뒤틀림), 전환 구간 부자연스러움 — **이번 배포에도 그대로 포함**, 아직 수정 안 됨.
- `CHR00_Melee_JabCross_raw`: Mixamo 원본 그대로 통짜 60프레임(Jab+Cross 미분리). Unity Model Importer의 Clips 기능으로 프레임 구간 나눠서 Jab/Cross 두 State로 쓰면 됨. Damage timing marker는 아직 안 찍힘.
- `CHR00_Aim_Idle`: 정지 포즈 placeholder, 정상.
- `PREVIEW_Strafe_plus_Aim_inplace`: 참고용 클립, 게임에 그대로 쓰지 말 것 — `AIM_STRAFE_LAYER_GUIDE.md` 참고해서 Layer 합성으로 대체.

## 속도 조절된 애니메이션
`CHR00_Getup_F`(70프레임), `CHR00_Jump`(21프레임), `CHR00_Knockdown_F`(39프레임), `CHR00_Knockdown_B`(33프레임) — 게임 템포에 맞춰 재생 속도 올림(원본 대비 2~5배).
