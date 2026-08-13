"""Build the BREAK NOTICE Showcase hit-VFX fidelity revision v005.

Unlike v004, this builder does not slice unrelated sprite banks into a broad
arrow/wedge.  It preserves the owner-selected 3/5/10 concept grammar by using
one approved-direction peak source per intensity, a fixed left contact anchor,
separated stress strokes, and an honest neutral-background review preview.

The outputs are review-only straight-alpha 4x4 flipbooks.  No gameplay hook,
damage, breakage, shader, package, or production-scene reference is created.
"""

from __future__ import annotations

import csv
import hashlib
import importlib.util
import json
import math
from dataclasses import dataclass
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageOps


CELL = 512
GRID = 4
ATLAS = CELL * GRID
FRAME_COUNT = 16
MIN_MARGIN = 24

SCRIPT_PATH = Path(__file__).resolve()
SOURCE_DIR = SCRIPT_PATH.parent
SHOWCASE_DIR = SOURCE_DIR.parents[1]
REPO_ROOT = SHOWCASE_DIR.parents[2]
FLIPBOOK_DIR = SHOWCASE_DIR / "Flipbooks" / "HitProduction_v005"
PREVIEW_DIR = SHOWCASE_DIR / "Preview" / "HitProduction_v005"
LICENSE_DIR = SHOWCASE_DIR / "Licenses"
UNITY_ROOT = REPO_ROOT / "Game" / "Assets" / "_Project" / "Art" / "VFX" / "Showcase"
UNITY_FLIPBOOK_DIR = UNITY_ROOT / "HitProduction_v005" / "Flipbooks"

CONCEPT_DIR = SHOWCASE_DIR / "Preview" / "HitProduction_v004" / "ConceptReferences"
CONCEPT_REFERENCES = {
    "Light": CONCEPT_DIR / "VFX_Showcase_Concept_Light_Directional_v004.png",
    "Medium": CONCEPT_DIR / "VFX_Showcase_Concept_Medium_ContactEcho_v004.png",
    "Heavy": CONCEPT_DIR / "VFX_Showcase_Concept_Heavy_Hybrid_v004.png",
}
BASE_SCENE = CONCEPT_DIR / "VFX_Showcase_Concept_BaseScene_v004.png"

IMAGEGEN_SOURCES = {
    "Light": SOURCE_DIR / "VFX_Showcase_HitLightPeak_ImageGen_v005.png",
    "Medium": SOURCE_DIR / "VFX_Showcase_HitMediumPeak_ImageGen_v005.png",
    "Heavy": SOURCE_DIR / "VFX_Showcase_HitHeavyPeak_ImageGen_v005.png",
}
ALPHA_SOURCES = {
    "Light": SOURCE_DIR / "VFX_Showcase_HitLightPeak_AlphaExtract_v005.png",
    "Medium": SOURCE_DIR / "VFX_Showcase_HitMediumPeak_AlphaExtract_v005.png",
    "Heavy": SOURCE_DIR / "VFX_Showcase_HitHeavyPeak_AlphaExtract_v005.png",
}

AUX_IMAGEGEN_SOURCES = {
    "ContactCompression": SOURCE_DIR / "VFX_Showcase_HitContactCompression_ImageGen_v005.png",
    "MaterialBody": SOURCE_DIR / "VFX_Showcase_HitMaterialBody_ImageGen_v005.png",
}
AUX_ALPHA_SOURCES = {
    "ContactCompression": SOURCE_DIR / "VFX_Showcase_HitContactCompression_AlphaExtract_v005.png",
    "MaterialBody": SOURCE_DIR / "VFX_Showcase_HitMaterialBody_AlphaExtract_v005.png",
}

LAYER_NAMES = ("Contact", "DirectionalBody", "HitStopEcho", "MaterialSettle")
VARIATIONS = ("A", "B", "C")
ORIGIN = (82.0, 256.0)


@dataclass(frozen=True)
class HitConfig:
    width: int
    height_limit: int
    brightness: float
    body_alpha: float
    ray_count: int
    settle_count: int
    preview_scale: float


CONFIGS = {
    "Light": HitConfig(330, 188, 0.92, 0.82, 5, 3, 0.92),
    "Medium": HitConfig(366, 224, 0.94, 0.88, 8, 5, 1.00),
    "Heavy": HitConfig(392, 252, 0.96, 0.92, 10, 7, 1.08),
}

ANGLE_BY_VARIATION = {"A": 0.0, "B": -5.0, "C": 5.5}
SCALE_BY_VARIATION = {"A": 1.0, "B": 0.94, "C": 0.98}
TIME_BIAS_BY_VARIATION = {"A": 0.0, "B": 0.018, "C": -0.014}
PROMPT_IDS = {
    "Light": "IG-VFX-HIT-005-LIGHT-DIRECTIONAL",
    "Medium": "IG-VFX-HIT-005-MEDIUM-CONTACT-ECHO",
    "Heavy": "IG-VFX-HIT-005-HEAVY-LAYERED",
}
AUX_PROMPT_IDS = {
    "ContactCompression": "IG-VFX-HIT-005-CONTACT-COMPRESSION",
    "MaterialBody": "IG-VFX-HIT-005-MATERIAL-BODY",
}


