using System.Collections.Generic;
using RimWorld;
using Verse;

namespace BANWlLib.WeaponRestrictions
{
    // 武器白名单工具，负责统一查询 Kind 配置、判断装备权限和卸下违规武器。
    public static class WeaponWhitelistUtility
    {
        // 判断指定 Pawn 是否允许装备目标物品，非武器和未配置白名单的 Kind 保持原版行为。
        public static bool CanEquip(Pawn pawn, Thing thing)
        {
            if (pawn?.kindDef == null || thing?.def == null || !thing.def.IsWeapon)
            {
                return true;
            }

            WeaponWhitelistKindExtension extension = pawn.kindDef.GetModExtension<WeaponWhitelistKindExtension>();
            if (extension == null)
            {
                return true;
            }

            return extension.allowedWeapons != null && extension.allowedWeapons.Contains(thing.def);
        }

        // 卸下地图中 Pawn 当前装备的违规武器，负责让读档后的旧装备立即符合白名单。
        public static void DropDisallowedWeapons(Pawn pawn)
        {
            if (pawn?.equipment == null || !pawn.Spawned)
            {
                return;
            }

            List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;
            for (int i = equipment.Count - 1; i >= 0; i--)
            {
                ThingWithComps equippedThing = equipment[i];
                if (CanEquip(pawn, equippedThing))
                {
                    continue;
                }

                ThingWithComps droppedThing;
                if (!pawn.equipment.TryDropEquipment(equippedThing, out droppedThing, pawn.Position, false))
                {
                    Log.Error($"[BANW] 无法卸下 {pawn.LabelShort} 的违规武器 {equippedThing.LabelShort}。");
                }
            }
        }
    }
}
