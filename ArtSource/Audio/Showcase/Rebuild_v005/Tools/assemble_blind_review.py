#!/usr/bin/env python3
"""Assemble isolated T3 v005 blind-review surfaces and phased ZIPs.

This is the only v005 tool permitted to read the retained approved-Y sonic
reference.  The production builder remains isolated.  The common MP4 is read
for its video stream only; its old audio stream is never mapped.

Automated measurements and package checks do not grant a perceptual PASS.
"""

from __future__ import annotations

import hashlib
import json
import math
import os
import random
import shutil
import subprocess
import tempfile
import zipfile
from pathlib import Path

import imageio_ffmpeg
import numpy as np
import pyloudnorm as pyln
import soundfile as sf
from scipy import signal


SAMPLE_RATE = 48_000
CLIP_FRAMES = 64_800
GAP_FRAMES = 31_200
PAIR_FRAMES = 160_800
PAIR_TARGET_LUFS = -20.0
LUFS_TOLERANCE = 0.1

ROOT = Path(__file__).resolve().parents[1]
SHOWCASE = ROOT.parent
REFERENCE_ROOT = SHOWCASE / "Rebuild_v003" / "Renders" / "Candidates" / "B"
COMMON_VIDEO = (
    SHOWCASE
    / "Rebuild_v004"
    / "Renders"
    / "ReferenceOnly"
    / "Videos"
    / "T3_Y_PAIR_COMMON_VIDEO_REVIEW.mp4"
)
COMMON_VIDEO_EXPECTED_SHA256 = "e98f75ba9f23aaefea6b925eb5cf7655b4d37310c3f56104576d5f927a229175"
REFERENCE_ALLOWLIST = {
    "B_ArcadeEdge_v01_REVIEW.wav": "9dd84fad2aeade3dbfc5a0dbb0e49a505da554abcea6185803070e5e14d3708f",
    "B_ArcadeEdge_v02_REVIEW.wav": "30335cd8498b10cf3c527965a20a3a3ea0ef6bf3fd71c49e848d8e93cb6292bb",
}
RAW_CANDIDATE_FILES = {
    "v01": "T3_CHAIR_RUNNER_v005_v01_RC_REVIEW.wav",
    "v02": "T3_CHAIR_RUNNER_v005_v02_RC_REVIEW.wav",
}

COMPOSITES = ROOT / "Renders" / "Composites"
STEMS = ROOT / "Renders" / "Stems"
REVIEW = ROOT / "Renders" / "Review"
PAIR_BLIND = REVIEW / "PairBlind"
VARIATION_BLIND = REVIEW / "VariationBlind"
COMMON_PICTURE = REVIEW / "CommonPicture"
PACKAGE_TEXT = REVIEW / "PackageText"
PRIVATE = ROOT / "Renders" / "Private"
EVIDENCE = ROOT / "Renders" / "Evidence"
PACKAGE = ROOT / "Renders" / "Package"

PAIR_ZIP = "BREAK_NOTICE_T3_V005_PAIR_BLIND_PHASE1.zip"
VARIATION_ZIP = "BREAK_NOTICE_T3_V005_VARIATION_BLIND_PHASE2.zip"
DETAIL_ZIP = "BREAK_NOTICE_T3_V005_DEBLIND_DETAIL_PHASE3.zip"
PACKAGE_MANIFEST = PACKAGE / "PHASE_PACKAGE_MANIFEST_REVIEW.json"

PHASE3_REQUIRED_DOCS = (
    "REBUILD_DIRECTION_REVIEW.md",
    "SOURCE_PROVENANCE.md",
    "REVIEW_CHECKLIST.md",
    "LISTENING_RESPONSE_TEMPLATE.md",
    "REHEARSAL_LOG.md",
)
PHASE3_REQUIRED_EVIDENCE = (
    "CUE_MANIFEST_REVIEW.json",
    "SOURCE_LEDGER_REVIEW.json",
    "TECHNICAL_QA_REVIEW.json",
    "T3_V005_WAVEFORM_SPECTROGRAM_REVIEW.png",
    "T3_V005_REVIEW_ASSEMBLY_MANIFEST.json",
    "T3_V005_VIDEO_QA_REVIEW.json",
    "ISOLATION_AUDIT_REVIEW.json",
)


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def canonical_json_bytes(value: object) -> bytes:
    return (json.dumps(value, indent=2, sort_keys=True) + "\n").encode("utf-8")


