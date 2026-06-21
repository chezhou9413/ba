using BANWlLib.BattleSystem;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    // 飞行穿透范围抛射体，负责沿路径周期判定并把伤害交给统一战斗层处理。
    public class Projectile_PiercingArea : Projectile
    {
        private readonly Dictionary<int, int> lastDamageTicksByThingId = new Dictionary<int, int>();
        private int ticksUntilDamage;

        // 抛射体更新频率，负责让穿透伤害稳定按 tick 运行。
        public override int UpdateRateTicks => 1;

        // 穿透弹配置，负责从 ThingDef 扩展中读取参数。
        private PiercingProjectileExtension Extension => def.GetModExtension<PiercingProjectileExtension>();

        // 保存飞行状态，负责让存档读档后继续按配置造成范围伤害。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksUntilDamage, "ticksUntilDamage", 0);
        }

        // 飞行 tick，负责替代原版命中即销毁逻辑并执行穿透范围伤害。
        protected override void TickInterval(int delta)
        {
            lifetime -= delta;
            if (landed)
            {
                return;
            }

            ticksToImpact -= delta;
            if (!ExactPosition.InBounds(Map))
            {
                ticksToImpact += delta;
                Position = ExactPosition.ToIntVec3();
                Destroy();
                return;
            }

            Vector3 newExactPosition = ExactPosition;
            Position = newExactPosition.ToIntVec3();
            TickDamage(delta);
            if (ticksToImpact <= 0)
            {
                if (DestinationCell.InBounds(Map))
                {
                    Position = DestinationCell;
                }

                DamagePawnsInArea();
                Destroy();
            }
        }

        // 绘制抛射体，负责让贴图按飞行进度缩放并淡入淡出。
        protected override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            Quaternion rotation = ExactRotation;
            if (def.projectile.spinRate != 0f)
            {
                float spinTicks = 60f / def.projectile.spinRate;
                rotation = Quaternion.AngleAxis((float)Find.TickManager.TicksGame % spinTicks / spinTicks * 360f, Vector3.up);
            }

            Vector2 drawSize = def.graphicData.drawSize * GetDrawScale();
            Material material = DrawMat;
            float alpha = GetDrawAlpha();
            if (alpha < 0.999f)
            {
                material = FadedMaterialPool.FadedVersionOf(material, alpha);
            }

            Graphics.DrawMesh(MeshPool.GridPlane(drawSize), drawLoc, rotation, material, 0);

            Comps_PostDraw();
        }

        // 处理周期伤害，负责按配置 tick 间隔触发一次范围判定。
        private void TickDamage(int delta)
        {
            ticksUntilDamage -= delta;
            if (ticksUntilDamage > 0)
            {
                return;
            }

            DamagePawnsInArea();
            ticksUntilDamage = DamageIntervalTicks();
        }

        // 对范围内目标造成伤害，负责同时处理 Pawn 和墙体等建筑目标。
        private void DamagePawnsInArea()
        {
            if (Map == null)
            {
                return;
            }

            foreach (IntVec3 cell in DamageCells())
            {
                List<Thing> thingList = cell.GetThingList(Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    Pawn pawn = thingList[i] as Pawn;
                    if (pawn == null || !CanDamagePawn(pawn))
                    {
                        continue;
                    }

                    ApplyDamageToThing(pawn);
                }

                Building edifice = cell.GetEdifice(Map);
                if (edifice == null || !CanDamageBuilding(edifice))
                {
                    continue;
                }

                ApplyDamageToThing(edifice);
            }
        }

        // 判断 Pawn 是否可伤害，负责跳过发射者、死亡目标、友军和冷却中的重复目标。
        private bool CanDamagePawn(Pawn pawn)
        {
            if (pawn == launcher || pawn.Dead)
            {
                return false;
            }

            if (!BattleStatUtility.ShouldAffectTarget(launcher as Pawn, pawn, BuildBattleAction()))
            {
                return false;
            }

            if (IsFriendlyFireImmune() && IsProtectedFriendlyPawn(pawn))
            {
                return false;
            }

            int currentTick = Find.TickManager.TicksGame;
            int lastDamageTick;
            if (lastDamageTicksByThingId.TryGetValue(pawn.thingIDNumber, out lastDamageTick) && currentTick - lastDamageTick < DamageIntervalTicks())
            {
                return false;
            }

            lastDamageTicksByThingId[pawn.thingIDNumber] = currentTick;
            return true;
        }

        // 判断建筑是否可伤害，负责让穿透弹命中墙体后继续飞行，并避免在冷却间隔内重复击中同一建筑。
        private bool CanDamageBuilding(Building building)
        {
            if (building.Destroyed || building == launcher || !CanHitBuilding(building) || !BuildBattleAction().canHitBuilding)
            {
                return false;
            }

            if (IsFriendlyFireImmune() && IsProtectedFriendlyThing(building))
            {
                return false;
            }

            int currentTick = Find.TickManager.TicksGame;
            int lastDamageTick;
            if (lastDamageTicksByThingId.TryGetValue(building.thingIDNumber, out lastDamageTick) && currentTick - lastDamageTick < DamageIntervalTicks())
            {
                return false;
            }

            lastDamageTicksByThingId[building.thingIDNumber] = currentTick;
            return true;
        }

        // 判断目标是否属于友军保护范围，负责只保护同阵营和盟友，不保护中立目标。
        private bool IsProtectedFriendlyPawn(Pawn pawn)
        {
            if (launcher == null || launcher.Faction == null || pawn.Faction == null)
            {
                return false;
            }

            if (pawn.Faction == launcher.Faction)
            {
                return true;
            }

            return launcher.Faction.RelationKindWith(pawn.Faction) == FactionRelationKind.Ally;
        }

        // 判断建筑是否属于友军保护范围，负责避免穿透弹误伤己方或盟友建筑。
        private bool IsProtectedFriendlyThing(Thing thing)
        {
            if (launcher == null || launcher.Faction == null || thing.Faction == null)
            {
                return false;
            }

            if (thing.Faction == launcher.Faction)
            {
                return true;
            }

            return launcher.Faction.RelationKindWith(thing.Faction) == FactionRelationKind.Ally;
        }

        // 对命中的目标应用统一伤害，负责让穿透弹共享攻击力、暴击和克制逻辑。
        private void ApplyDamageToThing(Thing target)
        {
            if (target == null)
            {
                return;
            }

            BattleStatUtility.ApplyAction(launcher, target, BuildBattleAction());
            TryTriggerDirectionalImpact(target);
            ApplyExtraDamages(target);
        }

        // 追加额外伤害，负责保留原有额外伤害 Def 的触发能力。
        private void ApplyExtraDamages(Thing target)
        {
            IEnumerable<ExtraDamage> extraDamages = ExtraDamages;
            if (extraDamages == null)
            {
                return;
            }

            bool instigatorGuilty = !(launcher is Pawn launcherPawn) || !launcherPawn.Drafted;
            foreach (ExtraDamage extraDamage in extraDamages)
            {
                if (!Rand.Chance(extraDamage.chance))
                {
                    continue;
                }

                DamageInfo extraDamageInfo = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, target, instigatorGuilty);
                target.TakeDamage(extraDamageInfo);
            }
        }

        // 触发方向命中特效，负责让特效沿子弹前进方向从命中点继续喷出。
        private void TryTriggerDirectionalImpact(Thing target)
        {
            if (Map == null || target == null || Extension?.directionalImpactEffect == null)
            {
                return;
            }

            if (target is Pawn && !Extension.directionalImpactOnPawn)
            {
                return;
            }

            if (target is Building && !Extension.directionalImpactOnBuilding)
            {
                return;
            }

            DirectionalImpactEffectContext.Register(
                target,
                GetTravelDirection(),
                Extension.directionalImpactSpeed,
                Extension.directionalImpactOffsetForward,
                Extension.directionalImpactOffsetUp);

            try
            {
                Effecter effecter = Extension.directionalImpactEffect.Spawn();
                effecter.Trigger(new TargetInfo(Position, Map), new TargetInfo(target));
                effecter.Cleanup();
            }
            finally
            {
                DirectionalImpactEffectContext.Clear(target);
            }
        }

        // 构建统一战斗动作，负责把投射物 XML 参数转成统一结算请求。
        private BattleActionConfig BuildBattleAction()
        {
            return new BattleActionConfig
            {
                baseAmount = ResolveBaseAmount(),
                attackPowerRatio = Extension?.attackPowerRatio ?? 0f,
                healPowerRatio = 0f,
                damageDef = DamageDef,
                penetration = ArmorPenetration,
                isHealing = false,
                canCrit = Extension?.canCrit ?? true,
                applyAffinity = Extension?.applyAffinity ?? true,
                canHitBuilding = Extension?.canHitBuilding ?? true,
                affectHostile = Extension?.affectHostile ?? true,
                affectFriendly = Extension?.affectFriendly ?? false
            };
        }

        // 解析基础伤害，负责让旧投射物的原版基础伤害继续作为新系统的基础值。
        private float ResolveBaseAmount()
        {
            if (Extension != null && Extension.baseAmount != 0f)
            {
                return Extension.baseAmount;
            }

            return DamageAmount;
        }

        // 计算范围格子，负责以当前弹体位置为中心生成沿飞行方向的矩形区域。
        private IEnumerable<IntVec3> DamageCells()
        {
            Vector3 forward = GetTravelDirection();
            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();
            Vector3 side = new Vector3(forward.z, 0f, -forward.x);
            Vector3 center = ExactPosition.Yto0();
            IntVec3 centerCell = center.ToIntVec3();
            float halfLength = DamageLength() * 0.5f;
            float halfWidth = DamageWidth() * 0.5f;
            int radius = Mathf.CeilToInt(Mathf.Max(halfLength, halfWidth)) + 1;

            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    IntVec3 cell = new IntVec3(centerCell.x + x, centerCell.y, centerCell.z + z);
                    if (!cell.InBounds(Map))
                    {
                        continue;
                    }

                    Vector3 offset = cell.ToVector3Shifted().Yto0() - center;
                    float forwardDistance = Vector3.Dot(offset, forward);
                    float sideDistance = Vector3.Dot(offset, side);
                    if (forwardDistance >= -halfLength && forwardDistance < halfLength &&
                        sideDistance >= -halfWidth && sideDistance < halfWidth)
                    {
                        yield return cell;
                    }
                }
            }
        }

        // 获取飞行方向，负责为判定区域和命中特效提供统一方向向量。
        private Vector3 GetTravelDirection()
        {
            Vector3 forward = (destination - origin).Yto0();
            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
            }

            return forward;
        }

        // 获取伤害间隔，负责对 XML 配置做安全下限。
        private int DamageIntervalTicks()
        {
            return Mathf.Max(1, Extension?.damageIntervalTicks ?? 1);
        }

        // 获取友军免疫开关，负责在没有 XML 扩展时仍保持安全默认值。
        private bool IsFriendlyFireImmune()
        {
            return Extension?.immuneFriendlyFire ?? true;
        }

        // 获取伤害宽度，负责对 XML 配置做安全下限。
        private float DamageWidth()
        {
            return Mathf.Max(1f, Extension?.damageWidth ?? 1f);
        }

        // 获取伤害长度，负责对 XML 配置做安全下限。
        private float DamageLength()
        {
            return Mathf.Max(1f, Extension?.damageLength ?? 1f);
        }

        // 判断建筑是否属于穿透弹可命中的实体，负责优先命中墙体和其他完整填充建筑。
        private bool CanHitBuilding(Building building)
        {
            return building.def != null && (building.def.Fillage == FillCategory.Full || building.def.passability == Traversability.Impassable);
        }

        // 获取绘制缩放，负责按飞行进度从起始尺寸插值到结束尺寸。
        private float GetDrawScale()
        {
            float startScale = Mathf.Max(0.01f, Extension?.startDrawScale ?? 1f);
            float endScale = Mathf.Max(0.01f, Extension?.endDrawScale ?? 1f);
            return Mathf.Lerp(startScale, endScale, DistanceCoveredFraction);
        }

        // 获取绘制透明度，负责按配置执行淡入和淡出。
        private float GetDrawAlpha()
        {
            float alpha = 1f;
            int fadeInTicks = Mathf.Max(0, Extension?.fadeInTicks ?? 0);
            if (fadeInTicks > 0)
            {
                float ageTicks = Mathf.Max(0f, StartingTicksToImpact - Mathf.Max(0f, ticksToImpact));
                alpha = Mathf.Min(alpha, ageTicks / fadeInTicks);
            }

            int fadeOutTicks = Mathf.Max(0, Extension?.fadeOutTicks ?? 0);
            if (fadeOutTicks > 0)
            {
                alpha = Mathf.Min(alpha, Mathf.Max(0f, ticksToImpact) / fadeOutTicks);
            }

            return Mathf.Clamp01(alpha);
        }
    }
}
