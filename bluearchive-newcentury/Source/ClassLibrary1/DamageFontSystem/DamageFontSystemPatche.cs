using BANWlLib.DamageFontSystem;
using BANWlLib.DamageFontSystem.Comp;
using BANWlLib.DamageFontSystem.Setting;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

public class DamageFontSystemPatche
{
    // 使用字典来存储暴击状态，key是Pawn的ThingID，value是是否暴击
    // 这样做比单纯的 static bool 更安全，防止在连环爆炸等复杂情况下状态错乱
    public static Dictionary<int, bool> CritState = new Dictionary<int, bool>();

    // ==========================================================
    // 1. PreApplyDamage: 负责计算暴击逻辑，并修改原始伤害
    // ==========================================================
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    public static class Patch_Pawn_PreApplyDamage
    {
        // 注意：源码中 dinfo 是 ref
        public static void Prefix(Pawn __instance, ref DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            // 安全检查
            if (__instance == null || dinfo.Instigator == null) return;

            // 这里的逻辑是你原有的
            DisableCriticalComp comp = Current.Game.GetComponent<DisableCriticalComp>();
            if (comp == null) return;

            string DamageType = dinfo.Def.defName;
            if (comp.DisableCritical.Any(p => p.defName == DamageType))
                return;

            Pawn attacker = dinfo.Instigator as Pawn;
            if (attacker == null) return;

            float critChance = attacker.GetStatValue(CriticalRef.BANW_CriticalChance);
            float critMultiplier = attacker.GetStatValue(CriticalRef.BANW_CriticalDamage);

            bool isForcedCrit = comp.EnsureCritical.Any(p => p.defName == DamageType);
            bool isCrit = isForcedCrit || Rand.Value < critChance;

            // 记录状态
            if (isCrit)
            {
                // 存入字典：这个Pawn这次受伤是暴击
                CritState[__instance.thingIDNumber] = true;
                // 修改伤害数值（这会影响后续的护甲计算）
                dinfo.SetAmount(dinfo.Amount * critMultiplier);
            }
            else
            {
                // 确保清理旧状态
                if (CritState.ContainsKey(__instance.thingIDNumber))
                    CritState.Remove(__instance.thingIDNumber);
            }
        }
    }

    // ==========================================================
    // 2. PostApplyDamage: 负责读取最终伤害，并显示飘字
    // ==========================================================
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PostApplyDamage))]
    public static class Patch_Pawn_PostApplyDamage
    {
        // 注意：源码中 dinfo 不是 ref，这里不要加 ref，否则会报错
        public static void Postfix(Pawn __instance, DamageInfo dinfo, float totalDamageDealt)
        {
            // 1. 检查是否开启了飘字功能
            if (!DamageFontMod.settings.enableDamageFloat) return;

            // 2. 检查是否有暴击状态
            bool isCrit = false;
            if (CritState.TryGetValue(__instance.thingIDNumber, out bool state))
            {
                isCrit = state;
                // 用完即删，保持字典清洁
                CritState.Remove(__instance.thingIDNumber);
            }

            // 3. 如果不是暴击，直接退出（或者你可以写else逻辑显示普通伤害）
            if (!isCrit) return;

            // 4. 如果实际伤害为0（比如完全被护甲弹开），可能不想显示暴击
            // 如果你想显示 "0" 暴击，可以把这行去掉
            if (totalDamageDealt <= 0.01f) return;
            Log.Message($"显示暴击飘字！最终伤害: {totalDamageDealt}");
            // 5. 弹出飘字，这里的 totalDamageDealt 就是计算护甲后的真实伤害
            CriticalObjPool.showCriticalShow(totalDamageDealt, __instance);
        }
    }
}