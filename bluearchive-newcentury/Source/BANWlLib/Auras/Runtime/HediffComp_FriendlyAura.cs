using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Auras
{
    //光环发射组件负责提供有效半径、强度和当前发射条件，扫描由地图组件统一执行。
    public sealed class HediffComp_FriendlyAura : HediffComp
    {
        public HediffCompProperties_FriendlyAura Props => (HediffCompProperties_FriendlyAura)props;
        public float Radius => Props.radius * GetMultiplier(Props.radiusMultiplierStat);
        public float Strength => Props.severity * (Props.multiplyBySourceSeverity ? parent.Severity : 1f)
            * GetMultiplier(Props.severityMultiplierStat);

        //判断源角色是否满足发射要求。
        public bool IsActive()
        {
            return Pawn.Spawned && !Pawn.Dead && parent.Severity >= Props.minimumSourceSeverity
                && (Props.allowSourceDowned || !Pawn.Downed)
                && (Props.allowSourceMentalState || !Pawn.InMentalState)
                && (!Props.requireSourceDrafted || Pawn.Drafted)
                && (!Props.requireSourceAwake || Pawn.Awake());
        }

        //读取可选倍率属性，未指定属性时使用单位倍率。
        private float GetMultiplier(StatDef stat)
        {
            return stat == null ? 1f : Mathf.Max(0f, Pawn.GetStatValue(stat));
        }

        public override string CompTipStringExtra => "友军光环半径：" + Radius.ToString("0.##")
            + "格\n受益状态：" + Props.effectHediff.LabelCap + "\n光环强度：" + Strength.ToString("0.##");
    }
}
