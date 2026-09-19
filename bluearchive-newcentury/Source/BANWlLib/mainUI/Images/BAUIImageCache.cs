using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
using Verse;

namespace BANWlLib.mainUI.Images
{
    //统一管理可见 UI 图片的引用与闲置缓存，所有 Unity 对象操作均在主线程执行。
    public static class BAUIImageCache
    {
        private static readonly Dictionary<string, BAUIImageEntry> entries = new Dictionary<string, BAUIImageEntry>(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<BAUIImageBinding> bindings = new HashSet<BAUIImageBinding>();

        //为图片控件绑定资源路径，控件显示、滚动与关闭时自动管理引用。
        public static void SetImage(Image image, string path)
        {
            if (image == null) return;
            BAUIImageCacheDriver.Ensure();
            BAUIImageBinding binding = image.GetComponent<BAUIImageBinding>() ?? image.gameObject.AddComponent<BAUIImageBinding>();
            bindings.Add(binding);
            binding.Configure(image, path);
        }

        //取得一份显示引用；首次引用时读取单张图片，失败只向调用控件报告一次。
        internal static BAUIImageEntry Acquire(string path)
        {
            string key = BAUIImagePath.Key(path);
            if (key == null) return null;
            try
            {
                if (!entries.TryGetValue(key, out BAUIImageEntry entry))
                {
                    entry = Load(path, key);
                    entries.Add(key, entry);
                }
                entry.References++;
                return entry;
            }
            catch (Exception ex)
            {
                Log.Error("[BA UI图片] 加载失败：" + path + "\n" + ex);
                return null;
            }
        }

        //优先读取专用 AB，缺包或未收录时读取散图，再查询原版共享贴图。
        private static BAUIImageEntry Load(string path, string key)
        {
            BAUIImageRecord record = BAUIImageCatalog.Find(key);
            Texture2D texture = null;
            string bundle = null;
            bool borrowed = false;
            if (record != null && !record.Shared)
            {
                texture = BAUIImageBundles.Load(record);
                if (texture != null) bundle = record.Bundle;
            }
            string nativePath = BAUIImagePath.NativePath(key);
            //共享图标先交给原版查找，避免同一张技能或物品贴图出现两份内存副本。
            if (texture == null && nativePath != null)
            {
                texture = ContentFinder<Texture2D>.Get(nativePath, false);
                borrowed = texture != null;
            }
            if (texture == null)
            {
                string file = BAUIImagePath.FindLoose(path, key);
                if (file != null) texture = BAUILooseTextureReader.Read(file);
            }
            if (texture == null) throw new System.IO.FileNotFoundException("AB 与散图均未找到图片", path);
            try
            {
                return new BAUIImageEntry
                {
                    Key = key, Texture = texture, Bundle = bundle, Borrowed = borrowed,
                    Transient = record?.Transient == true || key.IndexOf("/ManuaUI/Live/", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        key.IndexOf("/ManuaUI/Bg/", StringComparison.OrdinalIgnoreCase) >= 0 || (long)texture.width * texture.height >= 512 * 1024,
                    Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f),
                    Bytes = Profiler.GetRuntimeMemorySizeLong(texture)
                };
            }
            catch
            {
                if (bundle != null) BAUIImageBundles.Release(bundle, texture);
                else if (!borrowed) UnityEngine.Object.Destroy(texture);
                throw;
            }
        }

        //归还显示引用，大图立即释放，小图进入有上限的闲置缓存。
        internal static void Release(BAUIImageEntry entry)
        {
            if (entry == null) return;
            entry.References--;
            if (entry.References != 0) return;
            entry.LastUse = Time.realtimeSinceStartup;
            if (entry.Transient) Evict(entry);
            else Trim(false);
        }

        //按闲置时长和最近使用顺序回收缓存，绝不销毁仍被显示引用持有的对象。
        public static void Trim(bool allIdle)
        {
            List<BAUIImageEntry> idle = entries.Values.Where(e => e.References == 0).OrderBy(e => e.LastUse).ToList();
            long bytes = idle.Sum(e => e.Bytes);
            foreach (BAUIImageEntry entry in idle)
            {
                if (!allIdle && bytes <= BAUIImageCatalog.IdleBytes && Time.realtimeSinceStartup - entry.LastUse < BAUIImageCatalog.IdleSeconds) continue;
                bytes -= entry.Bytes;
                Evict(entry);
            }
        }

        //成对释放包装精灵和自有纹理，原版共享纹理只释放包装对象。
        private static void Evict(BAUIImageEntry entry)
        {
            entries.Remove(entry.Key);
            UnityEngine.Object.Destroy(entry.Sprite);
            if (entry.Bundle != null) BAUIImageBundles.Release(entry.Bundle, entry.Texture);
            else if (!entry.Borrowed) UnityEngine.Object.Destroy(entry.Texture);
        }

        //控件销毁时移除跟踪，防止静态集合保留已销毁的界面。
        internal static void Forget(BAUIImageBinding binding)
        {
            bindings.Remove(binding);
        }

        //重建或离开存档时先断开界面引用，再释放全部自有图片和索引。
        public static void Reset()
        {
            foreach (BAUIImageBinding binding in bindings.ToArray())
                if (binding != null) binding.Clear();
            bindings.Clear();
            Trim(true);
            BAUIImageCatalog.Reset();
        }

        //汇总实际已加载的纹理内存，区分可回收缓存与借用的原版图标。
        public static string Describe()
        {
            long active = entries.Values.Where(e => e.References > 0 && !e.Borrowed).Sum(e => e.Bytes);
            long idle = entries.Values.Where(e => e.References == 0 && !e.Borrowed).Sum(e => e.Bytes);
            long shared = entries.Values.Where(e => e.Borrowed).Sum(e => e.Bytes);
            return $"[BA UI内存] 图片 {entries.Count}，显示引用 {entries.Values.Sum(e => e.References)}，专用包 {BAUIImageBundles.Count}\n" +
                $"自有显示纹理 {active / 1048576f:F2} MiB，闲置纹理 {idle / 1048576f:F2} MiB，共享纹理 {shared / 1048576f:F2} MiB\n" +
                $"闲置上限 {BAUIImageCatalog.IdleBytes / 1048576} MiB，闲置期限 {BAUIImageCatalog.IdleSeconds} 秒。共享纹理生命周期由游戏管理。";
        }
    }
}
