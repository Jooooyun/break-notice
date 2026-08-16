# BREAK NOTICE WP02 공개 청취 패키지

상태: `REVIEW_CANDIDATE / NOT FINAL / NOT RUNTIME-INTEGRATED`

이 전달물은 정답 매핑을 숨긴 3단계 청취 패키지로 배포한다. 먼저 노트북
스피커와 고정된 OS 볼륨으로 `Docs/REVIEW_CHECKLIST.md` 순서대로 듣고
`LISTENING_RESPONSE_TEMPLATE.md`에 답을 기록한다. 공개 수신자는 정답 파일을
받지 않으며, 답을 잠근 뒤 Owner에게 회신한다. Owner가 저장소의
`Renders/Private/` 키로 점수와 T3 매핑을 대조한 뒤 Phase 3을 공개한다.

## 빠른 순서

1. Phase 1: X/Y MP4 두 개(재생 불가 시 X/Y WAV)만 받고 선택을 잠근다.
2. Phase 2: tier/material trial 두 개만 듣고 20개 답을 잠근다.
3. Phase 1·2 답을 Owner에게 보낸 뒤에만 Phase 3을 연다.
4. Phase 3: 정답·actual-dynamic ladder/영상·T1 stress·11개 cue·55개 stem과
   증거로 deblind와 최종 판정을 한다. 헤드폰 재검사도 이 단계에서 한다.

Phase-1 영상 X/Y는 동일한 960x540, 30 fps 영상 스트림을 사용하며 소리만
다르다. 두 pair는 -20.0 LUFS로 review-match한다. Phase 3의
`Evidence/VIDEO_QA_REVIEW.json`에 동일 영상 스트림 해시와 디코드 결과가 있다.

최종 기록은 `PASS LADDER`, `PASS WITH NOTES`, `REJECT LADDER` 중 하나다.
이 판정도 리뷰 팩 승인일 뿐 Runtime 연결이나 FINAL 승인을 뜻하지 않는다.

## Phase 1·2에서 의도적으로 제외되는 것

- 정답과 X/Y 매핑
- actual-dynamic ladder, T1 stress, 신규 RC 개별본, cue hash/metrics,
  라벨된 composite와 55개 작업용 stem
- Runtime, Mixer, BGM, VFX, 애니메이션, 물리, 카메라, 맵, UI 연결
- 사람 목소리, 숨, 신음
