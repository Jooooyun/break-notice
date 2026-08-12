/** Cloudflare Worker entry point for the BREAK NOTICE Showcase site. */
import { handleImageOptimization, DEFAULT_DEVICE_SIZES, DEFAULT_IMAGE_SIZES } from "vinext/server/image-optimization";
import handler from "vinext/server/app-router-entry";

interface Env {
  ASSETS?: Fetcher;
  IMAGES: {
    input(stream: ReadableStream): {
      transform(options: Record<string, unknown>): {
        output(options: { format: string; quality: number }): Promise<{ response(): Response }>;
      };
    };
  };
}

interface ExecutionContext {
  waitUntil(promise: Promise<unknown>): void;
  passThroughOnException(): void;
}

function withSecurityHeaders(response: Response): Response {
  const headers = new Headers(response.headers);
  headers.set("X-Content-Type-Options", "nosniff");
  headers.set("X-Frame-Options", "DENY");
  headers.set("Referrer-Policy", "strict-origin-when-cross-origin");
  headers.set("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=()");
  headers.set("Cross-Origin-Opener-Policy", "same-origin");
  headers.set("Cross-Origin-Resource-Policy", "same-origin");

  return new Response(response.body, {
    status: response.status,
    statusText: response.statusText,
    headers,
  });
}

const WEBGL_RELEASE_PREFIX = "/play/releases/";

function localWebGLFallback(url: URL): Promise<Response | null> {
  if (!url.pathname.startsWith(WEBGL_RELEASE_PREFIX)) return Promise.resolve(null);
  return import("node:fs/promises")
    .then(async ({ readFile }) => {
      const relativePath = url.pathname.slice(1) || "index.html";
      const assetPath = relativePath.endsWith("/") ? `${relativePath}index.html` : relativePath;
      const body = await readFile(new URL(`../client/${assetPath}`, import.meta.url));
      return new Response(body, { status: 200 });
    })
    .catch((error: NodeJS.ErrnoException) => {
      if (error.code === "ENOENT") return null;
      throw error;
    });
}

function withWebGLHeaders(request: Request, response: Response): Response {
  const url = new URL(request.url);
  const headers = new Headers(response.headers);

  if (url.pathname.startsWith(WEBGL_RELEASE_PREFIX)) {
    headers.set("Cache-Control", "public, max-age=31536000, immutable");
  } else if (url.pathname === "/play" || url.pathname === "/play/" || url.pathname.startsWith("/data/")) {
    headers.set("Cache-Control", "no-cache, max-age=0, must-revalidate");
  }

  if (url.pathname.endsWith(".wasm")) {
    headers.set("Content-Type", "application/wasm");
  } else if (url.pathname.endsWith(".data")) {
    headers.set("Content-Type", "application/octet-stream");
  } else if (url.pathname.endsWith(".js")) {
    headers.set("Content-Type", "text/javascript; charset=utf-8");
  } else if (url.pathname.endsWith("/") || url.pathname.endsWith(".html")) {
    headers.set("Content-Type", "text/html; charset=utf-8");
  } else if (url.pathname.endsWith(".css")) {
    headers.set("Content-Type", "text/css; charset=utf-8");
  } else if (url.pathname.endsWith(".png")) {
    headers.set("Content-Type", "image/png");
  } else if (url.pathname.endsWith(".ico")) {
    headers.set("Content-Type", "image/x-icon");
  } else if (url.pathname.endsWith(".json")) {
    headers.set("Content-Type", "application/json; charset=utf-8");
  }

  return new Response(response.body, {
    status: response.status,
    statusText: response.statusText,
    headers,
  });
}

const worker = {
  async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
    const url = new URL(request.url);

    if (url.pathname === "/_vinext/image") {
      const allowedWidths = [...DEFAULT_DEVICE_SIZES, ...DEFAULT_IMAGE_SIZES];
      const imageResponse = await handleImageOptimization(
        request,
        {
          fetchAsset: (path) => env.ASSETS.fetch(new Request(new URL(path, request.url))),
          transformImage: async (body, { width, format, quality }) => {
            const result = await env.IMAGES.input(body)
              .transform(width > 0 ? { width } : {})
              .output({ format, quality });
            return result.response();
          },
        },
        allowedWidths,
      );
      return withSecurityHeaders(imageResponse);
    }

    if (url.pathname.startsWith(WEBGL_RELEASE_PREFIX)) {
      const assetResponse = env?.ASSETS
        ? await env.ASSETS.fetch(request)
        : await localWebGLFallback(url);
      if (assetResponse && assetResponse.status !== 404) {
        return withSecurityHeaders(withWebGLHeaders(request, assetResponse));
      }
    }

    const response = await handler.fetch(request, env, ctx);
    return withSecurityHeaders(withWebGLHeaders(request, response));
  },
};

export default worker;
