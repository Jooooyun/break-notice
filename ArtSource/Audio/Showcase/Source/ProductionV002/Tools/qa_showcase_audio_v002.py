from __future__ import annotations

import hashlib
import json
import math
import wave
from collections import Counter, defaultdict
from pathlib import Path

import numpy as np


SR = 48_000
BITS = 24
EXPECTED_EXPORTS = 196
EXPECTED_CUES = 75
EXPECTED_FAMILY_COUNTS = {
    "AMB": 7,
    "PLY": 38,
    "BND": 21,
    "CHR": 20,
    "ALY": 15,
    "STR": 15,
    "TAG": 8,
    "RUN": 35,
    "DRW": 10,
    "FIN": 13,
    "RST": 2,
    "EXT": 5,
    "UI": 7,
}


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def read_wav24(path: Path) -> tuple[np.ndarray, dict]:
    with wave.open(str(path), "rb") as wav:
        channels = wav.getnchannels()
        rate = wav.getframerate()
        width = wav.getsampwidth()
        frames = wav.getnframes()
        compression = wav.getcomptype()
        raw = wav.readframes(frames)
    if width != 3:
        raise AssertionError(f"not 24-bit PCM: {path}")
    packed = np.frombuffer(raw, dtype=np.uint8).reshape(-1, 3)
    values_i = (
        packed[:, 0].astype(np.int32)
        | (packed[:, 1].astype(np.int32) << 8)
        | (packed[:, 2].astype(np.int32) << 16)
    )
    values_i = np.where(values_i & 0x800000, values_i - 0x1000000, values_i)
    values = values_i.astype(np.float64) / 8_388_608.0
    if channels > 1:
        values = values.reshape(-1, channels)
    return values, {
        "channels": channels,
        "sample_rate": rate,
        "bit_depth": width * 8,
        "frames": frames,
        "duration": frames / rate,
        "compression": compression,
        "max_integer": int(np.max(np.abs(values_i))) if len(values_i) else 0,
    }


def mono(samples: np.ndarray) -> np.ndarray:
    return samples if samples.ndim == 1 else np.mean(samples, axis=1)


def max_aligned_correlation(a: np.ndarray, b: np.ndarray) -> float:
    a = mono(a)
    b = mono(b)
    length = min(len(a), len(b), 24_000)
    if length < 1_000:
        return 0.0
    a = a[:length] - np.mean(a[:length])
    b = b[:length] - np.mean(b[:length])
    denominator = math.sqrt(float(np.dot(a, a)) * float(np.dot(b, b)))
    return abs(float(np.dot(a, b)) / denominator) if denominator > 1e-12 else 0.0


