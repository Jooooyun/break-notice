import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require("playwright");

const baseUrl = process.env.SITE_URL ?? "http://localhost:3000";
const browsers = [
  {
    name: "Chrome",
    executablePath: "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
  },
  {
    name: "Edge",
    executablePath: "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe",
  },
];
const viewports = [
  { name: "desktop-1366", width: 1366, height: 768 },
  { name: "desktop-1920", width: 1920, height: 1080 },
  { name: "mobile-390", width: 390, height: 844 },
];

const failures = [];
const results = [];

function check(condition, message) {
  if (!condition) failures.push(message);
}

function srgbChannel(value) {
  const normalized = value / 255;
  return normalized <= 0.04045 ? normalized / 12.92 : ((normalized + 0.055) / 1.055) ** 2.4;
}

function luminance([red, green, blue]) {
  return 0.2126 * srgbChannel(red) + 0.7152 * srgbChannel(green) + 0.0722 * srgbChannel(blue);
}

function parseRgb(value) {
  const numbers = value.match(/[\d.]+/g)?.map(Number) ?? [];
  return numbers.slice(0, 3);
}

function contrastRatio(foreground, background) {
  const light = Math.max(luminance(parseRgb(foreground)), luminance(parseRgb(background)));
  const dark = Math.min(luminance(parseRgb(foreground)), luminance(parseRgb(background)));
  return (light + 0.05) / (dark + 0.05);
}

