# Codex Collaboration Log

추측한 시간·결과를 기록하지 않는다. 각 work package는 실제 evidence와
commit SHA가 확보된 뒤 한 행을 닫는다.

## C0 — Contest Transition and Browser Qualification

- Date: `2026-08-06`
- Goal: contest branch, tags, authority, visual/art pipeline, separate scene,
  exact Web build pipeline과 Chrome/Edge qualification
- Human Owner Decisions:
  - `CR-0003`, `CONTEST-0001`, `EAR-CONTEST-0001-001` 직접 승인
  - 기존 26개 untracked 항목 식별·보호
  - 승인 reference를 production brief와 review에 사용
  - exact `6000.3.20f1` Web Build Support 설치 허용
- Codex Work:
  - local/remote SHA와 protected work 감사
  - 23개 REVIEW 이미지의 SHA-256·dimension·approval mapping 확인
  - contest authority, docs, art/runtime scaffolding과 local automation 작성
  - separate Contest Scene, tests, Web build와 browser evidence 확보
- Problems Found:
  - exact Editor의 Web Build Support 누락
  - Editor가 Hub-managed installation이 아니어서 Hub CLI module install 거부
  - remote push는 실행 환경의 별도 안전 승인 필요
  - Chrome와 Edge 모두 ChatGPT browser extension이 설치되지 않았고
    shared native-host registry registration도 없음
- Applied Resolution:
  - Unity 공식 6000.3.20f1 component installer의 Authenticode와 SHA-256을
    검증한 후 exact Editor root에 Web Build Support만 설치
  - protected untracked 파일은 명시 경로 stage만으로 제외
  - exact browser family를 다른 브라우저로 대체하지 않고 C1 진입 차단
- Tests: `EditMode 23/23 PASS / PlayMode 4/4 PASS / total 27/27`
- Manual Validation:
  `Web build PASS / warnings 0 / errors 0 / HTTP 19/19 PASS`
- Browser Validation:
  `BLOCKED / exact Chrome and Edge extension connections unavailable / Move,
  Look, Aim, pointer capture, Escape, resize and browser console not executed`
- Commit SHA: `PENDING`
- Actual Result: `C0 ENVIRONMENT BLOCKED / C1 NOT STARTED`
- Actual Work Time: `NOT RECORDED`
- Discarded Approach:
  - Hub CLI `install-modules` — Hub-managed Editor가 아니라 사용 불가
  - untracked REVIEW 원본 재등록 — tracked approved counterpart가 이미 존재
- Next: `install/reinstall browser extension, exact Chrome/Edge qualification,
  explicit remote-push approval, then C0 closure`

## C0 Recovery Attempt and Remote Publication — 2026-08-06

- Human Owner Decisions:
  - Computer Use browser extension installation completed
  - normal remote push explicitly approved
- Browser Diagnostics:
  - Chrome extension `1.2.27236.6274`: `INSTALLED / ENABLED`
  - Chrome native host: `VALID`
  - Chrome runtime: `NOT RUNNING / EXACT FAMILY UNAVAILABLE`
  - Edge extension `1.2.27236.6274`: `INSTALLED / ENABLED / CONNECTED`
  - Edge native host: `VALID`
  - Edge local HTTP navigation: `BLOCKED / ADMIN-ENFORCED POLICY COULD NOT BE VERIFIED / SAME-URL RETRY FAILED`
- Remote Publication:
  - contest and both art branches: `PUSHED / VERIFIED / 73d886eb2b6ccc0061e6f484afa663402418ec59`
  - `gb2026-prechallenge`: `PUSHED / TARGET b82bb382fc0439847d6497ee2fadd9c55781a9fa`
  - `gb2026-sprint-start`: `PUSHED / TARGET 28b7f70c4b6299159afe5b99ea7b98d1e5399352`
  - `origin/main`: `UNCHANGED / 28b7f70c4b6299159afe5b99ea7b98d1e5399352`
- Actual Result: `REMOTE PUSH COMPLETE / C0 BROWSER ACCEPTANCE STILL BLOCKED / C1 NOT STARTED`
- Next: `launch Chrome, restore Edge local-URL policy verification, rerun exact Chrome and Edge qualification`
