using System.Collections.Generic;
using System.Linq;
using BANWlLib.BattleSystem;
using BANWlLib.Projectiles;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BaVerb
{
    // 技能目标预览工具，负责统一计算和绘制圆形、直线、扇形、矩形施法范围。
    public static class BattleTargetPreviewUtility
    {
        private static readonly Color AreaColor = new Color(0.7f, 1f, 1f);
        private static readonly Color RangeColor = new Color(1f, 1f, 1f, 0.3f);

        // 绘制技能预览，负责按配置显示影响格、施法射程圈和目标高亮。
        internal static void DrawPreview(Pawn caster, LocalTargetInfo target, BattleTargetPreviewData data)
        {
            if (caster == null || caster.Map == null || data == null)
            {
                return;
            }

            float range = ResolvePositive(data.range, 0f);
            if (data.drawCasterRange && range > 0f)
            {
                GenDraw.DrawRadiusRing(caster.Position, range, RangeColor);
            }

            if (!target.IsValid || !target.Cell.InBounds(caster.Map))
            {
                return;
            }

            HashSet<IntVec3> cells = CalculateCells(caster, target, data);
            if (cells.Count > 0)
            {
                GenDraw.DrawFieldEdges(cells.ToList(), AreaColor);
            }

            if (data.shape == AbilityTargetPreviewShape.Circle && data.radius > 0f)
            {
                GenDraw.DrawRadiusRing(target.Cell, data.radius, Color.white);
            }

            if (data.drawTargetHighlight)
            {
                GenDraw.DrawTargetHighlight(target.Cell);
            }
        }

        // 计算预览格子，负责按形状分发到具体几何算法。
        internal static HashSet<IntVec3> CalculateCells(Pawn caster, LocalTargetInfo target, BattleTargetPreviewData data)
        {
            if (caster == null || data == null || !target.IsValid)
            {
                return new HashSet<IntVec3>();
            }

            switch (data.shape)
            {
                case AbilityTargetPreviewShape.Circle:
                    return CalculateCircleCells(caster, target, data.radius, data.range);
                case AbilityTargetPreviewShape.Line:
                    return CalculateLineCells(caster, target, data.length, data.width);
                case AbilityTargetPreviewShape.Fan:
                    return CalculateFanCells(caster, target, data.range, data.fanArc);
                case AbilityTargetPreviewShape.Box:
                    return CalculateBoxCells(caster, target, data.range, data.width);
                default:
                    return new HashSet<IntVec3>();
            }
        }

        // 计算圆形范围格子，负责以目标点为中心生成场地或范围治疗影响格。
        public static HashSet<IntVec3> CalculateCircleCells(Pawn caster, LocalTargetInfo target, float radius, float maxRange)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            if (caster == null || caster.Map == null || !target.IsValid)
            {
                return cells;
            }

            float actualRadius = Mathf.Max(0f, radius);
            foreach (IntVec3 cell in GenRadial.RadialCellsAround(target.Cell, actualRadius, true))
            {
                if (!cell.InBounds(caster.Map))
                {
                    continue;
                }

                if (maxRange > 0f && cell.DistanceTo(caster.Position) > maxRange)
                {
                    continue;
                }

                cells.Add(cell);
            }

            return cells;
        }

        // 计算直线范围格子，负责从施法者朝目标方向生成可穿墙直线预览。
        public static HashSet<IntVec3> CalculateLineCells(Pawn caster, LocalTargetInfo target, float length, float width)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            if (caster == null || caster.Map == null || !target.IsValid)
            {
                return cells;
            }

            Vector3 direction = TargetDirection(caster, target);
            if (direction.sqrMagnitude < 0.0001f)
            {
                cells.Add(caster.Position);
                return cells;
            }

            direction.Normalize();
            Vector3 side = new Vector3(direction.z, 0f, -direction.x);
            Vector3 start = caster.Position.ToVector3Shifted().Yto0();
            float actualLength = Mathf.Max(0f, length);
            float halfWidth = Mathf.Max(0.5f, width * 0.5f);
            int searchRadius = Mathf.CeilToInt(actualLength + halfWidth) + 1;

            for (int x = -searchRadius; x <= searchRadius; x++)
            {
                for (int z = -searchRadius; z <= searchRadius; z++)
                {
                    IntVec3 cell = new IntVec3(caster.Position.x + x, caster.Position.y, caster.Position.z + z);
                    if (!cell.InBounds(caster.Map))
                    {
                        continue;
                    }

                    Vector3 offset = cell.ToVector3Shifted().Yto0() - start;
                    float forwardDistance = Vector3.Dot(offset, direction);
                    float sideDistance = Mathf.Abs(Vector3.Dot(offset, side));
                    if (forwardDistance >= 0f && forwardDistance <= actualLength + 0.5f && sideDistance <= halfWidth)
                    {
                        cells.Add(cell);
                    }
                }
            }

            return cells;
        }

        // 计算扇形范围格子，负责从施法者朝目标方向展开指定角度。
        public static HashSet<IntVec3> CalculateFanCells(Pawn caster, LocalTargetInfo target, float range, float fanArc)
        {
            HashSet<IntVec3> cells = new HashSet<IntVec3>();
            if (caster == null || caster.Map == null || !target.IsValid)
            {
                return cells;
            }

            Vector3 direction = TargetDirection(caster, target);
            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector3.forward;
            }

            direction.Normalize();
            float actualRange = Mathf.Max(0f, range);
            int radius = Mathf.CeilToInt(actualRange);
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    IntVec3 cell = new IntVec3(caster.Position.x + x, caster.Position.y, caster.Position.z + z);
                    if (!cell.InBounds(caster.Map) || cell == caster.Position)
                    {
                        continue;
                    }

                    float distance = cell.DistanceTo(caster.Position);
                    if (distance > actualRange)
                    {
                        continue;
                    }

                    Vector3 directionToCell = (cell - caster.Position).ToVector3();
                    if (directionToCell.sqrMagnitude < 0.0001f)
                    {
                        continue;
                    }

                    float angle = Vector3.Angle(direction, directionToCell.normalized);
                    if (angle <= fanArc * 0.5f)
                    {
                        cells.Add(cell);
                    }
                }
            }

            return cells;
        }

        // 计算矩形范围格子，负责从施法者朝目标方向生成带宽度的长条范围。
        public static HashSet<IntVec3> CalculateBoxCells(Pawn caster, LocalTargetInfo target, float range, float width)
        {
            return CalculateLineCells(caster, target, range, Mathf.Max(1f, width));
        }

        // 从 AbilityDef 和关联 Comp 推导预览参数，负责让脱手场地和穿透投射物无需重复写尺寸。
        internal static BattleTargetPreviewData ResolvePreviewData(Verb_CastAbility verb)
        {
            if (verb == null)
            {
                return null;
            }

            AbilityDef abilityDef = verb.Ability?.def;
            AbilityTargetPreviewExtension extension = abilityDef?.GetModExtension<AbilityTargetPreviewExtension>();
            BattleTargetPreviewData data = extension != null ? FromExtension(extension, verb.EffectiveRange) : null;

            if (data == null)
            {
                data = TryAutoResolveFromBattleField(abilityDef, verb.EffectiveRange) ?? TryAutoResolveFromProjectile(abilityDef, verb.EffectiveRange);
            }
            else
            {
                ApplyAutomaticSizeOverrides(data, extension, abilityDef, verb.EffectiveRange);
            }

            return data;
        }

        // 根据显式形状构造预览参数，负责让旧 Verb 用统一工具保留原有显示行为。
        internal static BattleTargetPreviewData CreateData(AbilityTargetPreviewShape shape, float range, float radius, float width, float length, float fanArc)
        {
            return new BattleTargetPreviewData
            {
                shape = shape,
                range = range,
                radius = radius,
                width = width,
                length = length > 0f ? length : range,
                fanArc = fanArc,
                drawCasterRange = true,
                drawTargetHighlight = true
            };
        }

        // 合并 Def 扩展配置，负责让显式字段优先于自动推导。
        private static BattleTargetPreviewData FromExtension(AbilityTargetPreviewExtension extension, float effectiveRange)
        {
            return new BattleTargetPreviewData
            {
                shape = extension.shape,
                range = extension.range > 0f ? extension.range : effectiveRange,
                radius = extension.radius,
                width = extension.width,
                length = extension.length,
                fanArc = extension.fanArc,
                drawCasterRange = extension.drawCasterRange,
                drawTargetHighlight = extension.drawTargetHighlight
            };
        }

        // 应用自动尺寸覆盖，负责把配置中要求复用的场地半径和投射物尺寸填进去。
        private static void ApplyAutomaticSizeOverrides(BattleTargetPreviewData data, AbilityTargetPreviewExtension extension, AbilityDef abilityDef, float effectiveRange)
        {
            if (extension.useBattleFieldRadius && data.radius <= 0f)
            {
                BattleFieldControllerExtension fieldExtension = FindBattleFieldExtension(abilityDef);
                if (fieldExtension != null)
                {
                    data.radius = fieldExtension.radius;
                }
            }

            if (extension.usePiercingProjectileSize && data.width <= 0f)
            {
                PiercingProjectileExtension projectileExtension = FindPiercingProjectileExtension(abilityDef);
                if (projectileExtension != null)
                {
                    data.width = projectileExtension.damageWidth;
                }
            }

            NormalizeData(data, effectiveRange);
        }

        // 自动推导脱手场地预览，负责从场地控制器 Def 获取圆形半径。
        private static BattleTargetPreviewData TryAutoResolveFromBattleField(AbilityDef abilityDef, float effectiveRange)
        {
            BattleFieldControllerExtension extension = FindBattleFieldExtension(abilityDef);
            if (extension == null)
            {
                return null;
            }

            BattleTargetPreviewData data = CreateData(AbilityTargetPreviewShape.Circle, effectiveRange, extension.radius, 1f, effectiveRange, 30f);
            NormalizeData(data, effectiveRange);
            return data;
        }

        // 自动推导穿透投射物预览，负责从投射物 Def 获取直线宽度和长度。
        private static BattleTargetPreviewData TryAutoResolveFromProjectile(AbilityDef abilityDef, float effectiveRange)
        {
            PiercingProjectileExtension extension = FindPiercingProjectileExtension(abilityDef);
            if (extension == null)
            {
                return null;
            }

            BattleTargetPreviewData data = CreateData(AbilityTargetPreviewShape.Line, effectiveRange, 0f, extension.damageWidth, effectiveRange, 30f);
            NormalizeData(data, effectiveRange);
            return data;
        }

        // 查找脱手场地配置，负责从 AbilityComp 里定位生成的场地 ThingDef。
        private static BattleFieldControllerExtension FindBattleFieldExtension(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return null;
            }

            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                CompProperties_AbilitySpawnBattleField props = abilityDef.comps[i] as CompProperties_AbilitySpawnBattleField;
                BattleFieldControllerExtension extension = props?.fieldThingDef?.GetModExtension<BattleFieldControllerExtension>();
                if (extension != null)
                {
                    return extension;
                }
            }

            return null;
        }

        // 查找穿透投射物配置，负责从 AbilityComp 里定位 projectileDef。
        private static PiercingProjectileExtension FindPiercingProjectileExtension(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return null;
            }

            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                CompProperties_AbilityLaunchProjectile props = abilityDef.comps[i] as CompProperties_AbilityLaunchProjectile;
                PiercingProjectileExtension extension = props?.projectileDef?.GetModExtension<PiercingProjectileExtension>();
                if (extension != null)
                {
                    return extension;
                }
            }

            return null;
        }

        // 标准化预览参数，负责补齐缺省值并避免无效尺寸导致范围不显示。
        private static void NormalizeData(BattleTargetPreviewData data, float effectiveRange)
        {
            data.range = data.range > 0f ? data.range : effectiveRange;
            data.radius = data.radius > 0f ? data.radius : 1f;
            data.width = data.width > 0f ? data.width : 1f;
            data.length = data.length > 0f ? data.length : data.range;
            data.fanArc = data.fanArc > 0f ? data.fanArc : 30f;
        }

        // 获取目标方向，负责把施法者和目标格转换成水平单位向量。
        private static Vector3 TargetDirection(Pawn caster, LocalTargetInfo target)
        {
            return (target.Cell - caster.Position).ToVector3().Yto0();
        }

        // 解析正数参数，负责在配置无效时使用调用方默认值。
        private static float ResolvePositive(float value, float fallback)
        {
            return value > 0f ? value : fallback;
        }
    }
}
