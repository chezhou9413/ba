using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.comp
{
    // 再生组件配置，负责定义周期治疗的基础数值、间隔和是否允许修复旧伤。
    public class HediffCompProps_Regeneration : HediffCompProperties
    {
        public float healAmount = 0.1f;
        public float healPowerRatio = 0f;
        public int healIntervalTicks = 60;
        public bool isHeatScar = true;
        public bool isExSkill = false;

        // 初始化组件类型，负责把配置绑定到实际再生组件。
        public HediffCompProps_Regeneration()
        {
            compClass = typeof(Hediff_Regeneration);
        }
    }

    // 再生组件，负责按固定间隔把 Hediff 提供的治疗交给统一治疗结算层处理。
    public class Hediff_Regeneration : HediffComp
    {
        public HediffCompProps_Regeneration Props => (HediffCompProps_Regeneration)props;

        // 周期执行治疗，负责让持续回复也能吃治疗力与受疗率规则。
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (Pawn == null || Props.healIntervalTicks <= 0 || !Pawn.IsHashIntervalTick(Props.healIntervalTicks))
            {
                return;
            }

            BattleStatUtility.ApplyHealing(new BattleHealRequest
            {
                instigator = Pawn,
                target = Pawn,
                baseAmount = Props.healAmount,
                healPowerRatio = Props.healPowerRatio,
                canCrit = false,
                allowPermanentInjuryHealing = Props.isHeatScar,
                isExSkill = Props.isExSkill
            });
        }
    }
}
