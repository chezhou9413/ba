using BANWlLib.comp;
using Verse;

namespace BANWlLib.BattleSystem
{
    // Hediff 快照工具，负责在延后生效的再生类 Hediff 创建前统一注册施法者快照。
    public static class BattleHediffSnapshotUtility
    {
        // 按现成快照注册 Hediff 上下文，负责让后续 tick 继续使用施法者属性。
        public static void RegisterSnapshotIfNeeded(Pawn target, HediffDef hediffDef, BattleCasterSnapshot snapshot)
        {
            if (target == null || snapshot == null || hediffDef?.CompProps<HediffCompProps_Regeneration>() == null)
            {
                return;
            }

            HealProjectileContext.Register(target, snapshot);
        }

        // 向已经创建的 Hediff 写入快照，负责覆盖 DamageDef.additionalHediffs 这类创建后才带 DamageInfo 的路径。
        public static void ApplySnapshotIfNeeded(Hediff hediff, BattleCasterSnapshot snapshot)
        {
            if (hediff == null || snapshot == null)
            {
                return;
            }

            Hediff_Regeneration regeneration = hediff.TryGetComp<Hediff_Regeneration>();
            if (regeneration == null)
            {
                return;
            }

            regeneration.SetCasterSnapshot(snapshot);
        }

        // 按施法者即时属性注册 Hediff 上下文，负责给没有显式快照的入口补上施法者结算来源。
        public static void RegisterSnapshotIfNeeded(Pawn target, HediffDef hediffDef, Thing instigator)
        {
            if (target == null || hediffDef?.CompProps<HediffCompProps_Regeneration>() == null)
            {
                return;
            }

            Pawn casterPawn = instigator as Pawn;
            if (casterPawn == null)
            {
                return;
            }

            BattleCasterSnapshot snapshot = BattleStatUtility.CreateSnapshot(casterPawn);
            if (snapshot == null)
            {
                Log.Error("[BANW] 再生 Hediff 无法创建施法者快照：" + casterPawn.LabelShortCap + " -> " + hediffDef.defName);
                return;
            }

            HealProjectileContext.Register(target, snapshot);
        }

        // 按施法者即时属性写入已创建 Hediff，负责让伤害附加 Hediff 立即获得施法者属性。
        public static void ApplySnapshotIfNeeded(Hediff hediff, Thing instigator)
        {
            if (hediff == null || hediff.def?.CompProps<HediffCompProps_Regeneration>() == null)
            {
                return;
            }

            Pawn casterPawn = instigator as Pawn;
            if (casterPawn == null)
            {
                return;
            }

            BattleCasterSnapshot snapshot = BattleStatUtility.CreateSnapshot(casterPawn);
            if (snapshot == null)
            {
                Log.Error("[BANW] 再生 Hediff 无法写入施法者快照：" + casterPawn.LabelShortCap + " -> " + hediff.def.defName);
                return;
            }

            ApplySnapshotIfNeeded(hediff, snapshot);
        }
    }
}
