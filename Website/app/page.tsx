const controls = [
  { key: "W A S D", ko: "이동", en: "MOVE", detail: "작업 구역을 이동합니다." },
  { key: "MOUSE", ko: "시점", en: "LOOK", detail: "3인칭 카메라를 조작합니다." },
  { key: "RMB", ko: "조준", en: "AIM", detail: "우클릭을 누르는 동안 조준합니다." },
  { key: "E", ko: "습득", en: "PICKUP", detail: "가까운 Binder 또는 Chair를 집습니다." },
  { key: "HOLD LMB", ko: "차지 던지기", en: "CHARGE THROW", detail: "누르고 충전한 뒤 놓아 던집니다." },
  { key: "Q", ko: "동료 공급", en: "Q SUPPLY", detail: "준비된 동료에게 실제 물체 공급을 요청합니다." },
  { key: "BACKSPACE", ko: "리셋", en: "RESET", detail: "작성된 시작 상태로 Encounter를 복구합니다." },
];

const statusRows = [
  {
    label: "C0–C4 FOUNDATION",
    state: "IMPLEMENTED / ACCEPTED",
    tone: "good",
    detail: "이동, 카메라, 상호작용, Binder·Chair 습득·운반·던지기, Reset, 동료 물리 공급.",
  },
  {
    label: "THROW / PICKUP TUNING",
    state: "IMPLEMENTED / VERIFIED",
    tone: "good",
    detail: "LMB 차지 던지기, 바닥 물체 습득 보조, 동료 Binder 공급 속도 변주.",
  },
  {
    label: "OPERATIONS FLOOR S0",
    state: "DESIGN AUTHORITY REGISTERED",
    tone: "neutral",
    detail: "Showcase 설계 문서만 등록됨. S1/S2/MAP V2는 이 공개 빌드에 포함되지 않았습니다.",
  },
  {
    label: "C5 / CARGO STRAP",
    state: "NOT AUTHORIZED",
    tone: "warn",
    detail: "현재 공개 설명에서 구현 완료 기능으로 취급하지 않습니다.",
  },
  {
    label: "PUBLIC WEBGL",
    state: "LIVE / VERSIONED",
    tone: "good",
    detail: "검증된 C0–C4 accepted prototype WebGL을 immutable Build GUID 경로로 공개합니다.",
  },
  {
    label: "PRODUCTION ART",
    state: "NOT INTEGRATED",
    tone: "neutral",
    detail: "이 사이트의 이미지는 승인된 AI 생성 프로덕션 비주얼 레퍼런스이며 런타임 캡처가 아닙니다.",
  },
];

const loopSteps = [
  {
    number: "01",
    en: "READ THE FLOOR",
    ko: "공간을 읽는다",
    copy: "이동·시점·조준으로 바닥의 Binder와 Chair, 벽과 공급 경로를 확인합니다.",
  },
  {
    number: "02",
    en: "PICK A TOOL",
    ko: "사무용 물체를 든다",
    copy: "E로 Light Binder 또는 Medium Chair를 실제 공간에서 습득하고 운반합니다.",
  },
  {
    number: "03",
    en: "COMMIT THE THROW",
    ko: "차지하고 던진다",
    copy: "LMB를 누르는 시간으로 투척을 충전한 뒤 놓습니다. 물체는 실제 궤적과 충돌을 유지합니다.",
  },
  {
    number: "04",
    en: "CALL / CATCH / RESET",
    ko: "동료와 다시 시도한다",
    copy: "Q로 준비된 동료 공급을 요청하고 Binder를 Catch하거나 Chair Feed를 받은 뒤, 필요하면 Reset합니다.",
  },
];

