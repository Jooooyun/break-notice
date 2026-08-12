import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require("playwright");

const baseUrl = (process.env.SITE_URL ?? "http://127.0.0.1:3001").replace(/\/$/, "");
const buildGuid = "3cc7416d44c44067bc51c6474c46351d";
const browsers = [
  { name: "Chrome", executablePath: "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe" },
  { name: "Edge", executablePath: "C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe" },
];
const viewports = [
  { width: 1366, height: 768 },
  { width: 1920, height: 1080 },
];
const failures = [];
const results = [];

for (const browserConfig of browsers) {
  const browser = await chromium.launch({
    executablePath: browserConfig.executablePath,
    headless: true,
    args: ["--disable-extensions", "--no-default-browser-check", "--enable-webgl", "--ignore-gpu-blocklist"],
  });
  try {
    for (const viewport of viewports) {
      const context = await browser.newContext({ viewport });
      const page = await context.newPage();
      const consoleProblems = [];
      const nonBlockingWebglWarnings = [];
      const failedRequests = [];
      let dialogs = 0;

      page.on("console", (message) => {
        if (message.type() === "warning" && /WebGL: INVALID_ENUM: getInternalformatParameter: invalid internalformat/.test(message.text())) {
          nonBlockingWebglWarnings.push(message.text());
        } else if (["warning", "error"].includes(message.type())) {
          consoleProblems.push(`${message.type()}: ${message.text()}`);
        }
      });
      page.on("pageerror", (error) => consoleProblems.push(`pageerror: ${error.message}`));
      page.on("requestfailed", (request) => failedRequests.push(`${request.url()} :: ${request.failure()?.errorText ?? "unknown"}`));
      page.on("dialog", async (dialog) => {
        dialogs += 1;
        await dialog.dismiss();
      });

      const response = await page.goto(`${baseUrl}/play/`, { waitUntil: "domcontentloaded", timeout: 120_000 });
      const finalUrl = page.url();
      const canvas = page.locator("#unity-canvas");
      await canvas.waitFor({ state: "visible", timeout: 30_000 });
      let unityReady = false;
      try {
        await page.waitForFunction(
          () => getComputedStyle(document.querySelector("#unity-loading-bar")).display === "none",
          undefined,
          { timeout: 180_000 },
        );
        unityReady = true;
      } catch {
        unityReady = false;
      }

      const canvasMetrics = await canvas.evaluate((element) => ({
        width: element.clientWidth,
        height: element.clientHeight,
        renderWidth: element.width,
        renderHeight: element.height,
      }));
      const refreshResponse = await page.reload({ waitUntil: "domcontentloaded", timeout: 120_000 });
      const refreshedUrl = page.url();
      await page.locator("#unity-canvas").waitFor({ state: "visible", timeout: 30_000 });
      let refreshUnityReady = false;
      try {
        await page.waitForFunction(
          () => getComputedStyle(document.querySelector("#unity-loading-bar")).display === "none",
          undefined,
          { timeout: 180_000 },
        );
        refreshUnityReady = true;
      } catch {
        refreshUnityReady = false;
      }
      if (response?.status() !== 200) failures.push(`${browserConfig.name} ${viewport.width}: HTTP ${response?.status()}`);
      if (!finalUrl.includes(`/play/releases/${buildGuid}/`)) failures.push(`${browserConfig.name} ${viewport.width}: GUID URL ${finalUrl}`);
      if (!unityReady) failures.push(`${browserConfig.name} ${viewport.width}: Unity canvas not ready`);
      if (refreshResponse?.status() !== 200) failures.push(`${browserConfig.name} ${viewport.width}: refresh HTTP ${refreshResponse?.status()}`);
      if (refreshedUrl !== finalUrl) failures.push(`${browserConfig.name} ${viewport.width}: refresh changed GUID URL ${finalUrl} -> ${refreshedUrl}`);
      if (!refreshUnityReady) failures.push(`${browserConfig.name} ${viewport.width}: refreshed Unity canvas not ready`);
      if (dialogs) failures.push(`${browserConfig.name} ${viewport.width}: dialogs ${dialogs}`);
      if (consoleProblems.length) failures.push(`${browserConfig.name} ${viewport.width}: console ${consoleProblems.join(" | ")}`);
      if (failedRequests.length) failures.push(`${browserConfig.name} ${viewport.width}: requests ${failedRequests.join(" | ")}`);

      results.push({
        browser: browserConfig.name,
        viewport: `${viewport.width}x${viewport.height}`,
        status: response?.status(),
        guidUrl: finalUrl.includes(buildGuid),
        unityReady,
        refreshStatus: refreshResponse?.status(),
        sameGuidAfterRefresh: refreshedUrl === finalUrl,
        refreshUnityReady,
        dialogs,
        blockingConsoleProblems: consoleProblems.length,
        nonBlockingWebglWarnings: nonBlockingWebglWarnings.length,
        failedRequests: failedRequests.length,
        ...canvasMetrics,
      });
      await context.close();
    }
  } finally {
    await browser.close();
  }
}

console.table(results);
if (failures.length) {
  console.error("WEBGL BROWSER SMOKE FAILURES");
  for (const failure of failures) console.error(`- ${failure}`);
  process.exitCode = 1;
} else {
  console.log("WEBGL BROWSER SMOKE PASS");
}
