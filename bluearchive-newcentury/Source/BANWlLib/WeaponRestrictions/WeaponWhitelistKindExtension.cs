using System.Collections.Generic;
using Verse;

namespace BANWlLib.WeaponRestrictions
{
    // Kind 武器白名单配置，负责声明该 Kind 可以装备的精确武器 Def。
    public sealed class WeaponWhitelistKindExtension : DefModExtension
    {
        public List<ThingDef> allowedWeapons = new List<ThingDef>();

        // 检查白名单配置，负责在 Def 加载阶段报告空列表、重复项和非武器引用。
        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string error in base.ConfigErrors())
            {
                yield return error;
            }

            if (allowedWeapons == null || allowedWeapons.Count == 0)
            {
                yield return "Kind 武器白名单不能为空。";
                yield break;
            }

            HashSet<ThingDef> seenWeapons = new HashSet<ThingDef>();
            for (int i = 0; i < allowedWeapons.Count; i++)
            {
                ThingDef weaponDef = allowedWeapons[i];
                if (weaponDef == null)
                {
                    yield return "Kind 武器白名单包含无效的 ThingDef 引用。";
                    continue;
                }

                if (!weaponDef.IsWeapon)
                {
                    yield return $"Kind 武器白名单中的 {weaponDef.defName} 不是武器。";
                }

                if (!seenWeapons.Add(weaponDef))
                {
                    yield return $"Kind 武器白名单重复配置了 {weaponDef.defName}。";
                }
            }
        }
    }
}
