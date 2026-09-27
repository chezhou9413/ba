using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BANWlLib.Skills
{
    //目标持有的复制机会，负责技能实例、阶段同步、存档和施法完成后的资源回收。
    public class Hediff_RioCopy : HediffWithComps
    {
        public Hediff_SpecialSkillState issuer;
        public Ability copy;
        public bool spent;
        private Command_RioCopiedAbility command;
        private static readonly AccessTools.FieldRef<Ability, List<Tuple<Effecter, TargetInfo, TargetInfo>>> Effects =
            AccessTools.FieldRefAccess<Ability, List<Tuple<Effecter, TargetInfo, TargetInfo>>>("maintainedEffecters");

        public override bool ShouldRemove => false;
        public override string TipStringExtra => spent ? "复制机会已使用，正在完成技能效果。" :
            "本体施放：" + copy?.def.label + "\n使用本体当前阶段、充能、层数与属性，基础COST减1。";

        //每位莉音发放的机会独立保存，禁止同名状态合并。
        public override bool TryMergeWith(Hediff other) => false;

        //缓存当前复制实例的命令，命令始终以持有者作为实际施法者。
        public Command_RioCopiedAbility Command => command ?? (command = new Command_RioCopiedAbility(copy, pawn));

        //切换复制的EX定义，只创建目标的独立能力实例，不重建角色机制状态。
        public void SetAbility(AbilityDef def)
        {
            if (copy != null)
            {
                CancelOrders();
                pawn.abilities.abilities.Remove(copy);
            }
            copy = AbilityUtility.MakeAbility(def, pawn);
            issuer.copiedAbility = copy;
            pawn.abilities.abilities.Add(copy);
            pawn.abilities.Notify_TemporaryAbilitiesChanged();
            command = null;
        }

        //维护本体阶段，并在消费后等施法、冷却及原版托管特效结束再移除实例。
        public override void Tick()
        {
            base.Tick();
            if (copy == null || pawn.Dead || pawn.Destroyed || (!spent &&
                (issuer == null || issuer.pawn.Dead || !issuer.pawn.health.hediffSet.hediffs.Contains(issuer))))
            { pawn.health.RemoveHediff(this); return; }
            if (spent)
            {
                if (!copy.Casting && !copy.verb.Bursting && copy.CooldownTicksRemaining <= 0 && Effects(copy).Count == 0)
                    pawn.health.RemoveHediff(this);
                return;
            }
            if (copy.Casting) return;
            AbilityDef current = RioExResolver.CurrentPhase(pawn, copy.def);
            if (current != null && current != copy.def) SetAbility(current);
        }

        //移除复制能力及其托管表现，并清理发放者仍指向本机会的引用。
        public override void PostRemoved()
        {
            if (copy != null)
            {
                CancelOrders();
                foreach (var effect in Effects(copy)) effect.Item1.Cleanup();
                Effects(copy).Clear();
                pawn.abilities.abilities.Remove(copy);
                pawn.abilities.Notify_TemporaryAbilitiesChanged();
                if (issuer?.copiedAbility == copy) issuer.copiedAbility = null;
            }
            base.PostRemoved();
        }

        //撤销指向旧实例的瞄准与排队工作，避免阶段切换后继续使用已回收的技能。
        private void CancelOrders()
        {
            if (Find.Targeter.targetingSource?.GetVerb == copy.verb ||
                Find.Targeter.targetingSourceParent?.GetVerb == copy.verb) Find.Targeter.StopTargeting();
            pawn.jobs?.jobQueue.RemoveAll(pawn, job => job.ability == copy);
            if (!spent && pawn.CurJob?.ability == copy)
                pawn.jobs.EndCurrentJob(Verse.AI.JobCondition.InterruptForced);
        }

        //保存跨角色引用和消费标记，能力实例由目标的原版能力追踪器保存。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref issuer, "issuer");
            Scribe_References.Look(ref copy, "copy");
            Scribe_Values.Look(ref spent, "spent");
        }
    }
}
