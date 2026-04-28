using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib
{
    public class CompProperties_PawnProgressBarUI : CompProperties
    {
        // 星星的间隔比例
        public float starInterval = 1f;

        // 是否显示属性值进度条
        public bool showProgressBar = true;

        // 星星绘制的进度比例
        public List<float> starProgressRatios = new List<float> { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

        // 星星的尺寸
        public float starSize = 20f;

        // UI显示的种族，可以配置多个，留空为全部都显示
        public List<string> allowedRaces = new List<string>();

        // 星星的贴图路径
        public string starTexturePath = "UI/StarIcon";
        
        /// <summary>
        /// 构造函数
        /// 告诉游戏这个配置对应哪个组件类
        /// </summary>
        public CompProperties_PawnProgressBarUI()
        {
            this.compClass = typeof(CompUseEffect_PawnProgressBarUI);
        }
    }

    public class CompUseEffect_PawnProgressBarUI : ThingComp
    {
        /// <summary>
        /// 默认构造函数
        /// 环世界要求所有ThingComp都必须有默认构造函数
        /// </summary>
        public CompUseEffect_PawnProgressBarUI()
        {
            // 默认构造函数，无需特殊处理
        }
        
        /// <summary>
        /// 获取组件配置属性
        /// 这些配置来自XML文件中的设置
        /// </summary>
        public CompProperties_PawnProgressBarUI Props => (CompProperties_PawnProgressBarUI)this.props;

        public bool CheckRaceDefName(Pawn pawn)
        {
            try
            {
                if (Props.allowedRaces.Count < 1)
                {
                    return true; // 如果没有配置种族，则默认允许所有
                }
                if (Props.allowedRaces.Contains(pawn.def?.defName))
                {
                    return true; // 如果当前小人的种族在允许列表中
                }
                return false; // 如果不在允许列表中
            }
            catch
            {
                return false; // 如果没有配置种族，则默认不允许
            }
        }
    }

    /// <summary>
    /// 小人进度条UI类 - 负责在信息面板上绘制进度条
    /// </summary>
    public static class PawnProgressBarUI
    {
        // 进度条的样式配置
        private static readonly Color ProgressBarBgColor = Color.white;        // 背景色：白色
        private static readonly Color ProgressBarFillColor = Color.green;      // 填充色：绿色
        public const float ProgressBarHeight = 6f;                           // 进度条高度
        public const float ProgressBarWidth = 200f;                           // 进度条宽度
        
        // 性能优化：缓存进度计算结果
        public static Dictionary<Pawn, float> progressCache = new Dictionary<Pawn, float>();
        private static int lastCacheUpdateTick = 0;

        /// <summary>
        /// 绘制进度条
        /// </summary>
        /// <param name="showProgressBar">是否显示进度条</param>
        /// <param name="rect">绘制区域</param>
        /// <param name="progress">进度值(0.0-1.0)</param>
        /// <param name="starProgressRatios">星星显示的进度比例列表</param>
        /// <param name="starSize">星星尺寸</param>
        /// <param name="starTexturePath">星星贴图路径</param>
        /// <param name="label">标签文本</param>
        public static void DrawProgressBar(bool showProgressBar, Rect rect, float progress, List<float> starProgressRatios, float starSize, string starTexturePath, string label = "进度")
        {
            try
            {
                // 确保进度值在0-1之间
                progress = Mathf.Clamp01(progress);

                // 保存当前GUI状态
                Color originalColor = GUI.color;
                TextAnchor originalAnchor = Text.Anchor;

                // 只在showProgressBar为true时绘制进度条
                if (showProgressBar)
                {
                    // 绘制背景
                    GUI.color = ProgressBarBgColor;
                    GUI.DrawTexture(rect, BaseContent.WhiteTex);

                    // 绘制边框
                    GUI.color = Color.black;
                    Widgets.DrawBox(rect, 1);

                    // 绘制进度填充
                    if (progress > 0f)
                    {
                        Rect fillRect = new Rect(rect.x + 1, rect.y + 1,
                            (rect.width - 2) * progress, rect.height - 2);
                        GUI.color = ProgressBarFillColor;
                        GUI.DrawTexture(fillRect, BaseContent.WhiteTex);
                    }
                }

                // 始终绘制星星标记（无论是否显示进度条）
                DrawStarMarkers(starProgressRatios, starSize, rect, progress, starTexturePath);

                // 恢复GUI状态
                GUI.color = originalColor;
                Text.Anchor = originalAnchor;
            }
            catch (Exception ex)
            {
                // 确保GUI状态被恢复
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        /// <summary>
        /// 在进度条上绘制星星标记
        /// </summary>
        /// <param name="starProgressRatios">星星显示的进度比例列表</param>
        /// <param name="starSize">星星尺寸</param>
        /// <param name="progressRect">进度条区域</param>
        /// <param name="currentProgress">当前进度值</param>
        /// <param name="starTexturePath">星星贴图路径</param>
        private static void DrawStarMarkers(List<float> starProgressRatios, float starSize, Rect progressRect, float currentProgress, string starTexturePath)
        {
            try
            {
                // 尝试加载星星贴图
                Texture2D starTexture = ContentFinder<Texture2D>.Get(starTexturePath, false);

                // 如果找不到自定义星星图片，使用游戏内置的图标作为备用
                if (starTexture == null)
                {
                    // 使用RimWorld内置的图标作为备用
                    starTexture = TexUI.GrayTextBG; // 临时使用，你可以替换为其他内置图标
                }

                if (starTexture == null) return; // 如果还是没有贴图就不绘制

                // 在进度条中平均分配星星位置
                int starCount = starProgressRatios.Count;
                if (starCount <= 0) return; // 如果没有星星就不绘制

                for (int i = 0; i < starCount; i++)
                {
                    // 计算星星的平均分配位置
                    // 在进度条中平均分配，第一个星星在1/(starCount+1)位置，最后一个在starCount/(starCount+1)位置
                    float starPosRatio = (float)(i + 1) / (starCount + 1);
                    
                    // 计算星星的X位置（在进度条中平均分配）
                    float starX = progressRect.x + (progressRect.width - 2) * starPosRatio - starSize / 2f;

                    // 星星的Y位置（在进度条上方）
                    float starY = progressRect.y - 20f;

                    // 确保星星不超出面板上边界
                    if (starY < 0) starY = progressRect.y + progressRect.height + 2.5f; // 如果超出上边界，放到进度条下方

                    // 创建星星的绘制区域
                    Rect starRect = new Rect(starX, starY, starSize, starSize);

                    // 根据当前进度决定星星的颜色和透明度
                    // 使用原来的进度比例来判断星星是否点亮
                    Color starColor = Color.white;
                    if (currentProgress >= starProgressRatios[i])
                    {
                        // 已达到的里程碑 - 金色星星
                        starColor = Color.yellow;
                    }
                    else
                    {
                        // 未达到的里程碑 - 灰色半透明星星
                        starColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
                    }

                    // 绘制星星
                    GUI.color = starColor;
                    GUI.DrawTexture(starRect, starTexture);
                }
            }
            catch (Exception ex)
            {
                // 静默处理错误，避免影响游戏运行
            }
        }

        /// <summary>
        /// 获取小人的示例进度值
        /// 这里可以根据实际需求修改为任何你想要显示的数值
        /// 性能优化：使用缓存机制，避免重复计算
        /// </summary>
        /// <param name="pawn">小人对象</param>
        /// <returns>进度值(0.0-1.0)</returns>
        public static float GetPawnProgress(Pawn pawn)
        {
            try
            {
                if (pawn == null) return 0f;

                // 性能优化：每60tick更新一次缓存
                int currentTick = Find.TickManager.TicksGame;
                if (currentTick - lastCacheUpdateTick >= 60)
                {
                    progressCache.Clear();
                    lastCacheUpdateTick = currentTick;
                }
                
                // 检查缓存
                if (progressCache.TryGetValue(pawn, out float cachedProgress))
                {
                    return cachedProgress;
                }

                // 计算进度值
                float progress = 0f;
                if (HumanIntPropertyHelper.HasCustomIntProperty(pawn))
                {
                    var comp = HumanIntPropertyHelper.GetCustomIntPropertyComp(pawn);
                    if (comp != null)
                    {
                        int currentValue = comp.CustomIntValue;
                        int maxValue = comp.Props.maxValue;
                        if (maxValue > 0)
                        {
                            progress = (float)currentValue / maxValue;
                        }
                    }
                }
                
                // 缓存结果
                progressCache[pawn] = progress;
                return progress;
            }
            catch (Exception ex)
            {
                return 0f;
            }
        }
    }

    /// <summary>
    /// Harmony补丁类 - 修改小人信息面板以添加进度条
    /// 使用硬编码方式，避免反射问题
    /// </summary>
    [HarmonyPatch(typeof(ITab_Pawn_Character), "FillTab")]
    public static class ITab_Pawn_Character_FillTab_Patch
    {
        /// <summary>
        /// 在小人角色信息面板中添加进度条
        /// </summary>
        /// <param name="__instance">ITab_Pawn_Character实例</param>
        static void Postfix(ITab_Pawn_Character __instance)
        {
            Pawn selectpawn = Find.Selector.SingleSelectedThing as Pawn;
            CompUseEffect_PawnProgressBarUI PawnProgressBarUIComp = selectpawn.GetComp<CompUseEffect_PawnProgressBarUI>();
            if (PawnProgressBarUIComp == null)
            {
                return; // 如果没有组件就不执行效果
            }
            if (!PawnProgressBarUIComp.CheckRaceDefName(selectpawn))
            {
                return;
            }
            // 硬编码tab尺寸 - 使用ITab_Pawn_Character的标准尺寸
            Vector2 tabSize = new Vector2(432f, 480f); // RimWorld标准角色面板尺寸

            // 计算进度条位置
            // 在信息面板的底部添加进度条，为星星标记留出额外空间
            Rect progressRect = new Rect(
                40f,                                    // X坐标：左边距
                tabSize.y - 300f,                       // Y坐标：底部位置（增加空间给星星）
                Mathf.Min(PawnProgressBarUI.ProgressBarWidth * PawnProgressBarUIComp.Props.starInterval, tabSize.x - 20f), // 宽度（不超过面板宽度）
                PawnProgressBarUI.ProgressBarHeight    // 高度
            );

            // 确保进度条在有效区域内
            if (progressRect.y < 0 || progressRect.width <= 0 || progressRect.height <= 0)
            {
                return;
            }

            // 获取进度值
            float progress = PawnProgressBarUI.GetPawnProgress(selectpawn);

            // 根据是否有自定义属性决定标签文本
            string label = "状态";
            if (HumanIntPropertyHelper.HasCustomIntProperty(selectpawn))
            {
                label = "经验值";
            }

            // 绘制进度条
            PawnProgressBarUI.DrawProgressBar(PawnProgressBarUIComp.Props.showProgressBar, progressRect, progress, PawnProgressBarUIComp.Props.starProgressRatios, PawnProgressBarUIComp.Props.starSize, PawnProgressBarUIComp.Props.starTexturePath, label);
        }
    }
}
