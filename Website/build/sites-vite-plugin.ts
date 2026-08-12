import { access, cp, mkdir, open, readdir, rm, stat, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import type { Plugin } from "vite";

async function exists(path: string): Promise<boolean> {
  try {
    await access(path);
    return true;
  } catch (error) {
    if ((error as NodeJS.ErrnoException).code === "ENOENT") {
      return false;
    }
    throw error;
  }
}

const SITES_WEBGL_CHUNK_BYTES = 24 * 1024 * 1024;

async function chunkOversizedWebGLFiles(root: string): Promise<void> {
  const entries = await readdir(root, { withFileTypes: true });
  for (const entry of entries) {
    const path = resolve(root, entry.name);
    if (entry.isDirectory()) {
      await chunkOversizedWebGLFiles(path);
      continue;
    }
    if (!entry.isFile()) continue;

    const file = await stat(path);
    if (file.size <= SITES_WEBGL_CHUNK_BYTES) continue;

    const source = await open(path, "r");
    const parts: Array<{ path: string; bytes: number }> = [];
    try {
      let offset = 0;
      let partIndex = 0;
      while (offset < file.size) {
        const length = Math.min(SITES_WEBGL_CHUNK_BYTES, file.size - offset);
        const buffer = Buffer.allocUnsafe(length);
        const { bytesRead } = await source.read(buffer, 0, length, offset);
        if (bytesRead !== length) throw new Error(`Short read while chunking ${path}`);
        const partName = `${entry.name}.part${String(partIndex).padStart(3, "0")}`;
        await writeFile(resolve(root, partName), buffer);
        parts.push({ path: partName, bytes: bytesRead });
        offset += bytesRead;
        partIndex += 1;
      }
    } finally {
      await source.close();
    }

    await writeFile(
      `${path}.parts.json`,
      `${JSON.stringify({ schemaVersion: 1, bytes: file.size, parts })}\n`,
      "utf8",
    );
    await rm(path);
  }
}

// Packages Sites metadata and migrations after Vite finishes compiling.
export function sites(): Plugin {
  let root = process.cwd();

  return {
    name: "sites",
    apply: "build",
    configResolved(config) {
      root = config.root;
    },
    async closeBundle() {
      const outputDirectory = resolve(root, "dist", ".openai");
      const hostingConfig = resolve(root, ".openai", "hosting.json");
      const drizzleSource = resolve(root, "drizzle");

      await rm(outputDirectory, { recursive: true, force: true });
      await mkdir(outputDirectory, { recursive: true });

      if (await exists(hostingConfig)) {
        await cp(hostingConfig, resolve(outputDirectory, "hosting.json"));
      }
      if (await exists(drizzleSource)) {
        await cp(drizzleSource, resolve(outputDirectory, "drizzle"), {
          recursive: true,
        });
      }

      const webglStaging = resolve(root, "deployment-staging", "webgl");
      if (await exists(webglStaging)) {
        const releasesSource = resolve(webglStaging, "play", "releases");
        if (await exists(releasesSource)) {
          const releaseEntries = await import("node:fs/promises").then(({ readdir }) =>
            readdir(releasesSource, { withFileTypes: true }),
          );
          for (const release of releaseEntries) {
            if (!release.isDirectory()) continue;
            const destination = resolve(root, "dist", "client", "play", "releases", release.name);
            await rm(destination, { recursive: true, force: true });
            await cp(
              resolve(releasesSource, release.name),
              destination,
              {
                recursive: true,
              },
            );
            // Sites normalizes HTML assets. Keep a byte-identical, deployment-only
            // alias so the Worker can return Unity's original index without rewriting.
            await cp(
              resolve(destination, "index.html"),
              resolve(destination, "__webgl-index.bin"),
            );
            await rm(resolve(destination, "index.html"));
            await chunkOversizedWebGLFiles(destination);
          }
        }
      }
    },
  };
}
