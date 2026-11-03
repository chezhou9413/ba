using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //退场缓冲只显示所属技能的保命计数，不维护第二份层数。
    public class Hediff_ShirokoRetirementBuffer : HediffWithComps
    {
        public Hediff_SpecialSkillState owner;
        public override string LabelInBrackets => (owner?.stacks ?? 0) + "层";
        public override bool ShouldRemove => owner == null || pawn.Dead || owner.stacks <= 0 ||
            !pawn.health.hediffSet.hediffs.Contains(owner);
        public override string TipStringExtra => owner == null ? "" :
            $"退场缓冲：{owner.stacks}/{owner.profile.lethalStackLimit}层\n触发时1层，此后每游戏秒增加1层；受伤不额外加层。\n满血时清除；达到上限时退场。\n被动冷却剩余：{Mathf.Max(0, owner.passiveReadyTick - owner.Now) / 60f:0.0}秒";

        //保存来源引用，读档后仍使用原技能的层数与冷却。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref owner, "owner");
        }
    }
}
