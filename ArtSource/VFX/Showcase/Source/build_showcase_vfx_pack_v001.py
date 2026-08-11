#!/usr/bin/env python3
"""Build BREAK NOTICE Showcase VFX production-source pack v001.

This source tool is intentionally engine-independent. It creates transparent
RGBA delivery textures, three 4x4 flipbooks, editable SVG sources, inspection
previews, and the machine-readable provenance ledger. It does not create Unity
metadata or perform Runtime integration.
"""

from __future__ import annotations

import csv
import hashlib
import json
import math
import random
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageOps
from scipy import ndimage


SOURCE_DIR = Path(__file__).resolve().parent
SHOWCASE_DIR = SOURCE_DIR.parent
REPO_ROOT = SHOWCASE_DIR.parents[2]
TEXTURE_DIR = SHOWCASE_DIR / "Textures"
FLIPBOOK_DIR = SHOWCASE_DIR / "Flipbooks"
PREVIEW_DIR = SHOWCASE_DIR / "Preview"
LICENSE_DIR = SHOWCASE_DIR / "Licenses"

CELL_SIZE = 512
ATLAS_SIZE = 2048
SUPERSAMPLE = 4

PAPER_WHITE = (226, 232, 229, 226)
OFFICE_WHITE = (206, 216, 215, 216)
SAFETY_AMBER = (193, 146, 58, 224)
MAINTENANCE_GREEN = (98, 137, 112, 232)
WARNING_RED = (156, 76, 69, 232)
DUST_GRAY = (137, 129, 117, 160)


SVG_SOURCES: dict[str, str] = {
    "VFX_Showcase_InteractionSoftRing_v001.svg": """<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <g fill="none" stroke="#E2E8E5" stroke-linecap="round">
    <path d="M103 218 A164 164 0 0 1 218 103" stroke-width="30" opacity="0.13"/>
    <path d="M294 103 A164 164 0 0 1 409 218" stroke-width="30" opacity="0.13"/>
    <path d="M409 294 A164 164 0 0 1 294 409" stroke-width="30" opacity="0.13"/>
    <path d="M218 409 A164 164 0 0 1 103 294" stroke-width="30" opacity="0.13"/>
    <path d="M103 218 A164 164 0 0 1 218 103" stroke-width="12" opacity="0.88"/>
    <path d="M294 103 A164 164 0 0 1 409 218" stroke-width="12" opacity="0.88"/>
    <path d="M409 294 A164 164 0 0 1 294 409" stroke-width="12" opacity="0.88"/>
    <path d="M218 409 A164 164 0 0 1 103 294" stroke-width="12" opacity="0.88"/>
  </g>
  <path d="M238 447 H274" fill="none" stroke="#C1923A" stroke-width="10" stroke-linecap="round" opacity="0.88"/>
</svg>
""",
    "VFX_Showcase_PickupCatchConfirm_v001.svg": """<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <g fill="none" stroke-linecap="round" stroke-linejoin="round">
    <path d="M184 126 H132 V190 M328 126 H380 V190 M184 386 H132 V322 M328 386 H380 V322" stroke="#638970" stroke-width="32" opacity="0.13"/>
    <path d="M184 126 H132 V190 M328 126 H380 V190 M184 386 H132 V322 M328 386 H380 V322" stroke="#638970" stroke-width="13" opacity="0.94"/>
    <path d="M188 260 L236 308 L334 204" stroke="#E2E8E5" stroke-width="46" opacity="0.12"/>
    <path d="M188 260 L236 308 L334 204" stroke="#E2E8E5" stroke-width="18" opacity="0.94"/>
  </g>
</svg>
""",
    "VFX_Showcase_InteractionRejected_v001.svg": """<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <g fill="none" stroke-linecap="round" stroke-linejoin="round">
    <path d="M188 124 H128 V184 M324 124 H384 V184 M188 388 H128 V328 M324 388 H384 V328" stroke="#9C4C45" stroke-width="30" opacity="0.12"/>
    <path d="M188 124 H128 V184 M324 124 H384 V184 M188 388 H128 V328 M324 388 H384 V328" stroke="#9C4C45" stroke-width="13" opacity="0.94"/>
    <path d="M203 203 L309 309 M309 203 L203 309" stroke="#E2E8E5" stroke-width="44" opacity="0.11"/>
    <path d="M203 203 L309 309 M309 203 L203 309" stroke="#E2E8E5" stroke-width="17" opacity="0.92"/>
    <path d="M105 256 H151 M361 256 H407" stroke="#9C4C45" stroke-width="11" opacity="0.78"/>
  </g>
</svg>
""",
    "VFX_Showcase_AllyReadySupport_v001.svg": """<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <g fill="none" stroke-linecap="round" stroke-linejoin="round">
    <path d="M132 318 V374 H190 M380 318 V374 H322 M190 410 H322" stroke="#E2E8E5" stroke-width="32" opacity="0.12"/>
    <path d="M132 318 V374 H190 M380 318 V374 H322 M190 410 H322" stroke="#E2E8E5" stroke-width="12" opacity="0.92"/>
    <path d="M256 88 V132 M164 126 L190 164 M348 126 L322 164" stroke="#C1923A" stroke-width="13" opacity="0.92"/>
    <path d="M214 438 H298" stroke="#C1923A" stroke-width="9" opacity="0.78"/>
  </g>
</svg>
""",
    "VFX_Showcase_ExitActivation_v001.svg": """<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <g fill="none" stroke-linecap="round" stroke-linejoin="round">
    <path d="M148 414 V142 H364 V414" stroke="#638970" stroke-width="34" opacity="0.12"/>
    <path d="M148 414 V142 H364 V414" stroke="#638970" stroke-width="14" opacity="0.95"/>
    <path d="M210 238 L256 192 L302 238 M226 294 L256 264 L286 294" stroke="#E2E8E5" stroke-width="38" opacity="0.11"/>
    <path d="M210 238 L256 192 L302 238 M226 294 L256 264 L286 294" stroke="#E2E8E5" stroke-width="14" opacity="0.92"/>
    <path d="M194 442 H318" stroke="#C1923A" stroke-width="9" opacity="0.72"/>
  </g>
</svg>
""",
}


