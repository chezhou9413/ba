using Verse;

namespace BANWlLib.Skills
{
    //星野武器识别工具，负责限定形态与强化逻辑只影响角色主武器的射击动词。
    public static class HoshinoWeaponUtility
    {
        //读取当前主武器射击的原生星野状态，排除技能、近战和其他武器来源。
        public static Hediff_SpecialSkillState State(Verb verb)
        {
            if (!(verb is Verb_LaunchProjectile) || !verb.CasterIsPawn || verb.EquipmentSource == null ||
                verb.EquipmentSource != verb.CasterPawn.equipment?.Primary) return null;
            return Hediff_SpecialSkillState.Find(verb.CasterPawn, SpecialSkillRole.Hoshino);
        }
    }
}
