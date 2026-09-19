using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace BANWlLib.mainUI.Images
{
    //读取轻量图片目录，只在索引中记录位置，不预加载贴图或资源包。
    internal static class BAUIImageCatalog
    {
        private static Dictionary<string, BAUIImageRecord> records;
        internal static float IdleSeconds = 30f;
        internal static long IdleBytes = 32L * 1024 * 1024;

        //首次请求时加载索引和内存策略，没有索引时直接使用散图模式。
        internal static BAUIImageRecord Find(string key)
        {
            if (records == null)
            {
                records = new Dictionary<string, BAUIImageRecord>(StringComparer.OrdinalIgnoreCase);
                string path = Path.Combine(BAUIImagePath.Root, "1.6/UI/images/bundles.xml");
                if (File.Exists(path))
                {
                    XElement root = XDocument.Load(path).Root;
                    foreach (XElement image in root.Elements("Image"))
                    {
                        string bundle = (string)image.Attribute("bundle");
                        if (Path.GetFileName(bundle) != bundle) throw new InvalidDataException("UI 图片包名称不得包含目录：" + bundle);
                        records.Add((string)image.Attribute("key"), new BAUIImageRecord
                        {
                            Bundle = bundle,
                            Asset = (string)image.Attribute("asset"),
                            Shared = (bool?)image.Attribute("shared") ?? false,
                            Transient = (bool?)image.Attribute("transient") ?? false
                        });
                    }
                }
                string settings = Path.Combine(BAUIImagePath.Root, "1.6/UI/images/memory.xml");
                if (File.Exists(settings))
                {
                    XElement root = XDocument.Load(settings).Root;
                    IdleSeconds = Math.Max(0, (float?)root.Element("idleSeconds") ?? 30f);
                    IdleBytes = Math.Max(0, (long?)root.Element("idleMegabytes") ?? 32) * 1024 * 1024;
                }
            }
            records.TryGetValue(key, out BAUIImageRecord result);
            return result;
        }

        //清空目录，以便重建 UI 时重新读取配置。
        internal static void Reset()
        {
            records = null;
            IdleSeconds = 30f;
            IdleBytes = 32L * 1024 * 1024;
        }
    }
}
