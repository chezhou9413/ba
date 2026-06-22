using RimWorld;
using Verse;

namespace BANWlLib.BaVerb
{
    // 通用技能预览动词，负责给普通技能、脱手场地和直线投射物显示 Def 驱动的施法范围。
    public class Verb_CastAbilityWithTargetPreview : Verb_CastAbility
    {
        // 绘制施法高亮，负责从 AbilityDef 或关联 Comp 推导预览范围。
        public override void DrawHighlight(LocalTargetInfo target)
        {
            BattleTargetPreviewData data = BattleTargetPreviewUtility.ResolvePreviewData(this);
            if (data == null)
            {
                base.DrawHighlight(target);
                return;
            }

            BattleTargetPreviewUtility.DrawPreview(CasterPawn, target, data);
        }
    }
}
