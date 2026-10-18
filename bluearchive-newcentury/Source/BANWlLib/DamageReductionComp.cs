using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BANWlLib.BattleSystem;
using BANWlLib.DamageFontSystem;
using UnityEngine;
using Verse;
using BANWlLib.Tool;
using static Verse.DamageWorker;

namespace BANWlLib
{
    //按身体部位和自定义等级配置减伤与回复比例。
    public class DamageReductionCompProperties : CompProperties
    {
        // 受保护的身体部位，目前只支持单个部位
        // 比如 Torso、Head、LeftArm 等
        public string damageReductionBodyPart = "";

        // 属性值门槛，达到这些数值就能获得对应等级的减伤
        public List<int> customValueThresholds = new List<int> { 20, 40, 60, 80, 100 };

        // 每个等级对应的减伤比例
        public List<float> damageReductionRatios = new List<float> { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };

        //绑定减伤组件。
        public DamageReductionCompProperties()
        {
            this.compClass = typeof(DamageReductionComp);
        }
    }

    //处理指定部位的伤害减免。
    public class DamageReductionComp : ThingComp
    {
        //缓存上次检查的等级，等级不变时无需更新。
        private int lastKnownLevel = -1;
        private int lastUpdateTick = 0;
        
        //获取组件配置属性
        public DamageReductionCompProperties Props => (DamageReductionCompProperties)this.props;

        //判断当前组件父对象是否是已配置学生 Kind，让星级效果基于 PawnKindDef 生效。
        public bool IsStudentKindOwner()
        {
            return parent is Pawn pawn && StudentIdentityUtility.IsConfiguredStudentKind(pawn);
        }

        //根据自定义属性值计算减伤比例。
        public float GetDamageReductionRatio()
        {
            try
            {
                // 获取角色的自定义属性组件
                if (parent is Pawn pawn)
                {
                    if (!IsStudentKindOwner())
                    {
                        return 0f;
                    }

                    HumanIntPropertyComp customComp = pawn.GetComp<HumanIntPropertyComp>();
                    if (customComp != null)
                    {
                        int currentValue = customComp.CustomIntValue;

                        // 从最高等级开始检查，找到符合条件的最高等级
                        for (int i = Props.customValueThresholds.Count - 1; i >= 0; i--)
                        {
                            if (currentValue >= Props.customValueThresholds[i] && i < Props.damageReductionRatios.Count)
                            {
                                return Props.damageReductionRatios[i];
                            }
                        }
                    }
                }

                return 0f; // 如果没有达到任何阈值或没有组件，返回0减伤
            }
            catch
            {
                return 0f;
            }
        }

        //根据自定义属性值计算减伤等级。
        public int GetCurrentLevel()
        {
            try
            {
                // 获取角色的自定义属性组件
                if (parent is Pawn pawn)
                {
                    if (!IsStudentKindOwner())
                    {
                        return 0;
                    }

                    HumanIntPropertyComp customComp = pawn.GetComp<HumanIntPropertyComp>();
                    if (customComp != null)
                    {
                        int currentValue = customComp.CustomIntValue;

                        // 从高到低检查阈值，返回最高达到的等级
                        for (int i = Props.customValueThresholds.Count - 1; i >= 0; i--)
                        {
                            if (currentValue >= Props.customValueThresholds[i])
                            {
                                return i + 1; // 等级从1开始
                            }
                        }
                    }
                }

                return 0; // 如果没有达到任何阈值，返回0级
            }
            catch
            {
                return 0;
            }
        }

        //检查指定身体部位是否受到减伤保护
        public bool IsBodyPartProtected(BodyPartRecord bodyPart)
        {
            try
            {
                if (bodyPart?.def?.defName == null)
                {
                    return false;
                }

                return Props.damageReductionBodyPart == bodyPart.def.defName;
            }
            catch
            {
                return false;
            }
        }

        //按受伤部位和减伤比例计算伤害。
        public float CalculateReducedDamage(float originalDamage, BodyPartRecord bodyPart)
        {
            try
            {
                // 检查该部位是否受保护
                if (!IsBodyPartProtected(bodyPart))
                {
                    return originalDamage; // 不受保护的部位不减伤
                }

                // 获取减伤比例
                float reductionRatio = GetDamageReductionRatio();

                // 计算减伤后的伤害
                float reducedDamage = originalDamage * (1f - reductionRatio);

                return Mathf.Max(0f, reducedDamage);
            }
            catch
            {
                return originalDamage; // 如果计算出错，返回原始伤害
            }
        }

