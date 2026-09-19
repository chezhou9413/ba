using BANWlLib.BattleSystem;
using HarmonyLib;
using Verse;

namespace BANWlLib.Skills
{
    //伤害生命周期补丁，负责一次TakeDamage对应一次事件且异常时恢复上下文。
    [HarmonyPatch(typeof(Thing), nameof(Thing.TakeDamage))]
    public static class SpecialDamageEventPatch
    {
        //在原版受伤前记录本次伤害来源。
        public static void Prefix(Thing __instance, ref DamageInfo dinfo, out SpecialDamageEvent __state)
        {
            __state = SpecialSkillEvents.Begin(__instance, ref dinfo);
        }

        //在整个原版伤害处理完成后统计实际金额。
        public static void Postfix(SpecialDamageEvent __state, DamageWorker.DamageResult __result)
        {
            SpecialSkillEvents.Complete(__state, __result);
        }

        //释放本次强化攻击范围并恢复上一层伤害身份。
        public static void Finalizer(SpecialDamageEvent __state)
        {
            if (__state == null) return;
            __state.scope?.Dispose();
            SpecialSkillEvents.CurrentDamage = __state.previous;
        }
    }
}
