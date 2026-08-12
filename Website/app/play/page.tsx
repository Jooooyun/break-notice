import { redirect } from "next/navigation";
import releaseHistory from "../../data/webgl-releases.json";

export default function Play() {
  redirect(`/play/releases/${releaseHistory.latestBuildGuid}/`);
}
