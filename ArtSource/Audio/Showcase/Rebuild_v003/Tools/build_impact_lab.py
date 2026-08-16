#!/usr/bin/env python3
"""Build the isolated BREAK NOTICE Chair -> Runner T3 REVIEW pack.

This is a new deterministic processing chain. It does not read any prior
BREAK NOTICE audio branch, export, sample, script, or review scene.
"""

from __future__ import annotations

import hashlib
import json
import math
import random
import shutil
from dataclasses import dataclass
from fractions import Fraction
from pathlib import Path

import numpy as np
import pyloudnorm as pyln
import soundfile as sf
from scipy import signal


SAMPLE_RATE = 48_000
CONTACT_SECONDS = 0.200
RENDER_SECONDS = 1.350
TARGET_LUFS = -18.0
MAX_SAMPLE_PEAK = 10 ** (-1.2 / 20.0)
BLIND_SEED = 0x42524B4E_56303033  # "BRKNV003"

ROOT = Path(__file__).resolve().parents[1]
SOURCES = ROOT / "Sources"
RENDERS = ROOT / "Renders"
UNITY_REVIEW = (
    ROOT.parents[3]
    / "Game"
    / "Assets"
    / "_Project"
    / "Audio"
    / "Review"
    / "Showcase"
    / "Rebuild_v003"
)


SOURCE_FILES = {
    "chair_roll": SOURCES / "freesound-317850-office-chair-roll-rattle-hq-preview.mp3",
    "steel_chair": SOURCES / "freesound-494620-steel-chair-hq-preview.mp3",
    "plastic_core": SOURCES / "freesound-104051-plastic-metal-concrete-impact-hq-preview.mp3",
    "plastic_scrape": SOURCES / "freesound-708509-plastic-chair-concrete-scrape-hq-preview.mp3",
    "body_coat": SOURCES / "freesound-235451-body-hit-coat-wall-hq-preview.mp3",
    "cloth_hit": SOURCES / "freesound-629976-clothed-body-hit-hq-preview.mp3",
    "fast_slide": SOURCES / "freesound-51132-chair-sliding-quickly-hq-preview.mp3",
    "metal_clink_1": SOURCES / "oga_clink1_0.wav",
    "metal_clink_2": SOURCES / "oga_clink2.wav",
    "metal_clink_3": SOURCES / "oga_clink3.wav",
    "metal_thud_2": SOURCES / "oga_thud2.wav",
    "metal_thud_3": SOURCES / "oga_thud3.wav",
    "kenney_metal_1": SOURCES / "KenneyImpactSounds" / "Audio" / "impactMetal_heavy_001.ogg",
    "kenney_metal_2": SOURCES / "KenneyImpactSounds" / "Audio" / "impactMetal_heavy_003.ogg",
    "kenney_punch_1": SOURCES / "KenneyImpactSounds" / "Audio" / "impactPunch_heavy_000.ogg",
    "kenney_punch_2": SOURCES / "KenneyImpactSounds" / "Audio" / "impactPunch_heavy_004.ogg",
    "kenney_soft_1": SOURCES / "KenneyImpactSounds" / "Audio" / "impactSoft_heavy_001.ogg",
    "kenney_soft_2": SOURCES / "KenneyImpactSounds" / "Audio" / "impactSoft_heavy_004.ogg",
    "kenney_floor_1": SOURCES / "KenneyImpactSounds" / "Audio" / "footstep_concrete_001.ogg",
    "kenney_floor_2": SOURCES / "KenneyImpactSounds" / "Audio" / "footstep_concrete_004.ogg",
}


@dataclass(frozen=True)
class Direction:
    code: str
    label: str
    description: str


DIRECTIONS = {
    "A": Direction("A", "Grounded", "Real chair, body, cloth, and floor identity leads."),
    "B": Direction("B", "Arcade Edge", "Fast attack and signature clarity lead."),
    "C": Direction("C", "Target Hybrid", "Grounded material and arcade readability share authority."),
}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def ensure_stereo(data: np.ndarray) -> np.ndarray:
    if data.ndim == 1:
        data = data[:, None]
    if data.shape[1] == 1:
        data = np.repeat(data, 2, axis=1)
    elif data.shape[1] > 2:
        data = data[:, :2]
    return np.asarray(data, dtype=np.float64)


