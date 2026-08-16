#!/usr/bin/env python3
"""Fail-closed byte/path isolation audit for the T3 v005 rebuild."""

from __future__ import annotations

import hashlib
import importlib.util
import json
import subprocess
from pathlib import Path

import soundfile as sf


ROOT = Path(__file__).resolve().parents[1]
REPOSITORY = ROOT.parents[3]
TOOLS = ROOT / "Tools"
PRODUCTION_BUILDER = TOOLS / "build_t3_rebuild.py"
ASSEMBLER = TOOLS / "assemble_blind_review.py"
SOURCES = ROOT / "Sources" / "Lossless"
COMPOSITES = ROOT / "Renders" / "Composites"
STEMS = ROOT / "Renders" / "Stems"
EVIDENCE = ROOT / "Renders" / "Evidence"
PRIOR_PACKS = (ROOT.parent / "Rebuild_v003", ROOT.parent / "Rebuild_v004")
HISTORICAL_COMMITS = {
    "v001": "f903e351195731f490fbb1ec54969c02077f3dac",
    "v002": "071315b311621081e9b5bc9cbc5f6917177d09ea",
}
AUDIO_SUFFIXES = {".wav", ".flac", ".aiff", ".aif", ".ogg", ".mp3", ".m4a", ".aac"}
LOSSLESS_SUFFIXES = {".wav", ".flac", ".aiff", ".aif"}

EXPECTED_REFERENCE_ALLOWLIST = {
    "B_ArcadeEdge_v01_REVIEW.wav": "9dd84fad2aeade3dbfc5a0dbb0e49a505da554abcea6185803070e5e14d3708f",
    "B_ArcadeEdge_v02_REVIEW.wav": "30335cd8498b10cf3c527965a20a3a3ea0ef6bf3fd71c49e848d8e93cb6292bb",
}
EXPECTED_COMMON_VIDEO = (
    ROOT.parent
    / "Rebuild_v004"
    / "Renders"
    / "ReferenceOnly"
    / "Videos"
    / "T3_Y_PAIR_COMMON_VIDEO_REVIEW.mp4"
).resolve()
EXPECTED_COMMON_VIDEO_SHA256 = "e98f75ba9f23aaefea6b925eb5cf7655b4d37310c3f56104576d5f927a229175"


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


def git(*arguments: str) -> str:
    return subprocess.check_output(
        ["git", *arguments], cwd=REPOSITORY, text=True, encoding="utf-8", errors="replace"
    )


