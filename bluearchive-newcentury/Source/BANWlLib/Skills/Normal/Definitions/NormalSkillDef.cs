using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //配置独立自动普通技能的触发、目标筛选、战斗动作和表现资源。
    public class NormalSkillDef : Def
    {
        public NormalSkillCountMode countMode = NormalSkillCountMode.ShotFired;
        public int requiredCount = 3;
        public int cooldownTicks = 1;
        public float range = 25f;
        public bool onlyTargetAllies;
        public bool showBattleFormula = true;
        public TargetingParameters targetParams = new TargetingParameters
        {
            canTargetSelf = false, canTargetPawns = true, canTargetBuildings = false,
            canTargetLocations = false
        };
        public SoundDef castSound;
        public EffecterDef casterEffecter;
        public EffecterDef targetEffecter;
        public List<SpecialAttackConfig> actions = new List<SpecialAttackConfig>();
        public List<AbilityCompProperties> comps = new List<AbilityCompProperties>();
        private AbilityDef componentDef;

        //只给增益组件提供原版施法上下文，不注册主动按钮或加入角色能力列表。
        internal AbilityDef ComponentDef => componentDef ?? (componentDef = new AbilityDef
        {
            defName = defName + "_Components", label = label, comps = comps,
            verbProperties = new VerbProperties { verbClass = typeof(Verb_CastAbility) }
        });

        //明确报告不能执行的目标类型、触发参数和动作配置。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors()) yield return error;
            if (requiredCount <= 0 || cooldownTicks < 1 || range <= 0)
                yield return defName + " 的次数、冷却和范围必须大于零";
            if (targetParams == null || targetParams.canTargetLocations)
                yield return defName + " 必须配置实体目标筛选，自动普通技能不能自动选择空地";
            foreach (AbilityCompProperties properties in comps)
            {
                if (!(properties is CompProperties_AbilityGiveHediff give) ||
                    give.compClass != typeof(CompAbilityEffect_GiveHediff))
                { yield return defName + " 的comps仅支持CompAbilityEffect_GiveHediff"; continue; }
                if (give.hediffDef == null) yield return defName + " 的增益组件缺少hediffDef";
                else if (typeof(TimedSkillBuff).IsAssignableFrom(give.hediffDef.hediffClass))
                    yield return defName + " 的增益组件应使用原版HediffWithComps，TimedSkillBuff请放在actions中";
                if (give.hediffDef?.HasComp(typeof(HediffComp_Disappears)) == true && give.durationSecondsOverride.min <= 0f)
                    yield return defName + " 的限时增益组件必须设置正数durationSecondsOverride（秒）";
            }
            if (actions.NullOrEmpty()) { yield return defName + " 缺少actions"; yield break; }
            foreach (SpecialAttackConfig action in actions)
            {
                if (action == null) { yield return defName + " 存在空动作"; continue; }
                foreach (string error in action.TimingErrors()) yield return defName + "：" + error;
                if (action.shots <= 0 || action.shotIntervalTicks < 0 || action.radius < 0 ||
                    action.basePower < 0 || action.attackPowerRatio < 0 || action.penetration < 0 ||
                    action.healPowerRatio < 0 || action.shieldPowerRatio < 0)
                    yield return defName + " 的动作数值无效";
                if (action.isExSkill || action.isNormalAttack)
                    yield return defName + " 的普通技能动作不能标记为EX或武器普攻";
                if (action.damageDef == null && !action.isHealing && !action.isShield && action.triggerHediff == null)
                    yield return defName + " 的动作必须包含伤害、治疗、护盾或增益";
                if ((action.damageDef != null && (action.isHealing || action.isShield)) || (action.isHealing && action.isShield))
                    yield return defName + " 的伤害、治疗和护盾必须拆成不同动作，附加增益可与任一动作共用";
                if (action.projectileDef != null && !typeof(Projectile_SpecialSkill).IsAssignableFrom(action.projectileDef.thingClass))
                    yield return defName + " 的弹丸必须使用Projectile_SpecialSkill";
                if (onlyTargetAllies && (!action.affectFriendly || action.affectHostile ||
                    (action.damageDef != null && !action.canHitOwnPawn)))
                    yield return defName + " 的队友动作必须允许友军、关闭敌方作用，伤害动作还需明确允许己方伤害";
                if (action.triggerHediff != null && typeof(TimedSkillBuff).IsAssignableFrom(action.triggerHediff.hediffClass) && action.buffDurationTicks <= 0)
                    yield return defName + " 的限时增益必须配置正数buffDurationTicks";
            }
        }
    }
}
