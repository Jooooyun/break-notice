"""Build the complete BREAK NOTICE Showcase VFX review pack v003.

The output is presentation-only.  It creates one three-layer candidate for
every locked Showcase cue plus a bounded, nonlethal impact/material audition
library.  It never creates gameplay hooks, damage, breakage, collision, or a
custom shader.  All outputs remain REVIEW / OWNER APPROVAL PENDING.
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


SIZE = 512
GRID = 4
ATLAS_SIZE = SIZE * GRID
AA = 4
MIN_MARGIN = 24

SCRIPT_PATH = Path(__file__).resolve()
SOURCE_DIR = SCRIPT_PATH.parent
SHOWCASE_DIR = SOURCE_DIR.parent
REPO_ROOT = SHOWCASE_DIR.parents[2]
TEXTURE_DIR = SHOWCASE_DIR / "Textures" / "Complete_v003"
FLIPBOOK_DIR = SHOWCASE_DIR / "Flipbooks" / "Complete_v003"
PREVIEW_DIR = SHOWCASE_DIR / "Preview" / "Complete_v003"
LICENSE_DIR = SHOWCASE_DIR / "Licenses"

CLOTH_ORIGINAL = SOURCE_DIR / "VFX_Showcase_CompleteClothImpactBank_ImageGen_v003.png"
CLOTH_SOURCE = SOURCE_DIR / "VFX_Showcase_CompleteClothImpactBank_AlphaExtract_v003.png"
OFFICE_ORIGINAL = SOURCE_DIR / "VFX_Showcase_CompleteOfficeContactBank_ImageGen_v003.png"
OFFICE_SOURCE = SOURCE_DIR / "VFX_Showcase_CompleteOfficeContactBank_AlphaExtract_v003.png"
DUST_SOURCE = SOURCE_DIR / "VFX_Showcase_PremiumDustBank_AlphaExtract_v002.png"
RUBBER_SOURCE = SOURCE_DIR / "VFX_Showcase_PremiumRubberBank_AlphaExtract_v002.png"
PAPER_SOURCE = SOURCE_DIR / "VFX_Showcase_PremiumPaperBank_AlphaExtract_v002.png"
DUST_ORIGINAL = SOURCE_DIR / "VFX_Showcase_PremiumDustBank_ImageGen_v002.png"
RUBBER_ORIGINAL = SOURCE_DIR / "VFX_Showcase_PremiumRubberBank_ImageGen_v002.png"
PAPER_ORIGINAL = SOURCE_DIR / "VFX_Showcase_PremiumPaperBank_ImageGen_v002.png"

PROMPT_IDS = {
    CLOTH_ORIGINAL.name: "IG-VFX-SHW-COMPLETE-003-A",
    OFFICE_ORIGINAL.name: "IG-VFX-SHW-COMPLETE-003-B",
    DUST_ORIGINAL.name: "IG-VFX-SHW-PREMIUM-002-A",
    RUBBER_ORIGINAL.name: "IG-VFX-SHW-PREMIUM-002-B",
    PAPER_ORIGINAL.name: "IG-VFX-SHW-PREMIUM-002-C",
}


@dataclass(frozen=True)
class EffectSpec:
    cue_id: str
    slug: str
    display_name: str
    family: str
    physical: bool
    material: str = "neutral"
    strength: float = 1.0
    angle: float = 0.0
    variant: int = 1


EFFECTS = [
    EffectSpec("VFX-SHW-ACQ-001", "BinderEligible", "BINDER ACQUISITION AVAILABLE", "eligible", False, variant=1),
    EffectSpec("VFX-SHW-ACQ-002", "ChairEligible", "CHAIR ACQUISITION AVAILABLE", "eligible", False, variant=2),
    EffectSpec("VFX-SHW-ACQ-003", "AcquisitionConfirmed", "ACQUISITION CONFIRMED", "confirm", False),
    EffectSpec("VFX-SHW-THR-001", "ThrowChargeStage1", "THROW CHARGE / STAGE 1", "charge", False, variant=1),
    EffectSpec("VFX-SHW-THR-002", "ThrowChargeStage2", "THROW CHARGE / STAGE 2", "charge", False, variant=2),
    EffectSpec("VFX-SHW-THR-003", "ThrowChargeFull", "THROW CHARGE / FULL", "charge", False, variant=3),
    EffectSpec("VFX-SHW-THR-004", "ThrowPhysicalRelease", "THROW PHYSICAL RELEASE", "physical", True, "cloth", 0.82, 8),
    EffectSpec("VFX-SHW-THR-005", "BinderPhysicalContact", "BINDER PHYSICAL CONTACT", "physical", True, "paper", 0.92, 12),
    EffectSpec("VFX-SHW-THR-006", "ChairPhysicalContact", "CHAIR PHYSICAL CONTACT", "physical", True, "rubber", 1.15, 4),
    EffectSpec("VFX-SHW-ALY-001", "AllyReady", "ALLY READY SUPPORT", "ready", False),
    EffectSpec("VFX-SHW-ALY-002", "QBinderSupplyAccepted", "Q BINDER SUPPLY ACCEPTED", "physical", True, "paper", 0.78, 15),
    EffectSpec("VFX-SHW-ALY-003", "QChairSupplyAccepted", "Q CHAIR SUPPLY ACCEPTED", "physical", True, "rubber", 0.96, 2),
    EffectSpec("VFX-SHW-ALY-004", "BinderCatch", "BINDER CATCH", "physical", True, "paper", 0.76, -4),
    EffectSpec("VFX-SHW-ALY-005", "ChairFeedRelease", "CHAIR FLOOR-SUPPORTED FEED", "physical", True, "rubber", 0.92, 0),
    EffectSpec("VFX-SHW-ALY-006", "ChairWheelFloorContact", "CHAIR WHEEL / FLOOR CONTACT", "physical", True, "rubber", 1.02, -2),
    EffectSpec("VFX-SHW-ALY-007", "SupplyRejected", "SUPPLY REJECTED", "reject", False, variant=1),
    EffectSpec("VFX-SHW-ALY-008", "SupplyPathRejected", "SUPPLY PATH REJECTED", "path_reject", False),
    EffectSpec("VFX-SHW-TEAM-001", "PassBadge", "PASS BADGE", "pass", False),
    EffectSpec("VFX-SHW-TEAM-002", "BraceBadge", "BRACE BADGE", "brace", False),
    EffectSpec("VFX-SHW-TEAM-003", "BreakBadge", "BREAK BADGE", "break_badge", False),
    EffectSpec("VFX-SHW-TEAM-004", "TeamReady", "TEAM READY", "team", False),
    EffectSpec("VFX-SHW-FIN-001", "HandoverCommit", "HANDOVER CRASH / COMMIT", "commit", False),
    EffectSpec("VFX-SHW-FIN-002", "HandoverChairSkid", "HANDOVER / CHAIR SKID", "physical", True, "rubber", 1.18, 1),
    EffectSpec("VFX-SHW-FIN-003", "HandoverChairRunnerContact", "HANDOVER / CHAIR TO RUNNER", "physical", True, "cloth", 1.12, 7),
    EffectSpec("VFX-SHW-FIN-004", "HandoverRunnerDrawerContact", "HANDOVER / RUNNER TO DRAWER", "physical", True, "office", 1.18, 5),
    EffectSpec("VFX-SHW-OBJ-001", "DrawerCausalImpact", "DRAWER CAUSAL IMPACT", "physical", True, "office", 1.22, 3),
    EffectSpec("VFX-SHW-OBJ-002", "DrawerRouteOpen", "DRAWER ROUTE STATE OPEN", "route", False),
    EffectSpec("VFX-SHW-OBJ-003", "ExitActivation", "EXIT / ROUTE ACTIVATION", "exit", False),
    EffectSpec("VFX-SHW-RUN-001", "RunnerPressureTelegraph", "RUNNER PRESSURE TELEGRAPH", "telegraph", False),
    EffectSpec("VFX-SHW-RUN-002", "RunnerPressureContact", "RUNNER PRESSURE CONTACT", "physical", True, "cloth", 0.94, 4),
    EffectSpec("VFX-SHW-RUN-003", "RunnerStaggerOpportunity", "RUNNER STAGGER / TAG READABILITY", "opportunity", False),
    EffectSpec("VFX-SHW-RUN-004", "RunnerGrabPressure", "RUNNER GRAB PRESSURE", "grab", False),
    EffectSpec("VFX-SHW-SYS-001", "ResetStart", "RESET START", "reset_start", False),
    EffectSpec("VFX-SHW-SYS-002", "ResetComplete", "RESET COMPLETE", "reset_complete", False),
    EffectSpec("VFX-SHW-SYS-003", "UiConfirmation", "UI CONFIRMATION", "ui_confirm", False),
    EffectSpec("VFX-SHW-SYS-004", "UiRejection", "UI REJECTION", "ui_reject", False),
    EffectSpec("VFX-LAB-HIT-001", "PunchLightCloth", "PUNCH LIGHT / CLOTH CONTACT", "physical", True, "cloth", 0.72, 10),
    EffectSpec("VFX-LAB-HIT-002", "PunchMediumCloth", "PUNCH MEDIUM / CLOTH CONTACT", "physical", True, "cloth", 0.96, 4),
    EffectSpec("VFX-LAB-HIT-003", "PunchStrongNonlethal", "PUNCH STRONG / NONLETHAL CONTACT", "physical", True, "cloth", 1.20, -2),
    EffectSpec("VFX-LAB-HIT-004", "PunchGlancing", "PUNCH GLANCING CONTACT", "physical", True, "cloth", 0.78, 24),
    EffectSpec("VFX-LAB-HIT-005", "PunchBraceBlock", "PUNCH / FOREARM BRACE BLOCK", "physical", True, "cloth", 1.02, -16),
    EffectSpec("VFX-LAB-HIT-006", "ShoulderBodyCheck", "SHOULDER / BODY CHECK", "physical", True, "cloth", 1.16, 2),
    EffectSpec("VFX-LAB-MAT-001", "HandDeskMiss", "HAND TO DESK / MISS CONTACT", "physical", True, "office", 0.88, -8),
    EffectSpec("VFX-LAB-MAT-002", "MetalDrawerContact", "PAINTED METAL DRAWER CONTACT", "physical", True, "office", 1.08, 6),
    EffectSpec("VFX-LAB-MAT-003", "ChairFramePlasticContact", "CHAIR FRAME / PLASTIC CONTACT", "physical", True, "rubber", 0.96, 0),
    EffectSpec("VFX-LAB-MAT-004", "BinderPaperEdgeContact", "BINDER / PAPER EDGE CONTACT", "physical", True, "paper", 0.82, 18),
    EffectSpec("VFX-LAB-MAT-005", "ObjectFloorDrop", "OFFICE OBJECT / FLOOR DROP", "physical", True, "office", 1.18, 72),
    EffectSpec("VFX-LAB-MAT-006", "WallPartitionContact", "WALL / PARTITION CONTACT", "physical", True, "office", 1.00, -6),
]


def relative(path: Path) -> str:
    return path.resolve().relative_to(REPO_ROOT).as_posix()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def stable_seed(value: str) -> int:
    return int(hashlib.sha256(value.encode("utf-8")).hexdigest()[:8], 16)


def save_png(image: Image.Image, path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    image.convert("RGBA").save(path, "PNG", optimize=True, compress_level=9)


def sanitize_runtime_alpha(image: Image.Image) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.uint8).copy()
    alpha = rgba[:, :, 3]
    active = alpha > 0
    red = rgba[:, :, 0].astype(np.int16)
    green = rgba[:, :, 1].astype(np.int16)
    blue = rgba[:, :, 2].astype(np.int16)
    spill = active & (green > red * 1.18) & (green > blue * 1.18)
    neutral = np.clip((red + blue) // 2, 0, 255).astype(np.uint8)
    rgba[:, :, 1][spill] = neutral[spill]
    rgba[:, :, 3][alpha <= 3] = 0
    invisible = rgba[:, :, 3] == 0
    rgba[:, :, :3][invisible] = 0
    near_black = (rgba[:, :, 3] > 0) & (np.max(rgba[:, :, :3], axis=2) < 8)
    rgba[:, :, :3][near_black] = 8
    return Image.fromarray(rgba, "RGBA")


def enforce_frame_margin(image: Image.Image, margin: int = MIN_MARGIN) -> Image.Image:
    """Keep the authored composition while guaranteeing per-cell mip padding."""
    if transparent_margin(image) >= margin:
        return image
    inner = SIZE - margin * 2
    resized = image.resize((inner, inner), Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    canvas.alpha_composite(resized, (margin, margin))
    return sanitize_runtime_alpha(canvas)


def trim_alpha(image: Image.Image, threshold: int = 3) -> Image.Image:
    rgba = image.convert("RGBA")
    alpha = np.asarray(rgba.getchannel("A"))
    ys, xs = np.where(alpha > threshold)
    if len(xs) == 0:
        raise ValueError("Source bank cell is empty after alpha trim")
    return rgba.crop((int(xs.min()), int(ys.min()), int(xs.max()) + 1, int(ys.max()) + 1))


def split_bank(path: Path, columns: int, rows: int) -> list[Image.Image]:
    image = Image.open(path).convert("RGBA")
    cell_w = image.width / columns
    cell_h = image.height / rows
    pieces = []
    for row in range(rows):
        for column in range(columns):
            pad_x = cell_w * 0.025
            pad_y = cell_h * 0.025
            box = (
                int(round(column * cell_w + pad_x)),
                int(round(row * cell_h + pad_y)),
                int(round((column + 1) * cell_w - pad_x)),
                int(round((row + 1) * cell_h - pad_y)),
            )
            pieces.append(trim_alpha(image.crop(box)))
    return pieces


def neutralize_piece(image: Image.Image, tint: tuple[int, int, int], contrast: float = 1.0) -> Image.Image:
    rgba = np.asarray(image.convert("RGBA"), dtype=np.float32).copy()
    luminance = rgba[:, :, :3] @ np.array([0.2126, 0.7152, 0.0722], dtype=np.float32)
    luminance = np.clip((luminance - 128) * contrast + 158, 20, 255) / 255.0
    rgba[:, :, :3] = np.clip(luminance[:, :, None] * np.array(tint)[None, None, :], 0, 255)
    return sanitize_runtime_alpha(Image.fromarray(rgba.astype(np.uint8), "RGBA"))


def alpha_multiply(image: Image.Image, factor: float) -> Image.Image:
    rgba = image.convert("RGBA").copy()
    alpha = np.asarray(rgba.getchannel("A"), dtype=np.float32)
    rgba.putalpha(Image.fromarray(np.clip(alpha * factor, 0, 255).astype(np.uint8), "L"))
    return rgba


def paste_piece(canvas: Image.Image, piece: Image.Image, center: tuple[float, float], width: float,
                angle: float, opacity: float, mirror: bool = False) -> None:
    source = ImageOps.mirror(piece) if mirror else piece
    target_w = max(3, int(round(width)))
    target_h = max(3, int(round(target_w * source.height / max(1, source.width))))
    resized = source.resize((target_w, target_h), Image.Resampling.LANCZOS)
    rotated = resized.rotate(angle, Image.Resampling.BICUBIC, expand=True)
    rotated = alpha_multiply(rotated, max(0.0, min(1.0, opacity)))
    x = int(round(center[0] - rotated.width / 2))
    y = int(round(center[1] - rotated.height / 2))
    canvas.alpha_composite(rotated, (x, y))


def ss_canvas(callback) -> Image.Image:
    large = Image.new("RGBA", (SIZE * AA, SIZE * AA), (0, 0, 0, 0))
    callback(ImageDraw.Draw(large, "RGBA"), AA)
    return large.resize((SIZE, SIZE), Image.Resampling.LANCZOS)


def spoints(points, scale: int):
    return [(int(round(x * scale)), int(round(y * scale))) for x, y in points]


def sbox(box, scale: int):
    return tuple(int(round(value * scale)) for value in box)


def line(draw, points, color, width: float, scale: int) -> None:
    draw.line(spoints(points, scale), fill=color, width=max(1, int(round(width * scale))), joint="curve")


def static_layer(spec: EffectSpec, layer_index: int) -> Image.Image:
    def draw_layer(draw: ImageDraw.ImageDraw, scale: int) -> None:
        white = (226, 228, 218, 238 if layer_index == 0 else 170)
        cool = (164, 177, 179, 215 if layer_index == 0 else 145)
        amber = (190, 139, 54, 230 if layer_index == 0 else 155)
        green = (105, 148, 121, 220 if layer_index == 0 else 150)
        red = (165, 78, 64, 225 if layer_index == 0 else 150)
        dim = (110, 119, 118, 150)
        f = spec.family

        if f == "eligible":
            inset = 126 + layer_index * 14
            extent = 62 - layer_index * 12
            for sx, sy in ((1, 1), (-1, 1), (1, -1), (-1, -1)):
                x = 256 + sx * (256 - inset)
                y = 256 + sy * (256 - inset)
                line(draw, [(x, y - sy * extent), (x, y), (x - sx * extent, y)],
                     white if layer_index == 0 else cool, 7 - layer_index * 1.5, scale)
            for tick in range(spec.variant):
                x = 238 + tick * 36 - (spec.variant - 1) * 18
                line(draw, [(x, 398 - layer_index * 13), (x + 22, 398 - layer_index * 13)], amber, 5, scale)
        elif f == "charge":
            radius = 122 - layer_index * 16
            count = spec.variant
            for tick in range(count):
                angle = math.radians(210 + tick * 60)
                p0 = (256 + math.cos(angle) * radius, 256 + math.sin(angle) * radius)
                p1 = (256 + math.cos(angle) * (radius - 35), 256 + math.sin(angle) * (radius - 35))
                line(draw, [p0, p1], amber if layer_index < 2 else white, 10 - layer_index * 2, scale)
            if layer_index == 2:
                line(draw, [(230, 142), (282, 142)], cool, 3, scale)
        elif f in {"confirm", "ui_confirm"}:
            offset = 92 - layer_index * 18
            line(draw, [(256 - offset, 230), (228, 256), (256 - offset, 282)], green, 7 - layer_index, scale)
            line(draw, [(256 + offset, 230), (284, 256), (256 + offset, 282)], green, 7 - layer_index, scale)
            line(draw, [(218 + layer_index * 15, 326), (294 - layer_index * 15, 326)], white, 4, scale)
            if f == "ui_confirm":
                # UI confirmation stays reticle-local and visibly distinct from
                # world-space acquisition compression.
                y = 184 + layer_index * 13
                line(draw, [(211 + layer_index * 8, y), (242, y)], cool, 3.5, scale)
                line(draw, [(270, y), (301 - layer_index * 8, y)], cool, 3.5, scale)
            else:
                y = 359 - layer_index * 11
                line(draw, [(189, y), (213 + layer_index * 7, y - 9)], amber, 3.5, scale)
                line(draw, [(323 - layer_index * 7, y - 9), (347, y)], amber, 3.5, scale)
        elif f in {"reject", "ui_reject", "path_reject"}:
            y = 222 + layer_index * 24
            line(draw, [(108 + layer_index * 22, y), (202, y)], red, 9 - layer_index * 2, scale)
            line(draw, [(310, y), (404 - layer_index * 22, y)], red, 9 - layer_index * 2, scale)
            line(draw, [(224, 190 + layer_index * 18), (224, 242 + layer_index * 18)], dim, 4, scale)
            if f == "path_reject":
                line(draw, [(175, 342), (229, 316), (283, 342), (337, 316)], red, 5, scale)
            elif f == "ui_reject":
                # Reticle interruption is compact; it never becomes a world X.
                line(draw, [(238, 326 - layer_index * 9), (250, 313 - layer_index * 9)], cool, 3, scale)
                line(draw, [(262, 313 - layer_index * 9), (274, 326 - layer_index * 9)], cool, 3, scale)
            else:
                line(draw, [(184 + layer_index * 8, 340), (214, 350)], dim, 3, scale)
        elif f == "ready":
            line(draw, [(160, 292), (211, 245), (255, 263)], amber, 7, scale)
            line(draw, [(352, 292), (301, 245), (257, 263)], amber, 7, scale)
            draw.arc(sbox((178, 158, 334, 346), scale), 205, 335, fill=cool, width=(7 - layer_index) * scale)
        elif f == "pass":
            line(draw, [(151, 300), (236, 220), (275, 251)], green, 9 - layer_index, scale)
            line(draw, [(220, 300), (305, 220), (344, 251)], green, 9 - layer_index, scale)
            draw.arc(sbox((125, 120, 387, 388), scale), 205, 335, fill=cool, width=4 * scale)
        elif f == "brace":
            xoff = 76 - layer_index * 12
            line(draw, [(256 - xoff, 146), (256 - xoff, 352)], amber, 12 - layer_index * 2, scale)
            line(draw, [(256 + xoff, 146), (256 + xoff, 352)], amber, 12 - layer_index * 2, scale)
            line(draw, [(180, 332), (332, 332)], cool, 6, scale)
        elif f == "break_badge":
            spread = layer_index * 7
            line(draw, [(141 + spread, 174 + spread), (225, 174 + spread), (241, 221)], white, 8 - layer_index, scale)
            line(draw, [(371 - spread, 174 + spread), (287, 174 + spread), (271, 221)], white, 8 - layer_index, scale)
            line(draw, [(141 + spread, 338 - spread), (225, 338 - spread), (241, 291)], amber, 8 - layer_index, scale)
            line(draw, [(371 - spread, 338 - spread), (287, 338 - spread), (271, 291)], amber, 8 - layer_index, scale)
        elif f in {"team", "commit"}:
            spread = 104 - layer_index * 16
            line(draw, [(256 - spread, 178), (196, 256), (256 - spread, 334)], green, 8, scale)
            line(draw, [(256 + spread, 178), (316, 256), (256 + spread, 334)], green, 8, scale)
            line(draw, [(225, 256), (287, 256)], white, 5, scale)
            if f == "commit":
                line(draw, [(234, 208 + layer_index * 10), (256, 190 + layer_index * 10),
                            (278, 208 + layer_index * 10)], amber, 4, scale)
            else:
                line(draw, [(204 + layer_index * 7, 365),
                            (308 - layer_index * 7, 365)], cool, 4, scale)
        elif f in {"route", "exit"}:
            inset = 151 + layer_index * 13
            line(draw, [(inset, 358), (inset, 168), (512 - inset, 168), (512 - inset, 358)], green, 8 - layer_index, scale)
            line(draw, [(200, 389 - layer_index * 12), (256, 359 - layer_index * 8), (312, 389 - layer_index * 12)], amber, 5, scale)
            if f == "exit":
                draw.rounded_rectangle(sbox((194, 108, 318, 142), scale), radius=6 * scale,
                                       outline=white, width=5 * scale)
        elif f == "telegraph":
            line(draw, [(136, 326), (256, 160), (376, 326)], amber, 8 - layer_index, scale)
            line(draw, [(174, 350), (338, 350)], cool, 5, scale)
            line(draw, [(207, 378), (249, 378)], amber, 4, scale)
            line(draw, [(263, 378), (305, 378)], amber, 4, scale)
        elif f == "opportunity":
            draw.arc(sbox((140, 132, 372, 372), scale), 198, 342, fill=amber, width=(8 - layer_index) * scale)
            line(draw, [(182, 322), (219, 292)], cool, 5, scale)
            line(draw, [(330, 322), (293, 292)], cool, 5, scale)
        elif f == "grab":
            gap = 72 - layer_index * 10
            line(draw, [(130, 186), (256 - gap, 256), (130, 326)], red, 7, scale)
            line(draw, [(382, 186), (256 + gap, 256), (382, 326)], red, 7, scale)
            line(draw, [(229, 146), (283, 146)], amber, 4, scale)
        elif f in {"reset_start", "reset_complete"}:
            gap = (92 - layer_index * 16) if f == "reset_start" else (64 + layer_index * 8)
            for sx, sy in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
                x = 256 + sx * gap
                y = 256 + sy * gap
                line(draw, [(x - sx * 35, y), (x, y), (x, y - sy * 35)], cool if f == "reset_start" else green, 7, scale)
            line(draw, [(174, 382), (338, 382)], amber if f == "reset_start" else white, 4, scale)
        else:
            raise KeyError((spec.slug, f))

    image = sanitize_runtime_alpha(ss_canvas(draw_layer))
    if np.count_nonzero(np.asarray(image.getchannel("A"))) < 64:
        raise RuntimeError(f"Static layer too sparse: {spec.slug}/{layer_index}")
    return image


def physical_frame(spec: EffectSpec, layer_index: int, index: int,
                   banks: dict[str, list[Image.Image]]) -> Image.Image:
    t = index / 15.0
    seed = stable_seed(spec.slug)
    angle = math.radians(spec.angle)
    direction = np.array([math.cos(angle), -math.sin(angle)], dtype=np.float32)
    perpendicular = np.array([-direction[1], direction[0]], dtype=np.float32)
    origin = np.array([184.0, 302.0], dtype=np.float32)
    canvas = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))

    if layer_index == 0:
        def draw_transient(draw: ImageDraw.ImageDraw, scale: int) -> None:
            sharp = max(0.08, 1.0 - t * 0.94)
            reach = (55 + 116 * min(1.0, t * 3.2)) * spec.strength
            white = (232, 231, 217, int(255 * sharp))
            cool = (161, 176, 177, int(220 * sharp))
            amber = (187, 135, 51, int(170 * sharp))
            if spec.material == "cloth":
                # Cloth/body contact compresses at the hit point first, then
                # releases into an asymmetric directional fold fan.
                radius = (28 + 48 * min(1.0, t * 2.7)) * spec.strength
                draw.arc(sbox((origin[0] - radius, origin[1] - radius,
                               origin[0] + radius, origin[1] + radius), scale),
                         int(165 - spec.angle), int(342 - spec.angle), fill=cool,
                         width=max(2, int((7.0 - t * 3.1) * scale)))
                offsets = (-18.0, -4.0, 13.0, 25.0) if spec.strength >= 1.1 else (-15.0, 0.0, 18.0)
                for ray, offset in enumerate(offsets):
                    start = origin + perpendicular * offset * 0.18
                    end = origin + direction * reach * (0.66 + ray * 0.10) + perpendicular * offset
                    color = white if ray == 1 else (cool if ray % 2 == 0 else amber)
                    line(draw, [tuple(start), tuple(end)], color,
                         max(1.3, 6.4 - ray * 0.8 - t * 3.0), scale)
                if "Brace" in spec.slug:
                    opposing = origin - direction * reach * 0.36
                    line(draw, [tuple(origin + perpendicular * 35), tuple(opposing),
                                tuple(origin - perpendicular * 35)], amber, 4.2, scale)
            elif spec.material == "paper":
                # Paper contact reads as several flat, separated page edges.
                for ray, offset in enumerate((-25.0, -8.0, 10.0, 28.0)):
                    start = origin + perpendicular * offset * 0.28
                    end = origin + direction * reach * (0.68 + ray * 0.09) + perpendicular * offset
                    line(draw, [tuple(start), tuple(end)], white if ray < 2 else cool,
                         max(1.2, 5.2 - ray * 0.65 - t * 2.2), scale)
                fold = origin + direction * reach * 0.58
                line(draw, [tuple(fold - perpendicular * 31), tuple(fold),
                            tuple(fold + direction * 24 + perpendicular * 20)], amber, 3.1, scale)
            elif spec.material == "rubber":
                # Caster/rubber response remains low, paired and grounded.
                base = origin + np.array([0.0, 28.0], dtype=np.float32)
                for offset in (-20.0, 20.0):
                    start = base + perpendicular * offset
                    end = start + direction * reach * 0.88
                    line(draw, [tuple(start), tuple(end)], cool, max(2.0, 7.0 - t * 3.0), scale)
                bite = 30 + 34 * min(1.0, t * 2.5)
                for offset in (-27.0, 27.0):
                    c = base + perpendicular * offset
                    draw.arc(sbox((c[0] - bite, c[1] - bite * 0.42,
                                   c[0] + bite, c[1] + bite * 0.42), scale),
                             185, 352, fill=amber, width=max(2, int((5.0 - t * 2.0) * scale)))
            else:
                # Painted metal, laminate and floor contacts use a narrow hard
                # fan plus one restrained cold glint, never a spark shower.
                for ray, offset in enumerate((-14.0, 0.0, 16.0)):
                    start = origin + perpendicular * offset * 0.20
                    end = origin + direction * reach * (0.78 + ray * 0.11) + perpendicular * offset
                    line(draw, [tuple(start), tuple(end)], (cool, white, amber)[ray],
                         max(1.2, 5.8 - ray - t * 2.8), scale)
                glint = origin + direction * reach * 0.48
                line(draw, [tuple(glint - perpendicular * 22), tuple(glint + perpendicular * 22)],
                     (218, 222, 214, int(150 * sharp)), 2.2, scale)
                base_end = origin + direction * reach * 0.65
                line(draw, [tuple(origin - perpendicular * 30), tuple(base_end),
                            tuple(origin + perpendicular * 25)], cool, 3.4, scale)
        canvas = ss_canvas(draw_transient)
    else:
        source = banks[spec.material]
        tint = {
            "cloth": (208, 213, 207),
            "office": (190, 181, 157),
            "paper": (237, 232, 214),
            "rubber": (112, 105, 91),
        }[spec.material]
        piece_count = 3 if layer_index == 1 else 4
        for item in range(piece_count):
            piece_index = (seed + item * 5 + layer_index * 3) % len(source)
            piece = neutralize_piece(source[piece_index], tint, 1.03 if layer_index == 1 else 0.92)
            delay = item * (0.055 if layer_index == 1 else 0.09)
            q = max(0.0, min(1.0, (t - delay) / max(0.01, 1.0 - delay)))
            if layer_index == 1:
                envelope = max(0.07, math.sin(math.pi * min(1.0, q * 1.03)))
                travel = direction * (42 + (74 + item * 17) * q) * spec.strength
                side = perpendicular * ((item - 1) * 23 + math.sin(q * math.pi) * 8)
                center = origin + travel + side
                width = (82 + item * 14) * spec.strength * (0.82 + q * 0.20)
                rotation = -spec.angle + (item - 1) * 13 + q * (8 if item % 2 else -9)
                opacity = envelope * (0.84 - item * 0.11)
            else:
                travel = direction * (32 + (34 + item * 10) * q) * spec.strength
                fall = np.array([0.0, 28 + 82 * q], dtype=np.float32)
                side = perpendicular * ((item - 1.5) * 22)
                center = origin + travel + fall + side
                width = (45 + item * 9) * min(1.1, spec.strength) * (1.0 - q * 0.18)
                rotation = -spec.angle + item * 19 + q * (54 if item % 2 else -46)
                opacity = max(0.06, (0.60 - item * 0.07) * (1.0 - q * 0.72))
            paste_piece(canvas, piece, tuple(center), width, rotation, opacity, mirror=item % 2 == 1)

    canvas = enforce_frame_margin(sanitize_runtime_alpha(canvas))
    if np.count_nonzero(np.asarray(canvas.getchannel("A"))) < 48:
        raise RuntimeError(f"Physical frame too sparse: {spec.slug}/{layer_index}/{index}")
    return canvas


def assemble_flipbook(frames: list[Image.Image]) -> Image.Image:
    if len(frames) != 16:
        raise ValueError("Complete pack flipbooks require 16 frames")
    atlas = Image.new("RGBA", (ATLAS_SIZE, ATLAS_SIZE), (0, 0, 0, 0))
    for index, frame in enumerate(frames):
        column = index % GRID
        chronological_row = index // GRID
        image_row = GRID - 1 - chronological_row
        atlas.alpha_composite(frame, (column * SIZE, image_row * SIZE))
    return sanitize_runtime_alpha(atlas)


def extract_unity_frame(atlas: Image.Image, index: int) -> Image.Image:
    column = index % GRID
    image_row = GRID - 1 - index // GRID
    return atlas.crop((column * SIZE, image_row * SIZE,
                       (column + 1) * SIZE, (image_row + 1) * SIZE))


def transparent_margin(image: Image.Image) -> int:
    alpha = np.asarray(image.convert("RGBA").getchannel("A"))
    ys, xs = np.where(alpha > 0)
    if len(xs) == 0:
        return min(image.size)
    return int(min(xs.min(), image.width - 1 - xs.max(), ys.min(), image.height - 1 - ys.max()))


def validate_candidate(path: Path, flipbook: bool) -> dict[str, object]:
    if path.read_bytes()[:8] != b"\x89PNG\r\n\x1a\n":
        raise RuntimeError(f"PNG signature failure: {relative(path)}")
    image = Image.open(path).convert("RGBA")
    expected = (ATLAS_SIZE, ATLAS_SIZE) if flipbook else (SIZE, SIZE)
    if image.size != expected:
        raise RuntimeError(f"Dimension failure: {relative(path)} {image.size}")
    rgba = np.asarray(image, dtype=np.int16)
    alpha = rgba[:, :, 3]
    active = alpha > 8
    risks = {
        "magenta_pixels": int(np.count_nonzero(active & (rgba[:, :, 0] > 210) & (rgba[:, :, 2] > 190) & (rgba[:, :, 1] < 90))),
        "green_spill_pixels": int(np.count_nonzero(active & (rgba[:, :, 1] > rgba[:, :, 0] * 1.45) & (rgba[:, :, 1] > rgba[:, :, 2] * 1.45))),
        "neon_cyan_pixels": int(np.count_nonzero(active & (rgba[:, :, 1] > 205) & (rgba[:, :, 2] > 205) & (rgba[:, :, 0] < 90))),
    }
    if any(risks.values()):
        raise RuntimeError(f"Palette risk: {relative(path)} {risks}")
    if np.count_nonzero(alpha) == 0 or np.count_nonzero((alpha > 0) & (alpha < 255)) == 0:
        raise RuntimeError(f"Alpha coverage failure: {relative(path)}")
    frame_hashes = []
    minimum_margin = transparent_margin(image)
    if flipbook:
        margins = []
        for index in range(16):
            frame = extract_unity_frame(image, index)
            margins.append(transparent_margin(frame))
            if np.count_nonzero(np.asarray(frame.getchannel("A"))) < 48:
                raise RuntimeError(f"Sparse frame: {relative(path)} frame={index}")
            frame_hashes.append(hashlib.sha256(frame.tobytes()).hexdigest())
        minimum_margin = min(margins)
        if len(set(frame_hashes)) != 16:
            raise RuntimeError(f"Duplicate flipbook frames: {relative(path)}")
    if minimum_margin < MIN_MARGIN:
        raise RuntimeError(f"Transparent margin failure: {relative(path)} margin={minimum_margin}")
    return {
        "asset_path": relative(path),
        "dimensions": f"{image.width}x{image.height}",
        "alpha_nonzero_pixels": int(np.count_nonzero(alpha)),
        "alpha_partial_pixels": int(np.count_nonzero((alpha > 0) & (alpha < 255))),
        "transparent_margin_px": minimum_margin,
        "frame_order": "UNITY_WHOLE_SHEET_LOWER_LEFT_ROW_MAJOR" if flipbook else "N/A",
        "frame_hash_unique": len(set(frame_hashes)) if flipbook else "N/A",
        **risks,
        "sha256": sha256(path),
        "bytes": path.stat().st_size,
    }


def font(size: int):
    for candidate in ("C:/Windows/Fonts/segoeui.ttf", "DejaVuSans.ttf"):
        try:
            return ImageFont.truetype(candidate, size)
        except OSError:
            pass
    return ImageFont.load_default()


def composite(effect: EffectSpec, outputs: dict[str, list[Path]], frame_index: int = 5) -> Image.Image:
    result = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
    for path in outputs[effect.slug]:
        image = Image.open(path).convert("RGBA")
        if effect.physical:
            image = extract_unity_frame(image, frame_index)
        result.alpha_composite(image)
    return result


def checkerboard(size: tuple[int, int], square: int = 20) -> Image.Image:
    image = Image.new("RGBA", size, (38, 41, 42, 255))
    draw = ImageDraw.Draw(image)
    for y in range(0, size[1], square):
        for x in range(0, size[0], square):
            value = 48 if ((x // square) + (y // square)) % 2 == 0 else 65
            draw.rectangle((x, y, x + square, y + square), fill=(value, value + 2, value + 1, 255))
    return image


def build_contact_sheet(outputs: dict[str, list[Path]], grayscale_alpha: bool = False) -> Image.Image:
    sheet = Image.new("RGBA", (3840, 2160), (25, 29, 30, 255))
    draw = ImageDraw.Draw(sheet)
    label = font(21)
    tiny = font(15)
    panel_w, panel_h = 480, 360
    for index, effect in enumerate(EFFECTS):
        x0 = (index % 8) * panel_w
        y0 = (index // 8) * panel_h
        panel = checkerboard((panel_w - 18, panel_h - 58), 18)
        image = composite(effect, outputs)
        if grayscale_alpha:
            gray = ImageOps.grayscale(image).convert("RGBA")
            gray.putalpha(image.getchannel("A"))
            alpha = Image.merge("RGBA", (image.getchannel("A"),) * 4)
            gray.thumbnail((210, 265), Image.Resampling.LANCZOS)
            alpha.thumbnail((210, 265), Image.Resampling.LANCZOS)
            panel.alpha_composite(gray, (12, 18))
            panel.alpha_composite(alpha, (235, 18))
        else:
            image.thumbnail((292, 292), Image.Resampling.LANCZOS)
            panel.alpha_composite(image, ((panel.width - image.width) // 2, (panel.height - image.height) // 2))
        sheet.alpha_composite(panel, (x0 + 9, y0 + 45))
        draw.text((x0 + 15, y0 + 8), effect.cue_id, fill=(190, 199, 197, 255), font=label)
        draw.text((x0 + 15, y0 + 332), effect.display_name[:42], fill=(137, 148, 146, 255), font=tiny)
    return sheet


def build_impact_sheet(outputs: dict[str, list[Path]]) -> Image.Image:
    impact = [effect for effect in EFFECTS if effect.cue_id.startswith("VFX-LAB-")]
    sheet = Image.new("RGBA", (2560, 1440), (24, 28, 29, 255))
    draw = ImageDraw.Draw(sheet)
    title = font(28)
    label = font(20)
    for index, effect in enumerate(impact):
        x0 = (index % 4) * 640
        y0 = (index // 4) * 480
        panel = checkerboard((610, 400), 22)
        image = composite(effect, outputs, 4)
        image.thumbnail((380, 380), Image.Resampling.LANCZOS)
        panel.alpha_composite(image, ((panel.width - image.width) // 2, (panel.height - image.height) // 2))
        sheet.alpha_composite(panel, (x0 + 15, y0 + 55))
        draw.text((x0 + 18, y0 + 13), effect.cue_id, fill=(195, 203, 200, 255), font=title)
        draw.text((x0 + 18, y0 + 452), effect.display_name, fill=(142, 152, 149, 255), font=label)
    return sheet


def write_ledger(paths: list[Path], metrics: dict[str, dict[str, object]]) -> Path:
    ledger = LICENSE_DIR / "VFX_Showcase_Complete_Provenance_Ledger_v003.csv"
    fields = [
        "asset_path", "asset_role", "provenance_classification", "creator_or_generator",
        "source_prompt_id", "source_inputs", "third_party_source",
        "exact_license_or_rights_basis", "commercial_use", "repository_redistribution",
        "dimensions", "alpha_nonzero_pixels", "alpha_partial_pixels", "transparent_margin_px",
        "frame_order", "frame_hash_unique", "magenta_pixels", "green_spill_pixels",
        "neon_cyan_pixels", "sha256", "bytes", "review_status",
    ]
    source_inputs = ";".join(relative(path) for path in
                             (CLOTH_SOURCE, OFFICE_SOURCE, DUST_SOURCE, RUBBER_SOURCE, PAPER_SOURCE))
    rows = []
    for path in paths:
        item_metrics = metrics.get(relative(path), {})
        if path.name in PROMPT_IDS:
            classification = "ORIGINAL_AI_GENERATED_SOURCE"
            creator = "OpenAI built-in image generation; no reference image"
            prompt_id = PROMPT_IDS[path.name]
            inputs = "NONE"
            role = "PRESERVED_IMAGE_GENERATION_SOURCE"
        elif "AlphaExtract" in path.name:
            classification = "ORIGINAL_AI_GENERATED_DERIVATIVE_SOURCE"
            creator = "ImageGen source plus imagegen Skill chroma-key removal"
            original_name = path.name.replace("_AlphaExtract_", "_ImageGen_")
            prompt_id = PROMPT_IDS[original_name]
            inputs = relative(SOURCE_DIR / original_name)
            role = "STRAIGHT_ALPHA_ELEMENT_BANK_SOURCE"
        elif path.parent == TEXTURE_DIR:
            classification = "ORIGINAL_DETERMINISTIC_VECTOR_RASTER"
            creator = "Codex-authored deterministic supersampled mask"
            prompt_id = "NONE"
            inputs = relative(SCRIPT_PATH)
            role = "UNITY_REVIEW_TEXTURE_LAYER_CANDIDATE"
        elif path.parent == FLIPBOOK_DIR:
            classification = "ORIGINAL_AI_ASSISTED_DETERMINISTIC_DERIVATIVE"
            creator = "Codex-authored deterministic animation using project-original element banks"
            prompt_id = "IG-VFX-SHW-COMPLETE-003-A/B + IG-VFX-SHW-PREMIUM-002-A/B/C"
            inputs = source_inputs + ";" + relative(SCRIPT_PATH)
            role = "UNITY_REVIEW_4X4_FLIPBOOK_LAYER_CANDIDATE"
        elif path.parent == PREVIEW_DIR:
            classification = "ORIGINAL_DETERMINISTIC_REVIEW_COMPOSITE"
            creator = "Codex-authored review preview"
            prompt_id = "NONE"
            inputs = "ALL COMPLETE v003 CANDIDATES"
            role = "HUMAN_OWNER_REVIEW_PREVIEW"
        else:
            classification = "ORIGINAL_REPRODUCIBLE_SOURCE"
            creator = "Codex-authored deterministic build source"
            prompt_id = "NONE"
            inputs = "NONE"
            role = "REPRODUCIBLE_BUILD_SOURCE"
        dimensions = "N/A"
        if path.suffix.lower() == ".png":
            with Image.open(path) as image:
                dimensions = f"{image.width}x{image.height}"
        rows.append({
            "asset_path": relative(path), "asset_role": role,
            "provenance_classification": classification, "creator_or_generator": creator,
            "source_prompt_id": prompt_id, "source_inputs": inputs, "third_party_source": "NONE",
            "exact_license_or_rights_basis": "PROJECT ORIGINAL; NO THIRD-PARTY SOURCE; HUMAN OWNER AUTHORIZED CREATION",
            "commercial_use": "AUTHORIZED FOR BREAK NOTICE PROJECT AND CONTEST MATERIALS BY HUMAN OWNER",
            "repository_redistribution": "AUTHORIZED WITH PROJECT DELIVERY",
            "dimensions": item_metrics.get("dimensions", dimensions),
            "alpha_nonzero_pixels": item_metrics.get("alpha_nonzero_pixels", "SOURCE / N/A"),
            "alpha_partial_pixels": item_metrics.get("alpha_partial_pixels", "SOURCE / N/A"),
            "transparent_margin_px": item_metrics.get("transparent_margin_px", "SOURCE / N/A"),
            "frame_order": item_metrics.get("frame_order", "N/A"),
            "frame_hash_unique": item_metrics.get("frame_hash_unique", "N/A"),
            "magenta_pixels": item_metrics.get("magenta_pixels", "N/A"),
            "green_spill_pixels": item_metrics.get("green_spill_pixels", "N/A"),
            "neon_cyan_pixels": item_metrics.get("neon_cyan_pixels", "N/A"),
            "sha256": sha256(path), "bytes": path.stat().st_size,
            "review_status": "REVIEW / OWNER APPROVAL PENDING / GAMEPLAY INTEGRATION NONE",
        })
    ledger.parent.mkdir(parents=True, exist_ok=True)
    with ledger.open("w", encoding="utf-8", newline="") as stream:
        writer = csv.DictWriter(stream, fields, quoting=csv.QUOTE_ALL, lineterminator="\n")
        writer.writeheader()
        writer.writerows(rows)
    return ledger


def main() -> None:
    required = [CLOTH_ORIGINAL, CLOTH_SOURCE, OFFICE_ORIGINAL, OFFICE_SOURCE,
                DUST_ORIGINAL, DUST_SOURCE, RUBBER_ORIGINAL, RUBBER_SOURCE,
                PAPER_ORIGINAL, PAPER_SOURCE]
    for path in required:
        if not path.is_file():
            raise FileNotFoundError(path)
    for directory in (TEXTURE_DIR, FLIPBOOK_DIR, PREVIEW_DIR, LICENSE_DIR):
        directory.mkdir(parents=True, exist_ok=True)

    banks = {
        "cloth": split_bank(CLOTH_SOURCE, 4, 4),
        "office": split_bank(OFFICE_SOURCE, 4, 4),
        "paper": split_bank(PAPER_SOURCE, 5, 4),
        "rubber": split_bank(RUBBER_SOURCE, 4, 4),
    }
    if tuple(len(banks[key]) for key in ("cloth", "office", "paper", "rubber")) != (16, 16, 20, 16):
        raise RuntimeError("Source bank grid contract failed")

    outputs: dict[str, list[Path]] = {}
    candidates: list[Path] = []
    for effect in EFFECTS:
        outputs[effect.slug] = []
        if effect.physical:
            layer_names = ("PrimaryTransient", "DirectionalBody", "Settle")
            for layer_index, layer_name in enumerate(layer_names):
                path = FLIPBOOK_DIR / f"T_VFX_Showcase_{effect.slug}_{layer_name}_4x4_v003.png"
                frames = [physical_frame(effect, layer_index, frame, banks) for frame in range(16)]
                save_png(assemble_flipbook(frames), path)
                outputs[effect.slug].append(path)
                candidates.append(path)
        else:
            for layer_index, layer_name in enumerate(("Primary", "Support", "Settle")):
                path = TEXTURE_DIR / f"T_VFX_Showcase_{effect.slug}_{layer_name}_v003.png"
                save_png(static_layer(effect, layer_index), path)
                outputs[effect.slug].append(path)
                candidates.append(path)

    authored_preview_paths = [
        PREVIEW_DIR / "VFX_Showcase_Complete_AllCueContactSheet_v003.png",
        PREVIEW_DIR / "VFX_Showcase_Complete_GrayscaleAlphaAudit_v003.png",
        PREVIEW_DIR / "VFX_Showcase_Complete_ImpactLabContactSheet_v003.png",
    ]
    save_png(build_contact_sheet(outputs), authored_preview_paths[0])
    save_png(build_contact_sheet(outputs, grayscale_alpha=True), authored_preview_paths[1])
    save_png(build_impact_sheet(outputs), authored_preview_paths[2])
    preview_paths = authored_preview_paths + sorted(
        path for path in PREVIEW_DIR.glob("*.png")
        if path not in authored_preview_paths
    )

    validation = {}
    for path in candidates:
        row = validate_candidate(path, path.parent == FLIPBOOK_DIR)
        validation[relative(path)] = row

    provenance_paths = [CLOTH_ORIGINAL, CLOTH_SOURCE, OFFICE_ORIGINAL, OFFICE_SOURCE,
                        DUST_ORIGINAL, DUST_SOURCE, RUBBER_ORIGINAL, RUBBER_SOURCE,
                        PAPER_ORIGINAL, PAPER_SOURCE, SCRIPT_PATH] + candidates + preview_paths
    ledger = write_ledger(provenance_paths, validation)
    result = {
        "status": "PASS",
        "review_status": "REVIEW / OWNER APPROVAL PENDING",
        "locked_gameplay_cues": 36,
        "impact_material_auditions": 12,
        "effect_candidates": len(EFFECTS),
        "layer_candidates": len(candidates),
        "static_512_layers": sum(3 for effect in EFFECTS if not effect.physical),
        "flipbook_2048_layers": sum(3 for effect in EFFECTS if effect.physical),
        "flipbook_frames": sum(48 for effect in EFFECTS if effect.physical),
        "candidate_bytes": sum(path.stat().st_size for path in candidates),
        "provenance_rows": len(provenance_paths),
        "ledger": relative(ledger),
        "previews": [relative(path) for path in preview_paths],
        "candidates": list(validation.values()),
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
