using UnityEngine;
using UnityEngine.UI;

namespace BANWlLib.mainUI.Images
{
    //让单个 Image 的可见性控制图片引用，滚动出视口或关闭页面即归还资源。
    public sealed class BAUIImageBinding : MonoBehaviour
    {
        private Image target;
        private string path;
        private BAUIImageEntry entry;
        private ScrollRect[] scrolls;
        private CanvasGroup[] groups;
        private Canvas[] canvases;
        private readonly Vector3[] corners = new Vector3[4];
        private float nextCheck;
        private int earliestFrame;
        private bool failed;

        //替换控件的资源路径，延后一帧等待列表布局完成后判断可见性。
        internal void Configure(Image image, string resourcePath)
        {
            if (target == image && path == resourcePath && !failed) return;
            Release();
            target = image;
            target.sprite = null;
            path = resourcePath;
            failed = false;
            RefreshParents();
            earliestFrame = Time.frameCount + 1;
            nextCheck = 0;
        }

        //恢复显示时安排可见性检查，不立即加载尚未完成布局的列表图片。
        private void OnEnable()
        {
            earliestFrame = Time.frameCount + 1;
            nextCheck = 0;
            RefreshParents();
        }

        //随父节点改变更新裁剪和透明度依赖。
        private void OnTransformParentChanged()
        {
            RefreshParents();
            nextCheck = 0;
        }

        //保存可能影响可见性的父控件，避免每次检查重新扫描层级。
        private void RefreshParents()
        {
            scrolls = GetComponentsInParent<ScrollRect>(true);
            groups = GetComponentsInParent<CanvasGroup>(true);
            canvases = GetComponentsInParent<Canvas>(true);
        }

        //使用真实时间检查显示状态，游戏暂停时仍能释放闲置图片。
        private void LateUpdate()
        {
            if (Time.frameCount <= earliestFrame || Time.realtimeSinceStartup < nextCheck) return;
            nextCheck = Time.realtimeSinceStartup + 0.15f;
            bool visible = IsVisible();
            if (!visible) Release();
            else if (entry == null && !failed && !string.IsNullOrWhiteSpace(path))
            {
                entry = BAUIImageCache.Acquire(path);
                failed = entry == null;
                if (entry != null) target.sprite = entry.Sprite;
            }
        }

        //只为启用且与所有滚动视口相交的控件保留图片引用。
        private bool IsVisible()
        {
            if (target == null || !target.isActiveAndEnabled || target.color.a <= 0) return false;
            foreach (Canvas canvas in canvases)
                if (canvas != null && !canvas.enabled) return false;
            foreach (CanvasGroup group in groups)
            {
                if (group == null || !group.isActiveAndEnabled) continue;
                if (group.alpha <= 0) return false;
                if (group.ignoreParentGroups) break;
            }
            target.rectTransform.GetWorldCorners(corners);
            foreach (ScrollRect scroll in scrolls)
            {
                if (scroll == null) continue;
                RectTransform viewport = scroll.viewport != null ? scroll.viewport : scroll.GetComponent<RectTransform>();
                Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
                Vector2 max = new Vector2(float.MinValue, float.MinValue);
                foreach (Vector3 corner in corners)
                {
                    Vector2 point = viewport.InverseTransformPoint(corner);
                    min = Vector2.Min(min, point);
                    max = Vector2.Max(max, point);
                }
                if (!viewport.rect.Overlaps(Rect.MinMaxRect(min.x, min.y, max.x, max.y))) return false;
            }
            return true;
        }

        //控件或页面关闭时归还显示引用。
        private void OnDisable()
        {
            Release();
        }

        //控件销毁时归还引用并解除静态跟踪。
        private void OnDestroy()
        {
            Release();
            BAUIImageCache.Forget(this);
        }

        //清除资源路径，供全局 UI 重建使用。
        internal void Clear()
        {
            Release();
            path = null;
            failed = false;
        }

        //先解除图片控件的对象引用，再允许缓存卸载底层纹理。
        private void Release()
        {
            if (entry == null) return;
            if (target != null && target.sprite == entry.Sprite) target.sprite = null;
            BAUIImageCache.Release(entry);
            entry = null;
        }
    }
}
