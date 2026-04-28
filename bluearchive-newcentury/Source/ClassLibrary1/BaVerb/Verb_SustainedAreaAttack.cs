using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;
using static UnityEngine.GraphicsBuffer;

namespace BANWlLib.BaVerb
{
    public class VerbProperties_SustainedAreaAttack : VerbProperties
    {
        public float fanArc = 30;
        public JobDef JobDef;
        public HediffDef TiggerHediff = null;
    }

    //
    // --- 这是被修改的 Verb 类 ---
    //
    public class Verb_SustainedAreaAttack : Verb_CastAbility
    {
        private VerbProperties_SustainedAreaAttack VerbProperties => (VerbProperties_SustainedAreaAttack)this.verbProps;
        private float cachedEffectiveRange = -1f;

        // 【保留】我们仍然需要这个变量，但只是为了让 DrawHighlight 绘制它
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
            if (pawn == null) return cells;

            Map map = pawn.Map;
            if (!target.IsValid || !target.Cell.InBounds(map)) return cells;

            IntVec3 start = pawn.Position;
            float fanLength = EffectiveRange;
            float fanArc = VerbProperties.fanArc;

            Vector3 fanDirection = getFanDirection(target);
            if (fanDirection.magnitude < 0.01f)
            {
                fanDirection = Vector3.forward;
            }
            int minX = start.x - Mathf.CeilToInt(fanLength);
            int maxX = start.x + Mathf.CeilToInt(fanLength);
            int minZ = start.z - Mathf.CeilToInt(fanLength);
            int maxZ = start.z + Mathf.CeilToInt(fanLength);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    IntVec3 currentCell = new IntVec3(x, start.y, z);
                    if (!currentCell.InBounds(map) || currentCell == start)
                        continue;

                    float distance = currentCell.DistanceTo(start);
                    if (distance > fanLength)
                        continue;

                    Vector3 directionToCell = (currentCell - start).ToVector3();
                    if (directionToCell.magnitude < 0.01f)
                        continue;

                    float angleToCell = Vector3.Angle(fanDirection, directionToCell.normalized);
                    if (angleToCell <= fanArc / 2)
                    {
                        cells.Add(currentCell);
                    }
                }
            }
            return cells; // 返回新计算的列表
        }


        // --- 
        // --- 步骤 2：更新 DrawHighlight ---
        // ---
        public override void DrawHighlight(LocalTargetInfo target)
        {
            // 1. 调用新函数来计算格子
            // 并且把结果存到 cache 里，供 JobDriver（如果需要）快速访问
            this.affectedCellsCache = CalculateAffectedCells(target);

            // 2. 绘制扇形
            GenDraw.DrawFieldEdges(affectedCellsCache.ToList(), new Color(0.7f, 1f, 1f));

            // 3. 绘制其他 UI
            GenDraw.DrawRadiusRing(CasterPawn.Position, EffectiveRange, new Color(1f, 1f, 1f, 0.3f));
            GenDraw.DrawTargetHighlight(target.Cell);
        }

        // --- 
        // --- 步骤 3：更新 TryCastShot (关键！) ---
        // ---
        protected override bool TryCastShot()
        {
            Pawn caster = CasterPawn;
            if (caster == null || !currentTarget.IsValid)
            {
                return false;
            }
            HashSet<IntVec3> cellsToAttack = CalculateAffectedCells(this.currentTarget);
            if (cellsToAttack.Count == 0)
            {
                return false; // 没打中任何格子
            }
            List<LocalTargetInfo> cellTargetsList = new List<LocalTargetInfo>();
            foreach (IntVec3 cell in cellsToAttack)
            {
                cellTargetsList.Add(new LocalTargetInfo(cell));
            }
            
            // 关键修复：先调用基类的 TryCastShot，这会正确触发 Ability.Activate
            // 这样会触发所有的 CompAbilityEffect（包括 CompProperties_AbilityGiveHediff）
            bool castSuccess = base.TryCastShot();
            
            // 只有在技能成功激活后，才创建并分配自定义 Job
            if (castSuccess)
            {
                if (VerbProperties.TiggerHediff != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(VerbProperties.TiggerHediff, caster);
                    caster.health.AddHediff(hediff);
                }
                //创建并分配 Job
                Job job = JobMaker.MakeJob(VerbProperties.JobDef, this.currentTarget);
                // 使用 SetTarget 明确绑定主目标（供 JobDriver.rotateToFace 使用）
                job.SetTarget(TargetIndex.A, this.currentTarget);
                // 直接赋值目标队列（Job 没有 SetTargetQueue，手动写入即可）
                job.targetQueueA = new List<LocalTargetInfo>(cellTargetsList);
                caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            // 返回 Activate 的真实结果
            return castSuccess;
        }


        //
        // --- 你其他的函数保持不变 ---
        //
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