def resample(data: np.ndarray, source_rate: int, target_rate: int = SAMPLE_RATE) -> np.ndarray:
    if source_rate == target_rate:
        return data
    ratio = Fraction(target_rate, source_rate).limit_denominator(2_000)
    return signal.resample_poly(data, ratio.numerator, ratio.denominator, axis=0)


def load(path: Path) -> np.ndarray:
    data, source_rate = sf.read(path, always_2d=True, dtype="float64")
    data = ensure_stereo(data)
    data = resample(data, int(source_rate))
    data -= np.mean(data, axis=0, keepdims=True)
    return data


SOURCE_AUDIO: dict[str, np.ndarray] = {}


def source(name: str) -> np.ndarray:
    if name not in SOURCE_AUDIO:
        path = SOURCE_FILES[name]
        if not path.is_file():
            raise FileNotFoundError(f"Missing new source: {path}")
        SOURCE_AUDIO[name] = load(path)
    return SOURCE_AUDIO[name]


def cut(name: str, start: float, duration: float) -> np.ndarray:
    data = source(name)
    first = max(0, int(round(start * SAMPLE_RATE)))
    count = max(1, int(round(duration * SAMPLE_RATE)))
    result = np.zeros((count, 2), dtype=np.float64)
    available = data[first : first + count]
    result[: len(available)] = available
    return result


def fade(data: np.ndarray, fade_in: float = 0.005, fade_out: float = 0.020) -> np.ndarray:
    result = data.copy()
    count_in = min(len(result), int(round(fade_in * SAMPLE_RATE)))
    count_out = min(len(result), int(round(fade_out * SAMPLE_RATE)))
    if count_in > 1:
        result[:count_in] *= np.sin(np.linspace(0, math.pi / 2, count_in))[:, None] ** 2
    if count_out > 1:
        result[-count_out:] *= np.cos(np.linspace(0, math.pi / 2, count_out))[:, None] ** 2
    return result


def filtered(
    data: np.ndarray,
    *,
    highpass: float | None = None,
    lowpass: float | None = None,
    order: int = 3,
) -> np.ndarray:
    if highpass is None and lowpass is None:
        return data.copy()
    nyquist = SAMPLE_RATE / 2
    if highpass is not None and lowpass is not None:
        frequencies = [highpass / nyquist, lowpass / nyquist]
        kind = "bandpass"
    elif highpass is not None:
        frequencies = highpass / nyquist
        kind = "highpass"
    else:
        frequencies = lowpass / nyquist
        kind = "lowpass"
    sos = signal.butter(order, frequencies, btype=kind, output="sos")
    try:
        return signal.sosfiltfilt(sos, data, axis=0)
    except ValueError:
        return signal.sosfilt(sos, data, axis=0)


def repitch(data: np.ndarray, ratio: float, output_seconds: float | None = None) -> np.ndarray:
    output_count = max(1, int(round(len(data) / ratio)))
    shifted = signal.resample(data, output_count, axis=0)
    if output_seconds is None:
        return shifted
    target_count = int(round(output_seconds * SAMPLE_RATE))
    result = np.zeros((target_count, 2), dtype=np.float64)
    result[: min(target_count, len(shifted))] = shifted[:target_count]
    return result


def pan(data: np.ndarray, value: float) -> np.ndarray:
    value = float(np.clip(value, -1.0, 1.0))
    left = math.cos((value + 1.0) * math.pi / 4)
    right = math.sin((value + 1.0) * math.pi / 4)
    mono = np.mean(data, axis=1)
    return np.column_stack((mono * left, mono * right)) * math.sqrt(2)


def envelope(data: np.ndarray, attack_gain: float, attack_seconds: float) -> np.ndarray:
    result = data.copy()
    attack_count = min(len(result), int(round(attack_seconds * SAMPLE_RATE)))
    if attack_count > 0:
        curve = np.linspace(attack_gain, 1.0, attack_count)
        result[:attack_count] *= curve[:, None]
    return result


