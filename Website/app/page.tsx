const controlGroups = [
  {
    label: "MOVE",
    title: "공간을 장악한다",
    items: [
      { key: "WASD", action: "이동" },
      { key: "MOUSE", action: "시점" },
      { key: "RMB", action: "조준" },
    ],
  },
  {
    label: "HANDLE",
    title: "물체를 무기로 바꾼다",
    items: [
      { key: "E", action: "습득 / Binder 캐치" },
      { key: "HOLD LMB", action: "차지 후 릴리스 투척" },
    ],
  },
  {
    label: "COMMAND",
    title: "동료에게 일을 맡긴다",
    items: [
      { key: "F", action: "가장 가까운 물체 자동 Fetch" },
      { key: "Q", action: "Follow / Hold / Supply" },
      { key: "G", action: "Regroup" },
    ],
  },
  {
    label: "RECOVER",
    title: "즉시 다시 시작한다",
    items: [
      { key: "BACKSPACE", action: "Encounter Reset" },
      { key: "ESC", action: "브라우저 포인터 해제" },
    ],
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
          <a href="#showcase">Showcase</a>
          <a href="#controls">Controls</a>
          <a href="#visual">Visual</a>
          <a href="#status">Status</a>
          <a href="#credits">Credits</a>
        </nav>

        <a className="header-status" href="/play/">
          <span aria-hidden="true" />
          PLAY SHOWCASE
        </a>
      </header>

      <main id="main-content">
        <section className="hero" id="top" aria-labelledby="hero-title">
          <div className="hero-copy">
            <p className="eyebrow">A THIRD-PERSON OFFICE PHYSICS SHOWCASE</p>
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
                <small>DESKTOP / FULLSCREEN</small>
              </a>
              <a className="secondary-action" href="#showcase">
                SHOWCASE CUT <span aria-hidden="true">↓</span>
              </a>
            </div>
            <p className="play-status" id="play-status">
              <strong>OPERATIONS FLOOR / KEYBOARD + MOUSE</strong><br />
              전용 전체 화면 브라우저 빌드를 엽니다.
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

        <section className="showcase-statement" id="showcase" aria-labelledby="showcase-title">
          <div className="showcase-statement-inner">
            <div className="showcase-kicker">
              <span>THE SHOWCASE CUT</span>
              <span>ONE FLOOR / ONE ALLY / PHYSICS FIRST</span>
            </div>
            <h2 id="showcase-title">
              <span>PICK IT UP.</span>
              <span>CALL IT IN.</span>
              <span className="outline">LET IT FLY.</span>
            </h2>
            <p className="showcase-copy">
              총도, 마법도 없다. Binder와 Chair를 직접 들고 던지고,
              동료에게 가장 가까운 물체를 회수시켜 다음 물리적 선택을 만든다.
            </p>
            <div className="showcase-flow" aria-label="핵심 플레이 흐름">
              <span>MOVE</span><i aria-hidden="true">→</i>
              <span>PICKUP</span><i aria-hidden="true">→</i>
              <span>FETCH</span><i aria-hidden="true">→</i>
              <span>THROW</span>
            </div>
          </div>
        </section>

        <section className="controls-section" id="controls" aria-labelledby="controls-title">
          <div className="section-shell">
            <div className="section-heading controls-heading">
              <div>
                <p className="eyebrow">01 / CONTROL DECK</p>
                <h2 id="controls-title">생각보다 먼저 움직여라</h2>
              </div>
              <p className="section-summary">
                DESKTOP / KEYBOARD + MOUSE
              </p>
            </div>

            <div className="control-deck">
              {controlGroups.map((group, index) => (
                <article className="control-group" key={group.label}>
                  <div className="control-group-heading">
                    <span>{String(index + 1).padStart(2, "0")}</span>
                    <div>
                      <p>{group.label}</p>
                      <h3>{group.title}</h3>
                    </div>
                  </div>
                  <dl>
                    {group.items.map((item) => (
                      <div key={item.key}>
                        <dt><kbd>{item.key}</kbd></dt>
                        <dd>{item.action}</dd>
                      </div>
                    ))}
                  </dl>
                </article>
              ))}
            </div>
          </div>
        </section>

        <section className="trailer-section" id="visual" aria-labelledby="trailer-title">
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
              <p className="eyebrow">02 / AFTER-HOURS VISUAL DIRECTION</p>
              <h2 id="trailer-title">THE NIGHT SHIFT<br />HITS DIFFERENT.</h2>
              <p>
                차가운 형광등, 지친 직원, 평범한 사무실. 그리고 예상보다 훨씬 큰 물리적 결과.
                공개 Trailer·Cinematic은 아직 준비 중입니다.
              </p>
            </div>
            <figcaption>
              LGT_01 승인 AI 생성 조명 비주얼 레퍼런스 — 실제 Trailer 프레임·런타임 캡처 아님
            </figcaption>
          </figure>
        </section>

        <section className="current-cut" id="status" aria-labelledby="status-title">
          <div className="section-shell current-cut-grid">
            <div>
              <p className="eyebrow">03 / CURRENT CUT</p>
              <h2 id="status-title">ONE FLOOR.<br />ONE ALLY.<br /><span>PHYSICS FIRST.</span></h2>
            </div>
            <div className="current-cut-copy">
              <p>
                현재 공개 설명은 Human Owner가 승인한 Operations Floor S1–S3와
                Designer A 캐릭터 런타임 통합 범위만 담습니다.
              </p>
              <p className="current-cut-boundary">
                S4+ 전투·Cargo Strap·Drawer/Exit·Production VFX·Audio·BGM은 이 Showcase 설명에 포함하지 않습니다.
              </p>
            </div>
          </div>
          <div className="current-cut-meta" aria-label="현재 Showcase 상태">
            <span>S1–S3 / OWNER-APPROVED</span>
            <span>WEBGL / SEPARATE RELEASE</span>
            <span>AI VISUALS / REFERENCE ONLY</span>
          </div>
        </section>

        <section className="credits-section" id="credits" aria-labelledby="credits-title">
          <div className="section-shell">
            <div className="section-heading">
              <div>
                <p className="eyebrow">04 / TEAM &amp; CREDITS</p>
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
              조윤성의 1인 개발 프로젝트입니다. Designer A가 캐릭터·애니메이션 자산을 제공했고,
              OpenAI Codex가 구현·검증·빌드·웹사이트 작업을 지원했습니다.
            </p>
          </div>
        </section>

        <section className="notice-section" aria-labelledby="notice-title">
          <div className="section-shell notice-grid">
            <div className="notice-heading">
              <p className="eyebrow">05 / RELEASE NOTICE</p>
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
        <p><strong>PLAYABLE IN BROWSER</strong></p>
      </footer>
    </>
  );
}
