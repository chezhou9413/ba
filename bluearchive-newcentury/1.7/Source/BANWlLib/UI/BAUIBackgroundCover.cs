using RimUnity;

namespace BANWlLib.mainUI.Initialization;

//只调整独立 Spine 背景的覆盖比例，不改变前景人物和互动按钮。
internal sealed class BAUIBackgroundCover : MonoBehaviour
{
    private RectTransform rect;
    private Vector3 originalScale;
    private Vector2 originalPosition, lastScreen;

    //保存作者变换作为绝对基线，窗口反复缩放时不会累乘或漂移。
    internal void Initialize()
    {
        rect = (RectTransform)transform;
        originalScale = rect.localScale;
        originalPosition = rect.anchoredPosition;
        Refresh();
    }

    //仅屏幕尺寸变化时写入变换，普通帧不引发布局重建。
    private void LateUpdate() => Refresh();

    //使用与 BA Expand 画布相同的逻辑尺寸，不重复叠加游戏 UI 缩放。
    private void Refresh()
    {
        Vector2 screen = new(Screen.width, Screen.height);
        if (screen == lastScreen || screen.x <= 0 || screen.y <= 0) return;
        lastScreen = screen;
        Vector2 reference = BAFullscreenLayout.ReferenceSize;
        float x = screen.x / reference.x, y = screen.y / reference.y;
        float cover = Mathf.Max(x, y) / Mathf.Min(x, y);
        rect.localScale = originalScale * cover;
        rect.anchoredPosition = originalPosition * cover;
    }
}
