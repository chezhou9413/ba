using BANWlLib.BattleSystem;
using Verse;

namespace BANWlLib.Skills
{
    //延迟攻击记录，负责保存发射时间、目标、数值快照和蓄积标记。
    public class SpecialPendingAttack : IExposable
    {
        public Pawn caster;
        public Hediff_SpecialSkillState state;
        public Thing target;
        public SpecialAttackConfig action;
        public BattleCasterSnapshot snapshot;
        public int dueTick;
        public float multiplier = 1f;
        public float resolvedAmount = -1f;
        public bool canAccumulate = true;
        public bool startRecord;
        public bool droneOrigin;
        public bool normalHit;
        public Thing excludedTarget;
        public bool completed;

        //归还延迟攻击占用的状态引用计数，允许临时复制状态安全清理。
        public void Complete()
        {
            if (completed) return;
            completed = true;
            if (state != null) state.pendingAttacks--;
        }
        public IntVec3 center = IntVec3.Invalid;
        public EffecterDef impactEffecter;

        //保存仍在等待或飞行中的攻击，避免读档丢失数值和来源。
        public void ExposeData()
        {
            Scribe_References.Look(ref caster, "caster");
            Scribe_References.Look(ref state, "state");
            Scribe_References.Look(ref target, "target");
            Scribe_Deep.Look(ref action, "action");
            Scribe_Deep.Look(ref snapshot, "snapshot");
            Scribe_Values.Look(ref dueTick, "dueTick");
            Scribe_Values.Look(ref multiplier, "multiplier", 1f);
            Scribe_Values.Look(ref resolvedAmount, "resolvedAmount", -1f);
            Scribe_Values.Look(ref canAccumulate, "canAccumulate", true);
            Scribe_Values.Look(ref startRecord, "startRecord");
            Scribe_Values.Look(ref droneOrigin, "droneOrigin");
            Scribe_Values.Look(ref normalHit, "normalHit");
            Scribe_References.Look(ref excludedTarget, "excludedTarget");
            Scribe_Values.Look(ref completed, "completed");
            Scribe_Values.Look(ref center, "center", IntVec3.Invalid);
            Scribe_Defs.Look(ref impactEffecter, "impactEffecter");
        }
    }
}
