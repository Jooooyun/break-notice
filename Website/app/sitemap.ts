import type { MetadataRoute } from "next";

export default function sitemap(): MetadataRoute.Sitemap {
  return [
    {
      url: "https://zero-defect-steveyun.stevejoyun.chatgpt.site",
      lastModified: new Date("2026-08-12T00:00:00+09:00"),
      changeFrequency: "weekly",
      priority: 1,
    },
  ];
}
