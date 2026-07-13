using Verse;

namespace SandWormLib
{
    // SandWormSettings 负责保存沙虫模组的难度、视觉提示和开发者调试设置。
    public sealed class SandWormSettings : ModSettings
    {
        public const float MinMultiplier = 0.01f;
        public const float MaxMultiplier = 25f;

        public bool enableWorldDestruction = true;
        public float attackPowerMultiplier = 1f;
        public float playerDamageMultiplier = 1f;
        public float hitPointMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public bool enableHeadInstantKill = true;
        public bool showShockwaveWarningLines = true;

        public bool showDeveloperSettings;
        public bool showPressureDebugCells;
        public bool showHitProxyDebugRects;
        public bool showProjectedPath = false;
        public int projectedPathRefreshTicks = 20;
        public int hitProxySyncIntervalTicks = 10;
        public float damageWidthScale = 0.4f;
        public float pushWidthScale = 0.4f;
        public float headKillWidthScale = 0.5f;
        public float modelYOffset;

        // ResetDifficultyDefaults 负责把战斗难度相关设置恢复为默认值。
        public void ResetDifficultyDefaults()
        {
            attackPowerMultiplier = 1f;
            playerDamageMultiplier = 1f;
            hitPointMultiplier = 1f;
            moveSpeedMultiplier = 1f;
            enableHeadInstantKill = true;
        }

        // ExposeData 负责读写玩家设置，并为旧存档提供默认值。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableWorldDestruction, "enableWorldDestruction", true);
            Scribe_Values.Look(ref attackPowerMultiplier, "attackPowerMultiplier", 1f);
            Scribe_Values.Look(ref playerDamageMultiplier, "playerDamageMultiplier", 1f);
            Scribe_Values.Look(ref hitPointMultiplier, "hitPointMultiplier", 1f);
            Scribe_Values.Look(ref moveSpeedMultiplier, "moveSpeedMultiplier", 1f);
            Scribe_Values.Look(ref enableHeadInstantKill, "enableHeadInstantKill", true);
            Scribe_Values.Look(ref showShockwaveWarningLines, "showShockwaveWarningLines", true);
            Scribe_Values.Look(ref showDeveloperSettings, "showDeveloperSettings", false);
            Scribe_Values.Look(ref showPressureDebugCells, "showPressureDebugCells", false);
            Scribe_Values.Look(ref showHitProxyDebugRects, "showHitProxyDebugRects", false);
            Scribe_Values.Look(ref showProjectedPath, "showProjectedPath", false);
            Scribe_Values.Look(ref projectedPathRefreshTicks, "projectedPathRefreshTicks", 20);
            Scribe_Values.Look(ref hitProxySyncIntervalTicks, "hitProxySyncIntervalTicks", 10);
            Scribe_Values.Look(ref damageWidthScale, "damageWidthScale", 0.4f);
            Scribe_Values.Look(ref pushWidthScale, "pushWidthScale", 0.4f);
            Scribe_Values.Look(ref headKillWidthScale, "headKillWidthScale", 0.5f);
            Scribe_Values.Look(ref modelYOffset, "modelYOffset", 0f);
        }
    }
}
