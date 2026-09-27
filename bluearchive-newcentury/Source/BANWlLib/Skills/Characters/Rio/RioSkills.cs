using System.Linq;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //莉音技能，负责向目标本体授予一次EX复制机会、攻击支援和单次减费。
    public static class RioSkills
    {
        //读取目标当前阶段的默认EX，所有候选均来自其实际持有的COST技能。
        public static AbilityDef CopyDef(Pawn pawn) => RioExResolver.Default(pawn);

        //把独立冷却的复制实例交给目标持有，施法属性与角色机制沿用目标本体。
        public static void Cast(Hediff_SpecialSkillState s, Pawn target)
        {
            AbilityDef def = CopyDef(target);
            if (def == null) { Log.Error("[BANW] 复制目标没有当前阶段可见的COST技能"); return; }
            var link = (Hediff_RioCopy)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BANW_RioCopy"), target);
            link.issuer = s;
            target.health.AddHediff(link);
            link.SetAbility(def);
            if (s.profile.buffHediff != null) TimedSkillBuff.Apply(target, s.profile.buffHediff, s.profile.durationTicks);
            Messages.Message(target.LabelShort + "获得一次EX复制机会，请选中该角色使用；右键复制按钮可选择其他EX。",
                target, MessageTypeDefOf.PositiveEvent, false);
        }

        //从实际施法者的状态中定位复制关系，避免误判目标原有技能。
        public static Hediff_RioCopy Link(Ability ability)
        {
            return ability?.pawn?.health?.hediffSet.hediffs.OfType<Hediff_RioCopy>()
                .FirstOrDefault(h => h.copy == ability);
        }

        //取得仍未消费的一次性复制机会的发放者。
        public static Hediff_SpecialSkillState Owner(Ability ability)
        {
            var link = Link(ability);
            return link != null && !link.spent ? link.issuer : null;
        }

        //判断是否为独立复制能力。
        public static bool IsCopied(Ability ability) => Link(ability) != null;

        //在常规减费之前减少复制技能的基础费用。
        public static int BaseCost(Ability ability, int cost) => IsCopied(ability) ? UnityEngine.Mathf.Max(0, cost - 1) : cost;

        //在扣费之前拒绝已消费或阶段失效的复制实例，防止排队重复施法。
        public static bool CanActivateCopy(Ability ability, out string reason)
        {
            reason = null;
            var link = Link(ability);
            if (link == null) return true;
            if (link.spent || link.issuer?.copiedAbility != ability) reason = "这次EX复制机会已经使用";
            else if (!RioExResolver.Candidates(ability.pawn).Any(a => a.def == ability.def))
                reason = "目标当前阶段已切换，请重新选择复制EX";
            return reason == null;
        }

        //成功发动后归还莉音复制入口，剩余弹丸和持续效果继续由目标维护。
        public static void Consume(Ability ability)
        {
            var link = Link(ability);
            if (link == null || link.spent) return;
            link.spent = true;
            link.issuer.copiedAbility = null;
        }

        //目标死亡或被移除后收回无法使用的复制机会。
        public static void Tick(Hediff_SpecialSkillState s)
        {
            if (s.copiedAbility != null && (s.copiedAbility.pawn.Dead || s.copiedAbility.pawn.Destroyed)) RemoveCopy(s);
        }

        //只回收授予目标的复制实例，不改动其原有技能。
        public static void RemoveCopy(Hediff_SpecialSkillState s)
        {
            var link = Link(s.copiedAbility);
            if (link != null) link.pawn.health.RemoveHediff(link);
            s.copiedAbility = null;
        }
    }
}
