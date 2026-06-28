using Verse;
using RimWorld;
using BANWlLib.BattleSystem;
using BANWlLib.Tool;

namespace BANWlLib
{
    // 减伤状态 Hediff，负责在学生健康面板显示学生星级和星级成长总加成。
    public class DamageReductionHediff : Hediff
    {
        // 在健康面板中显示星级后缀，负责让状态名称后方直观看到当前星级。
        public override string LabelInBrackets
        {
            get
            {
                try
                {
                    if (!StudentIdentityUtility.IsConfiguredStudentKind(pawn))
                    {
                        return "";
                    }

                    if (pawn != null)
                    {
                        // 获取减伤组件
                        DamageReductionComp comp = pawn.GetComp<DamageReductionComp>();
                        if (comp != null)
                        {
                            int level = comp.GetCurrentLevel();
                            string stars = "";
                            for (int i = 0; i < level; i++)
                            {
                                stars += "★";
                            }
                            return stars;
                        }
                    }
                    return "配置错误";
                }
                catch
                {
                    return "错误";
                }
            }
        }

        // 获取鼠标悬停时显示的星级详情，负责合并减伤保护和当前星级成长总加成。
        public override string GetTooltip(Pawn pawn, bool showHediffSource = true)
        {
            try
            {
                if (!StudentIdentityUtility.IsConfiguredStudentKind(pawn))
                {
                    return "";
                }

                string baseTooltip = base.GetTooltip(pawn, showHediffSource);
                
                // 获取减伤组件
                DamageReductionComp comp = pawn.GetComp<DamageReductionComp>();
                if (comp != null)
                {
                    int currentValue = 0;
                    HumanIntPropertyComp customComp = pawn.GetComp<HumanIntPropertyComp>();
                    if (customComp != null)
                    {
                        currentValue = customComp.CustomIntValue;
                    }

                    int level = comp.GetCurrentLevel();
                    float ratio = comp.GetDamageReductionRatio();
                    string bodyPart = comp.Props.damageReductionBodyPart;

                    baseTooltip += $"\n\n减伤保护详情：";
                    baseTooltip += $"\n• 保护部位：{bodyPart}";
                    baseTooltip += $"\n• 当前属性值：{currentValue}";
                    
                    if (level > 0)
                    {
                        baseTooltip += $"\n• 当前等级：{level}";
                        baseTooltip += $"\n• 减伤比例：{ratio:P0}";
                    }
                    else
                    {
                        baseTooltip += $"\n• 状态：未激活";
                    }
                }

                string growthTooltip = StarGrowthDisplayUtility.BuildTooltip(pawn);
                if (!growthTooltip.NullOrEmpty())
                {
                    baseTooltip += "\n\n" + growthTooltip;
                }
                 
                return baseTooltip;
            }
            catch
            {
                return "减伤保护状态信息获取失败";
            }
        }

        // 检查这个 Hediff 是否应该被移除，负责在非学生或缺少星级组件时清理状态。
        public override bool ShouldRemove
        {
            get
            {
                try
                {
                    if (!StudentIdentityUtility.IsConfiguredStudentKind(pawn))
                    {
                        return true;
                    }

                    if (pawn != null)
                    {
                        DamageReductionComp comp = pawn.GetComp<DamageReductionComp>();
                        return comp == null;
                    }
                    return true;
                }
                catch
                {
                    return true;
                }
            }
        }
    }
} 
