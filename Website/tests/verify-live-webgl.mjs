import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";

const baseUrl = (process.env.SITE_URL ?? "http://127.0.0.1:3001").replace(/\/$/, "");
const releaseHistory = JSON.parse(
  await readFile(new URL("../data/webgl-releases.json", import.meta.url), "utf8"),
);
const buildGuid = process.env.BUILD_GUID ?? releaseHistory.latestBuildGuid;
const inventory = JSON.parse(
  await readFile(new URL(`../data/webgl-release-inventory-${buildGuid}.json`, import.meta.url), "utf8"),
);
const releaseBase = `${baseUrl}/play/releases/${buildGuid}/`;
const failures = [];
const rows = [];

for (const file of inventory.files) {
  const publicPath = file.publicPath ?? file.path;
  const requestedUrl = new URL(publicPath, releaseBase);
  const response = await fetch(requestedUrl, {
    headers: { "Accept-Encoding": "identity" },
    redirect: "follow",
  });
  const body = Buffer.from(await response.arrayBuffer());
  const hash = createHash("sha256").update(body).digest("hex");
  const contentType = response.headers.get("content-type") ?? "";
  const contentEncoding = response.headers.get("content-encoding");

  const expectedMime = file.path.endsWith(".wasm")
    ? "application/wasm"
    : file.path.endsWith(".data")
      ? "application/octet-stream"
      : file.path.endsWith(".js")
        ? "javascript"
        : null;

  if (response.status !== 200) failures.push(`${file.path}: HTTP ${response.status}`);
  if (new URL(response.url).pathname !== requestedUrl.pathname) {
    failures.push(`${file.path}: redirected to ${response.url}`);
  }
  if (body.byteLength !== file.bytes) failures.push(`${file.path}: bytes ${body.byteLength}`);
  if (hash !== file.sha256) failures.push(`${file.path}: SHA-256 ${hash}`);
  if (expectedMime && !contentType.toLowerCase().includes(expectedMime)) {
    failures.push(`${file.path}: MIME ${contentType}`);
  }
  if (contentEncoding) failures.push(`${file.path}: unexpected Content-Encoding ${contentEncoding}`);

  rows.push({ path: file.path, publicPath, status: response.status, bytes: body.byteLength, sha256: hash, contentType, contentEncoding });
}

const totalBytes = rows.reduce((total, row) => total + row.bytes, 0);
const aggregateSha256 = createHash("sha256")
  .update(rows
    .toSorted((left, right) => {
      const leftPath = left.path.toLowerCase();
      const rightPath = right.path.toLowerCase();
      return leftPath < rightPath ? -1 : leftPath > rightPath ? 1 : 0;
    })
    .map((row) => `${row.path}\t${row.sha256}\n`)
    .join(""))
  .digest("hex");
if (rows.length !== inventory.fileCount) failures.push(`file count ${rows.length}`);
if (totalBytes !== inventory.httpBytes) failures.push(`total bytes ${totalBytes}`);
if (aggregateSha256 !== inventory.aggregateSha256) failures.push(`aggregate SHA-256 ${aggregateSha256}`);

const latestResponse = await fetch(`${baseUrl}/play/latest.json`, { cache: "no-store" });
const latest = await latestResponse.json();
const promotedRelease = releaseHistory.releases.find(
  (release) => release.buildGuid === releaseHistory.latestBuildGuid,
);
if (!promotedRelease) failures.push("promoted release missing from history");
if (latest.buildGuid !== releaseHistory.latestBuildGuid) failures.push(`latest GUID ${latest.buildGuid}`);
if (promotedRelease && latest.sourceSha !== promotedRelease.sourceSha) {
  failures.push(`latest source SHA ${latest.sourceSha}`);
}

const launcherResponse = await fetch(releaseBase, {
  headers: { "Accept-Encoding": "identity" },
  redirect: "error",
});
const launcherHtml = await launcherResponse.text();
if (launcherResponse.status !== 200) failures.push(`launcher HTTP ${launcherResponse.status}`);
if (!launcherResponse.headers.get("content-type")?.toLowerCase().includes("text/html")) {
  failures.push(`launcher MIME ${launcherResponse.headers.get("content-type")}`);
}
if (!launcherHtml.includes('id="unity-canvas"')) failures.push("launcher Unity canvas missing");

console.table(rows);
console.log(
  JSON.stringify(
    {
      result: failures.length ? "FAIL" : "PASS",
      fileCount: rows.length,
      totalBytes,
      aggregateSha256,
      recordedAggregateSha256: inventory.aggregateSha256,
      perFileIdentity: failures.length === 0,
      launcher: {
        status: launcherResponse.status,
        contentType: launcherResponse.headers.get("content-type"),
        edgeHtmlBytes: Buffer.byteLength(launcherHtml),
        unityCanvas: launcherHtml.includes('id="unity-canvas"'),
      },
      latest,
      failures,
    },
    null,
    2,
  ),
);

if (failures.length) process.exitCode = 1;
