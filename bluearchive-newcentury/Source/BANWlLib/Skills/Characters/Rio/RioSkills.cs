using System.Linq;
using BANWlLib.CostSystem;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //莉音技能，负责默认EX解析、独立复制实例、单次减费和用后回收。
    public static class RioSkills
    {
        //读取显式声明的默认EX，拒绝复制入口本身。
        public static AbilityDef CopyDef(Pawn pawn)
        {
            AbilityDef def = pawn?.kindDef.GetModExtension<SpecialSkillKindExtension>()?.copyableEx;
            if (def?.comps?.OfType<CompProperties_SpecialSkill>().Any(c => c.profile.role == SpecialSkillRole.Rio) == true) return null;
            return def;
        }

        //为莉音创建新的能力实例，保留目标自己的冷却和组件状态。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            AbilityDef def = CopyDef(target);
            if (def == null) { Log.Error("[BANW] 复制目标没有有效的默认EX配置"); return; }
            s.copiedAbility = AbilityUtility.MakeAbility(def, s.pawn);
            s.pawn.abilities.abilities.Add(s.copiedAbility);
            s.pawn.abilities.Notify_TemporaryAbilitiesChanged();
            if (s.profile.buffHediff != null) TimedSkillBuff.Apply(target, s.profile.buffHediff, s.profile.durationTicks);
        }

        //查找指定能力是否属于莉音的一次性复制槽。
        public static Hediff_SpecialSkillState Owner(Ability ability)
        {
            return ability?.pawn?.health.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>()
                .FirstOrDefault(s => s.profile.role == SpecialSkillRole.Rio && s.copiedAbility == ability);
        }

        //判断是否为独立复制能力。
        public static bool IsCopied(Ability ability) => Owner(ability) != null;

        //在常规减费之前减少复制技能的基础费用。
        public static int BaseCost(Ability ability, int cost) => IsCopied(ability) ? UnityEngine.Mathf.Max(0, cost - 1) : cost;

        //成功发动后移除一次性能力，飞行弹丸和持续效果不依赖该实例。
        public static void RemoveCopy(Hediff_SpecialSkillState s)
        {
            s.pawn.abilities.abilities.Remove(s.copiedAbility);
            s.copiedAbility = null;
            s.pawn.abilities.Notify_TemporaryAbilitiesChanged();
        }
    }
}

