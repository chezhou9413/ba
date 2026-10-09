using System.Linq;
using Verse;

namespace BANWlLib.Skills
{
    //绑定职责：安装角色声明的普通技能状态，并向其发布独立普攻计数事件。
    public static class NormalSkillUtility
    {
        //安装职责：依据角色配置列表创建尚未持有的状态，不要求角色具备专属EX。
        public static void Install(Pawn pawn)
        {
            var skills = pawn.kindDef?.GetModExtension<SpecialSkillKindExtension>()?.normalSkills;
            if (skills == null) return;
            foreach (NormalSkillDef skill in skills)
            {
                if (pawn.health.hediffSet.hediffs.OfType<Hediff_NormalSkillState>().Any(existing => existing.skill == skill)) continue;
                var state = (Hediff_NormalSkillState)HediffMaker.MakeHediff(
                    DefDatabase<HediffDef>.GetNamed("BANW_NormalSkillRuntime"), pawn);
                state.skill = skill;
                pawn.health.AddHediff(state);
            }
        }

        //事件职责：通知施法者持有的每个通用普通技能，不依赖地图上的其他角色数量。
        public static void Notify(Pawn pawn, NormalSkillCountMode mode)
        {
            foreach (var state in pawn.health.hediffSet.hediffs.OfType<Hediff_NormalSkillState>()) state.Notify(mode);
        }
    }
}
