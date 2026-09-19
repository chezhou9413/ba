using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能效果组件，负责目标筛选、阶段按钮显隐和施法分派。
    public class CompAbilityEffect_SpecialSkill : CompAbilityEffect
    {
        public new CompProperties_SpecialSkill Props => (CompProperties_SpecialSkill)props;
        private Hediff_SpecialSkillState State => Hediff_SpecialSkillState.Find(parent.pawn, Props.profile);
        private bool IsCopy => RioSkills.IsCopied(parent);

        //按原生阶段显示对应按钮，独立复制实例保持自身默认入口可见。
        public override bool ShouldHideGizmo
        {
            get
            {
                if (IsCopy || State == null) return false;
                if (Props.command == SpecialSkillCommand.SwitchForm) return false;
                if (Props.profile.role == SpecialSkillRole.Nero)
                    return (Props.command == SpecialSkillCommand.AlternateEx) != (State.stage == 1);
                if (Props.profile.role == SpecialSkillRole.Hoshino)
                    return (Props.command == SpecialSkillCommand.AlternateEx) != (State.stage == 1);
                return Props.profile.role == SpecialSkillRole.Rio && State.copiedAbility != null;
            }
        }

        //在按钮与最终发动阶段报告不能施法的具体原因。
        public override bool GizmoDisabled(out string reason)
        {
            reason = null;
            if (IsCopy) return false;
            var s = State;
            if (s == null) { reason = "特殊技能状态尚未初始化"; return true; }
            if (ShouldHideGizmo) reason = "当前阶段不能使用此技能";
            else if (s.profile.role == SpecialSkillRole.Kei && (s.Active || s.releaseReady))
                reason = "增益场地或待释放蓄积尚未结束";
            else if (s.profile.role == SpecialSkillRole.Hoshino && s.castEndTick > s.Now)
                reason = "正在执行形态技能";
            return reason != null;
        }

        //验证自己、友军、敌人或落点，与技能配置保持一致。
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn caster = parent.pawn;
            bool valid = target.IsValid && caster.Map != null && target.Cell.InBounds(caster.Map);
            var role = Props.profile.role;
            bool ally = target.Pawn != null && !target.Pawn.Dead && target.Pawn.Map == caster.Map &&
                target.Pawn.Faction == caster.Faction;
            if (valid)
            {
                if (Props.command == SpecialSkillCommand.SwitchForm || role == SpecialSkillRole.Shiroko)
                    valid = target.Pawn == caster;
                else if (role == SpecialSkillRole.Nero && Props.command == SpecialSkillCommand.Ex)
                    valid = ally && target.Pawn != caster;
                else if (role == SpecialSkillRole.Rio)
                    valid = ally && target.Pawn != caster && RioSkills.CopyDef(target.Pawn) != null;
                else if (role == SpecialSkillRole.Arisu && target.Pawn == caster)
                    valid = IsCopy || (State != null && State.stage < 2);
                else if (role == SpecialSkillRole.Kei)
                    valid = target.Cell.Standable(caster.Map);
                else if (role == SpecialSkillRole.Hoshino)
                    valid = Props.command != SpecialSkillCommand.AlternateEx || target.Cell.Standable(caster.Map);
                else valid = target.Pawn != null && SpecialCombatUtility.ValidEnemy(caster, target.Pawn, Props.profile.range);
            }
            if (!valid && throwMessages)
                Messages.Message("当前目标不能使用此测试技能，或充能已满、目标未配置可复制EX。", MessageTypeDefOf.RejectInput, false);
            return valid;
        }

        //建立原生或复制运行状态，再执行对应角色技能。
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Hediff_SpecialSkillState state = IsCopy
                ? Hediff_SpecialSkillState.Create(parent.pawn, Props.profile, false)
                : State;
            SpecialEffects.Cast(state, target);
            SpecialSkillDispatcher.Cast(state, Props.command, target);
        }

        //显示当前状态和每个伤害段的结算模式。
        public override string ExtraTooltipPart()
        {
            var attack = Props.profile.exAttack;
            return "测试新技能\n" + (attack == null ? "" : attack.useBattleStats ? "EX伤害：BA属性结算\n" :
                "EX伤害：独立基数 " + attack.basePower + "\n") + (State?.TipStringExtra ?? "");
        }
    }
}