def write_json(path: Path, value: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(canonical_json_bytes(value))


def ffmpeg(*arguments: str) -> None:
    executable = imageio_ffmpeg.get_ffmpeg_exe()
    completed = subprocess.run(
        [executable, "-y", "-v", "error", *arguments],
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    if completed.returncode != 0:
        raise RuntimeError("ffmpeg failed: " + " ".join(arguments) + "\n" + completed.stderr)


def ffmpeg_probe_text(path: Path) -> str:
    executable = imageio_ffmpeg.get_ffmpeg_exe()
    completed = subprocess.run(
        [executable, "-hide_banner", "-i", str(path)],
        capture_output=True,
        text=True,
        encoding="utf-8",
    )
    return completed.stderr


def video_stream_hash(path: Path) -> str:
    with tempfile.TemporaryDirectory(prefix="break_notice_v005_video_hash_") as temporary:
        stream = Path(temporary) / "stream.h264"
        ffmpeg(
            "-i",
            str(path),
            "-map",
            "0:v:0",
            "-an",
            "-c:v",
            "copy",
            "-bsf:v",
            "h264_mp4toannexb",
            "-f",
            "h264",
            str(stream),
        )
        return sha256(stream)


def assert_video_only(path: Path) -> None:
    probe = ffmpeg_probe_text(path)
    if "Video:" not in probe or "Audio:" in probe:
        raise RuntimeError(f"Common-picture master is not video-only: {path}")


def read_review_wav(path: Path) -> np.ndarray:
    if path.is_symlink():
        raise RuntimeError(f"Symlink input rejected: {path}")
    data, rate = sf.read(path, always_2d=True, dtype="float64")
    info = sf.info(path)
    if rate != SAMPLE_RATE or data.shape != (CLIP_FRAMES, 2):
        raise RuntimeError(f"Unexpected T3 review input shape: {path} rate={rate} shape={data.shape}")
    if info.format not in {"WAV", "WAVEX"} or info.subtype not in {"PCM_16", "PCM_24", "PCM_32"}:
        raise RuntimeError(f"Non-PCM review input rejected: {path} {info.format}/{info.subtype}")
    if not np.all(np.isfinite(data)):
        raise RuntimeError(f"Non-finite input: {path}")
    return data


def dbfs(value: float) -> float:
    return 20.0 * math.log10(max(value, 1e-12))


def audio_metrics(data: np.ndarray) -> dict[str, float]:
    meter = pyln.Meter(SAMPLE_RATE)
    oversampled = signal.resample_poly(data, 4, 1, axis=0)
    return {
        "integrated_lufs": float(meter.integrated_loudness(data)),
        "sample_peak_dbfs": dbfs(float(np.max(np.abs(data)))),
        "estimated_true_peak_dbfs_4x": dbfs(float(np.max(np.abs(oversampled)))),
    }


def loudness_match(data: np.ndarray) -> np.ndarray:
    meter = pyln.Meter(SAMPLE_RATE)
    current = float(meter.integrated_loudness(data))
    matched = pyln.normalize.loudness(data, current, PAIR_TARGET_LUFS)
    if float(np.max(np.abs(matched))) >= 1.0:
        raise RuntimeError("Integrated-loudness scalar would clip; peak normalization is prohibited")
    return matched


def write_matched_wav(path: Path, data: np.ndarray) -> dict[str, float | str]:
    path.parent.mkdir(parents=True, exist_ok=True)
    matched = loudness_match(data)
    sf.write(path, matched, SAMPLE_RATE, subtype="PCM_24", format="WAV")
    decoded, rate = sf.read(path, always_2d=True, dtype="float64")
    if rate != SAMPLE_RATE or decoded.shape != data.shape:
        raise RuntimeError(f"Review WAV decode contract failed: {path}")
    row = audio_metrics(decoded)
    if abs(float(row["integrated_lufs"]) - PAIR_TARGET_LUFS) > LUFS_TOLERANCE:
        raise RuntimeError(f"Review WAV loudness outside tolerance: {path} {row['integrated_lufs']}")
    row.update(
        {
            "sha256": sha256(path),
            "frames": int(len(decoded)),
            "gain_method": "single integrated-loudness scalar; no peak normalization, limiter, or clipper",
        }
    )
    return row


def make_pair(first: np.ndarray, second: np.ndarray) -> np.ndarray:
    silence = np.zeros((GAP_FRAMES, 2), dtype=np.float64)
    result = np.concatenate((first, silence, second), axis=0)
    if result.shape != (PAIR_FRAMES, 2):
        raise RuntimeError(f"Pair shape contract failed: {result.shape}")
    return result


def rebuild_common_picture() -> dict[str, object]:
    if not COMMON_VIDEO.is_file() or sha256(COMMON_VIDEO) != COMMON_VIDEO_EXPECTED_SHA256:
        raise RuntimeError(f"Common-video allowlist mismatch: {COMMON_VIDEO}")
    if "Video:" not in ffmpeg_probe_text(COMMON_VIDEO):
        raise RuntimeError("Allowed common MP4 has no video stream")

    if COMMON_PICTURE.exists():
        shutil.rmtree(COMMON_PICTURE)
    COMMON_PICTURE.mkdir(parents=True, exist_ok=True)
    pair_master = COMMON_PICTURE / "T3_PAIR_COMMON_VIDEO_ONLY.mp4"
    v01_master = COMMON_PICTURE / "T3_V01_COMMON_VIDEO_ONLY.mp4"
    v02_master = COMMON_PICTURE / "T3_V02_COMMON_VIDEO_ONLY.mp4"

    # The old AAC track is intentionally not mapped.  -an is an additional guard.
    ffmpeg(
        "-i",
        str(COMMON_VIDEO),
        "-map",
        "0:v:0",
        "-an",
        "-c:v",
        "copy",
        str(pair_master),
    )
    for start, output in (("0.000", v01_master), ("2.000", v02_master)):
        ffmpeg(
            "-ss",
            start,
            "-i",
            str(pair_master),
            "-t",
            "1.350",
            "-map",
            "0:v:0",
            "-an",
            "-c:v",
            "libx264",
            "-preset",
            "medium",
            "-crf",
            "18",
            "-pix_fmt",
            "yuv420p",
            "-r",
            "30",
            str(output),
        )
    for path in (pair_master, v01_master, v02_master):
        assert_video_only(path)
    return {
        "allowed_source": {
            "path": str(COMMON_VIDEO.relative_to(SHOWCASE)).replace("\\", "/"),
            "sha256": sha256(COMMON_VIDEO),
            "video_stream_sha256": video_stream_hash(COMMON_VIDEO),
            "audio_mapping": "PROHIBITED / NOT MAPPED",
        },
        "masters": {
            path.name: {"sha256": sha256(path), "video_stream_sha256": video_stream_hash(path)}
            for path in (pair_master, v01_master, v02_master)
        },
    }


def mux(common_picture: Path, wav: Path, output: Path) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    ffmpeg(
        "-i",
        str(common_picture),
        "-i",
        str(wav),
        "-map",
        "0:v:0",
        "-map",
        "1:a:0",
        "-c:v",
        "copy",
        "-c:a",
        "aac",
        "-b:a",
        "192k",
        "-ar",
        str(SAMPLE_RATE),
        "-ac",
        "2",
        "-movflags",
        "+faststart",
        str(output),
    )


def deterministic_mapping(candidate_hashes: dict[str, str]) -> dict[str, dict[str, str]]:
    material = "|".join(candidate_hashes[key] for key in sorted(candidate_hashes))
    seed = int.from_bytes(hashlib.sha256(("BREAK_NOTICE_T3_V005_BLIND|" + material).encode()).digest()[:8], "big")
    generator = random.Random(seed)

    def shuffled(labels: tuple[str, str]) -> dict[str, str]:
        identities = ["NEW_V005_RC", "APPROVED_Y_REFERENCE"]
        generator.shuffle(identities)
        return dict(zip(labels, identities))

    return {
        "pair": shuffled(("M", "N")),
        "v01": shuffled(("A", "B")),
        "v02": shuffled(("P", "Q")),
    }


def clean_review_outputs() -> None:
    for path in (PAIR_BLIND, VARIATION_BLIND, PRIVATE):
        if path.exists():
            shutil.rmtree(path)
        path.mkdir(parents=True, exist_ok=True)
    PACKAGE.mkdir(parents=True, exist_ok=True)
    PACKAGE_TEXT.mkdir(parents=True, exist_ok=True)


def build_review_surfaces(
    candidates: dict[str, np.ndarray],
    references: dict[str, np.ndarray],
    mapping: dict[str, dict[str, str]],
) -> tuple[dict[str, object], dict[str, object]]:
    identities = {
        "NEW_V005_RC": candidates,
        "APPROVED_Y_REFERENCE": references,
    }
    pair_audio = {identity: make_pair(rows["v01"], rows["v02"]) for identity, rows in identities.items()}
    audio_rows: dict[str, object] = {"pair": {}, "v01": {}, "v02": {}}
    output_rows: dict[str, object] = {"pair": {}, "v01": {}, "v02": {}}

    pair_master = COMMON_PICTURE / "T3_PAIR_COMMON_VIDEO_ONLY.mp4"
    for label, identity in mapping["pair"].items():
        wav = PAIR_BLIND / f"T3_PAIR_{label}_BLIND_REVIEW.wav"
        mp4 = PAIR_BLIND / f"T3_PAIR_{label}_BLIND_REVIEW.mp4"
        audio_rows["pair"][identity] = write_matched_wav(wav, pair_audio[identity])
        mux(pair_master, wav, mp4)
        output_rows["pair"][label] = {
            "wav": {"path": str(wav.relative_to(ROOT)).replace("\\", "/"), "sha256": sha256(wav)},
            "mp4": {"path": str(mp4.relative_to(ROOT)).replace("\\", "/"), "sha256": sha256(mp4)},
        }

    for variation, labels in (("v01", mapping["v01"]), ("v02", mapping["v02"])):
        directory = VARIATION_BLIND / variation
        master = COMMON_PICTURE / f"T3_{variation.upper()}_COMMON_VIDEO_ONLY.mp4"
        for label, identity in labels.items():
            wav = directory / f"T3_{variation.upper()}_{label}_BLIND_REVIEW.wav"
            mp4 = directory / f"T3_{variation.upper()}_{label}_BLIND_REVIEW.mp4"
            audio_rows[variation][identity] = write_matched_wav(wav, identities[identity][variation])
            mux(master, wav, mp4)
            output_rows[variation][label] = {
                "wav": {"path": str(wav.relative_to(ROOT)).replace("\\", "/"), "sha256": sha256(wav)},
                "mp4": {"path": str(mp4.relative_to(ROOT)).replace("\\", "/"), "sha256": sha256(mp4)},
            }
    return audio_rows, output_rows


def video_qa(output_rows: dict[str, object], common: dict[str, object]) -> dict[str, object]:
    failures: list[str] = []
    comparison_master_names = {
        "pair": "T3_PAIR_COMMON_VIDEO_ONLY.mp4",
        "v01": "T3_V01_COMMON_VIDEO_ONLY.mp4",
        "v02": "T3_V02_COMMON_VIDEO_ONLY.mp4",
    }
    comparisons: dict[str, object] = {}
    for comparison, master_name in comparison_master_names.items():
        master_hash = str(common["masters"][master_name]["video_stream_sha256"])
        labels: dict[str, object] = {}
        for label, files in output_rows[comparison].items():
            mp4 = ROOT / files["mp4"]["path"]
            stream_hash = video_stream_hash(mp4)
            labels[label] = {"video_stream_sha256": stream_hash, "matches_common_picture": stream_hash == master_hash}
            if stream_hash != master_hash:
                failures.append(f"{comparison}/{label} video stream differs from common picture")
        comparisons[comparison] = {
            "common_video_stream_sha256": master_hash,
            "labels": labels,
        }
    return {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "old_common_mp4_audio_mapping": "PROHIBITED / NOT MAPPED",
        "comparisons": comparisons,
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }


def write_phase_readmes() -> None:
    PACKAGE_TEXT.mkdir(parents=True, exist_ok=True)
    (PACKAGE_TEXT / "PHASE1_README.md").write_text(
        "# T3 Phase 1 — pair blind\n\n"
        "이 ZIP만 새 폴더에 압축 해제하고, Phase 2·3 ZIP은 아직 열지 마십시오.\n\n"
        "1. `T3_PAIR_M_BLIND_REVIEW.mp4`와 `T3_PAIR_N_BLIND_REVIEW.mp4`를 먼저 비교합니다.\n"
        "2. WAV는 각 MP4와 같은 오디오의 무손실 대체본입니다. MP4 재생이 불안정하거나 세부를 다시 확인할 때만 사용하며 별도 후보로 세지 않습니다.\n"
        "3. 각 파일은 v01, 650ms 무음, v02 순서입니다. 시스템 음량을 바꾸지 말고 헤드폰에서 각 6회, 노트북 스피커에서 각 6회 반복합니다.\n"
        "4. 더 무거운 쪽, Chair→Runner→Floor 인과가 읽히는 쪽, 반복 피로가 적은 쪽을 판단합니다.\n"
        "5. `PHASE1_RESPONSE.md`의 모든 항목을 작성하고 저장한 뒤 그 전문을 현재 Codex 작업에 전송합니다. 전송된 답이 Phase 1 잠금본이며 이후 수정하지 않습니다.\n"
        "6. Phase 1 답을 전송하기 전에는 Phase 2를 열지 않습니다.\n\n"
        "이 단계에서는 정답표, 측정 자료, source·stem 정보를 열지 않습니다.\n",
        encoding="utf-8",
    )
    (PACKAGE_TEXT / "PHASE1_RESPONSE.md").write_text(
        "# T3 Phase 1 response\n\n"
        "- 헤드폰 각 6회: DONE / NOT DONE\n"
        "- 노트북 스피커 각 6회: DONE / NOT DONE\n"
        "- 판정: M / N / 둘 다 FAIL\n"
        "- 더 무겁게 들린 이유:\n"
        "- Chair→Runner→Floor 인과가 더 잘 읽힌 이유:\n"
        "- 가볍고 경쾌하거나 애니틱하게 들린 label: M / N / 둘 다 / 없음\n"
        "- 뭉개짐·장난감성·거친 반복 피로 등 문제:\n"
        "- 잠금 시각(KST): YYYY-MM-DD HH:MM\n",
        encoding="utf-8",
    )
    (PACKAGE_TEXT / "PHASE2_README.md").write_text(
        "# T3 Phase 2 — variation blind\n\n"
        "Phase 1 잠금본을 현재 Codex 작업에 전송한 뒤에만 이 ZIP을 새 폴더에 압축 해제하십시오. Phase 3은 아직 열지 마십시오.\n\n"
        "1. v01의 A/B와 v02의 P/Q를 서로 독립적으로 판정합니다. MP4를 먼저 사용하고, WAV는 같은 오디오의 무손실 대체본으로만 사용합니다.\n"
        "2. 시스템 음량을 바꾸지 말고 각 비교를 헤드폰 6회, 노트북 스피커 6회 반복합니다.\n"
        "3. 무게, Chair 재질, Runner body, Floor settle, 반복 피로를 기준으로 각 variation을 따로 고릅니다.\n"
        "4. `PHASE2_RESPONSE.md`의 모든 항목을 작성하고 저장한 뒤 그 전문을 현재 Codex 작업에 전송합니다. 전송된 답이 Phase 2 잠금본이며 이후 수정하지 않습니다.\n"
        "5. 두 답을 전송하기 전에는 Phase 3을 열지 않습니다. 어느 한 variation이라도 실패하면 T3 전체는 PASS가 아닙니다.\n",
        encoding="utf-8",
    )
    (PACKAGE_TEXT / "PHASE2_RESPONSE.md").write_text(
        "# T3 Phase 2 response\n\n"
        "## v01\n\n"
        "- 헤드폰/노트북 각 6회: DONE / NOT DONE\n"
        "- 판정: A / B / 둘 다 FAIL\n"
        "- 이유:\n\n"
        "## v02\n\n"
        "- 헤드폰/노트북 각 6회: DONE / NOT DONE\n"
        "- 판정: P / Q / 둘 다 FAIL\n"
        "- 이유:\n\n"
        "- 두 variation의 무게·밝기·피로도가 같은 family로 느껴졌는가: YES / NO\n"
        "- 잠금 시각(KST): YYYY-MM-DD HH:MM\n",
        encoding="utf-8",
    )
    (PACKAGE_TEXT / "PHASE3_README.md").write_text(
        "# T3 Phase 3 — deblind and detail\n\n"
        "Phase 1과 Phase 2 잠금본을 현재 Codex 작업에 모두 전송한 뒤에만 이 ZIP을 여십시오.\n\n"
        "1. `Private/T3_V005_BLIND_MAPPING_PRIVATE.json`에서 label 정체를 공개합니다.\n"
        "2. 잠근 답과 정체를 대조합니다. 신규 v005가 v01 또는 v02 중 하나라도 지면 전체 v005는 FAIL입니다.\n"
        "3. FAIL variation이 있으면 `Composites/`의 해당 composite를 듣고, 이어서 같은 variation의 `Stems/` 다섯 개를 `pre → signature → emitter → runner → floor_tail` 순서로 확인합니다.\n"
        "4. `Docs/REVIEW_CHECKLIST.md`를 실행하고 `Docs/LISTENING_RESPONSE_TEMPLATE.md`에 최종 PASS/FAIL과 수정 요청을 기록합니다.\n"
        "5. 제작 범위와 source 한계는 `Docs/REBUILD_DIRECTION_REVIEW.md`, provenance는 `Docs/SOURCE_PROVENANCE.md`, 기술 사실은 `Evidence/`에서 확인합니다.\n\n"
        "자동 QA는 청취 PASS를 부여하지 않습니다. 실제 하중 상태 사무용 의자와 인체의 정확한 녹음이 없는 proxy gap도 숨기지 않습니다.\n",
        encoding="utf-8",
    )


def zip_routes(output: Path, routes: list[tuple[str, Path]]) -> dict[str, object]:
    seen: set[str] = set()
    output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(output, "w", compression=zipfile.ZIP_STORED) as archive:
        for entry, local in sorted(routes, key=lambda row: row[0].casefold()):
            normalized = entry.replace("\\", "/")
            if normalized.casefold() in seen:
                raise RuntimeError(f"Duplicate ZIP entry: {normalized}")
            seen.add(normalized.casefold())
            if not local.is_file():
                raise FileNotFoundError(local)
            info = zipfile.ZipInfo(normalized, date_time=(1980, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_STORED
            info.external_attr = 0o100644 << 16
            archive.writestr(info, local.read_bytes())
    return {
        "sha256": sha256(output),
        "entries": {
            entry.replace("\\", "/"): {
                "local_path": str(local.relative_to(ROOT)).replace("\\", "/"),
                "sha256": sha256(local),
            }
            for entry, local in sorted(routes, key=lambda row: row[0].casefold())
        },
    }


def phase3_routes(mapping_path: Path) -> tuple[list[tuple[str, Path]], list[str]]:
    missing: list[str] = []
    for name in PHASE3_REQUIRED_DOCS:
        if not (ROOT / name).is_file():
            missing.append(name)
    for name in PHASE3_REQUIRED_EVIDENCE:
        if not (EVIDENCE / name).is_file():
            missing.append(f"Renders/Evidence/{name}")
    if not mapping_path.is_file():
        missing.append(str(mapping_path.relative_to(ROOT)).replace("\\", "/"))
    if missing:
        return [], missing

    isolation = json.loads((EVIDENCE / "ISOLATION_AUDIT_REVIEW.json").read_text(encoding="utf-8"))
    if isolation.get("status") != "PASS":
        return [], ["Renders/Evidence/ISOLATION_AUDIT_REVIEW.json status is not PASS"]

    routes: list[tuple[str, Path]] = [("README.md", PACKAGE_TEXT / "PHASE3_README.md")]
    routes.extend((f"Docs/{name}", ROOT / name) for name in PHASE3_REQUIRED_DOCS)
    routes.append((f"Private/{mapping_path.name}", mapping_path))
    routes.extend((f"Composites/{path.name}", path) for path in sorted(COMPOSITES.glob("*.wav")))
    routes.extend(
        (f"Stems/{path.relative_to(STEMS).as_posix()}", path)
        for path in sorted(STEMS.rglob("*.wav"))
    )
    routes.extend((f"Evidence/{name}", EVIDENCE / name) for name in PHASE3_REQUIRED_EVIDENCE)
    source_evidence = ROOT / "Sources" / "Evidence"
    routes.extend(
        (f"Provenance/Evidence/{path.relative_to(source_evidence).as_posix()}", path)
        for path in sorted(source_evidence.rglob("*"))
        if path.is_file()
    )
    routes.extend(
        (f"Blind/Pair/{path.name}", path)
        for path in sorted(PAIR_BLIND.glob("*"))
        if path.is_file()
    )
    routes.extend(
        (f"Blind/Variation/{path.relative_to(VARIATION_BLIND).as_posix()}", path)
        for path in sorted(VARIATION_BLIND.rglob("*"))
        if path.is_file()
    )
    if len(list(COMPOSITES.glob("*.wav"))) != 2 or len(list(STEMS.rglob("*.wav"))) != 10:
        return [], ["expected exactly 2 composites and 10 stems"]
    return routes, []


def assemble_packages(mapping_path: Path) -> dict[str, object]:
    write_phase_readmes()
    expected_names = {PAIR_ZIP, VARIATION_ZIP, DETAIL_ZIP}
    for old in PACKAGE.glob("*.zip"):
        if old.name in expected_names:
            old.unlink()

    phase1_routes: list[tuple[str, Path]] = [
        ("README.md", PACKAGE_TEXT / "PHASE1_README.md"),
        ("PHASE1_RESPONSE.md", PACKAGE_TEXT / "PHASE1_RESPONSE.md"),
    ]
    phase1_routes.extend(
        (path.name, path) for path in sorted(PAIR_BLIND.glob("*")) if path.suffix.lower() in {".wav", ".mp4"}
    )
    phase2_routes: list[tuple[str, Path]] = [
        ("README.md", PACKAGE_TEXT / "PHASE2_README.md"),
        ("PHASE2_RESPONSE.md", PACKAGE_TEXT / "PHASE2_RESPONSE.md"),
    ]
    phase2_routes.extend(
        (path.relative_to(VARIATION_BLIND).as_posix(), path)
        for path in sorted(VARIATION_BLIND.rglob("*"))
        if path.suffix.lower() in {".wav", ".mp4"}
    )

    rows: dict[str, object] = {
        PAIR_ZIP: zip_routes(PACKAGE / PAIR_ZIP, phase1_routes),
        VARIATION_ZIP: zip_routes(PACKAGE / VARIATION_ZIP, phase2_routes),
    }
    routes, missing = phase3_routes(mapping_path)
    if missing:
        rows[DETAIL_ZIP] = {"status": "PENDING", "missing": missing}
    else:
        rows[DETAIL_ZIP] = {"status": "READY", **zip_routes(PACKAGE / DETAIL_ZIP, routes)}
    manifest = {
        "status": "THREE_PHASE_READY" if not missing else "PHASE3_PENDING",
        "release_order": "Phase 1 verdict -> Phase 2 verdicts -> Phase 3 deblind/detail",
        "packages": rows,
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }
    write_json(PACKAGE_MANIFEST, manifest)
    return manifest


def main() -> None:
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    if set(REFERENCE_ALLOWLIST) != {
        "B_ArcadeEdge_v01_REVIEW.wav",
        "B_ArcadeEdge_v02_REVIEW.wav",
    }:
        raise RuntimeError("Reference allowlist filename set changed")
    for name, expected in REFERENCE_ALLOWLIST.items():
        path = REFERENCE_ROOT / name
        if not path.is_file() or sha256(path) != expected:
            raise RuntimeError(f"Approved-Y reference allowlist mismatch: {path}")

    raw_paths = {variation: COMPOSITES / name for variation, name in RAW_CANDIDATE_FILES.items()}
    production_paths = sorted(COMPOSITES.glob("*.wav")) + sorted(STEMS.rglob("*.wav"))
    if len(production_paths) != 12:
        raise RuntimeError(f"Expected 2 composites and 10 stems before review assembly, got {len(production_paths)}")
    production_before = {
        str(path.relative_to(ROOT)).replace("\\", "/"): sha256(path) for path in production_paths
    }
    raw_before = {variation: sha256(path) for variation, path in raw_paths.items()}
    candidate_audio = {variation: read_review_wav(path) for variation, path in raw_paths.items()}
    reference_audio = {
        variation: read_review_wav(REFERENCE_ROOT / name)
        for variation, name in zip(("v01", "v02"), REFERENCE_ALLOWLIST)
    }

    clean_review_outputs()
    common = rebuild_common_picture()
    mapping = deterministic_mapping(raw_before)
    audio_rows, output_rows = build_review_surfaces(candidate_audio, reference_audio, mapping)
    qa = video_qa(output_rows, common)
    write_json(EVIDENCE / "T3_V005_VIDEO_QA_REVIEW.json", qa)
    if qa["failures"]:
        raise RuntimeError("Video QA failed: " + "; ".join(qa["failures"]))

    private_mapping = {
        "status": "PRIVATE_ANSWER_KEY / OPEN ONLY AFTER PHASE 1 AND 2 VERDICTS",
        "mapping": mapping,
        "approved_y_reference_allowlist": REFERENCE_ALLOWLIST,
        "raw_v005_candidate_hashes": raw_before,
        "blind_outputs": output_rows,
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }
    mapping_path = PRIVATE / "T3_V005_BLIND_MAPPING_PRIVATE.json"
    write_json(mapping_path, private_mapping)
    mapping_hash = sha256(mapping_path)

    raw_after = {variation: sha256(path) for variation, path in raw_paths.items()}
    production_after = {
        str(path.relative_to(ROOT)).replace("\\", "/"): sha256(path) for path in production_paths
    }
    if production_before != production_after:
        raise RuntimeError("Blind-review assembly modified a raw v005 production composite or stem")

    assembly = {
        "status": "PASS_TECHNICAL_ASSEMBLY / HUMAN VERDICT REQUIRED",
        "timing_contract": {
            "variation_seconds": 1.350,
            "pair_gap_seconds": 0.650,
            "pair_seconds": 3.350,
            "contact_seconds": [0.200, 2.200],
        },
        "loudness_contract": {
            "target_lufs": PAIR_TARGET_LUFS,
            "tolerance_lufs": LUFS_TOLERANCE,
            "peak_normalization": "PROHIBITED / NOT USED",
        },
        "approved_y_reference_allowlist": REFERENCE_ALLOWLIST,
        "common_picture": common,
        "raw_v005_hashes_before": raw_before,
        "raw_v005_hashes_after": raw_after,
        "production_composite_and_stem_hashes_before": production_before,
        "production_composite_and_stem_hashes_after": production_after,
        "raw_v005_unchanged": production_before == production_after,
        "review_audio_metrics_by_private_identity": audio_rows,
        "private_mapping_sha256_lock": mapping_hash,
        "video_qa_status": qa["status"],
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }
    write_json(EVIDENCE / "T3_V005_REVIEW_ASSEMBLY_MANIFEST.json", assembly)
    packages = assemble_packages(mapping_path)
    print(
        json.dumps(
            {
                "status": "PASS_TECHNICAL_ASSEMBLY",
                "private_mapping_sha256_lock": mapping_hash,
                "raw_v005_unchanged": True,
                "video_qa": qa["status"],
                "packages": packages["status"],
                "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
            },
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