GEOMETRIC_TEXTURES: dict[str, str] = {
    "interaction": "T_VFX_Showcase_InteractionSoftRing_v001.png",
    "confirm": "T_VFX_Showcase_PickupCatchConfirm_v001.png",
    "reject": "T_VFX_Showcase_InteractionRejected_v001.png",
    "ready": "T_VFX_Showcase_AllyReadySupport_v001.png",
    "exit": "T_VFX_Showcase_ExitActivation_v001.png",
}


FLIPBOOKS: dict[str, str] = {
    "light_dust": "T_VFX_Showcase_LightContactDust_4x4_v001.png",
    "wheel_dust": "T_VFX_Showcase_ChairWheelContact_4x4_v001.png",
    "paper": "T_VFX_Showcase_RestrainedPaperBurst_4x4_v001.png",
}


def scaled(value: float) -> int:
    return int(round(value * SUPERSAMPLE))


def sbox(box: tuple[float, float, float, float]) -> tuple[int, int, int, int]:
    return tuple(scaled(value) for value in box)  # type: ignore[return-value]


def scolor(color: tuple[int, int, int, int], alpha_scale: float = 1.0) -> tuple[int, int, int, int]:
    return color[:3] + (max(0, min(255, int(round(color[3] * alpha_scale)))),)


def downsample(image: Image.Image) -> Image.Image:
    return image.resize((CELL_SIZE, CELL_SIZE), Image.Resampling.LANCZOS)


def draw_segmented_arcs(
    draw: ImageDraw.ImageDraw,
    box: tuple[float, float, float, float],
    segments: list[tuple[float, float]],
    fill: tuple[int, int, int, int],
    width: float,
) -> None:
    for start, end in segments:
        draw.arc(sbox(box), start=round(start), end=round(end), fill=fill, width=scaled(width))


