using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能效果组件，负责目标筛选、阶段按钮显隐和施法分派。
    public class CompAbilityEffect_SpecialSkill : CompAbilityEffect
    {
        public new CompProperties_SpecialSkill Props => (CompProperties_SpecialSkill)props;
        private Hediff_SpecialSkillState State => Hediff_SpecialSkillState.Find(parent.pawn, Props.profile);

        //原生与复制实例共用施法者本体阶段，显示对应的技能按钮。
        public override bool ShouldHideGizmo
        {
            get
            {
                if (State == null) return false;
                if (Props.command == SpecialSkillCommand.SwitchForm) return false;
                if (Props.profile.role == SpecialSkillRole.Nero)
                    return (Props.command == SpecialSkillCommand.AlternateEx) != (State.stage == 1);
                if (Props.profile.role == SpecialSkillRole.Hoshino)
                    return (Props.command == SpecialSkillCommand.AlternateEx || Props.command == SpecialSkillCommand.Normal) != (State.stage == 1);
                return Props.profile.role == SpecialSkillRole.Rio && State.copiedAbility != null;
            }
        }

        //在按钮与最终发动阶段报告不能施法的具体原因。
        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            var s = State;
            if (s == null) { reason = "特殊技能状态尚未初始化"; return true; }
            if (ShouldHideGizmo) reason = "当前阶段不能使用此技能";
            else if (s.profile.role == SpecialSkillRole.Nero && s.castEndTick >= s.Now)
                reason = "正在持续施放二段EX";
            else if (s.profile.role == SpecialSkillRole.Kei && (s.Active || s.releaseReady))
                reason = "增益场地或待释放蓄积尚未结束";
            else if (s.profile.role == SpecialSkillRole.Hoshino && s.castEndTick >= s.Now)
                reason = "正在执行形态技能";
            else if (s.profile.role == SpecialSkillRole.Arisu && s.castEndTick >= s.Now)
                reason = "正在执行直线EX";
            return reason != null;
        }

        //验证自己、友军、敌人或落点，与技能配置保持一致。
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent.pawn;
            bool valid = target.IsValid && caster.Map != null && target.Cell.InBounds(caster.Map);
            var role = Props.profile.role;
            bool ally = target.Pawn != null && !target.Pawn.Dead && target.Pawn.Map == caster.Map &&
                caster.Faction != null && target.Pawn.Faction == caster.Faction && !target.Pawn.HostileTo(caster);
            var parameters = parent.def.verbProperties.targetParams;
            if (valid && parameters != null)
                valid = target.Thing == caster ? parameters.canTargetSelf :
                    parameters.CanTarget(target.HasThing ? new TargetInfo(target.Thing) : new TargetInfo(target.Cell, caster.Map), parent.verb);
            if (Props.onlyTargetAllies && !ally) valid = false;
            if (valid)
            {
                if (Props.command == SpecialSkillCommand.SwitchForm || Props.command == SpecialSkillCommand.Normal || role == SpecialSkillRole.Shiroko)
                    valid = target.Pawn == caster;
                else if (role == SpecialSkillRole.Nero && Props.command == SpecialSkillCommand.Ex)
                    valid = ally && target.Pawn != caster;
                else if (role == SpecialSkillRole.Rio)
                    valid = ally && target.Pawn != caster && RioSkills.CopyDef(target.Pawn) != null;
                else if (role == SpecialSkillRole.Arisu && target.Pawn == caster)
                    valid = State != null && State.stage < 2;
                else if (role == SpecialSkillRole.Kei)
                    valid = target.Cell.Standable(caster.Map);
                else if (role == SpecialSkillRole.Hoshino)
                    valid = Props.command == SpecialSkillCommand.AlternateEx ? target.Cell.Standable(caster.Map)
                        : target.Pawn != null && SpecialCombatUtility.ValidEnemy(caster, target.Pawn, Props.profile.range);
                else valid = target.Pawn != null && SpecialCombatUtility.ValidEnemy(caster, target.Pawn, Props.profile.range);
            }
            if (!valid && throwMessages)
                Messages.Message("当前目标不能使用此测试技能，或充能已满、目标没有当前阶段可见的COST技能。", MessageTypeDefOf.RejectInput, false);
            return valid;
        }

        //使用实际施法者的本体状态执行角色技能，复制施法同样推进本体机制。
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Hediff_SpecialSkillState state = State;
            if (state.profile.role != SpecialSkillRole.Arisu) SpecialEffects.Cast(state, target);
            SpecialSkillDispatcher.Cast(state, Props.command, target);
        }

        //显示当前状态和每个伤害段的结算模式。
        public override string ExtraTooltipPart()
        {
            if (Props.profile.role == SpecialSkillRole.Hoshino)
                return "测试新技能\nEX使用独立伤害段；攻击普通技能按发数触发。\n" + (State?.TipStringExtra ?? "");
            var attack = Props.profile.exAttack;
            return "测试新技能\n" + (attack == null ? "" : attack.useBattleStats ? "EX伤害：BA属性结算\n" :
                "EX伤害：独立基数 " + attack.basePower + "\n") + (State?.TipStringExtra ?? "");
        }
    }
}
