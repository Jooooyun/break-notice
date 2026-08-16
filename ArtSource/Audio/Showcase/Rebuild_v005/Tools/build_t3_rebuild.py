#!/usr/bin/env python3
"""Build the isolated BREAK NOTICE Chair-to-Runner T3 v005 REVIEW pair.

This production builder reads only exact-hash files below Sources/Lossless.
It does not read any sonic reference or previous review pack.  Automated
measurements are diagnostic regression guards, never a perceptual approval.
"""

from __future__ import annotations

import hashlib
import json
import math
import shutil
from dataclasses import asdict, dataclass
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
PCM24_STEP = 1.0 / (2**23)
PRODUCTION_HEADROOM_SCALAR = 0.30
STEMS = ("pre", "signature", "emitter", "runner", "floor_tail")

ROOT = Path(__file__).resolve().parents[1]
LOSSLESS = (ROOT / "Sources" / "Lossless").resolve()
RENDERS = ROOT / "Renders"
COMPOSITES = RENDERS / "Composites"
STEM_ROOT = RENDERS / "Stems"
EVIDENCE = RENDERS / "Evidence"


@dataclass(frozen=True)
class SourceSpec:
    relative_path: str
    sha256: str
    source_page: str
    direct_url: str
    evidence_page: str
    author: str
    license: str
    role: str
    variation: int


