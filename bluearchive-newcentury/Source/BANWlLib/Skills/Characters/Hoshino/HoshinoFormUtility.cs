using System.Linq;
using BANWlLib.KindStats;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //同步星野形态对应的职位特性与外观，并保留现有伤势比例。
    public static class HoshinoFormUtility
    {
        //同步本技能声明的职位与形态特性，保留角色的其他特性。
        public static void Apply(Hediff_SpecialSkillState state, float previousHealthScale)
        {
            Pawn pawn = state.pawn;
            HoshinoSkillConfig config = state.profile.hoshino;
            TraitDef desired = state.stage == 0 ? config.outputTrait : config.tankTrait;
            TraitDef removed = state.stage == 0 ? config.tankTrait : config.outputTrait;
            foreach (Trait trait in pawn.story.traits.allTraits.Where(t => t.def == removed).ToList())
                pawn.story.traits.RemoveTrait(trait);
            if (!pawn.story.traits.HasTrait(desired)) pawn.story.traits.GainTrait(new Trait(desired));
            var removedTraits = state.stage == 0 ? config.tankTraits : config.outputTraits;
            var desiredTraits = state.stage == 0 ? config.outputTraits : config.tankTraits;
            foreach (Trait trait in pawn.story.traits.allTraits.Where(t => removedTraits.Contains(t.def)).ToList())
                pawn.story.traits.RemoveTrait(trait);
            foreach (TraitDef trait in desiredTraits)
                if (!pawn.story.traits.HasTrait(trait)) pawn.story.traits.GainTrait(new Trait(trait));
            HoshinoFormApparel.Apply(pawn, config, state.stage == 1);
            HealthScaleCache.Invalidate(pawn);
            float factor = pawn.HealthScale / previousHealthScale;
            foreach (Hediff_Injury injury in pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>().ToList())
                injury.Severity *= factor;
            pawn.health.summaryHealth.Notify_HealthChanged();
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }
    }
}
