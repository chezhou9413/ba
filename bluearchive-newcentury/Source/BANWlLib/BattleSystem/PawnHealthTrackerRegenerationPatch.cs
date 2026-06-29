using HarmonyLib;
using Verse;

namespace BANWlLib.BattleSystem
{
    // Pawn 健康追踪补丁，负责在原版伤害附带 Hediff 时把施法者快照注册给再生类 Hediff。
    [HarmonyPatch(typeof(Pawn_HealthTracker), "AddHediff", typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult))]
    public static class PawnHealthTrackerRegenerationPatch
    {
        private static readonly AccessTools.FieldRef<Pawn_HealthTracker, Pawn> PawnFieldRef = AccessTools.FieldRefAccess<Pawn_HealthTracker, Pawn>("pawn");

        // 添加 Hediff 前注册快照，负责让 DamageDef.additionalHediffs 生成的持续治疗使用真正施法者属性。
        static void Prefix(Pawn_HealthTracker __instance, Hediff hediff, DamageInfo? dinfo)
        {
            Pawn targetPawn = __instance == null ? null : PawnFieldRef(__instance);
            if (targetPawn == null || hediff?.def == null || !dinfo.HasValue)
            {
                return;
            }

            BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(targetPawn, hediff.def, dinfo.Value.Instigator);
            BattleHediffSnapshotUtility.ApplySnapshotIfNeeded(hediff, dinfo.Value.Instigator);
        }
    }
}
