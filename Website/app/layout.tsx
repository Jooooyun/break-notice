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
    "야근 중인 시설 직원이 사무실 물건을 집어 던지고 동료와 함께 난장판을 헤쳐 나가는 3인칭 물리 액션 게임, BREAK NOTICE의 공식 Showcase 사이트입니다.",
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
    description: "Grounded world. Absurd physics. 사무실 물건과 동료를 활용하는 3인칭 물리 액션 Showcase.",
    images: [
      {
        url: "/media/PGT_01_Gameplay_Camera_v002_FINAL.png",
        width: 1672,
        height: 941,
        alt: "BREAK NOTICE의 3인칭 사무실 액션 콘셉트 이미지",
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
