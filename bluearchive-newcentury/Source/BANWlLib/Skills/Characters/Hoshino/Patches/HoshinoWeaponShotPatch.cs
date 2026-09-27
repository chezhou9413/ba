using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //星野逐发射击补丁，负责替换强化弹丸并按成功发射次数累计普通技能进度。
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class HoshinoWeaponShotPatch
    {
        //强化时直接结算一次范围普攻，禁止生成原武器弹丸及其贴图。
        public static bool Prefix(Verb_LaunchProjectile __instance, ref bool __result, ref int ___lastShotTick,
            out Hediff_SpecialSkillState __state)
        {
            __state = HoshinoWeaponUtility.State(__instance);
            if (__state == null) return true;
            if (__state.castEndTick >= __state.Now) { __result = false; return false; }
            if (__state.stage != 0 || !__state.hoshino.empoweredBurst) return true;
            __result = HoshinoEmpoweredAttack.Fire(__instance, __state);
            if (__result)
            {
                ___lastShotTick = __state.Now;
                __instance.EquipmentSource.GetComp<CompChangeableProjectile>()?.Notify_ProjectileLaunched();
                __instance.EquipmentSource.GetComp<CompApparelVerbOwner_Charged>()?.UsedOnce();
            }
            return false;
        }

        //普通与强化武器每发只增加一次计数，技能弹和范围伤害事件不进入此入口。
        public static void Postfix(bool __result, Hediff_SpecialSkillState __state)
        {
            if (!__result || __state == null) return;
            if (__state.hoshino.empoweredBurst) __state.hoshino.empoweredReady = false;
            HoshinoNormalSkill.ShotFired(__state);
        }
    }
}
