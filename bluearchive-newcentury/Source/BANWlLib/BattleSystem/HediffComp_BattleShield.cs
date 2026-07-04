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
        private Effecter shieldEffecter;

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

        // 状态 Tick 后维护跟随特效，负责让 PawnKind 配置的护盾表现持续贴在 Pawn 身上。
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            TickShieldEffecter();
        }

        // 状态移除后清理跟随特效，负责避免护盾结束后残留持续 Mote。
        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            CleanupShieldEffecter();
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

        // 维护护盾跟随特效，负责读 PawnKind 配置并每 Tick 续命 Effecter。
        private void TickShieldEffecter()
        {
            Pawn pawn = Pawn;
            EffecterDef effecterDef = ShieldEffecterDef(pawn);
            if (effecterDef == null || pawn?.Map == null || pawn.Destroyed || !pawn.Spawned)
            {
                CleanupShieldEffecter();
                return;
            }

            if (shieldEffecter == null)
            {
                shieldEffecter = effecterDef.Spawn();
            }

            TargetInfo targetInfo = new TargetInfo(pawn);
            shieldEffecter.EffectTick(targetInfo, targetInfo);
        }

        // 读取 PawnKind 护盾特效配置，负责让不同 Kind 使用不同表现。
        private static EffecterDef ShieldEffecterDef(Pawn pawn)
        {
            return pawn?.kindDef?.GetModExtension<BattleBaseStatExtension>()?.shieldEffecterDef;
        }

        // 清理护盾跟随特效，负责在无配置、离图或护盾结束时释放 Effecter。
        private void CleanupShieldEffecter()
        {
            if (shieldEffecter == null)
            {
                return;
            }

            shieldEffecter.Cleanup();
            shieldEffecter = null;
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

            if (IsFriendlyDamage(__instance, dinfo.Instigator))
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

        // 判断是否是友军伤害，负责让护盾不消耗在自己人造成的攻击上。
        private static bool IsFriendlyDamage(Pawn target, Thing instigator)
        {
            if (target == null || instigator == null)
            {
                return false;
            }

            if (instigator == target)
            {
                return true;
            }

            return target.Faction != null && instigator.Faction != null && target.Faction == instigator.Faction;
        }
    }
}
