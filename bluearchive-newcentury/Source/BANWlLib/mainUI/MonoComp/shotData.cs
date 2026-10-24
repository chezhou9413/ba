#if RW_1_7
using Engine = RimUnity;
using AssetBundle = BANWlLib.RimUnityMigration.BAUiAssets;
#endif
using BANWlLib.mainUI.pojo;
using MyCoolMusicMod;
using newpro;
using System.IO;
#if RW_1_7
using RimUnity;
#else
using Engine = UnityEngine;
using UnityEngine;
#endif
#if RW_1_7
using RimUnity.UI;
#else
using UnityEngine.UI;
#endif
using Verse;
using System.Text.RegularExpressions;
using BANWlLib.mainUI.MonoComp;

//商品条目组件把商店配置绑定到单个商品预制体，并处理显示、购买按钮和刷新状态。
public class shotData : MonoBehaviour
{
    public shot shot;
    public string shoptype;
    public GameObject goumaiback;
    public Button gounauvbutton;

    [Header("Title Auto Fit")]
    public int baseFontSize = 35;          // 标题常规字号
    public int minFontSize = 16;           // 标题缩放下限
    public int chineseThreshold = 6;       // 中文标题开始缩放的字符阈值
    public int latinThreshold = 10;        // 拉丁标题开始缩放的字符阈值
    public float extraPadding = 4f;        // 文本容器保留的像素边距
    private Engine.UI.Text titleText; // 标题文本缓存
    private string lastTitle;              // 上次计算过的标题内容

    //销毁当前商品条目对象，用于商店列表重建时清理旧 UI。
    public void delect()
    {
        Destroy(this.gameObject);
    }

