using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace BANWlLib.comp
{
    public class LevelVisualCompPropertie : CompProperties
    {
        public List<string> levelAudio = new List<string>();
        public List<string> levelMote = new List<string>();

        public LevelVisualCompPropertie()
        {
            this.compClass = typeof(LevelVisualComp);
        }
    }

    public class LevelVisualComp : ThingComp
    {
        public LevelVisualCompPropertie Props => (LevelVisualCompPropertie)this.props;

        public int levelIndex = 0;
        private Pawn cachedPawn;
        public Hediff hediff;
        private HediffDef hediffDef = HediffDef.Named("BANW_LevelTrait");
        private float Severit = 0f;
        private Effecter currentEffecter;
        // 特效播放计时器（用于控制特效播放时长）
        private int effecterTimer = 0;
        
        // 性能优化：缓存Hediff引用，避免每帧查找
        private Hediff cachedHediff = null;
        private int lastHediffCheckTick = 0;
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (cachedPawn == null)
            {
                cachedPawn = this.parent as Pawn;
            }
        }
        public void playLeveleff()
        {
            if (hediff != null)
            {
                levelIndex = hediff.CurStageIndex;
                EffecterDef effecterDef = DefDatabase<EffecterDef>.GetNamed(Props.levelMote.RandomElement(), false);
                if (effecterDef != null)
                {
                    currentEffecter = effecterDef.Spawn(cachedPawn.Position, cachedPawn.Map);
                    TargetInfo targetInfo = new TargetInfo(cachedPawn);
                    currentEffecter.Trigger(targetInfo, targetInfo);
                    effecterTimer = 300;

                }
                SoundDef soundDef = DefDatabase<SoundDef>.GetNamed(Props.levelAudio.RandomElement(), false);
                if (soundDef != null)
                {
                    SoundInfo info = SoundInfo.InMap(cachedPawn);
                    soundDef.PlayOneShot(info);
                }
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            
            // 管理特效播放
            if (currentEffecter != null && effecterTimer > 0)
            {
                effecterTimer--;
                TargetInfo targetInfo = new TargetInfo(cachedPawn);
                currentEffecter.EffectTick(targetInfo, targetInfo);
                if (effecterTimer <= 0)
                {
                    currentEffecter.Cleanup();
                    currentEffecter = null;
                }
            }

            // 性能优化：每60tick检查一次Hediff，而不是每帧
            if (cachedPawn != null && hediffDef != null)
            {
                int currentTick = Find.TickManager.TicksGame;
                if (currentTick - lastHediffCheckTick >= 60) // 1秒 = 60 ticks
                {
                    cachedHediff = cachedPawn.health.hediffSet.GetFirstHediffOfDef(hediffDef);
                    lastHediffCheckTick = currentTick;
                }
                
                // 使用缓存的Hediff引用
                hediff = cachedHediff;
                if (hediff == null)
                {
                    hediff = HediffMaker.MakeHediff(hediffDef, cachedPawn);
                    hediff.Severity = Severit;
                    cachedPawn.health.AddHediff(hediff);
                    cachedHediff = hediff; // 更新缓存
                }
                else if (hediff != null)
                {
                    Severit = hediff.Severity;
                }
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref Severit, "Severit", 0f);
            Scribe_Values.Look(ref levelIndex, "levelIndex", 0);
        }
    }
}
