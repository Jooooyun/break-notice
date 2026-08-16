#!/usr/bin/env python3
"""Fail-closed byte and path isolation audit for Rebuild v004."""

from __future__ import annotations

import hashlib
import json
import subprocess
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
REPOSITORY = ROOT.parents[3]
PRODUCTION_BUILDER = ROOT / "Tools" / "build_impact_ladder.py"
SOURCES = ROOT / "Sources" / "Lossless"
COMPOSITES = ROOT / "Renders" / "Composites"
STEMS = ROOT / "Renders" / "Stems"
REFERENCE_ONLY = ROOT / "Renders" / "ReferenceOnly"
EVIDENCE = ROOT / "Renders" / "Evidence"
PRIOR_PACK = ROOT.parent / "Rebuild_v003"
PRIOR_AUDIO_COMMIT = "071315b"
EXPECTED_PRIOR_REVIEW_AUDIO_COUNT = 68
AUDIO_SUFFIXES = {".wav", ".flac", ".aiff", ".aif", ".ogg", ".mp3"}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def audio_files(root: Path) -> list[Path]:
    if not root.exists():
        return []
    return sorted(path for path in root.rglob("*") if path.is_file() and path.suffix.lower() in AUDIO_SUFFIXES)


def run_git(*arguments: str) -> str:
    return subprocess.check_output(["git", *arguments], cwd=REPOSITORY, text=True, encoding="utf-8")


def main() -> None:
    failures: list[str] = []
    builder_text = PRODUCTION_BUILDER.read_text(encoding="utf-8")
    forbidden_token = "Rebuild_" + "v003"
    if forbidden_token in builder_text:
        failures.append("production builder names the prior review pack")

    source_files = audio_files(SOURCES)
    lossy = [str(path) for path in source_files if path.suffix.lower() in {".ogg", ".mp3"}]
    if lossy:
        failures.append("lossy production sources present")

    production_files = source_files + audio_files(COMPOSITES) + audio_files(STEMS)
    prior_files = audio_files(PRIOR_PACK)
    if len(prior_files) != EXPECTED_PRIOR_REVIEW_AUDIO_COUNT:
        failures.append(
            f"prior review baseline count mismatch: expected {EXPECTED_PRIOR_REVIEW_AUDIO_COUNT}, got {len(prior_files)}"
        )
    production_hashes = {sha256(path): str(path.relative_to(ROOT)) for path in production_files}
    prior_hashes = {sha256(path): str(path.relative_to(PRIOR_PACK)) for path in prior_files}
    intersections = sorted(set(production_hashes) & set(prior_hashes))
    if intersections:
        failures.append("v004 production bytes intersect prior review audio")

    tree_lines = run_git("ls-tree", "-r", PRIOR_AUDIO_COMMIT).splitlines()
    old_blob_ids = {line.split()[2] for line in tree_lines if len(line.split()) >= 3}
    current_old_intersections = []
    for path in production_files:
        blob = run_git("hash-object", str(path)).strip()
        if blob in old_blob_ids:
            current_old_intersections.append(str(path.relative_to(ROOT)))
    if current_old_intersections:
        failures.append("v004 production bytes intersect prior v002 commit tree")

    reference_files = audio_files(REFERENCE_ONLY)
    report = {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "production_file_count": len(production_files),
        "source_file_count": len(source_files),
        "prior_review_audio_count": len(prior_files),
        "expected_prior_review_audio_count": EXPECTED_PRIOR_REVIEW_AUDIO_COUNT,
        "prior_review_sha256_intersection_count": len(intersections),
        "prior_v002_git_blob_intersection_count": len(current_old_intersections),
        "prior_v002_git_blob_intersections": current_old_intersections,
        "lossy_production_source_count": len(lossy),
        "reference_only_audio_count": len(reference_files),
        "reference_carveout": [str(path.relative_to(ROOT)) for path in reference_files],
        "production_builder_prior_pack_token_count": builder_text.count(forbidden_token),
    }
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "ISOLATION_AUDIT_REVIEW.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    if failures:
        raise RuntimeError("Isolation audit failed: " + "; ".join(failures))
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
