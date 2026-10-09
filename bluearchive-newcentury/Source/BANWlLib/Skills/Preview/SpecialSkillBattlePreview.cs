using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using UnityEngine;

namespace BANWlLib.Skills
{
    //预览职责：从实际特殊技能配置构造逐发公式，包含施放前可以确定的角色机制倍率。
    public static class SpecialSkillBattlePreview
    {
        //解析职责：按当前按钮操作选择真实攻击段，纯增益和形态切换不伪造伤害。
        public static List<BattleActionConfig> Resolve(Ability ability)
        {
            var props = ability.def.comps?.OfType<CompProperties_SpecialSkill>().FirstOrDefault();
            if (props == null) return null;
            var profile = props.profile;
            var state = Hediff_SpecialSkillState.Find(ability.pawn, profile);
            if (props.command == SpecialSkillCommand.SwitchForm || props.command == SpecialSkillCommand.Normal)
                return new List<BattleActionConfig>();
            switch (profile.role)
            {
                case SpecialSkillRole.Nero:
                    if (props.command != SpecialSkillCommand.AlternateEx) return new List<BattleActionConfig>();
                    float multiplier = 1f + Mathf.Min(profile.maxStacks, (state?.stacks ?? 0) + 1) * profile.stackMultiplier;
                    if (!profile.exAttack.useBattleStats) multiplier *= profile.exMultiplier;
                    return Expand(new[] { profile.exAttack }, multiplier);
                case SpecialSkillRole.Arisu:
                    return Expand(new[] { profile.exAttack }, 1f + (state?.stage ?? 0));
                case SpecialSkillRole.Hoshino:
                    if (props.command == SpecialSkillCommand.AlternateEx)
                        return new List<BattleActionConfig> { new BattleActionConfig
                        {
                            isShield = true, shieldSource = BattleShieldSource.MaxHealth,
                            shieldHediffDef = profile.shieldHediff, shieldPowerRatio = profile.shieldRatio,
                            useBattleStats = profile.shieldUseBattleStats, basePower = profile.shieldBasePower
                        } };
                    return Expand(profile.hoshino.exStages.Select(stage => stage.attack));
                case SpecialSkillRole.Wakamo:
                    return Expand(new[] { profile.exAttack });
                case SpecialSkillRole.Kei:
                    return Expand(profile.kei.FieldProperties.fieldThingDef
                        .GetModExtension<BattleFieldControllerExtension>().actions);
                default:
                    return new List<BattleActionConfig>();
            }
        }

        //展开职责：总伤害与治疗倍率按发数均分，护盾每次授予完整配置值，纯增益不显示为零伤害。
        public static List<BattleActionConfig> Expand(IEnumerable<SpecialAttackConfig> sources, float multiplier = 1f)
        {
            var actions = new List<BattleActionConfig>();
            foreach (SpecialAttackConfig source in sources)
            {
                if (source.damageDef == null && !source.isHealing && !source.isShield) continue;
                for (int i = 0; i < source.shots; i++)
                {
                    BattleActionConfig action = source.Copy();
                    action.previewMechanismMultiplier = multiplier / source.shots * (source.isNormalAttack ? source.attackPowerRatio : 1f);
                    action.healPowerRatio *= multiplier / source.shots;
                    action.shieldPowerRatio *= multiplier;
                    actions.Add(action);
                }
            }
            return actions;
        }

        //场地预览职责：直接使用场地的公共动作配置，不套用特殊技能连射分摊。
        private static List<BattleActionConfig> Expand(IEnumerable<BattleActionConfig> sources)
        {
            return sources.Where(source => source.damageDef != null || source.isHealing || source.isShield)
                .Select(source => source.Copy()).ToList();
        }
    }
}
