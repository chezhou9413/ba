"""按已导出的 UI 材质裁剪无效场景采样，并编译检查修改后的着色器。"""
from pathlib import Path
import gzip
import json
import re
import subprocess
import tempfile


def read(path):
    """读取导出器的 UTF-8 文本。"""
    if path.suffix == ".gz":
        return gzip.decompress(path.read_bytes()).decode("utf-8-sig")
    return path.read_text(encoding="utf-8-sig")


def block_end(source, start):
    """定位生成代码中的完整大括号块。"""
    depth, index = 1, start + 1
    while depth:
        depth += (source[index] == "{") - (source[index] == "}")
        index += 1
    return index


def fold(source, condition, enabled):
    """只折叠已核实关闭的材质开关，保留选中分支的原始计算。"""
    pattern = re.compile(r"\bif\s*\(" + condition + r"\)\s*\{")
    while match := pattern.search(source):
        start = source.index("{", match.start())
        end = block_end(source, start)
        other = re.match(r"\s*else\s*\{", source[end:])
        if not other:
            raise ValueError("导出分支结构改变：" + condition)
        other_start = end + other.end() - 1
        other_end = block_end(source, other_start)
        selected = source[start + 1:end - 1] if enabled else source[other_start + 1:other_end - 1]
        source = source[:match.start()] + selected + source[other_end:]
    return source


def drop_variable(source, pattern):
    """仅移除已经没有读者的局部计算，发现额外引用时中止构建。"""
    for match in reversed(list(re.finditer(pattern, source, re.M))):
        name = match.group(1)
        if len(re.findall(r"\b" + name + r"\b", source)) != 1:
            raise ValueError("仍有场景采样结果使用者：" + name)
        source = source[:match.start()] + source[match.end():]
    return source


def drop_function(source, name):
    """删除不再调用的采样辅助函数。"""
    if len(re.findall(r"\b" + re.escape(name) + r"\b", source)) != 1:
        raise ValueError("采样函数仍被调用：" + name)
    match = re.search(r"\b(?:float|vec4)\s+" + name + r"\([^)]*\)\s*\{", source)
    return source[:match.start()] + source[block_end(source, source.index("{", match.start())):]


def safe_smooth(source):
    """相等边界使用硬切，向量逐分量处理，消除零宽度除零。"""
    for size in range(1, 5):
        kind = "float" if size == 1 else "vec" + str(size)
        pattern = re.compile(r"\b" + kind + r" RU_CompatSmoothstep\(" + kind + r" low, " + kind + r" high, " + kind + r" x\) \{[^{}]*\}")
        if size == 1:
            body = "    if (low == high) return step(low, x);\n    float t = clamp((x - low) / (high - low), 0.0, 1.0);\n    return t * t * (3.0 - 2.0 * t);"
        else:
            body = "    return " + kind + "(" + ", ".join(f"RU_CompatSmoothstep(low.{c}, high.{c}, x.{c})" for c in "xyzw"[:size]) + ");"
        source = pattern.sub(lambda _: kind + " RU_CompatSmoothstep(" + kind + " low, " + kind + " high, " + kind + " x) {\n" + body + "\n}", source)
    return source


def specialize_panda(source):
    """UI 专用变体只保留实际使用的纹理、溶解与颜色计算。"""
    marker = "//碧蓝档案 UI：不读取场景颜色与地表深度。"
    if marker in source:
        return source
    source = fold(source, r"bool\(RU_Material_0\._Depthfadeon\)", False)
    source = fold(source, r"\(RU_Material_0\._DepthF\)\s*==\s*0\.0", True)
    source = fold(source, r"\(RU_Material_0\._ScreenAsMain\)\s*==\s*0\.0", True)
    for pattern in (r"^\s*vec4 (DepthColor412_\d+) = [^;]*;", r"^\s*float (temp_output_409_0_\d+) = [^;]*;", r"^\s*float (distanceDepth88_\d+) = [^;]*;", r"^\s*vec4 (screenColor917_\d+) = [^;]*;"):
        source = drop_variable(source, pattern)
    for name in ("RU_Ground_tex2D_0", "RU_GroundDepth_0", "RU_tex2D_GrabTexture_0"):
        source = drop_function(source, name)
    source, count = re.subn(r"layout\([^)]*\)\s*uniform sampler2D _GrabTexture;", "", source)
    if count != 1:
        raise ValueError("场景采样器声明结构改变")
    source = source.replace("RU_CompatPow3(1.0)", "1.0").replace("RU_CompatPow3(RU_Material_0._StaticNormalOffset)", "0.0")
    source = re.sub(r"\b(?:float|vec[234]) RU_CompatPow3\([^)]*\) \{[^{}]*\}", "", source)
    if any(token in source for token in ("RU_CompatPow3", "_GrabTexture", "RU_Ground")):
        raise ValueError("UI 变体仍有未裁剪的场景采样或整数幂")
    return source.replace("#version 450", "#version 450\n" + marker, 1)