def relative(path: Path) -> str:
    return path.resolve().relative_to(REPO_ROOT).as_posix()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def save_png(image: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    image.convert("RGBA").save(path, format="PNG", optimize=False, compress_level=6)


def font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    for candidate in (
        Path("C:/Windows/Fonts/malgun.ttf"),
        Path("C:/Windows/Fonts/segoeui.ttf"),
        Path("C:/Windows/Fonts/arial.ttf"),
    ):
        if candidate.is_file():
            return ImageFont.truetype(str(candidate), size=size)
    return ImageFont.load_default()


def trim_alpha(image: Image.Image, threshold: int = 2) -> Image.Image:
    rgba = image.convert("RGBA")
    alpha = np.asarray(rgba.getchannel("A"))
    ys, xs = np.where(alpha > threshold)
    if len(xs) == 0:
        raise RuntimeError("Peak source is empty after alpha trim")
    return rgba.crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def sanitize_straight_alpha(image: Image.Image) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    alpha = rgba[:, :, 3]
    active = alpha > 0
    red = rgba[:, :, 0].astype(np.int16)
    green = rgba[:, :, 1].astype(np.int16)
    blue = rgba[:, :, 2].astype(np.int16)
    green_spill = active & (green > red * 1.32) & (green > blue * 1.32)
    neutral = ((red + blue) // 2).astype(np.uint8)
    rgba[:, :, 1][green_spill] = neutral[green_spill]
    # Procedural low-alpha interpolation can occasionally land exactly in
    # Unity's missing-texture diagnostic magenta range.  It is not a missing
    # texture, but preserving those pixels would create a false review alarm.
    magenta_diagnostic = active & (red >= 240) & (green <= 16) & (blue >= 240)
    rgba[:, :, :3][magenta_diagnostic] = np.array([244, 229, 198], dtype=np.uint8)
    unsafe_dark = active & (np.max(rgba[:, :, :3], axis=2) < 9)
    rgba[:, :, :3][unsafe_dark] = np.array([18, 17, 15], dtype=np.uint8)
    rgba[:, :, :3][~active] = 0
    return Image.fromarray(rgba, "RGBA")


def recolor_peak(image: Image.Image, intensity: str) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.float32).copy()
    alpha = rgba[:, :, 3] / 255.0
    lum = (rgba[:, :, :3] @ np.array([0.2126, 0.7152, 0.0722], dtype=np.float32)) / 255.0
    # Preserve the generated brush value while removing the yellow-green key cast.
    value = np.clip(0.38 + lum * 0.86, 0.0, 1.0)
    warm = np.array([244.0, 229.0, 198.0], dtype=np.float32)
    charcoal = np.array([102.0, 96.0, 86.0], dtype=np.float32)
    dark_mix = np.clip((0.42 - lum) / 0.42, 0.0, 1.0)[:, :, None]
    rgb = warm[None, None, :] * value[:, :, None]
    rgb = rgb * (1.0 - 0.48 * dark_mix) + charcoal[None, None, :] * (0.48 * dark_mix)
    if intensity == "Light":
        gamma, gain = 1.38, 1.18
    elif intensity == "Medium":
        gamma, gain = 1.24, 1.24
    else:
        gamma, gain = 1.16, 1.26
    alpha = np.clip(np.power(alpha, gamma) * gain, 0.0, 1.0)
    alpha[alpha < 0.018] = 0.0
    rgba[:, :, :3] = np.clip(rgb, 0, 255)
    rgba[:, :, 3] = alpha * 255.0
    rgba[:, :, :3][alpha <= 0] = 0
    return sanitize_straight_alpha(Image.fromarray(rgba.astype(np.uint8), "RGBA"))


def recolor_material_body(image: Image.Image) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.float32).copy()
    alpha = rgba[:, :, 3] / 255.0
    lum = (rgba[:, :, :3] @ np.array([0.2126, 0.7152, 0.0722], dtype=np.float32)) / 255.0
    charcoal = np.array([92.0, 88.0, 81.0], dtype=np.float32)
    paper = np.array([184.0, 169.0, 143.0], dtype=np.float32)
    mix = np.clip((lum - 0.18) / 0.62, 0.0, 1.0)[:, :, None]
    rgba[:, :, :3] = charcoal[None, None, :] * (1.0 - mix) + paper[None, None, :] * mix
    alpha = np.clip(np.power(alpha, 1.12) * 1.08, 0.0, 1.0)
    alpha[alpha < 0.016] = 0.0
    rgba[:, :, 3] = alpha * 255.0
    rgba[:, :, :3][alpha <= 0] = 0
    return sanitize_straight_alpha(Image.fromarray(rgba.astype(np.uint8), "RGBA"))


def alpha_multiply(image: Image.Image, factor: float) -> Image.Image:
    result = image.convert("RGBA").copy()
    alpha = np.asarray(result.getchannel("A"), dtype=np.float32)
    result.putalpha(Image.fromarray(np.clip(alpha * factor, 0, 255).astype(np.uint8), "L"))
    return result


def anchored_paste(
    canvas: Image.Image,
    source: Image.Image,
    width: float,
    angle: float,
    opacity: float,
    origin: tuple[float, float] = ORIGIN,
) -> None:
    target_w = max(4, int(round(width)))
    target_h = max(4, int(round(target_w * source.height / max(1, source.width))))
    config_limit = 326
    if target_h > config_limit:
        target_h = config_limit
    resized = source.resize((target_w, target_h), Image.Resampling.LANCZOS)
    rotated = resized.rotate(angle, resample=Image.Resampling.BICUBIC, expand=True)
    rotated = alpha_multiply(rotated, max(0.0, min(1.0, opacity)))
    radians = math.radians(angle)
    center_x = origin[0] + math.cos(radians) * target_w * 0.5
    center_y = origin[1] + math.sin(radians) * target_w * 0.5
    canvas.alpha_composite(
        rotated,
        (int(round(center_x - rotated.width * 0.5)), int(round(center_y - rotated.height * 0.5))),
    )


def ease_out_cubic(value: float) -> float:
    value = max(0.0, min(1.0, value))
    return 1.0 - (1.0 - value) ** 3


def sampled_curve(frame: int, values: tuple[float, ...]) -> float:
    if len(values) != FRAME_COUNT:
        raise ValueError("Envelope table must contain exactly sixteen samples")
    return values[max(0, min(FRAME_COUNT - 1, frame))]