SOURCE_SPECS: dict[str, SourceSpec] = {
    "v01_pre_cart": SourceSpec(
        "LaSonotheque/LoadedHandTruckRoll_2_1317.wav",
        "6d006faa3e94fed8c0d1a6b21701a72c74d2a43fbd20bf680806437942e64921",
        "https://lasonotheque.org/diable-de-manutention-2-s1317.html",
        "https://lasonotheque.org/UPLOAD/bwf-en/1317.wav",
        "Sources/Evidence/lasonotheque-loaded-hand-truck-2.html",
        "Joseph SARDIN",
        "CC0 / public-domain-equivalent",
        "v01 hand-truck roll proxy",
        1,
    ),
    "v01_signature_joint": SourceSpec(
        "OpenGameArt_Mechanical/Mechanical_lightclunk1.wav",
        "d5a2aa8764c02924f6ed6fbaad0c650ab2d57ec57a626cee9e6645f941dbc8fa",
        "https://opengameart.org/content/mechanical-sounds",
        "https://opengameart.org/sites/default/files/lightclunk1.wav",
        "Sources/Evidence/opengameart-mechanical-sounds.html",
        "Brian MacIntosh / BMacZero",
        "CC0",
        "v01 damped structural-identity proxy; not a chair-specific recording",
        1,
    ),
    "v01_emitter_frame": SourceSpec(
        "OpenGameArt_Mechanical/Mechanical_clank1.wav",
        "899a46a8423fa0ca349b3c70a07a422d1eaa3a58695ce6facfbae074dd70226f",
        "https://opengameart.org/content/mechanical-sounds",
        "https://opengameart.org/sites/default/files/clank1.wav",
        "Sources/Evidence/opengameart-mechanical-sounds.html",
        "Brian MacIntosh / BMacZero",
        "CC0",
        "v01 mechanical-frame material proxy; not a chair-specific recording",
        1,
    ),
    "runner_bag_pool": SourceSpec(
        "LaSonotheque/CanvasBagFloor_0576.wav",
        "90ee960a18cc96ca29bc97ad9ca724fd81cb13189e3483e8b587a0a8e82ef8a9",
        "https://lasonotheque.org/sac-en-toile-sur-sol-s0576.html",
        "https://lasonotheque.org/UPLOAD/bwf-en/0576.wav",
        "Sources/Evidence/lasonotheque-canvas-bag-floor.html",
        "Joseph SARDIN",
        "CC0 / public-domain-equivalent",
        "nine recorded bag/cloth impact takes; v01 and v02 use distinct windows",
        0,
    ),
    "v01_runner_cloth": SourceSpec(
        "OpenGameArt_Owlish/BlanketMovement_2.wav",
        "797e3917079898e11c8597721d9747cd619fc61a59fb9bfaf0e253b72d68d65b",
        "https://opengameart.org/content/sound-effects-pack",
        "https://opengameart.org/sites/default/files/Owlish%20Media%20Sound%20Effects.zip",
        "Sources/Evidence/opengameart-owlish-sound-effects.html",
        "OwlishMedia / owlstorm",
        "CC0",
        "v01 independent blanket/cloth compression proxy",
        1,
    ),
    "v01_floor_load": SourceSpec(
        "LaSonotheque/DumbbellPlace_1_3295.wav",
        "c9da73a4ee1f750343b5f032747b9ecff3229a2d141343e281a4edfded2a35e3",
        "https://lasonotheque.org/haltere-pose-1-s3295.html",
        "https://lasonotheque.org/UPLOAD/bwf-en/3295.wav",
        "Sources/Evidence/lasonotheque-dumbbell-place-1.html",
        "Joseph SARDIN",
        "CC0 / public-domain-equivalent",
        "v01 short heavy floor load transfer",
        1,
    ),
    "v02_pre_rattle": SourceSpec(
        "OpenGameArt_Mechanical/Mechanical_rattle1.wav",
        "65ae71cd800986e4e4c0c1a180221c0f240f6ffd9e306f8634c0335a5e3f039a",
        "https://opengameart.org/content/mechanical-sounds",
        "https://opengameart.org/sites/default/files/rattle1.wav",
        "Sources/Evidence/opengameart-mechanical-sounds.html",
        "Brian MacIntosh / BMacZero",
        "CC0",
        "v02 restrained mechanical-rattle proxy; not a caster recording",
        2,
    ),
    "v02_signature_joint": SourceSpec(
        "OpenGameArt_Mechanical/Mechanical_lightclunk2.wav",
        "987be837a5941ded9a81572c319b9bf8f57d9f89741a5781fc7104adc87d60f5",
        "https://opengameart.org/content/mechanical-sounds",
        "https://opengameart.org/sites/default/files/lightclunk2.wav",
        "Sources/Evidence/opengameart-mechanical-sounds.html",
        "Brian MacIntosh / BMacZero",
        "CC0",
        "v02 alternate damped structural-identity proxy; not a chair-specific recording",
        2,
    ),
    "v02_emitter_shell": SourceSpec(
        "OpenGameArt_Mechanical/Mechanical_mechanical2.wav",
        "27b4b6c3517c90d635295fca48fccd747950f869d680c871662f26038c3665ef",
        "https://opengameart.org/content/mechanical-sounds",
        "https://opengameart.org/sites/default/files/mechanical2.wav",
        "Sources/Evidence/opengameart-mechanical-sounds.html",
        "Brian MacIntosh / BMacZero",
        "CC0",
        "v02 shell/mechanism material proxy; not a chair-specific recording",
        2,
    ),
    "v02_runner_cloth": SourceSpec(
        "OpenGameArt_Owlish/BlanketMovement_5.wav",
        "163e1968d3f6d18ab7859be6922a7584a36a95128be9fbc7c5db4c6cb33e603d",
        "https://opengameart.org/content/sound-effects-pack",
        "https://opengameart.org/sites/default/files/Owlish%20Media%20Sound%20Effects.zip",
        "Sources/Evidence/opengameart-owlish-sound-effects.html",
        "OwlishMedia / owlstorm",
        "CC0",
        "v02 independent blanket/cloth body proxy",
        2,
    ),
    "v02_floor_load": SourceSpec(
        "LaSonotheque/DumbbellPlace_3_3297.wav",
        "8fd17a3317ee7a30bb50288f3077d280fc5ac05d0f8dd65e0cc44bbc2a1ce38d",
        "https://lasonotheque.org/haltere-pose-3-s3297.html",
        "https://lasonotheque.org/UPLOAD/bwf-en/3297.wav",
        "Sources/Evidence/lasonotheque-dumbbell-place-3.html",
        "Joseph SARDIN",
        "CC0 / public-domain-equivalent",
        "v02 independent 10kg floor load-transfer take",
        2,
    ),
}


SOURCE_AUDIO: dict[str, np.ndarray] = {}
SOURCE_INFO: dict[str, dict[str, object]] = {}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def source_path(key: str) -> Path:
    path = (LOSSLESS / SOURCE_SPECS[key].relative_path).resolve()
    if not path.is_relative_to(LOSSLESS):
        raise RuntimeError(f"Source escaped lossless root: {path}")
    if path.is_symlink():
        raise RuntimeError(f"Symlink source rejected: {path}")
    return path


