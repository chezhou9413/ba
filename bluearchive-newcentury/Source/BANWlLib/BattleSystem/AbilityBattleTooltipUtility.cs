using System.Collections.Generic;
using System.Text;
using BANWlLib.BaDef;
using BANWlLib.BaVerb;
using BANWlLib.comp;
using BANWlLib.Pojo;
using BANWlLib.Projectiles;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.BattleSystem
{
    // 技能战斗悬浮工具，负责把配置化战斗参数格式化为地图技能按钮的说明文本。
    public static class AbilityBattleTooltipUtility
    {
        private static readonly Color DamageColor = new Color(1f, 0.35f, 0.28f);
        private static readonly Color HealColor = new Color(0.35f, 1f, 0.45f);
        private static readonly Color ShieldColor = new Color(0.35f, 0.86f, 1f);
        private static readonly Color ExColor = new Color(1f, 0.82f, 0.22f);
        private static readonly Color CritColor = new Color(1f, 0.58f, 0.18f);
        private static readonly Color AffinityColor = new Color(0.35f, 0.72f, 1f);
        private static readonly Color DisabledColor = ColorLibrary.Grey;
        private const int CompactActionThreshold = 8;
        private const int CompactGroupLimit = 10;
        private const int CompactHeadGroupCount = 6;
        private const int CompactTailGroupCount = 2;

        // 构建技能公式悬浮文本，负责根据当前 Pawn 的实时属性给出预估数值。
        public static string BuildTooltip(RimWorld.Ability ability)
        {
            if (ability?.def == null || ability.pawn == null)
            {
                return string.Empty;
            }

            Pawn pawn = ability.pawn;
            List<BattleActionConfig> actions = ResolvePreviewActions(ability.def, out bool hasAutomaticActions);
            if (actions.NullOrEmpty())
            {
                return string.Empty;
            }

            AbilityBattleTooltipExtension extension = ability.def.GetModExtension<AbilityBattleTooltipExtension>();
            if (!hasAutomaticActions && (extension == null || !extension.showBattleFormula))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("BA战斗公式".Colorize(ColoredText.TipSectionTitleColor));
            AppendCasterStats(builder, pawn);

            if (ShouldUseCompactActions(actions))
            {
                AppendCompactActions(builder, pawn, actions);
                return builder.ToString();
            }

            for (int i = 0; i < actions.Count;)
            {
                BattleActionConfig action = actions[i];
                if (action == null)
                {
                    i++;
                    continue;
                }

                int repeatCount = CountSameActions(actions, i, action);
                builder.AppendLine();
                builder.AppendLine(FormatSegmentTitle(i, repeatCount, action).Colorize(ColoredText.TipSectionTitleColor));
                if (action.isShield)
                {
                    AppendShieldAction(builder, pawn, action);
                }
                else if (action.isHealing)
                {
                    AppendHealAction(builder, ability.def, pawn, action);
                }
                else
                {
                    AppendDamageAction(builder, pawn, action);
                }

                i += repeatCount;
            }

            return builder.ToString();
        }

        // 判断是否使用紧凑显示，负责避免多段技能把悬浮说明撑得过长。
        private static bool ShouldUseCompactActions(List<BattleActionConfig> actions)
        {
            return actions != null && actions.Count >= CompactActionThreshold;
        }

        // 判断治疗技能是否只会作用于自身，负责让 tooltip 在自疗场景直接显示最终治疗量。
        private static bool IsSelfOnlyHealAbility(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return false;
            }

            bool foundGiveHediff = false;
            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                if (!(abilityDef.comps[i] is CompProperties_AbilityGiveHediff giveHediff))
                {
                    continue;
                }

                HediffCompProps_Regeneration regeneration = giveHediff.hediffDef?.CompProps<HediffCompProps_Regeneration>();
                if (regeneration == null || regeneration.healPowerRatio <= 0f)
                {
                    continue;
                }

                foundGiveHediff = true;
                if (!giveHediff.onlyApplyToSelf)
                {
                    return false;
                }
            }

            return foundGiveHediff;
        }

        //解析预览战斗段，负责让投射物、场地和范围 Job 技能优先读取真实配置，避免悬浮预估和实际结算分离。
        private static List<BattleActionConfig> ResolvePreviewActions(AbilityDef abilityDef, out bool hasAutomaticActions)
        {
            List<BattleActionConfig> multiShotProjectileActions = TryBuildMultiShotProjectileActions(abilityDef);
            if (multiShotProjectileActions != null)
            {
                hasAutomaticActions = true;
                return multiShotProjectileActions;
            }

            List<BattleActionConfig> projectileActions = TryBuildProjectileActions(abilityDef);
            if (projectileActions != null)
            {
                hasAutomaticActions = true;
                return projectileActions;
            }

            List<BattleActionConfig> fieldActions = TryBuildBattleFieldActions(abilityDef);
            if (fieldActions != null)
            {
                hasAutomaticActions = true;
                return fieldActions;
            }

            List<BattleActionConfig> jobActions = TryBuildJobActions(abilityDef);
            if (jobActions != null)
            {
                hasAutomaticActions = true;
                return jobActions;
            }

            List<BattleActionConfig> hediffHealActions = TryBuildGiveHediffHealActions(abilityDef);
            if (hediffHealActions != null)
            {
                hasAutomaticActions = true;
                return hediffHealActions;
            }

            hasAutomaticActions = false;
            return abilityDef.GetModExtension<AbilityBattleTooltipExtension>()?.previewActions;
        }

        //从技能附加的再生 Hediff 构建预览段，负责让持续治疗技能自动显示单次或多次触发的治愈量公式。
        private static List<BattleActionConfig> TryBuildGiveHediffHealActions(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return null;
            }

            List<BattleActionConfig> actions = new List<BattleActionConfig>();
            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                CompProperties_AbilityGiveHediff giveHediff = abilityDef.comps[i] as CompProperties_AbilityGiveHediff;
                HediffCompProps_Regeneration regeneration = giveHediff?.hediffDef?.CompProps<HediffCompProps_Regeneration>();
                if (regeneration == null || regeneration.healPowerRatio <= 0f)
                {
                    continue;
                }

                BattleActionConfig action = new BattleActionConfig
                {
                    isHealing = true,
                    healPowerRatio = regeneration.healPowerRatio,
                    canCrit = false,
                    alwaysShowHealText = regeneration.alwaysShowHealText,
                    affectFriendly = true,
                    affectHostile = false,
                    allowPermanentInjuryHealing = regeneration.isHeatScar,
                    isExSkill = regeneration.isExSkill
                };

                int triggerCount = ResolveRegenerationTriggerCount(giveHediff.hediffDef, regeneration);
                for (int repeatIndex = 0; repeatIndex < triggerCount; repeatIndex++)
                {
                    actions.Add(action);
                }
            }

            return actions.Count > 0 ? actions : null;
        }

        //解析持续治疗触发次数，负责在 Hediff 明确写出持续时间时把悬浮预估展开为总触发段数。
        private static int ResolveRegenerationTriggerCount(HediffDef hediffDef, HediffCompProps_Regeneration regeneration)
        {
            if (hediffDef == null || regeneration == null || regeneration.healIntervalTicks <= 0)
            {
                return 1;
            }

            HediffCompProperties_Disappears disappears = hediffDef.CompProps<HediffCompProperties_Disappears>();
            int durationTicks = disappears?.disappearsAfterTicks.min ?? 0;
            if (durationTicks <= 0)
            {
                return 1;
            }

            return Mathf.Max(1, durationTicks / regeneration.healIntervalTicks);
        }

        //从范围 Job 构建预览段，负责支持圆形脱手、持续圆形、扇形和直线 AOE 技能自动显示预计伤害。
        private static List<BattleActionConfig> TryBuildJobActions(AbilityDef abilityDef)
        {
            JobDef jobDef = ResolveVerbJobDef(abilityDef);
            if (jobDef is BaJobDef_SphereAreaAttack sphereAreaAttack)
            {
                return BuildSphereAreaActions(sphereAreaAttack);
            }

            if (jobDef is BaJobDef_SustainedAttack sustainedAttack)
            {
                return BuildSustainedActions(sustainedAttack);
            }

            return null;
        }

        // 解析技能动词绑定的 JobDef，负责从不同 AOE VerbProperties 中取得真实执行 Job。
        private static JobDef ResolveVerbJobDef(AbilityDef abilityDef)
        {
            if (abilityDef?.verbProperties is VerbProperties_SphereArea sphereArea)
            {
                return sphereArea.JobDef;
            }

            if (abilityDef?.verbProperties is VerbProperties_SustainedAreaAttack sustainedArea)
            {
                return sustainedArea.JobDef;
            }

            if (abilityDef?.verbProperties is VerbProperties_SustainedAreaAttackBox sustainedBox)
            {
                return sustainedBox.JobDef;
            }

            return null;
        }

        // 从圆形脱手 Job 构建预览段，负责展开主 tick 和延迟子段中的伤害配置。
        private static List<BattleActionConfig> BuildSphereAreaActions(BaJobDef_SphereAreaAttack jobDef)
        {
            if (jobDef?.damages == null || jobDef.damages.Count == 0)
            {
                return null;
            }

            List<BattleActionConfig> actions = new List<BattleActionConfig>();
            for (int i = 0; i < jobDef.damages.Count; i++)
            {
                TickDelayDamageAndHediff tickGroup = jobDef.damages[i];
                if (tickGroup?.damages == null)
                {
                    continue;
                }

                for (int j = 0; j < tickGroup.damages.Count; j++)
                {
                    AddPreviewAction(actions, tickGroup.damages[j]?.ToBattleAction());
                }
            }

            return actions.Count > 0 ? actions : null;
        }

        // 从持续 AOE Job 构建预览段，负责读取持续时间轴上的每段伤害或治疗。
        private static List<BattleActionConfig> BuildSustainedActions(BaJobDef_SustainedAttack jobDef)
        {
            if (jobDef?.damages == null || jobDef.damages.Count == 0)
            {
                return null;
            }

            List<BattleActionConfig> actions = new List<BattleActionConfig>();
            for (int i = 0; i < jobDef.damages.Count; i++)
            {
                AddPreviewAction(actions, jobDef.damages[i]?.ToBattleAction());
            }

            return actions.Count > 0 ? actions : null;
        }

        // 添加有效预览段，负责过滤纯特效或纯状态段，避免 tooltip 显示空伤害。
        private static void AddPreviewAction(List<BattleActionConfig> actions, BattleActionConfig action)
        {
            if (actions == null || action == null)
            {
                return;
            }

            if (action.isHealing)
            {
                if (action.healPowerRatio <= 0f)
                {
                    return;
                }
            }
            else if (action.isShield)
            {
                if (action.shieldPowerRatio <= 0f || action.shieldHediffDef == null)
                {
                    return;
                }
            }
            else if (action.damageDef == null || (action.isNormalAttack ? action.normalAttackMultiplier <= 0f : action.attackPowerRatio <= 0f))
            {
                return;
            }

            actions.Add(action);
        }

        //从场地脱手技能构建预览段，负责把 BattleFieldControllerExtension 的 actions 同步到技能公式显示。
        private static List<BattleActionConfig> TryBuildBattleFieldActions(AbilityDef abilityDef)
        {
            ThingDef fieldThingDef = FindBattleFieldThingDef(abilityDef);
            if (fieldThingDef == null)
            {
                return null;
            }

            BattleFieldControllerExtension extension = fieldThingDef.GetModExtension<BattleFieldControllerExtension>();
            if (extension?.actions == null || extension.actions.Count == 0)
            {
                return null;
            }

            // 复制一份避免修改原始配置。
            List<BattleActionConfig> copies = new List<BattleActionConfig>();
            for (int i = 0; i < extension.actions.Count; i++)
            {
                BattleActionConfig source = extension.actions[i];
                copies.Add(new BattleActionConfig
                {
                    attackPowerRatio = source.attackPowerRatio,
                    normalAttackMultiplier = source.normalAttackMultiplier,
                    baseMasteryMultiplier = source.baseMasteryMultiplier,
                    healPowerRatio = source.healPowerRatio,
                    shieldPowerRatio = source.shieldPowerRatio,
                    damageDef = source.damageDef,
                    triggerHediff = source.triggerHediff,
                    shieldHediffDef = source.shieldHediffDef,
                    effecterDef = source.effecterDef,
                    penetration = source.penetration,
                    isHealing = source.isHealing,
                    isShield = source.isShield,
                    isNormalAttack = source.isNormalAttack,
                    canCrit = source.canCrit,
                    alwaysCrit = source.alwaysCrit,
                    alwaysShowCriticalText = source.alwaysShowCriticalText,
                    alwaysShowHealText = source.alwaysShowHealText,
                    applyAffinity = source.applyAffinity,
                    canHitBuilding = source.canHitBuilding,
                    canHitOwnBuilding = source.canHitOwnBuilding,
                    canHitOwnPawn = source.canHitOwnPawn,
                    affectHostile = source.affectHostile,
                    affectFriendly = source.affectFriendly,
                    allowPermanentInjuryHealing = source.allowPermanentInjuryHealing,
                    isExSkill = source.isExSkill,
                    previewWeaponBaseAttack = source.previewWeaponBaseAttack
                });
            }

            return copies;
        }

        //查找技能生成的场地控制器 ThingDef，负责支持 CompProperties_AbilitySpawnBattleField 配置。
        private static ThingDef FindBattleFieldThingDef(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return null;
            }

            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                CompProperties_AbilitySpawnBattleField spawnBattleField = abilityDef.comps[i] as CompProperties_AbilitySpawnBattleField;
                if (spawnBattleField?.fieldThingDef != null)
                {
                    return spawnBattleField.fieldThingDef;
                }
            }

            return null;
        }

        // 从技能投射物构建预览段，负责把直线弹、普通弹和多段追加伤害同步到技能公式显示。
        private static List<BattleActionConfig> TryBuildProjectileActions(AbilityDef abilityDef)
        {
            ThingDef projectileDef = FindLaunchProjectileDef(abilityDef);
            if (projectileDef == null)
            {
                return null;
            }

            return TryBuildProjectileActions(projectileDef);
        }

        // 从投射物 Def 构建预览段，负责让单发技能和延迟多发技能复用同一套投射物公式解析。
        private static List<BattleActionConfig> TryBuildProjectileActions(ThingDef projectileDef)
        {
            BattleActionConfig piercingAction = TryBuildPiercingProjectileAction(projectileDef);
            if (piercingAction != null)
            {
                List<BattleActionConfig> piercingActions = new List<BattleActionConfig> { piercingAction };
                AddMultiHitPreviewActions(projectileDef, piercingActions);
                return piercingActions;
            }

            List<BattleActionConfig> normalProjectileActions = TryBuildNormalProjectileActions(projectileDef);
            return normalProjectileActions;
        }

        // 从延迟多发投射物组件构建预览段，负责把每发子弹的 BA 战斗配置展开到悬浮说明。
        private static List<BattleActionConfig> TryBuildMultiShotProjectileActions(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return null;
            }

            List<BattleActionConfig> actions = new List<BattleActionConfig>();
            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                CompProperties_AbilityMultiShotProjectile multiShot = abilityDef.comps[i] as CompProperties_AbilityMultiShotProjectile;
                if (multiShot?.shots == null)
                {
                    continue;
                }

                for (int shotIndex = 0; shotIndex < multiShot.shots.Count; shotIndex++)
                {
                    ThingDef projectileDef = multiShot.shots[shotIndex]?.projectileDef ?? multiShot.projectileDef;
                    List<BattleActionConfig> projectileActions = TryBuildProjectileActions(projectileDef);
                    if (projectileActions.NullOrEmpty())
                    {
                        continue;
                    }

                    actions.AddRange(projectileActions);
                }
            }

            return actions.Count > 0 ? actions : null;
        }

        // 从普通投射物构建预览段，负责把原始子弹伤害和命中追加多段伤害按真实配置展开。
        private static List<BattleActionConfig> TryBuildNormalProjectileActions(ThingDef projectileDef)
        {
            BattleProjectileExtension battleExtension = projectileDef?.GetModExtension<BattleProjectileExtension>();
            ProjectileMultiHitExtension multiHitExtension = projectileDef?.GetModExtension<ProjectileMultiHitExtension>();
            if (battleExtension == null && multiHitExtension?.extraDamages.NullOrEmpty() != false)
            {
                return null;
            }

            List<BattleActionConfig> actions = new List<BattleActionConfig>();
            float weaponBaseAttack = projectileDef.projectile?.GetDamageAmount(null) ?? 0f;
            if (battleExtension != null)
            {
                AddPreviewAction(actions, new BattleActionConfig
                {
                    attackPowerRatio = battleExtension.attackPowerRatio,
                    normalAttackMultiplier = battleExtension.normalAttackMultiplier,
                    baseMasteryMultiplier = battleExtension.baseMasteryMultiplier,
                    damageDef = projectileDef.projectile?.damageDef,
                    penetration = projectileDef.projectile?.GetArmorPenetration() ?? 0f,
                    isNormalAttack = battleExtension.isNormalAttack,
                    canCrit = battleExtension.canCrit,
                    alwaysShowCriticalText = battleExtension.alwaysShowCriticalText,
                    applyAffinity = battleExtension.applyAffinity,
                    canHitOwnBuilding = battleExtension.canHitOwnBuilding,
                    canHitOwnPawn = battleExtension.canHitOwnPawn,
                    isExSkill = battleExtension.isExSkill,
                    previewWeaponBaseAttack = weaponBaseAttack
                });
            }

            if (multiHitExtension?.extraDamages != null)
            {
                for (int i = 0; i < multiHitExtension.extraDamages.Count; i++)
                {
                    AddPreviewAction(actions, BuildExtraProjectilePreviewAction(projectileDef, multiHitExtension, multiHitExtension.extraDamages[i], weaponBaseAttack));
                }
            }

            return actions.Count > 0 ? actions : null;
        }

        // 追加投射物多段预览，负责让穿透弹和普通弹共用同一套额外伤害显示。
        private static void AddMultiHitPreviewActions(ThingDef projectileDef, List<BattleActionConfig> actions)
        {
            ProjectileMultiHitExtension multiHitExtension = projectileDef?.GetModExtension<ProjectileMultiHitExtension>();
            if (multiHitExtension?.extraDamages == null || actions == null)
            {
                return;
            }

            float weaponBaseAttack = projectileDef.projectile?.GetDamageAmount(null) ?? 0f;
            for (int i = 0; i < multiHitExtension.extraDamages.Count; i++)
            {
                AddPreviewAction(actions, BuildExtraProjectilePreviewAction(projectileDef, multiHitExtension, multiHitExtension.extraDamages[i], weaponBaseAttack));
            }
        }

        // 从投射物追加伤害配置构建预览段，负责复用多段子弹运行时的字段语义。
        private static BattleActionConfig BuildExtraProjectilePreviewAction(ThingDef projectileDef, ProjectileMultiHitExtension extension, ProjectileExtraDamageConfig config, float weaponBaseAttack)
        {
            if (config == null)
            {
                return null;
            }

            return new BattleActionConfig
            {
                attackPowerRatio = config.attackPowerRatio,
                normalAttackMultiplier = config.normalAttackMultiplier,
                baseMasteryMultiplier = config.baseMasteryMultiplier,
                damageDef = config.ResolveDamageDef(),
                penetration = config.penetration >= 0f ? config.penetration : projectileDef.projectile?.GetArmorPenetration() ?? 0f,
                isNormalAttack = config.isNormalAttack,
                canCrit = config.canCrit,
                alwaysCrit = config.alwaysCrit,
                alwaysShowCriticalText = config.alwaysShowCriticalText,
                applyAffinity = config.applyAffinity,
                canHitOwnBuilding = config.canHitOwnBuilding || extension?.canHitOwnBuilding == true,
                canHitOwnPawn = config.canHitOwnPawn || extension?.canHitOwnPawn == true,
                isExSkill = config.isExSkill,
                previewWeaponBaseAttack = weaponBaseAttack
            };
        }

        //从直线穿透弹构建预览段，负责把 ThingDef 扩展里的真实伤害参数同步到技能公式显示。
        private static BattleActionConfig TryBuildPiercingProjectileAction(ThingDef projectileDef)
        {
            PiercingProjectileExtension extension = projectileDef?.GetModExtension<PiercingProjectileExtension>();
            if (extension == null)
            {
                return null;
            }

            return new BattleActionConfig
            {
                attackPowerRatio = extension.attackPowerRatio,
                normalAttackMultiplier = extension.normalAttackMultiplier,
                baseMasteryMultiplier = extension.baseMasteryMultiplier,
                damageDef = projectileDef.projectile?.damageDef,
                penetration = projectileDef.projectile?.GetArmorPenetration() ?? 0f,
                canCrit = extension.canCrit,
                alwaysCrit = extension.alwaysCrit,
                alwaysShowCriticalText = extension.alwaysShowCriticalText,
                applyAffinity = extension.applyAffinity,
                canHitBuilding = extension.canHitBuilding,
                affectHostile = extension.affectHostile,
                affectFriendly = extension.affectFriendly,
                isExSkill = extension.isExSkill,
                isProjectilePreview = true,
                previewWeaponBaseAttack = projectileDef.projectile?.GetDamageAmount(null) ?? 0f
            };
        }

        //查找技能发射的投射物，负责支持原版 CompProperties_AbilityLaunchProjectile 配置。
        private static ThingDef FindLaunchProjectileDef(AbilityDef abilityDef)
        {
            if (abilityDef?.comps == null)
            {
                return null;
            }

            for (int i = 0; i < abilityDef.comps.Count; i++)
            {
                CompProperties_AbilityLaunchProjectile launchProjectile = abilityDef.comps[i] as CompProperties_AbilityLaunchProjectile;
                if (launchProjectile?.projectileDef != null)
                {
                    return launchProjectile.projectileDef;
                }
            }

            return null;
        }

        //统计连续相同战斗段，负责让多段相同伤害在悬浮说明里合并显示。
        private static int CountSameActions(List<BattleActionConfig> actions, int startIndex, BattleActionConfig action)
        {
            int count = 1;
            for (int i = startIndex + 1; i < actions.Count; i++)
            {
                if (!IsSamePreviewAction(action, actions[i]))
                {
                    break;
                }

                count++;
            }

            return count;
        }

        // 写入紧凑多段说明，负责用总览和短行展示高段数技能的关键战斗信息。
        private static void AppendCompactActions(StringBuilder builder, Pawn pawn, List<BattleActionConfig> actions)
        {
            CompactActionTotals totals = CalculateCompactTotals(pawn, actions);
            builder.AppendLine();
            builder.AppendLine(("多段总览（" + actions.Count + "段）").Colorize(ColoredText.TipSectionTitleColor));
            if (totals.damageCount > 0)
            {
                builder.AppendLine("伤害段：" + totals.damageCount + "段，倍率合计 " + FormatColor(FormatPercent(totals.damageRatioTotal), DamageColor) + "，基础预估合计 " + FormatColor(FormatNumber(totals.damageTotal), DamageColor));
            }

            if (totals.healCount > 0)
            {
                builder.AppendLine("治疗段：" + totals.healCount + "段，倍率合计 " + FormatColor(FormatPercent(totals.healRatioTotal), HealColor) + "，单次/多次基础预估合计 " + FormatColor(FormatNumber(totals.healTotal), HealColor));
            }

            if (totals.shieldCount > 0)
            {
                builder.AppendLine("护盾段：" + totals.shieldCount + "段，倍率合计 " + FormatColor(FormatPercent(totals.shieldRatioTotal), ShieldColor) + "，预估合计 " + FormatColor(FormatNumber(totals.shieldTotal), ShieldColor));
            }

            builder.AppendLine("修正：" + FormatCompactModifiers(totals));
            builder.AppendLine(FormatFormulaHint(totals.applyAffinity));
            builder.AppendLine();
            builder.AppendLine("段落明细".Colorize(ColoredText.TipSectionTitleColor));

            List<CompactActionGroup> groups = BuildCompactGroups(actions);
            for (int i = 0; i < groups.Count; i++)
            {
                if (ShouldSkipCompactGroup(groups.Count, i))
                {
                    if (i == CompactHeadGroupCount)
                    {
                        builder.AppendLine(FormatColor("  ……中间 " + (groups.Count - CompactHeadGroupCount - CompactTailGroupCount) + " 组已折叠", ColoredText.SubtleGrayColor));
                    }

                    continue;
                }

                AppendCompactGroupLine(builder, pawn, groups[i]);
            }
        }

        // 计算紧凑总览数值，负责统计总倍率和总预估量。
        private static CompactActionTotals CalculateCompactTotals(Pawn pawn, List<BattleActionConfig> actions)
        {
            CompactActionTotals totals = new CompactActionTotals();
            for (int i = 0; i < actions.Count; i++)
            {
                BattleActionConfig action = actions[i];
                if (action == null)
                {
                    continue;
                }

                if (action.isHealing)
                {
                    totals.healCount++;
                    totals.healRatioTotal += Mathf.Max(0f, action.healPowerRatio);
                    totals.healTotal += EstimateHealBeforeTargetReceived(pawn, action);
                    continue;
                }

                if (action.isShield)
                {
                    totals.shieldCount++;
                    totals.shieldRatioTotal += Mathf.Max(0f, action.shieldPowerRatio);
                    totals.shieldTotal += EstimateShield(pawn, action);
                    continue;
                }

                totals.damageCount++;
                totals.damageRatioTotal += GetDamageActionMultiplier(action);
                totals.damageTotal += EstimateDamage(pawn, action);
                totals.canCrit |= action.canCrit;
                totals.alwaysCrit |= action.alwaysCrit;
                totals.alwaysShowCriticalText |= action.alwaysShowCriticalText;
                totals.applyAffinity |= action.applyAffinity;
                totals.isExSkill |= action.isExSkill;
            }

            return totals;
        }

        // 构建紧凑分组，负责按连续相同段压缩明细。
        private static List<CompactActionGroup> BuildCompactGroups(List<BattleActionConfig> actions)
        {
            List<CompactActionGroup> groups = new List<CompactActionGroup>();
            for (int i = 0; i < actions.Count;)
            {
                BattleActionConfig action = actions[i];
                if (action == null)
                {
                    i++;
                    continue;
                }

                int repeatCount = CountSameActions(actions, i, action);
                groups.Add(new CompactActionGroup
                {
                    startIndex = i,
                    repeatCount = repeatCount,
                    action = action
                });
                i += repeatCount;
            }

            return groups;
        }

        // 判断紧凑分组是否需要折叠，负责保留头尾关键信息。
        private static bool ShouldSkipCompactGroup(int groupCount, int groupIndex)
        {
            if (groupCount <= CompactGroupLimit)
            {
                return false;
            }

            return groupIndex >= CompactHeadGroupCount && groupIndex < groupCount - CompactTailGroupCount;
        }

        // 写入一组紧凑明细，负责用一行展示段号、次数、倍率、预估和修正。
        private static void AppendCompactGroupLine(StringBuilder builder, Pawn pawn, CompactActionGroup group)
        {
            BattleActionConfig action = group.action;
            string title = FormatSegmentTitle(group.startIndex, group.repeatCount, action);
            if (action.isHealing)
            {
                float heal = EstimateHealBeforeTargetReceived(pawn, action) * group.repeatCount;
                builder.AppendLine("  " + title + "：" + FormatColor("治疗", HealColor) + " " + FormatPercent(action.healPowerRatio) + " x" + group.repeatCount + " = " + FormatColor(FormatNumber(heal), HealColor));
                return;
            }

            if (action.isShield)
            {
                float shield = EstimateShield(pawn, action) * group.repeatCount;
                builder.AppendLine("  " + title + "：" + FormatColor("护盾", ShieldColor) + " " + FormatPercent(action.shieldPowerRatio) + " x" + group.repeatCount + " = " + FormatColor(FormatNumber(shield), ShieldColor));
                return;
            }

            float damage = EstimateDamage(pawn, action) * group.repeatCount;
            builder.AppendLine("  " + title + "：" + FormatColor("伤害", DamageColor) + " " + FormatPercent(GetDamageActionMultiplier(action)) + " x" + group.repeatCount + " = " + FormatColor(FormatNumber(damage), DamageColor) + "，" + FormatFormulaModifiers(action.canCrit, action.alwaysCrit, action.applyAffinity, action.isExSkill));
        }

        // 估算单段伤害，负责复用战斗属性工具的真实伤害计算。
        private static float EstimateDamage(Pawn pawn, BattleActionConfig action)
        {
            BattleDamageResult result = BattleStatUtility.BuildDamageResult(new BattleDamageRequest
            {
                instigator = pawn,
                target = pawn,
                damageDef = action.damageDef,
                weaponBaseAttack = action.previewWeaponBaseAttack,
                attackPowerRatio = action.attackPowerRatio,
                normalAttackMultiplier = action.normalAttackMultiplier,
                baseMasteryMultiplier = action.baseMasteryMultiplier,
                penetration = action.penetration,
                isNormalAttack = action.isNormalAttack,
                canCrit = action.alwaysCrit && action.canCrit,
                alwaysCrit = action.alwaysCrit,
                applyAffinity = false,
                isExSkill = action.isExSkill
            });
            return result.finalAmount;
        }

        // 估算目标受疗前的单段治疗，负责和详细治疗段使用同一口径。
        private static float EstimateHealBeforeTargetReceived(Pawn pawn, BattleActionConfig action)
        {
            return BattleStatUtility.GetFinalHealPower(pawn) * Mathf.Max(0f, action.healPowerRatio);
        }

        // 估算单段护盾量，负责让护盾技能在悬浮说明里显示预计护盾值。
        private static float EstimateShield(Pawn pawn, BattleActionConfig action)
        {
            return BattleStatUtility.GetFinalHealPower(pawn) * Mathf.Max(0f, action.shieldPowerRatio);
        }

        //比较两个预览战斗段，负责判断它们是否可以在显示上合并。
        private static bool IsSamePreviewAction(BattleActionConfig left, BattleActionConfig right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return left.attackPowerRatio == right.attackPowerRatio &&
                   left.normalAttackMultiplier == right.normalAttackMultiplier &&
                   left.baseMasteryMultiplier == right.baseMasteryMultiplier &&
                   left.healPowerRatio == right.healPowerRatio &&
                   left.shieldPowerRatio == right.shieldPowerRatio &&
                   left.damageDef == right.damageDef &&
                   left.shieldHediffDef == right.shieldHediffDef &&
                   left.penetration == right.penetration &&
                   left.isHealing == right.isHealing &&
                   left.isShield == right.isShield &&
                   left.isNormalAttack == right.isNormalAttack &&
                   left.canCrit == right.canCrit &&
                   left.alwaysCrit == right.alwaysCrit &&
                   left.alwaysShowCriticalText == right.alwaysShowCriticalText &&
                   left.alwaysShowHealText == right.alwaysShowHealText &&
                   left.applyAffinity == right.applyAffinity &&
                   left.isExSkill == right.isExSkill &&
                   left.isProjectilePreview == right.isProjectilePreview &&
                   left.previewWeaponBaseAttack == right.previewWeaponBaseAttack;
        }

        //格式化段落标题，负责把连续相同段压缩成短标题。
        private static string FormatSegmentTitle(int startIndex, int repeatCount, BattleActionConfig action)
        {
            if (action?.isProjectilePreview == true)
            {
                return "直线弹";
            }

            if (repeatCount <= 1)
            {
                return "第" + (startIndex + 1) + "段";
            }

            return "第" + (startIndex + 1) + "-" + (startIndex + repeatCount) + "段（共" + repeatCount + "次）";
        }

        // 写入施法者属性，负责让玩家看到公式里的实时基础值。
        private static void AppendCasterStats(StringBuilder builder, Pawn pawn)
        {
            float weaponBaseAttack = BattleStatUtility.GetWeaponBaseAttack(pawn);
            float levelMultiplier = BattleStatUtility.GetAttackLevelMultiplier(pawn);
            float starMultiplier = BattleStatUtility.GetAttackStarMultiplier(pawn);
            float attackFlat = BattleStatUtility.GetAttackFlatBonus(pawn);
            float attackMultiplier = BattleStatUtility.GetAttackMultiplier(pawn);
            float finalAttack = BattleStatUtility.GetFinalAttackPower(pawn, weaponBaseAttack);
            float healBase = BattleStatUtility.GetInitialHeal(pawn) + pawn.GetStatValue(BattleStatDefOf.BANW_InitialHeal);
            float healLevelMultiplier = BattleStatUtility.GetHealLevelMultiplier(pawn);
            float healStarMultiplier = BattleStatUtility.GetHealStarMultiplier(pawn);
            float healFlatBonus = BattleStatUtility.GetHealFlatBonus(pawn);
            float healBonusMultiplier = BattleStatUtility.GetHealBonusMultiplier(pawn);
            float finalHeal = BattleStatUtility.GetFinalHealPower(pawn);
            float exMultiplier = BattleStatUtility.GetExSkillMultiplier(pawn);

            builder.AppendLine("角色攻击力：" + FormatNumber(weaponBaseAttack) + " x " + FormatPercent(levelMultiplier) + " x " + FormatPercent(starMultiplier) + " + " + FormatNumber(attackFlat) + " = " + FormatColor(FormatNumber(finalAttack), DamageColor));
            builder.AppendLine("攻击力加成：" + FormatColor(FormatPercent(attackMultiplier), DamageColor));
            builder.AppendLine("治愈力：((" + FormatNumber(healBase) + " x " + FormatPercent(healLevelMultiplier) + " x " + FormatPercent(healStarMultiplier) + ") + " + FormatNumber(healFlatBonus) + ") x " + FormatPercent(healBonusMultiplier) + " = " + FormatColor(FormatNumber(finalHeal), HealColor));
            builder.AppendLine("EX技能倍率：" + FormatColor(FormatPercent(exMultiplier), ExColor));
        }

        // 写入伤害段公式，负责展示固定值、攻击力倍率、暴击、克制和 EX 倍率。
        private static void AppendDamageAction(StringBuilder builder, Pawn pawn, BattleActionConfig action)
        {
            BattleDamageResult result = BattleStatUtility.BuildDamageResult(new BattleDamageRequest
            {
                instigator = pawn,
                target = pawn,
                damageDef = action.damageDef,
                weaponBaseAttack = action.previewWeaponBaseAttack,
                attackPowerRatio = action.attackPowerRatio,
                normalAttackMultiplier = action.normalAttackMultiplier,
                baseMasteryMultiplier = action.baseMasteryMultiplier,
                penetration = action.penetration,
                isNormalAttack = action.isNormalAttack,
                canCrit = action.alwaysCrit && action.canCrit,
                alwaysCrit = action.alwaysCrit,
                applyAffinity = false,
                isExSkill = action.isExSkill
            });

            builder.AppendLine("类型：" + FormatColor("伤害", DamageColor));
            builder.AppendLine("伤害倍率：" + FormatColor(FormatPercent(GetDamageActionMultiplier(action)), DamageColor));
            builder.AppendLine("暴击：" + FormatSwitch(action.canCrit, CritColor));
            builder.AppendLine("强制暴击：" + FormatSwitch(action.alwaysCrit, CritColor));
            builder.AppendLine("暴击文字：" + FormatSwitch(action.alwaysShowCriticalText, CritColor));
            builder.AppendLine("属性克制：" + FormatSwitch(action.applyAffinity, AffinityColor));
            builder.AppendLine("EX倍率：" + FormatEx(action.isExSkill, result.exSkillMultiplier));
            AppendDamageFormula(builder, action);
            builder.AppendLine("基础预估伤害：" + FormatColor(FormatNumber(result.finalAmount), DamageColor));
            builder.AppendLine(FormatFormulaHint(action.applyAffinity));
        }

        // 写入治疗段公式，负责展示最终治愈力、技能倍率、受回复率和非暴击规则。
        private static void AppendHealAction(StringBuilder builder, AbilityDef abilityDef, Pawn pawn, BattleActionConfig action)
        {
            float estimatedHealBeforeTargetReceived = BattleStatUtility.GetFinalHealPower(pawn) * Mathf.Max(0f, action.healPowerRatio);
            bool selfOnlyHeal = IsSelfOnlyHealAbility(abilityDef);
            float selfFinalHeal = estimatedHealBeforeTargetReceived * BattleStatUtility.GetHealReceivedMultiplier(pawn);

            builder.AppendLine("类型：" + FormatColor("治疗", HealColor));
            builder.AppendLine("最终治愈力：" + FormatColor(FormatNumber(BattleStatUtility.GetFinalHealPower(pawn)), HealColor));
            builder.AppendLine("技能治疗量乘数：" + FormatColor(FormatPercent(action.healPowerRatio), HealColor));
            builder.AppendLine("目标受回复倍率：" + FormatColor(selfOnlyHeal ? FormatPercent(BattleStatUtility.GetHealReceivedMultiplier(pawn)) : "命中目标后按被治疗者结算", HealColor));
            builder.AppendLine("暴击：" + FormatColor("不参与", DisabledColor));
            builder.AppendLine("EX倍率：" + FormatColor("不参与", DisabledColor));
            AppendHealFormula(builder, action);
            builder.AppendLine("单次基础预估治疗：" + FormatColor(FormatNumber(estimatedHealBeforeTargetReceived), HealColor));
            if (selfOnlyHeal)
            {
                builder.AppendLine("单次最终预估治疗：" + FormatColor(FormatNumber(selfFinalHeal), HealColor));
            }
            else
            {
                builder.AppendLine(FormatColor("该技能需要先选目标，按钮悬浮阶段无法预知目标当前受回复率，所以这里只显示按100%受回复率计算的基础值。", HealColor));
            }
            builder.AppendLine(FormatColor("实际回复还会受目标受回复率和可治疗伤势剩余量影响。", HealColor));
        }

        // 写入护盾段公式，负责展示护盾倍率和预计护盾值。
        private static void AppendShieldAction(StringBuilder builder, Pawn pawn, BattleActionConfig action)
        {
            float estimatedShield = EstimateShield(pawn, action);

            builder.AppendLine("类型：" + FormatColor("护盾", ShieldColor));
            builder.AppendLine("护盾倍率：" + FormatColor(FormatPercent(action.shieldPowerRatio), ShieldColor));
            builder.AppendLine("护盾状态：" + FormatColor(action.shieldHediffDef?.label ?? action.shieldHediffDef?.defName ?? "未配置", ShieldColor));
            builder.AppendLine("目标受回复倍率：" + FormatColor("不参与", DisabledColor));
            builder.AppendLine("暴击：" + FormatColor("不参与", DisabledColor));
            builder.AppendLine("EX倍率：" + FormatColor("不参与", DisabledColor));
            builder.AppendLine("算法：");
            builder.AppendLine(FormatColor("  护盾：最终治愈力 x 护盾倍率", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  同类护盾：数值叠加并刷新持续时间", ColoredText.SubtleGrayColor));
            builder.AppendLine("预估护盾：" + FormatColor(FormatNumber(estimatedShield), ShieldColor));
        }

        //写入伤害算法，负责用短行展示实际结算顺序，避免 tooltip 横向撑开。
        private static void AppendDamageFormula(StringBuilder builder, BattleActionConfig action)
        {
            builder.AppendLine("算法：");
            builder.AppendLine(FormatColor("  攻击：角色自身攻击力 x 技能倍率 x 攻击力加成", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  修正：" + FormatFormulaModifiers(action.canCrit, action.alwaysCrit, action.applyAffinity, action.isExSkill), ColoredText.SubtleGrayColor));
        }

        //写入治疗算法，负责用短行展示实际结算顺序，避免 tooltip 横向撑开。
        private static void AppendHealFormula(StringBuilder builder, BattleActionConfig action)
        {
            builder.AppendLine("算法：");
            builder.AppendLine(FormatColor("  治愈：((初始治愈力 x 升级倍率 x 升星倍率) + 固定治愈力) x 治愈力加成", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  技能：最终治愈力 x 技能治疗量乘数", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  合计：技能治疗量 x 目标受回复率", ColoredText.SubtleGrayColor));
            builder.AppendLine(FormatColor("  修正：暴击和 EX 不参与治疗", ColoredText.SubtleGrayColor));
        }

        //格式化公式修正项，负责把暴击、克制和 EX 这些附加步骤压缩成一行。
        private static string FormatFormulaModifiers(bool canCrit, bool alwaysCrit, bool applyAffinity, bool isExSkill)
        {
            List<string> modifiers = new List<string>();
            if (canCrit)
            {
                modifiers.Add(alwaysCrit ? "强制暴击" : "暴击");
            }

            if (applyAffinity)
            {
                modifiers.Add("克制");
            }

            if (isExSkill)
            {
                modifiers.Add("EX");
            }

            return modifiers.Count > 0 ? string.Join("、", modifiers.ToArray()) : "无";
        }

        // 获取伤害段显示倍率，负责区分技能倍率和普通攻击倍率两种配置口径。
        private static float GetDamageActionMultiplier(BattleActionConfig action)
        {
            if (action == null)
            {
                return 0f;
            }

            return Mathf.Max(0f, action.isNormalAttack ? action.normalAttackMultiplier : action.attackPowerRatio);
        }

        // 格式化紧凑总览修正项，负责把整套多段技能的参与机制合并成一行。
        private static string FormatCompactModifiers(CompactActionTotals totals)
        {
            List<string> modifiers = new List<string>();
            if (totals.canCrit)
            {
                modifiers.Add(FormatColor("暴击", CritColor));
            }

            if (totals.alwaysShowCriticalText)
            {
                modifiers.Add(FormatColor("暴击文字", CritColor));
            }

            if (totals.alwaysCrit)
            {
                modifiers.Add(FormatColor("强制暴击", CritColor));
            }

            if (totals.applyAffinity)
            {
                modifiers.Add(FormatColor("属性克制", AffinityColor));
            }

            if (totals.isExSkill)
            {
                modifiers.Add(FormatColor("EX", ExColor));
            }

            return modifiers.Count > 0 ? string.Join("、", modifiers.ToArray()) : FormatColor("无", DisabledColor);
        }

        // 格式化属性克制说明，负责避免在没有目标时误报具体克制倍率。
        private static string FormatFormulaHint(bool applyAffinity)
        {
            if (!applyAffinity)
            {
                return FormatColor("属性克制未参与；命中后仍会继续经过护甲、减伤和承伤系数。", DisabledColor);
            }

            return FormatColor("命中目标后会按目标护甲类型结算克制，并继续经过护甲、减伤和承伤系数。", AffinityColor);
        }

        // 格式化开关文本，负责把启用和禁用状态染成不同颜色。
        private static string FormatSwitch(bool enabled, Color enabledColor)
        {
            return enabled ? FormatColor("参与", enabledColor) : FormatColor("不参与", DisabledColor);
        }

        // 格式化 EX 倍率文本，负责只在 EX 技能段高亮真实倍率。
        private static string FormatEx(bool isExSkill, float multiplier)
        {
            return isExSkill ? FormatColor(FormatPercent(multiplier), ExColor) : FormatColor("不参与", DisabledColor);
        }

        // 格式化数值，负责让 tooltip 中的计算结果保持短小稳定。
        private static string FormatNumber(float value)
        {
            return value.ToString("0.#");
        }

        // 格式化倍率，负责把 1.2 显示为 120%。
        private static string FormatPercent(float value)
        {
            return value.ToString("P0");
        }

        // 格式化彩色文本，负责统一 RimWorld 富文本颜色写法。
        private static string FormatColor(string text, Color color)
        {
            return text.Colorize(color);
        }

        // 紧凑段落分组，负责记录连续相同段的起点、次数和配置。
        private class CompactActionGroup
        {
            public int startIndex;
            public int repeatCount;
            public BattleActionConfig action;
        }

        // 紧凑总览统计，负责保存多段技能的合计倍率、合计预估和全局修正开关。
        private class CompactActionTotals
        {
            public int damageCount;
            public int healCount;
            public int shieldCount;
            public float damageRatioTotal;
            public float healRatioTotal;
            public float shieldRatioTotal;
            public float damageTotal;
            public float healTotal;
            public float shieldTotal;
            public bool canCrit;
            public bool alwaysCrit;
            public bool alwaysShowCriticalText;
            public bool applyAffinity;
            public bool isExSkill;
        }
    }
}
