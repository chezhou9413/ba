using UnityEngine;
using Verse;
using HarmonyLib;

namespace SandWormLib
{
    // SandWormMod 负责注册 Harmony 补丁、加载玩家设置并绘制模组设置界面。
    public sealed class SandWormMod : Mod
    {
        private const float SettingsViewHeight = 900f;
        private const float BaseWanderSpeed = 0.042f;
        private const float BaseChargeSpeed = 0.224f;
        private static Vector2 settingsScrollPosition;

        public static SandWormSettings Settings { get; private set; }

        // SandWormMod 负责在模组载入时初始化设置、补丁和沙虫生命值配置。
        public SandWormMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<SandWormSettings>();
            new Harmony("SandWormLib").PatchAll();
            LongEventHandler.ExecuteWhenFinished(SandWormHitPointUtility.SyncConfiguredMaxHitPoints);
            Log.Message("[SandWorm] Mod loaded.");
        }

        // SettingsCategory 负责提供 RimWorld 设置列表中显示的模组名称。
        public override string SettingsCategory()
        {
            return "SandWorm_ModName".Translate();
        }

        // DoSettingsWindowContents 负责绘制玩家可调整的沙虫难度、视觉提示和开发者设置。
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, SettingsViewHeight);
            Widgets.BeginScrollView(inRect, ref settingsScrollPosition, viewRect);

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(viewRect);

            listing.CheckboxLabeled("SandWorm_Settings_EnableWorldDestruction".Translate(), ref Settings.enableWorldDestruction);
            listing.Label("SandWorm_Settings_EnableWorldDestructionDesc".Translate());
            listing.GapLine();

            DrawMultiplierSlider(listing, "SandWorm_Settings_AttackMultiplier".Translate(), ref Settings.attackPowerMultiplier);
            DrawMultiplierSlider(listing, "SandWorm_Settings_PlayerDamageMultiplier".Translate(), ref Settings.playerDamageMultiplier);
            DrawMultiplierSlider(listing, "SandWorm_Settings_HitPointMultiplier".Translate(), ref Settings.hitPointMultiplier, "SandWorm_Settings_HitPointExtra".Translate(CurrentHitPoints()));
            DrawMultiplierSlider(listing, "SandWorm_Settings_MoveSpeedMultiplier".Translate(), ref Settings.moveSpeedMultiplier, "SandWorm_Settings_MoveSpeedExtra".Translate(CurrentWanderSpeed().ToString("0.###"), CurrentChargeSpeed().ToString("0.###")));
            listing.CheckboxLabeled("SandWorm_Settings_EnableHeadInstantKill".Translate(), ref Settings.enableHeadInstantKill);
            listing.CheckboxLabeled("SandWorm_Settings_ShowShockwaveWarningLines".Translate(), ref Settings.showShockwaveWarningLines);
            listing.Label("SandWorm_Settings_ShowShockwaveWarningLinesDesc".Translate());

            DrawDifficultyRating(listing);

            if (listing.ButtonText("SandWorm_Settings_ResetDifficulty".Translate()))
            {
                Settings.ResetDifficultyDefaults();
            }

            listing.GapLine();
            listing.CheckboxLabeled("SandWorm_Settings_ShowDeveloperSettings".Translate(), ref Settings.showDeveloperSettings);
            if (Settings.showDeveloperSettings)
            {
                DrawDeveloperSettings(listing);
            }

            listing.End();
            Widgets.EndScrollView();
            SandWormHitPointUtility.SyncConfiguredMaxHitPoints();
            base.DoSettingsWindowContents(inRect);
        }

        // DrawDeveloperSettings 负责绘制只用于调试沙虫路径、碰撞和压伤范围的设置。
        private static void DrawDeveloperSettings(Listing_Standard listing)
        {
            listing.Gap();
            listing.CheckboxLabeled("SandWorm_Settings_ShowProjectedPath".Translate(), ref Settings.showProjectedPath);
            listing.Label("SandWorm_Settings_PathRefreshTicks".Translate(Settings.projectedPathRefreshTicks));
            Settings.projectedPathRefreshTicks = Mathf.Clamp(
                Mathf.RoundToInt(listing.Slider(Settings.projectedPathRefreshTicks, 1, 60)), 1, 60);

            listing.Label("SandWorm_Settings_HitProxySyncTicks".Translate(Settings.hitProxySyncIntervalTicks));
            Settings.hitProxySyncIntervalTicks = Mathf.Clamp(
                Mathf.RoundToInt(listing.Slider(Settings.hitProxySyncIntervalTicks, 1, 30)), 1, 30);

            listing.CheckboxLabeled("SandWorm_Settings_ShowHitProxyDebugRects".Translate(), ref Settings.showHitProxyDebugRects);
            listing.Label("SandWorm_Settings_HitProxyDebugRectsDesc".Translate());

            listing.Label("SandWorm_Settings_ModelYOffset".Translate(Settings.modelYOffset.ToString("0.00")));
            Settings.modelYOffset = Mathf.Clamp(listing.Slider(Settings.modelYOffset, -5f, 5f), -5f, 5f);

            listing.CheckboxLabeled("SandWorm_Settings_ShowPressureDebugCells".Translate(), ref Settings.showPressureDebugCells);
            listing.Label("SandWorm_Settings_PressureDebugDesc".Translate());

            listing.Label("SandWorm_Settings_DamageWidthScale".Translate(Settings.damageWidthScale.ToString("P0")));
            Settings.damageWidthScale = Mathf.Clamp(listing.Slider(Settings.damageWidthScale, 0.1f, 1.5f), 0.1f, 1.5f);

            listing.Label("SandWorm_Settings_PushWidthScale".Translate(Settings.pushWidthScale.ToString("P0")));
            Settings.pushWidthScale = Mathf.Clamp(listing.Slider(Settings.pushWidthScale, 0.1f, 1.5f), 0.1f, 1.5f);

            listing.Label("SandWorm_Settings_HeadKillWidthScale".Translate(Settings.headKillWidthScale.ToString("P0")));
            Settings.headKillWidthScale = Mathf.Clamp(listing.Slider(Settings.headKillWidthScale, 0.1f, 1.5f), 0.1f, 1.5f);
        }

        // DrawMultiplierSlider 负责绘制倍率标签和滑条，并把结果限制在合法范围。
        private static void DrawMultiplierSlider(Listing_Standard listing, string label, ref float value, string extra = null)
        {
            value = Mathf.Clamp(value, SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier);
            listing.Label($"{label}：{value:0.##}x" + (extra == null ? string.Empty : $"（{extra}）"));
            value = Mathf.Clamp(listing.Slider(value, SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier), SandWormSettings.MinMultiplier, SandWormSettings.MaxMultiplier);
        }

        // CurrentHitPoints 负责根据当前设置计算沙虫最大生命值预览。
        private static int CurrentHitPoints()
        {
            return Mathf.Max(1, Mathf.RoundToInt(50000f * Settings.hitPointMultiplier));
        }

        // CurrentWanderSpeed 负责根据当前设置计算沙虫漫游速度预览。
        private static float CurrentWanderSpeed()
        {
            return BaseWanderSpeed * Settings.moveSpeedMultiplier;
        }

        // CurrentChargeSpeed 负责根据当前设置计算沙虫冲锋速度预览。
        private static float CurrentChargeSpeed()
        {
            return BaseChargeSpeed * Settings.moveSpeedMultiplier;
        }

        // DrawDifficultyRating 负责根据当前倍率设置给出粗略难度评价。
        private static void DrawDifficultyRating(Listing_Standard listing)
        {
            float playerDamageDifficulty = 1f / Mathf.Max(SandWormSettings.MinMultiplier, Settings.playerDamageMultiplier);
            float score = Settings.attackPowerMultiplier * 0.3f
                + Settings.hitPointMultiplier * 0.3f
                + Settings.moveSpeedMultiplier * 0.2f
                + playerDamageDifficulty * 0.1f;
            if (Settings.enableHeadInstantKill)
            {
                score += 0.1f;
            }

            string label;
            string color;
            if (score < 0.75f)
            {
                label = "SandWorm_Difficulty_Easy".Translate();
                color = "#7CFC00";
            }
            else if (score < 1.5f)
            {
                label = "SandWorm_Difficulty_Scared".Translate();
                color = "#FFFF66";
            }
            else if (score < 3.5f)
            {
                label = "SandWorm_Difficulty_ComeOn".Translate();
                color = "#FFA500";
            }
            else if (score < 8f)
            {
                label = "SandWorm_Difficulty_Gamble".Translate();
                color = "#FF6666";
            }
            else
            {
                label = "SandWorm_Difficulty_Terror".Translate();
                color = "#CC66FF";
            }

            listing.Label("SandWorm_Settings_DifficultyLevel".Translate(color, label));
        }
    }
}
