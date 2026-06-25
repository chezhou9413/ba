using System.Collections.Generic;
using System.Text;
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
    // 通过动态写入 HediffStage.statOffsets 让原版 StatWorker 和悬浮提示显示当前加成。
    public class HediffComp_BattleStack : HediffComp
    {
        private int currentStacks = 1;
        private int ticksRemaining = -1;

        // 缓存上一次刷新层数，避免每 tick 重复构建 statOffsets。
        private int lastRefreshedStacks = -1;

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
            EnsureStageExists();
            RefreshStageStatOffsets();
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            // 层数变化后才刷新，避免每 tick 重建 statOffsets。
            if (lastRefreshedStacks != CurrentStacks)
            {
                RefreshStageStatOffsets();
            }

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
            RefreshStageStatOffsets();
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

        // 确保 HediffDef 有至少一个 stage，原版 StatWorker 和悬浮提示依赖 CurStage。
        private void EnsureStageExists()
        {
            if (parent?.def == null)
            {
                return;
            }

            if (parent.def.stages == null)
            {
                parent.def.stages = new List<HediffStage>();
            }

            if (parent.def.stages.Count == 0)
            {
                parent.def.stages.Add(new HediffStage());
            }
        }

        // 刷新当前 stage 的 statOffsets 为当前层数对应加值，让原版 StatWorker 和悬浮提示自动显示。
        private void RefreshStageStatOffsets()
        {
            if (parent?.def?.stages == null || parent.def.stages.Count == 0)
            {
                return;
            }

            HediffStage stage = parent.def.stages[0];
            if (stage.statOffsets == null)
            {
                stage.statOffsets = new List<StatModifier>();
            }
            stage.statOffsets.Clear();

            // 旧格式单属性 targetStat。
            if (Props.targetStat != null)
            {
                stage.statOffsets.Add(new StatModifier
                {
                    stat = Props.targetStat,
                    value = GetLegacyCurrentValue()
                });
            }

            // 新格式多属性组 statGroups。
            if (Props.statGroups != null)
            {
                for (int i = 0; i < Props.statGroups.Count; i++)
                {
                    BattleStackStatGroup group = Props.statGroups[i];
                    if (group?.targetStat == null)
                    {
                        continue;
                    }

                    // 同一个 Stat 可能既有旧格式又有新格式组，这里追加不覆盖。
                    stage.statOffsets.Add(new StatModifier
                    {
                        stat = group.targetStat,
                        value = group.GetValue(CurrentStacks)
                    });
                }
            }

            // 同步 label 带层数，让玩家在悬浮和信息面板看到当前叠层进度。
            UpdateStackLabel();

            lastRefreshedStacks = CurrentStacks;
        }

        // 更新 Hediff label 显示层数，负责让悬浮提示直观反映当前叠层状态。
        private void UpdateStackLabel()
        {
            if (parent?.def == null)
            {
                return;
            }

            // 首次记录原始 label，避免重复追加层数后缀。
            if (string.IsNullOrEmpty(baseLabel))
            {
                baseLabel = parent.def.label;
            }

            int max = Mathf.Max(1, Props.maxStacks);
            parent.def.label = $"{baseLabel} ({CurrentStacks}/{max})";
        }

        private string baseLabel;

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref currentStacks, "currentStacks", 1);
            Scribe_Values.Look(ref ticksRemaining, "ticksRemaining", -1);
            Scribe_Values.Look(ref baseLabel, "baseLabel", "");
        }

        // 存档读档后重建 statOffsets 和 label，避免显示旧数据。
        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            EnsureStageExists();
            RefreshStageStatOffsets();
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

