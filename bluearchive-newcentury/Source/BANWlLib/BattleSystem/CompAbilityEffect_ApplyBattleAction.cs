using RimWorld;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 战斗动作技能组件配置，负责让 AbilityDef 直接执行一段统一战斗动作。
    public class CompProperties_AbilityApplyBattleAction : CompProperties_AbilityEffect
    {
        public BattleActionConfig action = new BattleActionConfig();
        public bool applyToSelf = true;

        // 初始化组件类型，负责把 XML 配置绑定到战斗动作技能组件。
        public CompProperties_AbilityApplyBattleAction()
        {
            compClass = typeof(CompAbilityEffect_ApplyBattleAction);
        }
    }

    // 战斗动作技能组件，负责把技能施法者和目标交给统一战斗结算层。
    public class CompAbilityEffect_ApplyBattleAction : CompAbilityEffect
    {
        public new CompProperties_AbilityApplyBattleAction Props
        {
            get
            {
                return (CompProperties_AbilityApplyBattleAction)props;
            }
        }

        // 执行技能效果，负责让自护盾等即时技能直接使用 BattleStatUtility。
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent?.pawn;
            Thing resolvedTarget = Props.applyToSelf ? caster : target.Thing;
            if (caster == null || resolvedTarget == null)
            {
                Log.Error("[BANW] 战斗动作技能缺少施法者或目标，无法执行。");
                return;
            }

            if (Props.action == null)
            {
                Log.Error("[BANW] 战斗动作技能缺少 action 配置，无法执行。");
                return;
            }

            BattleStatUtility.ApplyAction(caster, resolvedTarget, Props.action, BattleStatUtility.CreateSnapshot(caster));
        }
    }
}