def validate_sources() -> None:
    allowed_formats = {"WAV", "WAVEX", "FLAC", "AIFF"}
    allowed_subtypes = {"PCM_16", "PCM_24", "PCM_32", "FLOAT", "DOUBLE"}
    seen_hashes: set[str] = set()
    by_variation: dict[int, set[str]] = {1: set(), 2: set()}
    for key, spec in SOURCE_SPECS.items():
        path = source_path(key)
        if path.suffix.lower() not in {".wav", ".flac", ".aiff", ".aif"}:
            raise RuntimeError(f"Lossy or unsupported source rejected: {path}")
        if not path.is_file():
            raise FileNotFoundError(path)
        actual_hash = sha256(path)
        if actual_hash != spec.sha256:
            raise RuntimeError(f"Source SHA mismatch for {key}: {actual_hash}")
        if actual_hash in seen_hashes:
            raise RuntimeError(f"Duplicate source bytes: {key}")
        seen_hashes.add(actual_hash)
        if spec.variation in by_variation:
            by_variation[spec.variation].add(actual_hash)
        info = sf.info(path)
        if info.format not in allowed_formats or info.subtype not in allowed_subtypes:
            raise RuntimeError(f"Lossless decode contract failed for {key}: {info}")
        SOURCE_INFO[key] = {
            **asdict(spec),
            "sha256": actual_hash,
            "format": info.format,
            "subtype": info.subtype,
            "sample_rate": info.samplerate,
            "channels": info.channels,
            "frames": info.frames,
            "duration_seconds": info.duration,
        }
    if by_variation[1] & by_variation[2]:
        raise RuntimeError("Variation source sets overlap")


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


def repair_isolated_full_scale(data: np.ndarray) -> tuple[np.ndarray, int]:
    """Interpolate isolated full-scale samples; never compress an event envelope."""
    result = data.copy()
    repaired = 0
    for channel in range(result.shape[1]):
        values = result[:, channel]
        candidates = np.flatnonzero(np.abs(values) >= 0.999999)
        for index in candidates:
            if index == 0 or index == len(values) - 1:
                continue
            if abs(values[index - 1]) < 0.95 and abs(values[index + 1]) < 0.95:
                values[index] = 0.5 * (values[index - 1] + values[index + 1])
                repaired += 1
    return result, repaired


def source(key: str) -> np.ndarray:
    if key not in SOURCE_AUDIO:
        data, rate = sf.read(source_path(key), always_2d=True, dtype="float64")
        data = resample(ensure_stereo(data), int(rate))
        data, repaired = repair_isolated_full_scale(data)
        data -= np.mean(data, axis=0, keepdims=True)
        SOURCE_INFO[key]["isolated_full_scale_samples_repaired"] = repaired
        SOURCE_AUDIO[key] = data
    return SOURCE_AUDIO[key]


def blank() -> np.ndarray:
    return np.zeros((RENDER_FRAMES, 2), dtype=np.float64)


def crop(key: str, first_seconds: float, last_seconds: float) -> np.ndarray:
    data = source(key)
    first = max(0, int(round(first_seconds * SAMPLE_RATE)))
    last = min(len(data), int(round(last_seconds * SAMPLE_RATE)))
    if last <= first:
        raise RuntimeError(f"Empty source slice: {key} {first_seconds}..{last_seconds}")
    return data[first:last].copy()


def causal_filter(
    data: np.ndarray,
    highpass: float | None = None,
    lowpass: float | None = None,
    order: int = 3,
) -> np.ndarray:
    if highpass is None and lowpass is None:
        return data.copy()
    nyquist = SAMPLE_RATE / 2.0
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
    return signal.sosfilt(sos, data, axis=0)


def fade(data: np.ndarray, fade_in_seconds: float, fade_out_seconds: float) -> np.ndarray:
    result = data.copy()
    count_in = min(len(result), int(round(fade_in_seconds * SAMPLE_RATE)))
    count_out = min(len(result), int(round(fade_out_seconds * SAMPLE_RATE)))
    if count_in > 1:
        result[:count_in] *= np.sin(np.linspace(0.0, math.pi / 2.0, count_in))[:, None] ** 2
    if count_out > 1:
        result[-count_out:] *= np.cos(np.linspace(0.0, math.pi / 2.0, count_out))[:, None] ** 2
    return result


def damp(data: np.ndarray, amount: float) -> np.ndarray:
    if len(data) == 0 or amount <= 0.0:
        return data.copy()
    envelope = np.exp(-np.linspace(0.0, amount, len(data)))[:, None]
    return data * envelope


def short_rms_envelope(data: np.ndarray, window_seconds: float = 0.010) -> np.ndarray:
    mono_power = np.mean(data * data, axis=1)
    count = max(1, int(round(window_seconds * SAMPLE_RATE)))
    return np.sqrt(np.convolve(mono_power, np.ones(count) / count, mode="same"))


def unit_short_rms(data: np.ndarray) -> np.ndarray:
    peak_rms = float(np.max(short_rms_envelope(data))) if len(data) else 0.0
    if peak_rms <= 1e-9:
        raise RuntimeError("Silent source slice")
    return data / peak_rms


