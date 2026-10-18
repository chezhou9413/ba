using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib
{
    //在角色信息面板绘制成长进度条。
    public static class PawnProgressBarUI
    {
        // 进度条的样式配置
        private static readonly Color ProgressBarBgColor = Color.white;        // 背景色：白色
        private static readonly Color ProgressBarFillColor = Color.green;      // 填充色：绿色
        public const float ProgressBarHeight = 6f;                           // 进度条高度
        public const float ProgressBarWidth = 200f;                           // 进度条宽度
        
        //缓存进度计算结果。
        public static Dictionary<Pawn, float> progressCache = new Dictionary<Pawn, float>();
        private static int lastCacheUpdateTick = 0;

        //绘制进度条，progress 的范围为 0～1。
        public static void DrawProgressBar(bool showProgressBar, Rect rect, float progress, List<float> starProgressRatios, float starSize, string starTexturePath, string label = "进度")
        {
            try
            {
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
                //恢复 GUI 状态。
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }

        //按进度比例在进度条上绘制星星。
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
                    starTexture = TexUI.GrayTextBG; //缺少星星贴图时使用灰色背景。
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

                    //星星位置不超过面板上边界。
                    if (starY < 0) starY = progressRect.y + progressRect.height + 2.5f; // 如果超出上边界，放到进度条下方

                    // 创建星星的绘制区域
                    Rect starRect = new Rect(starX, starY, starSize, starSize);

                    // 根据当前进度决定星星是否点亮，角色面板星星表示阶级进度，不表示抽卡稀有度。
                    Color starColor = Color.white;
                    bool starActive = currentProgress >= starProgressRatios[i];
                    if (starActive)
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

        //获取缓存的成长进度，返回值为 0～1。
        public static float GetPawnProgress(Pawn pawn)
        {
            try
            {
                if (pawn == null) return 0f;

                //每 60 tick 更新一次缓存。
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

    //向角色信息面板添加成长进度条。
    [HarmonyPatch(typeof(ITab_Pawn_Character), "FillTab")]
    public static class ITab_Pawn_Character_FillTab_Patch
    {
        //绘制角色成长进度。
        static void Postfix(ITab_Pawn_Character __instance)
        {
            Pawn selectpawn = Find.Selector.SingleSelectedThing as Pawn;
            if (selectpawn == null)
            {
                return;
            }

            PawnProgressBarKindExtension starConfig = selectpawn.kindDef?.GetModExtension<PawnProgressBarKindExtension>();
            if (starConfig == null)
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
                Mathf.Min(PawnProgressBarUI.ProgressBarWidth * starConfig.starInterval, tabSize.x - 20f), // 宽度（不超过面板宽度）
                PawnProgressBarUI.ProgressBarHeight    // 高度
            );

            //限制进度条的绘制区域。
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
            PawnProgressBarUI.DrawProgressBar(starConfig.showProgressBar, progressRect, progress, starConfig.starProgressRatios, starConfig.starSize, starConfig.starTexturePath, label);
        }
    }
}
