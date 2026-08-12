from __future__ import annotations

import csv
import hashlib
import json
import math
import re
import struct
import wave
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

import numpy as np
from PIL import Image, ImageDraw, ImageFont


SR = 48_000
BITS = 24
VERSION = "v002"


@dataclass
class Audio:
    samples: np.ndarray
    source_rate: int = SR
    source_bits: int = BITS

    @property
    def channels(self) -> int:
        return 1 if self.samples.ndim == 1 else self.samples.shape[1]

    @property
    def duration(self) -> float:
        return len(self.samples) / SR


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for block in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def read_wav(path: Path) -> Audio:
    with wave.open(str(path), "rb") as wav:
        channels = wav.getnchannels()
        rate = wav.getframerate()
        width = wav.getsampwidth()
        frames = wav.getnframes()
        raw = wav.readframes(frames)
    if width == 1:
        values = (np.frombuffer(raw, dtype=np.uint8).astype(np.float64) - 128.0) / 128.0
    elif width == 2:
        values = np.frombuffer(raw, dtype="<i2").astype(np.float64) / 32768.0
    elif width == 3:
        packed = np.frombuffer(raw, dtype=np.uint8).reshape(-1, 3)
        values_i = (
            packed[:, 0].astype(np.int32)
            | (packed[:, 1].astype(np.int32) << 8)
            | (packed[:, 2].astype(np.int32) << 16)
        )
        values_i = np.where(values_i & 0x800000, values_i - 0x1000000, values_i)
        values = values_i.astype(np.float64) / 8_388_608.0
    elif width == 4:
        values = np.frombuffer(raw, dtype="<i4").astype(np.float64) / 2_147_483_648.0
    else:
        raise ValueError(f"unsupported sample width {width}: {path}")
    if channels > 1:
        values = values.reshape(-1, channels)
    if rate != SR:
        old_x = np.linspace(0.0, 1.0, len(values), endpoint=False)
        new_len = int(round(len(values) * SR / rate))
        new_x = np.linspace(0.0, 1.0, new_len, endpoint=False)
        if values.ndim == 1:
            values = np.interp(new_x, old_x, values)
        else:
            values = np.stack([np.interp(new_x, old_x, values[:, c]) for c in range(channels)], axis=1)
    return Audio(values.astype(np.float64), rate, width * 8)


