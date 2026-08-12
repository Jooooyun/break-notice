import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { chromium } = require("playwright");

const browser = await chromium.launch({
  executablePath: "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe",
  headless: true,
  args: ["--disable-extensions"],
});

try {
  for (const viewport of [
    { name: "desktop-1366", width: 1366, height: 768 },
    { name: "desktop-1920", width: 1920, height: 1080 },
    { name: "mobile-390", width: 390, height: 844 },
  ]) {
    const page = await browser.newPage({ viewport });
    await page.goto("http://localhost:3000", { waitUntil: "load" });
    await page.screenshot({
      path: `work/screens/${viewport.name}.png`,
      fullPage: true,
    });
    await page.close();
  }
} finally {
  await browser.close();
}
