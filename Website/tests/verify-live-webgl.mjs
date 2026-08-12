import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";

const baseUrl = (process.env.SITE_URL ?? "http://127.0.0.1:3001").replace(/\/$/, "");
const buildGuid = "3cc7416d44c44067bc51c6474c46351d";
const inventory = JSON.parse(
  await readFile(new URL(`../data/webgl-release-inventory-${buildGuid}.json`, import.meta.url), "utf8"),
);
const releaseBase = `${baseUrl}/play/releases/${buildGuid}/`;
const failures = [];
const rows = [];

for (const file of inventory.files) {
  const response = await fetch(new URL(file.path, releaseBase), {
    headers: { "Accept-Encoding": "identity" },
    redirect: "error",
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
  if (body.byteLength !== file.bytes) failures.push(`${file.path}: bytes ${body.byteLength}`);
  if (hash !== file.sha256) failures.push(`${file.path}: SHA-256 ${hash}`);
  if (expectedMime && !contentType.toLowerCase().includes(expectedMime)) {
    failures.push(`${file.path}: MIME ${contentType}`);
  }
  if (contentEncoding) failures.push(`${file.path}: unexpected Content-Encoding ${contentEncoding}`);

  rows.push({ path: file.path, status: response.status, bytes: body.byteLength, sha256: hash, contentType, contentEncoding });
}

const totalBytes = rows.reduce((total, row) => total + row.bytes, 0);
if (rows.length !== inventory.fileCount) failures.push(`file count ${rows.length}`);
if (totalBytes !== inventory.httpBytes) failures.push(`total bytes ${totalBytes}`);

const latestResponse = await fetch(`${baseUrl}/play/latest.json`, { cache: "no-store" });
const latest = await latestResponse.json();
if (latest.buildGuid !== buildGuid) failures.push(`latest GUID ${latest.buildGuid}`);
if (latest.sourceSha !== inventory.sourceSha) failures.push(`latest source SHA ${latest.sourceSha}`);

console.table(rows);
console.log(
  JSON.stringify(
    {
      result: failures.length ? "FAIL" : "PASS",
      fileCount: rows.length,
      totalBytes,
      recordedAggregateSha256: inventory.aggregateSha256,
      perFileIdentity: failures.length === 0,
      latest,
      failures,
    },
    null,
    2,
  ),
);

if (failures.length) process.exitCode = 1;
