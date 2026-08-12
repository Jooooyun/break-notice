"""Build the Showcase VFX premium revision v002 source pack.

This script is intentionally source-lane only. It authors deterministic RGBA
textures/flipbooks and Human Owner review previews under ArtSource/VFX/Showcase.
It does not create Unity metadata, materials, prefabs, runtime hooks, shaders,
packages, or gameplay state.

All delivered candidates remain REVIEW / OWNER APPROVAL PENDING.
"""

from __future__ import annotations

import csv
import hashlib
import json
import math
from pathlib import Path
from typing import Callable, Iterable

import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps


SIZE = 512
GRID = 4
ATLAS_SIZE = SIZE * GRID
AA = 4
MIN_MARGIN = 28

SCRIPT_PATH = Path(__file__).resolve()
SOURCE_DIR = SCRIPT_PATH.parent
SHOWCASE_DIR = SOURCE_DIR.parent
REPO_ROOT = SHOWCASE_DIR.parents[2]
TEXTURE_DIR = SHOWCASE_DIR / "Textures" / "Premium_v002"
FLIPBOOK_DIR = SHOWCASE_DIR / "Flipbooks" / "Premium_v002"
PREVIEW_DIR = SHOWCASE_DIR / "Preview" / "Premium_v002"
LICENSE_DIR = SHOWCASE_DIR / "Licenses"

DUST_SOURCE = SOURCE_DIR / "VFX_Showcase_PremiumDustBank_AlphaExtract_v002.png"
RUBBER_SOURCE = SOURCE_DIR / "VFX_Showcase_PremiumRubberBank_AlphaExtract_v002.png"
PAPER_SOURCE = SOURCE_DIR / "VFX_Showcase_PremiumPaperBank_AlphaExtract_v002.png"

IMAGEGEN_ORIGINALS = [
    SOURCE_DIR / "VFX_Showcase_PremiumDustBank_ImageGen_v002.png",
    SOURCE_DIR / "VFX_Showcase_PremiumRubberBank_ImageGen_v002.png",
    SOURCE_DIR / "VFX_Showcase_PremiumPaperBank_ImageGen_v002.png",
]
ALPHA_EXTRACTS = [DUST_SOURCE, RUBBER_SOURCE, PAPER_SOURCE]

STATUS_LAYERS: dict[str, list[tuple[str, str]]] = {
    "InteractionTarget": [
        ("EdgeTrace", "T_VFX_Showcase_InteractionTarget_EdgeTrace_v002.png"),
        ("ContactBracket", "T_VFX_Showcase_InteractionTarget_ContactBracket_v002.png"),
        ("MaterialWink", "T_VFX_Showcase_InteractionTarget_MaterialWink_v002.png"),
    ],
    "InvalidRejected": [
        ("ReticleInterruption", "T_VFX_Showcase_InvalidRejected_ReticleInterruption_v002.png"),
        ("TargetDropout", "T_VFX_Showcase_InvalidRejected_TargetDropout_v002.png"),
        ("ReverseEcho", "T_VFX_Showcase_InvalidRejected_ReverseEcho_v002.png"),
    ],
    "AllyReady": [
        ("GazeTick", "T_VFX_Showcase_AllyReady_GazeTick_v002.png"),
        ("PostureCue", "T_VFX_Showcase_AllyReady_PostureCue_v002.png"),
        ("FootContact", "T_VFX_Showcase_AllyReady_FootContact_v002.png"),
    ],
    "ExitRoute": [
        ("PracticalLamp", "T_VFX_Showcase_ExitRoute_PracticalLamp_v002.png"),
        ("SignHardware", "T_VFX_Showcase_ExitRoute_SignHardware_v002.png"),
        ("RouteEdge", "T_VFX_Showcase_ExitRoute_RouteEdge_v002.png"),
    ],
}

PHYSICAL_LAYERS: dict[str, list[tuple[str, str]]] = {
    "CatchConfirm": [
        ("PrimaryCompression", "T_VFX_Showcase_CatchConfirm_PrimaryCompression_4x4_v002.png"),
        ("PaperSnap", "T_VFX_Showcase_CatchConfirm_PaperSnap_4x4_v002.png"),
        ("DirectionalSettle", "T_VFX_Showcase_CatchConfirm_DirectionalSettle_4x4_v002.png"),
    ],
    "BinderImpact": [
        ("PrimaryTransient", "T_VFX_Showcase_BinderImpact_PrimaryTransient_4x4_v002.png"),
        ("DirectionalDust", "T_VFX_Showcase_BinderImpact_DirectionalDust_4x4_v002.png"),
        ("FiberSettle", "T_VFX_Showcase_BinderImpact_FiberSettle_4x4_v002.png"),
    ],
    "ChairSkid": [
        ("CasterBite", "T_VFX_Showcase_ChairSkid_CasterBite_4x4_v002.png"),
        ("RubberBody", "T_VFX_Showcase_ChairSkid_RubberBody_4x4_v002.png"),
        ("ContactSettle", "T_VFX_Showcase_ChairSkid_ContactSettle_4x4_v002.png"),
    ],
    "PaperBurst": [
        ("PrimarySnap", "T_VFX_Showcase_PaperBurst_PrimarySnap_4x4_v002.png"),
        ("PageFan", "T_VFX_Showcase_PaperBurst_PageFan_4x4_v002.png"),
        ("FlutterSettle", "T_VFX_Showcase_PaperBurst_FlutterSettle_4x4_v002.png"),
    ],
}

EFFECT_ORDER = [
    "InteractionTarget",
    "CatchConfirm",
    "InvalidRejected",
    "AllyReady",
    "BinderImpact",
    "ChairSkid",
    "PaperBurst",
    "ExitRoute",
]

