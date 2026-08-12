"""Build the BREAK NOTICE Showcase hit-VFX production pack v004.

The pack is deliberately review-only. It creates self-authored, straight-alpha
4x4 flipbooks and large Human Owner previews, then copies only byte-identical
PNG candidates into the isolated Unity review area. It does not create gameplay
hooks, damage, breakage, shaders, packages, or production-scene references.
"""

from __future__ import annotations

import csv
import hashlib
import json
import math
from dataclasses import dataclass
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps


CELL = 512
GRID = 4
ATLAS = CELL * GRID
MIN_MARGIN = 24
FRAME_COUNT = 16

SCRIPT_PATH = Path(__file__).resolve()
SOURCE_DIR = SCRIPT_PATH.parent
SHOWCASE_DIR = SOURCE_DIR.parents[1]
REPO_ROOT = SHOWCASE_DIR.parents[2]
FLIPBOOK_DIR = SHOWCASE_DIR / "Flipbooks" / "HitProduction_v004"
PREVIEW_DIR = SHOWCASE_DIR / "Preview" / "HitProduction_v004"
LICENSE_DIR = SHOWCASE_DIR / "Licenses"
UNITY_ROOT = REPO_ROOT / "Game" / "Assets" / "_Project" / "Art" / "VFX" / "Showcase"
UNITY_FLIPBOOK_DIR = UNITY_ROOT / "HitProduction_v004" / "Flipbooks"

DIRECTIONAL_BANK = SOURCE_DIR / "VFX_Showcase_HitDirectionalBank_AlphaExtract_v004.png"
CONTACT_BANK = SOURCE_DIR / "VFX_Showcase_HitContactBank_AlphaExtract_v004.png"
ECHO_BANK = SOURCE_DIR / "VFX_Showcase_HitEchoBank_AlphaExtract_v004.png"
MATERIAL_BANK = SOURCE_DIR / "VFX_Showcase_HitMaterialBank_AlphaExtract_v004.png"
IMAGEGEN_BANKS = [
    SOURCE_DIR / "VFX_Showcase_HitDirectionalBank_ImageGen_v004.png",
    SOURCE_DIR / "VFX_Showcase_HitContactBank_ImageGen_v004.png",
    SOURCE_DIR / "VFX_Showcase_HitEchoBank_ImageGen_v004.png",
    SOURCE_DIR / "VFX_Showcase_HitMaterialBank_ImageGen_v004.png",
]
ALPHA_BANKS = [DIRECTIONAL_BANK, CONTACT_BANK, ECHO_BANK, MATERIAL_BANK]

CONCEPT_DIR = PREVIEW_DIR / "ConceptReferences"
CONCEPT_REFERENCES = [
    CONCEPT_DIR / "VFX_Showcase_Concept_BaseScene_v004.png",
    CONCEPT_DIR / "VFX_Showcase_Concept_Light_Directional_v004.png",
    CONCEPT_DIR / "VFX_Showcase_Concept_Medium_ContactEcho_v004.png",
    CONCEPT_DIR / "VFX_Showcase_Concept_Heavy_Layered_v004.png",
    CONCEPT_DIR / "VFX_Showcase_Concept_Heavy_Hybrid_v004.png",
]

LAYER_NAMES = ("Contact", "DirectionalBody", "HitStopEcho", "MaterialSettle")
VARIATIONS = ("A", "B", "C")


@dataclass(frozen=True)
class HitConfig:
    name: str
    body_width: int
    contact_width: int
    echo_width: int
    settle_count: int
    brightness: float
    body_opacity: float
    echo_opacity: float
    preview_scale: float


CONFIGS = {
    "Light": HitConfig("Light", 270, 118, 230, 3, 0.78, 0.72, 0.38, 0.72),
    "Medium": HitConfig("Medium", 330, 148, 285, 5, 0.91, 0.82, 0.54, 0.84),
    "Heavy": HitConfig("Heavy", 382, 184, 350, 7, 1.00, 0.92, 0.68, 0.96),
}

ANGLE_BY_VARIATION = {"A": 0.0, "B": -7.0, "C": 8.0}
TIME_BIAS_BY_VARIATION = {"A": 0.0, "B": 0.028, "C": -0.022}

SOURCE_SELECTIONS = {
    "Light": {
        "A": (0, 0, 5),
        "B": (5, 5, 7),
        "C": (8, 7, 12),
    },
    "Medium": {
        "A": (1, 2, 6),
        "B": (6, 3, 11),
        "C": (12, 8, 13),
    },
    "Heavy": {
        "A": (2, 1, 10),
        "B": (7, 6, 14),
        "C": (15, 9, 15),
    },
}

