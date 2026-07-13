# -*- coding: utf-8 -*-
"""生成沙虫挑战合约使用的统一矢量风 PNG 素材。"""

from __future__ import annotations

import math
import random
from pathlib import Path
from typing import Callable, Iterable

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "1.6" / "Textures" / "UI" / "SandWorm" / "Contract"
RAW_RISKS = ROOT / "Tools" / "GeneratedRawRisks"
SCALE = 3

GOLD = (214, 156, 64, 255)
AMBER = (245, 188, 82, 255)
BRONZE = (134, 94, 50, 255)
CYAN = (65, 232, 222, 255)
TEAL = (54, 164, 152, 255)
RED = (238, 79, 38, 255)
DARK = (11, 13, 18, 230)
PANEL = (20, 19, 27, 205)


RISKS = [
    ("SandWorm_Risk_SmallWorm1", "worm", 1),
    ("SandWorm_Risk_SmallWorm2", "worm", 2),
    ("SandWorm_Risk_SmallWorm3", "worm", 3),
    ("SandWorm_Risk_ChargeFrenzy1", "charge", 1),
    ("SandWorm_Risk_ChargeFrenzy2", "charge", 2),
    ("SandWorm_Risk_ChargeFrenzy3", "charge", 3),
    ("SandWorm_Risk_DamageCap1", "armor", 1),
    ("SandWorm_Risk_DamageCap2", "armor", 2),
    ("SandWorm_Risk_DamageCap3", "armor", 3),
    ("SandWorm_Risk_PawnRange1", "range", 1),
    ("SandWorm_Risk_PawnRange2", "range", 2),
    ("SandWorm_Risk_PawnRange3", "range", 3),
    ("SandWorm_Risk_PawnMove1", "move", 1),
    ("SandWorm_Risk_PawnMove2", "move", 2),
    ("SandWorm_Risk_PawnMove3", "move", 3),
    ("SandWorm_Risk_ShockwaveAttack1", "shock", 1),
    ("SandWorm_Risk_ShockwaveAttack2", "shock", 2),
    ("SandWorm_Risk_ShockwaveAttack3", "shock", 3),
    ("SandWorm_Risk_SandBlind1", "blind", 1),
    ("SandWorm_Risk_SandBlind2", "blind", 2),
    ("SandWorm_Risk_ResonancePressure1", "timer", 1),
    ("SandWorm_Risk_ResonancePressure2", "timer", 2),
]


def c(color: tuple[int, int, int, int], alpha: float = 1.0) -> tuple[int, int, int, int]:
    """按透明度返回颜色，便于统一控制素材层级。"""
    return (color[0], color[1], color[2], max(0, min(255, int(color[3] * alpha))))


def sp(points: Iterable[tuple[float, float]], scale: int) -> list[tuple[int, int]]:
    """把逻辑坐标转换为高分辨率绘制坐标。"""
    return [(round(x * scale), round(y * scale)) for x, y in points]


def line(draw: ImageDraw.ImageDraw, points: Iterable[tuple[float, float]], fill, width: float, scale: int) -> None:
    """绘制抗锯齿折线，高分辨率缩放后再回采样。"""
    draw.line(sp(points, scale), fill=fill, width=max(1, round(width * scale)), joint="curve")


def hex_points(size: float, radius: float | None = None) -> list[tuple[float, float]]:
    """返回统一的尖顶六边形顶点坐标。"""
    cx = cy = size * 0.5
    r = radius or size * 0.455
    return [(cx + math.cos(math.radians(-90 + 60 * i)) * r, cy + math.sin(math.radians(-90 + 60 * i)) * r) for i in range(6)]


def cut_rect(w: float, h: float, cut: float) -> list[tuple[float, float]]:
    """返回切角矩形顶点坐标，用于按钮和面板边框。"""
    return [(cut, 0), (w - cut, 0), (w, cut), (w, h - cut), (w - cut, h), (cut, h), (0, h - cut), (0, cut)]


