import { access, cp, mkdir, rm } from "node:fs/promises";
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
          }
        }
      }
    },
  };
}
