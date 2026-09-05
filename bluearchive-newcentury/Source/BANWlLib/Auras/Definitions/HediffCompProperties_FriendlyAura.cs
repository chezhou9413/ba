using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.Auras
{
    //友军光环配置负责声明覆盖范围、发射条件、目标过滤和受益强度。
    public sealed class HediffCompProperties_FriendlyAura : HediffCompProperties
    {
        public HediffDef effectHediff;
        public float radius = 8f;
        public float minimumRadius;
        public StatDef radiusMultiplierStat;
        public float severity = 1f;
        public bool multiplyBySourceSeverity;
        public StatDef severityMultiplierStat;
        public float minimumSourceSeverity;
        public bool includeSelf = true;
        public bool affectSameFaction = true;
        public bool affectAllies = true;
        public bool affectNeutral;
        public bool affectHumanlikes = true;
        public bool affectAnimals;
        public bool affectMechanoids = true;
        public bool affectDowned = true;
        public bool affectPrisoners;
        public bool affectSlaves = true;
        public bool requireLineOfSight;
        public bool requireSourceDrafted;
        public bool requireSourceAwake;
        public bool allowSourceDowned;
        public bool allowSourceMentalState;
        public bool allowTargetMentalState;
        public List<HediffDef> requiredTargetHediffs;
        public List<HediffDef> excludedTargetHediffs;

        //绑定光环发射组件。
        public HediffCompProperties_FriendlyAura()
        {
            compClass = typeof(HediffComp_FriendlyAura);
        }

        //在定义加载时报告不合法范围或不受管理的受益状态。
        public override IEnumerable<string> ConfigErrors(HediffDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef)) yield return error;
            if (radius < 0f || minimumRadius < 0f || minimumRadius > radius)
                yield return parentDef.defName + "：光环半径必须满足0≤minimumRadius≤radius。";
            if (severity <= 0f || minimumSourceSeverity < 0f)
                yield return parentDef.defName + "：光环强度必须大于0，源状态阈值不能为负。";
            if (effectHediff == null || effectHediff.hediffClass != typeof(Hediff_FriendlyAuraEffect))
                yield return parentDef.defName + "：effectHediff必须使用BANWlLib.Auras.Hediff_FriendlyAuraEffect。";
            if (effectHediff == parentDef)
                yield return parentDef.defName + "：光环发射状态不能同时作为自身受益状态。";
            if (parentDef.hediffClass == typeof(Hediff_FriendlyAuraEffect))
                yield return parentDef.defName + "：受益状态不能继续发射光环。";
        }
    }
}
