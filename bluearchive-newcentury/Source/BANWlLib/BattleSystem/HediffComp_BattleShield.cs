using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 护盾组件配置，负责把 Hediff 标记为可存储战斗护盾值的状态。
    public class HediffCompProperties_BattleShield : HediffCompProperties
    {
        // 初始化组件类型，负责让 XML Hediff 能挂载战斗护盾组件。
        public HediffCompProperties_BattleShield()
        {
            compClass = typeof(HediffComp_BattleShield);
        }
    }

    // 战斗护盾组件，负责保存剩余护盾值并提供伤害吸收接口。
    public class HediffComp_BattleShield : HediffComp
    {
        private float remainingShield;

        // 护盾耗尽时移除状态，负责避免空护盾残留在健康面板。
        public override bool CompShouldRemove => remainingShield <= 0.01f;

        // 健康面板括号文本，负责直接显示当前剩余护盾值。
        public override string CompLabelInBracketsExtra => remainingShield.ToString("0.#");

        // 健康面板提示文本，负责显示完整护盾说明。
        public override string CompTipStringExtra => "剩余护盾值：" + remainingShield.ToString("0.#");

        // 添加护盾值，负责同类护盾重复获得时数值叠加。
        public void AddShield(float amount)
        {
            remainingShield = Mathf.Max(0f, remainingShield + amount);
        }

        // 吸收本次伤害，负责按护盾规则完整抵住一次伤害。
        public bool TryAbsorbDamage(float amount)
        {
            if (amount <= 0f || remainingShield <= 0.01f)
            {
                return false;
            }

            remainingShield = Mathf.Max(0f, remainingShield - amount);
            return true;
        }

        // 保存和读取护盾值，负责让护盾状态随 Hediff 存档。
        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref remainingShield, "remainingShield", 0f);
        }
    }

    // 护盾伤害吸收补丁，负责在 Pawn 受伤前消耗战斗护盾。
    [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
    [HarmonyPriority(Priority.Last)]
    public static class BattleShieldPreApplyDamagePatch
    {
        // 受伤前消耗护盾，负责让护盾至少完整抵住一次伤害并清空不足的护盾值。
        public static bool Prefix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (__instance?.health?.hediffSet?.hediffs == null || dinfo.Amount <= 0f)
            {
                return true;
            }

            List<Hediff> hediffs = __instance.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                HediffComp_BattleShield shield = hediffs[i].TryGetComp<HediffComp_BattleShield>();
                if (shield == null)
                {
                    continue;
                }

                if (!shield.TryAbsorbDamage(dinfo.Amount))
                {
                    continue;
                }

                dinfo.SetAmount(0f);
                absorbed = true;
                return false;
            }

            return true;
        }
    }
}