def soft_saturate(data: np.ndarray, drive: float) -> np.ndarray:
    if drive <= 1.0:
        return data
    return np.tanh(data * drive) / math.tanh(drive)


def procedural_snap(variation: int, arcade: float) -> np.ndarray:
    rng = np.random.default_rng(BLIND_SEED + variation * 97)
    duration = 0.045
    count = int(round(duration * SAMPLE_RATE))
    noise = rng.standard_normal(count)
    noise = signal.sosfilt(
        signal.butter(3, [1_800 / (SAMPLE_RATE / 2), 12_000 / (SAMPLE_RATE / 2)], btype="bandpass", output="sos"),
        noise,
    )
    decay = np.exp(-np.linspace(0, 9.5 + 2.0 * arcade, count))
    click = np.sin(2 * math.pi * (4_100 + variation * 330) * np.arange(count) / SAMPLE_RATE)
    mono = (noise * 0.55 + click * 0.18) * decay
    mono /= max(1e-9, np.max(np.abs(mono)))
    return np.column_stack((mono, mono))


def blank() -> np.ndarray:
    return np.zeros((int(round(RENDER_SECONDS * SAMPLE_RATE)), 2), dtype=np.float64)


def add(canvas: np.ndarray, data: np.ndarray, start: float, gain: float = 1.0) -> None:
    first = int(round(start * SAMPLE_RATE))
    if first < 0:
        data = data[-first:]
        first = 0
    if first >= len(canvas) or not len(data):
        return
    count = min(len(data), len(canvas) - first)
    canvas[first : first + count] += data[:count] * gain


def grounded(variation: int) -> dict[str, np.ndarray]:
    stems = {name: blank() for name in ("pre", "signature", "chair", "runner", "floor_tail")}
    roll_times = (37.78, 32.78)
    body_times = (0.115, 2.335)
    scrape_times = (0.315, 2.265)
    slide_times = (5.18, 7.20)

    pre = filtered(cut("chair_roll", roll_times[variation], 0.270), highpass=130, lowpass=7_500)
    pre = fade(pre, 0.010, 0.060)
    add(stems["pre"], pan(pre, -0.10), CONTACT_SECONDS - 0.145, 0.80)

    clink = cut(f"metal_clink_{variation + 1}", 0.012, 0.145)
    clink = fade(filtered(clink, highpass=1_100, lowpass=12_000), 0.001, 0.035)
    add(stems["signature"], clink, CONTACT_SECONDS, 0.22)
    add(stems["signature"], procedural_snap(variation, 0.15), CONTACT_SECONDS, 0.105)

    plastic = fade(cut("plastic_core", 0.875, 0.520), 0.002, 0.090)
    steel = fade(cut("steel_chair", 8.625 if variation == 0 else 14.235, 0.420), 0.002, 0.080)
    add(stems["chair"], pan(filtered(plastic, highpass=70, lowpass=13_500), -0.18), CONTACT_SECONDS, 0.55)
    add(stems["chair"], pan(filtered(steel, highpass=100, lowpass=9_000), -0.28), CONTACT_SECONDS + 0.006, 0.52)

    body = fade(cut("body_coat", body_times[variation], 0.500), 0.002, 0.100)
    cloth = fade(cut("cloth_hit", 0.015, 0.300), 0.001, 0.060)
    soft = fade(source(f"kenney_soft_{variation + 1}"), 0.002, 0.050)
    add(stems["runner"], pan(filtered(body, highpass=55, lowpass=5_800), 0.22), CONTACT_SECONDS + 0.022, 0.92)
    add(stems["runner"], pan(filtered(cloth, highpass=350, lowpass=9_000), 0.28), CONTACT_SECONDS + 0.014, 0.62)
    add(stems["runner"], pan(filtered(soft, lowpass=4_200), 0.15), CONTACT_SECONDS + 0.030, 0.44)

    scrape = fade(cut("plastic_scrape", scrape_times[variation], 0.610), 0.006, 0.100)
    slide = fade(cut("fast_slide", slide_times[variation], 0.420), 0.008, 0.090)
    tail = fade(cut("chair_roll", roll_times[variation] + 0.15, 0.520), 0.010, 0.120)
    add(stems["floor_tail"], pan(filtered(scrape, highpass=160, lowpass=8_500), -0.05), CONTACT_SECONDS + 0.180, 0.27)
    add(stems["floor_tail"], pan(filtered(slide, highpass=250, lowpass=7_000), 0.12), CONTACT_SECONDS + 0.265, 0.12)
    add(stems["floor_tail"], pan(filtered(tail, highpass=140, lowpass=6_500), 0.05), CONTACT_SECONDS + 0.300, 0.32)
    return stems


