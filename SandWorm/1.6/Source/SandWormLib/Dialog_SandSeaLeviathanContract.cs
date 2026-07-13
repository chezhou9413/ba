using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace SandWormLib
{
    // Dialog_SandSeaLeviathanContract 负责展示沙海巨虫挑战的人员选择、词条矩阵和合约等级汇总。
    public sealed class Dialog_SandSeaLeviathanContract : Window
    {
        private const float TargetWidth = 1180f;
        private const float TargetHeight = 760f;
        private const float BottomBarHeight = 35f;
        private const float OuterPadding = 18f;
        private const float HeaderHeight = 92f;
        private const float FooterHeight = 72f;
        private const float PanelGap = 14f;
        private const float PawnPanelWidth = 245f;
        private const float SummaryPanelWidth = 300f;
        private const float NodeWidth = 88f;
        private const float NodeHeight = 88f;
        private const float NodeColumnStep = 148f;
        private const float NodeRowStep = 96f;
        private const float PawnRowHeight = 48f;
        private const float DustParticleCount = 28f;
        private const float DetailPopupWidth = 310f;

        private static readonly Color BackgroundColor = new Color(0.09f, 0.065f, 0.045f, 0.99f);
        private static readonly Color DeepPurpleColor = new Color(0.11f, 0.075f, 0.16f, 0.96f);
        private static readonly Color PanelColor = new Color(0.20f, 0.135f, 0.20f, 0.84f);
        private static readonly Color PanelInnerColor = new Color(0.30f, 0.22f, 0.17f, 0.40f);
        private static readonly Color SandGoldColor = new Color(0.95f, 0.67f, 0.29f, 1f);
        private static readonly Color AmberColor = new Color(1f, 0.82f, 0.42f, 1f);
        private static readonly Color CyanLineColor = new Color(0.34f, 0.90f, 1f, 0.72f);
        private static readonly Color DisabledColor = new Color(0.35f, 0.31f, 0.36f, 0.62f);
        private static readonly Color SelectedNodeColor = new Color(0.54f, 0.34f, 0.18f, 0.92f);
        private static readonly Color AvailableNodeColor = new Color(0.28f, 0.18f, 0.31f, 0.86f);
        private static readonly Color HoverNodeColor = new Color(0.48f, 0.31f, 0.20f, 0.92f);
        private static List<SandWormChallengeRiskDef> cachedRisks;
        private static readonly Vector2[] DustSeeds =
        {
            new Vector2(0.04f, 0.22f), new Vector2(0.10f, 0.68f), new Vector2(0.16f, 0.38f), new Vector2(0.22f, 0.82f),
            new Vector2(0.29f, 0.18f), new Vector2(0.34f, 0.54f), new Vector2(0.40f, 0.74f), new Vector2(0.46f, 0.31f),
            new Vector2(0.51f, 0.63f), new Vector2(0.56f, 0.13f), new Vector2(0.61f, 0.87f), new Vector2(0.66f, 0.44f),
            new Vector2(0.72f, 0.25f), new Vector2(0.78f, 0.77f), new Vector2(0.84f, 0.48f), new Vector2(0.90f, 0.20f),
            new Vector2(0.96f, 0.70f), new Vector2(0.08f, 0.91f), new Vector2(0.19f, 0.07f), new Vector2(0.31f, 0.96f),
            new Vector2(0.43f, 0.05f), new Vector2(0.58f, 0.35f), new Vector2(0.69f, 0.60f), new Vector2(0.81f, 0.11f),
            new Vector2(0.93f, 0.56f), new Vector2(0.25f, 0.49f), new Vector2(0.37f, 0.28f), new Vector2(0.74f, 0.93f)
        };

        private readonly SandWormContractAnimationState animationState = new SandWormContractAnimationState();
        private readonly HashSet<Pawn> selectedPawns = new HashSet<Pawn>();
        private readonly HashSet<SandWormChallengeRiskDef> selectedRisks = new HashSet<SandWormChallengeRiskDef>();
        private Vector2 pawnScrollPosition;
        private Vector2 riskMatrixScrollPosition;
        private Vector2 selectedRiskScrollPosition;
        private bool launchPending;
        private static float currentDrawAlpha = 1f;
        private SandWormChallengeRiskDef hoveredRisk;
        private Rect hoveredRiskRect;
        private Rect detailAnchorRect;
        private Rect currentMatrixVisibleRect;

        // Dialog_SandSeaLeviathanContract 负责初始化挑战终端窗口的行为参数。
        public Dialog_SandSeaLeviathanContract()
        {
            layer = WindowLayer.GameUI;
            doCloseX = false;
            doCloseButton = false;
            doWindowBackground = false;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnCancel = true;
            preventCameraMotion = true;
            forcePause = false;
            animationState.PlayOpen();
        }

        // InitialSize 负责根据当前屏幕缩放挑战终端大小，避免低分辨率下越界。
        public override Vector2 InitialSize
        {
            get
            {
                float width = Mathf.Min(TargetWidth, UI.screenWidth - 80f);
                float height = Mathf.Min(TargetHeight, UI.screenHeight - BottomBarHeight - 70f);
                return new Vector2(Mathf.Max(900f, width), Mathf.Max(620f, height));
            }
        }

        // SetInitialSizeAndPosition 负责把挑战终端放在底栏上方的可用屏幕区域中央。
        protected override void SetInitialSizeAndPosition()
        {
            Vector2 size = InitialSize;
            float x = Mathf.Max(0f, (UI.screenWidth - size.x) / 2f);
            float y = Mathf.Max(0f, (UI.screenHeight - BottomBarHeight - size.y) / 2f);
            windowRect = new Rect(x, y, size.x, size.y).Rounded();
        }

        // PostClose 负责在窗口关闭时释放 DOTween 动画，避免关闭后继续持有回调。
        public override void PostClose()
        {
            animationState.Dispose();
            base.PostClose();
        }

        // DoWindowContents 负责绘制整套沙漠科技挑战终端，并在结束时恢复全局 GUI 状态。
        public override void DoWindowContents(Rect inRect)
        {
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            Color oldColor = GUI.color;
            Matrix4x4 oldMatrix = GUI.matrix;
            currentDrawAlpha = Mathf.Clamp01(animationState.WindowAlpha);

            try
            {
                Rect animatedRect = inRect;
                animatedRect.y += animationState.WindowOffsetY;
                GUIUtility.ScaleAroundPivot(new Vector2(animationState.WindowScale, animationState.WindowScale), inRect.center);

                DrawTerminalBackground(animatedRect);
                DrawAnimatedSand(animatedRect);
                DrawHeader(HeaderRect(animatedRect));

                Rect bodyRect = BodyRect(animatedRect);
                Rect pawnRect = new Rect(bodyRect.x - animationState.LeftPanelSlide, bodyRect.y, PawnPanelWidth, bodyRect.height);
                Rect summaryRect = new Rect(bodyRect.xMax - SummaryPanelWidth + animationState.RightPanelSlide, bodyRect.y, SummaryPanelWidth, bodyRect.height);
                Rect matrixRect = new Rect(pawnRect.xMax + PanelGap + animationState.MatrixPanelSlide, bodyRect.y, bodyRect.width - PawnPanelWidth - SummaryPanelWidth - PanelGap * 2f, bodyRect.height);

                DrawPawnPanel(pawnRect);
                DrawRiskMatrix(matrixRect);
                DrawSelectedRiskPanel(summaryRect);
                DrawFooter(OffsetRect(FooterRect(animatedRect), 0f, animationState.FooterSlide));
                DrawStartupSweep(animatedRect);
                DrawLaunchOverlay(animatedRect);
                DrawRiskDetailPopup();
            }
            finally
            {
                GUI.matrix = oldMatrix;
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
                GUI.color = oldColor;
                currentDrawAlpha = 1f;
            }
        }

        // HeaderRect 负责返回顶部横幅区域。
        private static Rect HeaderRect(Rect inRect)
        {
            return new Rect(inRect.x + OuterPadding, inRect.y + OuterPadding, inRect.width - OuterPadding * 2f, HeaderHeight);
        }

        // BodyRect 负责返回中部三栏主体区域。
        private static Rect BodyRect(Rect inRect)
        {
            float y = inRect.y + OuterPadding + HeaderHeight + PanelGap;
            float height = inRect.height - OuterPadding * 2f - HeaderHeight - FooterHeight - PanelGap * 2f;
            return new Rect(inRect.x + OuterPadding, y, inRect.width - OuterPadding * 2f, height);
        }

        // FooterRect 负责返回底部操作条区域。
        private static Rect FooterRect(Rect inRect)
        {
            return new Rect(inRect.x + OuterPadding, inRect.yMax - OuterPadding - FooterHeight, inRect.width - OuterPadding * 2f, FooterHeight);
        }

        // DrawTerminalBackground 负责绘制暗紫沙漠终端底色、网格和斜角边框。
        private static void DrawTerminalBackground(Rect inRect)
        {
            Widgets.DrawBoxSolid(inRect, WithAlpha(BackgroundColor));
            if (!SandWormUiTextures.Draw(inRect.ContractedBy(2f), SandWormUiTextures.ContractBasePath + "TerminalBackground", WithAlpha(Color.white)))
            {
                Widgets.DrawBoxSolid(inRect.ContractedBy(8f), WithAlpha(DeepPurpleColor));
            }

            SandWormUiTextures.Draw(inRect.ContractedBy(8f), SandWormUiTextures.ContractBasePath + "SandDustOverlay", WithAlpha(new Color(1f, 1f, 1f, 0.36f)));
            DrawGrid(inRect.ContractedBy(14f));
            DrawCutCornerBox(inRect.ContractedBy(8f), WithAlpha(SandGoldColor), 2f);
            DrawCutCornerBox(inRect.ContractedBy(18f), WithAlpha(new Color(0.38f, 0.25f, 0.44f, 0.84f)), 1f);
        }

        // DrawGrid 负责绘制低透明科技网格，形成辛迪加终端背景。
        private static void DrawGrid(Rect rect)
        {
            Color lineColor = WithAlpha(new Color(0.55f, 0.38f, 0.22f, 0.13f));
            for (float x = rect.x + 30f; x < rect.xMax; x += 42f)
            {
                Widgets.DrawLine(new Vector2(x, rect.y), new Vector2(x, rect.yMax), lineColor, 1f);
            }

            for (float y = rect.y + 24f; y < rect.yMax; y += 36f)
            {
                Widgets.DrawLine(new Vector2(rect.x, y), new Vector2(rect.xMax, y), lineColor, 1f);
            }
        }

        // DrawAnimatedSand 负责绘制随时间漂移的轻量沙尘点和短线。
        private static void DrawAnimatedSand(Rect rect)
        {
            float time = Time.realtimeSinceStartup;
            for (int i = 0; i < DustParticleCount && i < DustSeeds.Length; i++)
            {
                Vector2 seed = DustSeeds[i];
                float x = rect.x + Mathf.Repeat(seed.x * rect.width + time * (12f + i * 0.35f), rect.width);
                float y = rect.y + seed.y * rect.height + Mathf.Sin(time * 0.55f + i) * 4f;
                float alpha = 0.08f + 0.08f * Mathf.Sin(time * 0.8f + i * 1.7f);
                Color color = WithAlpha(new Color(1f, 0.74f, 0.35f, alpha));
                Widgets.DrawLine(new Vector2(x, y), new Vector2(x + 10f, y + 1.5f), color, 1.2f);
            }
        }

        // DrawHeader 负责绘制顶部标题、合约等级、参战人数和关闭按钮。
        private void DrawHeader(Rect rect)
        {
            DrawPanel(rect, new Color(0.19f, 0.11f, 0.10f, 0.88f));
            DrawScanline(rect, 0.75f);

            Rect levelRect = new Rect(rect.xMax - 430f, rect.y + 18f, 160f, 54f);
            DrawMetricBox(levelRect, "SandWorm_Contract_Level".Translate(), ContractLevel().ToString());

            Rect pawnRect = new Rect(levelRect.xMax + 12f, rect.y + 18f, 130f, 54f);
            DrawMetricBox(pawnRect, "SandWorm_Contract_Pawns".Translate(), selectedPawns.Count.ToString());

            float titleWidth = Mathf.Max(240f, levelRect.x - rect.x - 48f);
            Rect titleRect = new Rect(rect.x + 22f, rect.y + 12f, titleWidth, Text.LineHeightOf(GameFont.Medium) + 4f);
            DrawClampedLabel(titleRect, "SandWorm_Contract_Title".Translate(), GameFont.Medium, TextAnchor.MiddleLeft, new Color(1f, 0.86f, 0.52f, 1f));

            Rect subtitleRect = new Rect(rect.x + 24f, titleRect.yMax + 4f, titleWidth, Text.LineHeightOf(GameFont.Small) + 4f);
            DrawClampedLabel(subtitleRect, "SandWorm_Contract_Subtitle".Translate(), GameFont.Small, TextAnchor.MiddleLeft, new Color(0.82f, 0.74f, 0.95f, 0.92f));

            Rect closeRect = new Rect(rect.xMax - 110f, rect.y + 26f, 86f, 34f);
            if (DrawTerminalButton(closeRect, "SandWorm_Contract_Close".Translate(), false, "button.close"))
            {
                Close();
            }
        }

        // DrawMetricBox 负责绘制顶部横幅中的小型统计盒。
        private static void DrawMetricBox(Rect rect, string label, string value)
        {
            DrawCutCornerSolid(rect, WithAlpha(new Color(0.09f, 0.06f, 0.08f, 0.78f)), 10f);
            DrawCutCornerBox(rect, WithAlpha(new Color(0.86f, 0.58f, 0.25f, 0.80f)), 1f);
            float tinyHeight = Text.LineHeightOf(GameFont.Tiny) + 2f;
            float mediumHeight = Text.LineHeightOf(GameFont.Medium) + 2f;
            DrawClampedLabel(new Rect(rect.x + 6f, rect.y + 5f, rect.width - 12f, tinyHeight), label, GameFont.Tiny, TextAnchor.MiddleCenter, new Color(0.83f, 0.78f, 0.95f, 0.92f));
            DrawClampedLabel(new Rect(rect.x + 6f, rect.yMax - mediumHeight - 5f, rect.width - 12f, mediumHeight), value, GameFont.Medium, TextAnchor.MiddleCenter, AmberColor);
        }

        // DrawPawnPanel 负责绘制主殖民地参战小人选择列表。
        private void DrawPawnPanel(Rect rect)
        {
            DrawPanel(rect, PanelColor);
            DrawPanelTitle(rect, "SandWorm_Contract_PawnPanel".Translate());

            Rect listRect = rect.ContractedBy(12f);
            listRect.yMin += 38f;
            int pawnCount = HomeColonistCount();
            if (pawnCount == 0)
            {
                DrawEmptyState(listRect, "SandWorm_Contract_NoPawns".Translate());
                return;
            }

            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, Mathf.Max(listRect.height, pawnCount * (PawnRowHeight + 6f)));
            Widgets.BeginScrollView(listRect, ref pawnScrollPosition, viewRect);
            float y = 0f;
            int index = 0;
            foreach (Map map in Find.Maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
                {
                    Rect rowRect = new Rect(0f, y, viewRect.width, PawnRowHeight);
                    DrawPawnRow(rowRect, pawn, index);
                    y += PawnRowHeight + 6f;
                    index++;
                }
            }

            Widgets.EndScrollView();
        }

        // HomeColonistCount 负责统计可作为测试参战对象的主殖民地自由殖民者数量。
        private static int HomeColonistCount()
        {
            int count = 0;
            foreach (Map map in Find.Maps)
            {
                if (!map.IsPlayerHome)
                {
                    continue;
                }

                count += map.mapPawns.FreeColonistsSpawned.Count;
            }

            return count;
        }

        // DrawPawnRow 负责绘制单个小人的选择行和选中高亮。
        private void DrawPawnRow(Rect rect, Pawn pawn, int index)
        {
            bool selected = selectedPawns.Contains(pawn);
            bool hovered = Mouse.IsOver(rect);
            string key = "pawn." + pawn.ThingID;
            float entry = animationState.Entry(key, index, 0.035f, 0.34f);
            float hover = animationState.Hover(key, hovered);
            float flash = animationState.FlashValue(key);
            Rect animatedRect = OffsetRect(rect, Mathf.Lerp(-18f, 0f, entry), 0f);
            float alpha = Mathf.Clamp01(entry) * currentDrawAlpha;
            Color fillColor = selected ? new Color(0.44f, 0.28f, 0.13f, 0.92f) : new Color(0.12f, 0.08f, 0.11f, 0.72f);
            fillColor = Color.Lerp(fillColor, new Color(0.39f, 0.25f, 0.16f, 0.92f), hover);
            fillColor = Color.Lerp(fillColor, new Color(0.72f, 0.44f, 0.16f, 0.96f), flash * 0.45f);

            DrawCutCornerSolid(animatedRect, WithAlpha(fillColor, fillColor.a * alpha), 10f);
            Color border = selected ? Color.Lerp(SandGoldColor, AmberColor, 0.35f + 0.45f * animationState.StartButtonPulse) : new Color(0.45f, 0.36f, 0.55f, 0.72f);
            border = Color.Lerp(border, AmberColor, hover * 0.55f + flash * 0.35f);
            DrawCutCornerBox(animatedRect, WithAlpha(border, border.a * alpha), selected ? 2f : 1f);

            if (hover > 0.01f || flash > 0.01f)
            {
                DrawGlowLine(new Rect(animatedRect.x + 8f, animatedRect.y + 4f, animatedRect.width - 16f, 2f), Mathf.Max(hover, flash), AmberColor);
            }

            float nameHeight = Text.LineHeightOf(GameFont.Small) + 2f;
            float mapHeight = Text.LineHeightOf(GameFont.Tiny) + 2f;
            Rect nameRect = new Rect(animatedRect.x + 10f, animatedRect.y + 5f, animatedRect.width - 20f, nameHeight);
            Rect mapRect = new Rect(animatedRect.x + 10f, nameRect.yMax + 2f, animatedRect.width - 20f, mapHeight);
            DrawClampedLabel(nameRect, pawn.LabelShortCap, GameFont.Small, TextAnchor.MiddleLeft, selected ? AmberColor : new Color(0.90f, 0.86f, 0.96f, 0.95f), alpha);
            DrawClampedLabel(mapRect, pawn.Map?.Parent?.LabelCap ?? "Map".Translate(), GameFont.Tiny, TextAnchor.MiddleLeft, new Color(0.72f, 0.76f, 0.86f, 0.86f), alpha);

            if (Widgets.ButtonInvisible(animatedRect))
            {
                if (selected)
                {
                    selectedPawns.Remove(pawn);
                    animationState.Flash(key);
                }
                else
                {
                    selectedPawns.Add(pawn);
                    animationState.Flash(key);
                }
            }
        }

        // DrawRiskMatrix 负责绘制可滚动的中央词条网络、递进连线和扫描线。
        private void DrawRiskMatrix(Rect rect)
        {
            DrawPanel(rect, new Color(0.14f, 0.09f, 0.17f, 0.90f));
            DrawPanelTitle(rect, "SandWorm_Contract_RiskMatrix".Translate());
            DrawScanline(rect.ContractedBy(10f), 1.35f);

            Rect matrixRect = rect.ContractedBy(22f);
            matrixRect.yMin += 44f;
            currentMatrixVisibleRect = matrixRect;
            hoveredRisk = null;
            hoveredRiskRect = default;
            SandWormUiTextures.Draw(matrixRect, SandWormUiTextures.ContractBasePath + "RiskMatrixGrid", WithAlpha(new Color(1f, 1f, 1f, 0.34f)));
            Rect viewRect = RiskMatrixViewRect(matrixRect);
            Widgets.BeginScrollView(matrixRect, ref riskMatrixScrollPosition, viewRect);
            DrawRiskConnections(viewRect);
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                DrawRiskNode(NodeRect(viewRect, risks[i]), risks[i], i);
            }

            Widgets.EndScrollView();
            animationState.SetDetailTarget(hoveredRisk?.defName);
        }

        // NodeRect 负责把词条网格坐标转换为中央矩阵里的实际矩形。
        private static Rect NodeRect(Rect matrixRect, SandWormChallengeRiskDef risk)
        {
            float x = matrixRect.x + 12f + risk.gridPosition.x * NodeColumnStep;
            float y = matrixRect.y + 12f + risk.gridPosition.y * NodeRowStep;
            return new Rect(x, y, NodeWidth, NodeHeight);
        }

        // RiskMatrixViewRect 负责按 XML 词条坐标扩展滚动内容范围，允许后续追加更多词条。
        private static Rect RiskMatrixViewRect(Rect matrixRect)
        {
            float maxX = 0f;
            float maxY = 0f;
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                maxX = Mathf.Max(maxX, risks[i].gridPosition.x);
                maxY = Mathf.Max(maxY, risks[i].gridPosition.y);
            }

            float width = Mathf.Max(matrixRect.width - 16f, maxX * NodeColumnStep + NodeWidth + 32f);
            float height = Mathf.Max(matrixRect.height - 16f, maxY * NodeRowStep + NodeHeight + 32f);
            return new Rect(0f, 0f, width, height);
        }

        // DrawRiskConnections 负责按六边形图标中心点绘制词条前置连线和已激活流光。
        private void DrawRiskConnections(Rect matrixRect)
        {
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                SandWormChallengeRiskDef risk = risks[i];
                if (risk.prerequisite.NullOrEmpty())
                {
                    continue;
                }

                SandWormChallengeRiskDef prereq = FindRisk(risk.prerequisite);
                if (prereq == null)
                {
                    continue;
                }

                Rect fromRect = NodeRect(matrixRect, prereq);
                Rect toRect = NodeRect(matrixRect, risk);
                Vector2 start = fromRect.center;
                Vector2 end = toRect.center;
                bool prereqSelected = selectedRisks.Contains(prereq);
                bool chainSelected = prereqSelected && selectedRisks.Contains(risk);
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.realtimeSinceStartup * 3.4f + risk.gridPosition.y * 0.37f);
                Color baseColor = prereqSelected ? new Color(0.88f, 0.62f, 0.28f, 0.70f) : new Color(0.32f, 0.28f, 0.35f, 0.58f);
                Widgets.DrawLine(start, end, WithAlpha(baseColor), prereqSelected ? 2.4f : 1.8f);
                Widgets.DrawLine(start, end, WithAlpha(new Color(0f, 0f, 0f, 0.26f)), prereqSelected ? 5.5f : 4.2f);

                if (prereqSelected)
                {
                    DrawEnergyLink(start, end, risk.gridPosition.y, chainSelected, pulse);
                    Rect nodeRect = CenteredRect(Vector2.Lerp(start, end, 0.5f), 20f, 20f);
                    SandWormUiTextures.Draw(nodeRect, SandWormUiTextures.ContractBasePath + "ConnectionNode", WithAlpha(new Color(1f, 1f, 1f, 0.65f + pulse * 0.22f)), ScaleMode.ScaleToFit);
                }
            }
        }

        // DrawEnergyLink 负责绘制前置连线上的分段能量包和节点粒子，避免使用廉价单条扫光。
        private static void DrawEnergyLink(Vector2 start, Vector2 end, float seed, bool chainSelected, float pulse)
        {
            Vector2 direction = end - start;
            float length = direction.magnitude;
            if (length <= 1f)
            {
                return;
            }

            Vector2 normal = new Vector2(-direction.y, direction.x).normalized;
            float speed = chainSelected ? 0.92f : 0.66f;
            float phase = Mathf.Repeat(Time.realtimeSinceStartup * speed + seed * 0.113f, 1f);
            int packets = chainSelected ? 4 : 3;
            for (int i = 0; i < packets; i++)
            {
                float t = Mathf.Repeat(phase + i / (float)packets, 1f);
                float head = Mathf.Clamp01(t);
                float tail = Mathf.Clamp01(t - 0.105f);
                Vector2 packetStart = Vector2.Lerp(start, end, tail);
                Vector2 packetEnd = Vector2.Lerp(start, end, head);
                float width = chainSelected ? 4.8f : 3.6f;
                float alpha = (chainSelected ? 0.68f : 0.48f) + pulse * 0.12f;
                Widgets.DrawLine(packetStart, packetEnd, WithAlpha(CyanLineColor, alpha), width);
                Widgets.DrawLine(packetStart + normal * 2f, packetEnd + normal * 2f, WithAlpha(AmberColor, alpha * 0.38f), 1.4f);

                Vector2 spark = Vector2.Lerp(start, end, Mathf.Repeat(t + 0.045f, 1f));
                Rect sparkRect = CenteredRect(spark + normal * Mathf.Sin((Time.realtimeSinceStartup + seed + i) * 5.1f) * 3f, chainSelected ? 5.5f : 4.2f, chainSelected ? 5.5f : 4.2f);
                DrawSolid(sparkRect, WithAlpha(AmberColor, alpha * 0.46f));
            }
        }

        // DrawRiskNode 负责绘制单个无文字六边形词条节点、图标状态和点击逻辑。
        private void DrawRiskNode(Rect rect, SandWormChallengeRiskDef risk, int index)
        {
            bool selected = selectedRisks.Contains(risk);
            bool available = RiskAvailable(risk);
            bool hovered = Mouse.IsOver(rect);
            string key = "risk." + risk.defName;
            float entry = animationState.Entry(key, index, 0.045f, 0.40f);
            float hover = animationState.Hover(key, hovered);
            float flash = animationState.FlashValue(key);
            Rect animatedRect = ScaleRect(rect, 0.86f + 0.14f * entry + (selected ? 0.06f : 0.035f * hover));
            animatedRect.y += Mathf.Lerp(14f, 0f, entry);
            float alpha = Mathf.Clamp01(entry) * currentDrawAlpha;
            string nodePath = NodeTexturePath(available, selected, hover);

            Color iconColor = WithAlpha(available ? Color.white : new Color(0.55f, 0.55f, 0.60f, 0.58f), alpha);
            Color oldColor = GUI.color;
            GUI.color = iconColor;
            GUI.DrawTexture(animatedRect, RiskIcon(risk), ScaleMode.ScaleToFit);
            GUI.color = oldColor;

            bool drewStateFrame = SandWormUiTextures.Draw(animatedRect, nodePath, WithAlpha(Color.white, alpha), ScaleMode.ScaleToFit);
            if (!drewStateFrame)
            {
                Color fillColor = !available ? DisabledColor : (selected ? SelectedNodeColor : AvailableNodeColor);
                fillColor = Color.Lerp(fillColor, HoverNodeColor, hover * (available ? 0.82f : 0.28f));
                fillColor = Color.Lerp(fillColor, new Color(0.72f, 0.42f, 0.16f, 0.96f), flash * 0.55f);
                DrawCutCornerSolid(animatedRect.ContractedBy(5f), WithAlpha(fillColor, fillColor.a * 0.28f * alpha), 6f);
            }

            Color borderColor = !available ? new Color(0.42f, 0.39f, 0.48f, 0.72f) : (selected ? AmberColor : Color.Lerp(SandGoldColor, AmberColor, 0.75f));
            borderColor = Color.Lerp(borderColor, AmberColor, hover * 0.55f + flash * 0.5f);
            if (!drewStateFrame)
            {
                DrawCutCornerBox(animatedRect, WithAlpha(borderColor, borderColor.a * alpha), selected ? 2.5f : 1.5f);
            }

            if (!available)
            {
                DrawLockedGlyph(animatedRect, alpha, flash);
            }

            if (Widgets.ButtonInvisible(animatedRect))
            {
                ToggleRisk(risk);
            }

            if (hovered)
            {
                hoveredRisk = risk;
                hoveredRiskRect = new Rect(currentMatrixVisibleRect.x + animatedRect.x - riskMatrixScrollPosition.x, currentMatrixVisibleRect.y + animatedRect.y - riskMatrixScrollPosition.y, animatedRect.width, animatedRect.height);
                detailAnchorRect = hoveredRiskRect;
            }
        }

        // ToggleRisk 负责按照前置递进规则切换词条选中状态。
        private void ToggleRisk(SandWormChallengeRiskDef risk)
        {
            if (!RiskAvailable(risk))
            {
                SandWormChallengeRiskDef prereq = FindRisk(risk.prerequisite);
                Messages.Message("SandWorm_Contract_RiskLocked".Translate(prereq?.LabelCap ?? string.Empty), MessageTypeDefOf.RejectInput, historical: false);
                animationState.PlayInvalidShake();
                animationState.Flash("risk." + risk.defName);
                return;
            }

            if (selectedRisks.Contains(risk))
            {
                RemoveSelectedRisk(risk);
            }
            else
            {
                selectedRisks.Add(risk);
                animationState.Flash("risk." + risk.defName);
            }
        }

        // RemoveSelectedRisk 负责取消已选词条，并同步取消依赖它的后续词条。
        private void RemoveSelectedRisk(SandWormChallengeRiskDef risk)
        {
            if (risk == null || !selectedRisks.Remove(risk))
            {
                return;
            }

            animationState.Flash("risk." + risk.defName);
            animationState.Flash("selectedRisk." + risk.defName);
            RemoveDependentRisks(risk.defName);
        }

        // RemoveDependentRisks 负责在取消前置词条时递归取消所有依赖词条。
        private void RemoveDependentRisks(string riskId)
        {
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                SandWormChallengeRiskDef entry = risks[i];
                if (entry.prerequisite == riskId && selectedRisks.Contains(entry))
                {
                    selectedRisks.Remove(entry);
                    animationState.Flash("risk." + entry.defName);
                    animationState.Flash("selectedRisk." + entry.defName);
                    RemoveDependentRisks(entry.defName);
                }
            }
        }

        // RiskAvailable 负责判断词条当前是否满足前置条件。
        private bool RiskAvailable(SandWormChallengeRiskDef risk)
        {
            return risk.prerequisite.NullOrEmpty() || selectedRisks.Contains(FindRisk(risk.prerequisite));
        }

        // FindRisk 负责按内部编号查找词条定义。
        private static SandWormChallengeRiskDef FindRisk(string id)
        {
            return id.NullOrEmpty() ? null : DefDatabase<SandWormChallengeRiskDef>.GetNamedSilentFail(id);
        }

        // ChallengeRisks 负责从 XML Def 中读取并按网格位置排序词条。
        private static List<SandWormChallengeRiskDef> ChallengeRisks()
        {
            if (cachedRisks == null)
            {
                cachedRisks = new List<SandWormChallengeRiskDef>(DefDatabase<SandWormChallengeRiskDef>.AllDefsListForReading);
                cachedRisks.SortBy(def => def.gridPosition.y, def => def.gridPosition.x, def => def.defName);
            }

            return cachedRisks;
        }

        // RiskIcon 负责按 XML 配置路径读取词条图标并缓存。
        private static Texture2D RiskIcon(SandWormChallengeRiskDef risk)
        {
            string path = risk.iconPath.NullOrEmpty() ? "Things/SandWorm/SandHammer" : risk.iconPath;
            return SandWormUiTextures.Get(path) ?? SandWormUiTextures.Get("Things/SandWorm/SandHammer") ?? BaseContent.BadTex;
        }

        // NodeTexturePath 负责根据节点状态选择对应的六边形底座素材。
        private static string NodeTexturePath(bool available, bool selected, float hover)
        {
            if (!available)
            {
                return SandWormUiTextures.ContractBasePath + "HexNodeLocked";
            }

            if (selected)
            {
                return SandWormUiTextures.ContractBasePath + "HexNodeSelected";
            }

            return hover > 0.05f ? SandWormUiTextures.ContractBasePath + "HexNodeHover" : SandWormUiTextures.ContractBasePath + "HexNodeNormal";
        }

        // DrawLockedGlyph 负责在锁定节点上绘制短促锁定反馈和禁用状态提示。
        private static void DrawLockedGlyph(Rect rect, float alpha, float flash)
        {
            Color color = WithAlpha(new Color(0.95f, 0.35f, 0.18f, 0.72f + flash * 0.25f), alpha);
            Rect slashA = new Rect(rect.center.x - 18f, rect.center.y - 1f, 36f, 2f);
            Rect slashB = new Rect(rect.center.x - 1f, rect.center.y - 18f, 2f, 36f);
            Widgets.DrawLine(new Vector2(slashA.x, slashA.y), new Vector2(slashA.xMax, slashA.y + 18f), color, 2f + flash);
            Widgets.DrawLine(new Vector2(slashB.x, slashB.yMax), new Vector2(slashB.xMax + 18f, slashB.y), color, 2f + flash);
        }

        // DrawRiskDetailPopup 负责在悬停词条时绘制素材化详情弹窗和安全文本布局。
        private void DrawRiskDetailPopup()
        {
            string detailKey = animationState.DetailKey;
            if (detailKey.NullOrEmpty() || animationState.DetailAlpha <= 0.001f)
            {
                return;
            }

            SandWormChallengeRiskDef risk = FindRisk(detailKey);
            if (risk == null)
            {
                return;
            }

            bool available = RiskAvailable(risk);
            float alpha = animationState.DetailAlpha * currentDrawAlpha;
            float contentWidth = DetailPopupWidth - 32f;
            string lockText = string.Empty;
            if (!available)
            {
                SandWormChallengeRiskDef prereq = FindRisk(risk.prerequisite);
                lockText = "SandWorm_Contract_RiskLocked".Translate(prereq?.LabelCap ?? string.Empty);
            }

            float titleHeight = Text.LineHeightOf(GameFont.Small) + 4f;
            float levelHeight = Text.LineHeightOf(GameFont.Tiny) + 3f;
            float descHeight = CalcWrappedHeight(risk.description, contentWidth, GameFont.Tiny);
            float lockHeight = lockText.NullOrEmpty() ? 0f : CalcWrappedHeight(lockText, contentWidth, GameFont.Tiny) + 8f;
            float popupHeight = Mathf.Max(132f, 18f + titleHeight + 4f + levelHeight + 8f + descHeight + lockHeight + 18f);
            Rect anchorRect = detailAnchorRect.width > 0f ? detailAnchorRect : hoveredRiskRect;
            Rect popupRect = new Rect(anchorRect.xMax + 12f, anchorRect.y - 14f - animationState.DetailOffsetY, DetailPopupWidth, popupHeight);
            popupRect = ClampRectToWindow(popupRect);

            if (!SandWormUiTextures.Draw(popupRect, SandWormUiTextures.ContractBasePath + "DetailPopupFrame", WithAlpha(Color.white, alpha)))
            {
                DrawCutCornerSolid(popupRect, WithAlpha(new Color(0.07f, 0.04f, 0.06f, 0.94f), alpha), 8f);
                DrawCutCornerBox(popupRect, WithAlpha(SandGoldColor, alpha), 1.6f);
            }

            DrawScanSweep(new Rect(popupRect.x + 12f, popupRect.y + 8f, popupRect.width - 24f, popupRect.height - 16f), alpha);
            Rect iconRect = new Rect(popupRect.x + 18f, popupRect.y + 18f, 48f, 48f);
            Color oldColor = GUI.color;
            GUI.color = WithAlpha(available ? Color.white : new Color(0.62f, 0.62f, 0.68f, 0.72f), alpha);
            GUI.DrawTexture(iconRect, RiskIcon(risk), ScaleMode.ScaleToFit);
            GUI.color = oldColor;

            Rect titleRect = new Rect(iconRect.xMax + 10f, popupRect.y + 18f, popupRect.width - 88f, titleHeight);
            DrawClampedLabel(titleRect, risk.LabelCap, GameFont.Small, TextAnchor.MiddleLeft, available ? AmberColor : new Color(0.78f, 0.72f, 0.82f, 0.92f), alpha);
            Rect levelRect = new Rect(titleRect.x, titleRect.yMax + 2f, titleRect.width, levelHeight);
            DrawClampedLabel(levelRect, "SandWorm_Contract_RiskLevel".Translate(risk.level), GameFont.Tiny, TextAnchor.MiddleLeft, CyanLineColor, alpha);

            Rect descRect = new Rect(popupRect.x + 16f, iconRect.yMax + 12f, contentWidth, descHeight);
            DrawWrappedLabel(descRect, risk.description, GameFont.Tiny, new Color(0.88f, 0.84f, 0.96f, 0.95f), alpha);

            if (!lockText.NullOrEmpty())
            {
                Rect lockRect = new Rect(descRect.x, descRect.yMax + 8f, contentWidth, lockHeight - 8f);
                DrawWrappedLabel(lockRect, lockText, GameFont.Tiny, new Color(1f, 0.44f, 0.24f, 0.94f), alpha);
            }
        }

        // DrawSelectedRiskPanel 负责绘制右侧已选词条效果清单。
        private void DrawSelectedRiskPanel(Rect rect)
        {
            DrawPanel(rect, PanelColor);
            DrawPanelTitle(rect, "SandWorm_Contract_SelectedRisks".Translate());

            Rect listRect = rect.ContractedBy(12f);
            listRect.yMin += 38f;
            SandWormUiTextures.Draw(listRect, SandWormUiTextures.ContractBasePath + "ScrollPanelFrame", WithAlpha(new Color(1f, 1f, 1f, 0.35f)));
            int selectedCount = SelectedRiskCount();
            if (selectedCount == 0)
            {
                DrawEmptyState(listRect, "SandWorm_Contract_NoRisks".Translate());
                return;
            }

            float viewHeight = Mathf.Max(listRect.height, SelectedRiskViewHeight(listRect.width - 16f));
            Rect viewRect = new Rect(0f, 0f, listRect.width - 16f, viewHeight);
            Widgets.BeginScrollView(listRect, ref selectedRiskScrollPosition, viewRect);
            float y = 0f;
            int selectedIndex = 0;
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                SandWormChallengeRiskDef risk = risks[i];
                if (!selectedRisks.Contains(risk))
                {
                    continue;
                }

                float rowHeight = SelectedRiskEntryHeight(viewRect.width, risk);
                Rect riskRect = new Rect(0f, y, viewRect.width, rowHeight);
                DrawSelectedRiskEntry(riskRect, risk, selectedIndex);
                y += rowHeight + 10f;
                selectedIndex++;
            }

            Widgets.EndScrollView();
        }

        // SelectedRiskViewHeight 负责按已选词条说明文字的真实高度计算滚动内容高度。
        private float SelectedRiskViewHeight(float width)
        {
            float height = 0f;
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                if (selectedRisks.Contains(risks[i]))
                {
                    height += SelectedRiskEntryHeight(width, risks[i]) + 10f;
                }
            }

            return height;
        }

        // SelectedRiskCount 负责统计当前已选词条数量。
        private int SelectedRiskCount()
        {
            int count = 0;
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                if (selectedRisks.Contains(risks[i]))
                {
                    count++;
                }
            }

            return count;
        }

        // DrawSelectedRiskEntry 负责绘制素材化已选词条卡片、图标、等级和效果说明。
        private void DrawSelectedRiskEntry(Rect rect, SandWormChallengeRiskDef risk, int index)
        {
            string key = "selectedRisk." + risk.defName;
            bool hovered = Mouse.IsOver(rect);
            float entry = animationState.Entry(key, index, 0.045f, 0.36f);
            float hover = animationState.Hover(key, hovered);
            float flash = animationState.FlashValue(key);
            float exit = animationState.ExitValue(key);
            Rect animatedRect = ScaleRect(OffsetRect(rect, Mathf.Lerp(18f, 0f, entry) + exit * 92f, 0f), 1f + hover * 0.018f + flash * 0.012f);
            float alpha = Mathf.Clamp01(entry) * (1f - exit) * currentDrawAlpha;
            if (hovered && Event.current.type == EventType.MouseDown && Event.current.button == 1 && !animationState.IsExiting(key))
            {
                Event.current.Use();
                animationState.PlayExit(key, () => RemoveSelectedRisk(risk));
            }

            Color fill = Color.Lerp(new Color(0.12f, 0.08f, 0.10f, 0.76f), new Color(0.26f, 0.16f, 0.13f, 0.86f), hover);
            fill = Color.Lerp(fill, new Color(0.58f, 0.32f, 0.12f, 0.90f), flash * 0.35f);
            if (!SandWormUiTextures.Draw(animatedRect, SandWormUiTextures.ContractBasePath + "SelectedRiskCardFrame", WithAlpha(Color.white, alpha)))
            {
                DrawCutCornerSolid(animatedRect, WithAlpha(fill, fill.a * alpha), 10f);
            }
            else if (hover > 0.01f || flash > 0.01f)
            {
                DrawCutCornerSolid(animatedRect.ContractedBy(4f), WithAlpha(fill, (0.15f + hover * 0.12f + flash * 0.20f) * alpha), 8f);
            }

            float smallHeight = Text.LineHeightOf(GameFont.Small) + 2f;
            float iconSize = 46f;
            Rect iconRect = new Rect(animatedRect.x + 10f, animatedRect.y + 10f, iconSize, iconSize);
            Color oldColor = GUI.color;
            GUI.color = WithAlpha(Color.white, alpha);
            GUI.DrawTexture(iconRect, RiskIcon(risk), ScaleMode.ScaleToFit);
            GUI.color = oldColor;

            Rect contentRect = new Rect(iconRect.xMax + 9f, animatedRect.y + 7f, animatedRect.width - iconSize - 30f, animatedRect.height - 14f);
            float tinyHeight = CalcWrappedHeight(risk.description, contentRect.width, GameFont.Tiny);
            Rect titleRect = new Rect(contentRect.x, contentRect.y, contentRect.width - 54f, smallHeight);
            Rect descriptionRect = new Rect(animatedRect.x + 9f, titleRect.yMax + 4f, animatedRect.width - 18f, tinyHeight);
            descriptionRect.x = contentRect.x;
            descriptionRect.width = contentRect.width;
            Rect badgeRect = new Rect(contentRect.xMax - 48f, contentRect.y, 46f, smallHeight);
            DrawClampedLabel(titleRect, risk.LabelCap, GameFont.Small, TextAnchor.MiddleLeft, AmberColor, alpha);
            DrawClampedLabel(badgeRect, "+" + risk.level, GameFont.Tiny, TextAnchor.MiddleCenter, CyanLineColor, alpha);
            DrawWrappedLabel(descriptionRect, risk.description, GameFont.Tiny, new Color(0.84f, 0.82f, 0.92f, 0.92f), alpha);

            if (hovered)
            {
                TooltipHandler.TipRegion(animatedRect, risk.description);
            }
        }

        // SelectedRiskEntryHeight 负责计算单个已选词条行所需的安全高度。
        private static float SelectedRiskEntryHeight(float width, SandWormChallengeRiskDef risk)
        {
            float contentWidth = Mathf.Max(80f, width - 84f);
            float wrapHeight = CalcWrappedHeight(risk.description, contentWidth, GameFont.Tiny);
            return Mathf.Max(82f, 8f + Text.LineHeightOf(GameFont.Small) + 6f + wrapHeight + 10f);
        }

        // CalcWrappedHeight 负责在指定字体和自动换行状态下测量文本高度，并恢复全局文本状态。
        private static float CalcWrappedHeight(string text, float width, GameFont font)
        {
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            try
            {
                Text.Font = font;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;
                return Text.CalcHeight(text, width);
            }
            finally
            {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
            }
        }

        // DrawFooter 负责绘制底部开始挑战按钮、选择摘要和合约等级。
        private void DrawFooter(Rect rect)
        {
            rect.x += animationState.InvalidShake * Mathf.Sin(Time.realtimeSinceStartup * 72f) * 7f;
            DrawPanel(rect, new Color(0.16f, 0.10f, 0.10f, 0.90f));
            string summary = "SandWorm_Contract_FooterSummary".Translate(selectedPawns.Count, ContractLevel());
            Rect startRect = new Rect(rect.xMax - 230f, rect.y + 17f, 205f, 38f);
            Rect summaryRect = new Rect(rect.x + 18f, rect.y + 10f, startRect.x - rect.x - 34f, Mathf.Max(Text.LineHeightOf(GameFont.Small) + 4f, rect.height - 20f));
            DrawClampedLabel(summaryRect, summary, GameFont.Small, TextAnchor.MiddleLeft, new Color(0.90f, 0.84f, 0.98f, 0.92f));

            if (DrawTerminalButton(startRect, "SandWorm_Contract_Start".Translate(), true, "button.start"))
            {
                TryStartChallengePreview();
            }
        }

        // TryStartChallengePreview 负责校验参战人员并把挑战终端交给辛迪加挑战状态组件启动真实副本。
        private void TryStartChallengePreview()
        {
            if (launchPending)
            {
                return;
            }

            if (selectedPawns.Count == 0)
            {
                Messages.Message("SandWorm_Contract_StartNeedPawn".Translate(), MessageTypeDefOf.RejectInput, historical: false);
                animationState.PlayInvalidShake();
                animationState.Flash("button.start");
                return;
            }

            SandWormSyndicateChallengeState state = Current.Game.GetComponent<SandWormSyndicateChallengeState>();
            if (state != null && state.TryStartChallenge(selectedPawns, selectedRisks, ContractLevel()))
            {
                launchPending = true;
                animationState.PlayLaunch(() => Close());
            }
        }

        // ContractLevel 负责合计当前所有已选词条的合约等级。
        private int ContractLevel()
        {
            int level = 0;
            List<SandWormChallengeRiskDef> risks = ChallengeRisks();
            for (int i = 0; i < risks.Count; i++)
            {
                if (selectedRisks.Contains(risks[i]))
                {
                    level += risks[i].level;
                }
            }

            return level;
        }

        // DrawPanelTitle 负责绘制各分区模块的标题栏。
        private static void DrawPanelTitle(Rect panelRect, string title)
        {
            Rect titleRect = new Rect(panelRect.x + 12f, panelRect.y + 10f, panelRect.width - 24f, 26f);
            DrawCutCornerSolid(titleRect, WithAlpha(new Color(0.09f, 0.06f, 0.09f, 0.70f)), 7f);
            Widgets.DrawLine(new Vector2(titleRect.x, titleRect.yMax), new Vector2(titleRect.xMax, titleRect.yMax), WithAlpha(SandGoldColor), 1.4f);
            DrawClampedLabel(titleRect.ContractedBy(8f, 0f), title, GameFont.Small, TextAnchor.MiddleLeft, AmberColor);
        }

        // DrawPanel 负责绘制带斜角边框的终端模块面板。
        private static void DrawPanel(Rect rect, Color fillColor)
        {
            DrawCutCornerSolid(rect, WithAlpha(fillColor), 16f);
            if (!SandWormUiTextures.Draw(rect, SandWormUiTextures.ContractBasePath + "MainPanelFrame", WithAlpha(Color.white, 0.88f)))
            {
                DrawCutCornerSolid(rect.ContractedBy(5f), WithAlpha(PanelInnerColor), 12f);
                DrawCutCornerBox(rect, WithAlpha(new Color(0.78f, 0.54f, 0.26f, 0.82f)), 1.2f);
                DrawCutCornerBox(rect.ContractedBy(5f), WithAlpha(new Color(0.42f, 0.32f, 0.56f, 0.55f)), 1f);
            }
        }

        // DrawEmptyState 负责在列表为空时绘制居中的空状态说明。
        private static void DrawEmptyState(Rect rect, string text)
        {
            DrawWrappedLabel(rect.ContractedBy(12f), text, GameFont.Small, new Color(0.74f, 0.70f, 0.82f, 0.86f), 1f);
        }

        // DrawScanline 负责绘制循环移动的扫描线效果。
        private static void DrawScanline(Rect rect, float speed)
        {
            float y = rect.y + Mathf.Repeat(Time.realtimeSinceStartup * 38f * speed, rect.height);
            Color color = WithAlpha(new Color(0.74f, 0.88f, 1f, 0.16f));
            Widgets.DrawLine(new Vector2(rect.x + 8f, y), new Vector2(rect.xMax - 8f, y), color, 2f);
        }

        // DrawTerminalButton 负责绘制沙金科技风按钮并返回点击状态。
        private bool DrawTerminalButton(Rect rect, string label, bool strong, string key)
        {
            bool hovered = Mouse.IsOver(rect);
            float hover = animationState.Hover(key, hovered);
            float flash = animationState.FlashValue(key);
            float buttonScale = 1f + hover * 0.018f + flash * 0.012f + (strong ? animationState.StartButtonPulse * 0.004f : 0f);
            Rect animatedRect = ScaleRect(rect, buttonScale);
            Color fill = strong ? new Color(0.45f, 0.25f, 0.08f, 0.96f) : new Color(0.18f, 0.10f, 0.12f, 0.92f);
            fill = Color.Lerp(fill, strong ? new Color(0.66f, 0.38f, 0.10f, 0.98f) : new Color(0.32f, 0.18f, 0.16f, 0.96f), hover);
            fill = Color.Lerp(fill, new Color(0.86f, 0.50f, 0.14f, 1f), flash * 0.45f);

            string buttonTexture = strong ? "ButtonPrimary" : "ButtonSecondary";
            bool drewButtonTexture = SandWormUiTextures.Draw(animatedRect, SandWormUiTextures.ContractBasePath + buttonTexture, WithAlpha(Color.white));
            if (!drewButtonTexture)
            {
                DrawCutCornerSolid(animatedRect, WithAlpha(fill), 9f);
            }
            else if (hover > 0.01f || flash > 0.01f)
            {
                DrawCutCornerSolid(animatedRect.ContractedBy(3f), WithAlpha(fill, (0.18f + hover * 0.16f + flash * 0.20f) * currentDrawAlpha), 7f);
            }

            Color border = Color.Lerp(SandGoldColor, AmberColor, Mathf.Clamp01(hover + flash + (strong ? animationState.StartButtonPulse * 0.28f : 0f)));
            if (!drewButtonTexture)
            {
                DrawCutCornerBox(animatedRect, WithAlpha(border), hover > 0.01f || flash > 0.01f ? 2f : 1f);
            }

            DrawClampedLabel(animatedRect, label, GameFont.Small, TextAnchor.MiddleCenter, Color.white);
            return Widgets.ButtonInvisible(animatedRect);
        }

        // DrawStartupSweep 负责绘制窗口打开时横跨终端的启动扫光。
        private void DrawStartupSweep(Rect rect)
        {
            float sweep = animationState.StartupSweep;
            if (sweep <= -0.2f || sweep >= 1.22f)
            {
                return;
            }

            float x = Mathf.Lerp(rect.x - 80f, rect.xMax + 40f, sweep);
            Color color = WithAlpha(CyanLineColor, 0.18f * currentDrawAlpha);
            Widgets.DrawLine(new Vector2(x, rect.y + 18f), new Vector2(x + 76f, rect.yMax - 18f), color, 3f);
            Widgets.DrawLine(new Vector2(x - 18f, rect.y + 22f), new Vector2(x + 44f, rect.yMax - 22f), WithAlpha(AmberColor, 0.11f * currentDrawAlpha), 1.5f);
        }

        // DrawLaunchOverlay 负责绘制开始挑战时的确认扫描覆盖层。
        private void DrawLaunchOverlay(Rect rect)
        {
            if (!animationState.LaunchVisible && animationState.LaunchAlpha <= 0.001f)
            {
                return;
            }

            float alpha = animationState.LaunchAlpha * currentDrawAlpha;
            Rect overlayRect = rect.ContractedBy(34f);
            Widgets.DrawBoxSolid(overlayRect, WithAlpha(new Color(0.05f, 0.03f, 0.04f, 0.86f), alpha * 0.86f));
            DrawCutCornerBox(overlayRect, WithAlpha(AmberColor, alpha), 2f);
            DrawScanline(overlayRect, 2.4f);

            float lineHeight = Text.LineHeightOf(GameFont.Medium) + 4f;
            Rect labelRect = new Rect(overlayRect.x + 36f, overlayRect.center.y - lineHeight - 12f, overlayRect.width - 72f, lineHeight);
            DrawClampedLabel(labelRect, "SandWorm_Contract_Start".Translate(), GameFont.Medium, TextAnchor.MiddleCenter, AmberColor, alpha);

            Rect progressBack = new Rect(overlayRect.x + 90f, overlayRect.center.y + 16f, overlayRect.width - 180f, 8f);
            Widgets.DrawBoxSolid(progressBack, WithAlpha(new Color(0.14f, 0.10f, 0.12f, 0.92f), alpha));
            Widgets.DrawBoxSolid(new Rect(progressBack.x, progressBack.y, progressBack.width * animationState.LaunchProgress, progressBack.height), WithAlpha(CyanLineColor, alpha));
            float sweepX = Mathf.Lerp(progressBack.x - 24f, progressBack.xMax + 16f, animationState.LaunchSweep);
            Widgets.DrawLine(new Vector2(sweepX, progressBack.y - 8f), new Vector2(sweepX + 28f, progressBack.yMax + 8f), WithAlpha(AmberColor, alpha), 2f);
        }

        // DrawClampedLabel 负责在单行安全高度内绘制文本，过长时省略并添加提示。
        private static void DrawClampedLabel(Rect rect, string text, GameFont font, TextAnchor anchor, Color color, float alpha = 1f)
        {
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            Color oldColor = GUI.color;
            try
            {
                Text.Font = font;
                Text.Anchor = anchor;
                Text.WordWrap = false;
                Rect safeRect = rect;
                safeRect.height = Mathf.Max(safeRect.height, Text.LineHeightOf(font) + 2f);
                string clamped = Text.ClampTextWithEllipsis(safeRect, text);
                GUI.color = WithAlpha(color, color.a * alpha);
                Widgets.Label(safeRect, clamped);
                if (clamped != text && Mouse.IsOver(safeRect))
                {
                    TooltipHandler.TipRegion(safeRect, text);
                }
            }
            finally
            {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
                GUI.color = oldColor;
            }
        }

        // DrawWrappedOrClampedLabel 负责在两行空间内绘制词条标题，超出时改为单行省略提示。
        private static void DrawWrappedOrClampedLabel(Rect rect, string text, GameFont font, Color color, float alpha)
        {
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            Color oldColor = GUI.color;
            try
            {
                Text.Font = font;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;
                float required = Text.CalcHeight(text, rect.width);
                GUI.color = WithAlpha(color, color.a * alpha);
                if (required <= rect.height + 0.5f)
                {
                    Widgets.Label(rect, text);
                    return;
                }

                Text.WordWrap = false;
                Rect clampedRect = rect;
                clampedRect.height = Mathf.Max(Text.LineHeightOf(font) + 2f, rect.height);
                string clamped = Text.ClampTextWithEllipsis(clampedRect, text);
                Widgets.Label(clampedRect, clamped);
                if (Mouse.IsOver(clampedRect))
                {
                    TooltipHandler.TipRegion(clampedRect, text);
                }
            }
            finally
            {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
                GUI.color = oldColor;
            }
        }

        // DrawWrappedLabel 负责按实际高度绘制多行说明文本并恢复全局文本状态。
        private static void DrawWrappedLabel(Rect rect, string text, GameFont font, Color color, float alpha)
        {
            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            bool oldWordWrap = Text.WordWrap;
            Color oldColor = GUI.color;
            try
            {
                Text.Font = font;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = true;
                GUI.color = WithAlpha(color, color.a * alpha);
                Widgets.Label(rect, text);
            }
            finally
            {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                Text.WordWrap = oldWordWrap;
                GUI.color = oldColor;
            }
        }

        // DrawGlowLine 负责绘制受动画强度控制的细光条。
        private static void DrawGlowLine(Rect rect, float strength, Color color)
        {
            float alpha = Mathf.Clamp01(strength) * currentDrawAlpha;
            if (alpha <= 0.001f)
            {
                return;
            }

            Widgets.DrawBoxSolid(rect, WithAlpha(color, color.a * alpha));
            Widgets.DrawBoxSolid(rect.ContractedBy(-1f, 1f), WithAlpha(color, color.a * alpha * 0.28f));
        }

        // OffsetRect 负责返回按像素偏移后的矩形，供动效绘制使用。
        private static Rect OffsetRect(Rect rect, float x, float y)
        {
            rect.x += x;
            rect.y += y;
            return rect;
        }

        // ScaleRect 负责以中心点缩放矩形，供节点入场和悬停动效使用。
        private static Rect ScaleRect(Rect rect, float scale)
        {
            float width = rect.width * scale;
            float height = rect.height * scale;
            return new Rect(rect.center.x - width / 2f, rect.center.y - height / 2f, width, height);
        }

        // CenteredRect 负责按中心点和尺寸生成矩形。
        private static Rect CenteredRect(Vector2 center, float width, float height)
        {
            return new Rect(center.x - width / 2f, center.y - height / 2f, width, height);
        }

        // ClampRectToWindow 负责把弹窗限制在当前窗口可见区域内。
        private Rect ClampRectToWindow(Rect rect)
        {
            Rect bounds = new Rect(OuterPadding, OuterPadding, windowRect.width - OuterPadding * 2f, windowRect.height - OuterPadding * 2f);
            if (rect.xMax > bounds.xMax)
            {
                Rect anchorRect = detailAnchorRect.width > 0f ? detailAnchorRect : hoveredRiskRect;
                rect.x = Mathf.Max(bounds.x, anchorRect.x - rect.width - 12f);
            }

            if (rect.yMax > bounds.yMax)
            {
                rect.y = bounds.yMax - rect.height;
            }

            if (rect.y < bounds.y)
            {
                rect.y = bounds.y;
            }

            return rect;
        }

        // DrawScanSweep 负责绘制详情弹窗和卡片上的柔化能量脉冲。
        private static void DrawScanSweep(Rect rect, float alpha)
        {
            alpha = Mathf.Clamp01(alpha);
            if (alpha <= 0.01f)
            {
                return;
            }

            float progress = Mathf.Repeat(Time.realtimeSinceStartup * 0.72f, 1f);
            float x = Mathf.Lerp(rect.x - rect.width * 0.18f, rect.xMax, progress);
            for (int i = 0; i < 4; i++)
            {
                float offset = i * 10f;
                float localAlpha = alpha * (0.18f - i * 0.032f);
                Widgets.DrawLine(new Vector2(x - offset, rect.y + 2f), new Vector2(x + 24f - offset, rect.yMax - 2f), WithAlpha(CyanLineColor, localAlpha), 1.2f + i * 0.35f);
            }

            for (int i = 0; i < 5; i++)
            {
                float t = Mathf.Repeat(progress + i * 0.19f, 1f);
                Vector2 point = new Vector2(Mathf.Lerp(rect.x + 10f, rect.xMax - 10f, t), rect.y + 7f + Mathf.Sin((Time.realtimeSinceStartup + i) * 3.7f) * 3f);
                DrawSolid(new Rect(point.x, point.y, 3f, 3f), WithAlpha(i % 2 == 0 ? AmberColor : CyanLineColor, alpha * 0.16f));
            }
        }

        // DrawSolid 负责绘制纯色小型特效块，并恢复 GUI.color。
        private static void DrawSolid(Rect rect, Color color)
        {
            Color oldColor = GUI.color;
            GUI.color = color;
            Widgets.DrawBoxSolid(rect, color);
            GUI.color = oldColor;
        }

        // WithAlpha 负责把当前窗口淡入透明度合成到绘制颜色中。
        private static Color WithAlpha(Color color)
        {
            return WithAlpha(color, color.a * currentDrawAlpha);
        }

        // WithAlpha 负责返回指定透明度的颜色。
        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = Mathf.Clamp01(alpha);
            return color;
        }

        // DrawCutCornerBox 负责绘制切角终端边框，避免在钝角素材上叠出锐角矩形。
        private static void DrawCutCornerBox(Rect rect, Color color, float thickness)
        {
            const float cornerLength = 16f;
            const float cut = 8f;
            Vector2 topLeftA = new Vector2(rect.x + cut, rect.y);
            Vector2 topRightA = new Vector2(rect.xMax - cut, rect.y);
            Vector2 rightTopA = new Vector2(rect.xMax, rect.y + cut);
            Vector2 rightBottomA = new Vector2(rect.xMax, rect.yMax - cut);
            Vector2 bottomRightA = new Vector2(rect.xMax - cut, rect.yMax);
            Vector2 bottomLeftA = new Vector2(rect.x + cut, rect.yMax);
            Vector2 leftBottomA = new Vector2(rect.x, rect.yMax - cut);
            Vector2 leftTopA = new Vector2(rect.x, rect.y + cut);
            Widgets.DrawLine(topLeftA, new Vector2(Mathf.Min(topLeftA.x + cornerLength, topRightA.x), rect.y), color, thickness + 0.5f);
            Widgets.DrawLine(new Vector2(Mathf.Max(topRightA.x - cornerLength, topLeftA.x), rect.y), topRightA, color, thickness + 0.5f);
            Widgets.DrawLine(topRightA, rightTopA, color, thickness + 0.5f);
            Widgets.DrawLine(rightTopA, new Vector2(rect.xMax, Mathf.Min(rightTopA.y + cornerLength, rightBottomA.y)), color, thickness + 0.5f);
            Widgets.DrawLine(new Vector2(rect.xMax, Mathf.Max(rightBottomA.y - cornerLength, rightTopA.y)), rightBottomA, color, thickness + 0.5f);
            Widgets.DrawLine(rightBottomA, bottomRightA, color, thickness + 0.5f);
            Widgets.DrawLine(bottomRightA, new Vector2(Mathf.Max(bottomRightA.x - cornerLength, bottomLeftA.x), rect.yMax), color, thickness + 0.5f);
            Widgets.DrawLine(new Vector2(Mathf.Min(bottomLeftA.x + cornerLength, bottomRightA.x), rect.yMax), bottomLeftA, color, thickness + 0.5f);
            Widgets.DrawLine(bottomLeftA, leftBottomA, color, thickness + 0.5f);
            Widgets.DrawLine(leftBottomA, new Vector2(rect.x, Mathf.Max(leftBottomA.y - cornerLength, leftTopA.y)), color, thickness + 0.5f);
            Widgets.DrawLine(new Vector2(rect.x, Mathf.Min(leftTopA.y + cornerLength, leftBottomA.y)), leftTopA, color, thickness + 0.5f);
            Widgets.DrawLine(leftTopA, topLeftA, color, thickness + 0.5f);
        }

        // DrawCutCornerSolid 负责用矩形分段模拟切角填充，避免斜角框内出现锐角底色。
        private static void DrawCutCornerSolid(Rect rect, Color color, float cut)
        {
            if (rect.width <= 0f || rect.height <= 0f)
            {
                return;
            }

            float safeCut = Mathf.Clamp(cut, 0f, Mathf.Min(rect.width, rect.height) * 0.5f);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y + safeCut, rect.width, Mathf.Max(0f, rect.height - safeCut * 2f)), color);
            Widgets.DrawBoxSolid(new Rect(rect.x + safeCut, rect.y, Mathf.Max(0f, rect.width - safeCut * 2f), safeCut), color);
            Widgets.DrawBoxSolid(new Rect(rect.x + safeCut, rect.yMax - safeCut, Mathf.Max(0f, rect.width - safeCut * 2f), safeCut), color);
        }

    }
}
