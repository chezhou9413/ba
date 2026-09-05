using RimWorld;
using Verse;

namespace BANWlLib.Auras
{
    //光环目标过滤负责检查距离、阵营、种族、角色状态与视线。
    public static class FriendlyAuraTargetUtility
    {
        //判断一个已生成角色是否符合当前光环的受益条件。
        public static bool Accepts(HediffComp_FriendlyAura aura, Pawn target, float radius)
        {
            Pawn source = aura.Pawn;
            HediffCompProperties_FriendlyAura props = aura.Props;
            if (target.Dead || !target.Spawned || target.Map != source.Map) return false;
            if (source == target && !props.includeSelf) return false;
            if (!props.affectDowned && target.Downed) return false;
            if (!props.affectPrisoners && target.IsPrisoner) return false;
            if (!props.affectSlaves && target.IsSlave) return false;
            if (!props.allowTargetMentalState && target.InMentalState) return false;
            if (target.RaceProps.Humanlike ? !props.affectHumanlikes
                : target.RaceProps.Animal ? !props.affectAnimals
                : !target.RaceProps.IsMechanoid || !props.affectMechanoids) return false;
            if (source != target && !AcceptsFaction(source, target, props)) return false;

            float distanceSquared = (source.Position - target.Position).LengthHorizontalSquared;
            if (distanceSquared > radius * radius || distanceSquared < props.minimumRadius * props.minimumRadius)
                return false;
            if (props.requireLineOfSight && !GenSight.LineOfSight(source.Position, target.Position, source.Map))
                return false;
            if (props.requiredTargetHediffs != null)
                foreach (HediffDef required in props.requiredTargetHediffs)
                    if (!target.health.hediffSet.HasHediff(required)) return false;
            if (props.excludedTargetHediffs != null)
                foreach (HediffDef excluded in props.excludedTargetHediffs)
                    if (target.health.hediffSet.HasHediff(excluded)) return false;
            return true;
        }

        //按照同阵营、盟友和中立开关筛选，敌对与无阵营目标不受益。
        private static bool AcceptsFaction(Pawn source, Pawn target, HediffCompProperties_FriendlyAura props)
        {
            if (source.Faction == null || target.Faction == null || source.HostileTo(target)) return false;
            if (source.Faction == target.Faction) return props.affectSameFaction;
            FactionRelationKind relation = source.Faction.RelationKindWith(target.Faction);
            return relation == FactionRelationKind.Ally ? props.affectAllies
                : relation == FactionRelationKind.Neutral && props.affectNeutral;
        }
    }
}
