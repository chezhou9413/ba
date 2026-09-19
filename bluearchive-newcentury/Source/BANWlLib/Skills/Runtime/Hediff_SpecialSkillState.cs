using System.Linq;
using BANWlLib.BattleSystem;
using RimWorld;
using UnityEngine;
using Verse;

namespace BANWlLib.Skills
{
    //角色特殊技能状态，负责保存阶段、计数、计时、蓄积和独立复制技能引用。
    public class Hediff_SpecialSkillState : HediffWithComps
    {
        public SpecialSkillProfileDef profile;
        public bool native = true;
        public int stage;
        public int stacks;
        public int hits;
        public int remainingHits;
        public int endTick = -1;
        public int nextNormalTick;
        public int passiveReadyTick;
        public int castEndTick = -1;
        public float recorded;
        public float recordCap;
        public bool releaseReady;
        public bool forceDeath;
        public IntVec3 center = IntVec3.Invalid;
        public Pawn recordTarget;
        public Thing field;
        public Map activeMap;
        public BattleCasterSnapshot snapshot;
        public Ability copiedAbility;
        public Hediff appliedBuff;
        public Hediff appliedShield;
        public Hediff countBuff;
        public int lastLethalEvent = -1;
        public int pendingAttacks;
        private Effecter maintainedEffect;
        private EffecterDef maintainedDef;
        public int Now => Verse.Find.TickManager.TicksGame;
        public bool Active => endTick > Now;
        public override bool ShouldRemove => false;
        //每个施法来源保留独立运行状态，禁止原版按同名Hediff合并。
        public override bool TryMergeWith(Hediff other) => false;
        public override string LabelBase => profile == null ? base.LabelBase : "测试新技能·" + profile.label;
        public override string TipStringExtra => $"阶段：{stage}　层数：{stacks}　命中：{hits}\n剩余次数：{remainingHits}　蓄积：{recorded:0.##}/{recordCap:0.##}\n持续：{Mathf.Max(0, endTick - Now) / 60f:0.0}秒　待释放：{releaseReady}";

        //取得指定配置的原生技能状态，复制技能使用独立的新状态。
        public static Hediff_SpecialSkillState Find(Pawn pawn, SpecialSkillProfileDef profile)
        {
            return pawn?.health?.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>()
                .FirstOrDefault(s => s.native && s.profile == profile);
        }

        //按角色类别查找原生状态，供属性、命中和死亡补丁使用。
        public static Hediff_SpecialSkillState Find(Pawn pawn, SpecialSkillRole role)
        {
            return pawn?.health?.hediffSet.hediffs.OfType<Hediff_SpecialSkillState>()
                .FirstOrDefault(s => s.native && s.profile?.role == role);
        }

        //创建属于当前角色的独立状态，不修改共享配置。
        public static Hediff_SpecialSkillState Create(Pawn pawn, SpecialSkillProfileDef profile, bool native = true)
        {
            var state = (Hediff_SpecialSkillState)HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("BANW_SpecialSkillRuntime"), pawn);
            state.profile = profile;
            state.native = native;
            state.nextNormalTick = Verse.Find.TickManager.TicksGame + profile.normalIntervalTicks;
            state.activeMap = pawn.Map;
            pawn.health.AddHediff(state);
            return state;
        }

        //推动状态到期和自动技能，使用绝对游戏时间避免离图后暂停计时。
        public override void Tick()
        {
            base.Tick();
            if (profile == null) return;
            if (pawn.Map != activeMap)
            {
                SpecialSkillDispatcher.LeaveMap(this);
                activeMap = pawn.Map;
            }
            SpecialSkillDispatcher.Tick(this);
            UpdateEffect();
            if (!native && !Active && recordTarget == null && !releaseReady && pendingAttacks == 0 && copiedAbility == null)
                pawn.health.RemoveHediff(this);
        }

        //维护一份持续特效，避免每次命中或层数变化反复创建特效。
        private void UpdateEffect()
        {
            bool charged = profile.role == SpecialSkillRole.Arisu && stage > 0;
            EffecterDef effect = charged ? SpecialEffects.Charge(profile.chargeStateEffecters, stage) : profile.stateEffecter;
            if (maintainedDef != effect) CleanupEffect();
            if (pawn.Spawned && !pawn.Dead && (Active || charged) && effect != null)
            {
                if (maintainedEffect == null)
                {
                    maintainedDef = effect;
                    maintainedEffect = effect.Spawn();
                    maintainedEffect.Trigger(pawn, pawn);
                }
                maintainedEffect.EffectTick(pawn, pawn);
            }
            else CleanupEffect();
        }

        //释放当前状态的持续表现资源。
        public void CleanupEffect()
        {
            maintainedEffect?.Cleanup();
            maintainedEffect = null;
            maintainedDef = null;
        }

        //删除状态时释放持续表现。
        public override void PostRemoved() { CleanupEffect(); base.PostRemoved(); }

        //序列化全部运行数据，支持当前版本存读档继续施法和计数。
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref profile, "profile");
            Scribe_Values.Look(ref native, "native", true);
            Scribe_Values.Look(ref stage, "stage");
            Scribe_Values.Look(ref stacks, "stacks");
            Scribe_Values.Look(ref hits, "hits");
            Scribe_Values.Look(ref remainingHits, "remainingHits");
            Scribe_Values.Look(ref endTick, "endTick", -1);
            Scribe_Values.Look(ref nextNormalTick, "nextNormalTick");
            Scribe_Values.Look(ref passiveReadyTick, "passiveReadyTick");
            Scribe_Values.Look(ref castEndTick, "castEndTick", -1);
            Scribe_Values.Look(ref recorded, "recorded");
            Scribe_Values.Look(ref recordCap, "recordCap");
            Scribe_Values.Look(ref releaseReady, "releaseReady");
            Scribe_Values.Look(ref forceDeath, "forceDeath");
            Scribe_Values.Look(ref pendingAttacks, "pendingAttacks");
            Scribe_Values.Look(ref center, "center", IntVec3.Invalid);
            Scribe_References.Look(ref recordTarget, "recordTarget");
            Scribe_References.Look(ref field, "field");
            Scribe_References.Look(ref activeMap, "activeMap");
            Scribe_References.Look(ref copiedAbility, "copiedAbility");
            Scribe_References.Look(ref appliedBuff, "appliedBuff");
            Scribe_References.Look(ref appliedShield, "appliedShield");
            Scribe_References.Look(ref countBuff, "countBuff");
            Scribe_Deep.Look(ref snapshot, "snapshot");
        }
    }
}
