#!/usr/bin/env python3
"""Fail-closed audit of the three sequential T3 v005 review ZIPs."""

from __future__ import annotations

import hashlib
import json
import zipfile
from pathlib import Path, PurePosixPath


ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / "Renders" / "Package"
EVIDENCE = ROOT / "Renders" / "Evidence"
MANIFEST = PACKAGE / "PHASE_PACKAGE_MANIFEST_REVIEW.json"

PAIR_ZIP = "BREAK_NOTICE_T3_V005_PAIR_BLIND_PHASE1.zip"
VARIATION_ZIP = "BREAK_NOTICE_T3_V005_VARIATION_BLIND_PHASE2.zip"
DETAIL_ZIP = "BREAK_NOTICE_T3_V005_DEBLIND_DETAIL_PHASE3.zip"
EXPECTED_ZIPS = {PAIR_ZIP, VARIATION_ZIP, DETAIL_ZIP}
EXPECTED_PHASE1 = {
    "README.md",
    "PHASE1_RESPONSE.md",
    "T3_PAIR_M_BLIND_REVIEW.wav",
    "T3_PAIR_M_BLIND_REVIEW.mp4",
    "T3_PAIR_N_BLIND_REVIEW.wav",
    "T3_PAIR_N_BLIND_REVIEW.mp4",
}
EXPECTED_PHASE2 = {
    "README.md",
    "PHASE2_RESPONSE.md",
    "v01/T3_V01_A_BLIND_REVIEW.wav",
    "v01/T3_V01_A_BLIND_REVIEW.mp4",
    "v01/T3_V01_B_BLIND_REVIEW.wav",
    "v01/T3_V01_B_BLIND_REVIEW.mp4",
    "v02/T3_V02_P_BLIND_REVIEW.wav",
    "v02/T3_V02_P_BLIND_REVIEW.mp4",
    "v02/T3_V02_Q_BLIND_REVIEW.wav",
    "v02/T3_V02_Q_BLIND_REVIEW.mp4",
}
PRIVATE_MAPPING = "Private/T3_V005_BLIND_MAPPING_PRIVATE.json"
REQUIRED_PHASE3_DOCS = {
    "Docs/REBUILD_DIRECTION_REVIEW.md",
    "Docs/SOURCE_PROVENANCE.md",
    "Docs/REVIEW_CHECKLIST.md",
    "Docs/LISTENING_RESPONSE_TEMPLATE.md",
    "Docs/REHEARSAL_LOG.md",
}
REQUIRED_PHASE3_EVIDENCE = {
    "Evidence/CUE_MANIFEST_REVIEW.json",
    "Evidence/SOURCE_LEDGER_REVIEW.json",
    "Evidence/TECHNICAL_QA_REVIEW.json",
    "Evidence/T3_V005_WAVEFORM_SPECTROGRAM_REVIEW.png",
    "Evidence/T3_V005_REVIEW_ASSEMBLY_MANIFEST.json",
    "Evidence/T3_V005_VIDEO_QA_REVIEW.json",
    "Evidence/ISOLATION_AUDIT_REVIEW.json",
}
LEAK_TOKENS = (
    b"private",
    b"mapping",
    b"answer_key",
    b"approved_y_reference",
    b"new_v005_rc",
    b"arcadeedge",
    b"composites/",
    b"stems/",
    b"sources/",
    b"metrics",
)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def safe_entries(path: Path) -> tuple[list[str], dict[str, bytes]]:
    payloads: dict[str, bytes] = {}
    with zipfile.ZipFile(path) as archive:
        bad = archive.testzip()
        if bad is not None:
            raise RuntimeError(f"CRC failure in {path.name}: {bad}")
        infos = [info for info in archive.infolist() if not info.is_dir()]
        names = [info.filename.replace("\\", "/") for info in infos]
        if len(names) != len(set(name.casefold() for name in names)):
            raise RuntimeError(f"Duplicate/case-colliding ZIP entry: {path.name}")
        for name in names:
            pure = PurePosixPath(name)
            if pure.is_absolute() or ".." in pure.parts or "" in pure.parts:
                raise RuntimeError(f"Unsafe ZIP path: {path.name}:{name}")
            payloads[name] = archive.read(name)
    return names, payloads


def local_file(local_path: str) -> Path:
    resolved = (ROOT / local_path).resolve()
    if not resolved.is_relative_to(ROOT.resolve()):
        raise RuntimeError(f"Manifest local path escapes v005 root: {local_path}")
    return resolved


