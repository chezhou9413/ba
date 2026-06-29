using BANWlLib.comp;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 原版技能施加 Hediff 拦截，负责让 CompAbilityEffect_GiveHediff 遇到叠层状态时走 BA 叠层逻辑。
    [HarmonyPatch(typeof(CompAbilityEffect_GiveHediff), "ApplyInner")]
    public static class CompAbilityEffectGiveHediffPatch
    {
        // 命中目标前置处理，负责检测叠层 Hediff 并替换原版每次新建的逻辑为叠层累加。
        // target 对应原版 ApplyInner 的第一个参数，即实际承受 Hediff 的 Pawn。
        static bool Prefix(CompAbilityEffect_GiveHediff __instance, Pawn target)
        {
            HediffDef hediffDef = __instance.Props?.hediffDef;
            if (hediffDef == null)
            {
                return true;
            }

            RegisterRegenerationSnapshot(__instance, target, hediffDef);

            // 只拦截配置了 BattleStack 组件的 HediffDef，其余走原版逻辑。
            if (hediffDef.CompProps<HediffCompProperties_BattleStack>() == null)
            {
                return true;
            }

            if (target == null)
            {
                return false;
            }

            BattleStackHediffUtility.ApplyStackedHediff(target, hediffDef);
            return false;
        }

        // 命中目标后置清理，负责避免 GiveHediff 失败时把施法者快照残留到后续治疗。
        static void Postfix(Pawn target)
        {
            HealProjectileContext.Clear(target);
        }

        // 给持续治疗 Hediff 注册施法者快照，负责让目标 tick 治疗时继续使用施法者的治愈力。
        private static void RegisterRegenerationSnapshot(CompAbilityEffect_GiveHediff effectComp, Pawn target, HediffDef hediffDef)
        {
            if (target == null)
            {
                return;
            }

            Pawn caster = effectComp?.parent?.pawn;
            if (caster == null)
            {
                Log.Error("[BANW] CompAbilityEffect_GiveHediff 缺少施法者，无法为持续治疗注册施法者快照。");
                return;
            }

            BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(target, hediffDef, caster);
        }
    }
}
