#!/usr/bin/env python3
"""Copy generated v004 review assets into the isolated Unity review folder.

Existing Unity .meta files are never deleted or replaced.
"""

from __future__ import annotations

import json
import shutil
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
REPOSITORY = ROOT.parents[3]
RENDERS = ROOT / "Renders"
UNITY = REPOSITORY / "Game" / "Assets" / "_Project" / "Audio" / "Review" / "Showcase" / "Rebuild_v004"


def copy_tree_files(source: Path, destination: Path, pattern: str) -> list[str]:
    copied: list[str] = []
    for path in sorted(source.rglob(pattern)):
        relative = path.relative_to(source)
        target = destination / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, target)
        copied.append(str(target.relative_to(UNITY)).replace("\\", "/"))
    return copied


def main() -> None:
    manifest_path = RENDERS / "Evidence" / "CUE_MANIFEST_REVIEW.json"
    if not manifest_path.is_file():
        raise FileNotFoundError("Build the production ladder before syncing Unity.")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    if len(manifest.get("cues", [])) != 11:
        raise RuntimeError("Cue manifest is not the authorized 11-cue ladder.")

    UNITY.mkdir(parents=True, exist_ok=True)
    copied: list[str] = []
    copied += copy_tree_files(RENDERS / "Stems", UNITY / "Stems", "*.wav")
    copied += copy_tree_files(RENDERS / "Composites", UNITY / "Composites", "*.wav")
    copied += copy_tree_files(RENDERS / "Derived", UNITY / "Derived", "*.wav")
    copied += copy_tree_files(RENDERS / "ReferenceOnly", UNITY / "ReferenceOnly", "*.wav")
    shutil.copy2(manifest_path, UNITY / "REVIEW_MANIFEST.json")
    copied.append("REVIEW_MANIFEST.json")

    expected_audio = 55 + 11 + 4 + 2
    actual_audio = len([path for path in UNITY.rglob("*.wav")])
    if actual_audio != expected_audio:
        raise RuntimeError(f"Unity review audio count {actual_audio} != {expected_audio}; stale files require bounded manual review.")
    print(json.dumps({
        "status": "PASS_UNITY_SYNC",
        "unity_folder": str(UNITY),
        "audio_files": actual_audio,
        "copied_files": len(copied),
    }, indent=2))


if __name__ == "__main__":
    main()

