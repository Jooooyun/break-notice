# Unity Humanoid 결합 가이드

## 파일 구성

`updatedAnimation/CHR00/`의 각 FBX는 다음을 함께 담은 독립 파일이다.

- 스킨 mesh
- `CHR00_Armature`
- 파일 이름과 같은 단일 Animation Clip

게임에서는 이 FBX의 mesh를 새 캐릭터로 배치할 필요가 없다. FBX 안의 Animation Clip만 기존 CHR00의 Animator Controller에 넣으면 Unity Humanoid가 현재 캐릭터 Avatar에 리타게팅한다.

## Unity import

1. FBX를 Unity 프로젝트의 `Assets` 아래로 복사한다.
2. Project 창에서 FBX를 선택하고 Inspector의 **Rig** 탭을 연다.
3. **Animation Type**을 `Humanoid`로 설정한다.
4. **Avatar Definition**은 `Create From This Model`로 설정하고 **Apply**를 누른다.
5. `Configure...`에서 필수 bone mapping이 초록색인지 확인한다. 문제가 있으면 `Enforce T-Pose` 후 다시 확인하고 **Done**을 누른다.
6. **Animation** 탭에서 Clip 이름과 frame 범위를 확인하고 **Apply**를 누른다.

각 FBX가 Armature를 포함하므로 기본값은 `Create From This Model`이 가장 안전하다. `Copy From Other Avatar`는 원본과 대상의 skeleton hierarchy 및 bind pose가 동일하고 Unity의 Avatar 검증이 통과할 때만 사용한다.

## 기존 CHR00에 연결

1. 실제 게임 캐릭터 모델도 **Rig > Animation Type = Humanoid**로 import한다.
2. 게임 캐릭터의 `Animator`에 그 캐릭터 모델에서 생성된 Avatar를 지정한다.
3. FBX를 펼쳐 보이는 Animation Clip을 기존 Animator Controller의 state 또는 Blend Tree에 넣는다.
4. `Knockdown_F` 뒤에는 `Getup_F`/`RollGetup_F_*`, `Knockdown_B` 뒤에는 `Getup_B`/`RollGetup_B_*`를 조건에 따라 연결한다.
5. 빠른 기상 clip은 종료 시 Idle 또는 locomotion state로 transition한다. 짧은 transition duration으로 시작해 발 미끄러짐과 자세 튐을 보며 조절한다.

서로 다른 FBX의 Armature나 mesh를 하나로 합칠 필요는 없다. Animator가 재생하는 대상은 게임 캐릭터 한 개이고, Animation Clip 데이터만 공유한다.

## Loop와 Root Motion

- `CHR00_Idle`처럼 반복 재생할 동작만 **Loop Time**을 켠다.
- Knockdown, Getup, RollGetup, Strap Attack처럼 한 번 재생하는 동작은 **Loop Time**을 끈다.
- 캐릭터 이동을 `CharacterController`, Rigidbody 또는 게임 코드가 담당하면 `Animator > Apply Root Motion`을 끈다.
- animation 자체의 이동량을 사용할 때만 `Apply Root Motion`을 켠다. 이 경우 **Root Transform Rotation / Position (Y) / Position (XZ)**의 `Bake Into Pose` 설정을 clip별로 확인한다.
- Getup 계열은 첫/마지막 frame에서 실제 게임 캐릭터의 위치와 방향이 맞는지 Scene 뷰에서 반드시 확인한다. 필요하면 transition 전 캐릭터 방향을 gameplay code에서 맞춘다.

## 이번 재추출분

원본은 `CHR00_FallingToRoll.blend`, export는 Blender 5.2 LTS / 30 FPS / `-Z Forward, Y Up` / leaf bone 제외 / frame당 bake 방식이다.

기존 파일을 현재 Blender 버전으로 교체한 Action:

- `CHR00_Getup_B`
- `CHR00_Getup_F`
- `CHR00_Idle`
- `CHR00_Knockdown_B`
- `CHR00_Knockdown_F`
- `CHR00_Strap_Pull`

새로 추가한 Action:

- `CHR00_RollGetup_B_Front`
- `CHR00_RollGetup_B_Side`
- `CHR00_RollGetup_F_Front`
- `CHR00_RollGetup_F_Side`
- `CHR00_Strap_Attack_F`

전체 frame 범위는 `CHR00/ANIMATION_LIST.md`에 기록돼 있다.
