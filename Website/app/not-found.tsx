import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "404 — Page Not Found",
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
        <p className="eyebrow">ROUTE REJECTED / SAFE STATE</p>
        <h1 id="not-found-title">PAGE NOT FOUND</h1>
        <p>
          요청한 경로는 이 Showcase release에 없습니다. 잘못된 링크는 다른 기능으로
          우회하지 않고 안전하게 거부됩니다.
        </p>
        <Link href="/">BREAK NOTICE로 돌아가기</Link>
      </section>
    </main>
  );
}
