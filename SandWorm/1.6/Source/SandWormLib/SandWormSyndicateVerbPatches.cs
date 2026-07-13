using HarmonyLib;
using RimWorld;
using Verse;

namespace SandWormLib
{
    // SandWormSyndicateVerbPatches 负责把挑战词条中的玩家射程惩罚限制在辛迪加挑战地图内。
    [HarmonyPatch(typeof(Verb), nameof(Verb.EffectiveRange), MethodType.Getter)]
    public static class SandWormSyndicateVerbPatches
    {
        // Postfix 负责在原版射程计算完成后，对参战小人的远程 Verb 应用挑战射程倍率。
        public static void Postfix(Verb __instance, ref float __result)
        {
            if (__instance == null || __result <= 0f || !IsRangedVerb(__instance))
            {
                return;
            }

            Pawn casterPawn = __instance.CasterPawn;
            SandWormSyndicateChallengeState state = Current.Game?.GetComponent<SandWormSyndicateChallengeState>();
            if (casterPawn == null || state == null || !state.TryGetParticipantRangeFactor(casterPawn, out float rangeFactor))
            {
                return;
            }

            __result *= rangeFactor;
        }

        // IsRangedVerb 负责排除近战和触碰类 Verb，避免射程词条影响近战攻击。
        private static bool IsRangedVerb(Verb verb)
        {
            VerbProperties props = verb.verbProps;
            if (props == null || props.IsMeleeAttack || props.range <= 1.5f)
            {
                return false;
            }

            return props.defaultProjectile != null || verb is Verb_LaunchProjectile || verb is Verb_ShootBeam || verb is Verb_Spray;
        }
    }
}