def pan_narrow(data: np.ndarray, pan: float) -> np.ndarray:
    mono = np.mean(data, axis=1)
    pan = float(np.clip(pan, -1.0, 1.0))
    left = math.cos((pan + 1.0) * math.pi / 4.0)
    right = math.sin((pan + 1.0) * math.pi / 4.0)
    return np.column_stack((mono * left, mono * right)) * math.sqrt(2.0)


def place(canvas: np.ndarray, data: np.ndarray, start_seconds: float, gain: float) -> None:
    first = int(round(start_seconds * SAMPLE_RATE))
    if first < 0:
        data = data[-first:]
        first = 0
    if first >= len(canvas) or not len(data):
        return
    count = min(len(data), len(canvas) - first)
    canvas[first : first + count] += data[:count] * gain


def make_body(data: np.ndarray, pan: float) -> np.ndarray:
    # All three bands come from the same real impact take. The extra 55–125 Hz
    # weight restores natural load without adding a synthesized or tonal boom.
    sub = causal_filter(data, highpass=55.0, lowpass=125.0, order=3) * 5.50
    low = causal_filter(data, highpass=95.0, lowpass=520.0, order=3) * 0.70
    mid = causal_filter(data, highpass=320.0, lowpass=3_000.0, order=3) * 0.38
    body = fade(sub + low + mid, 0.008, 0.045)
    return pan_narrow(unit_short_rms(body), pan)


def build_v01() -> tuple[dict[str, np.ndarray], dict[str, object]]:
    stems = {name: blank() for name in STEMS}

    pre = crop("v01_pre_cart", 10.200, 10.325)
    pre = fade(causal_filter(pre, highpass=120.0, lowpass=1_400.0), 0.040, 0.025)
    place(stems["pre"], pan_narrow(unit_short_rms(pre), -0.12), 0.070, 0.026)

    signature = crop("v01_signature_joint", 0.240, 0.300)
    signature = damp(fade(causal_filter(signature, highpass=450.0, lowpass=2_800.0), 0.002, 0.022), 3.0)
    place(stems["signature"], pan_narrow(unit_short_rms(signature), 0.0), CONTACT_SECONDS, 0.140)

    emitter = crop("v01_emitter_frame", 0.178, 0.252)
    emitter = damp(fade(causal_filter(emitter, highpass=140.0, lowpass=2_800.0), 0.002, 0.030), 1.8)
    place(stems["emitter"], pan_narrow(unit_short_rms(emitter), -0.06), CONTACT_SECONDS, 0.440)

    runner = crop("runner_bag_pool", 0.050, 0.370)
    place(stems["runner"], make_body(runner, 0.035), 0.2190, 0.535)
    cloth = crop("v01_runner_cloth", 0.840, 1.180)
    cloth = fade(causal_filter(cloth, highpass=95.0, lowpass=2_400.0), 0.010, 0.055)
    place(stems["runner"], pan_narrow(unit_short_rms(cloth), -0.025), 0.230, 0.155)

    floor_tail = crop("v01_floor_load", 0.115, 0.285)
    floor_tail = fade(causal_filter(floor_tail, highpass=50.0, lowpass=680.0), 0.004, 0.050)
    place(stems["floor_tail"], pan_narrow(unit_short_rms(floor_tail), -0.04), 0.3379, 0.250)

    lineage = {
        "pre": ["v01_pre_cart"],
        "signature": ["v01_signature_joint"],
        "emitter": ["v01_emitter_frame"],
        "runner": ["runner_bag_pool:take_01@0.050-0.370", "v01_runner_cloth"],
        "floor_tail": ["v01_floor_load"],
        "physical_difference": "hand-truck roll proxy, mechanical-frame proxy, distinct canvas-bag/blanket compression take, 10kg floor load",
    }
    return stems, lineage