def main():
    """全部阶段通过游戏编译器与布局检查后，才写回资源。"""
    mod = Path(__file__).resolve().parents[2]
    root = mod / "1.7/Data/RimUnity"
    assets = json.loads(read(root / "catalog.json"))["assets"]
    selected = [(a, json.loads(read(root / a["file"]))) for a in assets if a["type"] == "Shader"]
    selected = [(a, d) for a, d in selected if d["sourceName"] in ("VFX/Pandavfx_v2.3", "BAUIMIAN/UI/Cost Ring Progress")]
    panda = {a["id"] for a, d in selected if d["sourceName"] == "VFX/Pandavfx_v2.3"}
    switches = ("_Depthfadeon", "_DepthF", "_ScreenAsMain", "_StaticNormalOffset")
    checked = 0
    for asset in assets:
        if asset["type"] == "AnimationClip" and any(key in read(root / asset["file"]) for key in switches):
            raise ValueError("动画会改变 UI 专用开关：" + asset["name"])
        if asset["type"] != "Material":
            continue
        data = json.loads(read(root / asset["file"]))
        props = {m["name"]: m["value"] for m in data["members"]}
        if props["shader"].get("text") not in panda:
            continue
        for key in switches:
            if props[key]["number"] != 0:
                raise ValueError(asset["name"] + " 需要通用 Shader，不能裁剪 " + key)
        checked += 1
    if not checked or len(selected) != 7:
        raise ValueError("UI 资源集合改变，请重新核对导出内容")
    #程序身份与通用导出分离，防止其他模组的同名 Shader 命中 UI 专用缓存。
    suffix = "_BAUiSpecialized1"
    keys = {a["id"]: a["id"] + suffix for a, _ in selected if not a["id"].endswith(suffix)}
    pending, stages = {}, {}
    for asset, data in selected:
        for field, suffix in (("vertexFile", ".vert.glsl"), ("fragmentFile", ".frag.glsl")):
            path = root / data[field]
            if path not in pending:
                source = safe_smooth(read(path))
                if data["sourceName"] == "VFX/Pandavfx_v2.3" and field == "fragmentFile":
                    source = specialize_panda(source)
                elif data["sourceName"] == "VFX/Pandavfx_v2.3":
                    if len(re.findall(r"\b_GrabTexture\b", source)) > 1:
                        raise ValueError("顶点阶段仍在使用场景颜色")
                    source = re.sub(r"layout\([^)]*\)\s*uniform sampler2D _GrabTexture;[^\S\r\n]*(?://[^\r\n]*)?", "", source)
                pending[path], stages[path] = re.sub(r"(?m)^[ \t]+$", "", source), suffix
        for path in (root / asset["file"], root / data["descriptorFile"]):
            value = json.loads(pending.get(path, read(path)))
            prefixes = ("smoothstep 按 HLSL", "常量正整数幂", "GrabPass 按渲染队列", "软粒子深度使用")
            value["approximations"] = [v for v in value.get("approximations", []) if not v.startswith(prefixes)]
            if data["sourceName"] == "VFX/Pandavfx_v2.3":
                value["grabTexture"] = None
            pending[path] = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
    if keys:
        for path in {root / "catalog.json", *(root / a["file"] for a in assets)} | set(pending):
            source = pending.get(path, read(path))
            rewritten = source
            for before, after in keys.items():
                rewritten = rewritten.replace('"' + before + '"', '"' + after + '"')
            if rewritten != source:
                pending[path] = rewritten
    tool = Path("E:/RimModDev/RimUnity/Tools/bin/ShaderTool/RimUnity.ShaderTool.exe")
    with tempfile.TemporaryDirectory(prefix="BAUiShaders-") as temp:
        for index, (path, suffix) in enumerate(stages.items()):
            staged = Path(temp) / (str(index) + suffix)
            staged.write_text(pending[path], encoding="utf-8")
            subprocess.run([str(tool), "E:/steam/steamapps/common/RimWorld", "verify", str(staged)], check=True)
    for path, source in pending.items():
        if source != read(path):
            if path.suffix == ".gz":
                path.write_bytes(gzip.compress(source.encode("utf-8"), mtime=0))
            else:
                path.write_text(source, encoding="utf-8")
    print(f"已核对 {checked} 个 UI 材质，更新 {len(selected)} 个 Shader，{len(stages)} 个阶段通过编译。")


if __name__ == "__main__":
    main()
