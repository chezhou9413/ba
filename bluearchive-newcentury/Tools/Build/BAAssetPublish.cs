using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using RimUnity.Editor;
using UnityEngine;

//使用 RimUnity 正式整理器压缩预制体并去重载荷，合并两个制作工程的清单。
public static class BAAssetPublish
{
    public static void Prepare(string uiExport, string imageExport)
    {
        string suffix = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string output = uiExport + "-Ready-" + suffix;
        string ui = uiExport + "-Packed-" + suffix, images = imageExport + "-Packed-" + suffix;
        ExportStorage.CopyForPublish(uiExport + "/1.7/Data/RimUnity", ui);
        ExportStorage.CopyForPublish(imageExport + "/1.7/Data/RimUnity", images);
        var catalog = JObject.Parse(File.ReadAllText(ui + "/catalog.json"));
        var other = JObject.Parse(File.ReadAllText(images + "/catalog.json"));
        if ((string)catalog["owner"] != (string)other["owner"]) throw new InvalidDataException("两个工程的资源归属不一致。");
        var assets = (JArray)catalog["assets"];
        var ids = new HashSet<string>(assets.Select(a => (string)a["id"]), StringComparer.Ordinal);
        var paths = new HashSet<string>(assets.Select(a => (string)a["path"]), StringComparer.OrdinalIgnoreCase);
        foreach (var asset in (JArray)other["assets"])
        {
            if (!ids.Add((string)asset["id"]) || !paths.Add((string)asset["path"]))
                throw new InvalidDataException("工程之间存在重复资源身份或入口：" + asset["path"]);
            assets.Add(asset.DeepClone());
        }
        if (Directory.Exists(output)) throw new IOException("整理目录已经存在：" + output);
        Directory.CreateDirectory(output);
        foreach (string source in new[] { ui, images })
        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string relative = file.Substring(source.Length + 1);
            if (relative == "catalog.json") continue;
            string target = Path.Combine(output, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            if (!File.Exists(target)) File.Copy(file, target);
            else if (!File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(target)))
                throw new InvalidDataException("同名载荷内容不同：" + relative);
        }
        File.WriteAllText(output + "/catalog.json", catalog.ToString(Newtonsoft.Json.Formatting.None), new System.Text.UTF8Encoding(false));
        Debug.Log("BA_RIMUNITY_READY " + output + " assets=" + assets.Count);
    }
}
