using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 负责把当前选中的材质属性整理成 JSON 文本，并在 Unity 编辑器窗口中展示。
/// </summary>
public class MaterialJsonExporter : EditorWindow
{
    private Vector2 scrollPosition;
    private string jsonText = string.Empty;

    /// <summary>
    /// 负责保存材质导出时需要展示的基础信息和属性列表。
    /// </summary>
    [System.Serializable]
    class MatData
    {
        public string materialName;
        public string shaderName;
        public List<string> floats = new List<string>();
        public List<string> colors = new List<string>();
        public List<string> vectors = new List<string>();
        public List<string> textures = new List<string>();
    }

    /// <summary>
    /// 负责从菜单入口读取当前选中的材质，并把导出的 JSON 文本显示到编辑器窗口。
    /// </summary>
    [MenuItem("Tools/Export Material to JSON")]
    static void Export()
    {
        Material mat = Selection.activeObject as Material;
        if (mat == null) { Debug.LogError("请选中一个材质！"); return; }

        Shader shader = mat.shader;
        int count = ShaderUtil.GetPropertyCount(shader);
        MatData data = new MatData
        {
            materialName = mat.name,
            shaderName = shader.name
        };

        for (int i = 0; i < count; i++)
        {
            string name = ShaderUtil.GetPropertyName(shader, i);
            var type = ShaderUtil.GetPropertyType(shader, i);
            switch (type)
            {
                case ShaderUtil.ShaderPropertyType.Float:
                case ShaderUtil.ShaderPropertyType.Range:
                    data.floats.Add($"{name}: {mat.GetFloat(name)}");
                    break;
                case ShaderUtil.ShaderPropertyType.Color:
                    var c = mat.GetColor(name);
                    data.colors.Add($"{name}: ({c.r:F3}, {c.g:F3}, {c.b:F3}, {c.a:F3})");
                    break;
                case ShaderUtil.ShaderPropertyType.Vector:
                    data.vectors.Add($"{name}: {mat.GetVector(name)}");
                    break;
                case ShaderUtil.ShaderPropertyType.TexEnv:
                    var tex = mat.GetTexture(name);
                    data.textures.Add($"{name}: {(tex ? tex.name : "null")}");
                    break;
            }
        }

        string json = JsonUtility.ToJson(data, true);
        EditorGUIUtility.systemCopyBuffer = json;
        ShowJsonWindow(json);
        Debug.Log("材质 JSON 已在编辑器窗口中显示，并已自动复制到剪贴板。");
    }

    /// <summary>
    /// 负责创建或刷新编辑器窗口，让导出的 JSON 文本可以直接查看。
    /// </summary>
    static void ShowJsonWindow(string json)
    {
        MaterialJsonExporter window = GetWindow<MaterialJsonExporter>("Material JSON");
        window.jsonText = json;
        window.minSize = new Vector2(520f, 360f);
        window.Show();
    }

    /// <summary>
    /// 负责绘制 JSON 展示窗口和复制按钮。
    /// </summary>
    void OnGUI()
    {
        EditorGUILayout.LabelField("材质 JSON 输出", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("复制到剪贴板", GUILayout.Width(120f)))
            {
                EditorGUIUtility.systemCopyBuffer = jsonText;
                Debug.Log("材质 JSON 已复制到剪贴板。");
            }

            GUILayout.Label("执行菜单后会自动复制一次，可直接粘贴使用。");
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(jsonText, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }
}
