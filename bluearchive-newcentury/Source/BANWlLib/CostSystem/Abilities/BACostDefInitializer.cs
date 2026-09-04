using System.Text.RegularExpressions;
using BANWlLib.BaDef;
using RimWorld;
using Verse;

namespace BANWlLib.CostSystem
{
    //COST Def初始化器负责让学生图鉴中的EX费用文本与AbilityDef费用组件保持一致。
    [StaticConstructorOnStartup]
    public static class BACostDefInitializer
    {
        private static readonly Regex CostNumberPattern = new Regex(
            @"(?<=<color=blue>)\d+(?=</color>)",
            RegexOptions.Compiled);

        //Def加载完成后同步所有已配置学生的第一技能COST文本。
        static BACostDefInitializer()
        {
            foreach (BaStudentDef studentDef in DefDatabase<BaStudentDef>.AllDefsListForReading)
            {
                SynchronizeStudentCostText(studentDef);
            }
        }

        //用学生首个AbilityDef的费用组件替换图鉴中的旧数值或占位值。
        private static void SynchronizeStudentCostText(BaStudentDef studentDef)
        {
            if (studentDef?.kindDef?.abilities.NullOrEmpty() != false ||
                studentDef.BaStudentUI?.Ability1 == null)
            {
                return;
            }

            AbilityDef abilityDef = studentDef.kindDef.abilities[0];
            CompProperties_AbilityCost costProps = null;
            if (abilityDef.comps != null)
            {
                for (int index = 0; index < abilityDef.comps.Count; index++)
                {
                    costProps = abilityDef.comps[index] as CompProperties_AbilityCost;
                    if (costProps != null)
                    {
                        break;
                    }
                }
            }

            if (costProps == null)
            {
                return;
            }

            string subtitle = studentDef.BaStudentUI.Ability1.AbilitySubtitle;
            if (subtitle.NullOrEmpty() || !CostNumberPattern.IsMatch(subtitle))
            {
                return;
            }

            studentDef.BaStudentUI.Ability1.AbilitySubtitle =
                CostNumberPattern.Replace(subtitle, costProps.cost.ToString(), 1);
        }
    }
}
