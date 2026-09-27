using System.Linq;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //角色技能分派器，负责把按钮、时钟和有效命中交给对应角色模块。
    public static class SpecialSkillDispatcher
    {
        //分派主动技能，不在公共战斗公式中嵌入角色分支。
        public static void Cast(Hediff_SpecialSkillState s, SpecialSkillCommand command, LocalTargetInfo target)
        {
            switch (s.profile.role)
            {
                case SpecialSkillRole.Nero: NeroSkills.Cast(s, command, target.Pawn); break;
                case SpecialSkillRole.Arisu: ArisuSkills.Cast(s, target.Pawn); break;
                case SpecialSkillRole.Shiroko: ShirokoSkills.Cast(s); break;
                case SpecialSkillRole.Wakamo: WakamoSkills.Cast(s, target.Pawn); break;
                case SpecialSkillRole.Hoshino: HoshinoSkills.Cast(s, command, target); break;
                case SpecialSkillRole.Kei: KeiSkills.Cast(s, target.Cell); break;
                case SpecialSkillRole.Rio: RioSkills.Cast(s, target.Pawn); break;
            }
        }

        //维护持续状态和到期事件，周期普通技能只对原生技能生效。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.pawn.Dead) { s.CleanupEffect(); return; }
            if (s.pawn.Spawned) s.pawn.Map.GetComponent<MapComponent_SpecialSkills>().Register(s);
            switch (s.profile.role)
            {
                case SpecialSkillRole.Nero: NeroSkills.Tick(s); break;
                case SpecialSkillRole.Arisu: ArisuSkills.Tick(s); break;
                case SpecialSkillRole.Shiroko: ShirokoSkills.Tick(s); break;
                case SpecialSkillRole.Wakamo: WakamoSkills.Tick(s); break;
                case SpecialSkillRole.Hoshino: HoshinoSkills.Tick(s); break;
                case SpecialSkillRole.Kei: KeiSkills.Tick(s); break;
                case SpecialSkillRole.Rio: RioSkills.Tick(s); break;
            }
            if (!s.native || !SpecialCombatUtility.CanAct(s.pawn) || s.Now < s.nextNormalTick) return;
            if (s.profile.role == SpecialSkillRole.Shiroko)
                TryNormal(s);
            if (s.profile.role == SpecialSkillRole.Arisu && s.hits >= s.profile.arisu.normalSkill.requiredHits)
                TryNormal(s);
        }

        //执行一个就绪普通技能，有效目标缺失时保留机会。
        public static bool TryNormal(Hediff_SpecialSkillState s)
        {
            if (!SpecialCombatUtility.CanAct(s.pawn)) return false;
            if (s.profile.role == SpecialSkillRole.Arisu) return ArisuNormalSkill.TryCast(s);
            if (s.profile.role == SpecialSkillRole.Kei) return KeiNormalSkill.TryStart(s);
            if (s.profile.role == SpecialSkillRole.Hoshino &&
                (s.stage != 0 || s.castEndTick >= s.Now || s.hits < s.profile.hoshino.normalRequiredShots)) return false;
            Pawn target = SpecialCombatUtility.FindEnemy(s.pawn, s.profile.range);
            if (target == null) return false;
            switch (s.profile.role)
            {
                case SpecialSkillRole.Shiroko:
                    ShirokoSkills.Normal(s, target); break;
                case SpecialSkillRole.Hoshino:
                    HoshinoNormalSkill.Attack(s, target); break;
                default: return false;
            }
            s.nextNormalTick = s.Now + s.profile.normalIntervalTicks;
            return true;
        }

        //处理确认造成有效伤害的普攻命中，不立即递归发动额外攻击。
        public static void NormalHit(Hediff_SpecialSkillState s, Thing target)
        {
            if (s.profile.role == SpecialSkillRole.Arisu && s.native)
            {
                s.hits++;
            }
        }

        //角色离图时清理地图实体和表现，并终止依赖当前地图的记录。
        public static void LeaveMap(Hediff_SpecialSkillState s)
        {
            s.CleanupEffect();
            s.activeMap?.GetComponent<MapComponent_SpecialSkills>().Unregister(s);
            //凯伊引用的场地属于实际EX，角色离图不能销毁场地或提前触发普通技能。
            if (s.profile.role != SpecialSkillRole.Kei)
            {
                if (s.field != null && !s.field.Destroyed) s.field.Destroy();
                s.field = null;
            }
            if (s.profile.role == SpecialSkillRole.Wakamo) WakamoSkills.Clear(s);
            if (s.profile.role == SpecialSkillRole.Hoshino && (!s.hoshino.moving || s.pawn.Dead))
                HoshinoSkills.ClearFormEffects(s);
        }

        //调试重置只清理本模块拥有的状态和技能，不碰角色原有技能。
        public static void Reset(Hediff_SpecialSkillState s)
        {
            if (s.profile.role == SpecialSkillRole.Kei && s.pawn.CurJob?.def == s.profile.kei.normalJob)
                s.pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
            s.pawn.Map?.GetComponent<MapComponent_SpecialSkills>().Cancel(s.pawn);
            foreach (var buff in s.pawn.health.hediffSet.hediffs.OfType<ExMechanismBuff>().ToList())
                s.pawn.health.RemoveHediff(buff);
            foreach (var temporary in s.pawn.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>().Where(x => !x.native).ToList())
            {
                LeaveMap(temporary);
                s.pawn.health.RemoveHediff(temporary);
            }
            LeaveMap(s);
            if (s.copiedAbility != null) RioSkills.RemoveCopy(s);
            if (s.profile.role == SpecialSkillRole.Hoshino && s.stage != 0) HoshinoSkills.Switch(s);
            s.stage = s.stacks = s.hits = s.remainingHits = 0;
            s.endTick = s.castEndTick = -1;
            s.recorded = s.recordCap = 0;
            s.releaseReady = s.forceDeath = false;
            s.field = null;
            s.keiNormalJob = null;
            s.passiveReadyTick = 0;
            s.nextNormalTick = s.Now + (s.profile.role == SpecialSkillRole.Arisu
                ? s.profile.arisu.normalSkill.cooldownTicks : s.profile.normalIntervalTicks);
            foreach (Ability ability in s.pawn.abilities.abilities)
                if (ability.def == s.profile.primaryAbility || ability.def == s.profile.alternateAbility ||
                    ability.def == s.profile.switchAbility || ability.def == s.profile.hoshino?.tankNormalAbility)
                    ability.ResetCooldown();
        }
    }
}