for (const browserConfig of browsers) {
  const browser = await chromium.launch({
    executablePath: browserConfig.executablePath,
    headless: true,
    args: ["--disable-extensions", "--no-default-browser-check"],
  });

  try {
    for (const viewport of viewports) {
      const context = await browser.newContext({
        viewport: { width: viewport.width, height: viewport.height },
        colorScheme: "dark",
        reducedMotion: "reduce",
      });
      const page = await context.newPage();
      const consoleProblems = [];
      const failedRequests = [];

      page.on("console", (message) => {
        if (["warning", "error"].includes(message.type())) {
          consoleProblems.push(`${message.type()}: ${message.text()}`);
        }
      });
      page.on("pageerror", (error) => consoleProblems.push(`pageerror: ${error.message}`));
      page.on("requestfailed", (request) => {
        failedRequests.push(`${request.url()} :: ${request.failure()?.errorText ?? "unknown"}`);
      });

      const response = await page.goto(`${baseUrl}/`, { waitUntil: "load" });
      for (const selector of [".trailer-section", "#credits", "footer"]) {
        await page.locator(selector).scrollIntoViewIfNeeded();
        await page.waitForTimeout(250);
      }
      await page.evaluate(async () => {
        await Promise.all(
          [...document.images].map((image) => {
            if (image.complete) return undefined;
            return new Promise((resolve) => {
              image.addEventListener("load", resolve, { once: true });
              image.addEventListener("error", resolve, { once: true });
            });
          }),
        );
      });

      const metrics = await page.evaluate(() => {
        const html = document.documentElement;
        const play = document.querySelector(".play-button");
        const focusable = [...document.querySelectorAll("a[href], button:not([disabled])")];
        const headings = [...document.querySelectorAll("h1, h2, h3")].map((heading) => ({
          level: Number(heading.tagName.slice(1)),
          text: heading.textContent?.trim() ?? "",
        }));
        return {
          lang: html.lang,
          title: document.title,
          viewport: [window.innerWidth, window.innerHeight],
          horizontalOverflow: html.scrollWidth > html.clientWidth,
          h1Count: document.querySelectorAll("h1").length,
          headings,
          playDisabled: play?.disabled === true,
          playLabel: play?.textContent?.replace(/\s+/g, " ").trim(),
          missingAlt: [...document.images]
            .filter((image) => !image.hasAttribute("alt") || image.alt.trim() === "")
            .map((image) => image.src),
          brokenImages: [...document.images]
            .filter((image) => !image.complete || image.naturalWidth === 0)
            .map((image) => image.src),
          blankLinks: [...document.querySelectorAll("a")]
            .filter((link) => !(link.getAttribute("href") ?? "").trim())
            .map((link) => link.textContent?.trim()),
          missingHashTargets: [...document.querySelectorAll('a[href^="#"]')]
            .map((link) => link.getAttribute("href"))
            .filter((href) => href && href !== "#" && !document.querySelector(href)),
          focusableCount: focusable.length,
          externalScripts: [...document.scripts]
            .map((script) => script.src)
            .filter((src) => src && new URL(src).origin !== location.origin),
          trackerScripts: [...document.scripts]
            .map((script) => script.src)
            .filter((src) => /google-analytics|googletagmanager|segment|mixpanel|posthog|amplitude|facebook\.net/i.test(src)),
          colorSamples: ["body", ".hero-lede", ".play-status", ".status-row > p:last-child", ".notice-grid article p"]
            .map((selector) => {
              const element = document.querySelector(selector);
              if (!element) return null;
              const style = getComputedStyle(element);
              let background = style.backgroundColor;
              let ancestor = element.parentElement;
              while (ancestor && (background === "rgba(0, 0, 0, 0)" || background === "transparent")) {
                background = getComputedStyle(ancestor).backgroundColor;
                ancestor = ancestor.parentElement;
              }
              return { selector, color: style.color, background };
            })
            .filter(Boolean),
        };
      });

      const label = `${browserConfig.name} ${viewport.name}`;
      check(response?.status() === 200, `${label}: home status ${response?.status()}`);
      check(metrics.lang === "ko", `${label}: html lang is ${metrics.lang}`);
      check(metrics.title === "BREAK NOTICE — Showcase", `${label}: unexpected title`);
      check(!metrics.horizontalOverflow, `${label}: horizontal overflow`);
      check(metrics.h1Count === 1, `${label}: h1 count ${metrics.h1Count}`);
      check(metrics.playDisabled, `${label}: Play must remain disabled`);
      check(metrics.playLabel?.includes("BUILD PREPARING"), `${label}: Play status missing`);
      check(metrics.missingAlt.length === 0, `${label}: missing alt ${metrics.missingAlt.join(", ")}`);
      check(metrics.brokenImages.length === 0, `${label}: broken images ${metrics.brokenImages.join(", ")}`);
      check(metrics.blankLinks.length === 0, `${label}: blank links`);
      check(metrics.missingHashTargets.length === 0, `${label}: broken hash targets`);
      check(metrics.externalScripts.length === 0, `${label}: external scripts ${metrics.externalScripts.join(", ")}`);
      check(metrics.trackerScripts.length === 0, `${label}: tracker script detected`);
      for (const sample of metrics.colorSamples) {
        if (parseRgb(sample.color).length === 3 && parseRgb(sample.background).length === 3) {
          check(contrastRatio(sample.color, sample.background) >= 4.5, `${label}: low contrast ${sample.selector}`);
        }
      }
      check(consoleProblems.length === 0, `${label}: console ${consoleProblems.join(" | ")}`);
      check(failedRequests.length === 0, `${label}: failed requests ${failedRequests.join(" | ")}`);
      const homeConsoleProblemCount = consoleProblems.length;
      const homeFailedRequestCount = failedRequests.length;

      await page.keyboard.press("Home");
      await page.keyboard.press("Tab");
      const focused = await page.evaluate(() => ({
        text: document.activeElement?.textContent?.trim(),
        outlineStyle: getComputedStyle(document.activeElement).outlineStyle,
        outlineWidth: getComputedStyle(document.activeElement).outlineWidth,
      }));
      check(focused.text === "본문으로 건너뛰기", `${label}: skip link was not first focus target`);
      check(focused.outlineStyle !== "none" && focused.outlineWidth !== "0px", `${label}: focus indicator missing`);

      const controlsLink = page.getByRole("link", { name: "Controls", exact: true });
      await controlsLink.press("Enter");
      check((await page.url()).endsWith("#controls"), `${label}: Controls navigation failed`);

      consoleProblems.length = 0;
      failedRequests.length = 0;
      const notFound = await page.goto(`${baseUrl}/missing-${viewport.name}`, { waitUntil: "load" });
      check(notFound?.status() === 404, `${label}: 404 status ${notFound?.status()}`);
      check(await page.getByRole("heading", { name: "PAGE NOT FOUND" }).isVisible(), `${label}: 404 heading missing`);

      results.push({
        browser: browserConfig.name,
        viewport: `${viewport.width}x${viewport.height}`,
        homeStatus: response?.status(),
        notFoundStatus: notFound?.status(),
        consoleProblems: homeConsoleProblemCount,
        failedRequests: homeFailedRequestCount,
        brokenImages: metrics.brokenImages.length,
        horizontalOverflow: metrics.horizontalOverflow,
      });
      await context.close();
    }
  } finally {
    await browser.close();
  }
}

console.table(results);
if (failures.length > 0) {
  console.error("\nBROWSER SMOKE FAILURES");
  for (const failure of failures) console.error(`- ${failure}`);
  process.exitCode = 1;
} else {
  console.log("\nBROWSER SMOKE PASS");
}
