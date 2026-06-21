using newpro;
using UnityEngine;
using UnityEngine.UI;
using Verse;

public class ButtomEffect : MonoBehaviour
{
    public Button ShangdianButt;

    void Update() // ✅ Unity 正确的生命周期函数（大小写敏感）
    {
        if (!UiMapData.uiclose) return; // 如果 UI 没有打开则不执行
        if (ShangdianButt == null) return;

        int poit = Current.Game.GetComponent<newpro.PoitSaveComponent>().poit;

        if (poit < 200)
        {
            SetButtonGray(ShangdianButt.gameObject, true);  // 整体变灰并禁用
        }
        else
        {
            SetButtonGray(ShangdianButt.gameObject, false); // 恢复并启用
        }
    }

    /// <summary>
    /// 将按钮整体变灰或恢复（图像 + 文字 + 子控件 + 交互）
    /// </summary>
    public static void SetButtonGray(GameObject buttonObj, bool gray)
    {
        Color targetColor = gray ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white;

        // 设置主按钮图像
        Image mainImage = buttonObj.GetComponent<Image>();
        if (mainImage != null)
            mainImage.color = targetColor;

        // 设置是否可交互
        Button btn = buttonObj.GetComponent<Button>();
        if (btn != null)
            btn.interactable = !gray;

        // 设置所有子对象上的 UI 组件颜色（文字、图标等）
        foreach (UnityEngine.UI.Graphic graphic in buttonObj.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
        {
            graphic.color = targetColor;
        }
    }
}
