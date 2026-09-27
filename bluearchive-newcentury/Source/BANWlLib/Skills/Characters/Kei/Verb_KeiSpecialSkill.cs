using BANWlLib.BaVerb;
using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //凯伊测试EX瞄准动词，负责显示真实来源EX场地的圆形范围。
    public class Verb_KeiSpecialSkill : Verb_CastAbility
    {
        //读取实际场地半径，使测试EX的范围预览与增益判定一致。
        public override void DrawHighlight(LocalTargetInfo target)
        {
            var config = Ability.CompOfType<CompAbilityEffect_SpecialSkill>().Props.profile.kei;
            float radius = config.FieldProperties.fieldThingDef.GetModExtension<BattleFieldControllerExtension>().radius;
            BattleTargetPreviewUtility.DrawPreview(CasterPawn, target,
                BattleTargetPreviewUtility.CreateData(AbilityTargetPreviewShape.Circle, EffectiveRange, radius, 0f, 0f, 0f));
        }
    }
}
