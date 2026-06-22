using BANWlLib.KindStats;
using Verse;

namespace BANWlLib.Tool
{
    // 学生阶级工具，负责把角色信息面板的成长进度转换为战斗系统使用的阶级。
    public static class StudentRankUtility
    {
        // 获取当前阶级，负责按自定义经验值和阈值计算角色面板星星数量。
        public static int GetCurrentRankLevel(Pawn pawn)
        {
            if (!StudentIdentityUtility.IsConfiguredStudentKind(pawn))
            {
                return 1;
            }

            HumanIntPropertyComp humanComp = pawn.GetComp<HumanIntPropertyComp>();
            DamageReductionComp damageComp = pawn.GetComp<DamageReductionComp>();
            if (humanComp == null || damageComp?.Props?.customValueThresholds == null || damageComp.Props.customValueThresholds.Count == 0)
            {
                return 1;
            }

            int currentExperience = humanComp.CustomIntValue;
            for (int i = damageComp.Props.customValueThresholds.Count - 1; i >= 0; i--)
            {
                if (currentExperience >= damageComp.Props.customValueThresholds[i])
                {
                    return i + 1;
                }
            }

            return 1;
        }

        // 获取指定阶级需要的经验值，负责给 Debug 场景生成稳定的 1/3/5 阶测试对象。
        public static int GetExperienceForRank(Pawn pawn, int rankLevel)
        {
            DamageReductionComp damageComp = pawn?.GetComp<DamageReductionComp>();
            if (damageComp?.Props?.customValueThresholds == null || damageComp.Props.customValueThresholds.Count == 0)
            {
                return 0;
            }

            int index = UnityEngine.Mathf.Clamp(rankLevel - 1, 0, damageComp.Props.customValueThresholds.Count - 1);
            return damageComp.Props.customValueThresholds[index];
        }

        // 设置测试阶级，负责通过真实经验链路驱动面板星星、减伤和战斗成长。
        public static void SetRankByExperience(Pawn pawn, int rankLevel)
        {
            if (pawn == null)
            {
                return;
            }

            HumanIntPropertyComp humanComp = pawn.GetComp<HumanIntPropertyComp>();
            if (humanComp == null)
            {
                return;
            }

            humanComp.SetValue(GetExperienceForRank(pawn, rankLevel));
            HealthScaleCache.Invalidate(pawn);
            PawnProgressBarUI.progressCache.Remove(pawn);
        }
    }
}
