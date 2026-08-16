#!/usr/bin/env python3
"""Mux the common 30 fps review frames with the three blind audition WAVs."""

from __future__ import annotations

import argparse
import hashlib
import json
import subprocess
from pathlib import Path


FRAME_COUNT = 105
FRAME_RATE = 30
BLIND_LABELS = ("X", "Y", "Z")


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--frames", type=Path, required=True)
    parser.add_argument("--ffmpeg", type=Path, required=True)
    args = parser.parse_args()

    frames = args.frames.resolve()
    ffmpeg = args.ffmpeg.resolve()
    missing = [
        frames / f"frame_{index:04d}.png"
        for index in range(FRAME_COUNT)
        if not (frames / f"frame_{index:04d}.png").is_file()
    ]
    if missing:
        raise FileNotFoundError(f"Missing common review frame: {missing[0]}")
    if not ffmpeg.is_file():
        raise FileNotFoundError(f"ffmpeg not found: {ffmpeg}")

    root = Path(__file__).resolve().parents[1]
    blind_dir = root / "Renders" / "Blind"
    video_dir = blind_dir / "Videos"
    evidence_dir = root / "Renders" / "Evidence"
    video_dir.mkdir(parents=True, exist_ok=True)

    records: list[dict[str, object]] = []
    for label in BLIND_LABELS:
        audio = blind_dir / f"BLIND_{label}_AUDITION_2VAR_REVIEW.wav"
        output = video_dir / f"BLIND_{label}_AUDITION_2VAR_REVIEW.mp4"
        if not audio.is_file():
            raise FileNotFoundError(f"Blind audition WAV not found: {audio}")

        command = [
            str(ffmpeg),
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-framerate",
            str(FRAME_RATE),
            "-i",
            str(frames / "frame_%04d.png"),
            "-i",
            str(audio),
            "-map",
            "0:v:0",
            "-map",
            "1:a:0",
            "-c:v",
            "libx264",
            "-preset",
            "medium",
            "-crf",
            "18",
            "-pix_fmt",
            "yuv420p",
            "-r",
            str(FRAME_RATE),
            "-frames:v",
            str(FRAME_COUNT),
            "-c:a",
            "aac",
            "-b:a",
            "256k",
            "-ar",
            "48000",
            "-ac",
            "2",
            "-metadata",
            "creation_time=1970-01-01T00:00:00Z",
            "-movflags",
            "+faststart",
            str(output),
        ]
        subprocess.run(command, check=True)
        records.append(
            {
                "blind_label": label,
                "common_video_frames": FRAME_COUNT,
                "frame_rate": FRAME_RATE,
                "duration_seconds": FRAME_COUNT / FRAME_RATE,
                "audio_sha256": sha256(audio),
                "video_path": output.relative_to(root).as_posix(),
                "video_bytes": output.stat().st_size,
                "video_sha256": sha256(output),
            }
        )

    manifest = {
        "status": "REVIEW_ONLY_NOT_FINAL",
        "visual_condition": "Identical common Chair-to-Runner motion and camera for X/Y/Z",
        "records": records,
    }
    manifest_path = evidence_dir / "VIDEO_QA_REVIEW.json"
    manifest_path.write_text(
        json.dumps(manifest, indent=2, ensure_ascii=False) + "\n",
        encoding="utf-8",
    )
    print(manifest_path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
