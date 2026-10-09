using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //目标工具职责：统一执行类型、阵营、范围和视线筛选，并为队友技能选择最低生命比例目标。
    public static class NormalSkillTargetUtility
    {
        //筛选职责：检查实体是否合法，动作的作用对象开关也必须允许该目标。
        public static bool Valid(Pawn caster, Thing target, NormalSkillDef skill)
        {
            if (!CanAffect(caster, target, skill) || target.Map != caster.Map ||
                target.Position.DistanceTo(caster.Position) > skill.range ||
                !GenSight.LineOfSight(caster.Position, target.Position, caster.Map)) return false;
            return skill.actions.Any(action => BattleStatUtility.ShouldAffectTarget(caster, target, action));
        }

        //命中筛选职责：让单体和范围动作继续遵守类型与阵营开关，脱手弹丸不重新检查施法距离。
        public static bool CanAffect(Pawn caster, Thing target, NormalSkillDef skill)
        {
            if (target == null || target.Destroyed || !target.Spawned || (target is Pawn pawn && pawn.Dead) ||
                (target == caster && !skill.targetParams.canTargetSelf)) return false;
            if (target != caster && !skill.targetParams.CanTarget(new TargetInfo(target))) return false;
            if (skill.onlyTargetAllies)
            {
                if (!(target is Pawn) || caster.Faction == null || target.Faction != caster.Faction || target.HostileTo(caster))
                    return false;
            }
            else if (!target.HostileTo(caster)) return false;
            return true;
        }

        //选取职责：敌方优先当前攻击对象，队友按生命比例和距离排序，平局按实体编号稳定选择。
        public static Thing Find(Pawn caster, NormalSkillDef skill)
        {
            Thing current = caster.CurJob?.targetA.Thing;
            if (!skill.onlyTargetAllies && Valid(caster, current, skill)) return current;
            IEnumerable<Thing> targets = caster.Map.mapPawns.AllPawnsSpawned.Cast<Thing>();
            if (skill.targetParams.canTargetBuildings && !skill.onlyTargetAllies)
                targets = targets.Concat(caster.Map.listerThings.AllThings.OfType<Building>());
            return targets.Where(target => Valid(caster, target, skill))
                .OrderBy(target => skill.onlyTargetAllies ? ((Pawn)target).health.summaryHealth.SummaryHealthPercent : 0f)
                .ThenBy(target => target.Position.DistanceToSquared(caster.Position))
                .ThenBy(target => target.thingIDNumber).FirstOrDefault();
        }
    }
}
