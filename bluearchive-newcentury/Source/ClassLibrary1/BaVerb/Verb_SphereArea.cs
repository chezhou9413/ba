using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace BANWlLib.BaVerb
{
    public class VerbProperties_SphereArea : VerbProperties
    {
        // 设置一个默认值，防止 XML 忘记写时报错
        public float Sphereradius = 3f;
        public JobDef JobDef;
        public HediffDef TiggerHediff = null;
    }

    public class Verb_SphereArea : Verb_CastAbility
    {
        // 快捷访问属性的属性
        private VerbProperties_SphereArea Props => (VerbProperties_SphereArea)this.verbProps;

        private HashSet<IntVec3> affectedCellsCache = new HashSet<IntVec3>();

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
                if (Props.TiggerHediff != null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(Props.TiggerHediff, caster);
                    caster.health.AddHediff(hediff);
                }
                Job job = JobMaker.MakeJob(Props.JobDef, this.currentTarget);
                job.SetTarget(TargetIndex.A, this.currentTarget);
                job.targetA = this.currentTarget;
                job.targetQueueA = new List<LocalTargetInfo>(cellTargetsList);
                caster.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }

            // 返回 Activate 的真实结果
            return castSuccess;
        }
        public override void DrawHighlight(LocalTargetInfo target)
        {
            GenDraw.DrawRadiusRing(CasterPawn.Position, EffectiveRange, new Color(1f, 1f, 1f, 0.3f));
            if (target.IsValid && target.Cell.InBounds(CasterPawn.Map))
            {
                this.affectedCellsCache = CalculateAffectedCells(target);
                GenDraw.DrawFieldEdges(affectedCellsCache.ToList(), new Color(0.7f, 1f, 1f));
                GenDraw.DrawRadiusRing(target.Cell, Props.Sphereradius, Color.white);
                GenDraw.DrawTargetHighlight(target.Cell);
            }
        }

        private HashSet<IntVec3> CalculateAffectedCells(LocalTargetInfo target)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            Pawn pawn = CasterPawn;
            if (pawn == null || !target.IsValid) return cells;
            Map map = pawn.Map;
            float radius = Props.Sphereradius;
            float maxRange = EffectiveRange;
            IntVec3 casterPos = pawn.Position;
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(target.Cell, radius, true))
            {
                if (!cell.InBounds(map)) continue;
                if (cell.DistanceTo(casterPos) > maxRange) continue;
                cells.Add(cell);
            }
            return cells;
        }
    }
}
