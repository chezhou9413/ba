using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace BANWlLib.BattleSystem
{
    // 多发投射物技能配置，负责让 AbilityDef 声明按延迟逐发发射的子弹序列。
    public class CompProperties_AbilityMultiShotProjectile : CompProperties_AbilityEffect
    {
        public ThingDef projectileDef;
        public bool canHitOwnPawn = false;
        public bool canHitOwnBuilding = false;
        public bool cancelWhenCasterMoved = false;
        public bool requireCasterCanShoot = false;
        public bool requireLineOfSightEachShot = false;
        public bool lockCasterDuringSequence = false;
        public bool drawPrimaryWeaponAimDuringSequence = false;
        public bool repeatShotSound = false;
        public List<MultiShotProjectileShotConfig> shots = new List<MultiShotProjectileShotConfig>();

        // 初始化组件类型，负责把 XML 配置绑定到多发投射物执行组件。
        public CompProperties_AbilityMultiShotProjectile()
        {
            compClass = typeof(CompAbilityEffect_MultiShotProjectile);
        }
    }

    //多发投射物单发配置，负责描述某一发子弹的延迟、投射物、倍率和发射音效。
    public class MultiShotProjectileShotConfig
    {
        public int delayTicks = 0;
        public ThingDef projectileDef;
        public float attackPowerRatio = -1f;
        public SoundDef soundDef;
    }

    // 多发投射物技能组件，负责在施法时把每发子弹注册到地图延迟队列。
    public class CompAbilityEffect_MultiShotProjectile : CompAbilityEffect
    {
        public new CompProperties_AbilityMultiShotProjectile Props
        {
            get
            {
                return (CompProperties_AbilityMultiShotProjectile)props;
            }
        }

        // 执行技能效果，负责按配置生成延迟发射任务。
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent?.pawn;
            if (caster?.Map == null)
            {
                Log.Error("[BANW] 多发投射物技能缺少施法者或地图，无法发射。");
                return;
            }

            if (Props.shots.NullOrEmpty())
            {
                Log.Error("[BANW] 多发投射物技能 " + parent.def.defName + " 缺少 shots 配置。");
                return;
            }

            SoundDef repeatedShotSound = null;
            if (Props.repeatShotSound)
            {
                repeatedShotSound = parent.verb?.verbProps?.soundCast;
                if (repeatedShotSound == null)
                {
                    Log.Error("[BANW] 多发投射物技能 " + parent.def.defName + " 开启了 repeatShotSound，但 verbProperties.soundCast 未配置。");
                }
            }

            MultiShotProjectileDelayComponent.Queue(caster, target, Props, parent.verb?.preventFriendlyFire ?? false, repeatedShotSound);
        }

        // 判断 AI 是否能选择目标，负责让测试技能优先锁定 Pawn。
        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }
    }
}
