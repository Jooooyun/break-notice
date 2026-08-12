import releaseHistory from "../../../data/webgl-releases.json";

export async function GET() {
  return Response.json(releaseHistory, {
    headers: {
      "Cache-Control": "no-cache, max-age=0, must-revalidate",
    },
  });
}
