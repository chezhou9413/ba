using System.Globalization;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 武器伤害显示工具，负责在武器信息卡中显示攻击属性加成后的当前伤害。
    public static class WeaponDamageDisplayUtility
    {
        private static readonly FieldInfo ValueStringField = AccessTools.Field(typeof(StatDrawEntry), "valueStringInt");

        // 尝试改写武器伤害显示值，负责让 UI 直接显示当前持有者加成后的伤害。
        public static void TryOverrideValueString(StatDrawEntry entry, StatRequest request)
        {
            if (!TryGetContext(entry, request, out Pawn pawn, out float baseDamage))
            {
                return;
            }

            float finalDamage = BattleStatUtility.ScaleWeaponDamageBase(pawn, baseDamage);
            ValueStringField?.SetValue(entry, FormatDamage(finalDamage));
        }

        // 构建武器伤害解释，负责追加当前攻击力加成和算法。
        public static string BuildExplanation(StatDrawEntry entry, StatRequest request)
        {
            if (!TryGetContext(entry, request, out Pawn pawn, out float baseDamage))
            {
                return string.Empty;
            }

            float levelMultiplier = BattleStatUtility.GetAttackLevelMultiplier(pawn);
            float starMultiplier = BattleStatUtility.GetAttackStarMultiplier(pawn);
            float attackFlat = BattleStatUtility.GetAttackFlatBonus(pawn);
            float attackMultiplier = BattleStatUtility.GetAttackMultiplier(pawn);
            float characterAttack = BattleStatUtility.GetFinalAttackPower(pawn, baseDamage);
            float finalDamage = BattleStatUtility.ScaleWeaponDamageBase(pawn, baseDamage);

            return "\n\n" +
                   "武器伤害修正".Colorize(ColoredText.TipSectionTitleColor) + "\n" +
                   "武器初始攻击力：" + FormatDamage(baseDamage) + "\n" +
                   "升级攻击力倍率：" + levelMultiplier.ToString("P1") + "\n" +
                   "升星攻击力倍率：" + starMultiplier.ToString("P1") + "\n" +
                   "固定攻击力：" + FormatDamage(attackFlat) + "\n" +
                   "角色自身攻击力：" + FormatDamage(characterAttack) + "\n" +
                   "攻击力加成：" + attackMultiplier.ToString("P0") + "\n" +
                   "算法：((" + FormatDamage(baseDamage) + " x " + levelMultiplier.ToString("P1") + ") x " + starMultiplier.ToString("P1") + " + " + FormatDamage(attackFlat) + ") x " + attackMultiplier.ToString("P0") + "\n" +
                   "当前显示伤害：" + FormatDamage(finalDamage).Colorize(new Color(1f, 0.35f, 0.28f));
        }

        // 判断当前统计行是否是远程武器伤害行，负责避免影响护穿、射程和其他普通属性。
        private static bool TryGetContext(StatDrawEntry entry, StatRequest request, out Pawn pawn, out float baseDamage)
        {
            pawn = null;
            baseDamage = 0f;
            if (entry == null || request.Empty || !request.HasThing || request.Thing == null)
            {
                return false;
            }

            Thing weapon = request.Thing;
            if (weapon.def == null || !weapon.def.IsRangedWeapon)
            {
                return false;
            }

            if (!IsDamageRow(entry))
            {
                return false;
            }

            pawn = GetHolderPawn(weapon);
            if (pawn == null)
            {
                return false;
            }

            ThingDef projectileDef = weapon.def.Verbs?.FirstOrDefault(v => v?.defaultProjectile?.projectile != null)?.defaultProjectile;
            if (projectileDef?.projectile == null)
            {
                return false;
            }

            baseDamage = Mathf.Max(0f, projectileDef.projectile.GetDamageAmount(weapon));
            return baseDamage > 0f;
        }

        // 判断统计标签是否代表伤害，负责兼容中文和英文界面。
        private static bool IsDamageRow(StatDrawEntry entry)
        {
            string label = entry.LabelCap;
            if (label.NullOrEmpty())
            {
                return false;
            }

            bool hasDamageWord = label.Contains("伤害") || label.ToLowerInvariant().Contains("damage");
            bool excluded = label.Contains("护甲") ||
                            label.Contains("穿透") ||
                            label.Contains("倍率") ||
                            label.ToLowerInvariant().Contains("armor") ||
                            label.ToLowerInvariant().Contains("penetration") ||
                            label.ToLowerInvariant().Contains("multiplier");
            return hasDamageWord && !excluded;
        }

        // 查找武器持有者，负责从装备栏、衣物栏或容器里定位 Pawn。
        private static Pawn GetHolderPawn(Thing thing)
        {
            if (thing?.ParentHolder is Pawn_EquipmentTracker equipmentTracker)
            {
                return equipmentTracker.pawn;
            }

            if (thing?.ParentHolder is Pawn_ApparelTracker apparelTracker)
            {
                return apparelTracker.pawn;
            }

            if (thing?.ParentHolder is ThingOwner owner)
            {
                foreach (Thing innerThing in owner)
                {
                    if (innerThing is Pawn pawn)
                    {
                        return pawn;
                    }
                }
            }

            return null;
        }

        // 格式化伤害数值，负责去掉无意义的小数位。
        private static string FormatDamage(float value)
        {
            return value.ToString("0.#", CultureInfo.InvariantCulture);
        }
    }
}
