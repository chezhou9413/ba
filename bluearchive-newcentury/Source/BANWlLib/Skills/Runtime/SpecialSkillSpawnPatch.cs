using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //角色入图时按角色配置与实际持有的技能安装本体状态。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class SpecialSkillSpawnPatch
    {
        //生成或载入时安装缺失状态，并把已有状态注册到地图。
        public static void Postfix(Pawn __instance)
        {
            NormalSkillUtility.Install(__instance);
            var profile = __instance.kindDef?.GetModExtension<SpecialSkillKindExtension>()?.profile;
            if (profile != null) Hediff_SpecialSkillState.InstallNative(__instance, profile);
            if (__instance.abilities != null)
            {
                //只读取本体技能列表，复制实例与装备提供的临时技能不在此安装状态。
                foreach (Ability ability in __instance.abilities.abilities)
                {
                    var abilityProfile = ability.CompOfType<CompAbilityEffect_SpecialSkill>()?.Props.profile;
                    if (abilityProfile != null) Hediff_SpecialSkillState.InstallNative(__instance, abilityProfile);
                }
            }
            foreach (var state in __instance.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>())
                __instance.Map.GetComponent<MapComponent_SpecialSkills>().Register(state);
        }
    }
}
