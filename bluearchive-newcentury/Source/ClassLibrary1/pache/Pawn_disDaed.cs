using BANWlLib.mainUI.Mission.GameComp;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BANWlLib.pache
{
    public class Pawn_disDaed
    {
        public static GameComp_TaskQuest comp_TaskQuest;
        [HarmonyPatch(typeof(Pawn), "Kill")]
        [HarmonyPatch(new Type[] { typeof(DamageInfo?), typeof(Hediff) })]
        public static class PreventDeathPatch
        {
            private static readonly MethodInfo MakeDownedMethod2 = AccessTools.Method(
                typeof(Pawn_HealthTracker),
                "MakeDowned",
                new Type[] { typeof(DamageInfo?), typeof(Hediff) }
            );
            private static readonly MethodInfo MakeDownedMethod3 = AccessTools.Method(
                typeof(Pawn_HealthTracker),
                "MakeDowned",
                new Type[] { typeof(DamageInfo?), typeof(Hediff), typeof(HediffDef) }
            );

            public static bool Prefix(Pawn __instance, [HarmonyArgument("dinfo")] DamageInfo? dinfo, [HarmonyArgument("exactCulprit")] Hediff exactCulprit)
            {
                if (!IsUndyingTarget(__instance))
                {
                    return true;
                }

                // 禁止死亡：恢复生命值，使其保持活着
                RestoreVitalParts(__instance);
                ReduceLethalHediffSeverity(__instance);

                // 让Pawn倒地但不死亡（兼容不同版本的MakeDowned签名）
                if (!__instance.Downed)
                {
                    if (MakeDownedMethod3 != null)
                    {
                        // MakeDowned(DamageInfo? dinfo, Hediff hediff, HediffDef cause)
                        MakeDownedMethod3.Invoke(__instance.health, new object[] { dinfo, exactCulprit, null });
                    }
                    else if (MakeDownedMethod2 != null)
                    {
                        // MakeDowned(DamageInfo? dinfo, Hediff hediff)
                        MakeDownedMethod2.Invoke(__instance.health, new object[] { dinfo, exactCulprit });
                    }
                    else
                    {
                        Log.Warning("[BANW] 未找到 Pawn_HealthTracker.MakeDowned 的兼容签名，无法将角色置为倒地，但已阻止死亡。");
                    }
                }
                
                // ✅ 修复：添加60秒的麻醉Hediff，让学生倒地60秒后自动站起
                // 直接添加60秒的麻醉，让其在1分钟后自行醒来
                // 不依赖Downed检测，统一处理
                {
                    // 移除之前的麻醉状态，避免叠加
                    Hediff existingAnesthesia = __instance.health.hediffSet.GetFirstHediffOfDef(RimWorld.HediffDefOf.Anesthetic);
                    if (existingAnesthesia != null)
                    {
                        __instance.health.RemoveHediff(existingAnesthesia);
                    }

                    // 添加新的麻醉Hediff
                    Hediff anesthesia = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Anesthetic, __instance);
                    anesthesia.Severity = 1f; // 保证会倒地
                    __instance.health.AddHediff(anesthesia);

                    // 设置持续时间为3600 ticks（约60秒实时）
                    var compDisappear = anesthesia.TryGetComp<HediffComp_Disappears>();
                    if (compDisappear != null)
                    {
                        compDisappear.ticksToDisappear = 3600;
                    }
                    else
                    {
                        // 兼容性反射：某些版本字段名或权限不同
                        var compType = AccessTools.TypeByName("RimWorld.HediffComp_Disappears");
                        if (compType != null)
                        {
                            var comp = (anesthesia as HediffWithComps)?.comps?.FirstOrDefault(c => compType.IsInstanceOfType(c));
                            if (comp != null)
                            {
                                var field = AccessTools.Field(compType, "ticksToDisappear");
                                field?.SetValue(comp, 3600);
                            }
                        }
                    }
                }

                return false; // 阻止原始的Kill方法执行
            }
            private static bool IsUndyingTarget(Pawn p)
            {
                if (Current.Game == null) return false;
                var comp = Current.Game.GetComponent<GameComp_TaskQuest>();
                if (comp == null || comp.NoDie == null) return false;
                return comp.NoDie.Contains(p);
            }
            private static void RestoreVitalParts(Pawn p)
            {
                var hediffSet = p.health.hediffSet;
                for (int i = hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    Hediff h = hediffSet.hediffs[i];
                    if (h is Hediff_MissingPart && IsVitalPart(h.Part))
                    {
                        p.health.RemoveHediff(h);
                    }
                }
            }
            private static void ReduceLethalHediffSeverity(Pawn p)
            {
                var hediffs = p.health.hediffSet.hediffs;
                for (int i = 0; i < hediffs.Count; i++)
                {
                    Hediff h = hediffs[i];
                    if (h.def.lethalSeverity > 0 && h.Severity >= h.def.lethalSeverity)
                    {
                        h.Severity = h.def.lethalSeverity - 0.05f;
                    }
                }
            }
            private static bool IsVitalPart(BodyPartRecord part)
            {
                if (part == null) return false;
                var tags = part.def.tags;
                return tags.Contains(BodyPartTagDefOf.ConsciousnessSource) ||
                       tags.Contains(BodyPartTagDefOf.BloodPumpingSource) ||
                       tags.Contains(BodyPartTagDefOf.BreathingSource);
            }
        }
    }
}