def inset_cut_rect(w: float, h: float, inset: float, cut: float) -> list[tuple[float, float]]:
    """返回同心内缩的切角矩形顶点坐标，避免钝角外框内套出错位锐角线。"""
    inner_w = max(1, w - inset * 2)
    inner_h = max(1, h - inset * 2)
    inner_cut = min(max(1, cut), inner_w * 0.5, inner_h * 0.5)
    return [(x + inset, y + inset) for x, y in cut_rect(inner_w, inner_h, inner_cut)]


def save(img: Image.Image, rel: str) -> None:
    """保存 PNG 到 RimWorld 1.6 贴图目录。"""
    path = OUT / rel
    path.parent.mkdir(parents=True, exist_ok=True)
    img.save(path)
    print(path)


def render(size: tuple[int, int], painter: Callable[[Image.Image, ImageDraw.ImageDraw, int], None]) -> Image.Image:
    """用高分辨率画布绘制后回采样，保持矢量线条边缘清晰。"""
    w, h = size
    img = Image.new("RGBA", (w * SCALE, h * SCALE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, "RGBA")
    painter(img, draw, SCALE)
    return img.resize(size, Image.Resampling.LANCZOS)


def draw_hex_outline(draw: ImageDraw.ImageDraw, pts, color, width: float, scale: int) -> None:
    """绘制闭合六边形描边。"""
    line(draw, pts + [pts[0]], color, width, scale)


def draw_hex_base(draw: ImageDraw.ImageDraw, size: int, scale: int, fill_alpha: float = 1.0) -> None:
    """绘制所有风险图标共用的六边形底座。"""
    pts = hex_points(size)
    inner = hex_points(size, size * 0.385)
    draw.polygon(sp(pts, scale), fill=c(DARK, 0.86 * fill_alpha))
    draw.polygon(sp(inner, scale), fill=(21, 24, 31, round(182 * fill_alpha)))
    draw_hex_outline(draw, pts, c(BRONZE, 0.95), 14, scale)
    draw_hex_outline(draw, hex_points(size, size * 0.425), c(GOLD, 0.92), 5, scale)
    draw_hex_outline(draw, inner, c(CYAN, 0.36), 3, scale)
    for i in range(6):
        a = math.radians(-90 + i * 60)
        b = math.radians(-90 + (i + 1) * 60)
        p1 = (size * 0.5 + math.cos(a) * size * 0.31, size * 0.5 + math.sin(a) * size * 0.31)
        p2 = (size * 0.5 + math.cos(b) * size * 0.31, size * 0.5 + math.sin(b) * size * 0.31)
        line(draw, [p1, p2], c(TEAL, 0.16), 1.5, scale)


def draw_level_ticks(draw: ImageDraw.ImageDraw, level: int, size: int, scale: int) -> None:
    """用短线段表示等级，避免在图标上绘制文字。"""
    total = level * 22 + (level - 1) * 10
    x = size * 0.5 - total * 0.5
    y = size * 0.79
    for i in range(level):
        line(draw, [(x + i * 32, y), (x + i * 32 + 22, y)], c(AMBER, 0.95), 6, scale)


def draw_symbol(draw: ImageDraw.ImageDraw, family: str, level: int, scale: int) -> None:
    """根据词条类型绘制中心线稿符号。"""
    center = (256, 244)
    if family == "worm":
        pts = [(162, 292), (196, 250), (234, 220), (276, 210), (318, 228), (350, 264)]
        line(draw, pts, c(GOLD), 16, scale)
        for i, p in enumerate(pts[1:-1]):
            r = 22 + i * 2
            draw.ellipse([round((p[0]-r)*scale), round((p[1]-r)*scale), round((p[0]+r)*scale), round((p[1]+r)*scale)], outline=c(CYAN, 0.55), width=round(4 * scale))
        line(draw, [(290, 198), (318, 148), (332, 206)], c(AMBER), 10, scale)
    elif family == "charge":
        for i in range(3):
            y = 190 + i * 42
            line(draw, [(150, y), (305, y), (270, y - 28)], c(CYAN if i < level else GOLD, 0.95), 12 - i, scale)
        line(draw, [(202, 332), (350, 188)], c(AMBER), 8, scale)
    elif family == "armor":
        shield = [(256, 130), (344, 170), (328, 296), (256, 360), (184, 296), (168, 170)]
        draw.polygon(sp(shield, scale), outline=c(GOLD), width=round(10 * scale))
        for i in range(level + 1):
            y = 194 + i * 34
            line(draw, [(198, y), (314, y - 10)], c(CYAN, 0.34 + i * 0.12), 5, scale)
    elif family == "range":
        for r in (48, 82, 116):
            draw.ellipse([round((center[0]-r)*scale), round((center[1]-r)*scale), round((center[0]+r)*scale), round((center[1]+r)*scale)], outline=c(CYAN, 0.28 + r / 450), width=round(4 * scale))
        line(draw, [(256, 128), (256, 360)], c(GOLD), 7, scale)
        line(draw, [(140, 244), (372, 244)], c(GOLD), 7, scale)
        line(draw, [(170, 330), (342, 158)], c(AMBER), 7 + level, scale)
    elif family == "move":
        foot = [(210, 160), (282, 184), (300, 250), (270, 322), (196, 302), (178, 230)]
        draw.polygon(sp(foot, scale), outline=c(GOLD), width=round(9 * scale))
        for i in range(4):
            line(draw, [(156, 318 + i * 18), (344 - i * 22, 304 + i * 14)], c(CYAN, 0.25 + i * 0.09), 5, scale)
        line(draw, [(310, 170), (350, 132)], c(AMBER), 7, scale)
    elif family == "shock":
        for r in (42, 78, 116):
            draw.arc([round((256-r)*scale), round((244-r)*scale), round((256+r)*scale), round((244+r)*scale)], 18, 342, fill=c(CYAN, 0.35 + r / 320), width=round((4 + level) * scale))
        crack = [(256, 126), (238, 198), (270, 234), (232, 300), (260, 368)]
        line(draw, crack, c(AMBER), 10, scale)
        line(draw, [(158, 282), (216, 262), (252, 286), (324, 260), (368, 286)], c(GOLD, 0.8), 5, scale)
    elif family == "blind":
        eye = [(148, 246), (202, 186), (256, 170), (310, 186), (364, 246), (310, 306), (256, 322), (202, 306)]
        line(draw, eye + [eye[0]], c(GOLD), 8, scale)
        draw.ellipse([round(222*scale), round(212*scale), round(290*scale), round(280*scale)], outline=c(CYAN), width=round(8 * scale))
        for i in range(level + 2):
            x = 150 + i * 48
            line(draw, [(x, 146), (x + 72, 346)], c(AMBER if i % 2 else CYAN, 0.45), 5, scale)
    elif family == "timer":
        hour = [(198, 146), (314, 146), (286, 234), (314, 338), (198, 338), (226, 234)]
        line(draw, hour + [hour[0]], c(GOLD), 8, scale)
        top_sand = [(224, 182), (288, 182), (260, 224), (252, 224)]
        bottom_sand = [(256, 248), (288, 304), (224, 304)]
        draw.polygon(sp(top_sand, scale), fill=c(CYAN, 0.92))
        draw.polygon(sp(bottom_sand, scale), fill=c(CYAN, 0.86))
        line(draw, [(256, 224), (256, 276)], c(CYAN, 0.92), 6, scale)
        for i in range(level + 1):
            x = 238 + i * 18
            line(draw, [(x, 320), (x + 10, 330)], c(AMBER, 0.62), 3, scale)
        for r in (118, 142):
            draw.arc([round((256-r)*scale), round((242-r)*scale), round((256+r)*scale), round((242+r)*scale)], 210, 326, fill=c(AMBER, 0.35 + level * 0.16), width=round(5 * scale))


def make_risk_icon(name: str, family: str, level: int) -> Image.Image:
    """生成完整六边形词条图标，并把内容裁切到统一边框内。"""
    def paint(img: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        draw_hex_base(draw, 512, scale)
        model_symbol = load_model_symbol(name, img.size)
        if model_symbol is not None:
            glow = tint_alpha(model_symbol, CYAN, 0.28).filter(ImageFilter.GaussianBlur(radius=round(2.2 * scale)))
            img.alpha_composite(glow)
            img.alpha_composite(model_symbol)
        else:
            glow = Image.new("RGBA", img.size, (0, 0, 0, 0))
            gd = ImageDraw.Draw(glow, "RGBA")
            draw_symbol(gd, family, level, scale)
            img.alpha_composite(glow.filter(ImageFilter.GaussianBlur(radius=round(2.0 * scale))))
            draw_symbol(draw, family, level, scale)
        draw_level_ticks(draw, level, 512, scale)
        mask = Image.new("L", img.size, 0)
        ImageDraw.Draw(mask).polygon(sp(hex_points(512, 238), scale), fill=255)
        alpha = img.getchannel("A")
        img.putalpha(Image.composite(alpha, Image.new("L", img.size, 0), mask))
    return render((512, 512), paint)


def load_model_symbol(name: str, target_size: tuple[int, int]) -> Image.Image | None:
    """读取 MCP 生成的中心符号，并把它归一化到统一六边形内。"""
    path = RAW_RISKS / f"{name}.png"
    if not path.exists():
        return None

    src = Image.open(path).convert("RGBA")
    bbox = src.getbbox()
    if bbox is None:
        return None

    symbol = src.crop(bbox)
    alpha = symbol.getchannel("A")
    symbol.putalpha(alpha.point(lambda value: 0 if value < 24 else value))
    max_w = round(target_size[0] * 0.58)
    max_h = round(target_size[1] * 0.58)
    scale = min(max_w / max(1, symbol.width), max_h / max(1, symbol.height))
    new_size = (max(1, round(symbol.width * scale)), max(1, round(symbol.height * scale)))
    symbol = symbol.resize(new_size, Image.Resampling.LANCZOS)

    result = Image.new("RGBA", target_size, (0, 0, 0, 0))
    x = (target_size[0] - new_size[0]) // 2
    y = round(target_size[1] * 0.49 - new_size[1] * 0.5)
    result.alpha_composite(symbol, (x, y))
    return result


def tint_alpha(src: Image.Image, color: tuple[int, int, int, int], strength: float) -> Image.Image:
    """按源图透明度生成单色辉光层。"""
    alpha = src.getchannel("A").point(lambda value: round(value * strength))
    tinted = Image.new("RGBA", src.size, color)
    tinted.putalpha(alpha)
    return tinted


def make_node_frame(kind: str) -> Image.Image:
    """生成统一六边形节点状态叠加框。"""
    colors = {
        "Normal": (GOLD, CYAN, 0.32),
        "Hover": (AMBER, CYAN, 0.55),
        "Selected": (AMBER, CYAN, 0.82),
        "Locked": (BRONZE, RED, 0.46),
    }
    main, accent, strength = colors[kind]
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        pts = hex_points(512, 236)
        draw_hex_outline(draw, pts, c(main, 0.92), 12, scale)
        draw_hex_outline(draw, hex_points(512, 205), c(accent, strength), 4, scale)
        if kind == "Locked":
            line(draw, [(178, 334), (334, 178)], c(RED, 0.72), 12, scale)
        if kind == "Selected":
            draw_hex_outline(draw, hex_points(512, 222), c(CYAN, 0.72), 8, scale)
            draw_hex_outline(draw, hex_points(512, 188), c(AMBER, 0.34), 3, scale)
            for i, point in enumerate(hex_points(512, 236)):
                x, y = point
                draw.ellipse([round((x - 16) * scale), round((y - 16) * scale), round((x + 16) * scale), round((y + 16) * scale)], fill=c(DARK, 0.78), outline=c(AMBER, 0.95), width=round(4 * scale))
                draw.ellipse([round((x - 7) * scale), round((y - 7) * scale), round((x + 7) * scale), round((y + 7) * scale)], fill=c(CYAN if i % 2 == 0 else AMBER, 0.88))
            for i in range(6):
                a = math.radians(-90 + i * 60 + 30)
                cx = 256 + math.cos(a) * 232
                cy = 256 + math.sin(a) * 232
                tangent = a + math.pi * 0.5
                p1 = (cx + math.cos(tangent) * 24, cy + math.sin(tangent) * 24)
                p2 = (cx - math.cos(tangent) * 24, cy - math.sin(tangent) * 24)
                line(draw, [p1, p2], c(CYAN, 0.56), 5, scale)
    return render((512, 512), paint)


def make_cut_frame(size: tuple[int, int], accent=GOLD, fill=PANEL, cut=34) -> Image.Image:
    """生成切角矩形面板或按钮框。"""
    w, h = size
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        pts = cut_rect(w, h, cut)
        inner = inset_cut_rect(w, h, 10, max(8, cut - 10))
        draw.polygon(sp(pts, scale), fill=fill)
        line(draw, pts + [pts[0]], c(BRONZE, 0.92), 10, scale)
        line(draw, inner + [inner[0]], c(accent, 0.78), 4, scale)
        line(draw, [(cut + 22, 14), (w * 0.42, 14)], c(CYAN, 0.34), 3, scale)
        line(draw, [(w * 0.58, h - 14), (w - cut - 22, h - 14)], c(CYAN, 0.24), 3, scale)
    return render(size, paint)


def make_selected_risk_card_frame() -> Image.Image:
    """生成右侧已选词条使用的完整高级卡片骨架。"""
    w, h = 768, 256
    cut = 34
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        outer = cut_rect(w, h, cut)
        inner = cut_rect(w - 24, h - 24, cut - 10)
        draw.polygon(sp(outer, scale), fill=(12, 13, 19, 230))
        line(draw, outer + [outer[0]], c(BRONZE, 0.98), 12, scale)
        line(draw, [(x + 12, y + 12) for x, y in inner] + [(inner[0][0] + 12, inner[0][1] + 12)], c(AMBER, 0.82), 4.5, scale)
        line(draw, [(74, 28), (w - 78, 28)], c(CYAN, 0.38), 3.5, scale)
        line(draw, [(86, h - 30), (w - 96, h - 30)], c(CYAN, 0.28), 3, scale)

        icon_bay = [(24, 58), (72, 22), (134, 22), (182, 58), (182, 184), (134, 224), (72, 224), (24, 184)]
        draw.polygon(sp(icon_bay, scale), fill=(15, 23, 27, 196))
        line(draw, icon_bay + [icon_bay[0]], c(CYAN, 0.72), 4, scale)
        line(draw, [(196, 38), (196, 216)], c(AMBER, 0.46), 3.5, scale)

        corner_nodes = [(34, 34), (w - 34, 34), (34, h - 34), (w - 34, h - 34)]
        for i, (x, y) in enumerate(corner_nodes):
            draw.ellipse([round((x - 13) * scale), round((y - 13) * scale), round((x + 13) * scale), round((y + 13) * scale)], fill=c(DARK, 0.82), outline=c(AMBER, 0.92), width=round(3.5 * scale))
            draw.ellipse([round((x - 5) * scale), round((y - 5) * scale), round((x + 5) * scale), round((y + 5) * scale)], fill=c(CYAN if i % 2 == 0 else AMBER, 0.88))

        for x in (224, 292, 360, 428, 496, 564):
            line(draw, [(x, h - 42), (x + 28, h - 42)], c(AMBER, 0.36), 3, scale)
    return render((w, h), paint)


def make_background() -> Image.Image:
    """生成暗色沙海终端底图。"""
    rng = random.Random(9)
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        draw.rectangle([0, 0, 1536 * scale, 960 * scale], fill=(8, 10, 16, 236))
        for y in range(80, 930, 80):
            line(draw, [(40, y), (1490, y + rng.randint(-18, 18))], c(BRONZE, 0.10), 2, scale)
        for x in range(90, 1500, 120):
            line(draw, [(x, 40), (x + rng.randint(-24, 24), 930)], c(CYAN, 0.05), 1, scale)
        for _ in range(120):
            x, y = rng.randint(0, 1536), rng.randint(0, 960)
            draw.ellipse([x * scale, y * scale, (x + 2) * scale, (y + 2) * scale], fill=c(GOLD, rng.uniform(0.05, 0.18)))
    return render((1536, 960), paint)


def make_overlay(size=(1536, 960)) -> Image.Image:
    """生成透明沙尘与网格覆盖层。"""
    rng = random.Random(17)
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        for _ in range(180):
            x, y = rng.randint(0, size[0]), rng.randint(0, size[1])
            line(draw, [(x, y), (x + rng.randint(8, 34), y - rng.randint(1, 8))], c(GOLD, rng.uniform(0.04, 0.16)), 1.2, scale)
    return render(size, paint)


def make_grid() -> Image.Image:
    """生成风险矩阵透明网格背景。"""
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        for x in range(64, 960, 96):
            line(draw, [(x, 20), (x, 700)], c(CYAN, 0.08), 1.5, scale)
        for y in range(64, 700, 72):
            line(draw, [(20, y), (980, y)], c(GOLD, 0.08), 1.5, scale)
        for y in range(88, 660, 76):
            for x in range(92, 920, 110):
                draw_hex_outline(draw, [(px + x - 256, py + y - 256) for px, py in hex_points(512, 30)], c(GOLD, 0.10), 1.5, scale)
    return render((1024, 720), paint)


def make_connection_node() -> Image.Image:
    """生成连线中继能量节点。"""
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        draw_hex_base(draw, 128, scale, 0.65)
        line(draw, [(32, 64), (96, 64)], c(CYAN, 0.9), 5, scale)
        line(draw, [(64, 32), (64, 96)], c(AMBER, 0.7), 4, scale)
    return render((128, 128), paint)


def make_connection_flow() -> Image.Image:
    """生成连线流光纹理。"""
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        for i in range(0, 512, 64):
            line(draw, [(i, 32), (i + 38, 32)], c(CYAN, 0.65), 8, scale)
            line(draw, [(i + 42, 32), (i + 56, 32)], c(AMBER, 0.38), 4, scale)
    return render((512, 64), paint)


def make_hud_fill() -> Image.Image:
    """生成血条填充纹理。"""
    def paint(_: Image.Image, draw: ImageDraw.ImageDraw, scale: int) -> None:
        draw.rectangle([0, 0, 512 * scale, 64 * scale], fill=(255, 255, 255, 225))
        for x in range(-64, 560, 64):
            line(draw, [(x, 64), (x + 80, 0)], (0, 0, 0, 42), 12, scale)
            line(draw, [(x + 26, 64), (x + 106, 0)], c(CYAN, 0.14), 4, scale)
    return render((512, 64), paint)


def generate() -> None:
    """执行全部素材生成流程。"""
    save(make_background(), "TerminalBackground.png")
    save(make_overlay(), "SandDustOverlay.png")
    save(make_grid(), "RiskMatrixGrid.png")
    save(make_cut_frame((1024, 768), GOLD, (13, 13, 20, 212), 46), "MainPanelFrame.png")
    save(make_cut_frame((768, 512), AMBER, (17, 15, 24, 230), 42), "DetailPopupFrame.png")
    save(make_cut_frame((512, 640), GOLD, (13, 14, 20, 188), 34), "ScrollPanelFrame.png")
    save(make_selected_risk_card_frame(), "SelectedRiskCardFrame.png")
    save(make_cut_frame((768, 192), AMBER, (28, 24, 28, 224), 34), "ButtonPrimary.png")
    save(make_cut_frame((768, 192), GOLD, (20, 21, 29, 205), 34), "ButtonSecondary.png")
    save(make_cut_frame((768, 192), (92, 86, 92, 255), (15, 15, 18, 170), 34), "ButtonDisabled.png")
    save(make_cut_frame((768, 192), RED, (30, 14, 12, 218), 34), "ButtonDanger.png")
    for kind in ("Normal", "Hover", "Selected", "Locked"):
        save(make_node_frame(kind), f"HexNode{kind}.png")
    save(make_connection_node(), "ConnectionNode.png")
    save(make_connection_flow(), "ConnectionFlow.png")
    save(make_cut_frame((768, 128), AMBER, (12, 12, 17, 224), 30), "HudBossFrame.png")
    save(make_cut_frame((512, 96), TEAL, (10, 16, 18, 214), 20), "HudSmallFrame.png")
    save(make_hud_fill(), "HudBarFill.png")
    save(make_overlay((768, 128)), "HudCriticalOverlay.png")
    for name, family, level in RISKS:
        save(make_risk_icon(name, family, level), f"Risks/{name}.png")


if __name__ == "__main__":
    generate()
