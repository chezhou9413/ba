using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using static Verse.DamageWorker;

namespace BANWlLib
{
    /// <summary>
    /// 减伤组件配置属性类
    /// 定义减伤的部位和基于自定义属性值的分级减伤系统
    /// 新功能：减伤回复系统 - 当配置的部位受到伤害时，会根据减伤比例进行回复
    /// </summary>
    public class DamageReductionCompProperties : CompProperties
    {
        // 受保护的身体部位，目前只支持单个部位
        // 比如 Torso、Head、LeftArm 等
        public string damageReductionBodyPart = "";

        // 属性值门槛，达到这些数值就能获得对应等级的减伤
        public List<int> customValueThresholds = new List<int> { 20, 40, 60, 80, 100 };

        // 每个等级对应的减伤比例
        public List<float> damageReductionRatios = new List<float> { 0.05f, 0.10f, 0.15f, 0.20f, 0.25f };

        /// <summary>
        /// 构造函数
        /// 告诉游戏这个配置对应哪个组件类
        /// </summary>
        public DamageReductionCompProperties()
        {
            this.compClass = typeof(DamageReductionComp);
        }
    }

    /// <summary>
    /// 减伤组件实现类
    /// 为角色提供指定部位的伤害减免功能
    /// </summary>
    public class DamageReductionComp : ThingComp
    {
        // 性能优化：缓存上次检查的等级，避免重复更新
        private int lastKnownLevel = -1;
        private int lastUpdateTick = 0;
        
        /// <summary>
        /// 获取组件配置属性
        /// </summary>
        public DamageReductionCompProperties Props => (DamageReductionCompProperties)this.props;

        /// <summary>
        /// 获取当前减伤比例
        /// 根据玩家的自定义属性值计算减伤比例
        /// </summary>
        public float GetDamageReductionRatio()
        {
            try
            {
                // 获取角色的自定义属性组件
                if (parent is Pawn pawn)
                {
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

        /// <summary>
        /// 获取当前减伤等级
        /// 根据玩家的自定义属性值计算当前等级
        /// </summary>
        public int GetCurrentLevel()
        {
            try
            {
                // 获取角色的自定义属性组件
                if (parent is Pawn pawn)
                {
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

        /// <summary>
        /// 检查指定身体部位是否受到减伤保护
        /// </summary>
        /// <param name="bodyPart">身体部位对象</param>
        /// <returns>是否受保护</returns>
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

        /// <summary>
        /// 计算实际伤害值
        /// 根据减伤比例计算减伤后的伤害
        /// </summary>
        /// <param name="originalDamage">原始伤害值</param>
        /// <param name="bodyPart">受伤部位</param>
        /// <returns>减伤后的伤害值</returns>
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

                // 确保伤害不会变成负数
                return Mathf.Max(0f, reducedDamage);
            }
            catch
            {
                return originalDamage; // 如果计算出错，返回原始伤害
            }
        }

        /// <summary>
        /// 在角色信息面板中显示减伤回复系统的状态
        /// 显示当前保护的部位、减伤等级和回复比例
        /// </summary>
        public override string CompInspectStringExtra()
        {
            try
            {
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

        /// <summary>
        /// 组件初始化时调用
        /// 添加减伤状态到健康面板
        /// </summary>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            // 确保角色有减伤状态显示
            UpdateHealthPanelStatus();
        }

        /// <summary>
        /// 每帧更新时调用
        /// 保持健康面板状态同步
        /// 性能优化：只在减伤等级变化时更新，减少不必要的计算
        /// </summary>
        public override void CompTick()
        {
            base.CompTick();

            // 性能优化：每5秒检查一次，而不是每秒
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

        /// <summary>
        /// 更新健康面板中的减伤状态显示
        /// </summary>
        private void UpdateHealthPanelStatus()
        {
            try
            {
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

        /// <summary>
        /// Harmony 补丁：完全替换 ApplyDamageToPart 方法
        /// 使用 Prefix 返回 false 跳过原方法，用我们自己的逻辑处理伤害
        /// </summary>
        [HarmonyPatch(typeof(DamageWorker_AddInjury), "ApplyDamageToPart")]
        public static class Patch_ApplyDamageToPart_Replacement
        {
            /// <summary>
            /// 替换原方法，返回 false 跳过原方法执行
            /// </summary>
            [HarmonyPrefix]
            public static bool Prefix(DamageWorker_AddInjury __instance, DamageInfo dinfo, Pawn pawn, DamageResult result)
            {
                try
                {
                    // 调用我们自己的 ApplyDamageToPart 实现
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
                if (reductionComp != null && reductionComp.IsBodyPartProtected(dinfo.HitPart))
                {
                                     // 计算减伤后的伤害
                    float reducedDamage = reductionComp.CalculateReducedDamage(dinfo.Amount, dinfo.HitPart);               
                    dinfo.SetAmount(reducedDamage);
                    
                }

                //继续原版的伤害处理逻辑
                float damageAmount = dinfo.Amount;
                bool shouldCheckArmor = !dinfo.InstantPermanentInjury && !dinfo.IgnoreArmor;
                bool deflectedByMetalArmor = false;
                if (shouldCheckArmor)
                {
                    DamageDef damageDef = dinfo.Def;
                    damageAmount = ArmorUtility.GetPostArmorDamage(pawn, damageAmount, dinfo.ArmorPenetrationInt, dinfo.HitPart, ref damageDef, out deflectedByMetalArmor, out var diminishedByMetalArmor);
                    dinfo.Def = damageDef;
                    if (damageAmount < dinfo.Amount)
                    {
                        result.diminished = true;
                        result.diminishedByMetalArmor = diminishedByMetalArmor;
                    }
                }

                // 承伤系数计算
                if (dinfo.Def.ExternalViolenceFor(pawn))
                {
                    damageAmount *= pawn.GetStatValue(StatDefOf.IncomingDamageFactor);
                }

                // 检查是否完全格挡
                if (damageAmount <= 0f)
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
                        FinalizeAndAddInjury(worker, pawn, damageAmount, dinfo, result);
                    }
                    else
                    {
                        ApplySpecialEffectsToPart(worker, pawn, damageAmount, dinfo, result);
                    }
                }
            }

            /// <summary>
            /// 获取确切的受伤部位（复制原版逻辑）
            /// </summary>
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

            /// <summary>
            /// 检查是否爆头（复制原版逻辑）
            /// </summary>
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

            /// <summary>
            /// 应用特殊效果到部位（复制原版逻辑）
            /// </summary>
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

            /// <summary>
            /// 最终化并添加伤害（使用反射调用原版方法）
            /// </summary>
            private static void FinalizeAndAddInjury(DamageWorker_AddInjury worker, Pawn pawn, float totalDamage, DamageInfo dinfo, DamageResult result)
            {
                var method = typeof(DamageWorker_AddInjury).GetMethod("FinalizeAndAddInjury", BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(Pawn), typeof(float), typeof(DamageInfo), typeof(DamageResult) }, null);
                if (method != null)
                {
                    method.Invoke(worker, new object[] { pawn, totalDamage, dinfo, result });
                }
            }

            /// <summary>
            /// 检查外部部位重复伤害（使用反射调用原版方法）
            /// </summary>
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