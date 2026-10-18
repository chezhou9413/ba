using RimUnity;
using RimUnity.UI;
using RimUnity.Video;

namespace BANWlLib.mainUI.Initialization;

//沿用 BA 的设计分辨率，将底板和列表扩展到窗口，保留人物、卡片及弹窗的比例。
internal static class BAFullscreenLayout
{
    internal static readonly Vector2 ReferenceSize = new(1920, 1080);

    //只在主界面实例创建时设置锚点，之后由画布布局响应窗口大小变化。
    internal static void ConfigureMain(GameObject root)
    {
        ConfigureCanvas(root);
        foreach (string path in new[] { "dayTime", "nightTime", "Mission", "TotalWar", "shangdian", "cangku", "GaKa" })
            FillPage(Rect(root, path));
        foreach (string path in new[] { "Mission/back", "Mission/back2", "Mission/back4", "Mission/back4/Mask",
            "TotalWar/back", "shangdian/mask", "cangku/cangkulizt", "cangku/Details", "cangku/Details/Background", "GaKa/MainBack" })
            FillPage(Rect(root, path));

        //独立 Spine 背景按覆盖比例放大，前景人物和交互区域不跟着放大。
        root.transform.Find("dayTime/aronaSpineBack").gameObject.AddComponent<BAUIBackgroundCover>().Initialize();
        root.transform.Find("nightTime/aronaSpineBack").gameObject.AddComponent<BAUIBackgroundCover>().Initialize();

        SetRect(Rect(root, "daohang"), new Vector2(0, 1), Vector2.one, new Vector2(0, 74), new Vector2(0, -37));
        Anchor(Rect(root, "fanhui"), new Vector2(0, 1), new Vector2(85.1f, -66.9f));
        foreach (Transform item in root.transform.Find("daohang"))
        {
            if (item is not RectTransform rect) continue;
            Anchor(rect, new Vector2(1, .5f), rect.anchoredPosition - new Vector2(960, 0));
        }
        Anchor(Rect(root, "Buttom"), new Vector2(1, .5f), new Vector2(-960, 0));

        //列表使用真实可见区域，虚拟网格会随宽高增加列数和可见行数。
        //底板采用保留标题和顶部栏的九宫格，页头不会随屏幕高度拉长。
        Rect(root, "cangku/cangkulizt").GetComponent<Image>().type = Image.Type.Sliced;
        RectTransform students = Rect(root, "cangku/cangkulizt/Scroll View");
        SetRect(students, Vector2.zero, Vector2.one, new Vector2(-110, -255), new Vector2(.5f, -125.1f));
        Anchor(Rect(root, "cangku/cangkulizt/typeButtm"), new Vector2(.5f, 1), new Vector2(0, -545.5f));

        //商店列表向右侧和纵向扩展，商品说明与页签分别固定在下方和上方。
        SetRect(Rect(root, "shangdian/ScrollView"), new Vector2(.5f, 0), Vector2.one,
            new Vector2(80, -345.349854f), new Vector2(-69.5f, 4.7f));
        SetRect(Rect(root, "shangdian/dsp"), new Vector2(.5f, 0), new Vector2(1, 0),
            new Vector2(40, 147), new Vector2(-60, 85));
        Fill(Rect(root, "shangdian/dsp/Text"));
        Anchor(Rect(root, "shangdian/xuanze"), new Vector2(.5f, 1), new Vector2(0, -540));
        Anchor(Rect(root, "shangdian/suipianshuliang"), Vector2.one, new Vector2(-180, -125));
        Anchor(Rect(root, "shangdian/showtextsuipian"), Vector2.one, new Vector2(-136.7f, -125));

        SetRect(Rect(root, "GaKa/MainBack/back1"), new Vector2(1, 0), Vector2.one,
            new Vector2(853, 0), new Vector2(-426.5f, 0));
        Anchor(Rect(root, "GaKa/MainBack/box"), new Vector2(1, .5f), new Vector2(-641, -106));
        Anchor(Rect(root, "GaKa/MainBack/cardHead"), Vector2.one, new Vector2(-641.01f, -223));
    }

    //过场背板、视频和闪光覆盖屏幕，不接管纸张的翻转与缩放动画。
    internal static void ConfigureGacha(GameObject root)
    {
        ConfigureCanvas(root);
        root.AddComponent<BAGachaRenderOrder>().Initialize();
        foreach (string path in new[] { "Clip2", "Clip25", "Clip35", "Clip35/Clip45", "Clip55" })
            FillPage(Rect(root, path));
        RectTransform video = Rect(root, "Clip1");
        //源视频为 2400×1080；解码填满纹理，显示时再按源比例铺满画布，避免纹理内先产生黑边。
        video.GetComponent<VideoPlayer>().aspectRatio = VideoAspectRatio.Stretch;
        var fit = video.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectRatio = 2400f / 1080f;
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        Anchor(Rect(root, "Clip55/TryAgainButtom"), new Vector2(.5f, 0), new Vector2(329, 157));
        Anchor(Rect(root, "Clip55/BackButtom"), new Vector2(.5f, 0), new Vector2(-329, 157));
        Anchor(Rect(root, "Clip55/poit"), new Vector2(1, 0), new Vector2(-162.9f, 67.4f));
    }

    //独立提示框保持居中，仅让遮罩覆盖整个游戏视口。
    internal static void ConfigureDialog(GameObject root)
    {
        ConfigureCanvas(root);
        Fill(Rect(root, "back"));
    }

    //购买和兑换弹窗挂在全屏页面下，遮罩跟随父页面伸缩。
    internal static void ConfigureModal(GameObject root) => FillPage((RectTransform)root.transform);

    //Expand 保证设计范围始终完整可见，宽屏或竖屏多出的空间交给拉伸锚点。
    private static void ConfigureCanvas(GameObject root)
    {
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.referenceResolution = ReferenceSize;
    }

    //改变页面容器前固定直接子项的设计矩形，避免半高锚点扩大角色点击区域。
    private static void FillPage(RectTransform page)
    {
        Rect parent = page.rect;
        for (int i = 0; i < page.childCount; i++)
        {
            if (page.GetChild(i) is not RectTransform child) continue;
            Vector2 size = child.rect.size;
            Vector2 anchor = child.anchorMin + Vector2.Scale(child.anchorMax - child.anchorMin, child.pivot);
            Vector2 position = child.anchoredPosition + Vector2.Scale(parent.size, anchor - new Vector2(.5f, .5f));
            SetRect(child, new Vector2(.5f, .5f), new Vector2(.5f, .5f), size, position);
        }
        Fill(page);
    }

    //居中底板精确覆盖父矩形，不改变所在的 Canvas、层级或渲染顺序。
    private static void Fill(RectTransform rect) => SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

    //边缘按钮仅改变参照点，保持控件尺寸和枢轴。
    private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 position)
        => SetRect(rect, anchor, anchor, rect.sizeDelta, position);

    //一次配置由布局系统接管后续缩放，不在逐帧循环里重写所有控件。
    private static void SetRect(RectTransform rect, Vector2 minimum, Vector2 maximum, Vector2 size, Vector2 position)
    {
        rect.anchorMin = minimum;
        rect.anchorMax = maximum;
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
    }

    //这些路径来自 BA 主界面预制体，缺失时直接暴露资源与代码不匹配的问题。
    private static RectTransform Rect(GameObject root, string path) => (RectTransform)root.transform.Find(path);
}
