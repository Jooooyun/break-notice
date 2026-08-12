import type { Metadata, Viewport } from "next";
import "./globals.css";

const siteUrl = "https://zero-defect-steveyun.stevejoyun.chatgpt.site";

export const metadata: Metadata = {
  metadataBase: new URL(siteUrl),
  title: {
    default: "BREAK NOTICE — Showcase",
    template: "%s — BREAK NOTICE",
  },
  description:
    "평범한 사무용 물체와 한 명의 동료를 활용하는 3인칭 물리 액션 프로토타입 BREAK NOTICE의 공식 Showcase 사이트.",
  applicationName: "BREAK NOTICE Showcase",
  alternates: {
    canonical: "/",
  },
  robots: {
    index: true,
    follow: true,
  },
  openGraph: {
    type: "website",
    locale: "ko_KR",
    url: siteUrl,
    siteName: "BREAK NOTICE",
    title: "BREAK NOTICE — Showcase",
    description: "Grounded world. Absurd physics. 현재 검증된 브라우저 프로토타입 기반 Showcase.",
    images: [
      {
        url: "/media/PGT_01_Gameplay_Camera_v002_FINAL.png",
        width: 1672,
        height: 941,
        alt: "BREAK NOTICE 승인 프로덕션 비주얼 레퍼런스",
      },
    ],
  },
  twitter: {
    card: "summary_large_image",
    title: "BREAK NOTICE — Showcase",
    description: "Grounded world. Absurd physics.",
    images: ["/media/PGT_01_Gameplay_Camera_v002_FINAL.png"],
  },
};

export const viewport: Viewport = {
  width: "device-width",
  initialScale: 1,
  colorScheme: "dark",
  themeColor: "#08090b",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="ko">
      <body>{children}</body>
    </html>
  );
}
