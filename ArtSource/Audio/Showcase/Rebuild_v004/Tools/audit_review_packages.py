#!/usr/bin/env python3
"""Fail-closed audit for the sequential WP02 blind-review ZIPs."""

from __future__ import annotations

import hashlib
import json
import zipfile
from pathlib import Path, PurePosixPath


ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / "Renders" / "Package"
EVIDENCE = ROOT / "Renders" / "Evidence"
HASH_FILE = PACKAGE / "PHASE_PACKAGE_SHA256.txt"

PHASES = {
    "BREAK_NOTICE_WP02_T3_BLIND_PHASE1_v004.zip": {
        "LISTENING_RESPONSE_TEMPLATE.md",
        "README.md",
        "T3_X_PAIR_BLIND_REVIEW.wav",
        "T3_X_PAIR_COMMON_VIDEO_REVIEW.mp4",
        "T3_Y_PAIR_BLIND_REVIEW.wav",
        "T3_Y_PAIR_COMMON_VIDEO_REVIEW.mp4",
    },
    "BREAK_NOTICE_WP02_LADDER_BLIND_PHASE2_v004.zip": {
        "LISTENING_RESPONSE_TEMPLATE.md",
        "MATERIAL_10_TRIAL_BLIND_REVIEW.wav",
        "README.md",
        "TIER_10_TRIAL_BLIND_REVIEW.wav",
    },
}
PHASE3 = "BREAK_NOTICE_WP02_DETAIL_EVIDENCE_PHASE3_v004.zip"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def entries(path: Path) -> list[str]:
    with zipfile.ZipFile(path) as archive:
        bad = archive.testzip()
        if bad is not None:
            raise RuntimeError(f"CRC failure in {path.name}: {bad}")
        names = [info.filename.replace("\\", "/") for info in archive.infolist() if not info.is_dir()]
    if len(names) != len(set(name.casefold() for name in names)):
        raise RuntimeError(f"duplicate/case-colliding ZIP entry in {path.name}")
    for name in names:
        pure = PurePosixPath(name)
        if pure.is_absolute() or ".." in pure.parts:
            raise RuntimeError(f"unsafe ZIP path in {path.name}: {name}")
    return names


def local_source(phase: int, name: str) -> Path:
    if name == "README.md":
        return PACKAGE / f"PHASE{phase}_README.md"
    if name == "LISTENING_RESPONSE_TEMPLATE.md":
        return ROOT / name
    if phase == 1:
        if name.endswith(".mp4"):
            return ROOT / "Renders" / "ReferenceOnly" / "Videos" / name
        return ROOT / "Renders" / "ReferenceOnly" / name
    if phase == 2:
        return ROOT / "Renders" / "Derived" / name
    prefix_routes = {
        "Docs": ROOT,
        "Listening": ROOT / "Renders" / "Derived",
        "T3_Reference": ROOT / "Renders" / "ReferenceOnly",
        "Individual_Composites": ROOT / "Renders" / "Composites",
        "Stems": ROOT / "Renders" / "Stems",
        "Private": ROOT / "Renders" / "Private",
    }
    prefix, relative = name.split("/", 1)
    if prefix == "T3_Reference" and relative.endswith(".mp4"):
        return ROOT / "Renders" / "ReferenceOnly" / "Videos" / relative
    if prefix == "Evidence" and relative == "REFERENCE_ONLY_MANIFEST_REVIEW.json":
        return ROOT / "Renders" / "ReferenceOnly" / relative
    if prefix == "Evidence":
        return ROOT / "Renders" / "Evidence" / relative
    return prefix_routes[prefix] / relative


def verify_local_bytes(path: Path, phase: int, names: list[str]) -> int:
    checked = 0
    with zipfile.ZipFile(path) as archive:
        for name in names:
            local = local_source(phase, name)
            if not local.is_file():
                raise RuntimeError(f"local package source missing for {path.name}: {local}")
            if hashlib.sha256(archive.read(name)).hexdigest() != sha256(local):
                raise RuntimeError(f"ZIP entry differs from current local source: {path.name}:{name}")
            checked += 1
    return checked


def read_hashes() -> dict[str, str]:
    result: dict[str, str] = {}
    for line in HASH_FILE.read_text(encoding="utf-8").splitlines():
        digest, name = line.split(maxsplit=1)
        result[name] = digest
    return result


def main() -> None:
    failures: list[str] = []
    expected_hashes = read_hashes()
    expected_zip_names = set(PHASES) | {PHASE3}
    actual_zip_names = {path.name for path in PACKAGE.glob("*.zip")}
    if actual_zip_names != expected_zip_names:
        failures.append(
            f"package directory ZIP set mismatch: expected {sorted(expected_zip_names)}, got {sorted(actual_zip_names)}"
        )
    rows: dict[str, object] = {}
    for phase, (name, exact) in enumerate(PHASES.items(), start=1):
        path = PACKAGE / name
        actual_entries = entries(path)
        if set(actual_entries) != exact:
            failures.append(f"{name} exact entry contract failed")
        lowered = "\n".join(actual_entries).lower()
        if any(token in lowered for token in ("private/", "stems/", "individual_composites/", "evidence/")):
            failures.append(f"{name} leaks deblind/detail assets")
        actual_hash = sha256(path)
        if expected_hashes.get(name) != actual_hash:
            failures.append(f"{name} SHA-256 manifest mismatch")
        local_byte_matches = verify_local_bytes(path, phase, actual_entries)
        rows[name] = {"entries": len(actual_entries), "local_byte_matches": local_byte_matches, "sha256": actual_hash}

    phase3_path = PACKAGE / PHASE3
    phase3_entries = entries(phase3_path)
    phase3_hash = sha256(phase3_path)
    if expected_hashes.get(PHASE3) != phase3_hash:
        failures.append("Phase 3 SHA-256 manifest mismatch")
    prefix_counts = {
        "individual_composites": sum(name.startswith("Individual_Composites/") for name in phase3_entries),
        "stems": sum(name.startswith("Stems/") for name in phase3_entries),
        "private": sum(name.startswith("Private/") for name in phase3_entries),
        "evidence": sum(name.startswith("Evidence/") for name in phase3_entries),
    }
    if prefix_counts != {"individual_composites": 11, "stems": 55, "private": 2, "evidence": 8}:
        failures.append(f"Phase 3 deblind/detail counts failed: {prefix_counts}")
    required_phase3 = {
        "Listening/IMPACT_LADDER_ACTUAL_DYNAMIC_REVIEW.wav",
        "Listening/IMPACT_LADDER_ACTUAL_DYNAMIC_REVIEW.mp4",
        "Listening/T1_HAND_12_REPEAT_STRESS_REVIEW.wav",
        "Private/LADDER_QUIZ_MAPPING_PRIVATE.json",
        "Private/T3_REFERENCE_COMPARISON_MAPPING_PRIVATE.json",
    }
    if not required_phase3.issubset(phase3_entries):
        failures.append("Phase 3 required deblind/review files missing")
    phase3_local_byte_matches = verify_local_bytes(phase3_path, 3, phase3_entries)
    rows[PHASE3] = {
        "entries": len(phase3_entries),
        "local_byte_matches": phase3_local_byte_matches,
        "sha256": phase3_hash,
        "prefix_counts": prefix_counts,
    }

    report = {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "sequential_release": "Phase 1 answer -> Phase 2 answers -> Phase 3 deblind/detail",
        "packages": rows,
    }
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "PACKAGE_AUDIT_REVIEW.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))
    if failures:
        raise RuntimeError("Package audit failed")


if __name__ == "__main__":
    main()
