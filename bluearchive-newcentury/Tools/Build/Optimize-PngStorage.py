import argparse
import concurrent.futures
import io
import json
import struct
import subprocess
import zlib
from pathlib import Path
from xml.etree import ElementTree


#读取并校验PNG块，保留颜色、透明度和其他元数据的原始字节。
def read_png(original, path):
    if original[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError(f"不是有效的PNG：{path}")
    chunks = []
    image_data = []
    offset = 8
    while offset < len(original):
        if offset + 12 > len(original):
            raise ValueError(f"PNG块不完整：{path}")
        length = struct.unpack_from(">I", original, offset)[0]
        end = offset + length + 12
        if end > len(original):
            raise ValueError(f"PNG块超出文件：{path}")
        kind = original[offset + 4:offset + 8]
        data = original[offset + 8:end - 4]
        checksum = struct.unpack_from(">I", original, end - 4)[0]
        if zlib.crc32(kind + data) != checksum:
            raise ValueError(f"PNG块校验失败：{path}")
        chunks.append((kind, original[offset:end]))
        if kind == b"IDAT":
            image_data.append(data)
        offset = end
    if not image_data or chunks[-1][0] != b"IEND":
        raise ValueError(f"PNG缺少图像数据或结束块：{path}")
    return chunks, b"".join(image_data)


#仅替换图像数据，其他块原样写回，避免优化器重写颜色元数据。
def replace_idat(chunks, compressed):
    block = b"IDAT" + compressed
    block = struct.pack(">I", len(compressed)) + block + struct.pack(">I", zlib.crc32(block))
    output = [b"\x89PNG\r\n\x1a\n"]
    inserted = False
    for kind, chunk in chunks:
        if kind != b"IDAT":
            output.append(chunk)
        elif not inserted:
            output.append(block)
            inserted = True
    return b"".join(output)


#基础整理保留扫描线过滤；深度整理允许重选过滤方式，但必须逐像素一致。
def optimize_png(path, oxipng):
    original = path.read_bytes()
    chunks, image_data = read_png(original, path)
    if oxipng:
        process = subprocess.run(
            [str(oxipng), "-o", "4", "--nx", "--threads", "1", "--stdout", "--quiet", str(path)],
            stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        if process.returncode:
            raise RuntimeError(f"PNG压缩失败：{path}\n{process.stderr.decode('utf-8', errors='replace')}")
        candidate_chunks, compressed = read_png(process.stdout, path)
        fixed = {b"IHDR", b"PLTE", b"tRNS"}
        if [chunk for kind, chunk in chunks if kind in fixed] != [
                chunk for kind, chunk in candidate_chunks if kind in fixed]:
            raise ValueError(f"PNG图像格式发生变化：{path}")
    else:
        decoded = zlib.decompress(image_data)
        compressed = zlib.compress(decoded, 9)
        if zlib.decompress(compressed) != decoded:
            raise ValueError(f"PNG无损核对失败：{path}")
    result = replace_idat(chunks, compressed)
    if len(result) >= len(original):
        return len(original), len(original), False
    if oxipng:
        from PIL import Image
        with Image.open(io.BytesIO(original)) as before, Image.open(io.BytesIO(result)) as after:
            if before.mode != after.mode or before.size != after.size or before.tobytes() != after.tobytes():
                raise ValueError(f"PNG像素核对失败：{path}")
    temporary = path.with_suffix(".png.tmp")
    temporary.write_bytes(result)
    temporary.replace(path)
    return len(original), len(result), True


#在模组暂存副本中整理贴图，RimUnity载荷随后交给ResourcePacker重建内容哈希。
def main():
    parser = argparse.ArgumentParser(description="无损整理模组暂存副本的PNG文件")
    parser.add_argument("root", type=Path, help="模组暂存副本目录")
    parser.add_argument("--oxipng", type=Path, help="深度整理所用的oxipng可执行文件；需要Pillow核对像素")
    args = parser.parse_args()
    root = args.root.resolve(strict=True)
    oxipng = args.oxipng.resolve(strict=True) if args.oxipng else None
    owner = ElementTree.parse(root / "About/About.xml").findtext("packageId")
    if owner != "Archive.NewWorld":
        raise ValueError("暂存副本的packageId不匹配")
    folders = ("Common/Textures", "Common/UIAssets", "1.7/Textures", "1.7/Data/RimUnity/blobs")
    files = sorted((path for folder in folders for path in (root / folder).rglob("*.png")),
                   key=lambda path: path.stat().st_size, reverse=True)
    before = after = changed = 0
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as executor:
        futures = [executor.submit(optimize_png, path, oxipng) for path in files]
        for count, future in enumerate(concurrent.futures.as_completed(futures), 1):
            old_size, new_size, updated = future.result()
            before += old_size
            after += new_size
            changed += int(updated)
            if count % 25 == 0:
                print(f"PNG {count}/{len(files)}，已节省 {(before - after) / 1048576:.2f} MiB", flush=True)
    print(json.dumps(dict(files=len(files), changed=changed, beforeBytes=before,
                          afterBytes=after, savedBytes=before - after), ensure_ascii=False), flush=True)


if __name__ == "__main__":
    main()