MATERIAL_SELECTIONS = {
    "Light": {"A": (0, 4, 7), "B": (2, 10, 13), "C": (5, 14, 19)},
    "Medium": {"A": (0, 3, 7, 10, 13), "B": (2, 6, 11, 14, 15), "C": (5, 9, 12, 16, 19)},
    "Heavy": {"A": (0, 3, 6, 7, 10, 13, 15), "B": (2, 4, 6, 9, 11, 14, 17), "C": (5, 7, 10, 12, 16, 18, 19)},
}

PROMPTS = {
    "IG-VFX-HIT-004-A": "Directional impact blade and speed-streak bank; built-in image generation; approved 3/5/10 direction references; flat #00FF00 chroma key.",
    "IG-VFX-HIT-004-B": "Compact contact transient and micro-fragment bank; built-in image generation; approved 3/5/10 direction references; flat #00FF00 chroma key.",
    "IG-VFX-HIT-004-C": "Restrained iron-red hit-stop echo bank; built-in image generation; approved 3/5/10 direction references; flat #00FF00 chroma key.",
    "IG-VFX-HIT-004-D": "Office material-settle bank; built-in image generation; approved 3/5/10 direction references; flat #00FF00 chroma key.",
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
    image.convert("RGBA").save(path, format="PNG", optimize=True, compress_level=9)


def trim_alpha(image: Image.Image, threshold: int = 3) -> Image.Image:
    rgba = image.convert("RGBA")
    alpha = np.asarray(rgba.getchannel("A"))
    ys, xs = np.where(alpha > threshold)
    if len(xs) == 0:
        raise ValueError("Source-bank cell is empty after alpha trim")
    return rgba.crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def split_bank(path: Path, columns: int, rows: int) -> list[Image.Image]:
    image = Image.open(path).convert("RGBA")
    cell_w = image.width / columns
    cell_h = image.height / rows
    result: list[Image.Image] = []
    for row in range(rows):
        for column in range(columns):
            x0 = int(round(column * cell_w + cell_w * 0.025))
            y0 = int(round(row * cell_h + cell_h * 0.025))
            x1 = int(round((column + 1) * cell_w - cell_w * 0.025))
            y1 = int(round((row + 1) * cell_h - cell_h * 0.025))
            result.append(trim_alpha(image.crop((x0, y0, x1, y1))))
    return result


def alpha_multiply(image: Image.Image, factor: float) -> Image.Image:
    rgba = image.convert("RGBA").copy()
    alpha = np.asarray(rgba.getchannel("A"), dtype=np.float32)
    rgba.putalpha(Image.fromarray(np.clip(alpha * factor, 0, 255).astype(np.uint8), "L"))
    return rgba


def recolor(image: Image.Image, tint: tuple[int, int, int], gain: float, contrast: float = 1.0) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.float32).copy()
    luminance = rgba[:, :, :3] @ np.array([0.2126, 0.7152, 0.0722], dtype=np.float32)
    luminance = np.clip((luminance - 96.0) * contrast + 156.0, 8.0, 255.0) / 255.0
    tint_array = np.array(tint, dtype=np.float32)
    rgba[:, :, :3] = np.clip(luminance[:, :, None] * tint_array[None, None, :] * gain, 0, 255)
    active = rgba[:, :, 3] > 0
    rgba[:, :, 0][~active] = 0
    rgba[:, :, 1][~active] = 0
    rgba[:, :, 2][~active] = 0
    return Image.fromarray(rgba.astype(np.uint8), "RGBA")