def build_v02() -> tuple[dict[str, np.ndarray], dict[str, object]]:
    stems = {name: blank() for name in STEMS}

    pre = crop("v02_pre_rattle", 0.040, 0.160)
    pre = fade(causal_filter(pre, highpass=120.0, lowpass=1_250.0), 0.035, 0.028)
    place(stems["pre"], pan_narrow(unit_short_rms(pre), 0.12), 0.065, 0.024)

    signature = crop("v02_signature_joint", 0.108, 0.150)
    signature = damp(fade(causal_filter(signature, highpass=430.0, lowpass=2_600.0), 0.002, 0.018), 2.8)
    place(stems["signature"], pan_narrow(unit_short_rms(signature), 0.0), CONTACT_SECONDS, 0.140)

    emitter = crop("v02_emitter_shell", 0.430, 0.535)
    emitter = damp(fade(causal_filter(emitter, highpass=160.0, lowpass=3_000.0), 0.002, 0.035), 1.7)
    place(stems["emitter"], pan_narrow(unit_short_rms(emitter), 0.06), CONTACT_SECONDS, 0.430)

    runner = crop("runner_bag_pool", 4.228, 4.548)
    place(stems["runner"], make_body(runner, -0.035), 0.2230, 0.555)
    cloth = crop("v02_runner_cloth", 2.270, 2.580)
    cloth = fade(causal_filter(cloth, highpass=55.0, lowpass=2_200.0), 0.010, 0.060)
    place(stems["runner"], pan_narrow(unit_short_rms(cloth), 0.025), 0.253, 0.145)

    floor_tail = crop("v02_floor_load", 0.105, 0.275)
    floor_tail = fade(causal_filter(floor_tail, highpass=50.0, lowpass=680.0), 0.004, 0.050)
    place(stems["floor_tail"], pan_narrow(unit_short_rms(floor_tail), 0.04), 0.3237, 0.240)

    lineage = {
        "pre": ["v02_pre_rattle"],
        "signature": ["v02_signature_joint"],
        "emitter": ["v02_emitter_shell"],
        "runner": ["runner_bag_pool:take_06@4.228-4.548", "v02_runner_cloth"],
        "floor_tail": ["v02_floor_load"],
        "physical_difference": "restrained mechanical-rattle proxy, shell/mechanism proxy, independent dense canvas/blanket take, compact 10kg floor support",
    }
    return stems, lineage


def quantize_pcm24(data: np.ndarray) -> np.ndarray:
    if float(np.max(np.abs(data))) >= 1.0:
        raise RuntimeError("PCM24 quantization input would clip")
    return np.round(data / PCM24_STEP) * PCM24_STEP


def finish_stems(stems: dict[str, np.ndarray]) -> tuple[dict[str, np.ndarray], np.ndarray, float]:
    for name in STEMS:
        stems[name] = np.nan_to_num(stems[name], nan=0.0, posinf=0.0, neginf=0.0)
    raw = sum((stems[name] for name in STEMS), start=blank())
    if float(np.max(np.abs(raw))) <= 1e-9:
        raise RuntimeError("Silent composite")
    scale = PRODUCTION_HEADROOM_SCALAR
    quantized = {name: quantize_pcm24(stems[name] * scale) for name in STEMS}
    composite = sum((quantized[name] for name in STEMS), start=blank())
    if float(np.max(np.abs(composite))) >= 1.0:
        raise RuntimeError("Summed quantized stems would clip")
    composite = quantize_pcm24(composite)
    return quantized, composite, scale


def dbfs(value: float) -> float:
    return 20.0 * math.log10(max(value, 1e-12))


def band_shares(data: np.ndarray) -> dict[str, float]:
    mono = np.mean(data, axis=1)
    windowed = mono * np.hanning(len(mono))
    spectrum = np.abs(np.fft.rfft(windowed)) ** 2
    frequencies = np.fft.rfftfreq(len(windowed), 1.0 / SAMPLE_RATE)
    total = float(np.sum(spectrum)) or 1.0
    return {
        f"{low}_{high}_hz": float(np.sum(spectrum[(frequencies >= low) & (frequencies < high)]) / total)
        for low, high in ((20, 120), (120, 500), (500, 3_000), (3_000, 20_000))
    }


