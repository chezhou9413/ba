using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using RimUnity.Editor;
using RimUnity.Serialization;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

//通过正式导出器检查现有 UI，诊断副产物不进入模组发布目录。
public static class BAUiRimUnityProbe
{
    private const string Mod = "E:/RimModDev/bluearchive-newcentury/bluearchive-newcentury";
    private static readonly BindingFlags Internal = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static readonly List<object> Results = new List<object>();
    private static readonly List<object> Issues = new List<object>();
    private static readonly Dictionary<string, int> Types = new Dictionary<string, int>();
    private static string output;

    //仅刷新主画布的相机绘制变体，其余已完成入口保持原有清单。
    public static void RefreshMain()
    {
        string root = Environment.GetEnvironmentVariable("BA_RIMUNITY_UI_OUTPUT");
        if (string.IsNullOrWhiteSpace(root)) throw new InvalidOperationException("需要 BA_RIMUNITY_UI_OUTPUT 指定已有导出目录。");
        string oldTool = EditorPrefs.GetString("RimUnity.ShaderTool", ""), oldGame = EditorPrefs.GetString("RimUnity.GameRoot", "");
        try
        {
            EditorPrefs.SetString("RimUnity.ShaderTool", "E:/RimModDev/RimUnity/Tools/bin/ShaderTool/RimUnity.ShaderTool.exe");
            EditorPrefs.SetString("RimUnity.GameRoot", "E:/steam/steamapps/common/RimWorld");
            PrefabExporter.Export(root, new ExportOptions { cameraUi = true }, new ExportRequest
            { asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Scenes/Resources/UI/mainUI.prefab"), path = "Prefabs/mainUI" });
            BAAssetPublish.Prepare(root, "E:/ModdevMics/Outputs/BlueArchive/RimUnityImages-20261015");
        }
        finally
        {
            EditorPrefs.SetString("RimUnity.ShaderTool", oldTool); EditorPrefs.SetString("RimUnity.GameRoot", oldGame);
        }
    }

    //逐资源记录成功或完整异常；不删组件、不使用降级导出选项。
    public static void Export()
    {
        output = "E:/ModdevMics/Outputs/BlueArchive/RimUnityProbe-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
        Directory.CreateDirectory(output + "/About");
        File.Copy(Mod + "/About/About.xml", output + "/About/About.xml");
        string oldTool = EditorPrefs.GetString("RimUnity.ShaderTool", "");
        string oldGame = EditorPrefs.GetString("RimUnity.GameRoot", "");
        try
        {
            EditorPrefs.SetString("RimUnity.ShaderTool", "E:/RimModDev/RimUnity/Tools/bin/ShaderTool/RimUnity.ShaderTool.exe");
            EditorPrefs.SetString("RimUnity.GameRoot", "E:/steam/steamapps/common/RimWorld");
            var paths = File.ReadAllLines(Mod + "/Tools/Build/UIExportAssets.txt");
            foreach (string path in paths.Distinct())
            {
                if (!path.EndsWith(".prefab"))
                {
                    var asset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (asset is Texture2D) TryExport(asset, path, "Texture2D/" + path);
                    foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())
                        TryExport(sprite, path + "/" + sprite.name, "Sprite/" + path + "/" + sprite.name);
                    continue;
                }
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                TryExport(prefab, path, "Prefabs/" + Path.GetFileNameWithoutExtension(path));
                if (prefab != null) InspectComponents(prefab, path);
                Save();
            }
            string atlas = "Assets/Scenes/Resources/Gaka/abImage/card.spriteatlas";
            TryExport(AssetDatabase.LoadMainAssetAtPath(atlas), atlas, "Atlases/card");
            string digits = "Assets/Scenes/Resources/Cost/CostDigitsAtlas.png";
            foreach (var sprite in AssetDatabase.LoadAllAssetsAtPath(digits).OfType<Sprite>())
                TryExport(sprite, digits + "/" + sprite.name, "Sprites/Cost/" + sprite.name);
            Save();
            if (Results.Any(r => (string)r.GetType().GetProperty("status").GetValue(r) != "exported"))
                throw new InvalidOperationException("导出仍有失败项，详情见 " + output + "/report.json");
            BAAssetPublish.Prepare(output, "E:/ModdevMics/Outputs/BlueArchive/RimUnityImages-20261015");
            Debug.Log("BA_RIMUNITY_PROBE_COMPLETE " + output);
        }
        finally
        {
            EditorPrefs.SetString("RimUnity.ShaderTool", oldTool);
            EditorPrefs.SetString("RimUnity.GameRoot", oldGame);
        }
    }

    //单项失败只写报告，成功资源仍由正式导出器提交有效清单。
    private static void TryExport(Object asset, string source, string logical)
    {
        try
        {
            if (asset == null) throw new FileNotFoundException("源资源不存在：" + source);
            bool cameraUi = source.Contains("/GakaObj/") || source.Contains("/MissionGameObj/") || source.EndsWith("/lizi.prefab") || source.EndsWith("/mainUI.prefab");
            PrefabExporter.Export(output, new ExportOptions { cameraUi = cameraUi }, new ExportRequest { asset = asset, path = logical });
            Results.Add(new { source, status = "exported" });
        }
        catch (Exception e) { Results.Add(new { source, status = "failed", error = Unwrap(e).ToString() }); }
    }

    //沿用正式导出器的对象编号与组件转换，继续收集首个失败之后的独立阻塞点。
    private static void InspectComponents(GameObject prefab, string path)
    {
        Assembly assembly = typeof(PrefabExporter).Assembly;
        Type prefabWriter = assembly.GetType("RimUnity.Editor.PrefabWriter", true);
        Type resourceWriter = assembly.GetType("RimUnity.Editor.ResourceWriter", true);
        Type valueWriter = assembly.GetType("RimUnity.Editor.ValueWriter", true);
        MethodInfo write = assembly.GetType("RimUnity.Editor.ComponentWriter", true).GetMethod("Write", Internal);
        string directory = output + "/diagnostics/" + prefab.name;
        Directory.CreateDirectory(directory);
        object resources = Activator.CreateInstance(resourceWriter, Internal, null,
            new object[] { directory, "diagnostics/" + prefab.name, new Dictionary<string, AssetRecord>(), new ExportOptions() }, null);
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var objects = new Dictionary<Object, string>();
            prefabWriter.GetMethod("Collect", Internal).Invoke(null, new object[] { root.transform, "root", objects });
            object writer = Activator.CreateInstance(valueWriter, Internal, null, new object[] { objects, resources }, null);
            foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
            {
                string location = path + "/" + AnimationUtility.CalculateTransformPath(node, root.transform);
                foreach (Component component in node.GetComponents<Component>())
                {
                    if (component == null) { Issues.Add(new { path = location, type = "MissingScript", error = "源预制体有丢失脚本" }); continue; }
                    if (component is Transform || component is CanvasRenderer || component is UnityEngine.EventSystems.StandaloneInputModule) continue;
                    string type = component.GetType().FullName;
                    Types[type] = Types.TryGetValue(type, out int n) ? n + 1 : 1;
                    try { write.Invoke(null, new[] { (object)component, objects[component], writer, location + "/" + type, node == root.transform }); }
                    catch (Exception e) { Issues.Add(new { path = location, type, error = Unwrap(e).ToString() }); }
                }
            }
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    //移除反射调用包装，保留资源转换器给出的实际原因。
    private static Exception Unwrap(Exception e)
    {
        while (e is TargetInvocationException && e.InnerException != null) e = e.InnerException;
        return e;
    }

    //每个预制体完成后保存进度，意外退出时仍能读取已完成部分。
    private static void Save() => File.WriteAllText(output + "/report.json",
        JsonConvert.SerializeObject(new { output, results = Results, componentTypes = Types, componentIssues = Issues }, Formatting.Indented), new UTF8Encoding(false));
}
