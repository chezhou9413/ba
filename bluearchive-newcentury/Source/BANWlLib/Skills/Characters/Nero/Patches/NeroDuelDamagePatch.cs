using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //对决增伤在攻击公式完成后、闪避与护盾之前结算，覆盖普攻和技能且只乘一次。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class NeroDuelDamagePatch
    {
        //只有处于对决阶段的妮露攻击带标记的敌人时才获得伤害加成。
        [HarmonyPriority(Priority.Low)]
        public static void Prefix(Pawn __instance, ref DamageInfo dinfo)
        {
            if (dinfo.Amount <= 0f || !dinfo.Def.harmsHealth || !dinfo.Def.ExternalViolenceFor(__instance) ||
                !(dinfo.Instigator is Pawn attacker) || !__instance.HostileTo(attacker)) return;
            var state = Hediff_SpecialSkillState.Find(attacker, SpecialSkillRole.Nero);
            if (state == null || state.stage != 1 || !state.Active ||
                !Hediff_NeroDuel.ActiveOn(attacker) || !Hediff_NeroDuel.ActiveOn(__instance)) return;
            dinfo.SetAmount(dinfo.Amount * state.profile.exMultiplier);
        }
    }
}
