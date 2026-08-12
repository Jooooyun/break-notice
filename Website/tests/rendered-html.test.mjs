import assert from "node:assert/strict";
import { access, readFile, readdir, stat } from "node:fs/promises";
import { extname, join } from "node:path";
import { fileURLToPath } from "node:url";
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

async function renderWebglAsset(pathname, requestHeaders = {}, requestCapture = null) {
  const workerUrl = new URL("../dist/server/index.js", import.meta.url);
  workerUrl.searchParams.set("asset-test", `${process.pid}-${Date.now()}-${encodeURIComponent(pathname)}`);
  const { default: worker } = await import(workerUrl.href);

  const distClient = fileURLToPath(new URL("../dist/client/", import.meta.url));
  const contentTypes = new Map([
    [".wasm", "application/wasm"],
    [".data", "application/octet-stream"],
    [".js", "text/javascript; charset=utf-8"],
    [".html", "text/html; charset=utf-8"],
  ]);

  return worker.fetch(
    new Request(new URL(pathname, "http://localhost"), { headers: requestHeaders }),
    {
      ASSETS: {
        fetch: async (assetRequest) => {
          if (requestCapture) requestCapture.headers = new Headers(assetRequest.headers);
          const assetPath = new URL(assetRequest.url).pathname.replace(/^\//, "");
          try {
            const body = await readFile(join(distClient, assetPath));
            return new Response(body, {
              status: 200,
              headers: { "Content-Type": contentTypes.get(extname(assetPath)) ?? "application/octet-stream" },
            });
          } catch (error) {
            if (error.code !== "ENOENT") throw error;
            return new Response("Not found", { status: 404 });
          }
        },
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
  assert.match(html, /PLAY NOW/);
  assert.match(html, /CONTEST-0002 S3/);
  assert.match(html, /S4\+ \/ FINAL COMBAT \/ PRODUCTION ART NOT IN THIS BUILD/);
  assert.match(html, /href="\/play\/"/);
  assert.doesNotMatch(html, /BUILD PREPARING|disabled=""/);
  assert.match(html, /AI-GENERATED PRODUCTION VISUAL REFERENCE/);
  assert.match(html, /실제 런타임 캡처 아님/);
  assert.match(html, /NOT IN THIS BUILD/);
  assert.doesNotMatch(html, /codex-preview|react-loading-skeleton/i);
  assert.doesNotMatch(html, /https?:\/\/(?:127\.0\.0\.1|localhost)/i);
});

test("returns an explicit 404 page for unknown routes", async () => {
  const response = await render("/this-route-does-not-exist/");
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

test("publishes a no-cache latest pointer and release history", async () => {
  const latest = await render("/play/latest.json");
  assert.equal(latest.status, 200);
  assert.match(latest.headers.get("cache-control") ?? "", /no-cache/);
  const latestBody = await latest.json();
  assert.equal(latestBody.buildGuid, "89900eaad2e04c6d8aa82393cf39bc92");
  assert.equal(latestBody.sourceSha, "0e842d231abbb9f07aabd2afe93322629670e259");

  const history = await render("/data/webgl-releases.json");
  assert.equal(history.status, 200);
  assert.match(history.headers.get("cache-control") ?? "", /no-cache/);
  const historyBody = await history.json();
  assert.equal(historyBody.latestBuildGuid, latestBody.buildGuid);
  assert.equal(historyBody.releases[0].fileCount, 19);
  assert.equal(historyBody.releases[0].httpBytes, 114057258);
  assert.equal(historyBody.releases[0].previousVersion, "3cc7416d44c44067bc51c6474c46351d");
  assert.equal(historyBody.releases[1].buildGuid, "3cc7416d44c44067bc51c6474c46351d");
  assert.equal(historyBody.releases[1].status, "superseded");
});

test("sets immutable WebGL cache and exact uncompressed MIME headers", async () => {
  const releaseRoot = "/play/releases/89900eaad2e04c6d8aa82393cf39bc92/Build/";
  const wasm = await renderWebglAsset(`${releaseRoot}Development.wasm`);
  assert.equal(wasm.status, 200);
  assert.equal(wasm.headers.get("content-type"), "application/wasm");
  assert.match(wasm.headers.get("cache-control") ?? "", /max-age=31536000, immutable/);
  assert.equal(wasm.headers.get("content-encoding"), null);

  const data = await renderWebglAsset(`${releaseRoot}Development.data`);
  assert.equal(data.headers.get("content-type"), "application/octet-stream");
  assert.equal(data.headers.get("content-encoding"), null);

  const requestCapture = {};
  const revalidatedData = await renderWebglAsset(
    `${releaseRoot}Development.data`,
    { "if-none-match": '"cached-etag"', range: "bytes=0-1" },
    requestCapture,
  );
  assert.equal(revalidatedData.status, 200);
  assert.equal(requestCapture.headers.get("if-none-match"), null);
  assert.equal(requestCapture.headers.get("range"), null);
  assert.equal(requestCapture.headers.get("cache-control"), "no-store");

  const loader = await renderWebglAsset(`${releaseRoot}Development.loader.js`);
  assert.equal(loader.headers.get("content-type"), "text/javascript; charset=utf-8");
  assert.equal(loader.headers.get("content-encoding"), null);
});

test("reassembles chunked WebGL transport files byte-identically", async () => {
  const wasmPath = "/play/releases/89900eaad2e04c6d8aa82393cf39bc92/Build/Development.wasm";
  const response = await renderWebglAsset(wasmPath);
  const expected = await readFile(
    new URL(
      "../deployment-staging/webgl/play/releases/89900eaad2e04c6d8aa82393cf39bc92/Build/Development.wasm",
      import.meta.url,
    ),
  );
  assert.equal(response.status, 200);
  assert.equal(response.headers.get("content-type"), "application/wasm");
  assert.equal(response.headers.get("content-encoding"), null);
  assert.deepEqual(Buffer.from(await response.arrayBuffer()), expected);

  const transportDirectory = new URL(
    "../dist/client/play/releases/89900eaad2e04c6d8aa82393cf39bc92/Build/",
    import.meta.url,
  );
  const transportFiles = await readdir(transportDirectory);
  assert.equal(transportFiles.includes("Development.wasm"), false);
  assert.equal(transportFiles.filter((file) => /^Development\.wasm\.part\d{3}$/.test(file)).length, 5);
  assert.equal(transportFiles.includes("Development.wasm.parts.json"), true);
  for (const file of transportFiles) {
    assert.ok((await stat(new URL(file, transportDirectory))).size <= 26_214_400);
  }

  const releaseDirectory = new URL("../", transportDirectory);
  await assert.rejects(access(new URL("index.html", releaseDirectory)));
  assert.deepEqual(
    await readFile(new URL("__webgl-index.bin", releaseDirectory)),
    await readFile(
      new URL(
        "../deployment-staging/webgl/play/releases/89900eaad2e04c6d8aa82393cf39bc92/index.html",
        import.meta.url,
      ),
    ),
  );
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