def draw_geometric_texture(kind: str) -> Image.Image:
    canvas = Image.new("RGBA", (CELL_SIZE * SUPERSAMPLE, CELL_SIZE * SUPERSAMPLE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(canvas, "RGBA")
    arcs = [(202, 248), (292, 338), (22, 68), (112, 158)]

    if kind == "interaction":
        draw_segmented_arcs(draw, (92, 92, 420, 420), arcs, scolor(PAPER_WHITE, 0.16), 31)
        draw_segmented_arcs(draw, (92, 92, 420, 420), arcs, scolor(PAPER_WHITE, 0.96), 12)
        draw.line((scaled(238), scaled(447), scaled(274), scaled(447)), fill=scolor(SAFETY_AMBER, 0.92), width=scaled(10))
    elif kind == "confirm":
        bracket_lines = [
            [(184, 126), (132, 126), (132, 190)],
            [(328, 126), (380, 126), (380, 190)],
            [(184, 386), (132, 386), (132, 322)],
            [(328, 386), (380, 386), (380, 322)],
        ]
        for points in bracket_lines:
            pts = [(scaled(x), scaled(y)) for x, y in points]
            draw.line(pts, fill=scolor(MAINTENANCE_GREEN, 0.16), width=scaled(32), joint="curve")
            draw.line(pts, fill=scolor(MAINTENANCE_GREEN, 0.98), width=scaled(13), joint="curve")
        check = [(188, 260), (236, 308), (334, 204)]
        check_pts = [(scaled(x), scaled(y)) for x, y in check]
        draw.line(check_pts, fill=scolor(PAPER_WHITE, 0.13), width=scaled(46), joint="curve")
        draw.line(check_pts, fill=scolor(PAPER_WHITE, 0.98), width=scaled(18), joint="curve")
    elif kind == "reject":
        bracket_lines = [
            [(188, 124), (128, 124), (128, 184)],
            [(324, 124), (384, 124), (384, 184)],
            [(188, 388), (128, 388), (128, 328)],
            [(324, 388), (384, 388), (384, 328)],
        ]
        for points in bracket_lines:
            pts = [(scaled(x), scaled(y)) for x, y in points]
            draw.line(pts, fill=scolor(WARNING_RED, 0.15), width=scaled(30), joint="curve")
            draw.line(pts, fill=scolor(WARNING_RED, 0.98), width=scaled(13), joint="curve")
        for a, b in [((203, 203), (309, 309)), ((309, 203), (203, 309))]:
            coords = (scaled(a[0]), scaled(a[1]), scaled(b[0]), scaled(b[1]))
            draw.line(coords, fill=scolor(PAPER_WHITE, 0.13), width=scaled(44))
            draw.line(coords, fill=scolor(PAPER_WHITE, 0.96), width=scaled(17))
        draw.line((scaled(105), scaled(256), scaled(151), scaled(256)), fill=scolor(WARNING_RED, 0.82), width=scaled(11))
        draw.line((scaled(361), scaled(256), scaled(407), scaled(256)), fill=scolor(WARNING_RED, 0.82), width=scaled(11))
    elif kind == "ready":
        bracket_lines = [
            [(132, 318), (132, 374), (190, 374)],
            [(380, 318), (380, 374), (322, 374)],
            [(190, 410), (322, 410)],
        ]
        for points in bracket_lines:
            pts = [(scaled(x), scaled(y)) for x, y in points]
            draw.line(pts, fill=scolor(PAPER_WHITE, 0.14), width=scaled(32), joint="curve")
            draw.line(pts, fill=scolor(PAPER_WHITE, 0.96), width=scaled(12), joint="curve")
        for a, b in [((256, 88), (256, 132)), ((164, 126), (190, 164)), ((348, 126), (322, 164))]:
            draw.line((scaled(a[0]), scaled(a[1]), scaled(b[0]), scaled(b[1])), fill=scolor(SAFETY_AMBER, 0.96), width=scaled(13))
        draw.line((scaled(214), scaled(438), scaled(298), scaled(438)), fill=scolor(SAFETY_AMBER, 0.82), width=scaled(9))
    elif kind == "exit":
        door = [(148, 414), (148, 142), (364, 142), (364, 414)]
        door_pts = [(scaled(x), scaled(y)) for x, y in door]
        draw.line(door_pts, fill=scolor(MAINTENANCE_GREEN, 0.15), width=scaled(34), joint="curve")
        draw.line(door_pts, fill=scolor(MAINTENANCE_GREEN, 0.98), width=scaled(14), joint="curve")
        for points in [[(210, 238), (256, 192), (302, 238)], [(226, 294), (256, 264), (286, 294)]]:
            pts = [(scaled(x), scaled(y)) for x, y in points]
            draw.line(pts, fill=scolor(PAPER_WHITE, 0.13), width=scaled(38), joint="curve")
            draw.line(pts, fill=scolor(PAPER_WHITE, 0.96), width=scaled(14), joint="curve")
        draw.line((scaled(194), scaled(442), scaled(318), scaled(442)), fill=scolor(SAFETY_AMBER, 0.76), width=scaled(9))
    else:
        raise ValueError(f"Unknown geometric texture kind: {kind}")

    return downsample(canvas)


def chroma_key_source(path: Path, low: float = 34.0, high: float = 168.0) -> Image.Image:
    rgb = np.asarray(Image.open(path).convert("RGB"), dtype=np.float32)
    border = np.concatenate(
        [
            rgb[:24, :, :].reshape(-1, 3),
            rgb[-24:, :, :].reshape(-1, 3),
            rgb[:, :24, :].reshape(-1, 3),
            rgb[:, -24:, :].reshape(-1, 3),
        ],
        axis=0,
    )
    key = np.median(border, axis=0)
    distance = np.linalg.norm(rgb - key, axis=2)
    distance_alpha = np.clip((distance - low) / (high - low), 0.0, 1.0)
    distance_alpha = distance_alpha * distance_alpha * (3.0 - 2.0 * distance_alpha)

    # A second estimate based on green dominance retains soft natural-material
    # edges better than Euclidean distance alone. Background pixels have near
    # maximum green excess; neutral dust and paper approach zero excess.
    rb_max = np.maximum(rgb[:, :, 0], rgb[:, :, 2])
    green_excess = rgb[:, :, 1] - rb_max
    green_t = np.clip((green_excess - 12.0) / 226.0, 0.0, 1.0)
    green_t = green_t * green_t * (3.0 - 2.0 * green_t)
    green_alpha = 1.0 - green_t
    alpha = np.maximum(distance_alpha, green_alpha)

    # Recover an approximate straight-alpha foreground from the keyed RGB.
    # This prevents green key color from surviving in semi-transparent dust.
    safe_alpha = np.maximum(alpha[:, :, None], 0.035)
    cleaned = (rgb - (1.0 - alpha[:, :, None]) * key[None, None, :]) / safe_alpha
    cleaned = np.clip(cleaned, 0.0, 255.0)
    # Delivered dust/paper has no intentional green material role. Rebuild the
    # green channel from red/blue to keep warm/cool neutral variation while
    # removing chroma spill without introducing magenta.
    cleaned[:, :, 1] = cleaned[:, :, 0] * 0.54 + cleaned[:, :, 2] * 0.46
    magenta_artifact = (
        (cleaned[:, :, 0] > 210.0)
        & (cleaned[:, :, 2] > 180.0)
        & (cleaned[:, :, 1] < 120.0)
    )
    alpha[magenta_artifact] = 0.0

    alpha_u8 = np.clip(alpha * 255.0, 0, 255).astype(np.uint8)
    alpha_u8[alpha_u8 < 10] = 0
    rgba = np.dstack((np.clip(cleaned, 0, 255).astype(np.uint8), alpha_u8))
    rgba[alpha_u8 == 0, :3] = 0
    return Image.fromarray(rgba, mode="RGBA")


def alpha_bbox(image: Image.Image, threshold: int = 8) -> tuple[int, int, int, int]:
    alpha = np.asarray(image.getchannel("A"))
    ys, xs = np.where(alpha > threshold)
    if len(xs) == 0:
        raise ValueError("Image contains no visible alpha")
    return int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1


def trim_alpha(image: Image.Image, threshold: int = 8, padding: int = 4) -> Image.Image:
    x0, y0, x1, y1 = alpha_bbox(image, threshold)
    x0 = max(0, x0 - padding)
    y0 = max(0, y0 - padding)
    x1 = min(image.width, x1 + padding)
    y1 = min(image.height, y1 + padding)
    return image.crop((x0, y0, x1, y1))


def multiply_alpha(image: Image.Image, multiplier: float) -> Image.Image:
    result = image.copy()
    alpha = np.asarray(result.getchannel("A"), dtype=np.float32)
    alpha = np.clip(alpha * multiplier, 0, 255).astype(np.uint8)
    result.putalpha(Image.fromarray(alpha, mode="L"))
    return result


def place_sprite(
    canvas: Image.Image,
    sprite: Image.Image,
    center: tuple[float, float],
    size: tuple[int, int],
    opacity: float,
) -> None:
    resized = sprite.resize(size, Image.Resampling.LANCZOS)
    resized = multiply_alpha(resized, opacity)
    x = int(round(center[0] - resized.width / 2))
    y = int(round(center[1] - resized.height / 2))
    canvas.alpha_composite(resized, (x, y))


def dust_frames(source: Image.Image, wheel: bool) -> list[Image.Image]:
    base = trim_alpha(source, threshold=9, padding=3)
    frames: list[Image.Image] = []
    for frame_index in range(16):
        p = frame_index / 15.0
        growth = 1.0 - (1.0 - p) ** 2.2
        envelope = 0.08 + 0.92 * (math.sin(math.pi * p) ** 0.72)
        frame = Image.new("RGBA", (CELL_SIZE, CELL_SIZE), (0, 0, 0, 0))

        if wheel:
            width = int(round(142 + 280 * growth))
            height = int(round(24 + 70 * growth))
            center = (238 + 25 * p, 329 - height * 0.02)
            place_sprite(frame, base, center, (width, height), envelope * 0.82)
            overlay = Image.new("RGBA", frame.size, (0, 0, 0, 0))
            draw = ImageDraw.Draw(overlay, "RGBA")
            length = 64 + 286 * growth
            line_alpha = int(round(92 * envelope))
            x0 = 80 + 28 * p
            for line_y, offset in [(359, 0), (375, 17)]:
                draw.line(
                    (round(x0 + offset), line_y, round(min(438, x0 + length - offset)), line_y),
                    fill=(123, 117, 106, line_alpha),
                    width=4,
                )
            frame.alpha_composite(overlay)
        else:
            width = int(round(74 + 292 * growth))
            height = int(round(22 + 104 * growth))
            center = (252 + 16 * p, 321 - height * 0.04)
            place_sprite(frame, base, center, (width, height), envelope * 0.76)

        frames.append(frame)
    return frames


def extract_paper_pieces(source: Image.Image) -> list[Image.Image]:
    alpha = np.asarray(source.getchannel("A"))
    mask = alpha > 112
    labels, count = ndimage.label(mask, structure=np.ones((3, 3), dtype=np.uint8))
    objects = ndimage.find_objects(labels)
    pieces: list[tuple[int, int, int, Image.Image]] = []
    for label_index in range(1, count + 1):
        slc = objects[label_index - 1]
        if slc is None:
            continue
        region = labels[slc] == label_index
        area = int(region.sum())
        y_slice, x_slice = slc
        width = x_slice.stop - x_slice.start
        height = y_slice.stop - y_slice.start
        if area < 180 or width < 4 or height < 4:
            continue
        crop = source.crop((x_slice.start, y_slice.start, x_slice.stop, y_slice.stop))
        crop_alpha = np.asarray(crop.getchannel("A"))
        isolated_alpha = np.where(region, crop_alpha, 0).astype(np.uint8)
        crop.putalpha(Image.fromarray(isolated_alpha, mode="L"))
        pieces.append((area, y_slice.start, x_slice.start, trim_alpha(crop, 7, 3)))

    if len(pieces) < 8:
        raise ValueError(f"Expected at least 8 isolated paper pieces, found {len(pieces)}")
    pieces.sort(key=lambda item: (-item[0], item[1], item[2]))
    positions = np.linspace(0, len(pieces) - 1, 8, dtype=int)
    return [pieces[int(position)][3] for position in positions]


def paper_frames(source: Image.Image) -> list[Image.Image]:
    pieces = extract_paper_pieces(source)
    rng = random.Random(17011)
    params: list[dict[str, float]] = []
    for piece_index, piece in enumerate(pieces):
        direction = -1.0 + 2.0 * piece_index / max(1, len(pieces) - 1)
        params.append(
            {
                "vx": direction * rng.uniform(108.0, 172.0) + rng.uniform(-18.0, 18.0),
                "vy": rng.uniform(-124.0, -62.0),
                "gravity": rng.uniform(128.0, 172.0),
                "spin": rng.uniform(-152.0, 152.0),
                "rotation": rng.uniform(-38.0, 38.0),
                "long_side": rng.uniform(42.0, 68.0),
                "delay": piece_index * 0.012,
            }
        )

    frames: list[Image.Image] = []
    for frame_index in range(16):
        p = frame_index / 15.0
        frame = Image.new("RGBA", (CELL_SIZE, CELL_SIZE), (0, 0, 0, 0))
        for piece, param in zip(pieces, params):
            q = max(0.0, min(1.0, (p - param["delay"]) / (1.0 - param["delay"])))
            appear = 0.16 + 0.84 * (1.0 - math.exp(-7.0 * q))
            if p <= 0.62:
                fade = 0.88
            else:
                fade = 0.88 - 0.78 * ((p - 0.62) / 0.38)
            opacity = max(0.08, fade) * (0.28 + 0.72 * appear)

            aspect = piece.width / max(1, piece.height)
            if aspect >= 1.0:
                width = max(8, int(round(param["long_side"] * appear)))
                height = max(5, int(round(width / aspect)))
            else:
                height = max(8, int(round(param["long_side"] * appear)))
                width = max(5, int(round(height * aspect)))
            resized = piece.resize((width, height), Image.Resampling.LANCZOS)
            rotated = resized.rotate(
                param["rotation"] + param["spin"] * q,
                resample=Image.Resampling.BICUBIC,
                expand=True,
            )
            rotated = multiply_alpha(rotated, opacity)

            x = 256.0 + param["vx"] * q
            y = 330.0 + param["vy"] * q + 0.5 * param["gravity"] * q * q
            x = max(60.0 + rotated.width / 2, min(452.0 - rotated.width / 2, x))
            y = max(58.0 + rotated.height / 2, min(448.0 - rotated.height / 2, y))
            frame.alpha_composite(rotated, (int(round(x - rotated.width / 2)), int(round(y - rotated.height / 2))))
        frames.append(frame)
    return frames


def assemble_flipbook(frames: list[Image.Image]) -> Image.Image:
    if len(frames) != 16:
        raise ValueError(f"A 4x4 flipbook requires 16 frames, received {len(frames)}")
    atlas = Image.new("RGBA", (ATLAS_SIZE, ATLAS_SIZE), (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        if frame.size != (CELL_SIZE, CELL_SIZE):
            raise ValueError(f"Frame {index} has invalid dimensions: {frame.size}")
        atlas.alpha_composite(frame, ((index % 4) * CELL_SIZE, (index // 4) * CELL_SIZE))
    return atlas


def save_png(image: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path, format="PNG", optimize=True, compress_level=9)


def checkerboard(size: tuple[int, int], square: int = 28) -> Image.Image:
    width, height = size
    image = Image.new("RGBA", size, (36, 40, 41, 255))
    draw = ImageDraw.Draw(image)
    colors = ((43, 48, 49, 255), (63, 69, 68, 255))
    for y in range(0, height, square):
        for x in range(0, width, square):
            draw.rectangle((x, y, min(width, x + square), min(height, y + square)), fill=colors[((x // square) + (y // square)) % 2])
    return image


SEVEN_SEGMENTS = {
    1: "bc",
    2: "abdeg",
    3: "abcdg",
    4: "bcfg",
    5: "acdfg",
    6: "acdefg",
    7: "abc",
    8: "abcdefg",
}


def draw_index_marker(draw: ImageDraw.ImageDraw, index: int, origin: tuple[int, int], color: tuple[int, int, int, int]) -> None:
    x, y = origin
    segments = {
        "a": (x + 8, y, x + 28, y),
        "b": (x + 32, y + 4, x + 32, y + 22),
        "c": (x + 32, y + 28, x + 32, y + 46),
        "d": (x + 8, y + 50, x + 28, y + 50),
        "e": (x + 4, y + 28, x + 4, y + 46),
        "f": (x + 4, y + 4, x + 4, y + 22),
        "g": (x + 8, y + 25, x + 28, y + 25),
    }
    for key in SEVEN_SEGMENTS[index]:
        draw.line(segments[key], fill=color, width=5)


def fit_asset_for_tile(image: Image.Image, size: tuple[int, int]) -> Image.Image:
    result = image.copy()
    result.thumbnail(size, Image.Resampling.LANCZOS)
    return result


def asset_order() -> list[Path]:
    return [
        TEXTURE_DIR / GEOMETRIC_TEXTURES["interaction"],
        TEXTURE_DIR / GEOMETRIC_TEXTURES["confirm"],
        TEXTURE_DIR / GEOMETRIC_TEXTURES["reject"],
        FLIPBOOK_DIR / FLIPBOOKS["light_dust"],
        FLIPBOOK_DIR / FLIPBOOKS["wheel_dust"],
        FLIPBOOK_DIR / FLIPBOOKS["paper"],
        TEXTURE_DIR / GEOMETRIC_TEXTURES["ready"],
        TEXTURE_DIR / GEOMETRIC_TEXTURES["exit"],
    ]


def build_contact_sheet(paths: list[Path]) -> Image.Image:
    tile_w, tile_h = 512, 576
    sheet = Image.new("RGBA", (tile_w * 4, tile_h * 2), (27, 31, 32, 255))
    for index, path in enumerate(paths, start=1):
        col = (index - 1) % 4
        row = (index - 1) // 4
        x0, y0 = col * tile_w, row * tile_h
        panel = checkerboard((444, 444), 28)
        asset = fit_asset_for_tile(Image.open(path).convert("RGBA"), (420, 420))
        panel.alpha_composite(asset, ((panel.width - asset.width) // 2, (panel.height - asset.height) // 2))
        sheet.alpha_composite(panel, (x0 + 34, y0 + 66))
        draw = ImageDraw.Draw(sheet, "RGBA")
        draw.rounded_rectangle((x0 + 20, y0 + 14, x0 + 70, y0 + 72), radius=8, fill=(44, 49, 50, 255), outline=(193, 146, 58, 210), width=2)
        draw_index_marker(draw, index, (x0 + 27, y0 + 18), (226, 232, 229, 236))
    return sheet


def grayscale_rgba(image: Image.Image) -> Image.Image:
    rgba = image.convert("RGBA")
    rgb = rgba.convert("RGB")
    gray = ImageOps.grayscale(rgb)
    merged = Image.merge("RGBA", (gray, gray, gray, rgba.getchannel("A")))
    return merged


def build_grayscale_alpha_sheet(paths: list[Path]) -> Image.Image:
    tile_w, tile_h = 512, 480
    sheet = Image.new("RGBA", (tile_w * 4, tile_h * 4), (24, 27, 28, 255))
    for zero_index, path in enumerate(paths):
        index = zero_index + 1
        row = zero_index // 2
        pair = zero_index % 2
        source = Image.open(path).convert("RGBA")
        fitted = fit_asset_for_tile(source, (398, 398))

        gray_x = pair * 1024
        alpha_x = gray_x + 512
        y0 = row * tile_h

        gray_panel = checkerboard((420, 420), 28)
        gray_asset = grayscale_rgba(fitted)
        gray_panel.alpha_composite(gray_asset, ((420 - gray_asset.width) // 2, (420 - gray_asset.height) // 2))
        sheet.alpha_composite(gray_panel, (gray_x + 72, y0 + 48))

        alpha_panel = Image.new("RGBA", (420, 420), (8, 8, 8, 255))
        alpha = fitted.getchannel("A")
        alpha_visual = Image.merge("RGBA", (alpha, alpha, alpha, Image.new("L", alpha.size, 255)))
        alpha_panel.alpha_composite(alpha_visual, ((420 - alpha_visual.width) // 2, (420 - alpha_visual.height) // 2))
        sheet.alpha_composite(alpha_panel, (alpha_x + 20, y0 + 48))

        draw = ImageDraw.Draw(sheet, "RGBA")
        for marker_x, marker_color in [(gray_x + 18, (144, 151, 150, 255)), (alpha_x + 18, (235, 238, 237, 255))]:
            draw.rounded_rectangle((marker_x, y0 + 8, marker_x + 50, y0 + 62), radius=8, fill=(38, 42, 43, 255), outline=marker_color, width=2)
            draw_index_marker(draw, index, (marker_x + 7, y0 + 10), marker_color)
    return sheet


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def transparent_margin(image: Image.Image) -> int | str:
    if image.mode != "RGBA":
        return "N/A"
    alpha = np.asarray(image.getchannel("A"))
    if np.all(alpha == 255):
        return 0
    if np.all(alpha == 0):
        return min(image.size)
    nonzero = alpha > 0
    ys, xs = np.where(nonzero)
    return int(min(xs.min(), image.width - 1 - xs.max(), ys.min(), image.height - 1 - ys.max()))


def image_metrics(path: Path) -> dict[str, str | int]:
    with Image.open(path) as image:
        width, height = image.size
        mode = image.mode
        if "A" in image.getbands():
            alpha = np.asarray(image.getchannel("A"))
            nonzero = int(np.count_nonzero(alpha))
            partial = int(np.count_nonzero((alpha > 0) & (alpha < 255)))
            margin: int | str = transparent_margin(image.convert("RGBA"))
        else:
            nonzero = "N/A"
            partial = "N/A"
            margin = "N/A"
    return {
        "dimensions": f"{width}x{height}",
        "mode": mode,
        "alpha_nonzero_pixels": nonzero,
        "alpha_partial_pixels": partial,
        "transparent_margin_px": margin,
    }


def relative(path: Path) -> str:
    return path.relative_to(REPO_ROOT).as_posix()


def provenance_class(path: Path) -> tuple[str, str, str]:
    name = path.name
    if "ImageGen" in name:
        return (
            "ORIGINAL_AI_GENERATED_SOURCE",
            "OpenAI built-in image generation; Human Owner-authorized prompt; no reference image",
            "NONE",
        )
    if path.parent == FLIPBOOK_DIR:
        source_name = {
            FLIPBOOKS["light_dust"]: "VFX_Showcase_LightContactDust_ImageGen_v001.png",
            FLIPBOOKS["wheel_dust"]: "VFX_Showcase_ChairWheelDust_ImageGen_v001.png",
            FLIPBOOKS["paper"]: "VFX_Showcase_PaperElements_ImageGen_v001.png",
        }[name]
        return (
            "ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE",
            "OpenAI built-in image generation source plus deterministic local alpha extraction and animation",
            f"ArtSource/VFX/Showcase/Source/{source_name}; ArtSource/VFX/Showcase/Source/{Path(__file__).name}",
        )
    if path.parent == TEXTURE_DIR or path.suffix.lower() == ".svg":
        return (
            "ORIGINAL_DETERMINISTIC",
            "Codex-authored geometric source and deterministic Pillow rasterization",
            f"ArtSource/VFX/Showcase/Source/{Path(__file__).name}",
        )
    if path.parent == PREVIEW_DIR:
        return (
            "ORIGINAL_DETERMINISTIC_REVIEW_COMPOSITE",
            "Deterministic preview assembled from delivered textures and flipbooks",
            "; ".join(relative(asset) for asset in asset_order()),
        )
    return (
        "ORIGINAL_DETERMINISTIC_SOURCE",
        "Codex-authored reproducible source code",
        "NONE",
    )


def write_ledger(binary_paths: list[Path], text_paths: list[Path]) -> Path:
    ledger_path = LICENSE_DIR / "VFX_Showcase_Provenance_Ledger_v001.csv"
    fields = [
        "asset_path",
        "file_kind",
        "asset_role",
        "provenance_classification",
        "creator_or_generator",
        "source_inputs",
        "third_party_source",
        "exact_license_or_rights_basis",
        "commercial_use",
        "redistribution",
        "derivative_work",
        "dimensions",
        "mode",
        "alpha_nonzero_pixels",
        "alpha_partial_pixels",
        "transparent_margin_px",
        "sha256",
        "bytes",
        "review_status",
    ]
    rows: list[dict[str, str | int]] = []
    for path in binary_paths + text_paths:
        classification, generator, source_inputs = provenance_class(path)
        if path.suffix.lower() == ".png":
            metrics = image_metrics(path)
            file_kind = "BINARY_PNG"
        else:
            metrics = {
                "dimensions": "N/A",
                "mode": "UTF-8 TEXT",
                "alpha_nonzero_pixels": "N/A",
                "alpha_partial_pixels": "N/A",
                "transparent_margin_px": "N/A",
            }
            file_kind = "TEXT_SOURCE"
        if "ImageGen" in path.name:
            role = "CHROMA_KEY_GENERATION_SOURCE"
        elif path.parent == TEXTURE_DIR:
            role = "UNITY_READY_TEXTURE_CANDIDATE"
        elif path.parent == FLIPBOOK_DIR:
            role = "UNITY_READY_4X4_FLIPBOOK_CANDIDATE"
        elif path.parent == PREVIEW_DIR:
            role = "HUMAN_OWNER_REVIEW_PREVIEW"
        elif path.suffix.lower() == ".svg":
            role = "EDITABLE_VECTOR_SOURCE"
        else:
            role = "REPRODUCIBLE_BUILD_SOURCE"
        rows.append(
            {
                "asset_path": relative(path),
                "file_kind": file_kind,
                "asset_role": role,
                "provenance_classification": classification,
                "creator_or_generator": generator,
                "source_inputs": source_inputs,
                "third_party_source": "NONE",
                "exact_license_or_rights_basis": "PROJECT ORIGINAL; NO THIRD-PARTY LICENSE APPLIES; HUMAN OWNER AUTHORIZED CREATION",
                "commercial_use": "AUTHORIZED FOR BREAK NOTICE PROJECT AND CONTEST MATERIALS BY HUMAN OWNER",
                "redistribution": "AUTHORIZED WITH PROJECT DELIVERY; NO EXTERNAL SOURCE RESTRICTION",
                "derivative_work": "AUTHORIZED",
                **metrics,
                "sha256": sha256(path),
                "bytes": path.stat().st_size,
                "review_status": "DELIVERED / PENDING HUMAN OWNER TECHNICAL REVIEW / NOT RUNTIME INTEGRATED",
            }
        )
    LICENSE_DIR.mkdir(parents=True, exist_ok=True)
    with ledger_path.open("w", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, quoting=csv.QUOTE_ALL, lineterminator="\n")
        writer.writeheader()
        writer.writerows(rows)
    return ledger_path


def png_signature_ok(path: Path) -> bool:
    return path.read_bytes()[:8] == b"\x89PNG\r\n\x1a\n"


def alpha_stats(image: Image.Image) -> tuple[int, int]:
    alpha = np.asarray(image.getchannel("A"))
    return int(np.count_nonzero(alpha)), int(np.count_nonzero((alpha > 0) & (alpha < 255)))


def magenta_pixels(image: Image.Image) -> int:
    rgba = np.asarray(image.convert("RGBA"))
    mask = (
        (rgba[:, :, 0] > 210)
        & (rgba[:, :, 2] > 180)
        & (rgba[:, :, 1] < 120)
        & (rgba[:, :, 3] > 0)
    )
    return int(mask.sum())


def validate_delivery(binary_paths: list[Path]) -> dict[str, object]:
    expected_dimensions: dict[str, tuple[int, int]] = {
        **{name: (CELL_SIZE, CELL_SIZE) for name in GEOMETRIC_TEXTURES.values()},
        **{name: (ATLAS_SIZE, ATLAS_SIZE) for name in FLIPBOOKS.values()},
        "VFX_Showcase_ProductionSourcePack_ContactSheet_v001.png": (2048, 1152),
        "VFX_Showcase_GrayscaleAlphaReadability_v001.png": (2048, 1920),
    }
    errors: list[str] = []
    binary_results: list[dict[str, object]] = []
    for path in binary_paths:
        if not png_signature_ok(path):
            errors.append(f"PNG signature failed: {relative(path)}")
            continue
        with Image.open(path) as image:
            image.load()
            if path.name in expected_dimensions and image.size != expected_dimensions[path.name]:
                errors.append(f"Dimension mismatch: {relative(path)} {image.size}")
            if path.parent in {TEXTURE_DIR, FLIPBOOK_DIR}:
                if image.mode != "RGBA":
                    errors.append(f"RGBA required: {relative(path)} mode={image.mode}")
                nonzero, partial = alpha_stats(image.convert("RGBA"))
                if nonzero == 0 or partial == 0:
                    errors.append(f"Usable alpha failed: {relative(path)}")
                if transparent_margin(image.convert("RGBA")) < 24:  # type: ignore[operator]
                    errors.append(f"Transparent outer margin <24 px: {relative(path)}")
                if magenta_pixels(image) != 0:
                    errors.append(f"Magenta pixels found: {relative(path)}")
            binary_results.append(
                {
                    "path": relative(path),
                    "dimensions": f"{image.width}x{image.height}",
                    "mode": image.mode,
                    "sha256": sha256(path),
                    "bytes": path.stat().st_size,
                }
            )

    for flipbook_path in [FLIPBOOK_DIR / name for name in FLIPBOOKS.values()]:
        with Image.open(flipbook_path) as atlas:
            atlas = atlas.convert("RGBA")
            frame_hashes: list[str] = []
            for frame_index in range(16):
                x = (frame_index % 4) * CELL_SIZE
                y = (frame_index // 4) * CELL_SIZE
                frame = atlas.crop((x, y, x + CELL_SIZE, y + CELL_SIZE))
                nonzero, partial = alpha_stats(frame)
                if nonzero < 24 or partial < 12:
                    errors.append(f"Frame alpha invalid: {relative(flipbook_path)} frame={frame_index}")
                margin = transparent_margin(frame)
                if not isinstance(margin, int) or margin < 20:
                    errors.append(f"Frame margin invalid: {relative(flipbook_path)} frame={frame_index} margin={margin}")
                frame_hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
            if len(set(frame_hashes)) != 16:
                errors.append(f"Frame uniqueness failed: {relative(flipbook_path)}")

    result: dict[str, object] = {
        "status": "PASS" if not errors else "FAIL",
        "errors": errors,
        "binary_count": len(binary_paths),
        "total_binary_bytes": sum(path.stat().st_size for path in binary_paths),
        "binaries": binary_results,
    }
    if errors:
        raise RuntimeError(json.dumps(result, ensure_ascii=False, indent=2))
    return result


def main() -> None:
    for directory in (TEXTURE_DIR, FLIPBOOK_DIR, PREVIEW_DIR, LICENSE_DIR):
        directory.mkdir(parents=True, exist_ok=True)

    svg_paths: list[Path] = []
    for filename, content in SVG_SOURCES.items():
        path = SOURCE_DIR / filename
        path.write_text(content, encoding="utf-8", newline="\n")
        svg_paths.append(path)

    texture_paths: list[Path] = []
    for kind, filename in GEOMETRIC_TEXTURES.items():
        path = TEXTURE_DIR / filename
        save_png(draw_geometric_texture(kind), path)
        texture_paths.append(path)

    light_source_path = SOURCE_DIR / "VFX_Showcase_LightContactDust_ImageGen_v001.png"
    wheel_source_path = SOURCE_DIR / "VFX_Showcase_ChairWheelDust_ImageGen_v001.png"
    paper_source_path = SOURCE_DIR / "VFX_Showcase_PaperElements_ImageGen_v001.png"
    keyed_sources = [light_source_path, wheel_source_path, paper_source_path]
    for source_path in keyed_sources:
        if not source_path.is_file():
            raise FileNotFoundError(source_path)

    light_source = chroma_key_source(light_source_path)
    wheel_source = chroma_key_source(wheel_source_path)
    paper_source = chroma_key_source(paper_source_path)

    flipbook_paths = [
        FLIPBOOK_DIR / FLIPBOOKS["light_dust"],
        FLIPBOOK_DIR / FLIPBOOKS["wheel_dust"],
        FLIPBOOK_DIR / FLIPBOOKS["paper"],
    ]
    save_png(assemble_flipbook(dust_frames(light_source, wheel=False)), flipbook_paths[0])
    save_png(assemble_flipbook(dust_frames(wheel_source, wheel=True)), flipbook_paths[1])
    save_png(assemble_flipbook(paper_frames(paper_source)), flipbook_paths[2])

    ordered_assets = asset_order()
    contact_path = PREVIEW_DIR / "VFX_Showcase_ProductionSourcePack_ContactSheet_v001.png"
    grayscale_path = PREVIEW_DIR / "VFX_Showcase_GrayscaleAlphaReadability_v001.png"
    save_png(build_contact_sheet(ordered_assets), contact_path)
    save_png(build_grayscale_alpha_sheet(ordered_assets), grayscale_path)
    preview_paths = [contact_path, grayscale_path]

    binary_paths = keyed_sources + texture_paths + flipbook_paths + preview_paths
    text_paths = [Path(__file__).resolve()] + svg_paths
    ledger_path = write_ledger(binary_paths, text_paths)
    validation = validate_delivery(binary_paths)
    validation["ledger"] = relative(ledger_path)
    validation["ledger_rows"] = len(binary_paths) + len(text_paths)
    validation["svg_count"] = len(svg_paths)
    print(json.dumps(validation, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
