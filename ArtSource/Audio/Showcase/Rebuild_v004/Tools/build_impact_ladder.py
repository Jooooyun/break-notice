#!/usr/bin/env python3
"""Build the isolated BREAK NOTICE Arcade Edge impact ladder v004.

The production graph accepts only exact-hash lossless assets located below
this pack's Sources/Lossless directory. Reference comparisons are assembled by
a separate script and never enter these cue builders.
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

import matplotlib

matplotlib.use("Agg")
import matplotlib.pyplot as plt
import numpy as np
import pyloudnorm as pyln
import soundfile as sf
from scipy import signal


SAMPLE_RATE = 48_000
CONTACT_SECONDS = 0.200
RENDER_SECONDS = 1.350
RENDER_FRAMES = int(SAMPLE_RATE * RENDER_SECONDS)
STEMS = ("pre", "signature", "emitter", "runner", "floor_tail")
BUILD_SEED = 0x42524B4E_56303034

ROOT = Path(__file__).resolve().parents[1]
LOSSLESS = (ROOT / "Sources" / "Lossless").resolve()
RENDERS = ROOT / "Renders"
COMPOSITES = RENDERS / "Composites"
STEM_ROOT = RENDERS / "Stems"
DERIVED = RENDERS / "Derived"
EVIDENCE = RENDERS / "Evidence"
PRIVATE = RENDERS / "Private"


@dataclass(frozen=True)
class SourceSpec:
    relative_path: str
    sha256: str
    source_page: str
    role: str


@dataclass(frozen=True)
class CueSpec:
    cue_id: str
    family: str
    tier: int
    variation: int
    target_true_peak_dbfs: float
    branded_click: bool


SOURCE_SPECS: dict[str, SourceSpec] = {
    "punch10": SourceSpec("LaSonotheque/Punch_10_2465.wav", "049ce0074a7f779b5fbf9aad0389fcf7b7a84ca8551d12206c80a0de8a814d2f", "https://lasonotheque.org/coup-de-poing-10-s2465.html", "new hand/body contact"),
    "punch11": SourceSpec("LaSonotheque/Punch_11_2466.wav", "8b7da28fbb95c4a0097619e9aa752e8748a88dd97a630b0d5a241c2febb67e6d", "https://lasonotheque.org/coup-de-poing-11-s2466.html", "new hand/body contact"),
    "punch12": SourceSpec("LaSonotheque/Punch_12_2467.wav", "ad8facc4c662883a3b2b5e68b3ca43a0798cb32c7ac2a2f26353f07d35de5480", "https://lasonotheque.org/coup-de-poing-12-s2467.html", "new hand/body contact"),
    "body_hit": SourceSpec("OwlishMedia/impact_hit.wav", "7db1b151be34dd7721eb4eb8efd6f9123e660751e4317256dbc1abfd3abe7500", "https://opengameart.org/content/sound-effects-pack", "body weight"),
    "hand_slap": SourceSpec("OwlishMedia/impact_slap2.wav", "2b51f0b3f88e2df9a15d367c8864070b386d15a198577c4d7c7a19b3998361e3", "https://opengameart.org/content/sound-effects-pack", "hand emitter"),
    "shoulder_cloth": SourceSpec("OwlishMedia/impact_shouldergrab.wav", "13fa57a5d81879458297ba2c45365931ba94e60a6126976dc1eed81abddabc63", "https://opengameart.org/content/sound-effects-pack", "cloth/body tail"),
    "body_fruit": SourceSpec("OwlishMedia/impact_fruit1.wav", "f48f9caf344bbfc58415f7255b0f1f99d6ec789352592ae9be9aa0797e9b1e5b", "https://opengameart.org/content/sound-effects-pack", "organic low body layer"),
    "cloth4": SourceSpec("OwlishMedia/cloth_blanket_movement_4.wav", "9b3c97b7ada8ee6ba2c05df379e4d89a6f4ba715cc90cdf11d4849f3aa0e05b0", "https://opengameart.org/content/sound-effects-pack", "cloth reaction"),
    "cloth6": SourceSpec("OwlishMedia/cloth_blanket_movement_6.wav", "a082018a475c403f0168521c2bb6a443766f42a6da410a2e8a22cc3047d59ffb", "https://opengameart.org/content/sound-effects-pack", "cloth reaction"),
    "paper_crumple": SourceSpec("OwlishMedia/paper_crumples_1.wav", "564f3538593a4d174517e2970637efa82584001aef3e8e1bf61595e4e2c3322f", "https://opengameart.org/content/sound-effects-pack", "binder paper body"),
    "paper_flips": SourceSpec("OwlishMedia/paper_sketchbook_flips_1.wav", "880a7342ca3e4e6e12260e0f64b1846e3e74618e4a10e2315b89243a505bbcdc", "https://opengameart.org/content/sound-effects-pack", "binder paper tail"),
    "book2": SourceSpec("BookFlip/BookFlip_2.wav", "27e8f82128642df83e8dfb7fd170c2575080eadb5c65e8d2aecd94cd0a67f3fd", "https://opengameart.org/content/book-flip-sounds", "binder page/body"),
    "book9": SourceSpec("BookFlip/BookFlip_9.wav", "ef7b31c4c5e72d6f55562c0ed927b7b50cb4587f2cc2c8dfa3cf74777eb49c79", "https://opengameart.org/content/book-flip-sounds", "binder page/body"),
    "book12": SourceSpec("BookFlip/BookFlip_12.wav", "6fb6363219c4dfda014e70ff72e091f6dffb4bc50767a1a62df39ee9c3c4af65", "https://opengameart.org/content/book-flip-sounds", "binder page/body"),
    "plastic": SourceSpec("Workshop/hammering_on_plastic.wav", "88a391f5e157e60d4d80f38c4c19d806664084d490f680a023d5dea1aa70f118", "https://opengameart.org/content/68-workshop-sounds", "binder board/chair shell"),
    "ring": SourceSpec("Workshop/jingle_3.wav", "099bf9d1442cbecd202714482b3e04cd425fa4844c83ea9cf11a1dc4d53e007c", "https://opengameart.org/content/68-workshop-sounds", "binder ring/chair click"),
    "metal_drop": SourceSpec("Workshop/metal_drop_2.wav", "8cb516fa7323fd43b0f3a45814b7fbaf063c1052f7e985b02a096c859b9e8f37", "https://opengameart.org/content/68-workshop-sounds", "chair metal weight"),
    "thuds": SourceSpec("Workshop/thuds.wav", "c1974e127edae7f234693161c0c952e0cc4be4f67ddc8e49113c8db7553bd1d3", "https://opengameart.org/content/68-workshop-sounds", "chair/body low weight"),
    "workshop_scrape": SourceSpec("Workshop/scrape_5.wav", "58e7ab218e72d564c34b444ad7adfe8596eed2c00d998d0c6d978bfffec0e9b2", "https://opengameart.org/content/68-workshop-sounds", "chair floor tail"),
    "metal06": SourceSpec("Metal27/dull_metal_collision_06.wav", "d0d9d11495949aad7ff9dfd96196d45b3c10468c76c2c06942da9ee6d4f5fff4", "https://opengameart.org/content/27-metal-audio-samples-sfx", "chair frame"),
    "metal08": SourceSpec("Metal27/dull_metal_collision_08.wav", "6a9646ce4df6f0173e159b8048c41f64ff447f530da34dbdda75e4721a2537bd", "https://opengameart.org/content/27-metal-audio-samples-sfx", "chair frame"),
    "metal11": SourceSpec("Metal27/dull_metal_collision_11.wav", "df84f9f2caa4b39ac051c82eb374d1762ce28fb4c136e4466a18945d3b012ecf", "https://opengameart.org/content/27-metal-audio-samples-sfx", "chair frame"),
    "wobble": SourceSpec("Metal27/wobble_08.wav", "98829d6d35fb464ee5ee0731a2bc6cb62405d13cf1e9a5adedfd842ed288c55b", "https://opengameart.org/content/27-metal-audio-samples-sfx", "chair settle"),
    "scrape1": SourceSpec("Scrapes/scrape_1.wav", "21075c276b96d0c602ac19e4d2235e3197bc9c35d27ed10eda4308e874125978", "https://opengameart.org/content/scrapes", "chair approach"),
    "scrape2": SourceSpec("Scrapes/scrape_2.wav", "9b93f353f0155ffa391b1074a1595dd00321e8c604efe0ca9c1192a5f86129a0", "https://opengameart.org/content/scrapes", "chair approach"),
}


CUES = (
    CueSpec("T1_HAND_RUNNER_v01", "T1_HAND_RUNNER", 1, 1, -7.2, False),
    CueSpec("T1_HAND_RUNNER_v02", "T1_HAND_RUNNER", 1, 2, -6.8, False),
    CueSpec("T1_HAND_RUNNER_v03", "T1_HAND_RUNNER", 1, 3, -6.4, False),
    CueSpec("T2_HOOK_RUNNER_v01", "T2_HOOK_RUNNER", 2, 1, -3.9, True),
    CueSpec("T2_HOOK_RUNNER_v02", "T2_HOOK_RUNNER", 2, 2, -3.6, True),
    CueSpec("T2_HOOK_RUNNER_v03", "T2_HOOK_RUNNER", 2, 3, -3.3, True),
    CueSpec("T2_BINDER_RUNNER_v01", "T2_BINDER_RUNNER", 2, 1, -3.7, True),
    CueSpec("T2_BINDER_RUNNER_v02", "T2_BINDER_RUNNER", 2, 2, -3.4, True),
    CueSpec("T2_BINDER_RUNNER_v03", "T2_BINDER_RUNNER", 2, 3, -3.1, True),
    CueSpec("T3_CHAIR_RUNNER_RC_v01", "T3_CHAIR_RUNNER_RC", 3, 1, -1.5, True),
    CueSpec("T3_CHAIR_RUNNER_RC_v02", "T3_CHAIR_RUNNER_RC", 3, 2, -1.2, True),
)


SOURCE_AUDIO: dict[str, np.ndarray] = {}
SOURCE_INFO: dict[str, dict[str, object]] = {}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def source_path(name: str) -> Path:
    path = (LOSSLESS / SOURCE_SPECS[name].relative_path).resolve()
    if not path.is_relative_to(LOSSLESS):
        raise RuntimeError(f"Source escaped lossless root: {path}")
    return path


def validate_sources() -> None:
    allowed_suffixes = {".wav", ".flac", ".aiff", ".aif"}
    allowed_formats = {"WAV", "WAVEX", "FLAC", "AIFF"}
    allowed_lossless_subtypes = {"PCM_16", "PCM_24", "PCM_32", "FLOAT", "DOUBLE"}
    hashes: set[str] = set()
    for name, spec in SOURCE_SPECS.items():
        path = source_path(name)
        if path.suffix.lower() not in allowed_suffixes:
            raise RuntimeError(f"Lossy or unsupported extension rejected: {path}")
        if not path.is_file():
            raise FileNotFoundError(path)
        actual = sha256(path)
        if actual != spec.sha256:
            raise RuntimeError(f"Source SHA mismatch for {name}: {actual}")
        if actual in hashes:
            raise RuntimeError(f"Duplicate source bytes: {name}")
        hashes.add(actual)
        info = sf.info(path)
        if info.format not in allowed_formats or info.subtype not in allowed_lossless_subtypes:
            raise RuntimeError(f"Lossless decode contract failed for {name}: {info}")
        SOURCE_INFO[name] = {
            "path": spec.relative_path,
            "sha256": actual,
            "format": info.format,
            "subtype": info.subtype,
            "sample_rate": info.samplerate,
            "channels": info.channels,
            "frames": info.frames,
            "duration_seconds": info.duration,
            "source_page": spec.source_page,
            "role": spec.role,
        }


def ensure_stereo(data: np.ndarray) -> np.ndarray:
    if data.ndim == 1:
        data = data[:, None]
    if data.shape[1] == 1:
        data = np.repeat(data, 2, axis=1)
    elif data.shape[1] > 2:
        data = data[:, :2]
    return np.asarray(data, dtype=np.float64)


def resample(data: np.ndarray, source_rate: int) -> np.ndarray:
    if source_rate == SAMPLE_RATE:
        return data
    ratio = Fraction(SAMPLE_RATE, source_rate).limit_denominator(2_000)
    return signal.resample_poly(data, ratio.numerator, ratio.denominator, axis=0)


def source(name: str) -> np.ndarray:
    if name not in SOURCE_AUDIO:
        data, rate = sf.read(source_path(name), always_2d=True, dtype="float64")
        data = resample(ensure_stereo(data), int(rate))
        data -= np.mean(data, axis=0, keepdims=True)
        SOURCE_AUDIO[name] = data
    return SOURCE_AUDIO[name]


def blank() -> np.ndarray:
    return np.zeros((RENDER_FRAMES, 2), dtype=np.float64)


def add(canvas: np.ndarray, data: np.ndarray, start_seconds: float, gain: float = 1.0) -> None:
    first = int(round(start_seconds * SAMPLE_RATE))
    if first < 0:
        data = data[-first:]
        first = 0
    if first >= len(canvas) or len(data) == 0:
        return
    count = min(len(data), len(canvas) - first)
    canvas[first:first + count] += data[:count] * gain


def fade(data: np.ndarray, fade_in: float = 0.0015, fade_out: float = 0.035) -> np.ndarray:
    result = data.copy()
    count_in = min(len(result), int(round(fade_in * SAMPLE_RATE)))
    count_out = min(len(result), int(round(fade_out * SAMPLE_RATE)))
    if count_in > 1:
        result[:count_in] *= np.sin(np.linspace(0, math.pi / 2, count_in))[:, None] ** 2
    if count_out > 1:
        result[-count_out:] *= np.cos(np.linspace(0, math.pi / 2, count_out))[:, None] ** 2
    return result


def filtered(data: np.ndarray, highpass: float | None = None, lowpass: float | None = None, order: int = 3) -> np.ndarray:
    if highpass is None and lowpass is None:
        return data.copy()
    nyquist = SAMPLE_RATE / 2
    if highpass is not None and lowpass is not None:
        frequencies: float | list[float] = [highpass / nyquist, lowpass / nyquist]
        kind = "bandpass"
    elif highpass is not None:
        frequencies = highpass / nyquist
        kind = "highpass"
    else:
        frequencies = float(lowpass) / nyquist
        kind = "lowpass"
    sos = signal.butter(order, frequencies, btype=kind, output="sos")
    try:
        return signal.sosfiltfilt(sos, data, axis=0)
    except ValueError:
        return signal.sosfilt(sos, data, axis=0)


def pan(data: np.ndarray, value: float) -> np.ndarray:
    value = float(np.clip(value, -1.0, 1.0))
    mono = np.mean(data, axis=1)
    left = math.cos((value + 1.0) * math.pi / 4)
    right = math.sin((value + 1.0) * math.pi / 4)
    return np.column_stack((mono * left, mono * right)) * math.sqrt(2)


def repitch(data: np.ndarray, ratio: float) -> np.ndarray:
    return signal.resample(data, max(1, int(round(len(data) / ratio))), axis=0)


def soft_saturate(data: np.ndarray, drive: float) -> np.ndarray:
    return np.tanh(data * drive) / max(1e-9, math.tanh(drive))


def normalize_peak(data: np.ndarray) -> np.ndarray:
    peak = float(np.max(np.abs(data))) if len(data) else 0.0
    return data / max(peak, 1e-9)


def transient(name: str, occurrence: int, before: float, after: float, pitch: float = 1.0) -> np.ndarray:
    data = source(name)
    mono = np.max(np.abs(data), axis=1)
    smooth_count = max(1, int(round(0.002 * SAMPLE_RATE)))
    envelope = np.convolve(mono, np.ones(smooth_count) / smooth_count, mode="same")
    peaks, properties = signal.find_peaks(
        envelope,
        height=max(1e-7, float(np.max(envelope)) * 0.16),
        distance=int(round(0.070 * SAMPLE_RATE)),
    )
    if len(peaks) == 0:
        peaks = np.array([int(np.argmax(envelope))])
    ranked = peaks[np.argsort(properties.get("peak_heights", envelope[peaks]))[::-1]]
    chosen = int(ranked[occurrence % len(ranked)])
    first = max(0, chosen - int(round(before * SAMPLE_RATE)))
    last = min(len(data), chosen + int(round(after * SAMPLE_RATE)))
    segment = data[first:last]
    if pitch != 1.0:
        segment = repitch(segment, pitch)
    return fade(normalize_peak(segment), min(0.002, before * 0.5), min(0.060, after * 0.35))


def reverse_pre(name: str, occurrence: int, seconds: float, highpass: float, lowpass: float) -> np.ndarray:
    segment = transient(name, occurrence, 0.010, max(0.060, seconds), 1.0)
    segment = filtered(segment, highpass=highpass, lowpass=lowpass)
    segment = normalize_peak(segment[::-1])
    count = min(len(segment), int(round(seconds * SAMPLE_RATE)))
    return fade(segment[-count:], 0.018, 0.003)


def procedural_snap(variation: int, tier: int) -> np.ndarray:
    rng = np.random.default_rng(BUILD_SEED + tier * 1009 + variation * 97)
    duration = {1: 0.026, 2: 0.036, 3: 0.046}[tier]
    count = int(round(duration * SAMPLE_RATE))
    noise = rng.standard_normal(count)
    band = signal.sosfilt(
        signal.butter(3, [2_100 / (SAMPLE_RATE / 2), 13_500 / (SAMPLE_RATE / 2)], btype="bandpass", output="sos"),
        noise,
    )
    time = np.arange(count) / SAMPLE_RATE
    tone = np.sin(2 * math.pi * (3_900 + variation * 270 + tier * 110) * time)
    decay = np.exp(-np.linspace(0, 9.0 + tier * 1.5, count))
    mono = (band * 0.72 + tone * (0.05 + tier * 0.018)) * decay
    return np.column_stack((mono, mono)) / max(1e-9, np.max(np.abs(mono)))


def brand_click(variation: int, tier: int) -> np.ndarray:
    count = int(round((0.014 + tier * 0.003) * SAMPLE_RATE))
    time = np.arange(count) / SAMPLE_RATE
    base = 2_750 + variation * 180 + tier * 120
    mono = (np.sin(2 * math.pi * base * time) + 0.35 * np.sin(2 * math.pi * base * 2.03 * time))
    mono *= np.exp(-np.linspace(0, 12.0, count))
    mono[0] += 0.8
    mono /= max(1e-9, np.max(np.abs(mono)))
    return np.column_stack((mono, mono))


def new_stems() -> dict[str, np.ndarray]:
    return {name: blank() for name in STEMS}


def build_t1(spec: CueSpec) -> tuple[dict[str, np.ndarray], list[str]]:
    stems = new_stems()
    v = spec.variation - 1
    punches = ("punch11", "punch12", "punch10")
    cloth = ("cloth6", "cloth4", "cloth6")
    air = reverse_pre("hand_slap", v, 0.024 + v * 0.003, 2_100, 13_000)
    add(stems["pre"], air, CONTACT_SECONDS - 0.024 - v * 0.003, 0.018 + v * 0.002)
    add(stems["signature"], procedural_snap(v, 1), CONTACT_SECONDS, 0.12 + v * 0.008)
    hand = transient("hand_slap", v, 0.002, 0.090, 1.08 + v * 0.025)
    add(stems["emitter"], filtered(hand, highpass=700, lowpass=11_000), CONTACT_SECONDS - 0.002, 0.14)
    punch = transient(punches[v], 0, 0.004, 0.190, 1.04 + v * 0.018)
    add(stems["runner"], filtered(punch, highpass=75, lowpass=7_500), CONTACT_SECONDS - 0.004, 0.42)
    body = transient("body_fruit", 0, 0.004, 0.160, 0.98 - v * 0.02)
    add(stems["runner"], filtered(body, highpass=55, lowpass=1_650), CONTACT_SECONDS - 0.004, 0.13)
    tail = transient(cloth[v], v, 0.004, 0.180, 1.02 + v * 0.02)
    add(stems["floor_tail"], filtered(tail, highpass=180, lowpass=4_800), CONTACT_SECONDS + 0.020, 0.075)
    return stems, [punches[v], "hand_slap", "body_fruit", cloth[v]]


def build_t2_hook(spec: CueSpec) -> tuple[dict[str, np.ndarray], list[str]]:
    stems = new_stems()
    v = spec.variation - 1
    punches = ("punch10", "punch11", "punch12")
    cloth = ("cloth4", "cloth6", "shoulder_cloth")[v]
    add(stems["pre"], reverse_pre("hand_slap", v, 0.050 + v * 0.004, 650, 10_500), CONTACT_SECONDS - 0.050 - v * 0.004, 0.13)
    add(stems["signature"], procedural_snap(v + 10, 2), CONTACT_SECONDS, (0.18, 0.22, 0.22)[v])
    add(stems["signature"], brand_click(v + 3, 2), CONTACT_SECONDS + 0.001, 0.040 + v * 0.004)
    hand = transient("hand_slap", v, 0.002, 0.120, 0.98 + v * 0.025)
    add(stems["emitter"], soft_saturate(filtered(hand, highpass=420, lowpass=12_500), 1.65), CONTACT_SECONDS - 0.002, 0.26)
    punch = transient(punches[v], 0, 0.005, 0.250, 0.96 - v * 0.015)
    add(stems["runner"], soft_saturate(filtered(punch, highpass=55, lowpass=8_500), 2.25), CONTACT_SECONDS - 0.005, (0.52, 0.64, 0.58)[v])
    core = transient("body_hit", v, 0.004, 0.230, 0.97 + v * 0.018)
    add(stems["runner"], soft_saturate(filtered(core, highpass=45, lowpass=4_800), 2.10), CONTACT_SECONDS - 0.004, (0.54, 0.46, 0.46)[v])
    organic = transient("body_fruit", 0, 0.004, 0.190, 0.90 - v * 0.018)
    add(stems["runner"], filtered(organic, highpass=45, lowpass=1_250), CONTACT_SECONDS - 0.004, (0.23, 0.18, 0.18)[v])
    tail = transient(cloth, v, 0.004, 0.260, 0.98 + v * 0.02)
    add(stems["floor_tail"], filtered(tail, highpass=120, lowpass=5_200), CONTACT_SECONDS + 0.020, 0.15)
    return stems, [punches[v], "hand_slap", "body_hit", "body_fruit", cloth]


def build_t2_binder(spec: CueSpec) -> tuple[dict[str, np.ndarray], list[str]]:
    stems = new_stems()
    v = spec.variation - 1
    books = ("book2", "book9", "book12")
    add(stems["pre"], reverse_pre(books[v], 0, 0.072 + v * 0.006, 320, 10_500), CONTACT_SECONDS - 0.072 - v * 0.006, 0.25)
    add(stems["signature"], procedural_snap(v + 20, 2), CONTACT_SECONDS, 0.19)
    add(stems["signature"], brand_click(v + 8, 2), CONTACT_SECONDS + 0.001, 0.046)
    ring = transient("ring", v, 0.003, 0.120, 1.02 + v * 0.03)
    add(stems["signature"], filtered(ring, highpass=1_150, lowpass=12_000), CONTACT_SECONDS - 0.003, 0.075)
    board = transient("plastic", v, 0.004, 0.260, 0.96 + v * 0.025)
    add(stems["emitter"], soft_saturate(filtered(board, highpass=90, lowpass=9_500), 2.0), CONTACT_SECONDS - 0.004, 0.50)
    pages = transient(books[v], 0, 0.004, 0.240, 1.01 - v * 0.018)
    add(stems["emitter"], soft_saturate(filtered(pages, highpass=260, lowpass=11_500), 1.75), CONTACT_SECONDS - 0.004, 0.34)
    paper = transient("paper_crumple", v, 0.004, 0.230, 1.02 + v * 0.02)
    add(stems["emitter"], filtered(paper, highpass=260, lowpass=9_500), CONTACT_SECONDS + 0.008, 0.16)
    core = transient("body_hit", v, 0.004, 0.230, 0.98 - v * 0.012)
    add(stems["runner"], soft_saturate(filtered(core, highpass=50, lowpass=4_800), 2.0), CONTACT_SECONDS - 0.004, 0.50)
    organic = transient("body_fruit", 0, 0.004, 0.170, 0.92 + v * 0.015)
    add(stems["runner"], filtered(organic, highpass=45, lowpass=1_350), CONTACT_SECONDS - 0.004, 0.15)
    flutter = transient("paper_flips", v, 0.004, 0.290, 1.00 + v * 0.018)
    add(stems["floor_tail"], filtered(flutter, highpass=280, lowpass=9_500), CONTACT_SECONDS + 0.050, 0.17)
    return stems, [books[v], "plastic", "ring", "paper_crumple", "paper_flips", "body_hit", "body_fruit"]


def build_t3_chair(spec: CueSpec) -> tuple[dict[str, np.ndarray], list[str]]:
    stems = new_stems()
    v = spec.variation - 1
    approach = "scrape1" if v == 0 else "scrape2"
    frame = "metal06" if v == 0 else "metal11"
    punch = "punch10" if v == 0 else "punch12"
    add(stems["pre"], reverse_pre(approach, v, 0.125, 110, 9_000), CONTACT_SECONDS - 0.125, 0.33)
    add(stems["signature"], procedural_snap(v + 30, 3), CONTACT_SECONDS, 0.32)
    add(stems["signature"], brand_click(v + 13, 3), CONTACT_SECONDS + 0.001, 0.14)
    ring = transient("ring", v, 0.003, 0.150, 0.96 + v * 0.04)
    add(stems["signature"], filtered(ring, highpass=1_050, lowpass=13_000), CONTACT_SECONDS - 0.003, 0.105)
    metal = transient(frame, 0, 0.006, 0.430, 0.94 + v * 0.035)
    add(stems["emitter"], filtered(metal, highpass=45, lowpass=12_500), CONTACT_SECONDS - 0.006, 0.82)
    frame_accent = transient("metal08", v, 0.006, 0.300, 0.97 + v * 0.025)
    add(stems["emitter"], filtered(frame_accent, highpass=70, lowpass=10_500), CONTACT_SECONDS - 0.004, 0.18)
    drop = transient("metal_drop", v, 0.006, 0.440, 0.91 + v * 0.03)
    add(stems["emitter"], filtered(drop, highpass=45, lowpass=9_500), CONTACT_SECONDS - 0.006, 0.48)
    shell = transient("plastic", v + 1, 0.004, 0.310, 0.92 - v * 0.02)
    add(stems["emitter"], filtered(shell, highpass=85, lowpass=10_500), CONTACT_SECONDS - 0.004, 0.35)
    body = transient("body_hit", v, 0.005, 0.310, 0.90 - v * 0.02)
    add(stems["runner"], filtered(body, highpass=38, lowpass=4_500), CONTACT_SECONDS - 0.005, 0.66)
    low = transient("thuds", v, 0.006, 0.330, 0.88 + v * 0.03)
    add(stems["runner"], filtered(low, highpass=35, lowpass=1_850), CONTACT_SECONDS - 0.006, 0.47)
    foley = transient(punch, 0, 0.004, 0.230, 0.93 + v * 0.02)
    add(stems["runner"], filtered(foley, highpass=55, lowpass=6_800), CONTACT_SECONDS - 0.004, 0.41)
    wobble = transient("wobble", v, 0.004, 0.450, 0.93 + v * 0.02)
    add(stems["floor_tail"], filtered(wobble, highpass=70, lowpass=7_500), CONTACT_SECONDS + 0.035, 0.34)
    scrape = transient("workshop_scrape", v, 0.004, 0.440, 1.02 - v * 0.025)
    add(stems["floor_tail"], filtered(scrape, highpass=100, lowpass=7_200), CONTACT_SECONDS + 0.090, 0.23)
    return stems, [approach, frame, "metal08", "ring", "metal_drop", "plastic", "body_hit", "thuds", punch, "wobble", "workshop_scrape"]


BUILDERS = {
    "T1_HAND_RUNNER": build_t1,
    "T2_HOOK_RUNNER": build_t2_hook,
    "T2_BINDER_RUNNER": build_t2_binder,
    "T3_CHAIR_RUNNER_RC": build_t3_chair,
}


def true_peak(data: np.ndarray) -> float:
    oversampled = signal.resample_poly(data, 4, 1, axis=0)
    return float(np.max(np.abs(oversampled)))


def dbfs(value: float) -> float:
    return 20.0 * math.log10(max(value, 1e-12))


def finish_stems(stems: dict[str, np.ndarray], target_true_peak_dbfs: float) -> tuple[dict[str, np.ndarray], np.ndarray]:
    for name in STEMS:
        stems[name] = np.nan_to_num(stems[name], nan=0.0, posinf=0.0, neginf=0.0)
        stems[name] -= np.mean(stems[name], axis=0, keepdims=True)
    composite = sum((stems[name] for name in STEMS), start=blank())
    current = true_peak(composite)
    target = 10 ** (target_true_peak_dbfs / 20.0)
    scale = target / max(current, 1e-9)
    stems = {name: data * scale for name, data in stems.items()}
    composite = sum((stems[name] for name in STEMS), start=blank())
    return stems, composite


def write_wav(path: Path, data: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sf.write(path, np.clip(data, -0.999999, 0.999999), SAMPLE_RATE, subtype="PCM_24", format="WAV")


def metrics(data: np.ndarray) -> dict[str, float]:
    meter = pyln.Meter(SAMPLE_RATE)
    lufs = float(meter.integrated_loudness(data))
    contact_first = int(round(CONTACT_SECONDS * SAMPLE_RATE))
    contact_last = int(round((CONTACT_SECONDS + 0.350) * SAMPLE_RATE))
    contact = data[contact_first:contact_last]
    local = np.max(np.abs(data[contact_first:contact_first + int(0.012 * SAMPLE_RATE)]), axis=1)
    threshold = max(1e-9, float(np.max(local)) * 0.10)
    above = np.flatnonzero(local >= threshold)
    onset = int(above[0]) if len(above) else 999999
    return {
        "integrated_lufs": lufs,
        "sample_peak_dbfs": dbfs(float(np.max(np.abs(data)))),
        "true_peak_dbfs": dbfs(true_peak(data)),
        "rms_dbfs": dbfs(float(np.sqrt(np.mean(data * data)))),
        "contact_0_350ms_rms_dbfs": dbfs(float(np.sqrt(np.mean(contact * contact)))),
        "dc_offset_max": float(np.max(np.abs(np.mean(data, axis=0)))),
        "transient_onset_offset_ms": onset * 1000.0 / SAMPLE_RATE,
    }


def clear_outputs() -> None:
    for path in (COMPOSITES, STEM_ROOT, DERIVED, EVIDENCE, PRIVATE):
        if path.exists():
            shutil.rmtree(path)
        path.mkdir(parents=True, exist_ok=True)


def concatenate_clips(clips: list[np.ndarray], gap_seconds: float, crop: tuple[float, float] | None = None) -> np.ndarray:
    gap = np.zeros((int(round(gap_seconds * SAMPLE_RATE)), 2), dtype=np.float64)
    parts: list[np.ndarray] = []
    for index, clip in enumerate(clips):
        if crop is not None:
            first = max(0, int(round(crop[0] * SAMPLE_RATE)))
            last = min(len(clip), int(round(crop[1] * SAMPLE_RATE)))
            clip = clip[first:last]
        parts.append(clip)
        if index + 1 < len(clips):
            parts.append(gap)
    return np.concatenate(parts, axis=0)


def write_derived(composite_audio: dict[str, np.ndarray]) -> dict[str, object]:
    ordered = [composite_audio[spec.cue_id] for spec in CUES]
    write_wav(DERIVED / "IMPACT_LADDER_ACTUAL_DYNAMIC_REVIEW.wav", concatenate_clips(ordered, 0.45))

    t1_ids = [spec.cue_id for spec in CUES if spec.tier == 1]
    stress_ids = [t1_ids[index % len(t1_ids)] for index in range(12)]
    stress = [composite_audio[cue_id] for cue_id in stress_ids]
    write_wav(DERIVED / "T1_HAND_12_REPEAT_STRESS_REVIEW.wav", concatenate_clips(stress, 0.075, (0.145, 0.570)))

    rng = random.Random(BUILD_SEED)
    by_tier = {
        1: [spec.cue_id for spec in CUES if spec.tier == 1],
        2: [spec.cue_id for spec in CUES if spec.tier == 2],
        3: [spec.cue_id for spec in CUES if spec.tier == 3],
    }
    tier_schedule = [1, 2, 3, 2, 1, 3, 2, 1, 3, 2]
    rng.shuffle(tier_schedule)
    tier_trials = []
    tier_audio = []
    tier_counts = {1: 0, 2: 0, 3: 0}
    for trial, tier in enumerate(tier_schedule, start=1):
        cue_id = by_tier[tier][tier_counts[tier] % len(by_tier[tier])]
        tier_counts[tier] += 1
        tier_trials.append({"trial": trial, "cue_id": cue_id, "answer_tier": f"T{tier}"})
        tier_audio.append(composite_audio[cue_id])
    write_wav(DERIVED / "TIER_10_TRIAL_BLIND_REVIEW.wav", concatenate_clips(tier_audio, 0.38, (0.120, 0.780)))

    material_ids = {
        "HAND": [spec.cue_id for spec in CUES if "HAND" in spec.family or "HOOK" in spec.family],
        "BINDER": [spec.cue_id for spec in CUES if "BINDER" in spec.family],
        "CHAIR": [spec.cue_id for spec in CUES if "CHAIR" in spec.family],
    }
    material_schedule = ["HAND", "BINDER", "CHAIR", "HAND", "BINDER", "CHAIR", "HAND", "BINDER", "CHAIR", "HAND"]
    rng.shuffle(material_schedule)
    material_trials = []
    material_audio = []
    material_counts = {key: 0 for key in material_ids}
    for trial, material in enumerate(material_schedule, start=1):
        choices = material_ids[material]
        cue_id = choices[material_counts[material] % len(choices)]
        material_counts[material] += 1
        material_trials.append({"trial": trial, "cue_id": cue_id, "answer_material": material})
        material_audio.append(composite_audio[cue_id])
    write_wav(DERIVED / "MATERIAL_10_TRIAL_BLIND_REVIEW.wav", concatenate_clips(material_audio, 0.38, (0.120, 0.780)))

    mapping = {
        "status": "PRIVATE_ANSWER_KEY",
        "tier_trials": tier_trials,
        "material_trials": material_trials,
        "t1_stress_schedule": stress_ids,
    }
    (PRIVATE / "LADDER_QUIZ_MAPPING_PRIVATE.json").write_text(json.dumps(mapping, indent=2), encoding="utf-8")
    return mapping


def plot_waveforms(composite_audio: dict[str, np.ndarray]) -> None:
    figure, axes = plt.subplots(len(CUES), 1, figsize=(14, 16), sharex=True)
    times = np.arange(RENDER_FRAMES) / SAMPLE_RATE
    for axis, spec in zip(axes, CUES):
        mono = np.mean(composite_audio[spec.cue_id], axis=1)
        axis.plot(times, mono, color={1: "#5bb7ff", 2: "#f3b33d", 3: "#ff5c52"}[spec.tier], linewidth=0.55)
        axis.axvline(CONTACT_SECONDS, color="#ffffff", linewidth=0.6, alpha=0.7)
        axis.set_ylim(-1.0, 1.0)
        axis.set_ylabel(spec.cue_id.replace("_RUNNER", ""), rotation=0, ha="right", va="center", fontsize=7)
        axis.grid(alpha=0.13)
    axes[-1].set_xlabel("seconds; white line = contact 0.200 s")
    figure.suptitle("BREAK NOTICE Arcade Edge v004 — actual-dynamic impact ladder", fontsize=14)
    figure.tight_layout(rect=(0.11, 0.02, 1, 0.98))
    figure.savefig(EVIDENCE / "IMPACT_LADDER_WAVEFORMS_REVIEW.png", dpi=150)
    plt.close(figure)


def validate_outputs(composite_audio: dict[str, np.ndarray], qa_rows: list[dict[str, object]]) -> dict[str, object]:
    failures: list[str] = []
    if len(composite_audio) != 11:
        failures.append("composite count is not 11")
    stem_files = sorted(STEM_ROOT.rglob("*.wav"))
    if len(stem_files) != 55:
        failures.append("stem count is not 55")

    all_wavs = sorted(COMPOSITES.glob("*.wav")) + stem_files
    hashes = [sha256(path) for path in all_wavs]
    if len(set(hashes)) != len(hashes):
        failures.append("production WAV bytes are not all distinct")
    for path in all_wavs:
        info = sf.info(path)
        if info.samplerate != SAMPLE_RATE or info.channels != 2 or info.frames != RENDER_FRAMES or info.subtype != "PCM_24":
            failures.append(f"format contract failed: {path}")

    by_tier: dict[int, list[dict[str, object]]] = {1: [], 2: [], 3: []}
    for row in qa_rows:
        by_tier[int(row["tier"])].append(row)
        m = row["metrics"]
        if float(m["dc_offset_max"]) >= 0.001:
            failures.append(f"DC offset: {row['cue_id']}")
        if float(m["transient_onset_offset_ms"]) > 10.0:
            failures.append(f"late transient: {row['cue_id']}")
        if float(m["true_peak_dbfs"]) > -1.0:
            failures.append(f"headroom: {row['cue_id']}")

    t1_max_peak = max(float(row["metrics"]["true_peak_dbfs"]) for row in by_tier[1])
    t2_min_peak = min(float(row["metrics"]["true_peak_dbfs"]) for row in by_tier[2])
    t2_max_peak = max(float(row["metrics"]["true_peak_dbfs"]) for row in by_tier[2])
    t3_min_peak = min(float(row["metrics"]["true_peak_dbfs"]) for row in by_tier[3])
    if not (t3_min_peak > t2_max_peak > t1_max_peak):
        failures.append("true-peak tier ordering failed")

    t1_max_energy = max(float(row["metrics"]["contact_0_350ms_rms_dbfs"]) for row in by_tier[1])
    t2_min_energy = min(float(row["metrics"]["contact_0_350ms_rms_dbfs"]) for row in by_tier[2])
    t2_max_energy = max(float(row["metrics"]["contact_0_350ms_rms_dbfs"]) for row in by_tier[2])
    t3_min_energy = min(float(row["metrics"]["contact_0_350ms_rms_dbfs"]) for row in by_tier[3])
    if t2_min_energy - t1_max_energy < 1.5:
        failures.append("T1->T2 contact energy separation below 1.5 dB")
    if t3_min_energy - t2_max_energy < 1.5:
        failures.append("T2->T3 contact energy separation below 1.5 dB")

    family_correlations: dict[str, list[float]] = {}
    for family in sorted({spec.family for spec in CUES}):
        ids = [spec.cue_id for spec in CUES if spec.family == family]
        correlations: list[float] = []
        for first_index in range(len(ids)):
            for second_index in range(first_index + 1, len(ids)):
                first = composite_audio[ids[first_index]].reshape(-1)
                second = composite_audio[ids[second_index]].reshape(-1)
                correlation = float(np.corrcoef(first, second)[0, 1])
                correlations.append(correlation)
                if abs(correlation) >= 0.995:
                    failures.append(f"variation correlation too high: {ids[first_index]} / {ids[second_index]}")
        family_correlations[family] = correlations

    return {
        "automatic_status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "tier_bounds": {
            "t1_max_true_peak_dbfs": t1_max_peak,
            "t2_min_true_peak_dbfs": t2_min_peak,
            "t2_max_true_peak_dbfs": t2_max_peak,
            "t3_min_true_peak_dbfs": t3_min_peak,
            "t1_max_contact_rms_dbfs": t1_max_energy,
            "t2_min_contact_rms_dbfs": t2_min_energy,
            "t2_max_contact_rms_dbfs": t2_max_energy,
            "t3_min_contact_rms_dbfs": t3_min_energy,
        },
        "family_correlations": family_correlations,
        "human_checks_pending": [
            "laptop tier recognition at least 8/10",
            "laptop material recognition at least 8/10",
            "no click-machinegun fatigue in T1 12-repeat stress reel",
            "T3 lossless RC not worse than approved Z reference",
        ],
    }


def main() -> None:
    validate_sources()
    clear_outputs()
    composite_audio: dict[str, np.ndarray] = {}
    qa_rows: list[dict[str, object]] = []
    manifest_rows: list[dict[str, object]] = []

    for spec in CUES:
        stems, lineage = BUILDERS[spec.family](spec)
        stems, composite = finish_stems(stems, spec.target_true_peak_dbfs)
        composite_path = COMPOSITES / f"{spec.cue_id}_REVIEW.wav"
        write_wav(composite_path, composite)
        for stem_name in STEMS:
            write_wav(STEM_ROOT / spec.cue_id / f"{spec.cue_id}_{stem_name}_REVIEW.wav", stems[stem_name])
        composite_audio[spec.cue_id] = composite
        row = {
            "cue_id": spec.cue_id,
            "family": spec.family,
            "tier": spec.tier,
            "variation": spec.variation,
            "branded_click": spec.branded_click,
            "source_lineage": lineage,
            "composite_sha256": sha256(composite_path),
            "metrics": metrics(composite),
        }
        qa_rows.append(row)
        manifest_rows.append({
            **{key: row[key] for key in ("cue_id", "family", "tier", "variation", "branded_click", "source_lineage", "composite_sha256")},
            "stems": [
                {
                    "name": stem_name,
                    "path": f"Stems/{spec.cue_id}/{spec.cue_id}_{stem_name}_REVIEW.wav",
                    "sha256": sha256(STEM_ROOT / spec.cue_id / f"{spec.cue_id}_{stem_name}_REVIEW.wav"),
                }
                for stem_name in STEMS
            ],
        })

    quiz_mapping = write_derived(composite_audio)
    plot_waveforms(composite_audio)
    validation = validate_outputs(composite_audio, qa_rows)

    source_ledger = {
        "status": "VERIFIED_SOURCE_INPUT",
        "license_scope": "CC0/public-domain-equivalent pages; see SOURCE_PROVENANCE.md for limits",
        "lossless_root": "Sources/Lossless",
        "sources": SOURCE_INFO,
    }
    (EVIDENCE / "SOURCE_LEDGER_REVIEW.json").write_text(json.dumps(source_ledger, indent=2), encoding="utf-8")
    manifest = {
        "status": "REVIEW_CANDIDATE_NOT_FINAL_NOT_RUNTIME",
        "direction": "Z / Arcade Edge",
        "sample_rate": SAMPLE_RATE,
        "subtype": "PCM_24",
        "channels": 2,
        "frames": RENDER_FRAMES,
        "contact_seconds": CONTACT_SECONDS,
        "stems": list(STEMS),
        "cues": manifest_rows,
        "derived": [path.name for path in sorted(DERIVED.glob("*.wav"))],
        "private_answer_key": "Private/LADDER_QUIZ_MAPPING_PRIVATE.json",
    }
    (EVIDENCE / "CUE_MANIFEST_REVIEW.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    qa = {
        "status": "REVIEW_CANDIDATE",
        "automatic_validation": validation,
        "source_validation": {
            "count": len(SOURCE_INFO),
            "lossless_only": True,
            "exact_hash_checked_before_decode": True,
            "path_containment_checked": True,
        },
        "cue_metrics": qa_rows,
        "quiz_trial_counts": {
            "tier": len(quiz_mapping["tier_trials"]),
            "material": len(quiz_mapping["material_trials"]),
            "t1_stress": len(quiz_mapping["t1_stress_schedule"]),
        },
    }
    (EVIDENCE / "TECHNICAL_QA_REVIEW.json").write_text(json.dumps(qa, indent=2), encoding="utf-8")
    if validation["automatic_status"] != "PASS":
        raise RuntimeError("Automatic QA failed: " + "; ".join(validation["failures"]))
    print(json.dumps({
        "status": "PASS_AUTOMATIC_REVIEW_CANDIDATE",
        "composites": len(composite_audio),
        "stems": len(list(STEM_ROOT.rglob("*.wav"))),
        "sources": len(SOURCE_INFO),
        "evidence": str(EVIDENCE),
    }, indent=2))


if __name__ == "__main__":
    main()