def metrics(composite: np.ndarray, stems: dict[str, np.ndarray]) -> dict[str, object]:
    contact_first = int(round(CONTACT_SECONDS * SAMPLE_RATE))
    contact_last = int(round((CONTACT_SECONDS + 0.350) * SAMPLE_RATE))
    first_50_last = int(round((CONTACT_SECONDS + 0.050) * SAMPLE_RATE))
    pre_first = int(round((CONTACT_SECONDS - 0.125) * SAMPLE_RATE))
    contact = composite[contact_first:contact_last]
    first_50 = composite[contact_first:first_50_last]
    pre = composite[pre_first:contact_first]
    contact_energy = float(np.sum(contact * contact)) or 1e-12
    stem_energies = {
        name: float(np.sum(stems[name][contact_first:contact_last] ** 2)) for name in STEMS
    }
    stem_total = sum(stem_energies.values()) or 1e-12
    runner_envelope = short_rms_envelope(stems["runner"][contact_first:contact_last])
    body_peak_offset = int(np.argmax(runner_envelope)) / SAMPLE_RATE
    oversampled = signal.resample_poly(composite, 4, 1, axis=0)
    meter = pyln.Meter(SAMPLE_RATE)
    loudness = float(meter.integrated_loudness(composite))
    return {
        "integrated_lufs": loudness,
        "sample_peak_dbfs": dbfs(float(np.max(np.abs(composite)))),
        "estimated_true_peak_dbfs_4x": dbfs(float(np.max(np.abs(oversampled)))),
        "contact_0_350ms_rms_dbfs": dbfs(float(np.sqrt(np.mean(contact * contact)))),
        "contact_crest_db": dbfs(float(np.max(np.abs(contact)))) - dbfs(float(np.sqrt(np.mean(contact * contact)))),
        "first_50ms_energy_share": float(np.sum(first_50 * first_50) / contact_energy),
        "pre_peak_below_contact_db": dbfs(float(np.max(np.abs(contact)))) - dbfs(float(np.max(np.abs(pre)))),
        "contact_boundary_step_dbfs": dbfs(float(np.max(np.abs(composite[contact_first] - composite[contact_first - 1])))),
        "runner_body_peak_offset_seconds": body_peak_offset,
        "stem_contact_energy_share": {name: energy / stem_total for name, energy in stem_energies.items()},
        "contact_band_energy_share": band_shares(contact),
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }


def diagnostic_warnings(row: dict[str, object]) -> list[str]:
    warnings: list[str] = []
    shares = row["stem_contact_energy_share"]
    runner_share = float(shares["runner"])
    emitter_share = float(shares["emitter"])
    signature_share = float(shares["signature"])
    if not 0.55 <= runner_share <= 0.75:
        warnings.append(f"runner share outside 55-75% diagnostic band: {runner_share:.3f}")
    if emitter_share > 0.45:
        warnings.append(f"emitter share above 45%: {emitter_share:.3f}")
    if signature_share > 0.10:
        warnings.append(f"signature share above 10%: {signature_share:.3f}")
    if float(row["first_50ms_energy_share"]) > 0.50:
        warnings.append("first-50ms energy share above 50%")
    if not 0.030 <= float(row["runner_body_peak_offset_seconds"]) <= 0.070:
        warnings.append("runner body peak outside +30..70ms")
    if float(row["pre_peak_below_contact_db"]) < 12.0:
        warnings.append("pre peak is less than 12dB below contact")
    crest = float(row["contact_crest_db"])
    if not 15.0 <= crest <= 18.0:
        warnings.append(f"contact crest outside 15-18dB diagnostic band: {crest:.2f}")
    first_50 = float(row["first_50ms_energy_share"])
    if not 0.35 <= first_50 <= 0.48:
        warnings.append(f"first-50ms energy outside 35-48% diagnostic band: {first_50:.3f}")
    bands = row["contact_band_energy_share"]
    low_share = float(bands["20_120_hz"])
    high_share = float(bands["3000_20000_hz"])
    if not 0.40 <= low_share <= 0.55:
        warnings.append(f"20-120Hz energy outside 40-55% Y-direction diagnostic band: {low_share:.3f}")
    if not 0.05 <= high_share <= 0.12:
        warnings.append(f"3-20kHz energy outside 5-12% Y-direction diagnostic band: {high_share:.3f}")
    return warnings


def write_wav(path: Path, data: np.ndarray) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    sf.write(path, data, SAMPLE_RATE, subtype="PCM_24", format="WAV")


def render_diagnostic(rows: list[dict[str, object]], audio: dict[str, np.ndarray]) -> None:
    figure, axes = plt.subplots(2, 2, figsize=(14, 8), sharex="col")
    for column, cue_id in enumerate(audio):
        data = audio[cue_id]
        mono = np.mean(data, axis=1)
        time = np.arange(len(mono)) / SAMPLE_RATE - CONTACT_SECONDS
        axes[0, column].plot(time, mono, color="#d7e3f4", linewidth=0.65, label="waveform")
        axes[0, column].plot(time, short_rms_envelope(data), color="#ffb454", linewidth=1.2, label="10ms RMS")
        axes[0, column].axvline(0.0, color="white", alpha=0.6, linewidth=0.8)
        axes[0, column].set_title(cue_id)
        axes[0, column].set_xlim(-0.15, 0.65)
        axes[0, column].set_ylim(-0.75, 0.75)
        axes[0, column].grid(alpha=0.15)
        frequencies, times, spectrum = signal.spectrogram(mono, SAMPLE_RATE, nperseg=1024, noverlap=896)
        db = 10.0 * np.log10(np.maximum(spectrum, 1e-12))
        axes[1, column].pcolormesh(times - CONTACT_SECONDS, frequencies, db, shading="auto", vmin=-100, vmax=-25)
        axes[1, column].axvline(0.0, color="white", alpha=0.6, linewidth=0.8)
        axes[1, column].set_yscale("log")
        axes[1, column].set_ylim(20, 20_000)
        axes[1, column].set_xlim(-0.15, 0.65)
        axes[1, column].set_xlabel("seconds relative to contact")
    axes[0, 0].set_ylabel("amplitude")
    axes[1, 0].set_ylabel("frequency (Hz)")
    figure.suptitle("BREAK NOTICE T3 v005 diagnostics — measurements do not grant perceptual PASS")
    figure.tight_layout()
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    figure.savefig(EVIDENCE / "T3_V005_WAVEFORM_SPECTROGRAM_REVIEW.png", dpi=170)
    plt.close(figure)