export default function Home() {
  return (
    <>
      <a className="skip-link" href="#main-content">
        본문으로 건너뛰기
      </a>

      <header className="site-header">
        <a className="wordmark" href="#top" aria-label="BREAK NOTICE 홈">
          <span className="wordmark-mark" aria-hidden="true">BN</span>
          <span>
            BREAK NOTICE
            <small>ZERO DEFECT / SHOWCASE</small>
          </span>
        </a>

        <nav className="site-nav" aria-label="주요 탐색">
          <a href="#core-loop">Core Loop</a>
          <a href="#controls">Controls</a>
          <a href="#media">Media</a>
          <a href="#status">Status</a>
          <a href="#credits">Credits</a>
        </nav>

        <a className="header-status" href="/play/">
          <span aria-hidden="true" />
          PLAY WEBGL
        </a>
      </header>

      <main id="main-content">
        <section className="hero" id="top" aria-labelledby="hero-title">
          <div className="hero-copy">
            <p className="eyebrow">THIRD-PERSON PHYSICS ACTION PROTOTYPE</p>
            <h1 id="hero-title">
              BREAK
              <span>NOTICE</span>
            </h1>
            <p className="hero-lede">
              야근 중인 시설 직원이 평범한 사무용 물체를 들고, 던지고,
              한 명의 동료와 물리적으로 협업하는 3인칭 액션 프로토타입.
            </p>
            <p className="hero-tagline">GROUNDED WORLD. ABSURD PHYSICS.</p>

            <div className="hero-actions" id="play">
              <a className="play-button" href="/play/" aria-describedby="play-status">
                <span>PLAY NOW</span>
                <small>C0–C4 ACCEPTED PROTOTYPE</small>
              </a>
              <a className="secondary-action" href="#core-loop">
                구현된 루프 보기 <span aria-hidden="true">↓</span>
              </a>
            </div>
            <p className="play-status" id="play-status">
              <strong>C0–C4 ACCEPTED PROTOTYPE / CHARGED THROW TUNING</strong><br />
              전용 전체 화면 WebGL 경로를 엽니다. S1/S2/MAP V2 NOT IN THIS BUILD.
            </p>
          </div>

          <figure className="hero-media">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src="/media/PGT_01_Gameplay_Camera_v002_FINAL.png"
              width="1672"
              height="941"
              alt="사무실 안에서 플레이어와 동료, 사무용 물체가 보이는 승인된 3인칭 카메라 비주얼 레퍼런스"
              fetchPriority="high"
              decoding="async"
            />
            <div className="hero-media-overlay" aria-hidden="true">
              <span>CAM_01</span>
              <span>OTS / THIRD PERSON</span>
              <span>REFERENCE ONLY</span>
            </div>
            <figcaption>
              OWNER-APPROVED AI-GENERATED PRODUCTION VISUAL REFERENCE — 실제 런타임 캡처 아님
            </figcaption>
          </figure>
        </section>

        <section className="evidence-strip" aria-label="검증 상태 요약">
          <div>
            <span className="evidence-value">124 / 124</span>
            <span className="evidence-label">LATEST AUTOMATED TESTS</span>
          </div>
          <div>
            <span className="evidence-value">0 / 0</span>
            <span className="evidence-label">UNITY WARNINGS / ERRORS</span>
          </div>
          <div>
            <span className="evidence-value">C0–C4</span>
            <span className="evidence-label">HUMAN-ACCEPTED FOUNDATION</span>
          </div>
          <div>
            <span className="evidence-value">S1 / C5</span>
            <span className="evidence-label">NOT AUTHORIZED</span>
          </div>
        </section>

        <section className="section-shell intro-section" aria-labelledby="experience-title">
          <div className="section-index" aria-hidden="true">00 / EXPERIENCE</div>
          <div className="intro-heading">
            <p className="eyebrow">THE CURRENT PLAYABLE FOUNDATION</p>
            <h2 id="experience-title">일은 끝나지 않았다.<br />사무실만 움직이기 시작했다.</h2>
          </div>
          <div className="intro-copy">
            <p>
              BREAK NOTICE는 초능력이나 총기 대신 Binder와 Wheeled Office Chair를
              물리 도구로 사용합니다. 습득, 운반, 투척, 동료 공급과 Reset이 현재 검증된 루프입니다.
            </p>
            <p>
              Operations Floor Showcase의 확장 설계는 등록됐지만 구현되지 않았습니다.
              이 페이지는 C5 이후 기능, 파괴, 완성된 Cargo Strap 또는 최종 제품을 약속하지 않습니다.
            </p>
          </div>
        </section>

        <section className="section-shell loop-section" id="core-loop" aria-labelledby="loop-title">
          <div className="section-heading">
            <div>
              <p className="eyebrow">01 / CORE LOOP</p>
              <h2 id="loop-title">지금 구현된 플레이</h2>
            </div>
            <p className="section-summary">
              C0–C4 accepted foundation과 2026-08-11 post-acceptance tuning만 설명합니다.
            </p>
          </div>

          <ol className="loop-grid">
            {loopSteps.map((step) => (
              <li key={step.number}>
                <span className="loop-number">{step.number}</span>
                <span className="loop-en">{step.en}</span>
                <h3>{step.ko}</h3>
                <p>{step.copy}</p>
              </li>
            ))}
          </ol>
        </section>

        <section className="controls-section" id="controls" aria-labelledby="controls-title">
          <div className="section-shell">
            <div className="section-heading controls-heading">
              <div>
                <p className="eyebrow">02 / CONTROLS</p>
                <h2 id="controls-title">키보드와 마우스</h2>
              </div>
              <p className="section-summary">
                Desktop browser baseline. 브라우저 포인터 해제는 <kbd>ESC</kbd>입니다.
              </p>
            </div>

            <dl className="control-grid">
              {controls.map((control) => (
                <div className="control-card" key={control.en}>
                  <dt>
                    <kbd>{control.key}</kbd>
                    <span>{control.en}</span>
                  </dt>
                  <dd>
                    <strong>{control.ko}</strong>
                    <span>{control.detail}</span>
                  </dd>
                </div>
              ))}
            </dl>
          </div>
        </section>

        <section className="section-shell media-section" id="media" aria-labelledby="media-title">
          <div className="section-heading">
            <div>
              <p className="eyebrow">03 / GAMEPLAY MEDIA</p>
              <h2 id="media-title">증거 없는 장면은 재생하지 않는다</h2>
            </div>
            <p className="section-summary">
              공개 가능한 tracked runtime capture가 준비될 때 이 영역이 실제 영상으로 교체됩니다.
            </p>
          </div>

          <div className="runtime-media-grid">
            <div className="runtime-slot" aria-label="실제 런타임 캡처 준비 상태">
              <div className="runtime-slot-topline">
                <span>RUNTIME CAPTURE / SLOT A</span>
                <span>00:00:00</span>
              </div>
              <div className="runtime-slot-center">
                <span className="crosshair" aria-hidden="true" />
                <p>RUNTIME MEDIA<br /><strong>PREPARING</strong></p>
                <small>NO PLACEHOLDER FOOTAGE USED</small>
              </div>
              <div className="runtime-slot-footer">
                <span>SOURCE: NOT YET PUBLIC</span>
                <span>STATUS: HONEST HOLD</span>
              </div>
            </div>

            <aside className="media-evidence" aria-labelledby="media-evidence-title">
              <p className="eyebrow" id="media-evidence-title">VERIFIED INSTEAD</p>
              <ul>
                <li><strong>65 / 65</strong><span>EditMode tests</span></li>
                <li><strong>59 / 59</strong><span>PlayMode tests</span></li>
                <li><strong>19 / 19</strong><span>HTTP build files</span></li>
              </ul>
              <p>
                최근 Development Web build와 Human Owner 수동 체크리스트는 통과했습니다.
                그러나 이 권한은 기존 build 산출물 공개나 신규 WebGL 생성을 포함하지 않습니다.
              </p>
            </aside>
          </div>
        </section>

        <section className="trailer-section" aria-labelledby="trailer-title">
          <figure className="trailer-visual">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src="/media/LGT_01_Night_Archive_Lighting_v001_FINAL.png"
              width="1672"
              height="941"
              alt="차가운 형광등 아래 기록 보관실을 보여주는 승인된 조명 비주얼 레퍼런스"
              loading="lazy"
              decoding="async"
            />
            <div className="trailer-overlay">
              <p className="eyebrow">04 / TRAILER &amp; CINEMATIC</p>
              <h2 id="trailer-title">TRAILER<br />NOT RELEASED</h2>
              <p>
                공개 Trailer나 Cinematic 영상은 아직 없습니다. 가짜 재생 버튼이나 미공개 영상 링크를 제공하지 않습니다.
              </p>
            </div>
            <figcaption>
              LGT_01 승인 AI 생성 조명 비주얼 레퍼런스 — 실제 Trailer 프레임·런타임 캡처 아님
            </figcaption>
          </figure>
        </section>

        <section className="section-shell status-section" id="status" aria-labelledby="status-title">
          <div className="section-heading">
            <div>
              <p className="eyebrow">05 / DEVELOPMENT STATUS</p>
              <h2 id="status-title">무엇이 됐고, 무엇이 아직 아닌가</h2>
            </div>
            <p className="section-summary">STATUS SNAPSHOT / 2026.08.12 KST</p>
          </div>

          <div className="status-table" role="list">
            {statusRows.map((row) => (
              <article className="status-row" role="listitem" key={row.label}>
                <h3>{row.label}</h3>
                <p className={`status-chip status-${row.tone}`}>{row.state}</p>
                <p>{row.detail}</p>
              </article>
            ))}
          </div>
        </section>

        <section className="credits-section" id="credits" aria-labelledby="credits-title">
          <div className="section-shell">
            <div className="section-heading">
              <div>
                <p className="eyebrow">06 / TEAM &amp; CREDITS</p>
                <h2 id="credits-title">한 명의 책임, 명확한 도움</h2>
              </div>
              <p className="section-summary">숨겨진 기여자 없이 실제 역할만 기록합니다.</p>
            </div>

            <div className="credits-grid">
              <article className="credit-card credit-owner">
                <div className="portrait-frame">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src="/media/CHR_00_Player_Identity_Lock_v002_FINAL.png"
                    width="916"
                    height="1717"
                    alt="BREAK NOTICE 플레이어 캐릭터 CHR_00 승인 비주얼 레퍼런스"
                    loading="lazy"
                    decoding="async"
                  />
                </div>
                <div>
                  <span className="credit-role">HUMAN OWNER / PROJECT LEAD</span>
                  <h3>조윤성 · JO YOONSEONG</h3>
                  <p>Product Owner, Game Design, Tech Lead, Unity Integration, Scope Control, Final QA.</p>
                </div>
              </article>

              <article className="credit-card">
                <div className="portrait-frame portrait-ally">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src="/media/CHR_01_First_Ally_Identity_v001_FINAL.png"
                    width="916"
                    height="1716"
                    alt="BREAK NOTICE 동료 캐릭터 CHR_01 승인 비주얼 레퍼런스"
                    loading="lazy"
                    decoding="async"
                  />
                </div>
                <div>
                  <span className="credit-role">TECHNICAL ASSISTANCE</span>
                  <h3>OPENAI CODEX</h3>
                  <p>Repository analysis, implementation, test generation, build and browser verification assistance.</p>
                </div>
              </article>
            </div>

            <p className="credits-note">
              현재 공개 기준은 Codex를 활용한 1인 Human 프로젝트입니다. Designer 후보 전달물은 기술·권리 검토와 Owner 승인 전이며 이 사이트에 통합하지 않았습니다.
            </p>
          </div>
        </section>

        <section className="notice-section" aria-labelledby="notice-title">
          <div className="section-shell notice-grid">
            <div className="notice-heading">
              <p className="eyebrow">07 / RELEASE NOTICE</p>
              <h2 id="notice-title">접속 전 확인</h2>
            </div>

            <article>
              <h3>BROWSER / SYSTEM</h3>
              <p>
                이 웹사이트는 모바일과 최신 브라우저에 대응합니다. 공개 WebGL 게임은 Desktop Chrome·Edge,
                키보드·마우스를 우선 지원하며 모바일 gameplay는 검증되지 않았습니다. 마지막 승인 browser baseline은
                1366×768과 1920×1080입니다.
              </p>
            </article>

            <article>
              <h3>PRIVACY</h3>
              <p>
                계정, 쿠키 배너, 분석 도구, 광고, 사용자 추적을 사용하지 않습니다. 이 정적 Showcase 사이트는 의도적으로 개인정보를 수집하지 않습니다.
              </p>
            </article>

            <article>
              <h3>MEDIA DISCLOSURE</h3>
              <p>
                표시된 캐릭터·환경 이미지는 Human Owner가 승인한 AI 생성 프로덕션 비주얼 레퍼런스입니다.
                실제 gameplay 캡처, 최종 3D asset 또는 구현 완료 증거가 아닙니다. 이 사이트를 위해 새 AI 이미지를 생성하지 않았습니다.
              </p>
            </article>

            <article>
              <h3>LICENSE</h3>
              <p>
                © 2026 Project BREAK NOTICE. All rights reserved. 명시적 서면 허가 없는 복제, 배포,
                재라이선스, 파생 프로젝트 및 모델 학습 사용을 금지합니다.
              </p>
            </article>
          </div>
        </section>
      </main>

      <footer className="site-footer">
        <a className="footer-wordmark" href="#top">BREAK NOTICE</a>
        <p>ZERO DEFECT / PUBLIC SHOWCASE RELEASE SITE</p>
        <p>WEBGL: <strong>LIVE / 3CC7416D</strong></p>
      </footer>
    </>
  );
}
