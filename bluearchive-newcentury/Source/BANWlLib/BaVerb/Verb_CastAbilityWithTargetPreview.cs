using RimWorld;
using Verse;

namespace BANWlLib.BaVerb
{
    // 通用技能预览动词，负责给普通技能、脱手场地和直线投射物显示 Def 驱动的施法范围。
    public class Verb_CastAbilityWithTargetPreview : Verb_CastAbility
    {
        // 开始施法，负责在配置需要时让小人进入面向目标的射击准备姿态。
        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            bool started = base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire, nonInterruptingSelfCast);
            if (started && ShouldDrawPrimaryWeaponAim())
            {
                CasterPawn?.pather?.StopDead();
                CasterPawn?.rotationTracker?.FaceTarget(castTarg);
            }

            return started;
        }

        // 判断是否启用主武器瞄准表现，负责从技能预览扩展读取视觉开关。
        public bool ShouldDrawPrimaryWeaponAim()
        {
            return Ability?.def?.GetModExtension<AbilityTargetPreviewExtension>()?.drawPrimaryWeaponAim == true;
        }

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
