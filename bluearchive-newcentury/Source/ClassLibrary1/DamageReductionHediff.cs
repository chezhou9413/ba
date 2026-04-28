using Verse;
using RimWorld;

namespace BANWlLib
{
    // 减伤状态Hediff类
    // 用来在健康面板显示减伤保护状态
    public class DamageReductionHediff : Hediff
    {
        // 在健康面板中显示的标签后缀
        public override string LabelInBrackets
        {
            get
            {
                try
                {
                    if (pawn != null)
                    {
                        // 获取减伤组件
                        DamageReductionComp comp = pawn.GetComp<DamageReductionComp>();
                        if (comp != null)
                        {
                            int level = comp.GetCurrentLevel();
                            string bodyPart = comp.Props.damageReductionBodyPart;
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

        // 鼠标悬停时显示的详细信息
        public override string GetTooltip(Pawn pawn, bool showHediffSource = true)
        {
            try
            {
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

                    // 显示所有等级信息
                    baseTooltip += $"\n\n等级阈值：";
                    for (int i = 0; i < comp.Props.customValueThresholds.Count && i < comp.Props.damageReductionRatios.Count; i++)
                    {
                        int threshold = comp.Props.customValueThresholds[i];
                        float levelRatio = comp.Props.damageReductionRatios[i];
                        string status = currentValue >= threshold ? "★" : "☆";
                        baseTooltip += $"\n{status} 等级{i + 1}：{threshold}点 → 减伤{levelRatio:P0}";
                    }
                }
                
                return baseTooltip;
            }
            catch
            {
                return "减伤保护状态信息获取失败";
            }
        }

        // 检查这个Hediff是否应该被移除
        public override bool ShouldRemove
        {
            get
            {
                try
                {
                    if (pawn != null)
                    {
                        DamageReductionComp comp = pawn.GetComp<DamageReductionComp>();
                        return comp == null; // 如果没有减伤组件，则移除这个Hediff
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