def validate_outputs(cue_rows: list[dict[str, object]]) -> dict[str, object]:
    failures: list[str] = []
    composite_files = sorted(COMPOSITES.glob("*.wav"))
    stem_files = sorted(STEM_ROOT.rglob("*.wav"))
    if len(composite_files) != 2:
        failures.append(f"expected 2 composites, got {len(composite_files)}")
    if len(stem_files) != 10:
        failures.append(f"expected 10 stems, got {len(stem_files)}")
    file_rows: list[dict[str, object]] = []
    for path in composite_files + stem_files:
        info = sf.info(path)
        data, rate = sf.read(path, always_2d=True, dtype="float64")
        if (rate, data.shape[1], len(data), info.subtype) != (SAMPLE_RATE, 2, RENDER_FRAMES, "PCM_24"):
            failures.append(f"format contract failed: {path.relative_to(ROOT)}")
        if not np.all(np.isfinite(data)):
            failures.append(f"non-finite samples: {path.relative_to(ROOT)}")
        if float(np.max(np.abs(data))) >= 1.0:
            failures.append(f"clipping/full-scale sample: {path.relative_to(ROOT)}")
        if float(np.max(np.abs(np.mean(data, axis=0)))) > 1e-4:
            failures.append(f"DC offset above 1e-4: {path.relative_to(ROOT)}")
        contact_frame = int(round(CONTACT_SECONDS * SAMPLE_RATE))
        boundary_step = float(np.max(np.abs(data[contact_frame] - data[contact_frame - 1])))
        if boundary_step > 0.003:
            failures.append(f"contact boundary discontinuity above 0.003: {path.relative_to(ROOT)} {boundary_step}")
        file_rows.append({
            "path": str(path.relative_to(ROOT)).replace("\\", "/"),
            "sha256": sha256(path),
            "sample_rate": rate,
            "channels": data.shape[1],
            "frames": len(data),
            "subtype": info.subtype,
        })
    for row in cue_rows:
        cue_id = str(row["cue_id"])
        composite, _ = sf.read(COMPOSITES / f"{cue_id}_REVIEW.wav", always_2d=True, dtype="float64")
        stem_sum = np.zeros_like(composite)
        for stem in STEMS:
            path = STEM_ROOT / cue_id / f"{cue_id}_{stem}_REVIEW.wav"
            data, _ = sf.read(path, always_2d=True, dtype="float64")
            stem_sum += data
        null_error = float(np.max(np.abs(stem_sum - composite)))
        row["decoded_stem_sum_null_error"] = null_error
        if null_error > PCM24_STEP * 1.5:
            failures.append(f"stem sum null exceeds 1.5 PCM24 LSB: {cue_id} {null_error}")
    hashes = [row["sha256"] for row in file_rows]
    if len(hashes) != len(set(hashes)):
        failures.append("output files are not all byte-distinct")
    return {
        "status": "PASS" if not failures else "FAIL",
        "failures": failures,
        "files": file_rows,
        "composite_count": len(composite_files),
        "stem_count": len(stem_files),
        "contact_frame": int(CONTACT_SECONDS * SAMPLE_RATE),
        "automatic_perceptual_conclusion": "NONE / HUMAN VERDICT REQUIRED",
    }