def arcade_edge(variation: int) -> dict[str, np.ndarray]:
    stems = {name: blank() for name in ("pre", "signature", "chair", "runner", "floor_tail")}
    slide_times = (0.18, 8.18)
    body_times = (4.675, 0.115)

    whoosh = cut("fast_slide", slide_times[variation], 0.145)[::-1]
    whoosh = fade(filtered(whoosh, highpass=700, lowpass=11_500), 0.030, 0.004)
    rising = np.linspace(0.08, 1.0, len(whoosh))[:, None] ** 1.7
    add(stems["pre"], pan(whoosh * rising, -0.05), CONTACT_SECONDS - 0.125, 0.25)

    clink = cut(f"metal_clink_{variation + 2}", 0.010, 0.125)
    clink = envelope(fade(filtered(clink, highpass=1_500, lowpass=14_000), 0.001, 0.025), 1.35, 0.025)
    add(stems["signature"], clink, CONTACT_SECONDS, 0.42)
    add(stems["signature"], procedural_snap(variation, 1.0), CONTACT_SECONDS, 0.24)

    plastic = envelope(fade(cut("plastic_core", 0.888, 0.400), 0.001, 0.070), 1.45, 0.070)
    metal = source(f"kenney_metal_{variation + 1}")
    metal = envelope(fade(filtered(metal, highpass=90, lowpass=14_000), 0.001, 0.050), 1.35, 0.050)
    add(stems["chair"], pan(soft_saturate(plastic, 1.35), -0.14), CONTACT_SECONDS, 0.55)
    add(stems["chair"], pan(soft_saturate(metal, 1.50), -0.22), CONTACT_SECONDS + 0.002, 0.74)

    body = fade(cut("body_coat", body_times[variation], 0.390), 0.001, 0.075)
    punch = fade(source(f"kenney_punch_{variation + 1}"), 0.001, 0.050)
    thud = source("metal_thud_3" if variation == 0 else "metal_thud_2")
    thud = repitch(filtered(thud, lowpass=3_200), 0.82 if variation == 0 else 0.88, 0.500)
    add(stems["runner"], pan(soft_saturate(filtered(body, highpass=55, lowpass=5_000), 1.25), 0.18), CONTACT_SECONDS + 0.014, 0.78)
    add(stems["runner"], pan(soft_saturate(punch, 1.45), 0.15), CONTACT_SECONDS + 0.011, 0.66)
    add(stems["runner"], pan(thud, 0.08), CONTACT_SECONDS + 0.028, 0.42)

    scrape = cut("plastic_scrape", 0.350 if variation == 0 else 2.300, 0.420)
    scrape = fade(filtered(scrape, highpass=200, lowpass=8_000), 0.004, 0.080)
    roll = fade(cut("chair_roll", 37.90 if variation == 0 else 35.78, 0.390), 0.005, 0.090)
    add(stems["floor_tail"], pan(scrape, 0.10), CONTACT_SECONDS + 0.155, 0.24)
    add(stems["floor_tail"], pan(filtered(roll, highpass=170, lowpass=7_000), -0.05), CONTACT_SECONDS + 0.260, 0.25)
    return stems