def write_wav24(path: Path, samples: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    data = np.asarray(samples, dtype=np.float64)
    if data.ndim == 1:
        channels = 1
        flattened = data
    else:
        channels = data.shape[1]
        flattened = data.reshape(-1)
    peak = float(np.max(np.abs(flattened))) if len(flattened) else 0.0
    if peak >= 1.0:
        raise ValueError(f"full-scale/clipped source rejected for {path}: {peak}")
    # Deterministic TPDF dither at one 24-bit LSB; the seed is tied to the destination.
    seed = int(hashlib.sha256(path.as_posix().encode("utf8")).hexdigest()[:16], 16)
    rng = np.random.default_rng(seed)
    dither = (rng.random(len(flattened)) - rng.random(len(flattened))) / 8_388_608.0
    quantized = np.rint(np.clip(flattened + dither, -0.999999, 0.999999) * 8_388_607.0).astype(np.int32)
    unsigned = quantized.astype(np.uint32) & 0xFFFFFF
    packed = np.empty((len(unsigned), 3), dtype=np.uint8)
    packed[:, 0] = unsigned & 0xFF
    packed[:, 1] = (unsigned >> 8) & 0xFF
    packed[:, 2] = (unsigned >> 16) & 0xFF
    with wave.open(str(path), "wb") as wav:
        wav.setnchannels(channels)
        wav.setsampwidth(3)
        wav.setframerate(SR)
        wav.writeframes(packed.tobytes())


def mono(samples: np.ndarray) -> np.ndarray:
    if samples.ndim == 1:
        return samples.copy()
    if samples.shape[1] == 1:
        return samples[:, 0].copy()
    return np.mean(samples, axis=1)


def stereo(samples: np.ndarray, width: float = 0.10, seed: int = 0) -> np.ndarray:
    if samples.ndim == 2 and samples.shape[1] == 2:
        return samples.copy()
    x = mono(samples)
    delay = 5 + seed % 17
    right = np.roll(x, delay)
    right[:delay] = 0.0
    mid = x
    side = (right - x) * width
    return np.stack([mid - side, mid + side], axis=1)


def slice_seconds(samples: np.ndarray, start: float, duration: float) -> np.ndarray:
    i0 = max(0, int(round(start * SR)))
    i1 = min(len(samples), i0 + int(round(duration * SR)))
    return samples[i0:i1].copy()


def pad_to(samples: np.ndarray, duration: float) -> np.ndarray:
    length = int(round(duration * SR))
    if len(samples) >= length:
        return samples[:length].copy()
    shape = (length,) if samples.ndim == 1 else (length, samples.shape[1])
    result = np.zeros(shape, dtype=np.float64)
    result[: len(samples)] = samples
    return result


def time_scale(samples: np.ndarray, factor: float) -> np.ndarray:
    if factor <= 0:
        raise ValueError("factor must be positive")
    old = np.linspace(0.0, 1.0, len(samples), endpoint=False)
    new_len = max(8, int(round(len(samples) / factor)))
    new = np.linspace(0.0, 1.0, new_len, endpoint=False)
    if samples.ndim == 1:
        return np.interp(new, old, samples)
    return np.stack([np.interp(new, old, samples[:, c]) for c in range(samples.shape[1])], axis=1)


def remove_dc(samples: np.ndarray) -> np.ndarray:
    return samples - np.mean(samples, axis=0, keepdims=samples.ndim > 1)


def fade(samples: np.ndarray, fade_in_ms: float = 3.0, fade_out_ms: float = 14.0) -> np.ndarray:
    result = samples.copy()
    n_in = min(len(result) // 2, int(round(fade_in_ms * SR / 1000.0)))
    n_out = min(len(result) // 2, int(round(fade_out_ms * SR / 1000.0)))
    if n_in:
        ramp = np.sin(np.linspace(0, math.pi / 2, n_in, endpoint=True)) ** 2
        if result.ndim == 2:
            ramp = ramp[:, None]
        result[:n_in] *= ramp
    if n_out:
        ramp = np.cos(np.linspace(0, math.pi / 2, n_out, endpoint=True)) ** 2
        if result.ndim == 2:
            ramp = ramp[:, None]
        result[-n_out:] *= ramp
    return result


def trim_active(samples: np.ndarray, threshold_db: float = -52.0, pre_ms: float = 10.0, post_ms: float = 85.0) -> np.ndarray:
    x = mono(samples)
    threshold = 10.0 ** (threshold_db / 20.0)
    active = np.flatnonzero(np.abs(x) >= threshold)
    if not len(active):
        return samples.copy()
    pre = int(round(pre_ms * SR / 1000.0))
    post = int(round(post_ms * SR / 1000.0))
    i0 = max(0, int(active[0]) - pre)
    i1 = min(len(samples), int(active[-1]) + post + 1)
    return samples[i0:i1].copy()


def normalize_peak(samples: np.ndarray, target_db: float) -> np.ndarray:
    peak = float(np.max(np.abs(samples))) if len(samples) else 0.0
    if peak <= 1e-12:
        return samples.copy()
    return samples * ((10.0 ** (target_db / 20.0)) / peak)


def fft_filter(samples: np.ndarray, low_hz: float = 0.0, high_hz: float = SR / 2, slope_hz: float = 150.0) -> np.ndarray:
    def one(channel: np.ndarray) -> np.ndarray:
        spectrum = np.fft.rfft(channel)
        freq = np.fft.rfftfreq(len(channel), 1.0 / SR)
        mask = np.ones_like(freq)
        if low_hz > 0:
            mask *= np.clip((freq - low_hz) / max(slope_hz, 1.0), 0.0, 1.0)
        if high_hz < SR / 2:
            mask *= np.clip((high_hz - freq) / max(slope_hz, 1.0), 0.0, 1.0)
        return np.fft.irfft(spectrum * mask, n=len(channel))
    if samples.ndim == 1:
        return one(samples)
    return np.stack([one(samples[:, c]) for c in range(samples.shape[1])], axis=1)


def damped_tone(duration: float, frequency: float, decay: float, seed: int, harmonics: tuple[float, ...] = (1.0, 2.04, 3.12)) -> np.ndarray:
    n = int(round(duration * SR))
    t = np.arange(n) / SR
    rng = np.random.default_rng(seed)
    result = np.zeros(n, dtype=np.float64)
    for index, harmonic in enumerate(harmonics):
        phase = rng.uniform(-math.pi, math.pi)
        result += (1.0 / (1.0 + index * 1.5)) * np.sin(2 * math.pi * frequency * harmonic * t + phase)
    result *= np.exp(-t / decay)
    return result


def periodic_noise(duration: float, low_hz: float, high_hz: float, seed: int, channels: int = 1) -> np.ndarray:
    n = int(round(duration * SR))
    result = []
    freq = np.fft.rfftfreq(n, 1.0 / SR)
    envelope = np.clip((freq - low_hz) / 180.0, 0.0, 1.0) * np.clip((high_hz - freq) / 350.0, 0.0, 1.0)
    for channel in range(channels):
        rng = np.random.default_rng(seed + channel * 1009)
        phase = rng.uniform(0.0, 2.0 * math.pi, len(freq))
        magnitude = rng.rayleigh(1.0, len(freq)) * envelope
        spectrum = magnitude * np.exp(1j * phase)
        spectrum[0] = 0.0
        result.append(np.fft.irfft(spectrum, n=n))
    stacked = result[0] if channels == 1 else np.stack(result, axis=1)
    peak = np.max(np.abs(stacked))
    return stacked / max(peak, 1e-9)


def mix_layers(layers: Iterable[tuple[np.ndarray, float, float]], channels: int = 1) -> np.ndarray:
    prepared = []
    max_len = 1
    for source, gain, offset_seconds in layers:
        data = source.copy()
        if channels == 1:
            data = mono(data)
        elif data.ndim == 1:
            data = stereo(data, seed=int(offset_seconds * 10_000))
        elif data.shape[1] != 2:
            data = np.stack([mono(data), mono(data)], axis=1)
        offset = max(0, int(round(offset_seconds * SR)))
        max_len = max(max_len, offset + len(data))
        prepared.append((data, gain, offset))
    shape = (max_len,) if channels == 1 else (max_len, channels)
    result = np.zeros(shape, dtype=np.float64)
    for data, gain, offset in prepared:
        result[offset : offset + len(data)] += data * gain
    return result


def finalize_one_shot(samples: np.ndarray, target_db: float, stereo_width: float | None = None, seed: int = 0) -> np.ndarray:
    result = trim_active(samples)
    result = remove_dc(result)
    if stereo_width is not None:
        result = stereo(result, stereo_width, seed)
    result = fade(result, 3.0, 24.0)
    return normalize_peak(result, target_db)


def limit_one_shot(samples: np.ndarray, max_duration: float, fade_out_ms: float = 35.0) -> np.ndarray:
    maximum = int(round(max_duration * SR))
    if len(samples) <= maximum:
        return samples
    result = samples[:maximum].copy()
    return fade(result, 0.0, fade_out_ms)


def loop_from_segment(samples: np.ndarray, duration: float, start: float, crossfade: float = 0.12) -> np.ndarray:
    target = int(round(duration * SR))
    xfade = int(round(crossfade * SR))
    source = mono(samples)
    begin = int(round(start * SR))
    required = target + xfade
    if begin + required > len(source):
        begin = max(0, len(source) - required)
    segment = source[begin : begin + required].copy()
    if len(segment) < required:
        repeats = int(math.ceil(required / max(len(segment), 1)))
        segment = np.tile(segment, repeats)[:required]
    result = segment[:target].copy()
    blend = np.linspace(0.0, 1.0, xfade, endpoint=False)
    result[:xfade] = segment[target : target + xfade] * (1.0 - blend) + segment[:xfade] * blend
    result = remove_dc(result)
    return result


def finalize_loop(samples: np.ndarray, target_db: float) -> np.ndarray:
    result = remove_dc(samples)
    # Rotate an already continuous loop so its wrap falls at the most stable
    # local point. This preserves every sample and the perceived motion while
    # minimizing value/slope and short-window energy changes at the seam.
    if len(result) > int(0.08 * SR):
        probe = mono(result)
        window = int(0.010 * SR)
        ordinary_step = max(float(np.percentile(np.abs(np.diff(probe)), 99.0)), 1e-9)
        tiled = np.tile(probe, 3)
        best_index = 0
        best_cost = float("inf")
        for index in range(0, len(probe), 24):
            center = len(probe) + index
            before = tiled[center - window : center]
            after = tiled[center : center + window]
            before_rms = math.sqrt(float(np.mean(before * before)) + 1e-16)
            after_rms = math.sqrt(float(np.mean(after * after)) + 1e-16)
            level_delta = abs(20.0 * math.log10(before_rms / after_rms))
            value_jump = abs(float(tiled[center] - tiled[center - 1])) / ordinary_step
            slope_jump = abs(float((tiled[center + 1] - tiled[center]) - (tiled[center - 1] - tiled[center - 2]))) / ordinary_step
            cost = level_delta + 0.20 * value_jump + 0.10 * slope_jump
            if cost < best_cost:
                best_cost = cost
                best_index = index
        if best_index:
            result = np.concatenate([result[best_index:], result[:best_index]], axis=0)
    return normalize_peak(result, target_db)


def detect_hits(samples: np.ndarray, desired: int, duration: float = 0.42, separation: float = 0.24) -> list[np.ndarray]:
    x = mono(samples)
    window = max(8, int(0.006 * SR))
    kernel = np.ones(window) / window
    envelope = np.convolve(np.abs(x), kernel, mode="same")
    edge = int(0.08 * SR)
    usable_start = edge
    usable_end = len(x) - edge
    boundaries = np.linspace(usable_start, usable_end, desired + 1, dtype=int)
    selected = []
    for region in range(desired):
        start = boundaries[region]
        end = boundaries[region + 1]
        if end <= start:
            continue
        selected.append(start + int(np.argmax(envelope[start:end])))
    selected.sort()
    result = []
    for index in selected:
        start = max(0, index - int(0.055 * SR))
        result.append(slice_seconds(x, start / SR, duration))
    if len(result) < desired or any(np.max(np.abs(item)) < 1e-5 for item in result):
        raise ValueError(f"only found {len(result)} hits, needed {desired}")
    return result


def metric(samples: np.ndarray) -> dict:
    x = np.asarray(samples, dtype=np.float64)
    peak = float(np.max(np.abs(x))) if len(x) else 0.0
    rms = float(np.sqrt(np.mean(x * x))) if len(x) else 0.0
    dc = np.mean(x, axis=0) if len(x) else np.array([0.0])
    dc_max = float(np.max(np.abs(dc)))
    peak_db = 20.0 * math.log10(max(peak, 1e-12))
    rms_db = 20.0 * math.log10(max(rms, 1e-12))
    return {"peak": peak, "peak_dbfs": peak_db, "rms_dbfs": rms_db, "dc_max": dc_max}


COUNT_BY_CUE = {
    "AUD-AMB-001": 1, "AUD-AMB-002": 2, "AUD-AMB-003": 2, "AUD-AMB-004": 2,
    "AUD-PLY-001": 6, "AUD-PLY-002": 6, "AUD-PLY-003": 3, "AUD-PLY-004": 3,
    "AUD-PLY-005": 3, "AUD-PLY-006": 3, "AUD-PLY-007": 1, "AUD-PLY-008": 2,
    "AUD-PLY-009": 1, "AUD-PLY-010": 4, "AUD-PLY-011": 4, "AUD-PLY-012": 2,
    "AUD-BND-001": 2, "AUD-BND-002": 5, "AUD-BND-003": 4, "AUD-BND-004": 4,
    "AUD-BND-005": 3, "AUD-BND-006": 3,
    "AUD-CHR-001": 2, "AUD-CHR-002": 2, "AUD-CHR-003": 4, "AUD-CHR-004": 5,
    "AUD-CHR-005": 4, "AUD-CHR-006": 3,
    "AUD-ALY-001": 2, "AUD-ALY-002": 2, "AUD-ALY-003": 3, "AUD-ALY-004": 2,
    "AUD-ALY-005": 2, "AUD-ALY-006": 2, "AUD-ALY-007": 2,
    "AUD-STR-001": 3, "AUD-STR-002": 3, "AUD-STR-003": 2, "AUD-STR-004": 2,
    "AUD-STR-005": 3, "AUD-STR-006": 2,
    "AUD-TAG-001": 2, "AUD-TAG-002": 2, "AUD-TAG-003": 2, "AUD-TAG-004": 1,
    "AUD-TAG-005": 1,
    "AUD-RUN-001": 6, "AUD-RUN-002": 3, "AUD-RUN-003": 3, "AUD-RUN-004": 4,
    "AUD-RUN-005": 4, "AUD-RUN-006": 3, "AUD-RUN-007": 1, "AUD-RUN-008": 3,
    "AUD-RUN-009": 4, "AUD-RUN-010": 4,
    "AUD-DRW-001": 4, "AUD-DRW-002": 2, "AUD-DRW-003": 1, "AUD-DRW-004": 3,
    "AUD-FIN-001": 1, "AUD-FIN-002": 2, "AUD-FIN-003": 3, "AUD-FIN-004": 3,
    "AUD-FIN-005": 3, "AUD-FIN-006": 1,
    "AUD-RST-001": 1, "AUD-RST-002": 1,
    "AUD-EXT-001": 1, "AUD-EXT-002": 2, "AUD-EXT-003": 1, "AUD-EXT-004": 1,
    "AUD-UI-001": 3, "AUD-UI-002": 3, "AUD-UI-003": 1,
}


FOLDER_BY_FAMILY = {
    "AMB": "Ambience", "PLY": "Player", "BND": "Binder", "CHR": "Chair",
    "ALY": "Ally", "STR": "Strap", "TAG": "Tags", "RUN": "Runner",
    "DRW": "Drawer", "FIN": "Finisher", "RST": "Reset", "EXT": "Exit", "UI": "UI",
}


SPECIAL_NAMES = {
    "AUD-AMB-003": [
        "AUD_AMB_LockdownAlarm_Distant_PreClear_Loop_v002.wav",
        "AUD_AMB_LockdownAlarm_Distant_PostClearReduced_Loop_v002.wav",
    ],
    "AUD-PLY-008": [
        "AUD_PLY_ThrowCharge_Build_Light_Loop_v002.wav",
        "AUD_PLY_ThrowCharge_Build_Medium_Loop_v002.wav",
    ],
    "AUD-CHR-001": [
        "AUD_CHR_CasterRoll_Slow_Loop_v002.wav",
        "AUD_CHR_CasterRoll_Fast_Loop_v002.wav",
    ],
    "AUD-STR-003": [
        "AUD_STR_Tension_Light_Loop_v002.wav",
        "AUD_STR_Tension_Loaded_Loop_v002.wav",
    ],
    "AUD-STR-004": [
        "AUD_STR_Redirect_Load_Left_Loop_v002.wav",
        "AUD_STR_Redirect_Load_Right_Loop_v002.wav",
    ],
    "AUD-DRW-002": [
        "AUD_DRW_MoveScrape_Slow_Loop_v002.wav",
        "AUD_DRW_MoveScrape_Fast_Loop_v002.wav",
    ],
}


def output_names(cue_id: str, expected: str, count: int) -> list[str]:
    if cue_id in SPECIAL_NAMES:
        return SPECIAL_NAMES[cue_id]
    stem = re.sub(r"_v001$", "", Path(expected).stem)
    if count == 1:
        return [f"{stem}_v002.wav"]
    if stem.endswith("_01"):
        base = stem[:-3]
        return [f"{base}_{i:02d}_v002.wav" for i in range(1, count + 1)]
    if stem.endswith("_Loop"):
        base = stem[:-5]
        return [f"{base}_{i:02d}_Loop_v002.wav" for i in range(1, count + 1)]
    return [f"{stem}_{i:02d}_v002.wav" for i in range(1, count + 1)]


class SourceBank:
    def __init__(self, showcase_root: Path):
        self.showcase_root = showcase_root
        self.raw_root = showcase_root / "Source" / "ProductionV002" / "RawCC0" / "LaSonotheque"
        self.ref_root = showcase_root / "Source" / "ApprovedReferences"
        self.master_root = showcase_root / "Source" / "ProductionV002" / "Masters"
        self.raw = {}
        for path in sorted(self.raw_root.glob("*.wav")):
            match = re.search(r"BSB_(\d{4})", path.name)
            if match:
                self.raw[match.group(1)] = read_wav(path)
        self.reference_paths = {
            "foot": self.ref_root / "AUDITION_01_FOOTSTEPS_WEIGHTED_v002_48k24.wav",
            "body": self.ref_root / "AUDITION_02_PUNCH_WEIGHTED_v002_48k24.wav",
            "binder": self.ref_root / "AUDITION_BINDER_PHYSICAL_v003_48k24.wav",
            "chair": self.ref_root / "AUDITION_CHAIR_PHYSICAL_v003_48k24.wav",
        }
        self.references = {key: read_wav(path) for key, path in self.reference_paths.items()}
        self.ref_segments = {
            "foot": self._segments("foot", 8, 0.50, 0.27, -7.0),
            "body": self._segments("body", 6, 0.70, 0.47, -4.8),
            "binder": self._segments("binder", 6, 0.78, 0.40, -6.0),
            "chair": self._segments("chair", 6, 1.10, 0.55, -5.0),
        }
        self.foot_hits = detect_hits(self.raw["1318"].samples, 24, 0.46, 0.25) + detect_hits(self.raw["3441"].samples, 14, 0.46, 0.25)
        self.chair_hits = detect_hits(self.raw["0666"].samples, 24, 0.72, 0.45)
        self.workbook_hits = detect_hits(self.raw["0786"].samples, 20, 0.66, 0.35)

    def _segments(self, key: str, count: int, length: float, gap: float, target_db: float) -> list[np.ndarray]:
        source = mono(self.references[key].samples)
        result = []
        for index in range(count):
            start = index * (length + gap)
            segment = slice_seconds(source, start, length)
            segment = trim_active(segment, -56.0, 8.0, 55.0)
            segment = fade(remove_dc(segment), 2.0, 18.0)
            segment = normalize_peak(segment, target_db)
            result.append(segment)
            write_wav24(self.master_root / f"REF_{key.upper()}_{index + 1:02d}_{'v003' if key in {'binder', 'chair'} else 'v002'}.wav", segment)
        return result

    def ref(self, key: str, index: int) -> np.ndarray:
        return self.ref_segments[key][index % len(self.ref_segments[key])].copy()

    def raw_mono(self, source_id: str) -> np.ndarray:
        return mono(self.raw[source_id].samples)

    def raw_hit(self, pool: str, index: int) -> np.ndarray:
        selected = {"foot": self.foot_hits, "chair": self.chair_hits, "workbook": self.workbook_hits}[pool]
        return selected[index % len(selected)].copy()


def physical_click(bank: SourceBank, index: int, heavy: bool = False) -> np.ndarray:
    metal = trim_active(bank.raw_mono("2244"), -58.0, 4.0, 45.0)
    metal = slice_seconds(metal, (index % 3) * 0.28, 0.38)
    tone = damped_tone(0.32 if heavy else 0.22, 115.0 if heavy else 260.0, 0.09, 900 + index)
    return mix_layers([(metal, 0.70, 0.0), (tone, 0.18 if heavy else 0.09, 0.012)], 1)


def binder_material(bank: SourceBank, index: int, hard: bool = False) -> tuple[np.ndarray, list[str]]:
    ref = bank.ref("binder", index)
    paper_id = "2803" if index % 2 == 0 else "2808"
    paper = trim_active(bank.raw_mono(paper_id), -58.0, 7.0, 50.0)
    paper = slice_seconds(paper, (index * 0.17) % max(0.01, len(paper) / SR - 0.45), 0.45)
    workbook = bank.raw_hit("workbook", index)
    layers = [(ref, 0.82, 0.0), (paper, 0.24, 0.025 + 0.004 * (index % 3)), (workbook, 0.18, 0.0)]
    sources = ["OWNER_REF_BINDER_V003", f"LASO-{paper_id}", "LASO-0786"]
    if hard:
        plastic = trim_active(bank.raw_mono("2902"), -58.0, 4.0, 45.0)
        layers.append((plastic, 0.20, 0.008))
        sources.append("LASO-2902")
    return mix_layers(layers, 1), sources


def chair_material(bank: SourceBank, index: int, heavy: bool = True) -> tuple[np.ndarray, list[str]]:
    ref = bank.ref("chair", index)
    chair = bank.raw_hit("chair", index * 2 + 1)
    metal = trim_active(bank.raw_mono("1773"), -58.0, 4.0, 80.0)
    layers = [(ref, 0.82, 0.0), (chair, 0.30, 0.012), (metal, 0.13 if heavy else 0.08, 0.018)]
    sources = ["OWNER_REF_CHAIR_V003", "LASO-0666", "LASO-1773"]
    if heavy:
        weight = trim_active(bank.raw_mono("3296"), -58.0, 4.0, 80.0)
        layers.append((weight, 0.26, 0.01))
        sources.append("LASO-3296")
    return mix_layers(layers, 1), sources


def body_material(bank: SourceBank, index: int, medium: bool = False) -> tuple[np.ndarray, list[str]]:
    ref = bank.ref("body", index)
    punch_id = str(2459 + (index % 6))
    fall_ids = ["2452", "2453", "2454", "2455"]
    fall_id = fall_ids[index % len(fall_ids)]
    punch = trim_active(bank.raw_mono(punch_id), -58.0, 3.0, 80.0)
    fall = trim_active(bank.raw_mono(fall_id), -58.0, 3.0, 100.0)
    layers = [(ref, 0.84, 0.0), (punch, 0.30, 0.006), (fall, 0.20 if medium else 0.12, 0.018)]
    return mix_layers(layers, 1), ["OWNER_REF_BODY_CONTACT_V002", f"LASO-{punch_id}", f"LASO-{fall_id}"]


def step_material(bank: SourceBank, index: int, fast: bool = False, runner: bool = False) -> tuple[np.ndarray, list[str]]:
    raw_index = index * 3 + (18 if fast else 0) + (36 if runner else 0)
    recorded = bank.raw_hit("foot", raw_index)
    reference = bank.ref("foot", index + (3 if fast else 0) + (5 if runner else 0))
    if fast:
        recorded = time_scale(recorded, 1.08 + 0.015 * (index % 3))
    body = damped_tone(0.22, 92.0 + (index % 4) * 7.0, 0.065, 1200 + raw_index, (1.0, 1.73))
    result = mix_layers([(recorded, 0.68, 0.0), (reference, 0.62, 0.003), (body, 0.08, 0.012)], 1)
    return result, ["OWNER_REF_FOOTSTEPS_V002", "LASO-1318", "LASO-3441"]


def make_recorded_loop(bank: SourceBank, source_id: str, index: int, duration: float, target_db: float, start_step: float = 2.1) -> np.ndarray:
    source = bank.raw_mono(source_id)
    available = max(0.2, len(source) / SR - duration - 0.2)
    start = (0.75 + index * start_step) % available
    return finalize_loop(loop_from_segment(source, duration, start, min(0.18, duration * 0.1)), target_db)


def build_cue(bank: SourceBank, cue_id: str, index: int) -> tuple[np.ndarray, bool, list[str], str, str]:
    seed = int(hashlib.sha256(f"{cue_id}:{index}".encode()).hexdigest()[:8], 16)
    family = cue_id.split("-")[1]
    number = int(cue_id.split("-")[2])
    loop = False
    transform = "trim; DC removal; click-safe fades; deterministic layer mix; peak trim"

    if family == "AMB":
        if number == 1:
            bed = periodic_noise(8.0, 28.0, 1300.0, seed, 2)
            hum_t = np.arange(len(bed)) / SR
            hum = np.stack([
                np.sin(2 * math.pi * 60.0 * hum_t) + 0.35 * np.sin(2 * math.pi * 120.0 * hum_t),
                np.sin(2 * math.pi * 60.125 * hum_t + 0.15) + 0.32 * np.sin(2 * math.pi * 120.25 * hum_t),
            ], axis=1)
            audio = finalize_loop(bed * 0.78 + hum * 0.09, -16.0)
            return audio, True, ["PROJECT_ORIGINAL_DSP"], "periodic HVAC noise bed; restrained mains hum", "stereo"
        if number == 2:
            noise = periodic_noise(4.0, 850.0, 7800.0, seed, 1)
            t = np.arange(len(noise)) / SR
            hum = np.sin(2 * math.pi * (120.0 + index * 0.75) * t) + 0.22 * np.sin(2 * math.pi * 240.0 * t)
            audio = finalize_loop(noise * 0.24 + hum * 0.10, -18.0 + index)
            return audio, True, ["PROJECT_ORIGINAL_DSP"], "periodic fixture buzz and 120 Hz physical hum", "mono"
        if number == 3:
            metal = make_recorded_loop(bank, "2359", index, 6.0, -23.0, 0.7)
            t = np.arange(len(metal)) / SR
            gate = (np.sin(2 * math.pi * ((2.0 + index) / 6.0) * t) > 0.80).astype(float)
            alarm = np.sin(2 * math.pi * (420.0 + index * 18) * t) * gate * (0.028 if index == 0 else 0.012)
            noise = periodic_noise(6.0, 180.0, 1800.0, seed, 1) * 0.05
            audio = finalize_loop(metal * (0.30 if index == 0 else 0.16) + alarm + noise, -17.0 if index == 0 else -20.0)
            return audio, True, ["LASO-2359", "PROJECT_ORIGINAL_DSP"], "distant mechanical alarm; reduced post-clear variant", "mono"
        click = physical_click(bank, index, False)
        tail = damped_tone(1.0, 315.0 + index * 28, 0.23, seed, (1.0, 1.51, 2.22))
        audio = finalize_one_shot(mix_layers([(click, 0.28, 0.08), (tail, 0.07, 0.11)], 2), -16.0, None, seed)
        return audio, False, ["LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; distant relay perspective", "stereo"

    if family == "PLY":
        if number in (1, 2):
            raw, sources = step_material(bank, index, fast=number == 2)
            audio = finalize_one_shot(raw, -8.5 if number == 1 else -7.5, None, seed)
            return audio, False, sources, transform + ("; fast cadence trim" if number == 2 else "; weighted safety-shoe contact"), "mono"
        if number in (3, 5):
            raw, sources = binder_material(bank, index, hard=False)
            if number == 5:
                raw = fft_filter(raw, 80.0, 6200.0)
            audio = finalize_one_shot(raw, -7.8 if number == 3 else -9.0, None, seed)
            return audio, False, sources, transform + "; paper-board grip/settle", "mono"
        if number in (4, 6):
            raw, sources = chair_material(bank, index, heavy=number == 6)
            audio = finalize_one_shot(raw, -6.8 if number == 4 else -7.4, None, seed)
            return audio, False, sources, transform + "; frame/caster settle", "mono"
        if number == 7:
            raw = mix_layers([(physical_click(bank, index), 0.65, 0.0), (bank.raw_hit("workbook", 2), 0.13, 0.0)], 1)
            return finalize_one_shot(raw, -8.0), False, ["LASO-2244", "LASO-0786"], transform + "; neutral tactile start", "mono"
        if number == 8:
            if index == 0:
                recorded = loop_from_segment(bank.raw_mono("2803"), 1.20, 0.15, 0.10)
                dsp = periodic_noise(1.20, 180.0, 2600.0, seed, 1) * 0.12
                sources = ["LASO-2803", "PROJECT_ORIGINAL_DSP"]
                target = -14.0
            else:
                recorded = loop_from_segment(bank.raw_mono("0666"), 1.20, 5.8, 0.12)
                dsp = periodic_noise(1.20, 70.0, 1600.0, seed, 1) * 0.10
                sources = ["LASO-0666", "PROJECT_ORIGINAL_DSP"]
                target = -12.0
            return finalize_loop(recorded * 0.82 + dsp, target), True, sources, "seam-crossfaded recorded tension; non-tonal build", "mono"
        if number == 9:
            raw = mix_layers([(physical_click(bank, 3, True), 0.72, 0.0), (bank.raw_hit("workbook", 5), 0.12, 0.018)], 1)
            return finalize_one_shot(raw, -6.8), False, ["LASO-2244", "LASO-0786"], transform + "; single mechanical full-charge lock", "mono"
        if number == 10:
            binder, sources = binder_material(bank, index + 2, False)
            whoosh_id = "1798" if index % 2 == 0 else "1802"
            whoosh = trim_active(bank.raw_mono(whoosh_id), -58.0, 2.0, 55.0)
            raw = mix_layers([(whoosh, 0.30, 0.0), (binder, 0.92, 0.025)], 1)
            return finalize_one_shot(raw, -5.7), False, sources + [f"LASO-{whoosh_id}"], transform + "; hand release and restrained real-air movement", "mono"
        if number == 11:
            chair, sources = chair_material(bank, index + 2, True)
            whoosh_id = "1802" if index % 2 == 0 else "1798"
            whoosh = trim_active(bank.raw_mono(whoosh_id), -58.0, 2.0, 55.0)
            raw = mix_layers([(whoosh, 0.24, 0.0), (chair, 0.94, 0.022)], 1)
            return finalize_one_shot(raw, -4.8), False, sources + [f"LASO-{whoosh_id}"], transform + "; weighted chair release", "mono"
        raw = mix_layers([(bank.raw_hit("workbook", index + 8), 0.40, 0.0), (physical_click(bank, index), 0.22, 0.012)], 1)
        return finalize_one_shot(fft_filter(raw, 80, 4600), -10.0), False, ["LASO-0786", "LASO-2244"], transform + "; non-release decay/reject", "mono"

    if family == "BND":
        if number == 1:
            source_id = "2803" if index == 0 else "2808"
            recorded = loop_from_segment(bank.raw_mono(source_id), 1.45, 0.18 + index * 0.12, 0.15)
            air = periodic_noise(1.45, 600.0, 6800.0, seed, 1) * 0.06
            return finalize_loop(recorded * 0.76 + air, -13.0), True, [f"LASO-{source_id}", "PROJECT_ORIGINAL_DSP"], "seam-crossfaded paper edge flutter; restrained air layer", "mono"
        binder, sources = binder_material(bank, index + number * 2, hard=number == 4)
        if number == 2:
            return finalize_one_shot(binder, -6.5), False, sources, transform + "; dry office-surface impact", "mono"
        if number == 3:
            body, body_sources = body_material(bank, index + 1, False)
            raw = mix_layers([(binder, 0.80, 0.0), (body, 0.44, 0.018)], 1)
            return finalize_one_shot(raw, -4.9), False, sources + body_sources, transform + "; Binder-to-Runner nonlethal contact", "mono"
        if number == 4:
            steel = trim_active(bank.raw_mono("1773"), -58, 3, 80)
            raw = mix_layers([(binder, 0.82, 0.0), (steel, 0.23, 0.012)], 1)
            return finalize_one_shot(raw, -5.3), False, sources + ["LASO-1773"], transform + "; static-office hard contact", "mono"
        if number == 5:
            latch = bank.raw_hit("workbook", index + 10)
            raw = mix_layers([(binder, 0.62, 0.0), (latch, 0.32, 0.025), (physical_click(bank, index), 0.16, 0.032)], 1)
            return finalize_one_shot(raw, -6.5), False, sources + ["LASO-0786", "LASO-2244"], transform + "; secure catch closure", "mono"
        paper = trim_active(bank.raw_mono("1226"), -58, 4, 80)
        raw = mix_layers([(binder, 0.68, 0.0), (paper, 0.35, 0.035)], 1)
        return finalize_one_shot(raw, -7.0), False, sources + ["LASO-1226"], transform + "; miss and grounded landing", "mono"

    if family == "CHR":
        if number == 1:
            duration = 2.4 if index == 0 else 2.0
            start = 6.0 if index == 0 else 20.0
            recorded = loop_from_segment(bank.raw_mono("0666"), duration, start, 0.18)
            load = loop_from_segment(bank.raw_mono("1316"), duration, 2.0 + index * 5.0, 0.18)
            raw = recorded * 0.72 + load * (0.22 if index == 0 else 0.34)
            return finalize_loop(raw, -11.5 if index == 0 else -10.0), True, ["LASO-0666", "LASO-1316"], "recorded caster/tile loop; seam crossfade", "mono"
        if number == 2:
            recorded = loop_from_segment(bank.raw_mono("0666"), 1.8, 10.0 + index * 8.0, 0.16)
            load = loop_from_segment(bank.raw_mono("1316"), 1.8, 4.0 + index * 5.0, 0.16)
            return finalize_loop(recorded * 0.64 + load * 0.36, -10.5), True, ["LASO-0666", "LASO-1316"], "floor-supported ally push loop", "mono"
        if number == 3:
            skid = bank.raw_hit("chair", index * 3 + 3)
            skid = fft_filter(skid, 140.0, 6200.0)
            chair = bank.ref("chair", index)
            raw = mix_layers([(skid, 0.86, 0.0), (chair, 0.30, 0.02)], 1)
            return finalize_one_shot(raw, -6.0 + 0.2 * index), False, ["LASO-0666", "OWNER_REF_CHAIR_V003"], transform + "; caster skid intensity variation", "mono"
        chair, sources = chair_material(bank, index + number, heavy=True)
        if number == 4:
            return finalize_one_shot(chair, -4.7), False, sources, transform + "; metal/plastic static-office impact", "mono"
        if number == 5:
            body, body_sources = body_material(bank, index + 2, True)
            raw = mix_layers([(chair, 0.82, 0.0), (body, 0.48, 0.022)], 1)
            return finalize_one_shot(raw, -4.0), False, sources + body_sources, transform + "; Chair-to-Runner medium nonlethal contact", "mono"
        wheel = bank.raw_hit("chair", index * 4 + 5)
        raw = mix_layers([(chair, 0.50, 0.0), (wheel, 0.42, 0.028)], 1)
        return finalize_one_shot(raw, -7.0), False, sources + ["LASO-0666"], transform + "; hand release and wheel settle", "mono"

    if family == "ALY":
        if number == 1:
            step, step_sources = step_material(bank, index + 8, False, False)
            binder, binder_sources = binder_material(bank, index + 3, False)
            raw = mix_layers([(step, 0.28, 0.0), (binder, 0.42, 0.045), (physical_click(bank, index), 0.14, 0.09)], 1)
            return finalize_one_shot(raw, -8.0), False, step_sources + binder_sources + ["LASO-2244"], transform + "; workwear/object settle; no voice", "mono"
        if number == 2:
            raw = mix_layers([(bank.raw_hit("workbook", index + 14), 0.42, 0.0), (physical_click(bank, index), 0.34, 0.012)], 1)
            return finalize_one_shot(raw, -7.5), False, ["LASO-0786", "LASO-2244"], transform + "; grounded Q accept", "mono"
        if number == 3:
            binder, sources = binder_material(bank, index + 5, False)
            whoosh = trim_active(bank.raw_mono("1798" if index % 2 == 0 else "1802"), -58, 2, 55)
            raw = mix_layers([(binder, 0.72, 0.0), (whoosh, 0.22, 0.015)], 1)
            return finalize_one_shot(raw, -6.0), False, sources + ["LASO-1798" if index % 2 == 0 else "LASO-1802"], transform + "; hand release before airborne phase", "mono"
        if number == 4:
            chair, sources = chair_material(bank, index + 4, False)
            wheel = bank.raw_hit("chair", index + 12)
            raw = mix_layers([(chair, 0.52, 0.0), (wheel, 0.46, 0.025)], 1)
            return finalize_one_shot(raw, -7.0), False, sources + ["LASO-0666"], transform + "; grounded grip/push start", "mono"
        if number in (5, 6):
            reject = mix_layers([(bank.raw_hit("workbook", index + 4), 0.32, 0.0), (physical_click(bank, index), 0.28, 0.015)], 1)
            sources = ["LASO-0786", "LASO-2244"]
            if number == 6:
                heavy = trim_active(bank.raw_mono("3296"), -58, 3, 70)
                reject = mix_layers([(reject, 0.74, 0.0), (heavy, 0.24, 0.012)], 1)
                sources.append("LASO-3296")
            return finalize_one_shot(fft_filter(reject, 70, 4800), -9.0 if number == 5 else -7.5), False, sources, transform + ("; no-candidate nonlock" if number == 5 else "; blocked world-weight layer"), "mono"
        step, sources = step_material(bank, index + 12, False, False)
        latch = physical_click(bank, index + 2)
        raw = mix_layers([(step, 0.30, 0.0), (latch, 0.32, 0.03)], 1)
        return finalize_one_shot(raw, -8.0), False, sources + ["LASO-2244"], transform + "; restrained regroup confirmation", "mono"

    if family == "STR":
        if number in (3, 4):
            duration = 1.55
            base = loop_from_segment(bank.raw_mono("0666"), duration, 12.0 + index * 9.0 + number, 0.16)
            reel = periodic_noise(duration, 90.0 if number == 4 else 170.0, 2600.0, seed, 1)
            metal = loop_from_segment(bank.raw_mono("2359"), duration, 0.4 + index * 0.6, 0.14)
            raw = base * (0.46 if number == 3 else 0.56) + reel * (0.10 if index == 0 else 0.14) + metal * (0.14 if number == 3 else 0.22)
            return finalize_loop(raw, -12.0 if number == 3 and index == 0 else -10.0), True, ["LASO-0666", "LASO-2359", "PROJECT_ORIGINAL_DSP"], "recorded friction/tension loop; seam crossfade", "mono"
        if number == 1:
            metal_id = ("1773", "2244", "2359")[index % 3]
            metal = trim_active(bank.raw_mono(metal_id), -58, 3, 55)
            metal = slice_seconds(metal, (index * 0.17) % max(0.01, len(metal) / SR - 0.32), 0.32)
            air = trim_active(bank.raw_mono("1798" if index % 2 == 0 else "1802"), -58, 2, 55)
            raw = mix_layers([(air, 0.18 + 0.03 * index, 0.0), (metal, 0.48 - 0.04 * index, 0.020 + 0.010 * index), (physical_click(bank, index + 6), 0.22 + 0.03 * index, 0.065)], 1)
            return finalize_one_shot(raw, -6.5), False, [f"LASO-{metal_id}", "LASO-1798" if index % 2 == 0 else "LASO-1802", "LASO-2244"], transform + "; reel/carabiner deploy with distinct recorded metal body", "mono"
        if number == 2:
            gate = trim_active(bank.raw_mono("2359"), -58, 3, 70)
            raw = mix_layers([(gate, 0.35, 0.0), (physical_click(bank, index + 2, True), 0.70, 0.012)], 1)
            return finalize_one_shot(raw, -5.5), False, ["LASO-2359", "LASO-2244"], transform + "; positive carabiner/clamp lock", "mono"
        if number == 5:
            air = trim_active(bank.raw_mono("1802" if index % 2 == 0 else "1798"), -58, 2, 60)
            click = physical_click(bank, index + 5)
            noise = periodic_noise(0.65, 200.0, 4200.0, seed, 1) * np.exp(-np.arange(int(0.65 * SR)) / (0.22 * SR))
            raw = mix_layers([(air, 0.25, 0.0), (noise, 0.08, 0.0), (click, 0.42, 0.20 + index * 0.01)], 1)
            return finalize_one_shot(raw, -6.5), False, ["LASO-1798" if index % 2 else "LASO-1802", "LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; latch release and restrained retract", "mono"
        raw = mix_layers([(physical_click(bank, index), 0.45, 0.0), (trim_active(bank.raw_mono("1773"), -58, 2, 45), 0.16, 0.025)], 1)
        return finalize_one_shot(fft_filter(raw, 120, 5200), -9.0), False, ["LASO-2244", "LASO-1773"], transform + "; non-locking metal reject", "mono"

    if family == "TAG":
        if number == 1:
            binder, sources = binder_material(bank, index + 1, False)
            raw = mix_layers([(binder, 0.60, 0.0), (physical_click(bank, index), 0.26, 0.035)], 2)
            return finalize_one_shot(raw, -5.7, None, seed), False, sources + ["LASO-2244"], transform + "; PASS paper snap + secure click", "stereo"
        if number == 2:
            lock = physical_click(bank, index + 3, True)
            tail = damped_tone(0.50, 88 + index * 6, 0.14, seed, (1.0, 1.55))
            raw = mix_layers([(lock, 0.70, 0.0), (tail, 0.16, 0.025)], 2)
            return finalize_one_shot(raw, -5.5, None, seed), False, ["LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; BRACE lock + tension settle", "stereo"
        if number == 3:
            body, sources = body_material(bank, index + 2, False)
            paper = bank.raw_hit("workbook", index + 6)
            raw = mix_layers([(body, 0.58, 0.0), (paper, 0.22, 0.055)], 2)
            return finalize_one_shot(raw, -5.5, None, seed), False, sources + ["LASO-0786"], transform + "; BREAK body release; no voice", "stereo"
        if number == 4:
            binder, binder_sources = binder_material(bank, 4, False)
            lock = physical_click(bank, 7, True)
            neutral = damped_tone(0.44, 148, 0.13, seed, (1.0, 1.42, 2.13))
            raw = mix_layers([(binder, 0.38, 0.0), (lock, 0.50, 0.12), (neutral, 0.12, 0.25)], 2)
            return finalize_one_shot(raw, -4.8, None, seed), False, binder_sources + ["LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; two earned motifs + neutral close", "stereo"
        click = physical_click(bank, 9, False)
        paper = bank.raw_hit("workbook", 9)
        raw = mix_layers([(paper, 0.22, 0.0), (click, 0.34, 0.04)], 2)
        return finalize_one_shot(raw, -9.0, None, seed), False, ["LASO-0786", "LASO-2244"], transform + "; restrained conditional eligibility", "stereo"

    if family == "RUN":
        if number == 1:
            raw, sources = step_material(bank, index + 18, fast=index >= 3, runner=True)
            return finalize_one_shot(raw, -7.5), False, sources, transform + "; identical Runner footwear variation", "mono"
        if number == 2:
            step, step_sources = step_material(bank, index + 4, True, True)
            body, body_sources = body_material(bank, index, False)
            raw = mix_layers([(step, 0.58, 0.0), (body, 0.24, 0.08)], 1)
            return finalize_one_shot(raw, -6.0), False, step_sources + body_sources, transform + "; foot plant and upper-body weight shift", "mono"
        if number == 3:
            whoosh_id = "1798" if index % 2 == 0 else "1802"
            air = trim_active(bank.raw_mono(whoosh_id), -58, 2, 55)
            step, sources = step_material(bank, index + 10, True, True)
            raw = mix_layers([(air, 0.36, 0.0), (step, 0.44, 0.10)], 1)
            return finalize_one_shot(raw, -7.0), False, sources + [f"LASO-{whoosh_id}"], transform + "; body-check miss pass-by and brace", "mono"
        if number in (4, 5):
            body, sources = body_material(bank, index + number, medium=number == 4)
            step, step_sources = step_material(bank, index + 24, False, True)
            raw = mix_layers([(body, 0.92 if number == 4 else 0.76, 0.0), (step, 0.25, 0.032)], 1)
            return finalize_one_shot(raw, -4.2 if number == 4 else -5.0), False, sources + step_sources, transform + ("; Body Check nonlethal contact" if number == 4 else "; Shove contact and foot shift"), "mono"
        if number == 6:
            paper = bank.raw_hit("workbook", index + 5)
            body, sources = body_material(bank, index + 1, False)
            raw = mix_layers([(paper, 0.34, 0.0), (body, 0.38, 0.035)], 1)
            return finalize_one_shot(raw, -7.0), False, sources + ["LASO-0786"], transform + "; grip/contact start; no voice", "mono"
        if number == 7:
            cloth = loop_from_segment(bank.raw_mono("2803"), 1.4, 0.42, 0.16)
            friction = periodic_noise(1.4, 180.0, 3100.0, seed, 1) * 0.11
            return finalize_loop(cloth * 0.68 + friction, -15.0), True, ["LASO-2803", "PROJECT_ORIGINAL_DSP"], "restrained cloth/effort loop; no voice", "mono"
        if number == 8:
            body, sources = body_material(bank, index + 3, False)
            step, step_sources = step_material(bank, index + 15, False, True)
            raw = mix_layers([(body, 0.52, 0.0), (step, 0.42, 0.06), (bank.raw_hit("workbook", index + 10), 0.18, 0.025)], 1)
            return finalize_one_shot(raw, -6.0), False, sources + step_sources + ["LASO-0786"], transform + "; Grab rescue release and off-balance", "mono"
        body, sources = body_material(bank, index + number, medium=number == 10)
        step, step_sources = step_material(bank, index + 30, number == 10, True)
        gain = 0.52 if number == 9 else 0.76
        raw = mix_layers([(body, gain, 0.0), (step, 0.42, 0.035)], 1)
        return finalize_one_shot(raw, -6.2 if number == 9 else -4.5), False, sources + step_sources, transform + ("; light off-balance reaction" if number == 9 else "; medium nonlethal stagger"), "mono"

    if family == "DRW":
        if number == 2:
            duration = 2.3 if index == 0 else 1.9
            load = loop_from_segment(bank.raw_mono("1316"), duration, 1.8 + index * 7.5, 0.18)
            chair = loop_from_segment(bank.raw_mono("0666"), duration, 22.0 + index * 11.0, 0.18)
            raw = load * (0.74 if index == 0 else 0.82) + chair * (0.28 if index == 0 else 0.38)
            return finalize_loop(raw, -9.5 if index == 0 else -8.5), True, ["LASO-1316", "LASO-0666"], "heavy rolling asset scrape/roll; seam crossfade", "mono"
        if number == 1:
            load = trim_active(bank.raw_mono("3296"), -58, 3, 95)
            gate = trim_active(bank.raw_mono("2359"), -58, 3, 95)
            chair, sources = chair_material(bank, index, True)
            raw = mix_layers([(load, 0.50, 0.0), (gate, 0.42, 0.01), (chair, 0.44, 0.025)], 1)
            return finalize_one_shot(raw, -3.8), False, ["LASO-3296", "LASO-2359"] + sources, transform + "; attributed heavy Drawer contact", "mono"
        if number == 3:
            load = trim_active(bank.raw_mono("3296"), -58, 3, 90)
            click = physical_click(bank, 12, True)
            hum = damped_tone(0.72, 120.0, 0.18, seed, (1.0, 2.0, 3.02))
            raw = mix_layers([(load, 0.44, 0.0), (click, 0.40, 0.10), (hum, 0.08, 0.11)], 2)
            return finalize_one_shot(raw, -4.8, None, seed), False, ["LASO-3296", "LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; heavy settle then authoritative electrical confirmation", "stereo"
        load = trim_active(bank.raw_mono("3296"), -58, 3, 90)
        wheel = bank.raw_hit("chair", index + 18)
        raw = mix_layers([(wheel, 0.40, 0.0), (load, 0.62, 0.024), (physical_click(bank, index + 10), 0.16, 0.05)], 1)
        return finalize_one_shot(raw, -5.5), False, ["LASO-0666", "LASO-3296", "LASO-2244"], transform + "; parking-bay caster/frame settle", "mono"

    if family == "FIN":
        if number == 1:
            click = physical_click(bank, 15, True)
            gate = trim_active(bank.raw_mono("2359"), -58, 3, 70)
            raw = mix_layers([(click, 0.54, 0.0), (gate, 0.18, 0.018)], 2)
            return finalize_one_shot(raw, -6.0, None, seed), False, ["LASO-2244", "LASO-2359"], transform + "; restrained conditional commit", "stereo"
        if number == 2:
            step, step_sources = step_material(bank, index + 20, False, True)
            chair, chair_sources = chair_material(bank, index + 8, False)
            raw = mix_layers([(step, 0.35, 0.0), (bank.raw_hit("workbook", index + 3), 0.18, 0.05), (chair, 0.35, 0.12)], 1)
            return finalize_one_shot(raw, -7.0), False, step_sources + chair_sources + ["LASO-0786"], transform + "; cloth/foot/frame setup", "mono"
        if number == 3:
            chair, chair_sources = chair_material(bank, index + 10, True)
            body, body_sources = body_material(bank, index + 7, True)
            raw = mix_layers([(chair, 0.86, 0.0), (body, 0.55, 0.026)], 1)
            return finalize_one_shot(raw, -3.7), False, chair_sources + body_sources, transform + "; Chair-to-Runner causal contact", "mono"
        if number == 4:
            body, body_sources = body_material(bank, index + 10, True)
            load = trim_active(bank.raw_mono("3296"), -58, 3, 100)
            gate = trim_active(bank.raw_mono("2359"), -58, 3, 100)
            raw = mix_layers([(body, 0.42, 0.0), (load, 0.72, 0.085), (gate, 0.50, 0.098)], 1)
            return finalize_one_shot(raw, -3.4), False, body_sources + ["LASO-3296", "LASO-2359"], transform + "; Runner-to-Drawer contact offset preserves causal order", "mono"
        if number == 5:
            paper_id = "2803" if index % 2 == 0 else "2808"
            paper = trim_active(bank.raw_mono(paper_id), -58, 4, 90)
            wheel = bank.raw_hit("chair", index + 20)
            raw = mix_layers([(paper, 0.46, 0.0), (wheel, 0.26, 0.13)], 1)
            return finalize_one_shot(raw, -10.0), False, [f"LASO-{paper_id}", "LASO-0666"], transform + "; restrained paper/caster aftermath", "mono"
        click = physical_click(bank, 18, True)
        fixture = damped_tone(0.78, 188, 0.18, seed, (1.0, 1.48, 2.02))
        raw = mix_layers([(click, 0.52, 0.0), (fixture, 0.10, 0.04)], 2)
        return finalize_one_shot(raw, -6.0, None, seed), False, ["LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; fixture control return", "stereo"

    if family == "RST":
        if number == 1:
            click = physical_click(bank, 21, True)
            workbook = bank.raw_hit("workbook", 16)
            raw = mix_layers([(click, 0.66, 0.0), (workbook, 0.26, 0.055), (physical_click(bank, 22), 0.24, 0.16)], 2)
            return finalize_one_shot(raw, -4.8, None, seed), False, ["LASO-2244", "LASO-0786"], transform + "; authored mechanical Reset accept; no rewind", "stereo"
        load = trim_active(bank.raw_mono("3296"), -58, 3, 80)
        click = physical_click(bank, 24, False)
        raw = mix_layers([(load, 0.34, 0.0), (click, 0.28, 0.08)], 2)
        return finalize_one_shot(raw, -8.0, None, seed), False, ["LASO-3296", "LASO-2244"], transform + "; short baseline-restored settle", "stereo"

    if family == "EXT":
        if number == 1:
            gate = trim_active(bank.raw_mono("2359"), -58, 3, 80)
            click = physical_click(bank, 26, True)
            fixture = damped_tone(0.92, 160, 0.22, seed, (1.0, 2.02, 3.01))
            raw = mix_layers([(gate, 0.26, 0.0), (click, 0.48, 0.05), (fixture, 0.10, 0.06)], 2)
            return finalize_one_shot(raw, -4.8, None, seed), False, ["LASO-2359", "LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; grounded relay/fixture activation", "stereo"
        if number == 2:
            step, sources = step_material(bank, index + 25, True, False)
            gate = trim_active(bank.raw_mono("2359"), -58, 3, 70)
            raw = mix_layers([(step, 0.52, 0.0), (gate, 0.20, 0.12)], 1)
            return finalize_one_shot(raw, -6.5), False, sources + ["LASO-2359"], transform + "; movement transition toward Exit", "mono"
        if number == 3:
            click = physical_click(bank, 30, True)
            binder, sources = binder_material(bank, 2, False)
            raw = mix_layers([(click, 0.56, 0.0), (binder, 0.24, 0.06), (damped_tone(0.64, 132, 0.17, seed), 0.09, 0.04)], 2)
            return finalize_one_shot(raw, -4.5, None, seed), False, sources + ["LASO-2244", "PROJECT_ORIGINAL_DSP"], transform + "; short grounded clear confirmation", "stereo"
        click = physical_click(bank, 32, False)
        paper = bank.raw_hit("workbook", 18)
        raw = mix_layers([(paper, 0.26, 0.0), (click, 0.36, 0.05)], 2)
        return finalize_one_shot(raw, -7.0, None, seed), False, ["LASO-0786", "LASO-2244"], transform + "; neutral result-card settle", "stereo"

    if family == "UI":
        if number == 1:
            click = physical_click(bank, index + 35, False)
            workbook = bank.raw_hit("workbook", index + 1)
            raw = mix_layers([(click, 0.50, 0.0), (workbook, 0.20, 0.018)], 2)
            return finalize_one_shot(raw, -6.0, None, seed), False, ["LASO-2244", "LASO-0786"], transform + "; grounded confirm family", "stereo"
        if number == 2:
            click = fft_filter(physical_click(bank, index + 40, False), 90, 3600)
            paper = bank.raw_hit("workbook", index + 8)
            raw = mix_layers([(click, 0.32, 0.0), (paper, 0.18, 0.018)], 2)
            return finalize_one_shot(raw, -9.0, None, seed), False, ["LASO-2244", "LASO-0786"], transform + "; muted non-latching reject family", "stereo"
        click = physical_click(bank, 44, False)
        raw = mix_layers([(click, 0.24, 0.0), (bank.raw_hit("workbook", 12), 0.12, 0.015)], 2)
        return finalize_one_shot(raw, -12.0, None, seed), False, ["LASO-2244", "LASO-0786"], transform + "; very soft conditional context change", "stereo"

    raise KeyError(cue_id)


def create_contact_sheet(entries: list[dict], path: Path) -> None:
    columns = 7
    cell_w = 390
    cell_h = 100
    rows = math.ceil(len(entries) / columns)
    image = Image.new("RGB", (columns * cell_w, rows * cell_h), "#11151b")
    draw = ImageDraw.Draw(image)
    font = ImageFont.load_default()
    for index, entry in enumerate(entries):
        col = index % columns
        row = index // columns
        x0 = col * cell_w
        y0 = row * cell_h
        draw.rectangle((x0, y0, x0 + cell_w - 1, y0 + cell_h - 1), outline="#2c3642")
        draw.text((x0 + 6, y0 + 5), f"{entry['cue_id']}  {Path(entry['path']).name[:34]}", fill="#d9e2ec", font=font)
        audio = read_wav(path.parents[2] / entry["path"]).samples
        x = mono(audio)
        width = cell_w - 12
        stride = max(1, len(x) // width)
        peak = np.array([np.max(np.abs(x[i : i + stride])) for i in range(0, len(x), stride)])[:width]
        center = y0 + 62
        scale = 27
        points_top = [(x0 + 6 + i, center - int(v * scale)) for i, v in enumerate(peak)]
        points_bottom = [(x0 + 6 + i, center + int(v * scale)) for i, v in reversed(list(enumerate(peak)))]
        if points_top:
            draw.polygon(points_top + points_bottom, fill="#4fb3bf")
        draw.text((x0 + 6, y0 + 84), f"{entry['duration']:.3f}s  {entry['peak_dbfs']:.2f} dBFS", fill="#93a4b5", font=font)
    image.save(path, optimize=True)


def assemble_reel(entries: list[dict], showcase_root: Path, reel_path: Path) -> tuple[np.ndarray, list[dict]]:
    gap = np.zeros((int(0.38 * SR), 2), dtype=np.float64)
    lead = np.zeros((int(0.50 * SR), 2), dtype=np.float64)
    pieces = [lead]
    cursor = len(lead) / SR
    index_rows = []
    for entry in entries:
        audio = read_wav(showcase_root / entry["path"]).samples
        if audio.ndim == 1:
            audio = np.stack([audio, audio], axis=1)
        if entry["loop"] and len(audio) > int(2.6 * SR):
            audio = audio[: int(2.6 * SR)]
            audio = fade(audio, 8.0, 24.0)
        start = cursor
        pieces.append(audio)
        cursor += len(audio) / SR
        index_rows.append({
            "start": start,
            "end": cursor,
            "cue_id": entry["cue_id"],
            "filename": Path(entry["path"]).name,
            "variation_count": entry["variation_count"],
        })
        pieces.append(gap)
        cursor += len(gap) / SR
    reel = np.concatenate(pieces, axis=0)
    reel = normalize_peak(remove_dc(reel), -3.2)
    write_wav24(reel_path, reel)
    return reel, index_rows


def main() -> None:
    script_path = Path(__file__).resolve()
    showcase_root = script_path.parents[3]
    repo_root = showcase_root.parents[2]
    matrix_path = repo_root / "docs" / "10_contest" / "workstreams" / "audio" / "SHOWCASE_AUDIO_CUE_MATRIX_v001.csv"
    export_root = showcase_root / "Export" / "ProductionV002"
    preview_root = showcase_root / "Preview" / "ProductionV002"
    preview_root.mkdir(parents=True, exist_ok=True)

    # Two temp-tree copies were conservatively excluded after page/content review:
    # 2456 has a reported background-voice risk, while 2032 is computer-designed
    # rather than recorded Foley. Keep the upstream temp tree untouched and make
    # the repository selection exact.
    raw_root = showcase_root / "Source" / "ProductionV002" / "RawCC0" / "LaSonotheque"
    for excluded_name in ("BSB_2456_BodyFall_05.wav", "BSB_2032_Stick_Passage.wav"):
        excluded_path = (raw_root / excluded_name).resolve()
        if excluded_path.parent != raw_root.resolve():
            raise ValueError(f"unsafe excluded-source path: {excluded_path}")
        if excluded_path.exists():
            excluded_path.unlink()

    with matrix_path.open("r", encoding="utf-8-sig", newline="") as handle:
        matrix_rows = list(csv.DictReader(handle))
    matrix_ids = [row["Cue ID"] for row in matrix_rows]
    if matrix_ids != list(COUNT_BY_CUE):
        missing = sorted(set(COUNT_BY_CUE) - set(matrix_ids))
        extra = sorted(set(matrix_ids) - set(COUNT_BY_CUE))
        raise ValueError(f"matrix cue order/scope drift; missing={missing}; extra={extra}")
    if sum(COUNT_BY_CUE.values()) != 196:
        raise ValueError("expected exact 196-output plan")

    bank = SourceBank(showcase_root)
    entries = []
    by_cue = {}
    for row in matrix_rows:
        cue_id = row["Cue ID"]
        family = cue_id.split("-")[1]
        count = COUNT_BY_CUE[cue_id]
        names = output_names(cue_id, row["Expected Filename"], count)
        by_cue[cue_id] = []
        for index, filename in enumerate(names):
            samples, is_loop, sources, transformation, channel_intent = build_cue(bank, cue_id, index)
            if not is_loop:
                maximum_duration = {
                    "AMB": 1.25,
                    "PLY": 1.10,
                    "BND": 1.20,
                    "CHR": 1.25,
                    "ALY": 1.10,
                    "STR": 1.20,
                    "TAG": 1.10,
                    "RUN": 1.30,
                    "DRW": 1.40,
                    "FIN": 1.50,
                    "RST": 1.10,
                    "EXT": 1.30,
                    "UI": 0.80,
                }[family]
                samples = limit_one_shot(samples, maximum_duration)
            if channel_intent == "mono":
                samples = mono(samples)
            elif samples.ndim == 1:
                samples = stereo(samples, 0.10, index)
            path = export_root / FOLDER_BY_FAMILY[family] / filename
            write_wav24(path, samples)
            decoded = read_wav(path)
            stats = metric(decoded.samples)
            relative = path.relative_to(showcase_root).as_posix()
            entry = {
                "cue_id": cue_id,
                "locked_gameplay_event": row["Locked Gameplay Event"],
                "contest_usage": row["Contest Usage"],
                "family": family,
                "category": FOLDER_BY_FAMILY[family],
                "variation_index": index + 1,
                "variation_count": count,
                "path": relative,
                "filename": filename,
                "loop": is_loop,
                "channel_intent": channel_intent,
                "channels": decoded.channels,
                "sample_rate": SR,
                "bit_depth": BITS,
                "duration": decoded.duration,
                "bytes": path.stat().st_size,
                "sha256": sha256_file(path),
                "peak": stats["peak"],
                "peak_dbfs": stats["peak_dbfs"],
                "rms_dbfs": stats["rms_dbfs"],
                "dc_max": stats["dc_max"],
                "sources": sorted(set(sources)),
                "transformation": transformation,
                "source_status": "REVIEW_CANDIDATE",
                "runtime_integration": "NOT_PERFORMED",
                "future_runtime_hook": row["Future Runtime Hook"],
            }
            entries.append(entry)
            by_cue[cue_id].append(entry)

    manifest = {
        "schema": "BREAK_NOTICE_SHOWCASE_AUDIO_EXPORT_MANIFEST_V002",
        "ticket": "SHOWCASE-AUDIO-BULK-PRODUCTION-001",
        "relay": "EAR-CONTEST-0002-AUDIO-BULK-PRODUCTION-001",
        "review_status": "REVIEW_CANDIDATE",
        "runtime_integration": "NOT_PERFORMED",
        "cue_count": len(by_cue),
        "required_cue_count": sum(1 for row in matrix_rows if row["Contest Usage"] == "REQUIRED"),
        "conditional_cue_count": sum(1 for row in matrix_rows if row["Contest Usage"] == "CONDITIONAL"),
        "export_count": len(entries),
        "entries": entries,
    }
    manifest_path = preview_root / "SHOWCASE_AUDIO_EXPORT_MANIFEST_v002.json"
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")

    reel_specs = [
        ("AUDITION_A_PLAYER_BINDER_CHAIR_v002.wav", {"PLY", "BND", "CHR"}),
        ("AUDITION_B_ALLY_STRAP_TAGS_v002.wav", {"ALY", "STR", "TAG"}),
        ("AUDITION_C_RUNNER_PRESSURE_v002.wav", {"RUN"}),
        ("AUDITION_D_DRAWER_RESET_EXIT_UI_AMBIENCE_v002.wav", {"DRW", "RST", "EXT", "UI", "AMB"}),
        ("AUDITION_E_HANDOVER_CRASH_CONDITIONAL_v002.wav", {"FIN"}),
    ]
    reel_manifest = []
    for reel_name, families in reel_specs:
        selected = []
        for row in matrix_rows:
            if row["Cue ID"].split("-")[1] in families:
                selected.extend(by_cue[row["Cue ID"]])
        reel_path = preview_root / reel_name
        reel, index_rows = assemble_reel(selected, showcase_root, reel_path)
        reel_manifest.append({
            "filename": reel_name,
            "path": reel_path.relative_to(showcase_root).as_posix(),
            "duration": len(reel) / SR,
            "channels": 2,
            "sample_rate": SR,
            "bit_depth": BITS,
            "bytes": reel_path.stat().st_size,
            "sha256": sha256_file(reel_path),
            "peak_dbfs": metric(reel)["peak_dbfs"],
            "index": index_rows,
        })
    reel_path_json = preview_root / "SHOWCASE_AUDIO_AUDITION_INDEX_v002.json"
    reel_path_json.write_text(json.dumps(reel_manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")

    contact_path = preview_root / "SHOWCASE_AUDIO_WAVEFORM_CONTACT_SHEET_v002.png"
    create_contact_sheet(entries, contact_path)

    print(json.dumps({
        "cue_count": len(by_cue),
        "export_count": len(entries),
        "family_counts": {family: sum(1 for entry in entries if entry["family"] == family) for family in FOLDER_BY_FAMILY},
        "reels": [{"name": item["filename"], "duration": item["duration"]} for item in reel_manifest],
        "manifest": str(manifest_path),
        "contact_sheet": str(contact_path),
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
