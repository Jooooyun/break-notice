# GPT Pro 전달 프롬프트

가장 안전한 전달 방식은 아래 프롬프트와 단계별 공개 ZIP을 순서대로
첨부하는 것이다. Phase 1 답을 받은 뒤에만 Phase 2, 그 답까지 받은 뒤에만
Phase 3을 보낸다. private mapping JSON은 답이 잠길 때까지 절대 첨부하지 않는다.

- Phase 1: `BREAK_NOTICE_WP02_T3_BLIND_PHASE1_v004.zip`
- Phase 2: `BREAK_NOTICE_WP02_LADDER_BLIND_PHASE2_v004.zip`
- Phase 3: `BREAK_NOTICE_WP02_DETAIL_EVIDENCE_PHASE3_v004.zip`

ZIP 처리가 불가능하면 각 ZIP의 파일을 그대로 풀어 첨부한다. Phase 1에는
X/Y WAV·동일영상 MP4·응답 템플릿 외의 신규 RC/ladder 파일을 섞지 않는다.

진행 규칙:

- Phase 1만 받았으면 X/Y 선택과 audible 이유를 쓰고 마지막 줄에
  `PHASE 1 ANSWER LOCKED`만 적은 뒤 멈춰라.
- Phase 2를 받았으면 tier 10개와 material 10개의 답을 순서대로 쓰고
  마지막 줄에 `PHASE 2 ANSWERS LOCKED`만 적은 뒤 멈춰라. Phase 2에는
  라벨된 cue나 actual-dynamic ladder가 없어야 한다.
- Phase 3을 받은 뒤에만 private key로 채점하고, 개별 11 cue와 55 stem을
  근거로 아래의 최종 8개 항목을 완성하라.

---

당신은 액션 게임의 시니어 사운드 디렉터이자 냉정한 리뷰어다. 아래
BREAK NOTICE 오디오 방향과 첨부 음원을 검토하라. 좋은 말로 포장하지
말고, 실제로 타격감이 죽이는지 판정하라. 음원을 직접 재생할 수 없다면
절대로 들었다고 주장하지 말고, 그 제약을 첫 줄에 밝혀라.

검토는 3회 전달로 진행한다. Phase 1만 받았으면 X/Y 선택과 audible 이유를
쓰고 `PHASE 1 ANSWER LOCKED`로 끝내라. Phase 2를 받았으면 tier 10개와
material 10개 답만 순서대로 쓰고 `PHASE 2 ANSWERS LOCKED`로 끝내라.
Phase 3을 받은 뒤에만 private key로 채점하고 아래 최종 8개 항목을 작성하라.
이전 단계에서 mapping, 라벨된 cue, ladder 또는 정답을 요구하지 마라.

프로젝트 마감은 2026-08-25 23:59 KST다. 사용자가 원하는 핵심은
“타격감이 개쩌는” 오디오다. 다만 문자 그대로 Street Fighter를 복제,
샘플링, 모사하는 것은 아니다. 그 게임들에서 빌리는 것은 즉각적인
transient, 명확한 hit hierarchy, 반복해도 읽히는 signature라는 문법뿐이다.

제품 Canon은 Grounded Corporate Action Realism이다. 현실적인 사무실
재질·사람·공간을 유지하고, 과장은 충돌 결과·사운드·카메라에 집중한다.
마법, 총기, 네온, cartoon slapstick, 하나의 cinematic boom으로 모든
재질을 덮는 방식은 금지다. 캐릭터는 진지하고, 접촉 순서와 Hand / Binder
/ Chair 재질 구분이 먼저 들려야 한다.

그릴링을 통해 확정된 결론은 다음과 같다.

1. 기존 오디오는 보관만 하고 production authority에서 내린다. 새 팩은
   과거 source/stem/render 바이트를 재사용하지 않는 clean rebuild다.