def target_hybrid(variation: int) -> dict[str, np.ndarray]:
    stems = {name: blank() for name in ("pre", "signature", "chair", "runner", "floor_tail")}
    roll_times = (35.66, 29.91)
    body_times = (4.675, 2.335)
    scrape_times = (2.270, 0.325)

    pre_roll = fade(cut("chair_roll", roll_times[variation], 0.250), 0.008, 0.045)
    pre_air = cut("fast_slide", 1.12 if variation == 0 else 5.42, 0.110)[::-1]
    pre_air = fade(filtered(pre_air, highpass=900, lowpass=10_500), 0.025, 0.003)
    add(stems["pre"], pan(filtered(pre_roll, highpass=150, lowpass=7_800), -0.10), CONTACT_SECONDS - 0.145, 0.66)
    add(stems["pre"], pan(pre_air, -0.02), CONTACT_SECONDS - 0.098, 0.12)

    clink = cut(f"metal_clink_{variation + 2}", 0.010, 0.135)
    clink = envelope(fade(filtered(clink, highpass=1_250, lowpass=13_000), 0.001, 0.030), 1.15, 0.030)
    add(stems["signature"], clink, CONTACT_SECONDS, 0.31)
    add(stems["signature"], procedural_snap(variation, 0.62), CONTACT_SECONDS, 0.17)

    plastic = envelope(fade(cut("plastic_core", 0.882, 0.490), 0.001, 0.085), 1.18, 0.065)
    steel = fade(cut("steel_chair", 8.630 if variation == 0 else 14.245, 0.390), 0.001, 0.075)
    metal = fade(source(f"kenney_metal_{variation + 1}"), 0.001, 0.050)
    add(stems["chair"], pan(filtered(plastic, highpass=65, lowpass=13_500), -0.16), CONTACT_SECONDS, 0.58)
    add(stems["chair"], pan(filtered(steel, highpass=100, lowpass=9_500), -0.26), CONTACT_SECONDS + 0.004, 0.42)
    add(stems["chair"], pan(filtered(metal, highpass=120, lowpass=13_000), -0.20), CONTACT_SECONDS + 0.003, 0.40)

    body = fade(cut("body_coat", body_times[variation], 0.460), 0.001, 0.090)
    cloth = fade(cut("cloth_hit", 0.015, 0.300), 0.001, 0.060)
    punch = fade(source(f"kenney_punch_{variation + 1}"), 0.001, 0.050)
    soft = fade(source(f"kenney_soft_{variation + 1}"), 0.001, 0.060)
    add(stems["runner"], pan(filtered(body, highpass=52, lowpass=5_500), 0.20), CONTACT_SECONDS + 0.018, 0.84)
    add(stems["runner"], pan(filtered(cloth, highpass=320, lowpass=9_500), 0.30), CONTACT_SECONDS + 0.013, 0.54)
    add(stems["runner"], pan(filtered(punch, lowpass=6_500), 0.14), CONTACT_SECONDS + 0.018, 0.38)
    add(stems["runner"], pan(filtered(soft, lowpass=4_000), 0.18), CONTACT_SECONDS + 0.030, 0.35)

    scrape = fade(cut("plastic_scrape", scrape_times[variation], 0.570), 0.005, 0.100)
    roll = fade(cut("chair_roll", roll_times[variation] + 0.15, 0.500), 0.008, 0.110)
    floor = fade(source(f"kenney_floor_{variation + 1}"), 0.001, 0.055)
    add(stems["floor_tail"], pan(filtered(scrape, highpass=170, lowpass=8_200), 0.08), CONTACT_SECONDS + 0.170, 0.25)
    add(stems["floor_tail"], pan(filtered(roll, highpass=140, lowpass=6_800), 0.00), CONTACT_SECONDS + 0.285, 0.30)
    add(stems["floor_tail"], pan(filtered(floor, highpass=80, lowpass=6_000), 0.16), CONTACT_SECONDS + 0.105, 0.22)
    return stems


BUILDERS = {"A": grounded, "B": arcade_edge, "C": target_hybrid}


def integrated_loudness(data: np.ndarray) -> float:
    meter = pyln.Meter(SAMPLE_RATE, block_size=0.200)
    return float(meter.integrated_loudness(data))


def normalize_stems(stems: dict[str, np.ndarray], target_lufs: float = TARGET_LUFS) -> tuple[dict[str, np.ndarray], np.ndarray, float]:
    composite = sum(stems.values(), start=blank())
    loudness = integrated_loudness(composite)
    gain = 10 ** ((target_lufs - loudness) / 20.0)
    peak_after_loudness = float(np.max(np.abs(composite * gain)))
    if peak_after_loudness > MAX_SAMPLE_PEAK:
        gain *= MAX_SAMPLE_PEAK / peak_after_loudness
    normalized = {name: data * gain for name, data in stems.items()}
    composite = sum(normalized.values(), start=blank())
    return normalized, composite, gain


