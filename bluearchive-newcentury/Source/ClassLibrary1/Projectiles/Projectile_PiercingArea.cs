using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace BANWlLib.Projectiles
{
    /// <summary>
    /// 飞行穿透范围抛射体，负责在飞行途中按矩形范围周期性伤害敌方 Pawn。
    /// </summary>
    public class Projectile_PiercingArea : Projectile
    {
        private readonly Dictionary<int, int> lastDamageTicksByThingId = new Dictionary<int, int>();
        private int ticksUntilDamage;
        private static readonly List<IntVec3> checkedCells = new List<IntVec3>();

        /// <summary>
        /// 抛射体更新频率，负责让穿透伤害和阻挡检测稳定按 tick 运行。
        /// </summary>
        public override int UpdateRateTicks => 1;

        /// <summary>
        /// 穿透弹配置，负责从 ThingDef 扩展中读取参数。
        /// </summary>
        private PiercingProjectileExtension Extension => def.GetModExtension<PiercingProjectileExtension>();

        /// <summary>
        /// 保存飞行状态，负责让存档读档后继续按配置造成范围伤害。
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksUntilDamage, "ticksUntilDamage", 0);
        }

        /// <summary>
        /// 飞行 tick，负责替代原版命中即销毁逻辑并执行穿透范围伤害。
        /// </summary>
        protected override void TickInterval(int delta)
        {
            lifetime -= delta;
            if (landed)
            {
                return;
            }

            Vector3 lastExactPosition = ExactPosition;
            ticksToImpact -= delta;
            if (!ExactPosition.InBounds(Map))
            {
                ticksToImpact += delta;
                Position = ExactPosition.ToIntVec3();
                Destroy();
                return;
            }

            Vector3 newExactPosition = ExactPosition;
            if (TryStopOnBlockingThing(lastExactPosition, newExactPosition))
            {
                return;
            }

            Position = newExactPosition.ToIntVec3();
            TickDamage(delta);
            if (ticksToImpact <= 0)
            {
                if (DestinationCell.InBounds(Map))
                {
                    Position = DestinationCell;
                }

                DamagePawnsInArea();
                Impact(null);
            }
        }

        /// <summary>
        /// 绘制抛射体，负责让贴图按飞行进度缩放并淡入淡出。
        /// </summary>
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

        /// <summary>
        /// 处理周期伤害，负责按配置 tick 间隔触发一次范围判定。
        /// </summary>
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

        /// <summary>
        /// 对范围内 Pawn 造成伤害，负责执行敌我过滤和重复命中间隔限制。
        /// </summary>
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

                    ApplyDamageToPawn(pawn);
                }
            }
        }

        /// <summary>
        /// 判断 Pawn 是否可伤害，负责跳过发射者、死亡目标、友军和冷却中的重复目标。
        /// </summary>
        private bool CanDamagePawn(Pawn pawn)
        {
            if (pawn == launcher || pawn.Dead)
            {
                return false;
            }

            if (IsFriendlyFireImmune() && launcher != null && !pawn.HostileTo(launcher))
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

        /// <summary>
        /// 造成一次原版伤害，负责复用抛射体伤害、穿甲、额外伤害和武器品质。
        /// </summary>
        private void ApplyDamageToPawn(Pawn pawn)
        {
            bool instigatorGuilty = !(launcher is Pawn launcherPawn) || !launcherPawn.Drafted;
            DamageInfo damageInfo = new DamageInfo(DamageDef, DamageAmount, ArmorPenetration, ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
            damageInfo.SetWeaponQuality(equipmentQuality);
            pawn.TakeDamage(damageInfo);

            IEnumerable<ExtraDamage> extraDamages = ExtraDamages;
            if (extraDamages == null)
            {
                return;
            }

            foreach (ExtraDamage extraDamage in extraDamages)
            {
                if (!Rand.Chance(extraDamage.chance))
                {
                    continue;
                }

                DamageInfo extraDamageInfo = new DamageInfo(extraDamage.def, extraDamage.amount, extraDamage.AdjustedArmorPenetration(), ExactRotation.eulerAngles.y, launcher, null, equipmentDef, DamageInfo.SourceCategory.ThingOrUnknown, intendedTarget.Thing, instigatorGuilty);
                pawn.TakeDamage(extraDamageInfo);
            }
        }

        /// <summary>
        /// 计算范围格子，负责以当前弹体位置为中心生成沿飞行方向的矩形区域。
        /// </summary>
        private IEnumerable<IntVec3> DamageCells()
        {
            Vector3 forward = (destination - origin).Yto0();
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

        /// <summary>
        /// 检查飞行路径阻挡，负责在遇到完整实体时停止抛射体。
        /// </summary>
        private bool TryStopOnBlockingThing(Vector3 lastExactPosition, Vector3 newExactPosition)
        {
            if (lastExactPosition == newExactPosition)
            {
                return false;
            }

            IntVec3 lastCell = lastExactPosition.ToIntVec3();
            IntVec3 newCell = newExactPosition.ToIntVec3();
            if (!lastCell.InBounds(Map) || !newCell.InBounds(Map))
            {
                return false;
            }

            checkedCells.Clear();
            Vector3 stepVector = (newExactPosition - lastExactPosition);
            float distance = stepVector.MagnitudeHorizontal();
            int stepCount = Mathf.Max(1, Mathf.CeilToInt(distance / 0.2f));
            Vector3 step = stepVector / stepCount;
            for (int i = 1; i <= stepCount; i++)
            {
                Vector3 current = lastExactPosition + step * i;
                IntVec3 cell = current.ToIntVec3();
                if (!cell.InBounds(Map) || checkedCells.Contains(cell))
                {
                    continue;
                }

                checkedCells.Add(cell);
                Thing blocker = FirstBlockingThing(cell);
                if (blocker == null)
                {
                    continue;
                }

                Position = cell;
                Impact(blocker);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取格子内第一个完整阻挡物，负责让岩墙、墙体和关闭门停止抛射体。
        /// </summary>
        private Thing FirstBlockingThing(IntVec3 cell)
        {
            List<Thing> thingList = cell.GetThingList(Map);
            for (int i = 0; i < thingList.Count; i++)
            {
                Thing thing = thingList[i];
                if (thing == this)
                {
                    continue;
                }

                Building_Door door = thing as Building_Door;
                if (door != null && door.Open)
                {
                    continue;
                }

                if (thing.def.Fillage == FillCategory.Full)
                {
                    return thing;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取伤害间隔，负责对 XML 配置做安全下限。
        /// </summary>
        private int DamageIntervalTicks()
        {
            return Mathf.Max(1, Extension?.damageIntervalTicks ?? 1);
        }

        /// <summary>
        /// 获取友军免疫开关，负责在没有 XML 扩展时仍保持安全默认值。
        /// </summary>
        private bool IsFriendlyFireImmune()
        {
            return Extension?.immuneFriendlyFire ?? true;
        }

        /// <summary>
        /// 获取伤害宽度，负责对 XML 配置做安全下限。
        /// </summary>
        private float DamageWidth()
        {
            return Mathf.Max(1f, Extension?.damageWidth ?? 1f);
        }

        /// <summary>
        /// 获取伤害长度，负责对 XML 配置做安全下限。
        /// </summary>
        private float DamageLength()
        {
            return Mathf.Max(1f, Extension?.damageLength ?? 1f);
        }

        /// <summary>
        /// 获取绘制缩放，负责按飞行进度从起始尺寸插值到结束尺寸。
        /// </summary>
        private float GetDrawScale()
        {
            float startScale = Mathf.Max(0.01f, Extension?.startDrawScale ?? 1f);
            float endScale = Mathf.Max(0.01f, Extension?.endDrawScale ?? 1f);
            return Mathf.Lerp(startScale, endScale, DistanceCoveredFraction);
        }

        /// <summary>
        /// 获取绘制透明度，负责按配置执行淡入和淡出。
        /// </summary>
        private float GetDrawAlpha()
        {
            float alpha = 1f;
            int fadeInTicks = Mathf.Max(0, Extension?.fadeInTicks ?? 0);
            if (fadeInTicks > 0)
            {
                float ageTicks = Mathf.Max(0f, StartingTicksToImpact - lifetime);
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
