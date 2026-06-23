using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 叠层属性组配置，负责描述同一个叠层状态中某个 Stat 每层对应的加值。
    public class BattleStackStatGroup
    {
        public StatDef targetStat;
        public List<float> stackValues;
        public float valuePerStack = 0f;

        // 获取当前层数对应的 Stat 加值，负责支持指定表和值乘层数两种写法。
        public float GetValue(int stacks)
        {
            int safeStacks = Mathf.Max(1, stacks);
            if (stackValues != null && stackValues.Count > 0)
            {
                int index = Mathf.Clamp(safeStacks - 1, 0, stackValues.Count - 1);
                return stackValues[index];
            }

            return safeStacks * valuePerStack;
        }
    }

    // 叠层状态属性配置，负责声明层数、持续时间和一组或多组战斗 Stat 加值。
    public class HediffCompProperties_BattleStack : HediffCompProperties
    {
        public StatDef targetStat;
        public int maxStacks = 1;
        public bool refreshDurationOnApply = true;
        public List<float> stackValues;
        public float valuePerStack = 0f;
        public List<BattleStackStatGroup> statGroups;
        public int durationTicks = 600;
        public bool removeOnExpire = true;

        public HediffCompProperties_BattleStack()
        {
            compClass = typeof(HediffComp_BattleStack);
        }
    }

    // 叠层状态组件，负责记录当前层数、过期时间，并按层数提供多个 Stat 加值。
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
            return GetCurrentValue(Props.targetStat);
        }

        // 获取指定 Stat 的当前加值，负责支持同一个叠层 Hediff 同时修改多个属性。
        public float GetCurrentValue(StatDef statDef)
        {
            if (statDef == null)
            {
                return 0f;
            }

            float total = 0f;
            if (Props.statGroups != null)
            {
                for (int i = 0; i < Props.statGroups.Count; i++)
                {
                    BattleStackStatGroup group = Props.statGroups[i];
                    if (group?.targetStat == statDef)
                    {
                        total += group.GetValue(CurrentStacks);
                    }
                }
            }

            if (Props.targetStat == statDef)
            {
                total += GetLegacyCurrentValue();
            }

            return total;
        }

        // 判断当前叠层是否影响指定 Stat，负责让战斗属性聚合只读取相关组。
        public bool AffectsStat(StatDef statDef)
        {
            if (statDef == null)
            {
                return false;
            }

            if (Props.targetStat == statDef)
            {
                return true;
            }

            if (Props.statGroups == null)
            {
                return false;
            }

            for (int i = 0; i < Props.statGroups.Count; i++)
            {
                if (Props.statGroups[i]?.targetStat == statDef)
                {
                    return true;
                }
            }

            return false;
        }

        // 获取旧格式当前加值，负责兼容 targetStat、stackValues 和 valuePerStack 配置。
        private float GetLegacyCurrentValue()
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
