import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "404 — 페이지를 찾을 수 없습니다",
  robots: {
    index: false,
    follow: false,
  },
};

export default function NotFound() {
  return (
    <main className="not-found">
      <section className="not-found-card" aria-labelledby="not-found-title">
        <span className="not-found-code" aria-hidden="true">404</span>
        <p className="eyebrow">404 / NOTHING HERE</p>
        <h1 id="not-found-title">PAGE NOT FOUND</h1>
        <p>
          요청하신 페이지를 찾을 수 없습니다. 주소를 확인하거나
          BREAK NOTICE 홈페이지로 돌아가 주세요.
        </p>
        <Link href="/">BREAK NOTICE 홈으로 돌아가기</Link>
      </section>
    </main>
  );
}