def loop_seam_metrics(samples: np.ndarray) -> dict:
    x = mono(samples)
    differences = np.abs(np.diff(x))
    ordinary = float(np.percentile(differences, 99.0)) if len(differences) else 0.0
    jump = abs(float(x[0] - x[-1]))
    edge = min(len(x) // 8, int(0.010 * SR))
    if edge > 8:
        start_rms = float(np.sqrt(np.mean(x[:edge] ** 2)))
        end_rms = float(np.sqrt(np.mean(x[-edge:] ** 2)))
        rms_delta_db = 20.0 * math.log10(max(start_rms, 1e-12) / max(end_rms, 1e-12))
    else:
        rms_delta_db = 0.0
    return {
        "jump": jump,
        "jump_dbfs": 20.0 * math.log10(max(jump, 1e-12)),
        "ordinary_p99_step": ordinary,
        "jump_ratio": jump / max(ordinary, 1e-12),
        "edge_rms_delta_db": rms_delta_db,
    }


def main() -> None:
    script_path = Path(__file__).resolve()
    showcase_root = script_path.parents[3]
    preview_root = showcase_root / "Preview" / "ProductionV002"
    manifest_path = preview_root / "SHOWCASE_AUDIO_EXPORT_MANIFEST_v002.json"
    audition_path = preview_root / "SHOWCASE_AUDIO_AUDITION_INDEX_v002.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    audition = json.loads(audition_path.read_text(encoding="utf-8"))

    entries = manifest["entries"]
    assert manifest["cue_count"] == EXPECTED_CUES
    assert manifest["required_cue_count"] == 67
    assert manifest["conditional_cue_count"] == 8
    assert manifest["export_count"] == EXPECTED_EXPORTS
    assert len(entries) == EXPECTED_EXPORTS
    assert len({entry["path"] for entry in entries}) == EXPECTED_EXPORTS
    assert len({entry["filename"] for entry in entries}) == EXPECTED_EXPORTS
    assert len({entry["cue_id"] for entry in entries}) == EXPECTED_CUES
    assert Counter(entry["family"] for entry in entries) == Counter(EXPECTED_FAMILY_COUNTS)

    hashes = {}
    decoded_by_cue = defaultdict(list)
    loop_results = []
    max_dc = 0.0
    max_peak = 0.0
    min_rms_db = 0.0
    total_export_bytes = 0
    for entry in entries:
        path = showcase_root / entry["path"]
        assert path.is_file(), path
        samples, info = read_wav24(path)
        assert info["sample_rate"] == SR
        assert info["bit_depth"] == BITS
        assert info["compression"] == "NONE"
        assert info["frames"] > 0
        assert info["duration"] > 0.035
        assert info["channels"] == (1 if entry["channel_intent"] == "mono" else 2)
        assert info["max_integer"] < 8_388_607
        peak = float(np.max(np.abs(samples)))
        rms = float(np.sqrt(np.mean(samples * samples)))
        rms_db = 20.0 * math.log10(max(rms, 1e-12))
        dc = float(np.max(np.abs(np.mean(samples, axis=0))))
        assert peak > 1e-4, f"silent/corrupt: {path}"
        assert rms_db > -62.0, f"near-silent: {path} {rms_db}"
        assert peak < 1.0, f"clipped: {path}"
        assert dc < 2.5e-4, f"excessive DC: {path} {dc}"
        actual_hash = sha256_file(path)
        assert actual_hash == entry["sha256"], path
        assert actual_hash not in hashes, f"byte-identical export: {path} == {hashes[actual_hash]}"
        hashes[actual_hash] = str(path)
        decoded_by_cue[entry["cue_id"]].append(samples)
        max_dc = max(max_dc, dc)
        max_peak = max(max_peak, peak)
        min_rms_db = min(min_rms_db, rms_db)
        total_export_bytes += path.stat().st_size
        if entry["loop"]:
            seam = loop_seam_metrics(samples)
            loop_results.append({"path": entry["path"], **seam})

    correlation_results = []
    for cue_id, variations in decoded_by_cue.items():
        maximum = 0.0
        pair = None
        for left in range(len(variations)):
            for right in range(left + 1, len(variations)):
                correlation = max_aligned_correlation(variations[left], variations[right])
                if correlation > maximum:
                    maximum = correlation
                    pair = [left + 1, right + 1]
        if pair is not None:
            assert maximum < 0.985, f"excessive aligned variation correlation: {cue_id} {maximum}"
            correlation_results.append({"cue_id": cue_id, "maximum_aligned_correlation": maximum, "pair": pair})

    worst_loop_jump_item = max(loop_results, key=lambda item: item["jump_ratio"], default={"jump_ratio": 0.0})
    worst_loop_level_item = max(loop_results, key=lambda item: abs(item["edge_rms_delta_db"]), default={"edge_rms_delta_db": 0.0})
    worst_loop_jump = worst_loop_jump_item["jump_ratio"]
    worst_loop_level = abs(worst_loop_level_item["edge_rms_delta_db"])
    assert worst_loop_jump < 3.0, f"loop seam jump ratio {worst_loop_jump}"
    assert worst_loop_level < 4.0, f"loop seam 10ms RMS delta {worst_loop_level} dB: {worst_loop_level_item}"

    assert len(audition) == 5
    reel_hashes = set()
    reel_results = []
    represented = []
    for reel in audition:
        path = showcase_root / reel["path"]
        samples, info = read_wav24(path)
        assert info["sample_rate"] == SR and info["bit_depth"] == BITS and info["channels"] == 2
        assert info["duration"] <= 90.0
        actual_hash = sha256_file(path)
        assert actual_hash == reel["sha256"]
        assert actual_hash not in reel_hashes
        reel_hashes.add(actual_hash)
        assert float(np.max(np.abs(samples))) < 1.0
        assert float(np.max(np.abs(samples))) > 1e-4
        assert len(reel["index"]) > 0
        represented.extend(row["cue_id"] for row in reel["index"])
        previous_end = 0.0
        for row in reel["index"]:
            assert 0.0 <= row["start"] < row["end"] <= info["duration"]
            assert row["start"] >= previous_end
            previous_end = row["end"]
        reel_results.append({
            "path": reel["path"],
            "duration": info["duration"],
            "bytes": path.stat().st_size,
            "sha256": actual_hash,
        })
    assert len(represented) == EXPECTED_EXPORTS
    assert len(set(represented)) == EXPECTED_CUES
    indexed_filenames = [row["filename"] for reel in audition for row in reel["index"]]
    assert len(indexed_filenames) == EXPECTED_EXPORTS
    assert len(set(indexed_filenames)) == EXPECTED_EXPORTS
    assert set(indexed_filenames) == {entry["filename"] for entry in entries}

    approved_reference_expected = {
        "AUDITION_01_FOOTSTEPS_WEIGHTED_v002_48k24.wav": "dd6fcaf66e9ef4f26c7744379600b0da71b96a210f54e28cbdf23e0271bfae94",
        "AUDITION_02_PUNCH_WEIGHTED_v002_48k24.wav": "853ce15e701370b4180d7ec852f2e5f1860703cdaba77b28b3659381911c8791",
        "AUDITION_BINDER_PHYSICAL_v003_48k24.wav": "21c53a4b6d6878e527f81d04da07ee9ce1a209bc8fd6eeed7381ecb845099dd9",
        "AUDITION_CHAIR_PHYSICAL_v003_48k24.wav": "c314a2fcb601ed9d814921f4d06b04518e0a46ac526b6e3f5d4ebd87df51f8e8",
    }
    reference_results = []
    for name, expected in approved_reference_expected.items():
        path = showcase_root / "Source" / "ApprovedReferences" / name
        actual = sha256_file(path)
        assert actual == expected
        reference_results.append({"path": path.relative_to(showcase_root).as_posix(), "bytes": path.stat().st_size, "sha256": actual})

    raw_paths = sorted((showcase_root / "Source" / "ProductionV002" / "RawCC0" / "LaSonotheque").glob("*.wav"))
    evidence_paths = sorted((showcase_root / "Licenses" / "ProductionV002" / "Evidence").glob("*.html"))
    assert len(raw_paths) == 26
    assert len(evidence_paths) == 28
    raw_results = [{"path": path.relative_to(showcase_root).as_posix(), "bytes": path.stat().st_size, "sha256": sha256_file(path)} for path in raw_paths]
    evidence_results = [{"path": path.relative_to(showcase_root).as_posix(), "bytes": path.stat().st_size, "sha256": sha256_file(path)} for path in evidence_paths]

    v001_paths = sorted((showcase_root / "Export").glob("*.wav"))
    assert len(v001_paths) == 23

    all_files = [path for path in showcase_root.rglob("*") if path.is_file() and "node_modules" not in path.parts]
    total_bytes = sum(path.stat().st_size for path in all_files)
    production_v002_files = [path for path in (showcase_root).rglob("*") if path.is_file() and "ProductionV002" in path.parts and "node_modules" not in path.parts]
    production_v002_bytes = sum(path.stat().st_size for path in production_v002_files)

    report = {
        "status": "PASS",
        "cue_count": EXPECTED_CUES,
        "required_cue_count": 67,
        "conditional_cue_count": 8,
        "export_count": EXPECTED_EXPORTS,
        "family_counts": EXPECTED_FAMILY_COUNTS,
        "decode_pass": EXPECTED_EXPORTS,
        "sample_rate_pass": EXPECTED_EXPORTS,
        "bit_depth_pass": EXPECTED_EXPORTS,
        "clipped_file_count": 0,
        "full_scale_file_count": 0,
        "silent_or_corrupt_count": 0,
        "duplicate_export_hash_count": 0,
        "duplicate_destination_count": 0,
        "loop_count": len(loop_results),
        "loop_seam_pass": len(loop_results),
        "worst_loop_jump_ratio": max((item["jump_ratio"] for item in loop_results), default=0.0),
        "worst_loop_edge_rms_delta_db": max((abs(item["edge_rms_delta_db"]) for item in loop_results), default=0.0),
        "max_dc": max_dc,
        "max_peak": max_peak,
        "minimum_rms_dbfs": min_rms_db,
        "worst_variation_aligned_correlation": max((item["maximum_aligned_correlation"] for item in correlation_results), default=0.0),
        "export_bytes": total_export_bytes,
        "raw_source_count": len(raw_results),
        "license_evidence_count": len(evidence_results),
        "approved_reference_count": len(reference_results),
        "reel_count": len(reel_results),
        "reels": reel_results,
        "approved_references": reference_results,
        "raw_sources": raw_results,
        "license_evidence": evidence_results,
        "loop_metrics": loop_results,
        "variation_correlation": correlation_results,
        "v001_export_count_preserved": len(v001_paths),
        "production_v002_file_count": len(production_v002_files),
        "production_v002_bytes": production_v002_bytes,
        "showcase_total_bytes": total_bytes,
        "runtime_integration": "NOT_PERFORMED",
    }
    report_path = preview_root / "SHOWCASE_AUDIO_TECHNICAL_QA_v002.json"
    report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")
    print(json.dumps({key: report[key] for key in (
        "status", "cue_count", "required_cue_count", "conditional_cue_count", "export_count",
        "family_counts", "loop_count", "worst_loop_jump_ratio", "worst_loop_edge_rms_delta_db",
        "max_dc", "worst_variation_aligned_correlation", "export_bytes", "raw_source_count",
        "license_evidence_count", "production_v002_bytes", "showcase_total_bytes",
    )}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
