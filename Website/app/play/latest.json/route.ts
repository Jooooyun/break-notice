import releaseHistory from "../../../data/webgl-releases.json";

export async function GET() {
  const release = releaseHistory.releases.find(
    (entry) => entry.buildGuid === releaseHistory.latestBuildGuid,
  );

  if (!release) {
    return Response.json({ error: "Promoted WebGL release is missing." }, { status: 500 });
  }

  return Response.json(
    {
      buildGuid: release.buildGuid,
      sourceSha: release.sourceSha,
      releasePath: `/play/releases/${release.buildGuid}/`,
      publicUrl: release.publicUrl,
    },
    {
      headers: {
        "Cache-Control": "no-cache, max-age=0, must-revalidate",
      },
    },
  );
}