def verify_manifest_bytes(
    zip_path: Path, names: list[str], payloads: dict[str, bytes], row: dict[str, object]
) -> int:
    entries = row.get("entries")
    if not isinstance(entries, dict) or set(entries) != set(names):
        raise RuntimeError(f"Manifest entry set mismatch: {zip_path.name}")
    if row.get("sha256") != sha256(zip_path):
        raise RuntimeError(f"ZIP SHA manifest mismatch: {zip_path.name}")
    checked = 0
    for name in names:
        entry = entries[name]
        local = local_file(str(entry["local_path"]))
        if not local.is_file():
            raise RuntimeError(f"Local package source missing: {local}")
        payload_hash = hashlib.sha256(payloads[name]).hexdigest()
        if payload_hash != sha256(local) or payload_hash != entry.get("sha256"):
            raise RuntimeError(f"ZIP entry differs from current local bytes: {zip_path.name}:{name}")
        checked += 1
    return checked


def blind_leaks(names: list[str], payloads: dict[str, bytes]) -> list[str]:
    leaks: list[str] = []
    for name in names:
        lowered_name = name.casefold().encode("utf-8")
        for token in LEAK_TOKENS:
            if token in lowered_name:
                leaks.append(f"filename:{name}:{token.decode()}")
        if Path(name).suffix.lower() in {".md", ".txt", ".json", ".csv"}:
            lowered_payload = payloads[name].lower()
            for token in LEAK_TOKENS:
                if token in lowered_payload:
                    leaks.append(f"content:{name}:{token.decode()}")
    return leaks


def main() -> None:
    failures: list[str] = []
    if not MANIFEST.is_file():
        raise RuntimeError(f"Package manifest missing: {MANIFEST}")
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    manifest_packages = manifest.get("packages", {})
    actual_zips = {path.name for path in PACKAGE.glob("*.zip")}
    if actual_zips != EXPECTED_ZIPS:
        failures.append(f"exact ZIP set mismatch: expected {sorted(EXPECTED_ZIPS)}, got {sorted(actual_zips)}")
    if manifest.get("status") != "THREE_PHASE_READY":
        failures.append(f"package manifest not final: {manifest.get('status')}")

    rows: dict[str, object] = {}
    for zip_name, exact in ((PAIR_ZIP, EXPECTED_PHASE1), (VARIATION_ZIP, EXPECTED_PHASE2)):
        path = PACKAGE / zip_name
        if not path.is_file():
            continue
        try:
            names, payloads = safe_entries(path)
            if set(names) != exact:
                failures.append(f"{zip_name} exact entry contract failed")
            leaks = blind_leaks(names, payloads)
            if leaks:
                failures.append(f"{zip_name} leaks deblind/detail identity: {leaks}")
            checked = verify_manifest_bytes(path, names, payloads, manifest_packages.get(zip_name, {}))
            rows[zip_name] = {
                "entries": len(names),
                "local_byte_matches": checked,
                "sha256": sha256(path),
                "leaks": leaks,
            }
        except Exception as error:
            failures.append(f"{zip_name}: {error}")

    detail_path = PACKAGE / DETAIL_ZIP
    if detail_path.is_file():
        try:
            names, payloads = safe_entries(detail_path)
            name_set = set(names)
            if PRIVATE_MAPPING not in name_set:
                failures.append("Phase 3 private mapping missing")
            if not REQUIRED_PHASE3_DOCS.issubset(name_set):
                failures.append("Phase 3 required docs missing")
            if not REQUIRED_PHASE3_EVIDENCE.issubset(name_set):
                failures.append("Phase 3 required evidence missing")
            composite_count = sum(name.startswith("Composites/") and name.endswith(".wav") for name in names)
            stem_count = sum(name.startswith("Stems/") and name.endswith(".wav") for name in names)
            provenance_count = sum(name.startswith("Provenance/Evidence/") for name in names)
            if composite_count != 2 or stem_count != 10 or provenance_count < 1:
                failures.append(
                    f"Phase 3 detail counts failed: composites={composite_count} stems={stem_count} provenance={provenance_count}"
                )
            checked = verify_manifest_bytes(
                detail_path, names, payloads, manifest_packages.get(DETAIL_ZIP, {})
            )
            rows[DETAIL_ZIP] = {
                "entries": len(names),
                "local_byte_matches": checked,
                "sha256": sha256(detail_path),
                "composite_count": composite_count,
                "stem_count": stem_count,
                "provenance_evidence_count": provenance_count,
            }
        except Exception as error:
            failures.append(f"{DETAIL_ZIP}: {error}")

    report = {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "exact_zip_count": len(actual_zips),
        "safe_paths_crc_hash_manifest_local_byte_identity": "PASS" if not failures else "FAIL",
        "phase1_phase2_identity_leak_contract": "PASS" if not any("leak" in row for row in failures) else "FAIL",
        "release_order": "Phase 1 verdict -> Phase 2 verdicts -> Phase 3 deblind/detail",
        "packages": rows,
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / "PACKAGE_AUDIT_REVIEW.json").write_text(
        json.dumps(report, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(report, indent=2))
    if failures:
        raise RuntimeError("Package audit failed: " + "; ".join(failures))


if __name__ == "__main__":
    main()
