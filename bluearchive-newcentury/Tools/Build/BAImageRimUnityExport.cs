using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using RimUnity.Editor;
using UnityEditor;
using UnityEngine;

//从立绘和特效源工程导出标准图片清单，保留导入器决定的像素、色彩空间和采样状态。
public static class BAImageRimUnityExport
{
    private const string Mod = "E:/RimModDev/bluearchive-newcentury/bluearchive-newcentury";
    public static void Export()
    {
        string output = Environment.GetEnvironmentVariable("BA_RIMUNITY_IMAGE_OUTPUT");
        if (string.IsNullOrWhiteSpace(output)) throw new InvalidOperationException("需要 BA_RIMUNITY_IMAGE_OUTPUT 指定导出目录。");
        Directory.CreateDirectory(output + "/About");
        File.Copy(Mod + "/About/About.xml", output + "/About/About.xml", true);
        var requests = new List<ExportRequest>();
        var assets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in XDocument.Load(Mod + "/1.7/UI/images/bundles.xml").Root.Elements("Image"))
        {
            string path = (string)row.Attribute("asset");
            if (!assets.Add(path)) continue;
            Add(requests, path, "UIImage/" + path);
        }
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/EffectImages" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string relative = path.Substring("Assets/EffectImages/".Length);
            relative = relative.Substring(0, relative.Length - Path.GetExtension(relative).Length);
            Add(requests, path, "Effects/Effect/" + relative);
        }
        PrefabExporter.Export(output, requests.ToArray());
        Debug.Log("BA_RIMUNITY_IMAGES_COMPLETE " + requests.Count + " " + output);
    }
    private static void Add(List<ExportRequest> requests, string path, string logical)
    {
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (!texture) throw new FileNotFoundException("标准导出缺少源贴图：" + path);
        requests.Add(new ExportRequest { asset = texture, path = logical });
    }
}
