# Aim + Strafe 합성 지시서 (Unity 측 작업)

## 결론
`CHR00_Strafe_L` / `CHR00_Strafe_R`(하체) + `CHR00_Aim_Idle`(상체)을 지금 있는 애셋만으로 Unity Animator Layer + Avatar Mask 조합으로 완성 가능함. Blender에서 새 애니메이션 만들 필요 없음.

(참고: "물체를 들고 있는 상태로 Strafe"는 이 방식으로 안 됨 — 상반신 전용 캐리 루프 클립 자체가 아직 없어서, 이건 Blender 쪽에 남은 작업임. 이 문서는 Aim 조합에만 해당.)

## 방법
1. **Base Layer** (weight 1.0, Full Body Avatar Mask)
   - `CHR00_Strafe_L`, `CHR00_Strafe_R` 재생 (필요하면 `CHR00_Walk_F`, `CHR00_Run_F` 등 기존 로코모션도 같은 Blend Tree/State Machine에 포함)
2. **Aim Layer** (조준 중일 때만 weight 1.0, Avatar Mask로 하체 제외)
   - State: `CHR00_Aim_Idle` 하나만
   - Blending: Override (Additive 아님) — 상체 부분만 Base Layer 위에 덮어씀
3. **Avatar Mask**
   - Body/Legs(왼다리·오른다리) 체크 해제 — 하체 제외
   - Spine/Chest/Arms/Head 체크 — 상체 포함
   - 이 마스크 하나 새로 만들어서 Aim Layer에 물리면 됨

## 왜 새 애니메이션 없이 되는가
`CHR00_Aim_Idle`은 정지 포즈(placeholder, Throw windup 프레임 하나 고정)라 하체 움직임이 아예 없음. 그래서 Strafe_L/R의 하체 움직임과 겹쳐도 상체만 override하면 자연스럽게 "조준하면서 옆걸음" 결과가 나옴.

방향별 Aim 클립도 필요 없음 — Unity **Animation Rigging 패키지의 Multi-Aim Constraint**를 Aim Layer 위에 추가로 얹으면 실시간 조준 방향 반응까지 커버됨(이것도 클립 추가 제작 불필요).

## 참고
`CHR00_Carry_Small_UpperBody_Loop`(상반신 전용 캐리 루프)이 나중에 완성되면 동일한 원리로 Aim Layer 자리에 그 클립을 넣어서 "들고 걷기/뛰기"에도 재사용 가능함. 지금은 미제작 상태.
