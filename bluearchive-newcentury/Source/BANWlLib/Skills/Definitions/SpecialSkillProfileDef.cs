using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //特殊技能配置，负责集中声明角色机制、伤害段、持续时间和表现资源。
    public class SpecialSkillProfileDef : Def
    {
        public SpecialSkillRole role;
        public AbilityDef primaryAbility;
        public AbilityDef alternateAbility;
        public AbilityDef switchAbility;
        public SpecialAttackConfig exAttack;
        public SpecialAttackConfig normalAttack;
        public SpecialAttackConfig droneAttack;
        public SpecialAttackConfig burstAttack;
        public float range = 25f;
        public float radius = 3f;
        public int durationTicks = 2400;
        public int normalIntervalTicks = 1800;
        public int requiredHits = 3;
        public int maxStacks = 5;
        public float stackMultiplier = 0.35f;
        public float exMultiplier = 1.2f;
        public float lifeCostRatio = 0.2f;
        public int immortalityCooldownTicks = 5400;
        public int lethalStackLimit = 15;
        public float recordRatio = 1f;
        public float recordCapRatio = 13.22f;
        public HediffDef buffHediff;
        public HediffDef countHediff;
        public HediffDef shieldHediff;
        public ThingDef fieldThingDef;
        public float outputBaseAttack = 30f;
        public float tankBaseAttack = 10f;
        public float outputBaseHealth = 1000f;
        public float tankBaseHealth = 3000f;
        public int outputCastTicks = 60;
        public float damageTakenReduction = 0.85f;
        public int empoweredHits = 6;
        public int tankHits = 25;
        public float empoweredMultiplier = 1.5f;
        public float shieldRatio = 0.691f;
        public bool shieldUseBattleStats = true;
        public float shieldBasePower = 1000f;
        public float moveSpeed = 0.35f;
        public float interceptRadius = 3f;
        public string droneTexturePath;
        public Vector2 droneDrawSize = new Vector2(0.7f, 0.5f);
        public Vector3 droneOffset = new Vector3(0.8f, 0f, 0.5f);
        public EffecterDef casterEffecter;
        public EffecterDef targetEffecter;
        public EffecterDef stateEffecter;
        public EffecterDef stageEffecter;
        public EffecterDef endEffecter;
        public List<EffecterDef> chargeCastEffecters;
        public List<EffecterDef> chargeImpactEffecters;
        public List<EffecterDef> chargeStateEffecters;
        public SoundDef castSound;

        //检查关键机制参数，负责将错误配置明确报告到游戏日志。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors()) yield return error;
            if (primaryAbility == null) yield return defName + " 缺少 primaryAbility";
            if (durationTicks <= 0 || normalIntervalTicks <= 0 || requiredHits <= 0)
                yield return defName + " 的持续时间、普通技能间隔和命中次数必须大于零";
            if (maxStacks <= 0 || lethalStackLimit < 2 || range <= 0 || radius < 0)
                yield return defName + " 的层数、距离或范围无效";
            if (role == SpecialSkillRole.Hoshino && (shieldHediff == null || buffHediff == null))
                yield return defName + " 的星野配置缺少护盾或增益状态";
            foreach (SpecialAttackConfig action in new[] { exAttack, normalAttack, droneAttack, burstAttack })
            {
                if (action == null) continue;
                if (action.damageDef == null || action.shots <= 0 || action.shotIntervalTicks < 0)
                    yield return defName + " 的攻击段缺少伤害类型或连射参数无效";
                if (action.projectileDef != null && !typeof(Projectile_SpecialSkill).IsAssignableFrom(action.projectileDef.thingClass))
                    yield return defName + " 的 projectileDef 必须使用 Projectile_SpecialSkill";
                if (action.basePower < 0 || action.attackPowerRatio < 0)
                    yield return defName + " 的攻击段数值不能为负数";
            }
        }
    }
}
