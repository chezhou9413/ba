using BANWlLib.BattleSystem;
using System.Linq;
using Verse;
using Verse.Sound;

namespace BANWlLib.Skills
{
    //状态职责：独立保存一个通用普通技能的累计次数、冷却和定义引用。
    public class Hediff_NormalSkillState : HediffWithComps
    {
        public NormalSkillDef skill;
        public int count;
        public int nextCastTick;
        public override bool ShouldRemove => false;
        public override string LabelBase => skill?.label ?? base.LabelBase;
        public override string TipStringExtra => skill == null ? "" :
            "触发方式：" + (skill.countMode == NormalSkillCountMode.ShotFired ? "普攻成功发射" : "普攻有效命中") +
            "\n进度：" + count + "/" + skill.requiredCount + "\n目标：" +
            (skill.onlyTargetAllies ? "范围内最低生命比例队友" : "合法敌方目标") +
            "\n附加状态：" + string.Join("、", skill.actions.Where(action => action.triggerHediff != null)
                .Select(action => action.triggerHediff.label).ToArray()) +
            "\n冷却剩余：" + UnityEngine.Mathf.Max(0, nextCastTick - Find.TickManager.TicksGame) / 60f + "秒" +
            (skill.showBattleFormula ? AbilityBattleTooltipUtility.BuildActionsTooltip(pawn, null,
                SpecialSkillBattlePreview.Expand(skill.actions)) : "");

        //合并职责：同名运行状态保持独立，防止不同普通技能共用计数。
        public override bool TryMergeWith(Hediff other) => false;

        //计数职责：只接收配置指定的事件，额外技能动作不产生此类事件。
        public void Notify(NormalSkillCountMode mode)
        {
            if (skill.countMode == mode) count++;
        }

        //时钟职责：只在次数、冷却和行动条件满足时尝试施放。
        public override void Tick()
        {
            base.Tick();
            if (count >= skill.requiredCount && Find.TickManager.TicksGame >= nextCastTick &&
                SpecialCombatUtility.CanAct(pawn)) TryCast();
        }

        //施放职责：确认合法目标后消费一次进度，保存属性快照并安排各效果段。
        private void TryCast()
        {
            Thing target = NormalSkillTargetUtility.Find(pawn, skill);
            if (target == null) return;
            count -= skill.requiredCount;
            nextCastTick = Find.TickManager.TicksGame + skill.cooldownTicks;
            skill.castSound?.PlayOneShot(new TargetInfo(pawn));
            SpecialDirectionalEffects.Trigger(skill.casterEffecter, pawn, target);
            SpecialEffects.Trigger(skill.targetEffecter, target);
            var snapshot = BattleStatUtility.CreateSnapshot(pawn);
            foreach (SpecialAttackConfig action in skill.actions)
                SpecialCombatUtility.ScheduleNormal(pawn, target, action, snapshot, skill);
        }

        //存档职责：保存定义、累计次数与绝对冷却时刻。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref skill, "normalSkill");
            Scribe_Values.Look(ref count, "count");
            Scribe_Values.Look(ref nextCastTick, "nextCastTick");
        }
    }
}