CONTACT_ENV = (0.10, 0.72, 1.00, 0.84, 0.55, 0.30, 0.16, 0.09, 0.055, 0.038, 0.028, 0.021, 0.016, 0.012, 0.009, 0.006)
BODY_ENV = (0.045, 0.46, 0.92, 1.00, 0.91, 0.78, 0.63, 0.48, 0.35, 0.25, 0.17, 0.115, 0.075, 0.048, 0.030, 0.018)
ECHO_ENV = (0.018, 0.12, 0.53, 0.82, 1.00, 0.90, 0.74, 0.57, 0.41, 0.29, 0.20, 0.135, 0.088, 0.055, 0.032, 0.018)
SETTLE_ENV = (0.040, 0.10, 0.24, 0.48, 0.72, 0.90, 1.00, 0.94, 0.83, 0.70, 0.56, 0.43, 0.31, 0.21, 0.13, 0.075)


def draw_contact(
    canvas: Image.Image,
    intensity: str,
    variation: str,
    frame: int,
) -> None:
    env = sampled_curve(frame, CONTACT_ENV)
    angle = ANGLE_BY_VARIATION[variation]
    direction = np.array([math.cos(math.radians(angle)), math.sin(math.radians(angle))])
    normal = np.array([-direction[1], direction[0]])
    config = CONFIGS[intensity]
    growth = 0.48 + 0.52 * ease_out_cubic(min(1.0, frame / 2.4))
    scale = 2
    large = Image.new("RGBA", (CELL * scale, CELL * scale), (0, 0, 0, 0))
    sharp = ImageDraw.Draw(large, "RGBA")
    origin = np.array(ORIGIN)
    length = (24.0 + config.ray_count * 2.2) * growth
    half = (5.0 + config.ray_count * 0.55) * growth
    lens = [
        origin - direction * 3.5,
        origin + normal * half,
        origin + direction * length,
        origin - normal * half,
    ]
    alpha = int(178 * env * config.brightness)
    sharp.polygon(
        [(int(p[0] * scale), int(p[1] * scale)) for p in lens],
        fill=(247, 232, 202, alpha),
    )
    offsets = (-1.0, 0.55, 1.65) if intensity != "Light" else (-1.15, 1.25)
    for index, side in enumerate(offsets):
        ray_len = length * (1.20 + index * 0.18)
        end = origin + direction * ray_len + normal * side * half * 1.65
        sharp.line(
            [(int(origin[0] * scale), int(origin[1] * scale)), (int(end[0] * scale), int(end[1] * scale))],
            fill=(238, 218, 181, int((186 - index * 22) * env)),
            width=max(3, int((1.25 - index * 0.17) * scale)),
        )
    glow = large.filter(ImageFilter.GaussianBlur(radius=4.2 * scale))
    glow = alpha_multiply(glow, 0.22)
    canvas.alpha_composite(glow.resize((CELL, CELL), Image.Resampling.LANCZOS))
    canvas.alpha_composite(large.resize((CELL, CELL), Image.Resampling.LANCZOS))


