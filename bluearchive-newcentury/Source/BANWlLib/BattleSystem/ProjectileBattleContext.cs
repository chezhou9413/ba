using System.Collections.Generic;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 投射物战斗上下文缓存，负责把子弹发射瞬间的配置传递到命中伤害补丁。
    public static class ProjectileBattleContext
    {
        private static readonly Dictionary<int, ProjectileBattleData> ContextByProjectileId = new Dictionary<int, ProjectileBattleData>();
        private static readonly Dictionary<int, Thing> ImpactTargetByProjectileId = new Dictionary<int, Thing>();
        private static readonly Dictionary<string, ProjectileBattleData> SkillContextByInstigatorAndDamageDef = new Dictionary<string, ProjectileBattleData>();

        // 注册投射物战斗数据，负责在 Launch 后保存普通攻击和技能弹的结算参数。
        public static void Register(Projectile projectile, ProjectileBattleData data)
        {
            if (projectile == null || data == null)
            {
                return;
            }

            ContextByProjectileId[projectile.thingIDNumber] = data;
        }

        // 注册技能伤害类型上下文，负责让爆炸类投射物在实际伤害目标时也能读取技能倍率。
        public static void RegisterSkillDamage(Thing instigator, DamageDef damageDef, ProjectileBattleData data)
        {
            if (instigator == null || damageDef == null || data == null)
            {
                return;
            }

            data.expireTick = Find.TickManager.TicksGame + 600;
            SkillContextByInstigatorAndDamageDef[BuildSkillKey(instigator, damageDef)] = data;
        }

        // 读取技能伤害类型上下文，负责给爆炸和延迟命中伤害提供统一公式参数。
        public static bool TryGetSkillDamage(Thing instigator, DamageDef damageDef, out ProjectileBattleData data)
        {
            data = null;
            if (instigator == null || damageDef == null)
            {
                return false;
            }

            string key = BuildSkillKey(instigator, damageDef);
            if (!SkillContextByInstigatorAndDamageDef.TryGetValue(key, out data))
            {
                return false;
            }

            if (data.expireTick > 0 && Find.TickManager.TicksGame > data.expireTick)
            {
                SkillContextByInstigatorAndDamageDef.Remove(key);
                data = null;
                return false;
            }

            return true;
        }

        // 读取投射物战斗数据，负责给命中前伤害补丁提供统一公式参数。
        public static bool TryGet(Thing projectile, out ProjectileBattleData data)
        {
            data = null;
            if (projectile == null)
            {
                return false;
            }

            return ContextByProjectileId.TryGetValue(projectile.thingIDNumber, out data);
        }

        // 注册当前命中目标，负责让 DamageAmount 计算阶段可以读取暴击和克制目标。
        public static void RegisterImpactTarget(Projectile projectile, Thing target)
        {
            if (projectile == null || target == null)
            {
                return;
            }

            ImpactTargetByProjectileId[projectile.thingIDNumber] = target;
        }

        // 读取当前命中目标，负责让普通投射物完整进入战斗公式。
        public static bool TryGetImpactTarget(Projectile projectile, out Thing target)
        {
            target = null;
            if (projectile == null)
            {
                return false;
            }

            return ImpactTargetByProjectileId.TryGetValue(projectile.thingIDNumber, out target);
        }

        // 清理投射物战斗数据，负责避免投射物销毁后残留上下文。
        public static void Clear(Projectile projectile)
        {
            if (projectile == null)
            {
                return;
            }

            ContextByProjectileId.Remove(projectile.thingIDNumber);
            ImpactTargetByProjectileId.Remove(projectile.thingIDNumber);
        }

        // 构建技能上下文键，负责把施法者和 DamageDef 绑定到同一次技能弹配置。
        private static string BuildSkillKey(Thing instigator, DamageDef damageDef)
        {
            return instigator.thingIDNumber + ":" + damageDef.defName;
        }
    }

    // 投射物战斗数据，负责记录普通攻击或技能弹的伤害公式参数。
    public class ProjectileBattleData
    {
        public float weaponBaseAttack;
        public float attackPowerRatio;
        public float normalAttackMultiplier = 1f;
        public float baseMasteryMultiplier = 1f;
        public float shieldPowerRatio;
        public HediffDef shieldHediffDef;
        public bool isNormalAttack;
        public bool isShield;
        public bool isExSkill;
        public bool canCrit = true;
        public bool alwaysShowCriticalText = false;
        public bool applyAffinity = true;
        public bool canHitOwnBuilding = false;
        public bool canHitOwnPawn = false;
        public bool hasCustomExtension = false;
        public int expireTick;
    }
}