def load_assembler():
    spec = importlib.util.spec_from_file_location("v005_assemble_blind_review", ASSEMBLER)
    if spec is None or spec.loader is None:
        raise RuntimeError("Cannot import assembler for allowlist audit")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def main() -> None:
    failures: list[str] = []
    builder_text = PRODUCTION_BUILDER.read_text(encoding="utf-8")
    assembler_text = ASSEMBLER.read_text(encoding="utf-8")

    forbidden_builder_tokens = (
        "Rebuild_v003",
        "Rebuild_v004",
        "B_ArcadeEdge",
        "9dd84fad2aeade3dbfc5a0dbb0e49a505da554abcea6185803070e5e14d3708f",
        "30335cd8498b10cf3c527965a20a3a3ea0ef6bf3fd71c49e848d8e93cb6292bb",
        "T3_Y_PAIR_COMMON_VIDEO_REVIEW.mp4",
        "ReferenceOnly",
    )
    builder_token_counts = {token: builder_text.count(token) for token in forbidden_builder_tokens}
    if any(builder_token_counts.values()):
        failures.append("production builder contains a prior-pack path/name/hash token")

    assembler = load_assembler()
    assembler_allowlist_ok = assembler.REFERENCE_ALLOWLIST == EXPECTED_REFERENCE_ALLOWLIST
    assembler_reference_root_ok = Path(assembler.REFERENCE_ROOT).resolve() == (
        ROOT.parent / "Rebuild_v003" / "Renders" / "Candidates" / "B"
    ).resolve()
    assembler_common_video_ok = Path(assembler.COMMON_VIDEO).resolve() == EXPECTED_COMMON_VIDEO
    assembler_common_hash_ok = assembler.COMMON_VIDEO_EXPECTED_SHA256 == EXPECTED_COMMON_VIDEO_SHA256
    if not all(
        (assembler_allowlist_ok, assembler_reference_root_ok, assembler_common_video_ok, assembler_common_hash_ok)
    ):
        failures.append("reference assembler allowlist/path contract changed")
    assembler_prior_token_counts = {
        "Rebuild_v003": assembler_text.count("Rebuild_v003"),
        "Rebuild_v004": assembler_text.count("Rebuild_v004"),
        "approved_v01_hash": assembler_text.count(EXPECTED_REFERENCE_ALLOWLIST["B_ArcadeEdge_v01_REVIEW.wav"]),
        "approved_v02_hash": assembler_text.count(EXPECTED_REFERENCE_ALLOWLIST["B_ArcadeEdge_v02_REVIEW.wav"]),
        "common_video_hash": assembler_text.count(EXPECTED_COMMON_VIDEO_SHA256),
    }
    if assembler_prior_token_counts != {
        "Rebuild_v003": 1,
        "Rebuild_v004": 1,
        "approved_v01_hash": 1,
        "approved_v02_hash": 1,
        "common_video_hash": 1,
    }:
        failures.append("reference assembler prior-input token cardinality changed")

    source_files = audio_files(SOURCES)
    production_files = source_files + audio_files(COMPOSITES) + audio_files(STEMS)
    if not source_files:
        failures.append("no lossless sources found")
    if len(audio_files(COMPOSITES)) != 2 or len(audio_files(STEMS)) != 10:
        failures.append("production output count is not 2 composites + 10 stems")

    lossy_sources = [str(path.relative_to(ROOT)).replace("\\", "/") for path in source_files if path.suffix.lower() not in LOSSLESS_SUFFIXES]
    decode_failures: list[str] = []
    allowed_formats = {"WAV", "WAVEX", "FLAC", "AIFF"}
    allowed_subtypes = {"PCM_16", "PCM_24", "PCM_32", "FLOAT", "DOUBLE"}
    for path in source_files:
        try:
            info = sf.info(path)
            if info.format not in allowed_formats or info.subtype not in allowed_subtypes:
                decode_failures.append(f"{path.relative_to(ROOT)}: {info.format}/{info.subtype}")
        except Exception as error:  # fail closed on any decoder uncertainty
            decode_failures.append(f"{path.relative_to(ROOT)}: {error}")
    if lossy_sources or decode_failures:
        failures.append("production source lossless-decode contract failed")

    production_hashes = {sha256(path): str(path.relative_to(ROOT)).replace("\\", "/") for path in production_files}
    prior_rows: dict[str, object] = {}
    prior_union: dict[str, list[str]] = {}
    for prior in PRIOR_PACKS:
        files = audio_files(prior)
        hashes: dict[str, list[str]] = {}
        for path in files:
            digest = sha256(path)
            hashes.setdefault(digest, []).append(str(path.relative_to(prior)).replace("\\", "/"))
            prior_union.setdefault(digest, []).append(f"{prior.name}/{path.relative_to(prior).as_posix()}")
        intersection = sorted(set(production_hashes) & set(hashes))
        prior_rows[prior.name] = {
            "audio_file_count": len(files),
            "sha256_intersection_count": len(intersection),
            "intersections": [
                {"sha256": digest, "v005": production_hashes[digest], "prior": hashes[digest]}
                for digest in intersection
            ],
        }
        if intersection:
            failures.append(f"v005 production SHA intersects {prior.name} audio")

    historical_commit_rows: dict[str, object] = {}
    for label, commit in HISTORICAL_COMMITS.items():
        tree_lines = git("ls-tree", "-r", commit).splitlines()
        blob_ids = {line.split()[2] for line in tree_lines if len(line.split()) >= 3}
        intersections: list[dict[str, str]] = []
        for path in production_files:
            blob = git("hash-object", str(path)).strip()
            if blob in blob_ids:
                intersections.append(
                    {"path": str(path.relative_to(ROOT)).replace("\\", "/"), "git_blob": blob}
                )
        historical_commit_rows[label] = {
            "commit": commit,
            "git_blob_intersection_count": len(intersections),
            "git_blob_intersections": intersections,
        }
        if intersections:
            failures.append(f"v005 production bytes intersect the {label} commit tree")

    report = {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "scope": "Sources/Lossless + Renders/Composites + Renders/Stems only",
        "production_file_count": len(production_files),
        "source_file_count": len(source_files),
        "composite_file_count": len(audio_files(COMPOSITES)),
        "stem_file_count": len(audio_files(STEMS)),
        "lossy_source_count": len(lossy_sources),
        "lossy_sources": lossy_sources,
        "lossless_decode_failures": decode_failures,
        "prior_pack_sha256_audit": prior_rows,
        "prior_pack_union_intersection_count": len(set(production_hashes) & set(prior_union)),
        "historical_commit_blob_audit": historical_commit_rows,
        "production_builder_forbidden_token_counts": builder_token_counts,
        "assembler_exact_allowlist": {
            "audio_allowlist_ok": assembler_allowlist_ok,
            "reference_root_ok": assembler_reference_root_ok,
            "common_video_path_ok": assembler_common_video_ok,
            "common_video_hash_ok": assembler_common_hash_ok,
            "prior_input_token_counts": assembler_prior_token_counts,
        },
        "reference_carveout": "assembler only; approved Y v01/v02 WAV and common-picture video stream",
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "ISOLATION_AUDIT_REVIEW.json").write_text(
        json.dumps(report, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(report, indent=2))
    if failures:
        raise RuntimeError("Isolation audit failed: " + "; ".join(failures))


if __name__ == "__main__":
    main()