    //Unity 生命周期入口，在商品数据已经绑定后初始化显示内容和按钮事件。
    void Start()
    {
        if (shot == null)
        {
            Log.Error("[shotData] 商品数据未绑定，跳过商品条目初始化。");
            return;
        }

        goumaiback = this.transform.Find("lock").gameObject;
        gounauvbutton = this.transform.Find("goumai").GetComponent<Button>();

        titleText = this.transform.Find("title").GetComponent<Engine.UI.Text>();
        titleText.text = shot.ProductName;
        titleText.fontSize = baseFontSize;

        ApplyTitleAutoFit();

        this.transform.Find("cont").GetComponent<Engine.UI.Text>().text = "x" + shot.ProductAmount;
        this.transform.Find("pingzhi/" + shot.ProductQuality).gameObject.SetActive(true);
        string bodytitlepath = UiMapData.modRootPath + "/Common/Textures/" + shot.ProductImage + ".png";
        imgcvT2d.SetImage(this.transform.Find("bodytitle").GetComponent<Engine.UI.Image>(), bodytitlepath);
        string shotimagpath = UiMapData.modRootPath + "/Common/Textures/" + shot.CurrencyImage + ".png";
        imgcvT2d.SetImage(this.transform.Find("goumai/jiageback/shotimag").GetComponent<Engine.UI.Image>(), shotimagpath);
        this.transform.Find("goumai/jiageback/JIAGESHOW").GetComponent<Engine.UI.Text>().text = shot.CurrencyAmount.ToString();

        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            UiMapData.dsptext.text = shot.ProductDescription;
        });

        gounauvbutton.onClick.AddListener(() =>
        {
            GameObject maskgoumai = GameObject.Instantiate(UiMapData.goumaiMack,this.transform.parent.parent.parent.parent.parent);
#if RW_1_7
            BANWlLib.mainUI.Initialization.BAFullscreenLayout.ConfigureModal(maskgoumai);
#endif
            pilianggoumai pilianggoumai = maskgoumai.AddComponent<pilianggoumai>();
            pilianggoumai.shotData = this;
        });

        setLockButtton();

        // 验证粒子预制体是否正确加载
        ValidateParticlePrefab();
    }

    //Unity 尺寸变化回调，在布局尺寸改变后重新计算标题字号。
    void OnRectTransformDimensionsChange()
    {
        if (titleText != null && gameObject.activeInHierarchy)
        {
            ApplyTitleAutoFit();
        }
    }

    //刷新标题文本，并立即应用字号适配。
    public void RefreshTitle(string newTitle)
    {
        if (titleText == null)
            titleText = this.transform.Find("title").GetComponent<Engine.UI.Text>();

        titleText.text = newTitle;
        ApplyTitleAutoFit(true);
    }

    //自适应标题字号（阈值 + 容器宽度双重约束）
    private void ApplyTitleAutoFit(bool force = false)
    {
        if (titleText == null) return;

        string content = titleText.text ?? "";
        if (!force && content == lastTitle) return;
        lastTitle = content;

        bool mostlyLatin = Regex.IsMatch(content, @"^[\u0000-\u00FF]+$"); // 基础 ASCII 按拉丁文本处理
        int threshold = mostlyLatin ? latinThreshold : chineseThreshold;

        int targetFontSize = baseFontSize;
        if (content.Length > threshold)
        {
            float scale = (float)threshold / Mathf.Max(1, content.Length);
            targetFontSize = Mathf.RoundToInt(baseFontSize * Mathf.Clamp01(scale));
            targetFontSize = Mathf.Max(targetFontSize, minFontSize);
        }

        var rt = titleText.transform as RectTransform;
        float containerWidth = rt != null ? rt.rect.width : 0f;
        if (containerWidth > 0f)
        {
#if RW_1_7
            int previousSize = titleText.fontSize;
            titleText.fontSize = targetFontSize;
            float preferredWidth = titleText.preferredWidth;
            titleText.fontSize = previousSize;
#else
            var settings = titleText.GetGenerationSettings(Vector2.zero);
            settings.scaleFactor = 1f;

            settings.fontSize = targetFontSize;
            float preferredWidth = titleText.cachedTextGeneratorForLayout.GetPreferredWidth(content, settings) / titleText.pixelsPerUnit;
#endif

            float maxWidth = Mathf.Max(0f, containerWidth - extraPadding);
            if (preferredWidth > maxWidth && preferredWidth > 0.01f)
            {
                float widthScale = maxWidth / preferredWidth;
                int widthFitSize = Mathf.FloorToInt(targetFontSize * widthScale);
                targetFontSize = Mathf.Max(Mathf.Min(targetFontSize, widthFitSize), minFontSize);
            }
        }

        titleText.resizeTextForBestFit = false; //字号由标题布局计算。
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow; // 防止 Unity 因换行影响宽度计算
        titleText.verticalOverflow = VerticalWrapMode.Truncate;
        titleText.fontSize = targetFontSize;
    }

    //验证粒子预制体是否正确加载
    private void ValidateParticlePrefab()
    {
        if (UiMapData.buyParticle == null)
        {
            return;
        }


        var go = UiMapData.buyParticle as GameObject;
        if (go != null)
        {
            var ps = go.GetComponent<ParticleSystem>();
            if (ps != null)
            {
            }
            else
            {
                var childPs = go.GetComponentInChildren<ParticleSystem>();
                if (childPs != null)
                {
                }
                else
                {
                }
            }
        }
        else
        {
        }
    }

    //在购买按钮位置播放购买粒子效果。
    public void SpawnParticleAtButton(RectTransform btnRect)
    {

        if (UiMapData.buyParticle == null)
        {
            return;
        }

        var canvas = btnRect.GetComponentInParent<Canvas>();
        if (!canvas)
        {
            return;
        }

        var cam = canvas.worldCamera;
        if (cam == null)
        {
            cam = Camera.main;
        }
        if (cam == null)
        {
            return;
        }

        Vector3[] corners = new Vector3[4];
        btnRect.GetWorldCorners(corners);
        Vector3 buttonCenter = (corners[0] + corners[2]) / 2f;

        Vector3 screenPos = cam.WorldToScreenPoint(buttonCenter);

        Vector3 worldPos;
        RectTransform canvasRect = canvas.transform as RectTransform;

        if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect, screenPos, cam, out worldPos))
        {
            worldPos = buttonCenter;
        }

        GameObject instGo = null;
        ParticleSystem particleSystem = null;

        try
        {
            var goPrefab = UiMapData.buyParticle as GameObject;
            if (goPrefab != null)
            {
                instGo = Object.Instantiate(goPrefab, worldPos, Quaternion.identity, canvas.transform);
#if RW_1_7
                BANWlLib.mainUI.Initialization.BAUiParticles.Configure(instGo);
#endif
                particleSystem = instGo.GetComponent<ParticleSystem>();
                if (particleSystem == null)
                {
                    particleSystem = instGo.GetComponentInChildren<ParticleSystem>();
                }

                if (particleSystem != null)
                {
                }
                else
                {
                    Object.Destroy(instGo);
                    return;
                }
            }
            else
            {
                return;
            }

            if (particleSystem != null && instGo != null)
            {
                instGo.SetActive(true);
                particleSystem.gameObject.SetActive(true);
                particleSystem.Clear(true);
                particleSystem.Play(true);

                ApplySortingToParticles(instGo, canvas, 100);

                var main = particleSystem.main;
                float duration = main.duration;
                float startLifetime = main.startLifetime.constantMax;
                float destroyTime = duration + startLifetime + 0.5f;


                if (particleSystem.isPlaying)
                {
                }
                else
                {
                }

                int particleCount = particleSystem.particleCount;

                Object.Destroy(instGo, destroyTime);
            }
        }
        catch (System.Exception ex)
        {
            Log.Error($"SpawnParticleAtButton 发生错误: {ex.Message}\n{ex.StackTrace}");
            if (instGo != null)
            {
                Object.Destroy(instGo);
            }
        }
    }

    //把粒子渲染排序同步到当前 Canvas，确保粒子显示在购买界面上方。
    private void ApplySortingToParticles(GameObject root, Canvas canvas, int orderOffset)
    {
#if RW_1_7
        //相机 UI 的粒子与控件共同按层级提交，最后一个同级节点位于购买按钮上方。
        root.transform.SetAsLastSibling();
#else
        try
        {
            int layerID = canvas.sortingLayerID;
            int order = canvas.sortingOrder + orderOffset;

            var renderers = root.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers)
            {
                if (r != null)
                {
                    r.sortingLayerID = layerID;
                    r.sortingOrder = order;
                }
            }

            var particleRenderers = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
            foreach (var pr in particleRenderers)
            {
                if (pr != null)
                {
                    pr.sortingLayerID = layerID;
                    pr.sortingOrder = order;
                }
            }

        }
        catch (System.Exception ex)
        {
        }