        //显示保护部位、减伤等级和回复比例。
        public override string CompInspectStringExtra()
        {
            try
            {
                if (!IsStudentKindOwner())
                {
                    return null;
                }

                if (string.IsNullOrEmpty(Props.damageReductionBodyPart))
                {
                    return null; // 没有配置部位时不显示
                }

                int currentLevel = GetCurrentLevel();
                float reductionRatio = GetDamageReductionRatio();

                if (currentLevel > 0)
                {
                    // 根据当前等级生成对应数量的五角星
                    string stars = new string('★', currentLevel);
                    return $"学生星级：{stars} (减伤{reductionRatio:P})";
                }
                else
                {
                    return $"星级系统: {Props.damageReductionBodyPart} 未激活";
                }
            }
            catch
            {
                return null;
            }
        }

        //初始化健康面板中的减伤状态。
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            UpdateHealthPanelStatus();
        }

        //定期检查等级，变化时更新健康面板。
        public override void CompTick()
        {
            base.CompTick();

            //每 300 tick 检查一次。
            int currentTick = Find.TickManager.TicksGame;
            if (currentTick - lastUpdateTick >= 300) // 5秒 = 300 ticks
            {
                int currentLevel = GetCurrentLevel();
                
                // 只在等级变化时更新健康面板状态
                if (currentLevel != lastKnownLevel)
                {
                    UpdateHealthPanelStatus();
                    lastKnownLevel = currentLevel;
                }
                
                lastUpdateTick = currentTick;
            }
        }

