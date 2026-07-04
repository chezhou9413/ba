using BANWlLib.BattleSystem;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BANWlLib.BaVerb
{
    // 施法特效工具，负责让自定义范围 Verb 在施法成功时触发 EffecterDef。
    internal static class VerbCastEffecterUtility
    {
        // 触发施法特效，负责以施法者为锚点并把当前目标传给子特效计算方向。
        public static void TriggerCastEffecter(EffecterDef effecterDef, Pawn caster, LocalTargetInfo target)
        {
            if (effecterDef == null || caster?.Map == null)
            {
                return;
            }

            Effecter effecter = effecterDef.Spawn();
            TargetInfo casterTarget = new TargetInfo(caster);
            TargetInfo focusTarget = target.IsValid ? target.ToTargetInfo(caster.Map) : TargetInfo.Invalid;
            effecter.Trigger(casterTarget, focusTarget);
            effecter.Cleanup();
        }
    }

    // 圆形范围动词参数，负责声明圆形半径、执行 Job 和施法触发状态。
    public class VerbProperties_SphereArea : VerbProperties
    {
        public float Sphereradius = 3f;
        public JobDef JobDef;
        public HediffDef triggerHediff = null;
        public EffecterDef effecterDef = null;
    }

    // 圆形范围动词，负责显示目标点圆形预览并把受影响格子交给范围 Job。
    public class Verb_SphereArea : Verb_CastAbility
    {
        private VerbProperties_SphereArea Props => (VerbProperties_SphereArea)verbProps;

        private HashSet<IntVec3> affectedCellsCache = new HashSet<IntVec3>();

        // 尝试施放技能，负责触发 Ability 效果并把圆形目标队列交给 Job。
        protected override bool TryCastShot()
        {
            Pawn caster = CasterPawn;
            if (caster == null || !currentTarget.IsValid)
            {
                return false;
            }

            HashSet<IntVec3> cellsToAttack = CalculateAffectedCells(currentTarget);
            if (cellsToAttack.Count == 0)
            {
                return false;
            }

            List<LocalTargetInfo> cellTargetsList = new List<LocalTargetInfo>();
            foreach (IntVec3 cell in cellsToAttack)
            {
                cellTargetsList.Add(new LocalTargetInfo(cell));
            }

            bool castSuccess = base.TryCastShot();
            if (castSuccess)
            {
                VerbCastEffecterUtility.TriggerCastEffecter(Props.effecterDef, caster, currentTarget);

                if (Props.triggerHediff != null)
                {
                    BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(caster, Props.triggerHediff, caster);
                    Hediff hediff = HediffMaker.MakeHediff(Props.triggerHediff, caster);
                    BattleHediffSnapshotUtility.ApplySnapshotIfNeeded(hediff, caster);
                    caster.health.AddHediff(hediff);
                }

                Job job = JobMaker.MakeJob(Props.JobDef, currentTarget);
                job.SetTarget(TargetIndex.A, currentTarget);
                job.targetA = currentTarget;
                job.targetQueueA = new List<LocalTargetInfo>(cellTargetsList);
                caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            return castSuccess;
        }

        // 绘制施法高亮，负责优先使用 AbilityDef 预览配置，未配置时保留旧圆形预览。
        public override void DrawHighlight(LocalTargetInfo target)
        {
            BattleTargetPreviewUtility.DrawPreview(CasterPawn, target, ResolvePreviewData());
        }

        // 计算受影响格子，负责让实际 Job 和施法预览使用同一套圆形算法。
        private HashSet<IntVec3> CalculateAffectedCells(LocalTargetInfo target)
        {
            BattleTargetPreviewData data = ResolvePreviewData();
            HashSet<IntVec3> cells = BattleTargetPreviewUtility.CalculateCells(CasterPawn, target, data);
            affectedCellsCache = cells;
            return cells;
        }

        // 解析预览参数，负责让 AbilityDef 扩展优先覆盖旧圆形字段。
        private BattleTargetPreviewData ResolvePreviewData()
        {
            return BattleTargetPreviewUtility.ResolvePreviewData(this) ??
                   BattleTargetPreviewUtility.CreateData(AbilityTargetPreviewShape.Circle, EffectiveRange, Props.Sphereradius, 1f, EffectiveRange, 30f);
        }
    }
}