def sanitize_straight_alpha(image: Image.Image) -> Image.Image:
    """Remove key spill and unsafe dark RGB from antialiased active pixels."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    alpha = rgba[:, :, 3]
    active = alpha > 0
    red = rgba[:, :, 0].astype(np.int16)
    green = rgba[:, :, 1].astype(np.int16)
    blue = rgba[:, :, 2].astype(np.int16)
    green_spill = active & (green > red * 1.35) & (green > blue * 1.35)
    neutral = ((red + blue) // 2).astype(np.uint8)
    rgba[:, :, 1][green_spill] = neutral[green_spill]
    unsafe_dark = active & (np.max(rgba[:, :, :3], axis=2) < 8)
    rgba[:, :, 0][unsafe_dark] = 12
    rgba[:, :, 1][unsafe_dark] = 12
    rgba[:, :, 2][unsafe_dark] = 12
    # Neutralize isolated key-color quantization outliers, including alpha=1
    # pixels that are invisible in practice but trip missing-texture audits.
    magenta_outlier = active & (red >= 240) & (green <= 16) & (blue >= 240)
    rgba[:, :, 0][magenta_outlier] = 196
    rgba[:, :, 1][magenta_outlier] = 182
    rgba[:, :, 2][magenta_outlier] = 158
    rgba[:, :, 0][~active] = 0
    rgba[:, :, 1][~active] = 0
    rgba[:, :, 2][~active] = 0
    return Image.fromarray(rgba, "RGBA")


def paste_anchored(
    canvas: Image.Image,
    piece: Image.Image,
    origin: tuple[float, float],
    width: float,
    angle: float,
    opacity: float,
    mirror: bool = False,
) -> None:
    source = ImageOps.mirror(piece) if mirror else piece
    target_w = max(3, int(round(width)))
    target_h = max(3, int(round(target_w * source.height / max(1, source.width))))
    target_h = min(target_h, 330)
    resized = source.resize((target_w, target_h), Image.Resampling.LANCZOS)
    rotated = resized.rotate(angle, resample=Image.Resampling.BICUBIC, expand=True)
    rotated = alpha_multiply(rotated, max(0.0, min(1.0, opacity)))
    radians = math.radians(angle)
    cx = origin[0] + math.cos(radians) * target_w * 0.5
    cy = origin[1] + math.sin(radians) * target_w * 0.5
    canvas.alpha_composite(rotated, (int(round(cx - rotated.width / 2)), int(round(cy - rotated.height / 2))))


def draw_contact_core(canvas: Image.Image, config: HitConfig, frame: int, angle: float) -> None:
    scale = 4
    large = Image.new("RGBA", (CELL * scale, CELL * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(large, "RGBA")
    t = frame / (FRAME_COUNT - 1)
    peak = math.exp(-((t - 0.10) / 0.14) ** 2)
    fade = max(0.03, 1.0 - t) * peak
    origin = np.array([104.0, 256.0])
    direction = np.array([math.cos(math.radians(angle)), math.sin(math.radians(angle))])
    normal = np.array([-direction[1], direction[0]])
    length = (22.0 + config.contact_width * 0.25) * (0.35 + 0.65 * peak)
    half = (9.0 + config.contact_width * 0.055) * (0.45 + 0.55 * peak)
    points = [
        origin - direction * 5,
        origin + normal * half,
        origin + direction * length,
        origin - normal * half,
    ]
    rgba = (242, 228, 199, int(235 * fade * config.brightness))
    draw.polygon([(int(p[0] * scale), int(p[1] * scale)) for p in points], fill=rgba)
    for side in (-1, 1):
        end = origin + direction * (length * 1.45) + normal * side * (half * 2.0)
        draw.line(
            [(int(origin[0] * scale), int(origin[1] * scale)), (int(end[0] * scale), int(end[1] * scale))],
            fill=(231, 211, 174, int(185 * fade)),
            width=max(2, int(1.7 * scale)),
        )
    small = large.resize((CELL, CELL), Image.Resampling.LANCZOS)
    canvas.alpha_composite(small)


def envelope(t: float, attack: float, hold: float, release: float, floor: float = 0.015) -> float:
    if t < attack:
        return max(floor, t / max(attack, 0.001))
    if t < hold:
        return 1.0
    linear_release = 1.0 - (t - hold) / max(release - hold, 0.001)
    # Keep the residue alive without freezing several final cells to the same
    # quantized alpha. The small decaying tail is visible as settle, not as a
    # held sprite, and guarantees that all 16 authored frames remain distinct.
    decaying_tail = floor + 0.09 * max(0.0, 1.0 - t)
    return max(decaying_tail, linear_release)


def make_layer_frames(
    intensity: str,
    variation: str,
    layer: str,
    directional: list[Image.Image],
    contacts: list[Image.Image],
    echoes: list[Image.Image],
    materials: list[Image.Image],
) -> list[Image.Image]:
    config = CONFIGS[intensity]
    direction_index, contact_index, echo_index = SOURCE_SELECTIONS[intensity][variation]
    angle = ANGLE_BY_VARIATION[variation]
    origin = (104.0, 256.0)
    frames: list[Image.Image] = []

    body_piece = recolor(directional[direction_index], (235, 225, 204), config.brightness, 1.02)
    contact_piece = recolor(contacts[contact_index], (239, 224, 194), config.brightness, 1.12)
    echo_piece = recolor(echoes[echo_index], (126, 43, 31), 0.82, 1.20)
    material_pieces = [
        recolor(materials[index], (181, 170, 148) if index not in (9, 15) else (78, 75, 68), 0.92, 1.08)
        for index in MATERIAL_SELECTIONS[intensity][variation]
    ]

    for frame in range(FRAME_COUNT):
        t = frame / (FRAME_COUNT - 1)
        biased_t = max(0.0, min(1.0, t + TIME_BIAS_BY_VARIATION[variation]))
        canvas = Image.new("RGBA", (CELL, CELL), (0, 0, 0, 0))

        if layer == "Contact":
            env = envelope(biased_t, 0.055, 0.14, 0.52, 0.025)
            growth = 0.26 + 0.74 * min(1.0, biased_t / 0.18)
            paste_anchored(canvas, contact_piece, origin, config.contact_width * growth, angle, env * 0.92)
            draw_contact_core(canvas, config, frame, angle)

        elif layer == "DirectionalBody":
            env = envelope(biased_t, 0.08, 0.24, 0.76, 0.025)
            growth = 0.18 + 0.82 * (1.0 - math.exp(-8.0 * biased_t))
            paste_anchored(
                canvas,
                body_piece,
                origin,
                config.body_width * growth,
                angle,
                env * config.body_opacity,
                mirror=True,
            )
            if intensity == "Heavy":
                paste_anchored(
                    canvas,
                    body_piece,
                    (origin[0] + 4, origin[1]),
                    config.body_width * growth * 0.82,
                    angle + (4 if variation != "C" else -4),
                    env * 0.32,
                    mirror=True,
                )

        elif layer == "HitStopEcho":
            local = max(0.0, min(1.0, (biased_t - 0.035) / 0.965))
            env = envelope(local, 0.08, 0.25, 0.82, 0.02)
            growth = 0.28 + 0.72 * (1.0 - math.exp(-6.2 * local))
            paste_anchored(
                canvas,
                echo_piece,
                (origin[0] + 4, origin[1]),
                config.echo_width * growth,
                angle,
                env * config.echo_opacity,
            )

        elif layer == "MaterialSettle":
            env = envelope(biased_t, 0.12, 0.42, 1.0, 0.025)
            direction = np.array([math.cos(math.radians(angle)), math.sin(math.radians(angle))])
            normal = np.array([-direction[1], direction[0]])
            for index, piece in enumerate(material_pieces[: config.settle_count]):
                delay = 0.025 * index
                q = max(0.0, min(1.0, (biased_t - delay) / max(0.001, 1.0 - delay)))
                speed = 72 + 19 * index + (22 if intensity == "Heavy" else 0)
                side = ((index % 3) - 1) * (24 + 5 * index)
                gravity = 96 * q * q
                position = np.array(origin) + direction * speed * q + normal * side * q
                position[1] += gravity
                width = 24 + 5 * (index % 3) + (6 if intensity == "Heavy" else 0)
                opacity = env * (0.70 - index * 0.045)
                paste_anchored(
                    canvas,
                    piece,
                    (float(position[0]), float(position[1])),
                    width,
                    angle + index * 19 + q * 54,
                    max(0.04, opacity),
                )
        else:
            raise KeyError(layer)

        alpha = np.asarray(canvas.getchannel("A"))
        if np.count_nonzero(alpha) < 48:
            raise RuntimeError(f"Sparse frame: {intensity}/{variation}/{layer}/{frame}")
        frames.append(sanitize_straight_alpha(canvas))
    return frames


def assemble_flipbook(frames: list[Image.Image]) -> Image.Image:
    atlas = Image.new("RGBA", (ATLAS, ATLAS), (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        column = index % GRID
        chronological_row = index // GRID
        image_row = GRID - 1 - chronological_row
        atlas.alpha_composite(frame, (column * CELL, image_row * CELL))
    return atlas


def extract_frame(atlas: Image.Image, index: int) -> Image.Image:
    column = index % GRID
    chronological_row = index // GRID
    image_row = GRID - 1 - chronological_row
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


def font(size: int) -> ImageFont.FreeTypeFont | ImageFont.ImageFont:
    candidates = [
        Path("C:/Windows/Fonts/malgun.ttf"),
        Path("C:/Windows/Fonts/segoeui.ttf"),
        Path("C:/Windows/Fonts/arial.ttf"),
    ]
    for candidate in candidates:
        if candidate.is_file():
            return ImageFont.truetype(str(candidate), size=size)
    return ImageFont.load_default()


def fit_inside(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    copy = image.copy()
    copy.thumbnail(size, Image.Resampling.LANCZOS)
    return copy


def build_layer_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    sheet = Image.new("RGBA", (2560, 1536), (17, 19, 20, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((54, 28), "SHOWCASE HIT VFX v004 — FOUR-LAYER BREAKDOWN (VARIATION A)", font=font(44), fill=(232, 229, 216, 255))
    headers = ["CONTACT", "DIRECTIONAL BODY", "HIT-STOP ECHO", "MATERIAL SETTLE", "COMPOSITE"]
    for column, header in enumerate(headers):
        draw.text((65 + column * 500, 96), header, font=font(25), fill=(190, 184, 169, 255))
    for row, intensity in enumerate(CONFIGS):
        y0 = 150 + row * 450
        draw.text((42, y0 + 10), intensity.upper(), font=font(32), fill=(223, 207, 177, 255))
        atlases = {layer: Image.open(outputs[(intensity, "A")][layer]).convert("RGBA") for layer in LAYER_NAMES}
        images = [extract_frame(atlases[layer], 3) for layer in LAYER_NAMES] + [composite_frame(atlases, 3)]
        for column, image in enumerate(images):
            panel = checkerboard((470, 390), 20)
            fitted = fit_inside(image, (430, 350))
            panel.alpha_composite(fitted, ((470 - fitted.width) // 2, (390 - fitted.height) // 2))
            sheet.alpha_composite(panel, (42 + column * 500, y0 + 46))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_LayerBreakdown_v004.png"
    save_png(sheet, path)
    return path


def scene_crop(base: Image.Image) -> Image.Image:
    # Keeps the glove contact and target large while preserving the approved camera context.
    left = int(base.width * 0.39)
    top = int(base.height * 0.17)
    right = int(base.width * 0.92)
    bottom = int(base.height * 0.78)
    return base.crop((left, top, right, bottom))


def overlay_on_scene(base: Image.Image, effect: Image.Image, intensity: str) -> Image.Image:
    canvas = base.convert("RGBA").copy()
    config = CONFIGS[intensity]
    target_w = int(effect.width * config.preview_scale)
    target_h = int(effect.height * config.preview_scale)
    resized = effect.resize((target_w, target_h), Image.Resampling.LANCZOS)
    contact_x = int(canvas.width * 0.525)
    contact_y = int(canvas.height * 0.405)
    origin_x = int(104 * config.preview_scale)
    origin_y = int(256 * config.preview_scale)
    canvas.alpha_composite(resized, (contact_x - origin_x, contact_y - origin_y))
    return canvas


def build_contact_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    base = scene_crop(Image.open(CONCEPT_REFERENCES[0]).convert("RGBA"))
    sheet = Image.new("RGBA", (2400, 1440), (14, 16, 17, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((52, 28), "SHOWCASE HIT VFX v004 — LIGHT / MEDIUM / HEAVY × A / B / C", font=font(46), fill=(234, 230, 216, 255))
    for row, intensity in enumerate(CONFIGS):
        for column, variation in enumerate(VARIATIONS):
            atlases = {layer: Image.open(outputs[(intensity, variation)][layer]).convert("RGBA") for layer in LAYER_NAMES}
            preview = overlay_on_scene(base, composite_frame(atlases, 3), intensity)
            preview = ImageOps.fit(preview, (760, 410), method=Image.Resampling.LANCZOS)
            x = 35 + column * 790
            y = 105 + row * 435
            sheet.alpha_composite(preview, (x, y))
            draw.text((x + 18, y + 14), f"{intensity.upper()}  {variation}", font=font(29), fill=(244, 237, 216, 255), stroke_width=2, stroke_fill=(12, 12, 12, 230))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_ABC_ContactSheet_v004.png"
    save_png(sheet, path)
    return path


def build_alpha_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    sheet = Image.new("RGBA", (2304, 2304), (15, 17, 18, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((46, 26), "SHOWCASE HIT VFX v004 — GRAYSCALE / ALPHA READABILITY", font=font(42), fill=(230, 226, 214, 255))
    for index, (intensity, variation) in enumerate((i, v) for i in CONFIGS for v in VARIATIONS):
        row, column = divmod(index, 3)
        x0 = 28 + column * 758
        y0 = 94 + row * 728
        atlases = {layer: Image.open(outputs[(intensity, variation)][layer]).convert("RGBA") for layer in LAYER_NAMES}
        composite = composite_frame(atlases, 3)
        rgba = np.asarray(composite, dtype=np.uint8)
        lum = (rgba[:, :, 0] * 0.2126 + rgba[:, :, 1] * 0.7152 + rgba[:, :, 2] * 0.0722).astype(np.uint8)
        grayscale = Image.fromarray(np.dstack((lum, lum, lum, rgba[:, :, 3])), "RGBA")
        alpha = composite.getchannel("A")
        alpha_view = Image.merge("RGBA", (alpha, alpha, alpha, Image.new("L", alpha.size, 255)))
        panel = checkerboard((720, 660), 20)
        gray_fit = fit_inside(grayscale, (680, 450))
        alpha_fit = fit_inside(alpha_view, (330, 180))
        panel.alpha_composite(gray_fit, ((720 - gray_fit.width) // 2, 36))
        panel.alpha_composite(alpha_fit, ((720 - alpha_fit.width) // 2, 470))
        sheet.alpha_composite(panel, (x0, y0))
        draw.text((x0 + 16, y0 + 12), f"{intensity.upper()} {variation}", font=font(27), fill=(233, 222, 199, 255))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_GrayscaleAlpha_v004.png"
    save_png(sheet, path)
    return path


def build_animated_previews(outputs: dict[tuple[str, str], dict[str, Path]]) -> list[Path]:
    base = scene_crop(Image.open(CONCEPT_REFERENCES[0]).convert("RGBA"))
    # 960x540 is large enough for owner-side motion review while keeping the
    # three non-runtime GIFs comfortably below repository-size risk.
    base = ImageOps.fit(base, (960, 540), method=Image.Resampling.LANCZOS)
    paths: list[Path] = []
    for intensity in CONFIGS:
        gif_frames: list[Image.Image] = []
        for variation in VARIATIONS:
            atlases = {layer: Image.open(outputs[(intensity, variation)][layer]).convert("RGBA") for layer in LAYER_NAMES}
            for frame_index in range(FRAME_COUNT):
                frame = overlay_on_scene(base, composite_frame(atlases, frame_index), intensity)
                draw = ImageDraw.Draw(frame)
                draw.rounded_rectangle((28, 24, 330, 90), radius=12, fill=(8, 10, 11, 190), outline=(141, 118, 89, 210), width=2)
                draw.text((48, 38), f"{intensity.upper()}  VARIATION {variation}", font=font(30), fill=(242, 232, 210, 255))
                gif_frames.append(frame.convert("P", palette=Image.Palette.ADAPTIVE, colors=128))
            for _ in range(6):
                hold = base.copy()
                draw = ImageDraw.Draw(hold)
                draw.rounded_rectangle((28, 24, 330, 90), radius=12, fill=(8, 10, 11, 190), outline=(141, 118, 89, 210), width=2)
                draw.text((48, 38), f"{intensity.upper()}  VARIATION {variation}", font=font(30), fill=(242, 232, 210, 255))
                gif_frames.append(hold.convert("P", palette=Image.Palette.ADAPTIVE, colors=128))
        path = PREVIEW_DIR / f"VFX_Showcase_HitProduction_{intensity}_ABC_Animated_v004.gif"
        path.parent.mkdir(parents=True, exist_ok=True)
        gif_frames[0].save(path, save_all=True, append_images=gif_frames[1:], duration=42, loop=0, disposal=2, optimize=True)
        paths.append(path)
    return paths


def build_timing_sheet(outputs: dict[tuple[str, str], dict[str, Path]]) -> Path:
    frame_indices = (0, 1, 2, 3, 5, 8, 11, 15)
    sheet = Image.new("RGBA", (2432, 1736), (14, 16, 17, 255))
    draw = ImageDraw.Draw(sheet)
    draw.text((48, 28), "SHOWCASE HIT VFX v004 — 16-FRAME TIMING AUDIT (VARIATION A)", font=font(44), fill=(232, 228, 215, 255))
    for column, frame_index in enumerate(frame_indices):
        draw.text((68 + column * 300, 96), f"F{frame_index:02d}", font=font(27), fill=(191, 181, 162, 255))
    for row, intensity in enumerate(CONFIGS):
        atlases = {layer: Image.open(outputs[(intensity, "A")][layer]).convert("RGBA") for layer in LAYER_NAMES}
        y0 = 142 + row * 520
        draw.text((42, y0 + 14), intensity.upper(), font=font(32), fill=(226, 210, 180, 255))
        for column, frame_index in enumerate(frame_indices):
            panel = checkerboard((280, 430), 20)
            image = fit_inside(composite_frame(atlases, frame_index), (260, 390))
            panel.alpha_composite(image, ((280 - image.width) // 2, (430 - image.height) // 2))
            sheet.alpha_composite(panel, (42 + column * 300, y0 + 54))
    path = PREVIEW_DIR / "VFX_Showcase_HitProduction_TimingAudit_v004.png"
    save_png(sheet, path)
    return path


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
        frame_hashes: list[str] = []
        frame_alpha: list[int] = []
        partial = 0
        magenta = green = cyan = black_halo = 0
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
            frame_hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
            frame_alpha.append(count)
            active = alpha > 8
            partial += int(np.count_nonzero((alpha > 0) & (alpha < 255)))
            magenta += int(np.count_nonzero(active & (rgba[:, :, 0] > 220) & (rgba[:, :, 1] < 60) & (rgba[:, :, 2] > 190)))
            green += int(np.count_nonzero(active & (rgba[:, :, 1] > rgba[:, :, 0] * 1.45) & (rgba[:, :, 1] > rgba[:, :, 2] * 1.45)))
            cyan += int(np.count_nonzero(active & (rgba[:, :, 0] < 70) & (rgba[:, :, 1] > 200) & (rgba[:, :, 2] > 200)))
            black_halo += int(np.count_nonzero((alpha > 0) & (alpha < 64) & (np.max(rgba[:, :, :3], axis=2) < 5)))
        if len(set(frame_hashes)) != FRAME_COUNT:
            raise RuntimeError(f"Duplicate frames: {relative(path)}")
        if any((magenta, green, cyan, black_halo)):
            raise RuntimeError(f"Palette/alpha risk: {relative(path)} magenta={magenta} green={green} cyan={cyan} blackHalo={black_halo}")
        return {
            "path": relative(path),
            "dimensions": f"{ATLAS}x{ATLAS}",
            "mode": "RGBA",
            "frame_order": "UNITY_WHOLE_SHEET_LOWER_LEFT_ROW_MAJOR",
            "frame_hash_unique": len(set(frame_hashes)),
            "frame_alpha_counts": ";".join(str(value) for value in frame_alpha),
            "alpha_partial_pixels": partial,
            "transparent_margin_min_px": min(transparent_margin(extract_frame(image, index)) for index in range(FRAME_COUNT)),
            "magenta_pixels": magenta,
            "green_spill_pixels": green,
            "neon_cyan_pixels": cyan,
            "black_halo_pixels": black_halo,
            "sha256": sha256(path),
            "bytes": path.stat().st_size,
        }


def write_ledger(paths: list[Path], metrics: dict[str, dict[str, object]]) -> Path:
    ledger = LICENSE_DIR / "VFX_Showcase_HitProduction_Provenance_Ledger_v004.csv"
    fields = [
        "asset_path", "asset_role", "provenance_classification", "creator_or_generator",
        "source_prompt_id", "source_inputs", "third_party_source", "rights_basis",
        "commercial_use", "repository_redistribution", "dimensions", "mode", "frame_order",
        "frame_hash_unique", "alpha_partial_pixels", "transparent_margin_min_px", "sha256",
        "bytes", "review_status",
    ]
    rows: list[dict[str, object]] = []
    prompt_ids = ["IG-VFX-HIT-004-A", "IG-VFX-HIT-004-B", "IG-VFX-HIT-004-C", "IG-VFX-HIT-004-D"]
    for path in paths:
        rel = relative(path)
        record = metrics.get(rel, {})
        if path in IMAGEGEN_BANKS:
            index = IMAGEGEN_BANKS.index(path)
            role = "PRESERVED_IMAGEGEN_SOURCE"
            classification = "ORIGINAL_AI_GENERATED_SOURCE"
            creator = "OpenAI built-in image generation; approved project concept images supplied as direction references"
            prompt_id = prompt_ids[index]
            inputs = ";".join(relative(item) for item in CONCEPT_REFERENCES[1:])
        elif path in ALPHA_BANKS:
            index = ALPHA_BANKS.index(path)
            role = "STRAIGHT_ALPHA_ELEMENT_BANK_SOURCE"
            classification = "ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE"
            creator = "Imagegen skill chroma-key removal: auto-key border, soft matte, despill, edge-contract 1"
            prompt_id = prompt_ids[index]
            inputs = relative(IMAGEGEN_BANKS[index])
        elif path.parent == FLIPBOOK_DIR:
            role = "UNITY_REVIEW_4X4_FLIPBOOK_LAYER_CANDIDATE"
            classification = "ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE"
            creator = "Codex-authored deterministic 16-frame assembly"
            prompt_id = "IG-VFX-HIT-004-A/B/C/D"
            inputs = ";".join(relative(item) for item in ALPHA_BANKS) + ";" + relative(SCRIPT_PATH)
        elif path.parent == UNITY_FLIPBOOK_DIR:
            role = "UNITY_REVIEW_BYTE_IDENTICAL_COPY"
            classification = "BYTE_IDENTICAL_PROJECT_DELIVERY_COPY"
            creator = "Deterministic copy from ArtSource candidate"
            prompt_id = "IG-VFX-HIT-004-A/B/C/D"
            inputs = relative(FLIPBOOK_DIR / path.name)
        elif path in CONCEPT_REFERENCES:
            role = "APPROVED_DIRECTION_CONCEPT_REFERENCE"
            classification = "ORIGINAL_AI_GENERATED_PROJECT_REFERENCE"
            creator = "OpenAI built-in image generation during Human Owner concept review"
            prompt_id = "CONVERSATION CONCEPT PASS / OWNER SELECTED 3,5,10 DIRECTION"
            inputs = "NONE" if path == CONCEPT_REFERENCES[0] else relative(CONCEPT_REFERENCES[0])
        elif path.suffix.lower() in (".png", ".gif"):
            role = "HUMAN_OWNER_REVIEW_PREVIEW"
            classification = "DETERMINISTIC_REVIEW_COMPOSITE"
            creator = "Codex-authored local review compositor"
            prompt_id = "NONE"
            inputs = "HIT PRODUCTION v004 CANDIDATES"
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
            "asset_path": rel,
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
    required = IMAGEGEN_BANKS + ALPHA_BANKS + CONCEPT_REFERENCES
    missing = [path for path in required if not path.is_file()]
    if missing:
        raise FileNotFoundError(", ".join(str(path) for path in missing))
    for directory in (FLIPBOOK_DIR, PREVIEW_DIR, LICENSE_DIR, UNITY_FLIPBOOK_DIR):
        directory.mkdir(parents=True, exist_ok=True)

    directional = split_bank(DIRECTIONAL_BANK, 4, 4)
    contacts = split_bank(CONTACT_BANK, 4, 4)
    echoes = split_bank(ECHO_BANK, 4, 4)
    materials = split_bank(MATERIAL_BANK, 5, 4)
    if (len(directional), len(contacts), len(echoes), len(materials)) != (16, 16, 16, 20):
        raise RuntimeError("Source-bank grid count mismatch")

    outputs: dict[tuple[str, str], dict[str, Path]] = {}
    candidate_paths: list[Path] = []
    for intensity in CONFIGS:
        for variation in VARIATIONS:
            key = (intensity, variation)
            outputs[key] = {}
            for layer in LAYER_NAMES:
                filename = f"T_VFX_Showcase_Hit{intensity}_{variation}_{layer}_4x4_v004.png"
                path = FLIPBOOK_DIR / filename
                frames = make_layer_frames(intensity, variation, layer, directional, contacts, echoes, materials)
                save_png(assemble_flipbook(frames), path)
                outputs[key][layer] = path
                candidate_paths.append(path)

    validation: dict[str, dict[str, object]] = {}
    for path in candidate_paths:
        validation[relative(path)] = validate_candidate(path)
        unity_path = UNITY_FLIPBOOK_DIR / path.name
        unity_path.write_bytes(path.read_bytes())
        if sha256(unity_path) != sha256(path):
            raise RuntimeError(f"Unity candidate copy is not byte-identical: {path.name}")

    preview_paths = [
        build_contact_sheet(outputs),
        build_layer_sheet(outputs),
        build_alpha_sheet(outputs),
        build_timing_sheet(outputs),
        *build_animated_previews(outputs),
    ]
    unity_capture = PREVIEW_DIR / "VFX_Showcase_HitProduction_UnityActualPlay_v004.png"
    if unity_capture.is_file():
        preview_paths.append(unity_capture)
    provenance_paths = IMAGEGEN_BANKS + ALPHA_BANKS + CONCEPT_REFERENCES + candidate_paths + [UNITY_FLIPBOOK_DIR / path.name for path in candidate_paths] + preview_paths + [SCRIPT_PATH]
    ledger = write_ledger(provenance_paths, validation)

    result = {
        "status": "PASS",
        "review_status": "REVIEW / OWNER APPROVAL PENDING",
        "intensities": len(CONFIGS),
        "variations_per_intensity": len(VARIATIONS),
        "layers_per_variation": len(LAYER_NAMES),
        "flipbooks": len(candidate_paths),
        "frames": len(candidate_paths) * FRAME_COUNT,
        "unity_byte_identical": len(candidate_paths),
        "preview_count": len(preview_paths),
        "total_candidate_bytes": sum(path.stat().st_size for path in candidate_paths),
        "ledger": relative(ledger),
        "candidates": [validation[relative(path)] for path in candidate_paths],
        "previews": [{"path": relative(path), "sha256": sha256(path), "bytes": path.stat().st_size} for path in preview_paths],
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