PROMPT_IDS = {
    "VFX_Showcase_PremiumDustBank": "IG-VFX-SHW-PREMIUM-002-A",
    "VFX_Showcase_PremiumRubberBank": "IG-VFX-SHW-PREMIUM-002-B",
    "VFX_Showcase_PremiumPaperBank": "IG-VFX-SHW-PREMIUM-002-C",
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


def sanitize_runtime_alpha(image: Image.Image) -> Image.Image:
    """Neutralize chroma-key interpolation without hardening useful alpha."""
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    red = rgba[:, :, 0].astype(np.int16)
    green = rgba[:, :, 1].astype(np.int16)
    blue = rgba[:, :, 2].astype(np.int16)
    active = rgba[:, :, 3] > 0
    green_spill = active & (green > red * 1.35) & (green > blue * 1.35)
    neutral = ((red + blue) // 2).astype(np.uint8)
    rgba[:, :, 1][green_spill] = neutral[green_spill]
    near_black = active & (np.max(rgba[:, :, :3], axis=2) < 8)
    rgba[:, :, 0][near_black] = 8
    rgba[:, :, 1][near_black] = 8
    rgba[:, :, 2][near_black] = 8
    rgba[:, :, 3][rgba[:, :, 3] <= 3] = 0
    return Image.fromarray(rgba, mode="RGBA")


def alpha_multiply(image: Image.Image, factor: float) -> Image.Image:
    rgba = image.convert("RGBA").copy()
    alpha = np.asarray(rgba.getchannel("A"), dtype=np.float32)
    alpha = np.clip(alpha * factor, 0, 255).astype(np.uint8)
    rgba.putalpha(Image.fromarray(alpha, mode="L"))
    return rgba


def trim_alpha(image: Image.Image, threshold: int = 3) -> Image.Image:
    rgba = image.convert("RGBA")
    alpha = np.asarray(rgba.getchannel("A"))
    ys, xs = np.where(alpha > threshold)
    if len(xs) == 0:
        raise ValueError("Generated source cell is empty after alpha trim")
    return rgba.crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def split_bank(path: Path, columns: int, rows: int) -> list[Image.Image]:
    image = Image.open(path).convert("RGBA")
    cell_w = image.width / columns
    cell_h = image.height / rows
    pieces: list[Image.Image] = []
    for row in range(rows):
        for column in range(columns):
            x0 = int(round(column * cell_w + cell_w * 0.04))
            y0 = int(round(row * cell_h + cell_h * 0.04))
            x1 = int(round((column + 1) * cell_w - cell_w * 0.04))
            y1 = int(round((row + 1) * cell_h - cell_h * 0.04))
            pieces.append(trim_alpha(image.crop((x0, y0, x1, y1))))
    return pieces


def neutralize_piece(image: Image.Image, tint: tuple[int, int, int], contrast: float = 1.0) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.float32).copy()
    luminance = rgba[:, :, :3] @ np.array([0.2126, 0.7152, 0.0722], dtype=np.float32)
    luminance = np.clip((luminance - 128.0) * contrast + 150.0, 18.0, 255.0) / 255.0
    tint_array = np.array(tint, dtype=np.float32)
    rgba[:, :, :3] = np.clip(luminance[:, :, None] * tint_array[None, None, :], 0, 255)
    return Image.fromarray(rgba.astype(np.uint8), mode="RGBA")


def paste_piece(
    canvas: Image.Image,
    piece: Image.Image,
    center: tuple[float, float],
    width: float,
    angle: float,
    opacity: float,
    mirror: bool = False,
) -> None:
    source = ImageOps.mirror(piece) if mirror else piece
    aspect = source.height / max(1, source.width)
    target_w = max(2, int(round(width)))
    target_h = max(2, int(round(target_w * aspect)))
    resized = source.resize((target_w, target_h), Image.Resampling.LANCZOS)
    rotated = resized.rotate(angle, resample=Image.Resampling.BICUBIC, expand=True)
    rotated = alpha_multiply(rotated, max(0.0, min(1.0, opacity)))
    x = int(round(center[0] - rotated.width / 2))
    y = int(round(center[1] - rotated.height / 2))
    canvas.alpha_composite(rotated, (x, y))


def ss_canvas(draw_callback: Callable[[ImageDraw.ImageDraw, int], None]) -> Image.Image:
    large = Image.new("RGBA", (SIZE * AA, SIZE * AA), (0, 0, 0, 0))
    draw = ImageDraw.Draw(large, "RGBA")
    draw_callback(draw, AA)
    return large.resize((SIZE, SIZE), Image.Resampling.LANCZOS)


def sbox(box: Iterable[float], scale: int) -> tuple[int, ...]:
    return tuple(int(round(value * scale)) for value in box)


def spoints(points: Iterable[tuple[float, float]], scale: int) -> list[tuple[int, int]]:
    return [(int(round(x * scale)), int(round(y * scale))) for x, y in points]


def line(
    draw: ImageDraw.ImageDraw,
    points: Iterable[tuple[float, float]],
    color: tuple[int, int, int, int],
    width: float,
    scale: int,
) -> None:
    draw.line(spoints(points, scale), fill=color, width=max(1, int(round(width * scale))), joint="curve")


def static_layer(effect: str, layer: str) -> Image.Image:
    def draw_layer(draw: ImageDraw.ImageDraw, scale: int) -> None:
        warm_white = (220, 224, 216, 215)
        cool_white = (190, 202, 203, 205)
        amber = (187, 137, 55, 220)
        muted_green = (112, 145, 123, 200)
        reject = (157, 77, 66, 215)
        dim = (120, 128, 126, 150)

        if effect == "InteractionTarget" and layer == "EdgeTrace":
            line(draw, [(174, 139), (143, 153), (125, 181)], cool_white, 3, scale)
            line(draw, [(125, 181), (111, 219), (108, 257)], warm_white, 6, scale)
            line(draw, [(109, 282), (119, 326), (145, 354)], warm_white, 5, scale)
            line(draw, [(338, 139), (369, 153), (387, 181)], cool_white, 3, scale)
            line(draw, [(387, 181), (401, 219), (404, 257)], warm_white, 6, scale)
            line(draw, [(403, 282), (393, 326), (367, 354)], warm_white, 5, scale)
            line(draw, [(213, 128), (242, 124)], (221, 224, 215, 145), 2.5, scale)
            line(draw, [(270, 124), (299, 128)], (221, 224, 215, 125), 2.5, scale)
        elif effect == "InteractionTarget" and layer == "ContactBracket":
            line(draw, [(150, 355), (150, 379), (184, 379)], cool_white, 6, scale)
            line(draw, [(362, 355), (362, 379), (328, 379)], cool_white, 6, scale)
            line(draw, [(216, 397), (296, 397)], amber, 5, scale)
            line(draw, [(230, 386), (282, 386)], (220, 224, 216, 150), 2, scale)
        elif effect == "InteractionTarget" and layer == "MaterialWink":
            line(draw, [(132, 176), (154, 159), (169, 157)], (230, 230, 218, 170), 3, scale)
            line(draw, [(342, 157), (358, 160), (379, 178)], (230, 230, 218, 150), 3, scale)
            line(draw, [(112, 292), (124, 306)], amber, 3, scale)
            line(draw, [(400, 291), (389, 305)], amber, 3, scale)
            draw.ellipse(sbox((248, 132, 256, 140), scale), fill=(230, 230, 218, 180))
        elif effect == "InvalidRejected" and layer == "ReticleInterruption":
            line(draw, [(112, 224), (184, 224)], reject, 9, scale)
            line(draw, [(328, 224), (400, 224)], reject, 9, scale)
            line(draw, [(152, 242), (205, 242)], (184, 102, 86, 130), 3, scale)
            line(draw, [(307, 242), (360, 242)], (184, 102, 86, 130), 3, scale)
            line(draw, [(247, 126), (247, 176)], reject, 6, scale)
        elif effect == "InvalidRejected" and layer == "TargetDropout":
            draw.arc(sbox((116, 104, 396, 410), scale), 145, 186, fill=dim, width=5 * scale)
            draw.arc(sbox((116, 104, 396, 410), scale), 354, 26, fill=dim, width=5 * scale)
            draw.arc(sbox((143, 138, 369, 385), scale), 211, 244, fill=(103, 109, 108, 110), width=3 * scale)
            draw.arc(sbox((143, 138, 369, 385), scale), 295, 328, fill=(103, 109, 108, 110), width=3 * scale)
        elif effect == "InvalidRejected" and layer == "ReverseEcho":
            line(draw, [(177, 306), (203, 294), (219, 272)], reject, 4, scale)
            line(draw, [(335, 306), (309, 294), (293, 272)], reject, 4, scale)
            line(draw, [(192, 329), (224, 315)], (127, 76, 69, 125), 2, scale)
            line(draw, [(320, 329), (288, 315)], (127, 76, 69, 125), 2, scale)
        elif effect == "AllyReady" and layer == "GazeTick":
            line(draw, [(152, 211), (184, 187)], amber, 6, scale)
            line(draw, [(174, 241), (207, 217)], (210, 166, 84, 205), 5, scale)
            line(draw, [(197, 271), (228, 249)], (220, 201, 154, 160), 3, scale)
            line(draw, [(360, 205), (343, 192)], (214, 185, 120, 145), 3, scale)
        elif effect == "AllyReady" and layer == "PostureCue":
            draw.arc(sbox((172, 120, 372, 364), scale), 202, 292, fill=cool_white, width=5 * scale)
            line(draw, [(178, 296), (158, 321)], (166, 177, 177, 155), 3, scale)
            line(draw, [(343, 284), (370, 303)], (166, 177, 177, 155), 3, scale)
            line(draw, [(264, 137), (283, 137)], amber, 4, scale)
        elif effect == "AllyReady" and layer == "FootContact":
            draw.arc(sbox((163, 326, 255, 396), scale), 190, 344, fill=(176, 188, 184, 185), width=5 * scale)
            draw.arc(sbox((260, 326, 352, 396), scale), 196, 350, fill=(176, 188, 184, 185), width=5 * scale)
            line(draw, [(200, 403), (238, 403)], amber, 3, scale)
            line(draw, [(278, 403), (316, 403)], amber, 3, scale)
        elif effect == "ExitRoute" and layer == "PracticalLamp":
            draw.rounded_rectangle(sbox((144, 92, 368, 137), scale), radius=8 * scale,
                                   outline=(111, 128, 125, 220), width=5 * scale)
            draw.rounded_rectangle(sbox((171, 104, 341, 124), scale), radius=6 * scale,
                                   fill=(222, 225, 206, 225))
            line(draw, [(182, 113), (245, 113)], (244, 239, 208, 235), 5, scale)
            line(draw, [(267, 113), (330, 113)], (244, 239, 208, 190), 5, scale)
        elif effect == "ExitRoute" and layer == "SignHardware":
            line(draw, [(151, 174), (151, 332)], muted_green, 8, scale)
            line(draw, [(361, 174), (361, 332)], muted_green, 8, scale)
            line(draw, [(151, 174), (206, 174)], (150, 174, 158, 190), 6, scale)
            line(draw, [(306, 174), (361, 174)], (150, 174, 158, 190), 6, scale)
            draw.rounded_rectangle(sbox((199, 159, 313, 188), scale), radius=4 * scale,
                                   outline=(114, 128, 121, 180), width=3 * scale)
        elif effect == "ExitRoute" and layer == "RouteEdge":
            line(draw, [(173, 204), (173, 369)], (121, 163, 137, 185), 4, scale)
            line(draw, [(339, 204), (339, 369)], (121, 163, 137, 185), 4, scale)
            line(draw, [(205, 386), (307, 386)], amber, 5, scale)
            line(draw, [(231, 371), (281, 371)], (220, 224, 216, 165), 2, scale)
            line(draw, [(194, 233), (205, 222)], (201, 215, 204, 140), 3, scale)
            line(draw, [(318, 222), (329, 233)], (201, 215, 204, 140), 3, scale)
        else:
            raise KeyError((effect, layer))

    result = ss_canvas(draw_layer)
    if np.count_nonzero(np.asarray(result.getchannel("A"))) == 0:
        raise RuntimeError(f"Empty static layer: {effect}/{layer}")
    return result


def vector_contact_frame(
    effect: str,
    layer: str,
    index: int,
) -> Image.Image:
    t = index / 15.0

    def draw_frame(draw: ImageDraw.ImageDraw, scale: int) -> None:
        white = (221, 222, 211, int(255 * max(0.08, 1.0 - t * 0.90)))
        cool = (163, 176, 175, int(220 * max(0.08, 1.0 - t * 0.82)))
        amber = (180, 132, 53, int(210 * max(0.07, 1.0 - t * 0.88)))
        rubber = (67, 65, 60, int(220 * max(0.08, 1.0 - t * 0.76)))

        if effect == "CatchConfirm" and layer == "PrimaryCompression":
            spread = 34 + 92 * (1.0 - math.exp(-5.0 * t))
            height = 19 - 9 * min(1.0, t * 2.4)
            draw.arc(sbox((256 - spread, 256 - height, 256 + spread, 256 + height), scale),
                     188, 351, fill=white, width=max(2, int((7 - 3 * t) * scale)))
            line(draw, [(219 - 24 * t, 247), (250, 256)], cool, 4.5 - 1.5 * t, scale)
            line(draw, [(262, 256), (303 + 42 * t, 247 - 7 * t)], white, 5 - 2 * t, scale)
            if index < 5:
                line(draw, [(253, 228), (258, 246)], amber, 4, scale)
        elif effect == "BinderImpact" and layer == "PrimaryTransient":
            origin = (210, 323)
            extension = 28 + 108 * min(1.0, t * 2.2)
            line(draw, [origin, (210 + extension, 292 - 18 * t)], white, 5 - 2 * t, scale)
            line(draw, [origin, (207 + extension * 0.78, 319)], cool, 4 - 1.4 * t, scale)
            line(draw, [origin, (205 + extension * 0.62, 342 + 8 * t)], amber, 3 - t, scale)
            draw.arc(sbox((177, 304, 244 + 48 * t, 348), scale), 192, 342,
                     fill=(185, 176, 150, white[3]), width=max(2, int((5 - 2 * t) * scale)))
        elif effect == "ChairSkid" and layer == "CasterBite":
            shift = 20 * min(1.0, t * 2.5)
            draw.arc(sbox((163 + shift, 300, 253 + shift, 358), scale), 185, 347,
                     fill=rubber, width=max(2, int((9 - 3 * t) * scale)))
            draw.arc(sbox((250 + shift, 300, 340 + shift, 358), scale), 185, 347,
                     fill=(85, 80, 72, rubber[3]), width=max(2, int((9 - 3 * t) * scale)))
            line(draw, [(196 + shift, 354), (327 + shift + 35 * t, 354)],
                 (105, 99, 87, int(170 * max(0.08, 1 - t))), 3, scale)
            if index < 4:
                line(draw, [(335 + shift, 328), (358 + shift, 322)],
                     (203, 207, 200, 170), 2.5, scale)
        elif effect == "PaperBurst" and layer == "PrimarySnap":
            opening = 12 + 47 * min(1.0, t * 2.0)
            line(draw, [(219, 322), (299 + opening, 285 - opening * 0.35)], white, 6 - 2 * t, scale)
            line(draw, [(219, 322), (298 + opening * 0.8, 326 + opening * 0.25)], cool, 5 - 1.7 * t, scale)
            line(draw, [(226, 313), (278 + opening * 0.5, 303)], amber, 2.5, scale)
        else:
            raise KeyError((effect, layer))

    return ss_canvas(draw_frame)


def physical_frames(
    effect: str,
    layer: str,
    dust: list[Image.Image],
    rubber: list[Image.Image],
    paper: list[Image.Image],
) -> list[Image.Image]:
    frames: list[Image.Image] = []
    dust_neutral = [neutralize_piece(piece, (194, 186, 164), 1.05) for piece in dust]
    rubber_neutral = [neutralize_piece(piece, (92, 87, 76), 1.18) for piece in rubber]
    paper_neutral = [neutralize_piece(piece, (234, 231, 215), 0.92) for piece in paper]

    for index in range(16):
        t = index / 15.0
        canvas = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))

        if layer in {"PrimaryCompression", "PrimaryTransient", "CasterBite", "PrimarySnap"}:
            canvas.alpha_composite(vector_contact_frame(effect, layer, index))

        elif effect == "CatchConfirm" and layer == "PaperSnap":
            envelope = max(0.08, math.sin(math.pi * min(1.0, t * 1.05)))
            for piece_index, delay, vx, vy, width, angle in [
                (6, 0.00, 82, -34, 75, -12),
                (11, 0.05, 102, 10, 67, 8),
                (13, 0.12, 62, 28, 54, 16),
            ]:
                q = max(0.0, min(1.0, (t - delay) / max(0.01, 1.0 - delay)))
                paste_piece(canvas, paper_neutral[piece_index],
                            (226 + vx * q, 275 + vy * q + 24 * q * q),
                            width * (0.55 + 0.45 * min(1.0, q * 2.4)), angle + 18 * q,
                            envelope * (0.45 + 0.55 * q))
        elif effect == "CatchConfirm" and layer == "DirectionalSettle":
            q = min(1.0, t * 1.4)
            paste_piece(canvas, dust_neutral[3], (260 + 62 * q, 320 + 9 * q),
                        90 + 58 * q, 0, 0.34 * max(0.12, 1 - t))
            for offset, delay in [(-31, 0.0), (5, 0.08), (38, 0.14)]:
                local = max(0.0, min(1.0, (t - delay) / (1 - delay)))
                line_image = ss_canvas(lambda d, s, o=offset, ql=local: line(
                    d, [(246 + o + 35 * ql, 287 + 28 * ql), (259 + o + 54 * ql, 286 + 32 * ql)],
                    (205, 204, 189, int(155 * max(0.08, 1 - local))), 2.2, s))
                canvas.alpha_composite(line_image)
        elif effect == "BinderImpact" and layer == "DirectionalDust":
            piece = dust_neutral[[0, 5, 7, 10][min(3, index // 4)]]
            growth = 0.22 + 0.78 * (1.0 - math.exp(-5.0 * t))
            fade = max(0.08, 1.0 - max(0.0, t - 0.48) / 0.52)
            paste_piece(canvas, piece, (223 + 91 * t, 337 - 31 * t),
                        72 + 224 * growth, -7, 0.72 * fade)
        elif effect == "BinderImpact" and layer == "FiberSettle":
            for piece_index, delay, vx, vy, width, angle in [
                (16, 0.02, 70, -54, 38, -18),
                (7, 0.11, 98, -13, 31, 15),
                (15, 0.18, 48, 8, 26, 29),
                (19, 0.24, 115, -40, 22, -32),
            ]:
                q = max(0.0, min(1.0, (t - delay) / (1 - delay)))
                opacity = (0.28 + 0.60 * min(1.0, q * 3.0)) * max(0.09, 1 - t * 0.86)
                paste_piece(canvas, paper_neutral[piece_index],
                            (214 + vx * q, 321 + vy * q + 73 * q * q), width, angle + 55 * q, opacity)
        elif effect == "ChairSkid" and layer == "RubberBody":
            piece = rubber_neutral[[1, 4, 6, 10][min(3, index // 4)]]
            growth = 0.18 + 0.82 * (1.0 - math.exp(-4.5 * t))
            fade = max(0.10, 1.0 - max(0.0, t - 0.68) / 0.32)
            paste_piece(canvas, piece, (202 + 107 * t, 338), 78 + 268 * growth, 0,
                        0.90 * fade)
            paste_piece(canvas, rubber_neutral[2], (233 + 80 * t, 327), 108, 0,
                        0.34 * fade)
        elif effect == "ChairSkid" and layer == "ContactSettle":
            fade = max(0.08, 1.0 - t * 0.88)
            paste_piece(canvas, dust_neutral[12], (253 + 64 * t, 350), 128 + 95 * t, 0,
                        0.38 * fade)
            paste_piece(canvas, rubber_neutral[7], (214 + 86 * t, 359), 74 + 66 * t, 0,
                        0.42 * fade)
        elif effect == "PaperBurst" and layer == "PageFan":
            for piece_index, delay, vx, vy, width, angle, spin in [
                (3, 0.00, 82, -112, 84, -20, -18),
                (5, 0.04, 133, -67, 76, 12, 39),
                (11, 0.09, 157, -20, 73, 5, -22),
                (14, 0.14, 102, 35, 61, 22, 46),
                (16, 0.21, 58, -20, 46, -28, -34),
            ]:
                q = max(0.0, min(1.0, (t - delay) / (1 - delay)))
                opacity = (0.28 + 0.72 * min(1.0, q * 3.0)) * max(0.10, 1 - t * 0.78)
                paste_piece(canvas, paper_neutral[piece_index],
                            (203 + vx * q, 320 + vy * q + 62 * q * q), width,
                            angle + spin * q, opacity)
        elif effect == "PaperBurst" and layer == "FlutterSettle":
            for piece_index, delay, x, drift, width, angle in [
                (6, 0.00, 215, 40, 45, -13),
                (12, 0.07, 265, -23, 38, 18),
                (7, 0.13, 322, 33, 33, -25),
                (18, 0.22, 364, -18, 28, 29),
            ]:
                q = max(0.0, min(1.0, (t - delay) / (1 - delay)))
                opacity = (0.22 + 0.75 * min(1.0, q * 2.8)) * max(0.10, 1 - t * 0.76)
                y = 218 + 153 * q + 10 * math.sin(q * math.pi * 2.0 + piece_index)
                paste_piece(canvas, paper_neutral[piece_index],
                            (x + drift * q, y), width, angle + 80 * q, opacity)
        else:
            raise KeyError((effect, layer))

        if np.count_nonzero(np.asarray(canvas.getchannel("A"))) < 32:
            raise RuntimeError(f"Sparse/empty physical frame: {effect}/{layer}/{index}")
        frames.append(canvas)
    return frames


def assemble_flipbook(frames: list[Image.Image]) -> Image.Image:
    if len(frames) != 16:
        raise ValueError("Premium flipbooks require exactly 16 frames")
    atlas = Image.new("RGBA", (ATLAS_SIZE, ATLAS_SIZE), (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        # Unity ParticleSystem whole-sheet frame 0 samples the lower-left cell.
        # Store chronological frames bottom row first, left-to-right, then upward.
        column = index % GRID
        chronological_row = index // GRID
        image_row = GRID - 1 - chronological_row
        atlas.alpha_composite(frame, (column * SIZE, image_row * SIZE))
    return atlas


def extract_unity_frame(atlas: Image.Image, index: int) -> Image.Image:
    column = index % GRID
    chronological_row = index // GRID
    image_row = GRID - 1 - chronological_row
    return atlas.crop((column * SIZE, image_row * SIZE,
                       (column + 1) * SIZE, (image_row + 1) * SIZE))


def transparent_margin(image: Image.Image) -> int:
    alpha = np.asarray(image.convert("RGBA").getchannel("A"))
    ys, xs = np.where(alpha > 0)
    if len(xs) == 0:
        return min(image.size)
    return int(min(xs.min(), image.width - 1 - xs.max(), ys.min(), image.height - 1 - ys.max()))


def alpha_metrics(image: Image.Image) -> dict[str, int]:
    alpha = np.asarray(image.convert("RGBA").getchannel("A"))
    return {
        "alpha_nonzero_pixels": int(np.count_nonzero(alpha)),
        "alpha_partial_pixels": int(np.count_nonzero((alpha > 0) & (alpha < 255))),
        "transparent_margin_px": transparent_margin(image),
    }


def palette_risk_pixels(image: Image.Image) -> dict[str, int]:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.int16)
    active = rgba[:, :, 3] > 8
    magenta = active & (rgba[:, :, 0] > 210) & (rgba[:, :, 2] > 190) & (rgba[:, :, 1] < 90)
    green_spill = active & (rgba[:, :, 1] > rgba[:, :, 0] * 1.45) & (rgba[:, :, 1] > rgba[:, :, 2] * 1.45)
    cyan = active & (rgba[:, :, 1] > 200) & (rgba[:, :, 2] > 200) & (rgba[:, :, 0] < 90)
    return {
        "magenta_pixels": int(np.count_nonzero(magenta)),
        "green_spill_pixels": int(np.count_nonzero(green_spill)),
        "neon_cyan_pixels": int(np.count_nonzero(cyan)),
    }


def checkerboard(size: tuple[int, int], square: int = 24) -> Image.Image:
    image = Image.new("RGBA", size, (36, 39, 40, 255))
    draw = ImageDraw.Draw(image)
    for y in range(0, size[1], square):
        for x in range(0, size[0], square):
            value = 49 if ((x // square) + (y // square)) % 2 == 0 else 69
            draw.rectangle((x, y, min(size[0], x + square), min(size[1], y + square)),
                           fill=(value, value + 2, value + 1, 255))
    return image


def font(size: int) -> ImageFont.ImageFont:
    for candidate in ["C:/Windows/Fonts/segoeui.ttf", "DejaVuSans.ttf"]:
        try:
            return ImageFont.truetype(candidate, size=size)
        except OSError:
            continue
    return ImageFont.load_default()


def composite_frames(effect: str, outputs: dict[str, list[Path]], frame_index: int) -> Image.Image:
    result = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    for path in outputs[effect]:
        image = Image.open(path).convert("RGBA")
        if image.size == (ATLAS_SIZE, ATLAS_SIZE):
            image = extract_unity_frame(image, frame_index)
        result.alpha_composite(image)
    return result


def build_source_contact_sheet(outputs: dict[str, list[Path]]) -> Image.Image:
    sheet = Image.new("RGBA", (2560, 1440), (17, 20, 21, 255))
    draw = ImageDraw.Draw(sheet)
    title_font = font(28)
    label_font = font(18)
    columns = 4
    tile_w, tile_h = 640, 650
    sample_frames = [1, 4, 8, 13]
    for index, effect in enumerate(EFFECT_ORDER):
        x0 = (index % columns) * tile_w
        y0 = (index // columns) * tile_h + 100
        draw.text((x0 + 24, y0 - 42), f"{index + 1:02d}  {effect}", fill=(211, 217, 214, 255), font=title_font)
        if effect in PHYSICAL_LAYERS:
            for sample, frame_index in enumerate(sample_frames):
                preview = checkerboard((280, 230), 18)
                composite = composite_frames(effect, outputs, frame_index)
                composite.thumbnail((220, 220), Image.Resampling.LANCZOS)
                preview.alpha_composite(composite, ((280 - composite.width) // 2, (230 - composite.height) // 2))
                panel_x = sample % 2
                panel_y = sample // 2
                px = x0 + 24 + panel_x * 296
                py = y0 + 28 + panel_y * 270
                sheet.alpha_composite(preview, (px, py))
                draw.text((px + 118, py + 236), f"F{frame_index:02d}", fill=(159, 166, 164, 255), font=label_font)
        else:
            preview = checkerboard((590, 520), 26)
            composite = composite_frames(effect, outputs, 8)
            composite.thumbnail((480, 480), Image.Resampling.LANCZOS)
            preview.alpha_composite(composite, ((590 - composite.width) // 2, (520 - composite.height) // 2))
            sheet.alpha_composite(preview, (x0 + 24, y0 + 28))
            draw.text((x0 + 24, y0 + 556), "3-LAYER COMPOSITE", fill=(159, 166, 164, 255), font=label_font)
    draw.text((30, 18), "SHOWCASE VFX PREMIUM REVISION v002  /  REVIEW  /  OWNER APPROVAL PENDING",
              fill=(206, 211, 208, 255), font=title_font)
    return sheet


def build_layer_sheet(outputs: dict[str, list[Path]]) -> Image.Image:
    sheet = Image.new("RGBA", (2048, 2688), (18, 21, 22, 255))
    draw = ImageDraw.Draw(sheet)
    label_font = font(18)
    title_font = font(25)
    draw.text((28, 14), "PREMIUM v002 SOLO LAYERS + COMPOSITE  /  REVIEW",
              fill=(208, 214, 211, 255), font=title_font)
    sample_index = 6
    panel_size = 278
    for row, effect in enumerate(EFFECT_ORDER):
        y0 = 66 + row * 326
        draw.text((28, y0 + 4), f"{row + 1:02d} {effect}", fill=(188, 197, 194, 255), font=label_font)
        layers: list[Image.Image] = []
        for path in outputs[effect]:
            image = Image.open(path).convert("RGBA")
            layers.append(extract_unity_frame(image, sample_index) if image.width == ATLAS_SIZE else image)
        layers.append(composite_frames(effect, outputs, sample_index))
        for column, image in enumerate(layers):
            panel = checkerboard((panel_size, panel_size), 20)
            fitted = image.copy()
            fitted.thumbnail((250, 250), Image.Resampling.LANCZOS)
            panel.alpha_composite(fitted, ((panel_size - fitted.width) // 2, (panel_size - fitted.height) // 2))
            x0 = 350 + column * 410
            sheet.alpha_composite(panel, (x0, y0))
            name = "COMPOSITE" if column == 3 else f"SOLO L{column + 1}"
            draw.text((x0 + 72, y0 + 286), name, fill=(145, 154, 152, 255), font=label_font)
    return sheet


def build_alpha_sheet(outputs: dict[str, list[Path]]) -> Image.Image:
    sheet = Image.new("RGBA", (2048, 2176), (15, 17, 18, 255))
    draw = ImageDraw.Draw(sheet)
    title_font = font(25)
    label_font = font(17)
    draw.text((28, 14), "PREMIUM v002 GRAYSCALE / ALPHA / EDGE AUDIT  /  REVIEW",
              fill=(208, 214, 211, 255), font=title_font)
    for index, effect in enumerate(EFFECT_ORDER):
        row = index // 2
        column = index % 2
        x0 = column * 1024
        y0 = 66 + row * 520
        composite = composite_frames(effect, outputs, 6)
        gray = ImageOps.grayscale(composite.convert("RGB"))
        gray_rgba = Image.merge("RGBA", (gray, gray, gray, composite.getchannel("A")))
        alpha = composite.getchannel("A")
        alpha_rgba = Image.merge("RGBA", (alpha, alpha, alpha, Image.new("L", alpha.size, 255)))
        for panel_index, image in enumerate([gray_rgba, alpha_rgba]):
            panel = checkerboard((450, 430), 24) if panel_index == 0 else Image.new("RGBA", (450, 430), (5, 5, 5, 255))
            fitted = image.copy()
            fitted.thumbnail((410, 410), Image.Resampling.LANCZOS)
            panel.alpha_composite(fitted, ((450 - fitted.width) // 2, (430 - fitted.height) // 2))
            sheet.alpha_composite(panel, (x0 + 35 + panel_index * 480, y0 + 52))
        draw.text((x0 + 35, y0 + 12), f"{index + 1:02d} {effect}", fill=(187, 195, 192, 255), font=label_font)
        draw.text((x0 + 188, y0 + 487), "GRAYSCALE", fill=(139, 148, 146, 255), font=label_font)
        draw.text((x0 + 684, y0 + 487), "ALPHA", fill=(207, 212, 210, 255), font=label_font)
    return sheet


def validate_candidate(path: Path, flipbook: bool) -> dict[str, object]:
    if path.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n":
        raise RuntimeError(f"PNG signature failure: {relative(path)}")
    with Image.open(path) as image:
        image = image.convert("RGBA")
        expected = (ATLAS_SIZE, ATLAS_SIZE) if flipbook else (SIZE, SIZE)
        if image.size != expected:
            raise RuntimeError(f"Dimension failure: {relative(path)} {image.size}")
        metrics = alpha_metrics(image)
        if metrics["alpha_nonzero_pixels"] <= 0 or metrics["alpha_partial_pixels"] <= 0:
            raise RuntimeError(f"Alpha coverage failure: {relative(path)}")
        risks = palette_risk_pixels(image)
        if any(risks.values()):
            raise RuntimeError(f"Palette/spill risk: {relative(path)} {risks}")
        frame_hashes: list[str] = []
        frame_alpha: list[int] = []
        if flipbook:
            for index in range(16):
                frame = extract_unity_frame(image, index)
                frame_metrics = alpha_metrics(frame)
                if frame_metrics["transparent_margin_px"] < MIN_MARGIN:
                    raise RuntimeError(
                        f"Flipbook cell margin failure: {relative(path)} frame={index} "
                        f"margin={frame_metrics['transparent_margin_px']}")
                if frame_metrics["alpha_nonzero_pixels"] < 32:
                    raise RuntimeError(f"Sparse flipbook frame: {relative(path)} frame={index}")
                frame_hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
                frame_alpha.append(frame_metrics["alpha_nonzero_pixels"])
            if len(set(frame_hashes)) != 16:
                raise RuntimeError(f"Duplicate flipbook frames: {relative(path)}")
        elif metrics["transparent_margin_px"] < MIN_MARGIN:
            raise RuntimeError(f"Texture margin failure: {relative(path)}")
        return {
            "path": relative(path),
            "dimensions": f"{image.width}x{image.height}",
            "mode": "RGBA",
            **metrics,
            **risks,
            "frame_order": "UNITY_WHOLE_SHEET_LOWER_LEFT_ROW_MAJOR" if flipbook else "N/A",
            "frame_hash_unique": len(set(frame_hashes)) if flipbook else "N/A",
            "frame_alpha_counts": ";".join(str(value) for value in frame_alpha) if flipbook else "N/A",
            "sha256": sha256(path),
            "bytes": path.stat().st_size,
        }


def provenance_for(path: Path) -> tuple[str, str, str, str]:
    name = path.name
    if "ImageGen_v002" in name:
        prompt_id = next(value for key, value in PROMPT_IDS.items() if key in name)
        return (
            "ORIGINAL_AI_GENERATED_SOURCE",
            "OpenAI built-in image generation; no reference image; Human Owner-authorized prompt",
            prompt_id,
            "NONE",
        )
    if "AlphaExtract_v002" in name:
        original = name.replace("_AlphaExtract_", "_ImageGen_")
        prompt_id = next(value for key, value in PROMPT_IDS.items() if key in name)
        return (
            "ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE",
            "OpenAI built-in source plus imagegen Skill chroma-key removal with soft matte and despill",
            prompt_id,
            f"ArtSource/VFX/Showcase/Source/{original}",
        )
    if path.parent == TEXTURE_DIR:
        return (
            "ORIGINAL_DETERMINISTIC_VECTOR_RASTER",
            "Codex-authored deterministic supersampled geometric mask",
            "NONE",
            relative(SCRIPT_PATH),
        )
    if path.parent == FLIPBOOK_DIR:
        return (
            "ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE",
            "Codex-authored deterministic animation using original ImageGen element banks plus geometric masks",
            "IG-VFX-SHW-PREMIUM-002-A/B/C",
            ";".join(relative(item) for item in ALPHA_EXTRACTS) + ";" + relative(SCRIPT_PATH),
        )
    if path.parent == PREVIEW_DIR:
        return (
            "ORIGINAL_DETERMINISTIC_REVIEW_COMPOSITE",
            "Deterministic Human Owner review preview assembled from v002 candidates",
            "NONE",
            "ALL PREMIUM v002 CANDIDATES",
        )
    return (
        "ORIGINAL_REPRODUCIBLE_SOURCE",
        "Codex-authored deterministic source",
        "NONE",
        "NONE",
    )


def write_ledger(paths: list[Path], validation_rows: dict[str, dict[str, object]]) -> Path:
    ledger_path = LICENSE_DIR / "VFX_Showcase_Premium_Provenance_Ledger_v002.csv"
    fields = [
        "asset_path", "asset_role", "layer_or_source_role", "provenance_classification",
        "creator_or_generator", "source_prompt_id", "source_inputs", "third_party_source",
        "exact_license_or_rights_basis", "commercial_use", "repository_redistribution",
        "derivative_work", "dimensions", "mode", "alpha_nonzero_pixels",
        "alpha_partial_pixels", "transparent_margin_px", "frame_order",
        "frame_hash_unique", "sha256", "bytes", "review_status",
    ]
    rows: list[dict[str, object]] = []
    for path in paths:
        classification, creator, prompt_id, source_inputs = provenance_for(path)
        metrics = validation_rows.get(relative(path), {})
        if path.suffix.lower() == ".png":
            with Image.open(path) as image:
                fallback_dimensions = f"{image.width}x{image.height}"
                fallback_mode = image.mode
        else:
            fallback_dimensions = "N/A"
            fallback_mode = "UTF-8 TEXT"
        if path.parent == TEXTURE_DIR:
            asset_role = "UNITY_REVIEW_TEXTURE_LAYER_CANDIDATE"
        elif path.parent == FLIPBOOK_DIR:
            asset_role = "UNITY_REVIEW_4X4_FLIPBOOK_LAYER_CANDIDATE"
        elif path.parent == PREVIEW_DIR:
            asset_role = "HUMAN_OWNER_REVIEW_PREVIEW"
        elif "ImageGen" in path.name:
            asset_role = "PRESERVED_IMAGE_GENERATION_SOURCE"
        elif "AlphaExtract" in path.name:
            asset_role = "STRAIGHT_ALPHA_ELEMENT_BANK_SOURCE"
        else:
            asset_role = "REPRODUCIBLE_BUILD_SOURCE"
        rows.append({
            "asset_path": relative(path),
            "asset_role": asset_role,
            "layer_or_source_role": path.stem,
            "provenance_classification": classification,
            "creator_or_generator": creator,
            "source_prompt_id": prompt_id,
            "source_inputs": source_inputs,
            "third_party_source": "NONE",
            "exact_license_or_rights_basis": "PROJECT ORIGINAL; NO THIRD-PARTY SOURCE; HUMAN OWNER AUTHORIZED CREATION",
            "commercial_use": "AUTHORIZED FOR BREAK NOTICE PROJECT AND CONTEST MATERIALS BY HUMAN OWNER",
            "repository_redistribution": "AUTHORIZED WITH PROJECT DELIVERY; NO EXTERNAL SOURCE RESTRICTION",
            "derivative_work": "AUTHORIZED",
            "dimensions": metrics.get("dimensions", fallback_dimensions),
            "mode": metrics.get("mode", fallback_mode),
            "alpha_nonzero_pixels": metrics.get("alpha_nonzero_pixels", "SOURCE / NOT RUNTIME METRIC"),
            "alpha_partial_pixels": metrics.get("alpha_partial_pixels", "SOURCE / NOT RUNTIME METRIC"),
            "transparent_margin_px": metrics.get("transparent_margin_px", "SOURCE / NOT RUNTIME METRIC"),
            "frame_order": metrics.get("frame_order", "N/A"),
            "frame_hash_unique": metrics.get("frame_hash_unique", "N/A"),
            "sha256": sha256(path),
            "bytes": path.stat().st_size,
            "review_status": "REVIEW / OWNER APPROVAL PENDING / GAMEPLAY INTEGRATION ELIGIBILITY NONE",
        })
    ledger_path.parent.mkdir(parents=True, exist_ok=True)
    with ledger_path.open("w", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fieldnames=fields, quoting=csv.QUOTE_ALL, lineterminator="\n")
        writer.writeheader()
        writer.writerows(rows)
    return ledger_path


def main() -> None:
    for required in IMAGEGEN_ORIGINALS + ALPHA_EXTRACTS:
        if not required.is_file():
            raise FileNotFoundError(required)
    for directory in (TEXTURE_DIR, FLIPBOOK_DIR, PREVIEW_DIR, LICENSE_DIR):
        directory.mkdir(parents=True, exist_ok=True)

    dust = split_bank(DUST_SOURCE, 4, 4)
    rubber = split_bank(RUBBER_SOURCE, 4, 4)
    paper = split_bank(PAPER_SOURCE, 5, 4)
    if (len(dust), len(rubber), len(paper)) != (16, 16, 20):
        raise RuntimeError("ImageGen source bank element count mismatch")

    outputs: dict[str, list[Path]] = {}
    candidate_paths: list[Path] = []
    for effect, layers in STATUS_LAYERS.items():
        outputs[effect] = []
        for layer, filename in layers:
            path = TEXTURE_DIR / filename
            save_png(sanitize_runtime_alpha(static_layer(effect, layer)), path)
            outputs[effect].append(path)
            candidate_paths.append(path)

    for effect, layers in PHYSICAL_LAYERS.items():
        outputs[effect] = []
        for layer, filename in layers:
            path = FLIPBOOK_DIR / filename
            save_png(
                sanitize_runtime_alpha(
                    assemble_flipbook(physical_frames(effect, layer, dust, rubber, paper))),
                path,
            )
            outputs[effect].append(path)
            candidate_paths.append(path)

    source_preview_paths = [
        PREVIEW_DIR / "VFX_Showcase_Premium_SourceContactSheet_v002.png",
        PREVIEW_DIR / "VFX_Showcase_Premium_LayerBreakdown_Source_v002.png",
        PREVIEW_DIR / "VFX_Showcase_Premium_GrayscaleAlphaAudit_Source_v002.png",
    ]
    save_png(build_source_contact_sheet(outputs), source_preview_paths[0])
    save_png(build_layer_sheet(outputs), source_preview_paths[1])
    save_png(build_alpha_sheet(outputs), source_preview_paths[2])

    unity_review_capture_paths = [
        PREVIEW_DIR / "VFX_Showcase_Premium_OldVsNew_v002.png",
        PREVIEW_DIR / "VFX_Showcase_Premium_LayerBreakdown_v002.png",
        PREVIEW_DIR / "VFX_Showcase_Premium_PhysicalVariations_v002.png",
        PREVIEW_DIR / "VFX_Showcase_Premium_GameplayCamera_v002.png",
        PREVIEW_DIR / "VFX_Showcase_Premium_GrayscaleReadability_v002.png",
    ]
    missing_review_captures = [path for path in unity_review_capture_paths if not path.is_file()]
    if missing_review_captures:
        raise FileNotFoundError(
            "Unity review captures must exist before the final provenance pass: "
            + ", ".join(relative(path) for path in missing_review_captures)
        )
    preview_paths = source_preview_paths + unity_review_capture_paths

    validation_rows: dict[str, dict[str, object]] = {}
    for path in candidate_paths:
        row = validate_candidate(path, path.parent == FLIPBOOK_DIR)
        validation_rows[relative(path)] = row

    provenance_paths = IMAGEGEN_ORIGINALS + ALPHA_EXTRACTS + candidate_paths + preview_paths + [SCRIPT_PATH]
    ledger_path = write_ledger(provenance_paths, validation_rows)
    result = {
        "status": "PASS",
        "review_status": "REVIEW / OWNER APPROVAL PENDING",
        "status_effects": len(STATUS_LAYERS),
        "physical_effects": len(PHYSICAL_LAYERS),
        "replacement_effects": len(EFFECT_ORDER),
        "layer_candidates": len(candidate_paths),
        "static_512_layers": sum(len(value) for value in STATUS_LAYERS.values()),
        "flipbook_2048_layers": sum(len(value) for value in PHYSICAL_LAYERS.values()),
        "flipbook_frames": sum(len(value) for value in PHYSICAL_LAYERS.values()) * 16,
        "provenance_rows": len(provenance_paths),
        "total_candidate_bytes": sum(path.stat().st_size for path in candidate_paths),
        "ledger": relative(ledger_path),
        "candidates": [validation_rows[relative(path)] for path in candidate_paths],
        "previews": [{"path": relative(path), "sha256": sha256(path), "bytes": path.stat().st_size}
                     for path in preview_paths],
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