#endif
    }

    //根据玩家持有货币数量刷新购买按钮可用状态。
    private void setLockButtton()
    {
        if (shot == null || gounauvbutton == null || goumaiback == null)
        {
            return;
        }

        if (ItemUtility.GetTotalItemCount(shot.CurrencyDefName) >= shot.CurrencyAmount)
        {
            gounauvbutton.interactable = true;
            goumaiback.SetActive(false);
            this.gameObject.transform.SetAsFirstSibling();
        }
        else
        {
            gounauvbutton.interactable = false;
            goumaiback.SetActive(true);
        }
    }

    //Unity 销毁回调，解除商店刷新事件订阅。
    void OnDestroy()
    {
        ShopEvents.OnRefreshAllButtons -= setLockButtton;
    }

    //Unity 启用回调，订阅商店刷新事件并在数据可用时刷新排序。
    void OnEnable()
    {
        ShopEvents.OnRefreshAllButtons += setLockButtton;
        if (shot != null && ItemUtility.GetTotalItemCount(shot.CurrencyDefName) >= shot.CurrencyAmount)
        {
            this.gameObject.transform.SetAsFirstSibling();
        }
    }

    //Unity 禁用回调，解除商店刷新事件订阅。
    void OnDisable()
    {
        ShopEvents.OnRefreshAllButtons -= setLockButtton;
    }
}