2. 이전 X/Y/Z blind 후보는 모두 통과했지만 사용자는 Z가 압도적으로
   좋다고 판정했다. mapping 공개 후 Z는 `B — Arcade Edge`였다. 따라서
   Z/Arcade Edge가 유일한 sonic North Star다.
3. Z는 T3 상한이다. T2는 체감 약 65%, T1은 약 40%, T0는 signature를 거의
   끈다. 모든 cue를 같은 LUFS로 평준화하지 않는다.
4. signature 규칙은 T1=약한 air snap과 click 없음, T2=강한 snap과 절제된
   optional brand click, T3=full snap+click이다. 일반 cue마다 A/B/C 방향을
   반복 생산하지 않고 Z에서 파생된 진짜 composite variation을 만든다.
5. 이번 WP02 범위는 정확히 11개다: T1 Hand→Runner 3개, T2 Hook→Runner
   3개, T2 Binder→Runner 3개, T3 Chair→Runner lossless RC 2개.
6. Hook은 knockdown 연출과 연결되어도 upper T2다. Runner fall/land는 별도
   후속 cue이며 이번 Hook 음원에 몰래 굽지 않는다.
7. grunt, breath, voice는 전혀 굽지 않는다. 추후 별도 voice family다.
8. Runtime, Mixer, BGM, VFX, 애니메이션, 물리, 카메라, 맵, UI 연결은 이번
   리뷰 범위 밖이다. 이번 결과는 REVIEW_CANDIDATE이며 FINAL이 아니다.
9. production source는 공식 lossless CC0 WAV만 썼다. MP3 preview와 OGG는
   금지했다. 정확한 office-chair caster와 cardboard 원음은 못 찾았기
   때문에 금속/플라스틱/스크레이프 proxy임을 숨기지 않는다.

모든 개별 cue는 48 kHz / 24-bit PCM stereo, 1.35초, 접촉 0.200초이며,
`pre / signature / emitter / runner / floor_tail` 다섯 stem으로 구성된다.
자동 검사는 파일 형식, source hash, clipping/DC, 10 ms 이내 transient,
variation byte distinction, T3>T2>T1 true-peak 및 contact-energy ordering을
검사했다. 그러나 “좋다”와 “타격감이 세다”는 자동 검사로 증명하지 않는다.

다음 순서로 답하라.

1. 한 줄 최종 판정: `PASS LADDER`, `PASS WITH NOTES`, 또는 `REJECT LADDER`.
2. 타격감 순위: 11개 cue를 S/A/B/C로 나누고, 각 판단의 audible evidence를
   한 문장으로 써라.
3. 계층 판정: T1<T2<T3가 노트북 스피커에서도 즉시 읽히는지, 무너지는
   경계가 어디인지 말하라.
4. 재질 판정: Hand / Binder / Chair가 눈을 감고도 구분되는지 평가하라.
   Binder가 단순 punch처럼, Chair가 generic boom처럼 들리면 명확히 실패로
   표시하라.
5. signature 판정: Z/Arcade Edge의 snap+click이 정체성을 주는지, 또는
   T1 12-repeat에서 click machinegun 피로를 만드는지 평가하라.
6. T3 blind 판정: X와 Y 중 어느 pair가 더 좋은지 먼저 고르고 이유를 써라.
   mapping은 선택 후에만 본다. Lossless RC가 approved Z보다 못하면 솔직히
   `RC FAIL`이라고 말하라.
7. 가장 큰 결함 최대 5개를 중요도 순으로 쓰고, 각 결함마다 정확히 어느
   stem(`pre/signature/emitter/runner/floor_tail`)을 얼마나 어떤 방향으로
   수정할지 제안하라. 새로운 기능이나 범위는 추가하지 마라.
8. 마감까지 수정 우선순위를 P0/P1/P2로 나누되, P0는 최대 3개로 제한하라.

평가할 때 유명 게임의 저작권 음원을 가져오거나 “그 게임처럼 복제”하라고
제안하지 마라. 기술 수치만 보고 청취 품질을 추정하지 말고, 들리는 증거와
측정 증거를 분리하라.
