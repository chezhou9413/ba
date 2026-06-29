using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.comp
{
    // 再生组件配置，负责定义周期治疗的基础数值、间隔和是否允许修复旧伤。
    public class HediffCompProps_Regeneration : HediffCompProperties
    {
        public float healPowerRatio = 0f;
        public int healIntervalTicks = 60;
        public bool isHeatScar = true;
        public bool isExSkill = false;
        public bool alwaysShowHealText = false;

        // 初始化组件类型，负责把配置绑定到实际再生组件。
        public HediffCompProps_Regeneration()
        {
            compClass = typeof(Hediff_Regeneration);
        }
    }

    // 再生组件，负责按固定间隔把 Hediff 提供的治疗交给统一治疗结算层处理。
    // 支持两种治疗来源：自身施法（instigator=自身）和子弹治疗（通过施法者快照使用施法者属性）。
    public class Hediff_Regeneration : HediffComp
    {
        public HediffCompProps_Regeneration Props => (HediffCompProps_Regeneration)props;

        // 施法者快照，子弹治疗时由 HealProjectileContext 注入，为 null 时使用自身属性。
        private BattleCasterSnapshot casterSnapshot;

        // Hediff 创建后尝试获取施法者快照，负责让子弹附加的治疗 Hediff 使用施法者治疗力。
        public override void CompPostMake()
        {
            base.CompPostMake();
            if (HealProjectileContext.TryConsume(Pawn, out BattleCasterSnapshot snapshot))
            {
                casterSnapshot = snapshot;
            }
        }

        // 周期执行治疗，负责让持续回复也能吃治疗力与受疗率规则。
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn == null || Props.healIntervalTicks <= 0 || !Pawn.IsHashIntervalTick(Props.healIntervalTicks))
            {
                return;
            }

            // 有施法者快照时用施法者属性结算，否则用自身属性。
            Thing instigator = casterSnapshot != null ? null : Pawn;
            BattleStatUtility.ApplyHealing(new BattleHealRequest
            {
                instigator = instigator,
                target = Pawn,
                healPowerRatio = Props.healPowerRatio,
                canCrit = false,
                alwaysShowHealText = Props.alwaysShowHealText,
                allowPermanentInjuryHealing = Props.isHeatScar,
                isExSkill = Props.isExSkill,
                snapshot = casterSnapshot
            });
        }

        // 保存和读取施法者快照，负责让存档后的持续治疗保留施法者属性。
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref casterSnapshot, "casterSnapshot");
        }
    }
}
