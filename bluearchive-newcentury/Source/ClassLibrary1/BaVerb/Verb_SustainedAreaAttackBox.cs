using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BANWlLib.BaVerb
{
    public class VerbProperties_SustainedAreaAttackBox : VerbProperties
    {
        public int boxsize = 0;
        public JobDef JobDef;
        public HediffDef TiggerHediff = null;
    }

    public class Verb_SustainedAreaAttackBox : Verb_CastAbility
    {
        private VerbProperties_SustainedAreaAttackBox VerbProperties => (VerbProperties_SustainedAreaAttackBox)this.verbProps;
        private float cachedEffectiveRange = -1f;

        private HashSet<IntVec3> affectedCellsCache = new HashSet<IntVec3>();
        public override bool MultiSelect => true;
        public override float EffectiveRange
        {
            get
            {
                if (cachedEffectiveRange < 0f)
                {
                    cachedEffectiveRange = base.EffectiveRange;
                }
                return cachedEffectiveRange;
            }
        }

        // (这个 getFanDirection 方法现在不会被 CalculateAffectedCells 调用了，
        // 但如果你的 JobDriver 可能需要它，可以保留)
        private Vector3 getFanDirection(LocalTargetInfo target)
        {
            Pawn pawn = CasterPawn;
            IntVec3 start = pawn.Position;
            Vector3 fanDirection = (target.Cell - start).ToVector3();
            return fanDirection.normalized;
        }


        private HashSet<IntVec3> CalculateAffectedCells(LocalTargetInfo target)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            Pawn pawn = CasterPawn;
            if (pawn == null || !target.IsValid) return cells;
            Map map = pawn.Map;
            IntVec3 start = pawn.Position;
            IntVec3 end = target.Cell;

            Vector3 direction = (end - start).ToVector3();
            if (direction.magnitude < 0.01f)
            {
                cells.Add(start);
                return cells;
            }
            direction.Normalize();

            float maxRange = EffectiveRange;
            int halfWidth = VerbProperties.boxsize;

            // 我们不再直接用 Bresenham，而是自己迭代距离
            for (float dist = 0; dist <= maxRange; dist += 1f)
            {
                Vector3 currentPos = start.ToVector3Shifted() + direction * dist;
                IntVec3 centerCell = currentPos.ToIntVec3();

                // 生成矩形宽度
                for (int x = -halfWidth; x <= halfWidth; x++)
                {
                    for (int z = -halfWidth; z <= halfWidth; z++)
                    {
                        IntVec3 offsetCell = new IntVec3(centerCell.x + x, 0, centerCell.z + z);
                        if (offsetCell.InBounds(map))
                        {
                            Vector3 vecToCell = (offsetCell - start).ToVector3();
                            float forwardProj = Vector3.Dot(vecToCell, direction);
                            float sideProj = Vector3.Dot(vecToCell, new Vector3(direction.z, 0, -direction.x));
                            if (forwardProj >= 0f && forwardProj <= maxRange + 0.5f &&
                                Mathf.Abs(sideProj) <= halfWidth + 0.5f)
                            {
                                cells.Add(offsetCell);
                            }
                        }
                    }
                }
            }
            return cells;
        }


        // --- 
        // --- (以下所有方法都不需要修改) ---
        // ---

        public override void DrawHighlight(LocalTargetInfo target)
        {
            // 1. 调用 *新* 函数来计算格子 (现在会返回一个正方形)
            this.affectedCellsCache = CalculateAffectedCells(target);

            // 2. 绘制这个区域的边缘 (现在会绘制正方形)
            GenDraw.DrawFieldEdges(affectedCellsCache.ToList(), new Color(0.7f, 1f, 1f));

            // 3. 绘制其他 UI (施法范围环和目标高亮)
            GenDraw.DrawRadiusRing(CasterPawn.Position, EffectiveRange, new Color(1f, 1f, 1f, 0.3f));
            GenDraw.DrawTargetHighlight(target.Cell);
        }

        protected override bool TryCastShot()
        {
            Pawn caster = CasterPawn;
            if (caster == null || !currentTarget.IsValid)
            {
                return false;
            }

            // 1. 调用 *新* 函数来计算格子 (现在会返回一个正方形)
            HashSet<IntVec3> cellsToAttack = CalculateAffectedCells(this.currentTarget);
            if (cellsToAttack.Count == 0)
            {
                return false;
            }

            List<LocalTargetInfo> cellTargetsList = new List<LocalTargetInfo>();
            foreach (IntVec3 cell in cellsToAttack)
            {
                cellTargetsList.Add(new LocalTargetInfo(cell));
            }

            // 2. JobDriver 会接收到这个正方形格子的列表
            Job job = JobMaker.MakeJob(VerbProperties.JobDef, this.currentTarget);
            job.SetTarget(TargetIndex.A, this.currentTarget);
            job.targetQueueA = new List<LocalTargetInfo>(cellTargetsList);
            // 步骤 1：先调用 Activate 并检查其返回值
            // 这会处理所有的 Comp 检查、冷却和资源消耗
            bool castSuccess = base.TryCastShot();
            if (this.Ability != null)
            {
                castSuccess = this.Ability.Activate(this.currentTarget, this.currentDestination);
            }

            // 步骤 2：只有在技能成功激活后，才分配 Job
            if (castSuccess)
            {
                if(VerbProperties.TiggerHediff != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(VerbProperties.TiggerHediff, caster);
                    caster.health.AddHediff(hediff);
                }
                caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            // 步骤 3：返回 Activate 的真实结果
            return castSuccess;
        }

        public override void OnGUI(LocalTargetInfo target)
        {
            if (CanHitTarget(target) && ValidDashTarget(CasterPawn, target.Cell))
                base.OnGUI(target);
            else
                GenUI.DrawMouseAttachment(TexCommand.CannotShoot);
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            Pawn caster = CasterPawn;
            if (caster == null || !target.IsValid)
                return false;
            IntVec3 cell = target.Cell;
            if (!cell.InBounds(caster.Map))
                return false;

            // 验证依然是检查施法者到 *目标中心点* 的距离
            float dist = caster.Position.DistanceTo(cell);
            if (dist > EffectiveRange)
                return false;

            return true;
        }

        public override bool CanHitTargetFrom(IntVec3 root, LocalTargetInfo targ)
        {
            if (!targ.IsValid) return false;
            Map map = this.CasterPawn.Map;
            if (!targ.Cell.InBounds(map)) return false;

            // 验证依然是检查施法者到 *目标中心点* 的距离
            float distance = root.DistanceTo(targ.Cell);
            if (distance > EffectiveRange)
                return false;

            return true;
        }

        private bool ValidDashTarget(Pawn pawn, IntVec3 cell)
        {
            if (!cell.Walkable(pawn.Map)) return false;
            if (!pawn.CanReach(cell, PathEndMode.OnCell, Danger.Deadly)) return false;
            return true;
        }
    }
}