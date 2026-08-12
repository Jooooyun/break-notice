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
      const assetPath = relativePath.endsWith("/")
        ? `${relativePath}__webgl-index.bin`
        : relativePath.endsWith("/index.html")
          ? `${relativePath.slice(0, -"index.html".length)}__webgl-index.bin`
          : relativePath;
      const body = await readFile(new URL(`../client/${assetPath}`, import.meta.url));
      return new Response(body, { status: 200 });
    })
    .catch((error: NodeJS.ErrnoException) => {
      if (error.code === "ENOENT") return null;
      throw error;
    });
}

async function fetchWebGLAsset(request: Request, env: Env): Promise<Response | null> {
  const url = new URL(request.url);
  if (env?.ASSETS) {
    const assetRequestUrl = new URL(url);
    if (assetRequestUrl.pathname.endsWith("/")) {
      assetRequestUrl.pathname += "__webgl-index.bin";
    } else if (assetRequestUrl.pathname.endsWith("/index.html")) {
      assetRequestUrl.pathname = `${assetRequestUrl.pathname.slice(0, -"index.html".length)}__webgl-index.bin`;
    }
    const response = await env.ASSETS.fetch(new Request(assetRequestUrl, request));
    return response.status === 404 ? null : response;
  }
  return localWebGLFallback(url);
}

async function fetchChunkedWebGLAsset(request: Request, env: Env): Promise<Response | null> {
  const requestUrl = new URL(request.url);
  const manifestUrl = new URL(requestUrl);
  manifestUrl.pathname = `${manifestUrl.pathname}.parts.json`;
  const manifestResponse = await fetchWebGLAsset(new Request(manifestUrl), env);
  if (!manifestResponse) return null;

  const manifest = (await manifestResponse.json()) as {
    bytes: number;
    parts: Array<{ path: string; bytes: number }>;
  };
  let partIndex = 0;
  const stream = new ReadableStream<Uint8Array>({
    async pull(controller) {
      if (partIndex >= manifest.parts.length) {
        controller.close();
        return;
      }
      const part = manifest.parts[partIndex];
      const partUrl = new URL(requestUrl);
      partUrl.pathname = `${requestUrl.pathname.slice(0, requestUrl.pathname.lastIndexOf("/") + 1)}${part.path}`;
      const partResponse = await fetchWebGLAsset(new Request(partUrl), env);
      if (!partResponse) {
        controller.error(new Error(`Missing WebGL transport part: ${part.path}`));
        return;
      }
      const bytes = new Uint8Array(await partResponse.arrayBuffer());
      if (bytes.byteLength !== part.bytes) {
        controller.error(new Error(`WebGL transport part byte mismatch: ${part.path}`));
        return;
      }
      controller.enqueue(bytes);
      partIndex += 1;
    },
  });

  return new Response(stream, {
    status: 200,
    headers: { "Content-Length": String(manifest.bytes) },
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
      const assetResponse =
        (await fetchWebGLAsset(request, env)) ?? (await fetchChunkedWebGLAsset(request, env));
      if (assetResponse) {
        return withSecurityHeaders(withWebGLHeaders(request, assetResponse));
      }
    }

    const response = await handler.fetch(request, env, ctx);
    return withSecurityHeaders(withWebGLHeaders(request, response));
  },
};

export default worker;
