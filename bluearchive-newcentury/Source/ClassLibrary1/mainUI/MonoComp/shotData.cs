using BANWlLib.mainUI.pojo;
using MyCoolMusicMod;
using newpro;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using System.Text.RegularExpressions;
using BANWlLib.mainUI.MonoComp;

public class shotData : MonoBehaviour
{
    public shot shot;
    public string shoptype;
    public GameObject goumaiback;
    public Button gounauvbutton;

    // ====== [新增配置] 自适应标题参数 ======
    [Header("Title Auto Fit")]
    public int baseFontSize = 35;          // 基础字号（≤阈值时用）
    public int minFontSize = 16;           // 缩小时的下限，避免太小
    public int chineseThreshold = 6;       // 超过6个汉字开始缩
    public int latinThreshold = 10;        // 超过10个字母/数字开始缩
    public float extraPadding = 4f;        // 给文本留一点边距（像素）
    private UnityEngine.UI.Text titleText;                // 缓存 title Text
    private string lastTitle;              // 避免重复计算

    public void delect()
    {
        Destroy(this.gameObject);
    }
    void Start()
    {
        goumaiback = this.transform.Find("lock").gameObject;
        gounauvbutton = this.transform.Find("goumai").GetComponent<Button>();

        // ====== [修改处] 缓存标题组件并设置文本 ======
        titleText = this.transform.Find("title").GetComponent<UnityEngine.UI.Text>();
        titleText.text = shot.ProductName;
        titleText.fontSize = baseFontSize;

        // ====== [新增] 首次计算标题自适应 ======
        ApplyTitleAutoFit();

        this.transform.Find("cont").GetComponent<UnityEngine.UI.Text>().text = "x" + shot.ProductAmount;
        this.transform.Find("pingzhi/" + shot.ProductQuality).gameObject.SetActive(true);
        string bodytitlepath = UiMapData.modRootPath + "/Common/Textures/" + shot.ProductImage + ".png";
        this.transform.Find("bodytitle").GetComponent<UnityEngine.UI.Image>().sprite = imgcvT2d.LoadSpriteFromFile(bodytitlepath);
        string shotimagpath = UiMapData.modRootPath + "/Common/Textures/" + shot.CurrencyImage + ".png";
        this.transform.Find("goumai/jiageback/shotimag").GetComponent<UnityEngine.UI.Image>().sprite = imgcvT2d.LoadSpriteFromFile(shotimagpath);
        this.transform.Find("goumai/jiageback/JIAGESHOW").GetComponent<UnityEngine.UI.Text>().text = shot.CurrencyAmount.ToString();

        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            UiMapData.dsptext.text = shot.ProductDescription;
        });

        gounauvbutton.onClick.AddListener(() =>
        {
            GameObject maskgoumai = GameObject.Instantiate(UiMapData.goumaiMack,this.transform.parent.parent.parent.parent.parent);
            pilianggoumai pilianggoumai = maskgoumai.AddComponent<pilianggoumai>();
            pilianggoumai.shotData = this;
        });

        setLockButtton();

        // 验证粒子预制体是否正确加载
        ValidateParticlePrefab();
    }

    // ====== [新增] 当 RectTransform 大小变化（比如分辨率/布局变了）时，重新适配 ======
    void OnRectTransformDimensionsChange()
    {
        // titleText 可能还没初始化
        if (titleText != null && gameObject.activeInHierarchy)
        {
            ApplyTitleAutoFit();
        }
    }

    // ====== [新增] 外部如果修改了 shot.ProductName，可调用这个接口刷新标题并自适应 ======
    public void RefreshTitle(string newTitle)
    {
        if (titleText == null)
            titleText = this.transform.Find("title").GetComponent<UnityEngine.UI.Text>();

        titleText.text = newTitle;
        ApplyTitleAutoFit(true);
    }

    /// <summary>
    /// 自适应标题字号（阈值 + 容器宽度双重约束）
    /// </summary>
    private void ApplyTitleAutoFit(bool force = false)
    {
        if (titleText == null) return;

        string content = titleText.text ?? "";
        if (!force && content == lastTitle) return;
        lastTitle = content;

        // 1) 按内容类型决定是否需要缩放（> 阈值则缩）
        bool mostlyLatin = Regex.IsMatch(content, @"^[\u0000-\u00FF]+$"); // 基础ASCII视为拉丁
        int threshold = mostlyLatin ? latinThreshold : chineseThreshold;

        int targetFontSize = baseFontSize;
        if (content.Length > threshold)
        {
            float scale = (float)threshold / Mathf.Max(1, content.Length);
            targetFontSize = Mathf.RoundToInt(baseFontSize * Mathf.Clamp01(scale));
            targetFontSize = Mathf.Max(targetFontSize, minFontSize);
        }

        // 2) 进一步用真实渲染宽度约束，确保不超出容器
        var rt = titleText.transform as RectTransform;
        float containerWidth = rt != null ? rt.rect.width : 0f;
        if (containerWidth > 0f)
        {
            // 用 TextGenerator 计算期望宽度
            var settings = titleText.GetGenerationSettings(Vector2.zero);
            settings.scaleFactor = 1f;

            // 先用上一步算出的 targetFontSize 试算
            settings.fontSize = targetFontSize;
            float preferredWidth = titleText.cachedTextGeneratorForLayout.GetPreferredWidth(content, settings) / titleText.pixelsPerUnit;

            // 如果依旧超过容器，则继续按比例收缩
            float maxWidth = Mathf.Max(0f, containerWidth - extraPadding);
            if (preferredWidth > maxWidth && preferredWidth > 0.01f)
            {
                float widthScale = maxWidth / preferredWidth;
                int widthFitSize = Mathf.FloorToInt(targetFontSize * widthScale);
                targetFontSize = Mathf.Max(Mathf.Min(targetFontSize, widthFitSize), minFontSize);
            }
        }

        titleText.resizeTextForBestFit = false; // 关闭内置BestFit，使用我们控制
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow; // 防止 Unity 因换行影响宽度计算
        titleText.verticalOverflow = VerticalWrapMode.Truncate;
        titleText.fontSize = targetFontSize;
        // 可选：如需要两端留白可增加字间距（需 Text 支持/或改用 TMP 实现）
    }

    /// <summary>
    /// 验证粒子预制体是否正确加载
    /// </summary>
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

    private void ApplySortingToParticles(GameObject root, Canvas canvas, int orderOffset)
    {
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
    }

    private void setLockButtton()
    {
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

    void OnDestroy()
    {
        ShopEvents.OnRefreshAllButtons -= setLockButtton;
    }

    void OnEnable()
    {
        ShopEvents.OnRefreshAllButtons += setLockButtton;
        if(ItemUtility.GetTotalItemCount(shot.CurrencyDefName) >= shot.CurrencyAmount)
        {
            this.gameObject.transform.SetAsFirstSibling();
        }
    }

    void OnDisable()
    {
        ShopEvents.OnRefreshAllButtons -= setLockButtton;
    }
}
