using BANWlLib.BaVerb;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //爱丽丝EX瞄准动词，负责让所选目标预览与实际直线伤害共用同一几何规则。
    public class Verb_ArisuSpecialSkill : Verb_CastAbility
    {
        //对自己保留充能目标高亮，对其他生物显示配置的完整直线范围。
        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (target.Thing == CasterPawn) { base.DrawHighlight(target); return; }
            var config = Ability.CompOfType<CompAbilityEffect_SpecialSkill>().Props.profile.arisu;
            BattleTargetPreviewUtility.DrawPreview(CasterPawn, target,
                BattleTargetPreviewUtility.CreateData(AbilityTargetPreviewShape.Line, EffectiveRange, 0f,
                    config.lineWidth, config.lineLength, 0f));
        }
    }
}
