# Work Breakdown Structure

> **MIXED-AUTHORITY PLANNING INDEX — NOT A PRODUCTION COMMITMENT**
>
> 이 문서는 작업 분해 후보를 보존하는 planning index다. 각 항목의
> 실제 범위와 실행 권한은 Owner-approved Requirement, G2/G3 계약,
> 현재 Gate와 별도 bounded task가 소유한다. 특히 8절의 정확한 방,
> 적 archetype과 upgrade 수는
> `NON-AUTHORITATIVE G5/G6 DRAFT PRODUCTION CANDIDATES`이며 승인된
> Vertical Slice scope가 아니다. 이 문서는 Pre-G4 Full Design Lock,
> G4/G5 passage 또는 구현 권한을 변경하지 않는다.

## 1. Design Completion

- Canon 문서 검토
- 장비 작동 시트
- 동료 1종 비주얼
- PGT_01~03 게임플레이 목표 이미지
- Camera Spec
- Enemy silhouette
- Design Freeze Checklist

## 2. Project Bootstrap

- Unity Editor/Pkg 버전 잠금
- URP 프로젝트
- Git/asmdef/test 구조
- Input actions
- Prototype scene
- Debug overlay 기반

## 3. Combat Core

- Player movement
- Scoop
- Carry/Throw
- Impact/Stability
- Reset

## 4. Chain Core

- Lineage
- Chain events
- Momentum/Pip
- Debug visualization

## 5. Ally Core

- Relay Position
- Pass inventory
- Light throw
- Medium roll
- Catch/Perfect Catch
- Queued Throw
- Suppressed

## 6. Strap Core

- Anchor
- Attach/release
- Redirect
- Safety limits
- Visual strap

## 7. Destruction

- Breakables
- Structural route change
- Pooling
- Material VFX/audio

## 8. Vertical Slice Content — Non-Authoritative Candidates

- `3 rooms` — `NON-AUTHORITATIVE G5/G6 CANDIDATE`
- `2~3 enemy archetypes` — `NON-AUTHORITATIVE G5/G6 CANDIDATE`
- `12 upgrades` — `SUPERSEDED OR G5 CONTENT CANDIDATE / NOT APPROVED`
- UI/audio/VFX — `G5/G6 DRAFT`
- Tutorialization — `G5/G6 DRAFT`

## 9. Verification

- Automated tests
- Performance profiling
- Playtest
- Bug fixing
- Build packaging
