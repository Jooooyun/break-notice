# BREAK NOTICE WebGL Release Runbook

## Invariants

- WebGL binaries are deployment-only and ignored under `deployment-staging/webgl/`.
- Every release uses an immutable Build GUID route: `/play/releases/<build-guid>/`.
- `/play/` is the tracked latest pointer. Promote it only after artifact, copy and browser verification.
- Never overwrite or delete an existing GUID directory. Keep earlier Sites versions for rollback.
- Do not run Unity from this workflow. The input must be one exact, already verified build directory.

## Prepare a release

From `Website/`:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass `
  -File .\scripts\prepare-webgl-release.ps1 `
  -ArtifactDirectory 'C:\exact\verified\WebGL\build' `
  -ExpectedBuildGuid '<32-lowercase-hex-guid>' `
  -SourceCommitSha '<40-lowercase-hex-sha>' `
  -PromoteLatest
```

The script validates `BuildMetadata.json`, the recorded inventory, 19-file or
release-specific count, byte totals, required Unity files, per-file SHA-256,
safe relative paths and copy identity. It fails if the immutable destination
already exists.

## Publish

1. Build the tracked Website source.
2. Run the prepare command. Do not stage `deployment-staging/webgl/` in Git.
3. Run the production build. The Sites build plugin copies the ignored staging
   tree into `dist/client/` only. Files above the Sites 25 MiB individual-file
   limit are split into deployment transport parts there; the Worker streams
   them back from the original immutable URL with identical bytes and MIME.
   Sites의 HTML 정규화를 피하기 위해 Unity `index.html`과 바이트가 같은
   deployment-only `__webgl-index.bin`도 만들고, Worker가 release directory
   요청에 그 원본 바이트를 반환한다. 배포 패키지에서는 Sites가 변환할
   `index.html`만 제외한다. 별칭은 source artifact와 Git에 없다.
   공개 HTML 응답에는 Sites/Cloudflare가 보안 검사 스크립트를 덧붙일 수
   있으므로 `index.html`의 원본 바이트 검증은 inventory의 `publicPath`
   (`__webgl-index.bin`)에서 수행한다. 실제 GUID directory는 별도로 HTML
   200, Unity canvas, 브라우저 준비 상태를 검증한다.
   Package and save that Sites version.
4. Verify anonymous HTTPS for all inventory files, MIME, no compression header,
   Chrome/Edge canvas readiness and the exact GUID redirect.
5. Only then treat the tracked `latestBuildGuid` and marketing CTA as promoted.

## Rollback

Redeploy a previously saved Sites version ID. This restores the marketing page,
`/play/` pointer and its matching immutable files atomically. Do not delete the
newer version or mutate either GUID release directory.

## Current promoted build

- Build GUID: `3cc7416d44c44067bc51c6474c46351d`
- Source SHA: `f1fd9b15f407be9003eff622231e338358376bcc`
- Label: `C0-C4 ACCEPTED PROTOTYPE / CHARGED THROW TUNING`
- Excluded: `S1 / S2 / MAP V2 / S3+ / Production Art / premium VFX / Audio v002 / BGM / cinematic`
