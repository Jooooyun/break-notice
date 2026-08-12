import assert from "node:assert/strict";
import { access, readFile, readdir } from "node:fs/promises";
import test from "node:test";

async function render(pathname = "/") {
  const workerUrl = new URL("../dist/server/index.js", import.meta.url);
  workerUrl.searchParams.set("test", `${process.pid}-${Date.now()}-${encodeURIComponent(pathname)}`);
  const { default: worker } = await import(workerUrl.href);

  return worker.fetch(
    new Request(new URL(pathname, "http://localhost"), {
      headers: { accept: "text/html" },
    }),
    {
      ASSETS: {
        fetch: async () => new Response("Not found", { status: 404 }),
      },
    },
    {
      waitUntil() {},
      passThroughOnException() {},
    },
  );
}

test("server-renders the Korean Showcase release page", async () => {
  const response = await render();
  assert.equal(response.status, 200);
  assert.match(response.headers.get("content-type") ?? "", /^text\/html\b/i);

  const html = await response.text();
  assert.match(html, /<html[^>]+lang="ko"/i);
  assert.match(html, /<title>BREAK NOTICE — Showcase<\/title>/i);
  assert.match(html, /id="core-loop"/);
  assert.match(html, /id="controls"/);
  assert.match(html, /id="media"/);
  assert.match(html, /id="status"/);
  assert.match(html, /id="credits"/);
  assert.match(html, /BUILD PREPARING/);
  assert.match(html, /disabled=""/);
  assert.match(html, /AI-GENERATED PRODUCTION VISUAL REFERENCE/);
  assert.match(html, /실제 런타임 캡처 아님/);
  assert.match(html, /NOT AUTHORIZED/);
  assert.doesNotMatch(html, /codex-preview|react-loading-skeleton/i);
  assert.doesNotMatch(html, /https?:\/\/(?:127\.0\.0\.1|localhost)/i);
});

test("returns an explicit 404 page for unknown routes", async () => {
  const response = await render("/this-route-does-not-exist");
  assert.equal(response.status, 404);
  const html = await response.text();
  assert.match(html, /PAGE NOT FOUND/);
  assert.match(html, /BREAK NOTICE로 돌아가기/);
  assert.match(html, /<meta name="robots" content="noindex, nofollow"/i);
});

test("sets baseline privacy and browser security headers", async () => {
  const response = await render();
  assert.equal(response.headers.get("x-content-type-options"), "nosniff");
  assert.equal(response.headers.get("x-frame-options"), "DENY");
  assert.equal(response.headers.get("referrer-policy"), "strict-origin-when-cross-origin");
  assert.match(response.headers.get("permissions-policy") ?? "", /camera=\(\)/);
});

test("ships only approved public media and no source maps", async () => {
  const publicMedia = new URL("../public/media/", import.meta.url);
  const mediaFiles = (await readdir(publicMedia)).sort();
  assert.deepEqual(mediaFiles, [
    "CHR_00_Player_Identity_Lock_v002_FINAL.png",
    "CHR_01_First_Ally_Identity_v001_FINAL.png",
    "LGT_01_Night_Archive_Lighting_v001_FINAL.png",
    "PGT_01_Gameplay_Camera_v002_FINAL.png",
  ]);

  for (const file of mediaFiles) {
    await access(new URL(file, publicMedia));
  }

  const hosting = JSON.parse(await readFile(new URL("../.openai/hosting.json", import.meta.url), "utf8"));
  assert.equal(hosting.d1, null);
  assert.equal(hosting.r2, null);

  const distFiles = await readdir(new URL("../dist/", import.meta.url), { recursive: true });
  assert.equal(distFiles.some((file) => file.endsWith(".map")), false);
});

test("removes starter-only and persistence scaffolding", async () => {
  for (const path of ["app/_sites-preview", "db", "drizzle", "examples"]) {
    let entries;
    try {
      entries = await readdir(new URL(`../${path}`, import.meta.url), {
        recursive: true,
        withFileTypes: true,
      });
    } catch (error) {
      if (error.code !== "ENOENT") throw error;
      continue;
    }
    assert.equal(entries.some((entry) => entry.isFile()), false);
  }

  const packageJson = await readFile(new URL("../package.json", import.meta.url), "utf8");
  assert.doesNotMatch(packageJson, /react-loading-skeleton|drizzle|tailwind/i);
});
