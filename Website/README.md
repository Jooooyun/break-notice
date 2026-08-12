# BREAK NOTICE Showcase Release Site

BREAK NOTICE의 공개 Showcase 웹사이트다. 이 디렉터리는 게임 저장소와
분리된 독립 배포 표면이며 `Website/**` 밖의 파일을 배포하지 않는다.

## Local validation

Requirements: Node.js 22.13.0 이상.

```text
npm ci
npm run lint
npm test
npm run dev
```

개발 서버가 출력하는 HTTP URL로 접속한다. `file://` 실행은 지원하지 않는다.

## Release boundary

- 공개 WebGL URL이 검증되기 전까지 Play CTA는 비활성이다.
- 이 사이트는 Unity build 또는 `Game/**` 산출물을 포함하지 않는다.
- 공개 미디어는 `MEDIA_PROVENANCE.md`에 기록한 Owner-approved reference만 사용한다.
- analytics, advertising, tracking, account, cookie 또는 외부 API가 없다.
- Sites 프로젝트 ID 외의 credential은 파일이나 Git 설정에 저장하지 않는다.

## Rollback and redeploy

Sites에서 직전 성공 version을 선택해 동일 production target으로 재배포하면
source history를 되감지 않고 rollback할 수 있다. 수정 재배포는 새 one-purpose
commit, 새 build archive와 새 saved version을 사용한다. 공개 production URL은
같은 site slug를 유지한다.