        //更新健康面板中的减伤状态显示
        private void UpdateHealthPanelStatus()
        {
            try
            {
                if (!IsStudentKindOwner())
                {
                    return;
                }

                if (parent is Pawn pawn && pawn.health?.hediffSet != null)
                {
                    // 检查是否已经有减伤状态Hediff
                    Hediff existingHediff = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.DamageReductionStatus);

                    if (existingHediff == null)
                    {
                        // 如果没有，添加一个
                        Hediff newHediff = HediffMaker.MakeHediff(HediffDefOf.DamageReductionStatus, pawn);
                        pawn.health.AddHediff(newHediff);
                    }
                }
            }
            catch
            {
                // 静默处理错误，避免影响游戏运行
            }
        }

        //替换 ApplyDamageToPart，接入部位减伤。
        [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
        public static class Patch_ApplyDamageToPart_Replacement
        {
            //处理伤害并返回 false，跳过原方法。
            [HarmonyPrefix]
            public static bool Prefix(DamageWorker_AddInjury __instance, DamageInfo dinfo, Pawn pawn, DamageResult result)
            {
                try
                {
                    //使用部位减伤实现。
                    ApplyDamageToPartCustom(__instance, dinfo, pawn, result);
                    
                    // 返回 false 跳过原方法
                    return false;
                }
                catch (Exception ex)
                {
                    Log.Error($"[减伤系统] ApplyDamageToPart 替换方法执行出错: {ex.Message}");
                    // 如果出错，让原方法继续执行
                    return true;
                }
            }

           
            private static void ApplyDamageToPartCustom(DamageWorker_AddInjury worker, DamageInfo dinfo, Pawn pawn, DamageResult result)
            {
                BodyPartRecord exactPartFromDamageInfo = GetExactPartFromDamageInfo(dinfo, pawn);
                if (exactPartFromDamageInfo == null)
                {
                    return;
                }

                //设置受伤部位
                dinfo.SetHitPart(exactPartFromDamageInfo);

                //应用减伤逻辑
                DamageReductionComp reductionComp = pawn?.GetComp<DamageReductionComp>();
                if (reductionComp != null && reductionComp.IsStudentKindOwner() && reductionComp.IsBodyPartProtected(dinfo.HitPart))
                {
                                     // 计算减伤后的伤害
                    float reducedDamage = reductionComp.CalculateReducedDamage(dinfo.Amount, dinfo.HitPart);               
                    dinfo.SetAmount(reducedDamage);
                    
                }

                //继续原版的伤害处理逻辑
                float injuryDamage = dinfo.Amount;
                float beforeArmorDamage = injuryDamage;
                float afterArmorDamage = injuryDamage;
                bool shouldCheckArmor = !dinfo.InstantPermanentInjury && !dinfo.IgnoreArmor;
                bool deflectedByMetalArmor = false;
                if (shouldCheckArmor)
                {
                    DamageDef damageDef = dinfo.Def;
                    injuryDamage = ArmorUtility.GetPostArmorDamage(pawn, injuryDamage, dinfo.ArmorPenetrationInt, dinfo.HitPart, ref damageDef, out deflectedByMetalArmor, out var diminishedByMetalArmor);
                    dinfo.Def = damageDef;
                    if (injuryDamage < dinfo.Amount)
                    {
                        result.diminished = true;
                        result.diminishedByMetalArmor = diminishedByMetalArmor;
                    }
                }
                afterArmorDamage = injuryDamage;

                // 承伤系数计算
                float incomingDamageFactor = 1f;
                if (dinfo.Def.ExternalViolenceFor(pawn) && !DamageFontRuleUtility.ShouldSkipIncomingDamageFactor(dinfo.Def))
                {
                    incomingDamageFactor = pawn.GetStatValue(StatDefOf.IncomingDamageFactor);
                    injuryDamage *= incomingDamageFactor;
                }

                //星野护盾与实际扣血采用相同的减伤结果，完整吸收破盾的这一击。
                if (BANWlLib.Skills.HediffComp_HoshinoShield.TryAbsorb(pawn, dinfo, injuryDamage))
                    injuryDamage = 0f;
                BattleFormulaDebugUtility.LogDamagePipeline(pawn, dinfo, dinfo.HitPart, beforeArmorDamage, afterArmorDamage, incomingDamageFactor, injuryDamage, deflectedByMetalArmor);

                // 检查是否完全格挡
                if (injuryDamage <= 0f)
                {
                    result.AddPart(pawn, dinfo.HitPart);
                    result.deflected = true;
                    result.deflectedByMetalArmor = deflectedByMetalArmor;
                    return;
                }

                // 检查是否爆头
                if (IsHeadshot(dinfo, pawn))
                {
                    result.headshot = true;
                }

                // 应用伤害
                if (!dinfo.InstantPermanentInjury || (HealthUtility.GetHediffDefFromDamage(dinfo.Def, pawn, dinfo.HitPart).CompPropsFor(typeof(HediffComp_GetsPermanent)) != null && dinfo.HitPart.def.permanentInjuryChanceFactor != 0f && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(dinfo.HitPart)))
                {
                    if (!dinfo.AllowDamagePropagation)
                    {
                        FinalizeAndAddInjury(worker, pawn, injuryDamage, dinfo, result);
                    }
                    else
                    {
                        ApplySpecialEffectsToPart(worker, pawn, injuryDamage, dinfo, result);
                    }
                }
            }

            //获取确切的受伤部位（复制原版逻辑）
            private static BodyPartRecord GetExactPartFromDamageInfo(DamageInfo dinfo, Pawn pawn)
            {
                if (dinfo.HitPart != null)
                {
                    if (!pawn.health.hediffSet.GetNotMissingParts().Any((BodyPartRecord x) => x == dinfo.HitPart))
                    {
                        return null;
                    }
                    return dinfo.HitPart;
                }

                BodyPartRecord bodyPartRecord = pawn.health.hediffSet.GetRandomNotMissingPart(dinfo.Def, dinfo.Height, dinfo.Depth);
                if (bodyPartRecord == null)
                {
                }
                return bodyPartRecord;
            }

            //检查是否爆头（复制原版逻辑）
            private static bool IsHeadshot(DamageInfo dinfo, Pawn pawn)
            {
                if (dinfo.InstantPermanentInjury)
                {
                    return false;
                }

                if (dinfo.HitPart.groups.Contains(BodyPartGroupDefOf.FullHead))
                {
                    return dinfo.Def.isRanged;
                }

                return false;
            }

            //应用特殊效果到部位（复制原版逻辑）
            private static void ApplySpecialEffectsToPart(DamageWorker_AddInjury worker, Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
            {
                // 使用反射调用原版的受保护方法
                var method = typeof(DamageWorker_AddInjury).GetMethod("ReduceDamageToPreserveOutsideParts", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    totalDamage = (float)method.Invoke(worker, new object[] { totalDamage, dinfo, pawn });
                }

                FinalizeAndAddInjury(worker, pawn, totalDamage, dinfo, result);
                CheckDuplicateDamageToOuterParts(worker, dinfo, pawn, totalDamage, result);
            }

            //最终化并添加伤害（使用反射调用原版方法）
            private static void FinalizeAndAddInjury(DamageWorker_AddInjury worker, Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
            {
                var method = typeof(DamageWorker_AddInjury).GetMethod("FinalizeAndAddInjury", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Pawn), typeof(float), typeof(DamageInfo), typeof(DamageResult) }, null);
                if (method != null)
                {
                    method.Invoke(worker, new object[] { pawn, totalDamage, dinfo, result });
                }
            }

            //检查外部部位重复伤害（使用反射调用原版方法）
            private static void CheckDuplicateDamageToOuterParts(DamageWorker_AddInjury worker, DamageInfo dinfo, Pawn pawn, float totalDamage, DamageResult result)
            {
                var method = typeof(DamageWorker_AddInjury).GetMethod("CheckDuplicateDamageToOuterParts", BindingFlags.NonPublic | BindingFlags.Instance);
                if (method != null)
                {
                    method.Invoke(worker, new object[] { dinfo, pawn, totalDamage, result });
                }
            }
        }
    }
}