def draw_echo(
    canvas: Image.Image,
    intensity: str,
    variation: str,
    frame: int,
) -> None:
    env = sampled_curve(frame, ECHO_ENV)
    scale = 2
    large = Image.new("RGBA", (CELL * scale, CELL * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(large, "RGBA")
    angle = ANGLE_BY_VARIATION[variation]
    local = max(0.0, (frame - 1.0) / 14.0)
    radius_x = (30 + CONFIGS[intensity].ray_count * 3.6) * (0.62 + 0.72 * ease_out_cubic(local))
    radius_y = radius_x * (0.84 if intensity == "Light" else 1.02)
    cx = ORIGIN[0] + 3.0 + local * (11.0 if intensity == "Heavy" else 7.0)
    cy = ORIGIN[1]
    box = (
        int((cx - radius_x) * scale),
        int((cy - radius_y) * scale),
        int((cx + radius_x) * scale),
        int((cy + radius_y) * scale),
    )
    width = max(3, int((2.4 if intensity == "Heavy" else 1.7) * scale))
    base_alpha = int((124 if intensity == "Light" else 158 if intensity == "Medium" else 184) * env)
    # Broken right-facing pressure echo: two separated segments, never a full ring.
    for start, end in ((-61 + angle, -15 + angle), (17 + angle, 66 + angle)):
        draw.arc(box, start=start, end=end, fill=(169, 157, 139, base_alpha), width=width)
    if intensity != "Light":
        inset = int(9 * scale)
        inner = (box[0] + inset, box[1] + inset, box[2] - inset, box[3] - inset)
        draw.arc(
            inner,
            start=-37 + angle,
            end=5 + angle,
            fill=(132, 70, 52, int(base_alpha * 0.52)),
            width=max(2, int(width * 0.62)),
        )
    soft = large.filter(ImageFilter.GaussianBlur(radius=2.0 * scale))
    canvas.alpha_composite(alpha_multiply(soft, 0.28).resize((CELL, CELL), Image.Resampling.LANCZOS))
    canvas.alpha_composite(large.resize((CELL, CELL), Image.Resampling.LANCZOS))


def draw_settle(
    canvas: Image.Image,
    intensity: str,
    variation: str,
    frame: int,
) -> None:
    env = sampled_curve(frame, SETTLE_ENV)
    config = CONFIGS[intensity]
    seed = 5100 + tuple(CONFIGS).index(intensity) * 100 + VARIATIONS.index(variation) * 17
    rng = np.random.default_rng(seed)
    angle = math.radians(ANGLE_BY_VARIATION[variation])
    direction = np.array([math.cos(angle), math.sin(angle)])
    normal = np.array([-direction[1], direction[0]])
    q = ease_out_cubic(max(0.0, (frame - 1.0) / 14.0))
    scale = 2
    large = Image.new("RGBA", (CELL * scale, CELL * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(large, "RGBA")
    origin = np.array(ORIGIN)
    for index in range(config.settle_count):
        forward = rng.uniform(35, 92) + index * rng.uniform(8, 17)
        lateral = rng.uniform(-38, 38) + (index - config.settle_count * 0.5) * 3.5
        gravity = rng.uniform(34, 76) * q * q
        pos = origin + direction * forward * q + normal * lateral * q
        pos[1] += gravity
        length = rng.uniform(4.0, 10.0) * (1.0 + 0.18 * tuple(CONFIGS).index(intensity))
        theta = angle + rng.uniform(-0.7, 0.7) + q * rng.uniform(0.35, 1.05)
        delta = np.array([math.cos(theta), math.sin(theta)]) * length
        color = (183, 169, 143, int(rng.uniform(105, 178) * env))
        draw.line(
            [(int((pos[0] - delta[0]) * scale), int((pos[1] - delta[1]) * scale)),
             (int((pos[0] + delta[0]) * scale), int((pos[1] + delta[1]) * scale))],
            fill=color,
            width=max(2, int(rng.uniform(0.75, 1.45) * scale)),
        )
    # Thin dust compression close to the surface; no opaque cloud.
    haze = Image.new("RGBA", large.size, (0, 0, 0, 0))
    haze_draw = ImageDraw.Draw(haze, "RGBA")
    haze_w = (36 + config.settle_count * 5) * (0.45 + 0.80 * q)
    haze_h = (8 + config.settle_count * 1.7) * (0.65 + 0.52 * q)
    hx = ORIGIN[0] + 28 + 55 * q
    hy = ORIGIN[1] + 15 + 42 * q * q
    haze_draw.ellipse(
        ((hx - haze_w) * scale, (hy - haze_h) * scale, (hx + haze_w) * scale, (hy + haze_h) * scale),
        fill=(129, 123, 112, int(58 * env)),
    )
    haze = haze.filter(ImageFilter.GaussianBlur(radius=8 * scale))
    canvas.alpha_composite(haze.resize((CELL, CELL), Image.Resampling.LANCZOS))
    canvas.alpha_composite(large.resize((CELL, CELL), Image.Resampling.LANCZOS))


def make_layer_frames(
    intensity: str,
    variation: str,
    layer: str,
    peaks: dict[str, Image.Image],
    contact_compression: Image.Image,
    material_body: Image.Image,
) -> list[Image.Image]:
    config = CONFIGS[intensity]
    peak = peaks[intensity]
    angle = ANGLE_BY_VARIATION[variation]
    scale = SCALE_BY_VARIATION[variation]
    frames: list[Image.Image] = []
    for frame in range(FRAME_COUNT):
        canvas = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
        biased = max(0.0, min(15.0, frame + TIME_BIAS_BY_VARIATION[variation] * 15.0))
        if layer == "Contact":
            contact_env = sampled_curve(frame, CONTACT_ENV)
            contact_width = {"Light": 104.0, "Medium": 132.0, "Heavy": 158.0}[intensity]
            contact_growth = 0.58 + 0.44 * ease_out_cubic(min(1.0, biased / 2.5))
            anchored_paste(
                canvas,
                contact_compression,
                contact_width * contact_growth * scale,
                angle,
                contact_env * {"Light": 0.56, "Medium": 0.70, "Heavy": 0.82}[intensity],
            )
            draw_contact(canvas, intensity, variation, int(round(biased)))
        elif layer == "DirectionalBody":
            env = sampled_curve(frame, BODY_ENV) * config.body_alpha
            growth = 0.50 + 0.52 * ease_out_cubic(min(1.0, biased / 3.0))
            body_alpha = {"Light": 0.14, "Medium": 0.25, "Heavy": 0.38}[intensity]
            anchored_paste(
                canvas,
                material_body,
                config.width * scale * growth * {"Light": 0.72, "Medium": 0.82, "Heavy": 0.93}[intensity],
                angle + {"A": -1.5, "B": 1.8, "C": -2.2}[variation],
                env * body_alpha,
            )
            ray_alpha = {"Light": 0.92, "Medium": 0.76, "Heavy": 0.72}[intensity]
            anchored_paste(canvas, peak, config.width * scale * growth, angle, env * ray_alpha)
        elif layer == "HitStopEcho":
            draw_echo(canvas, intensity, variation, int(round(biased)))
        elif layer == "MaterialSettle":
            draw_settle(canvas, intensity, variation, int(round(biased)))
        else:
            raise KeyError(layer)
        result = sanitize_straight_alpha(canvas)
        if np.count_nonzero(np.asarray(result.getchannel("A"))) < 48:
            raise RuntimeError(f"Sparse frame: {intensity}/{variation}/{layer}/{frame}")
        frames.append(result)
    return frames


def assemble_flipbook(frames: list[Image.Image]) -> Image.Image:
    atlas = Image.new("RGBA", (ATLAS, ATLAS), (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        column = index % GRID
        chronological_row = index // GRID
        # Unity ParticleSystem WholeSheet indexes the visually top row first.
        # v004 reversed this row order, so Play sampled decay frames at impact.
        image_row = chronological_row
        atlas.alpha_composite(frame, (column * CELL, image_row * CELL))
    return atlas


def extract_frame(atlas: Image.Image, index: int) -> Image.Image:
    column = index % GRID
    chronological_row = index // GRID
    image_row = chronological_row
    return atlas.crop((column * CELL, image_row * CELL, (column + 1) * CELL, (image_row + 1) * CELL))


def composite_frame(layer_atlases: dict[str, Image.Image], index: int) -> Image.Image:
    result = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))
    for layer in ("MaterialSettle", "HitStopEcho", "DirectionalBody", "Contact"):
        result.alpha_composite(extract_frame(layer_atlases[layer], index))
    return result


def checkerboard(size: tuple[int, int], square: int = 24) -> Image.Image:
    result = Image.new("RGBA", size, (37, 40, 41, 255))
    draw = ImageDraw.Draw(result)
    for y in range(0, size[1], square):
        for x in range(0, size[0], square):
            if (x // square + y // square) % 2:
                draw.rectangle((x, y, x + square - 1, y + square - 1), fill=(55, 58, 59, 255))
    return result


def fit_inside(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    result = image.copy()
    result.thumbnail(size, Image.Resampling.LANCZOS)
    return result


def unity_truth_panel(effect: Image.Image, size: tuple[int, int], proxy: bool = True) -> Image.Image:
    panel = Image.new("RGBA", size, (17, 20, 21, 255))
    if proxy:
        draw = ImageDraw.Draw(panel, "RGBA")
        contact_x = int(size[0] * 0.18)
        draw.rounded_rectangle(
            (contact_x - 16, int(size[1] * 0.22), contact_x + 16, int(size[1] * 0.78)),
            radius=8,
            fill=(92, 95, 93, 255),
        )
    fitted = fit_inside(effect, (int(size[0] * 0.84), int(size[1] * 0.92)))
    x = int(size[0] * 0.18) - int(ORIGIN[0] * fitted.width / CELL)
    y = (size[1] - fitted.height) // 2
    panel.alpha_composite(fitted, (x, y))
    return panel


def build_unity_truth_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    sheet = Image.new("RGBA", (2400, 1440), (11, 13, 14, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((52, 28), "SHOWCASE HIT VFX v005  /  UNITY-LIKE STRAIGHT-ALPHA PEAK  /  NO SCENE COMPOSITE", font=font(42), fill=(234, 230, 216, 255))
    for row, intensity in enumerate(CONFIGS):
        for column, variation in enumerate(VARIATIONS):
            atlases = {layer: Image.open(outputs[(intensity, variation)][layer]).convert("RGBA") for layer in LAYER_NAMES}
            panel = unity_truth_panel(composite_frame(atlases, 3), (760, 410))
            x = 35 + column * 790
            y = 105 + row * 435
            sheet.alpha_composite(panel, (x, y))
            draw.text((x + 20, y + 16), f"{intensity.upper()}  {variation}", font=font(28), fill=(239, 225, 197, 255))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_UnityTruth_ABC_v005.png"
    save_png(sheet, path)
    return path


def concept_crop(image: Image.Image) -> Image.Image:
    return image.crop((770, 205, 1455, 665))


def build_reference_comparison(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    sheet = Image.new("RGBA", (2048, 1536), (13, 15, 16, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((48, 24), "OWNER-SELECTED DIRECTION vs v005 UNITY-LIKE PEAK", font=font(42), fill=(234, 230, 216, 255))
    draw.text((54, 82), "LEFT: direction reference crop    RIGHT: candidate on neutral review background", font=font(24), fill=(175, 173, 164, 255))
    for row, intensity in enumerate(CONFIGS):
        y = 132 + row * 455
        reference = ImageOps.fit(concept_crop(Image.open(CONCEPT_REFERENCES[intensity]).convert("RGBA")), (930, 408), method=Image.Resampling.LANCZOS)
        atlases = {layer: Image.open(outputs[(intensity, "A")][layer]).convert("RGBA") for layer in LAYER_NAMES}
        truth = unity_truth_panel(composite_frame(atlases, 3), (930, 408))
        sheet.alpha_composite(reference, (54, y))
        sheet.alpha_composite(truth, (1058, y))
        draw.text((70, y + 16), f"{intensity.upper()} REFERENCE", font=font(26), fill=(245, 236, 215, 255), stroke_width=2, stroke_fill=(10, 10, 10, 220))
        draw.text((1074, y + 16), f"{intensity.upper()} v005", font=font(26), fill=(245, 236, 215, 255))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_ReferenceComparison_v005.png"
    save_png(sheet, path)
    return path


def build_layer_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    sheet = Image.new("RGBA", (2560, 1536), (15, 17, 18, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((54, 28), "SHOWCASE HIT VFX v005  /  FOUR-LAYER BREAKDOWN  /  VARIATION A", font=font(42), fill=(232, 229, 216, 255))
    headers = ("CONTACT", "DIRECTIONAL BODY", "PRESSURE ECHO", "MATERIAL SETTLE", "COMPOSITE")
    for column, header in enumerate(headers):
        draw.text((66 + column * 500, 96), header, font=font(24), fill=(190, 184, 169, 255))
    for row, intensity in enumerate(CONFIGS):
        y0 = 150 + row * 450
        draw.text((42, y0 + 10), intensity.upper(), font=font(31), fill=(223, 207, 177, 255))
        atlases = {layer: Image.open(outputs[(intensity, "A")][layer]).convert("RGBA") for layer in LAYER_NAMES}
        images = [extract_frame(atlases[layer], 3) for layer in LAYER_NAMES] + [composite_frame(atlases, 3)]
        for column, item in enumerate(images):
            panel = checkerboard((470, 390), 20)
            fitted = fit_inside(item, (430, 350))
            panel.alpha_composite(fitted, ((470 - fitted.width) // 2, (390 - fitted.height) // 2))
            sheet.alpha_composite(panel, (42 + column * 500, y0 + 46))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_LayerBreakdown_v005.png"
    save_png(sheet, path)
    return path


def build_timing_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    indices = (0, 1, 2, 3, 5, 8, 11, 15)
    sheet = Image.new("RGBA", (2432, 1736), (14, 16, 17, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((48, 28), "SHOWCASE HIT VFX v005  /  16-FRAME TIMING AUDIT  /  VARIATION A", font=font(42), fill=(232, 228, 215, 255))
    for column, index in enumerate(indices):
        draw.text((68 + column * 300, 96), f"F{index:02d}", font=font(26), fill=(191, 181, 162, 255))
    for row, intensity in enumerate(CONFIGS):
        atlases = {layer: Image.open(outputs[(intensity, "A")][layer]).convert("RGBA") for layer in LAYER_NAMES}
        y0 = 142 + row * 520
        draw.text((42, y0 + 14), intensity.upper(), font=font(31), fill=(226, 210, 180, 255))
        for column, index in enumerate(indices):
            panel = checkerboard((280, 430), 20)
            item = fit_inside(composite_frame(atlases, index), (260, 390))
            panel.alpha_composite(item, ((280 - item.width) // 2, (430 - item.height) // 2))
            sheet.alpha_composite(panel, (42 + column * 300, y0 + 54))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_TimingAudit_v005.png"
    save_png(sheet, path)
    return path


def build_alpha_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    sheet = Image.new("RGBA", (2304, 2304), (15, 17, 18, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((46, 26), "SHOWCASE HIT VFX v005  /  GRAYSCALE + ALPHA READABILITY", font=font(40), fill=(230, 226, 214, 255))
    for index, (intensity, variation) in enumerate((i, v) for i in CONFIGS for v in VARIATIONS):
        row, column = divmod(index, 3)
        x0, y0 = 28 + column * 758, 94 + row * 728
        atlases = {layer: Image.open(outputs[(intensity, variation)][layer]).convert("RGBA") for layer in LAYER_NAMES}
        composite = composite_frame(atlases, 3)
        rgba = np.asarray(composite, dtype=np.uint8)
        lum = (rgba[:, :, 0] * 0.2126 + rgba[:, :, 1] * 0.7152 + rgba[:, :, 2] * 0.0722).astype(np.uint8)
        gray = Image.fromarray(np.dstack((lum, lum, lum, rgba[:, :, 3])), "RGBA")
        alpha = composite.getchannel("A")
        alpha_view = Image.merge("RGBA", (alpha, alpha, alpha, Image.new("L", alpha.size, 255)))
        panel = checkerboard((720, 660), 20)
        gray_fit = fit_inside(gray, (680, 450))
        alpha_fit = fit_inside(alpha_view, (330, 180))
        panel.alpha_composite(gray_fit, ((720 - gray_fit.width) // 2, 36))
        panel.alpha_composite(alpha_fit, ((720 - alpha_fit.width) // 2, 470))
        sheet.alpha_composite(panel, (x0, y0))
        draw.text((x0 + 16, y0 + 12), f"{intensity.upper()} {variation}", font=font(26), fill=(233, 222, 199, 255))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_GrayscaleAlpha_v005.png"
    save_png(sheet, path)
    return path


def build_animated_previews(outputs: dict[tuple[str, str], dict[str, Path]]) -> list[Path]:
    paths: list[Path] = []
    for intensity in CONFIGS:
        frames: list[Image.Image] = []
        for variation in VARIATIONS:
            atlases = {layer: Image.open(outputs[(intensity, variation)][layer]).convert("RGBA") for layer in LAYER_NAMES}
            for frame_index in range(FRAME_COUNT):
                frame = unity_truth_panel(composite_frame(atlases, frame_index), (960, 540))
                draw = ImageDraw.Draw(frame)
                draw.rounded_rectangle((28, 24, 390, 86), radius=12, fill=(8, 10, 11, 205), outline=(141, 118, 89, 210), width=2)
                draw.text((48, 37), f"{intensity.upper()}  VARIATION {variation}  /  UNITY-LIKE", font=font(26), fill=(242, 232, 210, 255))
                frames.append(frame.convert("P", palette=Image.Palette.ADAPTIVE, colors=128))
            for _ in range(7):
                hold = unity_truth_panel(Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0)), (960, 540))
                frames.append(hold.convert("P", palette=Image.Palette.ADAPTIVE, colors=128))
        path = PREVIEW_DIR / f"VFX_Showcase_HitProduction_{intensity}_ABC_UnityLike_v005.gif"
        path.parent.mkdir(parents=True, exist_ok=True)
        frames[0].save(path, save_all=True, append_images=frames[1:], duration=38, loop=0, disposal=2, optimize=False)
        paths.append(path)
    return paths


def transparent_margin(image: Image.Image) -> int:
    alpha = np.asarray(image.convert("RGBA").getchannel("A"))
    ys, xs = np.where(alpha > 0)
    if len(xs) == 0:
        return min(image.size)
    return int(min(xs.min(), image.width - 1 - xs.max(), ys.min(), image.height - 1 - ys.max()))


def validate_candidate(path: Path) -> dict[str, object]:
    with Image.open(path) as image:
        image = image.convert("RGBA")
        if image.size != (ATLAS, ATLAS):
            raise RuntimeError(f"Wrong dimensions: {relative(path)}={image.size}")
        hashes: list[str] = []
        counts: list[int] = []
        partial = magenta = green = cyan = black_halo = 0
        margins: list[int] = []
        for index in range(FRAME_COUNT):
            frame = extract_frame(image, index)
            rgba = np.asarray(frame, dtype=np.uint8)
            alpha = rgba[:, :, 3]
            count = int(np.count_nonzero(alpha))
            if count < 48:
                raise RuntimeError(f"Empty/sparse frame: {relative(path)} frame={index}")
            margin = transparent_margin(frame)
            if margin < MIN_MARGIN:
                raise RuntimeError(f"Frame margin failure: {relative(path)} frame={index} margin={margin}")
            hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
            counts.append(count)
            margins.append(margin)
            active = alpha > 8
            partial += int(np.count_nonzero((alpha > 0) & (alpha < 255)))
            magenta += int(np.count_nonzero(active & (rgba[:, :, 0] > 220) & (rgba[:, :, 1] < 60) & (rgba[:, :, 2] > 190)))
            green += int(np.count_nonzero(active & (rgba[:, :, 1] > rgba[:, :, 0] * 1.45) & (rgba[:, :, 1] > rgba[:, :, 2] * 1.45)))
            cyan += int(np.count_nonzero(active & (rgba[:, :, 0] < 70) & (rgba[:, :, 1] > 200) & (rgba[:, :, 2] > 200)))
            black_halo += int(np.count_nonzero((alpha > 0) & (alpha < 64) & (np.max(rgba[:, :, :3], axis=2) < 5)))
        if len(set(hashes)) != FRAME_COUNT:
            raise RuntimeError(f"Duplicate frames: {relative(path)}")
        if any((magenta, green, cyan, black_halo)):
            raise RuntimeError(f"Palette/alpha risk: {relative(path)} magenta={magenta} green={green} cyan={cyan} blackHalo={black_halo}")
        return {
            "path": relative(path),
            "dimensions": f"{ATLAS}x{ATLAS}",
            "mode": "RGBA",
            "frame_order": "UNITY_WHOLE_SHEET_TOP_LEFT_ROW_MAJOR",
            "frame_hash_unique": len(set(hashes)),
            "frame_alpha_counts": ";".join(str(value) for value in counts),
            "alpha_partial_pixels": partial,
            "transparent_margin_min_px": min(margins),
            "sha256": sha256(path),
            "bytes": path.stat().st_size,
        }


def write_ledger(paths: list[Path], metrics: dict[str, dict[str, object]]) -> Path:
    ledger = LICENSE_DIR / "VFX_Showcase_HitProduction_Provenance_Ledger_v005.csv"
    fields = [
        "asset_path", "asset_role", "provenance_classification", "creator_or_generator",
        "source_prompt_id", "source_inputs", "third_party_source", "rights_basis",
        "commercial_use", "repository_redistribution", "dimensions", "mode", "frame_order",
        "frame_hash_unique", "alpha_partial_pixels", "transparent_margin_min_px", "sha256",
        "bytes", "review_status",
    ]
    rows: list[dict[str, object]] = []
    for path in paths:
        record = metrics.get(relative(path), {})
        if path in IMAGEGEN_SOURCES.values():
            intensity = next(key for key, value in IMAGEGEN_SOURCES.items() if value == path)
            role = "PRESERVED_IMAGEGEN_PEAK_SOURCE"
            classification = "ORIGINAL_AI_GENERATED_SOURCE"
            creator = "OpenAI built-in image generation; owner-selected project concept used as direction reference"
            prompt_id = PROMPT_IDS[intensity]
            inputs = relative(CONCEPT_REFERENCES[intensity])
        elif path in ALPHA_SOURCES.values():
            intensity = next(key for key, value in ALPHA_SOURCES.items() if value == path)
            role = "STRAIGHT_ALPHA_PEAK_SOURCE"
            classification = "ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE"
            creator = "Imagegen skill chroma-key removal: auto-key border, soft matte, despill, edge-contract 1"
            prompt_id = PROMPT_IDS[intensity]
            inputs = relative(IMAGEGEN_SOURCES[intensity])
        elif path in AUX_IMAGEGEN_SOURCES.values():
            source_role = next(key for key, value in AUX_IMAGEGEN_SOURCES.items() if value == path)
            role = "PRESERVED_IMAGEGEN_SECONDARY_LAYER_SOURCE"
            classification = "ORIGINAL_AI_GENERATED_SOURCE"
            creator = "OpenAI built-in image generation; owner-selected project concept used as direction reference"
            prompt_id = AUX_PROMPT_IDS[source_role]
            inputs = relative(CONCEPT_REFERENCES["Medium" if source_role == "ContactCompression" else "Heavy"])
        elif path in AUX_ALPHA_SOURCES.values():
            source_role = next(key for key, value in AUX_ALPHA_SOURCES.items() if value == path)
            role = "STRAIGHT_ALPHA_SECONDARY_LAYER_SOURCE"
            classification = "ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE"
            creator = "Imagegen skill chroma-key removal: auto-key border, soft matte, despill, edge-contract 1"
            prompt_id = AUX_PROMPT_IDS[source_role]
            inputs = relative(AUX_IMAGEGEN_SOURCES[source_role])
        elif path.parent == FLIPBOOK_DIR:
            role = "UNITY_REVIEW_4X4_FLIPBOOK_LAYER_CANDIDATE"
            classification = "ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE"
            creator = "Codex-authored deterministic 16-frame layered assembly"
            prompt_id = "IG-VFX-HIT-005-LIGHT/MEDIUM/HEAVY"
            inputs = ";".join(
                relative(value)
                for value in (*ALPHA_SOURCES.values(), *AUX_ALPHA_SOURCES.values())
            ) + ";" + relative(SCRIPT_PATH)
        elif path.parent == UNITY_FLIPBOOK_DIR:
            role = "UNITY_REVIEW_BYTE_IDENTICAL_COPY"
            classification = "BYTE_IDENTICAL_PROJECT_DELIVERY_COPY"
            creator = "Deterministic copy from ArtSource candidate"
            prompt_id = "IG-VFX-HIT-005-LIGHT/MEDIUM/HEAVY"
            inputs = relative(FLIPBOOK_DIR / path.name)
        elif path in CONCEPT_REFERENCES.values() or path == BASE_SCENE:
            role = "OWNER_SELECTED_DIRECTION_REFERENCE"
            classification = "ORIGINAL_AI_GENERATED_PROJECT_REFERENCE"
            creator = "OpenAI built-in image generation during Human Owner concept review"
            prompt_id = "OWNER SELECTED 3/5/10 DIRECTION"
            inputs = "NONE" if path == BASE_SCENE else relative(BASE_SCENE)
        elif path.suffix.lower() in (".png", ".gif"):
            role = "HUMAN_OWNER_REVIEW_PREVIEW"
            classification = "DETERMINISTIC_REVIEW_COMPOSITE"
            creator = "Codex-authored neutral-background review compositor"
            prompt_id = "NONE"
            inputs = "HIT PRODUCTION v005 CANDIDATES"
        else:
            role = "REPRODUCIBLE_BUILD_SOURCE"
            classification = "ORIGINAL_DETERMINISTIC_SOURCE"
            creator = "Codex"
            prompt_id = "NONE"
            inputs = "NONE"
        dimensions = record.get("dimensions", "N/A")
        mode = record.get("mode", "BINARY OR UTF-8 SOURCE")
        if path.suffix.lower() in (".png", ".gif") and dimensions == "N/A":
            with Image.open(path) as image:
                dimensions = f"{image.width}x{image.height}"
                mode = image.mode
        rows.append({
            "asset_path": relative(path),
            "asset_role": role,
            "provenance_classification": classification,
            "creator_or_generator": creator,
            "source_prompt_id": prompt_id,
            "source_inputs": inputs,
            "third_party_source": "NONE",
            "rights_basis": "PROJECT ORIGINAL; NO THIRD-PARTY SOURCE; HUMAN OWNER AUTHORIZED CREATION",
            "commercial_use": "AUTHORIZED FOR BREAK NOTICE PROJECT AND CONTEST MATERIALS BY HUMAN OWNER",
            "repository_redistribution": "AUTHORIZED WITH PROJECT DELIVERY; NO EXTERNAL SOURCE RESTRICTION",
            "dimensions": dimensions,
            "mode": mode,
            "frame_order": record.get("frame_order", "N/A"),
            "frame_hash_unique": record.get("frame_hash_unique", "N/A"),
            "alpha_partial_pixels": record.get("alpha_partial_pixels", "N/A"),
            "transparent_margin_min_px": record.get("transparent_margin_min_px", "N/A"),
            "sha256": sha256(path),
            "bytes": path.stat().st_size,
            "review_status": "REVIEW / OWNER APPROVAL PENDING / GAMEPLAY INTEGRATION ELIGIBILITY NONE",
        })
    ledger.parent.mkdir(parents=True, exist_ok=True)
    with ledger.open("w", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, quoting=csv.QUOTE_ALL, lineterminator="\n")
        writer.writeheader()
        writer.writerows(rows)
    return ledger


def main() -> None:
    required = (
        list(IMAGEGEN_SOURCES.values()) + list(ALPHA_SOURCES.values()) +
        list(AUX_IMAGEGEN_SOURCES.values()) + list(AUX_ALPHA_SOURCES.values()) +
        list(CONCEPT_REFERENCES.values()) + [BASE_SCENE]
    )
    missing = [path for path in required if not path.is_file()]
    if missing:
        raise FileNotFoundError(", ".join(str(path) for path in missing))
    for directory in (FLIPBOOK_DIR, PREVIEW_DIR, LICENSE_DIR, UNITY_FLIPBOOK_DIR):
        directory.mkdir(parents=True, exist_ok=True)

    peaks = {key: recolor_peak(trim_alpha(Image.open(path)), key) for key, path in ALPHA_SOURCES.items()}
    contact_compression = recolor_peak(
        trim_alpha(Image.open(AUX_ALPHA_SOURCES["ContactCompression"])),
        "Medium",
    )
    material_body = recolor_material_body(
        trim_alpha(Image.open(AUX_ALPHA_SOURCES["MaterialBody"])),
    )
    outputs: dict[tuple[str, str], dict[str, Path]] = {}
    candidates: list[Path] = []
    for intensity in CONFIGS:
        for variation in VARIATIONS:
            key = (intensity, variation)
            outputs[key] = {}
            for layer in LAYER_NAMES:
                filename = f"T_VFX_Showcase_Hit{intensity}_{variation}_{layer}_4x4_v005.png"
                path = FLIPBOOK_DIR / filename
                frames = make_layer_frames(
                    intensity,
                    variation,
                    layer,
                    peaks,
                    contact_compression,
                    material_body,
                )
                save_png(assemble_flipbook(frames), path)
                outputs[key][layer] = path
                candidates.append(path)

    validation: dict[str, dict[str, object]] = {}
    for path in candidates:
        validation[relative(path)] = validate_candidate(path)
        unity_path = UNITY_FLIPBOOK_DIR / path.name
        unity_path.write_bytes(path.read_bytes())
        if sha256(unity_path) != sha256(path):
            raise RuntimeError(f"Unity candidate copy is not byte-identical: {path.name}")

    previews = [
        build_unity_truth_sheet(outputs),
        build_reference_comparison(outputs),
        build_layer_sheet(outputs),
        build_timing_sheet(outputs),
        build_alpha_sheet(outputs),
        *build_animated_previews(outputs),
    ]
    unity_capture = PREVIEW_DIR / "VFX_Showcase_HitProduction_UnityActualPlay_v005.png"
    if unity_capture.is_file():
        previews.append(unity_capture)
    provenance_paths = (
        list(IMAGEGEN_SOURCES.values()) + list(ALPHA_SOURCES.values()) +
        list(AUX_IMAGEGEN_SOURCES.values()) + list(AUX_ALPHA_SOURCES.values()) +
        list(CONCEPT_REFERENCES.values()) + [BASE_SCENE] + candidates +
        [UNITY_FLIPBOOK_DIR / path.name for path in candidates] + previews + [SCRIPT_PATH]
    )
    ledger = write_ledger(provenance_paths, validation)
    print(json.dumps({
        "status": "PASS",
        "review_status": "REVIEW / OWNER APPROVAL PENDING",
        "flipbooks": len(candidates),
        "frames": len(candidates) * FRAME_COUNT,
        "unity_byte_identical": len(candidates),
        "preview_count": len(previews),
        "total_candidate_bytes": sum(path.stat().st_size for path in candidates),
        "ledger": relative(ledger),
        "previews": [relative(path) for path in previews],
    }, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
