using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // SandWormHudOverlay 负责在地图界面绘制大沙虫 Boss 血条和小沙虫紧凑战术血条。
    public sealed class SandWormHudOverlay : GameComponent
    {
        private const string BossWormDefName = "SandWorm_Thing";
        private const string SmallWormDefName = "SandWorm_SmallThing";
        private const float BossPanelWidth = 420f;
        private const float BossPanelHeight = 54f;
        private const float BossPanelTopY = 80f;
        private const float BossBarHeight = 16f;
        private const float SmallPanelWidth = 260f;
        private const float SmallPanelHeight = 34f;
        private const float SmallPanelTopY = 92f;
        private const float SmallPanelRightMargin = 28f;
        private const float SmallPanelGap = 5f;
        private const float SmallBarHeight = 8f;
        private const int MaxVisibleSmallWormBars = 8;

        private readonly List<SandWormThing> bossWorms = new List<SandWormThing>();
        private readonly List<SandWormThing> smallWorms = new List<SandWormThing>();

        // SandWormHudOverlay 负责让 RimWorld 在创建或读取存档时实例化 HUD 组件。
        public SandWormHudOverlay(Game game)
        {
        }

        // GameComponentOnGUI 负责在重绘阶段收集当前地图沙虫并绘制对应血条。
        public override void GameComponentOnGUI()
        {
            if (Event.current.type != EventType.Repaint || Current.ProgramState != ProgramState.Playing)
            {
                return;
            }

            Map map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            CollectWorms(map);
            if (bossWorms.Count == 0 && smallWorms.Count == 0)
            {
                return;
            }

            Color oldColor = GUI.color;
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;

            try
            {
                DrawBossBars();
                DrawSmallBars();
            }
            finally
            {
                GUI.color = oldColor;
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
            }
        }

        // CollectWorms 负责从当前地图分别收集大沙虫和小沙虫，过滤死亡下沉或隐藏代理体。
        private void CollectWorms(Map map)
        {
            bossWorms.Clear();
            smallWorms.Clear();
            CollectWormsOfDef(map, BossWormDefName, bossWorms);
            CollectWormsOfDef(map, SmallWormDefName, smallWorms);
        }

        // CollectWormsOfDef 负责按 ThingDef 名称收集有效沙虫实例。
        private static void CollectWormsOfDef(Map map, string defName, List<SandWormThing> results)
        {
            ThingDef def = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (def == null)
            {
                return;
            }

            List<Thing> things = map.listerThings.ThingsOfDef(def);
            for (int i = 0; i < things.Count; i++)
            {
                if (things[i] is SandWormThing worm && !worm.Destroyed && !worm.IsDying)
                {
                    results.Add(worm);
                }
            }
        }

        // DrawBossBars 负责绘制顶部居中的大沙虫 Boss 血条列表。
        private void DrawBossBars()
        {
            for (int i = 0; i < bossWorms.Count; i++)
            {
                DrawBossBar(bossWorms[i], i);
            }
        }

        // DrawBossBar 负责绘制单条大沙虫血条、状态高光和生命值文本。
        private static void DrawBossBar(SandWormThing worm, int index)
        {
            float x = (UI.screenWidth - BossPanelWidth) * 0.5f;
            float y = BossPanelTopY + index * (BossPanelHeight + 6f);
            float maxHp = Mathf.Max(1f, worm.MaxHitPoints);
            float hp = Mathf.Clamp(worm.HitPoints, 0f, maxHp);
            float fraction = Mathf.Clamp01(hp / maxHp);
            bool charging = worm.CurrentBehaviorId == SandWormBehaviorIds.Charge;
            bool critical = fraction < 0.25f;
            float pulse = Mathf.Sin(Time.unscaledTime * (critical ? 9f : charging ? 6f : 2f)) * 0.5f + 0.5f;
            Color accent = BossAccentColor(charging, critical, pulse);
            Color fill = critical ? accent : charging ? new Color(0.92f, 0.38f, 0.08f) : new Color(0.70f, 0.52f, 0.14f);

            DrawRect(x + 3f, y + 3f, BossPanelWidth, BossPanelHeight, new Color(0f, 0f, 0f, 0.35f));
            Rect panelRect = new Rect(x, y, BossPanelWidth, BossPanelHeight);
            if (!SandWormUiTextures.Draw(panelRect, SandWormUiTextures.ContractBasePath + "HudBossFrame", Color.white))
            {
                DrawRect(x, y, BossPanelWidth, BossPanelHeight, new Color(0.05f, 0.03f, 0.01f, 0.90f));
            }

            DrawRect(x - 2f, y - 2f, BossPanelWidth + 4f, BossPanelHeight + 4f, new Color(accent.r, accent.g, accent.b, charging ? 0.16f + pulse * 0.12f : 0.07f));
            if (critical)
            {
                SandWormUiTextures.Draw(panelRect, SandWormUiTextures.ContractBasePath + "HudCriticalOverlay", new Color(1f, 1f, 1f, 0.32f + pulse * 0.24f));
            }

            DrawFrame(panelRect, new Color(accent.r, accent.g, accent.b, 0.80f));
            DrawCornerBrackets(panelRect, accent, 12f, 2f);

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            GUI.color = new Color(0.96f, 0.80f, 0.38f);
            float titleHeight = Text.LineHeightOf(GameFont.Tiny) + 4f;
            Widgets.Label(new Rect(x, y + 3f, BossPanelWidth, titleHeight), "SandWorm_Hud_Title".Translate());

            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.90f);
            Widgets.Label(new Rect(x + 10f, y + 3f, BossPanelWidth - 20f, titleHeight), "SandWorm_Hud_HitPoints".Translate(Mathf.RoundToInt(hp), Mathf.RoundToInt(maxHp)));

            float barX = x + 10f;
            float barY = y + 28f;
            float barW = BossPanelWidth - 20f;
            DrawSegmentedBar(new Rect(barX, barY, barW, BossBarHeight), fraction, fill, new Color(0.10f, 0.06f, 0.02f, 0.95f), 10, true);
            DrawHudSweep(new Rect(barX, barY - 2f, barW, BossBarHeight + 4f), accent, charging ? 1f : critical ? pulse : 0.45f, index);
        }

        // DrawSmallBars 负责绘制右上角小沙虫紧凑血条列表和折叠提示。
        private void DrawSmallBars()
        {
            int visibleCount = Mathf.Min(MaxVisibleSmallWormBars, smallWorms.Count);
            float x = Mathf.Max(8f, UI.screenWidth - SmallPanelRightMargin - SmallPanelWidth);
            for (int i = 0; i < visibleCount; i++)
            {
                float y = SmallPanelTopY + i * (SmallPanelHeight + SmallPanelGap);
                DrawSmallBar(smallWorms[i], i, new Rect(x, y, SmallPanelWidth, SmallPanelHeight));
            }

            int hiddenCount = smallWorms.Count - visibleCount;
            if (hiddenCount > 0)
            {
                float y = SmallPanelTopY + visibleCount * (SmallPanelHeight + SmallPanelGap);
                DrawSmallCollapsedBar(new Rect(x, y, SmallPanelWidth, SmallPanelHeight), hiddenCount);
            }
        }

        // DrawSmallBar 负责绘制单条小沙虫血条、编号、百分比和完整生命值提示。
        private static void DrawSmallBar(SandWormThing worm, int index, Rect rect)
        {
            float maxHp = Mathf.Max(1f, worm.MaxHitPoints);
            float hp = Mathf.Clamp(worm.HitPoints, 0f, maxHp);
            float fraction = Mathf.Clamp01(hp / maxHp);
            bool critical = fraction < 0.25f;
            float pulse = Mathf.Sin(Time.unscaledTime * (critical ? 8f : 3.5f) + index * 0.41f) * 0.5f + 0.5f;
            Color accent = critical
                ? Color.Lerp(new Color(0.85f, 0.16f, 0.08f), new Color(1f, 0.45f, 0.12f), pulse)
                : Color.Lerp(new Color(0.26f, 0.80f, 0.72f), new Color(0.95f, 0.66f, 0.22f), 0.35f + pulse * 0.15f);
            Color fill = critical ? accent : new Color(0.38f, 0.82f, 0.72f);

            Rect drawRect = rect;
            if (critical)
            {
                float shake = Mathf.Sin(Time.unscaledTime * 54f + index) * 1.4f * pulse;
                drawRect.x += shake;
            }

            DrawRect(drawRect.x + 2f, drawRect.y + 2f, drawRect.width, drawRect.height, new Color(0f, 0f, 0f, 0.28f));
            if (!SandWormUiTextures.Draw(drawRect, SandWormUiTextures.ContractBasePath + "HudSmallFrame", Color.white))
            {
                DrawRect(drawRect.x, drawRect.y, drawRect.width, drawRect.height, new Color(0.035f, 0.040f, 0.035f, 0.88f));
            }

            if (critical)
            {
                SandWormUiTextures.Draw(drawRect, SandWormUiTextures.ContractBasePath + "HudCriticalOverlay", new Color(1f, 1f, 1f, 0.20f + pulse * 0.20f));
            }

            DrawFrame(drawRect, new Color(accent.r, accent.g, accent.b, 0.52f + pulse * 0.12f));
            DrawSmallScanline(drawRect, accent, index);

            Text.Font = GameFont.Tiny;
            Text.WordWrap = false;
            float textHeight = Text.LineHeightOf(GameFont.Tiny) + 2f;
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = new Color(0.94f, 0.86f, 0.64f, 0.96f);
            Widgets.Label(new Rect(drawRect.x + 8f, drawRect.y + 3f, drawRect.width * 0.58f, textHeight), "SandWorm_Hud_SmallTitle".Translate((index + 1).ToString("00")));

            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = new Color(accent.r, accent.g, accent.b, 0.96f);
            Widgets.Label(new Rect(drawRect.x + drawRect.width * 0.58f, drawRect.y + 3f, drawRect.width * 0.38f - 8f, textHeight), Mathf.RoundToInt(fraction * 100f) + "%");

            DrawSegmentedBar(new Rect(drawRect.x + 8f, drawRect.y + drawRect.height - 12f, drawRect.width - 16f, SmallBarHeight), fraction, fill, new Color(0.08f, 0.07f, 0.04f, 0.92f), 8, true);
            TooltipHandler.TipRegion(drawRect, "SandWorm_Hud_SmallTooltip".Translate((index + 1).ToString("00"), Mathf.RoundToInt(hp), Mathf.RoundToInt(maxHp)));
        }

        // DrawSmallCollapsedBar 负责在小沙虫数量过多时绘制折叠计数，避免列表超出屏幕。
        private static void DrawSmallCollapsedBar(Rect rect, int hiddenCount)
        {
            Color accent = new Color(0.42f, 0.88f, 0.74f, 0.78f);
            if (!SandWormUiTextures.Draw(rect, SandWormUiTextures.ContractBasePath + "HudSmallFrame", new Color(1f, 1f, 1f, 0.82f)))
            {
                DrawRect(rect.x, rect.y, rect.width, rect.height, new Color(0.035f, 0.040f, 0.035f, 0.78f));
            }

            DrawFrame(rect, new Color(accent.r, accent.g, accent.b, 0.42f));

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            GUI.color = new Color(0.78f, 0.94f, 0.86f, 0.92f);
            Widgets.Label(rect, "SandWorm_Hud_SmallCollapsed".Translate(hiddenCount));
        }

        // DrawSegmentedBar 负责绘制带素材填充、分段线、高光和底部阴影的生命条。
        private static void DrawSegmentedBar(Rect rect, float fraction, Color fill, Color background, int segments, bool useTextureFill)
        {
            DrawRect(rect.x, rect.y, rect.width, rect.height, background);
            float filledWidth = rect.width * Mathf.Clamp01(fraction);
            if (filledWidth > 0.5f)
            {
                Rect fillRect = new Rect(rect.x, rect.y, filledWidth, rect.height);
                if (!useTextureFill || !SandWormUiTextures.Draw(fillRect, SandWormUiTextures.ContractBasePath + "HudBarFill", new Color(fill.r, fill.g, fill.b, 0.92f)))
                {
                    DrawRect(rect.x, rect.y, filledWidth, rect.height, fill);
                }

                DrawRect(rect.x, rect.y + 1f, filledWidth, rect.height * 0.30f, new Color(Mathf.Min(fill.r + 0.26f, 1f), Mathf.Min(fill.g + 0.18f, 1f), Mathf.Min(fill.b + 0.08f, 1f), 0.46f));
                DrawRect(rect.x, rect.y + rect.height * 0.70f, filledWidth, rect.height * 0.30f, new Color(fill.r * 0.28f, fill.g * 0.28f, fill.b * 0.28f, 0.85f));
            }

            for (int i = 1; i < segments; i++)
            {
                DrawRect(rect.x + rect.width * (i / (float)segments), rect.y, 1f, rect.height, new Color(0f, 0f, 0f, 0.38f));
            }

            DrawFrame(rect, new Color(0f, 0f, 0f, 0.42f));
        }

        // DrawHudSweep 负责在血条填充层上绘制分段能量脉冲，强调冲锋和低血量状态。
        private static void DrawHudSweep(Rect rect, Color accent, float strength, int index)
        {
            float alpha = Mathf.Clamp01(strength);
            if (alpha <= 0.01f)
            {
                return;
            }

            float progress = Mathf.Repeat(Time.unscaledTime * 0.64f + index * 0.17f, 1f);
            for (int i = 0; i < 4; i++)
            {
                float t = Mathf.Repeat(progress + i * 0.18f, 1f);
                float x = Mathf.Lerp(rect.x - 18f, rect.xMax + 8f, t);
                float packetAlpha = alpha * (0.18f - i * 0.026f);
                DrawRect(x, rect.y + 1f, 18f - i * 2f, rect.height - 2f, new Color(accent.r, accent.g, accent.b, packetAlpha));
                DrawRect(x + 4f, rect.y + 2f, 2f, rect.height - 4f, new Color(1f, 0.86f, 0.42f, packetAlpha * 1.42f));
            }

            for (int i = 0; i < 5; i++)
            {
                float t = Mathf.Repeat(progress + i * 0.21f, 1f);
                float x = Mathf.Lerp(rect.x + 4f, rect.xMax - 4f, t);
                float y = rect.y + 2f + Mathf.Sin(Time.unscaledTime * 5.2f + i + index) * 1.5f;
                DrawRect(x, y, 2f, 2f, new Color(1f, 0.84f, 0.36f, 0.18f * alpha));
            }
        }

        // DrawSmallScanline 负责给小沙虫血条添加短促战术脉冲，使其区别于 Boss 大血条。
        private static void DrawSmallScanline(Rect rect, Color accent, int index)
        {
            float progress = Mathf.Repeat(Time.unscaledTime * 0.78f + index * 0.13f, 1f);
            for (int i = 0; i < 3; i++)
            {
                float t = Mathf.Repeat(progress + i * 0.24f, 1f);
                float x = Mathf.Lerp(rect.x + 8f, rect.xMax - 18f, t);
                DrawRect(x, rect.y + 2f, 12f, 1.5f, new Color(accent.r, accent.g, accent.b, 0.18f));
                DrawRect(x + 3f, rect.y + rect.height - 4f, 6f, 1.2f, new Color(1f, 0.80f, 0.36f, 0.14f));
            }
        }

        // BossAccentColor 负责根据大沙虫状态返回顶部 Boss 血条的主题色。
        private static Color BossAccentColor(bool charging, bool critical, float pulse)
        {
            if (critical)
            {
                return Color.Lerp(new Color(0.80f, 0.12f, 0.08f), new Color(1f, 0.38f, 0.10f), pulse);
            }

            if (charging)
            {
                return Color.Lerp(new Color(0.96f, 0.40f, 0.07f), new Color(1f, 0.66f, 0.22f), pulse * 0.35f);
            }

            return new Color(0.32f, 0.82f, 0.76f);
        }

        // DrawFrame 负责绘制矩形边框。
        private static void DrawFrame(Rect rect, Color color)
        {
            DrawRect(rect.x, rect.y, rect.width, 1f, color);
            DrawRect(rect.x, rect.yMax - 1f, rect.width, 1f, color);
            DrawRect(rect.x, rect.y, 1f, rect.height, color);
            DrawRect(rect.xMax - 1f, rect.y, 1f, rect.height, color);
        }

        // DrawCornerBrackets 负责绘制 Boss 血条四角强化边线。
        private static void DrawCornerBrackets(Rect rect, Color color, float length, float width)
        {
            DrawRect(rect.x, rect.y, length, width, color);
            DrawRect(rect.x, rect.y, width, length, color);
            DrawRect(rect.xMax - length, rect.y, length, width, color);
            DrawRect(rect.xMax - width, rect.y, width, length, color);
            DrawRect(rect.x, rect.yMax - width, length, width, color);
            DrawRect(rect.x, rect.yMax - length, width, length, color);
            DrawRect(rect.xMax - length, rect.yMax - width, length, width, color);
            DrawRect(rect.xMax - width, rect.yMax - length, width, length, color);
        }

        // DrawRect 负责用白色贴图绘制指定颜色的矩形。
        private static void DrawRect(float x, float y, float width, float height, Color color)
        {
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            GUI.color = color;
            GUI.DrawTexture(new Rect(x, y, width, height), Texture2D.whiteTexture);
        }
    }
}
