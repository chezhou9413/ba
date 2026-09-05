using BANWlLib.BattleSystem;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace BANWlLib.BaVerb
{
    // 矩形持续攻击动词参数，负责声明矩形宽度、执行 Job 和施法触发状态。
    public class VerbProperties_SustainedAreaAttackBox : VerbProperties
    {
        public int boxsize = 0;
        public JobDef JobDef;
        public HediffDef triggerHediff = null;
        public EffecterDef effecterDef = null;
    }

    // 矩形持续攻击动词，负责显示长条预览并把受影响格子交给持续攻击 Job。
    public class Verb_SustainedAreaAttackBox : Verb_CastAbility
    {
        private VerbProperties_SustainedAreaAttackBox VerbProperties => (VerbProperties_SustainedAreaAttackBox)verbProps;

        private HashSet<IntVec3> affectedCellsCache = new HashSet<IntVec3>();

        public override bool MultiSelect => true;

        // 当前有效射程，负责实时读取属性加成后的射程，避免状态或装备变化后继续使用旧缓存。
        public override float EffectiveRange
        {
            get
            {
                return base.EffectiveRange;
            }
        }

        // 计算受影响格子，负责让实际 Job 和施法预览使用同一套矩形算法。
        private HashSet<IntVec3> CalculateAffectedCells(LocalTargetInfo target)
        {
            BattleTargetPreviewData data = ResolvePreviewData();
            HashSet<IntVec3> cells = BattleTargetPreviewUtility.CalculateCells(CasterPawn, target, data);
            affectedCellsCache = cells;
            return cells;
        }

        // 绘制施法高亮，负责优先使用 AbilityDef 预览配置，未配置时保留旧矩形预览。
        public override void DrawHighlight(LocalTargetInfo target)
        {
            BattleTargetPreviewUtility.DrawPreview(CasterPawn, target, ResolvePreviewData());
        }

        // 尝试施放技能，负责触发 Ability 效果并把矩形目标队列交给 Job。
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

            Job job = JobMaker.MakeJob(VerbProperties.JobDef, currentTarget);
            job.SetTarget(TargetIndex.A, currentTarget);
            job.targetQueueA = new List<LocalTargetInfo>(cellTargetsList);
            bool castSuccess = base.TryCastShot();

            if (castSuccess)
            {
                VerbCastEffecterUtility.TriggerCastEffecter(VerbProperties.effecterDef, caster, currentTarget);

                if (VerbProperties.triggerHediff != null)
                {
                    BattleHediffSnapshotUtility.RegisterSnapshotIfNeeded(caster, VerbProperties.triggerHediff, caster);
                    Hediff hediff = HediffMaker.MakeHediff(VerbProperties.triggerHediff, caster);
                    BattleHediffSnapshotUtility.ApplySnapshotIfNeeded(hediff, caster);
                    caster.health.AddHediff(hediff);
                }

                caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            return castSuccess;
        }

        // 绘制鼠标状态，负责在非法目标上显示不可射击提示。
        public override void OnGUI(LocalTargetInfo target)
        {
            if (CanHitTarget(target) && ValidDashTarget(CasterPawn, target.Cell))
            {
                base.OnGUI(target);
            }
            else
            {
                GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
            }
        }

        // 校验目标，负责限制目标必须在技能有效射程内。
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            Pawn caster = CasterPawn;
            if (caster == null || !target.IsValid)
            {
                return false;
            }

            IntVec3 cell = target.Cell;
            if (!cell.InBounds(caster.Map))
            {
                return false;
            }

            return caster.Position.DistanceTo(cell) <= EffectiveRange;
        }

        // 判断能否命中目标，负责让施法器按有效射程判定目标合法性。
        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (!targ.IsValid)
            {
                return false;
            }

            Map map = CasterPawn.Map;
            if (!targ.Cell.InBounds(map))
            {
                return false;
            }

            return root.DistanceTo(targ.Cell) <= EffectiveRange;
        }

        // 校验移动目标，负责保留原技能对可行走和可到达格子的限制。
        private bool ValidDashTarget(Pawn pawn, IntVec3 cell)
        {
            if (!cell.Walkable(pawn.Map))
            {
                return false;
            }

            return pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly);
        }

        // 解析预览参数，负责让 AbilityDef 扩展优先覆盖旧矩形字段。
        private BattleTargetPreviewData ResolvePreviewData()
        {
            return BattleTargetPreviewUtility.ResolvePreviewData(this) ??
                   BattleTargetPreviewUtility.CreateData(AbilityTargetPreviewShape.Box, EffectiveRange, 0f, VerbProperties.boxsize * 2 + 1, EffectiveRange, 30f);
        }
    }
}
