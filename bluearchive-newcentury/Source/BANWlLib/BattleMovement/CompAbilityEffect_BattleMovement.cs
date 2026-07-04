using System;
using System.Collections.Generic;
using BANWlLib.BattleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleMovement
{
    // 位移技能组件配置，负责把自身位移、路径伤害、落地伤害、击退和终点场地暴露给 XML。
    public class CompProperties_AbilityBattleMovement : CompProperties_AbilityEffect
    {
        public BattleMovementMode mode = BattleMovementMode.Slide;
        public float speed = 0.35f;
        public float jumpHeight = 2.5f;
        public ThingDef movementFlyerDef;
        public ThingDef jumpFlyerDef;
        public int pathWidth = 1;
        public float landingRadius = 0f;
        public EffecterDef selfEffecterDef;
        public BattleActionConfig pathAction;
        public BattleActionConfig landingAction;
        public BattleMovementKnockbackConfig knockback = new BattleMovementKnockbackConfig();
        public ThingDef fieldThingDef;
        public int durationTicksOverride = -1;

        // 初始化组件类型，负责把 XML 配置绑定到位移技能执行组件。
        public CompProperties_AbilityBattleMovement()
        {
            compClass = typeof(CompAbilityEffect_BattleMovement);
        }
    }

    // 位移技能组件，负责执行自身位移并把伤害、击退和场地交给战斗框架结算。
    public class CompAbilityEffect_BattleMovement : CompAbilityEffect
    {
        public new CompProperties_AbilityBattleMovement Props
        {
            get
            {
                return (CompProperties_AbilityBattleMovement)props;
            }
        }

        // 执行技能效果，负责解析终点并按位移模式启动移动或瞬移。
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent?.pawn;
            if (caster?.Map == null || !target.IsValid)
            {
                Log.Error("[BANW] 位移技能缺少施法者或目标，无法执行。");
                return;
            }

            Map map = caster.Map;
            IntVec3 destination = BattleMovementPathUtility.ResolveBlockedDestination(caster, target.Cell);
            if (!destination.IsValid || destination == caster.Position)
            {
                return;
            }

            Vector3 direction = BattleMovementPathUtility.Direction(caster.Position, destination);
            BattleCasterSnapshot snapshot = BattleStatUtility.CreateSnapshot(caster);
            FaceDestination(caster, destination);
            TriggerSelfEffect(caster);

            if (Props.mode == BattleMovementMode.Slide)
            {
                ApplyPathAction(caster, destination, direction, snapshot);
                StartFlyer(caster, destination, 0f, Props.speed, () => CompleteMovement(caster, destination, direction, snapshot));
                return;
            }

            if (Props.mode == BattleMovementMode.Blink)
            {
                PlacePawn(caster, destination);
                CompleteMovement(caster, destination, direction, snapshot);
                return;
            }

            if (Props.mode == BattleMovementMode.Jump)
            {
                StartFlyer(caster, destination, Props.jumpHeight, Props.speed, () => CompleteMovement(caster, destination, direction, snapshot));
                return;
            }
        }

        // 朝向目标终点，负责让冲刺、跳跃和瞬移在启动前面向目标点。
        private void FaceDestination(Pawn caster, IntVec3 destination)
        {
            IntVec3 offset = destination - caster.Position;
            if (offset.x == 0 && offset.z == 0)
            {
                return;
            }

            caster.Rotation = Rot4.FromAngleFlat(offset.ToVector3().AngleFlat());
        }

        // 启动原版飞行器位移，负责把平面冲刺和跳跃都交给 PawnFlyer 处理起飞、飞行和落地。
        private void StartFlyer(Pawn pawn, IntVec3 destination, float height, float speed, Action onCompleted)
        {
            if (pawn?.Map == null)
            {
                return;
            }

            Map map = pawn.Map;
            IntVec3 startCell = pawn.Position;
            FaceDestination(pawn, destination);
            ThingDef flyerDef = ResolveMovementFlyerDef();
            BattleMovementPawnFlyer flyer = BattleMovementPawnFlyer.MakeBattleFlyer(
                flyerDef,
                pawn,
                destination,
                height,
                speed,
                onCompleted);
            if (flyer == null)
            {
                return;
            }

            GenSpawn.Spawn(flyer, startCell, map);
        }

        // 解析位移飞行器 Def，负责允许 XML 替换默认飞行器。
        private ThingDef ResolveMovementFlyerDef()
        {
            return Props.movementFlyerDef ?? Props.jumpFlyerDef ?? DefDatabase<ThingDef>.GetNamed("BANW_BattleMovement_JumpFlyer");
        }

        // 瞬移放置 Pawn，负责停止寻路并通知原版传送状态。
        private void PlacePawn(Pawn pawn, IntVec3 cell)
        {
            if (pawn?.Map == null || !cell.IsValid)
            {
                return;
            }

            pawn.pather?.StopDead();
            pawn.Position = cell;
            pawn.Notify_Teleported(false, true);
        }

        // 播放自身原地特效，负责在位移开始时触发一次配置的 EffecterDef。
        private void TriggerSelfEffect(Pawn caster)
        {
            if (Props.selfEffecterDef == null || caster?.Map == null)
            {
                return;
            }

            Effecter effecter = Props.selfEffecterDef.Spawn();
            TargetInfo targetInfo = new TargetInfo(caster);
            effecter.Trigger(targetInfo, targetInfo);
            effecter.Cleanup();
        }

        // 处理平面冲撞路径效果，负责对路径内目标结算伤害并按冲撞方向击退。
        private void ApplyPathAction(Pawn caster, IntVec3 destination, Vector3 direction, BattleCasterSnapshot snapshot)
        {
            if (Props.pathAction == null)
            {
                return;
            }

            List<Pawn> targetPawns = new List<Pawn>(BattleMovementPathUtility.PawnsInPath(caster, destination, Props.pathWidth));
            for (int i = 0; i < targetPawns.Count; i++)
            {
                Pawn targetPawn = targetPawns[i];
                if (!BattleStatUtility.ShouldAffectTarget(caster, targetPawn, Props.pathAction))
                {
                    continue;
                }

                BattleStatUtility.ApplyAction(caster, targetPawn, Props.pathAction, snapshot);
                ApplyKnockback(caster, targetPawn, direction);
            }
        }

        // 完成位移，负责在终点结算落地范围效果、击退和场地。
        private void CompleteMovement(Pawn caster, IntVec3 destination, Vector3 movementDirection, BattleCasterSnapshot snapshot)
        {
            if (caster?.Map == null)
            {
                return;
            }

            if (Props.landingAction != null && Props.landingRadius > 0f)
            {
                List<Pawn> targetPawns = new List<Pawn>(BattleMovementPathUtility.PawnsInRadius(caster.Map, destination, Props.landingRadius));
                for (int i = 0; i < targetPawns.Count; i++)
                {
                    Pawn targetPawn = targetPawns[i];
                    if (!BattleStatUtility.ShouldAffectTarget(caster, targetPawn, Props.landingAction))
                    {
                        continue;
                    }

                    BattleStatUtility.ApplyAction(caster, targetPawn, Props.landingAction, snapshot);
                    Vector3 knockbackDirection = BattleMovementPathUtility.Direction(destination, targetPawn.Position);
                    if (knockbackDirection.sqrMagnitude < 0.0001f)
                    {
                        knockbackDirection = movementDirection;
                    }

                    ApplyKnockback(caster, targetPawn, knockbackDirection);
                }
            }

            SpawnBattleField(caster, destination);
        }

        // 执行击退，负责按阵营过滤并把目标推到阻挡前。
        private void ApplyKnockback(Pawn caster, Pawn targetPawn, Vector3 direction)
        {
            if (Props.knockback == null || !Props.knockback.enabled || Props.knockback.distance <= 0)
            {
                return;
            }

            if (!BattleMovementPathUtility.CanAffectPawn(caster, targetPawn, Props.knockback.affectHostile, Props.knockback.affectFriendly))
            {
                return;
            }

            IntVec3 destination = BattleMovementPathUtility.ResolveKnockbackDestination(targetPawn, direction, Props.knockback.distance);
            if (!destination.IsValid || destination == targetPawn.Position)
            {
                return;
            }

            StartFlyer(
                targetPawn,
                destination,
                0f,
                Props.knockback.speed,
                null);
        }

        // 生成终点脱手场地，负责复用现有 Thing_BattleFieldController。
        private void SpawnBattleField(Pawn caster, IntVec3 destination)
        {
            if (Props.fieldThingDef == null || caster?.Map == null)
            {
                return;
            }

            Thing thing = ThingMaker.MakeThing(Props.fieldThingDef);
            Thing_BattleFieldController controller = thing as Thing_BattleFieldController;
            if (controller == null)
            {
                Log.Error("[BANW] 位移技能场地 " + Props.fieldThingDef.defName + " 不是 Thing_BattleFieldController。");
                return;
            }

            GenSpawn.Spawn(controller, destination, caster.Map);
            controller.Setup(caster, Props.durationTicksOverride);
        }
    }
}