def main() -> None:
    validate_sources()
    for directory in (COMPOSITES, STEM_ROOT):
        if directory.exists():
            shutil.rmtree(directory)
        directory.mkdir(parents=True, exist_ok=True)
    EVIDENCE.mkdir(parents=True, exist_ok=True)

    builders = {
        "T3_CHAIR_RUNNER_v005_v01_RC": build_v01,
        "T3_CHAIR_RUNNER_v005_v02_RC": build_v02,
    }
    cue_rows: list[dict[str, object]] = []
    composites: dict[str, np.ndarray] = {}
    for cue_id, builder in builders.items():
        stems, lineage = builder()
        stems, composite, scale = finish_stems(stems)
        for stem_name in STEMS:
            write_wav(STEM_ROOT / cue_id / f"{cue_id}_{stem_name}_REVIEW.wav", stems[stem_name])
        composite_path = COMPOSITES / f"{cue_id}_REVIEW.wav"
        write_wav(composite_path, composite)
        row_metrics = metrics(composite, stems)
        cue_rows.append({
            "cue_id": cue_id,
            "status": "REVIEW_CANDIDATE / HUMAN VERDICT REQUIRED",
            "contact_seconds": CONTACT_SECONDS,
            "render_seconds": RENDER_SECONDS,
            "global_headroom_scalar": scale,
            "lineage": lineage,
            "composite": {
                "path": str(composite_path.relative_to(ROOT)).replace("\\", "/"),
                "sha256": sha256(composite_path),
            },
            "stems": [
                {
                    "name": stem_name,
                    "path": str((STEM_ROOT / cue_id / f"{cue_id}_{stem_name}_REVIEW.wav").relative_to(ROOT)).replace("\\", "/"),
                    "sha256": sha256(STEM_ROOT / cue_id / f"{cue_id}_{stem_name}_REVIEW.wav"),
                }
                for stem_name in STEMS
            ],
            "metrics": row_metrics,
            "diagnostic_warnings": diagnostic_warnings(row_metrics),
        })
        composites[cue_id] = composite

    render_diagnostic(cue_rows, composites)

    source_ledger = {
        "status": "VERIFIED_SOURCE_INPUT / PERCEPTUAL QUALITY NOT INFERRED",
        "license_scope": "official source pages identify CC0 or public-domain-equivalent use",
        "exact_chair_recording_present": False,
        "exact_human_body_recording_present": False,
        "chair_specific_source_gap": "OPEN: no loaded office-chair seat/back/frame/caster recording is present; mechanical sources are review proxies only",
        "variation_exclusive_source_byte_sets_are_disjoint": True,
        "complete_source_file_sets_are_disjoint": False,
        "shared_multitake_pool": {
            "source_key": "runner_bag_pool",
            "v01_window_frames": [2_400, 17_760],
            "v02_window_frames": [202_944, 218_304],
            "window_overlap_frames": 0,
        },
        "source_archives": {
            "OpenGameArt_Owlish": {
                "archive_sha256": "57d7059fe2c0ea5f47c7554ace22a538ea22ab40bfc0906b71aa259afffd3d18",
                "used_members": [
                    "Cloth, Rustle/320138__owlstorm__blanket-movement-2.wav",
                    "Cloth, Rustle/320143__owlstorm__blanket-movement-5.wav",
                ],
            }
        },
        "sources": SOURCE_INFO,
    }
    (EVIDENCE / "SOURCE_LEDGER_REVIEW.json").write_text(json.dumps(source_ledger, indent=2), encoding="utf-8")

    manifest = {
        "status": "REVIEW_CANDIDATE / HUMAN VERDICT REQUIRED",
        "sample_rate": SAMPLE_RATE,
        "subtype": "PCM_24",
        "channels": 2,
        "frames": RENDER_FRAMES,
        "contact_seconds": CONTACT_SECONDS,
        "stems": list(STEMS),
        "cues": cue_rows,
        "numeric_guardrails": "diagnostic only; cannot grant perceptual PASS",
    }
    (EVIDENCE / "CUE_MANIFEST_REVIEW.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")

    qa = validate_outputs(cue_rows)
    qa["cue_diagnostics"] = [
        {
            "cue_id": row["cue_id"],
            "metrics": row["metrics"],
            "diagnostic_warnings": row["diagnostic_warnings"],
            "decoded_stem_sum_null_error": row.get("decoded_stem_sum_null_error"),
        }
        for row in cue_rows
    ]
    (EVIDENCE / "TECHNICAL_QA_REVIEW.json").write_text(json.dumps(qa, indent=2), encoding="utf-8")
    (EVIDENCE / "CUE_MANIFEST_REVIEW.json").write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(json.dumps({
        "technical_status": qa["status"],
        "cues": [
            {"cue_id": row["cue_id"], "warnings": row["diagnostic_warnings"], "metrics": row["metrics"]}
            for row in cue_rows
        ],
    }, indent=2))
    if qa["failures"]:
        raise RuntimeError("Technical QA failed: " + "; ".join(qa["failures"]))


if __name__ == "__main__":
    main()
