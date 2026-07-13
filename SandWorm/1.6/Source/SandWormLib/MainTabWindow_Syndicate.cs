using RimWorld;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // MainTabWindow_Syndicate 负责在底栏辛迪加按钮打开后展示辛迪加挑战主页。
    public sealed class MainTabWindow_Syndicate : MainTabWindow
    {
        private const float WindowWidth = 920f;
        private const float WindowHeight = 560f;
        private const float OuterPadding = 22f;
        private const float CardSize = 154f;
        private const float CardGap = 14f;
        private const float CardPadding = 8f;
        private const float BottomBarHeight = 35f;
        private static readonly Color BackgroundColor = new Color(0.13f, 0.09f, 0.20f, 0.98f);
        private static readonly Color PanelColor = new Color(0.33f, 0.24f, 0.48f, 0.72f);
        private static readonly Color HoverColor = new Color(0.57f, 0.43f, 0.82f, 0.52f);
        private static readonly Color BorderColor = new Color(0.78f, 0.65f, 1f, 0.82f);
        private static readonly Color LineColor = new Color(0.65f, 0.90f, 1f, 0.72f);
        private static Texture2D sandSeaLeviathanTexture;

        // RequestedTabSize 负责约束辛迪加挑战主页大小，使列表卡片拥有足够展示空间。
        public override Vector2 RequestedTabSize => new Vector2(WindowWidth, WindowHeight);

        // Anchor 负责把辛迪加挑战主页固定到左侧，符合原版底栏主窗口打开习惯。
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Left;

        // SetInitialSizeAndPosition 负责让辛迪加挑战主页在可用游戏画面区域内居中显示。
        protected override void SetInitialSizeAndPosition()
        {
            base.SetInitialSizeAndPosition();
            windowRect.x = Mathf.Max(0f, (UI.screenWidth - windowRect.width) / 2f);
            windowRect.y = Mathf.Max(0f, (UI.screenHeight - BottomBarHeight - windowRect.height) / 2f);
        }

        // DoWindowContents 负责绘制淡紫色科技感背景、挑战列表和悬浮提示。
        public override void DoWindowContents(Rect inRect)
        {
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            Color oldColor = GUI.color;

            DrawBackground(inRect);
            DrawHeader(inRect);
            DrawChallengeList(inRect);

            Text.Font = oldFont;
            Text.Anchor = oldAnchor;
            Text.WordWrap = oldWordWrap;
            GUI.color = oldColor;
        }

        // DrawBackground 负责绘制主页底色、淡紫色面板和科技感线条装饰。
        private static void DrawBackground(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, BackgroundColor);
            Rect glowRect = inRect.ContractedBy(8f);
            Widgets.DrawBoxSolid(glowRect, new Color(0.20f, 0.12f, 0.30f, 0.80f));
            DrawColoredBox(glowRect, 1, BorderColor);

            float lineY = inRect.y + 92f;
            Widgets.DrawLine(new Vector2(inRect.x + OuterPadding, lineY), new Vector2(inRect.xMax - OuterPadding, lineY), LineColor, 1.5f);
            Widgets.DrawLine(new Vector2(inRect.x + OuterPadding, lineY + 8f), new Vector2(inRect.x + 250f, lineY + 8f), LineColor, 1f);
            Widgets.DrawLine(new Vector2(inRect.xMax - 250f, lineY + 8f), new Vector2(inRect.xMax - OuterPadding, lineY + 8f), LineColor, 1f);
        }

        // DrawHeader 负责绘制辛迪加挑战主页标题和副标题说明。
        private static void DrawHeader(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.WordWrap = true;
            GUI.color = new Color(0.93f, 0.86f, 1f, 1f);
            Widgets.Label(new Rect(inRect.x + OuterPadding, inRect.y + 20f, inRect.width - OuterPadding * 2f, 34f), "SandWorm_Syndicate_TabTitle".Translate());

            Text.Font = GameFont.Small;
            GUI.color = new Color(0.78f, 0.84f, 1f, 0.86f);
            Widgets.Label(new Rect(inRect.x + OuterPadding, inRect.y + 58f, inRect.width - OuterPadding * 2f, 28f), "SandWorm_Syndicate_TabBody".Translate());
            GUI.color = Color.white;
        }

        // DrawChallengeList 负责绘制一行约五个正方形挑战按钮，并为每个挑战卡片绑定点击和悬浮内容。
        private static void DrawChallengeList(Rect inRect)
        {
            Rect listRect = new Rect(inRect.x + OuterPadding, inRect.y + 120f, inRect.width - OuterPadding * 2f, inRect.height - 145f);
            Widgets.DrawBoxSolid(listRect, new Color(0.18f, 0.12f, 0.27f, 0.72f));
            DrawColoredBox(listRect, 1, new Color(0.50f, 0.40f, 0.72f, 0.70f));

            Rect cardRect = new Rect(listRect.x + 20f, listRect.y + 20f, CardSize, CardSize);
            DrawSandSeaLeviathanCard(cardRect);
        }

        // DrawSandSeaLeviathanCard 负责绘制沙海巨虫挑战的正方形图片按钮和悬浮说明。
        private static void DrawSandSeaLeviathanCard(Rect cardRect)
        {
            bool hovered = Mouse.IsOver(cardRect);
            Widgets.DrawBoxSolid(cardRect, hovered ? HoverColor : PanelColor);
            DrawColoredBox(cardRect, hovered ? 2 : 1, BorderColor);

            Rect imageRect = new Rect(cardRect.x + CardPadding, cardRect.y + CardPadding, cardRect.width - CardPadding * 2f, cardRect.height - CardPadding * 2f);
            GUI.DrawTexture(imageRect, SandSeaLeviathanTexture(), ScaleMode.ScaleAndCrop);
            DrawColoredBox(imageRect, 1, new Color(0.86f, 0.78f, 1f, 0.78f));

            Rect titleBackRect = new Rect(imageRect.x, imageRect.yMax - 30f, imageRect.width, 30f);
            Widgets.DrawBoxSolid(titleBackRect, new Color(0.10f, 0.06f, 0.16f, 0.82f));

            Rect titleRect = titleBackRect.ContractedBy(6f, 3f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            GUI.color = new Color(0.98f, 0.92f, 1f, 1f);
            Widgets.Label(titleRect, "SandWorm_Challenge_SandSeaLeviathan_Title".Translate());

            if (Widgets.ButtonInvisible(cardRect))
            {
                Find.WindowStack.Add(new Dialog_SandSeaLeviathanContract());
            }

            if (hovered)
            {
                TooltipHandler.TipRegion(cardRect, "SandWorm_Challenge_SandSeaLeviathan_Summary".Translate() + "\n\n" + "SandWorm_Challenge_SandSeaLeviathan_Detail".Translate());
            }

            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        // SandSeaLeviathanTexture 负责延迟加载沙海巨虫挑战封面，避免窗口未打开时提前占用资源。
        private static Texture2D SandSeaLeviathanTexture()
        {
            if (sandSeaLeviathanTexture == null)
            {
                sandSeaLeviathanTexture = ContentFinder<Texture2D>.Get("UI/SandWorm/Challenges/SandSeaLeviathan");
            }

            return sandSeaLeviathanTexture;
        }

        // DrawColoredBox 负责在不污染全局颜色状态的前提下绘制指定颜色边框。
        private static void DrawColoredBox(Rect rect, int thickness, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            Widgets.DrawBox(rect, thickness);
            GUI.color = oldColor;
        }
    }
}
