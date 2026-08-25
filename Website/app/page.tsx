const controlGroups = [
  {
    label: "MOVE",
    title: "이동과 시점",
    items: [
      { key: "WASD", action: "이동" },
      { key: "MOUSE", action: "시점" },
      { key: "RMB", action: "조준" },
    ],
  },
  {
    label: "HANDLE",
    title: "물건 들고 던지기",
    items: [
      { key: "E", action: "물건 줍기 / Binder 받기" },
      { key: "HOLD LMB", action: "누르고 충전 / 떼서 던지기" },
    ],
  },
  {
    label: "COMMAND",
    title: "동료 지시하기",
    items: [
      { key: "F", action: "동료에게 가까운 물건을 가져오게 하기" },
      { key: "Q", action: "따라오기 / 대기 / 물건 건네기" },
      { key: "G", action: "동료를 내 위치로 다시 부르기" },
    ],
  },
  {
    label: "RECOVER",
    title: "다시 시작하기",
    items: [
      { key: "BACKSPACE", action: "처음 상태로 되돌리기" },
      { key: "ESC", action: "마우스 잠금 해제" },
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
              야근 중인 시설 직원이 사무실 물건을 집어 들고 던지며,
              동료와 함께 난장판을 헤쳐 나가는 3인칭 물리 액션 게임.
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
              클릭하면 게임이 전체 화면으로 열립니다.
            </p>
          </div>

          <figure className="hero-media">
            {/* eslint-disable-next-line @next/next/no-img-element */}
            <img
              src="/media/PGT_01_Gameplay_Camera_v002_FINAL.png"
              width="1672"
              height="941"
              alt="사무실에서 플레이어와 동료, 여러 사무용 물건이 보이는 3인칭 시점 이미지"
              fetchPriority="high"
              decoding="async"
            />
            <div className="hero-media-overlay" aria-hidden="true">
              <span>CAM_01</span>
              <span>OTS / THIRD PERSON</span>
              <span>REFERENCE ONLY</span>
            </div>
            <figcaption>
              승인된 AI 콘셉트 이미지 · 실제 게임 화면이 아닙니다
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
              총도 마법도 없다. 손에 잡히는 건 Binder와 Chair뿐.
              직접 집어 던지고, 동료에게 가까운 물건을 가져오게 하며 다음 수를 만든다.
            </p>
            <div className="showcase-flow" aria-label="플레이 흐름">
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
                <h2 id="controls-title">키보드와 마우스</h2>
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
              alt="차가운 형광등이 켜진 기록 보관실의 야간 분위기를 표현한 이미지"
              loading="lazy"
              decoding="async"
            />
            <div className="trailer-overlay">
              <p className="eyebrow">02 / AFTER-HOURS VISUAL DIRECTION</p>
              <h2 id="trailer-title">THE NIGHT SHIFT<br />HITS DIFFERENT.</h2>
              <p>
                차가운 형광등 아래, 지친 직원들과 익숙한 사무실이 예상 밖의 물리 액션 무대가 됩니다.
                Trailer와 Cinematic은 준비 중입니다.
              </p>
            </div>
            <figcaption>
              승인된 AI 조명 콘셉트 이미지 · 실제 Trailer나 게임 화면이 아닙니다
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
                현재 Showcase에서는 Operations Floor를 누비며,
                한 명의 동료에게 물건을 가져오거나 건네게 할 수 있습니다.
              </p>
              <p className="current-cut-boundary">
                전투 확장, Cargo Strap, Drawer/Exit, 완성형 VFX·Audio·BGM은 아직 소개하지 않습니다.
              </p>
            </div>
          </div>
          <div className="current-cut-meta" aria-label="현재 Showcase 상태">
            <span>S1–S3 / 승인 완료</span>
            <span>WEBGL / 별도 배포</span>
            <span>AI 이미지 / 콘셉트 자료</span>
          </div>
        </section>

        <section className="credits-section" id="credits" aria-labelledby="credits-title">
          <div className="section-shell">
            <div className="section-heading">
              <div>
                <p className="eyebrow">04 / TEAM &amp; CREDITS</p>
                <h2 id="credits-title">만든 사람들</h2>
              </div>
              <p className="section-summary">한 사람이 이끌고, 필요한 곳에 도움을 받았습니다.</p>
            </div>

            <div className="credits-grid">
              <article className="credit-card credit-owner">
                <div className="portrait-frame">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src="/media/CHR_00_Player_Identity_Lock_v002_FINAL.png"
                    width="916"
                    height="1717"
                    alt="BREAK NOTICE 주인공 CHR_00의 콘셉트 이미지"
                    loading="lazy"
                    decoding="async"
                  />
                </div>
                <div>
                  <span className="credit-role">HUMAN OWNER / PROJECT LEAD</span>
                  <h3>조윤성 · JO YOONSEONG</h3>
                  <p>프로젝트 총괄 · 기획 · 기술 개발 · Unity 통합 · 범위 관리 · 최종 검수</p>
                </div>
              </article>

              <article className="credit-card">
                <div className="portrait-frame portrait-ally">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src="/media/CHR_01_First_Ally_Identity_v001_FINAL.png"
                    width="916"
                    height="1716"
                    alt="BREAK NOTICE 동료 CHR_01의 콘셉트 이미지"
                    loading="lazy"
                    decoding="async"
                  />
                </div>
                <div>
                  <span className="credit-role">TECHNICAL ASSISTANCE</span>
                  <h3>OPENAI CODEX</h3>
                  <p>저장소 분석 · 구현 보조 · 테스트 작성 · 빌드 및 브라우저 검증 · 웹사이트 제작 보조</p>
                </div>
              </article>
            </div>

            <p className="credits-note">
              Designer A는 캐릭터와 애니메이션 자산을 제작했습니다. OpenAI Codex는 구현과 검증,
              빌드, 웹사이트 작업을 보조했습니다. 프로젝트의 방향과 최종 판단은 조윤성이 맡았습니다.
            </p>
          </div>
        </section>

        <section className="notice-section" aria-labelledby="notice-title">
          <div className="section-shell notice-grid">
            <div className="notice-heading">
              <p className="eyebrow">05 / RELEASE NOTICE</p>
              <h2 id="notice-title">플레이 전 확인</h2>
            </div>

            <article>
              <h3>BROWSER / SYSTEM</h3>
              <p>
                사이트는 모바일과 최신 브라우저에서 볼 수 있습니다. WebGL 게임은 데스크톱용 Chrome·Edge와
                키보드·마우스 환경에서 테스트했습니다. 모바일 플레이는 아직 테스트하지 않았습니다.
                확인한 해상도는 1366×768과 1920×1080입니다.
              </p>
            </article>

            <article>
              <h3>PRIVACY</h3>
              <p>
                이 사이트에는 로그인, 쿠키 배너, 분석 도구, 광고, 사용자 추적 기능이 없습니다.
                개인정보를 따로 수집하지 않습니다.
              </p>
            </article>

            <article>
              <h3>MEDIA DISCLOSURE</h3>
              <p>
                캐릭터와 배경 이미지는 프로젝트 책임자가 승인한 AI 콘셉트 자료입니다.
                실제 게임 화면이나 최종 3D 모델이 아닙니다. 홈페이지 제작을 위해 새로 생성한 AI 이미지는 없습니다.
              </p>
            </article>

            <article>
              <h3>LICENSE</h3>
              <p>
                © 2026 Project BREAK NOTICE. All rights reserved. 서면 허가 없이 복제·배포하거나,
                다시 라이선스를 부여하거나, 파생 프로젝트 및 모델 학습에 사용할 수 없습니다.
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
