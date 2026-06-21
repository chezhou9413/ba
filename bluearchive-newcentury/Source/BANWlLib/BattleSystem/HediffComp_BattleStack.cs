using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    public class HediffCompProperties_BattleStack : HediffCompProperties
    {
        public StatDef targetStat;
        public int maxStacks = 1;
        public bool refreshDurationOnApply = true;
        public List<float> stackValues;
        public float valuePerStack = 0f;
        public int durationTicks = 600;
        public bool removeOnExpire = true;

        public HediffCompProperties_BattleStack()
        {
            compClass = typeof(HediffComp_BattleStack);
        }
    }

    public class HediffComp_BattleStack : HediffComp
    {
        private int currentStacks = 1;
        private int ticksRemaining = -1;

        public HediffCompProperties_BattleStack Props
        {
            get
            {
                return (HediffCompProperties_BattleStack)props;
            }
        }

        public int CurrentStacks
        {
            get
            {
                return Mathf.Clamp(currentStacks, 1, Mathf.Max(1, Props.maxStacks));
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            currentStacks = 1;
            ticksRemaining = Props.durationTicks;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (ticksRemaining < 0)
            {
                return;
            }

            ticksRemaining--;
            if (ticksRemaining > 0)
            {
                return;
            }

            if (Props.removeOnExpire && parent?.pawn != null)
            {
                parent.pawn.health.RemoveHediff(parent);
            }
        }

        public void AddStack()
        {
            currentStacks = Mathf.Clamp(currentStacks + 1, 1, Mathf.Max(1, Props.maxStacks));
            if (Props.refreshDurationOnApply)
            {
                ticksRemaining = Props.durationTicks;
            }
        }

        public float GetCurrentValue()
        {
            if (Props.stackValues != null && Props.stackValues.Count > 0)
            {
                int index = Mathf.Clamp(CurrentStacks - 1, 0, Props.stackValues.Count - 1);
                return Props.stackValues[index];
            }

            return CurrentStacks * Props.valuePerStack;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref currentStacks, "currentStacks", 1);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
        }
    }

    public static class BattleStackHediffUtility
    {
        // 施加可叠层 Hediff，负责在已有状态时只叠层并刷新时间。
        public static Hediff ApplyStackedHediff(Pawn pawn, HediffDef hediffDef)
        {
            Hediff existing = pawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
            if (existing != null)
            {
                HediffComp_BattleStack stackComp = existing.TryGetComp<HediffComp_BattleStack>();
                if (stackComp != null)
                {
                    stackComp.AddStack();
                    return existing;
                }
            }

            Hediff hediff = HediffMaker.MakeHediff(hediffDef, pawn);
            pawn.health.AddHediff(hediff);
            return hediff;
        }
    }
}