def true_peak(data: np.ndarray) -> float:
    oversampled = signal.resample_poly(data, 4, 1, axis=0)
    return float(np.max(np.abs(oversampled)))


def write_wav(path: Path, data: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sf.write(path, np.clip(data, -1.0, 1.0), SAMPLE_RATE, subtype="PCM_24")


def metrics(path: Path) -> dict[str, object]:
    data, rate = sf.read(path, always_2d=True, dtype="float64")
    return {
        "path": path.relative_to(ROOT).as_posix(),
        "sha256": sha256(path),
        "sample_rate": int(rate),
        "channels": int(data.shape[1]),
        "frames": int(data.shape[0]),
        "duration_seconds": round(len(data) / rate, 6),
        "sample_peak_dbfs": round(20 * math.log10(max(1e-12, float(np.max(np.abs(data))))), 4),
        "true_peak_dbtp": round(20 * math.log10(max(1e-12, true_peak(data))), 4),
        "integrated_lufs": round(integrated_loudness(data), 4),
        "dc_offset_max": float(np.max(np.abs(np.mean(data, axis=0)))),
        "finite": bool(np.isfinite(data).all()),
        "subtype": sf.info(path).subtype,
    }


def make_audition(first: np.ndarray, second: np.ndarray) -> np.ndarray:
    gap = np.zeros((int(0.800 * SAMPLE_RATE), 2), dtype=np.float64)
    return np.concatenate((first, gap, second), axis=0)


def make_waveform_sheet(candidates: dict[str, list[np.ndarray]]) -> None:
    import matplotlib

    matplotlib.use("Agg")
    import matplotlib.pyplot as plt

    figure, axes = plt.subplots(3, 2, figsize=(14, 8), sharex=True, sharey=True)
    timeline = np.arange(int(RENDER_SECONDS * SAMPLE_RATE)) / SAMPLE_RATE - CONTACT_SECONDS
    for row, code in enumerate(("A", "B", "C")):
        for column, data in enumerate(candidates[code]):
            mono = np.mean(data, axis=1)
            axes[row, column].plot(timeline[::24] * 1000, mono[::24], linewidth=0.7, color=("#60a5fa", "#f97316", "#a78bfa")[row])
            axes[row, column].axvline(0, color="#ef4444", linewidth=1.0)
            axes[row, column].set_title(f"{code} {DIRECTIONS[code].label} — variation {column + 1}")
            axes[row, column].grid(alpha=0.18)
    for axis in axes[-1]:
        axis.set_xlabel("milliseconds from contact")
    for axis in axes[:, 0]:
        axis.set_ylabel("amplitude")
    figure.suptitle("BREAK NOTICE — Chair → Runner T3 REVIEW waveforms (contact = 0 ms)")
    figure.tight_layout()
    output = RENDERS / "Evidence" / "NORTH_STAR_WAVEFORMS_REVIEW.png"
    output.parent.mkdir(parents=True, exist_ok=True)
    figure.savefig(output, dpi=160)
    plt.close(figure)


def clean_generated_outputs() -> None:
    for path in (RENDERS / "Candidates", RENDERS / "Blind", RENDERS / "Stems", RENDERS / "Evidence"):
        if path.exists():
            shutil.rmtree(path)
    if UNITY_REVIEW.exists():
        for child in UNITY_REVIEW.iterdir():
            if child.is_file() and child.suffix.lower() in {".wav", ".json", ".md"}:
                child.unlink()
            elif child.is_dir() and child.name == "Stems":
                shutil.rmtree(child)


def main() -> None:
    clean_generated_outputs()
    for path in SOURCE_FILES.values():
        if not path.is_file():
            raise FileNotFoundError(path)

    candidate_audio: dict[str, list[np.ndarray]] = {code: [] for code in DIRECTIONS}
    output_records: list[dict[str, object]] = []
    build_records: list[dict[str, object]] = []

    for code, direction in DIRECTIONS.items():
        for variation in (0, 1):
            raw_stems = BUILDERS[code](variation)
            stems, composite, gain = normalize_stems(raw_stems)
            candidate_audio[code].append(composite)
            composite_path = RENDERS / "Candidates" / code / f"{code}_{direction.label.replace(' ', '')}_v{variation + 1:02d}_REVIEW.wav"
            write_wav(composite_path, composite)
            output_records.append(metrics(composite_path))
            for stem_name, stem_audio in stems.items():
                stem_path = RENDERS / "Stems" / code / f"{code}_v{variation + 1:02d}_{stem_name}_REVIEW.wav"
                write_wav(stem_path, stem_audio)
                output_records.append(metrics(stem_path))
            build_records.append(
                {
                    "candidate": code,
                    "label": direction.label,
                    "variation": variation + 1,
                    "normalization_gain_db": round(20 * math.log10(gain), 4),
                    "contact_seconds": CONTACT_SECONDS,
                    "stems": list(stems),
                }
            )

    blind_codes = list(DIRECTIONS)
    random.Random(BLIND_SEED).shuffle(blind_codes)
    blind_mapping = {blind: code for blind, code in zip(("X", "Y", "Z"), blind_codes, strict=True)}

    UNITY_REVIEW.mkdir(parents=True, exist_ok=True)
    for blind, code in blind_mapping.items():
        for variation, composite in enumerate(candidate_audio[code], start=1):
            blind_path = RENDERS / "Blind" / f"BLIND_{blind}_v{variation:02d}_REVIEW.wav"
            write_wav(blind_path, composite)
            output_records.append(metrics(blind_path))
            shutil.copy2(blind_path, UNITY_REVIEW / blind_path.name)
            for stem_name in ("pre", "signature", "chair", "runner", "floor_tail"):
                source_stem = RENDERS / "Stems" / code / f"{code}_v{variation:02d}_{stem_name}_REVIEW.wav"
                unity_stem = UNITY_REVIEW / "Stems" / blind / f"BLIND_{blind}_v{variation:02d}_{stem_name}_REVIEW.wav"
                unity_stem.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(source_stem, unity_stem)
        audition = make_audition(candidate_audio[code][0], candidate_audio[code][1])
        audition_path = RENDERS / "Blind" / f"BLIND_{blind}_AUDITION_2VAR_REVIEW.wav"
        write_wav(audition_path, audition)
        output_records.append(metrics(audition_path))
        shutil.copy2(audition_path, UNITY_REVIEW / audition_path.name)

    mapping_payload = {
        "warning": "DO NOT OPEN BEFORE THE HUMAN OWNER RECORDS AN X/Y/Z LISTENING DECISION.",
        "seed_hex": hex(BLIND_SEED),
        "mapping": blind_mapping,
        "candidate_intent": {code: direction.__dict__ for code, direction in DIRECTIONS.items()},
    }
    mapping_path = RENDERS / "PRIVATE_BLIND_MAPPING.json"
    mapping_path.write_text(json.dumps(mapping_payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    source_records = []
    for name, path in sorted(SOURCE_FILES.items()):
        info = sf.info(path)
        source_records.append(
            {
                "role_id": name,
                "path": path.relative_to(ROOT).as_posix(),
                "sha256": sha256(path),
                "sample_rate": info.samplerate,
                "channels": info.channels,
                "duration_seconds": round(info.duration, 6),
                "format": info.format,
                "subtype": info.subtype,
            }
        )

    named_composites = {
        code: [metrics(RENDERS / "Candidates" / code / f"{code}_{DIRECTIONS[code].label.replace(' ', '')}_v{variation:02d}_REVIEW.wav") for variation in (1, 2)]
        for code in DIRECTIONS
    }
    correlations = {}
    for code, renders in candidate_audio.items():
        first = np.mean(renders[0], axis=1)
        second = np.mean(renders[1], axis=1)
        correlations[code] = round(float(np.corrcoef(first, second)[0, 1]), 6)

    qa_payload = {
        "status": "REVIEW_CANDIDATE_TECHNICAL_PASS_PENDING_HUMAN_LISTENING",
        "sample_rate": SAMPLE_RATE,
        "bit_depth": 24,
        "channels": 2,
        "contact_seconds": CONTACT_SECONDS,
        "target_lufs": TARGET_LUFS,
        "max_sample_peak_dbfs": round(20 * math.log10(MAX_SAMPLE_PEAK), 4),
        "candidate_variation_correlations": correlations,
        "named_composites": named_composites,
        "all_output_metrics": output_records,
        "checks": {
            "all_finite": all(bool(item["finite"]) for item in output_records),
            "all_48k_stereo_pcm24": all(
                item["sample_rate"] == SAMPLE_RATE and item["channels"] == 2 and item["subtype"] == "PCM_24"
                for item in output_records
            ),
            "no_sample_clipping": all(float(item["sample_peak_dbfs"]) < -0.05 for item in output_records),
            "dc_below_0_001": all(float(item["dc_offset_max"]) < 0.001 for item in output_records),
            "two_material_variations_per_direction": all(value < 0.995 for value in correlations.values()),
        },
        "human_only_checks": [
            "impact quality and satisfaction",
            "chair / runner / floor material separation",
            "contact synchronization",
            "laptop-speaker transient readability",
            "20-repeat fatigue and monotony",
            "command and state cue non-masking",
        ],
    }
    qa_path = RENDERS / "Evidence" / "TECHNICAL_QA_REVIEW.json"
    qa_path.parent.mkdir(parents=True, exist_ok=True)
    qa_path.write_text(json.dumps(qa_payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    make_waveform_sheet(candidate_audio)
    waveform_path = RENDERS / "Evidence" / "NORTH_STAR_WAVEFORMS_REVIEW.png"

    manifest_payload = {
        "status": "REVIEW_ONLY",
        "baseline": "b15d971375cfb3a0be7d978f9311b58702e9d431",
        "branch": "codex/showcase-audio-rebuild-v003",
        "old_audio_reused": False,
        "source_rights_boundary": {
            "all_sources": "newly acquired CC0 source bytes; no prior pack reuse",
            "freesound_bytes": "public HQ preview MP3 derivatives used for REVIEW only",
            "before_final": "replace selected Freesound previews with original lossless downloads and rerun QA unless the Human Owner explicitly accepts preview-derived sources",
        },
        "source_records": source_records,
        "build_records": build_records,
        "blind_labels": list(blind_mapping),
        "private_mapping_file": mapping_path.relative_to(ROOT).as_posix(),
        "qa_file": qa_path.relative_to(ROOT).as_posix(),
        "review_waveform": {
            "path": waveform_path.relative_to(ROOT).as_posix(),
            "sha256": sha256(waveform_path),
            "bytes": waveform_path.stat().st_size,
        },
    }
    manifest_path = RENDERS / "Evidence" / "BUILD_MANIFEST_REVIEW.json"
    manifest_path.write_text(json.dumps(manifest_payload, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")

    unity_manifest = {
        "status": "LOCAL_REVIEW_ONLY",
        "blind_labels": ["X", "Y", "Z"],
        "variations_each": 2,
        "stem_order": ["pre", "signature", "chair", "runner", "floor_tail"],
        "contact_seconds": CONTACT_SECONDS,
        "do_not_reveal_mapping_before_choice": True,
    }
    (UNITY_REVIEW / "REVIEW_MANIFEST.json").write_text(json.dumps(unity_manifest, indent=2) + "\n", encoding="utf-8")
    (UNITY_REVIEW / "README.md").write_text(
        "# LOCAL REVIEW ONLY\n\nThese clips are blind X/Y/Z review copies for the isolated Chair → Runner T3 Impact Lab. They are not Runtime or FINAL assets.\n",
        encoding="utf-8",
    )

    if not all(qa_payload["checks"].values()):
        raise RuntimeError(f"Technical QA failed: {qa_payload['checks']}")
    print(json.dumps({"blind_labels": list(blind_mapping), "qa": qa_payload["checks"]}, indent=2))


if __name__ == "__main__":
    main()
