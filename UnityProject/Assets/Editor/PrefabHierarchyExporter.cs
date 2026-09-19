using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 负责把选中预制体的层级名称导出为文本，便于检查和复制层级结构。
/// </summary>
public class PrefabHierarchyExporter : EditorWindow
{
    private Vector2 scrollPos;
    private string outputText = "";

    /// <summary>
    /// 负责打开预制体层级导出窗口。
    /// </summary>
    [MenuItem("Tools/Prefab Hierarchy Exporter")]
    public static void OpenWindow()
    {
        var win = GetWindow<PrefabHierarchyExporter>("Prefab Hierarchy");
        win.minSize = new Vector2(400, 300);
        win.Show();
    }

    /// <summary>
    /// 负责把 Project 窗口当前选中的预制体层级输出到控制台。
    /// </summary>
    [MenuItem("Assets/Export Prefab Hierarchy", false, 20)]
    public static void ExportToConsole()
    {
        var go = Selection.activeGameObject;
        if (go == null || PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab)
        {
            Debug.LogWarning("[PrefabHierarchy] 请选中一个预制体");
            return;
        }

        string result = BuildHierarchy(go.transform);
        Debug.Log(result);
    }

    /// <summary>
    /// 负责判断右键菜单是否应对当前选择的对象启用。
    /// </summary>
    [MenuItem("Assets/Export Prefab Hierarchy", true)]
    public static bool ExportValidate()
    {
        var go = Selection.activeGameObject;
        return go != null && PrefabUtility.GetPrefabAssetType(go) != PrefabAssetType.NotAPrefab;
    }

    /// <summary>
    /// 负责绘制层级导出窗口和文本预览区域。
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("预制体层级名称导出", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("导出选中预制体", GUILayout.Height(30)))
        {
            var go = Selection.activeGameObject;
            if (go == null || PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab)
            {
                EditorUtility.DisplayDialog("提示", "请先在 Project 窗口选中一个预制体", "OK");
            }
            else
            {
                outputText = BuildHierarchy(go.transform);
                Debug.Log(outputText);
            }
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("清空"))
        {
            outputText = "";
        }

        EditorGUILayout.Space(6);

        if (!string.IsNullOrEmpty(outputText))
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.ExpandHeight(true));
            EditorGUILayout.TextArea(outputText, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// 负责从根节点开始构建完整层级文本。
    /// </summary>
    private static string BuildHierarchy(Transform root)
    {
        var sb = new StringBuilder();
        sb.AppendLine(root.name);
        AppendChildren(root, sb, "");
        return sb.ToString();
    }

    /// <summary>
    /// 负责递归追加子节点名称并生成树状缩进。
    /// </summary>
    private static void AppendChildren(Transform transform, StringBuilder sb, string prefix)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            bool isLast = i == transform.childCount - 1;
            Transform child = transform.GetChild(i);

            sb.AppendLine($"{prefix}{(isLast ? "└── " : "├── ")}{child.name}");
            AppendChildren(child, sb, prefix + (isLast ? "    " : "│   "));
        }
    }
}