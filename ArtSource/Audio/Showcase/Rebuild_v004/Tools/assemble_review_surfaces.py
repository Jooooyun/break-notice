#!/usr/bin/env python3
"""Assemble the reference-only T3 blind comparison surface.

This script may read the approved Z review renders. It cannot write production
composites or stems and records the answer key only below Renders/Private.
"""

from __future__ import annotations

import hashlib
import json
import math
import random
from pathlib import Path

import numpy as np
import pyloudnorm as pyln
import soundfile as sf
from scipy import signal


SAMPLE_RATE = 48_000
PAIR_TARGET_LUFS = -20.0
ROOT = Path(__file__).resolve().parents[1]
REFERENCE_PACK = ROOT.parent / "Rebuild_v003"
REFERENCE_ROOT = REFERENCE_PACK / "Renders" / "Candidates" / "B"
RC_ROOT = ROOT / "Renders" / "Composites"
REFERENCE_ONLY = ROOT / "Renders" / "ReferenceOnly"
PRIVATE = ROOT / "Renders" / "Private"
EVIDENCE = ROOT / "Renders" / "Evidence"

REFERENCE_ALLOWLIST = {
    "B_ArcadeEdge_v01_REVIEW.wav": "9dd84fad2aeade3dbfc5a0dbb0e49a505da554abcea6185803070e5e14d3708f",
    "B_ArcadeEdge_v02_REVIEW.wav": "30335cd8498b10cf3c527965a20a3a3ea0ef6bf3fd71c49e848d8e93cb6292bb",
}
RC_FILES = (
    "T3_CHAIR_RUNNER_RC_v01_REVIEW.wav",
    "T3_CHAIR_RUNNER_RC_v02_REVIEW.wav",
)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def read_exact(path: Path) -> np.ndarray:
    data, rate = sf.read(path, always_2d=True, dtype="float64")
    if rate != SAMPLE_RATE or data.shape != (64_800, 2):
        raise RuntimeError(f"Unexpected review input format: {path} {rate} {data.shape}")
    return data


def dbfs(value: float) -> float:
    return 20.0 * math.log10(max(value, 1e-12))


def metrics(data: np.ndarray) -> dict[str, float]:
    contact = data[int(0.200 * SAMPLE_RATE):int(0.550 * SAMPLE_RATE)]
    oversampled = signal.resample_poly(data, 4, 1, axis=0)
    spectrum = np.abs(np.fft.rfft(np.mean(contact, axis=1)))
    frequencies = np.fft.rfftfreq(len(contact), 1.0 / SAMPLE_RATE)
    centroid = float(np.sum(frequencies * spectrum) / max(1e-12, np.sum(spectrum)))
    return {
        "integrated_lufs": float(pyln.Meter(SAMPLE_RATE).integrated_loudness(data)),
        "true_peak_dbfs": dbfs(float(np.max(np.abs(oversampled)))),
        "contact_0_350ms_rms_dbfs": dbfs(float(np.sqrt(np.mean(contact * contact)))),
        "contact_spectral_centroid_hz": centroid,
    }


def pair(clips: list[np.ndarray]) -> np.ndarray:
    gap = np.zeros((int(0.650 * SAMPLE_RATE), 2), dtype=np.float64)
    return np.concatenate((clips[0], gap, clips[1]), axis=0)


def loudness_match_pair(data: np.ndarray) -> np.ndarray:
    meter = pyln.Meter(SAMPLE_RATE)
    matched = pyln.normalize.loudness(data, meter.integrated_loudness(data), PAIR_TARGET_LUFS)
    if float(np.max(np.abs(matched))) >= 1.0:
        raise RuntimeError("Blind-pair loudness match would clip.")
    return matched


def main() -> None:
    for name, expected_hash in REFERENCE_ALLOWLIST.items():
        path = REFERENCE_ROOT / name
        if not path.is_file() or sha256(path) != expected_hash:
            raise RuntimeError(f"Reference allowlist mismatch: {path}")
    rc_before = {name: sha256(RC_ROOT / name) for name in RC_FILES}

    candidates = {
        "APPROVED_Z_REFERENCE": [read_exact(REFERENCE_ROOT / name) for name in REFERENCE_ALLOWLIST],
        "LOSSLESS_RC": [read_exact(RC_ROOT / name) for name in RC_FILES],
    }
    labels = ["X", "Y"]
    candidate_names = list(candidates)
    random.Random(0x5A5F56303034).shuffle(candidate_names)
    mapping = dict(zip(labels, candidate_names))

    REFERENCE_ONLY.mkdir(parents=True, exist_ok=True)
    PRIVATE.mkdir(parents=True, exist_ok=True)
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    for old in REFERENCE_ONLY.glob("*.wav"):
        old.unlink()

    safe_rows: list[dict[str, object]] = []
    for label in labels:
        output = REFERENCE_ONLY / f"T3_{label}_PAIR_BLIND_REVIEW.wav"
        matched_pair = loudness_match_pair(pair(candidates[mapping[label]]))
        sf.write(output, matched_pair, SAMPLE_RATE, subtype="PCM_24", format="WAV")
        safe_rows.append({
            "label": label,
            "sha256": sha256(output),
            "variations": 2,
            "integrated_lufs": metrics(matched_pair)["integrated_lufs"],
        })

    comparison = {
        "status": "REFERENCE_ONLY_HUMAN_VERDICT_REQUIRED",
        "approved_z_reference": {
            name: {"sha256": expected_hash, "metrics": metrics(read_exact(REFERENCE_ROOT / name))}
            for name, expected_hash in REFERENCE_ALLOWLIST.items()
        },
        "lossless_rc": {
            name: {"sha256": rc_before[name], "metrics": metrics(read_exact(RC_ROOT / name))}
            for name in RC_FILES
        },
        "automatic_conclusion": "NONE",
        "pending": "Human Owner decides whether the lossless RC sounds no worse than approved Z.",
    }
    (EVIDENCE / "T3_REFERENCE_COMPARISON_METRICS_REVIEW.json").write_text(json.dumps(comparison, indent=2), encoding="utf-8")
    (REFERENCE_ONLY / "REFERENCE_ONLY_MANIFEST_REVIEW.json").write_text(json.dumps({
        "status": "BLIND_MAPPING_SAFE",
        "pair_target_lufs": PAIR_TARGET_LUFS,
        "pairs": safe_rows,
        "warning": "One pair is the approved Z reference and one is the lossless RC. Mapping is private; distribute this Phase-1 surface without production composites or ladder reels.",
    }, indent=2), encoding="utf-8")
    (PRIVATE / "T3_REFERENCE_COMPARISON_MAPPING_PRIVATE.json").write_text(json.dumps({
        "status": "PRIVATE_ANSWER_KEY",
        "mapping": mapping,
        "reference_allowlist": REFERENCE_ALLOWLIST,
        "rc_hashes": rc_before,
    }, indent=2), encoding="utf-8")

    rc_after = {name: sha256(RC_ROOT / name) for name in RC_FILES}
    if rc_before != rc_after:
        raise RuntimeError("Reference assembly modified a production RC.")
    print(json.dumps({"status": "PASS_REFERENCE_CARVEOUT", "pairs": safe_rows}, indent=2))


if __name__ == "__main__":
    main()
